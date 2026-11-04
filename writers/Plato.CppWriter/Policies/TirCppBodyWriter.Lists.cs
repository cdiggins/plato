// List creation in the Cpu dialect goes through the list-builder policy (TKT-1183, --presize).
namespace Ara3D.Geometry.CppWriter
{
    public partial class TirCppBodyWriter
    {
        /// <summary>Writes <c>new List&lt;T&gt;()</c> or <c>new List&lt;T&gt;(capacity)</c> as the policy spells it.</summary>
        private void WriteListCreation(string cppType, System.Collections.Generic.IReadOnlyList<Ara3D.Geometry.Compiler.Checking.TirNode> args)
        {
            var (open, close) = _w.Policies.Lists.Create(cppType, args.Count == 1);
            Write(open);
            if (args.Count == 1)
                WriteNode(args[0]);
            Write(close);
        }
    }
}
