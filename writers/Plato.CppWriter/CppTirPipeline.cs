// Runs the shared optimizer passes (inline, fuse, loops, exact output) on one function's TIR for the Cpu dialect.
// The passes take a CSharpWriter host, so each CppWriter owns one private CSharpWriter over the same compilation,
// configured the way GenerateCommand configures it for C#. It is never asked to write a file.
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.CSharpWriter;
using CSharpHost = Ara3D.Geometry.CSharpWriter.CSharpWriter;

namespace Ara3D.Geometry.CppWriter
{
    public sealed class CppTirPipeline
    {
        public static bool IsCpu(CppWriter w) => w.Dialect == CppDialect.Cpu;

        private CSharpHost _host;
        private Compiler.Compilation _hostCompilation;
        private CppCpuOptions _hostOptions;

        /// <summary>The private C# writer the shared passes run against; built on first use, rebuilt when the compilation or options change.</summary>
        private CSharpHost Host(CppWriter w)
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
                ShareCalls = options.ShareCalls,
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
            var host = w.CpuPipeline.Host(w);
            // Shared passes expose thread-local counters. Scope them to this body and restore
            // the caller's state so another writer on this thread cannot affect emitted names.
            var renameId = TirInliner.NextRenameId;
            var fusionId = TirFusion.NextId;
            var loopId = TirLoopLowerer.NextId;
            TirInliner.NextRenameId = TirFusion.NextId = TirLoopLowerer.NextId = 0;
            try
            {
                tir = TirInliner.Inline(tir, host, ownerTypeName, out _);
                tir = TirFusion.Rewrite(tir, host);
                tir = TirMultiConsumerFusion.Rewrite(tir, host);
                tir = TirSharedCalls.Rewrite(tir, host);
                return TirLoopLowerer.Rewrite(tir, host);
            }
            finally
            {
                TirInliner.NextRenameId = renameId;
                TirFusion.NextId = fusionId;
                TirLoopLowerer.NextId = loopId;
            }
        }
    }
}
