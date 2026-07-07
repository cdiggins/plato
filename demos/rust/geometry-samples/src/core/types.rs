// Scene-description types shared by all samples.
//
// Geometry itself comes from the generated Plato library (src/plato.rs);
// these types only describe what to draw. (Port of the TypeScript
// demos/typescript/geometry-samples/src/core/types.ts — rendering-specific options such as
// color and opacity are omitted: the Rust port has no renderer.)

/// A drawable produced by a sample. Positions are flat [x0,y0,z0, x1,y1,z1, ...].
#[derive(Clone, Debug)]
pub enum Drawable {
    /// An indexed triangle mesh.
    Mesh {
        positions: Vec<f64>,
        indices: Vec<u32>,
        normals: Vec<f64>,
    },
    /// Disjoint line segments: every consecutive pair of points is one segment.
    Lines { positions: Vec<f64> },
    /// A point cloud.
    Points { positions: Vec<f64> },
}

impl Drawable {
    pub fn positions(&self) -> &Vec<f64> {
        match self {
            Drawable::Mesh { positions, .. } => positions,
            Drawable::Lines { positions } => positions,
            Drawable::Points { positions } => positions,
        }
    }
}

/// An indexed triangle mesh (the working representation used by the samples).
#[derive(Clone, Debug, Default)]
pub struct MeshData {
    pub positions: Vec<f64>,
    pub indices: Vec<u32>,
}

impl MeshData {
    pub fn into_drawable(self) -> Drawable {
        let normals = super::mesh_builder::compute_vertex_normals(&self.positions, &self.indices);
        Drawable::Mesh { positions: self.positions, indices: self.indices, normals }
    }
}
