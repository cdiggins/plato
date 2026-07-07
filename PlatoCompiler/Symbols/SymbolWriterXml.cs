namespace Ara3D.Geometry.Compiler.Symbols
{
    //    public class SymbolXmlWriter : CodeBuilder<SymbolXmlWriter>
    //    {
    //        public SymbolXmlWriter WriteXml(string tag, string text)
    //        {
    //            return WriteLine($"<{tag}>{text}</{tag}>");
    //        }

    //        public SymbolXmlWriter WriteXml(string tag, Func<SymbolXmlWriter, SymbolXmlWriter> outputFunc)
    //        {
    //            return outputFunc(
    //                WriteLine($"<{tag}>").Indent()).Dedent().WriteLine($"</{tag}>");
    //        }

    //        public SymbolXmlWriter WriteXml(string tag, Symbol value)
    //        {
    //            return WriteXml(tag, self => self.Write(value));
    //        }

    //        public SymbolXmlWriter WriteXml(string tag, IEnumerable<Symbol> values)
    //        {
    //            return WriteXml(tag, avw => avw.Write(values));
    //        }

    //        public SymbolXmlWriter Write(IEnumerable<Symbol> values)
    //        {
    //            return values.Aggregate(this, (self, value) => self.Write(value));
    //        }

    //        public SymbolXmlWriter Write(Symbol value)
    //        {
    //            switch (value)
    //            {
    //                case null:
    //                    return WriteXml("null", "");

    //                case Argument argument:
    //                    return WriteXml("ArgumentSymbol", avw => avw
    //                        .WriteXml("Position", argument.Position.ToString())
    //                        .WriteXml("Original", argument.Expression));

    //                case Assignment assignment:
    //                    return WriteXml("AssignmentSymbol", avw => avw
    //                        .WriteXml("LValue", assignment.LValue)
    //                        .WriteXml("RValue", assignment.RValue));

    //                case ConditionalExpressionSymbol conditional:
    //                    return WriteXml("ConditionalExpressionSymbol", avw => avw
    //                        .WriteXml("True", conditional.IfTrue)
    //                        .WriteXml("False", conditional.IfFalse)
    //                        .WriteXml("Condition", conditional.Condition));

    //                case FieldDefinition fieldDef:
    //                    return WriteXml("Field", avw => avw
    //                        .WriteXml("Name", fieldDef.Name)
    //                        .WriteXml("Type", fieldDef.Type));

    //                case FunctionDefinition function:
    //                    return WriteXml("Function", avw => avw.WriteXml("Parameters",
    //                        function.Parameters).Write(function.Type));

    //                case FunctionCall functionResult:
    //                    return WriteXml("Result", avw => avw
    //                        .Write(functionResult.Function)
    //                        .WriteXml("Arguments", functionResult.Args));

    //                case Literal literal:
    //                    return WriteXml("LiteralSymbol", literal.Value.ToString());

    //                case MethodDefinition methodDef:
    //                    return WriteXml("Method", methodDef.Function);

    //                case TypeParameterDefinition typeParameterDef:
    //                    return WriteXml("TypeParameter", typeParameterDef.Name);

    //                case MemberDefinition member:
    //                    throw new Exception("Not implemented");

    //                case ParameterDefinition parameter:
    //                    return WriteXml("ParameterSymbol", avw =>
    //                        avw.WriteXml("Name", parameter.Name).Write(parameter.Type));

    //                case TypeDefinition typeDef:
    //                    return WriteXml("TypeDefSymbol", avw => avw
    //                        .WriteXml("Methods", typeDef.Methods)
    //                        .WriteXml("Fields", typeDef.Fields)
    //                        .WriteXml("Parameters", typeDef.TypeParameters)
    //                        .WriteXml("Name", typeDef.Name)
    //                        .WriteXml("Kind", typeDef.Kind.ToString()));

    //                case TypeExpression typeRef:
    //                    return WriteXml("Type", avw => 
    //                        avw.WriteXml("Name", typeRef.Name)
    //                        .Write(typeRef.TypeArgs));

    //                case VariableDefinition variable:
    //                    return WriteXml("VariableSymbol", avw => avw
    //                        .WriteXml("Name", variable.Name)
    //                        .Write(variable.Type));

    //                default:
    //                    throw new ArgumentOutOfRangeException(nameof(value));
    //            }

    //            return this;
    //        }
    //    }

    //    public static class SymbolXmlWriterExtensions
    //    {
    //        public static string ToXml(this Symbol value)
    //        {
    //            var avw = new SymbolXmlWriter();
    //            avw.Write(value);
    //            return avw.ToString();
    //        }

    //        public static string ToXml(this IEnumerable<Symbol> values)
    //        {
    //            var avw = new SymbolXmlWriter();
    //            avw.Write(values);
    //            return avw.ToString();
    //        }
    //    }
}