// Runs the shared optimizer passes on one function's TIR for the Cpu dialect: the C++ pass list, by name.
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CppWriter
{
    /// <summary>
    /// The Cpu dialect's optimizer pipeline. <see cref="PassOrder"/> names the shared passes (Plato.Optimizer) the C++
    /// writer runs, in the C# writer's relative order; each pass gates itself on its switch, so the list can name a
    /// pass whose switch is off. The switches are <see cref="CppCpuOptions.Switches"/> (the CLI table's resolved set,
    /// whose C++ rows name the Cpu target) with the four loop options of <see cref="CppCpuOptions"/> on top.
    /// Porting a shared pass to C++ is adding its name here and the Cpu target to its switch's row.
    /// </summary>
    public sealed class CppTirPipeline
    {
        public static bool IsCpu(CppWriter w) => w.Dialect == CppDialect.Cpu;

        /// <summary>The passes the Cpu dialect runs, in order.</summary>
        public static readonly IReadOnlyList<string> PassOrder = new[]
        {
            SharedPasses.Inline, SharedPasses.Fuse, SharedPasses.FuseConsumers, SharedPasses.ShareCalls, SharedPasses.LowerLoops,
        };

        private static readonly IReadOnlyList<ITirPass> Passes = PassRegistry.Shared.Resolve(PassOrder);

        private PassContext _context;
        private Compiler.Compilation _contextCompilation;
        private CppCpuOptions _contextOptions;

        /// <summary>The switches the options select: the resolved set they came from, if any, with the loop options as given.</summary>
        public static SwitchSet SwitchesOf(CppCpuOptions options)
        {
            var s = options.Switches?.Copy() ?? new SwitchSet();
            s.Set("loops", options.Loops);
            s.Set("fuse", options.Fuse);
            s.Set("fuse-consumers", options.FuseConsumers);
            s.Set("exact-output", options.ExactOutput);
            s.Set("share-calls", options.ShareCalls);
            return s;
        }

        /// <summary>The pass context for the writer's current compilation and options; rebuilt when either changes.</summary>
        internal PassContext Context(CppWriter w)
        {
            var options = w.CpuOptions;
            if (_context != null && ReferenceEquals(_contextCompilation, w.Compilation) && _contextOptions == options)
                return _context;
            _context = new PassContext(w.Compilation, SwitchesOf(options), new InlineHost(w), new PassStats(), w);
            _contextCompilation = w.Compilation;
            _contextOptions = options;
            return _context;
        }

        /// <summary>
        /// Returns the TIR after the passes of <see cref="PassOrder"/>. Exact output is decided by the body writer from the
        /// lowered loops. The pass counters restart for this body and the caller's are restored after it, so another
        /// writer on this thread cannot affect emitted names. Other dialects: the identity.
        /// </summary>
        public static TirFunction Run(TirFunction tir, CppWriter w, string ownerTypeName)
        {
            if (tir == null || !IsCpu(w))
                return tir;
            var ctx = w.CpuPipeline.Context(w).ForBody(ownerTypeName);
            using (PassCounters.Scope())
                return PassRegistry.Run(tir, Passes, ctx);
        }

        /// <summary>
        /// The inliner's view of the C++ writer. TIR lookups go to the writer's own <c>TirEmitSource</c>, so the library is
        /// type-checked and monomorphized once per generation (TKT-1172). The type-name answers are the ones the
        /// pipeline's former private C# host gave (it never built extension plans): only the C# scalar names are
        /// concrete, and member names are not known. Kept for byte-identical output; whether the writer's own answers
        /// (<see cref="CppWriter.IsConcreteTypeName"/>) should replace them is a measured change of its own.
        /// </summary>
        private sealed class InlineHost : ITirInlineHost
        {
            private static readonly HashSet<string> ScalarNames = new() { "Number", "Integer", "Boolean", "Character", "String" };
            private readonly CppWriter _writer;

            public InlineHost(CppWriter writer) => _writer = writer;

            public bool InlineCalls => true;
            public bool InlineScalarBodies => false;
            public InlineReport InlineReport => null;
            public Compiler.Compilation Compilation => _writer.Compilation;

            public TirFunction TryGetGroundTirByTypeName(FunctionDef original, string concreteTypeName)
                => _writer.TryGetGroundTirByTypeName(original, concreteTypeName);

            public TirFunction TryGetStaticTir(FunctionDef original) => _writer.TryGetStaticTir(original);

            public bool IsConcreteTypeName(string name) => name != null && ScalarNames.Contains(name);

            public bool IsScalarPrimitiveName(string name) => name != null && ScalarNames.Contains(name);

            public ISet<string> TryGetKnownMemberNames(string typeName) => null;
        }
    }
}
