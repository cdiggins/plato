using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ara3D.Utils;
using Plato.AST;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public class FunctionAnalysis
    {
        public List<TypeVariable> Variables { get; } = new List<TypeVariable>();
        public List<IConstraint> Constraints { get; } = new List<IConstraint>();

        public Compilation Compilation { get; }
        public FunctionDef Def { get; }
        
        public TypeDef OwnerType 
            => Def.OwnerType;
        
        public bool IsConcept 
            => OwnerType.IsConcept();
        
        public bool IsGenericLibraryFunction 
            => OwnerType.IsLibrary() && Variables.Count > 0;

        public Dictionary<ParameterDef, IType> ParameterToTypeLookup { get; } =
            new Dictionary<ParameterDef, IType>();

        public Dictionary<TypeParameterDef, IType> TypeParameterToTypeLookup { get; } =
            new Dictionary<TypeParameterDef, IType>();
            
        public IType DeclaredReturnType { get; }
        public IReadOnlyList<IType> ParameterTypes { get; }
        public IType Self { get; }

        public IType CreateArrayType(IReadOnlyList<Expression> expressions)
        {
            // TODO: we need to get the type of the array from the expressions provided 
            return new TypeList(new[] { ToSimpleType("IArray"), GenerateTypeVariable() });
        }

        public FunctionAnalysis(Compilation compilation, FunctionDef def)
        {
            Compilation = compilation;
            Def = def;

            foreach (var tp in OwnerType.TypeParameters)
            {
                TypeParameterToTypeLookup.Add(tp, ToTypeVariable(tp));
            }

            if (OwnerType.IsConcept())
            {
                Self = GenerateConstrainedTypeVariable(OwnerType.ToTypeExpression());  
            }

            var pTypes  = new List<IType>();
            foreach (var p in def.Parameters)
            {
                var pt = ToIType(p.Type);

                /*
                I used to think that this would be a better way to do things, but resolving against
                type variables with constraints is very complicated. For now I am doing this.
                However, if a function has a concept listed twice, it is the same concept. 

                if (p.Type.Definition.IsConcept())
                {
                    pt = GenerateTypeVariable(pt);
                }
                */

                ParameterToTypeLookup.Add(p, pt);
                pTypes.Add(pt);
            }

            if (Def.ReturnType.Name.StartsWith("Function"))
            {
                var n = Def.ReturnType.TypeArgs.Count;
                if (n == 0)
                    throw new Exception("Functions returning functions are not supported yet because we can't easily determine their cardinality");
                var f = CreateFunctionType(n);
                DeclaredReturnType = f;
            }

            DeclaredReturnType = ToIType(Def.ReturnType);

            if (Def.ReturnType.Def.IsConcept())
            {
                DeclaredReturnType = GenerateTypeVariable(DeclaredReturnType);
            }

            ParameterTypes = pTypes;

            Debug.Assert(ParameterTypes.Count == Def.Parameters.Count);
        }

        public TypeList ToTypeList(IType first, IEnumerable<IType> rest)
            => new TypeList(rest.Prepend(first).ToList());

        public TypeList CreateFunctionType(IReadOnlyList<IType> args, IType returnType)
            =>  args.Count > 10 
                ? throw new NotImplementedException("Only functions up to 10 arguments are supported, if you want more update this function and std.primitives.types.plato")
                : ToTypeList(ToSimpleType($"Function{args.Count}"), args.Append(returnType));

        public IType CreateFunctionType(int n)
            => CreateFunctionType(Enumerable.Range(0, n).Select(_ => GenerateTypeVariable()).ToList(), GenerateTypeVariable());

        public IType ToSimpleType(string name)
            => ToSimpleType(Compilation.GetTypeDefinition(name));

        public IType ToSimpleType(TypeDef td) 
            => td is TypeParameterDef tpd
                ? ToTypeVariable(tpd)
                : new TypeConstant(td);

        public IType ToTypeVariable(TypeParameterDef tpd)
            => TypeParameterToTypeLookup.TryGetValue(tpd, out var r)  
                ? r : GenerateTypeVariable(tpd.Constraints.Select(ToIType).ToArray());

        public IType ToTypeList(TypeExpression expr)
        {
            var tmp = new List<IType> { ToSimpleType(expr.Def) };
            for (var i = 0; i < expr.Def.TypeParameters.Count; ++i)
            {
                var tp = expr.Def.TypeParameters[i];

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

        public IType ToIType(TypeDef td)
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

            if (tes.Def is TypeParameterDef tpd)
            {
                Verifier.Assert(tes.TypeArgs.Count == 0);
                Verifier.Assert(tpd.TypeParameters.Count == 0);

                if (TypeParameterToTypeLookup.ContainsKey(tpd))
                    return TypeParameterToTypeLookup[tpd];

                throw new NotImplementedException();
                //var tv = GenerateConstrainedTypeVariable(tpd.Constraint);
                //return tv;
            }
            
            if (tes.Def.IsConcept() || tes.Def.IsConcrete())
            {
                if (tes.Def.TypeParameters.Count > 0)
                    return ToTypeList(tes);

                return new TypeConstant(tes.Def);
            }
            
            if (tes.Def.IsLibrary())
            {
                throw new Exception("Libraries cannot be variables");
            }

            if (tes.Def.IsTypeVariable())
            {
                return GenerateTypeVariable();
            }
            
            throw new Exception($"Unrecognized type expression {tes}");
        }

        public TypeVariable GenerateTypeVariable(IType constraintType)
        {
            return GenerateTypeVariable(new[] { constraintType });
        }

        public TypeVariable GenerateTypeVariable(IType[] constraintTypes)
        {
            return GenerateTypeVariable(constraintTypes.Select(t => new TypeConstraint(t)).ToArray());
        }

        public TypeVariable GenerateTypeVariable(params TypeConstraint[] typeConstraints)
        {
            var r = new TypeVariable(typeConstraints);
            Variables.Add(r);
            return r;
        }

        public TypeVariable GenerateConstrainedTypeVariable(TypeExpression concept)
        {
            return GenerateTypeVariable(concept != null ? ToIType(concept) : null);
        }
       
        public string ParameterString(ParameterDef pd)
        {
            return $"{pd.Name}: {ParameterToTypeLookup[pd]}";
        }

        public string ParametersString()
        {
            return string.Join(", ", Def.Parameters.Select(ParameterString));
        }

        public string Signature
            => $"{Def.OwnerType}.{Def.Name}({ParametersString()}): {DeclaredReturnType}";

        public StringBuilder BuildAnalysisOutput(StringBuilder sb = null)
        {
            sb = sb ?? new StringBuilder();
            sb.AppendLine($"Original function: {Def}");
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
            if (Compilation.FunctionAnalyses.ContainsKey(lambda.Function))
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

                if (Def.Body is Expression expr)
                {
                    Process(expr);
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
            var r = Compilation.GetExpressionType(expr);
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
                    
                case FunctionGroupRefSymbol functionGroupReference:
                    r = ToSimpleType("IAny");
                    break;

                case Lambda lambda:
                    r = CreateTypeFromLambda(lambda);
                    break;
                
                case Literal literal:
                    r = ToIType(literal.TypeEnum);
                    break;
                
                case ParameterRefSymbol parameterReference:
                    r = ParameterToTypeLookup[parameterReference.Def];
                    break;

                case Parenthesized parenthesized:
                    r = Process(parenthesized);
                    break;

                case VariableRefSymbol variableReference:
                    r = ToIType(variableReference.Def.Type);
                    break;

                case TypeRefSymbol typeReference:
                    r = ToSimpleType("Type");
                    break;

                case RefSymbol reference:
                    throw new NotImplementedException();

                case ArrayLiteral arrayLiteral:
                    r = CreateArrayType(arrayLiteral.Expressions);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(expr));
            }

            if (Compilation.ExpressionTypes.TryGetValue(expr, out var tmp))
            {
                //Verifier.Assert(tmp.Equals(r));
                return tmp;
            }
            Compilation.ExpressionTypes.Add(expr, r);
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

            if (fc.Function is FunctionGroupRefSymbol fgr)
            {
                var fca = Compilation.ResolveFunctionGroup(this, fc, fgr, argTypes);
                var bestFunction = fca.BestFunctions.FirstOrDefault();

                // TODO: fix this ASAP!! 
                // TODO: log this
                if (bestFunction == null)
                    return GenerateTypeVariable();

                GenerateCallerConstraint(bestFunction, argTypes);
                return bestFunction.DeterminedReturnType;
            }
            
            if (fc.Function is ParameterRefSymbol paramRef)
            {
                // Look at the declared parameter type. If there is none, we are going to create an 
                var tmp = ToIType(paramRef.Type);
                
                // TODO: this should probably be a type list, with a type-variable for each parameter.
                // Accepting the fact that each type-variable can be cast from the appropriate argument. 
                if (tmp == null) 
                    return GenerateTypeVariable();

                var ret = tmp.GetFunctionReturnType();
                if (ret != null)
                    return ret;

                // This might just be "Function" without more information. 
                return GenerateTypeVariable();

                // throw new Exception($"Parameter had a type {paramRef.Type}, but it was not the correct type");
            }

            if (fc.Function is TypeRefSymbol typeRef)
            {
                return ToIType(typeRef.Def);
            }
            
            throw new NotImplementedException();
        }

        public FunctionCallAnalysis AnalyzeCall(IReadOnlyList<IType> argTypes)
            => new FunctionCallAnalysis(this, argTypes);
    }
}