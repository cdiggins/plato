// ============================================================================
// Animation pass — every frame, re-derive each channel's target from state
// and step the value toward it. This is the whole animation system: automatic
// channels (enter/exit/hover/press/drag/focus), state-tag channels, and
// part-declared channels (chase or impulse).
// ============================================================================
import { approach, Spring } from "./core.js";
export function animateScene(inst, dt, env) {
    // automatic channels
    if (!inst.exiting)
        inst.ch.enter = approach(inst.ch.enter, 1, 6, dt);
    else
        inst.ch.exit = approach(inst.ch.exit || 0, 1, 7, dt);
    inst.ch.hover = approach(inst.ch.hover || 0, inst === env.hovered ? 1 : 0, 16, dt);
    inst.ch.press = approach(inst.ch.press || 0, inst === env.pressed ? 1 : 0, 22, dt);
    inst.ch.drag = approach(inst.ch.drag || 0, inst === env.dragging ? 1 : 0, 14, dt);
    inst.ch.focus = approach(inst.ch.focus || 0, inst === env.focused ? 1 : 0, 14, dt);
    // state-tag channels (every tag ever seen keeps fading in/out)
    for (const k of inst.stateKeys)
        inst.ch[k] = approach(inst.ch[k] || 0, inst.states.has(k) ? 1 : 0, 10, dt);
    // part-declared channels (incl. extension-appended ones)
    const decls = env.eff(inst).channels;
    if (decls) {
        const node = env.nodeOf(inst);
        for (const k in decls) {
            const spec = decls[k];
            if (spec.decay !== undefined) {
                // impulse: kick() sets it, it fades to rest
                inst.ch[k] = approach(inst.ch[k] || 0, 0, spec.decay, dt);
                continue;
            }
            const target = spec.target ? spec.target(node) : 0;
            if (!(k in inst.ch)) {
                inst.ch[k] = target; // first frame: snap
                if (spec.spring)
                    inst.chSprings[k] = new Spring(target);
            }
            else if (spec.spring) {
                const sp = (inst.chSprings[k] ||= new Spring(inst.ch[k]));
                inst.ch[k] = sp.step(target, spec.spring.stiffness, spec.spring.damping, dt);
            }
            else {
                inst.ch[k] = approach(inst.ch[k], target, spec.rate ?? 10, dt);
            }
        }
    }
    for (const c of inst.children)
        animateScene(c, dt, env);
    for (const g of inst.ghosts)
        animateScene(g, dt, env);
}
//# sourceMappingURL=animate.js.map