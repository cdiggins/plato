using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

public partial class TirCSharpBodyWriter
{
    private static bool IsConcreteFlatMapType(TypeExpression type)
        => TirLoopLowerer.IsRenderableConcrete(type) && !type.IsTypeParameter
            && !type.IsSelfType() && type.TypeArgs.All(IsConcreteFlatMapType);

    private bool TryWriteFlatMapConditional(TirLoweredLoop loop, string list, string part, string index, string elem)
    {
        // Do not strip coercions: their original target context can perform a conversion.
        if (loop.Fn is not TirLambda { Body: TirConditional conditional } lambda
            || lambda.Type?.Name != "Function1" || lambda.Type.TypeArgs.Count != 2
            || !TirRewrite.IsListType(conditional.Type) || conditional.Type.TypeArgs.Count != 1
            || !IsConcreteFlatMapType(loop.ElemType)
            || !loop.ElemType.Equals(conditional.Type.TypeArgs[0])
            || !conditional.Type.Equals(loop.Type)
            || !conditional.Type.Equals(lambda.Type.TypeArgs[1])
            || !conditional.Type.Equals(conditional.IfTrue.Type)
            || !conditional.Type.Equals(conditional.IfFalse.Type)
            || conditional.IfTrue is TirCoerce || conditional.IfFalse is TirCoerce)
            return false;

        bool IsSingleton(TirNode node)
            => node is TirArray { Elements.Count: 1 } array
                && loop.ElemType.Equals(array.Elements[0].Type);

        if (!IsSingleton(conditional.IfTrue) && !IsSingleton(conditional.IfFalse))
            return false;

        void WriteArm(TirNode arm)
        {
            WriteStartBlock();
            if (IsSingleton(arm))
            {
                // List<T>.Add supplies the same element conversion context as MakeArray<T>.
                Write($"{list}.Add(");
                WriteNode(((TirArray)arm).Elements[0]);
                WriteLine(");");
            }
            else
            {
                Write($"IReadOnlyList<{elem}> {part} = ");
                WriteNode(arm);
                WriteLine(";");
                WriteLine($"for (var {index} = 0; {index} < {part}.Count; {index}++)");
                WriteStartBlock();
                WriteLine($"{list}.Add({part}[{index}]);");
                WriteEndBlock();
            }
            WriteEndBlock();
        }

        _lambdaDepth++;
        Write("if (");
        WriteBoolTest(conditional.Condition);
        WriteLine(")");
        WriteArm(conditional.IfTrue);
        WriteLine("else");
        WriteArm(conditional.IfFalse);
        _lambdaDepth--;
        return true;
    }
}
