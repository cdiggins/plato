using System;
using System.Collections.Generic;
using System.Linq;

namespace Plato.Compiler
{
    public class Compiler
    {
        public Compiler(IEnumerable<Parser> parsers, Logger logger)
        {
            Logger = logger;

            Log("Creating compiler");
            Parsers = parsers.ToList();
            ParsingSuccess = Parsers.All(p => p?.Success == true);
            if (!ParsingSuccess)
            {
                Log("Parsing was not successful");
            }

            Log("Gathering AST trees");
            Trees = Parsers.Select(p => p.AstTree).ToList();

            Log("Gathering type declarations");
            TypeDeclarations = Trees.SelectMany(tree => tree.GetAllTypes()).ToList();

            Log("Creating type definitions");
            try
            {
                SymbolResolver.CreateTypeDefs(TypeDeclarations);
                TypeDefs = SymbolResolver.TypeDefs.ToList();

                Log("Creating operations");
                Operations = new Operations(TypeDefs);

                Log("Type resolution");
                TypeResolver = new TypeResolver(Operations);

                Log("Checking semantics");
                CheckSemantics();

                foreach (var se in SemanticErrors)
                    Log("Semantic Error   : " + se);
                foreach (var sw in SemanticWarnings)
                    Log("Semantic Warning : " + sw);
                foreach (var ie in InternalErrors)
                    Log("Internal Errors  : " + ie);
            }
            catch (AstException ae)
            {
                Log($"Exception caught at {ae.Node}: " + ae.Message);
            }
            catch (Exception e)
            {
                Log("Exception caught: " + e.Message);
            }
        }

        public void Log(string message)
            => Logger.Log(message);

        public Logger Logger { get; }
        public bool ParsingSuccess { get; }
        public IReadOnlyList<Parser> Parsers { get; }
        public IReadOnlyList<AstNode> Trees { get; }
        public SymbolResolver SymbolResolver { get; } = new SymbolResolver();
        public IReadOnlyList<AstTypeDeclaration> TypeDeclarations { get; }
        public Operations Operations { get; }
        public TypeResolver TypeResolver { get; }
        public IReadOnlyList<FunctionSymbol> Functions => TypeResolver.Functions;
        public IReadOnlyList<TypeDefSymbol> TypeDefs { get; }

        public List<string> SemanticErrors { get; } = new List<string>();
        public List<string> SemanticWarnings { get; } = new List<string>();
        public List<string> InternalErrors { get; } = new List<string>();

        public void CheckSemantics()
        {
            foreach (var f in Functions)
            {
                foreach (var p in f.Parameters)
                    if (!p.GetParameterReferences(f).Any())
                        SemanticWarnings.Add($"No references found to {p}");

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
    }
}