using System;

namespace Ara3D.Geometry.Compiler.Symbols
{
    public static class SymbolWriterExtensions
    {
        public static string Pad(this string s)
            => " " + s + " ";

        public static string PadRight(this string s)
            => s + " ";

        public static string ToJavaScript(this Compilation compilation)
        {
            /*
            var tr = compiler.TypeResolver;
            var r = new SymbolWriterJavaScript(tr);
            if (tr == null)
                r.WriteLine("Compilation was not successful");
            else
                r.WriteFile(compiler.TypeDefs);
            return r.ToString();
            */
            throw new NotImplementedException();
        }

        public static string ToPlatoHtml(this Compilation compilation)
        {
            /*
            var tr = compiler.TypeResolver;
            var r = new SymbolWriterPlatoHtml(tr);
            r.WriteLine("<html><head><link rel=\"stylesheet\" href=\"style.css\"></head><body><pre>");

            if (tr == null)
                r.WriteLine("Compilation was not successful");
            else
                r.Write(compiler.TypeDefs);

            r.WriteLine("</pre></body></html>");
            return r.ToString();
            */
            throw new NotImplementedException();
        }


        public static string ToLiteralString(this object obj)
        {
            if (obj is string s)
                return $"\"{s}\"";
            if (obj is bool b)
                return b ? "true" : "false";
            return $"{obj}";
        }
    }
}