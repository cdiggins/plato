using Parakeet;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace PlatoAst
{
    public class SymbolWriterCSharp : CodeBuilder<SymbolWriterCSharp>
    {
        private Operations Ops { get; }

        public SymbolWriterCSharp(Operations ops)
            => Ops = ops;

        public SymbolWriterCSharp WriteBlock(params Symbol[] symbols)
            => WriteBlock((IEnumerable<Symbol>)symbols, false);

        public SymbolWriterCSharp Write(IEnumerable<Symbol> symbols)
            => symbols.Aggregate(this, (writer, symbol) => writer.Write(symbol));

        public SymbolWriterCSharp WriteBlock(IEnumerable<Symbol> symbols, bool semiColons)
        {
            var r = WriteLine("{").Indent();
            foreach (var symbol in symbols)
            {
                r = r.Write(symbol);
                
                if (semiColons)
                    r = r.WriteLine(";");
                else
                    r = r.WriteLine("");
            }
            r = r.Dedent().WriteLine("}");
            return r;
        }

        public SymbolWriterCSharp WriteRegion(IReadOnlyList<Symbol> symbols)
        {
            if (symbols.Count == 1 && symbols[0] is RegionSymbol rs)
                return Write(rs);
            var r = WriteLine("{").Indent();
            for (var i=0; i < symbols.Count; ++i)
            {
                var symbol = symbols[i];
                if (i == symbols.Count - 1)
                    r = r.Write("return ");
                r = r.Write(symbol);
                r = r.WriteLine(";");
            }
            r = r.Dedent().WriteLine("}");
            return r;
        }

        public SymbolWriterCSharp WriteTypeDecl(TypeRefSymbol typeRef, string defaultType = "var")
        {
            if (typeRef == null)
                return Write($"{defaultType} ");
            else
                return Write(typeRef.Name).Write(" ");
        }

        public string GetBestCandidate(FunctionArgConstraint fac)
        {
            var name = fac.Name;
            var members = Ops.GetMembers(name, fac.ArgumentCount);
            
            // NOTE: this is not accurate. It has to do with the actual type based on
            // the position. 
            var position = fac.Position;

            var tmp = members.Where(pair => pair.Item1.Kind == "concept").ToList();
            if (tmp.Count == 0)
                tmp = members.ToList();

            if (tmp.Count > 0)
            {
                var t = tmp[0].Item1;
                var m = tmp[0].Item2;
                if (m is FieldDefSymbol fds)
                {
                    return fds.Type?.Name ?? "dynamic";
                }

                if (m is MethodDefSymbol mds)
                {
                    if (position >= mds.Function.Parameters.Count)
                    {
                        Debug.WriteLine($"Position {position} out of range for {mds.Function}, probably auto-mapped?");
                        position = mds.Function.Parameters.Count - 1;
                    }

                    var param = mds.Function.Parameters[position];

                    // TODO: right now dynamic is just code for must be inferred later. 
                    return param.Type?.Name ?? "dynamic";
                }
            }

            return "Any";
        }

        public SymbolWriterCSharp WriteConstraints(FunctionSymbol function)
        {
            var lookup = Constraints.GetParameterConstraints(function);
            var r = this;
            foreach (var kv in lookup)
            {
                var refs = string.Join(", ", kv.Value);
                r = r.WriteLine($"// {kv.Key} = {refs}");
                foreach (var c in kv.Value)
                {
                    if (c is FunctionArgConstraint fac)
                    {
                        /*
                        var name = fac.Name;
                        var members = Ops.GetMembers(name, fac.ArgumentCount);

                        r = r.WriteLine($"// Candidate types for {kv.Key.Name}: ");
                        var temp = string.Join(" | ", members.Select(m => $"{m.Item1.Name}.{m.Item2.Name}"));
                        
                        // TODO: reduce the candidate list. 
                        // A single concept is preferable to a group of types.

                        if (members.Count == 0)
                            r = r.WriteLine($"// Any");
                        else
                            r = r.WriteLine($"// {temp}");
                        */

                        var best = GetBestCandidate(fac);
                        r = r.WriteLine($"// {kv.Key.Name} Best type is {best}");
                    }
                }
            }

            return r;
        }

        public SymbolWriterCSharp Write(FunctionSymbol function)
        {
            if (function.Name == "__lambda__")
            {
                return Write("(")
                    .WriteCommaList(function.Parameters.Select(p => p.Name))
                    .WriteLine(") => ")
                    .WriteConstraints(function)
                    .Write(function.Body);
            }

            // TODO: 
            // var node = function.Location;

            // function.T
            return WriteTypeDecl(function.Type, "void")
                .Write(function.Name)
                .Write("(")
                .WriteCommaList(function.Parameters)
                .WriteLine(")")
                .WriteConstraints(function)
                .Write(function.Body);
        }

        public SymbolWriterCSharp WriteCommaList(IEnumerable<Symbol> symbols)
        {
            var r = this;
            var first = true;
            foreach (var s in symbols)
            {
                if (!first)
                    r = r.Write(", ");
                first = false;
                r = r.Write(s);
            }

            return r;
        }

        public SymbolWriterCSharp WriteCommaList(IEnumerable<string> symbols)
        {
            var r = this;
            var first = true;
            foreach (var s in symbols)
            {
                if (!first)
                    r = r.Write(", ");
                first = false;
                r = r.Write(s);
            }

            return r;
        }

        public SymbolWriterCSharp Write(TypeDefSymbol typeDef)
        {
            if (typeDef.Kind == "concept")
            {
                var fullName = $"{typeDef.Name}<Self>";
                return Write("interface ").Write(fullName).Write(" where ")
                    .WriteLine($"Self : {fullName}")
                    .WriteBlock(typeDef.Members, false);
            }
            else 
            //if (typeDef.Kind == "type")
            {
                return Write("class ").WriteLine(typeDef.Name).WriteBlock(typeDef.Members, false);
            }
        }

        public SymbolWriterCSharp Write(Symbol value)
        {
            switch (value)
            {
                case null:
                    return Write("null");

                case RefSymbol refSymbol:
                    return Write(refSymbol.Name);

                case ArgumentSymbol argument:
                    return Write(argument.Original);

                case AssignmentSymbol assignment:
                    return Write(assignment.LValue)
                        .Write(" = ")
                        .Write(assignment.RValue);

                case ConditionalSymbol conditional:
                    return Write(conditional.Condition)
                        .Indent().WriteLine().Write("? ")
                        .Write(conditional.IfTrue)
                        .WriteLine()
                        .Write(": ")
                        .Write(conditional.IfFalse)
                        .Dedent().WriteLine();

                case FieldDefSymbol fieldDef:
                    return WriteTypeDecl(fieldDef.Type).Write(fieldDef.Name);

                case FunctionSymbol function:
                    return Write(function);
                    
                case FunctionResultSymbol functionResult:
                    return Write(functionResult.Function).Write("(")
                        .WriteCommaList(functionResult.Args).Write(")");

                case LiteralSymbol literal:
                    return Write(literal.Value.ToString());

                case MethodDefSymbol methodDef:
                    return Write("public static ").Write(methodDef.Function);

                case TypeParameterDefSymbol typeParameterDef:
                    throw new Exception("Not implemented");

                case MemberDefSymbol member:
                    throw new Exception("Not implemented");

                case NoValueSymbol noValue:
                    return Write("_");

                case ParameterSymbol parameter:
                    return WriteTypeDecl(parameter.Type).Write(parameter.Name);

                case RegionSymbol region:
                    return WriteRegion(region.Children);

                case TypeDefSymbol typeDef:
                    return Write(typeDef);
                
                case TypeRefSymbol typeRef:
                    throw new NotImplementedException("Type references are supposed to be handled in a function");

                case VariableSymbol variable:
                    return Write(variable.Name);    

                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }

            return this;
        }

        public static string ToCSharp(IEnumerable<Symbol> symbols, Operations ops)
        {
            var writer = new SymbolWriterCSharp(ops);
            var r = writer.Write(symbols);
            return r.ToString();
        }
    }
}