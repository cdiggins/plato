using System;
using System.Collections.Generic;
using System.Linq;
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
                TypeDeclarations = AstTree.GetAllTypes().ToList();
                SymbolResolver.CreateTypeDefs(TypeDeclarations);
                TypeDefs = SymbolResolver.TypeDefs.ToList();
                Operations = new Operations(TypeDefs);
                TypeResolver = new TypeResolver(Operations);
                CheckSemantics();

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

        public void CheckSemantics()
        {
            foreach (var f in Functions)
            {
                foreach (var p in f.Parameters)
                {
                    var refs = p.GetParameterReferences(f).ToList();
                    if (refs.Count == 0)
                        SemanticWarnings.Add($"No references found to {p}");
                }

                foreach (var r in f.Body.GetDescendantSymbols().OfType<RefSymbol>())
                    if (r.Def == null)
                        SemanticErrors.Add($"Could not resolve reference for {r}");
                    
                if (f.IsPartiallyTyped()) 
                    SemanticErrors.Add($"{f} is partially typed");
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

        public SymbolResolver SymbolResolver { get; } = new SymbolResolver();
        public IReadOnlyList<AstTypeDeclaration> TypeDeclarations { get; }
        public Operations Operations { get; }
        public TypeResolver TypeResolver { get; }
        public IReadOnlyList<FunctionSymbol> Functions => TypeResolver.Functions;
        public IReadOnlyList<TypeDefSymbol> TypeDefs { get;}
        public List<string> SemanticErrors { get; } = new List<string>();
        public List<string> SemanticWarnings { get; } = new List<string>();
    }
}