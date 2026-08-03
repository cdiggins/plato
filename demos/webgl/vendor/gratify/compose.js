// ============================================================================
// Composition + body expansion. `composeDef` resolves an element's effective
// definition through all three layering scopes (definition → theme extensions,
// ancestry-aware → use-site extensions) — the same order EffCache uses, now
// shared so the `body` facet layers identically. `expandBodies` is a pure
// pre-pass over the element tree that runs each composite's `body`, turning a
// part-made-of-parts into an ordinary keyed element tree the kernel already
// knows how to reconcile/layout/animate/draw. It runs only on state changes
// (or a theme bump), O(tree) — the same class as `view` itself.
// ============================================================================
import { activeThemeExts } from "./theme.js";
/** The effective definition of an element after definition → theme → use-site
 *  extensions. Pure; used by both EffCache (per instance) and body expansion. */
export function composeDef(el) {
    let def = el.part;
    for (const e of activeThemeExts(def.name, def.ancestors))
        def = e(def);
    for (const e of el.exts ?? [])
        def = e(def);
    return def;
}
/** Expand composites: replace each element's children with its `body` output
 *  (use-site children become the body's input slot), recursively. A depth
 *  guard turns accidental self-recursion into a console error, not a hang.
 *  Runs on the state clock only — a `body` may read local state (via the
 *  reader) but never channels. */
export function expandBodies(el, getLocal, path = [el.key], depth = 0) {
    if (depth > 64) {
        console.error(`gratify: body expansion too deep at "${el.key}" — a part likely emits itself`);
        return el;
    }
    const def = composeDef(el);
    const kids = def.body
        ? def.body(el.props, el.children ?? [], getLocal?.(path) ?? def.localInit)
        : el.children;
    return kids?.length
        ? { ...el, children: kids.map((k) => expandBodies(k, getLocal, [...path, k.key], depth + 1)) }
        : el;
}
//# sourceMappingURL=compose.js.map