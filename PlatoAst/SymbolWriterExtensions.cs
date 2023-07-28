namespace Plato.Compiler
{
    public static class SymbolWriterExtensions
    {
        public static string Pad(this string s)
            => " " + s + " ";
        
        public static string PadRight(this string s)
            => s + " ";

        public static string ToJavaScript(this Compilation compilation)
            => compilation.Operations.ToJavaScript();

        public static string ToJavaScript(this Operations ops)
        {
            var tg = new TypeResolver(ops);
            var writer = new SymbolWriterJavaScript(tg);
            var r = writer.Write(ops.Types);
            return r.ToString();
        }

        public static string ToPlatoHtml(this Compilation compilation)
            => compilation.Operations.ToPlatoHtml();

        public static string ToPlatoHtml(this Operations ops)
        {
            var tg = new TypeResolver(ops);
            var r = new SymbolWriterPlatoHtml(tg);
            r.WriteLine("<html><head><link rel=\"stylesheet\" href=\"style.css\"></head><body><pre>");
            r.Write(ops.Types);
            r.WriteLine("</pre></body></html>");
            return r.ToString();
        }

        public static string ToCSharp(this Compilation compilation)
            => compilation.Operations.ToCSharp();

        public static string ToCSharp(this Operations ops)
        {
            var tg = new TypeResolver(ops);
            var writer = new SymbolWriterCSharp(tg);
            var r = writer.Write(ops.Types);
            return r.ToString();
        }

        public static string ToLiteralString(this object obj)
        {
            if (obj is string s)
                return $"\"{s}\"";
            return $"{obj}";
        }
    }
}