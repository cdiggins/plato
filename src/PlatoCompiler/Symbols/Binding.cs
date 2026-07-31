namespace Ara3D.Geometry.Compiler.Symbols
{
    public class Binding
    {
        public string Name { get; }
        public Symbol Value { get; }

        public Binding(string name, Symbol value)
            => (Name, Value) = (name, value);
    }
}