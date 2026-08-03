// ============================================================================
// Stock effects library — concrete one-shot juice built on the Particles
// engine and the Fx contract. Apps spawn these (or ship their own) via
// node.spawn / runtime.spawnFx; the framework never references them.
// ============================================================================
import { calpha, v } from "./core.js";
import { Particles, rand } from "./particles.js";
/** Radial burst (confirmations, connections). */
export function burst(at, color) {
    return new Particles(color, () => {
        const ang = rand(0, Math.PI * 2), spd = rand(60, 220);
        return { p: { ...at }, vel: v(Math.cos(ang) * spd, Math.sin(ang) * spd - 40), life: rand(0.35, 0.7), max: 0.7, size: rand(1.5, 3.2) };
    }, 22);
}
/** Poof scattered across a deleted element's rect. */
export function poof(r, color) {
    return new Particles(color, () => {
        const p = v(r.x + rand(0, r.w), r.y + rand(0, r.h));
        return { p, vel: v(rand(-60, 60), rand(-90, -10)), life: rand(0.3, 0.6), max: 0.6, size: rand(1.5, 3) };
    }, 26);
}
/** Expanding ring (click ripple / confirm). */
export class Ring {
    at;
    color;
    max;
    dur;
    t = 0;
    done = false;
    constructor(at, color, max = 34, dur = 0.45) {
        this.at = at;
        this.color = color;
        this.max = max;
        this.dur = dur;
    }
    update(dt) { this.t += dt; if (this.t >= this.dur)
        this.done = true; }
    draw(p) {
        const k = this.t / this.dur;
        const r = this.max * (1 - Math.pow(1 - k, 3));
        p.ring(this.at, r, calpha(this.color, (1 - k) * 0.8), 2);
    }
}
//# sourceMappingURL=effects.js.map