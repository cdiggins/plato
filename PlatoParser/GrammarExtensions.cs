using System.Diagnostics;

namespace PlatoParser
{
    public static class GrammarExtensions
    {
        public static Rule Body(this Rule rule)
        {
            if (rule is TokenRule tr)
                return tr.Rule.Body();
            if (rule is NodeRule nr)
                return nr.Rule.Body();
            if (rule is RecursiveRule rr)
                return rr.RuleFunc().Body();
            return rule;
        }

        public static void OutputDefinitions(this Grammar grammar)
        {
            foreach (var r in grammar.GetRules())
            {
                var justNodes = r.Body().OnlyNodes();
                var simplified = r.Body().Simplify();
                if (justNodes != null)
                {
                    Console.WriteLine($"{r.Name}:");
                    Console.WriteLine($"  {r.Body().ToDefinition()}");

                    Console.WriteLine($"SIMPLIFIED:");
                    Console.WriteLine($"  {simplified.ToDefinition()}");

                    Console.WriteLine($"JUST NODES:");
                    Console.WriteLine($"  {justNodes.ToDefinition()}");
                    
                    var justNodesSimplified = justNodes.Simplify();
                    Console.WriteLine($"JUST NODES SIMPLIFIED:");
                    Console.WriteLine($"  {justNodesSimplified.ToDefinition()}");
                }
            }
        }

        public static string ToDefinition(this IEnumerable<Rule> rules, string sep)
            => string.Join(sep, rules.Select(r => r.ToDefinition()));
        
        public static string ToDefinition(this Rule r)
        {
            switch (r)
            {
                case NodeRule nr:
                    return r.Name;
                case TokenRule tr:
                    return r.Name;
                case Sequence seq:
                    return $"({seq.Rules.ToDefinition("+")})";
                case Choice ch:
                    return $"({ch.Rules.ToDefinition("|")})";
                case Optional opt:
                    return $"({opt.Rule.ToDefinition()})?";
                case ZeroOrMore z:
                    return $"({z.Rule.ToDefinition()})*";
                case RecursiveRule rec:
                    return rec.RuleFunc().ToDefinition();
                case StringMatchRule sm:
                    return $"\"{sm.Pattern}\"";
                case AnyCharRule:
                    return $".";
                case NotAt not:
                    return $"!({not.Rule.ToDefinition()})";
                case At at:
                    return $"&({at.Rule.ToDefinition()})";
                case CharRangeRule range:
                    return $"[{range.Low}..{range.High}]";
                case CharSetRule set:
                    return $"[{new string(set.Chars)}]";
                case AlwaysTrue:
                    return "&&";
                case AlwaysFalse:
                    return "!!";
                default:
                    return r.Name;
            }
        }

        public static bool HasNode(this Rule r)
        {
            switch (r)
            {
                case NodeRule nr:
                    return true;
                case Sequence seq:
                    return seq.Rules.Any(HasNode);
                case Choice ch:
                    return ch.Rules.Any(HasNode);
                case Optional opt:
                    return opt.Rule.HasNode();
                case ZeroOrMore z:
                    return z.Rule.HasNode();
                case RecursiveRule rec:
                    return rec.RuleFunc().HasNode();
                default:
                    return false;
            }
        }

        public static IEnumerable<Rule> ChildrenWithNodes(this Rule r, bool top = false)
        {
            switch (r)
            {
                case NodeRule nr:
                    return top ? nr.Rule.ChildrenWithNodes() : new[] { r };
                case Sequence seq:
                    return seq.Rules.SelectMany(r => r.ChildrenWithNodes());
                case Choice ch:
                    return ch.Rules.SelectMany(r => r.ChildrenWithNodes());
                case Optional opt:
                    return opt.Rule.ChildrenWithNodes();
                case ZeroOrMore z:
                    return z.Rule.ChildrenWithNodes();
                case RecursiveRule rec:
                    return rec.RuleFunc().ChildrenWithNodes();
                default:
                    return Enumerable.Empty<Rule>();
            }
        }

        public static Rule? OnlyNodes(this Rule r)
        {            
            switch (r)
            {
                case NodeRule nr:
                    return nr;
                case Sequence seq:
                    {
                        var tmp = seq.Rules.Select(r => r.OnlyNodes()).Where(x => x != null).ToList();
                        if (tmp.Count > 0)
                            return new Sequence(tmp!, r.Name);
                        break;
                    }
                case Choice ch:
                    {
                        var tmp = ch.Rules.Select(r => r.OnlyNodes()).Where(x => x != null).ToList();
                       
                        if (tmp.Count > 0)
                            return new Choice(tmp!, r.Name);
                        break;
                    }
                case Optional opt:
                    {
                        var tmp = opt.Rule.OnlyNodes();
                        if (tmp != null)
                            return new Optional(tmp, r.Name);
                        break;
                    }
                case ZeroOrMore z:
                    {
                        var tmp = z.Rule.OnlyNodes();
                        if (tmp != null)
                            return new ZeroOrMore(tmp, r.Name);
                        break;
                    }
                case RecursiveRule rec:
                    {
                        var tmp = rec.RuleFunc().OnlyNodes();
                        if (tmp != null)
                            return tmp.WithName(r.Name);
                        break;
                    }
            }
            return null;
        }

        public static Rule Simplify(this Rule r)
        {
            switch (r)
            {
                case Sequence seq:
                    {
                        var tmp = seq.Rules.SelectMany(
                            r =>
                            {
                                var tmp = r.Simplify();
                                if (tmp is Sequence seq)
                                    return seq.Rules;
                                return new[] { tmp };
                            }).ToList();
                        Debug.Assert(tmp.Count > 0);
                        Debug.Assert(!tmp.Any(t => t is Sequence));
                        if (tmp.Count == 1)
                            return tmp[0];
                        return new Sequence(tmp, r.Name);
                    }

                case Choice ch:
                    {
                        var tmp = ch.Rules.SelectMany(
                            r =>
                            {
                                var tmp = r.Simplify();
                                if (tmp is Choice ch)
                                    return ch.Rules;
                                return new[] { tmp };
                            }).ToList();
                        Debug.Assert(tmp.Count > 0);
                        Debug.Assert(!tmp.Any(t => t is Choice));
                        if (tmp.Count == 1)
                            return tmp[0];
                        return new Choice(tmp, r.Name);
                    }

                case Optional opt:
                    return new Optional(opt.Rule.Simplify(), r.Name);

                case ZeroOrMore z:
                    return new ZeroOrMore(z.Rule.Simplify(), r.Name);
            }
            return r;
        }
    }
}
