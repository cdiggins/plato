"""Generate the Plato root-spiral icon (the "Split" treatment) as SVG and raster files.

Geometry: the spiral of Theodorus. Vertex k sits at distance sqrt(k) from the origin,
turned by the sum of atan(1/sqrt(j)) for j < k. Eight right triangles (hypotenuses
sqrt(2) .. sqrt(9)) are filled gold and separated by transparent gaps where the spokes
would be, so the spokes read as negative space on any ground.

Run from the repo root:  python docs/assets/icon/make-icons.py
Requires Pillow. Rewrites every file in this folder except this script and README.md.
"""
from __future__ import annotations

import math
from pathlib import Path

from PIL import Image, ImageDraw

HERE = Path(__file__).resolve().parent
N = 8                      # triangles drawn
PAD = 4.0                  # margin inside the 100-unit box
GAP = 4.0                  # spoke gap, in box units, for the SVG and large rasters
MIN_GAP_PX = 1.3           # rasters never get a gap thinner than this (keeps 16 px legible)
GOLD = (0xE3, 0xB5, 0x3F)        # on dark grounds
GOLD_LIGHT = (0xC9, 0x96, 0x1A)  # on light grounds
PNG_SIZES = (16, 32, 48, 64, 128, 256, 512)
SUPERSAMPLE = 8


def vertices(n: int) -> list[tuple[float, float]]:
    pts, theta = [], 0.0
    for k in range(1, n + 2):
        r = math.sqrt(k)
        pts.append((r * math.cos(theta), -r * math.sin(theta)))  # y flipped: counter-clockwise on screen
        theta += math.atan(1 / math.sqrt(k))
    return pts


def fitted(n: int, pad: float) -> tuple[tuple[float, float], list[tuple[float, float]]]:
    """Origin and vertices mapped into a 100x100 box."""
    v = vertices(n)
    xs = [0.0] + [p[0] for p in v]
    ys = [0.0] + [p[1] for p in v]
    w, h = max(xs) - min(xs), max(ys) - min(ys)
    s = (100 - 2 * pad) / max(w, h)
    ox = pad + (100 - 2 * pad - w * s) / 2 - min(xs) * s
    oy = pad + (100 - 2 * pad - h * s) / 2 - min(ys) * s
    f = lambda p: (p[0] * s + ox, p[1] * s + oy)
    return f((0.0, 0.0)), [f(p) for p in v]


def hexc(rgb: tuple[int, int, int]) -> str:
    return "#%02x%02x%02x" % rgb


def svg(color: tuple[int, int, int]) -> str:
    o, q = fitted(N, PAD)
    pts = " ".join(f"{x:.2f},{y:.2f}" for x, y in [o, *q])
    spokes = "".join(
        f'<line x1="{o[0]:.2f}" y1="{o[1]:.2f}" x2="{x:.2f}" y2="{y:.2f}"/>' for x, y in q
    )
    return (
        '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" role="img" aria-label="Plato">\n'
        f'  <mask id="gaps"><rect width="100" height="100" fill="#fff"/>'
        f'<g stroke="#000" stroke-width="{GAP:g}" stroke-linecap="round">{spokes}</g></mask>\n'
        f'  <polygon points="{pts}" fill="{hexc(color)}" mask="url(#gaps)"/>\n'
        "</svg>\n"
    )


def raster(size: int, color: tuple[int, int, int]) -> Image.Image:
    big = size * SUPERSAMPLE
    scale = big / 100
    o, q = fitted(N, PAD)
    gap_units = max(GAP, MIN_GAP_PX * 100 / size)
    gap = gap_units * scale
    im = Image.new("RGBA", (big, big), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.polygon([(x * scale, y * scale) for x, y in [o, *q]], fill=(*color, 255))
    clear = (0, 0, 0, 0)
    ox, oy = o[0] * scale, o[1] * scale
    for x, y in q:
        d.line([(ox, oy), (x * scale, y * scale)], fill=clear, width=round(gap))
        d.ellipse([x * scale - gap / 2, y * scale - gap / 2, x * scale + gap / 2, y * scale + gap / 2], fill=clear)
    d.ellipse([ox - gap / 2, oy - gap / 2, ox + gap / 2, oy + gap / 2], fill=clear)
    return im.resize((size, size), Image.LANCZOS)


def main() -> None:
    (HERE / "root-spiral.svg").write_text(svg(GOLD), encoding="utf-8")
    (HERE / "root-spiral-light.svg").write_text(svg(GOLD_LIGHT), encoding="utf-8")
    pngs = {}
    for size in PNG_SIZES:
        im = raster(size, GOLD)
        im.save(HERE / f"root-spiral-{size}.png")
        pngs[size] = im
    # Pillow keeps every supplied frame only when the largest image is the one saved.
    ico = [pngs[48], pngs[32], pngs[16]]
    ico[0].save(HERE / "favicon.ico", format="ICO", sizes=[(i.width, i.height) for i in ico], append_images=ico[1:])
    print("wrote", len(PNG_SIZES) + 3, "files to", HERE)


if __name__ == "__main__":
    main()
