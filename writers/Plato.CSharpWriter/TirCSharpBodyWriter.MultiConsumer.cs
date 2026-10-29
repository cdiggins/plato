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
            Write($"{_tw.ToCSharpType(binding.Value.Type)} {CSharpTypeWriter.Identifier(binding.Def.Name)} = ");
            WriteNode(((TirCall)binding.Value).Args[1]);
            WriteLine(";");
        }
        var isList = IsInterfaceList(pair.Source);
        WriteLine($"for (var {index} = 0; {index} < {(isList ? ListCount(source) : $"{source}.Count")}; {index}++)");
        WriteStartBlock();
        WriteLine($"var {element} = {(isList ? ListAt(source, index) : $"{source}[{index}]")};");
        WriteReductionStep(pair.First, element);
        WriteReductionStep(pair.Second, element);
        WriteEndBlock();
    }

    private void WriteReductionStep(TirLet binding, string element)
    {
        var lambda = (TirLambda)((TirCall)binding.Value).Args[2];
        var accumulator = CSharpTypeWriter.Identifier(binding.Def.Name);
        var saved = new Dictionary<ParameterDef, string>();
        foreach (var parameter in lambda.Parameters)
            if (_loopParamNames.TryGetValue(parameter, out var previous))
                saved.Add(parameter, previous);
        _loopParamNames[lambda.Parameters[0]] = accumulator;
        _loopParamNames[lambda.Parameters[1]] = element;
        Write($"{accumulator} = ");
        _lambdaDepth++;
        WriteNode(lambda.Body);
        _lambdaDepth--;
        WriteLine(";");
        foreach (var parameter in lambda.Parameters)
            if (saved.TryGetValue(parameter, out var previous))
                _loopParamNames[parameter] = previous;
            else
                _loopParamNames.Remove(parameter);
    }
}
