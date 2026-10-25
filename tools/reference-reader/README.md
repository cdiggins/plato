# Plato reference reader

This folder is a standalone React/Vite reader for the Markdown pages assembled by `tools/reference/build.py`. It reads the generated `public/reference.json` at runtime and does not require a server API. Markdown HTML is not enabled; Mermaid diagrams use Mermaid's strict SVG rendering mode.

## Run it from the repository root

Use Node.js 20.19 or a release at least 22.12, npm, and Python 3.10 or later. The checked
dependency versions are pinned in `package-lock.json`; `npm ci` installs that exact dependency tree.

```powershell
python tools/reference/build.py
npm --prefix tools/reference-reader install
npm --prefix tools/reference-reader run dev
```

Open `http://127.0.0.1:5391`. Use the exact printed loopback address and dedicated port;
a generic localhost URL can reach a different listener on a shared development machine.
Build and serve the production bundle with:

```powershell
npm --prefix tools/reference-reader run build
npm --prefix tools/reference-reader run preview
```

The small pure navigation/search helpers can be checked with `npm --prefix tools/reference-reader test`.

The `predev` and `prebuild` scripts regenerate the reference data automatically, so Python 3.10
or later is also required. A failed source/link check stops the build instead of silently using
stale reader data. The initial explicit generator command is useful for seeing documentation
errors before dependency installation.

## Structure and extension points

- `src/navigation.js` parses hash routes, derives heading slugs, builds the hierarchical folder tree, and searches page titles and Markdown bodies.
- `src/main.jsx` owns route state, the expandable navigation, Markdown rendering, Mermaid rendering, and raw Markdown download.
- `src/style.css` contains the responsive dark navigation and light reading surface. Add future playgrounds as separate reader panels that consume the same page record shape.

Routes use `#/PAGE_ID` and `#/PAGE_ID@HEADING_SLUG`. The source revision and page path come from the generator's data. The reader currently offers search over titles and page bodies, page-level active navigation, a static heading outline, and browser-native history through the hash.

## Limits

Search is a simple in-memory term match and does not rank exact phrases or highlight matches. The heading outline is derived from Markdown heading lines, so unusual heading syntax may not be reflected there even though Markdown renders it. Reader assets are bundled locally; no remote fonts, scripts, or runtime data services are used.
