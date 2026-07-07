using System.Collections.Generic;

namespace Ara3D.Geometry.Compiler.Symbols
{
    public class Scope
    {
        public Scope Parent { get; }
        public Dictionary<string, Symbol> Bindings { get; } = new Dictionary<string, Symbol>();

        public Scope(Scope parent)
            => Parent = parent;

        public Scope Bind(string name, Symbol value)
        {
            // TODO: this overwrites bindings, instead we probably want a linked list of bindings. 
            Bindings[name] = value;
            return this;
        }

        public Binding Find(string name)
        {
            if (Bindings.ContainsKey(name))
                return new Binding(name, Bindings[name]);
            return Parent?.Find(name);
        }

        public Symbol GetValue(string name)
            => Find(name)?.Value;

        public Scope Push()
            => new Scope(this);

        public Scope Pop()
            => Parent;

        public static Binding CreateBinding(string name, Symbol symbol)
            => new Binding(name, symbol);

        public IEnumerable<Binding> GetAllBindings()
        {
            for (var curScope = this; curScope != null; curScope = curScope.Parent)
                foreach (var kv in curScope.Bindings)
                    yield return CreateBinding(kv.Key, kv.Value);
        }
    }
}