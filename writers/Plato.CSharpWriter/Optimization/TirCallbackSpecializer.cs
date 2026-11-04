// Removes the delegate from a call whose function-typed argument is a lambda literal by cloning the callee per call shape (--specialize-callbacks).
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Compiler.Analysis;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>A call to a clone the callback specializer made (an emission-only marker, like
/// <see cref="TirComponentAccess"/>). Renders as <c>CallbackSpecializations.Name(args)</c>; the later
/// passes see an opaque node with ordinary children.</summary>
public sealed class TirSpecializedCall : TirNode
{
    public string Name { get; }
    public IReadOnlyList<TirNode> Args { get; }

    public TirSpecializedCall(string name, IReadOnlyList<TirNode> args, TypeExpression type, Symbol origin)
        : base(type, origin)
    {
        Name = name;
        Args = args;
    }

    public override IEnumerable<TirNode> Children => Args;
    public override TirNode WithChildren(IReadOnlyList<TirNode> children)
        => new TirSpecializedCall(Name, children.ToList(), Type, Origin);
    public override string ToString() => $"{Name}!({string.Join(", ", Args)})";
}

/// <summary>One clone: a callee with some function-typed parameters replaced by a lambda body and the
/// lambda's captured values appended as parameters. Emitted by <see cref="CallbackSpecializationWriter"/>.</summary>
public sealed class CallbackClone
{
    public string Key;
    public string Name;
    public FunctionDef Callee;
    public string ReceiverTypeName;
    public TypeDef ReceiverType;
    public ExtensionStylePlan Plan;
    public CSharpFunctionInfo Function;
    public TirFunction Tir;
    public IReadOnlyList<string> ParameterTypes;
    public string ReturnType;
    public int Depth;
}

/// <summary>The clone registry of one generation: a clone per (callee, receiver type, lambda shape with
/// captures abstracted), so one clone serves every call site with the same shape. Filled while bodies are
/// optimized, including the clones' own bodies, and emitted at the end of <c>WriteAll</c>.</summary>
public sealed class CallbackSpecializations
{
    public const string ClassName = "CallbackSpecializations";
    public const string FileName = ClassName + ".g.cs";
    /// <summary>A clone whose body asks for another clone is served up to this depth, so a callback threaded
    /// through a chain of forwarding callees becomes delegate-free along the whole chain.</summary>
    public const int MaxDepth = 6;

    private readonly Dictionary<string, CallbackClone> _byKey = new();
    private readonly List<CallbackClone> _clones = new();
    private readonly HashSet<string> _names = new();
    private readonly Dictionary<object, int> _ids = new(ReferenceEqualityComparer.Instance);

    public IReadOnlyList<CallbackClone> Clones => _clones;
    public int SitesSpecialized;
    public readonly SortedDictionary<string, int> Refusals = new(System.StringComparer.Ordinal);

    /// <summary>A run-local id for a symbol, so a key names the resolved callee and not just its name.</summary>
    public int Id(object symbol)
    {
        if (!_ids.TryGetValue(symbol, out var id))
            _ids[symbol] = id = _ids.Count;
        return id;
    }

    public CallbackClone TryGet(string key)
        => _byKey.TryGetValue(key, out var clone) ? clone : null;

    /// <summary>Registers a clone under its key; a name another shape already took gets a suffix.</summary>
    public void Add(CallbackClone clone)
    {
        var name = clone.Name;
        for (var n = 2; !_names.Add(name); n++)
            name = $"{clone.Name}_{n}";
        clone.Name = name;
        _byKey[clone.Key] = clone;
        _clones.Add(clone);
    }

    /// <summary>Takes back a clone registered while a build that then failed.</summary>
    public void Remove(CallbackClone clone)
    {
        _byKey.Remove(clone.Key);
        _clones.Remove(clone);
        _names.Remove(clone.Name);
    }

    public void Refuse(string reason)
    {
        Refusals.TryGetValue(reason, out var n);
        Refusals[reason] = n + 1;
    }
}

/// <summary>
/// Pass 1c of the C# optimizer (<c>--specialize-callbacks</c>; plato-brep "lambda removal L2"). A call
/// whose argument for a function-typed parameter is a lambda literal costs a delegate allocation and an
/// indirect call per invocation when the callee is one the inliner refuses (a statement body, most often).
/// This pass replaces such a call with a call to a CLONE of the callee in which the parameter is gone, every
/// invocation of it is the lambda body (β-reduced), and the values the lambda captured arrive as extra
/// parameters. Clones are shared per (callee, receiver type, lambda shape with captures abstracted) through
/// <see cref="CallbackSpecializations"/>, named <c>{Callee}_{hash of the shape}</c> so an unrelated edit does
/// not rename them, and emitted as static methods of <c>CallbackSpecializations.g.cs</c>. A clone's body runs
/// through the same passes, so a callback the callee forwards to another function-typed parameter is
/// specialized again, to <see cref="CallbackSpecializations.MaxDepth"/>.
///
/// Refused (the call stays as it was): the callee has no ground TIR for the receiver, is kept in its struct,
/// is generic, type-level or recursive on itself; the parameter is used anywhere but as an invocation target
/// or an argument of a resolved call (stored, returned, let-bound, put in a tuple or a constructor); the
/// lambda has a statement body, a bare name, a type variable, or a result type its delegate converts; or a
/// β-reduction would evaluate an invocation argument more than once. Runs after the inliner, which already
/// β-reduces a lambda into an expression-bodied callee; the interpreter never sees this pass.
/// </summary>
public static class TirCallbackSpecializer
{
    public static TirFunction Rewrite(TirFunction tir, CSharpWriter writer, string ownerTypeName)
    {
        if (!writer.SpecializeCallbacks || tir?.Body == null || writer.ExtensionPlans == null)
            return tir;
        var context = new Context(writer, tir, ownerTypeName);
        var body = TirRewrite.Rewrite(tir.Body, n => n is TirCall call ? context.TrySpecialize(call) ?? n : n);
        return ReferenceEquals(body, tir.Body)
            ? tir
            : new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body, tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    private sealed class Capture
    {
        public object Def;
        public TirNode Node;
        public TypeExpression Type;
        public ParameterDef Parameter;
    }

    private sealed class Context
    {
        private readonly CSharpWriter _writer;
        private readonly TirFunction _caller;
        private readonly string _owner;
        private readonly CallbackSpecializations _registry;

        public Context(CSharpWriter writer, TirFunction caller, string owner)
        {
            _writer = writer;
            _caller = caller;
            _owner = owner;
            _registry = writer.Specializations;
        }

        private static readonly bool Trace = System.Environment.GetEnvironmentVariable("PLATO_SPECIALIZE_TRACE") == "1";

        private TirNode Refuse(string reason, TirCall call = null, string receiverTypeName = null)
        {
            _registry.Refuse(reason);
            if (Trace)
                System.Console.Error.WriteLine($"specialize refused: {reason} | {call?.Callee?.OwnerType?.Name}.{call?.Name} on {receiverTypeName} in {_owner}.{_caller.Original?.Name}");
            return null;
        }

        public TirNode TrySpecialize(TirCall call)
        {
            var callee = call.Callee;
            if (call.Args.Count < 2)
                return null;
            var lambdaAt = new List<int>();
            for (var i = 0; i < call.Args.Count; i++)
                if (TirRewrite.StripCoerce(call.Args[i]) is TirLambda)
                    lambdaAt.Add(i);
            if (lambdaAt.Count == 0)
                return null;
            if (callee == null)
                return Refuse("callee unresolved", call);
            if (callee.Body == null)
                return Refuse("callee has no body", call);
            // A law or a witness runs once per trial; cloning its callees would only grow the output.
            if (_caller.Original?.Name != null && (_caller.Original.Name.StartsWith("Law_") || _caller.Original.Name.StartsWith("Witness_")))
                return Refuse("law body", call);
            // --specialize-callbacks-lean: a Check_ body is validation code the mesher never runs, and the
            // library's Check combinators it calls (CheckAll, CheckGrid, ...) re-wrap their callbacks into
            // recursive folds; clones of them were half of the clone file.
            if (_writer.SpecializeCallbacksLean && _caller.Original?.Name != null && _caller.Original.Name.StartsWith("Check_"))
                return Refuse("validation body", call);
            // A combinator the loop lowerer or the component unroller consumes with its lambda in place
            // (a loop, a fan-out) does better than a clone of the stdlib body.
            if (LoweredCombinators.Contains(callee.Name))
                return Refuse("combinator lowered in place", call);

            if (_writer.SpecializationDepth >= CallbackSpecializations.MaxDepth)
                return Refuse("clone depth");
            if (ReferenceEquals(callee, _caller.Original))
                return Refuse("callee recursive");
            var receiverTypeName = TirInliner.TrustedTypeName(_caller, call.Args[0], _writer, _owner);
            if (receiverTypeName == null)
                return Refuse("receiver type unknown");
            var refused = Resolve(callee, receiverTypeName, out var calleeTir, out var fi, out var plan, out var receiverType);
            if (refused != null)
                return Refuse(refused, call, receiverTypeName);
            var paramDefs = calleeTir.Parameters;
            if (paramDefs == null || paramDefs.Count != call.Args.Count || fi.ParameterTypes.Count != paramDefs.Count)
                return Refuse("arity", call, receiverTypeName);
            if (paramDefs[0]?.Name == "_")
                return Refuse("type-level callee", call, receiverTypeName);
            if (calleeTir.Body.Descendants().OfType<TirCall>().Any(c => ReferenceEquals(c.Callee, callee)))
                return Refuse("callee recursive", call, receiverTypeName);
            foreach (var i in lambdaAt)
            {
                var type = paramDefs[i]?.Type;
                if (type?.Def == null || !Compiler.Analysis.Linter.IsFunctionTypeName(type.Def.Name))
                    return Refuse("parameter not function-typed", call, receiverTypeName);
                if (!UsesAreConsuming(calleeTir.Body, paramDefs[i]))
                    return Refuse("callback stored or escapes", call, receiverTypeName);
            }
            var typeWriter = new CSharpTypeWriter(_writer, receiverType);
            // A generic callee (Corresponds<T> on Array) is cloned with its body as elaborated, open in the
            // type variables, under a signature spelled from the call site's concrete types. A scalar bound
            // to a type variable would be a wrapper in the body and a lowered primitive at the boundary,
            // so those stay generic.
            IReadOnlyList<string> siteTypes = null;
            var returnType = fi.ReturnType;
            if (fi.AllGenerics.Count > 0)
            {
                var spelled = new List<string>();
                for (var i = 0; i < paramDefs.Count; i++)
                {
                    // A specialized callback leaves the signature; the others keep their spelling.
                    if (lambdaAt.Contains(i) || !HasTypeVariable(paramDefs[i]?.Type))
                    {
                        spelled.Add(fi.ParameterTypes[i]);
                        continue;
                    }
                    var site = TirRewrite.StripCoerce(call.Args[i])?.Type;
                    var csharp = site == null || HasTypeVariable(site) ? null : typeWriter.ToCSharpType(site);
                    if (csharp == null || csharp.Contains('$') || MentionsScalar(csharp))
                        return Refuse("generic callee over a scalar or an open type", call, receiverTypeName);
                    spelled.Add(csharp);
                }
                returnType = call.Type == null || HasTypeVariable(call.Type) ? null : typeWriter.ToCSharpType(call.Type);
                if (returnType == null || returnType.Contains('$') || (HasTypeVariable(calleeTir.ReturnType) && MentionsScalar(returnType)))
                    return Refuse("generic callee over a scalar or an open type", call, receiverTypeName);
                siteTypes = spelled;
            }

            // The lambda shapes, with captures abstracted; shared captures are abstracted once.
            var captures = new List<Capture>();
            var captureIndex = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
            // The key names the resolved callee, the receiver type and, for a generic callee, the signature
            // the call site spells: the lambda shape `(x, y) => x.Equals(y)` is the same over every element
            // type, and a clone over IReadOnlyList<BrepCurve> cannot serve a call over IReadOnlyList<Vertex>.
            var key = new StringBuilder($"F{_registry.Id(callee)}@{receiverTypeName}");
            var stable = new StringBuilder($"{callee.OwnerType?.Name}.{callee.Name}@{receiverTypeName}");
            if (siteTypes != null)
            {
                var signature = $"<{string.Join(",", siteTypes)}>{returnType}";
                key.Append(signature);
                stable.Append(signature);
            }
            var lambdas = new Dictionary<int, TirLambda>();
            foreach (var i in lambdaAt)
            {
                var lambda = (TirLambda)TirRewrite.StripCoerce(call.Args[i]);
                var refusal = Analyze(lambda, captures, captureIndex, typeWriter, key, stable, i);
                if (refusal != null)
                    return Refuse(refusal, call, receiverTypeName);
                lambdas[i] = lambda;
            }

            var clone = _registry.TryGet(key.ToString());
            if (clone == null)
            {
                var refusal = Build(callee, calleeTir, receiverTypeName, receiverType, plan, fi, siteTypes, returnType,
                    lambdaAt, lambdas, captures, key.ToString(), stable.ToString(), out clone);
                if (refusal != null)
                    return Refuse(refusal, call, receiverTypeName);
                _registry.Add(clone);
            }

            var args = new List<TirNode>();
            for (var i = 0; i < call.Args.Count; i++)
                if (!lambdaAt.Contains(i))
                    args.Add(call.Args[i]);
            foreach (var c in captures)
                args.Add(c.Node);
            _registry.SitesSpecialized++;
            if (Trace)
                System.Console.Error.WriteLine($"specialize accepted: {callee.OwnerType?.Name}.{callee.Name} in {_owner}.{_caller.Original?.Name}");
            return new TirSpecializedCall(clone.Name, args, call.Type, call.Origin);
        }

        /// <summary>The callee's body, signature and emission context, as the writer emits the callee itself:
        /// a member moved out of a concrete type's struct (its ground or open-generic TIR, the type's plan), or
        /// a static library function over a runtime class with no struct (Array, List, Function1: its static
        /// TIR, no plan). Returns a refusal reason or null.</summary>
        private string Resolve(FunctionDef callee, string receiverTypeName, out TirFunction calleeTir,
            out CSharpFunctionInfo fi, out ExtensionStylePlan plan, out TypeDef receiverType)
        {
            calleeTir = null;
            fi = null;
            plan = _writer.GetExtensionPlanByTypeName(receiverTypeName);
            receiverType = _writer.Compilation.GetTypeDef(receiverTypeName);
            if (plan == null || receiverType == null)
            {
                var receiver = callee.Parameters.Count > 0 ? callee.Parameters[0].Type : null;
                if (receiver?.Def == null || receiver.Def.Name != receiverTypeName)
                    return "receiver has no extension plan";
                if (!CSharpWriter.IsListExtensionReceiver(receiver) && !CSharpWriter.IsRuntimeClassExtensionReceiver(receiver))
                    return "receiver has no extension plan";
                if (CSharpWriter.IgnoredFunctions.Contains(callee.Name) || (callee.Body == null && CSharpWriter.IsHostIntrinsic(callee)))
                    return "intrinsic callee";
                plan = null;
                receiverType = null;
                calleeTir = _writer.TryGetStaticTir(callee);
                if (calleeTir?.Body == null)
                    return "no ground TIR";
                fi = new CSharpFunctionInfo(new FunctionInstance(callee, null, null, FunctionInstanceKind.InterfaceExtension), null, _writer.NewDefaultTypeWriter());
                return null;
            }
            // The body the moved member itself is emitted from: ground, or open in the type variables its
            // signature declares.
            calleeTir = _writer.TryGetGroundTirByTypeName(callee, receiverTypeName)
                ?? _writer.TryGetOpenGenericTir(callee, receiverType);
            if (calleeTir?.Body == null)
                return "no ground TIR";
            if (plan.HasFieldCollision(callee))
                return "callee collides with a field";
            var moved = plan.MovedFunctions.FirstOrDefault(f => ReferenceEquals(f.Implementation, callee));
            if (moved != null)
            {
                fi = new CSharpTypeWriter(_writer, receiverType).ToFunctionInfo(moved, plan.IsClosedInstanceMove(moved) ? null : receiverType);
                return null;
            }
            // A member kept in the struct (a concept obligation such as Deform): its clone renders in the
            // same extension context a moved member would, the receiver by name.
            var type = receiverType;
            var concrete = _writer.Compilation.ConcreteTypes.FirstOrDefault(c => ReferenceEquals(c.TypeDef, type));
            var kept = concrete?.ImplementedFunctions.FirstOrDefault(f => ReferenceEquals(f.Implementation, callee));
            if (kept == null)
                return "callee kept in its struct";
            fi = new CSharpTypeWriter(_writer, receiverType).ToFunctionInfo(kept, receiverType);
            if (fi.IsStatic || fi.IsOperator || fi.IsImplicit || fi.IsIndexer)
                return "callee kept in its struct";
            return null;
        }

        /// <summary>The combinators pass 2 (<see cref="TirComponentUnroller"/>) and pass 4
        /// (<see cref="TirLoopLowerer"/>) rewrite with their lambda argument in place.</summary>
        private static readonly HashSet<string> LoweredCombinators = new()
        {
            "Map", "MapEager", "MapIdx", "MapRange", "MapRangeEager", "Zip", "Reduce", "All", "Any", "FlatMap", "Filter",
            "Reverse", "WithNext", "MapPairs", "MapTriplets", "MapQuartets", "Sort", "SortedIndices",
            "MapComponents", "ZipComponents", "AllZipComponents", "AnyZipComponents", "AllComponents", "AnyComponent", "CreateFromComponents",
        };

        private static bool MentionsScalar(string csharpType)
            => System.Text.RegularExpressions.Regex.IsMatch(csharpType, @"\b(Number|Integer|Boolean|Character|String)\b");

        /// <summary>Every reference to <paramref name="pd"/> is the target of an invocation or an
        /// argument of a resolved, non-tuple, non-constructor call (where the clone passes the lambda on, for
        /// the next level to specialize or a combinator lowering to consume). Anything else stores or returns
        /// the callback, and the clone could not represent it.</summary>
        private static bool UsesAreConsuming(TirNode body, ParameterDef pd)
        {
            var total = 0;
            var consuming = 0;
            bool IsRef(TirNode n) => TirRewrite.StripCoerce(n) is TirParameter p && ReferenceEquals(p.Def, pd);
            void Walk(TirNode n)
            {
                if (n == null)
                    return;
                if (n is TirParameter p && ReferenceEquals(p.Def, pd))
                    total++;
                if (n is TirInvoke inv && IsRef(inv.Target))
                    consuming++;
                // The receiver position counts: `eval.CanonicalOnDomain(...)` forwards the callback as well.
                if (n is TirCall c && c.Callee != null && c.EmissionKind != EmissionKind.Constructor
                    && !(c.Name != null && c.Name.StartsWith("Tuple")))
                    for (var i = 0; i < c.Args.Count; i++)
                        if (IsRef(c.Args[i]))
                            consuming++;
                foreach (var child in n.Children)
                    Walk(child);
            }
            Walk(body);
            return total == consuming;
        }

        /// <summary>Checks a lambda can leave its caller and records its captures; appends the shape to the
        /// keys. Returns a refusal reason or null.</summary>
        private string Analyze(TirLambda lambda, List<Capture> captures, Dictionary<object, int> captureIndex,
            CSharpTypeWriter typeWriter, StringBuilder key, StringBuilder stable, int position)
        {
            if (lambda.Body == null || TirRewrite.IsStatementNode(lambda.Body))
                return "statement-bodied lambda";
            var functionType = lambda.Type;
            if (functionType?.Def == null || functionType.TypeArgs.Count != lambda.Parameters.Count + 1)
                return "lambda type unknown";
            var declaredReturn = functionType.TypeArgs[functionType.TypeArgs.Count - 1]?.Name;
            if (declaredReturn == null || lambda.Body.Type?.Name != declaredReturn)
                return "lambda result converts";

            var bound = new HashSet<object>(ReferenceEqualityComparer.Instance);
            foreach (var p in lambda.Parameters)
                bound.Add(p);
            foreach (var n in lambda.Body.Descendants())
            {
                if (n is TirLambda nested)
                {
                    foreach (var p in nested.Parameters)
                        bound.Add(p);
                    if (nested.Origin is Lambda l && l.Function.NumParameters > 0 && l.Function.Parameters[0].Type.IsTypeVariable)
                        return "nested generic lambda";
                }
                if (n is TirLet let && let.Def != null)
                    bound.Add(let.Def);
            }
            var ownerType = _writer.Compilation.GetTypeDef(_owner ?? "");
            foreach (var n in lambda.Body.Descendants())
            {
                // A constructor call renders as `new T(args)` wherever it sits; a member call must exist on
                // its receiver's emitted surface.
                if (n is TirName || n is TirUnresolved || n is TirAssign
                    || (n is TirCall c && c.EmissionKind != EmissionKind.Constructor && !TirInliner.IsCallableOnReceiver(_writer, c)))
                {
                    if (Trace)
                        System.Console.Error.WriteLine($"specialize detail: not self-contained | {n.GetType().Name} {n} : {TirRewrite.StripCoerce((n as TirCall)?.Args[0])?.Type}");
                    return "lambda body not self-contained";
                }
                if (n is TirTypeRef t && t.TypeDef?.Name == "Self" && ownerType == null)
                    return "lambda body names Self";
                if (HasTypeVariable(n.Type))
                    return "lambda body has a type variable";
                object def = n is TirParameter p ? p.Def : n is TirVariable v ? v.Def : null;
                if (def == null || bound.Contains(def) || captureIndex.ContainsKey(def))
                    continue;
                var type = n.Type;
                if (type?.Def == null || HasTypeVariable(type))
                    return "capture type unknown";
                captureIndex[def] = captures.Count;
                captures.Add(new Capture { Def = def, Node = n, Type = type });
            }
            key.Append('|').Append(position).Append('=');
            stable.Append('|').Append(position).Append('=');
            Print(lambda, new Dictionary<object, string>(ReferenceEqualityComparer.Instance), captureIndex, key, stable);
            return null;
        }

        private static bool HasTypeVariable(TypeExpression type)
            => type?.Def != null && (type.IsTypeVariable || type.TypeArgs.Any(HasTypeVariable));

        /// <summary>The shape of a lambda: node kinds, resolved callees, literals, types, lambda parameters by
        /// position and captures by index. <paramref name="key"/> names callees by run-local id (the registry
        /// key); <paramref name="stable"/> by name (hashed into the clone's name).</summary>
        private void Print(TirNode n, Dictionary<object, string> locals, Dictionary<object, int> captureIndex,
            StringBuilder key, StringBuilder stable)
        {
            void Both(string s) { key.Append(s); stable.Append(s); }
            switch (n)
            {
                case null:
                    Both("_");
                    return;
                case TirLambda lam:
                    for (var i = 0; i < lam.Parameters.Count; i++)
                        locals[lam.Parameters[i]] = $"p{locals.Count}";
                    Both("\\(");
                    Both(string.Join(",", lam.Parameters.Select(p => locals[p] + ":" + p.Type?.Name)));
                    Both(")");
                    Print(lam.Body, locals, captureIndex, key, stable);
                    return;
                case TirParameter p when captureIndex.TryGetValue(p.Def, out var c):
                    Both($"${c}:{p.Type}");
                    return;
                case TirVariable v when captureIndex.TryGetValue(v.Def, out var c):
                    Both($"${c}:{v.Type}");
                    return;
                case TirParameter p:
                    Both(locals.TryGetValue(p.Def, out var pName) ? pName : p.Def?.Name);
                    return;
                case TirVariable v:
                    Both(locals.TryGetValue(v.Def, out var vName) ? vName : v.Def?.Name);
                    return;
                case TirLet let:
                    locals[let.Def] = $"v{locals.Count}";
                    Both($"let {locals[let.Def]}=");
                    Print(let.Value, locals, captureIndex, key, stable);
                    return;
                case TirLiteral lit:
                    Both($"L{lit.LiteralType}:{lit.Value}");
                    return;
                case TirTypeRef t:
                    Both($"T{t.TypeDef?.Name}");
                    return;
                case TirCall c:
                    key.Append('C').Append(c.Callee == null ? "?" : _registry.Id(c.Callee).ToString());
                    stable.Append('C').Append(c.Callee?.OwnerType?.Name);
                    Both($":{c.Name}:{c.EmissionKind}:{c.Type}(");
                    break;
                case TirCoerce co:
                    Both($"({co.ToType})");
                    break;
                default:
                    Both($"{n.GetType().Name}:{n.Type}(");
                    break;
            }
            var first = true;
            foreach (var child in n.Children)
            {
                if (!first) Both(",");
                first = false;
                Print(child, locals, captureIndex, key, stable);
            }
            Both(")");
        }

        /// <summary>Builds the clone: the callee body with each lambda (captures rebound to the new
        /// parameters) substituted for its parameter and every invocation β-reduced. Returns a refusal
        /// reason or null.</summary>
        private string Build(FunctionDef callee, TirFunction calleeTir, string receiverTypeName, TypeDef receiverType,
            ExtensionStylePlan plan, CSharpFunctionInfo fi, IReadOnlyList<string> siteTypes, string returnType,
            List<int> lambdaAt, Dictionary<int, TirLambda> lambdas, List<Capture> captures, string key, string stable, out CallbackClone clone)
        {
            clone = null;
            var paramDefs = calleeTir.Parameters;
            var scope = paramDefs[0].Scope;
            var calleeNames = TirInliner.CollectBoundNames(calleeTir, calleeTir.Body);
            for (var k = 0; k < captures.Count; k++)
            {
                captures[k].Parameter = new ParameterDef(scope, $"_cap{k}", captures[k].Type, paramDefs.Count + k);
                calleeNames.Add(captures[k].Parameter.Name);
            }
            var captureParams = captures.ToDictionary(c => c.Def, c => c.Parameter, ReferenceEqualityComparer.Instance);
            var ownerType = _writer.Compilation.GetTypeDef(_owner ?? "");

            var byDef = new Dictionary<ParameterDef, TirNode>();
            foreach (var i in lambdaAt)
            {
                var rebound = TirRewrite.Rewrite(lambdas[i], n =>
                {
                    if (n is TirParameter p && p.Def != null && captureParams.TryGetValue(p.Def, out var cp))
                        return new TirParameter(cp, n.Type, n.Origin);
                    if (n is TirVariable v && v.Def != null && captureParams.TryGetValue(v.Def, out cp))
                        return new TirParameter(cp, n.Type, n.Origin);
                    if (n is TirTypeRef t && t.TypeDef?.Name == "Self" && ownerType != null)
                        return new TirTypeRef(ownerType, t.Type, t.Origin);
                    return n;
                });
                byDef[paramDefs[i]] = TirInliner.AlphaRenameCollidingLambdas(rebound, calleeNames);
            }

            string failure = null;
            // --specialize-callbacks-lean: a lambda that cannot be beta-reduced at an invocation (its parameter
            // is used twice and the argument is not cheap to repeat) becomes a helper method of the same
            // file, called with the argument: C# evaluates the argument once, in the original order.
            var helperOf = new Dictionary<TirNode, int>(ReferenceEqualityComparer.Instance);
            foreach (var i in lambdaAt)
                helperOf[TirRewrite.StripCoerce(byDef[paramDefs[i]])] = i;
            var helpers = new Dictionary<int, CallbackClone>();
            var body = TirRewrite.Rewrite(calleeTir.Body, n =>
            {
                if (n is TirInvoke invoke && TirRewrite.StripCoerce(invoke.Target) is TirLambda target)
                {
                    // In a generic callee the invocation's arguments are typed by its type variables
                    // (`xs[i] : $T`); the lambda body reads them at the call site's concrete types, and the
                    // emitter decides field and method syntax from the receiver's type, so retype them.
                    invoke = RetypeArguments(invoke, target, ref failure);
                    if (failure != null)
                        return n;
                    if (TirRewrite.TryBetaReduce(invoke, out var reduced, CheapToRepeat))
                        return reduced;
                    if (_writer.SpecializeCallbacksLean && helperOf.TryGetValue(target, out var position))
                    {
                        if (!helpers.TryGetValue(position, out var helper))
                        {
                            helper = LambdaHelper(callee, receiverTypeName, receiverType, plan, fi,
                                (TirLambda)target, position, captures, key, stable, ref failure);
                            if (helper != null)
                                helpers[position] = helper;
                        }
                        if (helper != null)
                        {
                            var helperArgs = invoke.Args.ToList();
                            foreach (var c in captures)
                                if (helper.Tir.Parameters.Contains(c.Parameter))
                                    helperArgs.Add(new TirParameter(c.Parameter, c.Type, invoke.Origin));
                            return new TirSpecializedCall(helper.Name, helperArgs, invoke.Type, invoke.Origin);
                        }
                        if (failure != null)
                            return n;
                    }
                    failure = "invocation argument used more than once";
                    if (Trace)
                        System.Console.Error.WriteLine($"specialize detail: {failure} | {callee.OwnerType?.Name}.{callee.Name}: {invoke}");
                    return n;
                }
                if (n is TirParameter p && p.Def != null && byDef.TryGetValue(p.Def, out var lambda))
                    return lambda;
                return n;
            });
            if (failure != null)
            {
                foreach (var helper in helpers.Values)
                    _registry.Remove(helper);
                return failure;
            }

            var parameters = new List<ParameterDef>();
            var zonked = calleeTir.ZonkedParameterTypes == null ? null : new List<TypeExpression>();
            var types = new List<string>();
            for (var i = 0; i < paramDefs.Count; i++)
            {
                if (lambdaAt.Contains(i))
                    continue;
                parameters.Add(paramDefs[i]);
                zonked?.Add(i < calleeTir.ZonkedParameterTypes.Count ? calleeTir.ZonkedParameterTypes[i] : null);
                types.Add(siteTypes != null ? siteTypes[i] : fi.ParameterTypes[i]);
            }
            foreach (var c in captures)
            {
                var csharp = ((ITypeToCSharp)new CSharpTypeWriter(_writer, receiverType)).ToCSharpType(c.Type);
                if (string.IsNullOrEmpty(csharp) || csharp.Contains('$'))
                {
                    foreach (var helper in helpers.Values)
                        _registry.Remove(helper);
                    return "capture type unknown";
                }
                parameters.Add(c.Parameter);
                zonked?.Add(c.Type);
                types.Add(csharp);
            }
            clone = new CallbackClone
            {
                Key = key,
                Name = $"{callee.Name}_{Hash(stable)}",
                Callee = callee,
                ReceiverTypeName = receiverTypeName,
                ReceiverType = receiverType,
                Plan = plan,
                Function = fi,
                Tir = new TirFunction(callee, parameters, calleeTir.ReturnType, body, zonked, calleeTir.ZonkedReturnType),
                ParameterTypes = types,
                ReturnType = returnType,
                Depth = _writer.SpecializationDepth + 1,
            };
            return null;
        }

        /// <summary>The helper method for one lambda of a clone: the lambda's parameters then the captures it
        /// names, its body unchanged, in the callee's extension context. Registered at once (the clone body
        /// calls it by name); <see cref="Build"/> takes it back when the clone then fails. Null with
        /// <paramref name="failure"/> set when a type cannot be spelled.</summary>
        private CallbackClone LambdaHelper(FunctionDef callee, string receiverTypeName, TypeDef receiverType,
            ExtensionStylePlan plan, CSharpFunctionInfo fi, TirLambda lambda, int position,
            List<Capture> captures, string key, string stable, ref string failure)
        {
            var typeWriter = (ITypeToCSharp)new CSharpTypeWriter(_writer, receiverType);
            var signature = lambda.Type.TypeArgs;
            var used = new HashSet<ParameterDef>();
            foreach (var n in lambda.Body.Descendants())
                if (n is TirParameter p && p.Def != null)
                    used.Add(p.Def);
            var parameters = new List<ParameterDef>(lambda.Parameters);
            var types = new List<string>();
            for (var k = 0; k < lambda.Parameters.Count; k++)
                types.Add(typeWriter.ToCSharpType(signature[k]));
            foreach (var c in captures)
                if (used.Contains(c.Parameter))
                {
                    parameters.Add(c.Parameter);
                    types.Add(typeWriter.ToCSharpType(c.Type));
                }
            var returnType = typeWriter.ToCSharpType(signature[signature.Count - 1]);
            if (returnType == null || returnType.Contains('$') || types.Any(t => string.IsNullOrEmpty(t) || t.Contains('$')))
            {
                failure = "open-typed invocation argument";
                return null;
            }
            var helper = new CallbackClone
            {
                Key = $"{key}#L{position}",
                Name = $"{callee.Name}_{Hash($"{stable}#L{position}")}_L{position}",
                Callee = callee,
                ReceiverTypeName = receiverTypeName,
                ReceiverType = receiverType,
                Plan = plan,
                Function = fi,
                Tir = new TirFunction(callee, parameters, signature[signature.Count - 1], lambda.Body),
                ParameterTypes = types,
                ReturnType = returnType,
                Depth = _writer.SpecializationDepth + 1,
            };
            _registry.Add(helper);
            return helper;
        }

        /// <summary>The invocation with each argument whose type still holds a type variable retyped to
        /// the lambda's parameter type (the concrete type the call site spelled). A node kind that cannot
        /// be rebuilt with another type fails the clone.</summary>
        private static TirInvoke RetypeArguments(TirInvoke invoke, TirLambda lambda, ref string failure)
        {
            var signature = lambda.Type?.TypeArgs;
            if (signature == null || signature.Count != invoke.Args.Count + 1)
                return invoke;
            List<TirNode> args = null;
            for (var i = 0; i < invoke.Args.Count; i++)
            {
                var arg = invoke.Args[i];
                var target = signature[i];
                if (arg?.Type == null || !HasTypeVariable(arg.Type) || target?.Def == null || HasTypeVariable(target))
                    continue;
                var retyped = Retype(arg, target);
                if (retyped == null)
                {
                    failure = "open-typed invocation argument";
                    return invoke;
                }
                args ??= invoke.Args.ToList();
                args[i] = retyped;
            }
            return args == null ? invoke : new TirInvoke(invoke.Target, args, invoke.Type, invoke.Origin);
        }

        private static TirNode Retype(TirNode n, TypeExpression type)
            => n switch
            {
                TirCoerce c => Retype(c.Inner, type) is { } inner ? new TirCoerce(inner, c.FromType, type, c.ConversionFn, c.Origin) : null,
                TirCall c => new TirCall(c.Callee, c.EmissionKind, c.ParameterTypes, type, c.Args, type, c.Origin, c.Name),
                TirParameter p => new TirParameter(p.Def, type, p.Origin),
                TirVariable v => new TirVariable(v.Def, type, v.Origin),
                TirInvoke i => new TirInvoke(i.Target, i.Args, type, i.Origin),
                TirConditional c => Retype(c.IfTrue, type) is { } t && Retype(c.IfFalse, type) is { } f
                    ? new TirConditional(c.Condition, t, f, type, c.Origin) : null,
                TirSpecializedCall sc => new TirSpecializedCall(sc.Name, sc.Args, type, sc.Origin),
                _ => null,
            };

        /// <summary>An invocation argument the lambda body may evaluate more than once: the inliner's cheap
        /// projection (leaves, field reads, scalar operators), or one element read <c>xs[i]</c> whose list
        /// and index are such projections (the chordal callees apply their callbacks to <c>cs[j]</c> and
        /// <c>cs[(j + 1) % 3]</c>). Pure, so only the cost repeats, and a load is cheaper than a delegate.</summary>
        private bool CheapToRepeat(TirNode n)
        {
            if (TirInliner.IsCheapProjection(_writer, n))
                return true;
            return TirRewrite.StripCoerce(n) is TirCall { Name: "At" } read && read.Args.Count == 2
                && TirInliner.IsCheapProjection(_writer, read.Args[0]) && TirInliner.IsCheapProjection(_writer, read.Args[1]);
        }

        /// <summary>FNV-1a over the stable shape, eight hex digits: the same shape names the same clone in
        /// every generation, whatever else changed.</summary>
        private static string Hash(string s)
        {
            var h = 2166136261u;
            foreach (var ch in s)
                h = (h ^ ch) * 16777619u;
            return h.ToString("x8");
        }
    }
}
