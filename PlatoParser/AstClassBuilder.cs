using PlatoAstWriter;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static CodeBuilder OutputAstField(CodeBuilder cb, List<string> fieldNames, Rule r, int index, int child)
        {
            var fieldName = fieldNames[index];
            var cnt = 0;
            for (var i=0; i < index; ++i)
            {
                if (fieldNames[i] == fieldName)
                {
                    cnt++;
                }
            }

            // In case a field name is used multiple times. 
            if (cnt > 0)
                fieldName = $"{fieldName}_{cnt}";

            if (r is NodeRule nr)
                return cb.WriteLine($"public {nr.Name} {fieldName} => Children[{child}] as {nr.Name};");
            
            if (r is Sequence)
                return cb.WriteLine($"public AstSequence {fieldName} => Children[{child}] as AstSequence;");
            
            if (r is Choice)
                return cb.WriteLine($"public AstChoice {fieldName} => Children[{child}] as AstChoice;");
            
            if (r is Optional opt)
                return cb.WriteLine($"public AstOptional<{opt.Rule.AstTypeName()}> {fieldName} => Children[{child}] as AstOptional<{opt.Rule.AstTypeName()}>;");
            
            if (r is ZeroOrMore z)
                return cb.WriteLine($"public AstZeroOrMore<{z.Rule.AstTypeName()}> {fieldName} => Children[{child}] as AstZeroOrMore<{z.Rule.AstTypeName()}>;");

            if (r is RecursiveRule rr)
                return OutputAstField(cb, fieldNames, rr.RuleFunc(), index, child);

            throw new NotImplementedException($"Unrecognized rule type {r}");
        }

        public static int NumAstChildren(Rule r)
        {
            var body = r.Body()?.OnlyNodes()?.Simplify();
            if (body == null)
                return 0;
            if (body is Sequence sequence)
                return sequence.Count;
            if (body is Choice choice)
                return choice.Count;
            return 1;
        }
        
        public static string AstFieldName(this Rule r)
        {
            if (r is NodeRule nr)
                return nr.Name;
            if (r is ZeroOrMore z)
                return "ZeroOrMore" + z.Rule.AstFieldName();
            if (r is Sequence s)
                return string.IsNullOrWhiteSpace(s.Name) ? "Sequence" : s.Name;
            if (r is Choice c)
                return string.IsNullOrWhiteSpace(c.Name) ? "Choice" : c.Name;
            if (r is Optional opt)
                return opt.Rule.AstFieldName();
            if (r is RecursiveRule rec)
                return rec.RuleFunc().AstFieldName();
            return "Node";

        }

        public static CodeBuilder OutputAstClass(CodeBuilder cb, Rule r)
        {
            if (!(r is NodeRule))
                return cb;

            var body = r.Body()?.OnlyNodes()?.Simplify();

            cb = cb.WriteLine($"// Original Rule: {r.Body().ToDefinition()}");
            cb = cb.WriteLine($"// Only Nodes: {body?.ToDefinition()}");
            cb = cb.Write($"public class {r.Name}");
           
            if (body is Sequence)
            {
                cb = cb.WriteLine($" : AstSequence");
            }
            else if (body is Choice)
            {
                cb = cb.WriteLine($" : AstChoice");
            }
            else 
            {
                cb = cb.WriteLine($" : AstNode");
            }

            cb = cb.WriteLine("{").Indent();

            cb = cb.WriteLine($"public {r.Name}(params AstNode[] children) : base(children) {{ }}");
            cb = cb.WriteLine($"public override AstNode Transform(Func<AstNode, AstNode> f) => new {r.Name}(Children.Select(f).ToArray());");
            var index = 0;
            if (body == null)
            {
                cb = cb.WriteLine("// No children");
            }
            else if (body is Sequence sequence)
            {
                var names = sequence.Rules.Select(AstFieldName).ToList();
                foreach (var child in sequence.Rules)
                    cb = OutputAstField(cb, names, child, index, index++);
            }
            else if (body is Choice choice)
            {
                var names = choice.Rules.Select(AstFieldName).ToList();
                foreach (var child in choice.Rules)
                    cb = OutputAstField(cb, names, child, index++, 0);
            }            
            else
            {
                cb = OutputAstField(cb, new List<string>() { body.AstFieldName() }, body, index, index++);
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
                    if (NumAstChildren(nr) == 0)
                    {
                        cb.WriteLine($"case \"{nr.Name}\": return new {nr.Name}(node.Contents);");
                    }
                    else
                    {
                        cb.WriteLine($"case \"{nr.Name}\": return new {nr.Name}(node.Children.Select(ToNode).ToArray());");
                    }
                }
            }
            cb.WriteLine($"default: throw new Exception($\"Unrecognized parse node {{node.Type}}\");");
            cb.Dedent().WriteLine("}");
            cb.Dedent().WriteLine("}");
            cb.Dedent().WriteLine("}");
        }

        public static void OutputAstFile(CodeBuilder cb, string namespaceName, IEnumerable<Rule> rules)
        {
            cb.WriteLine($"// NOTE: Autogenerated file created on {DateTime.Now}. DO NOT EDIT!");
            cb.WriteLine($"using System;");
            cb.WriteLine($"using System.Linq;");
            cb.WriteLine();
            cb.WriteLine($"namespace {namespaceName}");
            cb.WriteLine("{").Indent();
            OutputAstClasses(cb, rules);
            OutputHelpers(cb, rules);
            cb.Dedent().WriteLine("}");
        }
    }
}
