namespace Ara3D.Geometry.Compiler.Symbols
{
    //public class SymbolWriterJavaScript : CodeBuilder<SymbolWriterJavaScript>
    //{
    //    //public TypeResolver TypeResolver { get; }
    //    public bool WriteTypes { get; } = true;

    //    public SymbolWriterJavaScript(TypeResolver tg)
    //        => TypeResolver = tg;

    //    public SymbolWriterJavaScript Write(IEnumerable<Symbol> symbols)
    //        => symbols.Aggregate(this, (writer, symbol) => writer.Write(symbol));

    //    public SymbolWriterJavaScript WriteBlock(IEnumerable<Symbol> symbols)
    //        => symbols.Aggregate(WriteStartBlock(),
    //            (w, s) => w.Write(s)).WriteEndBlock(); 

    //    public SymbolWriterJavaScript AnnotateType(Symbol s)
    //        => AnnotateType(TypeResolver.GetType(s));

    //    public SymbolWriterJavaScript AnnotateType(TypeExpression trs)
    //        => AnnotateType(trs?.Definition);

    //    public SymbolWriterJavaScript AnnotateType(TypeDefinition tds)
    //    {
    //        if (WriteTypes)
    //        {
    //            var name = tds?.UniqueName ?? "UnknownType";
    //            return Write($"/* : {name} */");
    //        }

    //        return this;
    //    }

    //    public SymbolWriterJavaScript Write(FunctionDefinition function)
    //    {
    //        return Write("function".PadRight())
    //            .Write("(")
    //            .WriteCommaList(function.Parameters)
    //            .Write(") ")
    //            .AnnotateType(function.Type)
    //            .Write("{ ")
    //            .Write("return ")
    //            .Write(function.Body)
    //            .Write(";".PadRight())
    //            .Write("}");
    //    }

    //    public SymbolWriterJavaScript WriteCommaList(IEnumerable<Definition> symbols)
    //    {
    //        var r = this;
    //        var first = true;
    //        foreach (var s in symbols)
    //        {
    //            if (!first)
    //                r = r.Write(", ");
    //            first = false;
    //            r = r.Write(s);
    //        }

    //        return r;
    //    }

    //    public string GetName(Reference rs)
    //        => GetName(rs.Def);

    //    public string GetName(Definition ds)
    //    {
    //        if (ds is TypeDefinition ts) 
    //            return $"{ts.UniqueName}_{ts.Kind}";
    //        if (ds is FunctionGroupDefintion fgs)
    //        {
    //            if (fgs.Members.Count > 1)
    //                Debug.WriteLine($"Multiple functions found");
    //            return $"{fgs.Members[0].UniqueName}";
    //        }

    //        return $"{ds.UniqueName}";
    //    }

    //    public SymbolWriterJavaScript WriteConcept(TypeDefinition type)
    //    {
    //        if (type?.Kind != TypeKind.Concept) throw new Exception("Expected concept");
    //        WriteLine($"class {GetName(type)}");
    //        WriteStartBlock();
    //        WriteLine("constructor(self) { this.Self = self; };");
    //        WriteMethods(type);
    //        WriteEndBlock();
    //        return this;
    //    }

    //    public SymbolWriterJavaScript WriteMethods(TypeDefinition type)
    //    {
    //        foreach (var method in type.Methods)
    //        {
    //            var f = method.Function;
    //            Write($"static {GetName(f)} = ");
    //            Write(f);
    //            WriteLine(";");
    //        }

    //        return this;
    //    }

    //    public SymbolWriterJavaScript WriteType(TypeDefinition type)
    //    {
    //        if (type?.Kind != TypeKind.Type) throw new Exception("Expected type");
    //        var name = GetName(type);
    //        WriteLine($"class {name}");
    //        WriteStartBlock();

    //        // TODO: the default implementation of concepts will require this as well. 
    //        Write("constructor")
    //            .Write("(")
    //            .Write(string.Join(", ", type.Fields.Select(GetName)))
    //            .Write(")")
    //            .WriteLine();
    //        WriteStartBlock();
    //        WriteLine("// field initialization ");
    //        foreach (var field in type.Fields)
    //        {
    //            WriteLine($"this.{GetName(field)} = {GetName(field)};");
    //        }

    //        var concepts = type.GetAllImplementedConcepts();
    //        foreach (var concept in concepts)
    //        {
    //            var cName = GetName(concept.Definition);
    //            foreach (var m in concept.Definition.Methods)
    //            {
    //                var f = m.Function;
    //                var fName = GetName(f);
    //                WriteLine($"this.{fName} = {name}.{cName}.{fName};");
    //            }
    //        }

    //        WriteEndBlock();

    //        WriteLine("// field accessors");
    //        foreach (var field in type.Fields)
    //        {
    //            Write($"static {GetName(field)} = function(self)");
    //            WriteLine($" {{ return self.{GetName(field)}; }}");
    //        }

    //        WriteLine("// implemented concepts ");
    //        var conceptNames = new List<string>();
    //        foreach (var concept in concepts)
    //        {
    //            if (concept.Definition.Kind != TypeKind.Concept) 
    //                throw new Exception("Expected concept");
    //            var conceptName = GetName(concept.Definition);
    //            conceptNames.Add(conceptName);
    //            WriteLine($"static {conceptName} = new {conceptName}({name});");
    //        }
    //        WriteLine($"static Implements = [{string.Join(",", conceptNames)}];");

    //        WriteEndBlock();
    //        return this;
    //    }

    //    public SymbolWriterJavaScript WriteLibrary(TypeDefinition type)
    //    {
    //        if (type?.Kind != TypeKind.Library) throw new Exception("Expected library");
    //        var name = GetName(type);
    //        WriteLine($"class {name}");
    //        WriteStartBlock();
    //        foreach (var method in type.Methods)
    //        {
    //            var f = method.Function;
    //            Write($"static {GetName(f)} = ");
    //            Write(f);
    //            WriteLine(";");
    //        }
    //        WriteEndBlock();
    //        return this;
    //    }

    //    public SymbolWriterJavaScript WriteFile(IReadOnlyList<TypeDefinition> typeDefs)
    //    {
    //        var concepts = typeDefs.Where(s => s.Kind == TypeKind.Concept).ToList();
    //        var libraries = typeDefs.Where(s => s.Kind == TypeKind.Library).ToList();
    //        var types = typeDefs.Where(s => s.Kind == TypeKind.Type).ToList();

    //        foreach (var x in libraries)
    //            WriteLibrary(x);
    //        foreach (var x in concepts)
    //            WriteConcept(x);
    //        foreach (var x in types)
    //            WriteType(x);
    //        return this;
    //    }

    //    public SymbolWriterJavaScript Write(Definition value)
    //    {

    //    }

    //    public SymbolWriterJavaScript Write(Expression value)
    //    {
    //        switch (value)
    //        {
    //            case null:
    //                return Write("null");

    //            case Reference refSymbol:
    //                return Write(GetName(refSymbol))
    //                    .AnnotateType(refSymbol);

    //            case Argument argument:
    //                return Write(argument.Expression);

    //            case Assignment assignment:
    //                return Write(assignment.LValue)
    //                    .Write(" = ")
    //                    .Write(assignment.RValue)
    //                    .AnnotateType(assignment);

    //            case ConditionalExpressionSymbol conditional:
    //                return Write(conditional.Condition)
    //                    .Indent().WriteLine()
    //                    .Write("? ")
    //                    .Write(conditional.IfTrue)
    //                    .WriteLine()
    //                    .Write(": ")
    //                    .Write(conditional.IfFalse)
    //                    .Dedent()
    //                    .WriteLine();

    //            case FunctionDefinition function:
    //                return Write(function);

    //            case FunctionCall functionCall:
    //                return Write(functionCall.Function).Write("(")
    //                    .WriteCommaList(functionCall.Args).Write(")")
    //                    .AnnotateType(functionCall);

    //            case Literal literal:
    //                // TODO: add a constructor 
    //                return Write(literal.Value.ToLiteralString())
    //                    .AnnotateType(literal);

    //            case MethodDefinition methodDef:
    //                throw new Exception("Not implemented");

    //            case TypeParameterDefinition typeParameterDef:
    //                throw new Exception("Not implemented");

    //            case MemberDefinition member:
    //                throw new Exception("Not implemented");

    //            case ParameterDefinition parameter:
    //                return Write(GetName(parameter))
    //                    .AnnotateType(parameter);

    //            case TypeDefinition typeDef:
    //                throw new NotImplementedException("Type definitions are supposed to be handled first ");

    //            case TypeExpression typeRef:
    //                throw new NotImplementedException("Type references are supposed to be handled in a function");

    //            case VariableDefinition variable:
    //                return Write(GetName(variable));

    //            case PredefinedDefinition predefined:
    //                return Write(predefined.Name);

    //            case FunctionGroupDefintion memberGroup:
    //                return Write(memberGroup.Name);

    //            default:
    //                throw new ArgumentOutOfRangeException(nameof(value));
    //        }

    //        return this;
    //    }
    //}
}