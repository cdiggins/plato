using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace PlatoVSIX
{
    // PlatoColors.cs
    internal static class PlatoColors
    {
        // Colors for classification types
        public static readonly Color KeywordDefinitionColor = Colors.Blue;
        public static readonly Color KeywordClauseColor = Colors.Teal;
        public static readonly Color KeywordStatementColor = Colors.Purple;
        public static readonly Color CommentColor = Colors.Green;
        public static readonly Color StringColor = Colors.Maroon;
        public static readonly Color IdentifierColor = Colors.Black;
        public static readonly Color NumberColor = Colors.Magenta;
        public static readonly Color OperatorColor = Colors.DarkCyan;
    }

    public class PlatoClassifier : IClassifier
    {

        private readonly IClassificationType _keywordDefinitionType;
        private readonly IClassificationType _keywordClauseType;
        private readonly IClassificationType _keywordStatementType;
        private readonly IClassificationType _commentType;
        private readonly IClassificationType _stringType;
        private readonly IClassificationType _identifierType;
        private readonly IClassificationType _numberType;
        private readonly IClassificationType _operatorType;

        // Keyword groups
        private static readonly string[] KeywordDefinitions = new[]
        {
            "class", "interface", "type", "enum", "module", "namespace", "function", "const", "let", "var"
        };

        private static readonly string[] KeywordClauses = new[]
        {
            "extends", "implements", "from", "as", "export", "import", "of", "in", "instanceof", "typeof"
        };

        private static readonly string[] KeywordStatements = new[]
        {
            "if", "else", "for", "while", "do", "switch", "case", "default", "break", "continue", "return", "throw",
            "try", "catch", "finally", "await", "async", "yield", "new", "delete"
        };

        private readonly Regex _regex;

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        public PlatoClassifier(IClassificationTypeRegistryService registry)
        {
            Debug.WriteLine("PlatoClassifier created");
            _keywordDefinitionType = registry.GetClassificationType(PlatoClassificationTypes.KeywordDefinition);
            _keywordClauseType = registry.GetClassificationType(PlatoClassificationTypes.KeywordClause);
            _keywordStatementType = registry.GetClassificationType(PlatoClassificationTypes.KeywordStatement);
            _commentType = registry.GetClassificationType(PlatoClassificationTypes.Comment);
            _stringType = registry.GetClassificationType(PlatoClassificationTypes.String);
            _identifierType = registry.GetClassificationType(PlatoClassificationTypes.Identifier);
            _numberType = registry.GetClassificationType(PlatoClassificationTypes.Number);
            _operatorType = registry.GetClassificationType(PlatoClassificationTypes.Operator);

            // Regular expression for tokenizing the code
            _regex = new Regex(
                @"(?<Comment>//.*$)|" +
                @"(?<String>""[^""\\]*(?:\\.[^""\\]*)*""|'[^'\\]*(?:\\.[^'\\]*)*')|" +
                @"(?<Number>\b\d+(\.\d+)?\b)|" +
                @"(?<Word>\b\w+\b)|" +
                @"(?<Operator>[+\-*/%=<>!&|^~?:]+)",
                RegexOptions.Compiled | RegexOptions.Multiline);
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            Debug.WriteLine("GetClassificationSpans called");

            var classifications = new List<ClassificationSpan>();
            var text = span.GetText();

            foreach (Match match in _regex.Matches(text))
            {
                ClassificationSpan classificationSpan = null;

                if (match.Groups["Comment"].Success)
                {
                    var commentSpan = new SnapshotSpan(span.Snapshot, span.Start + match.Index, match.Length);
                    classificationSpan = new ClassificationSpan(commentSpan, _commentType);
                }
                else if (match.Groups["String"].Success)
                {
                    var stringSpan = new SnapshotSpan(span.Snapshot, span.Start + match.Index, match.Length);
                    classificationSpan = new ClassificationSpan(stringSpan, _stringType);
                }
                else if (match.Groups["Number"].Success)
                {
                    var numberSpan = new SnapshotSpan(span.Snapshot, span.Start + match.Index, match.Length);
                    classificationSpan = new ClassificationSpan(numberSpan, _numberType);
                }
                else if (match.Groups["Word"].Success)
                {
                    var word = match.Value;
                    var wordSpan = new SnapshotSpan(span.Snapshot, span.Start + match.Index, match.Length);

                    if (Array.Exists(KeywordDefinitions, k => k == word))
                    {
                        classificationSpan = new ClassificationSpan(wordSpan, _keywordDefinitionType);
                    }
                    else if (Array.Exists(KeywordClauses, k => k == word))
                    {
                        classificationSpan = new ClassificationSpan(wordSpan, _keywordClauseType);
                    }
                    else if (Array.Exists(KeywordStatements, k => k == word))
                    {
                        classificationSpan = new ClassificationSpan(wordSpan, _keywordStatementType);
                    }
                    else
                    {
                        classificationSpan = new ClassificationSpan(wordSpan, _identifierType);
                    }
                }
                else if (match.Groups["Operator"].Success)
                {
                    var operatorSpan = new SnapshotSpan(span.Snapshot, span.Start + match.Index, match.Length);
                    classificationSpan = new ClassificationSpan(operatorSpan, _operatorType);
                }

                if (classificationSpan != null)
                {
                    classifications.Add(classificationSpan);
                }
            }

            return classifications;
        }
    }
}
