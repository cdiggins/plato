using System.IO;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        public static ScriptFile Create(ScriptFileId id, CSharpParseOptions options, CancellationToken token = default)
        {
            var f = id.Path;
            var newSource = SourceText.From(File.ReadAllText(f), System.Text.Encoding.UTF8);
            var newTree = CSharpSyntaxTree.ParseText(newSource, options, f, token);
            return new ScriptFile(id, newSource, newTree);
        }

        public static ScriptFile Create(string file, CSharpParseOptions options = null, CancellationToken token = default)
            => Create(ScriptFileId.Create(file), options, token);

    }
}
