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
        => MatchCost(param, arg, new Dictionary<string, TypeExpression>());

    /// <summary>As <see cref="MatchCost(TypeExpression, TypeExpression)"/>, with the type variables
    /// one parameter list shares: the first argument that mentions a variable (directly, or through
    /// the type arguments of the interface instance it implements) binds it, and a later argument
    /// must fit the binding. <c>Subtract(x: IDifference&lt;$D&gt;, delta: $D)</c> on two Point2D
    /// values does not fit, because Point2D binds $D to Vector2D.</summary>
    private int? MatchCost(TypeExpression param, TypeExpression arg, Dictionary<string, TypeExpression> bindings)
    {
        if (param?.Def == null)
            return 1;
        if (param.IsTypeVariable || param.IsTypeParameter)
        {
            if (bindings.TryGetValue(param.Name, out var bound) && IsBindable(bound))
            {
                var inner = MatchCost(bound, arg, bindings);
                return inner == null ? null : Math.Max(1, inner.Value);
            }
            if (IsBindable(arg))
                bindings[param.Name] = arg;
            return 1;
        }
        if (param.Name == "Self" || param.Name == "Any")
            return 1;
        if (arg?.Def == null)
            return null;
        if (param.Def.IsInterface())
        {
            var instance = ConceptClosure.FindInstance(arg, param.Def.Name);
            if (instance == null)
                return null;
            Unify(param, instance, bindings);
            return 2;
        }
        if (arg.Def.Name == param.Def.Name)
        {
            Unify(param, arg, bindings);
            return 0;
        }
        if (arg.Def.Name == "Integer" && param.Def.Name == "Number")
            return 3;
        if (HasConversion(arg, param))
            return 3;
        return null;
    }

    /// <summary>A type that can stand as a binding: concrete, not a variable, parameter or Any.</summary>
    private static bool IsBindable(TypeExpression t)
        => t?.Def != null && !t.IsTypeVariable && !t.IsTypeParameter && t.Name != "Self" && t.Name != "Any";

    /// <summary>Binds the type variables in a pattern's type arguments to the actual's, positionally.</summary>
    private static void Unify(TypeExpression pattern, TypeExpression actual, Dictionary<string, TypeExpression> bindings)
    {
        var n = Math.Min(pattern.TypeArgs.Count, actual.TypeArgs.Count);
        for (var i = 0; i < n; i++)
        {
            var p = pattern.TypeArgs[i];
            var a = actual.TypeArgs[i];
            if (p?.Def == null || !IsBindable(a))
                continue;
            if (p.IsTypeVariable || p.IsTypeParameter)
                bindings.TryAdd(p.Name, a);
            else
                Unify(p, a, bindings);
        }
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
        => ResolveByValues(name, args, out converted, out _, allowInterfaceDeclarations);

    /// <summary>A resolution outcome, memoized per (name, argument types): the winner, its summed
    /// cost, and the per-argument tier costs that say which arguments need converting.</summary>
    private sealed class Resolution
    {
        public FunctionDef Best;
        public int BestCost;
        public int[] Costs;
    }

    private readonly Dictionary<string, Resolution> _resolutions = new Dictionary<string, Resolution>(StringComparer.Ordinal);
    private readonly Dictionary<TypeExpression, string> _typeKeys = new Dictionary<TypeExpression, string>(ReferenceEqualityComparer.Instance);

    /// <summary>The memo key of a resolution: the name, the interface flag, and each argument's
    /// run-time type. Resolution depends on nothing else, so one outcome serves every call with
    /// the same shape; without this memo a law over a small model spends most of its time
    /// resolving the same few hundred call shapes tens of thousands of times.</summary>
    private string ResolutionKey(string head, IReadOnlyList<TypeExpression> argTypes, bool allowInterfaceDeclarations)
    {
        var sb = new System.Text.StringBuilder(head, 64);
        sb.Append(allowInterfaceDeclarations ? '+' : '-');
        foreach (var t in argTypes)
        {
            if (!_typeKeys.TryGetValue(t, out var key))
                _typeKeys[t] = key = t.ToString();
            sb.Append('|').Append(key);
        }
        return sb.ToString();
    }

    private FunctionDef ResolveByValues(string name, IReadOnlyList<object> args, out object[] converted,
        out int bestCost, bool allowInterfaceDeclarations)
    {
        converted = args.ToArray();
        var argTypes = args.Select(TypeOf).ToList();
        var key = ResolutionKey(name, argTypes, allowInterfaceDeclarations);
        if (!_resolutions.TryGetValue(key, out var r))
        {
            r = Resolve(name, argTypes, allowInterfaceDeclarations);
            _resolutions[key] = r;
        }
        bestCost = r.BestCost;
        if (r.Best == null)
            return null;
        for (var i = 0; i < args.Count; i++)
            if (r.Costs[i] == 3)
                converted[i] = CoerceTo(args[i], r.Best.Parameters[i].Type);
        return r.Best;
    }

    private Resolution Resolve(string name, IReadOnlyList<TypeExpression> argTypes, bool allowInterfaceDeclarations)
    {
        ResolutionCount++;
        var argCount = argTypes.Count;
        FunctionDef best = null;
        var bestCost = int.MaxValue;
        var bestRank = int.MaxValue;
        foreach (var f in FunctionsNamed(name))
        {
            if (f.Parameters.Count != argCount)
                continue;
            if (!allowInterfaceDeclarations && f.FunctionType == FunctionType.Interface)
                continue;
            var costs = CostsOf(f, argTypes);
            if (costs == null)
                continue;
            var cost = costs.Sum();
            var rank = (f.FunctionType == FunctionType.Interface ? 2 : 0) + (f.Body == null && f.FunctionType == FunctionType.Library ? 1 : 0);
            if (cost < bestCost || (cost == bestCost && rank < bestRank))
            {
                best = f;
                bestCost = cost;
                bestRank = rank;
            }
        }
        return new Resolution { Best = best, BestCost = bestCost, Costs = best == null ? null : CostsOf(best, argTypes) };
    }

    /// <summary>The tier cost of each argument against its parameter, with the type variables
    /// bound across the list; null when some argument does not fit.</summary>
    private int[] CostsOf(FunctionDef f, IReadOnlyList<TypeExpression> argTypes)
    {
        var bindings = new Dictionary<string, TypeExpression>();
        var costs = new int[argTypes.Count];
        for (var i = 0; i < argTypes.Count; i++)
        {
            var c = MatchCost(f.Parameters[i].Type, argTypes[i], bindings);
            if (c == null)
                return null;
            costs[i] = c.Value;
        }
        return costs;
    }

    private int? CostOf(FunctionDef f, IReadOnlyList<TypeExpression> argTypes)
        => CostsOf(f, argTypes)?.Sum();

    private static bool IsAbstract(TypeExpression t)
        => t?.Def == null || t.IsTypeVariable || t.IsTypeParameter || t.Name == "Self" || t.Name == "Any" || t.Def.IsInterface();

    /// <summary>A statically resolved callee with an abstract parameter (an interface, a type
    /// variable) was chosen inside a generic body, before the argument types were known. Once the
    /// values are known, a non-interface overload that fits them strictly better takes over, the
    /// decision the monomorphizer makes after substitution; a callee the values do not fit at
    /// all is replaced by whichever one they do fit. Arguments needing a conversion come back
    /// converted.</summary>
    private readonly Dictionary<FunctionDef, bool> _hasAbstractParameter = new Dictionary<FunctionDef, bool>();

    private FunctionDef Redispatch(FunctionDef f, ref IReadOnlyList<object> args)
    {
        if (!_hasAbstractParameter.TryGetValue(f, out var abstractParams))
            _hasAbstractParameter[f] = abstractParams = f.Parameters.Any(p => IsAbstract(p.Type));
        if (!abstractParams)
            return f;
        var argTypes = args.Select(TypeOf).ToList();
        var key = ResolutionKey("#" + f.Id, argTypes, false);
        if (!_resolutions.TryGetValue(key, out var r))
        {
            var own = CostOf(f, argTypes);
            var candidate = Resolve(f.Name, argTypes, allowInterfaceDeclarations: false);
            r = candidate.Best == null || ReferenceEquals(candidate.Best, f) || (own != null && candidate.BestCost >= own.Value)
                ? new Resolution { Best = f, BestCost = own ?? 0, Costs = null }
                : candidate;
            _resolutions[key] = r;
        }
        if (ReferenceEquals(r.Best, f))
            return f;
        var converted = args.ToArray();
        for (var i = 0; i < converted.Length; i++)
            if (r.Costs[i] == 3)
                converted[i] = CoerceTo(converted[i], r.Best.Parameters[i].Type);
        args = converted;
        return r.Best;
    }

    /// <summary>A call written type-level on a named-receiver function (<c>Point2D.Origin</c>,
    /// where <c>Origin(x: IOriginBased&lt;$D&gt;)</c> reads <c>x</c>) receives the type's default
    /// value, as the C# writer's <c>default(Point2D).Origin()</c> does.</summary>
    private IReadOnlyList<object> TypeRefReceiverAsDefault(FunctionDef f, IReadOnlyList<object> args)
    {
        if (args.Count == 0 || args[0] is not TypeToken tk || f.Parameters.Count == 0 || f.Parameters[0].Name == "_")
            return args;
        if (tk.Type?.Def == null || !tk.Type.Def.IsConcrete())
            return args;
        var copy = args.ToArray();
        copy[0] = DefaultValue(tk.Type);
        return copy;
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
        if (TryPrimitiveMember(decl.Name, args, out var member))
            return member;
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
        if (TryPrimitiveMember(name, args, out var member))
            return member;

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

    /// <summary>A member the C# runtime gives a primitive struct directly, with no Plato
    /// declaration behind it (<c>String.Count</c> and <c>String.At</c> discharge the
    /// <c>ICountable</c> and indexing interfaces <c>String</c> declares): the host entry keyed by
    /// the arguments' run-time type names, when the receiver is a primitive.</summary>
    private bool TryPrimitiveMember(string name, IReadOnlyList<object> args, out object result)
    {
        result = null;
        if (args.Count == 0 || TypeOf(args[0]).Def?.IsPrimitiveDeclaration != true)
            return false;
        var key = $"{name}({string.Join(", ", args.Select(a => TypeOf(a).Def?.Name ?? "?"))})";
        if (!Host.TryGet(key, out var impl))
            return false;
        try
        {
            result = impl(this, args);
            return true;
        }
        catch (EvaluationException) { throw; }
        catch (Exception e) when (IsRuntimeFault(e))
        {
            throw new EvaluationException($"{key}: {e.Message}", e);
        }
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
            {
                var element = HostIntrinsics.ElementTypeOf(type, 0);
                return new PlatoBuilder(element, true, args.Count > 0 ? (int)(long)args[0] : 0, DefaultValue(element));
            }
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
