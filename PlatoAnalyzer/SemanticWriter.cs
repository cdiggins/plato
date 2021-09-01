using System;
using System.Collections.Generic;
using System.Text;

namespace PlatoAnalyzer
{
    public class SemanticWriter
    {
        public StringBuilder Builder = new StringBuilder();
        public int tabs;

        public override string ToString()
            => Builder.ToString();

        public SemanticWriter Add(string s = null)
        {
            if (s != null)
                Builder.Append(s);
            return this;
        }

        public SemanticWriter Add(IEnumerable<ISemantic> semantics, string separator = ", ")
        {
            var first = true;
            var x = this;
            foreach (var sem in semantics)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (separator == "\n")
                        AddLine();
                    else
                        Add(separator);
                }
                x.Add(sem);
            }

            return x;
        }

        public SemanticWriter Indent()
        {
            tabs++;
            return this;
        }

        public SemanticWriter Outdent()
        {
            tabs--;
            return this;
        }

        public SemanticWriter AddLine(string s = null)
            => Add(s).Add("\n").Add(new string(' ', tabs * 4));

        public SemanticWriter AddBraced(Func<SemanticWriter, SemanticWriter> f)
            => f(Add("{").Indent().AddLine()).Outdent().AddLine().AddLine("}");

        public SemanticWriter AddBraced(IEnumerable<ISemantic> xs)
            => AddBraced(x => x.Add(xs, "\n"));

        public SemanticWriter AddParenthesized(IEnumerable<ISemantic> nodes)
            => Add("(").Add(nodes, ", ").Add(")");

        public SemanticWriter AddIf(bool cond, Func<SemanticWriter, SemanticWriter> f)
            => cond ? this : f(this);

        public SemanticWriter AddBracedIfNeeded(IStatement statement)
            => statement is IBlockStatement st
                ? Add(st)
                : AddBraced(x => x.Add(statement));

        public SemanticWriter AddInitializers(IList<IArg> args) 
            => args?.Count == 0 ? this : Add("{").Add(args).Add("}");

        public SemanticWriter AddTypeArgs(IList<ITypeRef> typeArgs)
            => typeArgs?.Count == 0 ? this : Add("<").Add(typeArgs).Add(">");

        public SemanticWriter Add(ISemantic semantic)
        {
            switch (semantic)
            {
                case IArg arg:
                    return arg.Name != null
                        ? Add(arg.Name).Add(" = ").Add(arg.Value)
                        : Add(arg.Value);
                case IArray array:
                    return Add("new ").Add(array.Type).Add("{").Add(array.Elements, ", ").Add("}");
                case IAssignmentOp assignment:
                    return Add(assignment[0]).Add(" ").Add(assignment.Operator).Add(" ").Add(assignment[1]);
                case IBinaryOp binaryOp:
                    return Add(binaryOp[0]).Add(" ").Add(binaryOp.Operator).Add(" ").Add(binaryOp[1]);
                case IBlockStatement blockStatement:
                    return AddBraced(blockStatement.Statements);
                case IBreakStatement breakStatement:
                    return Add("break;");
                case ICastOp castOp:
                    return Add("(").Add(castOp.Type).Add(")").Add(castOp.Operands[0]);
                case IClass @class:
                    return Add("public class ").Add(@class.Name).AddBraced(@class.Members);
                case ICompoundStatement compoundStatement:
                    return Add(compoundStatement.Statements, "\n");
                case IConditionalOp conditionalOp:
                    return Add(conditionalOp[0]).Add("?").Add(conditionalOp[1]).Add(":").Add(conditionalOp[2]);
                case IConditionalStatement conditionalStatement:
                    return Add("if (").Add(conditionalStatement.Condition).AddLine(")")
                        .AddBracedIfNeeded(conditionalStatement.OnTrue).AddLine("else")
                        .AddBracedIfNeeded(conditionalStatement.OnFalse);
                case IContinueStatement continueStatement:
                    return Add("continue;");
                case IDefaultOp defaultOp:
                    return Add("default"); 
                case IEmptyStatement emptyStatement:
                    return Add(";");
                case IField field:
                    return Add("public ").Add(field.Variable).Add(";");
                case IFunctionDef functionDef:
                    return Add(functionDef.ReturnType).Add(" ").AddParenthesized(functionDef.Parameters)
                        .Add("\n").Add(functionDef.Body);
                case IFunctionRef functionRef:
                    return Add(functionRef.Name).AddTypeArgs(functionRef.TypeArgs);
                case IIndexOp indexOp:
                    return Add(indexOp[0]).Add("[").Add(indexOp[1]).Add("]");
                case IInvocation invocation:
                    return Add(invocation.Function).AddParenthesized(invocation.Args);
                case ILambda lambda:
                    return AddParenthesized(lambda.Function.Parameters).AddLine(" => ").Add(lambda.Function.Body);
                case ILiteral literal:
                    return Add(literal.Value.ToString());
                case IMemberRef memberRef:
                    return Add(memberRef.Reciever).Add(".").Add(memberRef.Name);
                case IMethod method:
                    return Add("public ").Add(method.Function);
                case INewOp newOp:
                    return Add("new ").Add(newOp.Type).AddParenthesized(newOp.Args).AddInitializers(newOp.Initializers);
                case IParameter parameter:
                    return Add(parameter.Type).Add(" ").Add(parameter.Name);
                case IParenthesizedOp parenthesizedOp:
                    return Add("(").Add(parenthesizedOp[0]).Add(")");
                case IPostfixOp postfixOp:
                    return Add(postfixOp[0]).Add(postfixOp.Operator);
                case IPrefixOp prefixOp:
                    return Add(prefixOp.Operator).Add(prefixOp[0]);
                case IProperty property:
                    return Add("TODO");
                case IReturnStatement returnStatement:
                    return Add("return ").Add(returnStatement.Expression).Add(";");
                case IThisOp thisOp:
                    return Add("this");
                case IThrowOp throwOp:
                    return Add("throw ").Add(throwOp[0]);
                case ITupleOp tupleOp:
                    return AddParenthesized(tupleOp.Operands);
                case ITypeOfOp typeOfOp:
                    return Add("typeof(").Add(typeOfOp.Type).Add(")");
                case ITypeParam typeParam:
                    return Add(typeParam.Name);
                case ITypeRef typeRef:
                    return Add(typeRef.Name).AddTypeArgs(typeRef.TypeArgs);
                case IVarDeclaration varDeclaration:
                    return Add(varDeclaration.Type).Add(" ").Add(varDeclaration.Name).AddIf(
                        varDeclaration.Initializer != null, sw => sw.Add(" = ").Add(varDeclaration.Initializer));
                case IVarDeclStatement varDeclStatement:
                    return Add(varDeclStatement.Declaration).Add(";");
                case IWhileStatement whileStatement:
                    return Add("while (").Add(whileStatement.Condition).AddLine(")").Add(whileStatement.Body);
                case IExpressionStatement expressionStatement:
                    return Add(expressionStatement.Expression).Add(";");
                default:
                    throw new ArgumentOutOfRangeException(nameof(semantic));
            }

            return this;
        }
    }
}