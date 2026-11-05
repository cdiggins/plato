// Runs the Cpu pipeline over the member bodies on several threads before the ordered emission loop reads them.
using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Utilities;

namespace Ara3D.Geometry.CppWriter
{
    public partial class CppWriter
    {
        // Optimized member bodies waiting for TryEmitFunction, by source body and owner. A body the prepass did not
        // reach, or whose pipeline threw, is simply absent, and TryEmitFunction runs the pipeline itself.
        private readonly Dictionary<(TirFunction, string), TirFunction> _optimizedBodies = new();

        /// <summary>The pipeline is a function of the body and its owner: its counters are per thread and restart per
        /// body (TKT-1137), and it reads the immutable TIR source, so a result does not depend on emission order. The
        /// claims, the emitted list and the skip log stay in the sequential loop.</summary>
        private void OptimizeMemberBodies(IEnumerable<ConcreteType> concreteTypes, ISet<ConcreteType> uniqueOwners)
        {
            var bodies = concreteTypes.Where(EmitsMembers)
                .SelectMany(ct => MemberCandidates(ct, uniqueOwners).Select(f => (Tir: SourceTir(f, ct), Owner: ct.TypeDef.Name)))
                .Where(b => b.Tir != null)
                .Distinct()
                .ToList();
            CpuPipeline.Context(this);
            var optimized = OrderedParallel.Map(bodies, b => TryOptimize(b.Tir, b.Owner));
            for (var i = 0; i < bodies.Count; i++)
                if (optimized[i] != null)
                    _optimizedBodies.Add(bodies[i], optimized[i]);
        }

        private TirFunction TryOptimize(TirFunction tir, string owner)
        {
            try { return CppTirPipeline.Run(tir, this, owner); }
            catch (Exception) { return null; }
        }

        /// <summary>The body the prepass left for this owner, or the pipeline's result now. Each result is handed out once,
        /// so a body emitted twice gets two separate trees, as it would without the prepass.</summary>
        private TirFunction OptimizedBody(TirFunction tir, string owner)
            => _optimizedBodies.Remove((tir, owner), out var optimized) ? optimized : CppTirPipeline.Run(tir, this, owner);

        private TirFunction SourceTir(FunctionInstance fi, ConcreteType ct)
            => fi.Implementation?.Body == null || fi.ParameterNames.Count == 0
                ? null
                : TirSource.TryGetGroundTir(fi.Implementation, ct.TypeDef) ?? TirSource.TryGetStaticTir(fi.Implementation);

        /// <summary>The functions the emission loop tries for a type, before it knows which members
        /// <see cref="EmitArrayLikeMembers"/> handled. A few bodies optimized here are never emitted.</summary>
        private IEnumerable<FunctionInstance> MemberCandidates(ConcreteType ct, ISet<ConcreteType> uniqueOwners)
        {
            if (uniqueOwners.Contains(ct))
                return ct.ConcreteFunctions.Where(f => !SkipFunction(ct, f));
            return ct.InterfaceFunctionGroups.Select(TryChooseBest).Where(f => f != null)
                .Concat(ct.UnimplementedFunctions)
                .Where(f => !SkipFunction(ct, f));
        }

        private FunctionInstance TryChooseBest(IReadOnlyList<FunctionInstance> group)
        {
            try { return Analyzer.ChooseBestFunction(group, out _); }
            catch (Exception) { return null; }
        }
    }
}
