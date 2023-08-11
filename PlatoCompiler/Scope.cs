using System.Collections.Generic;

namespace Plato.Compiler
{
    public class Scope
    {
        public Scope Parent { get; }
        public Dictionary<string, Symbol> Bindings { get; } = new Dictionary<string, Symbol>();

        public Scope(Scope parent)
            => Parent = parent;

        public Scope Bind(string name, Symbol value)
        {
            Bindings[name] =value;
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
    }
}