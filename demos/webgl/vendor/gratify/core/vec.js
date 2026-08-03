// 2D vector — plain data + free functions.
import { lerp } from "./utils.js";
export const v = (x = 0, y = 0) => ({ x, y });
export const add = (a, b) => ({ x: a.x + b.x, y: a.y + b.y });
export const sub = (a, b) => ({ x: a.x - b.x, y: a.y - b.y });
export const mul = (a, s) => ({ x: a.x * s, y: a.y * s });
export const vlerp = (a, b, t) => ({ x: lerp(a.x, b.x, t), y: lerp(a.y, b.y, t) });
export const vlen = (a) => Math.hypot(a.x, a.y);
export const vdist = (a, b) => Math.hypot(a.x - b.x, a.y - b.y);
//# sourceMappingURL=vec.js.map