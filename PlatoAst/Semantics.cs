using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlatoAst
{
    public class Semantics
    {
        public static Dictionary<string, List<Symbol>> GetParameterReferences(FunctionSymbol f)
        {
            var r = new Dictionary<string, List<Symbol>>();

            if (f != null)
            {
                foreach (var ps in f.Parameters)
                {
                    if (!r.ContainsKey(ps.Name))
                        r.Add(ps.Name, new List<Symbol>());
                }

                foreach (var sym in f.Body.AllDescendantSymbols())
                {
                    if (sym is RefSymbol rs)
                    {
                        if (r.ContainsKey(rs.Name))
                            r[rs.Name].Add(sym);
                        //else throw new Exception("Refers to parameter in outer scope. Legal, but not expected yet.");
                    }
                }
            } 
            return r;
        }
    }
}
