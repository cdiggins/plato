using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PlatoAnalyzer
{
    public class Semantic<T>
        where T : PlatoSyntaxNode
    {
        public PlatoSemanticMapping Mapping { get; }
        public T PlatoSyntax { get; }

        public SyntaxElement CSharpSyntaxElement => throw new NotImplementedException();

        public TypeInfo? CSharpTypeInfo => CSharpSyntaxElement?.Type;
        public ITypeSymbol CSharpType => CSharpTypeInfo?.ConvertedType;
        public IOperation CSharpOp => CSharpSyntaxElement?.Operation;

        public SymbolInfo? CSharpSymbolInfo => CSharpSyntaxElement?.Symbol;
        public ISymbol CSharpSymbol => CSharpSymbolInfo?.Symbol;
        public SyntaxNode CSharpNode => CSharpSyntaxElement?.Node;
        public Location CSharpLocation => CSharpNode?.GetLocation();

        public bool HasValue => CSharpSyntaxElement?.HasConstantValue ?? false;
        public object Value => CSharpSyntaxElement?.ConstantValue;

        public SyntaxReference Declaration =>
            CSharpSymbol?.DeclaringSyntaxReferences.FirstOrDefault();

        public Semantic(PlatoSemanticMapping mapping, T platoSyntaxNode)
            => (Mapping, PlatoSyntax) = (mapping, platoSyntaxNode);
    }

    public static class SemanticExtensions
    {
        public static IEnumerable<IField> GetFields(this IClass cls)
            => cls.Children.OfType<IField>();
 
        public static IEnumerable<IProperty> GetProperties(this IClass cls)
            => cls.Children.OfType<IProperty>();

        public static IEnumerable<IMethod> GetMethods(this IClass cls)
            => cls.Children.OfType<IMethod>();
    }
}
