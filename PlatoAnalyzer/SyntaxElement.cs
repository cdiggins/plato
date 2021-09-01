using Microsoft.CodeAnalysis;

namespace PlatoAnalyzer
{
    public class SyntaxElement
    {
        public SyntaxNode Node { get; }
        public SemanticModel Model { get; }

        public SyntaxElement(SyntaxNode node, SemanticModel model)
            => (Node, Model) = (node, model);

        public SymbolInfo Symbol =>
            Model.GetSymbolInfo(Node);

        public TypeInfo Type =>
            Model.GetTypeInfo(Node);

        public Location Location =>
            Node.GetLocation();

        public IOperation Operation =>
            Model.GetOperation(Node);

        public object ConstantValue =>
            Model.GetConstantValue(Node).Value;

        public bool HasConstantValue =>
            Model.GetConstantValue(Node).HasValue;

        public SyntaxElement<T> ToTypedElement<T>()
            where T: SyntaxNode
            => new SyntaxElement<T>((T) Node, Model);
    }

    public class SyntaxElement<T> : SyntaxElement
        where T : SyntaxNode
    {
        public new T Node => (T)base.Node;

        public SyntaxElement(T node, SemanticModel model)
            : base(node, model)
        { }
    }

    public static class SyntaxElementExtensions
    {
        public static SyntaxElement<T> ToElement<T>(this T node, SemanticModel model)
            where T : SyntaxNode
            => new SyntaxElement<T>(node, model);
    }
}