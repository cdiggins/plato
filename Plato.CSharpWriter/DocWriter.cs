using System.Collections.Generic;
using System.Linq;
using Ara3D.Utils;
using Plato.AST;
using Plato.Compiler;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.CSharpWriter
{
    public class DocWriter : HtmlBuilder
    {
        public const string Prefix = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/> 
    <link rel=""stylesheet"" href=""style.css""/>
    <title>Plato Geometry Documentation</title>
</head>
<body>
";

        public const string Suffix = @"
</body>
<html>";

        public DocWriter(Compilation compilation)
        {
            var types = compilation.AllTypeAndLibraryDefinitions.ToList();

            WriteLine(Prefix);

            WriteStartTag("div", "container").WriteLine();
            WriteStartTag("div", "nav").WriteLine();
            WriteToc(types);
            WriteEndTag("div").WriteLine();

            WriteStartTag("div", "content").WriteLine();
            WriteClasses(types);
            WriteInterfaces(types);
            WriteEndTag("div").WriteLine();
            WriteEndTag("div");

            WriteLine(Suffix);
        }

        public void WriteToc(IReadOnlyList<TypeDef> typeDefs)
        {
            WriteTaggedText("h3", "Classes", ("class", "collapsible")).WriteLine();
            var concreteTypes = typeDefs.Where(t => t.IsConcrete()).OrderBy(t => t.Name).ToList();
            WriteStartTag("ul").WriteLine();
            foreach (var t in concreteTypes)
            {
                var te = t.ToTypeExpression();
                WriteUnescapedTaggedText("li", $"{LinkType(te)}").WriteLine();
            }
            WriteEndTag("ul").WriteLine();
        
            WriteTaggedText("h3", "Interfaces", ("class", "collapsible")).WriteLine();
            var interfaceTypes = typeDefs.Where(t => t.IsInterface()).OrderBy(t => t.Name).ToList();
            WriteStartTag("ul").WriteLine();
            foreach (var t in interfaceTypes)
            {
                var te = t.ToTypeExpression();
                WriteUnescapedTaggedText("li", $"{LinkType(te)}").WriteLine();
            }
            WriteEndTag("ul").WriteLine();
        }

        public void WriteClasses(IEnumerable<TypeDef> typeDefs)
        {
            var types = typeDefs.Where(t => t.IsConcrete()).OrderBy(t => t.Name).ToList();
            WriteTaggedText("h1", "Classes", ("class", "collapsible")).WriteLine();
            foreach (var t in types)
                WriteClass(t);
        }

        public void WriteInterfaces(IEnumerable<TypeDef> typeDefs)
        {
            var types = typeDefs.Where(t => t.IsInterface()).OrderBy(t => t.Name).ToList();
            WriteTaggedText("h1", "Interfaces", ("class", "collapsible")).WriteLine();
            foreach (var t in types)
                WriteInterface(t);
        }

        public static HtmlAttribute IdAttr(TypeDef td)
            => IdAttr(Id(td));

        public static string Id(TypeDef td)
        {
            var kind = td.Kind == TypeKind.Concept ? "interface" : "class";
            return $"{kind}-{td.Name}";
        }

        public string LinkType(TypeExpression te)
        {
            var id = Id(te.Def);
            var baseName = (te.Def.IsConcrete() || te.Def.IsInterface())
                ? $"<a href='#{id}'>{te.Def.Name}</a>"
                : te.Def.Name;
            if (te.TypeArgs.Count == 0)
                return baseName;
            var args = te.TypeArgs.Select(LinkType).JoinStringsWithComma();
            return $"{baseName}&lt;{args}&gt;";
        }

        public void WriteClass(TypeDef type)
        {
            WriteUnescapedTaggedText("h2", $"Class {type.Name}", IdAttr(type)).WriteLine();

            WriteTaggedText("h3", $"Fields").WriteLine();

            WriteStartTag("ul").WriteLine();
            foreach (var f in type.Fields)
                WriteUnescapedTaggedText("li", $"{LinkType(f.Type)} {f.Name}").WriteLine();
            WriteEndTag("ul").WriteLine();

            WriteTaggedText("h3", $"Interfaces").WriteLine();

            WriteStartTag("ul").WriteLine();
            foreach (var impl in type.Implements)
                WriteUnescapedTaggedText("li", $"{LinkType(impl)}").WriteLine();
            WriteEndTag("ul").WriteLine();
                
            Write("<hr class='solid'/>");
        }

        public void WriteInterface(TypeDef type)
        {
            WriteUnescapedTaggedText("h2", $"Interface {type.Name}", IdAttr(type)).WriteLine();

            WriteTaggedText("h3", $"Functions").WriteLine();

            WriteStartTag("ul").WriteLine();
            foreach (var f in type.Functions)
            {
                var paramStr = f.Parameters.Select(p => $"{LinkType(p.Type)} {p.Name}").JoinStringsWithComma(); 
                WriteUnescapedTaggedText("li", $"{LinkType(f.ReturnType)} {f.Name}({paramStr});").WriteLine();
            }
            WriteEndTag("ul").WriteLine();

            WriteTaggedText("h3", $"Inherits").WriteLine();

            WriteStartTag("ul").WriteLine();
            foreach (var te in type.Inherits)
                WriteUnescapedTaggedText("li", $"{LinkType(te)}").WriteLine();
            WriteEndTag("ul").WriteLine();

            Write("<hr class='solid'/>");
        }
    }
}
