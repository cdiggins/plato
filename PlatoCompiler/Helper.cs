namespace Plato.Compiler
{
    public static class Helper
    {
        public static AstConstant ToConstant(this string s)
            => AstConstant.Create(s);
    }
}