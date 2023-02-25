using PlatoAstWriter;

namespace PlatoParser
{
    public static class AstClassBuilder
    {
        public static string AstTypeName(this Rule r)
        {
            if (r is NodeRule nr)
                return nr.Name;
            return "AstNode";
        }

        public static CodeBuilder OutputAstField(CodeBuilder cb, Rule r, int index, int child)
        {            
            if (r is NodeRule nr)
                return cb.WriteLine($"public {nr.Name} Node{index} => Nodes[{child}] as {nr.Name};");
            
            if (r is Sequence)
                return cb.WriteLine($"public Sequence Node{index} => Nodes[{child}] as Sequence;");
            
            if (r is Choice)
                return cb.WriteLine($"public Choice Node{index} => Nodes[{child}] as Choice;");
            
            if (r is Optional opt)
                return cb.WriteLine($"public {opt.Rule.AstTypeName()} Node{index} => Nodes[{child}] as {opt.Rule.AstTypeName()};");
            
            if (r is ZeroOrMore z)
                return cb.WriteLine($"public Star<{z.Rule.AstTypeName()}> Node{index} => Nodes[{child}];");

            if (r is RecursiveRule rr)
                return OutputAstField(cb, rr.RuleFunc(), index, child);

            throw new NotImplementedException($"Unrecognized rule type {r}");
        }

        public static CodeBuilder OutputAstClass(CodeBuilder cb, Rule r)
        {
            if (!(r is NodeRule))
                return cb;

            var body = r.Body()?.OnlyNodes()?.Simplify();

            cb = cb.WriteLine($"// Original Rule: {r.Body().ToDefinition()}");
            cb = cb.WriteLine($"// Only Nodes: {body?.ToDefinition()}");
            cb = cb.WriteLine($"public class {r.Name} : AstNode");
            cb = cb.WriteLine("{").Indent();
            cb = cb.WriteLine($"public {r.Name}(params AstNode[] nodes) : base(nodes) {{ }}");
            var index = 0;
            if (body == null)
            {
                cb = cb.WriteLine("// No children");
            }
            else if (body is Sequence sequence)
            {
                foreach (var child in sequence.Rules)
                    cb = OutputAstField(cb, child, index, index++);
            }
            else if (body is Choice choice)
            {
                foreach (var child in choice.Rules)
                    cb = OutputAstField(cb, child, index++, 0);
            }
            else
            {
                cb = OutputAstField(cb, body, index, index++);
            }
            cb = cb.Dedent().WriteLine("}");            
            return cb.WriteLine();
        }
    
        public static void OutputAstClasses(CodeBuilder cb, IEnumerable<Rule> rules)
        {
            foreach (var r in rules)
            {
                OutputAstClass(cb, r);
            }
        }
    }
}
