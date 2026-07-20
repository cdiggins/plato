// ============================================================================
// Gratify — public API barrel. Apps and examples import from here only.
//
// Layering of the source tree:
//   core/        value types + animation primitives (no DOM)
//   scene        Element/Instance + keyed reconcile
//   part         the part() facet model
//   interact     interactors (input as values) + Query
//   extend       the wrap/append extension algebra (three scopes)
//   effective    per-instance layering composition cache
//   layout/animate/draw   the three per-frame passes
//   runtime      the two-clock loop + input pipeline
//   painter      the drawing contract (Canvas2D + headless)
//   theme        tokens, themes, theme-scope extensions
//   fx/particles/effects  transient-effect contract / engine / stock library
//   containers/label      built-in parts
//   middleware   app-wide policies (undo, logging)
// ============================================================================
export * from "./core.js";
export * from "./painter.js";
export * from "./theme.js";
export * from "./part.js";
export * from "./scene.js";
export * from "./interact.js";
export * from "./style.js";
export * from "./containers.js";
export * from "./label.js";
export * from "./extend.js";
export * from "./compose.js";
export * from "./fx.js";
export * from "./particles.js";
export * from "./effects.js";
export * from "./middleware.js";
export * from "./runtime.js";
//# sourceMappingURL=index.js.map