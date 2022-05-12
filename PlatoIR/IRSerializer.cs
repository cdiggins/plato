using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlatoIR
{
    public class IRSerializer
    {
        public bool ShowResolution = false;

        /// <summary>
        /// Initial tests show a small amount of improvement: a bit more than 10%
        /// </summary>
        public bool InliningAttribute = false;

        /// <summary>
        /// Initial tests showed no improvement (at least when combined with inlining attribute, which makes sense)
        /// Also not fully implemented.
        /// Can't reassign "in" parameter.
        /// Can't use "in" parameters in a lambda
        /// </summary>
        public bool UseInParameters = false;

        /// <summary>
        /// Whether to use implicit local variable type declarations.
        /// </summary>
        public bool UseImplicitVars = false;

        public IRSerializer(StreamWriter writer)
            => Writer = writer;

        public StreamWriter Writer { get; }

        public IRSerializer Write(string s)
        {
            Writer.Write(s);
            return this;
        }

        public IRSerializer Write(IEnumerable<IR> irs, string indent, string separator)
        {
            var r = this;
            var first = true;
            foreach (var ir in irs)
            {
                if (first) first = false;
                else Write(separator);
                r = r.Write(ir, indent);
            }

            return r;
        }

        public IRSerializer WriteLines(IEnumerable<IR> irs, string indent)
            => irs.Aggregate(this, (a, b) => a.Write(b, indent).WriteLine(indent));

        public IRSerializer WriteLine(string text, string indent)
            => Write(text).WriteLine(indent);

        public IRSerializer WriteLine(string indent)
        {
            Writer.WriteLine();
            return Write(indent);
        }

        public IRSerializer WriteParenthesizedList(IEnumerable<IR> argList, string indent)
            => Write("(").Write(argList, indent, ", ").Write(")");

        public IRSerializer WriteBracedList(IEnumerable<IR> argList, string indent)
            => Write("{").Write(argList, indent, ", ").Write("}");

        public IRSerializer WriteBracketedList(IEnumerable<IR> argList, string indent)
            => Write("[").Write(argList, indent, ", ").Write("]");

        public IRSerializer WriteOptionalAngledList(IEnumerable<IR> argList, string indent)
            => argList == null || !argList.Any() ? this : Write("<").Write(argList, indent, ", ").Write(">");

        public IRSerializer WriteOptionalInitializer(IR value, string indent)
            => value == null ? this : Write(" = ").Write(value, indent);

        public IRSerializer WriteTypeArgsOrParameters(IEnumerable<IR> typeParams, string indent)
            => WriteOptionalAngledList(typeParams, indent);

        public IRSerializer WriteBaseClassAndInterfaces(IReadOnlyList<TypeReferenceIR> baseList, string indent)
        {
            var r = this;
            for (var i = 0; i < baseList.Count; ++i)
            {
                r = r.Write(i == 0 ? " : " : ", ");
                r = r.Write(baseList[i], indent);
            }
            return r;
        }

        public IRSerializer WriteFunctionNameAndType(MethodDeclarationIR functionDeclarationIr, string indent)
            => functionDeclarationIr.Name == "implicit operator" ||
                functionDeclarationIr.Name == "explicit operator"
            ? Write(functionDeclarationIr.Name).Write(" ").Write(functionDeclarationIr.Type, indent)
            : Write(functionDeclarationIr.Type, indent).Write(" ").Write(functionDeclarationIr.Name);

        
        public IRSerializer WriteFunction(MethodDeclarationIR functionDeclarationIr, string indent)
            => 
                (InliningAttribute ? Write("[MethodImpl(MethodImplOptions.AggressiveInlining)]") : this)
                .Write("public ").Write(functionDeclarationIr.IsStatic ? "static " : "")
                .WriteFunctionNameAndType(functionDeclarationIr, indent)
                .WriteTypeArgsOrParameters(functionDeclarationIr.TypeParameters, indent)
                .WriteParenthesizedList(functionDeclarationIr.Parameters, indent)
                .WriteLine(indent)
                .Write(functionDeclarationIr.Body, indent);

        public IRSerializer WriteDeclaration(DeclarationIR ir)
            => ir == null ? Write("/* unresolved */") : ShowResolution ? Write($"/* {ir.Name}@{ir.Id} */") : this;

        public IRSerializer WriteMetaData(ExpressionIR expr)
            => expr is LambdaIR lambdaIr ? Write("/* Captured: ").Write(lambdaIr.CapturedVariables, "", ", ").Write("*/") : this;

        public IRSerializer WriteGetterBody(BlockStatementIR body, string indent)
            => body == null ? WriteLine("get;", indent) : WriteLine("get", indent + "  ").Write(body, indent + "  ");

        public IRSerializer WriteGetter(MethodDeclarationIR method, string indent)
            => method == null ? this
                : WriteLine(indent).WriteLine("{", indent).WriteGetterBody(method.Body, indent + "  ").WriteLine("}", indent);

        public IRSerializer WriteExpressionAsStatement(ExpressionIR ir, string indent)
        {
            var r = this;
            if (ir is TupleIR tupleIr)
            {
                foreach (var arg in tupleIr.Args)
                {
                    r = r.WriteExpressionAsStatement(arg, indent);
                }

                return r;
            }

            if (ir is LetIR letIr)
            {
                return WriteExpressionAsStatement(letIr.Expression, indent);
            }

            if (ir is ParenthesizedIR parenthesizedIr)
            {
                return WriteExpressionAsStatement(parenthesizedIr.Expression, indent);
            }

            return Write(ir, indent).WriteLine(";", indent);
        }

        public IRSerializer WriteReference(ReferenceIR referenceIr, string indent)
        {
            var r = this;

            if (referenceIr.Name == "Void")
                return Write("void");

            if (referenceIr.Receiver != null)
                r = r.Write(referenceIr.Receiver, indent).Write(".");

            switch (referenceIr)
            {
                case FieldReferenceIR fieldReferenceIr:
                    r = r.Write(fieldReferenceIr.Name);
                    break;
                
                case MethodReferenceIR methodReferenceIr:
                    r = r.Write(methodReferenceIr.Name);
                    break;
                
                case NamespaceReferenceIR namespaceReferenceIr:
                    r = r.Write(namespaceReferenceIr.Name);
                    break;
                
                case ParameterReferenceIR parameterReferenceIr:
                    r = r.Write(parameterReferenceIr.Name);
                    break;

                case PropertyReferenceIR propertyReferenceIr:
                    r = r.Write(propertyReferenceIr.PropertyDeclaration?.Field?.Name ?? propertyReferenceIr.Name);
                    break;

                case TypeReferenceIR typeReferenceIr:
                    // NOTE: fast exit from function
                    if (typeReferenceIr.Name == "[]")
                    {
                        // TODO: check that there is exaclyt one type argument
                        if (typeReferenceIr.TypeArguments.Count != 1)
                            throw new Exception("arrays should have exactly one type argument");
                        return r.Write(typeReferenceIr.TypeArguments[0], indent).Write("[]");
                    }
                    r = r.Write(typeReferenceIr.Name);
                    break;

                case UnknownReferenceIR unknownReferenceIr:
                    r = r.Write(unknownReferenceIr.Name);
                    break;

                case VariableReferenceIR variableReferenceIr:
                    r = r.Write(variableReferenceIr.Name);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(referenceIr));
            }

            return r.WriteTypeArgsOrParameters(referenceIr.TypeArguments, indent);
        }

        public IRSerializer WriteInParameter(ParameterDeclarationIR parameter)
        {
            if (!UseInParameters) return this;
            var typeDecl = parameter?.Type?.TypeDeclaration;
            if (typeDecl == null) return this;
            if (typeDecl.IsValueType) return Write("in ");
            return this;
        }

        public IRSerializer Write(IR ir, string indent)
        {
            if (ir == null) return this;

            if (!string.IsNullOrEmpty(ir.Source))
            {
                WriteLine("/*", indent);
                WriteLine(ir.Source.Replace("*/", " "), indent);
                WriteLine("*/", indent);
            }

            // Recurse through all declarations in all expressions in a single statement
            if (ir is StatementIR statement)
            {
                foreach (var d in statement.GetAllExpressionDeclarations())
                {
                    WriteLine("// Let declaration", indent);
                    Write(d, indent).WriteLine(";", indent);
                }
            }

            if (ir is OperationIR operation)
            {
                //Write("/*").Write(operation.Function?.Declaration?.Name ?? "unresolved").Write("*/");
            }

            switch (ir)
            {
                case ReferenceIR referenceIr:
                    return WriteReference(referenceIr, indent);

                case ArrayIR arrayIr:
                {
                    var r = Write("new ").Write(arrayIr.ExpressionType.TypeArguments[0], indent).Write("[")
                        .Write(arrayIr.Size,indent).Write("]");
                    if (arrayIr.Args?.Count != 0) r = r.WriteBracedList(arrayIr.Args, indent);
                    return r;
                }

                case AssignmentIR assignmentIr:
                    return Write(assignmentIr.LValue, indent).Write(" = ").Write(assignmentIr.RValue, indent);

                case BaseIR baseIr:
                    return Write("base");

                case BinaryOperatorIR binaryOperatorIr:
                    return Write(binaryOperatorIr.Operand1, indent)
                        .Write(binaryOperatorIr.Operator)
                        .Write(binaryOperatorIr.Operand2, indent);

                case BlockStatementIR blockStatementIr:
                    return WriteLine("{", indent + "  ")
                        .Write(blockStatementIr.Statements, indent + "  ", "")
                        .WriteLine("}", indent);
                     
                case CastIR castIr:
                    return Write("(")
                        .Write(castIr.CastType, indent).Write(")").Write(castIr.Args[0], indent);

                case ConditionalIR conditionalIr:
                    return Write(conditionalIr.Args[0], indent).Write(" ? ").Write(conditionalIr.Args[1], indent).Write(" : ")
                        .Write(conditionalIr.Args[2], indent);

                case ConstructorDeclarationIr constructorIr:
                    return Write("public ").Write(constructorIr.IsStatic ? "static " : "").Write(constructorIr.Name).WriteParenthesizedList(constructorIr.Parameters, indent).WriteDeclaration(constructorIr)
                        .WriteLine(indent).Write(constructorIr.Body, indent + "  ");

                case DeclarationStatementIR declarationStatementIr:
                    return Write(declarationStatementIr.Declaration, indent).WriteDeclaration(declarationStatementIr.Declaration).WriteLine(";", indent);

                case DefaultIR defaultIr:
                    return defaultIr.DefaultType != null
                        ? Write("default").Write("(").Write(defaultIr.DefaultType, indent).Write(")")
                        : Write("default");

                case DiscardIR discardIr:
                    return Write("_");

                case DoStatementIR doStatementIr:
                    return Write("do").Write("(").Write(doStatementIr.Condition, indent).WriteLine(")", indent)
                        .Write(doStatementIr.Body, indent + "  ");

                case ExpressionStatementIR expressionStatementIr:
                    return WriteExpressionAsStatement(expressionStatementIr.Expression, indent);

                case FieldDeclarationIR fieldIr:
                    return Write("public ").Write("readonly ").Write(fieldIr.IsStatic ? "static " : "").Write(fieldIr.Type, indent).Write(" ").Write(fieldIr.Name).Write(" ")
                        .WriteOptionalInitializer(fieldIr.InitialValue, indent).WriteDeclaration(fieldIr).WriteLine(";", indent);

                case IfStatementIR ifStatementIr:
                    return Write("if").Write("(").Write(ifStatementIr.Condition, indent).WriteLine(")", indent)
                        .Write(ifStatementIr.OnTrue, indent + "  ").WriteLine("else", indent).Write(ifStatementIr.OnFalse, indent + "  ");

                case IndexerDeclarationIR indexerIr:
                    return Write("public ").Write(indexerIr.IsStatic ? "static " : "")
                        .Write(indexerIr.Type, indent).Write(" this ").WriteBracketedList(
                        indexerIr.Getter.Parameters, indent).WriteLine(indent).WriteGetter(indexerIr.Getter, indent);

                case LambdaIR lambdaIr:
                    return WriteMetaData(lambdaIr)
                        .WriteParenthesizedList(lambdaIr.Parameters, indent).WriteLine(indent).Write(" => ")
                        .Write(lambdaIr.Body, indent + "  ");

                case LetIR letIR:
                    return Write(letIR.Expression, indent);

                case LiteralIR literalIr:
                    return Write(literalIr.Text);

                case MultiStatementIR multiStatementIr:
                    return multiStatementIr.Statements.Aggregate(this, (a, st) => a.Write(st, indent));

                case NewIR newIr:
                    return Write("new ").Write(newIr.CreatedType, indent).WriteParenthesizedList(newIr.Args, indent);

                case OperationDeclarationIR operationIr:
                    return WriteFunction(operationIr, indent);

                case ParameterDeclarationIR parameterIr:
                    return Write(parameterIr.IsThisParameter ? "this " : "")
                        .WriteInParameter(parameterIr)
                        .Write(parameterIr.Type, indent)
                        .Write(" ").Write(parameterIr.Name).WriteOptionalInitializer(parameterIr.DefaultValue, indent).WriteDeclaration(parameterIr);

                case ParenthesizedIR parenthesizedIr:
                    return Write("(").Write(parenthesizedIr.Args[0], indent).Write(")");

                case PostfixOperatorIR postfixOperatorIr:
                    return Write(postfixOperatorIr.Args[0], indent).Write(postfixOperatorIr.Operator);

                case PrefixOperatorIR prefixOperatorIr:
                    return Write(prefixOperatorIr.Operator).Write(prefixOperatorIr.Args[0], indent);

                case PropertyDeclarationIR propertyIr:
                    return Write("public ").Write(propertyIr.IsStatic ? "static " : "").Write(propertyIr.Type, indent).Write(" ").Write(propertyIr.Name)
                        .WriteGetter(propertyIr.Getter, indent)
                        .WriteLine(propertyIr.Getter == null ? ";" : "", indent);

                case ReturnStatementIR returnStatementIr:
                    return Write("return ").Write(returnStatementIr.Expression, indent).WriteLine(";", indent);

                case SubscriptIR subscriptIr:
                    return Write(subscriptIr.Args[0], indent).WriteBracketedList(subscriptIr.Args.Skip(1), indent);

                case SwitchIR switchIr:
                {
                    var r = Write(switchIr.Args[0], indent).WriteLine(" switch ", indent)
                        .WriteLine("{", indent + "  ");

                    for (var i = 1; i < switchIr.Args.Count; i += 2)
                    {
                        r = r.Write(switchIr.Args[i], indent + "  ").Write(" => ")
                            .Write(switchIr.Args[i + 1], indent + "  ").WriteLine(",", indent + "  ");
                    }

                    return r.WriteLine("}", indent);
                }

                case ThisIR thisIr:
                    return Write("this");

                case ThrowIR throwIr:
                    return Write("throw ").Write(throwIr.Args[0], indent);

                case TupleIR tupleIr:
                    return WriteParenthesizedList(tupleIr.Args, indent);

                case TypeDeclarationIR typeDeclarationIr:
                    return
                        WriteLine("//==begin==//", indent)
                        .Write("public ")
                        .Write(typeDeclarationIr.IsStatic ? "static " : "")
                        .Write(typeDeclarationIr.Kind)
                        .Write(" ").Write(typeDeclarationIr.Name)
                        .WriteTypeArgsOrParameters(typeDeclarationIr.TypeParameters, indent)
                        .WriteBaseClassAndInterfaces(typeDeclarationIr.Bases, indent)
                        .WriteDeclaration(typeDeclarationIr)
                        .WriteLine(indent)
                        .WriteLine("{", indent + "  ")
                        .Write(typeDeclarationIr.Fields, indent + "  ", "")
                        .Write(typeDeclarationIr.Constructors, indent + "  ", "")
                        .Write(typeDeclarationIr.Properties, indent + "  ", "")
                        .Write(typeDeclarationIr.Methods, indent + "  ", "")
                        .Write(typeDeclarationIr.Indexers, indent + "  ", "")
                        .Write(typeDeclarationIr.Operations, indent + "  ", "")
                        .WriteLine("}", indent)
                        .WriteLine("//==end==//", indent);

                case TypeOfIR typeOfIr:
                    return Write("typeof").Write("(").Write(typeOfIr.Args[0], indent).Write(")");

                case TypeParameterDeclarationIR typeParameterDeclarationIr:
                    return Write(typeParameterDeclarationIr.Name).WriteDeclaration(typeParameterDeclarationIr);

                case VariableDeclarationIR variableDeclarationIr:
                    return (variableDeclarationIr.Type == null || UseImplicitVars ? Write("var ") : Write(variableDeclarationIr.Type, indent).Write(" ")).Write(variableDeclarationIr.Name)
                        .Write(" ").WriteOptionalInitializer(variableDeclarationIr.InitialValue, indent).WriteDeclaration(variableDeclarationIr);

                case WhileStatementIR whileStatementIr:
                    return Write("while").Write("(").Write(whileStatementIr.Condition, indent).WriteLine(")", indent)
                        .Write(whileStatementIr.Body, indent + "  ");

                case MethodDeclarationIR methodIr:
                    return WriteFunction(methodIr, indent);

                case InvocationIR invocationIr:
                    return Write(invocationIr.Function, indent).WriteParenthesizedList(invocationIr.Args, indent);

                //case OperationIR operatorIr:
                case StatementIR statementIr:
                case DeclarationIR declarationIr:
                case ExpressionIR expressionIr:
                default:
                    throw new ArgumentOutOfRangeException(nameof(ir));
            }
        }
    }
}
