using System.Collections.Generic;
using System.Linq;
using System.Text;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public class FunctionCallAnalysis
    {
        public IReadOnlyList<IType> Arguments { get; }
        public IReadOnlyList<IType> Parameters { get; }
        public FunctionAnalysis Function { get; }
        public Compilation Compilation => Function.Compilation;
        public IType DeterminedReturnType { get; }
        public bool Callable { get; }
        public bool MaybeCallable { get; } 
        public bool PerfectFit { get; }
        public bool ArityMatches => Arguments.Count == Parameters.Count;
        public List<ArgFit> ArgFits { get; }
        public List<FunctionDefinition> Casts { get; } = new List<FunctionDefinition>();
        public int NumConcreteTypes { get; } 

        public static bool IsNotFit(ArgFit argFit)
        {
            return argFit == ArgFit.NoFitNullTypeArg || argFit == ArgFit.NoFitConceptToConcrete || argFit == ArgFit.NoFitNullTypeParam || argFit == ArgFit.NoFit;

        }

        public static bool IsMaybeFit(ArgFit argFit)
        {
            return argFit == ArgFit.UnknownArgIsTypeVar || argFit == ArgFit.UnknownParamIsTypeVar;
        }

        public string ArgDetails(int index)
        {
            var name = Function.Function.GetParameterName(index);
            return $"{name}:{Parameters[index]} <= {Arguments[index]} = {ArgFits[index]}";
        }

        public FunctionCallAnalysis(FunctionAnalysis function, IReadOnlyList<IType> args)
        {
            Function = function;
            Arguments = args;
            Parameters = function.ParameterTypes;
            
            // TEMP:
            DeterminedReturnType = function.DeclaredReturnType;

            if (!ArityMatches)
                return;

            ArgFits = new List<ArgFit>();
            for (var i = 0; i < args.Count; i++)
            {
                ArgFits.Add(ComputeArgFit(Compilation, args[i], Function.ParameterTypes[i], out var cast));
                Casts.Add(cast);
            }

            Callable = !ArgFits.Any(IsNotFit);
            MaybeCallable = !ArgFits.Any(IsMaybeFit);

            NumConcreteTypes = Function.ParameterTypes.Count(pt => pt.IsConcrete());
        }
        
        public enum ArgFit
        {
            NoFitNullTypeArg,
            NoFitNullTypeParam,
            NoFitConceptToConcrete,
            NoFit,
            UnknownArgIsTypeVar,
            UnknownParamIsTypeVar,
            FitEquality,
            FitWithCast,
            FitConcreteImplementsConcept,
            FitConceptInheritsConcept,
        }

        public static ArgFit ComputeArgFit(Compilation compilation, IType typeArg, IType typeParam, out FunctionDefinition cast)
        {
            cast = null;
            if (typeArg == null)
                return ArgFit.NoFitNullTypeArg;
            if (typeParam == null)
                return ArgFit.NoFitNullTypeParam;
            if (typeArg.Equals(typeParam))
                return ArgFit.FitEquality;
            if (typeArg.IsTypeVariable())
                return ArgFit.UnknownArgIsTypeVar;
            if (typeParam.IsTypeVariable())
                return ArgFit.UnknownParamIsTypeVar;
            if (typeArg.IsConcept() && typeParam.IsConcrete())
                return ArgFit.NoFitConceptToConcrete;

            if (typeArg.IsConcept())
            {
                if (typeParam.IsConcept())
                {
                    if (typeArg.Inherits(typeParam))
                        return ArgFit.FitConceptInheritsConcept;
                }

                if (typeParam.IsConcrete())
                {
                    if (typeArg.Implements(typeParam))
                        return ArgFit.FitConcreteImplementsConcept;
                }
            }

            cast = compilation.FindImplicitCast(typeArg, typeParam);
            if (cast != null)
                return ArgFit.FitWithCast;

            return ArgFit.NoFit;
        }

    }
}