# vendored

- `gratify/` — the built ESM of the Gratify UI framework (`submodules/gratify/dist`,
  copied here so the static gallery can `import` it with no build step). Relative
  import specifiers were rewritten to include `.js` extensions for the browser.
  Regenerate with `demos/glsl/vendor/refresh.sh`.
- `widgets.js` — the shared example widgets (Button/Slider/Toggle/Card/Labeled)
  from `gratify/examples/shared/widgets.ts`, transpiled to JS with esbuild.

Both are build artifacts of the Gratify submodule, checked in so the demo is
self-contained and offline-runnable.
