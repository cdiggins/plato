using System;
using System.Linq;
using System.Reflection;

namespace PlatoAst
{
    public class Evaluator
    {
        public Scope Root { get; } = new Scope(null);
        public Scope Current { get; private set; }

        public Evaluator()
        {
            Current = Root;
        }

        public void PushScope()
        {
            Current = new Scope(Current);
        }

        public void PopScope()
        {
            Current = Current.Parent;
        }

        public object Evaluate(AstNode node)
        {
            switch (node)
            {
                case AstAssign astAssign: 
                    return Evaluate(astAssign);
                case AstBlock astBlock: 
                    return Evaluate(astBlock);
                case AstConditional astConditional:
                    return Evaluate(astConditional);
                case AstInvoke astInvoke:
                    return Evaluate(astInvoke);
                case AstLambda astLambda:
                    return Evaluate(astLambda);
                case AstLoop astLoop:
                    return Evaluate(astLoop);
                case AstVarDef astVarDef:
                    return Evaluate(astVarDef);
                case AstVarRef astVarRef:
                    return Evaluate(astVarRef);
                case AstReducible _:
                    throw new NotSupportedException(
                        $"Nodes of type {node.GetType()} are reducible and should be removed before evaluating");
                default:
                    throw new ArgumentOutOfRangeException(nameof(node));
            }
        }

        public object Evaluate(AstBlock block)
        {
            PushScope();
            var r = block.Statements.Aggregate(null as object, (_, node) => Evaluate(node));
            PopScope();
            return r;
        }

        public object Evaluate(AstConditional conditional)
            => (bool)Evaluate(conditional.Condition) 
                ? Evaluate(conditional.IfTrue) 
                : Evaluate(conditional.IfFalse);

        public object Evaluate(AstVarDef def)
        {
            Current.Declare(def);
            return new AstVarRef(def.Name);
        }
        
        public object Evaluate(AstVarRef var)
            => Current.FindName(var.Name).GetValue(var.Name);

        public object Evaluate(AstLoop loop)
        {
            object tmp = null;
            while ((bool)Evaluate(loop.Condition))
            {
                tmp = Evaluate(loop.Body);
            }
            return tmp;
        }

        public object Evaluate(AstInvoke invoke)
        {
            var args = invoke.AstArguments.Select(Evaluate).ToArray();
            var func = (MethodBase)Evaluate(invoke.Function);
            return func.Invoke(null, args);
        }

        public object Evaluate(AstAssign assign)
        {
            var val = Evaluate(assign.Value);
            var name = assign.Var.Name;
            Current.FindName(name).Bind(name, val);
            return val;
        }

        public object Evaluate(AstLambda lambda)
        {
            object LambdaImplementation(Evaluator e, AstNode[] args)
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

            return ((Func<Evaluator, AstNode[], object>)LambdaImplementation).Method;
        }
    }
}
