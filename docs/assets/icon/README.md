# Plato icon

The root spiral: the spiral of Theodorus, named in Plato's *Theaetetus*. Eight right triangles with hypotenuses √2 … √9, filled gold, separated by transparent gaps where the spokes would be. Irrationals drawn exactly, as geometry rather than as a floating-point approximation. Usage rules for the mark are in the [brand guidelines](../../brand/README.md).

Every file here except this README is written by `make-icons.py`, which holds the geometry. Do not edit the outputs by hand; change the script and rerun it:

```bash
python docs/assets/icon/make-icons.py
```

| File | Use |
| --- | --- |
| `root-spiral.svg` | Master, gold for dark grounds. Gaps are transparent, so it also sits on light grounds. |
| `root-spiral-light.svg` | Same shape in a darker gold for light grounds where the brighter gold lacks contrast. |
| `root-spiral-<size>.png` | Transparent rasters, 16 to 512 px. The small sizes widen the gaps so the spokes stay visible. |
| `favicon.ico` | 16, 32 and 48 px frames from the PNGs above. |

For a web page, link the SVG as the favicon and the ICO as the fallback:

```html
<link rel="icon" href="root-spiral.svg" type="image/svg+xml">
<link rel="icon" href="favicon.ico" sizes="16x16 32x32 48x48">
```
