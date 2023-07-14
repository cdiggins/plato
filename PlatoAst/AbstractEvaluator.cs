using Parakeet.Demos.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PlatoAst
{
    /// <summary>
    /// Used primarily to figure out what each name means, and what the type of each expression is.
    /// This allows us to figure out which methods will get called at run-time.
    /// An abstract evaluator returns abstract values which have types, scopes, and locations 
    /// </summary>
    public class AbstractEvaluator
    {
        public Scope Scope { get; set; } = new Scope(null, null);
        public List<TypeDef> TypeDefs { get; } = new List<TypeDef>();

        public T Bind<T>(string name, T value) where T : AbstractValue
        {
            Scope = Scope.Bind(name, value);
            return value;
        }

        public AbstractValue GetValue(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Invalid variable name");
            return Scope.GetValue(name);
        }

        public TypeRef Evaluate(AstTypeNode astTypeNode)
        {
            if (astTypeNode == null)
                return null;

            return new TypeRef(astTypeNode, Scope, astTypeNode.Name,
                astTypeNode.TypeArguments.Select(Evaluate).ToArray());
        }

        public Parameter Evaluate(AstParameterDeclaration astParameterDeclaration)
        {
            return Bind(astParameterDeclaration.Name,
                new Parameter(astParameterDeclaration, Scope, astParameterDeclaration.Name, null));
        }

        // This is not a pure function. It updates the scope every time a new value is bound
        public AbstractValue Evaluate(AstNode node)
        {
            if (node == null)
                return null;

            switch (node)
            {
                case AstTypeNode astTypeNode:
                    return Evaluate(astTypeNode);

                case AstAssign astAssign:
                    return Bind(astAssign.Var,
                        new Assignment(node, Scope,
                            GetValue(astAssign.Var),
                            Evaluate(astAssign.Value)));

                case AstBlock astBlock:
                    return Region(node, astBlock.Statements);

                case AstBreak astBreak:
                    return NoValue;

                case AstMulti astMulti:
                    return Region(node, astMulti.Nodes, false);

                case AstParenthesized astParenthesized:
                    return Evaluate(astParenthesized.Inner);
                
                case AstReturn astReturn:
                    return Region(node, new [] { astReturn.Value });

                case AstConditional astConditional:
                    return Scoped(() => new Conditional(node, Scope,
                        Evaluate(astConditional.Condition),
                        Evaluate(astConditional.IfTrue),
                        Evaluate(astConditional.IfFalse)));

                case AstConstant astConstant1:
                    return new Literal(node, Scope, astConstant1.Value);

                case AstContinue astContinue:
                    return NoValue;

                case AstExpressionStatement astExpressionStatement:
                    return Scoped(() => Evaluate(astExpressionStatement.Expression));
                    
                case AstIdentifier astIdentifier:
                    return GetValue(astIdentifier.Text);

                case AstIntrinsic astIntrinsic:
                    return new Intrinsic(astIntrinsic, Scope, astIntrinsic.Name);

                case AstInvoke astInvoke:
                {
                    var args = astInvoke.AstArguments.Select((a,i) => 
                        new Argument(a, Scope, Evaluate(a), i)).ToArray();
                    var func = Evaluate(astInvoke.Function);

                    // TODO: we need to figure out what function it is because really 
                    // evaluating a name will gives us a method group!
                    return new FunctionResult(node, Scope, TypeRef.NotImplemented, func, args);
                }
                
                case AstLambda astLambda:
                    return new Function(astLambda, Scope, "lambda",
                        TypeRef.Inferred, Scoped(() => Evaluate(astLambda.Body)),
                        astLambda.Parameters.Select(Evaluate).ToArray());
 
                case AstNoop astNoop:
                    return NoValue;

                case AstMemberAccess astMemberAccess:
                    return new MemberRef(node, Scope, null, null, 
                        Evaluate(astMemberAccess.Receiver));
                
                case AstVarDef astVarDef:
                    return Bind(astVarDef.Name, 
                        new Variable(node, Scope, astVarDef.Name, Evaluate(astVarDef.Type)));

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
                var typeDef = new TypeDef(astTypeDeclaration, Scope);
                TypeDefs.Add(typeDef);
                Bind(typeDef.Name, typeDef);
            }

            // Foreach type def create the members 
            foreach (var typeDef in TypeDefs)
            {
                Scope = Scope.Push();

                foreach (var tp in typeDef.AstTypeDeclaration.TypeParameters)
                {
                    var tpd = new TypeParameterDef(tp, Scope, tp.Name);
                    Bind(tpd.Name, tpd);
                    typeDef.TypeParameters.Add(tpd);
                }

                foreach (var m in typeDef.AstTypeDeclaration.Members)
                {
                    if (m is AstFieldDeclaration fd)
                    {
                        var fDef = new FieldDef(fd, Scope, Evaluate(fd.Type), fd.Name);
                        typeDef.Fields.Add(fDef);
                        Bind(fDef.Name, fDef);
                    }
                    else if (m is AstMethodDeclaration md)
                    {
                        var mDef = new MethodDef(md, Scope, Evaluate(md.Type), md.Name);
                        typeDef.Methods.Add(mDef);
                        Bind(mDef.Name, mDef);
                    }
                    else
                    {
                        throw new NotSupportedException($"Member not recognized {m}");
                    }
                }

                // For each method in the type 
                foreach (var method in typeDef.Methods)
                {
                    Scope = Scope.Push();
                    var location = method.Location as AstMethodDeclaration;
                    var list = new List<Parameter>();
                    foreach (var p in location.Parameters)
                    {
                        var pValue = Evaluate(p);
                        list.Add(pValue);
                    }

                    var body = Evaluate(location.Body);
                    var f = new Function(location, Scope, method.Name, method.Type, body, list.ToArray());
                    method.Function = f;
                    Scope = Scope.Pop();
                }

                // 
                Scope = Scope.Pop();
            }
        }

    /*
    public object Evaluate(AstLambda lambda)
    {
        object LambdaImplementation(AbstractEvaluator e, AstNode[] args)
        {
            e.PushScope();
            if (args.Length != lambda.Parameters.Count) throw new Exception("Mismatched argument length size");
            for (var i = 0; i < args.Length; i++)
            {
                var a = args[i];
                var p = lambda.Parameters[i];
                e.Current.Declare(p, a);
            }

            var r = e.Evaluate(lambda.Body);
            e.PopScope();
            return r;
        }

        return ((Func<AbstractEvaluator, AstNode[], object>)LambdaImplementation).Function;
    }
    */

        public T Scoped<T>(Func<T> f) where T : AbstractValue
        {
            Scope = Scope.Push();
            var r = f();
            Scope = Scope.Pop();
            return r;
        }

        public Region Region(AstNode location, IEnumerable<AstNode> nodes, bool scoped = true)
        {
            if (scoped)
                Scope = Scope.Push();
            var r = new Region(location, Scope, nodes.Select(Evaluate).ToArray());
            if (scoped)
                Scope = Scope.Pop();
            return r;
        }

        public static NoValue NoValue = NoValue.Instance;

        public static AbstractValue EvaluateTopLevel(AstTypeDeclaration astTypeDeclaration)
        {
            throw new NotImplementedException();
        }
    }
}
