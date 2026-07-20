// Spring1D — the entire "game feel" is step(). Semi-implicit Euler toward a
// target. Retained per Instance so a reconciled node springs from where it was.
export class Spring {
    v;
    vel = 0;
    constructor(v = 0) { this.v = v; }
    step(target, stiffness, damping, dt) {
        const steps = dt > 0.032 ? 2 : 1; // sub-step for stability at large dt
        const h = dt / steps;
        for (let i = 0; i < steps; i++) {
            const f = stiffness * (target - this.v) - damping * this.vel;
            this.vel += f * h;
            this.v += this.vel * h;
        }
        return this.v;
    }
    set(x) { this.v = x; this.vel = 0; }
}
//# sourceMappingURL=spring.js.map