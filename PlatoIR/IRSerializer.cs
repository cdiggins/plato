using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlatoIR
{
    /*
     * DONE: too many constructors
     * DONE: missing parameter types
     * DONE: names not becoming references
     * DONE: some classes missing names.
     * DONE: fix extra indent after first line
     * DONE: need more declaration information
     * DONE: types are missing the names
     * DONE: need to output type parameters for functions and classes
     * DONE: references are unresolved for fields (and probably more things)
     * DONE: need to have the proper name for the ".ctor"     
     * DONE: don't have a line break in the class declaration
     * DONE: don't output the method and getter bodies for interfaces.
     * DONE: I am missing type arguments, particularly when working with built-in types
     * DONE: does Void actually work?
     * DONE: does Single work as well?
     * DONE: I need to make my lambdas work.
     * TODO: I would like the operators to be called explicitly
     * TODO: call the conversion operators explicitly.
     * TODO: design the testing program
     * TODO: namespace is missing
     * TODO: I want to have an explicit "this" when it can be added
     * TODO: I want to convert a whole file into C#, and then compile and test the result.
     * DONE: add the parameters (and variable declarations) to the references. 
     * DONE: some parameters aren't being added to the IRBuilder
     * TODO: renamed the "TYPE operator implicit" to "operator implicit TYPE"
     * DONE: output the property getter
     * DONE: output the indexer getter
     * DPME: generate fields when the property requires it.
     * DONE: don't have return statements in constructors.
     * DONE: replace the built-in type names with the correct key words
     * DONE: output the source code 
     * TODO: output the source location
     * DONE: remove the function reference (should only be type reference)
     * DONE: move the type arguments into a reference
     * DONE: add the list of interfaces to generated code
     * DONE: add the inherited class to code
     * TODO: generate JavaScript from the serializer
     * TODO: compile the generated code
     * TODO: report the results
     * TODO: why do "names" exist? I should really be using a refernce.
     * DONE: only allow block statements for method bodies.
     * DONE: type parameters of methods aren't being added as IR items.
     * TODO: output recievers.
     * DONE: resolve type parameters.
     * DONE: some types aren't showing. 
     * DONE: after merging TypeParameterReference with TypeParameter, some types aren't showing.
     * DONE: I now have some "#unknown" tags.
     * TODO: when a function is a parameter it is shown as "unresolved".
     * TODO: In "ToTriangles" I don't get the array declaration
     * TODO: the type of bilt-in arrays is invisible and shows as unresolved.
     * DONE: add public and static (where appropriate) to members.
     * DONE: indexers did not have the correct return type.
     * TODO: I want to start doing some inlining.
     * TODO: I want to try rewriting the code in a variable declaration form (like I'm doing in JavaScript).
     * TODO: I want to try evaluating the IR.
     * TODO: create a test project.
     * TODO: profiling of code nsippets
     * DONE: dont't put blocks for empty getters.
     * TODO: Fix the formatting, it looks awful. 
     * TODO: make the meta-data optional. 
     * DONE: interfaces should not have fields.
     * TODO: indexer body is missing in some cases
     * TODO: assigning to the auto-property needs to be allowed. 
     * TODO: all references to a property as a lvalue need to be replaced with the auto-property.
     * TODO: decide if I should change it on write, or on parse.
     * TODO: create a hidden constructor. 
     * TODO: normalization of the code. 
     * TODO: rewrite all tuples as lvalues. 
     * TODO: handle "this" function calls.
     * TODO: handle "base" function calls
     * TODO: add deconstructable classes (maybe the just need a "Item1" / "Item2" / etc.
     * TODO: the semantic rules should probably be in the constructors of the IR.
     * DONE: add let statement to IR
     * DONE: add compound expression to IR
     * DONE: tuples to expression statements become compound expressions
     * DONE: output let statements
     * TODO: make everything route through the IRBuilder.
     * DONE: get child expressions
     * TODO: remove all setters from ExpressionIR, meaning types is outside.
     * DONE: get all declarations from a statement
     * DONE: output var, when a type reference is not known.
     * TODO: customize output.https://docs.microsoft.com/en-us/cpp/build/formatting-the-output-of-a-custom-build-step-or-build-event?view=msvc-170
     * DONE: extra semicolons on the property
     * TODO: base class is a "var" 
     * TODO: assignment to properties, should be assignment to the field 
     */
    public class IRSerializer
    {
        public bool ShowResolution = false;

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

        public IRSerializer WriteTypeParameters(IEnumerable<IR> typeParams, string indent)
            => WriteOptionalAngledList(typeParams, indent);

        public IRSerializer WriteBaseClassAndInterfaces(TypeReferenceIR baseClass, List<TypeReferenceIR> interfaces, string indent)
        {
            var types = interfaces.Prepend(baseClass).ToList();
            var r = this;
            for (var i = 0; i < types.Count; ++i)
            {
                r = r.Write(i == 0 ? " : " : ", ");
                r = r.Write(types[i], indent);
            }
            return r;
        }

        public IRSerializer WriteFunction(MethodDeclarationIR functionDeclarationIr, string indent)
            => Write("public ").Write(functionDeclarationIr.IsStatic ? "static " : "")
                .Write(functionDeclarationIr.Type, indent).Write(" ")
                .Write(functionDeclarationIr.Name)
                .WriteTypeParameters(functionDeclarationIr.TypeParameters, indent)
                .WriteParenthesizedList(functionDeclarationIr.Parameters, indent)
                .WriteLine(indent)
                .Write(functionDeclarationIr.Body, indent);

        public IRSerializer WriteDeclaration(DeclarationIR ir)
            => ir == null ? Write("/* unresolved */") : ShowResolution ? Write($"/* {ir.Name}@{ir.Id} */") : this;

        public IRSerializer WriteReference(ReferenceIR ir)
            => WriteDeclaration(ir?.Declaration);

        public IRSerializer WriteMetaData(ExpressionIR expr)
            => expr is LambdaIR lambdaIr ? Write("/* Captured: ").Write(lambdaIr.CapturedVariables, ", ").Write("*/") : this;

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

        public IRSerializer Write(TypeReferenceIR ir, string indent)
            => ir == null ? Write("var") : Write(ir as IR, indent);

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
                foreach (var d in statement.AllExpressionDeclarations)
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
                case ArrayIR arrayIr:
                    return Write("new ").Write(arrayIr.ExpressionType, indent).Write("[]").WriteBracedList(arrayIr.Args, indent);

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
                        .Write(blockStatementIr.Statements, indent + "  ")
                        .WriteLine("}", indent);
                     
                case CastIR castIr:
                    return Write("(")
                        .Write(castIr.CastType, indent).Write(")").Write(castIr.Args[0], indent);

                case ConditionalIR conditionalIr:
                    return Write(conditionalIr.Args[0], indent).Write(" ? ").Write(conditionalIr.Args[1], indent).Write(" : ")
                        .Write(conditionalIr.Args[2], indent);

                case ConstructorDeclarationIr constructorIr:
                    return Write("public ").Write(constructorIr.IsStatic ? "static " : "").Write(constructorIr.Parent.Name).WriteParenthesizedList(constructorIr.Parameters, indent).WriteDeclaration(constructorIr)
                        .WriteLine(indent).Write(constructorIr.Body, indent + "  ");

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
                    return WriteExpressionAsStatement(expressionStatementIr.Expression, indent);

                case FieldDeclarationIR fieldIr:
                    return Write("public ").Write(fieldIr.IsStatic ? "static " : "").Write(fieldIr.Type, indent).Write(" ").Write(fieldIr.Name).Write(" ")
                        .WriteOptionalInitializer(fieldIr.InitialValue, indent).WriteDeclaration(fieldIr).WriteLine(";", indent);

                case FieldReferenceIR fieldReferenceIr:
                    return Write(fieldReferenceIr.Name).WriteReference(fieldReferenceIr);

                case IfStatementIR ifStatementIr:
                    return Write("if").Write("(").Write(ifStatementIr.Condition, indent).WriteLine(")", indent)
                        .Write(ifStatementIr.OnTrue, indent + "  ").WriteLine("else", indent).Write(ifStatementIr.OnFalse, indent + "  ");

                case IndexerDeclarationIr indexerIr:
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

                case MethodReferenceIR methodReferenceIr:
                    return Write(methodReferenceIr.Name).WriteReference(methodReferenceIr);

                case MultiStatementIR multiStatementIr:
                    return multiStatementIr.Statements.Aggregate(this, (a, st) => a.Write(st, indent));

                case NameIR nameIr:
                    return (nameIr.Reciever == null ? this : Write(nameIr.Reciever, indent).Write("."))
                        .Write(nameIr.Name).WriteDeclaration(nameIr.ReferencedIR);

                case NewIR newIr:
                    return Write("new ").Write(newIr.CreatedType, indent).WriteParenthesizedList(newIr.Args, indent);

                case OperationDeclarationIr operationIr:
                    return WriteFunction(operationIr, indent);

                case ParameterDeclarationIR parameterIr:
                    return Write(parameterIr.Type, indent).Write(" ").Write(parameterIr.Name).WriteOptionalInitializer(parameterIr.DefaultValue, indent).WriteDeclaration(parameterIr);

                case ParameterReferenceIR parameterReferenceIr:
                    return Write(parameterReferenceIr.Name).WriteReference(parameterReferenceIr);

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

                case PropertyReferenceIR propertyReferenceIr:
                    return Write(propertyReferenceIr.Name).WriteReference(propertyReferenceIr);

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
                        .Write(typeDeclarationIr.Kind).Write(" ").Write(typeDeclarationIr.Name)
                        .WriteTypeParameters(typeDeclarationIr.TypeParameters, indent)
                        .WriteBaseClassAndInterfaces(typeDeclarationIr.BaseClass, typeDeclarationIr.Interfaces, indent)
                        .WriteDeclaration(typeDeclarationIr)
                        .WriteLine(indent)
                        .WriteLine("{", indent + "  ")
                        .Write(typeDeclarationIr.Fields, indent + "  ")
                        .Write(typeDeclarationIr.Constructors, indent + "  ")
                        .Write(typeDeclarationIr.Properties, indent + "  ")
                        .Write(typeDeclarationIr.Methods, indent + "  ")
                        .Write(typeDeclarationIr.Indexers, indent + "  ")
                        .Write(typeDeclarationIr.Operations, indent + "  ")
                        .WriteLine("}", indent)
                        .WriteLine("//==end==//", indent);

                case TypeOfIR typeOfIr:
                    return Write("typeof").Write("(").Write(typeOfIr.Args[0], indent).Write(")");

                case TypeParameterDeclarationIR typeParameterDeclarationIr:
                    return Write(typeParameterDeclarationIr.Name).WriteDeclaration(typeParameterDeclarationIr);

                case TypeReferenceIR typeReferenceIr:
                    return Write(typeReferenceIr.Name).WriteOptionalAngledList(typeReferenceIr.TypeArguments, indent).WriteReference(typeReferenceIr);

                case VariableDeclarationIR variableDeclarationIr:
                    return Write(variableDeclarationIr.Type, indent).Write(" ").Write(variableDeclarationIr.Name)
                        .Write(" ").WriteOptionalInitializer(variableDeclarationIr.InitialValue, indent).WriteDeclaration(variableDeclarationIr);

                case VariableReferenceIR variableReferenceIr:
                    return Write(variableReferenceIr.Name).WriteReference(variableReferenceIr);

                case WhileStatementIR whileStatementIr:
                    return Write("while").Write("(").Write(whileStatementIr.Condition, indent).WriteLine(")", indent)
                        .Write(whileStatementIr.Body, indent + "  ");

                case MethodDeclarationIR methodIr:
                    return WriteFunction(methodIr, indent);

                case InvocationIR invocationIr:
                    return Write(invocationIr.Function, indent).WriteParenthesizedList(invocationIr.Args, indent);

                //case OperationIR operatorIr:
                case MemberReferenceIR memberReferenceIr:
                case StatementIR statementIr:
                case ReferenceIR referenceIr:
                case DeclarationIR declarationIr:
                case ExpressionIR expressionIr:
                default:
                    throw new ArgumentOutOfRangeException(nameof(ir));
            }
        }
    }
}
