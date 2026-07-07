// The sample registry: pure modules, usable from both the browser app and
// Node tests. (The browser additionally loads each sample's source text via
// src/app/sources.ts, which uses Vite's ?raw imports.)

import type { Sample } from '../core/types.js';
import { parametricSurfaceSample } from './parametricSurface.js';
import { icosphereSample } from './icosphere.js';
import { terrainSample } from './terrain.js';
import { delaunaySample } from './delaunay.js';
import { convexHullSample } from './convexHull.js';
import { splineTubeSample } from './splineTube.js';
import { octreeSample } from './octree.js';
import { bvhSample } from './bvh.js';
import { halfEdgeSample } from './halfEdge.js';
import { raycastSample } from './raycast.js';
import { poissonDiskSample } from './poissonDisk.js';
import { marchingSquaresSample } from './marchingSquares.js';

export const samples: Sample[] = [
    parametricSurfaceSample,
    icosphereSample,
    terrainSample,
    delaunaySample,
    convexHullSample,
    splineTubeSample,
    octreeSample,
    bvhSample,
    halfEdgeSample,
    raycastSample,
    poissonDiskSample,
    marchingSquaresSample,
];
