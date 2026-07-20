// ============================================================================
// Particles — a small generic particle engine implementing Fx. Framework
// machinery only: it knows drag, gravity, and fading dots — not what a
// "burst" or a "poof" is. Stock effects live in effects.ts; custom effects
// can reuse this engine with their own spawn functions.
// ============================================================================
import { add, calpha, mul } from "./core.js";
export class Particles {
    color;
    ps = [];
    done = false;
    drag;
    gravity;
    constructor(color, spawn, count, opts = {}) {
        this.color = color;
        this.drag = opts.drag ?? 3.5;
        this.gravity = opts.gravity ?? 120;
        for (let i = 0; i < count; i++)
            this.ps.push(spawn());
    }
    update(dt) {
        for (const q of this.ps) {
            q.p = add(q.p, mul(q.vel, dt));
            q.vel = mul(q.vel, Math.exp(-this.drag * dt));
            q.vel.y += this.gravity * dt;
            q.life -= dt;
        }
        this.ps = this.ps.filter((q) => q.life > 0);
        if (this.ps.length === 0)
            this.done = true;
    }
    draw(p) {
        for (const q of this.ps) {
            const t = Math.max(0, q.life / q.max);
            p.dot(q.p, q.size * (0.4 + 0.6 * t), calpha(this.color, t));
        }
    }
}
/** Uniform random in [a, b) — for spawn functions. */
export const rand = (a, b) => a + Math.random() * (b - a);
//# sourceMappingURL=particles.js.map