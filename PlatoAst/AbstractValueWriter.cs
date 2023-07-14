using System;
using System.Collections.Generic;
using System.Linq;
using Parakeet;

namespace PlatoAst
{
    public class AbstractValueWriter : CodeBuilder<AbstractValueWriter>
    {
        public AbstractValueWriter WriteXml(string tag, string text)
        {
            return WriteLine($"<{tag}>{text}</{tag}>");
        }

        public AbstractValueWriter WriteXml(string tag, Func<AbstractValueWriter, AbstractValueWriter> outputFunc)
        {
            return outputFunc(
                WriteLine($"<{tag}>").Indent()).Dedent().WriteLine($"</{tag}>");
        }

        public AbstractValueWriter WriteXml(string tag, AbstractValue value)
        {
            return WriteXml(tag, self => self.Write(value));
        }

        public AbstractValueWriter WriteXml(string tag, IEnumerable<AbstractValue> values)
        {
            return WriteXml(tag, avw => avw.Write(values));
        }

        public AbstractValueWriter Write(IEnumerable<AbstractValue> values)
        {
            return values.Aggregate(this, (self, value) => self.Write(value));
        }

        public AbstractValueWriter Write(AbstractValue value)
        {
            switch (value)
            {
                case null:
                    return WriteXml("null", "");

                case Argument argument:
                    return WriteXml("Argument", avw => avw
                        .WriteXml("Position", argument.Position.ToString())
                        .WriteXml("Original", argument.Original));

                case Assignment assignment:
                    return WriteXml("Assignment", avw => avw
                        .WriteXml("LValue", assignment.LValue)
                        .WriteXml("RValue", assignment.RValue));
                
                case Conditional conditional:
                    return WriteXml("Conditional", avw => avw
                        .WriteXml("True", conditional.IfTrue)
                        .WriteXml("False", conditional.IfFalse)
                        .WriteXml("Condition", conditional.Condition));

                case FieldDef fieldDef:
                    return WriteXml("Field", avw => avw
                        .WriteXml("Name", fieldDef.Name)
                        .WriteXml("Type", fieldDef.Type));
                    
                case Function function:
                    return WriteXml("Function", avw => avw.WriteXml("Parameters",
                        function.Parameters).Write(function.Type));

                case FunctionResult functionResult:
                    return WriteXml("Result", avw => avw
                        .Write(functionResult.Function)
                        .WriteXml("Arguments", functionResult.Args));

                case Intrinsic intrinsic:
                    return WriteXml("Intrinsic", intrinsic.Name);

                case Literal literal:
                    return WriteXml("Literal", literal.Value.ToString());

                case MethodDef methodDef:
                    return WriteXml("Method", methodDef.Function);
                
                case TypeParameterDef typeParameterDef:
                    return WriteXml("TypeParameter", typeParameterDef.Name);

                case Member member:
                    throw new Exception("Not implemented");
                
                case MemberRef memberRef:
                    return WriteXml("MemberRef", avw =>
                        avw.WriteXml("Receiver", memberRef.Receiver)
                            .WriteXml("Name", memberRef.Name));
                
                case NoValue noValue:
                    return WriteXml("NoValue", "");

                case Parameter parameter:
                    return WriteXml("Parameter", avw =>
                        avw.WriteXml("Name", parameter.Name).Write(parameter.Type));

                case Region region:
                    return WriteXml("Region", region.Children);

                case TypeDef typeDef:
                    return WriteXml("TypeDef", avw => avw
                        .WriteXml("Methods", typeDef.Methods)
                        .WriteXml("Fields", typeDef.Fields)
                        .WriteXml("Parameters", typeDef.TypeParameters)
                        .WriteXml("Name", typeDef.Name)
                        .WriteXml("Kind", typeDef.Kind));

                case TypeRef typeRef:
                    return WriteXml("Type", avw => 
                        avw.WriteXml("Name", typeRef.Name)
                        .Write(typeRef.TypeArgs));

                case Variable variable:
                    return WriteXml("Variable", avw => avw
                        .WriteXml("Name", variable.Name)
                        .Write(variable.Type));
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }

            return this;
        }
    }

    public static class AbstractValueExtensions
    {
        public static string ToXml(this AbstractValue value)
        {
            var avw = new AbstractValueWriter();
            avw.Write(value);
            return avw.ToString();
        }

        public static string ToXml(this IEnumerable<AbstractValue> values)
        {
            var avw = new AbstractValueWriter();
            avw.Write(values);
            return avw.ToString();
        }
    }
}