# Reader checkpoint

State: implemented; helpers verified, production build verified without generated data.

- React/Vite app, safe Markdown/GFM rendering, strict Mermaid SVG, hierarchical navigation, search, deep links, heading outline, history, raw Markdown download, loading/error/missing-page states, and responsive navigation are implemented.
- Pure route, tree, slug, multi-term search, fenced-heading extraction, title stripping, and ancestor-key tests pass with `node --test src/navigation.test.js`.
- `npm install` completed with registry access (278 packages). A production build succeeded without `public/reference.json`; bundle warnings identify optional Mermaid chunks above 500 kB, while the app's initial chunk is about 394 kB. Rerun after supervisor-generated data arrives.
- The supervisor reviewed edge cases; the implementation now ignores headings inside fences, expands all route ancestors, strips the first H1 after generated comments, handles malformed hashes and empty data, and scrolls pages to the top when navigating without a heading.
- Scope remains `tools/reference-reader/` only. No shared tracker, docs, or root scratch files are owned here.
