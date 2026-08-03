// ============================================================================
// Layout pass — a real two-phase protocol (measure-arrange-plan.md):
//   PASS 1  measure  — parent-driven, top-down: each node is asked "given at
//                      most this much room, how big do you want to be?". A
//                      container measures its children through a MeasureCtx
//                      under whatever constraints its layout implies.
//   PASS 2  arrange  — top-down: each node is handed its final rect and places
//                      its children (absolute rects) inside it.
// Then step position springs and size approaches so every layout change glides
// (README: "layout results feed channels"). Pure over its inputs: no runtime
// state beyond the tree; the per-pass MeasureMemo caches desired sizes so a
// single-constraint tree measures each node once — O(n), the same class as the
// old single-pass engine.
// ============================================================================
import { approach, Rect, v } from "./core.js";
import { UNBOUNDED } from "./part.js";
const POS_SPRING = { k: 240, d: 26 };
const SIZE_RATE = 18;
/** Per-pass measurement cache. A parent may measure a child more than once (a
 *  future `grow` container measures twice), so desired sizes are memoized per
 *  (instance, avail); `last` remembers each node's final desired size for the
 *  arrange pass. Cleared every tick — a fresh instance per layoutScene call. */
class MeasureMemo {
    eff;
    textM;
    cache = new WeakMap();
    last = new WeakMap();
    constructor(eff, textM) {
        this.eff = eff;
        this.textM = textM;
    }
    /** Desired size of `inst` under `avail`, memoized. */
    measure(inst, avail) {
        let byAvail = this.cache.get(inst);
        if (!byAvail) {
            byAvail = new Map();
            this.cache.set(inst, byAvail);
        }
        const key = `${avail.x}|${avail.y}`;
        const hit = byAvail.get(key);
        if (hit)
            return hit;
        const s = this.dispatch(inst, avail);
        byAvail.set(key, s);
        this.last.set(inst, s);
        return s;
    }
    /** The desired size a node last reported (what arrange uses). Falls back to a
     *  fresh unbounded measure for a child its parent never measured. */
    sizeOf(inst) {
        return this.last.get(inst) ?? this.measure(inst, UNBOUNDED);
    }
    dispatch(inst, avail) {
        const part = this.eff(inst);
        const ctx = this.ctxFor(inst);
        if (part.size)
            return part.size(inst.props, ctx);
        if (part.measure)
            return part.measure(inst.props, avail, ctx);
        // default container: union (max on each axis) of children under `avail`.
        const sizes = ctx.children(avail);
        return sizes.reduce((acc, s) => v(Math.max(acc.x, s.x), Math.max(acc.y, s.y)), v(0, 0));
    }
    ctxFor(inst) {
        const kids = inst.children;
        return {
            text: (s, size) => this.textM.text(s, size),
            count: kids.length,
            child: (i, av) => this.measure(kids[i], av),
            children: (av) => kids.map((c) => this.measure(c, av)),
        };
    }
}
function arrangeInst(inst, target, memo, eff) {
    inst.target = target;
    const part = eff(inst);
    if (part.arrange && inst.children.length) {
        const kids = inst.children.map((c) => ({ key: c.key, size: memo.sizeOf(c), props: c.props, pos: c.el.pos }));
        const rects = part.arrange(inst.props, target, kids);
        inst.children.forEach((c, i) => arrangeInst(c, rects[i], memo, eff));
    }
    else {
        // default arrange: each child at the node's origin at its desired size.
        for (const c of inst.children) {
            const s = memo.sizeOf(c);
            arrangeInst(c, new Rect(target.x, target.y, s.x, s.y), memo, eff);
        }
    }
}
function stepRects(inst, dt) {
    const t = inst.target;
    if (!inst.placed) {
        inst.sx.set(t.x);
        inst.sy.set(t.y);
        inst.cw = t.w;
        inst.chh = t.h;
        inst.placed = true;
    }
    else if (!inst.exiting) {
        inst.sx.step(t.x, POS_SPRING.k, POS_SPRING.d, dt);
        inst.sy.step(t.y, POS_SPRING.k, POS_SPRING.d, dt);
        inst.cw = approach(inst.cw, t.w, SIZE_RATE, dt);
        inst.chh = approach(inst.chh, t.h, SIZE_RATE, dt);
    }
    inst.rect = new Rect(inst.sx.v, inst.sy.v, inst.cw, inst.chh);
    for (const c of inst.children)
        stepRects(c, dt);
    for (const g of inst.ghosts)
        stepRects(g, dt);
}
/** One full layout pass over a tree: measure from the viewport down, arrange
 *  into the final rects, then step the channels that make it glide. */
export function layoutScene(root, dt, eff, m, viewW, viewH) {
    const memo = new MeasureMemo(eff, m);
    const s = memo.measure(root, v(viewW, viewH)); // PASS 1: measure
    arrangeInst(root, new Rect(0, 0, Math.max(s.x, viewW), Math.max(s.y, viewH)), memo, eff); // PASS 2: arrange
    stepRects(root, dt); // channels consume arrange targets
}
//# sourceMappingURL=layout.js.map