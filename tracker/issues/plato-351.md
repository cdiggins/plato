---
id: plato-351
title: Evaluate GeoJSON-shaped type set for Plato
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-335, plato-274, plato-275]
---

## Idea
Would a set of types mapping to GeoJSON (Point, LineString, Polygon, Feature, FeatureCollection, …) be useful in Plato? Interpretation: first-class geometry/feature records aligned with the GeoJSON spec for interchange with web/GIS tooling — not a full OGC stack.

## Assumptions
- Interop with web maps / GIS exports is a real need (see geospatial types + plato-335 CRS gap).
- Plato already has Point2D/3D, polylines, polygons; GeoJSON is a *serialization vocabulary* more than new math.
- Without CRS identity (plato-335), GeoJSON types risk silent datum bugs.

## Design decisions
- **Scope** — geometry objects only vs Feature/FeatureCollection with properties (Dynamic/JSON?).
- **CRS** — assume WGS84 per GeoJSON RFC vs require explicit CRS token (plato-335).
- **2D vs 3D** — Position as Number2 vs optional altitude.

## Related
- [plato-335](plato-335.md) — no CRS identity token.
- Existing geo/point types in stdlib (`points.concepts.plato` Coordinate).
- [plato-274](plato-274.md) / [plato-275](plato-275.md) — SVG typed model + parser (parallel interchange idea).

## Approaches
Short term: GeoJsonPosition / GeoJsonGeometry sum type + ToJson/FromJson sketches without Feature properties.
Long term: FeatureCollection + Studio export path; CRS-aware positions.
Adjacent: TopoJSON; WKT.

## Bedrock
If pursued, sits at the **interchange boundary** (like SVG ideas), not inside core geometry. Verdict: **simplest-along-the-grain** only after CRS policy. Simple version must NOT pretend GeoJSON Positions are CRS-free Points interchangeable with engineering Point3D.

## Done means
- [ ] Written verdict (pursue/park/drop) with CRS dependency explicit
- [ ] If pursue: minimal geometry sum type + round-trip test against one fixture
- [ ] Documented non-goals (no full Simple Features)

## Simplest possible implementation
Sum type for Point/LineString/Polygon + Array coordinates; skip Features.
- Pros: small; tests interop.
- Cons: useless for real GIS without properties/CRS.

## Case against
- **Duplication.** Plato already has geometry; GeoJSON is JSON shape — belong in a serializer, not stdlib types.
- **CRS footgun.** RFC 7946 WGS84 default fights local engineering coordinates used elsewhere in Studio.
- **Properties blob.** Feature.properties forces Dynamic/JSON into core or a weak stringly API.
- **Maintenance.** Spec edge cases (right-hand rule, bbox, foreign members) expand forever.
- Verdict: **park** until plato-335 lands a CRS story; lean **drop** as stdlib core — better as an optional interchange library. Quality: weak as a core idea, moderate as a satellite package.
