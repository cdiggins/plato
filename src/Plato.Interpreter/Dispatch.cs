using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Interpreter;

// Run-time dispatch, conversions and value construction: the part of the interpreter that
// decides WHICH function a call reaches once the argument values are known, and how a value
// crosses a type boundary. docs/interpreter.md, "Dispatch" and "Conversions".
public sealed partial class Interpreter
{
    // --- run-time types --------------------------------------------------------

    /// <summary>The run-time type of a value, the one dispatch matches parameters against.</summary>
    public TypeExpression TypeOf(object v)
    {
        switch (v)
        {
            case double: return PrimitiveType("Number");
            case long: return PrimitiveType("Integer");
            case bool: return PrimitiveType("Boolean");
            case char: return PrimitiveType("Character");
            case string: return PrimitiveType("String");
            case PlatoRecord r: return r.Type;
            case PlatoSum s: return s.Type;
            case TypeToken t: return t.Type;
            case PlatoArray a:
                return Instance("Array", a.ElementType);
            case PlatoBuilder b:
                return Instance(b.IsBuffer ? "Buffer" : "List", b.ElementType);
            case PlatoFunction f:
                return f.Type ?? Instance($"Function{f.Arity}");
            case null:
                return PrimitiveType("Any");
            default:
                throw new EvaluationException($"a host value of type {v.GetType().Name} has no Plato type");
        }
    }

    private TypeExpression Instance(string name, TypeExpression arg = null)
    {
        var def = Compilation.GetTypeDef(name);
        if (def == null)
            return PrimitiveType("Any");
        if (arg == null)
            return def.ToTypeExpression();
        var args = def.TypeParameters.Select(tp => (TypeExpression)null).ToArray();
        if (args.Length > 0)
            args[0] = arg;
        return new TypeExpression(def, args.Select(a => a ?? PrimitiveType("Any")).ToArray());
    }

    /// <summary>How well a run-time argument type fits a declared parameter type, by the checker's
    /// tiers: 0 exact, 1 generic, 2 interface, 3 conversion; null when it does not fit.</summary>
    public int? MatchCost(TypeExpression param, TypeExpression arg)
    {
        if (param?.Def == null)
            return 1;
        if (param.IsTypeVariable || param.IsTypeParameter || param.Name == "Self" || param.Name == "Any")
            return 1;
        if (arg?.Def == null)
            return null;
        if (param.Def.IsInterface())
            return ConceptClosure.FindInstance(arg, param.Def.Name) != null ? 2 : (int?)null;
        if (arg.Def.Name == param.Def.Name)
            return 0;
        if (arg.Def.Name == "Integer" && param.Def.Name == "Number")
            return 3;
        if (HasConversion(arg, param))
            return 3;
        return null;
    }

    /// <summary>A user conversion (a one-parameter function named after the target type that takes
    /// the source type) or the compiler's single-field cast exists from one type to another.</summary>
    private bool HasConversion(TypeExpression from, TypeExpression to)
    {
        foreach (var f in FunctionsNamed(to.Def.Name))
            if (f.Parameters.Count == 1 && f.Parameters[0].Type?.Def?.Name == from.Def.Name
                && f.FunctionType != FunctionType.Constructor)
                return true;
        return from.Def.IsConcrete() && from.Def.Fields.Count == 1
               && from.Def.Fields[0].Type?.Def?.Name == to.Def.Name;
    }

    // --- resolution ------------------------------------------------------------

    /// <summary>The best overload of a name for these argument values, by summed tier cost; ties go
    /// to an implementation over an interface declaration, then to a bodied function, then to
    /// declaration order. Arguments needing a conversion come back converted.</summary>
    public FunctionDef ResolveByValues(string name, IReadOnlyList<object> args, out object[] converted,
        bool allowInterfaceDeclarations = true)
    {
        converted = args.ToArray();
        var argTypes = args.Select(TypeOf).ToList();
        FunctionDef best = null;
        var bestCost = int.MaxValue;
        var bestRank = int.MaxValue;
        foreach (var f in FunctionsNamed(name))
        {
            if (f.Parameters.Count != args.Count)
                continue;
            if (!allowInterfaceDeclarations && f.FunctionType == FunctionType.Interface)
                continue;
            var cost = 0;
            var ok = true;
            for (var i = 0; i < args.Count && ok; i++)
            {
                var c = MatchCost(f.Parameters[i].Type, argTypes[i]);
                if (c == null) ok = false;
                else cost += c.Value;
            }
            if (!ok)
                continue;
            var rank = (f.FunctionType == FunctionType.Interface ? 2 : 0) + (f.Body == null && f.FunctionType == FunctionType.Library ? 1 : 0);
            if (cost < bestCost || (cost == bestCost && rank < bestRank))
            {
                best = f;
                bestCost = cost;
                bestRank = rank;
            }
        }
        if (best == null)
            return null;
        for (var i = 0; i < args.Count; i++)
            if (MatchCost(best.Parameters[i].Type, argTypes[i]) == 3)
                converted[i] = CoerceTo(args[i], best.Parameters[i].Type);
        return best;
    }

    /// <summary>A call whose static callee is an interface declaration: re-dispatch on the receiver's
    /// run-time type, as the monomorphizer does once the receiver is concrete. A field of the
    /// receiver's type with the member's name satisfies it; so does a library function over the
    /// concrete type, then one over an interface the type implements; failing those, the members
    /// the C# writer synthesizes from a struct's shape (equality, At and Count over the fields,
    /// components) are supplied here, and a default body on the interface runs last.</summary>
    private object DispatchInterface(FunctionDef decl, IReadOnlyList<object> args, TirCall site)
    {
        var impl = ResolveByValues(decl.Name, args, out var converted, allowInterfaceDeclarations: false);
        if (impl != null)
            return Apply(impl, converted, site);
        var derived = ShapeDerived(decl.Name, args, out var handled);
        if (handled)
            return derived;
        if (decl.Body != null)
            return InvokeBody(decl, args);
        throw new HostObligationException($"{decl.OwnerType?.Name}.{decl.Name} on {TypeOf(args.Count > 0 ? args[0] : null)}");
    }

    /// <summary>A call the elaborator left syntactic (no callee): the sum-type predicates, projections
    /// and factories the match lowering and the qualified case syntax produce, a field read, or a
    /// name resolved against the argument values.</summary>
    private object ApplySyntactic(TirCall call, object[] args, Environment env)
    {
        var name = call.Name;
        if (args.Length >= 1)
        {
            switch (args[0])
            {
                case PlatoSum s when args.Length == 1:
                {
                    if (name == s.Case.PredicateName)
                        return true;
                    foreach (var c in s.Def.Cases)
                        if (name == c.PredicateName)
                            return false;
                    for (var i = 0; i < s.Case.Fields.Count; i++)
                        if (s.Case.Fields[i].FlatName == name)
                            return s.Fields[i];
                    foreach (var c in s.Def.Cases)
                        if (c.Fields.Any(cf => cf.FlatName == name))
                            throw new EvaluationException($"projection '{name}' on a {s.Def.Name}.{s.Case.Name} value");
                    break;
                }
                case TypeToken tk when tk.Type.Def.IsSum:
                {
                    var sumCase = tk.Type.Def.Cases.FirstOrDefault(c => c.Name == name);
                    if (sumCase != null && sumCase.Fields.Count == args.Length - 1)
                        return new PlatoSum(tk.Type, sumCase, args.Skip(1).ToArray());
                    break;
                }
                case PlatoRecord r when args.Length == 1:
                {
                    var idx = r.IndexOf(name);
                    if (idx >= 0)
                        return r.Fields[idx];
                    break;
                }
            }
        }

        var f = ResolveByValues(name, args, out var converted);
        if (f != null)
            return Apply(f, converted, call);

        var derived = ShapeDerived(name, args, out var handled);
        if (handled)
            return derived;

        throw new EvaluationException(
            $"cannot resolve '{name}' for ({string.Join(", ", args.Select(a => TypeOf(a)))})");
    }

    /// <summary>The members the C# writer synthesizes from a struct's declared shape rather than
    /// from Plato source (CSharpConcreteTypeWriter): structural Equals/NotEquals and Hash, At and
    /// Count over the fields (or over the single indexable field), Components and the component
    /// constructors, and the reflective FieldNames/FieldValues/TypeName.</summary>
    private object ShapeDerived(string name, IReadOnlyList<object> args, out bool handled)
    {
        handled = true;
        var receiver = args.Count > 0 ? args[0] : null;
        switch (name)
        {
            case "Equals" when args.Count == 2:
                return ValueEquality.Equals(args[0], args[1]);
            case "NotEquals" when args.Count == 2:
                return !ValueEquality.Equals(args[0], args[1]);
            case "Hash" when args.Count == 1:
                return Numbers.I(ValueEquality.Hash(receiver));
        }
        if (receiver is PlatoRecord r)
        {
            var single = SingleIndexableField(r);
            switch (name)
            {
                case "Count" when args.Count == 1:
                    return single != null ? CountOf(single) : (long)r.Fields.Length;
                case "At" when args.Count == 2:
                    return single != null ? AtOf(single, (long)args[1]) : r.Fields[(int)(long)args[1]];
                case "NumComponents" when args.Count == 1:
                    return (long)r.Fields.Length;
                case "Components" when args.Count == 1:
                    return new PlatoArray(r.Def.Fields.Count > 0 ? r.Def.Fields[0].Type : null, r.Fields.ToArray());
                case "FieldValues" when args.Count == 1:
                    return new PlatoArray(PrimitiveType("Any"), r.Fields.ToArray());
                case "FieldNames" when args.Count == 1:
                    return new PlatoArray(PrimitiveType("String"), r.Def.Fields.Select(f => (object)f.Name).ToArray());
                case "TypeName" when args.Count == 1:
                    return r.Def.Name;
            }
        }
        if (name is "FromComponents" or "CreateFromComponents" && args.Count == 2 && args[1] is PlatoArray xs)
        {
            var type = receiver is TypeToken tk ? tk.Type : TypeOf(receiver);
            if (type?.Def != null && type.Def.IsConcrete() && !type.Def.IsSum)
                return Construct(type.Def, type, xs.Items);
        }
        handled = false;
        return null;
    }

    private static object SingleIndexableField(PlatoRecord r)
        => r.Fields.Length == 1 && (r.Fields[0] is PlatoArray || r.Fields[0] is PlatoRecord) ? r.Fields[0] : null;

    private object CountOf(object v)
        => v is PlatoArray a ? a.Count : Call("Count", v);

    private object AtOf(object v, long i)
        => v is PlatoArray a ? a[(int)i] : Call("At", v, i);

    /// <summary>A bare group name in value position (<c>One</c>, <c>Zero</c>, <c>Epsilon</c>): the
    /// nullary function returning the expected type, or the <c>_</c>-receiver function of it.</summary>
    private object ResolveBareName(string name, TypeExpression expected, Environment env)
    {
        var candidates = FunctionsNamed(name);
        var want = expected?.Name == "Self" ? env.Self : expected;
        foreach (var f in candidates)
            if (f.Parameters.Count == 0 && (want?.Def == null || f.ReturnType?.Def?.Name == want.Def.Name))
                return Apply(f, Array.Empty<object>(), null);
        if (want?.Def != null)
            foreach (var f in candidates)
                if (f.Parameters.Count == 1 && f.Parameters[0].Type?.Def?.Name == want.Def.Name)
                    return Apply(f, new object[] { new TypeToken(want) }, null);
        if (candidates.Count == 1 && candidates[0].Parameters.Count == 0)
            return Apply(candidates[0], Array.Empty<object>(), null);
        throw new EvaluationException($"bare name '{name}' has no nullary meaning as {want}");
    }

    // --- conversions -----------------------------------------------------------

    /// <summary>An explicit coercion the checker recorded: the named conversion function when it
    /// gave one, else the structural rules of <see cref="CoerceTo"/>; a value that fits neither is
    /// a fault, because the checker promised a conversion.</summary>
    private object Coerce(object value, TypeExpression to, IFunction conversion)
    {
        if (to?.Def == null)
            return value;
        var fd = conversion as FunctionDef ?? (conversion as ReifiedFunction)?.Original;
        if (fd != null && fd.FunctionType != FunctionType.Constructor && MatchCost(fd.Parameters[0].Type, TypeOf(value)) is 0 or 1 or 2)
            return Apply(fd, new[] { value }, null);
        var r = TryCoerce(value, to);
        if (r.ok)
            return r.value;
        throw new EvaluationException($"no conversion from {TypeOf(value)} to {to}");
    }

    /// <summary>The structural conversions the checker admits at a boundary, applied when they fit
    /// and the value returned unchanged otherwise: Integer to Number, a tuple to the same-shape
    /// struct, a one-field struct to its field's type, and a user conversion named after the
    /// target.</summary>
    public object CoerceTo(object value, TypeExpression to)
        => TryCoerce(value, to).value;

    private (bool ok, object value) TryCoerce(object value, TypeExpression to)
    {
        if (to?.Def == null || value == null)
            return (true, value);
        if (to.IsTypeVariable || to.IsTypeParameter || to.Name == "Self" || to.Def.IsInterface())
            return (true, value);
        var from = TypeOf(value);
        if (from.Def.Name == to.Def.Name)
            return (true, value);
        if (value is long l && to.Def.Name == "Number")
            return (true, Numbers.ToNumber(l));
        if (value is PlatoRecord r)
        {
            if (IsTuple(r.Def) && to.Def.IsConcrete() && !to.Def.IsSum && to.Def.Fields.Count == r.Fields.Length)
                return (true, Construct(to.Def, to, r.Fields));
            if (r.Def.Fields.Count == 1 && r.Def.Fields[0].Type?.Def?.Name == to.Def.Name)
                return (true, r.Fields[0]);
        }
        foreach (var f in FunctionsNamed(to.Def.Name))
            if (f.Parameters.Count == 1 && f.FunctionType != FunctionType.Constructor
                && MatchCost(f.Parameters[0].Type, from) is 0 or 2)
                return (true, Apply(f, new[] { value }, null));
        return (false, value);
    }

    private static bool IsTuple(TypeDef def)
        => def.Name.StartsWith("Tuple") && def.Name.Length > 5 && char.IsDigit(def.Name[5]);

    // --- construction ------------------------------------------------------------

    /// <summary>A product value from constructor arguments: one per field, or one tuple of the same
    /// shape (the compiler's tuple constructor); each argument coerced to its field's type.</summary>
    public PlatoRecord Construct(TypeDef def, TypeExpression type, IReadOnlyList<object> args)
    {
        if (def.IsSum)
            throw new EvaluationException($"{def.Name} is a sum type; build it with a case");
        if (args.Count == 1 && def.Fields.Count > 1 && args[0] is PlatoRecord tuple && IsTuple(tuple.Def))
            args = tuple.Fields;
        if (args.Count != def.Fields.Count)
            throw new EvaluationException($"{def.Name} has {def.Fields.Count} fields, got {args.Count} values");
        var fields = new object[args.Count];
        for (var i = 0; i < fields.Length; i++)
            fields[i] = CoerceTo(args[i], def.Fields[i].Type);
        return new PlatoRecord(type ?? def.ToTypeExpression(), fields);
    }

    private object NewValue(TypeExpression type, IReadOnlyList<object> args)
    {
        if (type?.Def == null)
            throw new EvaluationException("'new' with no type");
        switch (type.Def.Name)
        {
            case "List":
                return new PlatoBuilder(HostIntrinsics.ElementTypeOf(type, 0), false, 0);
            case "Buffer":
                return new PlatoBuilder(HostIntrinsics.ElementTypeOf(type, 0), true, args.Count > 0 ? (int)(long)args[0] : 0);
        }
        return Construct(type.Def, type, args);
    }

    /// <summary>The <c>default</c> value of a type: zero for the scalars, the empty string and
    /// array, a product of defaults, a sum's first case over defaults; null for a function.</summary>
    public object DefaultValue(TypeExpression type, int depth = 0)
    {
        if (type?.Def == null || depth > 32)
            return null;
        switch (type.Def.Name)
        {
            case "Number": return 0.0;
            case "Integer": return 0L;
            case "Boolean": return false;
            case "Character": return '\0';
            case "String": return "";
            case "Array": return new PlatoArray(HostIntrinsics.ElementTypeOf(type, 0), Array.Empty<object>());
            case "List": return new PlatoBuilder(HostIntrinsics.ElementTypeOf(type, 0), false, 0);
            case "Buffer": return new PlatoBuilder(HostIntrinsics.ElementTypeOf(type, 0), true, 0);
        }
        if (type.Def.IsSum)
        {
            var c = type.Def.Cases[0];
            return new PlatoSum(type, c, c.Fields.Select(f => DefaultValue(f.Type, depth + 1)).ToArray());
        }
        if (type.Def.IsConcrete())
            return new PlatoRecord(type, type.Def.Fields.Select(f => DefaultValue(f.Type, depth + 1)).ToArray());
        return null;
    }
}
