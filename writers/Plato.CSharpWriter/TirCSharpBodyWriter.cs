// Renders typed-IR statements, expressions, and calls as C# function bodies.
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// The SOLE C# function-body writer (the legacy symbol-graph <c>CSharpFunctionBodyWriter</c> was
/// retired at consolidation-plan C4). Renders a C# function <em>body</em> from a monomorphized
/// <see cref="TirFunction"/> (the Typed IR produced by <see cref="Elaborator"/> +
/// <see cref="Monomorphizer"/>, then the writer-side optimizer passes; the last stage of the pipeline in
/// docs/csharp-optimizer-pipeline.md, consuming any TirLoweredLoop/TirTempRef/TirComponentAccess markers
/// the passes left and producing C# text): it maps the recorded
/// <see cref="EmissionKind"/> of every <see cref="TirCall"/> directly to syntax and renders every
/// conversion from an explicit <see cref="TirCoerce"/> node â€” no emit-time semantic re-derivation.
///
/// Type/self/namespace rendering is delegated to the same <see cref="CSharpTypeWriter"/> that emits
/// signatures; the property-getter wrapper / <c>=&gt;</c> / <c>;</c> / trailing-newline framing is
/// reproduced here so the whole member renders in one place.
/// </summary>
public partial class TirCSharpBodyWriter : CodeBuilder<TirCSharpBodyWriter>
{
    private readonly CSharpTypeWriter _tw;
    private readonly TirFunction _tir;
    private readonly string _selfType;
    private readonly CSharpFunctionInfo _fi;

    // Static mode (constants in Constants.g.cs, the IArray library functions in Extensions.g.cs):
    // parameter #0 is emitted by name instead of `this`, and the property-getter framing applies
    // to ZERO-parameter functions (a member needs its receiver parameter, a static does not).
    private readonly bool _isStatic;

    // >0 while rendering a lambda body: parameter #0 is emitted by name, not `this`
    // (the reference lambda body writer runs in "static" mode, isStatic:true).
    private int _lambdaDepth;
    // The names a lowered loop's inlined lambda parameters are declared under (TKT-505): a
    // lambda parameter may shadow an enclosing local, a loop local of the same name may not.
    private readonly Dictionary<ParameterDef, string> _loopParamNames = new();

    public TirCSharpBodyWriter(CSharpTypeWriter tw, TirFunction tir, bool isStatic = false,
        CSharpFunctionInfo fi = null)
    {
        IndentLevel = tw.IndentLevel;
        _tw = tw;
        _tir = tir;
        _selfType = tw.SelfType;
        _isStatic = isStatic;
        _fi = fi;
        WriteFunctionBody();
    }

    private bool IsTypeName(string name)
        => name != null && _tw.Writer.AllTypeNames.Contains(name);

    // See CSharpWriter.IsStructSurfaceProperty (plato-323): the rendering rule resolved against the
    // RECEIVER's own struct surface, so an erased scalar receiver (and any receiver that does not
    // itself carry the name) takes "()".
    private bool IsSurfacePropertyOn(string recvTypeName, string name)
        => _tw.Writer.IsStructSurfaceProperty(recvTypeName, name);

    // Extension style, MOVED bodies only (ExtensionReceiverName != null): a bare name that bound
    // implicitly inside the partial struct must be re-qualified inside the static library class â€”
    // receiver parameter for instance members (plus "()" for moved no-arg methods), the
    // namespace-qualified type name for statics, the namespace for bare type names. Mirrors the
    // FunctionGroupRefSymbol extension branch of the legacy writer exactly. In default mode (and
    // for struct-kept extension-style bodies) the context is null and the name is written bare.
    // Whether a bare STATIC name of the current receiver type is a generated static METHOD
    // (needs "()"); handwritten statics keep member-access syntax.
    private bool IsGeneratedStaticMethodName(string name)
        => _tw.TypeDef != null
           && _tw.Writer.GetExtensionPlanByTypeName(_tw.TypeDef.Name) is ExtensionStylePlan plan
           && plan.GeneratedNoArgStaticNames.Contains(name);

    private void WriteBareName(string name)
    {
        if (_tw.ExtensionReceiverName == null)
        {
            Write(name);
            // A bare generated static of the enclosing type is a method.
            if (IsGeneratedStaticMethodName(name))
                Write("()");
            return;
        }
        if (_tw.ExtensionInstanceNames.Contains(name))
        {
            Write($"{_tw.ExtensionReceiverName}.{name}");
            // Moved no-arg members are classic extension methods, not properties. Under
            // every no-arg member whose name is not pinned to struct-surface
            // property syntax is a method (this also covers the scalar types' members, whose
            // erased receiver is a primitive on which they are all extension methods).
            if (_tw.Writer.MovedNoArgNames.Contains(name)
                || !IsSurfacePropertyOn(_tw.TypeDef?.Name, name))
                Write("()");
            return;
        }
        if (_tw.ExtensionStaticNames.Contains(name))
        {
            Write($"{_tw.ExtensionStaticQualifier}.{name}");
            if (IsGeneratedStaticMethodName(name))
                Write("()");
            return;
        }
        // Type references: namespace-qualified because members of the enclosing static library
        // class can shadow type names.
        if (IsTypeName(name))
        {
            Write($"{_tw.Writer.Namespace}.{name}");
            return;
        }
        // Unknown to the compiler: a handwritten member of the receiver type (Number.Zero).
        Write($"{_tw.ExtensionStaticQualifier}.{name}");
    }

    // Recognizes the normalizer's eta-expansion of a bare member-group reference in value
    // position: `M` -> `(_eta{id}_0, ..., _eta{id}_N) => M(_eta{id}_0, ..., _eta{id}_N)`
    // (Normalizer.EtaExpand, R3). The shape is a lambda whose parameters were all synthesized by
    // that pass (their names carry the `_eta` prefix it hard-codes) and whose body is a single
    // resolved call forwarding those parameters, in order, as the call's arguments (arity >= 1).
    // The reference writer consumes the ORIGINAL (un-normalized) graph, so it emits the bare member
    // name `M`; we recover it here. The `_eta`-name guard keeps a USER-authored forwarding lambda
    // (`(x) => x.M()`) â€” which the reference writer keeps as a lambda â€” from being collapsed.
    private static bool TryGetEtaForwardedName(TirLambda lam, out string name)
    {
        name = null;
        if (lam?.Parameters == null || lam.Parameters.Count == 0)
            return false;
        if (!lam.Parameters.All(p => p.Name != null && p.Name.StartsWith("_eta")))
            return false;
        if (!(lam.Body is TirCall call) || call.Name == null)
            return false;
        if (call.Args.Count != lam.Parameters.Count)
            return false;
        for (var i = 0; i < call.Args.Count; i++)
            if (!(call.Args[i] is TirParameter p) || !ReferenceEquals(p.Def, lam.Parameters[i]))
                return false;
        name = call.Name;
        return true;
    }

    // --- top level: mirror CSharpFunctionBodyWriter's default-mode framing exactly ----------

    // --- local names ---------------------------------------------------------
    //
    // A C# local's scope is the whole block that declares it, before and after the declaration, and a
    // nested scope may not reuse the name (CS0136) nor may one scope declare it twice (CS0128). Plato
    // lets an inner block declare a name the enclosing block declares later, so a let whose name is
    // taken by an enclosing scope, or by an earlier let of its own scope, is written under a fresh name
    // (name_2, name_3, ...). Every reference goes through LocalName, so declaration and uses agree.

    private readonly Dictionary<VariableDef, string> _localNames = new();

    private string LocalName(VariableDef def)
        => def != null && _localNames.TryGetValue(def, out var renamed)
            ? renamed
            : CSharpTypeWriter.Identifier(def?.Name) ?? "?";

    private void PlanLocalNames(TirNode body)
    {
        _localNames.Clear();
        var root = new HashSet<string>(_tir.Parameters.Select(p => p.Name));
        var all = new HashSet<string>(root);
        CollectNames(body, all);
        PlanScopes(body, new List<HashSet<string>> { root }, all);
    }

    private static void CollectNames(TirNode n, HashSet<string> all)
    {
        if (n == null)
            return;
        if (n is TirLet let && let.Def?.Name != null)
            all.Add(let.Def.Name);
        if (n is TirLambda lam)
            foreach (var p in lam.Parameters)
                all.Add(p.Name);
        foreach (var c in n.Children)
            CollectNames(c, all);
    }

    private static void ShallowLets(TirNode n, List<TirLet> lets, bool isRoot)
    {
        if (n == null || (!isRoot && (n is TirBlock || n is TirLambda)))
            return;
        if (n is TirLet let)
            lets.Add(let);
        foreach (var c in n.Children)
            ShallowLets(c, lets, false);
    }

    private void PlanScopes(TirNode n, List<HashSet<string>> enclosing, HashSet<string> all)
    {
        if (n == null)
            return;
        var stack = enclosing;
        if (n is TirBlock || n is TirLambda)
        {
            var mine = new HashSet<string>();
            if (n is TirLambda lam)
                foreach (var p in lam.Parameters)
                    mine.Add(p.Name);
            var lets = new List<TirLet>();
            ShallowLets(n, lets, true);
            foreach (var let in lets)
            {
                var name = let.Def?.Name;
                if (name == null)
                    continue;
                var emitted = name;
                if (mine.Contains(name) || enclosing.Any(s => s.Contains(name)))
                {
                    var i = 2;
                    while (all.Contains($"{name}_{i}"))
                        i++;
                    emitted = $"{name}_{i}";
                    all.Add(emitted);
                    _localNames[let.Def] = emitted;
                }
                mine.Add(emitted);
            }
            stack = new List<HashSet<string>>(enclosing) { mine };
        }
        foreach (var c in n.Children)
            PlanScopes(c, stack, all);
    }

    private void WriteFunctionBody()
    {
        if (_tir?.Body == null)
        {
            WriteLine(CSharpWriter.BodilessStub(_tir?.Original?.Name ?? "?"));
            return;
        }

        // A receiver-only member (1 parameter) or a nullary static is emitted as a property
        // getter â€” the same rule the reference writer applies per isStatic.
        var numParams = _tir.Original?.NumParameters ?? _tir.Parameters.Count;
        var isProp = _isStatic ? numParams == 0 : numParams == 1;
        // The signature declared a METHOD unless the name is pinned to property syntax â€” frame
        // the body to match.
        if (_fi != null && _fi.EmitAsMethod)
            isProp = false;

        // The reference writer hoists lambda-captured references into `var _var{N} = x;` blocks
        // before emitting (RewriteLambdasCapturingVars); mirror it, sharing the same counter.
        var body = TirLambdaCaptureRewriter.Rewrite(_tir.Body);
        PlanLocalNames(body);
        var isBlock = body is TirBlock;

        if (isProp)
            Write($" {{ {CSharpTypeWriter.Annotation} get ");

        if (!isBlock)
        {
            Write(" => ");
            WriteNode(body);
            Write(";");
        }
        else
        {
            WriteStatement(body);
        }

        if (isProp)
            Write(" } ");

        WriteLine();
    }

    // --- statements ----------------------------------------------------------


    private void WriteStatement(TirNode n)
    {
        switch (n)
        {
            case TirInlineRegion region:
                if (Lower && ScalarOf(region, region.Result, null) is { } regionScalar)
                {
                    WriteLine($"{PrimitiveType(regionScalar)} {region.Result.Name};");
                    _primitiveLocals[region.Result] = regionScalar;
                }
                else
                    WriteLine($"{CSharpTypeName(region.Type)} {region.Result.Name};");
                WriteStatement(region.Body);
                WriteLine($"{region.ExitLabel}: ;");
                return;
            case TirInlineExit exitRegion:
                Write($"{exitRegion.Result.Name} = ");
                if (_primitiveLocals.ContainsKey(exitRegion.Result))
                    WritePrimitive(exitRegion.Value);
                else
                    WriteNode(exitRegion.Value);
                WriteLine($"; goto {exitRegion.Result.Name}_exit;");
                return;
            case TirInlineBinding binding:
                if (Lower && ScalarOf(binding, binding.Def, binding.Value) is { } bindingScalar && LowersLet(binding.Value))
                {
                    WritePrimitiveLet(binding.Def, bindingScalar, binding.Value);
                    WriteLine(";");
                    return;
                }
                Write(CSharpTypeName(binding.Type));
                Write($" {CSharpTypeWriter.Identifier(binding.Def.Name)} = ");
                WriteNode(binding.Value);
                WriteLine(";");
                return;
            case TirBlock b:
                WriteStartBlock();
                foreach (var s in b.Statements)
                {
                    if (s == null) continue;
                    if (TirRewrite.IsStatementNode(s))
                    {
                        WriteStatement(s);
                    }
                    else
                    {
                        WriteNode(s);
                        WriteLine(";");
                    }
                }
                WriteEndBlock();
                return;

            case TirReturn r:
                Write("return ");
                WriteNode(r.Value);
                WriteLine(";");
                return;

            case TirLet let:
                if (Lower && ScalarOf(let, let.Def, let.Value) is { } letScalar && LowersLet(let.Value))
                {
                    WritePrimitiveLet(let.Def, letScalar, let.Value);
                    WriteLine(";");
                    return;
                }
                Write("var ");
                Write(LocalName(let.Def));
                Write(" = ");
                WriteNode(let.Value);
                WriteLine(";");
                return;

            case TirIf iff:
                Write("if (");
                WriteCondition(iff.Condition);
                WriteLine(")");
                WriteStatement(iff.IfTrue);
                if (iff.IfFalse != null)
                {
                    WriteLine("else");
                    WriteStatement(iff.IfFalse);
                }
                return;

            case TirGuardedLoop loop:
                WriteGuardedLoop(loop);
                return;

            case TirLoop l:
                Write("while (");
                WriteCondition(l.Condition);
                WriteLine(")");
                WriteStartBlock();
                WriteStatement(l.Body);
                WriteEndBlock();
                WriteLine();
                return;

            case TirLoweredLoop ll:
                WriteLoweredLoop(ll);
                return;

            case TirMultiReduction pair:
                WriteMultiReduction(pair);
                return;

            default:
                // an expression used in statement position
                WriteNode(n);
                return;
        }
    }

    // --- expressions ---------------------------------------------------------

    // A C# indexer takes an int, which an Integer converts to implicitly. A typed index (a
    // product type whose one field is an Integer, such as BrepFaceIndex) converts to Integer, but
    // C# will not chain two user-defined conversions, so the subscript names the Integer step
    // (plato-455). No conversion to int is added to the index types themselves: Plato has none.
    private void WriteIndexArgument(TirNode index)
    {
        var type = TirRewrite.StripCoerce(index)?.Type;
        var def = type?.Def;
        var typedIndex = def != null && type.Name != "Integer" && !def.IsSum
            && def.Fields.Count == 1 && def.Fields[0].Type?.Name == "Integer";
        if (typedIndex && Lower)
        {
            // Lowered: the index's one Integer field, then its value; no conversion operator runs.
            WriteAsInt(() =>
            {
                WritePostfixReceiver(index);
                Write($".{CSharpTypeWriter.Identifier(def.Fields[0].Name)}.Value");
            });
        }
        else if (typedIndex)
            WriteScalarCast("Integer", index, NeedsReceiverParentheses(index));
        else if (Lower && ScalarOf(type) == "Integer")
            WriteAsInt(() => WritePrimitive(index));
        else
            WriteNode(index);
    }

    private static bool NeedsReceiverParentheses(TirNode node)
        => TirRewrite.StripCoerce(node) is TirConditional or TirLambda or TirAssign or TirBooleanChain;

    private void WritePostfixReceiver(TirNode node)
    {
        var wrap = NeedsReceiverParentheses(node);
        if (wrap) Write("(");
        WriteNode(node);
        if (wrap) Write(")");
    }

    /// <summary>The C# name a constructor call writes after <c>new</c>. In extension-style library
    /// classes a type name can be shadowed by a member of the enclosing static class, so it is
    /// qualified with the namespace (legacy rule). A generic type takes its type arguments from the
    /// call's type: <c>new LocatedDifference&lt;_T0&gt;(...)</c>, where the bare name is CS0305.</summary>
    private string ConstructedTypeName(string name, TypeExpression type)
    {
        var qualified = _tw.ExtensionReceiverName != null ? $"{_tw.Writer.Namespace}.{name}" : name;
        if (type?.Def == null || type.TypeArgs.Count == 0 || type.Def.Name != name)
            return qualified;
        var args = type.TypeArgs.Select(TypeArgumentName).ToList();
        return args.Any(a => a == null) ? qualified : $"{qualified}<{string.Join(", ", args)}>";
    }

    /// <summary>The C# delegate type a lambda of this Plato function type converts to, with
    /// every type variable spelled as this function's signature declares it; null when the type is
    /// not a function type or a variable has no such parameter.</summary>
    private string DelegateTypeName(TypeExpression type)
    {
        if (type?.Def == null || !Compiler.Analysis.Linter.IsFunctionTypeName(type.Def.Name))
            return null;
        var args = type.TypeArgs.Select(CSharpTypeName).ToList();
        return args.Count == 0 || args.Any(a => a == null) ? null : $"System.Func<{string.Join(", ", args)}>";
    }

    /// <summary>A type as C#, or null when it mentions a type variable this function's signature
    /// does not declare.</summary>
    private string CSharpTypeName(TypeExpression type)
    {
        if (type?.Def == null)
            return null;
        if (type.IsTypeVariable)
            return TypeArgumentName(type);
        if (type.TypeArgs.Count == 0 || !HasTypeVariable(type))
            return _tw.ToCSharpType(type);
        if (Compiler.Analysis.Linter.IsFunctionTypeName(type.Def.Name))
            return DelegateTypeName(type);
        var full = _tw.ToCSharpType(new TypeExpression(type.Def));
        var open = full.IndexOf('<');
        var head = open < 0 ? full : full.Substring(0, open);
        var args = type.TypeArgs.Select(CSharpTypeName).ToList();
        return args.Any(a => a == null) ? null : $"{head}<{string.Join(", ", args)}>";
    }

    private static bool HasTypeVariable(TypeExpression type)
        => type?.Def != null && (type.IsTypeVariable || type.TypeArgs.Any(HasTypeVariable));

    /// <summary>A type argument as this function's signature spells it: a type variable by the
    /// generic parameter the signature declares for it (<c>$T</c> is <c>_T0</c>), anything else by
    /// its C# type; null when a type variable has no such parameter.</summary>
    private string TypeArgumentName(TypeExpression arg)
    {
        if (arg?.Def == null)
            return null;
        if (!arg.IsTypeVariable)
            return _tw.ToCSharpType(arg);
        var analysis = _fi?.Function?.TypeVariableAnalysis;
        return analysis != null && analysis.TryGetTypeVariable(TypeInstance.Create(arg), out var tv) ? tv.ToString() : null;
    }

    private void WriteArgs(IEnumerable<TirNode> args)
    {
        var first = true;
        foreach (var a in args)
        {
            if (!first)
                Write(", ");
            first = false;
            WriteNode(a);
        }
    }

    private void WriteNode(TirNode n)
    {
        if (Lower && TryWriteLowered(n))
            return;
        switch (n)
        {
            case null:
                return;

            case TirLiteral lit:
                Write($"(({lit.LiteralType}){_tw.Writer.LiteralString(lit.Value)})");
                return;

            case TirParameter p:
            {
                var idx = p.Def?.Index ?? -1;
                var isThis = idx == 0 && !_isStatic && _lambdaDepth == 0;
                // Under scalar erasure the parameter's declared C# type is already the primitive, so
                // the reference is bare; any boundary cast is an explicit TirCoerce node.
                Write(isThis ? "this"
                    : p.Def != null && _loopParamNames.TryGetValue(p.Def, out var loopName) ? loopName
                    : CSharpTypeWriter.Identifier(p.Def?.Name) ?? "?");
                return;
            }

            case TirVariable v:
                Write(LocalName(v.Def));
                return;

            case TirTypeRef t:
                // A raw-TypeExpression reference is namespace-qualified (`Ara3D.Geometry.Number`),
                // a bound type reference is bare (`Self` -> the concrete type name) â€” the two
                // paths of the reference writer.
                if (t.NamespaceQualified && t.TypeDef?.Name != "Self")
                    Write($"{_tw.Writer.Namespace}.{t.TypeDef?.Name ?? "?"}");
                else
                    Write(t.TypeDef?.Name == "Self" ? _selfType : t.TypeDef?.Name ?? "?");
                return;

            case TirLet let:
                // A let in expression position (statement-position lets are handled in
                // WriteStatement); the reference writes the same `var` form.
                Write($"var {LocalName(let.Def)} = ");
                WriteNode(let.Value);
                return;

            case TirDefault:
                Write("default");
                return;

            case TirName nm:
                // A bare multi-overload group reference (`One`, `Zero`): the reference writer
                // emits the bare name and lets C# member lookup bind it (re-qualified in moved
                // extension-style bodies).
                WriteBareName(nm.Name);
                return;

            case TirCall call:
                WriteCall(call);
                return;

            case TirCoerce c:
                // Non-scalar (default) mode: a TirCoerce is a solver-inserted IMPLICIT-widening
                // conversion (the Elaborator only wraps ArgMatchKind.Conversion arguments):
                // Integer->Number, Self->interface, Number->Vector3 broadcast, etc. The reference
                // writer never renders these â€” it writes the inner value and lets C#'s implicit
                // conversion operators widen at the call boundary. Genuine SOURCE-level conversions
                // the programmer wrote arrive as a TirCall with EmissionKind.Conversion (WriteCall).
                WriteNode(c.Inner);
                return;

            case TirInvoke inv:
                WriteNode(inv.Target);
                Write(".Invoke(");
                WriteArgs(inv.Args);
                Write(")");
                return;

            case TirConditional cond:
            {
                // A conditional (or assignment) used as the condition must keep its parentheses:
                // `?:` is right-associative, so `(a ? b : false) ? x : y` written bare parses as
                // `a ? b : (false ? x : y)` and compiles with a different meaning (plato-454).
                var condNode = TirRewrite.StripCoerce(cond.Condition);
                var wrap = condNode is TirConditional || condNode is TirAssign;
                if (wrap) Write("(");
                WriteNode(cond.Condition);
                if (wrap) Write(")");
                Write(" ? ");
                WriteNode(cond.IfTrue);
                Write(" : ");
                WriteNode(cond.IfFalse);
                return;
            }

            case TirNew nw:
                Write($"new {_tw.ToCSharpType(nw.NewType)}(");
                WriteArgs(nw.Args);
                Write(")");
                return;

            case TirArray arr:
            {
                // Pin the element type with an explicit MakeArray<T> when the array's element type
                // is a concrete renderable type. Without it, C# infers T from the arguments, which
                // loses a per-element implicit conversion (e.g. a Vector3-valued element in a
                // Point3D array â€” the case the fixed-array unroller exposes when it replaces the
                // loop lowerer's `new Point3D[n]`, which pinned the type). Fall back to inference
                // for a type-variable / non-renderable element.
                var elem = arr.Type != null && arr.Type.TypeArgs.Count == 1 ? arr.Type.TypeArgs[0] : null;
                var elemName = elem?.Name;
                var renderable = !string.IsNullOrEmpty(elemName) && !elem.IsTypeVariable
                    && (char.IsLetter(elemName[0]) || elemName[0] == '_');
                Write(renderable ? $"Intrinsics.MakeArray<{_tw.ToCSharpType(elem)}>(" : "Intrinsics.MakeArray(");
                WriteArgs(arr.Elements);
                Write(")");
                return;
            }

            case TirAssign asg:
                WriteNode(asg.LValue);
                Write(" = ");
                WriteNode(asg.RValue);
                return;

            case TirLambda lam:
                // The normalizer eta-expands a bare member-group reference used in value position
                // into a forwarding lambda `(_p0..._pN) => M(_p0, ..., _pN)` (Normalizer R3). The
                // reference writer never saw that rewrite â€” it consumes the original graph and
                // emits the bare member name `M`. Recover the bare reference so we match it.
                if (TryGetEtaForwardedName(lam, out var etaName))
                {
                    WriteBareName(etaName);
                    return;
                }
                Write("(");
                Write(string.Join(", ", lam.Parameters.Select(p => CSharpTypeWriter.Identifier(p.Name))));
                Write(") ");
                _lambdaDepth++;
                // The reference constructs a fresh body writer per lambda, whose constructor
                // re-runs the capture hoist on the lambda's own body, and writes " => " only for
                // an expression body. Mirror both (a hoisted lambda body is block-shaped).
                var lamBody = TirLambdaCaptureRewriter.Rewrite(lam.Body);
                if (TirRewrite.IsStatementNode(lamBody))
                {
                    // A C# lambda with a statement body still needs the arrow AND a brace-delimited
                    // block. The inliner / capture hoist can leave EITHER shape inside a lambda: a
                    // TirBlock (which WriteStatement brace-wraps itself) or a bare statement â€” most
                    // often a lone TirReturn, produced when the hoist lifts the lambda's captured
                    // reference into an ENCLOSING block and leaves the tail behind. That bare form
                    // used to render as `(i) return e;`, which does not parse (plato-323: the first
                    // Array-receiver library function to reach this writer, JaggedArray.FromRows,
                    // was one syntax error that masked the whole conformance build).
                    Write("=> ");
                    if (lamBody is TirBlock)
                    {
                        WriteStatement(lamBody);
                    }
                    else
                    {
                        WriteStartBlock();
                        WriteStatement(lamBody);
                        WriteEndBlock();
                    }
                }
                else
                {
                    Write(" => ");
                    WriteNode(lamBody);
                }
                _lambdaDepth--;
                return;

            // Emission-only marker nodes produced by TirComponentUnroller (--optimize, P3.1).
            case TirComponentAccess ca:
                if (ca.CastTo != null)
                {
                    Write($"(({ca.CastTo})");
                    WriteNode(ca.Receiver);
                    Write($".{ca.FieldName})");
                }
                else
                {
                    WriteNode(ca.Receiver);
                    Write($".{ca.FieldName}");
                }
                return;

            case TirConstructorCall cc:
                // In extension-style library classes type names can be shadowed by members of
                // the enclosing static class, so qualify with the namespace (legacy rule).
                Write("new ");
                Write(ConstructedTypeName(cc.TypeName, cc.Type));
                Write("(");
                WriteArgs(cc.Args);
                Write(")");
                return;

            case TirTempRef tr:
                Write(tr.Name);
                return;

            case TirInPlaceSet ips:
            {
                // The element type is named so the value converts to it at the call, as a literal's
                // MakeArray<T> does; a type variable the signature does not declare falls back to inference.
                var elemName = ips.Type != null && ips.Type.TypeArgs.Count == 1 ? CSharpTypeName(ips.Type.TypeArgs[0]) : null;
                Write(elemName != null ? $"Intrinsics.SetAtInPlace<{elemName}>(" : "Intrinsics.SetAtInPlace(");
                WriteNode(ips.Array);
                Write(", ");
                WriteIndexArgument(ips.Index);
                Write(", ");
                WriteNode(ips.Value);
                Write(")");
                return;
            }

            case TirBooleanChain bc:
                for (var i = 0; i < bc.Terms.Count; i++)
                {
                    if (i > 0)
                        Write($" {bc.Op} ");
                    // A term that binds looser than && or || (a conditional, from the short-circuit
                    // rewrite of a predicate's own && and ||; an assignment; a nested chain) keeps its
                    // parentheses: `a ? b : false || c` would parse as `a ? b : (false || c)` (TKT-506).
                    var term = TirRewrite.StripCoerce(bc.Terms[i]);
                    var wrap = term is TirConditional || term is TirAssign || term is TirLambda
                        || (term is TirBooleanChain inner && inner.Op != bc.Op);
                    if (wrap) Write("(");
                    WriteNode(bc.Terms[i]);
                    if (wrap) Write(")");
                }
                return;

            case TirUnresolved u:
                // A call the elaborator could not resolve (bare nullary group ref, local var,
                // unhandled node). Deliberately un-mimicable so it always counts as a mismatch.
                Write($"/*unresolved:{u.Original?.Name}*/");
                return;

            default:
                Write("/*?*/");
                return;
        }
    }


    // Scalar erasure only: the primitive a TIR NODE is known to erase to, when the origin-symbol
    // analysis cannot know it. The one case: component-access markers substituted by the
    // Lowered rendering: an inner node that already renders as a cast to the same scalar primitive
    // makes an enclosing cast to it redundant (collapses `((float)((float)x))` -> `((float)x)`).
    private static bool IsSameScalarCast(TirNode inner, string prim)
        => inner is TirCoerce c && c.ToType?.Name == prim;

    /// <summary>Whether <paramref name="inner"/> emits as C# code whose type is unambiguously the
    /// scalar primitive <paramref name="to"/> (either the primitive name like "float" or its
    /// wrapper "Number"), so a same-type cast around it is pure redundant noise. Conservative â€” only
    /// cases whose EMITTED type is provably the primitive regardless of surrounding overloads:
    ///   * a native scalar literal;
    ///   * a real erased component field the unroller produced (`TirComponentAccess` with no wrapper
    ///     cast, carrying its <see cref="TirComponentAccess.ScalarComponentPrim"/>);
    ///   * an erased parameter that is NOT the `this` receiver of a scalar WRAPPER-struct instance
    ///     member â€” there `this` is the wrapper (`Number`), and its cast pins a broadcast overload
    ///     (`Number.Multiply(Vector2)` vs `(float)`); loop-temp lambda params (rendered under a
    ///     lambda) and ordinary erased params are fine.
    /// Excluded: pseudo-field method calls (`Vector2.X` â†’ wrapper), `TirVariable` lets, and any
    /// other node whose emitted type is context-dependent.</summary>
    private bool RendersAsPrimitive(TirNode inner, string to)
    {
        var prim = CSharpWriter.ScalarPrimitives.TryGetValue(to, out var p) ? p : to;
        bool ErasesTo(TypeExpression t)
        {
            var name = t?.Def?.Name ?? t?.Name;
            if (name == null) return false;
            return name == prim || (CSharpWriter.ScalarPrimitives.TryGetValue(name, out var q) && q == prim);
        }
        switch (inner)
        {
            case TirLiteral:
                return true;
            case TirComponentAccess ca:
                return ca.CastTo == null && ca.ScalarComponentPrim == prim;
            case TirParameter par:
                var isThisReceiver = par.Def?.Index == 0 && !_isStatic && _lambdaDepth == 0;
                return !isThisReceiver && ErasesTo(par.Type);
            case TirCall call when call.EmissionKind == Ara3D.Geometry.Compiler.Checking.EmissionKind.Operator:
                // A scalar OPERATOR (Add/Subtract/Multiply/...) whose result erases to the primitive:
                // its emitted type is that primitive (a broadcast operator would have a vector result
                // type, so ErasesTo fails and its cast is kept).
                return ErasesTo(call.Type);
            default:
                return false;
        }
    }

    // Render `(({cast})inner)`, parenthesizing a low-precedence inner (a conditional would
    // otherwise bind the cast to its condition â€” see the TirCoerce case).
    private void WriteScalarCast(string cast, TirNode inner, bool innerParens)
    {
        Write($"(({cast})");
        if (innerParens) Write("(");
        WriteNode(inner);
        if (innerParens) Write(")");
        Write(")");
    }

    // The scalar WRAPPER for an erased primitive (float -> Number) â€” used to restore wrapper-ness at
    // a broadcast boundary where C# will not chain float -> Number -> {struct} implicitly.
    private static string WrapperOfPrim(string prim)
        => CSharpWriter.ScalarPrimitives.FirstOrDefault(kv => kv.Value == prim).Key;

    // Emits a call whose callee declares a `_` (ignored, type-level) receiver as the C# STATIC it
    // is: `{ns}.{ReceiverType}.{Name}(rest)`. Returns false when the call is not type-level or the
    // receiver type cannot be named, leaving the generic member form to handle it.
    private bool TryWriteTypeLevelCall(TirCall call)
    {
        var callee = call.Callee;
        var args = call.Args;
        if (callee == null || args.Count == 0 || callee.NumParameters != args.Count)
            return false;
        if (callee.GetParameterName(0) != "_")
            return false;
        // The IArrayLike scaffolding (NumComponents / CreateFromComponents / CreateFromComponent /
        // Components) declares a `_` receiver but the emitter generates NO static for it â€” the
        // handwritten runtime provides it in receiver form (`v.NumComponents()`). Same for every
        // other name on the "implemented elsewhere" list.
        if (CSharpWriter.IgnoredFunctions.Contains(call.Name))
            return false;
        // A receiver written as the type itself is already static-shaped.
        if (TirRewrite.StripCoerce(args[0]) is TirTypeRef)
            return false;

        var recvTypeName = TirRewrite.StripCoerce(args[0])?.Type?.Name
                           ?? (call.ParameterTypes.Count > 0 ? call.ParameterTypes[0]?.Name : null);
        // A receiver typed as the raw primitive (float, at the handwritten-intrinsics boundary)
        // still finds its STATIC on the wrapper struct (Ara3D.Geometry.Number.Pi), which is where
        // the Plato.Intrinsics surface declares it.
        if (recvTypeName != null && CSharpWriter.ScalarPrimitives.ContainsValue(recvTypeName))
            recvTypeName = WrapperOfPrim(recvTypeName);
        if (recvTypeName == null || !_tw.Writer.IsConcreteTypeName(recvTypeName))
            return false;

        Write($"{_tw.Writer.Namespace}.{recvTypeName}.{call.Name}");
        if (args.Count > 1)
        {
            Write("(");
            WriteArgs(args.Skip(1));
            Write(")");
            return true;
        }
        // No further arguments: a GENERATED nullary static emits as a method;
        // a handwritten one (Number.Pi, Number.MinValue) is a static field and keeps member syntax.
        var plan = _tw.Writer.GetExtensionPlanByTypeName(recvTypeName);
        if (plan != null && plan.GeneratedNoArgStaticNames.Contains(call.Name))
            Write("()");
        return true;
    }

    // Emits a call written type-level (`Number3.One`) whose callee is an INSTANCE member, as
    // `default({ns}.{T}).{Name}(rest)`. Returns false unless the receiver is a type reference AND
    // the callee takes a named (value) receiver: a `_` receiver is genuinely type-level and emits
    // as a static, handwritten or generated, which the plain member form already gets right.
    private bool TryWriteTypeRefInstanceCall(TirCall call)
    {
        var callee = call.Callee;
        var args = call.Args;
        if (callee == null || args.Count == 0 || callee.NumParameters != args.Count)
            return false;
        if (callee.GetParameterName(0) == "_")
            return false;
        if (TirRewrite.StripCoerce(args[0]) is not TirTypeRef recvT)
            return false;
        var recvTypeName = recvT.TypeDef?.Name;
        if (recvTypeName == null || !_tw.Writer.IsConcreteTypeName(recvTypeName))
            return false;

        Write($"default({_tw.Writer.Namespace}.{recvTypeName}).{call.Name}(");
        WriteArgs(args.Skip(1));
        Write(")");
        return true;
    }

    private void WriteCall(TirCall call)
    {
        // Under scalar lowering every scalar-returning-wrapper call site already carries an explicit
        // disambiguating TirCoerce (inserted by TirScalarLowerer), so no float-land wrapping here.
        WriteCallCore(call);
    }

    private void WriteCallCore(TirCall call)
    {
        var name = call.Name;
        var args = call.Args;

        // Zero-argument call: the reference emits a Constants.<Name> reference. (The `default`
        // keyword arrives as TirDefault, not here.)
        if (args.Count == 0)
        {
            // Constants are static methods.
            Write($"Constants.{name}()");
            return;
        }

        // Tuple constructor: TupleN(a, b, ...) -> (a, b, ...).
        if (name != null && name.StartsWith("Tuple"))
        {
            Write("(");
            WriteArgs(args);
            Write(")");
            return;
        }

        // Constructor call (`FunctionVolume3D(p => self.Eval(p).Not)`): every argument is a
        // constructor parameter, not a receiver + rest. A TirCall reaches here (rather than the
        // TirConstructorCall node above) whenever it was never rewritten by the inliner/component
        // unroller â€” e.g. it is already the top-level body of the function being written. Without
        // this check it falls into the generic receiver-style call below, which reads args[0] as
        // THE RECEIVER and mangles the call postfix onto it (plato-310:
        // `((p) => ...).FunctionVolume3D()` instead of `new FunctionVolume3D(p => ...)`). Mirrors
        // Plato.CppWriter.TirCppBodyWriter's identical EmissionKind.Constructor case, and reuses
        // the same rendering as the TirConstructorCall node above.
        if (call.EmissionKind == EmissionKind.Constructor && name != null)
        {
            Write("new ");
            Write(ConstructedTypeName(name, call.Type));
            Write("(");
            WriteArgs(args);
            Write(")");
            return;
        }

        if (args[0]?.Type?.Def is { } receiver && _tw.Writer.ExtensionPlans != null
            && _tw.Writer.ExtensionPlans.TryGetValue(receiver, out var fieldPlan)
            && fieldPlan.HasFieldCollision(call.Callee))
        {
            Write($"global::{_tw.Writer.Namespace}.{fieldPlan.FieldCollisionClassName}.{call.Callee.Name}(");
            if (TirRewrite.StripCoerce(args[0]) is TirTypeRef)
            {
                Write($"default(global::{_tw.Writer.Namespace}.{receiver.Name})");
                if (args.Count > 1)
                {
                    Write(", ");
                    WriteArgs(args.Skip(1));
                }
            }
            else
                WriteArgs(args);
            Write(")");
            return;
        }

        // A `_` receiver is Plato's TYPE-LEVEL idiom (`FromOffset(_: Point2D, v: Vector2D): Point2D`,
        // `Pi(_: Number): Number`): the member emits as a C# STATIC (CSharpFunctionInfo.IsStatic),
        // so a VALUE receiver at the call site must be replaced by the receiver's type name â€” C#
        // rejects `p.FromOffset(v)` for a static with CS0176 (472 in the forward stdlib). The
        // ignored receiver argument is dropped. A type-ref receiver already reads as the type and
        // falls through to the generic member form below.
        if (TryWriteTypeLevelCall(call))
            return;

        // The MIRROR case: a TYPE-REF receiver on a member whose callee declares a NAMED (value)
        // receiver. `Zero` / `One` / `Origin` are deliberately INSTANCE obligations - a runtime-arity
        // VectorN takes its arity from the receiver, so the interface is shaped for that implementor
        // (tracker/decisions/2026-07-29-static-concept-members-corrected.md) - yet the stdlib reads
        // them type-level (`Number3.One`, `Point3D.Origin`, `Vector3D.Zero`). Written literally that
        // is a C# method group, so the receiver becomes `default(T)`: for a fixed-arity type the
        // member ignores the receiver's VALUE, which is exactly what `default` supplies.
        if (TryWriteTypeRefInstanceCall(call))
            return;

        // `==` and `!=` on arrays compare values (the interpreter's ValueEquality), but the runtime
        // array is an IReadOnlyList, whose Equals is reference equality and which has no NotEquals
        // (TKT-1040). Intrinsics.ArrayEquals compares counts, then each element with its own
        // equality and a nested array element-wise, as a generated record does for an array field.
        if ((name == "Equals" || name == "NotEquals") && args.Count == 2
            && TirRewrite.StripCoerce(args[0])?.Type?.Name == "Array"
            && TirRewrite.StripCoerce(args[1])?.Type?.Name == "Array")
        {
            Write(name == "Equals" ? "((Boolean)Intrinsics.ArrayEquals(" : "((Boolean)!Intrinsics.ArrayEquals(");
            WriteArgs(args);
            Write("))");
            return;
        }

        // --concrete-arrays: freezing a builder hands over its backing T[] itself when that array is
        // exactly full (always for a Buffer), so the value is an exact T[] for the helper's reads.
        if (_tw.Writer.ConcreteArrays && name == "Freeze" && args.Count == 1
            && TirRewrite.StripCoerce(args[0])?.Type?.Name is "List" or "Buffer")
        {
            WritePostfixReceiver(args[0]);
            Write(".FreezeExact()");
            return;
        }

        // Plato's `Count(xs: Array<$T>): Integer` maps onto the runtime list interface, whose
        // Count is a BCL `int`. Under the WRAPPER-scalar recipe Integer and int are different
        // types, so any no-arg Plato member chained off the count (`xs.Count.Range`,
        // `xs.Count.ToNumber`) renders as PROPERTY syntax against an `int` receiver - and C#
        // has no extension properties, so nothing can satisfy it. Re-wrap at the source.
        if (name == "Count" && args.Count == 1
            && TirRewrite.StripCoerce(args[0])?.Type?.Name == "Array")
        {
            if (_tw.Writer.ConcreteArrays && IsInterfaceList(args[0]))
            {
                Write($"((Integer){CSharpWriter.ConcreteArrayAccessClass}.Count(");
                WriteNode(args[0]);
                Write("))");
                return;
            }
            Write("((Integer)");
            WritePostfixReceiver(args[0]);
            Write(".Count)");
            return;
        }

        // Indexer: At(a, i, ...) -> a[i, ...]. Generated structs have no indexer,
        // so a receiver of a known generated type calls .At(...) instead (IReadOnlyList and
        // other unknown receivers keep their own indexers).
        if (name == "At")
        {
            var recvType = TirRewrite.StripCoerce(args[0])?.Type?.Name;
            if (recvType != null
                && _tw.Writer.GetExtensionPlanByTypeName(recvType) != null
                && !CSharpWriter.ScalarPrimitives.ContainsKey(recvType))
            {
                WritePostfixReceiver(args[0]);
                Write(".At(");
                WriteArgs(args.Skip(1));
                Write(")");
                return;
            }
            if (_tw.Writer.ConcreteArrays && args.Count == 2 && IsInterfaceList(args[0]))
            {
                Write($"{CSharpWriter.ConcreteArrayAccessClass}.At(");
                WriteNode(args[0]);
                Write(", ");
                WriteIndexArgument(args[1]);
                Write(")");
                return;
            }
            WritePostfixReceiver(args[0]);
            Write("[");
            var firstIndex = true;
            foreach (var index in args.Skip(1))
            {
                if (!firstIndex) Write(", ");
                firstIndex = false;
                WriteIndexArgument(index);
            }
            Write("]");
            return;
        }

        // Member / method-call form: receiver first, then `.Name`, then `(rest)` unless it reads
        // as a property (no-arg member access) or a type-named conversion property. A ternary
        // (or lambda/assignment) receiver must be parenthesized or the member access binds to its
        // last operand; such receivers only arise from the inliner (the flag differentials pin
        // that no non-inline output has them).
        var recvNode = TirRewrite.StripCoerce(args[0]);
        if (recvNode is TirLambda recvLambda && DelegateTypeName(recvLambda.Type) is { } delegateType)
        {
            // A lambda has no type in C# until it is converted to a delegate, so `(x => ...).F()`
            // is CS0023; convert it to the delegate its TIR type names first (TKT-353).
            Write($"(({delegateType})(");
            WriteNode(args[0]);
            Write("))");
        }
        else if (recvNode is TirTypeRef { TypeDef: { IsSum: true } sum } && sum.TypeParameters.Count > 0)
        {
            // A generic sum's case factory is a static on the instantiated struct, and C# has no
            // `Maybe.Just(n)` without the type arguments: the call's result type carries them
            // (`Maybe<Integer>.Just(n)`, TKT-443).
            Write(ConstructedTypeName(sum.Name, call.Type));
        }
        else
        {
            WritePostfixReceiver(args[0]);
        }
        Write(".");
        Write(name);

        var noParens = args.Count == 1
            && (call.EmissionKind == EmissionKind.Property
                || (call.EmissionKind == EmissionKind.Conversion && IsTypeName(name)));
        if (noParens)
        {
            // Extension style: no-arg library functions that moved out of their structs are
            // classic extension METHODS (v.Length()), so their call sites need "()". Applies in
            // ALL extension-style bodies (kept and moved) â€” the moved/kept name partition is
            // global, so this is decidable by name.
            if (_tw.Writer.ExtensionStyle && _tw.Writer.MovedNoArgNames.Contains(name))
            {
                Write("()");
                return;
            }
            // Every no-arg member access is a method call unless the name is
            // pinned to property/field syntax. STATIC member accesses (type-ref receiver) are
            // decided by the target type's plan: generated statics are methods, handwritten
            // statics (Number.MinValue) keep member syntax.
            if (TirRewrite.StripCoerce(args[0]) is TirTypeRef recvT)
            {
                var recvPlan = _tw.Writer.GetExtensionPlanByTypeName(recvT.TypeDef?.Name);
                if (recvPlan != null && recvPlan.GeneratedNoArgStaticNames.Contains(name))
                    Write("()");
            }
            else if (!IsSurfacePropertyOn(TirRewrite.StripCoerce(args[0])?.Type?.Name, name))
                Write("()");
            return;
        }

        // Every scalar disambiguation is an explicit TirCoerce the lowerer inserted; the printer
        // just writes each argument (TirCoerce renders as the cast in WriteNode).
        Write("(");
        var argIndex = 1;
        foreach (var a in args.Skip(1))
        {
            if (argIndex > 1)
                Write(", ");
            WriteNode(a);
            argIndex++;
        }
        Write(")");
    }

}
