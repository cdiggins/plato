using System;
using System.Text;

namespace PlatoAstWriter
{
    //[Mutable]
    public class CodeBuilder
    {
        public StringBuilder sb { get; } = new StringBuilder();
        public bool AtNewLine { get; private set; } 
        public int IndentLevel { get; private set; }
        public CodeBuilder Indent()
        {
            IndentLevel++;
            return this;
        }
        public CodeBuilder Dedent()
        {
            IndentLevel--;
            return this;
        }
        public string Indentation()
        {
            return new string(' ', IndentLevel * 4);
        }
        public CodeBuilder Write(string s)
        {
            if (string.IsNullOrEmpty(s))
                return this;
            if (AtNewLine)
            {
                sb.Append(Indentation());
                AtNewLine = false;
            }
            sb.Append(s);
            return this;
        }
        public CodeBuilder WriteLine()
        {
            AtNewLine = true;
            sb.AppendLine();
            return this;
        }
        public CodeBuilder WriteLine(string s)
        {
            return Write(s).WriteLine();
        }
        public CodeBuilder Keyword(string s)
        {
            return Write(s).Write(" ");
        }
        public CodeBuilder Parenthesize(Func<CodeBuilder, CodeBuilder> f)
        {
            return f(Write("(")).Write(")");
        }
        public CodeBuilder Brace(Func<CodeBuilder, CodeBuilder> f)
        {
            return f(WriteLine("{").Indent().WriteLine()).Dedent().WriteLine("}");
        }
        public CodeBuilder WriteIf(bool condition, Func<CodeBuilder, CodeBuilder> f)
        {
            return condition ? f(this) : this;
        }
        public override string ToString()
        {
            return sb.ToString();
        }
    }
}