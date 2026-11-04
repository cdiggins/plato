#!/usr/bin/env python3
"""Turn the brand tokens into the reader's stylesheet and copy the mark.

``docs/brand/tokens.json`` is the only place a brand value lives. The reader
never restates a colour, font or spacing step: this module writes
``public/brand/tokens.css`` from the tokens and copies the icon files beside
it, and ``build.py`` runs it before the reader starts. Both outputs are build
products, ignored by git like ``reference.json``.

The first theme in the tokens is the default. A later theme applies when the
viewer prefers it (``prefers-color-scheme``), when the page sets
``data-theme`` on the root element, and, for the light theme, in print.
"""
from __future__ import annotations

import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
TOKENS = ROOT / 'docs/brand/tokens.json'
ICONS = ROOT / 'docs/assets/icon'
ICON_FILES = ('root-spiral.svg', 'root-spiral-light.svg', 'favicon.ico')
HEADER = '/* Plato — generated from docs/brand/tokens.json by tools/reference/build.py; do not edit. */'


def theme_value(value, theme: str, default: str) -> str:
    """A token value is a plain string (the default theme) or a map by theme id."""
    if isinstance(value, dict):
        return value.get(theme, value.get(default))
    return value


def colour_lines(tokens: dict, theme: str, default: str) -> list[str]:
    return [f'  --{t["name"]}: {theme_value(t["value"], theme, default)};' for t in tokens['color']['tokens']]


def type_lines(tokens: dict) -> list[str]:
    lines = [f'  --font-{name}: {stack};' for name, stack in tokens['type']['families'].items()]
    for group in tokens['type']['groups']:
        family = f'var(--font-{group["family"]})'
        for style in group['styles']:
            weight = style.get('fontWeight', 400)
            lines.append(f'  --type-{style["name"]}: {weight} {style["fontSize"]}/{style["lineHeight"]} {family};')
            if 'letterSpacing' in style:
                lines.append(f'  --type-{style["name"]}-tracking: {style["letterSpacing"]};')
    return lines


def scale_lines(tokens: dict, family: str) -> list[str]:
    return [f'  --{t["name"]}: {t["value"]};' for t in tokens.get(family, {}).get('tokens', [])]


def stylesheet(tokens: dict) -> str:
    themes = [t['id'] for t in tokens['color']['themes']]
    default = themes[0]
    block = lambda lines: '\n'.join(lines)
    parts = [HEADER, ':root {', f'  color-scheme: {default};',
             block(colour_lines(tokens, default, default)), block(type_lines(tokens)),
             block(scale_lines(tokens, 'spacing')), block(scale_lines(tokens, 'radius')), '}']
    for theme in themes[1:]:
        body = block([f'  color-scheme: {theme};'] + colour_lines(tokens, theme, default))
        parts.append(f'@media (prefers-color-scheme: {theme}) {{\n:root:not([data-theme]) {{\n{body}\n}}\n}}')
        parts.append(f':root[data-theme="{theme}"] {{\n{body}\n}}')
        if theme == 'light':
            parts.append(f'@media print {{\n:root {{\n{body}\n}}\n}}')
    return '\n'.join(parts) + '\n'


def write(folder: Path, tokens_path: Path = TOKENS, icons: Path = ICONS) -> None:
    """Write tokens.css and the icon files into ``folder``, creating it."""
    import json
    folder.mkdir(parents=True, exist_ok=True)
    tokens = json.loads(tokens_path.read_text(encoding='utf-8-sig'))
    (folder / 'tokens.css').write_text(stylesheet(tokens), encoding='utf-8', newline='\n')
    for name in ICON_FILES:
        shutil.copyfile(icons / name, folder / name)


if __name__ == '__main__':
    write(ROOT / 'tools/reference-reader/public/brand')
