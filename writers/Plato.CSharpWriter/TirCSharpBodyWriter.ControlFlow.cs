namespace Ara3D.Geometry.CSharpWriter;

public partial class TirCSharpBodyWriter
{
    private void WriteGuardedLoop(TirGuardedLoop loop)
    {
        WriteLine("while (true)");
        WriteStartBlock();
        foreach (var statement in loop.Test.Statements)
            WriteStatement(statement);
        Write("if (!(bool)(");
        WriteNode(loop.Test.Value);
        WriteLine(")) break;");
        WriteStatement(loop.Body);
        WriteEndBlock();
    }
}
