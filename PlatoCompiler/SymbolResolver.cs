using Parakeet.Demos.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Plato.Compiler
{
    /// <summary>
    /// Used primarily to figure out what each name means, and what the type of each expression is.
    /// This allows us to figure out which methods will get cal led at run-time.
    /// An abstract evaluator returns abstract values which have types, scopes, and locations 
    /// </summary>
    public class SymbolResolver
    {
        public class ResolutionError
        {
            public AstNode Node { get; }
            public string Message { get; }

            public ResolutionError(string message, AstNode node)
            {
                Node = node;
                Message = message;
            }
        }

        public SymbolResolver(Logger logger)
        {
            BindPredefinedSymbols();
            Logger = logger;
        }

        public Logger Logger { get; }
        public List<ResolutionError> Errors { get; } = new List<ResolutionError>();
        public Scope ValueBindingsScope { get; set; } = new Scope(null, null);
        public Scope TypeBindingsScope { get; set; } = new Scope(null, null);
        public Dictionary<Symbol, AstNode> SymbolsToNames = new Dictionary<Symbol, AstNode>();

        public IEnumerable<TypeDefSymbol> TypeDefs => SymbolsToNames.Keys.OfType<TypeDefSymbol>();
        
        public void BindPredefinedSymbols()
        {
            BindPredefined(null, "intrinsic");
            BindPredefined(null, "Tuple");
            BindType(PrimitiveTypes.Tuple);
            BindType(PrimitiveTypes.Function);
            BindType(PrimitiveTypes.Any);
            BindType(PrimitiveTypes.Self);
            BindType(PrimitiveTypes.String);
            BindType(PrimitiveTypes.Float);
            BindType(PrimitiveTypes.Integer);
            BindType(PrimitiveTypes.Type);
            BindType(PrimitiveTypes.Boolean);
        }

        public void BindPredefined(TypeRefSymbol type, string name)
        {
            BindValue(name, new PredefinedSymbol(type, name));
        }

        public T BindValue<T>(string name, T value) where T : Symbol
        {
            ValueBindingsScope = ValueBindingsScope.Bind(name, value);
            return value;
        }

        public TypeDefSymbol BindType(TypeDefSymbol value)
        {
            if (value.Kind == TypeKind.Library)
                return null;
            return BindType(value.Name, value);
        }

        public TypeDefSymbol BindType(string name, TypeDefSymbol value)
        {
            TypeBindingsScope = TypeBindingsScope.Bind(name, value);
            return value;
        }

        public MemberGroupSymbol BindMemberToGroup(MemberDefSymbol member)
        {
            var name = member.Name;
            var val = ValueBindingsScope.GetValue(name);

            if (val is MemberGroupSymbol group)
            {
                return BindValue(name, group.Add(member));
            }

            var fgs = new MemberGroupSymbol(new[] { member }, name);
            return BindValue(name, fgs);
        }

        public RefSymbol GetValue(string name, AstNode node)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                LogError("Invalid variable name", node);
                return null;
            }

            var sym = ValueBindingsScope.GetValue(name);
            if (sym == null)
            {
                LogError($"Could not find symbol {name}", node);
                return null;
            }

            if (sym is DefSymbol ds)
                return new RefSymbol(ds);

            LogError($"Could not properly resolve symbol {name}", node);
            return null;
        }

        public void LogError(string message, AstNode node)
        {
            Errors.Add(new ResolutionError(message, node));
        }

        // TODO: we might want to resolve types of a specific kind. 
        // Concepts and Libraries can have the same name. 
        public TypeRefSymbol ResolveType(AstTypeNode astTypeNode)
        {
            if (astTypeNode == null)
            {
                LogError("Missing type", astTypeNode);
                return null;
            }
            var name = astTypeNode.Name.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                LogError("Invalid variable name", astTypeNode);
                return null;
            }
            var sym = TypeBindingsScope.GetValue(name);
            if (sym == null)
            {
                LogError($"Could not find type {name}", astTypeNode);
                return null;
            }
            var tds = sym as TypeDefSymbol;

            // TODO: Won't resolve if the type is a parimitive. Will all names potneitally conflict with values.
            // I think I need to have type versus value scopes (or name binding). 

            return new TypeRefSymbol(tds, astTypeNode.TypeArguments.Select(ResolveType).ToArray());
        }

        public ParameterSymbol Resolve(AstParameterDeclaration astParameterDeclaration)
        {
            return BindValue(astParameterDeclaration.Name,
                new ParameterSymbol(astParameterDeclaration.Name, 
                    ResolveType(astParameterDeclaration.Type)));
        }

        public static bool CanCastTo(TypeRefSymbol fromType, TypeRefSymbol toType, bool allowConversions = true)
        {
            var fromDef = fromType.Def;
            var toDef = toType.Def;
            if (fromDef == null || toDef == null) 
                return true;
            if (toDef.Name == "Any")
                return true;
            if (fromDef.Name == "Any")
                return true;
            if (fromDef.Name == toDef.Name)
                return true;
            if (TypeResolver.IsSubType(fromDef, toDef))
                return true;

            if (allowConversions)
            {
                // We look for the implicit operators.
                // TODO: look for functions of the name "ToX" and "FromX" when allowing more than just the default. 

                if (toType.Def.IsType())
                {
                    // TODO: check that the type of each argument matches 
                    if (fromType.Name == "Tuple")
                        return fromType.TypeArgs.Count == toType.Def.Fields.Count;
                    
                    // All types have a constructor that acts as an implicit cast 
                    if (toType.Def.Fields.Count == 1)
                    {
                        var fieldType = toType.Def.Fields[0].Type;
                        return CanCastTo(fromType, fieldType, false);
                    }
                }

                if (fromType.Def.IsType())
                {
                    // TODO: check that the type of each argument matches 
                    if (toType.Name == "Tuple")
                        return toType.TypeArgs.Count == fromType.Def.Fields.Count;

                    // All types with one field can implicit cast to that field. 
                    if (fromType.Def.Fields.Count == 1)
                    {
                        var fieldType = fromType.Def.Fields[0].Type;
                        return CanCastTo(fieldType, toType, false);
                    }
                }
            }

            return false;
        }

        public static int DistanceFromAny(TypeRefSymbol parameterType)
        {
            if (parameterType == null || parameterType.Name == "Any")
                return 1;
            if (parameterType.Def.IsConcept())
                return 2;
            return 3;
        }


        public static int MatchScore(TypeRefSymbol argType, TypeRefSymbol parameterType)
        {
            if (argType == null || argType.Name == "Any")
                return DistanceFromAny(parameterType);
            if (parameterType == null || parameterType.Name == "Any")
                return DistanceFromAny(argType);
            var argDef = argType.Def;
            var paramDef = parameterType.Def;
            if (argDef.Name == paramDef.Name)
                return 1;
            return 1;
        }

        public IReadOnlyList<FunctionSymbol> FindMatchingFunctions(IReadOnlyList<FunctionSymbol> funcs,
            IReadOnlyList<TypeRefSymbol> argumentTypes)
        {
            var candidates = funcs.Where(f => f.Parameters.Count == argumentTypes.Count).ToList();

            for (var i = 0; i < argumentTypes.Count; ++i)
            {
                candidates = candidates.Where(c => Matches(argumentTypes[i], c.Parameters[i].Type)).ToList();
            }

            return candidates;
        }

        public static bool Matches(TypeRefSymbol argType, TypeRefSymbol parameterType)
        {
            if (argType == null) return true;
            if (parameterType == null) return true;
            return CanCastTo(argType, parameterType);
        }

        public static TypeRefSymbol GetArrayParameter(TypeRefSymbol trs)
        {
            if (trs.Name == "Array")
            {
                if (trs.TypeArgs.Count != 1)
                    throw new Exception("Arrays must have one type parameter");
                return trs.TypeArgs[0];
            }

            foreach (var tmp in trs.Def.GetAllImplementedConcepts())
            {
                if (tmp.Name == "Array")
                    return GetArrayParameter(tmp);
            }

            return trs;
        }

        public FunctionSymbol FindBestFunction(AstNode location, MemberGroupSymbol mgs, IReadOnlyList<ArgumentSymbol> arguments)
        {
            var functions = mgs.Members.Select(m => m.Function).Where(f => f.Parameters.Count == arguments.Count).ToList();

            if (functions.Count == 0)
            {
                LogError($"Found no functions in group {mgs} that matched number of arguments {arguments.Count}", location);
                return null;
            }

            var argTypes = arguments.Select(a => a.Type).ToList();
            var candidates = FindMatchingFunctions(functions, argTypes);
            var argsStr = string.Join(", ", arguments.Select(arg => $"{arg.Type}"));

            if (candidates.Count == 0)
            {
                var argTypes2 = argTypes.Select(GetArrayParameter).ToList();
                candidates = FindMatchingFunctions(functions, argTypes2);

                if (candidates.Count == 0)
                {
                    LogError($"Found no functions in group {mgs} that matched arguments ({argsStr})", location);
                    var argTypes3 = argTypes.Select(GetArrayParameter).ToList();
                    return null;
                }
            }

            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            Debug.Assert(candidates.Count > 1);

            var candidateStr = string.Join(",", candidates.Select(c => c.Signature));
            LogError($"Found too many matches ({candidates.Count}) for {mgs} with argument types ({argsStr}) : {candidateStr}", location);
            return candidates[0];
        }
            
        // This is not a pure function. It updates the scope every time a new value is bound
        public Symbol Resolve(AstNode node)
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
                        return BindValue(astAssign.Var,
                            new AssignmentSymbol(
                                GetValue(astAssign.Var, astAssign),
                                Resolve(astAssign.Value)));

                    case AstBlock astBlock:
                    {
                        if (astBlock.Statements.Count == 0)
                            return null;
                        if (astBlock.Statements.Count > 1)
                            LogError("Cannot handle AstBlock with multiple children", astBlock);
                        return Resolve(astBlock.Statements[0]);
                    }

                    case AstMulti astMulti:
                    {
                        if (astMulti.Nodes.Count == 0)
                            return null;
                        if (astMulti.Nodes.Count > 1)
                            LogError("Cannot handle AstMulti with multiple children", astMulti);
                        return Resolve(astMulti.Nodes[0]);
                    }

                    case AstParenthesized astParenthesized:
                        return Resolve(astParenthesized.Inner);

                    case AstReturn astReturn:
                        return Resolve(astReturn.Value);

                    case AstConditional astConditional:
                        return Scoped(() => new ConditionalExpressionSymbol(
                            Resolve(astConditional.Condition),
                            Resolve(astConditional.IfTrue),
                            Resolve(astConditional.IfFalse)));

                    case AstConstant astConstant1:
                        return new LiteralSymbol(astConstant1.Type, astConstant1.Value);

                    case AstExpressionStatement astExpressionStatement:
                        return Scoped(() => Resolve(astExpressionStatement.Expression));

                    case AstIdentifier astIdentifier:
                        return GetValue(astIdentifier.Text, astIdentifier);

                    case AstInvoke astInvoke:
                    {
                        var args = astInvoke.AstArguments.Select((a, i) =>
                            new ArgumentSymbol(Resolve(a), i)).ToArray();
                        foreach (var a in args)
                            if (a.Type == null)
                                throw new Exception("Could not determine argument type");
                        var funcRef = Resolve(astInvoke.Function);

                        if (!(funcRef is RefSymbol rs))
                            throw new Exception("Can only call references");

                        var func = rs.Def; 

                        if (func is FunctionSymbol fs)
                            return new FunctionCallSymbol(fs, args);
                        if (func is MemberGroupSymbol fgs)
                        {
                            var bestFunc = FindBestFunction(astInvoke, fgs, args);
                            return new FunctionCallSymbol(bestFunc, args);
                        }

                        if (func is ParameterSymbol ps)
                            return new FunctionCallSymbol(ps, args);

                        // TODO: Do I really need predefined symbols? Why not have them just as intrinsics.
                        if (func is PredefinedSymbol pds)
                            return new FunctionCallSymbol(pds, args);

                        throw new Exception($"Can't invoke symbol {func}");
                    }

                    case AstLambda astLambda:
                    {
                        ValueBindingsScope = ValueBindingsScope.Push();
                        var ps = astLambda.Parameters.Select(Resolve).ToArray();
                        var body = Resolve(astLambda.Body);
                        var r = new FunctionSymbol("__lambda__",
                            PrimitiveTypes.Lambda.ToRef(), body, ps);
                        return r;
                    }

                    case AstVarDef astVarDef:
                        return BindValue(astVarDef.Name,
                            new VariableSymbol(astVarDef.Name, ResolveType(astVarDef.Type)));

                    case AstLoop astLoop:
                        LogError("NotImplementedException", node);
                        return null;
                }

                LogError($"Node can't be evaluated", node);
                return null;
            }
            catch (Exception e) 
            {
                LogError($"Exception occurred {e}", node);
                return null;
            }
        }

        public IEnumerable<TypeDefSymbol> CreateTypeDefs(IEnumerable<AstTypeDeclaration> types)
        {
            // Typedefs and their methods are all at the top-level
            foreach (var astTypeDeclaration in types)
            {
                var typeDef = new TypeDefSymbol(astTypeDeclaration.Kind, astTypeDeclaration.Name);
                SymbolsToNames.Add(typeDef, astTypeDeclaration);
                BindType(typeDef);
            }

            foreach (var typeDef in TypeDefs.ToList())
            {
                var astTypeDeclaration = SymbolsToNames[typeDef] as AstTypeDeclaration;
                TypeBindingsScope = TypeBindingsScope.Push();

                // TODO: all of the type bindings have to be done again when resolving functions! 

                foreach (var tp in astTypeDeclaration.TypeParameters)
                {
                    var tpd = new TypeParameterDefSymbol(tp.Name, ResolveType(tp.Constraint));
                    BindType(tpd.Name, tpd.Constraint.Def);
                    typeDef.TypeParameters.Add(tpd);
                }

                BindType("Self", typeDef);

                foreach (var m in astTypeDeclaration.Members)
                {
                    if (m is AstFieldDeclaration fd)
                    {
                        var fDef = new FieldDefSymbol(typeDef, ResolveType(fd.Type), fd.Name);
                        typeDef.Fields.Add(fDef);
                        SymbolsToNames.Add(fDef, fd);
                        BindMemberToGroup(fDef);
                    }
                    else if (m is AstMethodDeclaration md)
                    {
                        var mDef = new MethodDefSymbol(typeDef, ResolveType(md.Type), md.Name);

                        // Create an empty body function first. It will be replaced in a separate step.  
                        var list = new List<ParameterSymbol>();
                        foreach (var p in md.Parameters)
                            list.Add(new ParameterSymbol(p.Name, ResolveType(p.Type)));
                        var f = new FunctionSymbol(md.Name, mDef.Type, null, list.ToArray());
                        mDef.Function = f;

                        typeDef.Methods.Add(mDef);
                        SymbolsToNames.Add(mDef, md);
                        BindMemberToGroup(mDef);
                    }
                    else
                    {
                        LogError($"MemberDefSymbol not recognized", m);
                        return null;
                    }
                }

                TypeBindingsScope = TypeBindingsScope.Pop();
            }

            // Foreach type add the inherited and the implemented type.
            foreach (var td in TypeDefs)
            {
                var astTypeDecl = SymbolsToNames[td] as AstTypeDeclaration;

                foreach (var inheritedType in astTypeDecl.Inherits)
                    td.Inherits.Add(ResolveType(inheritedType));

                foreach (var implementedType in astTypeDecl.Implements)
                    td.Implements.Add(ResolveType(implementedType));
            }

            // Foreach type def create the members 
            foreach (var kv in SymbolsToNames)
            {
                var typeDef = kv.Key as TypeDefSymbol;
                var typeDecl = kv.Value as AstTypeDeclaration;

                if (typeDef == null)
                    continue; 

                ValueBindingsScope = ValueBindingsScope.Push();
                TypeBindingsScope = TypeBindingsScope.Push();

                foreach (var tpd in typeDef.TypeParameters)
                    BindType(tpd.Name, tpd.Constraint.Def);

                BindType("Self", typeDef);

                if (typeDef.IsConcept() || typeDef.IsType())
                    BindValue("Self", new PredefinedSymbol(typeDef.ToRef(), "Self"));

                // For each method in the type 
                foreach (var method in typeDef.Methods)
                {
                    ValueBindingsScope = ValueBindingsScope.Push();
                    var location = SymbolsToNames[method] as AstMethodDeclaration;
                    var list = new List<ParameterSymbol>();
                    foreach (var p in location.Parameters)
                    {
                        var pValue = Resolve(p);
                        list.Add(pValue);
                    }

                    var body = Resolve(location.Body);
                    var f = new FunctionSymbol(method.Name, method.Type, body, list.ToArray());
                    method.Function = f;
                    ValueBindingsScope = ValueBindingsScope.Pop();
                }

                ValueBindingsScope = ValueBindingsScope.Pop();
                TypeBindingsScope = TypeBindingsScope.Pop();
            }

            return TypeDefs;
        }

        public T Scoped<T>(Func<T> f) where T : Symbol
        {
            ValueBindingsScope = ValueBindingsScope.Push();
            var r = f();
            ValueBindingsScope = ValueBindingsScope.Pop();
            return r;
        }
    }
}
