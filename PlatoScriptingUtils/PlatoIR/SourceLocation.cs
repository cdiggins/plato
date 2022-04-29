namespace PlatoIR
{
    public class SourceLocation
    {
        public string FilePath { get; set; }
        public int Position { get; set; }
        public int Length { get; set; }
        public int SyntaxKind { get; set; }
    }
}
