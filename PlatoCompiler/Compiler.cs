using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ara3D.Utils;
using Parakeet;
using Parakeet.Demos.Plato;
using Plato.Compiler.Ast;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;
using Plato.Compiler.Vsg;

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
        public SymbolFactory SymbolFactory { get; set; }
        public IReadOnlyList<AstTypeDeclaration> TypeDeclarations { get; set; }
        public IDictionary<FunctionDefinition, FunctionAnalysis> FunctionAnalyses { get; } = new Dictionary<FunctionDefinition, FunctionAnalysis>();

        public IEnumerable<TypeDefinition> AllTypeAndLibraryDefinitions => TypeDefinitionsByName.Values
            .Concat(LibraryDefinitionsByName.Values);

        public Dictionary<string, TypeDefinition> TypeDefinitionsByName { get; } = new Dictionary<string, TypeDefinition>();
        public Dictionary<string, TypeDefinition> LibraryDefinitionsByName { get; } = new Dictionary<string, TypeDefinition>();
        public IReadOnlyList<FunctionDefinition> FunctionDefinitions { get; set; } 

        public Dictionary<Expression, IType> ExpressionTypes { get; } = new Dictionary<Expression, IType>();
        public Dictionary<string, ReifiedType> ReifiedTypes { get; set; }
        public Dictionary<string, List<ReifiedFunction>> ReifiedFunctionsByName { get; set; }

        public Dictionary<FunctionCall, FunctionGroupCallResolution> FunctionGroupCalls { get; } =
            new Dictionary<FunctionCall, FunctionGroupCallResolution>();

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
                SymbolFactory = new SymbolFactory(Logger);

                Log("Creating type definitions");
                var typeDefs = SymbolFactory.CreateTypeDefs(TypeDeclarations)
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

                Log($"Found {SymbolFactory.Errors.Count} symbol resolution errors");
                LogResolutionErrors(SymbolFactory.Errors);

                if (SymbolFactory.Errors.Count > 0)
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
                ReifiedTypes = TypeDefinitionsByName.Values.Where(td => td.IsConcrete())
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
                foreach (var fd in FunctionDefinitions)
                    GetOrComputeFunctionAnalysis(fd);

                /*
                foreach (var fa in FunctionAnalyses.Values.Where(f => f.IsConcept)) 
                    fa.BuildAnalysisOutput(sb);
                
                sb.AppendLine("Generic library functions"); 
                foreach (var fa in FunctionAnalyses.Values.Where(f => f.IsGenericLibraryFunction))
                    fa.BuildAnalysisOutput(sb);
                */

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

        public FunctionAnalysis GetOrComputeFunctionAnalysis(FunctionDefinition fd)
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

                    var firstParamType = f.Parameters[0].Type?.Definition;

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

        public void LogResolutionErrors(IEnumerable<SymbolFactory.ResolutionError> resolutionErrors)
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

        public FunctionAnalysis GetProcessedFunctionAnalysis(FunctionDefinition fd)
        {
            var r = FunctionAnalyses[fd];
            r.Process();
            return r;
        }

        public FunctionGroupCallResolution ResolveFunctionGroup(FunctionAnalysis context, FunctionCall callSite, FunctionGroupReference fgr, List<IType> argTypes)
        {
            if (FunctionGroupCalls.ContainsKey(callSite))
                return FunctionGroupCalls[callSite];
            var tmp = new FunctionGroupCallResolution(callSite, context, fgr, argTypes);
            FunctionGroupCalls.Add(callSite, tmp);
            return tmp;
        }

        public FunctionDefinition FindImplicitCast(IType from, IType to)
        {
            var name = to.GetTypeDefinition()?.Name;
            if (string.IsNullOrEmpty(name)) return null;
            var funcs = FunctionDefinitions.Where(fd => fd.Name == $"To{name}").Select(GetProcessedFunctionAnalysis).ToList();
            funcs = funcs.Where(fd => fd?.DeclaredReturnType?.Equals(to) == true).ToList();
            funcs = funcs.Where(fd => fd.ParameterTypes.Count == 1 && fd.ParameterTypes[0].Equals(from)).ToList();
            if (funcs.Count == 0) return null;
            if (funcs.Count > 1) throw new Exception("Ambiguous cast functions");
            return funcs[0].Function;
        }

        public FunctionDefinition FindCastConstructor(IType from, IType to)
        {
            var name = to.GetTypeDefinition()?.Name;
            if (string.IsNullOrEmpty(name)) return null;
            var funcs = FunctionDefinitions.Where(fd => fd.Name == $"{name}").Select(GetProcessedFunctionAnalysis).ToList();
            funcs = funcs.Where(fd => fd?.DeclaredReturnType?.Equals(to) == true).ToList();
            funcs = funcs.Where(fd => fd.ParameterTypes.Count == 1 && fd.ParameterTypes[0].Equals(from)).ToList();
            if (funcs.Count == 0) return null;
            if (funcs.Count > 1) throw new Exception("Ambiguous constructor functions");
            return funcs[0].Function;
        }

        public void OutputFunctionCallAnalysis(StringBuilder sb, FunctionCallAnalysis fca)
        {
            sb.AppendLine($"    Function = {fca.Function.Signature}");
            //sb.AppendLine($"    Callable = {fca.Callable}");
            //sb.AppendLine($"    Message = {fca.Message}");
            sb.AppendLine($"    First Score = {fca.FirstScore}");
            sb.AppendLine($"    Final Score = {fca.FinalScore}");
            sb.AppendLine($"    Scores = {string.Join(", ", fca.Scores)}");

            var ca = new ConstraintAnalysis(fca.Function);
            ca.Output(sb, "        ");
        }


        public void OutputFunctionAnalysis()
        {
            var sb = new StringBuilder();
            var i = 0;
            foreach (var fa in FunctionAnalyses.Values)
            {
                var typeSig = $"({string.Join(", ", fa.ParameterTypes)}) => {fa.DeclaredReturnType}";
                sb.AppendLine($"{i++}. {fa.Function.Name}");
                sb.AppendLine($"  Type sig: {typeSig}");
                sb.AppendLine($"  Sig: {fa.Signature}");
                sb.AppendLine($"  Body: {fa.Function.Body}");
                //sb.AppendLine($"  Has {fa.TypeParameterToTypeLookup.Count} Type parameters ");
                //sb.AppendLine($"  Has type parameter in return: {fa.DeclaredReturnType.HasTypeVariable()}");
            }
            var outputFille = Path.Combine(Path.GetTempPath(), "FunctionAnalysis.txt");
            File.WriteAllText(outputFille, sb.ToString());
        }

        public void OutputFunctionCallAnalysis()
        {
            var results = FunctionGroupCalls.Values.Where(fgc => fgc.DistinctReturnTypes.Count != 1).ToList();
            var sb = new StringBuilder();

            var i = 0;
            sb.AppendLine($"Function group call analysis");
            foreach (var fgc in results)
            {
                sb.AppendLine($"{i++}.");
                sb.AppendLine($"  Callsite: {fgc.Callsite}");
                sb.AppendLine($"  Args: {fgc.ArgString}");
                sb.AppendLine($"  results: {string.Join(", ", fgc.DistinctReturnTypes)}");
                sb.AppendLine($"  All functions count: {fgc.Functions.Count}");
                sb.AppendLine($"  Callable function count: {fgc.CallableFunctions.Count}");
                sb.AppendLine($"  Best function count: {fgc.BestFunctions.Count}");
                foreach (var f in fgc.Functions)
                    OutputFunctionCallAnalysis(sb, f);
            }

            var outputFille = Path.Combine(Path.GetTempPath(), "FunctionCallAnalysis.txt");
            File.WriteAllText(outputFille, sb.ToString()); 
        }
    }

}