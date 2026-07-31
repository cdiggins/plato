namespace Ara3D.Geometry.Compiler.Symbols
{
    public static class SymbolWriterExtensions
    {
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
