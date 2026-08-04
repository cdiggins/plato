// Catmull-Rom spline swept into a tube.
//
// Both halves are stdlib types. `CatmullRomCurve3D` interpolates its control
// points (Alpha selects the parameterization: 0 uniform, 0.5 centripetal, 1
// chordal — centripetal is the one that cannot self-intersect or cusp).
// `TubeSurface` wraps a circle of the given radius around any ICurve3D and is
// itself an IParametricSurface, so the tube mesh is one ToTriangleMesh call.
//
// The frame TubeSurface sweeps is a fixed-up frame — tangent plus a
// deterministically chosen perpendicular — not a rotation-minimizing one; the
// two agree wherever the path does not turn about its own tangent. See the
// note at the head of stdlib/geometry/surfaces.library.plato.

import type { Drawable, Sample } from '../core/types.js';
import { CatmullRomCurve3D, Line3D, Point3D, TubeSurface } from '../plato/plato.g.js';
import { fromArray, toArray } from '../core/meshBuilder.js';

export const controlPoints: Point3D[] = [
    new Point3D(-1.6, -0.4, 0.6), new Point3D(-0.9, 0.5, -0.6), new Point3D(0.0, -0.3, 0.7),
    new Point3D(0.8, 0.6, -0.2), new Point3D(1.5, -0.2, 0.5), new Point3D(1.9, 0.7, -0.6),
];

/** A centripetal Catmull-Rom curve through the control points. */
export const spline = new CatmullRomCurve3D(fromArray(controlPoints), 0.5, false);

/** Samples the curve at count evenly spaced parameters over [0, 1]. */
export const sampleSpline = (curve: CatmullRomCurve3D, count: number): Point3D[] =>
    Array.from({ length: count }, (_, i) => curve.Eval(i / (count - 1)));

export const splineTubeSample: Sample = {
    id: 'spline-tube',
    title: 'Spline + Tube Sweep',
    description: 'CatmullRomCurve3D wrapped in a TubeSurface, tessellated by ToTriangleMesh.',
    build(): Drawable[] {
        // The cast works around plato-443: the generated TubeSurface declares
        // Path as ICurve3D<TubeSurface> — Self bound to the containing type — so
        // no actual curve satisfies the field's declared type.
        const tube = new TubeSurface(spline as never, 0.09);
        const controlPolygon = controlPoints
            .slice(0, -1)
            .map((p, i) => new Line3D(p, controlPoints[i + 1]));

        return [
            { kind: 'mesh', mesh: tube.ToTriangleMesh(160, 12), color: 0xc678dd },
            { kind: 'lines', segments: controlPolygon, color: 0x556070 },
            { kind: 'points', points: controlPoints, color: 0xffd166, size: 0.09 },
        ];
    },
};

/** Re-exported for tests: the curve's own samples. */
export const curveSamples = (count: number): Point3D[] => sampleSpline(spline, count);
export { toArray };
