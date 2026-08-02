using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace PlatoVSIX
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("plato")]
    [Name("PlatoClassifierProvider")]
    [Order(Before = Priority.Default)]
    public class PlatoClassifierProvider : IClassifierProvider
    {
        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry = null; // Set via MEF

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            Debug.WriteLine("Get classifier being called");
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new PlatoClassifier(ClassificationRegistry));
        }
    }

    // Keyword Definition Format
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = PlatoClassificationTypes.KeywordDefinition)]
    [Name("PlatoKeywordDefinition")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PlatoKeywordDefinitionFormat : ClassificationFormatDefinition
    {
        public PlatoKeywordDefinitionFormat()
        {
            DisplayName = "Plato Keyword Definition";
            ForegroundColor = PlatoColors.KeywordDefinitionColor;
        }
    }

    // Keyword Clause Format
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = PlatoClassificationTypes.KeywordClause)]
    [Name("PlatoKeywordClause")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PlatoKeywordClauseFormat : ClassificationFormatDefinition
    {
        public PlatoKeywordClauseFormat()
        {
            DisplayName = "Plato Keyword Clause";
            ForegroundColor = PlatoColors.KeywordClauseColor;
        }
    }

    // Keyword Statement Format
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = PlatoClassificationTypes.KeywordStatement)]
    [Name("PlatoKeywordStatement")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PlatoKeywordStatementFormat : ClassificationFormatDefinition
    {
        public PlatoKeywordStatementFormat()
        {
            DisplayName = "Plato Keyword Statement";
            ForegroundColor = PlatoColors.KeywordStatementColor;
        }
    }

    // Comment Format
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = PlatoClassificationTypes.Comment)]
    [Name("PlatoComment")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PlatoCommentFormat : ClassificationFormatDefinition
    {
        public PlatoCommentFormat()
        {
            DisplayName = "Plato Comment";
            ForegroundColor = PlatoColors.CommentColor;
        }
    }

    // String Format
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = PlatoClassificationTypes.String)]
    [Name("PlatoString")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PlatoStringFormat : ClassificationFormatDefinition
    {
        public PlatoStringFormat()
        {
            DisplayName = "Plato String";
            ForegroundColor = PlatoColors.StringColor;
        }
    }

    // Identifier Format
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = PlatoClassificationTypes.Identifier)]
    [Name("PlatoIdentifier")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PlatoIdentifierFormat : ClassificationFormatDefinition
    {
        public PlatoIdentifierFormat()
        {
            DisplayName = "Plato Identifier";
            ForegroundColor = PlatoColors.IdentifierColor;
        }
    }

    // Number Format
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = PlatoClassificationTypes.Number)]
    [Name("PlatoNumber")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PlatoNumberFormat : ClassificationFormatDefinition
    {
        public PlatoNumberFormat()
        {
            DisplayName = "Plato Number";
            ForegroundColor = PlatoColors.NumberColor;
        }
    }

    // Operator Format
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = PlatoClassificationTypes.Operator)]
    [Name("PlatoOperator")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class PlatoOperatorFormat : ClassificationFormatDefinition
    {
        public PlatoOperatorFormat()
        {
            DisplayName = "Plato Operator";
            ForegroundColor = PlatoColors.OperatorColor;
        }
    }
}
