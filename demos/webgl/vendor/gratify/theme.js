// ============================================================================
// Gratify tokens + themes. Tokens are the named design values every style
// function reads. setTheme() retargets them and the live values cross-fade —
// a theme swap is a choreographed fade of the whole UI, for free.
// ============================================================================
import { approach, cmix, rgb } from "./core.js";
const dark = {
    bg: rgb(18, 20, 26),
    surface: rgb(36, 40, 52),
    surfaceHi: rgb(52, 58, 74),
    muted: rgb(70, 76, 94),
    text: rgb(206, 212, 224),
    textDim: rgb(130, 138, 156),
    textBright: rgb(242, 246, 252),
    accent: rgb(64, 186, 255),
    accent2: rgb(168, 130, 255),
    danger: rgb(255, 92, 108),
};
const light = {
    bg: rgb(243, 245, 249),
    surface: rgb(255, 255, 255),
    surfaceHi: rgb(238, 242, 250),
    muted: rgb(198, 205, 218),
    text: rgb(38, 44, 56),
    textDim: rgb(120, 128, 144),
    textBright: rgb(10, 14, 22),
    accent: rgb(20, 122, 255),
    accent2: rgb(128, 84, 240),
    danger: rgb(224, 56, 76),
};
export const themes = { dark, light };
/** The live tokens — values chase the active theme's palette every frame. */
export const tokens = {
    ...structuredClone(dark),
    mix: cmix,
};
let target = dark;
export let themeName = "dark";
/** Bumped whenever the active theme (palette or extensions) changes; the
 *  runtime uses it to invalidate per-instance composed-definition caches. */
export let themeVersion = 0;
export function setTheme(name) {
    if (themes[name]) {
        target = themes[name];
        themeName = name;
        themeVersion++;
    }
}
const themeExts = new Map();
/** Register an extension on every instance of a part (and its derivatives)
 *  while the named theme is active. */
export function extendTheme(theme, partName, ext) {
    let m = themeExts.get(theme);
    if (!m)
        themeExts.set(theme, (m = new Map()));
    m.set(partName, [...(m.get(partName) ?? []), ext]);
    if (theme === themeName)
        themeVersion++;
}
/** Remove a theme's extensions for a part (debug/toggling). */
export function clearThemeExt(theme, partName) {
    themeExts.get(theme)?.delete(partName);
    if (theme === themeName)
        themeVersion++;
}
/** Extensions the ACTIVE theme applies to a part with the given name+ancestry. */
export function activeThemeExts(partName, ancestors) {
    const m = themeExts.get(themeName);
    if (!m)
        return [];
    const out = [];
    for (const a of ancestors ?? [])
        out.push(...(m.get(a) ?? []));
    out.push(...(m.get(partName) ?? []));
    return out;
}
/** Step live token values toward the target palette. True while still fading. */
export function tickTheme(dt) {
    let moving = false;
    for (const k of Object.keys(target)) {
        const cur = tokens[k], want = target[k];
        for (const c of ["r", "g", "b", "a"]) {
            const next = approach(cur[c], want[c], 8, dt);
            if (Math.abs(next - want[c]) > 0.5)
                moving = true;
            cur[c] = next;
        }
    }
    return moving;
}
//# sourceMappingURL=theme.js.map