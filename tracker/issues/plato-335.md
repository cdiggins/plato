---
id: plato-335
title: "No CRS identity token: geospatial types cannot say which reference system they are in"
type: problem
status: idea
priority: p3
effort: S
risk: low
area: plato
sprint: 
created: 2026-07-30
closed:
links: [submodules/Plato/stdlib/geo-spatial-reference-systems.plato, submodules/Plato/stdlib/points-curvilinear.plato]
---

## Issue
The stdlib models the *machinery* of geospatial referencing well — `ReferenceEllipsoid`,
`GeodeticDatum`, `MapProjectionFamily` (a proper sum type, with conic standard
parallels as payload), `MapProjection`, `UtmZone`, `ProjectedCoordinate`,
`EcefCoordinate`, `EnuCoordinate`, and `GeoCoordinate` for lat/lon/altitude. What it
has no representation for is the *identity* of a reference system as a citable token
(EPSG-style authority + code, e.g. `EPSG:26910`), and consequently no way for a
coordinate or a model to state which system it is expressed in. `ProjectedCoordinate`
carries a bare `ZoneNumber: Integer` and nothing else; two `ProjectedCoordinate`
values from different projections are the same type and compare as if commensurable.

This is an open design question rather than a defect: the machinery may be the
deliberate scope, with identity left to the host application. The question is whether
Plato should model CRS identity at all, and if so, whether coordinates carry it.

## Impact
Matters specifically for AEC/BIM interop, which is Ara3D's market. Survey data,
IFC georeferencing (`IfcMapConversion`/`IfcProjectedCRS`, which is literally an
authority-and-code string plus an offset), GIS basemaps, and point-cloud deliverables
all identify their CRS by EPSG code, and the code is the thing that travels between
tools. Without a token, an importer can parse a projection into `MapProjection` but
cannot round-trip the identifier it came from, and cannot detect that two datasets
disagree. Today the consequence is silent: coordinates from different systems can be
added, and nothing objects.

Frequency is low right now because nothing in-repo consumes these types across
system boundaries — the geospatial file appears to be content-wave output not yet
wired to an importer.

## Affected code
- `submodules/Plato/stdlib/geo-spatial-reference-systems.plato:15-25` —
  `ReferenceEllipsoid`, `GeodeticDatum` (has `Name: String`, but a name is not an
  authority-qualified identifier).
- `submodules/Plato/stdlib/geo-spatial-reference-systems.plato:37-55` —
  `MapProjectionFamily`, `MapProjection`: fully parameterized, unnamed.
- `submodules/Plato/stdlib/geo-spatial-reference-systems.plato:60-79` — `UtmZone`,
  and `ProjectedCoordinate` whose only system reference is `ZoneNumber: Integer`,
  documented as "0 when the projection is not zonal" — a sentinel doing identity work.
- `submodules/Plato/stdlib/points-curvilinear.plato:42-48` — `GeoCoordinate`
  (lat/lon/altitude), untagged by datum; a WGS84 and an NAD83 position are the
  same type.
- No occurrence of `Epsg`, `Authority`, or `Code` anywhere in `stdlib/` — verified
  by grep.

## Cause / analysis
Not accidental omission — the file is coherent and well-documented, and reads as a
deliberate decision to model geometry-of-the-earth rather than metadata-about-
coordinates. The gap is at the boundary: Plato's value types describe *what a
coordinate is*, and CRS identity describes *what convention it was produced under*,
which is arguably host metadata. The tension is that in AEC the convention is
data — it is carried in the file format, must survive round-trip, and determines
whether an operation is legal.

Prior art worth weighing: the old `plato-src-v2` sketch had
`CoordinateReferenceSystem { Name, Authority, Code }` plus an
`IReferencedPosition<TPoint, TReferenceSystem>` interface pairing a point with its
system — the tagging approach. PROJ and GDAL take the same line. The counter-example
is that most geometry kernels deliberately stay CRS-free and push it to the
application layer.

## Priority
Recommend **p3**. No current consumer, so nothing is blocked today, and the answer
is cheap to implement once decided (a small record and possibly a wrapper type).
But it is a *design* question that gets more expensive after an importer exists —
retrofitting a tag onto a coordinate type that is already in use is a breaking
change, whereas deciding now costs one ADR. Worth answering before any AEC importer
work starts, not before.

## Dependencies
- Blocked by: nothing.
- Blocks: any georeferenced import/export work (none filed). Should be answered
  before that starts, or it will be answered implicitly and badly.
- Touches: `geo-spatial-reference-systems.plato`, `points-curvilinear.plato`. Small
  surface, low collision risk.

## Fix approaches
Candidate answers — closing this should produce an ADR plus follow-up issues, not a
direct code change.

1. **Identity token only** — add `CrsIdentifier { Authority: String, Code: String }`
   and let `GeodeticDatum`/`MapProjection` optionally carry one. Coordinates stay
   untagged. Cheap, round-trips EPSG codes, does not make illegal arithmetic
   unrepresentable.
2. **Token plus tagged position** — additionally a `ReferencedPosition<TPoint>`
   pairing a coordinate with its system, so cross-system arithmetic is at least
   visible at the type level. Closer to correct; costs a wrapper at every boundary
   and pressure to unwrap it in hot code.
3. **Explicitly out of scope** — record in an ADR that CRS identity is host
   metadata, and that the stdlib models geometry only. Zero code; sets the
   expectation so an importer author does not assume otherwise. Legitimate answer.

## Bedrock
The seam is the boundary between *a coordinate's value* and *the convention that
gives it meaning*. Plato already takes the strong position elsewhere that conventions
should be data rather than prose — `MapProjectionFamily` is a sum type with real
payload rather than a string, and CONVENTIONS.md exists precisely because implicit
conventions cause bugs (see plato-333, where a convention living in a doc comment
diverged from the code). The consistent extension of that principle is option 1 at
minimum: an authority-qualified identifier is data, and `ZoneNumber: Integer` with a
`0` sentinel is the prose-equivalent stand-in for it. Option 2 goes further and makes
the convention *enforceable*, which is where the same principle points, but it taxes
every call site.

Verdict: **right** — the question deserves a real answer, and options 1 and 3 are
both defensible; what is not defensible is leaving `ZoneNumber: Integer` as the de
facto identity field, because that is the sentinel pattern already flagged in
plato-079. Whatever is decided, that field should change.

## Done means
- [ ] ADR recording the decision and its reasoning
- [ ] `ProjectedCoordinate.ZoneNumber` no longer carries identity via a `0` sentinel
- [ ] follow-up issues filed for whatever the ADR implies
- [ ] if tagging is adopted, `GeoCoordinate` states its datum assumption explicitly
      (even a doc comment naming WGS84 as the default is an improvement)

## Simplest fix
Option 1: one record, two optional fields, no wrapper. Gain: EPSG codes round-trip,
the sentinel goes away, importers have somewhere to put what they parsed. Give up:
does not prevent mixing coordinates from different systems — that stays a runtime
concern.

## Prevention
- **Not a recurrence-prone class** — this is a one-off scope question, not a pattern.
  The generalizable part is the sentinel (`ZoneNumber: Integer` = 0 for "none"),
  already tracked as plato-079.
- Note for the record: this issue was originally filed on the false premise that
  stdlib had no CRS support at all. It has extensive support; the gap is only
  identity. Anyone auditing stdlib coverage should search by interface, not by the
  names another library happens to use.
