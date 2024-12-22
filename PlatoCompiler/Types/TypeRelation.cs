using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    /// <summary>
    /// Exists when Source can be used where Dest is requested. 
    /// </summary>
    public class TypeRelation
    {
        public TypeDef Source;
        public TypeDef Dest;
        public int Depth;
        public TypeExpression Expr;
        public IFunction Cast;

        public bool IsValid
        {
            get
            {
                if (Source == null || Dest == null) 
                    return false;
                if (!Source.IsConcept() || !Source.IsConcrete())
                    return false;
                if (!Dest.IsConcept() || !Dest.IsConcrete())
                    return false;
                if (Dest.IsConcrete())
                    return Cast != null;
                return true;
            }
        }
    }
}