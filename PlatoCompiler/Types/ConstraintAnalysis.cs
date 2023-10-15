using System.Text;

namespace Plato.Compiler.Types
{
    public class ConstraintAnalysis
    {
        public ConstraintAnalysis(FunctionAnalysis function)
        {
            Function = function;
        }

        public StringBuilder Output(StringBuilder sb, string indent)
        {
            sb.AppendLine($"{indent}Constraint analysis for {Function.Signature}");
            sb.AppendLine($"{indent}Function body {Function.Function.Body}");
            sb.AppendLine($"{indent}Has {Function.TypeParameterToTypeLookup.Count} Type parameters ");
            sb.AppendLine($"{indent}Has type parameter in return: {Function.DeclaredReturnType.HasTypeVariable()}");
            sb.AppendLine($"{indent}Found {Function.Constraints.Count} constraints");
            foreach (var c in Function.Constraints)
            {
                sb.AppendLine($"{indent}  Constraint: {c}");
            }

            return sb;
        }

        public FunctionAnalysis Function { get; }
    }
}