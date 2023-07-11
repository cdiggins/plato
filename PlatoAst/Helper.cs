namespace PlatoAst
{
    public static class Helper
    {
        public static AstConstant<string> ToConstant(this string s)
            => AstConstant.Create(s);
    }
}