using System;
using System.Collections.Generic;
using System.Linq;

namespace Plato.Compiler
{
    public class Constraint  
    { }

    public class FunctionArgConstraint : Constraint
    {
        public Symbol Function { get; }
        public int Position { get; }
        public int ArgumentCount { get; }
        public string Name { get; }

        public FunctionArgConstraint(string name, Symbol fs, int position, int count)
        {
            (Name, Function, Position, ArgumentCount) = (name, fs, position, count);
            if (Position >= ArgumentCount)
                throw new Exception("Internal error!");
        }

        public override string ToString()
            => $"Argument:{Function}({Position+1}/{ArgumentCount})";
    }

    public class FunctionCallConstraint : Constraint
    {
        public IReadOnlyList<Symbol> Args { get; }

        public FunctionCallConstraint(IEnumerable<Symbol> args)
            => Args = args.ToList();

        public override string ToString()
            => $"Invoked:({string.Join(",", Args)})";
    }

    public static class Constraints
    {
        public static Dictionary<ParameterSymbol, List<Constraint>> GetParameterConstraints(FunctionSymbol f)
        {
            var r = new Dictionary<ParameterSymbol, List<Constraint>>();

            if (f != null)
            {
                foreach (var ps in f.Parameters)
                {
                    if (!r.ContainsKey(ps))
                        r.Add(ps, new List<Constraint>());
                }

                foreach (var sym in f.Body.GetDescendantSymbols())
                {
                    if (sym is FunctionCallSymbol fs)
                    {

                        var name = (fs.Function as RefSymbol)?.Name ?? "_unknownfunc_";

                        {
                            if (fs.Function.GetDef() is ParameterSymbol ps)
                            {
                                if (r.ContainsKey(ps))
                                {
                                    r[ps].Add(new FunctionCallConstraint(fs.Args));
                                }
                            }
                        }

                        foreach (var arg in fs.Args)
                        {
                            if (arg.Original is RefSymbol rs && rs.Def is ParameterSymbol ps)
                            {
                                if (r.ContainsKey(ps))
                                {
                                    r[ps].Add(new FunctionArgConstraint(name, fs.Function, arg.Position, fs.Args.Count));
                                }
                            }
                        }
                    }
                }
            } 
            return r;
        }
    }
}
