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
        public TypeDef CurrentType { get; set; }
        public TypeNames TypeNames { get; set; }

        public Dictionary<Method, AbstractValue> FunctionBodies { get; } = new Dictionary<Method, AbstractValue>();

        public AbstractValue Bind(string name, AbstractValue value)
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

        public TypeRef GetType(AstTypeNode typeNode)
        {
            return new TypeRef(typeNode, Scope, typeNode.Name,
                typeNode.TypeArguments.Select(GetType).ToArray());
        }

        // This is not a pure function. It updates the scope every time a new value is bound
        public AbstractValue Evaluate(AstNode node)
        {
            if (node == null)
                return null;

            switch (node)
            {
                // TODO: it seems that referencing a type is the same as any other kind of abstract value. 
                case AstTypeNode astTypeNode:
                    throw new NotImplementedException();

                case AstParameterDeclaration astParameterDeclaration:
                    return Bind(astParameterDeclaration.Name,
                        new Parameter(node, Scope, astParameterDeclaration.Name, null));

                case AstMethodDeclaration astMethod:
                    // Start a new scope
                    Scope = Scope.Push();
                    
                    var parameters = astMethod.Parameters.Select(Evaluate).Cast<Parameter>().ToArray();

                    // Make the method available to itself for recursion
                    var function = new Function(node, Scope, astMethod.Name, GetType(astMethod.Type), parameters);
                    var method = new Method(node, Scope, function);
                    Bind(astMethod.Name, method);

                    FunctionBodies.Add(method, null);

                    // Return to outer scope 
                    Scope = Scope.Pop();

                    // Make the method available to the outer scope as well. 
                    return method;

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
                    // TODO: store the intrinsics in a top-level scope 
                    throw new NotImplementedException();

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
                    // 
                    throw new NotImplementedException();

                case AstNoop astNoop:
                    return NoValue;

                case AstMemberAccess astMemberAccess:
                    // TODO: figure out the correct type def. 
                    return new MemberRef(node, Scope, null, null, 
                        Evaluate(astMemberAccess.Receiver));
                
                case AstVarDef astVarDef:
                    return Bind(astVarDef.Name, new Variable(node, Scope, astVarDef.Name,
                        GetType(astVarDef.Type)));

                case AstDirective astDirective:
                    return NoValue;

                case AstLoop astLoop:
                    return Region(node, new[] { astLoop.Condition, astLoop.Body });

                case AstFieldDeclaration astFieldDeclaration:
                    return new Field(node, Scope, new Variable(node, Scope,
                        astFieldDeclaration.Name, GetType(astFieldDeclaration.Type)));
                
                case AstNamespace astNamespace:
                    return new Namespace(node, Scope, astNamespace.Name, 
                        astNamespace.Children.Select(Evaluate));

                case AstTypeDeclaration astTypeDeclaration:
                {
                    var typeDef = new TypeDef(astTypeDeclaration, Scope);
                    Scope = Scope.Push();
                    foreach (var t in astTypeDeclaration.TypeParameters)
                    {
                        var tp = new TypeParameter(t, Scope, t.Name);
                        Bind(tp.Name, tp);
                        typeDef.TypeParameters.Add(tp);
                    }

                    foreach (var m in astTypeDeclaration.Members)
                    {

                    }
                    return typeDef;
                }

                case AstDeclaration astDeclaration:
                case AstError astError:
                case AstLeaf astLeaf:
                    break;
            }

            throw new Exception($"Node can't be evaluated : {node}");
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
