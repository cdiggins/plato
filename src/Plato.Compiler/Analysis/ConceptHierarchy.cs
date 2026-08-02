using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Analysis
{
    /// <summary>
    /// Interface-lattice helpers shared by LINT016 and the ContextExport hierarchy dump:
    /// redundant direct <c>inherits</c> edges, and an ASCII spanning forest of the DAG.
    /// </summary>
    public static class ConceptHierarchy
    {
        public readonly struct RedundantInherit
        {
            public TypeDef Interface { get; }
            public TypeExpression Parent { get; }
            public TypeExpression ImpliedBy { get; }

            public RedundantInherit(TypeDef iface, TypeExpression parent, TypeExpression impliedBy)
            {
                Interface = iface;
                Parent = parent;
                ImpliedBy = impliedBy;
            }
        }

        public static IReadOnlyList<TypeDef> Interfaces(Compilation compilation)
            => compilation.GetConcepts()
                .OrderBy(c => c.Name, StringComparer.Ordinal)
                .ToList();

        /// <summary>
        /// Direct <c>inherits</c> clauses already reached through another direct parent
        /// of the same interface (same predicate as LINT011, over interface-to-interface edges).
        /// </summary>
        public static IReadOnlyList<RedundantInherit> RedundantInherits(Compilation compilation)
        {
            var results = new List<RedundantInherit>();
            foreach (var iface in Interfaces(compilation))
            {
                if (iface.Inherits.Count < 2)
                    continue;
                foreach (var parent in iface.Inherits)
                {
                    if (parent?.Def == null)
                        continue;
                    var impliedBy = iface.Inherits.FirstOrDefault(other =>
                        other?.Def != null &&
                        other.Def != parent.Def &&
                        other.Def.GetAllImplementedConcepts().Any(i => i.Def == parent.Def));
                    if (impliedBy == null)
                        continue;
                    results.Add(new RedundantInherit(iface, parent, impliedBy));
                }
            }
            return results;
        }

        public static string DisplayName(TypeDef iface)
        {
            if (iface == null)
                return "?";
            if (iface.TypeParameters.Count == 0)
                return iface.Name;
            return $"{iface.Name}<{string.Join(",", iface.TypeParameters.Select(p => p.Name))}>";
        }

        /// <summary>
        /// ASCII spanning forest of the interface DAG. Each interface is printed once under its
        /// first direct parent in declaration order (or as a root if it inherits nothing).
        /// Secondary parents are noted with <c>*</c> on a one-line cross-reference, without
        /// re-expanding the subtree. A trailing section lists redundant inherits.
        /// </summary>
        public static string FormatAscii(Compilation compilation)
        {
            var interfaces = Interfaces(compilation);
            var byDef = interfaces.ToDictionary(c => c);
            var children = new Dictionary<TypeDef, List<TypeDef>>();
            foreach (var c in interfaces)
                children[c] = new List<TypeDef>();

            // Child → ordered list of direct parents (declaration order).
            var parents = new Dictionary<TypeDef, List<TypeExpression>>();
            foreach (var c in interfaces)
            {
                parents[c] = c.Inherits.Where(i => i?.Def != null && byDef.ContainsKey(i.Def)).ToList();
                foreach (var p in parents[c])
                    children[p.Def].Add(c);
            }

            foreach (var list in children.Values)
                list.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));

            // Canonical parent = first declared inherits that resolves; else root.
            var canonicalParent = new Dictionary<TypeDef, TypeDef>();
            var roots = new List<TypeDef>();
            foreach (var c in interfaces)
            {
                var first = parents[c].FirstOrDefault();
                if (first == null)
                    roots.Add(c);
                else
                    canonicalParent[c] = first.Def;
            }

            var sb = new StringBuilder();
            var edgeCount = parents.Values.Sum(p => p.Count);
            var redundant = RedundantInherits(compilation);
            sb.AppendLine($"# Plato interface hierarchy");
            sb.AppendLine($"# {interfaces.Count} interfaces, {edgeCount} direct inherits, {redundant.Count} redundant");
            sb.AppendLine($"# Tree edge = first declared parent; * = also inherits (see multi-parent section)");
            sb.AppendLine();

            var printed = new HashSet<TypeDef>();
            foreach (var root in roots)
                FormatNode(sb, root, children, canonicalParent, parents, printed, prefix: "", isLast: true, isRoot: true);

            // Interfaces whose only parents failed to resolve still need a home.
            foreach (var orphan in interfaces.Where(c => !printed.Contains(c)))
            {
                sb.AppendLine();
                sb.AppendLine($"# unresolved-parent:");
                FormatNode(sb, orphan, children, canonicalParent, parents, printed, prefix: "", isLast: true, isRoot: true);
            }

            var multiParent = interfaces
                .Where(c => parents[c].Count > 1)
                .Select(c => (Interface: c, Parents: parents[c]))
                .ToList();
            if (multiParent.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("## Multi-parent interfaces");
                foreach (var (iface, pars) in multiParent)
                {
                    var names = string.Join(", ", pars.Select(p => p.ToString()));
                    var canonical = canonicalParent.TryGetValue(iface, out var cp) ? DisplayName(cp) : "?";
                    sb.AppendLine($"{DisplayName(iface)} inherits {names}  (tree under {canonical})");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"## Redundant inherits ({redundant.Count})");
            if (redundant.Count == 0)
            {
                sb.AppendLine("(none)");
            }
            else
            {
                foreach (var r in redundant)
                {
                    sb.AppendLine(
                        $"{DisplayName(r.Interface)} inherits {r.Parent}  " +
                        $"# already via {r.ImpliedBy}");
                }
            }

            return sb.ToString().TrimEnd() + Environment.NewLine;
        }

        static void FormatNode(
            StringBuilder sb,
            TypeDef node,
            Dictionary<TypeDef, List<TypeDef>> children,
            Dictionary<TypeDef, TypeDef> canonicalParent,
            Dictionary<TypeDef, List<TypeExpression>> parents,
            HashSet<TypeDef> printed,
            string prefix,
            bool isLast,
            bool isRoot)
        {
            if (!printed.Add(node))
                return;

            var name = DisplayName(node);
            var extraParents = parents[node]
                .Where(p => canonicalParent.TryGetValue(node, out var cp) && p.Def != cp)
                .Select(p => p.ToString())
                .ToList();
            var star = extraParents.Count > 0 ? $" *also {string.Join(", ", extraParents)}" : "";

            if (isRoot)
            {
                sb.AppendLine(name + star);
            }
            else
            {
                sb.Append(prefix);
                sb.Append(isLast ? "`-- " : "+-- ");
                sb.AppendLine(name + star);
            }

            var kids = children[node]
                .Where(ch => canonicalParent.TryGetValue(ch, out var cp) && cp == node)
                .ToList();
            var childPrefix = isRoot ? "" : prefix + (isLast ? "    " : "|   ");
            for (var i = 0; i < kids.Count; i++)
                FormatNode(sb, kids[i], children, canonicalParent, parents, printed,
                    childPrefix, i == kids.Count - 1, isRoot: false);
        }
    }
}
