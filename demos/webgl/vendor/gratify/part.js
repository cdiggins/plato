// ============================================================================
// Gratify parts — the unit of widget definition (README §"one part() call").
// A part declares up to seven facets; all optional except (usually) render.
// Function facets will gain map* wrappers in M2 (the layering algebra); for
// M1 they are plain functions.
// ============================================================================
import { Rect, v } from "./core.js";
/** Unbounded on both axes — "size to your content." */
export const UNBOUNDED = v(Infinity, Infinity);
/** A tight availability of exactly (w, h). */
export const tight = (w, h) => v(w, h);
/** Runtime: a callable ctor with chain methods; each step re-wraps a new def.
 *  Types above are the guardrails; the data below is a plain PartDef. */
function builderOf(def) {
    const b = makePart(def.name, def);
    const chain = (patch) => builderOf({ ...def, ...patch });
    b.props = () => chain({});
    b.defaults = (d) => chain({ defaults: { ...def.defaults, ...d } });
    b.size = (f) => chain({ size: f });
    b.intrinsic = (w, h) => chain({ size: () => v(w, h) });
    b.measure = (f) => chain({ measure: f });
    b.arrange = (f) => chain({ arrange: f });
    b.fill = () => chain({ measure: (_p, avail) => avail });
    b.pack = (f) => chain({
        measure: (props, avail, m) => f(m.children(UNBOUNDED), avail, props).size,
        arrange: (props, r, kids) => {
            const { offsets } = f(kids.map((k) => k.size), v(r.w, r.h), props);
            return offsets.map((o, i) => new Rect(r.x + o.x, r.y + o.y, kids[i].size.x, kids[i].size.y));
        },
    });
    b.body = (f) => chain({ body: f });
    b.local = (init) => chain({ localInit: init });
    b.reduce = (f) => chain({ reduce: f });
    b.style = (f) => chain({ style: f });
    b.render = (f) => chain({ render: f });
    b.channels = (c) => chain({ channels: { ...def.channels, ...c } });
    b.on = (...is) => chain({ on: [...(def.on ?? []), ...is] });
    const addI = (i) => chain({ on: [...(def.on ?? []), i] });
    b.press = (to) => addI({ kind: "press", to });
    b.drag1d = (o) => addI({ kind: "drag1d", ...o });
    b.gesture = (spec) => addI({ kind: "gesture", spec });
    b.keys = (map) => addI({ kind: "keys", map });
    b.adorn = (f) => chain({ adorn: def.adorn ? (n) => [...def.adorn(n), ...f(n)] : f });
    b.anchors = (f) => chain({ anchors: f });
    b.hit = (f) => chain({ hit: f });
    return b;
}
/** Re-open a part as a builder under a new name — derivation with the same
 *  vocabulary as definition. Ancestry is recorded, so theme extensions
 *  targeting the base also reach the derivative. */
export function extendPart(name, base) {
    return builderOf({
        ...base.def,
        name,
        ancestors: [...(base.def.ancestors ?? []), base.def.name],
    });
}
export function part(name, spec) {
    if (name === undefined)
        return (n, s) => makePart(n, s);
    if (spec === undefined)
        return builderOf({ name });
    return makePart(name, spec);
}
/** Build the element constructor for a part definition. */
export function makePart(name, spec) {
    const def = { ...spec, name }; // name last: spec may be a spread def
    const ctor = ((key, props, children) => {
        // defaults merge under use-site props ONCE, at element creation — facets
        // downstream read complete props, never `?? fallback`.
        const merged = def.defaults ? { ...def.defaults, ...props } : props;
        return {
            key,
            part: def,
            props: merged,
            children,
            states: merged?.states,
        };
    });
    ctor.def = def;
    return ctor;
}
//# sourceMappingURL=part.js.map