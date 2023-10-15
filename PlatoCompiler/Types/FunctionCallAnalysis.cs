using System;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public class FunctionCallAnalysis
    {
        public const int CantPassGenericToConcreteFit = -6;
        public const int DoesntImplementConceptFit = -5;
        public const int MismatchedTypeFit = -4;
        public const int CantPassConceptToConcreteFit = -3;
        public const int DoesntMatchDeclaredConstraintFit = -2;
        public const int NoFit = -1;
        public const int PerfectFit = 0;
        public const int GenericFit = 10 * 1000;
        public const int GenericToSpecificFitPenalty = 200 * 1000;
        public const int SpecificToGenericFitPenalty = 100 * 1000;
        public const int AlmostNotAFit = 1000 * 1000;
        public const int InheritsFitPenalty = 10;
        public const int ImplementsFitPenalty = 100;

        // TODO: I'm not sure which of these two should be considered a better fit.
        // The chances of having both present seem slim, and should probably be treated as an error. 
        public const int ImplicitCastPenalty = 1000;
        public const int CastConstructorPenalty = 2000;

        public FunctionAnalysis Function { get; }
        public Compiler Compiler => Function.Compiler;
        public Dictionary<TypeVariable, List<IType>> TypeVariableConstraints { get; } = new Dictionary<TypeVariable, List<IType>>();
        public IType DeterminedReturnType { get; }
        public bool Callable { get; }
        public List<FunctionDefinition> Casts { get; } = new List<FunctionDefinition>();
        public FunctionDefinition CurrentCast { get; private set; }
        public IReadOnlyList<int> Scores { get; } = new List<int>();
        public int FirstScore => Scores.FirstOrDefault();
        public int FinalScore => Scores.Sum();
        public string Message { get; private set; }
      
        public FunctionCallAnalysis(FunctionAnalysis function, IReadOnlyList<IType> args)
        {
            Function = function;
                
            if (Function.ParameterTypes.Count != args.Count)
            {
                Message = "Mismatched arity";
                return;
            }

            var scores = new List<int>();
            for (var i = 0; i < args.Count; i++)
            {
                var argType = args[i];
                var paramType = function.ParameterTypes[i];
                if (!BindTypeVariables(argType, paramType))
                    return;

                scores.Add(ArgumentFit(argType, paramType));
                Casts.Add(CurrentCast);
            }

            Scores = scores;
            if (Scores.Any(s => s < 0))
            {
                Message = "One of the scores was less than zero";
                return; 
            }

            DeterminedReturnType = ReplaceTypeVariable(Function.DeclaredReturnType);
            Callable = true;
        }

        public void AddTypeConstraint(TypeVariable tv, IType constraint)
        {
            if (!TypeVariableConstraints.ContainsKey(tv))
                TypeVariableConstraints.Add(tv, new List<IType>());
            TypeVariableConstraints[tv].Add(constraint);
        }

        public bool BindTypeVariables(IType typeArg, IType typeParam)
        {
            if (typeParam is TypeVariable tv)
            {
                AddTypeConstraint(tv, typeArg);
            }
            else if (typeParam is TypeList paramList)
            {
                if (typeArg is TypeList argList)
                {
                    if (argList.Children.Count != paramList.Children.Count)
                    {
                        Message = "Generic type arity mismatch";
                        return false;
                    }
                    for (var i = 0; i < argList.Children.Count; ++i)
                    {
                        BindTypeVariables(argList.Children[i], paramList.Children[i]);
                    }
                }
            }

            return true;
        }

        public IType Unify(IReadOnlyList<IType> types)
        {
            var tmp = types.Distinct().ToList();
            //if (tmp.Count != 1) throw new Exception($"Too many types found ({string.Join(", ", types)}");
            return tmp[0];
        }

        public IType ReplaceTypeVariable(IType input)
        {
            if (input is TypeVariable tv)
            {
                if (!TypeVariableConstraints.ContainsKey(tv))
                {
                    // TODO: finish this. I would need to analyze the types for every function.
                    return input;
                    // throw new Exception($"Could not find type variable {tv}");
                }

                return Unify(TypeVariableConstraints[tv]);
            }
            
            if (input is TypeList tl)
            {
                return new TypeList(tl.Children.Select(ReplaceTypeVariable).ToArray());
            }
            
            return input;
        }

        /// <summary>
        /// below 0 is a non-fit, 0 is a perfect fit, above 0 is an increasingly bad fit. 
        /// </summary>
        public int ArgumentFit(IType typeArgument, IType typeParameter)
        {
            if (typeArgument == null)
                return NoFit;

            if (typeArgument is TypeVariable tv0)
                typeArgument = tv0.Constraint;

            if (typeArgument.Equals(typeParameter))
                return PerfectFit;

            // For example: are we going from a "(Function, int)" to "(Function)"
            if (typeArgument is TypeList typeArgumentAsList && typeParameter is TypeConstant typeParameterAsConstant)
            {
                var tmp = ArgumentFit(typeArgumentAsList.Children[0], typeParameter);
                if (tmp >= 0)
                    return tmp + SpecificToGenericFitPenalty;
                return tmp;
            }

            if (typeArgument is TypeConstant typeArgumentAsConstant && typeParameter is TypeList typeParameterAsList)
            {
                var tmp = ArgumentFit(typeArgumentAsConstant, typeParameterAsList.Children[0]);
                if (tmp >= 0)
                    return tmp + GenericToSpecificFitPenalty;
                return tmp;
            }

            if (typeParameter is TypeVariable tv)
            {
                if (typeArgument.IsConcrete() || typeArgument.IsPrimitive())
                {
                    // Check that the type argument implements all of the constraints of the type variable
                    if (!typeArgument.Implements(tv.Constraint))
                        return DoesntMatchDeclaredConstraintFit;

                    // All of the declared constraints are satisfied.
                    // So we are a match, but anything else would match better 
                    return GenericFit;
                }

                if (typeArgument.IsConcept())
                {
                    // Check that the type argument inherits all of the constraints of the type variable
                    if (!typeArgument.Inherits(tv.Constraint))
                        return DoesntMatchDeclaredConstraintFit;

                    // All of the declared constraints are satisfied.
                    // So we are a match, but anything else would match better 
                    return GenericFit;
                }

                throw new InvalidOperationException("Should not be reachable");
            }

            if (typeParameter.IsConcept())
            {
                if (typeArgument.IsConcept())
                {
                    var r = typeArgument.InheritsDepth(typeParameter) + InheritsFitPenalty;
                    return r >= 0 ? r : DoesntImplementConceptFit;
                }

                if (typeArgument.IsConcrete() || typeArgument.IsPrimitive())
                {
                    var r = typeArgument.ImplementsDepth(typeParameter) + ImplementsFitPenalty;
                    return r >= 0 ? r : DoesntImplementConceptFit;
                }

                throw new InvalidOperationException("Should not be reachable");
            }

            if (typeParameter.IsConcrete() || typeParameter.IsPrimitive())
            {
                CurrentCast = Compiler.FindImplicitCast(typeArgument, typeParameter);
                if (CurrentCast != null)
                {
                    return ImplicitCastPenalty;
                }

                CurrentCast = Compiler.FindCastConstructor(typeArgument, typeParameter);
                if (CurrentCast != null)
                {
                    return CastConstructorPenalty;
                }

                if (typeArgument.IsConcept())
                {
                    // Cannot pass a concept to a concrete type 
                    return CantPassConceptToConcreteFit;
                }

                if (typeParameter.IsConcrete() || typeParameter.IsPrimitive())
                {
                    var td1 = typeParameter.GetTypeDefinition();
                    var td2 = typeArgument.GetTypeDefinition();

                    return td1.Equals(td2) ? 0 : MismatchedTypeFit;
                }

                throw new InvalidOperationException("Should not be reachable");
            }

            throw new InvalidOperationException("Should not be reachable");
        }
    }
}