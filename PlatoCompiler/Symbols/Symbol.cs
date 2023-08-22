namespace Plato.Compiler.Symbols
{
    public class Symbol
    {
        public int Id { get; } = NextId++;
        public static int NextId = 0;

        public override bool Equals(object obj) => obj is Definition sd && sd.Id == Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}