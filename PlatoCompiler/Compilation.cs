using System;
using System.Collections.Generic;
using System.IO;
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

                Libraries = new LibraryFunctions(this);
                ConcreteTypes = GetConcreteTypes()
                    .Select(c => new ConcreteType(c, Libraries))
                    .ToList();

                Log("Reified types");
                WriteReifiedTypes();

                Log("Creating function analysis");
                var sb = new StringBuilder();
                foreach (var fd in FunctionDefinitions)
                    GetOrComputeFunctionAnalysis(fd);

                foreach (var fa in FunctionAnalyses.Values.Where(f => f.IsConcept)) 
                    fa.BuildAnalysisOutput(sb);
                
                sb.AppendLine("Generic library functions"); 
                foreach (var fa in FunctionAnalyses.Values.Where(f => f.IsGenericLibraryFunction))
                    fa.BuildAnalysisOutput(sb);

                Log("Gathering constraints for each function");
                foreach (var fa in FunctionAnalyses.Values)
                    fa.Process();

                OutputFunctionCallAnalysis();
                OutputFunctionAnalysis();

                var noResults = FunctionGroupCalls.Values.Where(fgc => fgc.DistinctReturnTypes.Count == 0).ToList();
                sb.AppendLine($"Function group call unresolved: no functions {noResults.Count}");
                foreach (var fgc in noResults)
                    sb.AppendLine(fgc.ToString());

                var ambResults = FunctionGroupCalls.Values.Where(fgc => fgc.DistinctReturnTypes.Count > 1).ToList();
                sb.AppendLine($"Function group call unresolved: ambiguous {ambResults.Count}");
                foreach (var fgc in ambResults)
                    sb.AppendLine(fgc.ToString());

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

        public bool DisplayWarnings = false;
        public ILogger Logger { get; }
        public bool CompletedCompilation { get; }
        public IReadOnlyList<AstNode> Trees { get; }
        public SymbolFactory SymbolFactory { get; }
        public IReadOnlyList<AstTypeDeclaration> TypeDeclarations { get; }
        public IDictionary<FunctionDef, FunctionAnalysis> FunctionAnalyses { get; } = new Dictionary<FunctionDef, FunctionAnalysis>();

        public IEnumerable<TypeDef> AllTypeAndLibraryDefinitions => TypeDefinitions
            .Concat(LibraryDefinitionsByName.Values);

        public Dictionary<string, TypeDef> TypeDefinitionsByName { get; } = new Dictionary<string, TypeDef>();
        public Dictionary<string, TypeDef> LibraryDefinitionsByName { get; } = new Dictionary<string, TypeDef>();
        public IReadOnlyList<FunctionDef> FunctionDefinitions { get; }
        public IEnumerable<TypeDef> TypeDefinitions => TypeDefinitionsByName.Values;
        public IEnumerable<Symbol> Symbols => SymbolFactory.SymbolsToNodes.Keys.OrderBy(s => s.Id);

        public Dictionary<Expression, IType> ExpressionTypes { get; } = new Dictionary<Expression, IType>();
        public Dictionary<string, ReifiedType> ReifiedTypes { get; }
        public Dictionary<string, List<ReifiedFunction>> ReifiedFunctionsByName { get; }

        public Dictionary<FunctionCall, FunctionGroupCallResolution> FunctionGroupCalls { get; } =
            new Dictionary<FunctionCall, FunctionGroupCallResolution>();

        public List<string> SemanticErrors { get; } = new List<string>();
        public List<string> SemanticWarnings { get; } = new List<string>();
        public List<string> InternalErrors { get; } = new List<string>();

        public IEnumerable<string> Diagnostics => SemanticWarnings.Select(w => $"[WARNING] {w}")
            .Concat(SemanticErrors.Select(e => $"[ERROR] {e}"))
            .Concat(InternalErrors.Select(i => $"[INTERNAL ERROR] {i}"));

        public List<VisualSyntaxGraph> Graphs { get; } = new List<VisualSyntaxGraph>();

        public LibraryFunctions Libraries { get; }
        public IReadOnlyList<ConcreteType> ConcreteTypes { get; }

        public FunctionAnalysis GetOrComputeFunctionAnalysis(FunctionDef fd)
        {
            if (FunctionAnalyses.ContainsKey(fd))
                return FunctionAnalyses[fd];
            var fa = new FunctionAnalysis(this, fd);
            FunctionAnalyses.Add(fd, fa);
            return fa;
        }

        public void AddLibraryFunctionsToReifiedTypes()
        {
            foreach (var library in LibraryDefinitionsByName.Values)
            {
                foreach (var f in library.Functions)
                {
                    if (f.Parameters.Count == 0)
                        continue;

                    var firstParamType = f.Parameters[0].Type?.Def;

                    Verifier.AssertNotNull(firstParamType, $"First parameter type of {f}");

                    if (firstParamType.IsConcrete())
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

        public IType GetExpressionType(Expression expr)
            => ExpressionTypes.TryGetValue(expr, out var r) ? r : null;
        
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
                    else if (t2.Def?.Kind != TypeKind.Concept)
                        SemanticErrors.Add($"Only concepts can be implemented. Instead {t} implements {t2}");
                }

                foreach (var t2 in t.Inherits)
                {
                    if (t2 == null)
                        SemanticErrors.Add($"One of the inherited types of {t} was not resolved");
                    else if (t2.Def?.Kind != TypeKind.Concept)
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
        {
            return TypeDefinitionsByName[name];
        }

        public FunctionAnalysis GetProcessedFunctionAnalysis(FunctionDef fd)
        {
            var r = FunctionAnalyses[fd];
            r.Process();
            return r;
        }

        public FunctionGroupCallResolution ResolveFunctionGroup(FunctionAnalysis context, FunctionCall callSite, FunctionGroupRefSymbol fgr, List<IType> argTypes)
        {
            if (FunctionGroupCalls.ContainsKey(callSite))
                return FunctionGroupCalls[callSite];
            var tmp = new FunctionGroupCallResolution(callSite, context, fgr, argTypes);
            FunctionGroupCalls.Add(callSite, tmp);
            return tmp;
        }

        public FunctionDef FindImplicitCast(IType from, IType to)
        {
            var name = to.GetTypeDefinition()?.Name;
            if (string.IsNullOrEmpty(name)) return null;
            var funcs = FunctionDefinitions.Where(fd => fd.FunctionType == FunctionType.Cast).Select(GetProcessedFunctionAnalysis).ToList();
            funcs = funcs.Where(fd => fd?.DeclaredReturnType?.Equals(to) == true).ToList();
            funcs = funcs.Where(fd => fd.ParameterTypes.Count == 1 && fd.ParameterTypes[0].Equals(from)).ToList();
            if (funcs.Count == 0) return null;
            if (funcs.Count > 1) throw new Exception("Ambiguous cast functions");
            return funcs[0].Def;
        }

        public FunctionDef FindCastConstructor(IType from, IType to)
        {
            var name = to.GetTypeDefinition()?.Name;
            if (string.IsNullOrEmpty(name)) return null;
            var funcs = FunctionDefinitions.Where(fd => fd.FunctionType == FunctionType.Constructor).Select(GetProcessedFunctionAnalysis).ToList();
            funcs = funcs.Where(fd => fd?.DeclaredReturnType?.Equals(to) == true).ToList();
            funcs = funcs.Where(fd => fd.ParameterTypes.Count == 1 && fd.ParameterTypes[0].Equals(from)).ToList();
            if (funcs.Count == 0) return null;
            if (funcs.Count > 1) throw new Exception("Ambiguous constructor functions");
            return funcs[0].Def;
        }

        public void OutputFunctionCallAnalysis(StringBuilder sb, FunctionCallAnalysis fca)
        {
            sb.AppendLine($"    Function = {fca.Function.Signature}");
            sb.AppendLine($"    Callable = {fca.Callable}, Has body = {fca.HasBody}, Arity Matches = {fca.ArityMatches}, # Concrete type = {fca.NumConcreteTypes}");
            if (fca.ArityMatches)
            {
                for (var i = 0; i < fca.Arguments.Count; ++i)
                    sb.AppendLine($"      Argument {i} = {fca.ArgDetails(i)}");
            }
        }

        public void OutputFunctionAnalysis()
        {
            var sb = new StringBuilder();
            var i = 0;
            foreach (var fa in FunctionAnalyses.Values)
            {
                var typeSig = $"({string.Join(", ", fa.ParameterTypes)}) => {fa.DeclaredReturnType}";
                sb.AppendLine($"{i++}. {fa.Def.Name}");
                sb.AppendLine($"  Type sig: {typeSig}");
                sb.AppendLine($"  Sig: {fa.Signature}");
                sb.AppendLine($"  Body: {fa.Def.Body}");
                //sb.AppendLine($"  Has {fa.TypeParameterToTypeLookup.Count} Type parameters ");
                //sb.AppendLine($"  Has type parameter in return: {fa.DeclaredReturnType.HasTypeVariable()}");
            }
            var outputFille = Path.Combine(Path.GetTempPath(), "FunctionAnalysis.txt");
            File.WriteAllText(outputFille, sb.ToString());
        }

        public void OutputFunctionCallAnalysis()
        {
            var results = FunctionGroupCalls.Values.Where(fgc => fgc.BestFunctions.Count != 1).ToList();
            //var results = FunctionGroupCalls.Values.ToList();
            var sb = new StringBuilder();

            var i = 0;
            sb.AppendLine($"Function call analysis");
            foreach (var fgc in results)
            {
                sb.AppendLine($"{i++}.");
                sb.AppendLine($"  Callsite: {fgc.Callsite}");
                sb.AppendLine($"  Args: {fgc.ArgString}");
                sb.AppendLine($"  Possible Return Types: {string.Join(", ", fgc.DistinctReturnTypes)}");
                sb.AppendLine($"  Callable function count: {fgc.CallableFunctions.Count}");
                sb.AppendLine($"  Best function count: {fgc.BestFunctions.Count}");
                foreach (var f in fgc.CallableFunctions)
                    OutputFunctionCallAnalysis(sb, f);
            }

            var outputFille = Path.Combine(Path.GetTempPath(), "FunctionCallAnalysis.txt");
            File.WriteAllText(outputFille, sb.ToString()); 
        }

        public IEnumerable<TypeDef> GetConcreteTypes()
            => AllTypeAndLibraryDefinitions.Where(t => t.IsConcrete());

        public IEnumerable<TypeDef> GetConcepts()
            => AllTypeAndLibraryDefinitions.Where(t => t.IsConcept());

        public AstNode GetAstNode(Symbol symbol)
        {
            if (SymbolFactory.SymbolsToNodes.ContainsKey(symbol))
                return SymbolFactory.SymbolsToNodes[symbol];
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
    }
}