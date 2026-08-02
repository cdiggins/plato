using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace PlatoVSIX
{
    internal static class PlatoClassificationTypes
    {
        public const string KeywordDefinition = "PlatoKeywordDefinition";
        public const string KeywordClause = "PlatoKeywordClause";
        public const string KeywordStatement = "PlatoKeywordStatement";
        public const string Comment = "PlatoComment";
        public const string String = "PlatoString";
        public const string Identifier = "PlatoIdentifier";
        public const string Number = "PlatoNumber";
        public const string Operator = "PlatoOperator";
    }

    public static class PlatoClassificationDefinitions
    {
        
        // Classification Type Definitions
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(PlatoClassificationTypes.KeywordDefinition)]
        internal static ClassificationTypeDefinition PlatoKeywordDefinitionType;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(PlatoClassificationTypes.KeywordClause)]
        internal static ClassificationTypeDefinition PlatoKeywordClauseType;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(PlatoClassificationTypes.KeywordStatement)]
        internal static ClassificationTypeDefinition PlatoKeywordStatementType;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(PlatoClassificationTypes.Comment)]
        internal static ClassificationTypeDefinition PlatoCommentType;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(PlatoClassificationTypes.String)]
        internal static ClassificationTypeDefinition PlatoStringType;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(PlatoClassificationTypes.Identifier)]
        internal static ClassificationTypeDefinition PlatoIdentifierType;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(PlatoClassificationTypes.Number)]
        internal static ClassificationTypeDefinition PlatoNumberType;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(PlatoClassificationTypes.Operator)]
        internal static ClassificationTypeDefinition PlatoOperatorType;
    }
}