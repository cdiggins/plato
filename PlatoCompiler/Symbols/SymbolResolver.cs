﻿using System;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Ast;

namespace Plato.Compiler.Symbols
{
    /// <summary>
    /// Used primarily to figure out what each name means, and what the type of each expression is.
    /// This allows us to figure out which methods will get called at run-time.
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
        public Scope ValueBindingsScope { get; set; } = new Scope(null);
        public Scope TypeBindingsScope { get; set; } = new Scope(null);

        public Dictionary<Symbol, AstNode> SymbolsToNodes = new Dictionary<Symbol, AstNode>();

        public List<TypeDefinition> TypeDefs { get; } = new List<TypeDefinition>();

        public void BindPredefinedSymbols()
        {
            BindType(PrimitiveTypeDefinitions.Tuple);
            BindType(PrimitiveTypeDefinitions.Function);
            BindType(PrimitiveTypeDefinitions.Self);
            BindPredefined(null, "intrinsic");
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

        public FunctionGroupDefinition BindMemberToGroup(MemberDefinition member)
        {
            var name = member.Name;
            var val = ValueBindingsScope.GetValue(name);

            if (val is FunctionGroupDefinition group)
            {
                return BindValue(name, group.Add(member.Function));
            }

            var fgs = new FunctionGroupDefinition(new[] { member.Function }, name);
            return BindValue(name, fgs);
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

            if (sym is Definition def)
                return def.ToReference();

            LogError($"Could not properly resolve symbol {name}", node);
            return null;
        }

        public void LogError(string message, AstNode node)
        {
            Errors.Add(new ResolutionError(message, node));
        }

        public TypeExpression ResolveType(AstTypeNode astTypeNode)
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
                            var r = new Lambda(new FunctionDefinition("_lambda_", 
                                PrimitiveTypeDefinitions.Lambda.ToTypeExpression(), body, ps));
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

                foreach (var m in astTypeDeclaration.Members)
                {
                    if (m is AstFieldDeclaration fd)
                    {
                        var fDef = new FieldDefinition(typeDef, ResolveType(fd.Type), fd.Name);
                        typeDef.Fields.Add(fDef);
                        SymbolsToNodes.Add(fDef, fd);
                        BindMemberToGroup(fDef);
                    }
                    else if (m is AstMethodDeclaration md)
                    {
                        var mDef = new MethodDefinition(typeDef, ResolveType(md.Type), md.Name);

                        // Create an empty body function first. It will be replaced in a separate step.  
                        var list = new List<ParameterDefinition>();
                        foreach (var p in md.Parameters)
                            list.Add(new ParameterDefinition(p.Name, ResolveType(p.Type)));
                        var f = new FunctionDefinition(md.Name, mDef.Type, null, list.ToArray());
                        mDef.Function = f;

                        typeDef.Methods.Add(mDef);
                        SymbolsToNodes.Add(mDef, md);
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
            foreach (var typeDef in TypeDefs)
            {
                ValueBindingsScope = ValueBindingsScope.Push();
                TypeBindingsScope = TypeBindingsScope.Push();

                foreach (var tpd in typeDef.TypeParameters)
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

                if (typeDef.IsConcept() || typeDef.IsType())
                    BindValue("Self", new PredefinedDefinition(typeDef.ToTypeExpression(), "Self"));

                // TODO: why is only the "self" type bound? Why can't I refer to other names?

                // For each method in the type 
                foreach (var method in typeDef.Methods)
                {
                    ValueBindingsScope = ValueBindingsScope.Push();
                    var location = SymbolsToNodes[method] as AstMethodDeclaration;
                    var list = new List<ParameterDefinition>();
                    foreach (var p in location.Parameters)
                    {
                        var pValue = Resolve(p);
                        list.Add(pValue);
                    }

                    var body = ResolveExpr(location.Body);
                    var f = new FunctionDefinition(method.Name, method.Type, body, list.ToArray());
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