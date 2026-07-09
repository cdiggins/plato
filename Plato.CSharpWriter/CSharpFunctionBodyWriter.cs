using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;

namespace Ara3D.Geometry.CSharpWriter;

public class CSharpFunctionBodyWriter : CodeBuilder<CSharpFunctionBodyWriter>
{
    public CSharpTypeWriter TypeWriter { get; }
    public CSharpWriter Writer => TypeWriter.Writer;
    public bool IsStaticOrLambda { get; }
    public CSharpFunctionInfo Function { get; }     
    public string SelfType => TypeWriter.SelfType;

    public string Namespace => Writer.Namespace;

    // "propOverride" forces (or suppresses) the property-getter wrapper independently of the
    // isStatic/NumParameters heuristic. Used by ExtensionStyleWriter, which writes moved no-arg
    // members as classic extension METHODS (never properties, propOverride: false) with the
    // body in "static" mode (parameter names, no "this"). Null preserves the original behavior.
    // Scalar erasure only (null otherwise): the EMITTED C# type per parameter definition of
    // the function being written. Lambda parameters often have inferred/generic Plato types,
    // so Def.Type alone cannot decide scalar-ness; the monomorphized emitted type can.
    private Dictionary<ParameterDef, string> _paramEmittedTypes;

    public CSharpFunctionBodyWriter(CSharpTypeWriter typeWriter, CSharpFunctionInfo fi, bool isStatic, bool isLambda, bool? propOverride = null, string lambdaParamPrim = null)
    {
        IndentLevel = typeWriter.IndentLevel;
        TypeWriter = typeWriter;
        IsStaticOrLambda = isStatic;

        Function = fi;

        if (typeWriter.Writer.ScalarErase)
        {
            _paramEmittedTypes = new Dictionary<ParameterDef, string>();
            var impl = fi.Function.Implementation;
            for (var i = 0; i < impl.Parameters.Count && i < fi.ParameterTypes.Count; ++i)
                _paramEmittedTypes[impl.Parameters[i]] = fi.ParameterTypes[i];
            // Lambda whose parameters are known (from the call site) to erase to a primitive.
            if (lambdaParamPrim != null)
                foreach (var p in impl.Parameters)
                    _paramEmittedTypes[p] = lambdaParamPrim;
        }

        if (fi.Body == null)
        {
            WriteLine(" => throw new NotImplementedException();");
            return;
        }

        var isProp = propOverride ?? (isStatic ? fi.NumParameters == 0 : fi.NumParameters == 1);
        if (isProp)
            Write($" {{ {CSharpTypeWriter.Annotation} get ");

        var body = fi.Body;
        // Component-op unrolling (--optimize, roadmap P3.1): rewrite recognized component-HOF
        // call sites before the lambda-capture rewriting (the unrolled result has no lambdas,
        // so RewriteLambdasCapturingVars is a no-op on it). Off by default: with Optimize false
        // this block never runs and the output is byte-identical to the original writer.
        if (Writer.Optimize)
            body = ComponentUnroller.TryUnroll(fi, Writer) ?? body;
        body = body?.RewriteLambdasCapturingVars();
        if (body is Expression)
        {
            Write($" => ").Write(body, fi.ReturnType);
            if (!isLambda)
                Write(";");
        }
        else if (body is BlockStatement)
            Write(body);
        if (isProp)
            Write(" } ");
        if (!isLambda)
            WriteLine();
    }

    public CSharpFunctionBodyWriter Write(IEnumerable<Symbol> symbols)
        => symbols.Aggregate(this, (writer, symbol) => writer.Write(symbol));

    public CSharpFunctionBodyWriter WriteCommaList(IEnumerable<Symbol> symbols)
        => WriteCommaList(symbols, (w, s) => w.Write(s));

    public CSharpFunctionBodyWriter WriteCommaList(IEnumerable<string> symbols)
        => WriteCommaList(symbols, (w, s) => w.Write(s));

    public CSharpFunctionBodyWriter Write(Symbol symbol, string type = null)
    {
        switch (symbol)
        {
            case TypeExpression typeExpression:
                // Scalar erasure: erased scalar types have no generated ".Default" scaffolding;
                // a bare scalar type reference is the primitive's default value.
                if (Writer.ScalarErase && CSharpWriter.ScalarPrimitives.TryGetValue(typeExpression.Name, out var sdPrim))
                    return Write($"default({sdPrim})");
                return Write(typeExpression).Write(".Default");

            case Expression expression:
                return Write(expression, type);

            case Statement statement:
                return Write(statement);
            
            case null:
                return Write("null");
            
            case FieldDef fieldDef:
                return Write("public ").Write(fieldDef.Type).Write(fieldDef.Name).Write(" { get; }").WriteLine();
            
            case FunctionGroupDef memberGroup:
                return Write(memberGroup.Name);
            
            case ParameterDef parameter:
                return Write(parameter.Type).Write(parameter.Name);
            
            case VariableDef variable:
                return Write("var ").Write(variable.Name).Write(" = ").Write(variable.Value).WriteLine(";");
            
            default:
                throw new ArgumentOutOfRangeException(nameof(symbol));
        }
    }

    public static string GetLiteralType(Literal literal)
        => literal.TypeEnum.ToString();

    public static string GetLiteralValue(Literal literal)
        => literal.Value.ToLiteralString();

    //==
    // Scalar erasure (--scalar=float) helpers. All are inert unless Writer.ScalarErase.

    public static bool IsScalarTypeName(string name)
        => name != null && CSharpWriter.ScalarPrimitives.ContainsKey(name);

    /// <summary>
    /// If every overload of the referenced function group declares the SAME scalar wrapper
    /// return type, returns the primitive it erases to; otherwise null. Used to normalize
    /// generated bodies to "float-land": call results that would be wrapper-typed (kept
    /// members, handwritten intrinsics) are cast down to the primitive at the call site.
    /// </summary>
    public static string ScalarReturnPrimitive(Symbol function)
    {
        if (!(function is FunctionGroupRefSymbol fgr) || fgr.Def?.Functions == null || fgr.Def.Functions.Count == 0)
            return null;
        string name = null;
        foreach (var f in fgr.Def.Functions)
        {
            var rt = f.ReturnType?.Name;
            if (rt == null)
                return null;
            if (name == null)
                name = rt;
            else if (name != rt)
                return null;
        }
        return name != null && CSharpWriter.ScalarPrimitives.TryGetValue(name, out var prim) ? prim : null;
    }

    private static readonly HashSet<string> ScalarPrimitiveNames =
        new HashSet<string>(CSharpWriter.ScalarPrimitives.Values);

    /// <summary>
    /// The primitive a scalar-typed parameter erases to, or null when the parameter is not
    /// provably scalar. Checks the declared Plato type first, then the monomorphized emitted
    /// C# type (which catches generic/inferred lambda parameters instantiated at a scalar).
    /// </summary>
    public string ScalarPrimitiveOfParam(ParameterDef def)
    {
        if (def == null)
            return null;
        if (def.Type?.Name != null && CSharpWriter.ScalarPrimitives.TryGetValue(def.Type.Name, out var prim))
            return prim;
        if (_paramEmittedTypes != null && _paramEmittedTypes.TryGetValue(def, out var emitted)
            && ScalarPrimitiveNames.Contains(emitted))
            return emitted;
        return null;
    }

    /// <summary>
    /// Conservative "which primitive is this expression in the erased output" analysis, used
    /// to decide method-call syntax for member accesses on scalars (all scalar members are
    /// extension methods on the primitives under erasure) and to disambiguate arguments.
    /// Returning null only means "unknown": a wrong null produces a loud compile error in the
    /// erased output, never a silent misbinding.
    /// </summary>
    public string ScalarPrimOf(Expression e)
    {
        switch (e)
        {
            case ParameterRefSymbol pr:
                return ScalarPrimitiveOfParam(pr.Def);
            case VariableRefSymbol vr:
                return vr.Def?.Value is Expression init ? ScalarPrimOf(init) : null;
            case Literal lit:
                switch (lit.TypeEnum)
                {
                    case Ara3D.Geometry.AST.LiteralTypesEnum.Number: return "float";
                    case Ara3D.Geometry.AST.LiteralTypesEnum.Integer: return "int";
                    case Ara3D.Geometry.AST.LiteralTypesEnum.Boolean: return "bool";
                    case Ara3D.Geometry.AST.LiteralTypesEnum.String: return "string";
                    default: return null;
                }
            case FunctionCall fc:
            {
                var p = ScalarReturnPrimitive(fc.Function);
                if (p != null)
                    return p;
                // A member call whose receiver and arguments are provably scalar binds an
                // all-scalar-parameter overload (exact match beats every conversion), so its
                // result is that overload's return type.
                if (fc.Args.Count >= 1 && fc.Function is FunctionGroupRefSymbol g)
                {
                    var rprim = ScalarPrimOf(fc.Args[0]);
                    if (rprim == null)
                        // Receiver-type-aware fallback: a member call on a receiver of a KNOWN
                        // non-scalar type whose candidate overloads all MOVED to extension
                        // classes has a fully erased result (e.g. angle.Turns() -> float).
                        return MovedScalarReturnPrim(ReceiverPlatoTypeOf(fc.Args[0]), g.Name, fc.Args.Count);
                    for (var i = 1; i < fc.Args.Count; ++i)
                        if (ScalarPrimOf(fc.Args[i]) == null)
                            return null;
                    var overloads = MatchingScalarOverloads(g.Name, fc.Args.Count, rprim);
                    if (overloads == null || overloads.Count == 0)
                        return null;
                    string ret = null;
                    foreach (var o in overloads)
                    {
                        if (!CSharpWriter.ScalarPrimitives.TryGetValue(o.Return, out var rp))
                            return null;
                        if (ret == null)
                            ret = rp;
                        else if (ret != rp)
                            return null;
                    }
                    return ret;
                }
                return null;
            }
            case FunctionGroupRefSymbol fgr:
                // A bare reference to a no-arg function is a Constants member (see
                // WriteFunctionCallCore); its erased type is its declared return type.
                if (fgr.Def?.Functions != null && fgr.Def.Functions.Count == 1
                    && fgr.Def.Functions[0].NumParameters == 0
                    && fgr.Def.Functions[0].ReturnType?.Name != null
                    && CSharpWriter.ScalarPrimitives.TryGetValue(fgr.Def.Functions[0].ReturnType.Name, out var cprim))
                    return cprim;
                return null;
            case ComponentAccessExpression ca:
                return ca.ScalarComponentPrim;
            case ConditionalExpression cond:
            {
                var a = ScalarPrimOf(cond.IfTrue);
                return a != null && a == ScalarPrimOf(cond.IfFalse) ? a : null;
            }
            default:
                return null;
        }
    }

    public bool IsScalarValued(Expression e) => ScalarPrimOf(e) != null;

    /// <summary>The concrete (non-scalar) Plato type name of a receiver expression, when
    /// determinable; null otherwise. Parameters and Self only - deliberately conservative.</summary>
    public string ReceiverPlatoTypeOf(Expression e)
    {
        switch (e)
        {
            case ParameterRefSymbol pr:
            {
                var n = pr.Def?.Type?.Name;
                if (n == null || n == "Self")
                    n = TypeWriter.TypeDef?.Name;
                if ((n == null || Writer.GetExtensionPlanByTypeName(n) == null)
                    && _paramEmittedTypes != null && pr.Def != null
                    && _paramEmittedTypes.TryGetValue(pr.Def, out var emitted))
                    n = emitted;
                return n;
            }
            case VariableRefSymbol vr:
                return vr.Def?.Value is Expression init ? ReceiverPlatoTypeOf(init) : null;
            default:
                return null;
        }
    }

    /// <summary>When every same-name same-arity candidate member of the given concrete type
    /// MOVED to an extension class (fully erased) and they all return the same scalar,
    /// returns the primitive; null otherwise ("unknown" or wrapper-typed kept member).</summary>
    public string MovedScalarReturnPrim(string receiverTypeName, string name, int argc)
    {
        var plan = Writer.GetExtensionPlanByTypeName(receiverTypeName);
        if (plan == null)
            return null;
        string prim = null;
        var found = false;
        foreach (var f in plan.CandidateFunctions)
        {
            if (f.Name != name || f.ParameterTypes.Count != argc)
                continue;
            found = true;
            if (!plan.ShouldMove(f))
                return null; // kept => wrapper-typed result (or unknown)
            var rt = f.ReturnType?.Name;
            if (rt == null || !CSharpWriter.ScalarPrimitives.TryGetValue(rt, out var rp))
                return null;
            if (prim == null)
                prim = rp;
            else if (prim != rp)
                return null;
        }
        return found ? prim : null;
    }

    /// <summary>All-scalar-parameter overloads of a scalar member name matching the given
    /// arity and receiver primitive (from CSharpWriter.ScalarOverloads).</summary>
    public List<(IReadOnlyList<string> Params, string Return)> MatchingScalarOverloads(string name, int argc, string receiverPrim)
    {
        if (!Writer.ScalarOverloads.TryGetValue(name, out var list))
            return null;
        List<(IReadOnlyList<string>, string)> result = null;
        foreach (var o in list)
        {
            if (o.Params.Count != argc)
                continue;
            if (!CSharpWriter.ScalarPrimitives.TryGetValue(o.Params[0], out var rp) || rp != receiverPrim)
                continue;
            var allScalar = true;
            for (var i = 1; i < o.Params.Count; ++i)
                if (!CSharpWriter.ScalarPrimitives.ContainsKey(o.Params[i]))
                {
                    allScalar = false;
                    break;
                }
            if (!allScalar)
                continue;
            (result ?? (result = new List<(IReadOnlyList<string>, string)>())).Add(o);
        }
        return result;
    }

    // Scalar erasure only: when the enclosing call site could determine the element type its
    // lambda argument receives (component HOFs on IArrayLike values, Map/Zip/... on
    // IReadOnlyList<primitive>), this is the primitive the lambda's parameters erase to.
    // Lambda parameter defs have inferred/generic Plato types, so this is the only channel
    // through which the lambda body writer learns they are scalar-valued.
    public string PendingLambdaParamPrim;

    public CSharpFunctionBodyWriter WriteLambdaBody(Lambda lambda)
    {
        var def = lambda.Function;
        var fi = TypeWriter.ToFunctionInfo(def, null, FunctionInstanceKind.Lambda);
        // The body is written by the CONSTRUCTOR, so the lambda-parameter primitive (when the
        // call site determined one) must be passed in up front.
        var tmp = new CSharpFunctionBodyWriter(TypeWriter, fi, true, true, null, PendingLambdaParamPrim);
        Write(tmp.ToString());
        return this;
    }

    /// <summary>
    /// Scalar erasure only: the primitive ELEMENT type of an expression used as the receiver
    /// of a HOF taking a lambda (component type for IArrayLike values, IReadOnlyList generic
    /// argument otherwise), or null when undeterminable.
    /// </summary>
    public string ElementPrimOf(Expression e)
    {
        switch (e)
        {
            case ParameterRefSymbol pr:
            {
                string typeName = null;
                if (_paramEmittedTypes != null && _paramEmittedTypes.TryGetValue(pr.Def, out var emitted))
                    typeName = emitted;
                typeName = typeName ?? pr.Def?.Type?.Name;
                if (typeName == "Self")
                    typeName = TypeWriter.TypeDef?.Name;
                if (typeName == null)
                    return null;
                var fromList = ElementPrimOfListTypeName(typeName);
                if (fromList != null)
                    return fromList;
                var comp = Writer.GetComponentPlatoType(typeName);
                return comp != null && CSharpWriter.ScalarPrimitives.TryGetValue(comp, out var p) ? p : null;
            }
            case FunctionCall fc when fc.Function is FunctionGroupRefSymbol g:
            {
                // xs.Components.Map(...): the elements of Components are the receiver's components.
                if (g.Name == "Components" && fc.Args.Count == 1)
                {
                    if (fc.Args[0] is ParameterRefSymbol rpr)
                    {
                        var tn = rpr.Def?.Type?.Name;
                        if (tn == null && _paramEmittedTypes != null && _paramEmittedTypes.TryGetValue(rpr.Def, out var re))
                            tn = re;
                        var comp = Writer.GetComponentPlatoType(tn);
                        return comp != null && CSharpWriter.ScalarPrimitives.TryGetValue(comp, out var p) ? p : null;
                    }
                    return null;
                }
                // A call whose every overload returns IArray<scalar>.
                string prim = null;
                if (g.Def?.Functions == null || g.Def.Functions.Count == 0)
                    return null;
                foreach (var f in g.Def.Functions)
                {
                    var rt = f.ReturnType;
                    if (rt?.Name == null || !rt.Name.StartsWith("IArray") || rt.TypeArgs.Count != 1)
                        return null;
                    if (!CSharpWriter.ScalarPrimitives.TryGetValue(rt.TypeArgs[0].Name, out var p))
                        return null;
                    if (prim == null)
                        prim = p;
                    else if (prim != p)
                        return null;
                }
                return prim;
            }
            default:
                return null;
        }
    }

    private static string ElementPrimOfListTypeName(string typeName)
    {
        // Emitted C# types: "IReadOnlyList<float>" etc.
        foreach (var p in ScalarPrimitiveNames)
            if (typeName == $"IReadOnlyList<{p}>" || typeName == $"IArray<{p}>")
                return p;
        return null;
    }

    public CSharpFunctionBodyWriter Write(Statement st)
    {
        switch (st)
        {
            case ReturnStatement returnSymbol:
                Write("return ");
                if (returnSymbol.Expression != null)
                    Write(returnSymbol.Expression);
                WriteLine(";");
                return this;
                
            case LoopStatement loopSymbol:
                Write("while (").Write(loopSymbol.Condition).WriteLine(")");
                WriteStartBlock();
                Write(loopSymbol.Body);
                WriteEndBlock();
                WriteLine();
                return this;

            case MultiStatement multiStatement:
                foreach (var child in multiStatement.Symbols)
                {
                    Write(child);
                    if (child is Expression)
                        WriteLine(";");
                }
                return this;

            case BlockStatement block:
            {
                WriteStartBlock();
                foreach (var x in block.Symbols)
                {
                    Write(x);
                    if (x is Expression)
                        WriteLine(";");
                }

                return WriteEndBlock();
            }

            case CommentStatement commentStatement:
                return Write($"/* {commentStatement.Comment} */");

            case IfStatement ifStatement:
                Write("if (");
                Write(ifStatement.Condition);
                WriteLine(")");
                Write(ifStatement.IfTrue);
                if (ifStatement.IfFalse != null)
                {
                    WriteLine("else");
                    Write(ifStatement.IfFalse);
                }

                return this;
        }
                    
        return this;
    }

    public CSharpFunctionBodyWriter WriteFunctionCall(FunctionCall functionCall)
    {
        // Scalar erasure: wrap calls whose function group determinately returns a scalar
        // wrapper in a cast to the primitive, so every scalar-valued intermediate in the
        // emitted body is primitive-typed ("float-land"). When the callee's emitted return
        // type is already erased the cast is an identity conversion; when the callee is a
        // wrapper-typed kept member or a handwritten intrinsic it performs the (implicit
        // anyway) wrapper->primitive conversion explicitly, keeping receiver types and
        // generic inference uniform.
        var scalarPrim = Writer.ScalarErase ? ScalarReturnPrimitive(functionCall.Function) : null;
        if (scalarPrim != null)
        {
            Write($"(({scalarPrim})");
            WriteFunctionCallCore(functionCall);
            return Write(")");
        }
        return WriteFunctionCallCore(functionCall);
    }

    private CSharpFunctionBodyWriter WriteFunctionCallCore(FunctionCall functionCall)
    {
        // If there are no arguments, it is a constant.
        if (functionCall.Args.Count == 0)
        {
            if (functionCall.Function is KeywordRefSymbol krs)
            {
                if (krs.Name != "default")
                    throw new Exception("Only default keyword supported");
                return Write("default");
            }
            else
            {
                return Write("Constants.").Write(functionCall.Function);
            }
        }


        // Calling a parameter, or variable 
        if (functionCall.Function is ParameterRefSymbol 
            || functionCall.Function is VariableRefSymbol
            || functionCall.Function is FunctionCall)
        {
            return Write(functionCall.Function).Write(".Invoke(").WriteCommaList(functionCall.Args).Write(")");
        }

        var f = functionCall.Function;
        if (f.Name.StartsWith("Tuple"))
            return Write("(").WriteCommaList(functionCall.Args).Write(")");

        var arg = functionCall.Args[0];

        if (f.Name == "At")
        {
            return Write(arg).Write("[").WriteCommaList(functionCall.Args.Skip(1)).Write("]");
        }

        Write(arg).Write(".");
        // Member-access position: the name is resolved against the receiver, so the
        // extension-style re-qualification (and the constants lookup) must never apply here.
        if (Writer.ExtensionStyle && functionCall.Function is FunctionGroupRefSymbol memberRef)
            Write(memberRef.Name);
        else
            Write(functionCall.Function);

        // A single-argument call is receiver-only, so its callee is a nullary-after-receiver
        // member. Member form (v.Length, HasArgList false) is already property-style here.
        // Prefix form (Foo(v), HasArgList true) usually needs "()" because the intrinsic
        // IArray/IReadOnlyList helpers are C# METHODS (e.g. xs.Components.Reverse()). The one
        // exception is a call whose name is a generated TYPE: a type-named single-argument
        // conversion (Vector3(0.0), Point3D(v)) always resolves to a nullary conversion
        // PROPERTY on the argument, so it must NOT get "()", or the prefix broadcast miscompiles
        // to ((Number)0).Vector3() against the Vector3 property.
        var isConversionProperty = functionCall.HasArgList
            && functionCall.Function is FunctionGroupRefSymbol
            && Writer.AllTypeNames.Contains(functionCall.Function.Name);
        if (functionCall.Args.Count == 1 && (!functionCall.HasArgList || isConversionProperty))
        {
            // Extension style: no-arg library functions that moved out of their structs are
            // classic extension METHODS (v.Length()), so their call sites need "()". Names in
            // MovedNoArgNames are guaranteed not to be properties on ANY generated type
            // (conflicts are demoted back into the structs), so this is decidable by name.
            if (Writer.ExtensionStyle && Writer.MovedNoArgNames.Contains(functionCall.Function.Name))
                return Write("()");
            // Scalar erasure: every member of the five scalar types is an extension METHOD on
            // the primitive, so a no-arg access on a provably scalar-valued receiver needs
            // "()" even when the same name is a kept property on some non-scalar type.
            if (Writer.ScalarErase
                && Writer.ScalarMemberNames.Contains(functionCall.Function.Name)
                && IsScalarValued(arg))
                return Write("()");
            return this;
        }

        // Scalar erasure: when a lambda argument's parameters receive the ELEMENTS of the
        // receiver (known element-wise HOFs only: component HOFs on IArrayLike values,
        // Map/Zip/... over scalar lists), tell the lambda body writer the primitive they erase
        // to (lambda defs carry only inferred Plato types).
        var savedPrim = PendingLambdaParamPrim;
        var fnName = functionCall.Function?.Name;
        if (Writer.ScalarErase && functionCall.Args.Skip(1).Any(a => a is Lambda))
            PendingLambdaParamPrim = ElementWiseHofNames.Contains(fnName) ? ElementPrimOf(arg) : null;

        // Scalar erasure: when the receiver is provably scalar and a unique all-scalar-param
        // overload matches, cast the (provably scalar) arguments to the declared parameter
        // primitives. This keeps overload resolution exact where implicit conversions from the
        // wrappers would otherwise make it ambiguous (e.g. Multiply(float, float) vs the
        // erased Multiply(float, Angle) with an int argument).
        IReadOnlyList<string> argPrims = null;
        string receiverPrim = null;
        if (Writer.ScalarErase && functionCall.Function is FunctionGroupRefSymbol fgrc)
        {
            receiverPrim = ScalarPrimOf(arg);
            if (receiverPrim != null)
            {
                var overloads = MatchingScalarOverloads(fgrc.Name, functionCall.Args.Count, receiverPrim);
                if (overloads != null && overloads.Count >= 1)
                {
                    // All matching overloads must agree on the parameter primitives.
                    var first = overloads[0].Params;
                    var agree = true;
                    foreach (var o in overloads)
                        for (var i = 1; i < o.Params.Count && agree; ++i)
                            agree = o.Params[i] == first[i];
                    if (agree)
                        argPrims = first;
                }
            }
        }

        Write("(");
        var argIndex = 1;
        foreach (var a in functionCall.Args.Skip(1))
        {
            if (argIndex > 1)
                Write(", ");
            var argPrim = Writer.ScalarErase && a is Expression ae ? ScalarPrimOf(ae) : null;
            if (argPrims != null && argPrim != null
                && CSharpWriter.ScalarPrimitives.TryGetValue(argPrims[argIndex], out var castPrim))
                // Provably scalar receiver: pin the argument to the declared parameter
                // primitive so the fully erased overload is an exact match.
                Write($"(({castPrim})(").Write(a).Write("))");
            else if (argPrim != null && receiverPrim == null)
                // Receiver not provably scalar: the callee is (or may be) a wrapper-typed kept
                // member of a non-scalar type. Restore the V1 conversion graph by handing it a
                // WRAPPER-typed argument: exact matches against Number-typed parameters, and a
                // single user-defined conversion away from the broadcast conversions
                // (Number -> Vector2 etc.), where a raw float would be ambiguous or stuck.
                Write($"(({WrapperOfPrim(argPrim)})(").Write(a).Write("))");
            else
                WriteArg(a);
            argIndex++;
        }
        Write(")");
        PendingLambdaParamPrim = savedPrim;
        return this;
    }

    private static string WrapperOfPrim(string prim)
    {
        foreach (var kv in CSharpWriter.ScalarPrimitives)
            if (kv.Value == prim)
                return kv.Key;
        return prim;
    }

    // Known element-wise HOF names: the lambda's parameters receive elements of the receiver.
    private static readonly HashSet<string> ElementWiseHofNames = new HashSet<string>
    {
        "MapComponents", "ZipComponents", "AllZipComponents", "AnyZipComponents",
        "AllComponents", "AnyComponent", "Reduce",
        "Map", "Zip", "Filter", "All", "Any",
        "MapPairs", "MapTriplets", "MapQuartets",
    };

    /// <summary>
    /// Writes a call ARGUMENT. Scalar erasure only: a reference to a function-typed parameter
    /// or variable is eta-expanded into a lambda ("f" => "(_e0, _e1) => f(_e0, _e1)").
    /// Delegate types are invariant, so an erased Func&lt;float,bool&gt; value would not
    /// convert to the Func&lt;T,Boolean&gt; the handwritten IArray intrinsics expect (nor vice
    /// versa); a lambda is target-typed and bridges both directions through the wrappers'
    /// implicit conversions.
    /// </summary>
    public CSharpFunctionBodyWriter WriteArg(Symbol s)
    {
        if (Writer.ScalarErase && s is RefSymbol rs
            && (s is ParameterRefSymbol || s is VariableRefSymbol)
            && rs.Def?.Type?.Name != null && rs.Def.Type.Name.StartsWith("Function")
            && int.TryParse(rs.Def.Type.Name.Substring("Function".Length), out var arity))
        {
            var ps = string.Join(", ", Enumerable.Range(0, arity).Select(i => $"_e{i}"));
            return Write($"({ps}) => {rs.Name}({ps})");
        }
        return Write(s);
    }

    public CSharpFunctionBodyWriter Write(Expression expr, string type = null)
    {
        if (expr == null)
            return this;    
            
        switch (expr)
        {
            // Emission-only marker nodes produced by ComponentUnroller (--optimize, P3.1).
            case ComponentAccessExpression ca:
                if (ca.CastTo != null)
                    return Write($"(({ca.CastTo})").Write(ca.Receiver).Write(".").Write(ca.FieldName).Write(")");
                return Write(ca.Receiver).Write(".").Write(ca.FieldName);

            case ConstructorCallExpression cc:
                // In extension-style library classes type names can be shadowed by members of
                // the enclosing static class, so qualify with the namespace (same rule as bare
                // type references in the FunctionGroupRefSymbol case below).
                return Write("new ")
                    .Write(TypeWriter.ExtensionReceiverName != null ? $"{Namespace}.{cc.TypeName}" : cc.TypeName)
                    .Write("(").WriteCommaList(cc.Args).Write(")");

            case BooleanChainExpression bc:
            {
                for (var i = 0; i < bc.Terms.Count; ++i)
                {
                    if (i > 0)
                        Write($" {bc.Op} ");
                    Write(bc.Terms[i]);
                }
                return this;
            }

            case TypeExpression typeExpression:
                if (typeExpression.TypeArgs.Count > 0)
                    throw new NotSupportedException();
                return Write(Namespace + "." + typeExpression.Name);

            case NewExpression newExpression:
                // Scalar erasure: "new Number(x)" would erase to the invalid "new float(x)";
                // a scalar constructor call is just a cast of its single argument.
                if (Writer.ScalarErase && CSharpWriter.ScalarPrimitives.TryGetValue(newExpression.Type?.Name ?? "", out var snPrim)
                    && newExpression.Args.Count == 1)
                    return Write($"(({snPrim})").Write(newExpression.Args[0]).Write(")");
                return Write($"new {Function.ToCSharpType(newExpression.Type)}(").WriteCommaList(newExpression.Args).Write(")");

            case ParameterRefSymbol pr:
                // Scalar erasure: scalar-typed parameter references are normalized to the
                // primitive. In erased signatures the cast is an identity conversion; in
                // wrapper-typed positions (interface obligations, the partial-struct shim,
                // lambdas target-typed by unerased delegates) it converts Number->float etc.,
                // so bodies are uniformly "float-land".
                if (Writer.ScalarErase)
                {
                    var spPrim = ScalarPrimitiveOfParam(pr.Def);
                    if (spPrim != null)
                        return pr.Def.Index == 0 && !IsStaticOrLambda
                            ? Write($"(({spPrim})this)")
                            : Write($"(({spPrim}){pr.Name})");
                }
                return pr.Def.Index == 0 && !IsStaticOrLambda
                    ? Write("this")
                    : Write(pr.Name);

            case FunctionGroupRefSymbol fgr:
                // HACK: check if it is a constant.
                // TODO: I need to have all function calls properly resolved to generate better quality code. 
                if (fgr.Def.Functions.Count == 1 &&
                    fgr.Def.Functions[0].NumParameters == 0)
                    return Write($"Constants.{fgr.Name}");
                // Extension-style mode: bare names bound implicitly inside the partial struct
                // in the default mode; inside an extension block they must be re-qualified.
                if (TypeWriter.ExtensionReceiverName != null)
                {
                    if (TypeWriter.ExtensionInstanceNames.Contains(fgr.Name))
                    {
                        Write($"{TypeWriter.ExtensionReceiverName}.{fgr.Name}");
                        // Moved no-arg members are classic extension methods, not properties.
                        // Under scalar erasure the receiver of a scalar type's moved member is
                        // a primitive, so ALL its no-arg members are extension methods too.
                        if (Writer.MovedNoArgNames.Contains(fgr.Name)
                            || (TypeWriter.ExtensionReceiverIsScalar && Writer.ScalarMemberNames.Contains(fgr.Name)))
                            Write("()");
                        return this;
                    }
                    if (TypeWriter.ExtensionStaticNames.Contains(fgr.Name))
                        return Write($"{TypeWriter.ExtensionStaticQualifier}.{fgr.Name}");
                    // Type references (e.g. "Integer2" in "Integer2.CreateFromComponents"):
                    // namespace-qualified, because members of the enclosing static library
                    // class can shadow type names.
                    if (Writer.AllTypeNames.Contains(fgr.Name))
                        return Write($"{Namespace}.{fgr.Name}");
                    // Unknown to the compiler: a handwritten member of the receiver type in
                    // Plato.Intrinsics (e.g. Number.Zero). In the default mode it bound inside
                    // the enclosing struct; statics are the only observed case. Under scalar
                    // erasure such statics are wrapper-typed (handwritten), so normalize the
                    // value to the primitive for "float-land" bodies.
                    if (TypeWriter.ExtensionReceiverIsScalar
                        && CSharpWriter.ScalarPrimitives.TryGetValue(TypeWriter.TypeDef?.Name ?? "", out var sqPrim))
                        return Write($"(({sqPrim}){TypeWriter.ExtensionStaticQualifier}.{fgr.Name})");
                    return Write($"{TypeWriter.ExtensionStaticQualifier}.{fgr.Name}");
                }
                return Write(fgr.Name);

            case RefSymbol refSymbol:
                return Write(refSymbol.Name == "Self" ? SelfType : refSymbol.Name);
                
                //if (refSymbol.Name == "Self") throw new Exception("Self type references no longer supported");
            
                //return Write(refSymbol.Name);

            case Assignment assignment:
                return Write(assignment.LValue)
                    .Write(" = ")
                    .Write(assignment.RValue);

            case ConditionalExpression conditional:
                return Write(conditional.Condition)
                    .Write(" ? ").Write(conditional.IfTrue)
                    .Write(" : ").Write(conditional.IfFalse);

            case FunctionCall functionCall:
                return WriteFunctionCall(functionCall);

            case Literal literal:
                // Scalar erasure: literals lose their wrapper casts and become native C#
                // literals - ((Number)0.5) => 0.5f, ((Integer)3) => 3, ((Boolean)true) => true,
                // ((String)"x") => "x".
                if (Writer.ScalarErase)
                {
                    var v = GetLiteralValue(literal);
                    return Write(literal.TypeEnum == Ara3D.Geometry.AST.LiteralTypesEnum.Number ? v + "f" : v);
                }
                // TODO: once validating that the cost is superfluous, we can remove this.
                return Write($"(({GetLiteralType(literal)}){GetLiteralValue(literal)})");
            
            case Lambda lambda:
                return Write("(")
                    .WriteCommaList(lambda.Function.Parameters.Select(p => p.Name))
                    .Write(") ")
                    .WriteLambdaBody(lambda);

            case ArrayLiteral arrayLiteral:
            {
                var arg = "";
                if (type != null && type.StartsWith("IArray"))
                    arg = type.Substring("IArray".Length);
                return Write($"Intrinsics.MakeArray{arg}(")
                    .WriteCommaList(arrayLiteral.Expressions)
                    .Write(")");
            }
        }

        throw new ArgumentOutOfRangeException(nameof(expr));
    }
}