// ============================================================================
// Paint pass — three fixed passes per frame (M3 layers):
//   world   (viewport-transformed content)
//   overlay (world coords, drawn above all content — gesture previews, fx)
//   screen  (untransformed HUD)
// Each instance gets the automatic enter/exit transform (fade + pop/shrink).
// ============================================================================
import { clamp, easeOutBack } from "./core.js";
import { tokens } from "./theme.js";
export function renderScene(p, s, env) {
    p.clear(tokens.bg, s.viewW, s.viewH);
    p.view(s.viewport.pan, s.viewport.zoom, s.dpr);
    drawPass(s.root, p, "world", "world", env);
    drawPass(s.root, p, "overlay", "world", env);
    if (s.adornRoot)
        drawPass(s.adornRoot, p, "overlay", "overlay", env); // tooltips, badges, grips
    if (s.gestureRoot)
        drawPass(s.gestureRoot, p, "overlay", "overlay", env); // active drag previews, topmost
    for (const f of s.fx)
        f.draw(p);
    p.screen(s.dpr);
    drawPass(s.root, p, "screen", "world", env);
}
function drawPass(inst, p, pass, inherited, env) {
    const layer = inst.el.layer ?? inherited;
    p.push();
    const en = clamp(inst.ch.enter, 0, 1);
    const ex = clamp(inst.ch.exit || 0, 0, 1);
    if (en < 1) {
        p.alpha(en);
        p.scaleAt(inst.rect.center.x, inst.rect.center.y, 0.8 + 0.2 * easeOutBack(en));
    }
    if (ex > 0) {
        p.alpha(1 - ex);
        p.scaleAt(inst.rect.center.x, inst.rect.center.y, 1 - 0.25 * ex);
    }
    if (layer === pass) {
        const part = env.eff(inst);
        if (part.render) {
            const style = part.style ? part.style(tokens, inst.ch, inst.props) : {};
            part.render(env.nodeOf(inst), p, style);
        }
    }
    for (const g of inst.ghosts)
        drawPass(g, p, pass, layer, env);
    for (const c of inst.children)
        drawPass(c, p, pass, layer, env);
    p.pop();
}
//# sourceMappingURL=draw.js.map