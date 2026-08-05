// Triage for the polyhedron-studio members: ok / FAIL / NaN per member.
import '../src/plato/array-ext.ts';
import { Angle, ColorGradient, PolygonMesh3D } from '../src/plato/plato.g.ts';

function probe(name: string, run: () => unknown): void {
  try {
    const value = run();
    const bad =
      typeof value === 'number' && !Number.isFinite(value) ? ' NON-FINITE' : '';
    console.log(`ok   ${name}${bad}  ${describe(value)}`);
  } catch (error) {
    console.log(`FAIL ${name}  ${(error as Error).message}`);
    if (process.env.PROBE_STACK) console.log((error as Error).stack);
  }
}

function describe(value: unknown): string {
  if (value instanceof PolygonMesh3D) {
    return `V ${value.VertexCount()} E ${value.UndirectedEdgeCount()} F ${value.FaceCount()}`;
  }
  if (typeof value === 'number') return value.toPrecision(6);
  if (typeof value === 'string') return value;
  if (value && typeof (value as { Count?: () => number }).Count === 'function') {
    const arr = value as { Count(): number; At(i: number): unknown };
    const n = arr.Count();
    const first = n > 0 ? arr.At(0) : undefined;
    return `count ${n} first ${JSON.stringify(first)}`;
  }
  return String(value);
}

const cube = PolygonMesh3D.Cube();
const dodeca = PolygonMesh3D.Dodecahedron();

// Operators. Expected topology, cube seed:
// kis V14 E36 F24 · join V14 E24 F12 · needle V14 E36 F24 · zip V24 E36 F14
// ortho V26 E48 F24 · meta V26 E72 F48 · gyro V38 E60 F24
// chamfer V32 E48 F18 · propeller V32 E60 F30 · reflect V8 E12 F6
probe('Kis(0.3)', () => cube.Kis(0.3));
probe('Join(0)', () => cube.Join(0));
probe('Needle(0.2)', () => cube.Needle(0.2));
probe('Zip', () => cube.Zip());
probe('Ortho', () => cube.Ortho());
probe('Meta', () => cube.Meta());
probe('Gyro', () => cube.Gyro());
probe('Chamfer(0.7)', () => cube.Chamfer(0.7));
probe('Propeller', () => cube.Propeller());
probe('Reflect', () => cube.Reflect());
probe('ApplyConwayOperator(10)', () => cube.ApplyConwayOperator(10, 0.5, new Angle(0.2)));

// Readings. Cube projected to unit sphere: edge 2/sqrt(3)=1.1547, area 6*(4/3)=8,
// volume 8/(3*sqrt(3))=1.5396, ratio 1, planarity 0, euler 2.
probe('TotalFaceArea', () => cube.TotalFaceArea());
probe('EnclosedVolume', () => cube.EnclosedVolume());
probe('EdgeLengthRatio', () => cube.EdgeLengthRatio());
probe('PlanarityDeviation', () => cube.PlanarityDeviation());
probe('Sphericity', () => cube.Sphericity());
probe('EulerCharacteristic', () => cube.EulerCharacteristic());
probe('FaceAreas', () => cube.FaceAreas());
probe('UndirectedEdgeLengths', () => cube.UndirectedEdgeLengths());

// Colouring.
probe('StudioRamp', () => ColorGradient.StudioRamp());
probe('StudioRamp.ColorAtParameter(0.5)', () => ColorGradient.StudioRamp().ColorAtParameter(0.5));
probe('FaceAspects(arity)', () => cube.FaceAspects(0));
probe('FaceAspectColors(area)', () => dodeca.FaceAspectColors(1));
probe('FaceAspectColors(radius)', () => dodeca.Dual().FaceAspectColors(2));

// Presentation.
probe('Exploded(0.3)', () => cube.Exploded(0.3));
probe('Spherized(0.5)', () => PolygonMesh3D.RhombicDodecahedron().Spherized(0.5));

// Chains a page will actually run.
probe('Dodecahedron.Gyro', () => dodeca.Gyro());
probe('Cube.Chamfer.Chamfer', () => cube.Chamfer(0.7).Chamfer(0.7));
probe('Icosahedron.Kis.Dual', () => PolygonMesh3D.Icosahedron().Kis(0.1).Dual());
probe('Needle == Truncate.Dual topologically', () => {
  const a = cube.Needle(0);
  const b = cube.Truncate().Dual();
  return `nV ${a.VertexCount()}=${b.VertexCount()} nF ${a.FaceCount()}=${b.FaceCount()}`;
});
