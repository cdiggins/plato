using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

public partial class TirCSharpBodyWriter
{
    private void WriteCallbackResult(TirLambda lambda, VariableDef result)
    {
        _lambdaDepth++;
        if (TirRewrite.IsStatementNode(lambda.Body))
            WriteStatement(new TirInlineRegion(result, TirInlineCloner.RedirectReturns(lambda.Body, result)));
        else
        {
            Write($"{CSharpTypeName(result.Type)} {result.Name} = ");
            WriteNode(lambda.Body);
            WriteLine(";");
        }
        _lambdaDepth--;
    }
}
