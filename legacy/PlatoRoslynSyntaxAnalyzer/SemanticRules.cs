using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PlatoIR;

namespace PlatoRoslynSyntaxAnalyzer
{
    // TODO: add more things to the declarations. 
    public class SemanticRules
    {
        public static void UnsupportedSymbol(ISymbol symbol)
            => throw new Exception($"Could not find symbol {symbol}");

        public static void CheckResolution(ISymbol symbol, IR ir)
        {
            if (ir == null)
                throw new Exception($"could not resolve IR for {symbol}");
        }

        public static void ResolutionFailed(SyntaxNode node)
            => throw new Exception($"no symbol found for {node}");

        public static void UnsupportedSyntax(SyntaxNode node)
            => throw new Exception($"unsupported syntax {node}");

        public static void NotAValidLValue(SyntaxNode syntax)
            => throw new Exception($"cannot assign to {syntax}");

        public static void OnlyOneSubscriptSupported(SyntaxNode syntax)
            => throw new Exception($"only one subscript is allowed {syntax}");

        public static void NotSupportedLiteral(LiteralExpressionSyntax literal)
            => throw new Exception($"literal is not supported {literal}");

        public static void OnlyDotNotationSupported(SyntaxNode syntax)
            => throw new Exception($"only dot notation supported {syntax}");

        public static void AssureBodyOrBlock(SyntaxNode syntax, bool cond)
        {
            if (!cond)
                throw new Exception($"either body or block must be present, but not both {syntax}");
        }

        public static Exception LabeledArgsNotSupportedException(SyntaxNode node)
            => new Exception($"labeled arguments not supported {node}");

        public static void InvalidArray(SyntaxNode syntax)
            => new Exception($"Not a valid array form {syntax}");
    }
}
