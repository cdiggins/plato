// Color (RGBA, r/g/b 0..255, a 0..1) — plain data + free functions.
import { clamp, lerp } from "./utils.js";
export const rgb = (r, g, b, a = 1) => ({ r, g, b, a });
export const cmix = (a, b, t) => ({
    r: lerp(a.r, b.r, t), g: lerp(a.g, b.g, t), b: lerp(a.b, b.b, t), a: lerp(a.a, b.a, t),
});
export const calpha = (c, a) => ({ r: c.r, g: c.g, b: c.b, a: c.a * a });
export const css = (c) => `rgba(${c.r | 0},${c.g | 0},${c.b | 0},${c.a})`;
/** hsl → rgba. h 0..360, s/l 0..1 */
export function hsl(h, s, l, a = 1) {
    h = ((h % 360) + 360) % 360;
    const c = (1 - Math.abs(2 * l - 1)) * s;
    const x = c * (1 - Math.abs(((h / 60) % 2) - 1));
    const m = l - c / 2;
    let r = 0, g = 0, b = 0;
    if (h < 60)
        [r, g, b] = [c, x, 0];
    else if (h < 120)
        [r, g, b] = [x, c, 0];
    else if (h < 180)
        [r, g, b] = [0, c, x];
    else if (h < 240)
        [r, g, b] = [0, x, c];
    else if (h < 300)
        [r, g, b] = [x, 0, c];
    else
        [r, g, b] = [c, 0, x];
    return { r: (r + m) * 255, g: (g + m) * 255, b: (b + m) * 255, a };
}
/** desaturate toward grey by amount 0..1 (disabled look). */
export function desat(c, amt) {
    const y = 0.299 * c.r + 0.587 * c.g + 0.114 * c.b;
    return { r: lerp(c.r, y, amt), g: lerp(c.g, y, amt), b: lerp(c.b, y, amt), a: c.a };
}
export const hexOf = (c) => "#" + [c.r, c.g, c.b].map((n) => Math.round(clamp(n, 0, 255)).toString(16).padStart(2, "0")).join("");
//# sourceMappingURL=color.js.map