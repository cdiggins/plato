using PlatoAbstractSyntaxTree;

namespace PlatoAstWriter
{

    public interface IAstWriter
    {
        public CodeBuilder Write(CodeBuilder builder, AbstractNode node);
    }
}