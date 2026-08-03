// Smoke test: generated stdlib TypeScript must evaluate core SDF members.
import { Point3D, Vector3D, FunctionSdf3D } from '../src/plato/plato.g.ts';

let failures = 0;
function check(name: string, actual: number, expected: number, tol = 1e-9) {
  const ok = Math.abs(actual - expected) <= tol;
  if (!ok) failures++;
  console.log(`${ok ? 'ok  ' : 'FAIL'} ${name}: ${actual}${ok ? '' : ` (expected ${expected})`}`);
}

const p = new Point3D(1, 0, 0);
check('Between.Magnitude', new Point3D(0, 0, 0).Between(p).Magnitude(), 1);
check('DistanceToSphere', p.DistanceToSphere(0.5), 0.5);
check('DistanceToBox', p.DistanceToBox(new Vector3D(0.5, 0.5, 0.5)), 0.5);
check('SmoothUnionDistance', (2.0).SmoothUnionDistance(0.5, 0.25), 0.5);
const sdf = FunctionSdf3D.Create((q: Point3D) => q.DistanceToSphere(0.5));
check('FunctionSdf3D.Eval', sdf.Eval(p), 0.5);
check('FunctionSdf3D.Eval origin', sdf.Eval(new Point3D(0, 0, 0)), -0.5);

if (failures > 0) {
  console.error(`${failures} smoke check(s) failed`);
  process.exit(1);
}
console.log('smoke: all checks passed');
