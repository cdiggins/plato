// ============================================================================
// Gratify update middleware — app-wide policies as wrappers around AppSpec
// (README: "undo/redo as a three-line middleware"). The framework never
// stores history itself; withUndo turns any app into an undoable one.
// ============================================================================
const isUndo = (i) => {
    const k = i?.kind;
    return k === "undo" || k === "redo";
};
/** Wrap an app so every intent snapshots history; `{kind:"undo"|"redo"}`
 *  travel it. Requires a pure (non-mutating) update. */
export function withUndo(app, limit = 200) {
    return {
        init: { past: [], present: app.init, future: [] },
        update(s, intent) {
            if (isUndo(intent)) {
                if (intent.kind === "undo" && s.past.length)
                    return { past: s.past.slice(0, -1), present: s.past[s.past.length - 1], future: [s.present, ...s.future] };
                if (intent.kind === "redo" && s.future.length)
                    return { past: [...s.past, s.present], present: s.future[0], future: s.future.slice(1) };
                return s;
            }
            const next = app.update(s.present, intent);
            if (next === s.present)
                return s;
            return { past: [...s.past, s.present].slice(-limit), present: next, future: [] };
        },
        view: (s) => app.view(s.present),
    };
}
/** Log every intent (README §3.9-style policy middleware). */
export function withLog(app, log = (i) => console.log("[intent]", i)) {
    return {
        ...app,
        update(d, i) { const next = app.update(d, i); log(i, next); return next; },
    };
}
//# sourceMappingURL=middleware.js.map