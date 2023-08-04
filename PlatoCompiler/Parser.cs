using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Parakeet;

namespace Plato.Compiler
{
    public class Parser
    {
        public Parser(
            string fileName,
            ParserInput input,
            Rule rule,
            Rule tokenizerRule,
            Func<ParserTree, CstNode> cstGenerator,
            Func<CstNode, AstNode> astBuilder,
            Stopwatch stopWatch)
        {
            StopWatch = stopWatch;
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
                ParseTree = State.Node?.ToParseTree();
                
                Log($"Creating Concrete Syntax Tree (CST)");
                CstTree = cstGenerator(ParseTree);

                Log($"Creating Abstract Syntax Tree (AST)");
                AstTree = astBuilder(CstTree);
                
                /* TODO: this needs to move into a separate compiler class.
                TypeDeclarations = AstTree.GetAllTypes().ToList();
                SymbolResolver.CreateTypeDefs(TypeDeclarations);
                TypeDefs = SymbolResolver.TypeDefs.ToList();
                Operations = new Operations(TypeDefs);
                TypeResolver = new TypeResolver(Operations);
                CheckSemantics();
                */

                // TODO: having parsing errors in the same format would be nice!

                if (InternalErrors.Count > 0)
                    Message += Environment.NewLine + "Internal errors:" + Environment.NewLine +
                               string.Join(Environment.NewLine, InternalErrors);
                
                if (SemanticWarnings.Count > 0)
                    Message += Environment.NewLine + "Semantic warnings:" + Environment.NewLine +
                               string.Join(Environment.NewLine, SemanticWarnings);

                if (SemanticErrors.Count > 0)
                    Message += Environment.NewLine + "Semantic errors:" + Environment.NewLine + 
                               string.Join(Environment.NewLine, SemanticErrors);

                if (!State.AtEnd())
                    Message += Environment.NewLine + "Error: Only partially completed parse";

                Success = State.AtEnd() && ParsingErrors.Count == 0 && SemanticErrors.Count == 0 && InternalErrors.Count == 0;
                Log($"Completed all steps, result is successful: {Success}");
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

        public static string PrettyPrintTimeElapsed(Stopwatch sw)
            => $"{(int)(Math.Floor(sw.Elapsed.TotalMinutes))}:{sw.Elapsed.Seconds:00}.{sw.Elapsed.Milliseconds:000}";

        public void Log(string message)
        {
            var timeStr = PrettyPrintTimeElapsed(StopWatch);
            Diagnostics.Add($"[{timeStr}] {message}");
        }

        public void CheckSemantics()
        {
            foreach (var f in Functions)
            {
                /*
                foreach (var p in f.Parameters)
                {
                    var refs = p.GetParameterReferences(f).ToList();
                    if (refs.Count == 0)
                        SemanticWarnings.Add($"No references found to {p}");
                }
                */

                foreach (var r in f.Body.GetDescendantSymbols().OfType<RefSymbol>())
                    if (r.Def == null)
                        SemanticErrors.Add($"Could not resolve reference for {r}");
                    
                if (f.IsPartiallyTyped())
                    SemanticWarnings.Add($"{f} is partially typed");
            }

            foreach (var t in TypeDefs)
            {
                foreach (var t2 in t.Implements)
                    if (t2.Def?.Kind != TypeKind.Concept)
                        InternalErrors.Add($"Only concepts can be implemented. Instead {t} implements {t2}");

                foreach (var t2 in t.Inherits)
                    if (t2.Def?.Kind != TypeKind.Concept)
                        InternalErrors.Add($"Only concepts can be inherited. Instead {t} inherits {t2}");

                if (t.Kind == TypeKind.Concept)
                {
                    if (t.Implements.Count > 0)
                        InternalErrors.Add("Concepts should not have implements clauses");
                    if (t.Fields.Count > 0)
                        InternalErrors.Add("Concepts should not have fields");
                }
                else if (t.Kind == TypeKind.Library)
                {
                    if (t.Inherits.Count > 0)
                        InternalErrors.Add("Libraries should not be able to inherit");
                    if (t.Implements.Count > 0)
                        InternalErrors.Add("Libraries should not have implements clauses");
                    if (t.Fields.Count > 0)
                        InternalErrors.Add("Libraries should not have fields");
                }
                else if (t.Kind == TypeKind.Type)
                {
                    if (t.Inherits.Count > 0)
                        InternalErrors.Add("Types should not be able to inherit");
                    if (t.Methods.Count > 0)
                        InternalErrors.Add("Types should not have methods");
                }
            }
        }

        public Rule Rule { get; }
        public Rule TokenizerRule { get; }
        public string Message { get; }
        public bool Success { get; }
        public Exception Exception { get; }
        public ParserState State { get; }
        public ParserState TokenizerState { get; }
        public ParserInput Input { get; }
        public ParserTree ParseTree { get; }
        public IReadOnlyList<ParserNode> TokenNodes { get; }
        public IReadOnlyList<ParserNode> Nodes { get; }
        public CstNode CstTree { get; }
        public AstNode AstTree { get; }
        public Stopwatch StopWatch { get; }

        public SymbolResolver SymbolResolver { get; } = new SymbolResolver();
        public IReadOnlyList<AstTypeDeclaration> TypeDeclarations { get; }
        public Operations Operations { get; }
        public TypeResolver TypeResolver { get; }
        public IReadOnlyList<FunctionSymbol> Functions => TypeResolver.Functions;
        public IReadOnlyList<TypeDefSymbol> TypeDefs { get;}

        public List<string> Diagnostics { get; } = new List<string>();
        public List<ParserError> ParsingErrors { get; } = new List<ParserError>();
        public List<string> SemanticErrors { get; } = new List<string>();
        public List<string> SemanticWarnings { get; } = new List<string>();
        public List<string> InternalErrors { get; } = new List<string>();
    }
}