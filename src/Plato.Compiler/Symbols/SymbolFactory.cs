using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Logging;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Symbols
{
    /// <summary>
    /// Used primarily to figure out what each name means, and what the type of each expression is.
    /// This allows us to figure out which methods will get called at run-time.
    /// </summary>
    public class SymbolFactory
    {
        public SymbolFactory(ILogger logger)
        {
            Logger = logger;

            // Create and bind Primitives type-defs
            var td = new TypeDef(null, TypeKind.Primitive, "Any");
            BindType(td);
        }

        public ILogger Logger { get; }
        public List<ResolutionError> Errors { get; } = new List<ResolutionError>();
        public Scope ValueBindingsScope { get; set; } = new Scope(null);
        public Scope TypeBindingsScope { get; set; } = new Scope(null);

        public readonly Dictionary<Symbol, AstNode> SymbolsToNodes = new Dictionary<Symbol, AstNode>();

        /// <summary>Every resolved type-name occurrence, in resolution order (Plato.Navigation,
        /// plato-236). Deliberately NOT stored in <see cref="SymbolsToNodes"/>: TypeExpression
        /// overrides Equals/GetHashCode by value, so a dictionary keyed by symbol collapses every
        /// occurrence of the same type into a single entry — which erases exactly the per-site
        /// information find-refs needs. Purely additive; nothing in the compiler reads it.</summary>
        public readonly List<TypeReference> TypeReferences = new List<TypeReference>();

        public List<TypeDef> TypeDefs { get; } = new List<TypeDef>();

        public T BindValue<T>(string name, T value) where T : Symbol
        {
            ValueBindingsScope = ValueBindingsScope.Bind(name, value);
            return value;
        }

        public TypeDef BindType(TypeDef value)
        {
            return BindType(value.Name, value);
        }

        public TypeDef BindType(string name, TypeDef value)
        {
            TypeBindingsScope = TypeBindingsScope.Bind(name, value);
            return value;
        }

        public FunctionGroupDef CreateOrLookupGroupDefinition(MemberDef member)
            => CreateOrLookupGroupDefinition(member.Name);

        public FunctionGroupDef CreateOrLookupGroupDefinition(string name)
        {
            var val = ValueBindingsScope.GetValue(name);
            if (val is FunctionGroupDef group)
                return group;

            var fgs = new FunctionGroupDef(ValueBindingsScope, CreateAny(), Array.Empty<FunctionDef>(), name);
            return BindValue(name, fgs);
        }

        public void AddToGroupDefinitionAndCreateIfNeeded(FunctionDef function)
            => CreateOrLookupGroupDefinition(function.Name).Add(function);

        public FunctionGroupDef AddToGroupDefinition(FunctionDef function)
        {
            var name = function.Name;
            var val = ValueBindingsScope.GetValue(name);
            if (val == null)
                throw new Exception($"Could not find function group {function.Name}");

            if (val is FunctionGroupDef group)
                group.Add(function);
            else
                throw new Exception($"Expected a group definition but got a {val}");
            return group;
        }

        public RefSymbol GetValue(string name, AstNode node)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                LogResolutionError("Invalid variable name", node);
                return null;
            }

            var sym = ValueBindingsScope.GetValue(name);
            if (sym == null)
            {
                var td = GetTypeDefinition(name);
                if (td == null)
                {
                    if (name == "default")
                        return KeywordRefSymbol.Default;

                    LogResolutionError($"Could not find symbol {name}", node);
                    return null;
                }

                return td.ToReference();
            }

            if (sym is DefSymbol def)
            {
                return def.ToReference();
            }

            LogResolutionError($"Could not properly resolve symbol {name}", node);
            return null;
        }

        public void LogResolutionError(string message, AstNode node)
        {
            // TODO: break if you want to catch every isngle 
            //Debugger.Break();
            Errors.Add(new ResolutionError(message, node));
        }

        public TypeDef GetTypeDefinition(string name)
        {
            return TypeBindingsScope.GetValue(name) as TypeDef;
        }

        public TypeExpression ResolveType(AstTypeNode astTypeNode)
        {
            if (astTypeNode == null)
            {
                //LogError("Missing type", astTypeNode);
                //return null;
                return CreateAny();
            }
            var name = astTypeNode.Name.Text.Trim();
            if (name == "var")
            {
                // NOTE: maybe this might be 
                return CreateAny();
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                LogResolutionError("Invalid variable name", astTypeNode);
                return null;
            }

            // Is it a Type Variable
            if (name.StartsWith("$"))
            {
                var typeVar = TypeExpression.CreateTypeVar(TypeBindingsScope, name);
                TypeReferences.Add(new TypeReference(astTypeNode, typeVar));
                return typeVar;
            }

            var sym = TypeBindingsScope.GetValue(name);
            if (sym == null)
            {
                LogResolutionError($"Could not find type {name}", astTypeNode);
                return null;
            }
            var tds = sym as TypeDef;
            if (tds == null)
            {
                LogResolutionError($"Could not resolve type {name} instead got {sym}", astTypeNode);
                return null;
            }

            // TODO: maybe the symbols should contain the ast nodes instead of using dictionaries everywhere.
            var args = astTypeNode.TypeArguments.Select(ResolveType).ToArray();
            var result = new TypeExpression(tds, args);
            TypeReferences.Add(new TypeReference(astTypeNode, result));
            return result;
        }

        public ParameterDef Resolve(AstParameterDeclaration astParameterDeclaration)
        {
            var result = BindValue(astParameterDeclaration.Name,
                new ParameterDef(ValueBindingsScope, astParameterDeclaration.Name,
                    ResolveType(astParameterDeclaration.Type), astParameterDeclaration.Index));
            SymbolsToNodes[result] = astParameterDeclaration;
            return result;
        }

        public Expression ResolveExpr(AstNode node)
        {
            var r = Resolve(node);
            if (r == null) return null;
            if (r is Expression x) return x;
            if (r is TypeExpression tx) return new TypeRefSymbol(tx.Def);
            var msg = $"Expected an expression not {r}";
            LogResolutionError(msg, node);
            throw new Exception(msg);
        }

        public Symbol Resolve(AstNode node)
        {
            var r = InternalResolve(node);
            if (r != null)
                SymbolsToNodes[r] = node;
            return r;
        }

        public AstNode ReduceFunctionToLoop(AstInvoke invoke)
        {
            // NOTE: I need a generalized way to convert an expression into a statement when needed. 
            // Perhaps in the AST a block can have a value, and if it is expressed as a value ... then the whole function body 
            // needs to be written as a block? I can imagine function bodies being written in a "block/statement" form, or an "expression" form
            // depdning on theneeds.
            // I don't want to use block form always because it is very verbose, and might be slow because of the extra things that are introduced.
            // However, I can't just always use "function" form, because it is VERY slow, when lambdas are involved. 

            var f = invoke.Function;
            if (f is AstIdentifier ident && ident.Text == "Reduce")
            {
                if (invoke.Arguments.Count != 3)
                    throw new Exception("Reduce expression must have three arguments");

                var collection = invoke.Arguments[0];
                var initialValue = invoke.Arguments[1];
                var func = invoke.Arguments[2] as AstLambda;
                if (func == null)
                    throw new Exception("Expected third argument to be a lambda");

                // TODO : use AstBuilder 
                /*
                var result = new AstVarDef(null, new AstIdentifier(null, "__result"), initialValue, null);
                var loopVar = new AstVarDef(null, new AstIdentifier(null, "__i"), 0, new AstTypeNode(null, "Integer"));
                var forLoop = new AstLoop(null, null, null);
                var forBody = func.Body;
                */
                // create a for-loop, and a variable. 

            }

            throw new NotImplementedException();
        }

        /// <summary>Resolve a match expression (wave-2, plato-232). The subject's resolved type
        /// (a sum type in a well-formed match) determines each arm's binder types: a binder is a
        /// local typed by the case's field in declaration order. Well-formedness (exhaustiveness,
        /// unknown/duplicate case, binder arity, non-sum subject) is validated by SumTypeChecker;
        /// this method stays total and best-effort so the checker can produce located diagnostics
        /// (binders past a case's field count, or on a non-sum subject, are typed <c>Any</c>).</summary>
        public Symbol ResolveMatch(AstMatch astMatch)
        {
            var scrutinee = ResolveExpr(astMatch.Scrutinee);
            var subjectDef = (scrutinee as RefSymbol)?.Def?.Type?.Def;

            var arms = new List<MatchArm>();
            foreach (var astArm in astMatch.Arms)
            {
                ValueBindingsScope = ValueBindingsScope.Push();
                var caseName = astArm.CaseName.Text;
                var caseDef = subjectDef?.Cases?.FirstOrDefault(c => c.Name == caseName);
                var binders = new List<VariableDef>();
                for (var i = 0; i < astArm.Binders.Count; i++)
                {
                    var bName = astArm.Binders[i].Text;
                    var bType = caseDef != null && i < caseDef.Fields.Count ? caseDef.Fields[i].Type : CreateAny();
                    binders.Add(BindValue(bName, new VariableDef(ValueBindingsScope, bName, bType, null)));
                }
                var body = ResolveExpr(astArm.Body);
                ValueBindingsScope = ValueBindingsScope.Pop();
                arms.Add(new MatchArm(caseName, binders, body));
            }

            return new MatchExpression(scrutinee, subjectDef, arms);
        }

        public Symbol NewExpression(AstNew astNew)
        {
            var args = astNew.Arguments.Select(ResolveExpr).ToArray();
            var type = ResolveType(astNew.Type);
            return new NewExpression(type, args);
        }

        public Symbol ResolveFunctionCall(AstInvoke astInvoke)
        {
            var args = astInvoke.Arguments.Select(ResolveExpr).ToArray();

            // "AstNew" is parsed  as if it is a function.
            if (astInvoke.Function is AstNew astNew)
            {
                throw new InvalidOperationException();
                //var type = ResolveType(astNew.Type);
                //return new NewExpression(type, args);
            }

            var funcRef = ResolveExpr(astInvoke.Function);

            if (funcRef == null)
            {
                var msg = $"Could not find function {astInvoke.Function}";
                LogResolutionError(msg, astInvoke);
                throw new Exception(msg);
            }

            if (args.Length > 0 && args[0] is FunctionGroupRefSymbol fgr)
            {
                var typeSym = TypeBindingsScope.GetValue(fgr.Name);
                if (typeSym != null)
                {
                    var td = typeSym as TypeDef;
                    if (td == null)
                        throw new Exception("Expected a TypeDef");
                    args[0] = td.ToTypeExpression();
                }
            }

            return new FunctionCall(funcRef, astInvoke.HasArgList, args);
        }

        public Symbol InternalResolve(AstNode node)
        {
            try
            {
                if (node == null)
                    return null;

                switch (node)
                {
                    case AstTypeNode astTypeNode:
                        return ResolveType(astTypeNode);

                    case AstAssign astAssign:
                        // An assignment must NOT rebind the name in the current scope (studio-096).
                        // It resolves its target to the single existing declaration and leaves the
                        // binding alone. Rebinding it to this Assignment (an Expression, not a
                        // DefSymbol) meant a later GetValue from a more-deeply-nested loop body found
                        // the Assignment instead of the VariableDef and failed with "Could not
                        // properly resolve symbol" — which is exactly why a var written at two
                        // loop-nesting depths broke. Shadowing (a nested `var d`) still goes through
                        // the AstVarDef case, which binds a fresh VariableDef, so it is unaffected.
                        return new Assignment(
                            GetValue(astAssign.Var, astAssign),
                            ResolveExpr(astAssign.Value));

                    case AstBlock astBlock:
                        {
                            // TODO: the value bindings scope should be in a "using" block.
                            ValueBindingsScope = ValueBindingsScope.Push();
                            var r = new BlockStatement(
                                astBlock.Statements.Select(Resolve).ToArray());
                            ValueBindingsScope = ValueBindingsScope.Pop();
                            return r;
                        }
                
                    case AstMulti astMulti:
                        {
                            if (astMulti.Nodes.Count == 0)
                                return null;
                            if (astMulti.Nodes.Count > 1)
                            {
                                return new MultiStatement(
                                    astMulti.Nodes.Select(Resolve).ToArray()
                                );
                            }

                            return Resolve(astMulti.Nodes[0]);
                        }

                    case AstParenthesized astParenthesized:
                        return Resolve(astParenthesized.Inner);

                    case AstReturn astReturn:
                        return new ReturnStatement(Resolve(astReturn.Value));

                    case AstConditional astConditional:
                        return Scoped(() => new ConditionalExpression(
                            ResolveExpr(astConditional.Condition),
                            ResolveExpr(astConditional.IfTrue),
                            ResolveExpr(astConditional.IfFalse)));

                    case AstIfStatement astIf:
                        return new IfStatement(
                            ResolveExpr(astIf.Condition),
                            Resolve(astIf.IfTrue),
                            Resolve(astIf.IfFalse));

                    case AstConstant astConstant1:
                        return new Literal(astConstant1.TypeEnum, astConstant1.Value);

                    case AstExpressionStatement astExpressionStatement:
                        return Scoped(() => new ExpressionStatement(
                            ResolveExpr(astExpressionStatement.Expression)));

                    case AstIdentifier astIdentifier:
                        return GetValue(astIdentifier.Text, astIdentifier);

                    case AstInvoke astInvoke:
                        return ResolveFunctionCall(astInvoke);

                    case AstNew astNew:
                        return NewExpression(astNew);

                    case AstLambda astLambda:
                    {
                        var outerScope = ValueBindingsScope;
                            ValueBindingsScope = ValueBindingsScope.Push();
                            var ps = astLambda.Parameters.Select(Resolve).ToArray();
                            // A lambda body is usually an expression (`i => expr`), but Plato also
                            // allows a compound-statement body (`i => { var x = ..; return ..; }`).
                            // ResolveExpr rejects a statement, so bind a block body with Resolve
                            // (FunctionDef.Body is a Symbol, exactly as for a normal block-bodied
                            // function). Expression bodies keep the ResolveExpr path unchanged.
                            var body = astLambda.Body is AstBlock
                                ? Resolve(astLambda.Body)
                                : (Symbol)ResolveExpr(astLambda.Body);
                            var r = new Lambda(new FunctionDef(outerScope, $"_lambda_{NextLambdaId++}", null, 
                                CreateLambdaType(ps.Length), body, ps));
                            ValueBindingsScope = ValueBindingsScope.Pop();
                            return r;
                        }

                    case AstVarDef astVarDef:
                        return BindValue(astVarDef.Name,
                            new VariableDef(ValueBindingsScope, astVarDef.Name, ResolveType(astVarDef.Type), ResolveExpr(astVarDef.Value)));

                    case AstLoop astLoop:
                        return new LoopStatement(Resolve(astLoop.Condition), Resolve(astLoop.Body));

                    case AstBinaryOp astBinaryOp:
                        return InternalResolve(astBinaryOp.ToInvocation());

                    case AstArrayLiteral astArray:
                        return new ArrayLiteral(astArray.Nodes.Select(ResolveExpr).ToArray());

                    case AstMatch astMatch:
                        return ResolveMatch(astMatch);
                }

                LogResolutionError($"Node can't be evaluated", node);
                return null;
            }
            catch (Exception e)
            {
                LogResolutionError($"Exception occurred {e}", node);
                return null;
            }
        }

        public static int NextLambdaId = 0;

        public IEnumerable<TypeDef> CreateTypeDefs(IEnumerable<AstTypeDeclaration> types)
        {
            // First, we bind the libraries 
            foreach (var astTypeDeclaration in types)
            {
                if (astTypeDeclaration.Kind == TypeKind.Library)
                {
                    var typeDef = new TypeDef(TypeBindingsScope, astTypeDeclaration.Kind, astTypeDeclaration.Name);
                    SymbolsToNodes.Add(typeDef, astTypeDeclaration);
                    BindType(typeDef);
                    TypeDefs.Add(typeDef);
                }
            }

            // Now we bind the types (might be overlap in names)
            foreach (var astTypeDeclaration in types)
            {
                if (astTypeDeclaration.Kind != TypeKind.Library)
                {
                    var typeDef = new TypeDef(TypeBindingsScope, astTypeDeclaration.Kind, astTypeDeclaration.Name);

                    // Affine-type modifier (roadmap Phase 6): only the intrinsic builder
                    // types may be declared unique. Hard-reject anything else: the error
                    // halts compilation (SymbolFactory.Errors is checked by Compilation).
                    typeDef.IsUnique = astTypeDeclaration.IsUnique;

                    // Primitive-type keyword (plato-367): recorded, nothing else changes.
                    typeDef.IsPrimitiveDeclaration = astTypeDeclaration.IsPrimitiveDeclaration;

                    if (typeDef.IsUnique && !UniqueTypes.IsUniqueTypeName(typeDef.Name))
                        LogResolutionError(
                            $"The 'unique' modifier is not allowed on type '{typeDef.Name}': only the intrinsic " +
                            $"builder types ({string.Join(", ", UniqueTypes.Names)}) may be declared unique",
                            astTypeDeclaration);

                    SymbolsToNodes.Add(typeDef, astTypeDeclaration);
                    BindType(typeDef);
                    TypeDefs.Add(typeDef);

                    // Sum types (wave-2) get per-case factories, not a product constructor.
                    if (astTypeDeclaration.Kind == TypeKind.ConcreteType && astTypeDeclaration.Cases.Count == 0)
                    {
                        // Add a global constructor function
                        var ctor = new FunctionDef(ValueBindingsScope, typeDef.Name, typeDef,
                            typeDef.ToTypeExpression(), null,
                            typeDef.Fields.Select((f, i) => new ParameterDef(ValueBindingsScope, f.Name, f.Type, i))
                                .ToArray());
                        AddCompilerGeneratedFunction(typeDef, ctor);
                    }
                }
            }

            BindType(SelfType.Instance);

            foreach (var typeDef in TypeDefs)
            {
                var astTypeDeclaration = SymbolsToNodes[typeDef] as AstTypeDeclaration;
                TypeBindingsScope = TypeBindingsScope.Push();
                        
                var typeParameterDefs = new List<TypeParameterDef>();
                foreach (var tp in astTypeDeclaration.TypeParameters)
                {
                    var tpd = new TypeParameterDef(TypeBindingsScope, tp.Name);
                    SymbolsToNodes[tpd] = tp;
                    BindType(tpd.Name, tpd);
                    typeDef.TypeParameters.Add(tpd);
                    typeParameterDefs.Add(tpd);
                }

                // Declared bounds (plato-382) are resolved only once EVERY parameter of this
                // declaration is bound, because a bound may name a sibling parameter — as in
                // `IBounds<TValue, TDelta> where TValue: IDifference<TDelta>`. Bounds are matched to
                // their parameter by identifier TEXT: AstIdentifier has no value equality, so the
                // former node comparison never matched and every bound was silently discarded.
                foreach (var tpd in typeParameterDefs)
                    tpd.Constraints.AddRange(astTypeDeclaration.Constraints
                        .Where(c => c.Name.Text == tpd.Name)
                        .Select(c => ResolveType(c.Constraint)));

                foreach (var m in astTypeDeclaration.Members)
                {
                    if (m is AstFieldDeclaration fd)
                    {
                        var fDef = new FieldDef(ValueBindingsScope, typeDef, ResolveType(fd.Type), fd.Name.Text);
                        typeDef.Fields.Add(fDef);
                        SymbolsToNodes.Add(fDef, fd);
                        CreateOrLookupGroupDefinition(fDef);
                    }
                    else if (m is AstMethodDeclaration md)
                    {
                        var mDef = new MethodDef(ValueBindingsScope, typeDef, ResolveType(md.Type), md.Name.Text);
                        typeDef.Methods.Add(mDef);
                        SymbolsToNodes.Add(mDef, md);
                        CreateOrLookupGroupDefinition(mDef);
                    }
                    else
                    {
                        LogResolutionError($"MemberDefSymbol not recognized", m);
                        return null;
                    }
                }

                // Sum-type cases (wave-2, plato-232): resolve each case's fields here, with the
                // type parameters (for a generic sum) in scope. Case-field names may repeat across
                // cases; the flattened struct disambiguates them by the "Case_" prefix.
                for (var ci = 0; ci < astTypeDeclaration.Cases.Count; ci++)
                {
                    var astCase = astTypeDeclaration.Cases[ci];
                    var fields = astCase.Fields
                        .Select(fd => new SumCaseField(astCase.Name.Text, fd.Name.Text, ResolveType(fd.Type)))
                        .ToList();
                    typeDef.Cases.Add(new SumCaseDef(astCase.Name.Text, ci, fields));
                }

                // Per-case static factories (wave-2): PathSegment2D.Move(p). Created HERE — before
                // any function body is resolved — so a qualified case-constructor call in any
                // library resolves regardless of declaration order. One per DISTINCT case name (a
                // duplicate is diagnosed CHK305; a second same-signature factory would trip
                // FunctionGroupDef.Validate).
                if (typeDef.IsSum)
                {
                    var seenCases = new HashSet<string>();
                    foreach (var caseDef in typeDef.Cases)
                    {
                        if (!seenCases.Add(caseDef.Name))
                            continue;
                        var ps = caseDef.Fields
                            .Select((f, i) => new ParameterDef(ValueBindingsScope, f.Name, f.Type, i))
                            .ToArray();
                        AddCompilerGeneratedFunction(typeDef,
                            new FunctionDef(ValueBindingsScope, caseDef.Name, typeDef,
                                typeDef.ToTypeExpression(), null, ps));
                    }
                }

                TypeBindingsScope = TypeBindingsScope.Pop();
            }

            // For each type 
            foreach (var typeDef in TypeDefs)
            {
                ValueBindingsScope = ValueBindingsScope.Push();
                TypeBindingsScope = TypeBindingsScope.Push();

                foreach (var tpd in typeDef.TypeParameters)
                    BindType(tpd.Name, tpd);
                
                var astTypeDecl = SymbolsToNodes[typeDef] as AstTypeDeclaration;

                // Resolve the inherits and implemented type declarations
                foreach (var inheritedType in astTypeDecl.Inherits)
                {
                    if (inheritedType == null)
                        throw new Exception($"Null inherited type declaration in {typeDef}");
                    typeDef.Inherits.Add(ResolveType(inheritedType));
                }

                foreach (var implementedType in astTypeDecl.Implements)
                {
                    if (implementedType == null)
                        throw new Exception($"Null implemented type declaration in {typeDef}");
                    typeDef.Implements.Add(ResolveType(implementedType));
                }

                // For each method in the type create a function, and add it to a function group 
                foreach (var m in typeDef.Methods)
                {
                    // Create the function 

                    // NOTE: I haven't really tested whether this makes sense. 
                    // It looks like we are creating a new scope for each function here,
                    // I have a hard time understanding how that would work for constructors. 

                    ValueBindingsScope = ValueBindingsScope.Push();
                    var location = SymbolsToNodes[m] as AstMethodDeclaration;
                    if (location == null) throw new Exception($"Missing AstMethodDeclaration at {location}");
                    var list = new List<ParameterDef>();
                    foreach (var p in location.Parameters)
                    {
                        var pValue = Resolve(p);
                        list.Add(pValue);
                    }

                    var body = Resolve(location.Body);
                    if (body == null && location.Body != null)
                    {
                        LogResolutionError("Unexpected missing body", location.Body);
                        continue;
                    }

                    
                    var f = new FunctionDef(ValueBindingsScope, m.Name, typeDef, m.Type, body, list.ToArray());

                    // Bounds this function DECLARES on its own signature variables (plato-393):
                    // `DeCasteljau(xs: Array<$T>, t: Number): $T where $T: Interpolatable`. Resolved
                    // here, where the owner's type parameters are in scope; the target is kept as
                    // WRITTEN ($T), because that is the name the checker's substitution uses. A
                    // target naming nothing in the signature is reported by LINT002, not dropped
                    // silently.
                    foreach (var c in location.Constraints)
                    {
                        var bound = ResolveType(c.Constraint);
                        if (bound != null)
                            f.DeclaredBounds.Add(new DeclaredFunctionBound(c.Name.Text, bound));
                    }

                    Debug.Assert(m.Function == null);
                    m.Function = f;

                    AddToGroupDefinition(f);
                        
                    // Check if this is a static method, but not on a library. 
                    if (!typeDef.IsLibrary() && f.Parameters.Count == 0)
                    {
                        Verifier.Assert(list.Count == 0);

                        // We create a second version of the function that accepts a member of the given type
                        // This is something that C# does not support, but is convenient in generic code. (e.g., x += x.One). 
                        
                        // TEMP:
                        //var f2 = new FunctionDefinition(m.Name, typeDef, m.Type, body, new ParameterDefinition("_", typeDef.ToTypeExpression()));
                        //AddCompilerGeneratedFunction(typeDef, f2);
                    }

                    ValueBindingsScope = ValueBindingsScope.Pop();
                }

                // For each field take the function, and add it to the function group 
                foreach (var f in typeDef.Fields)
                {
                    AddToGroupDefinition(f.Function);
                }

                // If there is at least one field, add a constructor function
                if (typeDef.Fields.Count > 0)
                {
                    var ctor = new FunctionDef(ValueBindingsScope, typeDef.Name, typeDef, typeDef.ToTypeExpression(), null,
                        typeDef.Fields.Select((f, i) => new ParameterDef(ValueBindingsScope, f.Name, f.Type, i)).ToArray());
                    AddCompilerGeneratedFunction(typeDef, ctor);
                }

                // If there is exactly one field, add a "ToX" implicit cast function for that field 
                if (typeDef.Fields.Count == 1)
                {
                    var field = typeDef.Fields[0];
                    var cast = new FunctionDef(ValueBindingsScope, field.Type.Name, typeDef, field.Type, null,
                        new ParameterDef(ValueBindingsScope, "arg", typeDef.ToTypeExpression(), 0));
                    AddCompilerGeneratedFunction(typeDef, cast);
                }

                // If there are more than one fields, Add a tuple constructor and a ToTuple implicit cast function.
                // Guard on the TupleN type existing: the production stdlib always declares Tuple2..Tuple10,
                // but a self-contained fixture (e.g. plato-test-sum) may declare a multi-field product type
                // without them — skip the tuple ctor there rather than crash.
                if (typeDef.Fields.Count > 1 && GetTypeDefinition($"Tuple{typeDef.Fields.Count}") != null)
                {
                    var tupleType = CreateTuple(typeDef.Fields.Select(f => f.Type).ToArray());
                    var ctor = new FunctionDef(ValueBindingsScope, typeDef.Name, typeDef, typeDef.ToTypeExpression(), null, 
                        new ParameterDef(ValueBindingsScope, "arg", tupleType, 0));
                    AddCompilerGeneratedFunction(typeDef, ctor);

                    /*
                    TODO: not sure what to do here. Is it really required?  
                    var tupleCast = new FunctionDef(ValueBindingsScope, "Tuple", typeDef, tupleType, null, 
                        new ParameterDef(ValueBindingsScope, "self", typeDef.ToTypeExpression(), 0));
                    AddCompilerGeneratedFunction(typeDef, tupleCast);
                    */
                }

                ValueBindingsScope = ValueBindingsScope.Pop();
                TypeBindingsScope = TypeBindingsScope.Pop();
            }

            return TypeDefs;
        }

        public void AddCompilerGeneratedFunction(TypeDef typeDef, FunctionDef f)
        {
            typeDef.CompilerGeneratedFunctions.Add(f);
            AddToGroupDefinitionAndCreateIfNeeded(f);
        }
        
        public T Scoped<T>(Func<T> f) where T : Symbol
        {
            ValueBindingsScope = ValueBindingsScope.Push();
            var r = f();
            ValueBindingsScope = ValueBindingsScope.Pop();
            return r;
        }

        public TypeExpression GetTypeExpression(string name)
            => GetTypeDefinition(name).ToTypeExpression();

        public TypeExpression CreateTuple(params TypeExpression[] args)
            => args.Length > 10 
                ? throw new Exception("Only tuples up to 10 fields are supported, update here and 'std.primitive.types.plato' if you want more")
                : new TypeExpression(GetTypeDefinition($"Tuple{args.Length}"), args);

        public TypeExpression CreateLambdaType(int args)
        {
            var typeArgs = Enumerable.Range(0, args + 1).Select(i => TypeExpression.CreateTypeVar(TypeBindingsScope, $"$T{i}")).ToArray();
            return new TypeExpression(GetTypeDefinition($"Function{args}"), typeArgs);
        }

        public TypeExpression CreateAny()
            => GetTypeExpression("Any");
    }
}
