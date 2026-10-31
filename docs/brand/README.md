# Plato brand guidelines

How Plato looks and sounds: its voice, colour, type, spacing, mark and wordmark. Use this when you write for Plato, build a page for it, or place its mark.

Plato is a small, pure, statically-typed language for writing geometric and numeric libraries once and compiling them to other platforms. The brand says the same thing the language does: one exact source, drawn plainly, with nothing approximate.

The files that carry the brand:

- [`tokens.json`](tokens.json) holds every colour, type style, spacing step and radius, each with a usage note. It is the only place a value lives; this document names tokens and never restates their values. The [reference reader](../../tools/reference-reader/README.md) consumes it the same way: its builder turns the tokens into a stylesheet, and the reader's own stylesheet names tokens only.
- [`../assets/icon/`](../assets/icon/README.md) holds the mark in every format, generated from one geometry script.
- The same guidelines, with live previews of the wordmark and a cover, are published as a shareable [design-system page](https://claude.ai/artifact/4TtKnKzegCVbm24quQXbjK). This folder is the authority; the page is a mirror.

## Voice

- Write plain technical prose. Short sentences, one idea each, active voice. Say what a thing is and why it was decided that way.
- State design, never measurements. [`documentation-conventions.md`](../documentation-conventions.md) is the rule; a durable page names the decision and points at the live source for any count, tally or timing.
- Speak to the reader as *you*. Speak of the project as *Plato*, never "the Plato language" or "the Plato platform".
- Sentence case for headings and labels. "Plato" is always capitalised; identifiers stay exactly as the code spells them, in `code` style.
- No emoji, no exclamation marks, no marketing adjectives. The tagline is the loudest the brand gets:

  > Write geometry once. Run it everywhere.

- Describe strengths as consequences of the design, the way the [README](../../README.md) does: "Every port is generated, so the ports can't drift."

## Colour

Plato is dark first. The home ground is `ground` in the Dark theme; the Light theme exists for documents, print and pages that sit inside someone else's site.

- Set page backgrounds in `ground`. Set cards, panels and code blocks in `ground-raised`. Separate regions with a one-pixel `rule`, never with shadows.
- Set text in `ink`. Set secondary text, captions and labels in `ink-muted`. Both must meet the WCAG AA text contrast ratio on `ground` and `ground-raised` in both themes; a change to any of the four that breaks this is a change to the brand, not a tweak.
- `gold` is the brand colour. It belongs to the mark, large display type, and one primary action per page. In the Light theme `gold` does not meet the text contrast ratio against `ground`, so never set body or small text in it there; use it for the icon, for display type, and for fills with `on-gold` text.
- `blue` is the secondary colour, taken from the spokes of the reference drawing. Use it for links, selected states, and a second data series.
- Put `on-gold` text on any `gold` fill, in both themes. Never put `ink` on `gold` by name; the token exists so the pair stays legible if `gold` changes.
- No gradients, no transparency effects, no colour outside the tokens. A chart needs at most `gold`, `blue` and `ink-muted`.

## Typography

Three families, all hosted on Google Fonts, each with a real fallback stack in `tokens.json`.

- `display`: Spectral, for headings and the name on a cover. Semibold, never bold. Headings use `display-xl`, `heading-1` and `heading-2`.
- `sans`: IBM Plex Sans, for everything read or operated. Body copy in `body`, secondary copy in `body-small`, uppercase labels in `label` with its letter-spacing. The name beside the mark is `wordmark`.
- `mono`: IBM Plex Mono, for code, identifiers and anything numeric that lines up. Use `code` for blocks and inline code alike, and tabular numerals on any column of digits.
- Keep running text near a comfortable reading measure of about 65 characters. Do not justify. Balance heading line breaks.
- Do not substitute Inter, Roboto or a rounded geometric sans. The mark is faceted and sharp; the type must have straight strokes and real weight to sit beside it.

## Spacing, radius and layout

- Lay out on the spacing scale `space-1` through `space-16`. Card padding is `space-4`; section gaps are `space-8` or `space-12`; the page gutter is never less than `space-4`.
- Three radii for interface. `radius-sm` for inputs and chips, `radius-md` for buttons and cards, `radius-lg` for panels and image frames. `radius-tile` is only for the app-icon tile. Nothing is a pill.
- Borders, not shadows. A raised surface is `ground-raised` with a `rule` border; there is no shadow family on purpose.
- One column of reading text. Wide tables, code and diagrams scroll inside their own container rather than widening the page.

## The mark

The mark is the root spiral: the spiral of Theodorus, which Plato's geometry teacher drew and which the *Theaetetus* names. Eight right triangles with legs of one and hypotenuses √2 through √9, filled `gold`, separated by gaps where the spokes would be. The gaps are transparent, so the spokes are whatever ground the mark sits on. It is numbers made visible as geometry: irrationals, the thing floating point can only approximate, drawn exactly.

- Use the files in [`../assets/icon/`](../assets/icon/README.md) and nothing redrawn. `root-spiral.svg` is the master for dark grounds; `root-spiral-light.svg` is the same geometry in the Light theme's `gold`. Rasters are for favicons and places that cannot take SVG. To change the mark, change `make-icons.py` and regenerate.
- Keep the mark in `gold` on `ground` or `ground-raised`. The only other permitted ink is `ink`, for single-colour print. Never recolour it `blue`, never add an outline, a shadow or a second colour per wedge.
- Keep clear space of one eighth of the mark's width on every side. Nothing enters it, including the wordmark.
- Never draw the spokes as lines, never close the outer envelope with a stroke, and never show fewer or more than eight wedges. The sixteen-triangle sketch with hairline spokes is the origin of the mark, not a usable form of it.
- Do not rotate, mirror or skew it. The seed triangle (√2) points right and the spiral opens counter-clockwise.
- The smallest raster in the icon folder is the smallest permitted use. Below it, use the wordmark alone.
- Interface icons elsewhere on a page are a single-weight line set in `ink` or `ink-muted`. The mark is never used as a UI glyph.

## Wordmark

The name set in `wordmark` (IBM Plex Sans semibold) in `ink`, with the mark to its left.

- Mark height is the wordmark's font size plus a tenth, centred on the name's x-height. The gap between mark and name is `space-2`.
- In a button or a tab bar, the lockup fits the row and the mark shrinks with it.
- The name is never set in `display` beside the mark; Spectral is for headings and covers, not the lockup.
- On `gold` fills, set the name in `on-gold` and omit the mark.
