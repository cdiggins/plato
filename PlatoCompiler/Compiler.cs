using System;
using System.Collections.Generic;
using System.Linq;
using Parakeet;
using Parakeet.Demos.Plato;
using Plato.Compiler.Ast;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;
using Plato.Compiler.Vsg;
using Type = Plato.Compiler.Types.Type;

namespace Plato.Compiler
{
    public class Compiler
    {
        public Compiler(
            CstNodeFactory cstNodeFactory,
            AstNodeFactory astNodeFactory,
            Logger logger)
        {
            CstNodeFactory = cstNodeFactory;
            AstNodeFactory = astNodeFactory;

            Logger = logger;
            Log("Creating compiler");
        }

        public bool DisplayWarnings = false;
        public AstNodeFactory AstNodeFactory { get; }
        public CstNodeFactory CstNodeFactory { get; }
        public TypeFactory TypeFactory { get; set; }
        public Logger Logger { get; }
        public bool CompletedCompilation { get; set; }
        public bool ParsingSuccess { get; set; }
        public IReadOnlyList<Parser> Parsers { get; set; }
        public IReadOnlyList<AstNode> Trees { get; set; }
        public SymbolResolver SymbolResolver { get; set; }
        public IReadOnlyList<AstTypeDeclaration> TypeDeclarations { get; set; }
        public IReadOnlyList<FunctionDefinition> Functions { get; } 
        public IReadOnlyList<TypeDefinition> TypeDefs { get; set; }
        public Dictionary<FunctionDefinition, TypeResolver> Resolvers { get; } = new Dictionary<FunctionDefinition, TypeResolver>();
        public Dictionary<Expression, Type> Types { get; } = new Dictionary<Expression, Type>();

        public List<string> SemanticErrors { get; } = new List<string>();
        public List<string> SemanticWarnings { get; } = new List<string>();
        public List<string> InternalErrors { get; } = new List<string>();

        public List<VisualSyntaxGraph> Graphs { get; } = new List<VisualSyntaxGraph>();

        public void Compile(IReadOnlyList<Parser> parsers) 
        {
            Log("Initializing Compiler");
            CompletedCompilation = false;
            SemanticErrors.Clear();
            SemanticWarnings.Clear();
            InternalErrors.Clear();

            Log("Gathering parsers");
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

            try
            {
                Log("Creating symbol resolver");
                SymbolResolver = new SymbolResolver(Logger);

                Log("Creating type definitions");
                TypeDefs = SymbolResolver.CreateTypeDefs(TypeDeclarations).ToList();

                Log($"Found {SymbolResolver.Errors.Count} symbol resolution errors");
                LogResolutionErrors(SymbolResolver.Errors);

                if (SymbolResolver.Errors.Count > 0)
                {
                    Log("Halting further computation");
                    return;
                }

                Log("Creating type factory");
                TypeFactory = new TypeFactory(TypeDefs);
                Log(TypeFactory);

                foreach (var f in TypeFactory.FunctionDefinitions)
                {
                    Log($"Creating type resolver for {f}");
                    var tr = new TypeResolver(TypeFactory, null, null, null, f);
                    Resolvers.Add(f, tr);

                    Log($"Gathering expressions types for {f}");
                    var et = tr.ExpressionTypes;

                    while (et != null)
                    {
                        Types.Add(et.Expression, et.Type);
                        et = et.Parent;
                    }
                }

                // TODO: get the functions 

                /*
                Log("Creating operations");
                // TODO: this can be removed I think.
                Operations = new Operations(TypeDefs);

                Log("Type resolution");
                TypeResolver = new TypeResolver(Operations, SymbolResolver);

                Log($"Found {TypeResolver.Errors.Count} type resolution errors");
                LogResolutionErrors(TypeResolver.Errors);

                Log("Checking semantics");
                CheckSemantics();

                Log("Generating Visual Syntax Graphs");
                Graphs.Clear();
                foreach (var f in Functions) 
                    Graphs.Add(new VsgBuilder().ToVsg(f));

                foreach (var se in SemanticErrors)
                    Log("Semantic Error   : " + se);
                foreach (var ie in InternalErrors)
                    Log("Internal Errors  : " + ie);
                if (DisplayWarnings)
                    foreach (var sw in SemanticWarnings)
                        Log("Semantic Warning : " + sw);
                */

                CompletedCompilation = true;
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

        public Type GetType(Expression expr)
            => Types.TryGetValue(expr, out var r) ? r : null;

        public void Log(TypeFactory atr)
        {
            Log($"Type Factory");
            Log($"Found {atr.Concepts.Count} Concepts");
            foreach (var c in atr.Concepts)
            {
                Log($"== Concept {c.Name} ==");

                Log($"= Functions =");
                foreach (var f in c.Functions)
                {
                    Log($"{f}");
                }
            }

            Log($"= Type Variables =");
            foreach (var tv in atr.TypeVariables)
            {
                Log($"{tv}");
            }

            Log($"= Type References =");
            foreach (var tr in atr.TypeReferences)
            {
                Log($"{tr}");
            }
        }

        public void LogResolutionErrors(IEnumerable<SymbolResolver.ResolutionError> resolutionErrors)
        {
            foreach (var error in resolutionErrors)
            {
                var pos = GetParserTreeNode(error.Node);
                Log($"Symbol resolution error: {error.Message}");
                if (pos?.Node != null)
                {
                    Log(pos.Node.Range.End.Input.File);
                    Log(pos.Node.Range.End.CurrentLine);
                    Log(pos.Node.Range.End.Indicator);
                }
                else
                {
                    Log("Unknown location.");
                }
            }
        }

        public void Log(string message)
            => Logger.Log(message);

        public CstNode GetCstNode(AstNode node)
        {
            if (node == null) return null;
            if (AstNodeFactory.Lookup.ContainsKey(node))
                return AstNodeFactory.Lookup[node];
            return null;
        }

        public ParserTreeNode GetParserTreeNode(AstNode node)
        {
            if (node == null) return null;
            return GetParserTreeNode(GetCstNode(node));
        }

        public ParserTreeNode GetParserTreeNode(CstNode node)
        {
            if (node == null) return null;
            if (CstNodeFactory.Lookup.ContainsKey(node))
                return CstNodeFactory.Lookup[node];
            return null;
        }

        public void CheckSemantics()
        {
            foreach (var f in Functions)
            {
                foreach (var p in f.Parameters)
                    if (!p.GetParameterReferences(f).Any())
                        SemanticWarnings.Add($"No references found to {p}");

                foreach (var r in f.Body.GetExpressionTree().OfType<Reference>())
                    if (r.Definition == null)
                        SemanticErrors.Add($"Could not resolve reference for {r}");

                if (f.IsPartiallyTyped())
                    SemanticWarnings.Add($"{f} is partially typed");
            }

            foreach (var t in TypeDefs)
            {
                foreach (var t2 in t.Implements)
                {
                    if (t2 == null)
                        SemanticErrors.Add($"One of the implemented types of {t} was not resolved");
                    else if (t2.Definition?.Kind != TypeKind.Concept)
                        SemanticErrors.Add($"Only concepts can be implemented. Instead {t} implements {t2}");
                }

                foreach (var t2 in t.Inherits)
                {
                    if (t2 == null)
                        SemanticErrors.Add($"One of the inherited types of {t} was not resolved");
                    else if (t2.Definition?.Kind != TypeKind.Concept)
                        InternalErrors.Add($"Only concepts can be inherited. Instead {t} inherits {t2}");
                }
                    
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