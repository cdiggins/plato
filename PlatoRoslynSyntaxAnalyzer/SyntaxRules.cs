using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlatoRoslynSyntaxAnalyzer
{
    public static class SyntaxRules
    {
        public static bool Throw(SyntaxNode node, string msg) => throw new Exception($"Unsupported syntax {msg} at {node}");

        /// <summary>
        /// Plato will probably never support support ref parameters. They can lead to hard to read non-fluent code. 
        /// </summary>
        public static bool NoRefParameters(this SyntaxNode node, bool condition) => condition || Throw(node, "ref parameters");
        
        /// <summary>
        /// Plato may support out parameters in the future. 
        /// </summary>
        public static bool NoOutParameters(this SyntaxNode node, bool condition) => condition || Throw(node, "out parameters");

        /// <summary>
        /// Later versions of Plato will probably support variadic (aka rest) parameters
        /// </summary>
        public static bool NoRestParameters(this SyntaxNode node, bool condition) => condition || Throw(node, "rest parameters");
        
        /// <summary>
        /// Only affine types can be modified, but even then setters are not supported.
        /// In the future Plato may require that affine types that are self-modifying have special attributes.
        /// The rationale is to facilitate refactoring to remove affine types, which are sometimes used incorrectly.  
        /// </summary>
        public static bool NoSetter(this SyntaxNode node, bool condition) => condition || Throw(node, "setter");

        /// <summary>
        /// This rule exists for compatibility with Unity
        /// </summary>
        public static bool NoInitOnlySetter(this SyntaxNode node, bool condition) => condition || Throw(node, "init only setter");

        /// <summary>
        /// Static indexers are not supported,
        /// </summary>
        public static bool NoStaticIndexer(this SyntaxNode node, bool condition) => condition || Throw(node, "static indexers");

        /// <summary>
        /// An indexer must have a getter
        /// </summary>
        public static bool MustHaveGetter(this SyntaxNode node, bool condition) => condition || Throw(node, "indexer missing getter");

        /// <summary>
        /// Indexers must not have zero or multiple parameters (rare, but legal)
        /// </summary>
        public static bool IndexerHasSingleParameter(this SyntaxNode node, bool condition) => condition || Throw(node, "indexer with number of parameters other than one");

        /// <summary>
        /// Used for signaling unrecognized statement types
        /// </summary>
        public static bool UnrecognizedStatement(this StatementSyntax node) => Throw(node, "unrecognized statement");

        /// <summary>
        /// Used for signaling unsupported statement types
        /// </summary>
        public static bool UnsupportedStatement(this StatementSyntax node) => Throw(node, "unsupported statement");

        /// <summary>
        /// Used for signaling unrecognized expression types
        /// </summary>
        public static bool UnrecognizedExpression(this ExpressionSyntax node) => Throw(node, "unrecognized expression");

        /// <summary>
        /// Used for signaling unsupported expression types
        /// </summary>
        public static bool UnsupportedExpression(this ExpressionSyntax node) => Throw(node, "unsupported exporession");

        /// <summary>
        /// Unrecognized type declaration
        /// </summary>
        public static bool SupportedType(this TypeDeclarationSyntax node, bool condition) => condition || Throw(node, "unrecognized type");

        public class UnsupportedSymbolException : Exception
        {
            public UnsupportedSymbolException(ISymbol symbol) : base($"unsupported symbol {symbol}") { }
        }

    }
}
