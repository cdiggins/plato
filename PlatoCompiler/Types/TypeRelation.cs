using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Types
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
                if (!Source.IsInterface() || !Source.IsConcrete())
                    return false;
                if (!Dest.IsInterface() || !Dest.IsConcrete())
                    return false;
                if (Dest.IsConcrete())
                    return Cast != null;
                return true;
            }
        }
    }
}