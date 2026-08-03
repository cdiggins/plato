// Cubic "wire" helpers — the horizontal-tangent bezier used by connectors,
// plus sampled distance/intersection queries for hit-testing and slicing.
import { clamp } from "./utils.js";
import { v, vdist } from "./vec.js";
/** Control points for a horizontal-tangent cubic between two anchor points. */
export function wireCtrl(a, b) {
    const dx = clamp(Math.abs(b.x - a.x) * 0.5, 36, 190);
    return [v(a.x + dx, a.y), v(b.x - dx, b.y)];
}
/** Point at t on that cubic. */
export function wireAt(a, b, t) {
    const [c1, c2] = wireCtrl(a, b);
    const u = 1 - t;
    return v(u * u * u * a.x + 3 * u * u * t * c1.x + 3 * u * t * t * c2.x + t * t * t * b.x, u * u * u * a.y + 3 * u * u * t * c1.y + 3 * u * t * t * c2.y + t * t * t * b.y);
}
/** Approximate distance from p to the wire cubic (sampled). */
export function wireDist(a, b, p, samples = 24) {
    let best = Infinity;
    for (let i = 0; i <= samples; i++)
        best = Math.min(best, vdist(wireAt(a, b, i / samples), p));
    return best;
}
/** Do segment p1→p2 and the wire cubic cross? (sampled polyline test) */
export function wireCrossesSegment(a, b, p1, p2, samples = 24) {
    const side = (q) => (p2.x - p1.x) * (q.y - p1.y) - (p2.y - p1.y) * (q.x - p1.x);
    let prev = wireAt(a, b, 0), prevS = side(prev);
    for (let i = 1; i <= samples; i++) {
        const cur = wireAt(a, b, i / samples), curS = side(cur);
        if (prevS === 0 || curS === 0 || (prevS < 0) !== (curS < 0)) {
            // wire crosses the infinite line; check the crossing lies within the segment span
            const t = ((prev.x - p1.x) * (p2.x - p1.x) + (prev.y - p1.y) * (p2.y - p1.y)) /
                Math.max(1e-6, (p2.x - p1.x) ** 2 + (p2.y - p1.y) ** 2);
            if (t >= -0.05 && t <= 1.05)
                return true;
        }
        prev = cur;
        prevS = curS;
    }
    return false;
}
//# sourceMappingURL=curve.js.map