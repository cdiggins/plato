// ============================================================================
// Gratify scene — Element (immutable blueprint) + Instance (retained node) +
// keyed reconcile. Reuse-by-key is why juice is free: a matched node keeps its
// springs and channels, so it always knows where it *was*.
// ============================================================================
import { Rect, Spring } from "./core.js";
/** Position an element (adornments, `Free` children): sets the top-left of its
 *  rect to `pos` in its layer's coordinates. */
export const at = (element, pos) => ({ ...element, pos });
/** Mark an adornment element modal: it gets input first, and a press outside
 *  it (or Escape) dispatches `dismiss` — typically `Local({ kind: "close" })`
 *  — and is consumed. One rule; the whole click-away story. */
export const modal = (element, dismiss) => ({ ...element, modal: { dismiss } });
export class Instance {
    key;
    part;
    el;
    parent;
    children = [];
    ghosts = []; // exiting children, animating out
    rect = new Rect(); // current animated rect (what renders/hit-tests)
    target = new Rect(); // layout's target rect this frame
    sx = new Spring(0);
    sy = new Spring(0); // position springs
    cw = 0;
    chh = 0; // animated size (exponential)
    placed = false; // first layout snaps instead of gliding
    ch = Object.create(null); // animated channels
    chSprings = Object.create(null);
    stateKeys = new Set(); // every state tag ever seen (to fade out removed ones)
    states = new Set();
    exiting = false;
    freshGhost = false;
    local; // instance-local UI state; written only by reduce (unset = part's localInit)
    constructor(e, parent) {
        this.key = e.key;
        this.part = e.part;
        this.el = e;
        this.parent = parent;
        this.ch.enter = 0;
    }
    get props() { return this.el.props; }
    cval(k) { return this.ch[k] || 0; }
}
/** Keyed diff: match by key + part name → reuse (springs/channels survive);
 *  mismatch → fresh instance (plays enter). Vanished children become ghosts. */
export function reconcile(prev, e, parent) {
    let inst;
    if (prev && prev.key === e.key && prev.part.name === e.part.name) {
        inst = prev;
        inst.el = e;
    }
    else {
        inst = new Instance(e, parent);
    }
    inst.states = new Set(Object.keys(e.states || {}).filter((k) => e.states[k]));
    for (const k of inst.states)
        inst.stateKeys.add(k);
    const oldByKey = new Map(inst.children.map((c) => [c.key, c]));
    const kids = e.children || [];
    const newKeys = new Set(kids.map((c) => c.key));
    // Duplicate sibling keys silently collide in the keyed diff (springs/channels
    // bleed between the twins) — loud beats mysterious.
    if (newKeys.size !== kids.length) {
        const seen = new Set();
        for (const c of kids) {
            if (seen.has(c.key))
                console.error(`gratify: duplicate child key "${c.key}" under "${e.key}" — reconcile will collide them`);
            seen.add(c.key);
        }
    }
    for (const c of inst.children) {
        if (!newKeys.has(c.key) && !c.exiting) {
            c.exiting = true;
            c.freshGhost = true;
            inst.ghosts.push(c);
        }
    }
    inst.children = kids.map((ce) => reconcile(oldByKey.get(ce.key) || null, ce, inst));
    return inst;
}
/** Depth-first visit of every live instance. */
export function walk(inst, fn) {
    fn(inst);
    for (const c of inst.children)
        walk(c, fn);
}
//# sourceMappingURL=scene.js.map