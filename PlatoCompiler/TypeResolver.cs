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

        public Dictionary<Symbol, TypeDefSymbol> ExpressionTypes { get; }
            = new Dictionary<Symbol, TypeDefSymbol>();

        public TypeResolver(Operations ops, Logger logger)
        {
            Logger = logger;
            Ops = ops;

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

            // Update the type of all literals 
            foreach (var ls in Functions.SelectMany(f => f.GetDescendantSymbols()).OfType<LiteralSymbol>())
                AddType(ls, ls.Type?.Def);

            ComputeExpressionTypes();
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

        public static bool InheritsFrom(TypeDefSymbol self, TypeDefSymbol other)
            => self.GetSelfAndAllInheritedTypes().Contains(other);

        public static bool Implements(TypeDefSymbol self, TypeDefSymbol other)
            => self.GetAllImplementedConcepts().Select(c => c.Def).Contains(other);

        public static bool IsSubType(TypeDefSymbol self, TypeDefSymbol other)
        {
            if (self.Equals(other))
                return true;
            if (InheritsFrom(self, other))
                return true;
            if (Implements(self, other))
                return true;
            return false;
        }

        public static bool IsSubType(TypeDefSymbol self, IEnumerable<TypeDefSymbol> others)
            => others.All(x => IsSubType(self, x));

        public static TypeDefSymbol Unify(IEnumerable<TypeDefSymbol> conceptsA, IEnumerable<TypeDefSymbol> conceptsB)
        {
            // Which concepts in A supercede all of those in B? 
            var superTypesA = conceptsA.Where(c => IsSubType(c, conceptsB)).ToList();

            // Which concepts in B supercede all of those in B
            var superTypesB = conceptsB.Where(c => IsSubType(c, conceptsA)).ToList();

            if (superTypesA.Count > 0)
                return superTypesA[0];

            if (superTypesB.Count > 0)
                return superTypesB[0];

            throw new Exception("Unable to unify type");
        }

        public static TypeDefSymbol Unify(TypeDefSymbol a, TypeDefSymbol b)
        {
            // If one type inher

            if (a == null)
                return b;

            if (b == null)
                return a;

            if (a.Equals(b))
                return a;

            // If one type is a concept, and the other is a regular type, then choose the type. 
            if (a.IsType() && b.IsConcept())
                return a;

            if (a.IsConcept() && a.IsType())
                return a;

            if (a.IsType() && b.IsType())
            {
                var aConcepts = a.Implements.Select(i => i.Def);
                var bConcepts = b.Implements.Select(i => i.Def);
                return Unify(aConcepts, bConcepts);
            }

            if (a.IsConcept() && b.IsConcept())
            {
                var aConcepts = a.GetSelfAndAllInheritedTypes();
                var bConcepts = b.GetSelfAndAllInheritedTypes();
                return Unify(aConcepts, bConcepts);
            }

            return b;
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
                    return Unify(
                        ComputeType(conditionalExpressionSymbol.IfTrue),
                        ComputeType(conditionalExpressionSymbol.IfFalse));

                case MemberGroupSymbol functionGroupSymbol:
                    throw new Exception("For now this is supposed to be unreachable code. This occurs when a method group is used as a symbol, and not invoked.");

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
                            return PrimitiveTypes.Integer;
                        case LiteralTypes.Float:
                            return PrimitiveTypes.Number;
                        case LiteralTypes.Bool:
                            return PrimitiveTypes.Boolean;
                        case LiteralTypes.String:
                            return PrimitiveTypes.String;
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
                throw new Exception("WTF?!");
            AddType(s, tmp);
            return tmp;
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
                if (t?.Name != f.Type.Name)
                {
                    LogError($"Function type declared as {f.Type.Name} but computed type is {t?.Name}");
                }
            }
        }

        public static int CanCastTo(TypeRefSymbol fromType, TypeRefSymbol toType, bool allowConversions = true)
        {
            var fromDef = fromType.Def;
            var toDef = toType.Def;
            if (fromDef == null || toDef == null)
                return 1;
            if (toDef.Name == "Any")
                return 2;
            if (fromDef.Name == "Any")
                return 2;
            if (fromDef.Name == toDef.Name)
                return 3;
            if (IsSubType(fromDef, toDef))
                return 3;

            if (allowConversions)
            {
                // We look for the implicit operators.
                // TODO: look for functions of the name "ToX" and "FromX" when allowing more than just the default. 

                if (toType.Def.IsType())
                {
                    // TODO: check that the type of each argument matches 
                    if (fromType.Name == "Tuple")
                        return fromType.TypeArgs.Count == toType.Def.Fields.Count ? 4 : 0;

                    // All types have a constructor that acts as an implicit cast 
                    if (toType.Def.Fields.Count == 1)
                    {
                        var fieldType = toType.Def.Fields[0].Type;
                        return CanCastTo(fromType, fieldType, false) > 0 ? 4 : 0;
                    }
                }

                if (fromType.Def.IsType())
                {
                    // TODO: check that the type of each argument matches 
                    if (toType.Name == "Tuple")
                        return toType.TypeArgs.Count == fromType.Def.Fields.Count ? 4 : 0;

                    // All types with one field can implicit cast to that field. 
                    if (fromType.Def.Fields.Count == 1)
                    {
                        var fieldType = fromType.Def.Fields[0].Type;
                        return CanCastTo(fieldType, toType, false) > 0 ? 4 : 0;
                    }
                }
            }

            return 0;
        }

        public static int DistanceFromAny(TypeRefSymbol parameterType)
        {
            if (parameterType == null || parameterType.Name == "Any")
                return 1;
            if (parameterType.Def.IsConcept())
                return 2;
            return 3;
        }

        public static int MatchScore(TypeRefSymbol argType, TypeRefSymbol parameterType)
        {
            if (argType == null || argType.Name == "Any")
                return DistanceFromAny(parameterType);
            if (parameterType == null || parameterType.Name == "Any")
                return DistanceFromAny(argType);
            var argDef = argType.Def;
            var paramDef = parameterType.Def;
            if (argDef.Name == paramDef.Name)
                return 1;
            return 1;
        }

        public IReadOnlyList<FunctionSymbol> FindMatchingFunctions(IReadOnlyList<FunctionSymbol> funcs,
            IReadOnlyList<TypeRefSymbol> argumentTypes)
        {
            var candidates = funcs.Where(f => f.Parameters.Count == argumentTypes.Count).ToList();

            for (var i = 0; i < argumentTypes.Count; ++i)
            {
                candidates = candidates.Where(c => Matches(argumentTypes[i], c.Parameters[i].Type) > 0).ToList();
            }

            return candidates;
        }

        public static int Matches(TypeRefSymbol argType, TypeRefSymbol parameterType)
        {
            if (argType == null) return 1;
            if (parameterType == null) return 1;
            return CanCastTo(argType, parameterType);
        }

        public static TypeRefSymbol GetArrayParameter(TypeRefSymbol trs)
        {
            if (trs.Name == "Array")
            {
                if (trs.TypeArgs.Count != 1)
                    // TODO: Fix this HACK! 
                    return PrimitiveTypes.Any.ToRef();
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
            var f = FindBestFunction(mgs, arguments);
            return f.Type?.Def;
        }

        public FunctionSymbol FindBestFunction(MemberGroupSymbol mgs,
            IReadOnlyList<ArgumentSymbol> arguments)
        {
            var functions = mgs.Members.Select(m => m.Function).Where(f => f.Parameters.Count == arguments.Count)
                .ToList();

            if (functions.Count == 0)
            {
                LogError($"Found no functions in group {mgs} that matched number of arguments {arguments.Count}");
                return null;
            }

            var argTypes = arguments.Select(ComputeType).Select(x => x.ToRef()).ToList();
            var candidates = FindMatchingFunctions(functions, argTypes);
            var argsStr = string.Join(", ", argTypes.Select(arg => $"{arg}"));

            if (candidates.Count == 0)
            {
                var argTypes2 = argTypes.Select(GetArrayParameter).ToList();
                candidates = FindMatchingFunctions(functions, argTypes2);

                if (candidates.Count == 0)
                {
                    LogError($"Found no functions in group {mgs} that matched arguments ({argsStr})");
                    // The following is just for debugging purposes. 
                    var argTypes3 = argTypes.Select(GetArrayParameter).ToList();
                    return null;
                }
            }

            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            Debug.Assert(candidates.Count > 1);

            var candidateStr = string.Join(",", candidates.Select(c => c.Signature));
            LogError($"Found too many matches ({candidates.Count}) for {mgs} with argument types ({argsStr}) : {candidateStr}");
            return candidates[0];
        }

        public void LogError(string message)
        {
            // TODO: get an AstNode from a symbol. Pass a symbol to this function.
            Errors.Add(new ResolutionError(message, null));
        }
    }
}