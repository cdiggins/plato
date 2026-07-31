---
lesson: geo-distance-haversine
title: Great-Circle Distance and the Haversine Formula
domain: Coordinate systems & bounds
v3-files: [68-geo-spatial.plato, 11-points.plato]
audience: High-school trigonometry and comfort with latitude/longitude as angles; no geodesy background assumed
status: draft-v1
---

# Great-Circle Distance and the Haversine Formula

Flat-map intuition says the distance between two GPS pins is
$\sqrt{(\Delta x)^2 + (\Delta y)^2}$. On Earth that is wrong as soon as the
pins are more than a few kilometres apart, and catastrophically wrong near the
poles or across the antimeridian. The shortest path on a sphere is a
**great-circle** arc — the intersection of the sphere with a plane through its
centre. The **haversine** formula is the numerically stable way to get that arc
length from two latitudes and longitudes.

## The idea

### Why Euclidean distance fails

Latitude $\phi$ and longitude $\lambda$ are angles, not Cartesian metres.
One degree of longitude is $\approx 111\,\mathrm{km}\cos\phi$ on the ground —
full width at the equator, zero at the poles. Treating $(\phi, \lambda)$ as if
they were $(x, y)$ stretches east–west distances by $1/\cos\phi$ and ignores
that longitude wraps at $\pm 180^\circ$.

```
        N
        |
   W----+----E     great-circle arc (shortest on the sphere)
        |  .-'
        |.'
        S
```

### Spherical law of cosines (the fragile form)

For unit sphere, angular separation $\Delta\sigma$ between points with
latitudes $\phi_1, \phi_2$ and longitudes $\lambda_1, \lambda_2$ satisfies

$$
\cos\Delta\sigma
  = \sin\phi_1\sin\phi_2
  + \cos\phi_1\cos\phi_2\cos\Delta\lambda
$$

with $\Delta\lambda = \lambda_2 - \lambda_1$. Ground distance is
$R\cdot\Delta\sigma$ for sphere radius $R$. This form loses precision when
points are very close ($\cos\Delta\sigma \approx 1$).

### Haversine (the robust form)

Define $\mathrm{hav}\,\theta = \sin^2(\theta/2)$. Then

$$
\mathrm{hav}\,\Delta\sigma
  = \mathrm{hav}\,\Delta\phi
  + \cos\phi_1\cos\phi_2\,\mathrm{hav}\,\Delta\lambda
$$

and

$$
\Delta\sigma = 2\arcsin\sqrt{\mathrm{hav}\,\Delta\sigma}
$$

(or $2\,\mathrm{atan2}(\sqrt{h},\,\sqrt{1-h})$ with $h = \mathrm{hav}\,\Delta\sigma$).
For Earth as a sphere, mean radius $R \approx 6\,371\,\mathrm{km}$ is a common
choice. Surface distance $= R\cdot\Delta\sigma$.

### Sphere vs ellipsoid

Real Earth is an oblate spheroid. Haversine on a sphere is typically within
0.5% for ordinary navigation distances; surveying and aviation use geodesic
distance on a **reference ellipsoid** (WGS84: semi-major axis and inverse
flattening). The spherical formula remains the right *first* tool and the
right teaching model.

### Tiny worked example

Paris roughly $\phi=48.857^\circ$, $\lambda=2.351^\circ$; London roughly
$\phi=51.507^\circ$, $\lambda=-0.128^\circ$. Convert to radians, plug into
haversine with $R=6371\,\mathrm{km}$: you get about $344\,\mathrm{km}$ —
compare with a naive degree-Euclidean estimate that wanders depending on
whether you scale longitude by $\cos\phi$.

## In Plato

Geodetic positions live on `GeoCoordinate`. Geospatial structure and the
planet model live beside them.

From `11-points.plato`:

```plato
// A position on a planet's surface: geodetic latitude, longitude, and altitude
// above the reference ellipsoid.
type GeoCoordinate
    implements Value
{
    Latitude: Angle;
    Longitude: Angle;
    Altitude: Length;
}
```

From `68-geo-spatial.plato`:

```plato
type GeoSegment
    implements Value
{
    Start: GeoCoordinate;
    End: GeoCoordinate;
}

type GeoCircle
    implements GeoRegion
{
    Center: GeoCoordinate;
    Radius: Length;
}

type ReferenceEllipsoid
    implements Value
{
    SemiMajorAxis: Length;
    InverseFlattening: Number;
}

type GeodeticDatum
{
    Name: String;
    Ellipsoid: ReferenceEllipsoid;
}

type GeoPath
{
    Coordinates: Array<GeoCoordinate>;
}
```

Usage-shaped sketches:

```plato
paris = GeoCoordinate {
    Latitude: Angle { Radians: 0.8527 };   // ~48.857°
    Longitude: Angle { Radians: 0.0410 };  // ~2.351°
    Altitude: Length { Meters: 35.0 };
}

london = GeoCoordinate {
    Latitude: Angle { Radians: 0.8990 };
    Longitude: Angle { Radians: -0.0022 };
    Altitude: Length { Meters: 11.0 };
}

leg = GeoSegment { Start: paris; End: london; }

wgs84 = GeodeticDatum {
    Name: "WGS84";
    Ellipsoid: ReferenceEllipsoid {
        SemiMajorAxis: Length { Meters: 6378137.0 };
        InverseFlattening: 298.257223563;
    };
}

// "Is London inside 400 km of Paris?" — GeoCircle uses geodesic Radius,
// but v3 does not yet declare the distance function that would feed it.
ring = GeoCircle {
    Center: paris;
    Radius: Length { Meters: 400000.0 };
}

route = GeoPath {
    Coordinates: [paris, london];
}
```

**Gap to state plainly:** v3 declares `GeoSegment`, `GeoPath`, and
`GeoCircle.Radius` as geodesic quantities, but declares **no** function that
returns a `Length` between two `GeoCoordinate` values (haversine, spherical
cosine, or ellipsoid geodesic). The formula above is the mathematics those
types presuppose; it is not yet a named operation in the vocabulary.

Altitude on `GeoCoordinate` is ignored for surface distance: haversine is a
horizontal-surface (or ellipsoid-surface) measure. For slant range through
space you would convert both points to `EcefCoordinate` and take a chord —
a different problem.

## Pitfalls / fine print

- **Degree/radian bugs.** `Angle` stores `Radians`. Feeding degree literals
  into $\sin/\cos$ without conversion is the classic GPS bug.
- **Antimeridian.** $\Delta\lambda$ must be wrapped into $(-\pi,\pi]$ before
  haversine; otherwise a short hop across $180^\circ$ looks like a trip around
  the world. `GeoBounds` documents the same wrap issue for regions.
- **Poles.** All longitudes meet; $\Delta\lambda$ becomes meaningless as a
  “sideways” offset. Haversine still works; local ENU frames are better for
  nearby relative motion (`EnuCoordinate`).
- **Sphere radius choice.** $6371\,\mathrm{km}$ mean vs equatorial
  $6378\,\mathrm{km}$ vs authalic radii — pick one and document it. Do not
  mix with ellipsoid geodesics mid-calculation.
- **Chord vs arc.** ECEF Euclidean distance is a straight tunnel through the
  Earth; haversine is along the surface. For continental distances they
  differ by kilometres.
- **`GeoPath` length.** Summing consecutive segment distances is the usual
  track length; there is no declared `PathLength` helper yet.

## Try it

1. Two points share latitude $0$ (equator) and differ by $1^\circ$ longitude.
   With $R=6371\,\mathrm{km}$, about how long is the great-circle distance?
2. Same $1^\circ$ longitude separation at latitude $60^\circ$. Does the distance
   grow, shrink, or stay the same vs the equator case?
3. Why is `GeoCircle { Center, Radius }` not enough to *evaluate*
   `ContainsCoordinate` without a distance primitive?

<details>
<summary>Answers</summary>

1. On the equator, $1^\circ$ of longitude is $R\cdot\pi/180 \approx 111.2\,\mathrm{km}$.
2. It shrinks by about $\cos 60^\circ = 1/2$, so roughly $55.6\,\mathrm{km}$.
   That is exactly why raw $\Delta\lambda$ in degrees is not a metre distance.
3. Containment is “geodesic distance from Center ≤ Radius”. Without a declared
   distance (or an equivalent predicate), the concept `GeoRegion.ContainsCoordinate`
   has nothing to compute with for `GeoCircle`.

</details>

## Library recommendations

- **missing-function** — `68-geo-spatial.plato`: no
  `Distance(a: GeoCoordinate, b: GeoCoordinate, radius: Length): Length` (spherical)
  or `Distance(a: GeoCoordinate, b: GeoCoordinate, ellipsoid: ReferenceEllipsoid): Length`
  (geodesic). `GeoSegment`, `GeoPath`, and `GeoCircle` all read as if that
  operation existed; the haversine lesson cannot name it in Plato today.
- **missing-function** — `68-geo-spatial.plato`:
  `ContainsCoordinate` on `GeoCircle` needs the same primitive; consider also
  `InitialBearing(a: GeoCoordinate, b: GeoCoordinate): CompassBearing` (forward
  azimuth), which navigation lessons always introduce beside distance.
- **pedagogy** — `11-points.plato`: `GeoCoordinate` doc comment says “above the
  reference ellipsoid” but the type does not name which datum. Teaching distance
  requires saying whether altitudes and lat/lon are WGS84 or something else —
  a `Datum` field or a parallel `GeodeticPosition { Coordinate; Datum }` would
  make that explicit.
- **doc-comment** — `68-geo-spatial.plato`: `GeoSegment` should state whether
  “shortest arc” means spherical great-circle or ellipsoid geodesic, and point
  at the (currently missing) distance function. Right now both readings are
  plausible and conflict at ~0.5% relative error.
