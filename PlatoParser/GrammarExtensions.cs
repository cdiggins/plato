using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

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
                case AnyCharRule ac:
                    return $".";
                case NotAt not:
                    return $"!({not.Rule.ToDefinition()})";
                case At at:
                    return $"&({at.Rule.ToDefinition()})";
                case CharRangeRule range:
                    return $"[{range.Low}..{range.High}]";
                case CharSetRule set:
                    return $"[{new string(set.Chars)}]";
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
                    return seq.Rules.SelectMany(r1 => r1.ChildrenWithNodes());
                case Choice ch:
                    return ch.Rules.SelectMany(r1 => r1.ChildrenWithNodes());
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

        public static Rule OnlyNodes(this Rule r)
        {
            switch (r)
            {
                case NodeRule nr:
                    return nr;
                case Sequence seq:
                    {
                        var tmp = seq.Rules.Select(r1 => r1.OnlyNodes()).Where(x => x != null).ToList();
                        if (tmp.Count > 0)
                            return new Sequence(tmp, r.Name);
                        break;
                    }
                case Choice ch:
                    {
                        var tmp = ch.Rules.Select(r1 => r1.OnlyNodes()).Where(x => x != null).ToList();

                        if (tmp.Count > 0)
                            return new Choice(tmp, r.Name);
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
                            r1 =>
                            {
                                var tmp1 = r1.Simplify();
                                if (tmp1 is Sequence seq1)
                                    return seq1.Rules;
                                return new[] { tmp1 };
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
                            r1 =>
                            {
                                var tmp1 = r1.Simplify();
                                if (tmp1 is Choice ch1)
                                    return ch1.Rules;
                                return new[] { tmp1 };
                            }).ToList();

                        Debug.Assert(tmp.Count > 0);
                        Debug.Assert(!tmp.Any(t => t is Choice));
                        if (tmp.Count == 1)
                            return tmp[0];

                        if (tmp.Count == 2)
                        {
                            // A|A* => A*
                            {
                                if (tmp[1] is ZeroOrMore z && z.Rule.Equals(tmp[0]))
                                    return z;
                            }
                            // A*|A => A*
                            {
                                if (tmp[0] is ZeroOrMore z && z.Rule.Equals(tmp[1]))
                                    return z;
                            }
                            // (A|A+B) => A+B?
                            {
                                if (tmp[1] is Sequence seq && seq.Count == 2 && seq[0].Equals(tmp[0]))
                                    return new Sequence(tmp[0], new Optional(seq[1]));
                            }
                            // (A+B|A) => A+B?
                            {
                                if (tmp[0] is Sequence seq && seq.Count == 2 && seq[0].Equals(tmp[1]))
                                    return new Sequence(tmp[1], new Optional(seq[1]));
                            }
                            // (B|A+B) => A?+B
                            {
                                if (tmp[1] is Sequence seq && seq.Count == 2 && seq[1].Equals(tmp[0]))
                                    return new Sequence(new Optional(tmp[0]), seq[1]);
                            }
                            // (A+B|B) => A?+B
                            {
                                if (tmp[0] is Sequence seq && seq.Count == 2 && seq[1].Equals(tmp[1]))
                                    return new Sequence(new Optional(seq[0]), tmp[1]);
                            }
                        }

                        return new Choice(tmp, r.Name);
                    }

                case Optional opt:
                    {
                        var tmp = new Optional(opt.Rule.Simplify(), r.Name);

                        // A*? => A*
                        if (tmp.Rule is ZeroOrMore)
                            return tmp.Rule;

                        // Look for: (A+(A*))? and transform it to A*
                        // This happens when we parse separated lists 

                        if (!(tmp.Rule is Sequence seq))
                            return tmp;

                        if (seq.Count == 2)
                        {
                            if (seq[1] is ZeroOrMore z1)
                                if (seq[0].Equals(z1.Rule))
                                    return z1;
                        }

                        return tmp;
                    }

                case ZeroOrMore z:
                    return new ZeroOrMore(z.Rule.Simplify(), r.Name);
            }
            return r;
        }
    }
}
