using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Plato
{
    public class ScriptFile
    {
        public readonly ScriptFileId Id;
        public readonly EmbeddedText EmbeddedText;
        public readonly SourceText SourceText;
        public readonly SyntaxTree SyntaxTree;
        public string FilePath => Id.Path;

        public ScriptFile(ScriptFileId id, SourceText source, SyntaxTree tree)
        {
            Id = id;
            SourceText = source;
            EmbeddedText = EmbeddedText.FromSource(FilePath, SourceText);
            SyntaxTree = tree;
        }
    }
}
