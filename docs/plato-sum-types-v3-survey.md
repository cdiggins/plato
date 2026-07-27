# Sum types in plato-src-v3 — kind-pattern migration survey

**Date:** 2026-07-27 · **Tracker:** [plato-232](../../../tracker/issues/plato-232.md) ·
**Parent doc:** [`plato-sum-types-design-2026-07-27.md`](plato-sum-types-design-2026-07-27.md)

Companion survey for the sum-type feature. It enumerates every `XxxKind`
kind-pattern declaration in `plato-src-v3`, classifies each as a **pure enum** or a
**true sum**, and recommends the flagship migrations for wave 3. It **drafts** the
"after" of `40-paths.plato`'s affected types — a draft only; `plato-src-v3` is not
edited here.

---

## 1. Counts

```
type <Name>Kind declarations in plato-src-v3 : 115
distinct files containing them               : 39
```

(Reproduce: `Select-String -Path plato-src-v3\*.plato -Pattern '^type\s+\w+Kind\b'`.)

## 2. Classification method

Two shapes hide under the single `XxxKind` convention (every one is a
`type XxxKind { Value: Integer; }` today):

- **Pure enum (degenerate sum).** The `Kind` is a *uniform selector*: it labels a
  variant but the carrier that holds it has the same fields regardless of the tag.
  Example: `LineCapKind` on a stroke style — every stroke has a cap; nothing about the
  cap value makes other fields appear or vanish. Migration is a mechanical collapse to a
  payload-free sum, `type LineCap = Butt | Round | Square;`, with **zero behavior
  change** — pure clarity + exhaustiveness. This is the dominant pattern.
- **True sum.** A carrier type pairs the `Kind` with fields that are *conditionally
  meaningful* — valid for some tags, junk (zero / `-1` / "ignored") for others. The
  `PathSegment2D` archetype. Migration folds the carrier and its `Kind` into one sum
  with per-case payloads, and is where the type-safety value lives.

True sums were found by scanning the folder for conditional-payload language in
doc-comments (`ignored`, `apply only when`, `when Kind`, `-1 when unused`, `hold
zeroes`) and reading each carrier. The default classification is **enum**; a row is
**SUM** only when a conditional-payload carrier was verified, and **SUM?** when the
structure strongly suggests a true sum but the carrier was not fully read (confirm at
migration).

## 3. Split

| Class | Count | Notes |
|-------|-------|-------|
| **True sum (verified)** | 11 | conditional-payload carrier read and confirmed |
| **True sum? (borderline)** | ~4 | `LightKind`, `SdfCombineKind`, `ColumnKind`, `BrdfKind` — likely per-tag payloads; confirm at migration |
| **Pure enum** | ~100 | uniform selectors; mechanical collapse to payload-free sums |
| **Total** | 115 | |

The headline: **the large majority (~100 of 115) are degenerate enums**; only ~11–15
are true sums, and those carry the migration value.

### Verified true sums

| Kind | File | Carrier | Conditional payload |
|------|------|---------|---------------------|
| PathSegmentKind | 40-paths | `PathSegment2D` | P1/P2 control pts, Radii/AxisRotation/LargeArc/Sweep "apply only when Kind is Arc" |
| PaintKind | 41-vector-styling | `FillStyle` | Color/Linear/Radial/Sweep/Texture — "others hold defaults and are ignored" |
| NodeContentKind | 43-scene2d | `SceneNode2D` | ContentIndex selects one of several parallel pools (indexed/deferred payload) |
| MaskKind | 43-scene2d | `ClipMask2D` | SourceNodeIndex "-1 when unused, i.e. when Kind is Path" |
| FieldOperationKind | 26-fields | `ScalarFieldNode2D/3D` | InputA/B/C/Source/Constant used per-op (14-way expression-graph node) |
| ThresholdKind | 46-image-processing | threshold settings | Level "ignored by Otsu" |
| AlphaModeKind | 50-materials | `AlphaSettings` | Cutoff is the Mask-mode threshold (conditional on Mode) |
| MovingWindowKind | 58-statistics | moving-window params | std-dev "ignored for Simple" |
| WindowFunctionKind | 60-signals | `AnalysisWindow` | ShapeParameter = Kaiser β / Gaussian σ / Tukey fraction, "ignored by the other kinds" |
| EmitterShapeKind | 57-particles-simulation | `ParticleEmitter2D/3D` | ConeAngle "used by cone-shaped emitters (ignored otherwise)" |
| ErrorPropagationKind | 63-uncertainty | `ErrorPropagationParameters` | SampleCount/Seed "apply only to the MonteCarlo kind and are ignored by the others" |

## 4. Full enumeration (all 115, grouped by file)

Rows are `SUM` / `SUM?` / (blank = enum). File numbers are the `plato-src-v3` prefixes.

| File | Kind types (class) |
|------|--------------------|
| 24-surfaces | SubdivisionSchemeKind |
| 25-solids | CsgOperationKind, PlatonicSolidKind, ArchimedeanSolidKind |
| 26-fields | **FieldOperationKind (SUM)** |
| 27-implicit-sdf | SdfCombineKind (SUM?) |
| 28-noise | NoiseKind, WorleyDistanceKind, WorleyFeatureKind |
| 29-sampling-grids | SamplePatternKind, InterpolationSchemeKind, GridBoundaryKind |
| 30-topology | WindingOrderKind, ManifoldnessKind |
| 32-mesh-attributes | AttributeDomainKind |
| 35-spatial-queries | ContainmentKind |
| 36-easing | EasingFamilyKind, EasingPhaseKind, EasingKind, StepPositionKind |
| 37-keyframes-tracks | InterpolationKind, ExtrapolationKind, PlaybackDirectionKind |
| 38-skeletal-animation | IkSolverKind |
| 39-motion-graphics | CycleKind, StaggerOriginKind, OscillatorKind |
| 40-paths | **PathSegmentKind (SUM)**, FillRuleKind, PathBooleanOperationKind |
| 41-vector-styling | LineCapKind, LineJoinKind, StrokeAlignKind, GradientSpreadKind, **PaintKind (SUM)** |
| 42-text | FontStyleKind, FontStretchKind, TextDecorationKind, TextAlignKind, TextBaselineKind, TextDirectionKind, TextOverflowKind |
| 43-scene2d | **NodeContentKind (SUM)**, **MaskKind (SUM)** |
| 44-color-spaces | WhitePointKind, TransferFunctionKind, ColorSpaceKind, RenderingIntentKind, ChromaticAdaptationKind, ColorHarmonyKind, ColorDifferenceKind |
| 45-images | ImageOriginKind, PixelFormatKind, ImageCodecKind |
| 46-image-processing | EdgeDetectionKind, MorphologyKind, StructuringElementKind, **ThresholdKind (SUM)**, ColorChannelKind, BlendModeKind, PorterDuffKind, ResampleFilterKind, ImageOrientationKind, DitherKind |
| 47-texturing | WrapModeKind, FilterModeKind, CubemapLayoutKind, TextureProjectionKind, TextureChannelMaskKind |
| 48-cameras | CameraProjectionKind, FisheyeMappingKind |
| 49-lights | AttenuationKind, LightKind (SUM?) |
| 50-materials | **AlphaModeKind (SUM)**, BrdfKind (SUM?) |
| 51-scene3d | VisibilityKind, SceneCameraKind |
| 52-render-settings | VertexAttributeKind, TonemapKind, AntiAliasingKind, FogKind, DenoiserKind, ShadowFilterKind, QualityKind, FaceCullingKind, DisplayColorSpaceKind |
| 54-rigid-dynamics | BodyMotionKind, CombineKind |
| 55-collision | CollisionEventKind |
| 57-particles-simulation | **EmitterShapeKind (SUM)** |
| 58-statistics | CorrelationKind, **MovingWindowKind (SUM)**, OutlierDetectionKind |
| 60-signals | **WindowFunctionKind (SUM)**, BiquadFilterKind, SignalNoiseKind, WaveformKind, ResamplingKind, CrossfadeKind |
| 62-optimization | OptimizationGoalKind, TerminationReasonKind, LineSearchKind, ConstraintKind |
| 63-uncertainty | **ErrorPropagationKind (SUM)** |
| 64-differential-geometry | SurfacePointShapeKind |
| 65-graphs | GraphLayoutKind |
| 66-engineering | BeamSupportKind, BeamLoadKind, FitKind |
| 67-scientific-data | ColumnKind (SUM?), ResamplingPolicyKind, MissingValuePolicyKind |
| 68-geo-spatial | MapProjectionKind |
| 69-higher-dimensions | RegularPolytopeKind, Projection4DKind |

## 5. Recommended flagship migrations (wave 3)

Five, chosen to (a) prove the feature across representative sum shapes and (b) start
with the archetype. **`40-paths.plato` goes first.**

1. **`40-paths.plato` — `PathSegment2D`** (PathSegmentKind). The archetype: 6 verbs,
   heavily conditional payload, SVG-domain, high demo value. This is the "before" the
   design doc quotes. Also collapses `FillRuleKind` and `PathBooleanOperationKind` to
   enum sums in the same file. **First.**
2. **`41-vector-styling.plato` — `FillStyle`** (PaintKind). Sibling of paths in the
   same 2D-vector domain: a 5-way paint union (Solid | Linear | Radial | Sweep |
   Texture). Demonstrates the *outer-product wrapper* pattern — the shared `Opacity`
   field stays on `FillStyle`, the union becomes a `Paint` sum
   (`type FillStyle { Paint: Paint; Opacity: Proportion; }`).
3. **`43-scene2d.plato` — `ClipMask2D`** (MaskKind) [+ `NodeContent`]. Scene-graph
   unions. `MaskKind` is the clean case (Path vs Alpha/Luminance, with the conditional
   `SourceNodeIndex`); `NodeContentKind` demonstrates the harder *indexed/deferred*
   variant where the payload is a pool index — a good stress test for the migration.
4. **`26-fields.plato` — `ScalarFieldNode2D/3D`** (FieldOperationKind). The textbook
   tagged-union / expression-tree node (14 operations, per-op operands). Proves sum
   types beyond geometry, and is the shape most improved by exhaustiveness — an
   evaluator `match`ing every op cannot silently forget one.
5. **`60-signals.plato` — `AnalysisWindow`** (WindowFunctionKind). A crisp "parameters
   union": `ShapeParameter` means different things per window and is ignored by most.
   Small, self-contained, an exemplar for the ~dozen parameter-carrier true sums.
   *(Runners-up: `57` EmitterShape, `63` ErrorPropagation — same shape, pick per wave-3
   appetite.)*

After the flagship five, the ~100 pure enums migrate in bulk mechanically (collapse
`type XxxKind { Value: Integer; }` + its selector field into `type Xxx = A | B | …;`),
one file at a time, gated on the same `lint` + regen checks.

---

## Appendix A — `40-paths.plato` affected types, drafted "after"

**Draft only — `plato-src-v3` is not edited by this doc.** Shows the four affected
declarations. `PathSegmentKind`, `FillRuleKind`, and `PathBooleanOperationKind` are
**removed** (absorbed into the sums). `Contour2D`, `Path2D`, `PathLocation`,
`CornerRadii2D`, and the parameter types are unchanged except where they reference a
renamed type (`Path2D.FillRule`).

```plato
// The interior rule of a self-intersecting or multi-contour path.
type FillRule = NonZero | EvenOdd;

// The boolean (clipping) operation used when combining two paths.
type PathBooleanOperation = Union | Intersection | Difference | ExclusiveOr;

// One drawing verb of a contour, following SVG path semantics. Every segment
// starts at the previous segment's endpoint; each verb carries exactly the
// control data it needs and nothing more.
type PathSegment2D
    implements Value
    = Move(EndPoint: Point2D)                       // start a new sub-path
    | Line(EndPoint: Point2D)                        // straight line to EndPoint
    | Quadratic(Control: Point2D, EndPoint: Point2D) // one control point
    | Cubic(Control1: Point2D, Control2: Point2D, EndPoint: Point2D) // two control points
    | Arc(Radii: Vector2, AxisRotation: Angle, LargeArc: Boolean, Sweep: Boolean, EndPoint: Point2D) // SVG elliptical arc
    | Close;                                          // line back to the sub-path start

// A connected run of segments (a sub-path). Unchanged: still references
// PathSegment2D by value.
type Contour2D
    implements Value
{
    Segments: Array<PathSegment2D>;
    Closed: Boolean;
}

// A complete vector path. Only the FillRule field's TYPE changes (FillRuleKind
// -> FillRule); the field name is unchanged.
type Path2D
    implements Value
{
    Contours: Array<Contour2D>;
    FillRule: FillRule;
}
```

Notes for the wave-3 migrator:
- The old `PathSegment2D` fields `P1`/`P2`/`P3` (with "the on-curve endpoint is always
  P3") become per-case `Control`/`Control1`/`Control2`/`EndPoint`. Any stdlib code that
  read `seg.P3` becomes an `EndPoint(seg)` match (see `plato-test-sum/pathsegment.plato`)
  or a per-case field access after a `match`.
- Every construction site `PathSegment2D { Kind = PathSegmentKind(2), P1 = c, P3 = e }`
  becomes `PathSegment2D.Quadratic(c, e)`.
- The three removed `XxxKind` types leave no orphan references inside `40-paths.plato`;
  a folder-wide check for external references to `FillRuleKind` /
  `PathBooleanOperationKind` is part of the migration diff.
