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
        public SymbolResolver SymbolResolver { get; }
        public Logger Logger => SymbolResolver.Logger;
        public List<ResolutionError> Errors { get; } = new List<ResolutionError>();

        public Dictionary<string, FunctionSymbol> ImplicitConversions { get; }
        public Dictionary<string, TypeDefSymbol> TypesFromNames { get; }

        public Dictionary<Symbol, TypeDefSymbol> ExpressionTypes { get; }
            = new Dictionary<Symbol, TypeDefSymbol>();

        public TypeResolver(Operations ops, SymbolResolver symbols)
        {
            SymbolResolver = symbols;
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

            ImplicitConversions = Functions
                .Where(f => f.Name == $"To{f.Type.Name}")
                .ToDictionary(f => f.Name, f => f);

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

            var typeDefs = new [] { tAny, tNumber, tUnit, tString, tArray, tNumerical, tValue, tBool };
            foreach (var td in typeDefs)
            {
                Debug.Assert(td != null);
                Debug.Assert(CanCastTo(td, tAny) > 0);
            }

            Debug.Assert(CanCastTo(tNumber, tNumerical) > 0);
            Debug.Assert(CanCastTo(tNumber, tValue) > 0);
            Debug.Assert(CanCastTo(tUnit, tNumber) > 0);
            Debug.Assert(tString != null);
            Debug.Assert(CanCastTo(tString, tArray) > 0);
            Debug.Assert(CanCastTo(tString, tNumber) == 0);


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
                    return PrimitiveTypes.Function;

                case FunctionCallSymbol functionCallSymbol:
                {
                    if (!(functionCallSymbol.Function is RefSymbol rs))
                        throw new Exception("Can only call references");
                        
                    if (rs.Def is MemberGroupSymbol mgs)
                    {
                        return ComputeTypeOfMemberGroup(mgs, functionCallSymbol);
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
                if (CanCastTo(t, f.Type.Def) <= 0)
                {
                    // TODO: temp. We really need proper constraint resolution. Remember that based on what is passed in, 
                    // will affect what the concepts in a function actually are. They are actually a form of function generic. 
                    if (!t.IsConcept() || CanCastTo(f.Type.Def, t) <= 0)
                        LogError($"Function type declared as {f.Type.Name} but computed type is {t?.Name}. No conversion was found.", f);
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

        public TypeDefSymbol ComputeTypeOfMemberGroup(MemberGroupSymbol mgs, FunctionCallSymbol fcs)
        {
            var arguments = fcs.Args;
            var funcs = FindBestFunctions(mgs, fcs,arguments);
            if (funcs.Count == 0)
            {
                LogError($"No function found for {mgs.Name}", fcs);
                return PrimitiveTypes.Error;
            }
            if (funcs.Count > 1)
            {
                LogError($"Multiple best functions found for {mgs.Name}", fcs);
            }
            var f = funcs[0];
            // TODO: I'm not crazy about this. Ideally I would unify the type.
            return f?.Type?.Def ?? PrimitiveTypes.Error;
        }

        public IReadOnlyList<FunctionSymbol> FindBestFunctions(MemberGroupSymbol mgs, FunctionCallSymbol fcs,
            IReadOnlyList<ArgumentSymbol> arguments)
        {
            var functions = mgs.Members.Select(m => m.Function).Where(f  => f.Parameters.Count == arguments.Count)
                .ToList();

            if (functions.Count == 0)
            {
                LogError($"Found no functions in group {mgs} that matched number of arguments {arguments.Count}", fcs);
                return functions;
            }

            var argTypes = arguments.Select(ComputeType).Select(x => x.ToRef()).ToList();
            var candidates = FindMatchingFunctions(functions, argTypes);
            var argsStr = string.Join(", ", argTypes.Select(arg => $"{arg}"));

            if (candidates.Count == 0)
            {
                // If no candidates are found we are going to look to see if anything is an array, and 
                // resort to using the array types. Note: somehow I have to add a special map call here. 

                var argTypes2 = argTypes.Select(GetArrayParameter).ToList();
                candidates = FindMatchingFunctions(functions, argTypes2);
            }

            if (candidates.Count == 0)
            {
                LogError($"No candidates found for {mgs} even after mapping. Arg types are {argsStr}", fcs);
                return functions;
            }

            if (candidates.Count == 1)
            {
                return candidates;
            }

            for (var i = 0; i < argTypes.Count; ++i)
            {
                var groups = candidates.GroupBy(c => MatchesScore(argTypes[i], c.Parameters[i].Type)).OrderBy(grp => grp.Key);
                candidates = groups.First().ToList();
                if (candidates.Count == 1)
                    return candidates;
                if (candidates.Count == 0)
                    throw new Exception("Unexpected 0 candidates");
            }

            // Functions with a body are preferred. 
            var tmp = candidates.Where(f => f.Body != null).ToList();
            if (tmp.Count == 1)
                return tmp;

            var candidateStr = string.Join(",", candidates.Select(c => c.Signature));
            LogError($"Found too many matches ({candidates.Count}) for {mgs} with argument types ({argsStr}) : {candidateStr}", fcs);
            return candidates;
        }

        public void LogError(string message, Symbol symbol)
        {
            var node = SymbolResolver.SymbolsToNodes.TryGetValue(symbol, out var n) ? n : null;
            Errors.Add(new ResolutionError(message, node));
        }

        public static double CanCastTo(TypeRefSymbol fromType, TypeRefSymbol toType, bool allowConversions = true)
        {
            return CanCastTo(fromType?.Def, toType?.Def, allowConversions);
        }

        public static double CanCastTo(TypeDefSymbol fromDef, TypeDefSymbol toDef, bool allowConversions = true)
        {
            if (fromDef == null || toDef == null)
                throw new Exception("Could not find type definitions");

            if (fromDef.Equals(toDef))
                return 1;

            if (fromDef.IsSubType(toDef))
                return 2;

            if (fromDef.Name == "Any")
                return 3;

            if (allowConversions)
            {
                // We look for the implicit operators.
                // TODO: look for functions of the name "ToX" and "FromX" when allowing more than just the default. 

                if (toDef.IsType())
                {
                    // TODO: check that the type of each argument matches 
                    // TODO: add tuple support 
                    //if (fromType.Name == "Tuple") return fromType.TypeArgs.Count == toType.Def.Fields.Count ? 4 : 0;

                    // All types have a constructor that acts as an implicit cast 
                    if (toDef.Fields.Count == 1)
                    {
                        var fieldType = toDef.Fields[0].Type?.Def;
                        return CanCastTo(fromDef, fieldType, false) > 0 ? 4 : 0;
                    }
                }

                if (fromDef.IsType())
                {
                    // TODO: check that the type of each argument matches 
                    // TODO: add tuple support 
                    //if (toType.Name == "Tuple") return toType.TypeArgs.Count == fromType.Def.Fields.Count ? 4 : 0;

                    // All types with one field can implicit cast to that field. 
                    if (fromDef.Fields.Count == 1)
                    {
                        var fieldType = fromDef.Fields[0].Type?.Def;
                        return CanCastTo(fieldType, toDef, false) > 0 ? 5 : 0;
                    }
                }
            }

            return 0;
        }

        public static double MatchesScore(TypeRefSymbol argType, TypeRefSymbol parameterType)
        {
            if (argType == null) return 1;
            if (parameterType == null) return 1;
            return CanCastTo(argType, parameterType);
        }

        public static IReadOnlyList<FunctionSymbol> FindMatchingFunctions(IReadOnlyList<FunctionSymbol> funcs,
            IReadOnlyList<TypeRefSymbol> argumentTypes)
        {
            var candidates = funcs.Where(f => f.Parameters.Count == argumentTypes.Count).ToList();

            for (var i = 0; i < argumentTypes.Count; ++i)
            {
                candidates = candidates.Where(c => MatchesScore(argumentTypes[i], c.Parameters[i].Type) > 0).ToList();
            }

            return candidates;
        }
    }
}