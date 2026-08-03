// ============================================================================
// Gratify painter — the drawing contract parts render through, plus the
// Canvas2D implementation and a headless null implementation (for tests and
// deterministic stepping without a DOM).
// ============================================================================
import { css, wireCtrl } from "./core.js";
const SANS = `"Segoe UI", system-ui, sans-serif`;
const MONO = `"Cascadia Code", ui-monospace, monospace`;
const font = (o) => `${o?.weight || 400} ${o?.size || 13}px ${o?.mono ? MONO : SANS}`;
export class CanvasPainter {
    canvas;
    ctx;
    constructor(canvas) {
        this.canvas = canvas;
        this.ctx = canvas.getContext("2d");
    }
    measure = {
        text: (s, size = 13) => {
            this.ctx.font = `400 ${size}px ${SANS}`;
            return { x: this.ctx.measureText(s).width, y: size * 1.3 };
        },
    };
    clear(c, w, h) {
        this.ctx.setTransform(1, 0, 0, 1, 0, 0);
        this.ctx.fillStyle = css(c);
        this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);
        void w;
        void h;
    }
    screen(dpr) { this.ctx.setTransform(dpr, 0, 0, dpr, 0, 0); }
    view(pan, zoom, dpr) {
        this.ctx.setTransform(zoom * dpr, 0, 0, zoom * dpr, pan.x * dpr, pan.y * dpr);
    }
    wire(a, b, col, lw) {
        const c = this.ctx, [c1, c2] = wireCtrl(a, b);
        c.beginPath();
        c.moveTo(a.x, a.y);
        c.bezierCurveTo(c1.x, c1.y, c2.x, c2.y, b.x, b.y);
        c.strokeStyle = css(col);
        c.lineWidth = lw;
        c.lineCap = "round";
        c.stroke();
    }
    push() { this.ctx.save(); }
    pop() { this.ctx.restore(); }
    alpha(a) { this.ctx.globalAlpha *= a; }
    translate(dx, dy) { this.ctx.translate(dx, dy); }
    scaleAt(cx, cy, s) {
        this.ctx.translate(cx, cy);
        this.ctx.scale(s, s);
        this.ctx.translate(-cx, -cy);
    }
    glow(c, blur, draw) {
        const x = this.ctx;
        x.save();
        x.shadowColor = css(c);
        x.shadowBlur = blur;
        draw();
        x.restore();
    }
    roundRect(r, rad) {
        const c = this.ctx;
        if (r.w <= 0 || r.h <= 0) {
            c.beginPath();
            return;
        }
        rad = Math.max(0, Math.min(rad, r.w / 2, r.h / 2));
        c.beginPath();
        c.moveTo(r.x + rad, r.y);
        c.arcTo(r.right, r.y, r.right, r.bottom, rad);
        c.arcTo(r.right, r.bottom, r.x, r.bottom, rad);
        c.arcTo(r.x, r.bottom, r.x, r.y, rad);
        c.arcTo(r.x, r.y, r.right, r.y, rad);
        c.closePath();
    }
    box(r, corner, fill, stroke, lw = 1) {
        this.roundRect(r, corner);
        if (fill.a > 0) {
            this.ctx.fillStyle = css(fill);
            this.ctx.fill();
        }
        if (stroke) {
            this.ctx.strokeStyle = css(stroke);
            this.ctx.lineWidth = lw;
            this.ctx.stroke();
        }
    }
    label(s, at, color, o) {
        const c = this.ctx;
        c.fillStyle = css(color);
        c.font = font(o);
        c.textAlign = o?.align || "center";
        c.textBaseline = "middle";
        c.fillText(s, at.x, at.y);
    }
    line(a, b, col, lw = 1) {
        const c = this.ctx;
        c.beginPath();
        c.moveTo(a.x, a.y);
        c.lineTo(b.x, b.y);
        c.strokeStyle = css(col);
        c.lineWidth = lw;
        c.stroke();
    }
    dot(p, r, col) {
        const c = this.ctx;
        c.beginPath();
        c.arc(p.x, p.y, Math.max(0, r), 0, 7);
        c.fillStyle = css(col);
        c.fill();
    }
    ring(p, r, col, lw = 2) {
        const c = this.ctx;
        c.beginPath();
        c.arc(p.x, p.y, Math.max(0.1, r), 0, 7);
        c.strokeStyle = css(col);
        c.lineWidth = lw;
        c.stroke();
    }
}
/** Headless painter: draws nothing, measures approximately. For tests. */
export class NullPainter {
    measure = { text: (s, size = 13) => ({ x: s.length * size * 0.55, y: size * 1.3 }) };
    clear() { }
    box() { }
    label() { }
    dot() { }
    ring() { }
    line() { }
    wire() { }
    glow(_c, _b, draw) { draw(); }
    push() { }
    pop() { }
    alpha() { }
    translate() { }
    scaleAt() { }
    screen() { }
    view() { }
}
//# sourceMappingURL=painter.js.map