using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.CSharpWriter;

public partial class TirCSharpBodyWriter
{
    private HashSet<string> _exactOutputNames;

    private bool TryWriteExactOutput(TirLoweredLoop loop, string source, string elem, string count)
    {
        if (!_tw.Writer.ExactOutputAllocation
            || !TirExactOutput.TryGetWidth(loop, _tw.Writer.Compilation, out var width))
            return false;

        var lambda = (TirLambda)loop.Fn;
        var k = loop.Id;
        var n = $"_n{k}";
        var i = $"_i{k}";
        var j = $"_j{k}";
        var part = $"_p{k}";
        var array = $"_a{k}";
        var list = $"_l{k}";
        var names = _exactOutputNames ??= new HashSet<string>(_tir.Parameters.Select(p => p.Name)
            .Concat(_tir.AllNodes.OfType<TirLet>().Select(l => l.Def.Name))
            .Concat(_tir.AllNodes.OfType<TirVariable>().Select(v => v.Def.Name))
            .Concat(_tir.AllNodes.OfType<TirLambda>().SelectMany(l => l.Parameters).Select(p => p.Name))
            .Concat(_tir.AllNodes.OfType<TirInlineBinding>().Select(b => b.Def.Name))
            .Concat(_tir.AllNodes.OfType<TirInlineRegion>().Select(r => r.Result.Name)));
        names.UnionWith(_loopParamNames.Values);
        string Fresh(string name)
        {
            while (!names.Add(name)) name += "_";
            return name;
        }
        var size = Fresh($"_exactSize{k}");
        var offset = Fresh($"_exactOffset{k}");
        var input = _tw.ToCSharpType(TirRewrite.ListElementType(loop.Sources[0].Type));
        WriteLine($"var {n} = {count};");
        WriteLine($"var {size} = (long){n} * {width};");
        WriteLine($"IReadOnlyList<{elem}> {loop.TempName};");
        WriteLine($"if ({source} is {input}[] && {size} <= System.Array.MaxLength)");
        WriteStartBlock();
        WriteLine($"var {array} = new {elem}[(int){size}];");
        WriteLine($"var {offset} = 0;");
        WriteItems($"{array}[{offset}++] = {ListAt(part, j)};");
        WriteLine($"{loop.TempName} = {array};");
        WriteEndBlock();
        WriteLine("else");
        WriteStartBlock();
        WriteLine($"var {list} = new System.Collections.Generic.List<{elem}>();");
        WriteItems($"{list}.Add({ListAt(part, j)});");
        WriteLine($"{loop.TempName} = {list};");
        WriteEndBlock();
        return true;

        void WriteItems(string append)
        {
            WriteLine($"for (var {i} = 0; {i} < {n}; {i}++)");
            WriteStartBlock();
            WriteLine($"var {_loopParamNames[lambda.Parameters[0]]} = {(IsInterfaceList(loop.Sources[0]) ? ListAt(source, i) : $"{source}[{i}]")};");
            // Retain the literal's complete eager evaluation and conversion context.
            Write($"IReadOnlyList<{elem}> {part} = ");
            _lambdaDepth++;
            WriteNode(lambda.Body);
            _lambdaDepth--;
            WriteLine(";");
            WriteLine($"for (var {j} = 0; {j} < {ListCount(part)}; {j}++)");
            WriteStartBlock();
            WriteLine(append);
            WriteEndBlock();
            WriteEndBlock();
        }
    }
}
