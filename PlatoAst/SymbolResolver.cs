using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoAst
{
    /// <summary>
    /// Used primarily to figure out what each name means, and what the type of each expression is.
    /// This allows us to figure out which methods will get called at run-time.
    /// An abstract evaluator returns abstract values which have types, scopes, and locations 
    /// </summary>
    public class SymbolResolver
    {
        public Scope Scope { get; set; } = new Scope(null, null);
        public List<TypeDefSymbol> TypeDefs { get; } = new List<TypeDefSymbol>();

        public T Bind<T>(string name, T value) where T : Symbol
        {
            Scope = Scope.Bind(name, value);
            return value;
        }

        public RefSymbol GetValue(AstNode location, Scope scope, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Invalid variable name");
            var sym = Scope.GetValue(name);
            if (sym == null)
                return null;
            if (sym is DefSymbol ds)
                return new RefSymbol(location, scope, ds);
            throw new NotImplementedException();
        }

        public TypeRefSymbol Resolve(AstTypeNode astTypeNode)
        {
            if (astTypeNode == null)
                return null;
            var name = astTypeNode.Name;
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Invalid variable name");
            var sym = Scope.GetValue(name);
            if (sym == null)
                return null;

            var tds = sym as TypeDefSymbol;

            // TODO: Won't resolve if the type is a parimitive. Will al nameso potneitally conflict with values.
            // I think I need to have type versus value scopes (or name binding). 

            return new TypeRefSymbol(astTypeNode, Scope, tds, astTypeNode.TypeArguments.Select(Resolve).ToArray());
        }

        public ParameterSymbol Resolve(AstParameterDeclaration astParameterDeclaration)
        {
            return Bind(astParameterDeclaration.Name,
                new ParameterSymbol(astParameterDeclaration, Scope, astParameterDeclaration.Name, null));
        }

        // This is not a pure function. It updates the scope every time a new value is bound
        public Symbol Resolve(AstNode node)
        {
            if (node == null)
                return null;

            switch (node)
            {
                case AstTypeNode astTypeNode:
                    return Resolve(astTypeNode);

                case AstAssign astAssign:
                    return Bind(astAssign.Var,
                        new AssignmentSymbol(node, Scope,
                            GetValue(node, Scope, astAssign.Var),
                            Resolve(astAssign.Value)));

                case AstBlock astBlock:
                    return Region(node, astBlock.Statements);

                case AstBreak astBreak:
                    return NoValueSymbol;

                case AstMulti astMulti:
                    return Region(node, astMulti.Nodes, false);

                case AstParenthesized astParenthesized:
                    return Resolve(astParenthesized.Inner);
                
                case AstReturn astReturn:
                    return Region(node, new [] { astReturn.Value });

                case AstConditional astConditional:
                    return Scoped(() => new ConditionalSymbol(node, Scope,
                        Resolve(astConditional.Condition),
                        Resolve(astConditional.IfTrue),
                        Resolve(astConditional.IfFalse)));

                case AstConstant astConstant1:
                    return new LiteralSymbol(node, Scope, astConstant1.Value);

                case AstContinue astContinue:
                    return NoValueSymbol;

                case AstExpressionStatement astExpressionStatement:
                    return Scoped(() => Resolve(astExpressionStatement.Expression));
                    
                case AstIdentifier astIdentifier:
                    return GetValue(node, Scope, astIdentifier.Text);

                case AstIntrinsic astIntrinsic:
                    return new IntrinsicSymbol(astIntrinsic, Scope, astIntrinsic.Name);

                case AstInvoke astInvoke:
                {
                    var args = astInvoke.AstArguments.Select((a,i) => 
                        new ArgumentSymbol(a, Scope, Resolve(a), i)).ToArray();
                    var func = Resolve(astInvoke.Function);

                    // TODO: we need to figure out what function it is because really 
                    // evaluating a name will gives us a method group!
                    return new FunctionResultSymbol(node, Scope, func, args);
                }

                case AstLambda astLambda:
                {
                    Scope = Scope.Push();
                    var ps = astLambda.Parameters.Select(Resolve).ToArray();
                    var body = Resolve(astLambda.Body);
                    var r = new FunctionSymbol(astLambda, Scope, "lambda",
                        Primitives.Lambda.ToRef, body, ps);
                    return r;
                }

                case AstNoop astNoop:
                    return NoValueSymbol;

                case AstVarDef astVarDef:
                    return Bind(astVarDef.Name, 
                        new VariableSymbol(node, Scope, astVarDef.Name, Resolve(astVarDef.Type)));

                case AstLoop astLoop:
                    return Region(node, new[] { astLoop.Condition, astLoop.Body });
            }

            throw new Exception($"Node can't be evaluated : {node}");
        }

        public void CreateTypeDefs(IEnumerable<AstTypeDeclaration> types)
        {
            // Typedefs are at the top-level
            foreach (var astTypeDeclaration in types)
            {
                var typeDef = new TypeDefSymbol(astTypeDeclaration, Scope);
                TypeDefs.Add(typeDef);
                Bind(typeDef.Name, typeDef);
            }

            // Foreach type def create the members 
            foreach (var typeDef in TypeDefs)
            {
                Scope = Scope.Push();

                foreach (var tp in typeDef.AstTypeDeclaration.TypeParameters)
                {
                    var tpd = new TypeParameterDefSymbol(tp, Scope, tp.Name);
                    Bind(tpd.Name, tpd);
                    typeDef.TypeParameters.Add(tpd);
                }

                foreach (var m in typeDef.AstTypeDeclaration.Members)
                {
                    if (m is AstFieldDeclaration fd)
                    {
                        var fDef = new FieldDefSymbol(fd, Scope, Resolve(fd.Type), fd.Name);
                        typeDef.Fields.Add(fDef);
                        Bind(fDef.Name, fDef);
                    }
                    else if (m is AstMethodDeclaration md)
                    {
                        var mDef = new MethodDefSymbol(md, Scope, Resolve(md.Type), md.Name);
                        typeDef.Methods.Add(mDef);
                        Bind(mDef.Name, mDef);
                    }
                    else
                    {
                        throw new NotSupportedException($"MemberDefSymbol not recognized {m}");
                    }
                }

                // For each method in the type 
                foreach (var method in typeDef.Methods)
                {
                    Scope = Scope.Push();
                    var location = method.Location as AstMethodDeclaration;
                    var list = new List<ParameterSymbol>();
                    foreach (var p in location.Parameters)
                    {
                        var pValue = Resolve(p);
                        list.Add(pValue);
                    }

                    var body = Resolve(location.Body);
                    var f = new FunctionSymbol(location, Scope, method.Name, method.Type, body, list.ToArray());
                    method.Function = f;
                    Scope = Scope.Pop();
                }

                Scope = Scope.Pop();
            }

            // Foreach type add the inherited and the implemented type.
            foreach (var td in TypeDefs)
            {
                var astTypeDecl = td.AstTypeDeclaration;
                
                foreach (var inheritedType in astTypeDecl.Inherits)
                    td.Inherits.Add(Resolve(inheritedType));

                foreach (var implementedType in astTypeDecl.Implements)
                    td.Inherits.Add(Resolve(implementedType));
            }
        }

        public T Scoped<T>(Func<T> f) where T : Symbol
        {
            Scope = Scope.Push();
            var r = f();
            Scope = Scope.Pop();
            return r;
        }

        public RegionSymbol Region(AstNode location, IEnumerable<AstNode> nodes, bool scoped = true)
        {
            if (scoped)
                Scope = Scope.Push();
            var r = new RegionSymbol(location, Scope, nodes.Select(Resolve).ToArray());
            if (scoped)
                Scope = Scope.Pop();
            return r;
        }

        public static NoValueSymbol NoValueSymbol = NoValueSymbol.Instance;
    }
}
