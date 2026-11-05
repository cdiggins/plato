// --cpp-prune-to-roots: the functions and structs the roots reach, closed so that C++ overload resolution in what is
// kept cannot differ from the whole program.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ara3D.Geometry.CppWriter
{
    public partial class CppWriter
    {
        private static readonly Regex CppIdentifier = new(@"[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);

        /// <summary>The structs written when pruning; null writes all of them.</summary>
        private HashSet<string> _keptStructs;

        private readonly List<string> _unknownPruneRoots = new List<string>();

        /// <summary><paramref name="live"/> reduced to what the roots reach; unchanged without --cpp-prune-to-roots.</summary>
        private List<Emitted> KeptFromRoots(List<Emitted> live)
        {
            if (CpuOptions.PruneRoots == null)
                return live;
            var (kept, structs) = Reached(live);
            _keptStructs = structs;
            var written = live.Where(e => !e.Provided).SelectMany(e => new[] { e.Name, DeclaredName(e.Prototype) }).ToHashSet();
            _unknownPruneRoots.AddRange(Roots().Where(r => !written.Contains(r) && !AllCpuStructs().Any(s => s.Name == r)).OrderBy(r => r, StringComparer.Ordinal));
            return live.Where(kept.Contains).ToList();
        }

        private HashSet<string> Roots() => CpuOptions.Roots.Concat(CpuOptions.PruneRoots).ToHashSet();

        private static IEnumerable<string> CppIdentifiers(string text)
            => CppIdentifier.Matches(text).Select(m => m.Value);

        /// <summary>Where a prototype's parameter list opens: the first parenthesis outside angle brackets, or -1.</summary>
        private static int ParameterListStart(string prototype)
        {
            var depth = 0;
            for (var i = 0; i < prototype.Length; i++)
            {
                if (prototype[i] == '<') depth++;
                else if (prototype[i] == '>') depth--;
                else if (prototype[i] == '(' && depth == 0) return i;
            }
            return -1;
        }

        /// <summary>The name a prototype declares.</summary>
        private static string DeclaredName(string prototype)
        {
            var open = ParameterListStart(prototype);
            return open < 0 ? null : CppIdentifiers(prototype.Substring(0, open)).LastOrDefault();
        }

        /// <summary>The struct names among a prototype's parameter types.</summary>
        private static HashSet<string> ParameterStructs(string prototype, HashSet<string> structs)
        {
            var open = ParameterListStart(prototype);
            return open < 0 ? new HashSet<string>() : CppIdentifiers(prototype.Substring(open)).Where(structs.Contains).ToHashSet();
        }

        /// <summary>
        /// The functions and structs the roots reach. The closure runs over identifiers in the written text, not the
        /// recorded call sites, because C++ picks among overloads by name: a function is kept when kept text names it and
        /// every struct among its parameter types is kept, so a call sees the same candidates as in the whole program
        /// (no value of a dropped struct exists). A root keeps every overload of its name. Names the prelude spells are
        /// demanded from the start, since its templates reach generated functions by argument-dependent lookup. A kept
        /// struct keeps the structs its definition names, and each resolved call keeps its target. The result is a least
        /// fixed point, so it does not depend on the order of visits.
        /// </summary>
        private (HashSet<Emitted> Kept, HashSet<string> Structs) Reached(List<Emitted> emitted)
        {
            var structText = AllCpuStructs().ToDictionary(s => s.Name, s =>
            {
                var sb = new StringBuilder();
                AppendCpuStruct(sb, s);
                return sb.ToString();
            });
            var structs = structText.Keys.ToHashSet();
            var roots = Roots();

            var names = roots.Concat(roots.Select(FunctionName)).Concat(CppCpuPrelude.FilesFor(CpuOptions).SelectMany(f => CppIdentifiers(f.Value))).ToHashSet();
            var keptStructs = names.Where(structs.Contains).ToHashSet();
            var kept = new HashSet<Emitted>();
            var pending = new Queue<Emitted>();
            void Keep(Emitted e)
            {
                if (!e.Provided && kept.Add(e))
                    pending.Enqueue(e);
            }

            var declared = new Dictionary<Emitted, string>();
            var parameters = new Dictionary<Emitted, HashSet<string>>();
            var byKey = new Dictionary<string, Emitted>();
            var byName = Array.Empty<Emitted>().ToLookup(e => e.Name);
            var indexed = 0;
            void Index()
            {
                for (; indexed < emitted.Count; indexed++)
                {
                    var e = emitted[indexed];
                    byKey.TryAdd(e.Key, e);
                    if (e.Provided)
                        continue;
                    declared[e] = DeclaredName(e.Prototype);
                    parameters[e] = ParameterStructs(e.Prototype, structs);
                }
                byName = emitted.Where(e => e.Label != "generated.template").ToLookup(e => e.Name);
            }

            Index();
            var visitedStructs = new HashSet<string>();
            bool changed;
            do
            {
                changed = false;
                foreach (var s in keptStructs.Where(visitedStructs.Add).ToList())
                {
                    keptStructs.UnionWith(CppIdentifiers(structText[s]).Where(structs.Contains));
                    changed = true;
                }
                foreach (var e in emitted)
                    if (!e.Provided && (roots.Contains(e.Name) || roots.Contains(declared[e])
                        || (names.Contains(declared[e]) && parameters[e].IsSubsetOf(keptStructs))))
                        Keep(e);
                while (pending.Count > 0)
                {
                    changed = true;
                    var e = pending.Dequeue();
                    foreach (var id in CppIdentifiers(e.Definition))
                    {
                        names.Add(id);
                        if (structs.Contains(id))
                            keptStructs.Add(id);
                    }
                    foreach (var c in e.Callees)
                    {
                        var target = FindCpu(c, byKey, byName, true);
                        if (target != null)
                            Keep(target);
                    }
                }
            } while (changed);
            return (kept, keptStructs);
        }
    }
}
