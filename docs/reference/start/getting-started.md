# Getting started

Use these steps on a development machine with the Plato repository checked out.
They refresh the source-derived pages and open this reader.

## Requirements

- Python 3.10 or later
- Node.js 20.19 or later, or 22.12 or later
- npm (ships with Node.js)

## Build the reference data

From the repository root:

```powershell
python tools/reference/build.py
```

`build.py --check` fails when generated API evidence is stale or a local Markdown
link is broken.

## Run the reader

```powershell
npm --prefix tools/reference-reader install
npm --prefix tools/reference-reader run dev
```

Open the printed address, typically `http://127.0.0.1:5391`. Prefer that exact
loopback URL. A shared `localhost` port can hit a different process.

Build the production bundle with:

```powershell
npm --prefix tools/reference-reader run build
npm --prefix tools/reference-reader run preview
```

## Suggested reading order

1. [What is Plato](what-is-plato.md)
2. [Quick tour](tour.md)
3. [Language reference](../language/README.md)
4. The library domain you need from the [library guide](../library/README.md)

Implementers continue to the [implementation guide](../implementation/README.md)
and [known discrepancies](../implementation/discrepancies.md).

## Reader tips

Search with Ctrl K (⌘ K on macOS). Download any page as Markdown from the top bar.
Code examples use colored Plato and C# highlighting. Many type names link into the
API catalog.
