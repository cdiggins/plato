// Runs the shared optimizer passes (inline, fuse, loops, exact output) on one function's TIR for the Cpu dialect.
// The passes take a CSharpWriter host, so this class owns one private CSharpWriter over the same compilation,
// configured the way GenerateCommand configures it for C#. It is never asked to write a file.
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Types;
using CSharpHost = Ara3D.Geometry.CSharpWriter.CSharpWriter;

namespace Ara3D.Geometry.CppWriter
{
    public static class CppTirPipeline
    {
        public static bool IsCpu(CppWriter w) => w.Dialect == CppDialect.Cpu;

        private static CSharpHost _host;
        private static Compiler.Compilation _hostCompilation;
        private static CppCpuOptions _hostOptions;

        /// <summary>The private C# writer the shared passes run against; built on first use, rebuilt when the compilation or options change.</summary>
        private static CSharpHost Host(CppWriter w)
        {
            var options = w.CpuOptions;
            if (_host != null && ReferenceEquals(_hostCompilation, w.Compilation) && _hostOptions == options)
                return _host;
            _host = new CSharpHost(w.Compilation, w.OutputFolder)
            {
                Profile = NumberProfile.TryParse("cpu64i32") ?? NumberProfile.Default,
                InlineCalls = true,
                LowerLoops = options.Loops,
                FuseCombinators = options.Fuse,
                FuseConsumers = options.FuseConsumers,
                ExactOutputAllocation = options.ExactOutput,
            };
            _hostCompilation = w.Compilation;
            _hostOptions = options;
            return _host;
        }

        /// <summary>
        /// Returns the TIR after the passes the options select, in the C# writer's order
        /// (inline, fuse, consumer fusion, loop lowering); each pass gates itself on the host's flag.
        /// Exact output is decided by the body writer from the lowered loops. Other dialects: the identity.
        /// </summary>
        public static TirFunction Run(TirFunction tir, CppWriter w, string ownerTypeName)
        {
            if (tir == null || !IsCpu(w))
                return tir;
            var host = Host(w);
            tir = Ara3D.Geometry.CSharpWriter.TirInliner.Inline(tir, host, ownerTypeName, out _);
            tir = Ara3D.Geometry.CSharpWriter.TirFusion.Rewrite(tir, host);
            tir = Ara3D.Geometry.CSharpWriter.TirMultiConsumerFusion.Rewrite(tir, host);
            tir = Ara3D.Geometry.CSharpWriter.TirLoopLowerer.Rewrite(tir, host);
            return tir;
        }
    }
}
