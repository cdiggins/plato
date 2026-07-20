// ============================================================================
// Effective part definitions — the layering composition (extend.ts, scopes):
//   definition → theme extensions (ancestry-aware) → use-site extensions.
// Composed once per instance and cached; the cache invalidates when the
// element blueprint changes or themeVersion bumps (setTheme / extendTheme).
// ============================================================================
import { themeVersion } from "./theme.js";
import { composeDef } from "./compose.js";
export class EffCache {
    cache = new WeakMap();
    get(inst) {
        const hit = this.cache.get(inst);
        if (hit && hit.ver === themeVersion && hit.el === inst.el)
            return hit.def;
        const def = composeDef(inst.el);
        this.cache.set(inst, { ver: themeVersion, el: inst.el, def });
        return def;
    }
}
//# sourceMappingURL=effective.js.map