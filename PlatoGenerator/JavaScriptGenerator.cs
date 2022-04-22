using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlatoGenerator
{
    public class JavaScriptGenerator
    {
        public Dictionary<SyntaxNode, FunctionDefinition> SyntaxToFunctions = new Dictionary<SyntaxNode, FunctionDefinition>();
        public Dictionary<SyntaxNode, PropertyDefinition> SyntaxToProperties = new Dictionary<SyntaxNode, PropertyDefinition>();

        public StreamWriter sw;

        // TODO: catch and deal with recursion.
        //public HashSet<FunctionDefinition> InliningFunction;

        public string LogFileName = Path.Combine(Path.GetTempPath(), "plato.js");

        public string ReduceExpression(Expression expr, bool declareVar = true)
        {
            var childNames = expr.Children.Select(x => ReduceExpression(x)).ToList();
            var args = string.Join(",", childNames);
            var ds = expr.DeclarationSyntax;
            var dsKind = ds?.Kind().ToString() ?? "unknown declaration syntax";

            // TODO: somehow this isn't always expression syntax. I had a cast and it blew up. 
            var syntax = expr.Syntax;
            switch (syntax)
            {
                case AliasQualifiedNameSyntax aliasQualifiedNameSyntax:
                    break;
                case AnonymousMethodExpressionSyntax anonymousMethodExpressionSyntax:
                    break;
                case AnonymousObjectCreationExpressionSyntax anonymousObjectCreationExpressionSyntax:
                    break;
                case ArrayCreationExpressionSyntax arrayCreationExpressionSyntax:
                    break;
                case ArrayTypeSyntax arrayTypeSyntax:
                    break;
                case AssignmentExpressionSyntax assignmentExpressionSyntax:
                    return $"{childNames[0]} {assignmentExpressionSyntax.OperatorToken} {childNames[1]}";
                case AwaitExpressionSyntax awaitExpressionSyntax:
                    break;
                case BaseExpressionSyntax baseExpressionSyntax:
                    return "base";
                case CastExpressionSyntax castExpressionSyntax:
                    break;
                case CheckedExpressionSyntax checkedExpressionSyntax:
                    break;
                case ConditionalAccessExpressionSyntax conditionalAccessExpressionSyntax:
                    break;
                case DeclarationExpressionSyntax declarationExpressionSyntax:
                    break;
                case DefaultExpressionSyntax defaultExpressionSyntax:
                    return "default";
                case ElementAccessExpressionSyntax elementAccessExpressionSyntax:
                    break;
                case ElementBindingExpressionSyntax elementBindingExpressionSyntax:
                    break;
                case FunctionPointerTypeSyntax functionPointerTypeSyntax:
                    break;
                case GenericNameSyntax genericNameSyntax:
                    break;
                case IdentifierNameSyntax identifierNameSyntax:
                    return identifierNameSyntax.Identifier.ToString();
                case ImplicitArrayCreationExpressionSyntax implicitArrayCreationExpressionSyntax:
                    break;
                case ImplicitElementAccessSyntax implicitElementAccessSyntax:
                    break;
                case ImplicitObjectCreationExpressionSyntax implicitObjectCreationExpressionSyntax:
                {
                    var type = expr.Type;
                    // TODO: get the type ID. I currently don't have the PlatoType.
                    return $"new {type.Name}({args})";
                }

                case ImplicitStackAllocArrayCreationExpressionSyntax implicitStackAllocArrayCreationExpressionSyntax:
                    break;
                case InitializerExpressionSyntax initializerExpressionSyntax:
                    break;
                case InterpolatedStringExpressionSyntax interpolatedStringExpressionSyntax:
                    break;
                case InvocationExpressionSyntax invocationExpressionSyntax:
                    break;
                case IsPatternExpressionSyntax isPatternExpressionSyntax:
                    break;
                case LiteralExpressionSyntax literalExpressionSyntax:
                    return literalExpressionSyntax.ToString();

                case MakeRefExpressionSyntax makeRefExpressionSyntax:
                    break;
                case MemberAccessExpressionSyntax memberAccessExpressionSyntax:
                    if (memberAccessExpressionSyntax.OperatorToken.ToString() != ".")
                        throw new NotImplementedException("Only . access is supported at current time");
                    return $"{childNames.Single()}.{memberAccessExpressionSyntax.Name.Identifier}";
                case MemberBindingExpressionSyntax memberBindingExpressionSyntax:
                    break;
                case NullableTypeSyntax nullableTypeSyntax:
                    break;
                case ObjectCreationExpressionSyntax objectCreationExpressionSyntax:
                    {
                        var type = expr.Type;
                        // TODO: get the type ID. I currently don't have the PlatoType.
                        return $"new {type.Name}({args})";
                    }
                case OmittedArraySizeExpressionSyntax omittedArraySizeExpressionSyntax:
                    break;
                case OmittedTypeArgumentSyntax omittedTypeArgumentSyntax:
                    break;
                case ParenthesizedExpressionSyntax parenthesizedExpressionSyntax:
                    return childNames.Single();
                case ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax:
                    break;
                case PointerTypeSyntax pointerTypeSyntax:
                    break;
                case PredefinedTypeSyntax predefinedTypeSyntax:
                    break;
                case QualifiedNameSyntax qualifiedNameSyntax:
                    break;
                case QueryExpressionSyntax queryExpressionSyntax:
                    break;
                case RangeExpressionSyntax rangeExpressionSyntax:
                    break;
                case RefExpressionSyntax refExpressionSyntax:
                    break;
                case RefTypeExpressionSyntax refTypeExpressionSyntax:
                    break;
                case RefTypeSyntax refTypeSyntax:
                    break;
                case RefValueExpressionSyntax refValueExpressionSyntax:
                    break;
                case SimpleLambdaExpressionSyntax simpleLambdaExpressionSyntax:
                    break;
                case SizeOfExpressionSyntax sizeOfExpressionSyntax:
                    break;
                case StackAllocArrayCreationExpressionSyntax stackAllocArrayCreationExpressionSyntax:
                    break;
                case SwitchExpressionSyntax switchExpressionSyntax:
                    // TODO: turn this into a Switch statement
                    break;
                case ThisExpressionSyntax thisExpressionSyntax:
                    return "this";
                case ThrowExpressionSyntax throwExpressionSyntax:
                    return $"throw {childNames.FirstOrDefault()}";
                case TupleExpressionSyntax tupleExpressionSyntax:
                    break;
                case TupleTypeSyntax tupleTypeSyntax:
                    break;
                case TypeOfExpressionSyntax typeOfExpressionSyntax:
                    break;
                case WithExpressionSyntax withExpressionSyntax:
                    break;
                case BaseObjectCreationExpressionSyntax baseObjectCreationExpressionSyntax:
                    break;
                case InstanceExpressionSyntax instanceExpressionSyntax:
                    break;
                case LambdaExpressionSyntax lambdaExpressionSyntax:
                    break;
                case SimpleNameSyntax simpleNameSyntax:
                    return simpleNameSyntax.Identifier.ToString();
                case AnonymousFunctionExpressionSyntax anonymousFunctionExpressionSyntax:
                    break;
                case NameSyntax nameSyntax:
                    return nameSyntax.ToString();
                case TypeSyntax typeSyntax:
                    break;
                default:
                    // TODO: when I figure out why expressions have non-expression syntax, reinstate this check. 
                    //throw new ArgumentOutOfRangeException(nameof(syntax));
                    break;
            }

            // TODO: do the same for initializers. 
            var exprVarName = $"var{expr.Id}";
            FunctionDefinition def = null;
            if (ds != null) SyntaxToFunctions.TryGetValue(ds, out def);
            var defText = def != null ? $"{def.Name}@{def.Id}" : "no defintion found";

            var name = expr.Name;
            if (name == "#at")
            {
                name = "AtOperator";
            }

            var exprValue = $"{name}({args})";

            if (name.StartsWith("#operator"))
            {
                // TODO: this should go to functions so that we can have operator overloading. 

                switch (expr.Syntax)
                {
                    case ConditionalExpressionSyntax conditionalExpressionSyntax:
                        if (childNames.Count != 3) throw new Exception("Expected three children");
                        exprValue = $"{childNames[0]} ? {childNames[1]} : {childNames[2]}";
                        break;
                    case BinaryExpressionSyntax binaryExpressionSyntax:
                        if (childNames.Count != 2) throw new Exception("Expected two children");
                        exprValue = $"{childNames[0]} {binaryExpressionSyntax.OperatorToken} {childNames[1]}";
                        break;
                    case PostfixUnaryExpressionSyntax postfixUnaryExpressionSyntax:
                        if (childNames.Count != 1) throw new Exception("Expected one child");
                        exprValue = $"{childNames[0]}{postfixUnaryExpressionSyntax.OperatorToken}";
                        break;
                    case PrefixUnaryExpressionSyntax prefixUnaryExpressionSyntax:
                        if (childNames.Count != 1) throw new Exception("Expected one child");
                        exprValue = $"{prefixUnaryExpressionSyntax.OperatorToken}{childNames[0]}";
                        break;
                    default:
                        throw new Exception($"Unrecognized operator {name} : {expr.SyntaxKind}");
                }
            }
            else if (name.StartsWith("#invoke"))
            {
                // TODO: an interesting problem is that when a method is an extension method, we need to explicitly call the "this"
                // I can make a trampoline function maybe? I think no matter what I need to know when I am accessing a method group,
                // and if that method group is known at compile-time I am going to do some things differently. 
                // This touches on one of the problems that I also want to tackle. This system can learn new things about values. 
                // In other-words, I can't just rely on Roslyn to tell me things... if a value can be determined by this tool, it 
                // has to be kept. This is what the result of inlining gives us, and is a key observation of the system. 

                // TODO: this means that I have to come up with a decent intermediate representation that will facilitate the work required. 
                // Good news, is that what this system is doing looks a bit like the system that will create the intermediate representation. 

                var invokeArgList = string.Join(", ", childNames.Skip(1));
                exprValue = $"{childNames[0]}({invokeArgList})";
            }
            else if (name.StartsWith("#lambda"))
            {
                var f = (FunctionDefinition)expr;
                var parameters = string.Join(", ", f.Parameters.Select(p => p.Name));
                sw.WriteLine($"var {exprVarName} = ({parameters}) => ");
                sw.WriteLine("{");
                OutputStatement(f.Body, "  ");
                sw.WriteLine("}");
                
                // NOTE: this returns early, unlike other forms. This is because
                return exprVarName;
            }
            else if (name.StartsWith("#with"))
            {
                // TODO: forbid
                //Debug.Assert(false);
            }
            else if (name.StartsWith("#declaration"))
            {
                // TODO: what to do? 
                //Debug.Assert(false);
            }
            else if (name.StartsWith("#"))
            {
                Debug.WriteLine($"Unrecognized type {name}");
                throw new Exception("Unrecognized special syntax");
            }

            if (declareVar)
            {
                sw.WriteLine($"    var {exprVarName} = {exprValue}; // {dsKind} {defText}");
            }

            return exprVarName;
        }

        public void OutputStatement(PlatoStatementSyntax st, SemanticModel model, string indent = "")
        {
            if (st == null)
                return;

            sw.WriteLine($"{indent}{st.Node.GetType().Name}@{st.Id}");

            //var statementSymbol = model.GetSymbolInfo(st.Node).Symbol;
            //sw.WriteLine($"Has statement symbol {statementSymbol}");

            if (st.ChildExpressions.Count > 0)
            {
            }

            if (st.ChildStatements.Count > 0)
            {
                sw.WriteLine(indent + "  Statements:");
                foreach (var c in st.ChildStatements)
                {
                    OutputStatement(c, model, indent + "     ");
                }
            }
        }

        public void OutputStatement(Statement statement, string indent = "")
        {
            if (statement == null)
                return;
            switch (statement)
            {
                case BlockStatement blockStatementIr:
                    sw.WriteLine($"{indent}{{");
                    foreach (var s in blockStatementIr.Statements)
                        OutputStatement(s, indent + "  ");
                    sw.WriteLine($"{indent}}}");
                    break;

                case ExpressionStatement expressionStatementIr:
                    sw.WriteLine($"{indent}{ReduceExpression(expressionStatementIr.Expression)};");
                    break;

                case IfStatement ifStatementIr:
                    sw.WriteLine($"{indent}if ({ReduceExpression(ifStatementIr.Condition)})");
                    OutputStatement(ifStatementIr.OnTrue, indent + "  ");
                    
                    if (ifStatementIr.OnFalse != null && !(ifStatementIr.OnFalse is EmptyStatement))
                    {
                        sw.WriteLine($"{indent}else");
                        OutputStatement(ifStatementIr.OnFalse);
                    }
                    break;

                case ReturnStatement returnStatementIr:
                    if (returnStatementIr.Expression == null)
                        sw.WriteLine($"{indent}return;");
                    else
                        sw.WriteLine($"{indent}return {ReduceExpression(returnStatementIr.Expression)};");
                    break;

                case ThrowStatement throwStatementIr:
                    if (throwStatementIr.Expression != null)
                        sw.WriteLine($"{indent}throw {ReduceExpression(throwStatementIr.Expression)}");
                    else
                        sw.WriteLine($"{indent}throw;");
                    break;

                case WhileStatement whileStatementIr:
                    OutputStatement(whileStatementIr.Initialization, indent);
                    var condName = ReduceExpression(whileStatementIr.Condition);
                    sw.WriteLine($"{indent}while ({condName})");
                    sw.WriteLine($"{indent}{{");
                    OutputStatement(whileStatementIr.Body, indent + "  ");
                    OutputStatement(whileStatementIr.Increment, indent + "  ");

                    // This triggers a regeneration of the statements to create the expression
                    sw.WriteLine($"{condName} = {ReduceExpression(whileStatementIr.Condition, false)};");
                    sw.WriteLine($"{indent}}}");
                    break;

                case EmptyStatement _:
                    break;

                case MultiStatement multiStatement:
                    foreach (var st in multiStatement.Statements)
                        OutputStatement(st, indent);
                    break;

                case UnsupportedStatement unsupportedStatementIr:
                    throw new NotSupportedException($"{unsupportedStatementIr.Syntax.Kind()}");

                default:
                    throw new ArgumentOutOfRangeException(nameof(statement));
            }
        }

        public void OutputFunction(FunctionDefinition func, string indent = "")
        {
            var paramList = string.Join(", ", func.Parameters.Select(p => $"{p.Name}_{p.Id} /* :{p.Type} */"));
            var staticKeyword = func.IsStatic ? "static " : "";
            sw.WriteLine($"/* ORIGINAL: ");
            sw.WriteLine(func.Syntax.ToString());
            sw.WriteLine($"*/");
            sw.WriteLine($"{indent}{staticKeyword}{func.Name}_{func.Id}({paramList}) // :{func.Result?.Type}");
            sw.WriteLine($"{indent}{{");
            OutputStatement(func.Body, indent + "  ");
            sw.WriteLine($"{indent}}}");
        }

        public void OutputType(PlatoTypeSyntax t, string indent = "")
        {
            sw.WriteLine($"class {t.Name} // {t.Kind}");
            sw.WriteLine("{");
            indent += "  ";
            foreach (var f in t.Fields)
            {
                var staticKeyword = f.IsStatic ? "static " : "";
                sw.WriteLine($"{indent}{staticKeyword}{f.Name};");
            }

            foreach (var p in t.Properties)
            {
                var staticKeyword = p.IsStatic ? "static " : "";
                sw.WriteLine($"{indent}{staticKeyword}get {p.Name}()");

                var pdef = SyntaxToProperties[p.Node];

                if (pdef != null)
                {
                    sw.WriteLine($"{indent}{{");
                    OutputStatement(pdef.Getter.Body, indent + "  ");
                    sw.WriteLine($"{indent}}}");
                }
                else
                {
                    sw.WriteLine($"{indent}; // ERROR: couldn't find property definition ");
                }
            }

            // A global namespace version of each function and one just on the type? 
            foreach (var m in t.Methods)
            {
                var func = SyntaxToFunctions[m.Node];
                OutputFunction(func, "  ");
            }

            sw.WriteLine("}");
        }

        public void CreateLookups(GeneratorExecutionContext context, IReadOnlyList<PlatoTypeSyntax> types)
        {
            foreach (var t in types)
            {
                var model = context.Compilation.GetSemanticModel(t.Node.SyntaxTree);
                foreach (var m in t.Methods)
                {
                    var func = FunctionDefinition.Create(m.Node, model);
                    SyntaxToFunctions.Add(m.Node, func);
                }

                foreach (var p in t.Properties)
                {
                    // TODO: The problem here is a property is complicated. It can have multiple functions associated with it. 
                    // A getter/init/setter
                    // So the solution is to make these things 
                    var prop = PropertyDefinition.Create(p.Node, model);
                    SyntaxToProperties.Add(p.Node, prop);
                }
            }
        }

        public void ToJavaScript(GeneratorExecutionContext context)
        {
            var types = context.Compilation.SyntaxTrees.GetPlatoTypes();

            CreateLookups(context, types);
           
            using (sw = new StreamWriter(File.Create(LogFileName)))
            {
                foreach (var t in types)
                {
                    OutputType(t);
                }
            }
        }
    }
}
