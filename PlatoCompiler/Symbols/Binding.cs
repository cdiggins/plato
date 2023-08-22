using System.Collections.Generic;
using System.Linq;

namespace Plato.Compiler.Symbols
{
    public class Binding
    {
        public string Name { get; }
        public Symbol Value { get; }
        public Binding Previous { get; }
        public Binding(string name, Symbol value, Binding previous = null)
            => (Name, Value, Previous) = (name, value, previous);
    }

    public static class BindingExtensions
    {
        public static IEnumerable<Binding> Enumerate(this Binding binding)
        {
            for (; binding != null; binding = binding.Previous)
                yield return binding;
        }

        public static Binding Find(this Binding binding, string name)
            => binding.Enumerate().FirstOrDefault(x => x.Name == name);

        public static Binding Add(this Binding binding, string name, Definition value)
            => new Binding(name, value, binding);
    }
}