// The refinement pass's clones: copies of a function specialized to known cases of its arguments, emitted under derived names.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Optimizer;

namespace Ara3D.Geometry.CppWriter
{
    /// <summary>One clone the refinement pass asked for: the function, the owner it is emitted under, and the facts its body
    /// starts from (<see cref="PassContext.Seed"/>).</summary>
    public sealed class SumClone
    {
        public string Name { get; init; }
        public FunctionDef Callee { get; init; }
        public FunctionInstance Function { get; init; }
        public ConcreteType Owner { get; init; }
        public TirFunction Tir { get; init; }
        public IReadOnlyList<SumFact> Facts { get; init; }
    }

    public partial class CppWriter : ISumRefinementHost
    {
        private readonly object _sumCloneLock = new();
        private readonly Dictionary<string, SumClone> _sumClonesByKey = new(StringComparer.Ordinal);
        private readonly List<SumClone> _sumClones = new();
        private readonly ConcurrentDictionary<object, object> _sumMemo = new();
        private Dictionary<string, ConcreteType> _concreteTypesByName;
        private int _sumClonesEmitted;

        /// <summary>The clones requested so far, in request order.</summary>
        public IReadOnlyList<SumClone> SumClones => _sumClones;

        /// <summary>Requests refused for want of an emission owner or the budget (--refine-clones).</summary>
        public int SumClonesRefused { get; private set; }

        public T Memo<T>(object key, Func<T> compute)
        {
            if (_sumMemo.TryGetValue(key, out var hit))
                return (T)hit;
            var value = compute();
            _sumMemo.TryAdd(key, value);
            return value;
        }

        public string RequestClone(FunctionDef callee, string receiverTypeName, TirFunction calleeTir, IReadOnlyList<SumFact> facts)
        {
            if (!IsCpu || callee == null || calleeTir == null || facts == null || facts.Count == 0)
                return null;
            var key = callee.Name + "#" + FunctionOrdinal(callee) + "@" + receiverTypeName + "|"
                + string.Join(";", facts.Select(f => f.Key + "=" + f.Set.ToString("x")));
            lock (_sumCloneLock)
            {
                if (_sumClonesByKey.TryGetValue(key, out var existing))
                    return existing?.Name;
                var budget = CpuOptions.Switches?.Int(TirSumRefinement.ClonesSwitch, TirSumRefinement.DefaultMaxClones) ?? TirSumRefinement.DefaultMaxClones;
                var owner = ConcreteTypeNamed(receiverTypeName);
                var function = owner == null ? null : InstanceOf(owner, callee);
                if (function == null || _sumClones.Count >= budget)
                {
                    SumClonesRefused++;
                    _sumClonesByKey[key] = null;
                    return null;
                }
                var clone = new SumClone
                {
                    Name = callee.Name + "__r" + Fnv48(key).ToString("x12"),
                    Callee = callee,
                    Function = function,
                    Owner = owner,
                    Tir = calleeTir,
                    Facts = facts.ToList(),
                };
                _sumClonesByKey[key] = clone;
                _sumClones.Add(clone);
                return clone.Name;
            }
        }

        private int FunctionOrdinal(FunctionDef f)
        {
            var list = Compilation.FunctionDefinitions;
            for (var i = 0; i < list.Count; i++)
                if (ReferenceEquals(list[i], f))
                    return i;
            return -1;
        }

        private ConcreteType ConcreteTypeNamed(string name)
        {
            if (name == null)
                return null;
            _concreteTypesByName ??= Compilation.ConcreteTypes.GroupBy(c => c.TypeDef.Name).ToDictionary(g => g.Key, g => g.First());
            return _concreteTypesByName.TryGetValue(name, out var ct) ? ct : null;
        }

        private static FunctionInstance InstanceOf(ConcreteType ct, FunctionDef callee)
            => ct.InterfaceFunctionGroups.SelectMany(g => g).Concat(ct.UnimplementedFunctions).Concat(ct.ConcreteFunctions)
                .FirstOrDefault(f => ReferenceEquals(f.Implementation, callee));

        private static ulong Fnv48(string s)
        {
            var h = 14695981039346656037UL;
            foreach (var ch in s)
            {
                h ^= ch;
                h *= 1099511628211UL;
            }
            return h & 0xFFFFFFFFFFFFUL;
        }

        /// <summary>Emits every clone requested and not yet emitted, in request order; emitting one may request more, and
        /// those are emitted in the same loop. True when anything was emitted.</summary>
        private bool DrainSumClones()
        {
            var any = false;
            while (true)
            {
                SumClone next;
                lock (_sumCloneLock)
                {
                    if (_sumClonesEmitted >= _sumClones.Count)
                        return any;
                    next = _sumClones[_sumClonesEmitted++];
                }
                TryEmitFunction(next.Function, next.Owner, next);
                any = true;
            }
        }
    }
}
