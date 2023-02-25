using PlatoAstWriter;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

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
                return cb.WriteLine($"public {nr.Name} Node{index} => Children[{child}] as {nr.Name};");
            
            if (r is Sequence)
                return cb.WriteLine($"public AstSequence Node{index} => Children[{child}] as Sequence;");
            
            if (r is Choice)
                return cb.WriteLine($"public AstChoice Node{index} => Children[{child}] as Choice;");
            
            if (r is Optional opt)
                return cb.WriteLine($"public {opt.Rule.AstTypeName()} Node{index} => Children[{child}] as {opt.Rule.AstTypeName()};");
            
            if (r is ZeroOrMore z)
                return cb.WriteLine($"public AstStar<{z.Rule.AstTypeName()}> Node{index} => Children[{child}];");

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

            cb = cb.WriteLine($"public {r.Name}(params AstNode[] children) : base(children) {{ }}");
            cb = cb.WriteLine($"public override AstNode Transform(Func<AstNode, AstNode> f) => new {r.Name}(Children.Select(f).ToList()));");
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

        public static void OutputHelpers(CodeBuilder cb, IEnumerable<Rule> rules)
        {
            cb.WriteLine("public static class AstNodeHelpers");
            cb.WriteLine("{").Indent();
            cb.WriteLine("public static AstNode ToNode(this ParseTree node)");
            cb.WriteLine("{").Indent();
            cb.WriteLine("switch (node.Type)");
            cb.WriteLine("{").Indent();
            foreach (var r in rules)
            {
                if (r is NodeRule nr)
                {
                    cb.WriteLine($"case \"{nr.Name}\": return new {nr.Name}(node.Children.Select(ToNode).ToArray());");
                }
            }
            cb.WriteLine($"default: => throw new Exception(\"Unrecognized parse node {{node.Type}}\");");
            cb.Dedent();
            cb.Dedent().WriteLine("}");
            cb.Dedent().WriteLine("}");
            cb.Dedent().WriteLine("}");
        }

        public static void OutputAstFile(CodeBuilder cb, string namespaceName, IEnumerable<Rule> rules)
        {
            cb.WriteLine($"namespace {namespaceName};");
            OutputAstClasses(cb, rules);
            OutputHelpers(cb, rules);
        }
    }
}
