using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parakeet;
using Parakeet.Demos.Plato;
using Plato.Compiler.Ast;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;
using Plato.Compiler.Vsg;
using Ptarmigan.Utils;

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
        public Logger Logger { get; }
        public bool CompletedCompilation { get; set; }
        public bool ParsingSuccess { get; set; }
        public IReadOnlyList<Parser> Parsers { get; set; }
        public IReadOnlyList<AstNode> Trees { get; set; }
        public SymbolResolver SymbolResolver { get; set; }
        public IReadOnlyList<AstTypeDeclaration> TypeDeclarations { get; set; }

        public IEnumerable<TypeDefinition> AllTypeAndLibraryDefinitions => TypeDefinitionsByName.Values
            .Concat(LibraryDefinitionsByName.Values);

        public Dictionary<string, TypeDefinition> TypeDefinitionsByName { get; } = new Dictionary<string, TypeDefinition>();
        public Dictionary<string, TypeDefinition> LibraryDefinitionsByName { get; } = new Dictionary<string, TypeDefinition>();
        public IReadOnlyList<FunctionDefinition> FunctionDefinitions { get; set; } 
        public IReadOnlyList<TypeResolver> TypeResolvers { get; set; }

        public Dictionary<Expression, TypeExpression> ExpressionTypes { get; } = new Dictionary<Expression, TypeExpression>();
        public Dictionary<string, ReifiedType> ReifiedTypes { get; set; }
        public Dictionary<string, List<ReifiedFunction>> ReifiedFunctionsByName { get; set; }

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
                var typeDefs = SymbolResolver.CreateTypeDefs(TypeDeclarations)
                    .Concat(PrimitiveTypeDefinitions.AllPrimitives);

                foreach (var td in typeDefs)
                {
                    if (td.IsLibrary())
                    {
                        LibraryDefinitionsByName.Add(td.Name, td);
                    }
                    else
                    {
                        TypeDefinitionsByName.Add(td.Name, td);
                    }
                }

                Log($"Found {SymbolResolver.Errors.Count} symbol resolution errors");
                LogResolutionErrors(SymbolResolver.Errors);

                if (SymbolResolver.Errors.Count > 0)
                {
                    Log("Halting further computation");
                    return;
                }

                Log("Gathering function definitions");
                FunctionDefinitions = TypeDefinitionsByName.Values.SelectMany(td => td.Functions)
                    .Concat(LibraryDefinitionsByName.Values.SelectMany(ld => ld.Functions)).ToList();
                Log($"Found {FunctionDefinitions.Count} functions");

                Log("Checking semantics");
                CheckSemantics();

                Log("Creating Reified Types");
                ReifiedTypes = TypeDefinitionsByName.Values.Where(td => td.IsConcreteType())
                    .ToDictionary(td => td.Name, td => new ReifiedType(td));

                Log($"Found {ReifiedTypes.Count} types");
                Log($"Created a total of {ReifiedTypes.Values.Sum(rt => rt.Functions.Count)} reified functions");

                Log("Adding library functions to reified types");
                AddLibraryFunctionsToReifiedTypes();

                Log("Grouping Reified functions by name for faster type resolution");
                ReifiedFunctionsByName = ReifiedTypes.Values.SelectMany(rt => rt.Functions)
                    .ToDictionaryOfLists(rf => rf.Name);

                //Log("Reified types");
                //WriteReifiedTypes();

                Log("Creating function analysis");
                var sb = new StringBuilder();
                sb.Append("Function Analysis");
                foreach (var f in FunctionDefinitions)
                {
                    var fa = new FunctionAnalysis(this, f);
                    fa.BuildAnalysisOuput(sb);
                }
                Logger.Log(sb.ToString());                

                /*
                Log("Creating resolvers");
                TypeResolvers = FunctionDefinitions
                    // TODO: TEMP: skip concept function implementations for now.
                    .Where(fd => !fd.OwnerType.IsConcept())
                    .Select(fd => new TypeResolver(this, fd))
                    .ToList();
                Log($"Found {TypeResolvers.Count} resolvers");
                Log($"Applied type to {ExpressionTypes.Count} expressions");
                */

                //Log("Generating Visual Syntax Graphs");
                //Graphs.Clear();
                //foreach (var f in Functions) 
                //    Graphs.Add(new VsgBuilder().ToVsg(f));

                Log("Outputting errors and warnings");
                foreach (var se in SemanticErrors)
                    Log("Semantic Error   : " + se);
                foreach (var ie in InternalErrors)
                    Log("Internal Errors  : " + ie);
                if (DisplayWarnings)
                    foreach (var sw in SemanticWarnings)
                        Log("Semantic Warning : " + sw);

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

        public void ResolveExpressionTypes()
        {
            foreach (var rt in ReifiedTypes.Values)
            {
                foreach (var rf in rt.Functions)
                {
                    //  So: do I process the type of every version of every expression?
                    // That would be a lot expressions, and a lot of types. 
                    // The thing is that the parameter references would be different for each one
                    // I don't know what to do next. 
                    // I mean: I think I have to make whole-sale copies of each function to properly 
                    // I could. I just have to be efficient. 
                    
                    // I could find a way to
                    // I don't actually have to type-check every variation.
                    // The variations are simply useful for find ing the correct overload, and computing 
                    // return types. 
                }
            }
        }

        public void AddLibraryFunctionsToReifiedTypes()
        {
            foreach (var library in LibraryDefinitionsByName.Values)
            {
                foreach (var f in library.Functions)
                {
                    if (f.Parameters.Count == 0)
                        continue;

                    var firstParamType = f.Parameters[0].Type?.Definition;

                    Verifier.AssertNotNull(firstParamType, $"First parameter type of {f}");

                    if (firstParamType.IsConcreteType())
                    {
                        var rt = ReifiedTypes[firstParamType.Name];

                        rt.AddConcreteTypeLibraryFunction(library, f);
                    }
                    else if (firstParamType.IsConcept())
                    {
                        foreach (var rt in ReifiedTypes.Values)
                        {
                            if (rt.Type.Implements(firstParamType))
                            {
                                rt.AddConceptLibraryFunction(library, f);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"First parameter type in function {f} not a concrete type or concept");
                    }
                }
            }
        }

        public void WriteReifiedTypes()
        {
            foreach (var kv in ReifiedTypes)
            {
                var rt = kv.Value;
                Log($"= Reified type {rt.Name}");
                var funcGroups = rt.Functions.GroupBy(f => f.OwnerType);
                foreach (var group in funcGroups)
                {
                    Log($" = Reified functions for group {group.Key}");
                    foreach (var f in group)
                    {
                        Log($"    {f}");
                    }
                }
            }
        }

        public TypeExpression GetType(Expression expr)
            => ExpressionTypes.TryGetValue(expr, out var r) ? r : null;

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
            /*
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
            }*/

        }

        public TypeDefinition GetTypeDefinition(string name)
        {
            return TypeDefinitionsByName[name];
        }

        public TypeExpression GetTypeExpression(string name)
        {
            return GetTypeDefinition(name).ToTypeExpression();
        }
    }
}