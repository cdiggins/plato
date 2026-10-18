using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Interpreter;

/// <summary>
/// An evaluator over the elaborated Typed IR (TIR) of a <see cref="Compilation"/>: every function
/// with a body can be invoked on interpreter values (<see cref="Values"/>), and any TIR expression
/// can be evaluated against an <see cref="Environment"/>, with no code generation and no host
/// build. The design, and the reasons for each choice, are in docs/interpreter.md.
///
/// Bodies are checked and elaborated lazily, one function at a time, the first time they are
/// reached: a law over a small part of a library costs only the functions it calls. Generic
/// bodies run unspecialized: a call whose callee is an interface declaration is dispatched on the
/// receiver's run-time type, the same decision the monomorphizer makes statically.
/// </summary>
public sealed partial class Interpreter
{
    public Compilation Compilation { get; }
    public NumberProfile Profile { get; }
    public Numbers Numbers { get; }
    public HostIntrinsics Host { get; }

    /// <summary>Activations deeper than this are a fault, not a stack overflow.</summary>
    public int MaxDepth { get; set; } = 20_000;

    /// <summary>When set, an activation that starts after this instant faults with "time budget
    /// exceeded", so a caller can bound one evaluation (the law runner sets it per law).</summary>
    public DateTime? Deadline { get; set; }

    private readonly TypeChecker _checker;
    private readonly Elaborator _elaborator;
    private readonly Dictionary<FunctionDef, TirFunction> _tir = new Dictionary<FunctionDef, TirFunction>();
    private readonly Dictionary<string, List<FunctionDef>> _groups;
    private readonly Dictionary<string, TypeExpression> _primitiveTypes = new Dictionary<string, TypeExpression>();
    private int _depth;

    public Interpreter(Compilation compilation, NumberProfile profile = null)
    {
        Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
        if (!compilation.CompletedCompilation)
            throw new ArgumentException("the compilation did not complete; nothing to interpret");
        Profile = profile ?? NumberProfile.Default;
        Numbers = new Numbers(Profile);
        Host = new HostIntrinsics(Numbers);
        _checker = new TypeChecker(compilation);
        _elaborator = new Elaborator(compilation);
        _groups = compilation.FunctionDefinitions
            .GroupBy(f => f.Name, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
    }

    /// <summary>Every function of the compilation with this name, in declaration order.</summary>
    public IReadOnlyList<FunctionDef> FunctionsNamed(string name)
        => _groups.TryGetValue(name, out var fs) ? fs : Array.Empty<FunctionDef>();

    /// <summary>Every function of the compilation.</summary>
    public IEnumerable<FunctionDef> Functions => Compilation.FunctionDefinitions;

    /// <summary>Time spent checking and elaborating bodies on first use (a cost figure for reports).</summary>
    public TimeSpan ElaborationTime { get; private set; }

    /// <summary>How many run-time overload resolutions were performed (a cost figure for reports).</summary>
    public long ResolutionCount { get; internal set; }

    /// <summary>How many bodies have been elaborated so far (a cost figure for reports).</summary>
    public int ElaboratedCount => _tir.Count;

    /// <summary>The elaborated TIR of a bodied function, produced on first use; null for a
    /// bodiless one.</summary>
    public TirFunction GetTir(FunctionDef f)
    {
        if (f?.Body == null)
            return null;
        if (_tir.TryGetValue(f, out var tir))
            return tir;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = _checker.Check(f);
        tir = _elaborator.Elaborate(result);
        ElaborationTime += sw.Elapsed;
        _tir[f] = tir;
        return tir;
    }

    public TypeExpression PrimitiveType(string name)
    {
        if (!_primitiveTypes.TryGetValue(name, out var t))
        {
            t = Compilation.GetTypeDef(name)?.ToTypeExpression();
            _primitiveTypes[name] = t;
        }
        return t;
    }

    // --- entry points --------------------------------------------------------

    /// <summary>Calls a function by name on run-time values, choosing the overload as the
    /// checker would from the values' types.</summary>
    public object Call(string name, params object[] args)
    {
        var f = ResolveByValues(name, args, out var converted)
            ?? throw new EvaluationException($"no overload of '{name}' takes ({string.Join(", ", args.Select(a => TypeOf(a)))})");
        return Apply(f, converted, null);
    }

    /// <summary>Invokes one function on run-time values: runs its body, or the host intrinsic it
    /// names, or builds the value a constructor, field, cast or sum factory denotes.</summary>
    public object Invoke(FunctionDef f, params object[] args)
        => Apply(f, args, null);

    /// <summary>Evaluates one TIR expression against an environment. The engine SPEC-EXECUTION.md's
    /// <c>plato verify</c> builds on: a bounded quantifier binds its variable in the environment
    /// and evaluates its body here.</summary>
    public object Evaluate(TirNode node, Environment env)
    {
        switch (node)
        {
            case null:
                return null;

            case TirLiteral lit:
                return LiteralValue(lit);

            case TirParameter p:
                return env.Get(p.Def);

            case TirVariable v:
                return env.Get(v.Def);

            case TirTypeRef t:
                return new TypeToken(t.TypeDef?.Name == "Self"
                    ? env.Self ?? t.Type
                    : (t.TypeDef?.ToTypeExpression() ?? t.Type));

            case TirLet let:
            {
                var value = Evaluate(let.Value, env);
                env.Define(let.Def, value);
                return value;
            }

            case TirDefault d:
                return DefaultValue(d.Type ?? throw new EvaluationException("'default' with no contextual type"));

            case TirName nm:
                return ResolveBareName(nm.Name, nm.Type, env);

            case TirCall call:
            {
                var args = new object[call.Args.Count];
                for (var i = 0; i < args.Length; i++)
                    args[i] = Evaluate(call.Args[i], env);
                return ApplyCall(call, args, env);
            }

            case TirCoerce c:
                return Coerce(Evaluate(c.Inner, env), c.ToType, c.ConversionFn);

            case TirInvoke inv:
            {
                var target = Evaluate(inv.Target, env);
                var args = new object[inv.Args.Count];
                for (var i = 0; i < args.Length; i++)
                    args[i] = Evaluate(inv.Args[i], env);
                return InvokeValue(target, args);
            }

            case TirConditional cond:
                return AsBoolean(Evaluate(cond.Condition, env), "a conditional's condition")
                    ? Evaluate(cond.IfTrue, env)
                    : Evaluate(cond.IfFalse, env);

            case TirNew nw:
            {
                var args = new object[nw.Args.Count];
                for (var i = 0; i < args.Length; i++)
                    args[i] = Evaluate(nw.Args[i], env);
                return NewValue(nw.NewType ?? nw.Type, args);
            }

            case TirArray arr:
            {
                var items = new object[arr.Elements.Count];
                for (var i = 0; i < items.Length; i++)
                    items[i] = Evaluate(arr.Elements[i], env);
                return new PlatoArray(HostIntrinsics.ElementTypeOf(arr.Type, 0), items);
            }

            case TirAssign asg:
            {
                var value = Evaluate(asg.RValue, env);
                switch (asg.LValue)
                {
                    case TirVariable lv: env.Set(lv.Def, value); break;
                    case TirParameter lp: env.Set(lp.Def, value); break;
                    default: throw new EvaluationException($"assignment to a non-variable {asg.LValue}");
                }
                return value;
            }

            case TirLambda lam:
                return MakeClosure(lam, env);

            case TirBlock b:
            {
                var r = Exec(b, new Environment(env), out var returned);
                return returned ? r : null;
            }

            case TirReturn:
            case TirIf:
            case TirLoop:
            {
                var r = Exec(node, env, out var returned);
                return returned ? r : null;
            }

            case TirUnresolved u:
                throw new EvaluationException($"unresolved expression '{u.Original?.Name}': {u.Reason}");

            default:
                throw new EvaluationException($"unknown TIR node {node.GetType().Name}");
        }
    }

    // --- statements ----------------------------------------------------------

    /// <summary>Runs a statement; <paramref name="returned"/> says whether a <c>return</c> fired,
    /// in which case the result is its value.</summary>
    private object Exec(TirNode stmt, Environment env, out bool returned)
    {
        returned = false;
        switch (stmt)
        {
            case null:
                return null;

            case TirBlock b:
                foreach (var s in b.Statements)
                {
                    var r = Exec(s, env, out returned);
                    if (returned)
                        return r;
                }
                return null;

            case TirReturn ret:
                returned = true;
                return Evaluate(ret.Value, env);

            case TirIf iff:
                return AsBoolean(Evaluate(iff.Condition, env), "an if condition")
                    ? Exec(iff.IfTrue, env, out returned)
                    : Exec(iff.IfFalse, env, out returned);

            case TirLoop loop:
                while (AsBoolean(Evaluate(loop.Condition, env), "a while condition"))
                {
                    var r = Exec(loop.Body, env, out returned);
                    if (returned)
                        return r;
                }
                return null;

            default:
                Evaluate(stmt, env);
                return null;
        }
    }

    private object EvalBody(TirNode body, Environment env)
    {
        if (body is TirBlock || body is TirReturn || body is TirIf || body is TirLoop)
        {
            var r = Exec(body, env, out var returned);
            if (!returned)
                throw new EvaluationException("a block body ended without returning");
            return r;
        }
        return Evaluate(body, env);
    }

    // --- function application -----------------------------------------------

    private object ApplyCall(TirCall call, object[] args, Environment env)
    {
        if (call.Callee == null)
            return ApplySyntactic(call, args, env);
        return Apply(call.Callee, args, call);
    }

    /// <summary>Applies a resolved function to evaluated arguments.</summary>
    private object Apply(FunctionDef f, IReadOnlyList<object> args, TirCall site)
    {
        switch (f.FunctionType)
        {
            case FunctionType.Field:
                return ApplyField(f, args);

            case FunctionType.Constructor:
                return Construct(f.OwnerType, site?.ReturnType ?? site?.Type ?? f.OwnerType.ToTypeExpression(), args);

            case FunctionType.Cast:
                return args[0];

            case FunctionType.SumFactory:
            {
                var sumCase = f.OwnerType.Cases.First(c => c.Name == f.Name);
                return new PlatoSum(f.OwnerType.ToTypeExpression(), sumCase, args.ToArray());
            }

            case FunctionType.SumEquality:
                return f.Name == "Equals"
                    ? ValueEquality.Equals(args[0], args[1])
                    : !ValueEquality.Equals(args[0], args[1]);

            case FunctionType.Intrinsic:
                return ApplyHost(f, args);

            case FunctionType.Interface:
                return DispatchInterface(f, TypeRefReceiverAsDefault(f, args), site);

            case FunctionType.Library:
            {
                args = TypeRefReceiverAsDefault(f, args);
                var g = Redispatch(f, ref args);
                if (!ReferenceEquals(g, f))
                    return Apply(g, args, site);
                return f.Body == null ? ApplyHost(f, args) : InvokeBody(f, args);
            }

            default:
                throw new EvaluationException($"cannot apply a {f.FunctionType} function '{f.Name}'");
        }
    }

    private object ApplyHost(FunctionDef f, IReadOnlyList<object> args)
    {
        if (Host.TryGet(f, out var impl))
        {
            try
            {
                return impl(this, args);
            }
            catch (EvaluationException) { throw; }
            catch (Exception e) when (IsRuntimeFault(e))
            {
                throw new EvaluationException($"{HostIntrinsics.KeyOf(f)}: {e.Message}", e);
            }
        }
        throw new HostObligationException(HostIntrinsics.KeyOf(f));
    }

    private object ApplyField(FunctionDef f, IReadOnlyList<object> args)
    {
        var receiver = args[0];
        if (receiver is PlatoRecord r)
        {
            var idx = r.IndexOf(f.Name);
            if (idx >= 0)
                return r.Fields[idx];
            // The compiler-generated single-field cast (`Number(arg: Angle)`): typed Field by the
            // symbol factory, named after the field's type, reading the one field.
            if (r.Def.Fields.Count == 1 && r.Def.Fields[0].Type?.Def?.Name == f.Name)
                return r.Fields[0];
        }
        if (receiver is PlatoSum s)
        {
            var idx = Array.FindIndex(s.Case.Fields.ToArray(), cf => cf.Name == f.Name);
            if (idx >= 0)
                return s.Fields[idx];
        }
        throw new EvaluationException($"no field '{f.Name}' on {TypeOf(receiver)}");
    }

    /// <summary>Runs a bodied function: binds its parameters, evaluates the body, and coerces the
    /// result to the declared return type (the return-boundary conversions the checker admits,
    /// a tuple becoming the struct of the same shape, are not TIR nodes).</summary>
    public object InvokeBody(FunctionDef f, IReadOnlyList<object> args)
    {
        var tir = GetTir(f) ?? throw new HostObligationException(Signature(f));
        if (args.Count != tir.Parameters.Count)
            throw new EvaluationException($"{f.Name} takes {tir.Parameters.Count} arguments, got {args.Count}");

        var self = args.Count > 0 ? ReceiverType(f, args[0]) : null;
        var env = new Environment(null, self);
        for (var i = 0; i < args.Count; i++)
        {
            env.Define(tir.Parameters[i], args[i]);
            if (!ReferenceEquals(tir.Parameters[i], f.Parameters[i]))
                env.Define(f.Parameters[i], args[i]);
        }

        if (Deadline != null && DateTime.UtcNow > Deadline.Value)
            throw new EvaluationException("time budget exceeded");
        if (++_depth > MaxDepth)
        {
            _depth = 0;
            throw new EvaluationException($"recursion deeper than {MaxDepth} activations in {f.Name}");
        }
        try
        {
            var result = EvalBody(tir.Body, env);
            return CoerceTo(result, tir.ReturnType);
        }
        catch (EvaluationException e)
        {
            e.PlatoStack.Add(f.Name);
            throw;
        }
        catch (Exception e) when (IsRuntimeFault(e))
        {
            var wrapped = new EvaluationException($"{e.GetType().Name}: {e.Message}", e);
            wrapped.PlatoStack.Add(f.Name);
            throw wrapped;
        }
        finally
        {
            _depth--;
        }
    }

    private static bool IsRuntimeFault(Exception e)
        => e is DivideByZeroException || e is IndexOutOfRangeException || e is ArgumentOutOfRangeException
           || e is InvalidCastException || e is NullReferenceException || e is OverflowException
           || e is ArgumentException || e is KeyNotFoundException || e is InvalidOperationException;

    /// <summary>The type <c>Self</c> denotes inside this activation: the receiver's run-time type
    /// when the first parameter is abstract (Self, an interface, a type variable), else its
    /// declared type.</summary>
    private TypeExpression ReceiverType(FunctionDef f, object receiver)
    {
        var declared = f.Parameters.Count > 0 ? f.Parameters[0].Type : null;
        if (declared?.Def == null || declared.Def.IsInterface() || declared.IsTypeVariable
            || declared.IsTypeParameter || declared.Name == "Self")
            return receiver is TypeToken tk ? tk.Type : TypeOf(receiver);
        return declared;
    }

    private object InvokeValue(object target, object[] args)
    {
        if (target is PlatoFunction fn)
            return fn.Invoke(args);
        throw new EvaluationException($"invoking a non-function value {ValueFormat.Format(target, 60)}");
    }

    private PlatoFunction MakeClosure(TirLambda lam, Environment env)
    {
        var captured = env.Snapshot();
        var ps = lam.Parameters;
        return new PlatoFunction(lam.Type, ps.Count, args =>
        {
            if (args.Count != ps.Count)
                throw new EvaluationException($"a lambda of {ps.Count} parameters was given {args.Count} arguments");
            var e = new Environment(captured);
            for (var i = 0; i < ps.Count; i++)
                e.Define(ps[i], args[i]);
            return EvalBody(lam.Body, e);
        });
    }

    private object LiteralValue(TirLiteral lit)
    {
        switch (lit.LiteralType)
        {
            case LiteralTypesEnum.Integer:
            {
                var l = Convert.ToInt64(lit.Value);
                return lit.Type?.Def?.Name == "Number" ? (object)Numbers.ToNumber(l) : Numbers.I(l);
            }
            case LiteralTypesEnum.Number:
                return Numbers.N(Convert.ToDouble(lit.Value));
            case LiteralTypesEnum.Boolean:
                return (bool)lit.Value;
            case LiteralTypesEnum.String:
                return lit.Value?.ToString() ?? "";
            default:
                throw new EvaluationException($"unknown literal kind {lit.LiteralType}");
        }
    }

    private static bool AsBoolean(object v, string what)
        => v is bool b ? b : throw new EvaluationException($"{what} is not a Boolean: {ValueFormat.Format(v, 60)}");

    public static string Signature(FunctionDef f)
        => $"{f.Name}({string.Join(", ", f.Parameters.Select(p => $"{p.Name}: {p.Type}"))}): {f.ReturnType}";
}
