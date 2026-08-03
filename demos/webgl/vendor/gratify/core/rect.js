// Rectangle — a class (not a bag) so authoring code reads well:
// `node.rect.center`, `node.rect.raise(style.lift)`.
import { v } from "./vec.js";
export class Rect {
    x;
    y;
    w;
    h;
    constructor(x = 0, y = 0, w = 0, h = 0) {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
    }
    get center() { return v(this.x + this.w / 2, this.y + this.h / 2); }
    get right() { return this.x + this.w; }
    get bottom() { return this.y + this.h; }
    /** Same rect shifted up by `lift` px (hover-lift idiom). */
    raise(lift) { return new Rect(this.x, this.y - lift, this.w, this.h); }
    inset(d) { return new Rect(this.x + d, this.y + d, this.w - 2 * d, this.h - 2 * d); }
    contains(p) { return p.x >= this.x && p.y >= this.y && p.x <= this.right && p.y <= this.bottom; }
    overlaps(o) { return this.x < o.right && this.right > o.x && this.y < o.bottom && this.bottom > o.y; }
}
export const rect = (x = 0, y = 0, w = 0, h = 0) => new Rect(x, y, w, h);
//# sourceMappingURL=rect.js.map