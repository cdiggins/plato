using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Ast;
using Plato.Compiler.Types;

namespace Plato.Compiler.Symbols
{
    /// <summary>
    /// Used primarily to figure out what each name means, and what the type of each expression is.
    /// This allows us to figure out which methods will get called at run-time.
    /// </summary>
    public class SymbolFactory
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

        public SymbolFactory(Logger logger)
        {
            BindPredefinedSymbols();
            Logger = logger;
        }

        public Logger Logger { get; }
        public List<ResolutionError> Errors { get; } = new List<ResolutionError>();
        public Scope ValueBindingsScope { get; set; } = new Scope(null);
        public Scope TypeBindingsScope { get; set; } = new Scope(null);

        public Dictionary<Symbol, AstNode> SymbolsToNodes = new Dictionary<Symbol, AstNode>();

        public List<TypeDefinition> TypeDefs { get; } = new List<TypeDefinition>();

        public void BindPredefinedSymbols()
        {
            BindType(PrimitiveTypeDefinitions.Tuple);
            BindType(PrimitiveTypeDefinitions.Function);
            // TODO: is this used? 
            BindPredefined(null, "intrinsic");
            // TODO: what is this? 
            BindPredefined(PrimitiveTypeDefinitions.Tuple.ToTypeExpression(), "Tuple");
        }

        public void BindPredefined(TypeExpression type, string name)
        {
            BindValue(name, new PredefinedDefinition(type, name));
        }

        public T BindValue<T>(string name, T value) where T : Symbol
        {
            ValueBindingsScope = ValueBindingsScope.Bind(name, value);
            return value;
        }

        public TypeDefinition BindType(TypeDefinition value)
        {
            if (value.Kind == TypeKind.Library)
                return null;
            return BindType(value.Name, value);
        }

        public TypeDefinition BindType(string name, TypeDefinition value)
        {
            TypeBindingsScope = TypeBindingsScope.Bind(name, value);
            return value;
        }

        public FunctionGroupDefinition CreateOrLookupGroupDefinition(MemberDefinition member)
            => CreateOrLookupGroupDefinition(member.Name);

        public FunctionGroupDefinition CreateOrLookupGroupDefinition(string name)
        {
            var val = ValueBindingsScope.GetValue(name);
            if (val is FunctionGroupDefinition group)
                return group;

            var fgs = new FunctionGroupDefinition(Array.Empty<FunctionDefinition>(), name);
            return BindValue(name, fgs);
        }

        public void AddToGroupDefinitionAndCreateIfNeeded(FunctionDefinition function)
            => CreateOrLookupGroupDefinition(function.Name).Add(function);

        public FunctionGroupDefinition AddToGroupDefinition(FunctionDefinition function)
        {
            var name = function.Name;
            var val = ValueBindingsScope.GetValue(name);
            if (val == null)
                throw new Exception($"Could not find function group {function.Name}");

            if (val is FunctionGroupDefinition group)
                group.Add(function);
            else
                throw new Exception($"Expected a group definition but got a {val}");
            return group;
        }

        public Reference GetValue(string name, AstNode node)
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

            if (sym is DefinitionSymbol def)
            {
                return def.ToReference();
            }

            LogError($"Could not properly resolve symbol {name}", node);
            return null;
        }

        public void LogError(string message, AstNode node)
        {
            Errors.Add(new ResolutionError(message, node));
        }

        public TypeDefinition GetTypeDefinition(string name)
        {
            return TypeBindingsScope.GetValue(name) as TypeDefinition;
        }

        public TypeExpression ResolveType(AstTypeNode astTypeNode)
        {
            if (astTypeNode == null)
            {
                //LogError("Missing type", astTypeNode);
                //return null;
                return CreateAny();
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
            var tds = sym as TypeDefinition;
            if (tds == null)
            {
                LogError($"Could not resolve type {name} instead got {sym}", astTypeNode);
                return null;
            }
            return new TypeExpression(tds, astTypeNode.TypeArguments.Select(ResolveType).ToArray());
        }

        public ParameterDefinition Resolve(AstParameterDeclaration astParameterDeclaration)
        {
            return BindValue(astParameterDeclaration.Name,
                new ParameterDefinition(astParameterDeclaration.Name, 
                    ResolveType(astParameterDeclaration.Type)));
        }

        public Expression ResolveExpr(AstNode node)
        {
            var r = Resolve(node);
            if (r == null) return null;
            if (r is Expression x) return x;
            throw new Exception($"Expected an expression not {r}");
        }

        public Symbol Resolve(AstNode node)
        {
            var r = InternalResolve(node);
            if (r != null)
                SymbolsToNodes[r] = node;
            return r;
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
                        return BindValue(astAssign.Var,
                            new Assignment(
                                GetValue(astAssign.Var, astAssign),
                                ResolveExpr(astAssign.Value)));

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
                        return Scoped(() => new ConditionalExpression(
                            ResolveExpr(astConditional.Condition),
                            ResolveExpr(astConditional.IfTrue),
                            ResolveExpr(astConditional.IfFalse)));

                    case AstConstant astConstant1:
                        return new Literal(astConstant1.TypeEnum, astConstant1.Value);

                    case AstExpressionStatement astExpressionStatement:
                        return Scoped(() => Resolve(astExpressionStatement.Expression));

                    case AstIdentifier astIdentifier:
                        return GetValue(astIdentifier.Text, astIdentifier);

                    case AstInvoke astInvoke:
                        {
                            var args = astInvoke.Arguments.Select((a, i) =>
                                new Argument(ResolveExpr(a), i)).ToArray();
                            var funcRef = ResolveExpr(astInvoke.Function);
                            if (funcRef == null)
                            {
                                LogError($"Could not find function {astInvoke.Function}", astInvoke);
                            }

                            return new FunctionCall(funcRef, args);
                        }

                    case AstLambda astLambda:
                        {
                            ValueBindingsScope = ValueBindingsScope.Push();
                            var ps = astLambda.Parameters.Select(Resolve).ToArray();
                            var body = ResolveExpr(astLambda.Body);
                            var r = new Lambda(new FunctionDefinition("_lambda_", null, 
                                PrimitiveTypeDefinitions.Function.ToTypeExpression(), body, ps));
                            return r;
                        }

                    case AstVarDef astVarDef:
                        return BindValue(astVarDef.Name,
                            new VariableDefinition(astVarDef.Name, ResolveType(astVarDef.Type)));

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

        public IEnumerable<TypeDefinition> CreateTypeDefs(IEnumerable<AstTypeDeclaration> types)
        {
            // Typedefs and their methods are all at the top-level
            foreach (var astTypeDeclaration in types)
            {
                var typeDef = new TypeDefinition(astTypeDeclaration.Kind, astTypeDeclaration.Name);
                SymbolsToNodes.Add(typeDef, astTypeDeclaration);
                BindType(typeDef);
                TypeDefs.Add(typeDef);
            }

            foreach (var typeDef in TypeDefs)
            {
                var astTypeDeclaration = SymbolsToNodes[typeDef] as AstTypeDeclaration;
                TypeBindingsScope = TypeBindingsScope.Push();
                foreach (var tp in astTypeDeclaration.TypeParameters)
                {
                    var tpd = new TypeParameterDefinition(tp.Name, ResolveType(tp.Constraint));
                    BindType(tpd.Name, tpd);
                    typeDef.TypeParameters.Add(tpd);
                }

                // Add a Self type
                if (typeDef.Self != null)
                    BindType(typeDef.Self);
                
                foreach (var m in astTypeDeclaration.Members)
                {
                    if (m is AstFieldDeclaration fd)
                    {
                        var fDef = new FieldDefinition(typeDef, ResolveType(fd.Type), fd.Name);
                        typeDef.Fields.Add(fDef);
                        SymbolsToNodes.Add(fDef, fd);
                        CreateOrLookupGroupDefinition(fDef);
                    }
                    else if (m is AstMethodDeclaration md)
                    {
                        var mDef = new MethodDefinition(typeDef, ResolveType(md.Type), md.Name);
                        typeDef.Methods.Add(mDef);
                        SymbolsToNodes.Add(mDef, md);
                        CreateOrLookupGroupDefinition(mDef);
                    }
                    else
                    {
                        LogError($"MemberDefSymbol not recognized", m);
                        return null;
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
                    if (tpd.Constraint != null)
                        BindType(tpd.Name, tpd.Constraint.Definition);

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

                // Add a Self type
                if (typeDef.Self != null)
                    BindType(typeDef.Self);

                // For each method in the type create a function, and add it to a function group 
                foreach (var m in typeDef.Methods)
                {
                    // Create the function 
                    ValueBindingsScope = ValueBindingsScope.Push();
                    var location = SymbolsToNodes[m] as AstMethodDeclaration;
                    if (location == null) throw new Exception($"Missing AstMethodDeclaration at {location}");
                    var list = new List<ParameterDefinition>();
                    foreach (var p in location.Parameters)
                    {
                        var pValue = Resolve(p);
                        list.Add(pValue);
                    }

                    var body = ResolveExpr(location.Body);
                    var f = new FunctionDefinition(m.Name, typeDef, m.Type, body, list.ToArray());
                    Debug.Assert(m.Function == null);
                    m.Function = f;

                    AddToGroupDefinition(f);

                    // Check if this is a static method, but not on a library. 
                    if (!typeDef.IsLibrary() && f.Parameters.Count == 0)
                    {
                        Verifier.Assert(list.Count == 0);

                        // We create a second version of the function that accepts a member of the given type
                        // This is something that C# does not support, but is convenient in generic code. (e.g., x += x.One). 
                        var f2 = new FunctionDefinition(m.Name, typeDef, m.Type, body, new ParameterDefinition("_", typeDef.SelfTypeExpression));
                        AddCompilerGeneratedFunction(typeDef, f2);
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
                    var ctor = new FunctionDefinition(typeDef.Name, typeDef, typeDef.SelfTypeExpression, null,
                        typeDef.Fields.Select(f => new ParameterDefinition(f.Name, f.Type)).ToArray());
                    AddCompilerGeneratedFunction(typeDef, ctor);
                }

                // If there is exactly one field, add a "ToX" implicit cast function for that field 
                if (typeDef.Fields.Count == 1)
                {
                    var field = typeDef.Fields[0];
                    var cast = new FunctionDefinition($"To{field.Name}", typeDef, field.Type, null,
                        new ParameterDefinition("arg", typeDef.SelfTypeExpression));
                    AddCompilerGeneratedFunction(typeDef, cast);
                }

                // If there are more than one fields, Add a tuple constructor and a ToTuple implicit cast function 
                if (typeDef.Fields.Count > 1)
                {
                    var tupleType = CreateTuple(typeDef.Fields.Select(f => f.Type).ToArray());
                    var ctor = new FunctionDefinition(typeDef.Name, typeDef, typeDef.SelfTypeExpression, null,
                        new ParameterDefinition("arg", tupleType));
                    AddCompilerGeneratedFunction(typeDef, ctor);

                    var tupleCast = new FunctionDefinition("ToTuple", typeDef, tupleType, null,
                        new ParameterDefinition("self", typeDef.SelfTypeExpression));
                    AddCompilerGeneratedFunction(typeDef, tupleCast);
                }

                ValueBindingsScope = ValueBindingsScope.Pop();
                TypeBindingsScope = TypeBindingsScope.Pop();
            }

            return TypeDefs;
        }

        public void AddCompilerGeneratedFunction(TypeDefinition typeDef, FunctionDefinition f)
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
        {
            return GetTypeDefinition(name).ToTypeExpression();
        }

        public TypeExpression CreateTuple(params TypeExpression[] args)
        {
            return new TypeExpression(GetTypeDefinition("Tuple"), args);
        }

        public TypeExpression CreateAny()
            => GetTypeExpression("Any");
    }
}
