using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

public partial class TirCSharpBodyWriter
{
    private void WriteMultiReduction(TirMultiReduction pair)
    {
        var prefix = $"_mc{pair.Id}_";
        var source = prefix + "source";
        var index = prefix + "index";
        var element = prefix + "element";
        Write($"var {source} = ");
        WriteNode(pair.Source);
        WriteLine(";");
        foreach (var binding in new[] { pair.First, pair.Second })
        {
            // Lowered, a scalar accumulator is a primitive local for the whole loop.
            if (Lower && ScalarOf(binding.Value.Type) is { } scalar)
                WritePrimitiveLet(binding.Def, scalar, ((TirCall)binding.Value).Args[1]);
            else
            {
                Write($"{_tw.ToCSharpType(binding.Value.Type)} {CSharpTypeWriter.Identifier(binding.Def.Name)} = ");
                WriteNode(((TirCall)binding.Value).Args[1]);
            }
            WriteLine(";");
        }
        WriteLine($"for (var {index} = 0; {index} < {source}.Count; {index}++)");
        WriteStartBlock();
        WriteLine($"var {element} = {source}[{index}];");
        WriteReductionStep(pair.First, element);
        WriteReductionStep(pair.Second, element);
        WriteEndBlock();
    }

    private void WriteReductionStep(TirLet binding, string element)
    {
        var lambda = (TirLambda)((TirCall)binding.Value).Args[2];
        var accumulator = CSharpTypeWriter.Identifier(binding.Def.Name);
        var saved = new Dictionary<ParameterDef, (string Name, string Scalar)>();
        foreach (var parameter in lambda.Parameters)
            if (_loopParamNames.TryGetValue(parameter, out var previous))
                saved.Add(parameter, (previous, _primitiveParams.TryGetValue(parameter, out var s) ? s : null));
        var primitive = _primitiveLocals.TryGetValue(binding.Def, out var accScalar);
        BindLoopParameter(lambda.Parameters[0], accumulator, primitive ? accScalar : null);
        BindLoopParameter(lambda.Parameters[1], element, null);
        Write($"{accumulator} = ");
        _lambdaDepth++;
        if (primitive)
            WritePrimitive(lambda.Body);
        else
            WriteNode(lambda.Body);
        _lambdaDepth--;
        WriteLine(";");
        foreach (var parameter in lambda.Parameters)
            if (saved.TryGetValue(parameter, out var previous))
                BindLoopParameter(parameter, previous.Name, previous.Scalar);
            else
                UnbindLoopParameter(parameter);
    }
}
