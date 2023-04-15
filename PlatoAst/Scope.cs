using System;
using System.Collections.Generic;

namespace PlatoAst
{
    public class Scope
    {
        public Scope Parent { get; }

        public Scope(Scope parent)
            => Parent = parent;

        public bool HasName(string name)
            => Names.ContainsKey(name);

        public object GetValue(string name)
            => Names[name].Item2;

        public void Bind(string name, object value)
            => Names[name] = (Names[name].Item1, value);

        public void Declare(AstVarDef def)
            => Declare(def, null);

        public void Declare(AstVarDef def, object value)
            => Names[def.Name] = (def, value);

        public Dictionary<string, (AstVarDef, object)> Names { get; } 
            = new Dictionary<string, (AstVarDef, object)>();

        public Scope FindName(string name)
            => HasName(name) 
                ? this 
                : Parent?.FindName(name) 
                  ?? throw new Exception($"No scope found contains {name}");
    }
}