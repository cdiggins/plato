using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Plato.Compiler.Symbols;
using Ptarmigan.Utils;

namespace Plato.Compiler.Types
{
    public interface IType
    {
    }

    public class TypeVariable : IType
    {
        public int Id { get; } = NextId++;
        public static int NextId;

        public override string ToString()
            => $"${Id}";
    }

    public class TypeConstant : IType
    {
        public TypeDefinition TypeDefinition { get; }

        public TypeConstant(TypeDefinition tds)
        {
            Verifier.Assert(tds.IsConcept() || tds.IsPrimitive() || tds.IsConcreteType());
            TypeDefinition = tds;
        }

        public override string ToString()
            => TypeDefinition.Name;
    }

    public class TypeList : IType
    {
        public TypeList(IReadOnlyList<IType> children)
            => Children = children;

        public IReadOnlyList<IType> Children { get; }

        public override string ToString()
            => $"({string.Join(", ", Children)})"; 
    }

    public class Constraint
    {
        public TypeVariable Variable { get; }

        public Constraint(TypeVariable tv)
            => Variable = tv;
    }
    
    public class ImplementsConstraint : Constraint
    {
        public ImplementsConstraint(TypeVariable tv, IType implemented)
            : base(tv)
        {
            Implemented = implemented;
        }

        public IType Implemented { get; }

        public override string ToString()
        {
            return $"{Variable} implements {Implemented}";
        }
    }

    public class FunctionAnalysis
    {
        public List<TypeVariable> Variables { get; } = new List<TypeVariable>();
        public List<Constraint> Constraints { get; } = new List<Constraint>();

        public Compiler Compiler { get; }
        public FunctionDefinition Function { get; }

        public Dictionary<ParameterDefinition, IType> ParameterTypeLookup { get; } =
            new Dictionary<ParameterDefinition, IType>();
        
        public IType ReturnType { get; }
        public TypeList Parameters { get; }

        public FunctionAnalysis(Compiler compiler, FunctionDefinition function)
        {
            Compiler = compiler;
            Function = function;

            var pTypes  = new List<IType>();
            foreach (var p in function.Parameters)
            {
                // Try to figure out the cardinality of each parameter. 
                // For now, we are only really worried about functions.
                // And I think we can just look at how things are called. 

                if (p.Name == "Function")
                {
                    var n = ComputeCardinalityOfFunction(p, function.Body);
                    var f = CreateFunctionType(n);
                    ParameterTypeLookup.Add(p, f);
                    pTypes.Add(f);
                }
                else if (p.Name == "Tuple")
                {
                    throw new Exception("Tuples not supported");
                }
                else
                {
                    var pt = ToIType(p.Type);
                    ParameterTypeLookup.Add(p, pt);
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

        public IType CreateFunctionType(int n)
        {
            var args = Enumerable.Range(0, n + 1).Select(_ => GenerateTypeVariable());
            var funcDef = ToIType(Compiler.GetTypeDefinition("Function"));
            return new TypeList(args.Prepend(funcDef).ToList());
        }

        // TODO: I am confident that this will fail. When referring to a type, there might be references to type parameters .
        public bool IsTypeExpressionComplete(TypeExpression tes)
        {
            return tes.TypeArgs.Count == tes.Definition.TypeParameters.Count
                   && !tes.Definition.IsTypeVariable()
                   && tes.TypeArgs.All(IsTypeExpressionComplete);
        }

        public IType ToIType(TypeDefinition td)
        {
            if (td is TypeParameterDefinition tpd)
            {
                return GenerateConstrainedTypeVariable(tpd.Constraint);
            }
            else {
                return new TypeConstant(td);
            }
        }

        public IType ToTypeList(TypeExpression expr)
        {
            var tmp = new List<IType>();
            tmp.Add(ToIType(expr.Definition));
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
                    tmp.Add(ToIType(tp));
                }
            }

            return new TypeList(tmp);
        }

        public IType ToIType(TypeExpression tes)
        {
            if (tes.Definition is TypeParameterDefinition tpd)
            {
                Verifier.Assert(tes.TypeArgs.Count == 0);
                Verifier.Assert(tpd.TypeParameters.Count == 0);
                var tv = GenerateConstrainedTypeVariable(tpd.Constraint);
                return tv;
            }
            else if (tes.Definition.IsConcept())
            {
                var tv = GenerateTypeVariable();
                var constraint = ToTypeList(tes);
                Constraints.Add(new ImplementsConstraint(tv, constraint));
                return tv; 
            }
            else if (tes.Definition.IsConcreteType() || tes.Definition.IsPrimitive())
            {
                if (tes.Definition.TypeParameters.Count > 0)
                    return ToTypeList(tes);
                else
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
                Constraints.Add(new ImplementsConstraint(tv, newType));
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

        public string Signature
            => $"{Parameters} -> {ReturnType}";

        public StringBuilder BuildAnalysisOuput(StringBuilder sb = null)
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
    }
}