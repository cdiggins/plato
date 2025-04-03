using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Logging;
using Ara3D.Utils;
using Ara3D.Parakeet;
using Plato.AST;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;
using Plato.Compiler.Vsg;
using Plato.Compiler.Analysis;

namespace Plato.Compiler
{
    public class Compilation
    {
        public Compilation(ILogger logger, IEnumerable<AstNode> trees)
        {
            Logger = logger;

            Log("Creating compiler");
            CompletedCompilation = false;
            Trees = trees.ToList();

            if (Trees.Count == 0)
            {
                Log("No nodes provided");
                return;
            }

            Log("Gathering type declarations");
            TypeDeclarations = Trees.SelectMany(tree => tree.GetAllTypes()).ToList();
            
            try
            {
                Log("Creating symbol resolver");
                SymbolFactory = new SymbolFactory(Logger);

                try
                {
                    Log("Creating type definitions");
                    var typeDefs = SymbolFactory.CreateTypeDefs(TypeDeclarations);

                    foreach (var td in typeDefs)
                    {
                        try
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
                        catch (Exception e)
                        {
                            LogSymbolError($"Failed to store type definition {td.Name} for reason {e}.", td);
                        }
                    }
                }
                finally
                {
                    LogResolutionErrors(SymbolFactory.Errors);
                    Log($"Found {SymbolFactory.Errors.Count} symbol resolution errors");
                }

                if (SymbolFactory.Errors.Count > 0)
                {
                    Log("Halting further computation");
                    return;
                }

                Log("Gathering function definitions");
                FunctionDefinitions = TypeDefinitions.SelectMany(td => td.Functions)
                    .Concat(LibraryDefinitionsByName.Values.SelectMany(ld => ld.Functions))
                    .ToList();
                Log($"Found {FunctionDefinitions.Count} functions");

                Log("Checking semantics");
                CheckSemantics();

                Log("Creating Reified Types");
                ReifiedTypes = TypeDefinitions.Where(td => td.IsConcrete())
                    .ToDictionary(td => td.Name, td => new ReifiedType(td));

                Log($"Found {ReifiedTypes.Count} types");
                Log($"Created a total of {ReifiedTypes.Values.Sum(rt => rt.Functions.Count)} reified functions");

                Log("Adding library functions to reified types");
                AddLibraryFunctionsToReifiedTypes();

                Log("Grouping Reified functions by name for faster type resolution");
                ReifiedFunctionsByName = ReifiedTypes.Values.SelectMany(rt => rt.Functions)
                    .ToDictionaryOfLists(rf => rf.Name);

                // ?? 
                Libraries = new LibraryFunctions(this);
                ConcreteTypes = GetConcreteTypes()
                    .Select(c => new ConcreteType(c, Libraries))
                    .ToList();

                Log("Computing the type relations");
                TypeRelations = new TypeRelations(this);                    

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

                Log("Creating function analysis");
                var sb = new StringBuilder();
                foreach (var fd in FunctionDefinitions)
                    GetOrComputeFunctionAnalysis(fd);

                Logger.Log(sb.ToString());

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

        public readonly bool DisplayWarnings = false;
        public ILogger Logger { get; }
        public bool CompletedCompilation { get; }
        public IReadOnlyList<AstNode> Trees { get; }
        public SymbolFactory SymbolFactory { get; }
        public IReadOnlyList<AstTypeDeclaration> TypeDeclarations { get; }
        public TypeRelations TypeRelations { get; }

        public IDictionary<FunctionDef, FunctionAnalysis> FunctionAnalyses { get; } = new Dictionary<FunctionDef, FunctionAnalysis>();
        public IDictionary<FunctionCall, FunctionGroupCallAnalysis> FunctionGroupCallAnalyses { get; } = new Dictionary<FunctionCall, FunctionGroupCallAnalysis>();

        public IEnumerable<ReifiedFunction> ReifiedFunctions =>
            ReifiedFunctionsByName.SelectMany(kv => kv.Value);

        public IEnumerable<TypeDef> AllTypeAndLibraryDefinitions => TypeDefinitions
            .Concat(LibraryDefinitionsByName.Values);

        public Dictionary<string, TypeDef> TypeDefinitionsByName { get; } = new Dictionary<string, TypeDef>();
        public Dictionary<string, TypeDef> LibraryDefinitionsByName { get; } = new Dictionary<string, TypeDef>();
        public IReadOnlyList<FunctionDef> FunctionDefinitions { get; }
        
        public IEnumerable<TypeDef> TypeDefinitions => TypeDefinitionsByName.Values;
        public IEnumerable<Symbol> Symbols => SymbolFactory.SymbolsToNodes.Keys.OrderBy(s => s.Id);

        public Dictionary<string, ReifiedType> ReifiedTypes { get; }
        public Dictionary<string, List<ReifiedFunction>> ReifiedFunctionsByName { get; }

        public List<string> SemanticErrors { get; } = new List<string>();
        public List<string> SemanticWarnings { get; } = new List<string>();
        public List<string> InternalErrors { get; } = new List<string>();

        public IEnumerable<string> Diagnostics => SemanticWarnings.Select(w => $"[WARNING] {w}")
            .Concat(SemanticErrors.Select(e => $"[ERROR] {e}"))
            .Concat(InternalErrors.Select(i => $"[INTERNAL ERROR] {i}"));

        public LibraryFunctions Libraries { get; }
        public IReadOnlyList<ConcreteType> ConcreteTypes { get; }

        public FunctionAnalysis GetOrComputeFunctionAnalysis(FunctionDef fd)
        {
            if (FunctionAnalyses.TryGetValue(fd, out var analysis))
                return analysis;
            var fa = new FunctionAnalysis(this, fd);
            FunctionAnalyses.Add(fd, fa);
            return fa;
        }

        public FunctionGroupCallAnalysis GetOrComputeFunctionGroupCallAnalysis(FunctionAnalysis context, FunctionCall fc)
        {
            if (FunctionGroupCallAnalyses.TryGetValue(fc, out var analysis))
                return analysis;
            var r = new FunctionGroupCallAnalysis(context, fc);
            FunctionGroupCallAnalyses.Add(fc, r);
            return r;
        }

        public void AddLibraryFunctionsToReifiedTypes()
        {
            foreach (var library in LibraryDefinitionsByName.Values)
            {
                foreach (var f in library.Functions)
                {
                    if (f.Parameters.Count == 0)
                        continue;

                    var firstParamType = f.Parameters[0].Type;

                    Verifier.AssertNotNull(firstParamType, $"First parameter type of {f}");

                    if (firstParamType.Def.IsConcrete())
                    {
                        var rt = ReifiedTypes[firstParamType.Name];

                        rt.AddConcreteTypeLibraryFunction(library, f);
                    }
                    else if (firstParamType.Def.IsInterface())
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
            
        // TODO: I have to make these more like Parser Errors 
        public void LogResolutionErrors(IEnumerable<ResolutionError> resolutionErrors)
        {
            foreach (var error in resolutionErrors)
            {
                Log($"Symbol resolution error: {error.Message}");
                LogPosition(error.Node);
            }
        }

        public void Log(string message)
            => Logger.Log(message);
        
        public static ParserTreeNode GetParserTreeNode(AstNode node)
            => !(node.Location is CstNode cstNode) 
                ? null : GetParserTreeNode(cstNode);

        public static ParserTreeNode GetParserTreeNode(CstNode node)
        {
            if (node == null) return null;
            if (node.Location is ParserTreeNode ptn)
                return ptn;
            return node.Children.Select(GetParserTreeNode).FirstOrDefault();
        }

        public void CheckSemantics()
        {
            foreach (var f in FunctionDefinitions)
            {
                foreach (var p in f.Parameters)
                    if (!p.GetParameterReferences(f).Any())
                        SemanticWarnings.Add($"No references found to {p}");

                foreach (var r in f.Body.GetSymbolTree().OfType<RefSymbol>())
                    if (r.Def == null)
                        SemanticErrors.Add($"Could not resolve reference for {r}");

                if (f.IsPartiallyTyped())
                    SemanticWarnings.Add($"{f} is partially typed");
            }

            foreach (var t in TypeDefinitions)
            {
                foreach (var t2 in t.Implements)
                {
                    if (t2 == null)
                        SemanticErrors.Add($"One of the implemented types of {t} was not resolved");
                    else if (t2.Def?.Kind != TypeKind.Interface)
                        SemanticErrors.Add($"Only concepts can be implemented. Instead {t} implements {t2}");
                }

                foreach (var t2 in t.Inherits)
                {
                    if (t2 == null)
                        SemanticErrors.Add($"One of the inherited types of {t} was not resolved");
                    else if (t2.Def?.Kind != TypeKind.Interface)
                        InternalErrors.Add($"Only concepts can be inherited. Instead {t} inherits {t2}");
                }
                    
                if (t.Kind == TypeKind.Interface)
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
                else if (t.Kind == TypeKind.ConcreteType)
                {
                    if (t.Inherits.Count > 0)
                        InternalErrors.Add("Types should not be able to inherit");
                    if (t.Methods.Count > 0)
                        InternalErrors.Add("Types should not have methods");
                }
            }
        }

        public TypeDef GetTypeDefinition(string name) 
            => TypeDefinitionsByName[name];
        
        public IEnumerable<TypeDef> GetConcreteTypes()
            => AllTypeAndLibraryDefinitions.Where(t => t.IsConcrete());

        public IEnumerable<TypeDef> GetImplementers(TypeExpression te)
            => AllTypeAndLibraryDefinitions.Where(t => t.IsConcrete() && t.Implements(te));

        public IEnumerable<TypeDef> GetConcepts()
            => AllTypeAndLibraryDefinitions.Where(t => t.IsInterface());

        public AstNode GetAstNode(Symbol symbol)
        {
            if (SymbolFactory.SymbolsToNodes.TryGetValue(symbol, out var node))
                return node;
            return null;
        }

        public void LogSymbolError(string message, Symbol symbol)
        {
            Logger.LogError(message);
            var node = GetAstNode(symbol);
            LogPosition(node);
        }

        public void LogPosition(ILocation location)
        {
            var range = location?.GetRange();

            var fileAndRange = range.ToFileAndRange();
            if (range != null)
            {
                Log(fileAndRange.ToString());
                Log(range.Begin?.CurrentLine);
                Log(range.Begin?.Indicator);
                Log(range.End.CurrentLine);
                Log(range.End.Indicator);
            }
            else
            {
                Log("Unknown location.");
            }
        }

        public TypeRelation GetRelation(TypeExpression src, TypeExpression dest)
        {
            if (!TypeRelations.RelationLookup.TryGetValue(src.Def, out var list))
                return null;
            var options = list.Where(rel => rel.Dest.Equals(dest.Def)).ToList();
            if (options.Count > 1) 
                throw new Exception($"Expected only one relation matching {src} to {dest}");
            if (options.Count == 0) 
                return null;
            return options[0];
        }

        public TypeDef GetTypeDef(string name)
        {
            if (TypeDefinitionsByName.TryGetValue(name, out var td))
                return td;
            if (LibraryDefinitionsByName.TryGetValue(name, out var ld))
                return ld;
            return null;
        }
    }
}