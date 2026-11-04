using System.Collections;
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// The solver's variable bindings, with an optional parent. A trial that used to start from
    /// <c>new Dictionary(Substitution)</c> (a copy of every binding, once per overload candidate) now
    /// starts from <c>new TypeBindings(Substitution)</c>: reads fall through to the parent, writes stay
    /// local, and enumeration yields only the local writes. Adopting a trial back into its parent
    /// (<c>foreach (kv in scratch) parent[kv.Key] = kv.Value</c>) therefore copies the delta, which is
    /// the same result as copying a full snapshot over the parent it was taken from.
    /// A parent must not be written to while a child is being read, other than by adopting that child.
    /// </summary>
    public sealed class TypeBindings : IEnumerable<KeyValuePair<string, TypeExpression>>
    {
        private readonly TypeBindings _parent;
        private Dictionary<string, TypeExpression> _own;

        public TypeBindings() { }

        public TypeBindings(TypeBindings parent) { _parent = parent; }

        public bool TryGetValue(string name, out TypeExpression value)
        {
            for (var b = this; b != null; b = b._parent)
                if (b._own != null && b._own.TryGetValue(name, out value))
                    return true;
            value = null;
            return false;
        }

        public TypeExpression this[string name]
        {
            get => TryGetValue(name, out var v) ? v : throw new KeyNotFoundException(name);
            set => (_own ??= new Dictionary<string, TypeExpression>())[name] = value;
        }

        public IEnumerator<KeyValuePair<string, TypeExpression>> GetEnumerator()
            => (_own ?? new Dictionary<string, TypeExpression>()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
