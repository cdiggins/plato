// ============================================================================
// Gratify interactors — input as values (README §5). A recognizer is a pure
// description parameterized by *what intent to emit*; all gesture state is
// runtime-owned. Interactors emit intents and set tags — never touch the doc.
//
// Editor-grade gestures (M3, layering guide §5c) get three bounded powers:
//   state — a private record, born on press, dead on release
//   query — read-only scene access (anchors, modifiers)
//   view  — overlay elements shown while the gesture runs (rubber wires,
//           marquees, slice lines) — the element tree stays the whole truth
// ============================================================================
import { clamp } from "./core.js";
/** Wrap an intent as LOCAL: routed to the nearest enclosing `reduce`, never to
 *  the app's `update`. Emit these from a composite's own interactors
 *  (`.press(() => Local({ kind: "toggle" }))`). */
export const Local = (intent) => ({ __gratifyLocal: intent });
export const isLocal = (i) => typeof i === "object" && i !== null && "__gratifyLocal" in i;
export const unwrapLocal = (i) => i.__gratifyLocal;
/** Emit an intent on click/tap (release inside, below drag threshold). */
export const Press = (to) => ({ kind: "press", to });
/** Maintain the hover tag; nothing else. */
export const Hover = () => ({ kind: "hover" });
/** Drag along one axis, reporting position as a 0..1 fraction of the track. */
export const Drag1D = (o) => ({ kind: "drag1d", ...o });
/** A full gesture: private state + query + overlay view (see GestureSpec). */
export const Gesture = (spec) => ({ kind: "gesture", spec: spec });
/** Viewport pan/zoom for the hosting part (typically the surface root). */
export const Pan = () => ({ kind: "pan" });
/** Keyboard mapping. Routed focus-first, then hover chain, then root. */
export const Keys = (map) => ({ kind: "keys", map });
/** Clicking this part gives it keyboard focus (ch.focus eases 0→1). */
export const Focusable = () => ({ kind: "focusable" });
/** Fraction of the way through a rect along an axis, honoring pad. */
export function axisFraction(rect, axis, pad, px, py) {
    return axis === "x"
        ? clamp((px - rect.x - pad) / Math.max(1, rect.w - 2 * pad), 0, 1)
        : clamp((py - rect.y - pad) / Math.max(1, rect.h - 2 * pad), 0, 1);
}
//# sourceMappingURL=interact.js.map