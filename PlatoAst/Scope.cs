using System.Collections.Generic;
using System.Linq;

namespace PlatoAst
{
    public class Scope
    {
        public Scope Parent { get; }
        public Binding Bindings { get; }

        public Scope(Scope parent, Binding bindings)
            => (Parent, Bindings) = (parent, bindings);
    }

    public static class ScopeExtensions
    {
        public static Scope Bind(this Scope scope, string name, Symbol value)
            => new Scope(scope.Parent, scope.Bindings.Add(name, value));

        public static Binding Find(this Scope scope, string name)
            => scope.EnumerateBindings().FirstOrDefault(x => x.Name == name);
       
        public static Symbol GetValue(this Scope scope, string name)
            => scope.Find(name)?.Value;

        public static Scope Push(this Scope scope)
            => new Scope(scope, null);

        public static Scope Pop(this Scope scope)
            => scope.Parent;

        public static IEnumerable<Scope> Enumerate(this Scope scope)
        {
            for (;scope != null; scope = scope.Parent)
                yield return scope;
        }

        public static IEnumerable<Binding> EnumerateBindings(this Scope scope) 
            => scope.Enumerate().SelectMany(scp => scp.Bindings.Enumerate());
    }
}