namespace PlatoTypeInference
{
    public class AbstractSyntaxTree
    {
        public AbstractSyntaxNode Root { get; }
    }

    public class AbstractSyntaxNode
    {
        public SourceRange Range { get; }
    }

    public class SourceRange
    {
        public SourceLocation Begin { get; }
        public SourceLocation End { get; }
    }

    public class SourceLocation
    {
        public string FileName { get; }
        public int Index { get; }
    }

    public class LineColumn
    {
        public int Line { get; }
        public int Column { get; }
    }

}