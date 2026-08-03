// ============================================================================
// Style recipes + the shared surface protocol. Two costs kept authors out of
// the style facet: a named interface per widget (removed by curried part<P>()
// inference) and re-deriving the same hover/press blends every time (removed
// here). A recipe takes (Tokens, Channels, opts) and returns a record; it only
// makes sense inside a style function, so reaching for one pulls the author
// INTO the facet — the pit of success. Spread it, then add your widget's own
// fields.
//
// SurfaceStyle is the vocabulary a theme-scope restyle may assume: any widget
// that spreads `surface(...)` carries { fill, edge, text }, so one
// mapStyle<SurfaceStyle> extension restyles all of them at once.
// ============================================================================
/** The house recipe for an interactive surface: emphasis blends that grow on
 *  hover and press. `tint` colors the fill toward an accent/danger; `strength`
 *  scales the whole emphasis (0 = flat). */
export const surface = (t, ch, o = {}) => {
    const hover = ch.hover || 0, press = ch.press || 0;
    const tint = o.tint ?? t.surfaceHi;
    const k = (o.strength ?? 1) * (0.18 + 0.32 * hover + 0.4 * press);
    return {
        fill: t.mix(t.surface, tint, k),
        edge: t.mix(t.muted, o.tint ?? t.accent, hover),
        text: t.mix(t.text, t.textBright, hover),
    };
};
/** Text tone for labels: base color from emphasis flags, dimmed/faded by a
 *  `done` amount (the list-item idiom). Reads only tokens + a scalar. */
export const textTone = (t, o = {}) => {
    const base = o.bright ? t.textBright : o.dim ? t.textDim : t.text;
    const done = o.done ?? 0;
    return { text: t.mix(base, t.textDim, done), fade: 1 - 0.3 * done };
};
//# sourceMappingURL=style.js.map