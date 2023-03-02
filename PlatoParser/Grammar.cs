using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PlatoParser
{
    /// <summary>
    /// A class inheriting from grammar contains a set of parsing rules. Each parse rule is defined as 
    /// property getters returning a rule created by "Token" (generating no node) or "Phrase" (generating a node).  
    /// This class will store the rules as they are created, and assign them names 
    /// so that they can have recursive relations in them, have fixed names based on the properties, 
    /// and minimizes creating superfluous objects. 
    /// </summary>
    // [Mutable]
    public class Grammar
    {
        public Rule WhitespaceRule { get; protected set; }

        public Rule GetRuleFromName(string name)
        {
            var t = GetType();
            var pi = t.GetProperties().FirstOrDefault(p => p.Name == name);
            if (pi == null) return null;
            return pi.GetValue(this) as Rule;
        }

        public IEnumerable<Rule> GetRules()
            => GetType()
                .GetProperties()
                .Where(pi => typeof(Rule).IsAssignableFrom(pi.PropertyType))
                .Select(pi => pi.GetValue(this) as Rule);

        public static Rule Choice(IEnumerable<Rule> rules, [CallerMemberName] string name = "")
            => new Choice(rules, name);

        public static Rule Sequence(IEnumerable<Rule> rules, [CallerMemberName] string name = "")
            => new Sequence(rules, name);

        public static Rule Recursive(Func<Rule> f, [CallerMemberName] string name = "")
            => new RecursiveRule(f, name);

        public Rule Token(Rule r, [CallerMemberName] string name = "")
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name must not be null");
            if (Lookup.ContainsKey(name)) return Lookup[name];
            r = new TokenRule(r, name);
            r = r.WithName(name);
            Lookup.Add(name, r);
            return r;
        }

        public Rule Phrase(Rule r, [CallerMemberName] string name = "")
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name must not be null");
            if (Lookup.ContainsKey(name)) return Lookup[name];
            r = new NodeRule(r, WhitespaceRule, name);
            Lookup.Add(name, r);
            return r;
        }

        public OnError OnError(Rule r)
            => new OnError(r);       

        public Dictionary<string, Rule> Lookup = new Dictionary<string, Rule>();
    }
}