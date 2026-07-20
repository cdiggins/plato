// ============================================================================
// Built-in layout containers: Stack, Row, Free, Layers, Flow. Containers are
// just parts with measure/arrange facets — a custom layout is a part, no class
// ceremony. Layout results feed position springs, so reflow animates.
//
// Two-phase (measure-arrange-plan.md): `measure(avail)` reports desired size
// given at most `avail` room (a Stack hands each child its width but unbounded
// height, so a wrapping Flow child can report an honest height); `arrange(rect)`
// places children in the final box.
//
// Written in the builder form (part.ts §builder): `.defaults()` merges under
// use-site props once, so the facets read `props.gap`, never `props.gap ?? 8`.
// ============================================================================
import { Rect, v } from "./core.js";
import { part, UNBOUNDED } from "./part.js";
const alignOff = (align, avail, size) => align === "center" ? (avail - size) / 2 : align === "end" ? avail - size : 0;
/** Vertical flow. */
export const Stack = part("stack")
    .props()
    .defaults({ gap: 8, pad: 0 })
    .measure((p, avail, m) => {
    // hand each child our width but UNBOUNDED height — a width-dependent child
    // (Flow, wrapped text) now receives a real width and reports a real height.
    const sizes = m.children(v(avail.x - 2 * p.pad, Infinity));
    const w = sizes.reduce((mx, s) => Math.max(mx, s.x), 0);
    const h = sizes.reduce((a, s) => a + s.y, 0) + p.gap * Math.max(0, sizes.length - 1);
    return v(w + 2 * p.pad, h + 2 * p.pad);
})
    .arrange((p, r, kids) => {
    const inner = r.w - 2 * p.pad;
    let y = r.y + p.pad;
    return kids.map(({ size: s }) => {
        const w = p.align === "stretch" ? inner : s.x;
        const out = new Rect(r.x + p.pad + alignOff(p.align, inner, s.x), y, w, s.y);
        y += s.y + p.gap;
        return out;
    });
});
/** Horizontal flow (children vertically centered by default). */
export const Row = part("row")
    .props()
    .defaults({ gap: 8, pad: 0, align: "center" })
    .measure((p, avail, m) => {
    // children get our height but unbounded width
    const sizes = m.children(v(Infinity, avail.y - 2 * p.pad));
    const w = sizes.reduce((a, s) => a + s.x, 0) + p.gap * Math.max(0, sizes.length - 1);
    const h = sizes.reduce((mx, s) => Math.max(mx, s.y), 0);
    return v(w + 2 * p.pad, h + 2 * p.pad);
})
    .arrange((p, r, kids) => {
    const inner = r.h - 2 * p.pad;
    // "between" spreads any slack (rect wider than content) across the gaps.
    let extra = 0;
    if (p.justify === "between" && kids.length > 1) {
        const content = kids.reduce((a, k) => a + k.size.x, 0) + p.gap * (kids.length - 1);
        extra = Math.max(0, (r.w - 2 * p.pad) - content) / (kids.length - 1);
    }
    let x = r.x + p.pad;
    return kids.map(({ size: s }) => {
        const out = new Rect(x, r.y + p.pad + alignOff(p.align, inner, s.y), s.x, s.y);
        x += s.x + p.gap + extra;
        return out;
    });
});
/** Children at explicit positions (child props carry `pos: Vec`) — canvases,
 *  node editors, desktops. Positions feed springs, so moving is animating. */
export const Free = part("free")
    .props()
    // union of children (intrinsic) — a Free reports a real bound instead of the
    // old (0,0), so it composes inside a Stack/Row like any other child.
    .measure((_p, _avail, m) => m.children(UNBOUNDED).reduce((mx, s) => v(Math.max(mx.x, s.x), Math.max(mx.y, s.y)), v(0, 0)))
    .arrange((_p, r, kids) => kids.map(({ size, props, pos: elPos }) => {
    // prefer the element-level pos (set via `at(...)`), fall back to props.pos
    const pos = elPos ?? props?.pos ?? v(0, 0);
    return new Rect(r.x + pos.x, r.y + pos.y, size.x, size.y);
}));
/** Every child gets the full rect (layer stacking: content, overlays, HUD). */
export const Layers = part("layers")
    .props()
    .measure((_p, avail, m) => m.children(avail).reduce((mx, s) => v(Math.max(mx.x, s.x), Math.max(mx.y, s.y)), v(0, 0)))
    .arrange((_p, r, kids) => kids.map(() => new Rect(r.x, r.y, r.w, r.h)));
/** Pack fixed-size boxes into `innerW`, wrapping rows. Returns each box's offset
 *  from the content origin, plus the total wrapped height. */
function packRows(sizes, innerW, gap) {
    const offsets = [];
    let x = 0, y = 0, rowH = 0;
    for (const s of sizes) {
        if (x + s.x > innerW && x > 0) {
            x = 0;
            y += rowH + gap;
            rowH = 0;
        }
        offsets.push(v(x, y));
        x += s.x + gap;
        rowH = Math.max(rowH, s.y);
    }
    return { offsets, height: y + rowH };
}
export const Flow = part("flow")
    .props()
    .defaults({ gap: 8, pad: 12 })
    .pack((sizes, avail, p) => {
    const { offsets, height } = packRows(sizes, avail.x - 2 * p.pad, p.gap);
    return {
        offsets: offsets.map((o) => v(o.x + p.pad, o.y + p.pad)),
        size: v(avail.x, height + 2 * p.pad), // honest height, from width
    };
});
//# sourceMappingURL=containers.js.map