// ============================================================================
// Gratify extension algebra — "wrap, don't edit" (README §3, layering guide).
// An extension is an ordinary function PartDef → PartDef. Function facets
// (size/style/render) extend by WRAPPING (you receive the base result and
// state only your delta); list facets (channels/on) extend by APPENDING.
//
// The same extension value applies at three scopes:
//   1. definition —  derivePart("fancy", Button, sparkle)   (a new named part)
//   2. theme      —  extendTheme("dark", "button", sparkle) (app-wide, incl.
//                    parts inside code you don't own; hits derived parts too)
//   3. use site   —  withExt(Button("k", …), sparkle)       (this element only)
// Order: definition first, theme second, use site last — closest wins.
// ============================================================================
import { Rect, v } from "./core.js";
import { part } from "./part.js";
/** Compose extensions left-to-right (later wraps closer to the surface). */
export const composeExt = (...exts) => (def) => exts.reduce((d, e) => e(d), def);
// ---- wrapping function facets ----------------------------------------------
/** Wrap the style: receive the base result, state only your delta. */
export const mapStyle = (f) => (def) => ({
    ...def,
    style: (t, ch, props) => f(t, ch, props, (def.style ? def.style(t, ch, props) : {})),
});
/** Wrap the render: call base() to paint the original — before (under),
 *  after (over), or not at all. */
export const mapRender = (f) => (def) => ({
    ...def,
    render: (node, p, style) => f(node, p, style, () => def.render?.(node, p, style)),
});
/** Wrap the intrinsic size (leaf sugar). */
export const mapSize = (f) => (def) => ({
    ...def,
    size: (props, m) => f(props, m, def.size ? def.size(props, m) : v(0, 0)),
});
/** Wrap the desired size given `avail` — the container-layout counterpart to
 *  `mapSize`. Receive the base's desired size and state only your delta. */
export const mapMeasure = (f) => (def) => ({
    ...def,
    measure: (props, avail, m) => f(props, avail, m, def.measure ? def.measure(props, avail, m) : v(0, 0)),
});
/** Wrap the child placement (today's arrange). Receive the base's rects (the
 *  framework default — each child at the origin at its desired size — when the
 *  part declares no arrange) and return the transformed list. */
export const mapArrange = (f) => (def) => ({
    ...def,
    arrange: (props, rect, kids) => f(props, rect, kids, def.arrange
        ? def.arrange(props, rect, kids)
        : kids.map((k) => new Rect(rect.x, rect.y, k.size.x, k.size.y))),
});
/** Wrap the structure: receive the base body's output (for a body-less part,
 *  its use-site children) and return the transformed list. Completes the
 *  extension algebra's coverage — `mapBody` is to `body` what `mapStyle` is to
 *  `style`. On a body-less part the base IS the use-site children, so
 *  "append a badge child to any container" is a one-liner. */
export const mapBody = (f) => (def) => ({
    ...def,
    body: (props, children, local) => f(props, children, def.body ? def.body(props, children, local) : children),
});
// ---- appending list facets ----------------------------------------------------
/** Append animated channels (namespace yours: "fx/sheen", not "sheen"). */
export const addChannels = (chs) => (def) => ({ ...def, channels: { ...def.channels, ...chs } });
/** Append interactors. All appended behaviors run; none replace. */
export const addOn = (...is) => (def) => ({ ...def, on: [...(def.on ?? []), ...is] });
/** Append adornments to ANY part — layer a tooltip, badge, or resize grip onto
 *  a widget that never planned for it. The host's own adornments (if any) run
 *  first, then yours; the results concatenate. This is decoration by
 *  composition — the core reason the facet exists. */
export const addAdorn = (fn) => (def) => ({ ...def, adorn: (node) => [...(def.adorn?.(node) ?? []), ...fn(node)] });
// ---- scope 1: definition — a new named part ------------------------------------
/** Bake extensions into a new named part. Remembers its ancestry, so theme
 *  extensions targeting the base also reach the derivative. The extensions are
 *  typed against the base's props — inline `mapStyle`/`addOn` calls see P. */
export function derivePart(name, base, ...exts) {
    const wrapped = composeExt(...exts)(base.def);
    const spec = { ...wrapped };
    const ctor = part(name, spec);
    ctor.def.ancestors = [...(base.def.ancestors ?? []), base.def.name];
    return ctor;
}
// ---- scope 3: use site — this element only ---------------------------------------
/** Apply extensions to one element. (Elements are prop-erased, so use-site
 *  extensions take PartExt<any>; type inline callbacks via PropsOf.) */
export const withExt = (el, ...exts) => ({ ...el, exts: [...(el.exts ?? []), ...exts] });
//# sourceMappingURL=extend.js.map