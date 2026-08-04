// Marching squares: iso-contours of a scalar field.
//
// Both halves are stdlib. `MetaBallSystem2D` is a set of radial falloff
// kernels (the Wyvill cubic) summed and offset by a threshold, so its zero
// level set is the blob outline — and because it implements IScalarField2D,
// any field operation applies to it.
//
// `IScalarField2D.IsoContour(domain, resolution, isoLevel)` is the stdlib's
// marching squares: sample the field on a lattice, read each cell's four
// corners into a 4-bit configuration, and look up which edges the contour
// crosses. It is numbered to match marching cubes in `library Voxels`, and
// each segment is wound with the at-or-above region on its left.

import type { Drawable, Sample } from '../core/types.js';
import {
    Bounds2D, IntegerVector2, Line3D, MetaBall2D, MetaBallSystem2D, Point2D, Point3D,
} from '../plato/plato.g.js';
import { fromArray, toArray } from '../core/meshBuilder.js';

/** Three metaballs: a smooth field whose contours change topology with the level. */
export const metaballs = new MetaBallSystem2D(fromArray([
    new MetaBall2D(new Point2D(-0.5, 0.1), 0.95, 1.0),
    new MetaBall2D(new Point2D(0.45, 0.25), 0.85, 0.85),
    new MetaBall2D(new Point2D(0.1, -0.45), 0.75, 0.7),
]), 0);

export const domain = new Bounds2D(new Point2D(-1.6, -1.4), new Point2D(1.6, 1.4));

/** The contour of the field at one level, lifted into the drawing plane. */
export function contourAt(isoLevel: number, height: number): Line3D[] {
    const segments = toArray(metaballs.IsoContour(domain, new IntegerVector2(160, 140), isoLevel));
    const lift = (p: Point2D): Point3D => new Point3D(p.X, height, p.Y);
    return segments.map(s => new Line3D(lift(s.A), lift(s.B)));
}

export const marchingSquaresSample: Sample = {
    id: 'marching-squares',
    title: 'Marching Squares',
    description: 'IScalarField2D.IsoContour over a stdlib MetaBallSystem2D, at three levels.',
    build(): Drawable[] {
        // The field peaks near 1 at a ball centre, so the levels stay inside (0, 1):
        // an iso level at or above the maximum has no crossing to find.
        const levels = [
            { iso: 0.15, color: 0x4da3ff, height: 0.0 },
            { iso: 0.4, color: 0xffd166, height: 0.012 },
            { iso: 0.75, color: 0xff6b6b, height: 0.024 },
        ];
        return levels.map(({ iso, color, height }): Drawable => ({
            kind: 'lines',
            segments: contourAt(iso, height),
            color,
        }));
    },
};
