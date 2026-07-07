// C-ABI surface for the browser demo (web/): hand-rolled exports instead of
// wasm-bindgen, so the only toolchain requirement is the wasm32 target.
//
// Protocol (all functions are wasm exports; memory is the default export):
//   sample_count()                 -> number of registered samples
//   sample_ids_ptr/len()           -> comma-separated sample ids (utf-8 bytes)
//   build_sample(i)                -> builds sample i, returns drawable count
//   drawable_kind(d)               -> 0 = mesh, 1 = lines, 2 = points
//   drawable_positions_ptr/len(d)  -> flat f64 xyz triples
//   drawable_normals_ptr/len(d)    -> flat f64 xyz triples (mesh only, else 0)
//   drawable_indices_ptr/len(d)    -> u32 triangle indices (mesh only, else 0)
//
// The pointers stay valid until the next build_sample call. The module is
// compiled for native targets too (it is plain Rust), so `cargo test` keeps
// covering it.

use std::cell::RefCell;

use crate::core::types::Drawable;
use crate::samples::samples;

thread_local! {
    static DRAWABLES: RefCell<Vec<Drawable>> = const { RefCell::new(Vec::new()) };
    static IDS: RefCell<String> = const { RefCell::new(String::new()) };
}

fn ids() -> String {
    samples().iter().map(|(id, _)| *id).collect::<Vec<_>>().join(",")
}

#[no_mangle]
pub extern "C" fn sample_count() -> u32 {
    samples().len() as u32
}

#[no_mangle]
pub extern "C" fn sample_ids_ptr() -> *const u8 {
    IDS.with(|cell| {
        let mut s = cell.borrow_mut();
        if s.is_empty() {
            *s = ids();
        }
        s.as_ptr()
    })
}

#[no_mangle]
pub extern "C" fn sample_ids_len() -> u32 {
    ids().len() as u32
}

#[no_mangle]
pub extern "C" fn build_sample(i: u32) -> u32 {
    let all = samples();
    let Some((_, build)) = all.get(i as usize) else {
        return 0;
    };
    let drawables = build();
    let count = drawables.len() as u32;
    DRAWABLES.with(|cell| *cell.borrow_mut() = drawables);
    count
}

fn with_drawable<R>(d: u32, f: impl FnOnce(&Drawable) -> R, default: R) -> R {
    DRAWABLES.with(|cell| {
        let drawables = cell.borrow();
        drawables.get(d as usize).map_or(default, f)
    })
}

#[no_mangle]
pub extern "C" fn drawable_kind(d: u32) -> u32 {
    with_drawable(
        d,
        |dr| match dr {
            Drawable::Mesh { .. } => 0,
            Drawable::Lines { .. } => 1,
            Drawable::Points { .. } => 2,
        },
        u32::MAX,
    )
}

#[no_mangle]
pub extern "C" fn drawable_positions_ptr(d: u32) -> *const f64 {
    with_drawable(d, |dr| dr.positions().as_ptr(), std::ptr::null())
}

#[no_mangle]
pub extern "C" fn drawable_positions_len(d: u32) -> u32 {
    with_drawable(d, |dr| dr.positions().len() as u32, 0)
}

#[no_mangle]
pub extern "C" fn drawable_normals_ptr(d: u32) -> *const f64 {
    with_drawable(
        d,
        |dr| match dr {
            Drawable::Mesh { normals, .. } => normals.as_ptr(),
            _ => std::ptr::null(),
        },
        std::ptr::null(),
    )
}

#[no_mangle]
pub extern "C" fn drawable_normals_len(d: u32) -> u32 {
    with_drawable(
        d,
        |dr| match dr {
            Drawable::Mesh { normals, .. } => normals.len() as u32,
            _ => 0,
        },
        0,
    )
}

#[no_mangle]
pub extern "C" fn drawable_indices_ptr(d: u32) -> *const u32 {
    with_drawable(
        d,
        |dr| match dr {
            Drawable::Mesh { indices, .. } => indices.as_ptr(),
            _ => std::ptr::null(),
        },
        std::ptr::null(),
    )
}

#[no_mangle]
pub extern "C" fn drawable_indices_len(d: u32) -> u32 {
    with_drawable(
        d,
        |dr| match dr {
            Drawable::Mesh { indices, .. } => indices.len() as u32,
            _ => 0,
        },
        0,
    )
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn wasm_api_round_trip() {
        assert_eq!(sample_count(), 12);
        assert_eq!(sample_ids_len() as usize, ids().len());
        let n = build_sample(1); // icosphere: four meshes
        assert_eq!(n, 4);
        assert_eq!(drawable_kind(0), 0);
        assert!(drawable_positions_len(0) > 0);
        assert!(drawable_indices_len(0) > 0);
        assert_eq!(drawable_normals_len(0), drawable_positions_len(0));
        assert_eq!(drawable_kind(99), u32::MAX);
    }
}
