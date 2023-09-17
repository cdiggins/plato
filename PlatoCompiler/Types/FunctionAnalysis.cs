using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Plato.Compiler.Ast;
using Plato.Compiler.Symbols;
using Ptarmigan.Utils;
using Tuple = Plato.Compiler.Symbols.Tuple;

namespace Plato.Compiler.Types
{
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

        public IType ReturnType { get; }
        public TypeList Parameters { get; }
        public TypeList Self { get; }

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
                Self = ToTypeList(OwnerType);

                // Make sure that the type parameters lookup are used in the self type. 
                Verifier.Assert(OwnerType.TypeParameters.Count + 1 == Self.Children.Count);
                for (var i = 0; i < OwnerType.TypeParameters.Count; i++)
                {
                    var lookup = TypeParameterToTypeLookup[OwnerType.TypeParameters[i]];
                    Verifier.Assert(Self.Children[i + 1].Equals(lookup));
                }
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
                else if (p.Type.Name == "Tuple")
                {
                    throw new Exception("Tuples not supported");
                }
                else
                {
                    var pt = ToIType(p.Type);
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
                ReturnType = f;
            }

            ReturnType = ToIType(Function.ReturnType);
            Parameters = new TypeList(pTypes);
        }

        public TypeList ToTypeList(IType first, IEnumerable<IType> rest)
            => new TypeList(rest.Prepend(first).ToList());

        public TypeList CreateFunctionType(IEnumerable<IType> args)
            => ToTypeList(ToSimpleType("Function"), args);

        public IType CreateFunctionType(int n)
            => CreateFunctionType(Enumerable.Range(0, n + 1).Select(_ => GenerateTypeVariable()));

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

        public TypeList ToTypeList(TypeDefinition td)
            => ToTypeList(ToSimpleType(td), td.TypeParameters.Select(ToTypeVariable));

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
            else if (tes.Definition.IsConcept())
            {
                var tv = GenerateTypeVariable();
                var constraint = ToTypeList(tes);
                Constraints.Add(new CastTypeVariableConstraint(tv, constraint));
                return tv; 
            }
            else if (tes.Definition.IsConcreteType() || tes.Definition.IsPrimitive())
            {
                if (tes.Definition.TypeParameters.Count > 0)
                    return ToTypeList(tes);
                return new TypeConstant(tes.Definition);
            }
            else if (tes.Definition.IsLibrary())
            {
                throw new Exception("Libraries cannot be variables");
            }
            else
            {
                throw new Exception($"Unrecognized type expression {tes}");
            }
        }

        public TypeVariable GenerateTypeVariable()
        {
            var r = new TypeVariable();
            Variables.Add(r);
            return r;
        }

        public TypeVariable GenerateConstrainedTypeVariable(TypeExpression concept)
        {
            var tv = GenerateTypeVariable();
            if (concept != null)
            {
                Verifier.Assert(concept.Definition.IsConcept());
                var newType = ToTypeList(concept);
                Constraints.Add(new CastTypeVariableConstraint(tv, newType));
            }

            return tv;
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
            => $"{Function.OwnerType}.{Function.Name}({ParametersString()}): {ReturnType}";

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

        public void AddConstraint(Expression expr, TypeExpression type)
        { }

        public void AddConstraint(Expression expr, TypeDefinition type)
        { }

        public void AddConstraint(Expression expr, string name)
        {
            AddConstraint(expr, Compiler.GetTypeDefinition("Boolean"));
        }

        public IType Unify(Expression expr1, Expression expr2)
        {
            throw new NotImplementedException();
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
            return CreateFunctionType(lambda.Function.GetParameterTypes().Select(ToIType));
        }

        public IType ResolveType(Expression expr)
        {
            var t = Compiler.GetType(expr);
            var r = t == null ? null : ToIType(t);
            if (r != null)
                return r;

            foreach (var x in expr.GetChildSymbols().OfType<Expression>())
                ResolveType(x);

            switch (expr)
            {
                case Argument argument:
                    r = ResolveType(argument.Expression);
                    break;

                case Assignment assignment:
                    throw new Exception("Assignment not supported");

                case ConditionalExpression conditionalExpression:
                    AddConstraint(conditionalExpression.Condition, "Boolean");
                    r = Unify(conditionalExpression.IfTrue, conditionalExpression.IfFalse);
                    break;
                
                case FunctionCall functionCall:
                    // TODO: 
                    throw new NotImplementedException();

                case FunctionGroupReference functionGroupReference:
                    // TODO: this is a function. 
                    throw new NotImplementedException();

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
                    r = ResolveType(parenthesized);
                    break;

                case PredefinedReference predefinedReference:
                    throw new NotImplementedException();

                case Reference reference:
                    throw new NotImplementedException();

                case Tuple tuple:
                    throw new NotImplementedException();
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(expr));
            }

            return r;
        }
    }
}