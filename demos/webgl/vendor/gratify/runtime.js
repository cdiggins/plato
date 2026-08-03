// ============================================================================
// Gratify runtime — the two-clock loop and the input pipeline. The passes it
// orchestrates live in their own modules: layout.ts (measure/place/springs),
// animate.ts (channels), draw.ts (layered paint), effective.ts (layering
// composition). This file owns state: doc, retained trees, viewport, pointer,
// gesture, focus, anchors, wake/sleep.
//
// The input pipeline (pointerDown/Move/Up, wheel, key) is public so headless
// tests drive real interactions; step(n, dt) advances deterministic frames.
//
// HOST BOUNDARY (plan §0.1): this file imports no app/example module. The app
// arrives as AppSpec { init, update, view } — nothing else crosses.
// ============================================================================
import { clamp, rect, v } from "./core.js";
import { CanvasPainter, NullPainter } from "./painter.js";
import { reconcile, walk } from "./scene.js";
import { axisFraction, isLocal, unwrapLocal } from "./interact.js";
import { themeVersion, tickTheme } from "./theme.js";
import { EffCache } from "./effective.js";
import { expandBodies } from "./compose.js";
import { layoutScene } from "./layout.js";
import { animateScene } from "./animate.js";
import { renderScene } from "./draw.js";
/** Internal container for gesture preview elements. */
const GESTURE_ROOT = { name: "__gesture-root" };
/** Internal container for adornments: places each child at its element `pos`
 *  (world coords), which the adorn author computes from the host's rect. */
const ADORN_ROOT = {
    name: "__adorn-root",
    measure: () => v(0, 0),
    arrange: (_props, r, kids) => kids.map((k) => rect(r.x + (k.pos?.x ?? 0), r.y + (k.pos?.y ?? 0), k.size.x, k.size.y)),
};
export class Runtime {
    app;
    painter;
    doc;
    root;
    fx = [];
    time = 0;
    viewport = { pan: v(0, 0), zoom: 1 };
    effs = new EffCache();
    dirty = false;
    themeVer = themeVersion; // re-expand+reconcile when a theme-scope mapBody changes structure
    dpr = 1;
    viewW;
    viewH;
    pointer = null; // screen coords
    mods = { shift: false, alt: false, ctrl: false };
    press = null;
    gesture = null;
    gestureRoot = null;
    adornRoot = null;
    adornHosts = new Map(); // adorn-root child key → host instance (routing bridge)
    modalInst = null; // topmost modal adornment, if any
    panDrag = null;
    focus = null;
    anchorMap = new Map();
    awake = true;
    idleT = 0;
    moving = true;
    lastSig = 0;
    last = 0;
    stopped = false;
    constructor(canvas, app, opts = {}) {
        this.app = app;
        this.viewW = opts.width ?? 800;
        this.viewH = opts.height ?? 600;
        this.painter = canvas && !opts.headless ? new CanvasPainter(canvas) : new NullPainter();
        this.doc = app.init;
        this.root = reconcile(null, expandBodies(app.view(this.doc)));
        if (canvas && !opts.headless)
            this.attach(canvas);
    }
    // ---- public --------------------------------------------------------------
    dispatch = (i) => {
        if (isLocal(i)) {
            console.warn("gratify: Local(...) intent sent to dispatch() — local intents route from a node; emit them from an interactor instead. Dropped:", unwrapLocal(i));
            return;
        }
        this.doc = this.app.update(this.doc, i);
        this.dirty = true;
        this.wake();
    };
    spawnFx(f) { this.fx.push(f); this.wake(); }
    // ---- local intent routing (guide §4d) -------------------------------------
    /** Dispatch from an ORIGINATING instance — the single seam every interactor
     *  intent flows through. `Local(...)` intents walk up to the nearest
     *  enclosing part with a `reduce` (crossing from an adornment to its host)
     *  and never reach the app's `update`; everything else goes to `dispatch`.
     *  A reducer's forwarded intent re-enters routing ABOVE it, so nested
     *  composites compose: bare → app update, `Local(...)` → a higher reducer. */
    dispatchFrom(origin, i) {
        if (i == null)
            return;
        if (!isLocal(i)) {
            this.dispatch(i);
            return;
        }
        let cur = origin;
        while (cur && !this.effs.get(cur).reduce)
            cur = this.hostParent(cur);
        if (!cur) {
            console.warn("gratify: Local(...) intent found no enclosing part with a reduce — dropped:", unwrapLocal(i));
            return;
        }
        const def = this.effs.get(cur);
        const [next, forward] = def.reduce(cur.local ?? def.localInit, unwrapLocal(i), this.nodeOf(cur));
        cur.local = next;
        this.dirty = true; // state clock: re-expand bodies/adorns that read local
        this.wake();
        if (forward != null)
            this.dispatchFrom(this.hostParent(cur), forward);
    }
    /** Parent for routing purposes: an adornment's logical parent is its HOST,
     *  not the internal adorn root — a dropdown's list items route to the
     *  dropdown's reducer. */
    hostParent(inst) {
        if (inst.parent && inst.parent === this.adornRoot)
            return this.adornHosts.get(inst.key);
        return inst.parent;
    }
    /** A LocalReader over a retained tree — how `expandBodies` (a pure element
     *  pre-pass) sees the previous frame's instance-local state by key path. */
    localOf(root) {
        return (path) => {
            let cur = root && root.key === path[0] ? root : undefined;
            for (let i = 1; cur && i < path.length; i++)
                cur = cur.children.find((c) => c.key === path[i]);
            return cur?.local;
        };
    }
    /** Advance n deterministic frames (headless testing / golden frames). */
    step(n = 1, dt = 1 / 60) { for (let i = 0; i < n; i++)
        this.tick(dt); }
    stop() { this.stopped = true; }
    /** Whether the scene is still animating (drove the last frame's decision to
     *  schedule another). False means the loop has gone to sleep at rest. */
    get animating() { return this.moving; }
    /** Screen → world (inverse viewport transform). */
    toWorld(p) {
        return v((p.x - this.viewport.pan.x) / this.viewport.zoom, (p.y - this.viewport.pan.y) / this.viewport.zoom);
    }
    /** The composed ("effective") part definition for an instance, after all
     *  layering scopes are applied: definition → theme extensions → use-site
     *  extensions. This is the answer to "why does this widget look/behave
     *  like this?" — for tests, inspectors, and debug tooling. */
    effectiveDef(inst) {
        return this.effs.get(inst);
    }
    // ---- the query capability (read-only scene access for gestures/effects) ----
    query = {
        anchor: (id) => this.anchorMap.get(id),
        anchors: (pred) => {
            const out = [];
            for (const a of this.anchorMap.values())
                if (!pred || pred(a))
                    out.push(a);
            return out;
        },
        nearestAnchor: (p, radius, pred) => {
            let best, bestD = radius;
            for (const a of this.anchorMap.values()) {
                if (pred && !pred(a))
                    continue;
                const d = Math.hypot(a.pos.x - p.x, a.pos.y - p.y);
                if (d <= bestD) {
                    bestD = d;
                    best = a;
                }
            }
            return best;
        },
        mods: this.mods, // same object; pointer handlers mutate it in place
    };
    // ---- input pipeline (public: DOM wiring and tests both call these) ---------
    pointerDown(p, mods) {
        Object.assign(this.mods, mods);
        this.pointer = p;
        // Modal capture (guide §5e, the ONE extra input rule): while a modal
        // adornment is up, a press outside it dispatches its dismiss intent and is
        // consumed — click-away closes and does NOT also press what's underneath.
        if (this.modalInst && !this.withinModal(p)) {
            this.dispatchFrom(this.modalInst, this.modalInst.el.modal?.dismiss);
            this.wake();
            return;
        }
        const hit = this.topInteractiveHit(p);
        if (hit) {
            const eff = this.effs.get(hit);
            const node = this.nodeOf(hit);
            const lp = this.layerPoint(this.layerOfInst(hit), p);
            // gestures get first crack; begin() may decline with null
            for (const it of eff.on ?? []) {
                if (it.kind === "gesture") {
                    const s = it.spec.begin(node, lp, this.query);
                    if (s !== null) {
                        this.gesture = { inst: hit, spec: it.spec, state: s };
                        break;
                    }
                }
            }
            const drag = this.gesture ? undefined
                : eff.on.find((i) => i.kind === "drag1d");
            if (!this.gesture && !drag && eff.on.some((i) => i.kind === "pan")) {
                this.panDrag = { pan0: { ...this.viewport.pan }, p0: p };
            }
            this.press = { inst: hit, p0: p, moved: false, drag };
            if (drag)
                this.dispatchDrag(hit, drag, p);
            if (eff.on.some((i) => i.kind === "focusable"))
                this.focus = hit;
            else if (!this.gesture && !drag)
                this.focus = null;
        }
        else {
            this.focus = null;
        }
        this.wake();
    }
    pointerMove(p, mods) {
        Object.assign(this.mods, mods);
        this.pointer = p;
        if (this.press && Math.hypot(p.x - this.press.p0.x, p.y - this.press.p0.y) > 4)
            this.press.moved = true;
        if (this.gesture) {
            const g = this.gesture;
            const lp = this.layerPoint(this.layerOfInst(g.inst), p);
            const node = this.nodeOf(g.inst);
            if (g.spec.move)
                g.state = g.spec.move(g.state, node, lp, this.query);
            const live = g.spec.during?.(g.state, node, lp, this.query);
            this.dispatchFrom(g.inst, live);
        }
        else if (this.panDrag) {
            this.viewport.pan = v(this.panDrag.pan0.x + (p.x - this.panDrag.p0.x), this.panDrag.pan0.y + (p.y - this.panDrag.p0.y));
        }
        else if (this.press?.drag) {
            this.dispatchDrag(this.press.inst, this.press.drag, p);
        }
        this.wake();
    }
    pointerUp(p) {
        this.pointer = p;
        const ps = this.press;
        this.press = null;
        const g = this.gesture;
        this.gesture = null;
        this.panDrag = null;
        if (g) {
            const lp = this.layerPoint(this.layerOfInst(g.inst), p);
            const out = g.spec.up?.(g.state, this.nodeOf(g.inst), lp, this.query);
            if (out !== undefined && out !== null) {
                for (const i of Array.isArray(out) ? out : [out])
                    this.dispatchFrom(g.inst, i);
            }
        }
        // a clean click (no movement) still runs press behaviors, gesture or not
        if (ps && !ps.drag && !ps.moved && this.hitTest(ps.inst, p)) {
            for (const it of this.effs.get(ps.inst).on ?? []) {
                if (it.kind === "press")
                    this.dispatchFrom(ps.inst, it.to(this.nodeOf(ps.inst)));
            }
        }
        this.wake();
    }
    wheel(delta, at) {
        // zoom only when some part opted in via Pan()
        if (!this.anyPan(this.root))
            return;
        const z0 = this.viewport.zoom;
        const z1 = clamp(z0 * Math.exp(-delta * 0.0011), 0.25, 3);
        this.viewport.pan = v(at.x - (at.x - this.viewport.pan.x) * (z1 / z0), at.y - (at.y - this.viewport.pan.y) * (z1 / z0));
        this.viewport.zoom = z1;
        this.wake();
    }
    key(k) {
        // modal capture: Escape dismisses the topmost modal adornment, consumed
        if (this.modalInst && k === "Escape") {
            this.dispatchFrom(this.modalInst, this.modalInst.el.modal?.dismiss);
            this.wake();
            return;
        }
        // focus first, then the hover chain, then the root
        const chain = [];
        if (this.focus)
            chain.push(this.focus);
        let h = this.pointer ? this.renderHit(this.root, this.pointer) : null;
        while (h) {
            chain.push(h);
            h = h.parent ?? null;
        }
        if (!chain.includes(this.root))
            chain.push(this.root);
        for (const inst of chain) {
            for (const it of this.effs.get(inst).on ?? []) {
                if (it.kind === "keys" && it.map[k]) {
                    this.dispatchFrom(inst, it.map[k](this.nodeOf(inst)));
                    this.wake();
                    return;
                }
            }
        }
    }
    // ---- one frame -----------------------------------------------------------
    tick(dt) {
        this.time += dt;
        const themeFading = tickTheme(dt);
        // a themeVersion bump (setTheme / extendTheme) may change composite structure
        // via a theme-scope mapBody, so treat it like a dirty view.
        if (themeVersion !== this.themeVer) {
            this.themeVer = themeVersion;
            this.dirty = true;
        }
        if (this.dirty) {
            this.root = reconcile(this.root, expandBodies(this.app.view(this.doc), this.localOf(this.root)));
            this.dirty = false;
        }
        const eff = (i) => this.effs.get(i);
        layoutScene(this.root, dt, eff, this.painter.measure, this.viewW, this.viewH);
        this.publishAnchors();
        this.syncGestureView(dt, eff);
        const env = {
            eff,
            nodeOf: (i) => this.nodeOf(i),
            hovered: this.hoverInst(),
            pressed: this.press?.inst ?? null,
            dragging: this.gesture?.inst ?? null,
            focused: this.focus,
        };
        animateScene(this.root, dt, env);
        if (this.gestureRoot)
            animateScene(this.gestureRoot, dt, { ...env, hovered: null });
        // adornments read host channels (hover) + rect, so sync them after the host
        // has animated; they get their own enter/exit/hover via animateScene.
        this.syncAdornments(dt, eff);
        if (this.adornRoot)
            animateScene(this.adornRoot, dt, env);
        for (const f of this.fx)
            f.update(dt);
        this.fx = this.fx.filter((f) => !f.done);
        this.prune(this.root);
        if (this.gestureRoot)
            this.prune(this.gestureRoot);
        if (this.adornRoot)
            this.prune(this.adornRoot);
        renderScene(this.painter, {
            root: this.root, gestureRoot: this.gestureRoot, adornRoot: this.adornRoot, fx: this.fx,
            viewport: this.viewport, dpr: this.dpr, viewW: this.viewW, viewH: this.viewH,
        }, env);
        const sig = this.signature(this.root) + (this.gestureRoot ? this.signature(this.gestureRoot) : 0) +
            (this.adornRoot ? this.signature(this.adornRoot) : 0);
        this.moving = themeFading || this.fx.length > 0 || this.dirty || !!this.press || !!this.gesture ||
            (this.app.ambient?.(this.doc, this.time) ?? false) ||
            Math.abs(sig - this.lastSig) > 0.002;
        this.lastSig = sig;
    }
    // ---- anchors: published by layout, read through Query (guide §5b) ----------
    publishAnchors() {
        this.anchorMap.clear();
        const rec = (inst) => {
            const part = this.effs.get(inst);
            if (part.anchors) {
                for (const a of part.anchors(this.nodeOf(inst))) {
                    this.anchorMap.set(a.id, { ...a, key: inst.key });
                }
            }
            for (const c of inst.children)
                rec(c);
        };
        rec(this.root);
    }
    // ---- gesture preview view (overlay elements while a gesture runs) ----------
    syncGestureView(dt, eff) {
        const els = this.gesture?.spec.view?.(this.gesture.state, this.query) ?? [];
        if (els.length || this.gestureRoot?.children.length || this.gestureRoot?.ghosts.length) {
            const rootEl = { key: "__gestures", part: GESTURE_ROOT, props: {}, children: els, layer: "overlay" };
            this.gestureRoot = reconcile(this.gestureRoot, expandBodies(rootEl, this.localOf(this.gestureRoot)));
            layoutScene(this.gestureRoot, dt, eff, this.painter.measure, this.viewW, this.viewH);
        }
        else {
            this.gestureRoot = null;
        }
    }
    // ---- adornments (overlay elements anchored to hosts, guide §9) --------------
    syncAdornments(dt, eff) {
        const kids = [];
        this.adornHosts.clear();
        const collect = (inst) => {
            const part = eff(inst);
            if (part.adorn) {
                // namespace each adornment key under its host so two hosts can't collide
                for (const el of part.adorn(this.nodeOf(inst))) {
                    const key = `${inst.key}::${el.key}`;
                    kids.push({ ...el, key });
                    this.adornHosts.set(key, inst); // Local intents from the adornment route to the host
                }
            }
            for (const c of inst.children)
                collect(c);
        };
        collect(this.root);
        if (kids.length || this.adornRoot?.children.length || this.adornRoot?.ghosts.length) {
            const rootEl = { key: "__adorn", part: ADORN_ROOT, props: {}, children: kids, layer: "overlay" };
            this.adornRoot = reconcile(this.adornRoot, expandBodies(rootEl, this.localOf(this.adornRoot)));
            layoutScene(this.adornRoot, dt, eff, this.painter.measure, this.viewW, this.viewH);
        }
        else {
            this.adornRoot = null;
        }
        // the topmost (last in paint order) LIVE modal adornment owns modal capture
        this.modalInst = null;
        if (this.adornRoot)
            walk(this.adornRoot, (i) => { if (i.el.modal && !i.exiting)
                this.modalInst = i; });
    }
    // ---- node capability record ------------------------------------------------
    nodeOf(inst) {
        const lp = this.pointer ? this.layerPoint(this.layerOfInst(inst), this.pointer) : undefined;
        return {
            key: inst.key, props: inst.props, rect: inst.rect, ch: inst.ch, states: inst.states,
            local: inst.local ?? this.effs.get(inst).localInit,
            pointer: lp,
            spawn: (f) => this.spawnFx(f),
            anchor: (id) => this.anchorMap.get(id)?.pos,
            kick: (k, val = 1) => { inst.ch[k] = val; this.wake(); },
            view: { pan: this.viewport.pan, zoom: this.viewport.zoom, w: this.viewW, h: this.viewH },
            time: this.time,
        };
    }
    // ---- hit-testing (layer-aware) -----------------------------------------------
    layerOfInst(inst) {
        let cur = inst;
        while (cur) {
            if (cur.el.layer)
                return cur.el.layer;
            cur = cur.parent;
        }
        return "world";
    }
    layerPoint(layer, p) {
        return layer === "screen" ? p : this.toWorld(p);
    }
    hoverInst() {
        if (!this.pointer)
            return null;
        // Only INTERACTIVE adornments (with `on`, e.g. a close button) capture hover
        // and clicks; decorative ones (tooltip, badge) stay transparent so the host
        // keeps its hover and clicks pass through to it.
        if (this.adornRoot) {
            const h = this.interactiveHit(this.adornRoot, this.pointer);
            if (h)
                return h;
        }
        return this.renderHit(this.root, this.pointer);
    }
    /** Is the press point inside the topmost modal adornment (its own bounds or
     *  any descendant's)? Geometric, so decorative interior (padding, labels)
     *  counts as inside — only a genuine click-away dismisses. */
    withinModal(p) {
        const m = this.modalInst;
        if (!m)
            return false;
        if (this.hitTest(m, p))
            return true;
        let inside = false;
        walk(m, (i) => { if (!inside && this.hitTest(i, p))
            inside = true; });
        return inside;
    }
    /** Interactive hit, overlay (adornments) first, then main content. */
    topInteractiveHit(p) {
        if (this.adornRoot) {
            const h = this.interactiveHit(this.adornRoot, p);
            if (h)
                return h;
        }
        return this.interactiveHit(this.root, p);
    }
    hitTest(inst, p) {
        const lp = this.layerPoint(this.layerOfInst(inst), p);
        const part = this.effs.get(inst);
        return part.hit ? part.hit(this.nodeOf(inst), lp) : inst.rect.contains(lp);
    }
    renderHit(inst, p) {
        for (let i = inst.children.length - 1; i >= 0; i--) {
            const hit = this.renderHit(inst.children[i], p);
            if (hit)
                return hit;
        }
        const part = this.effs.get(inst);
        if ((part.render || part.on?.length) && this.hitTest(inst, p))
            return inst;
        return null;
    }
    /** Nearest self-or-ancestor of the hit with interactors attached. */
    interactiveHit(root, p) {
        let cur = this.renderHit(root, p);
        while (cur && !this.effs.get(cur).on?.length)
            cur = cur.parent;
        return cur ?? null;
    }
    anyPan(inst) {
        if (this.effs.get(inst).on?.some((i) => i.kind === "pan"))
            return true;
        return inst.children.some((c) => this.anyPan(c));
    }
    dispatchDrag(inst, drag, p) {
        const lp = this.layerPoint(this.layerOfInst(inst), p);
        const f = axisFraction(inst.rect, drag.axis, drag.pad ?? 8, lp.x, lp.y);
        this.dispatchFrom(inst, drag.to(this.nodeOf(inst), f));
    }
    // ---- ghost pruning + rest detection --------------------------------------------
    prune(inst) {
        inst.ghosts = inst.ghosts.filter((g) => (g.ch.exit || 0) < 0.99);
        for (const c of inst.children)
            this.prune(c);
    }
    signature(inst) {
        let s = 0;
        const rec = (i) => {
            for (const k in i.ch)
                s += i.ch[k];
            s += (i.sx.v + i.sy.v + i.cw + i.chh) * 0.01;
            for (const c of i.children)
                rec(c);
            for (const g of i.ghosts)
                rec(g);
        };
        rec(inst);
        return s;
    }
    // ---- DOM wiring + wake/sleep -------------------------------------------------
    attach(canvas) {
        this.dpr = Math.min(window.devicePixelRatio || 1, 2);
        const resize = () => {
            const w = canvas.clientWidth, h = canvas.clientHeight;
            if (!w || !h)
                return;
            canvas.width = Math.floor(w * this.dpr);
            canvas.height = Math.floor(h * this.dpr);
            this.viewW = w;
            this.viewH = h;
            this.wake();
        };
        resize();
        new ResizeObserver(resize).observe(canvas);
        window.addEventListener("resize", resize);
        const pos = (ev) => {
            const b = canvas.getBoundingClientRect();
            return v(ev.clientX - b.left, ev.clientY - b.top);
        };
        const m = (ev) => ({ shift: ev.shiftKey, alt: ev.altKey, ctrl: ev.ctrlKey || ev.metaKey });
        canvas.addEventListener("pointerdown", (ev) => { this.pointerDown(pos(ev), m(ev)); canvas.setPointerCapture(ev.pointerId); });
        canvas.addEventListener("pointermove", (ev) => this.pointerMove(pos(ev), m(ev)));
        canvas.addEventListener("pointerup", (ev) => this.pointerUp(pos(ev)));
        canvas.addEventListener("pointerleave", () => { this.pointer = null; this.wake(); });
        canvas.addEventListener("wheel", (ev) => { ev.preventDefault(); this.wheel(ev.deltaY, pos(ev)); }, { passive: false });
        window.addEventListener("keydown", (ev) => this.key(ev.key));
        // debug hooks (deterministic stepping from the console)
        const w = window;
        w.gratify = this;
        w.gratifyStep = (n = 1, dt = 1 / 60) => { this.stopped = true; this.step(n, dt); };
        w.gratifyResume = () => { this.stopped = false; this.wake(); };
        this.last = performance.now();
        requestAnimationFrame((t) => this.frame(t));
    }
    wake() {
        this.idleT = 0;
        if (!this.awake && !this.stopped) {
            this.awake = true;
            this.last = performance.now();
            requestAnimationFrame((t) => this.frame(t));
        }
    }
    frame(now) {
        if (this.stopped)
            return;
        const dt = Math.min((now - this.last) / 1000 || 0, 0.05);
        this.last = now;
        this.tick(dt);
        if (this.moving)
            this.idleT = 0;
        else
            this.idleT += dt;
        if (this.idleT > 0.4) {
            this.awake = false;
            return;
        } // sleep until woken
        requestAnimationFrame((t) => this.frame(t));
    }
}
/** Mount a Gratify app on a canvas. The entire framework entry point. */
export function mount(canvas, app) {
    return new Runtime(canvas, app);
}
//# sourceMappingURL=runtime.js.map