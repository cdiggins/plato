---
lesson: geo-coordinates
title: Geographic Coordinates
domain: Coordinate systems & bounds
v3-files: [11-points.plato, 68-geo-spatial.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Geographic Coordinates

Drop two pins on a map ten city blocks apart and your intuition about flat vectors still
works: “east 200 meters, north 50 meters” is fine. Drop pins in San Francisco and Tokyo
and the same habit lies to you. The shortest path is not a straight line on the page; a
degree of longitude shrinks as you approach the poles; and “up” is not a single world
$Z$ axis shared by every city.

Geographic coordinates exist because Earth is (approximately) an oblate spheroid, maps
are flat lies we choose carefully, and navigation needs headings measured from true
north — not from the $X$ axis of whatever CAD scene you opened last.

## The idea

### Latitude, longitude, altitude

A **geodetic** position names a point relative to a **reference ellipsoid** (a flattened
sphere fitted to sea level):

- **Latitude** — angle north or south of the equator (usually $[-90^\circ, +90^\circ]$).
- **Longitude** — angle east or west of a prime meridian (usually $(-180^\circ, +180^\circ]$
  or $[0^\circ, 360^\circ)$).
- **Altitude** — height above the ellipsoid (or a related geoid/sea-level surface —
  applications must say which).

```
        N pole
          *
         /|\
        / | \     latitude φ: from equator toward pole
    W--+--|--+-E  longitude λ: around the axis
        \ | /
         \|/
          *
```

These are **not** Cartesian $(x,y,z)$ in meters. Treating $(\lambda, \phi)$ as a
`Point2D` and subtracting yields “degree vectors” whose length in meters depends on
latitude. Near the equator, $1^\circ$ of longitude is about $111\,\mathrm{km}$; at
$60^\circ$N it is about half that.

### Why flat vector math breaks

1. **Great circles / geodesics.** The shortest surface path between two points is a
   geodesic on the ellipsoid (a great circle on a perfect sphere). Map-straight lines
   (rhumb lines) keep constant bearing but are longer.
2. **Antimeridian.** A region from longitude $170^\circ$ to $-170^\circ$ crosses the wrap
   seam. Min/max longitude without a wrap rule invents a huge false box.
3. **Poles.** All longitudes meet; small steps in longitude are meaningless; bearings
   become singular.
4. **Local frames.** Engineers near a job site want East/North/Up meters. That frame is
   only valid near its origin; it is not a global chart.

### Map projections and ECEF

To draw or compute in the plane, pick a **map projection** (Mercator, UTM, …). Every
projection distorts something: angles, areas, or distances. **ECEF** (Earth-centered,
Earth-fixed) is a true 3D Cartesian frame with origin at Earth’s center — great for
satellites and geometry libraries, awkward for “how far is the cafe down the street.”

## In Plato

`11-points.plato` declares the geodetic position type among other coordinate systems:

```plato
// A position on a planet's surface: geodetic latitude, longitude, and altitude
// above the reference ellipsoid.
type GeoCoordinate
{
    Latitude: Angle;
    Longitude: Angle;
    Altitude: Length;
}
```

Angles are `Angle`, altitude is `Length` — not raw `Number`. That matches the quantity
convention: you do not silently add a latitude to a meter offset.

`68-geo-spatial.plato` builds the working vocabulary around that point type: regions,
paths, datums, projections, and alternate Cartesian embeddings.

```plato
type GeoBounds
    implements GeoRegion
{
    South: Angle;
    North: Angle;
    West: Angle;
    East: Angle;
}

type GeoPath
{
    Coordinates: Array<GeoCoordinate>;
}

type GeoPolygon
    implements GeoRegion
{
    Boundary: GeoPath;
    Holes: Array<GeoPath>;
}

type GeoSegment
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

type CompassBearing
{
    Angle: Angle;  // clockwise from true north
}

type GeoPose
{
    Position: GeoCoordinate;
    Heading: CompassBearing;
    Pitch: Angle;
    Roll: Angle;
}

type ReferenceEllipsoid
{
    SemiMajorAxis: Length;
    InverseFlattening: Number;
}

type GeodeticDatum
{
    Name: String;
    Ellipsoid: ReferenceEllipsoid;
}

type EcefCoordinate
{
    X: Length;
    Y: Length;
    Z: Length;
}

type EnuCoordinate
{
    East: Length;
    North: Length;
    Up: Length;
    Origin: GeoCoordinate;
}

type ProjectedCoordinate
{
    Easting: Length;
    Northing: Length;
    ZoneNumber: Integer;
}
```

`GeoBounds` documents the antimeridian rule explicitly: West greater than East means the
band crosses the wrap. `GeoRegion` is the containment concept:

```plato
concept GeoRegion
{
    ContainsCoordinate(x: Self, coordinate: GeoCoordinate): Boolean;
}
```

Usage-shaped sketches:

```plato
let sf = GeoCoordinate {
    Latitude: /* 37.77° N */,
    Longitude: /* 122.42° W */,
    Altitude: /* 0 m */
};

let pacificWrap = GeoBounds {
    South: /* -10° */,
    North: /* 10° */,
    West: /* 170° */,
    East: /* -170° */   // West > East ⇒ crosses antimeridian
};

let local = EnuCoordinate {
    East: /* 30 m */,
    North: /* -10 m */,
    Up: /* 0 m */,
    Origin: sf
};

let ecef = EcefCoordinate { X: /* … */, Y: /* … */, Z: /* … */ };
// Same physical point as sf once a GeodeticDatum (e.g. WGS84) is fixed
```

`MapProjectionFamily` is a sum type (`Mercator | WebMercator | Utm | …`) configuring a
`MapProjection`. `UtmZone` and `MapTileIndex` cover zonal grids and slippy-map tiles.
`ElevationGrid` stores terrain samples over a `GeoBounds`.

Contrast with Cartesian `Point3D`: geo types refuse to pretend the planet is a flat
vector space. You convert into `EcefCoordinate`, `EnuCoordinate`, or
`ProjectedCoordinate` when you need Euclidean tools.

## Pitfalls / fine print

**Degree arithmetic.** Subtracting longitudes without normalizing to $(-180, 180]$ (or
your chosen chart) invents $350^\circ$ “short” hops that are actually $10^\circ$ the other
way.

**Altitude ambiguity.** Ellipsoid height ≠ orthometric (sea-level) height. Aviation,
surveying, and game “Y-up” scenes often silently disagree.

**Datum mismatch.** The same lat/lon numbers on NAD27 vs WGS84 are different points on
Earth — sometimes by hundreds of meters. Always carry a `GeodeticDatum` when precision
matters.

**Local ENU range.** `EnuCoordinate` is excellent within a few kilometers of `Origin`.
Across a continent, curvature and scale errors dominate; switch to geodesic formulas or
ECEF.

**Spherical shortcuts.** Haversine on a sphere is often “good enough”; ellipsoidal
geodesics (Vincenty / Karney) are the engineering truth for long baselines. The type
`GeoSegment` names the geodesic idea; the implementation must pick a model.

**Bearing vs math angle.** `CompassBearing` is clockwise from true north. Graphics
angles are often counter-clockwise from $+X$. Converting without an explicit basis change
rotates every vehicle the wrong way.

## Try it

1. At latitude $0^\circ$, roughly how many kilometers is $1^\circ$ of longitude? At
   $60^\circ$N, what happens to that length?
2. Why can `GeoBounds` with West $= 170^\circ$ and East $= -170^\circ$ be a small Pacific
   strip rather than almost the whole world?
3. You need to test whether a drone is inside a $500\,\mathrm{m}$ geofence. Which type
   fits better: `GeoCircle` or a Cartesian `Circle` on lon/lat as `Point2D`?

<details>
<summary>Answers</summary>

1. About $111\,\mathrm{km}$ at the equator; about half that at $60^\circ$ (scales with
   $\cos\phi$).
2. Because the type defines the longitude band as traveling eastward from West to East,
   and West > East means the band crosses the antimeridian — a short wrap, not the long
   way around.
3. `GeoCircle` — radius is a surface `Length` from a `GeoCoordinate` center. Lon/lat as
   `Point2D` makes “500 m” a nonsense number of degrees.

</details>

## Library recommendations

- **missing-function** — `68-geo-spatial.plato`: `GeoSegment` and `GeoCoordinate` have no
  declared `GeodesicDistance`, `InitialBearing`, or `Destination(distance, bearing)`
  helpers. Teaching “Earth ruins flat vectors” without a named geodesic distance on the
  vocabulary leaves the punchline unimplemented at the API surface.

- **missing-function** — `11-points.plato` / `68-geo-spatial.plato`: no declared conversion
  trio `GeoCoordinate` ↔ `EcefCoordinate` ↔ `EnuCoordinate` (given a `GeodeticDatum`).
  Those conversions are the bridge every geospatial pipeline needs; they should be
  first-class once libraries land.

- **doc-comment** — `11-points.plato`: `GeoCoordinate` does not state latitude/longitude
  ranges or the longitude wrap convention. A one-line normative range (and “altitude
  above ellipsoid”) would prevent silent degree-vs-radian and wrap bugs in callers.

- **pedagogy** — `68-geo-spatial.plato`: `GeoBounds.ContainsCoordinate` ignores altitude
  (documented on `GeoRegion`), which is correct for surface regions but surprising next
  to `GeoCoordinate.Altitude`. A sibling vertical interval or explicit “2.5D” note on
  `GeoCircle` would clarify airspace vs map-fence use.
