using System;
using System.Collections.Generic;
using System.Linq;
using Parakeet;

namespace Plato.Compiler
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

                Success = State.AtEnd() && SemanticErrors.Count == 0 && InternalErrors.Count == 0;

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
        public List<string> InternalErrors { get; } = new List<string>();
    }
}