using System;
using Parakeet;

namespace PlatoAst
{
    public class Compilation
    {
        public Compilation(
            string input,
            Rule rule,
            Func<ParserTree, CstNode> cstGenerator,
            Func<CstNode, AstNode> astBuilder)
            : this(new ParserInput(input), rule, cstGenerator, astBuilder)
        {
        }

        public Compilation(
            ParserInput input,
            Rule rule,
            Func<ParserTree, CstNode> cstGenerator,
            Func<CstNode, AstNode> astBuilder)
        {
            Input = input;
            Rule = rule;
            try
            {
                State = Input.Parse(Rule);
                if (State == null)
                {
                    Message = "Compilation failed";
                    return;
                }

                ParseTree = State.Node.ToParseTree();
                CstTree = cstGenerator(ParseTree);
                AstTree = astBuilder(CstTree);
                Success = State.AtEnd();
                if (!Success)
                {
                    Message = "Partial parse happened";
                }
            }
            catch (ParserException pe)
            {
                Exception = pe;
                Message = $"Parsing exception {pe.Message} occurred at {pe.LastValidState}";
            }
            catch (Exception e)
            {
                Exception = e;
                Message = e.Message;
            }

        }

        public Rule Rule { get; }
        public string Message { get; }
        public bool Success { get; }
        public Exception Exception { get; }
        public ParserState State { get; }
        public ParserInput Input { get; }
        public ParserTree ParseTree { get; }
        public CstNode CstTree { get; }
        public AstNode AstTree { get; }
    }
}