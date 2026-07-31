namespace Ara3D.Geometry.ContextExport;

public sealed record ExportDiagnostics(
    int Files,
    int TypeCount,
    int InterfaceCount,
    int CharacterCount,
    int LineCount,
    int EstimatedTokens)
{
    public static ExportDiagnostics FromOutput(string output, int files, int typeCount, int interfaceCount)
    {
        var characters = output.Length;
        var lines = output.Length == 0 ? 0 : output.Count(c => c == '\n') + 1;
        var estimatedTokens = (int)Math.Ceiling(characters / 4.0);
        return new ExportDiagnostics(files, typeCount, interfaceCount, characters, lines, estimatedTokens);
    }

    public void Write(TextWriter writer)
    {
        writer.WriteLine($"files: {Files}");
        writer.WriteLine($"types: {TypeCount}");
        writer.WriteLine($"interfaces: {InterfaceCount}");
        writer.WriteLine($"characters: {CharacterCount}");
        writer.WriteLine($"lines: {LineCount}");
        writer.WriteLine($"estimated_tokens: {EstimatedTokens}");
    }
}
