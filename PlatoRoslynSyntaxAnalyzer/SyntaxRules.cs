using System;
using Microsoft.CodeAnalysis;

namespace PlatoRoslynSyntaxAnalyzer
{
    public static class SyntaxRules
    {
        public static bool Throw(SyntaxNode node, string msg) => throw new Exception($"Unsupported syntax: {msg}");

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


    }
}
