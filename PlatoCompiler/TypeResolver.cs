using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Plato.Compiler.SymbolResolver;

namespace Plato.Compiler
{
    /// <summary>
    /// How to test. GEt the list of functions.I want to iterate on the types. Figure out the
    /// types of the parameters, figure out the types of the variables, figure out the functions
    /// called figure out the return values.
    /// Get all possible functions. Filter based on the types of the arguments, and the number of the
    /// arguments.   
    /// </summary>
    public class TypeResolver
    {
        public Dictionary<ParameterSymbol, List<Constraint>> ParameterConstraints { get; }
            = new Dictionary<ParameterSymbol, List<Constraint>>();

        public Operations Ops { get; }
        public IReadOnlyList<TypeDefSymbol> Types => Ops.Types;
        public IReadOnlyList<FunctionSymbol> Functions { get; }
        public IReadOnlyList<ParameterSymbol> Parameters { get; }
        public Logger Logger { get; }
        public List<ResolutionError> Errors { get; } = new List<ResolutionError>();

        public Dictionary<string, TypeDefSymbol> TypesFromNames { get; }

        public Dictionary<Symbol, TypeDefSymbol> ExpressionTypes { get; }
            = new Dictionary<Symbol, TypeDefSymbol>();

        public TypeResolver(Operations ops, Logger logger)
        {
            Logger = logger;
            Ops = ops;

            TypesFromNames = Types.Where(t => t.IsType() || t.IsConcept()).ToDictionary(t => t.Name, t => t);

            // Get the functions declared as methods
            Functions = Types
                .SelectMany(t => t.GetDescendantSymbols().OfType<MethodDefSymbol>().Select(md => md.Function)).ToList();

            // Update the type associated with each function. 
            foreach (var f in Functions)
                AddType(f, f.Type?.Def);

            // Get the parameters 
            Parameters = Functions.SelectMany(f => f.Parameters).ToList();

            // Update the type associated with the parameter. 
            foreach (var p in Parameters)
                AddType(p, p.Type?.Def);

            TestCasts();

            ComputeExpressionTypes();
        }

        public void TestCasts()
        {
            var tAny = FindType("Any");
            var tNumber = FindType("Number");
            var tUnit = FindType("Unit");
            var tString = FindType("String");
            var tArray = FindType("Array");
            var tNumerical = FindType("Numerical");
            var tValue = FindType("Value");
            var tBool = FindType("Boolean");

            var typeDefs = new TypeDefSymbol[] { tAny, tNumber, tUnit, tString, tArray, tNumerical, tValue, tBool };
            foreach (var td in typeDefs)
            {
                Debug.Assert(td != null);
                Debug.Assert(td.CanCastTo(tAny) > 0);
            }

            Debug.Assert(tNumber.CanCastTo(tNumerical) > 0);
            Debug.Assert(tNumber.CanCastTo(tValue) > 0);
            Debug.Assert(tUnit.CanCastTo(tNumber) > 0);
            Debug.Assert(tString != null);
            Debug.Assert(tString.CanCastTo(tArray) > 0);
            Debug.Assert(tString.CanCastTo(tNumber) == 0);


            Debug.Assert(tArray != null);
            Debug.Assert(tNumerical != null);
            Debug.Assert(tValue != null);
            Debug.Assert(tBool != null);
        }

        public void AddType(Symbol symbol, TypeDefSymbol type)
        {
            if (type == null)
                throw new Exception("Unexpected null symbol");

            if (!ExpressionTypes.ContainsKey(symbol))
                ExpressionTypes.Add(symbol, type);
            else if (ExpressionTypes[symbol] == null)
                ExpressionTypes[symbol] = type;
        }

       

        TypeDefSymbol InternalComputeType(Symbol s)
        {
            switch (s)
            {
                case ArgumentSymbol argumentSymbol:
                    return ComputeType(argumentSymbol.Original);

                case AssignmentSymbol assignmentSymbol:
                    throw new NotImplementedException("Not implemented yet?");

                case ConditionalExpressionSymbol conditionalExpressionSymbol:
                    // NOTE: the condition component is guaranteed to be a boolean.
                    // The fact that it must be a boolean, might affect what is called? 
                    return ComputeType(
                        conditionalExpressionSymbol.IfTrue).Unify(
                        ComputeType(conditionalExpressionSymbol.IfFalse));

                case MemberGroupSymbol functionGroupSymbol:
                    // TODO: figure out a good type for the method group. 
                    return PrimitiveTypes.Error;
                    //throw new Exception("For now this is supposed to be unreachable code. This occurs when a method group is used as a symbol, and not invoked.");

                case FunctionCallSymbol functionCallSymbol:
                {
                    if (!(functionCallSymbol.Function is RefSymbol rs))
                        throw new Exception("Can only call references");
                        
                    if (rs.Def is MemberGroupSymbol mgs)
                    {
                        return ComputeTypeOfMemberGroup(mgs, functionCallSymbol.Args);
                    }
                    return ComputeType(rs);
                }

                case LiteralSymbol literalSymbol:
                    switch (literalSymbol.LiteralType)
                    {
                        case LiteralTypes.Int:
                            return FindType("Integer");
                        case LiteralTypes.Float:
                            return FindType("Number");
                        case LiteralTypes.Bool:
                            return FindType("Boolean");
                        case LiteralTypes.String:
                            return FindType("String");
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                case RefSymbol refSymbol:
                    return ComputeType(refSymbol.Def);

                case TypeRefSymbol typeRefSymbol:
                    return typeRefSymbol.Def;

                case FieldDefSymbol fieldDefSymbol:
                    throw new NotImplementedException("Should not be called");

                case FunctionSymbol functionSymbol:
                    return GetType(functionSymbol);

                case MethodDefSymbol methodDefSymbol:
                    return ComputeType(methodDefSymbol.Function);

                case ParameterSymbol parameterSymbol:
                    return GetType(parameterSymbol);

                case PredefinedSymbol predefinedSymbol:
                    // Intrinsic or Tuple
                    return predefinedSymbol.Type?.Def;

                case TypeParameterDefSymbol typeParameterDefSymbol:
                    throw new NotImplementedException("What are the predefined symbols?");

                case TypeDefSymbol typeDefSymbol:
                    return typeDefSymbol;

                case VariableSymbol variableSymbol:
                    throw new NotImplementedException("Not supported yet");

                default:
                    throw new ArgumentOutOfRangeException(nameof(s));
            }
        }

        public TypeDefSymbol ComputeType(Symbol s)
        {
            // TODO: the "Self" type is special. 

            if (s == null)
                return null;

            // Is the type already computed? Just return it then. 
            var r = GetType(s);
            if (r != null)
                return r;

            var tmp = InternalComputeType(s);
            if (tmp == null)
            {
                // TODO: we know this can be triggered by trying to get the type of a tuple. 
                return PrimitiveTypes.Error;
            }

            AddType(s, tmp);
            return tmp;
        }

        public TypeDefSymbol FindType(string name)
        {
            return TypesFromNames[name];
        }

        public TypeDefSymbol GetType(Symbol s)
            => s == null
                ? null
                : ExpressionTypes.TryGetValue(s, out var r)
                    ? r
                    : null;

        public void ComputeExpressionTypes()
        {
            foreach (var f in Functions)
            {
                if (f?.Body == null ) 
                    continue;
                var t = ComputeType(f.Body);
                if (t.CanCastTo(f.Type.Def) == 0)
                {
                    LogError($"Function type declared as {f.Type.Name} but computed type is {t?.Name}. No conversion was found.");
                }
            }
        }

        public TypeRefSymbol GetArrayParameter(TypeRefSymbol trs)
        {
            if (trs.Name == "Array")
            {
                if (trs.TypeArgs.Count != 1)
                    // TODO: Fix this HACK! 
                    return FindType("Any").ToRef();
                    //throw new Exception("Arrays must have one type parameter");
                return trs.TypeArgs[0];
            }

            foreach (var tmp in trs.Def.GetAllImplementedConcepts())
            {
                if (tmp.Name == "Array")
                    return GetArrayParameter(tmp);
            }

            return trs;
        }

        public TypeDefSymbol ComputeTypeOfMemberGroup(MemberGroupSymbol mgs, IReadOnlyList<ArgumentSymbol> arguments)
        {
            var funcs = FindBestFunctions(mgs, arguments);
            if (funcs.Count == 0)
            {
                LogError($"No function found for {mgs.Name}");
                return PrimitiveTypes.Error;
            }
            if (funcs.Count > 1)
            {
                LogError($"Multiple best functions found for {mgs.Name}");
            }
            var f = funcs[0];
            return f?.Type?.Def ?? PrimitiveTypes.Error;
        }

        public IReadOnlyList<FunctionSymbol> FindBestFunctions(MemberGroupSymbol mgs,
            IReadOnlyList<ArgumentSymbol> arguments)
        {
            var functions = mgs.Members.Select(m => m.Function).Where(f  => f.Parameters.Count == arguments.Count)
                .ToList();

            if (functions.Count == 0)
            {
                LogError($"Found no functions in group {mgs} that matched number of arguments {arguments.Count}");
                return functions;
            }

            var argTypes = arguments.Select(ComputeType).Select(x => x.ToRef()).ToList();
            var candidates = functions.FindMatchingFunctions(argTypes);
            var argsStr = string.Join(", ", argTypes.Select(arg => $"{arg}"));

            if (candidates.Count == 0)
            {
                // If no candidates are found we are going to look to see if anything is an array, and 
                // resort to using the array types. Note: somehow I have to add a special map call here. 

                var argTypes2 = argTypes.Select(GetArrayParameter).ToList();
                candidates = functions.FindMatchingFunctions(argTypes2);
            }

            if (candidates.Count == 0)
            {
                LogError($"No candidates found for {mgs} even after mapping");
                return functions;
            }

            if (candidates.Count == 1)
            {
                return candidates;
            }

            for (var i = 0; i < argTypes.Count; ++i)
            {
                var groups = candidates.GroupBy(c => argTypes[i].MatchesScore(c.Parameters[i].Type)).OrderBy(grp => grp.Key);
                candidates = groups.First().ToList();
                if (candidates.Count == 1)
                    return candidates;
                if (candidates.Count == 0)
                    throw new Exception("Unexepected 0 candidates");
            }

            var candidateStr = string.Join(",", candidates.Select(c => c.Signature));
            LogError($"Found too many matches ({candidates.Count}) for {mgs} with argument types ({argsStr}) : {candidateStr}");
            return candidates;
        }

        public void LogError(string message)
        {
            // TODO: get an AstNode from a symbol. Pass a symbol to this function.
            Errors.Add(new ResolutionError(message, null));
        }
    }
}