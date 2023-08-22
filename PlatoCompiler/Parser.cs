using System;
using System.Collections.Generic;
using System.Linq;
using Parakeet;
using Plato.Compiler.Ast;

namespace Plato.Compiler
{
    public class Parser
    {
        public Parser(
            string fileName,
            ParserInput input,
            Rule rule,
            Rule tokenizerRule,
            Compiler compiler)
        {
            Compiler = compiler;
            Log($"Starting compiling {fileName} at {DateTime.Now}");
            Input = input;
            TokenizerRule = tokenizerRule;
            Rule = rule;
            try
            {
                Log($"Starting to parse {Input.LineToChar.Count} lines containing {Input.Text.Length} characters");

                Log($"Tokenization phase");
                TokenizerState = Input.Parse(TokenizerRule);
                if (!TokenizerState.AtEnd())
                    Log($"Partially completed tokenize {State.Position}/{State.Input.Length}");
                else
                    Log($"Completed tokenization");
                TokenNodes = TokenizerState.AllNodes().ToList();

                Log($"Starting main parse");
                State = Input.Parse(Rule);
                if (!State.AtEnd())
                    Log($"Partially completed parsing {State.Position}/{State.Input.Length}");
                else 
                    Log($"Completed parsing");

                if (State == null)
                    throw new Exception("Unrecoverable parser failure");

                Log($"Gathering parse errors");
                foreach (var error in State.AllErrors())
                    Log(error);
                Log($"Found {ParsingErrors.Count} errors");

                Log($"Gathering parse nodes");
                Nodes = State.Node?.AllNodesReversed().ToList() ?? new List<ParserNode>();
                Log($"Found {Nodes.Count} nodes");

                Log($"Creating parse tree");
                ParseTreeNode = State.Node?.ToParseTree();
                
                Log($"Creating Concrete Syntax Tree (CST)");
                CstTree = Compiler.CstNodeFactory.Create(ParseTreeNode);

                Log($"Creating Abstract Syntax Tree (AST)");
                AstTree = Compiler.AstNodeFactory.ToAst(CstTree);

                Success = State.AtEnd() && ParsingErrors.Count == 0;
                    
                if (Success)
                    Log($"Completed all steps, result is successful");
                else
                    Log($"Completed all steps, result was not successful");
            }
            catch (ParserException pe)
            {
                Exception = pe;
                Log($"Parsing exception {pe.Message} occurred at {pe.LastValidState}");
            }
            catch (Exception e)
            {
                Exception = e;
                Log($"Exception: {e}");
            }
        }

        public void Log(ParserError e)
        {
            var msg = $"Parsing Error at {e.State}. Expected rule {e.Expected}, parent state is {e.ParentState}, message is {e.Message}";
            Log(msg);
            Log(e.State.CurrentLine);
            Log(e.State.Indicator);
            ParsingErrors.Add(e);
        }

        public void Log(string message)
            => Logger.Log(message);

        public Rule Rule { get; }
        public Rule TokenizerRule { get; }
        public bool Success { get; }
        public Exception Exception { get; }
        public ParserState State { get; }
        public ParserState TokenizerState { get; }
        public ParserInput Input { get; }
        public ParserTreeNode ParseTreeNode { get; }
        public IReadOnlyList<ParserNode> TokenNodes { get; }
        public IReadOnlyList<ParserNode> Nodes { get; }
        public CstNode CstTree { get; }
        public AstNode AstTree { get; }
        public Compiler Compiler { get; }
        public Logger Logger => Compiler.Logger;

        public List<string> Diagnostics { get; } = new List<string>();
        public List<ParserError> ParsingErrors { get; } = new List<ParserError>();

    }
}