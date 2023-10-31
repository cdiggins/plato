using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ara3D.Utils;
using Plato.Compiler.Ast;
using Plato.Compiler.Symbols;
using Tuple = Plato.Compiler.Symbols.Tuple;

namespace Plato.Compiler.Types
{
    /// <summary>
    /// The function analysis is a place-holder for all of the information. 
    /// </summary>
    public class FunctionAnalysis
    {
        public List<TypeVariable> Variables { get; } = new List<TypeVariable>();
        public List<IConstraint> Constraints { get; } = new List<IConstraint>();

        public Compiler Compiler { get; }
        public FunctionDefinition Function { get; }
        
        public TypeDefinition OwnerType 
            => Function.OwnerType;
        
        public bool IsConcept 
            => OwnerType.IsConcept();
        
        public bool IsGenericLibraryFunction 
            => OwnerType.IsLibrary() && Variables.Count > 0;

        public Dictionary<ParameterDefinition, IType> ParameterToTypeLookup { get; } =
            new Dictionary<ParameterDefinition, IType>();

        public Dictionary<TypeParameterDefinition, IType> TypeParameterToTypeLookup { get; } =
            new Dictionary<TypeParameterDefinition, IType>();

        public IType DeclaredReturnType { get; }
        public IReadOnlyList<IType> ParameterTypes { get; }
        public IType Self { get; }

        public FunctionAnalysis(Compiler compiler, FunctionDefinition function)
        {
            Compiler = compiler;
            Function = function;

            foreach (var tp in OwnerType.TypeParameters)
            {
                TypeParameterToTypeLookup.Add(tp, ToTypeVariable(tp));
            }

            if (OwnerType.IsConcept())
            {
                Self = GenerateConstrainedTypeVariable(OwnerType.ToTypeExpression());  
            }

            var pTypes  = new List<IType>();
            foreach (var p in function.Parameters)
            {
                // Try to figure out the cardinality of each parameter. 
                // For now, we are only really worried about functions.
                // And I think we can just look at how things are called. 

                if (p.Type.Name == "Function")
                {
                    var n = ComputeCardinalityOfFunction(p, function.Body);
                    var f = CreateFunctionType(n);
                    ParameterToTypeLookup.Add(p, f);
                    pTypes.Add(f);
                }
                else
                {
                    var pt = ToIType(p.Type);

                    if (p.Type.Definition.IsConcept())
                    {
                        pt = GenerateTypeVariable(pt);
                    }
                    
                    ParameterToTypeLookup.Add(p, pt);
                    pTypes.Add(pt);
                }
            }

            if (Function.ReturnType.Name.StartsWith("Function"))
            {
                var n = Function.ReturnType.TypeArgs.Count;
                if (n == 0)
                    throw new Exception("Functions returning functions are not supported yet because we can't easily determine their cardinality");
                var f = CreateFunctionType(n);
                DeclaredReturnType = f;
            }

            DeclaredReturnType = ToIType(Function.ReturnType);

            if (Function.ReturnType.Definition.IsConcept())
            {
                DeclaredReturnType = GenerateTypeVariable(DeclaredReturnType);
            }

            ParameterTypes = pTypes;

            Debug.Assert(ParameterTypes.Count == Function.Parameters.Count);
        }

        public TypeList ToTypeList(IType first, IEnumerable<IType> rest)
            => new TypeList(rest.Prepend(first).ToList());

        public TypeList CreateFunctionType(IEnumerable<IType> args, IType returnType)
            => ToTypeList(ToSimpleType("Function"), args.Append(returnType));

        public IType CreateFunctionType(int n)
            => CreateFunctionType(Enumerable.Range(0, n).Select(_ => GenerateUnconstrainedTypeVariable()), GenerateUnconstrainedTypeVariable());

        // TODO: I am confident that this will fail. When referring to a type, there might be references to type parameters .
        public bool IsTypeExpressionComplete(TypeExpression tes) 
            => tes.TypeArgs.Count == tes.Definition.TypeParameters.Count
            && !tes.Definition.IsTypeVariable()
            && tes.TypeArgs.All(IsTypeExpressionComplete);

        public IType ToSimpleType(string name)
            => ToSimpleType(Compiler.GetTypeDefinition(name));

        public IType ToSimpleType(TypeDefinition td) 
            => td is TypeParameterDefinition tpd
                ? ToTypeVariable(tpd)
                : new TypeConstant(td);

        public IType ToTypeVariable(TypeParameterDefinition tpd)
            => TypeParameterToTypeLookup.TryGetValue(tpd, out var r) 
                    ? r : GenerateConstrainedTypeVariable(tpd.Constraint);

        public IType ToTypeList(TypeExpression expr)
        {
            var tmp = new List<IType> { ToSimpleType(expr.Definition) };
            for (var i = 0; i < expr.Definition.TypeParameters.Count; ++i)
            {
                var tp = expr.Definition.TypeParameters[i];

                if (i < expr.TypeArgs.Count)
                {
                    var ta = ToIType(expr.TypeArgs[i]);
                    tmp.Add(ta);
                }
                else
                {
                    tmp.Add(ToTypeVariable(tp));
                }
            }

            return new TypeList(tmp);
        }

        public IType ToIType(TypeDefinition td)
        {
            var tmp = ToSimpleType(td);
            return td.TypeParameters.Count == 0 
                ? tmp 
                : ToTypeList(tmp, td.TypeParameters.Select(ToTypeVariable).ToList());
        }

        public IType ToIType(TypeExpression tes)
        {
            if (tes.IsSelfType())
            {
                Verifier.AssertNotNull(Self, "Self");
                Verifier.Assert(tes.TypeArgs.Count == 0);
                return Self;
            }

            if (tes.Definition is TypeParameterDefinition tpd)
            {
                Verifier.Assert(tes.TypeArgs.Count == 0);
                Verifier.Assert(tpd.TypeParameters.Count == 0);

                if (TypeParameterToTypeLookup.ContainsKey(tpd))
                    return TypeParameterToTypeLookup[tpd];

                var tv = GenerateConstrainedTypeVariable(tpd.Constraint);
                return tv;
            }
            
            if (tes.Definition.IsConcept() || tes.Definition.IsConcrete() || tes.Definition.IsPrimitive())
            {
                if (tes.Definition.TypeParameters.Count > 0)
                    return ToTypeList(tes);

                return new TypeConstant(tes.Definition);
            }
            
            if (tes.Definition.IsLibrary())
            {
                throw new Exception("Libraries cannot be variables");
            }
            
            throw new Exception($"Unrecognized type expression {tes}");
        }

        public TypeVariable GenerateTypeVariable(IType constraint)
        {
            constraint = constraint ?? ToSimpleType("Any");
            var r = new TypeVariable(constraint);
            Variables.Add(r);
            return r;
        }

        public TypeVariable GenerateUnconstrainedTypeVariable()
        {
            return GenerateTypeVariable(null);
        }

        public TypeVariable GenerateConstrainedTypeVariable(TypeExpression concept)
        {
            return GenerateTypeVariable(concept != null ? ToIType(concept) : null);
        }

        public int ComputeCardinalityOfFunction(ParameterDefinition pd, Expression body)
        {
            Verifier.Assert(pd.Type.Name == "Function");
            var r = -1;
            foreach (var fc in body.GetExpressionTree().OfType<FunctionCall>())
            {
                if (fc.Function is ParameterReference pr)
                {
                    if (pr.Definition.Equals(pd))
                    {
                        if (r < 0)
                        {
                            r = fc.Args.Count;
                        }

                        if (r != fc.Args.Count)
                        {
                            throw new Exception($"Inconsistent cardinality of function call to {pd} expected {r} at {fc}");
                        }
                    }
                }
            }

            if (r < 0)
                throw new Exception($"Unable to determine cardinality of function {pd}, it never appears to be called");
            
            return r;
        }

        public string ParameterString(ParameterDefinition pd)
        {
            return $"{pd.Name}: {ParameterToTypeLookup[pd]}";
        }

        public string ParametersString()
        {
            return string.Join(", ", Function.Parameters.Select(ParameterString));
        }

        public string Signature
            => $"{Function.OwnerType}.{Function.Name}({ParametersString()}): {DeclaredReturnType}";

        public StringBuilder BuildAnalysisOutput(StringBuilder sb = null)
        {
            sb = sb ?? new StringBuilder();
            sb.AppendLine($"Original function: {Function}");
            sb.AppendLine($"New signature: {Signature}");
            sb.AppendLine($"Variables: {string.Join(", ", Variables)}");
            sb.AppendLine($"Constraints:");
            foreach (var c in Constraints)
                sb.AppendLine($"   {c}");
            return sb;
        }

        public void AddCastConstraint(Expression expr, IType target)
        {
            var src = Process(expr);
            AddConstraint(new CastToConstraint(src, target));
        }

        public void AddConstraint(IConstraint constraint)
        {
            Constraints.Add(constraint);
        }

        public void AddCastConstraint(Expression expr, string name)
        {
            AddCastConstraint(expr, ToSimpleType(name));
        }

        public IType Unify(Expression expr1, Expression expr2)
        {
            // TODO: perform a proper unification.
            var t1 = Process(expr1);
            var t2 = Process(expr2);
            AddConstraint(new UnifyConstraint(t1, t2));
            return t1;
        }

        public IType CreateTuple(IReadOnlyList<IType> children)
        {
            return new TypeList(children.Prepend(ToSimpleType("Tuple")).ToList());
        }

        public IType ToIType(LiteralTypesEnum typeEnum)
        {
            switch (typeEnum)
            {
                case LiteralTypesEnum.Integer:
                    return ToSimpleType("Integer");
                case LiteralTypesEnum.Number:
                    return ToSimpleType("Number");
                case LiteralTypesEnum.Boolean:
                    return ToSimpleType("Boolean");
                case LiteralTypesEnum.String:
                    return ToSimpleType("String");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IType CreateTypeFromLambda(Lambda lambda)
        {
            if (Compiler.FunctionAnalyses.ContainsKey(lambda.Function))
                throw new Exception("The lambdas are not in the list of function definitions");
            
            // TODO: infer the types of the functions 
            return CreateFunctionType(lambda.Function.NumParameters);
        }

        //==================================================================
        // Code for gathering constraints and compute constraints

        public bool IsProcessed { get; private set; }
        public bool IsProcessing { get; private set; }

        public void Process()
        {
            if (IsProcessed || IsProcessing)
                return;

            try
            {
                IsProcessing = true;

                if (Function.Body != null)
                {
                    if (Function.OwnerType.IsConcrete() || Function.OwnerType.IsLibrary())
                        Process(Function.Body);
                }
            }
            finally
            {
                IsProcessing = false;
                IsProcessed = true;
            }
        }

        /// <summary>
        /// Looks up the type of the expression. If it is found, returns it.
        /// If not found computes the type of the child expressions,
        /// then computes the type, then stores it, then returns the type.
        /// If any constraints are found during the process, stores them. 
        /// </summary>
        public IType Process(Expression expr)
        {
            var r = Compiler.GetExpressionType(expr);
            if (r != null)
                return r;

            foreach (var x in expr.GetChildSymbols().OfType<Expression>())
                Process(x);

            switch (expr)
            {
                case Argument argument:
                    r = Process(argument.Expression);
                    break;

                case Assignment assignment:
                    throw new Exception("Assignment not supported");

                case ConditionalExpression conditionalExpression:
                    AddCastConstraint(conditionalExpression.Condition, "Boolean");
                    r = Unify(conditionalExpression.IfTrue, conditionalExpression.IfFalse);
                    break;
                
                case FunctionCall functionCall:
                    r = Process(functionCall);
                    break;
                    
                case FunctionGroupReference functionGroupReference:
                    r = ToSimpleType("Function");
                    break;

                case Lambda lambda:
                    r = CreateTypeFromLambda(lambda);
                    break;
                
                case Literal literal:
                    r = ToIType(literal.TypeEnum);
                    break;
                
                case ParameterReference parameterReference:
                    r = ParameterToTypeLookup[parameterReference.Definition];
                    break;

                case Parenthesized parenthesized:
                    r = Process(parenthesized);
                    break;

                case PredefinedReference predefinedReference:
                    r = ToIType(predefinedReference.Definition.Type)    ;
                    break;

                case Reference reference:
                    throw new NotImplementedException();

                case Tuple tuple:
                    r = CreateTuple(tuple.Children.Select(Process).ToList());
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(expr));
            }

            if (Compiler.ExpressionTypes.TryGetValue(expr, out var tmp))
            {
                //Verifier.Assert(tmp.Equals(r));
                return tmp;
            }
            Compiler.ExpressionTypes.Add(expr, r);
            return r;
        }

        public void GenerateArgConstraint(IType argType, IType paramType)
        {
            if (argType is TypeVariable argTv)
            {
                AddConstraint(new PassFromVariable(paramType, argTv));
            }

            if (paramType is TypeVariable paramTv)
            {
                AddConstraint(new PassToVariable(paramType, paramTv));
            }

            if (argType is TypeList typeListA && paramType is TypeList typeListB)
            {
                for (var i=0; i < Math.Min(typeListA.Children.Count, typeListB.Children.Count); ++i)
                    GenerateArgConstraint(typeListA.Children[i], typeListB.Children[i]);
            }
        }

        public void GenerateCallerConstraint(FunctionCallAnalysis fca, IReadOnlyList<IType> argTypes)
        {
            for (var i=0; i < argTypes.Count; ++i)
            {
                GenerateArgConstraint(argTypes[i], fca.Function.ParameterTypes[i]);
            }
        }

        public IType Process(FunctionCall fc)
        {
            var argTypes = fc.Args.Select(Process).ToList();
            
            var ft = Process(fc.Function);
            AddConstraint(new CallableConstraint(ft, argTypes));

            if (fc.Function is FunctionGroupReference fgr)
            {
                var fca = Compiler.ResolveFunctionGroup(this, fc, fgr, argTypes);
                var bestFunction = fca.BestFunctions.FirstOrDefault();

                // TODO: log this
                if (bestFunction == null)
                    return GenerateUnconstrainedTypeVariable();

                GenerateCallerConstraint(bestFunction, argTypes);
                return bestFunction.DeterminedReturnType;
            }
            
            if (fc.Function is PredefinedReference pr)
            {
                if (pr.Definition.Name == "Tuple")
                    return CreateTuple(argTypes);
                
                throw new Exception($"Unrecognized predefined definition {pr.Definition.Name}");
            }

            if (fc.Function is ParameterReference paramRef)
            {
                // Look at the declared parameter type. If there is none, we are going to create an 
                var tmp = ToIType(paramRef.Type);
                
                // TODO: this should probably be a type list, with a type-variable for each parameter.
                // Accepting the fact that each type-variable can be cast from the appropriate argument. 
                if (tmp == null) 
                    return GenerateUnconstrainedTypeVariable();

                var ret = tmp.GetFunctionReturnType();
                if (ret != null)
                    return ret;

                // This might just be "Function" without more information. 
                return GenerateUnconstrainedTypeVariable();

                // throw new Exception($"Parameter had a type {paramRef.Type}, but it was not the correct type");
            }
            
            throw new NotImplementedException();
        }

        public FunctionCallAnalysis AnalyzeCall(IReadOnlyList<IType> argTypes)
            => new FunctionCallAnalysis(this, argTypes);
    }
}