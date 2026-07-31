using System.Linq;

namespace Ara3D.Geometry.AST
{
    public sealed record PlatoFormatOptions(
        bool Pretty = false,
        bool Compressed = true,
        bool TightDelimiters = true)
    {
        public static PlatoFormatOptions CompactDefault { get; } = new();

        public static PlatoFormatOptions FromArgs(string[] args)
        {
            var pretty = args.Contains("--pretty");
            var compressed = pretty
                ? args.Contains("--compressed")
                : !args.Contains("--no-compressed");
            var tightDelimiters = pretty
                ? args.Contains("--tight-delimiters")
                : !args.Contains("--no-tight-delimiters");
            return new PlatoFormatOptions(pretty, compressed, tightDelimiters);
        }
    }
}
