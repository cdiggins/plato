using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlatoIR
{
    /*
     * TODO: too many constructors
     * TODO: missing parameter types
     * TODO: names not becoming references
     * TODO: some classes missing names.
     * TODO: fix extra indent after first line
     * TODO: need more declaration information
     * TODO: types are missing the names
     * TODO: need to output type parameters for functions and classes
     * TODO: references are unresolved for fields (and probably more things)
     * TODO: need to have real ".ctor"
     * TODO: don't havea  line break in the class declaration
     *
     */
    public class IRSerializer
    {
        public IRSerializer(StreamWriter writer)
            => Writer = writer;

        public StreamWriter Writer { get; }

        public IRSerializer Write(string s)
        {
            Writer.Write(s);
            return this;
        }

        public IRSerializer Write(IEnumerable<IR> irs, string indent, string separator = "")
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

        public IRSerializer WriteParenthesizedList(IEnumerable<IR> argList, string indent = "")
            => Write("(").Write(argList, indent, ", ").Write(")");

        public IRSerializer WriteBracedList(IEnumerable<IR> argList, string indent = "")
            => Write("{").Write(argList, indent, ", ").Write("}");

        public IRSerializer WriteBracketedList(IEnumerable<IR> argList, string indent = "")
            => Write("[").Write(argList, indent, ", ").Write("]");

        public IRSerializer WriteOptionalInitializer(IR value, string indent)
            => value == null ? this : Write(" = ").Write(value, indent);

        public IRSerializer WriteSignature(FunctionDeclarationIR functionDeclarationIr, string indent)
            => Write("public ").Write(functionDeclarationIr.IsStatic ? "static " : "").Write(functionDeclarationIr.ReturnType, indent).Write(" ")
                .Write(functionDeclarationIr.Name).WriteParenthesizedList(functionDeclarationIr.Parameters).WriteLine(indent)
                .Write(functionDeclarationIr.Body, indent + "  ");

        public IRSerializer WriteDeclaration(DeclarationIR ir)
            => ir == null ? Write("/* unresolved */") : Write($"/* {ir.Name}@{ir.Id} */");

        public IRSerializer WriteReference(ReferenceIR ir)
            => WriteDeclaration(ir?.Declaration);

        public IRSerializer Write(IR ir, string indent)
        {
            if (ir == null) return this;

            switch (ir)
            {
                case ArrayIR arrayIr:
                    return Write("new ").Write(arrayIr.ExpressionType, indent).Write("[]").WriteBracedList(arrayIr.Args, indent);

                case AssignmentIR assignmentIr:
                    return Write(assignmentIr.LValue, indent).Write(" = ").Write(assignmentIr.RValue, indent);

                case BaseIR baseIr:
                    return Write("base");

                case BinaryOperatorIr binaryOperatorIr:
                    return Write(binaryOperatorIr.Operand1, indent).Write(binaryOperatorIr.Operator)
                        .Write(binaryOperatorIr.Operand2, indent);

                case BlockStatementIR blockStatementIr:
                    return WriteLine("{", indent).Write(blockStatementIr.Statements, indent + "  ")
                        .WriteLine("}", indent);

                case BuiltInTypeIR builtInTypeIr:
                    return Write("#").Write(nameof(builtInTypeIr));

                case CastIR castIr:
                    return Write("(").Write(castIr.CastType, indent).Write(")").Write(castIr.Args[0], indent);

                case ConditionalIR conditionalIr:
                    return Write(conditionalIr.Args[0], indent).Write(" ? ").Write(conditionalIr.Args[1], indent).Write(" : ")
                        .Write(conditionalIr.Args[2], indent);

                case ConstructorDeclarationIr constructorIr:
                    return Write("public ").Write(".ctor").WriteParenthesizedList(constructorIr.Parameters, indent)
                        .WriteLine(indent).Write(constructorIr.Body, indent + "  ");

                case ConverterDeclarationIr converterIr:
                    return Write("public ").Write("static ").Write("operator ").Write(converterIr.IsImplicit ? "implicit " : "explicit ").Write(converterIr.ReturnType, indent)
                        .WriteParenthesizedList(converterIr.Parameters).WriteLine(indent).Write(converterIr.Body, indent + "  ");

                case ArgumentIR argumentIr:
                    return Write(string.IsNullOrWhiteSpace(argumentIr.Name) ? "" : $"{argumentIr.Name}: ")
                        .Write(argumentIr.Value, indent);

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
                    return Write(expressionStatementIr.Expression, indent).WriteLine(";", indent);

                case FieldDeclarationIR fieldIr:
                    return Write(fieldIr.Type, indent).Write(" ").Write(fieldIr.Name).Write(" ")
                        .Write(fieldIr.InitialValue != null ? " = " : "").Write(fieldIr.InitialValue, indent).WriteLine(";", indent);

                case FieldReferenceIR fieldReferenceIr:
                    return Write(fieldReferenceIr.Name).WriteReference(fieldReferenceIr);

                case FunctionReferenceIR functionReferenceIr:
                    return Write(functionReferenceIr.Name).WriteReference(functionReferenceIr);

                case GenericFunctionIR genericFunctionIr:
                    return Write("#").Write(nameof(genericFunctionIr));

                case IfStatementIR ifStatementIr:
                    return Write("if").Write("(").Write(ifStatementIr.Condition, indent).WriteLine(")", indent)
                        .Write(ifStatementIr.OnTrue, indent + "  ").WriteLine("else", indent).Write(ifStatementIr.OnFalse, indent + "  ");

                case IndexerDeclarationIr indexerIr:
                    return Write(indexerIr.ReturnType, indent).Write(" ").Write("this").Write("[")
                        .Write(indexerIr.Getter.Parameters, indent, ", ").Write("]").WriteLine(indent)
                        .WriteLine("{", indent + "  ").Write("get")
                        .Write(indexerIr.Getter.Body, indent + "  " + "  ").WriteLine("}", indent);

                case KnownFunctionIR knownFunctionIr:
                    return Write("#").Write(nameof(knownFunctionIr));

                case KnownType knownType:
                    return Write("#").Write(nameof(knownType));

                case LambdaIR lambdaIr:
                    return Write("#").Write(nameof(lambdaIr));

                case LiteralIR literalIr:
                    return Write(literalIr.Text);

                case MethodReferenceIR methodReferenceIr:
                    return Write(methodReferenceIr.Name).WriteReference(methodReferenceIr);

                case MultiStatementIR multiStatementIr:
                    return multiStatementIr.Statements.Aggregate(this, (a, st) => a.Write(st, indent));

                case NameIR nameIr:
                    return Write(nameIr.Name).WriteDeclaration(nameIr.ReferencedIR);

                case NewIR newIr:
                    return Write("new ").Write(newIr.CreatedType, indent).WriteParenthesizedList(newIr.Args);

                case OperationDeclarationIr operationIr:
                    return Write("#").Write(nameof(operationIr));

                case ParameterDeclarationIR parameterIr:
                    return Write(parameterIr.Type, indent).Write(parameterIr.Name).WriteOptionalInitializer(parameterIr.DefaultValue, indent);

                case ParameterReferenceIR parameterReferenceIr:
                    return Write(parameterReferenceIr.Name).WriteReference(parameterReferenceIr);

                case ParenthesizedIR parenthesizedIr:
                    return Write("(").Write(parenthesizedIr.Args[0], indent).Write(")");

                case PostfixOperatorIr postfixOperatorIr:
                    return Write(postfixOperatorIr.Args[0], indent).Write(postfixOperatorIr.Operator);

                case PrefixOperatorIr prefixOperatorIr:
                    return Write(prefixOperatorIr.Operator).Write(prefixOperatorIr.Args[0], indent);

                case PropertyDeclarationIR propertyIr:
                    // TODO: add the getter. 
                    return Write(propertyIr.Type, indent).Write(" ").Write(propertyIr.Name).WriteLine(";", indent);

                case PropertyReferenceIR propertyReferenceIr:
                    return Write(propertyReferenceIr.Name).WriteReference(propertyReferenceIr);

                case ReturnStatementIR returnStatementIr:
                    return Write("return ").Write(returnStatementIr.Expression, indent).WriteLine(";", indent);

                case SubscriptIR subscriptIr:
                    return Write(subscriptIr.Args[0], indent).WriteBracketedList(subscriptIr.Args.Skip(1), indent);

                case SwitchIR switchIr:
                    return Write("#").Write(nameof(switchIr));

                case ThisIR thisIr:
                    return Write("this");

                case ThrowIR throwIr:
                    return Write("throw ").Write(throwIr.Args[0], indent);

                case TupleIR tupleIr:
                    return WriteParenthesizedList(tupleIr.Args, indent);

                case TypeDeclarationIR typeDeclarationIr:
                    return Write("public ").Write("class ").WriteLine(typeDeclarationIr.Name, indent).WriteDeclaration(typeDeclarationIr)
                        .WriteLine("{", indent + "  ")
                        .Write(typeDeclarationIr.Fields, indent + "  ")
                        .Write(typeDeclarationIr.Constructors, indent + "  ")
                        .Write(typeDeclarationIr.Properties, indent + "  ")
                        .Write(typeDeclarationIr.Methods, indent + "  ")
                        .Write(typeDeclarationIr.Converters, indent + "  ")
                        .Write(typeDeclarationIr.Operations, indent + "  ")
                        .WriteLine("}", indent);

                case TypeOfIR typeOfIr:
                    return Write("typeof").Write("(").Write(typeOfIr.Args[0], indent).Write(")");

                case TypeParameterDeclarationIR typeParameterDeclarationIr:
                    return Write(typeParameterDeclarationIr.Name);

                case TypeParameterReferenceIR typeParameterReferenceIr:
                    return Write(typeParameterReferenceIr.Name).WriteReference(typeParameterReferenceIr);

                case TypeReferenceIR typeReferenceIr:
                    return Write(typeReferenceIr.Name).WriteReference(typeReferenceIr);

                case VariableDeclarationIR variableDeclarationIr:
                    return Write(variableDeclarationIr.Type, indent).Write(" ").Write(variableDeclarationIr.Name)
                        .Write(" ").WriteOptionalInitializer(variableDeclarationIr.InitialValue, indent).WriteDeclaration(variableDeclarationIr);

                case VariableReferenceIR variableReferenceIr:
                    return Write(variableReferenceIr.Name).WriteReference(variableReferenceIr);

                case WhileStatementIR whileStatementIr:
                    return Write("while").Write("(").Write(whileStatementIr.Condition, indent).WriteLine(")", indent)
                        .Write(whileStatementIr.Body, indent + "  ");

                case MemberReferenceIR memberReferenceIr:
                    return Write(memberReferenceIr.Receiver, indent).Write(".").Write(memberReferenceIr.Name)
                        .WriteReference(memberReferenceIr);

                case MethodDeclarationIr methodIr:
                    return WriteSignature(methodIr, indent);

                case OperatorIR operatorIr:
                    return Write("#").Write(nameof(operatorIr));

                case StatementIR statementIr:
                    return Write("#").Write(nameof(statementIr));

                case FunctionDeclarationIR functionIr:
                    return WriteSignature(functionIr, indent);

                case InvocationIR invocationIr:
                    return Write(invocationIr.Function, indent).WriteParenthesizedList(invocationIr.Args, indent);

                case ReferenceIR referenceIr:
                    return Write("#").Write(nameof(referenceIr)).WriteReference(referenceIr);

                case DeclarationIR declarationIr:
                    return Write("#").Write(nameof(declarationIr));

                case ExpressionIR expressionIr:
                    return Write("#").Write(nameof(expressionIr));

                default:
                    throw new ArgumentOutOfRangeException(nameof(ir));
            }
        }
    }
}
