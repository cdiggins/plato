using System;
using System.Collections.Generic;
using System.Linq;
using Parakeet;

namespace PlatoAst
{
    public class SymbolWriter : CodeBuilder<SymbolWriter>
    {
        public SymbolWriter WriteXml(string tag, string text)
        {
            return WriteLine($"<{tag}>{text}</{tag}>");
        }

        public SymbolWriter WriteXml(string tag, Func<SymbolWriter, SymbolWriter> outputFunc)
        {
            return outputFunc(
                WriteLine($"<{tag}>").Indent()).Dedent().WriteLine($"</{tag}>");
        }

        public SymbolWriter WriteXml(string tag, Symbol value)
        {
            return WriteXml(tag, self => self.Write(value));
        }

        public SymbolWriter WriteXml(string tag, IEnumerable<Symbol> values)
        {
            return WriteXml(tag, avw => avw.Write(values));
        }

        public SymbolWriter Write(IEnumerable<Symbol> values)
        {
            return values.Aggregate(this, (self, value) => self.Write(value));
        }

        public SymbolWriter Write(Symbol value)
        {
            switch (value)
            {
                case null:
                    return WriteXml("null", "");

                case ArgumentSymbol argument:
                    return WriteXml("ArgumentSymbol", avw => avw
                        .WriteXml("Position", argument.Position.ToString())
                        .WriteXml("Original", argument.Original));

                case AssignmentSymbol assignment:
                    return WriteXml("AssignmentSymbol", avw => avw
                        .WriteXml("LValue", assignment.LValue)
                        .WriteXml("RValue", assignment.RValue));
                
                case ConditionalSymbol conditional:
                    return WriteXml("ConditionalSymbol", avw => avw
                        .WriteXml("True", conditional.IfTrue)
                        .WriteXml("False", conditional.IfFalse)
                        .WriteXml("Condition", conditional.Condition));

                case FieldDefSymbol fieldDef:
                    return WriteXml("Field", avw => avw
                        .WriteXml("Name", fieldDef.Name)
                        .WriteXml("Type", fieldDef.Type));
                    
                case FunctionSymbol function:
                    return WriteXml("Function", avw => avw.WriteXml("Parameters",
                        function.Parameters).Write(function.Type));

                case FunctionResultSymbol functionResult:
                    return WriteXml("Result", avw => avw
                        .Write(functionResult.Function)
                        .WriteXml("Arguments", functionResult.Args));

                case IntrinsicSymbol intrinsic:
                    return WriteXml("IntrinsicSymbol", intrinsic.Name);

                case LiteralSymbol literal:
                    return WriteXml("LiteralSymbol", literal.Value.ToString());

                case MethodDefSymbol methodDef:
                    return WriteXml("Method", methodDef.Function);
                
                case TypeParameterDefSymbol typeParameterDef:
                    return WriteXml("TypeParameter", typeParameterDef.Name);

                case MemberDefSymbol member:
                    throw new Exception("Not implemented");
                
                case MemberRefSymbol memberRef:
                    return WriteXml("MemberRefSymbol", avw =>
                        avw.WriteXml("Receiver", memberRef.Receiver)
                            .WriteXml("Name", memberRef.Name));
                
                case NoValueSymbol noValue:
                    return WriteXml("NoValueSymbol", "");

                case ParameterSymbol parameter:
                    return WriteXml("ParameterSymbol", avw =>
                        avw.WriteXml("Name", parameter.Name).Write(parameter.Type));

                case RegionSymbol region:
                    return WriteXml("RegionSymbol", region.Children);

                case TypeDefSymbol typeDef:
                    return WriteXml("TypeDefSymbol", avw => avw
                        .WriteXml("Methods", typeDef.Methods)
                        .WriteXml("Fields", typeDef.Fields)
                        .WriteXml("Parameters", typeDef.TypeParameters)
                        .WriteXml("Name", typeDef.Name)
                        .WriteXml("Kind", typeDef.Kind));

                case TypeRefSymbol typeRef:
                    return WriteXml("Type", avw => 
                        avw.WriteXml("Name", typeRef.Name)
                        .Write(typeRef.TypeArgs));

                case VariableSymbol variable:
                    return WriteXml("VariableSymbol", avw => avw
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
        public static string ToXml(this Symbol value)
        {
            var avw = new SymbolWriter();
            avw.Write(value);
            return avw.ToString();
        }

        public static string ToXml(this IEnumerable<Symbol> values)
        {
            var avw = new SymbolWriter();
            avw.Write(values);
            return avw.ToString();
        }
    }
}