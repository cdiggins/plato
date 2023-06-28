using Parakeet;
using Parakeet.Demos.CSharp;
using PlatoAst;

namespace PlatoWinFormsEditor;

public class Compilation
{
    public Compilation(string input, Rule rule)
    {
        Input = new ParserInput(input);
        Rule = rule;
        try
        {
            State = Input.Parse(Rule);
            if (State == null)
            {
                Message = "Compilation failed";
                return;
            }

            Tree = State.Node.ToParseTree();
            CstTree = CstNodeFactory.Create(Tree);
            AstTree = new AstFromCst().ToAst(CstTree);
            Success = State.AtEnd();
            if (!Success)
            {
                Message = "Partial parse happened";
            }
        }
        catch (ParserException pe)
        {
            Exception = pe;
            Message = $"Parsing exception {pe.Message} occured at {pe.LastValidState}";
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
    public ParserTree Tree { get; }
    public CstNode CstTree { get; }
    public AstNode AstTree { get; }
}