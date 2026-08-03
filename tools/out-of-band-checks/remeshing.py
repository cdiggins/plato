"""Out-of-band check of the index arithmetic in stdlib/geometry/remeshing.library.plato.

Mirrors the Plato bodies literally (same formulas, same scan order) and runs them
over small closed and open meshes. Nothing here is a port for use; it exists only
to confirm the arithmetic, since no gate executes geometry bodies yet (plato-308).
"""

import math

# ---------------------------------------------------------------- corners

def corner_face(c): return c // 3
def next_corner(c): return c - c % 3 + (c + 1) % 3
def prev_corner(c): return next_corner(next_corner(c))

def corner_vertex(faces, c): return faces[c // 3][c % 3]
def corner_dest(faces, c): return corner_vertex(faces, next_corner(c))

def twin_corner(faces, c):
    n = len(faces) * 3
    frm, to = corner_vertex(faces, c), corner_dest(faces, c)
    found = -1
    for o in range(n):
        if corner_vertex(faces, o) == to and corner_dest(faces, o) == frm:
            found = o
    return found

def names_its_edge(twins, c): return twins[c] < 0 or c < twins[c]

def topology_of(positions, faces):
    n = len(faces) * 3
    twins = [twin_corner(faces, c) for c in range(n)]
    naming = [names_its_edge(twins, c) for c in range(n)]
    rank = [sum(1 for d in range(c) if naming[d]) for c in range(n)]
    corner_edges = [rank[c] if naming[c] else rank[twins[c]] for c in range(n)]
    edge_corners = [c for c in range(n) if naming[c]]
    def canon(a, b): return (a, b) if a <= b else (b, a)
    edges = [canon(corner_vertex(faces, c), corner_dest(faces, c)) for c in edge_corners]
    return dict(corner_edges=corner_edges, twins=twins, edges=edges, edge_corners=edge_corners)

def is_boundary_edge(t, e): return t['twins'][t['edge_corners'][e]] < 0
def edge_apex(faces, t, e): return corner_vertex(faces, prev_corner(t['edge_corners'][e]))
def edge_far_apex(faces, t, e):
    tw = t['twins'][t['edge_corners'][e]]
    return -1 if tw < 0 else corner_vertex(faces, prev_corner(tw))

# ---------------------------------------------------------------- remap

def compact_merge(reps, placement):
    n = len(reps)
    survives = [reps[i] == i for i in range(n)]
    rank = [sum(1 for d in range(i) if survives[d]) for i in range(n)]
    targets = [rank[reps[i]] for i in range(n)]
    kept = [placement(i) for i in range(n) if survives[i]]
    return targets, kept

def resolved(reps):
    for _ in range(32):
        reps = [reps[v] for v in reps]
    return reps

def apply_remap(faces, remap):
    targets, kept = remap
    out = []
    for f in faces:
        g = (targets[f[0]], targets[f[1]], targets[f[2]])
        if -1 in g or g[0] == g[1] or g[1] == g[2] or g[2] == g[0]:
            continue
        out.append(g)
    return kept, out

def weld(positions, faces, tol):
    n = len(positions)
    raw = []
    for i in range(n):
        found = i
        for j in range(i, -1, -1):          # descending scan; last match kept
            if dist(positions[j], positions[i]) <= tol:
                found = j
        raw.append(found)
    reps = resolved(raw)
    return apply_remap(faces, compact_merge(reps, lambda i: positions[i]))

def dist(a, b): return math.dist(a, b)

# ---------------------------------------------------------------- split

def split_vertex_numbers(mask, base):
    return [base + sum(1 for d in range(e) if mask[d]) if mask[e] else -1
            for e in range(len(mask))]

def split_face_triangles(v0, v1, v2, m0, m1, m2):
    s0, s1, s2 = m0 >= 0, m1 >= 0, m2 >= 0
    if s0 and s1 and s2:
        return [(v0, m0, m2), (m0, v1, m1), (m2, m1, v2), (m0, m1, m2)]
    if s0 and s1:
        return [(m0, v1, m1), (v0, m0, m1), (v0, m1, v2)]
    if s1 and s2:
        return [(m1, v2, m2), (v0, v1, m1), (v0, m1, m2)]
    if s0 and s2:
        return [(v0, m0, m2), (m0, v1, v2), (m0, v2, m2)]
    if s0: return [(v0, m0, v2), (m0, v1, v2)]
    if s1: return [(v0, v1, m1), (v0, m1, v2)]
    if s2: return [(v0, v1, m2), (m2, v1, v2)]
    return [(v0, v1, v2)]

def split_edges(positions, faces, t, mask):
    ids = split_vertex_numbers(mask, len(positions))
    mids = [midpoint(positions, t['edges'][e]) for e in range(len(mask)) if mask[e]]
    out = []
    for f in range(len(faces)):
        out += split_face_triangles(faces[f][0], faces[f][1], faces[f][2],
                                    ids[t['corner_edges'][f * 3]],
                                    ids[t['corner_edges'][f * 3 + 1]],
                                    ids[t['corner_edges'][f * 3 + 2]])
    return positions + mids, out

def midpoint(positions, pair):
    a, b = positions[pair[0]], positions[pair[1]]
    return tuple((x + y) / 2 for x, y in zip(a, b))

# ---------------------------------------------------------------- collapse

def collapse_claims(edges, nbrs, mask, vcount):
    state = [-1] * vcount
    def free(v): return state[v] == -1 and all(state[x] == -1 for x in nbrs[v])
    for e in range(len(edges)):
        a, b = edges[e]
        if mask[e] and free(a) and free(b):
            ring = set(nbrs[a]) | set(nbrs[b])
            state = [a if (v == a or v == b) else (-2 if v in ring else state[v])
                     for v in range(vcount)]
    return state

def neighbor_table(positions, t):
    rows = []
    for v in range(len(positions)):
        row = []
        for (a, b) in t['edges']:
            if a == v: row.append(b)
            elif b == v: row.append(a)
        rows.append(row)
    return rows

def link_condition(t, nbrs, e):
    a, b = t['edges'][e]
    shared = sum(1 for x in nbrs[a] if x in nbrs[b])
    return shared == (1 if is_boundary_edge(t, e) else 2)

def collapse_edges(positions, faces, t, mask, placement):
    edges = t['edges']
    nbrs = neighbor_table(positions, t)
    mask = [mask[e] and link_condition(t, nbrs, e) for e in range(len(edges))]
    state = collapse_claims(edges, nbrs, mask, len(positions))
    moved = list(positions)
    for e in range(len(edges)):
        a, b = edges[e]
        if a != b and state[a] == a and state[b] == a:
            moved = [placement(e) if v == a else moved[v] for v in range(len(moved))]
    reps = [v if state[v] < 0 else state[v] for v in range(len(positions))]
    return apply_remap(faces, compact_merge(reps, lambda i: moved[i]))

# ---------------------------------------------------------------- flip

def are_adjacent(edges, a, b):
    return any((p[0] == a and p[1] == b) or (p[0] == b and p[1] == a) for p in edges)

def is_flip_candidate(faces, t, e):
    near, far = edge_apex(faces, t, e), edge_far_apex(faces, t, e)
    return far >= 0 and not are_adjacent(t['edges'], near, far)

def flip_vertex_claims(positions, faces, t, mask):
    claims = [-1] * len(positions)
    for e in range(len(t['edges'])):
        a, b = t['edges'][e]
        near, far = edge_apex(faces, t, e), edge_far_apex(faces, t, e)
        at = lambda v: -1 if v < 0 else claims[v]
        if (mask[e] and is_flip_candidate(faces, t, e)
                and at(a) < 0 and at(b) < 0 and at(near) < 0 and at(far) < 0):
            claims = [e if v in (a, b, near, far) else claims[v] for v in range(len(positions))]
    return claims

def is_accepted_flip(faces, t, claims, e):
    a, b = t['edges'][e]
    near, far = edge_apex(faces, t, e), edge_far_apex(faces, t, e)
    return (far >= 0 and claims[a] == e and claims[b] == e
            and claims[near] == e and claims[far] == e)

def flip_edges(positions, faces, t, mask):
    claims = flip_vertex_claims(positions, faces, t, mask)
    out = []
    for f in range(len(faces)):
        found = -1
        for e in range(len(t['edges'])):
            c = t['edge_corners'][e]
            tw = t['twins'][c]
            if is_accepted_flip(faces, t, claims, e) and (c // 3 == f or tw // 3 == f):
                found = e
        if found < 0:
            out.append(faces[f]); continue
        c = t['edge_corners'][found]
        tw = t['twins'][c]
        u = corner_vertex(faces, c)
        v = corner_vertex(faces, tw)
        w = corner_vertex(faces, prev_corner(c))
        x = corner_vertex(faces, prev_corner(tw))
        out.append((w, u, x) if c // 3 == f else (w, x, v))
    return positions, out

# ---------------------------------------------------------------- meshes

def tetrahedron():
    p = [(1, 1, 1), (1, -1, -1), (-1, 1, -1), (-1, -1, 1)]
    f = [(0, 1, 2), (0, 3, 1), (0, 2, 3), (1, 3, 2)]
    return p, f

def octahedron():
    p = [(1, 0, 0), (-1, 0, 0), (0, 1, 0), (0, -1, 0), (0, 0, 1), (0, 0, -1)]
    f = [(0, 2, 4), (2, 1, 4), (1, 3, 4), (3, 0, 4),
         (2, 0, 5), (1, 2, 5), (3, 1, 5), (0, 3, 5)]
    return p, f

def grid(n):
    """An open n x n quad grid split into triangles: a disk with a boundary."""
    p = [(i, j, 0) for j in range(n + 1) for i in range(n + 1)]
    f = []
    for j in range(n):
        for i in range(n):
            a = j * (n + 1) + i; b = a + 1; c = b + n + 1; d = a + n + 1
            f += [(a, b, c), (a, c, d)]
    return p, f

# ---------------------------------------------------------------- checks

def euler(positions, faces, t): return len(positions) - len(t['edges']) + len(faces)

def outward(positions, faces):
    """Every face normal points away from the centroid (valid for star-shaped closed input)."""
    cx = tuple(sum(p[k] for p in positions) / len(positions) for k in range(3))
    for (a, b, c) in faces:
        A, B, C = positions[a], positions[b], positions[c]
        u = [B[k] - A[k] for k in range(3)]
        v = [C[k] - A[k] for k in range(3)]
        nrm = [u[1]*v[2]-u[2]*v[1], u[2]*v[0]-u[0]*v[2], u[0]*v[1]-u[1]*v[0]]
        d = [(A[k]+B[k]+C[k])/3 - cx[k] for k in range(3)]
        if sum(nrm[k]*d[k] for k in range(3)) <= 0:
            return False
    return True

def manifold(faces):
    """Every directed edge appears exactly once (closed, consistently oriented)."""
    seen = {}
    for f in faces:
        for k in range(3):
            e = (f[k], f[(k + 1) % 3])
            seen[e] = seen.get(e, 0) + 1
    if any(v != 1 for v in seen.values()):
        return False
    return all((b, a) in seen for (a, b) in seen)

fails = []
def ok(name, cond):
    print(("  PASS " if cond else "  FAIL ") + name)
    if not cond: fails.append(name)

for name, (p, f) in [("tetrahedron", tetrahedron()), ("octahedron", octahedron())]:
    print(name)
    t = topology_of(p, f)
    ok("euler = 2", euler(p, f, t) == 2)
    ok("edge count = 3F/2", len(t['edges']) == len(f) * 3 // 2)
    ok("no boundary edges", not any(is_boundary_edge(t, e) for e in range(len(t['edges']))))
    ok("every corner has a distinct edge number in range",
       sorted(set(t['corner_edges'])) == list(range(len(t['edges']))))
    ok("twin pairs agree on edge number",
       all(t['corner_edges'][c] == t['corner_edges'][t['twins'][c]] for c in range(len(f) * 3)))
    ok("apexes differ from endpoints", all(
        edge_apex(f, t, e) not in t['edges'][e] and edge_far_apex(f, t, e) not in t['edges'][e]
        for e in range(len(t['edges']))))

    # uniform 1-to-4 split
    p2, f2 = split_edges(p, f, t, [True] * len(t['edges']))
    t2 = topology_of(p2, f2)
    ok("split: 4x faces", len(f2) == 4 * len(f))
    ok("split: V + E vertices", len(p2) == len(p) + len(t['edges']))
    ok("split: euler still 2", euler(p2, f2, t2) == 2)
    ok("split: still closed and oriented", manifold(f2))
    ok("split: still wound outward", outward(p2, f2))

    # partial splits: every one of the eight masks on face 0's edges
    for bits in range(8):
        mask = [False] * len(t['edges'])
        for k in range(3):
            if bits & (1 << k):
                mask[t['corner_edges'][k]] = True
        pp, ff = split_edges(p, f, t, mask)
        ok(f"split mask {bits:03b}: closed and oriented", manifold(ff))

    # flip every interior edge, one independent set at a time
    pf, ff = flip_edges(p, f, t, [True] * len(t['edges']))
    tf = topology_of(pf, ff)
    ok("flip: face count unchanged", len(ff) == len(f))
    ok("flip: positions unchanged", pf == p)
    ok("flip: still closed and oriented", manifold(ff))
    ok("flip: euler still 2", euler(pf, ff, tf) == 2)
    # On a tetrahedron every pair of vertices is already joined, so no flip is
    # legal and the mesh must come back untouched. On anything larger some flip
    # must land.
    if name == "tetrahedron":
        ok("flip: a tetrahedron admits no legal flip", ff == f)
    else:
        ok("flip: something actually flipped", set(map(frozenset, ff)) != set(map(frozenset, f)))

    # collapse every edge to its midpoint
    pc, fc = collapse_edges(p, f, t, [True] * len(t['edges']),
                            lambda e: midpoint(p, t['edges'][e]))
    ok("collapse: vertex count fell", len(pc) < len(p))
    ok("collapse: faces fell", len(fc) < len(f))
    ok("collapse: no degenerate face left",
       all(len(set(x)) == 3 for x in fc))

print("open grid (boundary handling)")
p, f = grid(3)
t = topology_of(p, f)
ok("euler = 1 (disk)", euler(p, f, t) == 1)
nb = sum(1 for e in range(len(t['edges'])) if is_boundary_edge(t, e))
ok("boundary edge count = 12", nb == 12)
p2, f2 = split_edges(p, f, t, [True] * len(t['edges']))
t2 = topology_of(p2, f2)
ok("split: 4x faces", len(f2) == 4 * len(f))
ok("split: euler still 1", euler(p2, f2, t2) == 1)
ok("split: boundary edges doubled", sum(1 for e in range(len(t2['edges']))
                                        if is_boundary_edge(t2, e)) == 24)

print("welding unwelded soup")
p, f = octahedron()
soup_p, soup_f = [], []
for (a, b, c) in f:
    base = len(soup_p)
    soup_p += [p[a], p[b], p[c]]
    soup_f.append((base, base + 1, base + 2))
wp, wf = weld(soup_p, soup_f, 1e-9)
ok("weld: 24 corners collapse to 6 vertices", len(wp) == 6)
ok("weld: 8 faces survive", len(wf) == 8)
ok("weld: closed and oriented", manifold(wf))
tw = topology_of(wp, wf)
ok("weld: euler = 2", euler(wp, wf, tw) == 2)

# a soup with two triangles sharing a nearly-coincident chain a-b-c
chain = [(0, 0, 0), (0.4, 0, 0), (0.8, 0, 0), (5, 0, 0), (0, 5, 0)]
cf = [(0, 3, 4), (2, 3, 4)]
cp, cff = weld(chain, cf, 0.5)
ok("weld: chained tolerance groups resolve to one representative", len(cp) == 3)

print("quadric minimizer")
def plane_quadric(n, d):
    v = (n[0], n[1], n[2], d)
    return [[v[i] * v[j] for j in range(4)] for i in range(4)]
def qadd(a, b): return [[a[i][j] + b[i][j] for j in range(4)] for i in range(4)]
def qerr(q, p):
    v = (p[0], p[1], p[2], 1.0)
    return sum(v[i] * q[i][j] * v[j] for i in range(4) for j in range(4))
# three orthogonal planes through (1,2,3): the minimizer must be that point
q = plane_quadric((1, 0, 0), -1)
q = qadd(q, plane_quadric((0, 1, 0), -2))
q = qadd(q, plane_quadric((0, 0, 1), -3))
# solve as the Plato body does: replace the last COLUMN by (0,0,0,1), invert, read row 4
import itertools
def invert4(m):
    n = [row[:] + [1.0 if i == j else 0.0 for j in range(4)] for i, row in enumerate(m)]
    for col in range(4):
        piv = max(range(col, 4), key=lambda r: abs(n[r][col]))
        if abs(n[piv][col]) < 1e-12: return None
        n[col], n[piv] = n[piv], n[col]
        d = n[col][col]
        n[col] = [x / d for x in n[col]]
        for r in range(4):
            if r != col and n[r][col] != 0:
                fct = n[r][col]
                n[r] = [a - fct * b for a, b in zip(n[r], n[col])]
    return [row[4:] for row in n]
solve = [[q[0][0], q[0][1], q[0][2], 0.0],
         [q[1][0], q[1][1], q[1][2], 0.0],
         [q[2][0], q[2][1], q[2][2], 0.0],
         [q[3][0], q[3][1], q[3][2], 1.0]]
inv = invert4(solve)
pt = tuple(inv[3][:3])
ok("minimizer of three orthogonal planes is their common point",
   all(abs(pt[k] - (1, 2, 3)[k]) < 1e-9 for k in range(3)))
ok("error there is zero", abs(qerr(q, pt)) < 1e-9)
ok("error grows away from it", qerr(q, (1, 2, 4)) > 0.9)

print()
print("FAILURES: " + (", ".join(fails) if fails else "none"))


# ------------------------------------------------- full remeshing pipeline

print()
print("pipeline on a twice-subdivided octahedron")
p, f = octahedron()
for _ in range(2):
    t = topology_of(p, f)
    p, f = split_edges(p, f, t, [True] * len(t['edges']))
ok("subdivided twice: 128 faces", len(f) == 128)
t = topology_of(p, f)
ok("subdivided: closed and oriented", manifold(f))
ok("subdivided: euler 2", euler(p, f, t) == 2)

def edge_len(p, pair): return dist(p[pair[0]], p[pair[1]])
def avg_edge(p, t): return sum(edge_len(p, e) for e in t['edges']) / len(t['edges'])

def boundary_flags(p, f, t):
    return [any(is_boundary_edge(t, e) and v in t['edges'][e] for e in range(len(t['edges'])))
            for v in range(len(p))]

L = avg_edge(p, t)
for it in range(4):
    t = topology_of(p, f)
    p, f = split_edges(p, f, t, [edge_len(p, t['edges'][e]) > L * 4 / 3
                                 for e in range(len(t['edges']))])
    t = topology_of(p, f)
    b = boundary_flags(p, f, t)
    p, f = collapse_edges(p, f, t,
        [edge_len(p, t['edges'][e]) < L * 4 / 5 and b[t['edges'][e][0]] == b[t['edges'][e][1]]
         for e in range(len(t['edges']))],
        lambda e: midpoint(p, t['edges'][e]))
    t = topology_of(p, f)
    val = [sum(1 for e in t['edges'] if v in e) for v in range(len(p))]
    def improves(e):
        a, bb = t['edges'][e]
        w, x = edge_apex(f, t, e), edge_far_apex(f, t, e)
        if x < 0: return False
        dev = lambda n: abs(n - 6)
        before = dev(val[a]) + dev(val[bb]) + dev(val[w]) + dev(val[x])
        after = dev(val[a]-1) + dev(val[bb]-1) + dev(val[w]+1) + dev(val[x]+1)
        return after < before
    p, f = flip_edges(p, f, t, [improves(e) for e in range(len(t['edges']))])
    t = topology_of(p, f)
    ok(f"isotropic pass {it}: closed and oriented", manifold(f))
    ok(f"isotropic pass {it}: euler 2", euler(p, f, t) == 2)

t = topology_of(p, f)
spread_before = max(edge_len(p, e) for e in t['edges']) / min(edge_len(p, e) for e in t['edges'])
print(f"  (edge-length spread after 4 passes: {spread_before:.2f}, faces {len(f)})")

# ------------------------------------------------- decimation ranking

print("decimation ranking")
costs = [3.0, 1.0, 2.0, 1.0, 5.0]
ranks = [sum(1 for j in range(len(costs))
             if costs[j] < costs[i] or (costs[j] == costs[i] and j < i))
         for i in range(len(costs))]
ok("cost ranks are a permutation", sorted(ranks) == [0, 1, 2, 3, 4])
ok("cheapest two are ranks 0 and 1", {i for i, r in enumerate(ranks) if r < 2} == {1, 3})

# ------------------------------------------------- Catmull-Clark on a cube

print("Catmull-Clark index arithmetic on a cube")
cube_p = [(0,0,0),(1,0,0),(1,1,0),(0,1,0),(0,0,1),(1,0,1),(1,1,1),(0,1,1)]
cube_f = [[0,3,2,1],[4,5,6,7],[0,1,5,4],[1,2,6,5],[2,3,7,6],[3,0,4,7]]
offsets, values = [0], []
for face in cube_f:
    values += face
    offsets.append(len(values))
def arity(fi): return offsets[fi+1] - offsets[fi]
def face_corner(fi, slot): return offsets[fi] + slot % arity(fi)
corner_faces = [fi for fi in range(len(cube_f)) for _ in range(arity(fi))]
corner_dests = [values[offsets[fi] + (s + 1) % arity(fi)]
                for fi in range(len(cube_f)) for s in range(arity(fi))]
n = len(values)
poly_twins = []
for c in range(n):
    found = -1
    for d in range(n):
        if values[d] == corner_dests[c] and corner_dests[d] == values[c]:
            found = d
    poly_twins.append(found)
poly_naming = [poly_twins[c] < 0 or c < poly_twins[c] for c in range(n)]
poly_rank = [sum(1 for d in range(c) if poly_naming[d]) for c in range(n)]
poly_edges = [poly_rank[c] if poly_naming[c] else poly_rank[poly_twins[c]] for c in range(n)]
edge_count = sum(1 for c in range(n) if poly_naming[c])
ok("cube has 24 corners", n == 24)
ok("cube has 12 edges", edge_count == 12)
ok("every corner has a twin", all(x >= 0 for x in poly_twins))
ok("twins agree on edge number", all(poly_edges[c] == poly_edges[poly_twins[c]] for c in range(n)))
vcount = len(cube_p)
new_faces = []
for fi in range(len(cube_f)):
    for s in range(arity(fi)):
        here, back = face_corner(fi, s), face_corner(fi, s + arity(fi) - 1)
        new_faces.append([values[here],
                          vcount + poly_edges[here],
                          vcount + edge_count + fi,
                          vcount + poly_edges[back]])
ok("Catmull-Clark emits one quad per corner", len(new_faces) == 24)
ok("every new face is a quad of four distinct vertices",
   all(len(set(q)) == 4 for q in new_faces))
ok("new vertex numbering covers V + E + F",
   max(max(q) for q in new_faces) == vcount + edge_count + len(cube_f) - 1)
# every directed edge of the new mesh appears exactly once => closed, oriented
seen = {}
for q in new_faces:
    for k in range(4):
        seen[(q[k], q[(k+1) % 4])] = seen.get((q[k], q[(k+1) % 4]), 0) + 1
ok("Catmull-Clark result is closed and consistently oriented",
   all(v == 1 for v in seen.values()) and all((b, a) in seen for (a, b) in seen))

# ------------------------------------------------- Doo-Sabin on a cube

print("Doo-Sabin index arithmetic on a cube")
poly_next = [face_corner(fi, s + 1) for fi in range(len(cube_f)) for s in range(arity(fi))]
ds_face_faces = [[face_corner(fi, s) for s in range(arity(fi))] for fi in range(len(cube_f))]
ds_edge_faces = [[poly_next[poly_twins[c]], poly_twins[c], poly_next[c], c]
                 for c in range(n) if poly_twins[c] >= 0 and c < poly_twins[c]]
# vertex faces: reversed corner ring, walked as meshes-polygon does
def corner_face_of(c): return corner_faces[c]
def rotate(c): return poly_next[poly_twins[c]]
def ring(v):
    first = min(c for c in range(n) if values[c] == v)
    deg = sum(1 for c in range(n) if values[c] == v)
    r = [first]
    for _ in range(deg - 1):
        r.append(rotate(r[-1]))
    return r
ds_vertex_faces = [list(reversed(ring(v))) for v in range(vcount)]
ds = ds_face_faces + ds_edge_faces + ds_vertex_faces
ok("Doo-Sabin emits F + E + V faces", len(ds) == len(cube_f) + edge_count + vcount)
ok("every Doo-Sabin face has distinct corners", all(len(set(x)) == len(x) for x in ds))
seen = {}
for q in ds:
    for k in range(len(q)):
        e = (q[k], q[(k+1) % len(q)])
        seen[e] = seen.get(e, 0) + 1
ok("Doo-Sabin result is closed and consistently oriented",
   all(v == 1 for v in seen.values()) and all((b, a) in seen for (a, b) in seen))

print()
print("FAILURES: " + (", ".join(fails) if fails else "none"))


print()
print("isotropic remeshing away from the starting density")

def isotropic(p, f, L, iters):
    for _ in range(iters):
        t = topology_of(p, f)
        p, f = split_edges(p, f, t, [edge_len(p, t['edges'][e]) > L * 4 / 3
                                     for e in range(len(t['edges']))])
        t = topology_of(p, f)
        b = boundary_flags(p, f, t)
        p, f = collapse_edges(p, f, t,
            [edge_len(p, t['edges'][e]) < L * 4 / 5 and b[t['edges'][e][0]] == b[t['edges'][e][1]]
             for e in range(len(t['edges']))],
            (lambda pp, tt: (lambda e: midpoint(pp, tt['edges'][e])))(p, t))
        t = topology_of(p, f)
        val = [sum(1 for e in t['edges'] if v in e) for v in range(len(p))]
        def improves(e):
            a, bb = t['edges'][e]
            w, x = edge_apex(f, t, e), edge_far_apex(f, t, e)
            if x < 0: return False
            dev = lambda k: abs(k - 6)
            return (dev(val[a]-1) + dev(val[bb]-1) + dev(val[w]+1) + dev(val[x]+1)
                    < dev(val[a]) + dev(val[bb]) + dev(val[w]) + dev(val[x]))
        p, f = flip_edges(p, f, t, [improves(e) for e in range(len(t['edges']))])
        t = topology_of(p, f)
        if not manifold(f) or euler(p, f, t) != 2:
            return p, f, False
    return p, f, True

base_p, base_f = octahedron()
for _ in range(2):
    tt = topology_of(base_p, base_f)
    base_p, base_f = split_edges(base_p, base_f, tt, [True] * len(tt['edges']))
L0 = avg_edge(base_p, topology_of(base_p, base_f))

p, f, good = isotropic(base_p, base_f, L0 * 0.5, 5)
ok("refining to half the edge length stays a closed oriented manifold", good)
ok("refining grew the face count", len(f) > len(base_f))
print(f"  (refine: {len(base_f)} -> {len(f)} faces)")

p, f, good = isotropic(base_p, base_f, L0 * 2.0, 12)
ok("coarsening to twice the edge length stays a closed oriented manifold", good)
ok("coarsening shrank the face count", len(f) < len(base_f))
print(f"  (coarsen: {len(base_f)} -> {len(f)} faces)")

print()
print("FAILURES: " + (", ".join(fails) if fails else "none"))


print()
print("quadric decimation end to end")

def tri_quadric(A, B, C):
    u = [B[k]-A[k] for k in range(3)]
    v = [C[k]-A[k] for k in range(3)]
    nr = [u[1]*v[2]-u[2]*v[1], u[2]*v[0]-u[0]*v[2], u[0]*v[1]-u[1]*v[0]]
    m = math.sqrt(sum(x*x for x in nr))
    if m == 0: return [[0.0]*4 for _ in range(4)]
    n = [x/m for x in nr]
    d = -sum(n[k]*A[k] for k in range(3))
    q = plane_quadric(n, d)
    area = m/2
    return [[q[i][j]*area for j in range(4)] for i in range(4)]

def vertex_quadrics(p, f):
    qs = [[[0.0]*4 for _ in range(4)] for _ in p]
    for (a,b,c) in f:
        q = tri_quadric(p[a], p[b], p[c])
        for v in (a,b,c): qs[v] = qadd(qs[v], q)
    return qs

def minimizer(q, fallback):
    solve = [[q[0][0], q[0][1], q[0][2], 0.0],
             [q[1][0], q[1][1], q[1][2], 0.0],
             [q[2][0], q[2][1], q[2][2], 0.0],
             [q[3][0], q[3][1], q[3][2], 1.0]]
    inv = invert4(solve)
    return fallback if inv is None else tuple(inv[3][:3])

def decimate(p, f, target, passes):
    for _ in range(passes):
        t = topology_of(p, f)
        qs = vertex_quadrics(p, f)
        b = boundary_flags(p, f, t)
        tgt, cost = [], []
        for e in range(len(t['edges'])):
            a, bb = t['edges'][e]
            q = qadd(qs[a], qs[bb])
            pt = minimizer(q, midpoint(p, t['edges'][e]))
            tgt.append(pt); cost.append(qerr(q, pt))
        budget = max((len(f) - target)//2, 0)
        rank = [sum(1 for j in range(len(cost))
                    if cost[j] < cost[i] or (cost[j] == cost[i] and j < i))
                for i in range(len(cost))]
        mask = [rank[e] < budget and b[t['edges'][e][0]] == b[t['edges'][e][1]]
                for e in range(len(t['edges']))]
        p, f = collapse_edges(p, f, t, mask, (lambda tt: (lambda e: tt[e]))(tgt))
        tt = topology_of(p, f)
        if not manifold(f) or euler(p, f, tt) != 2:
            return p, f, False
    return p, f, True

p, f = octahedron()
for _ in range(2):
    tt = topology_of(p, f)
    p, f = split_edges(p, f, tt, [True] * len(tt['edges']))
before = len(f)
p2, f2, good = decimate(p, f, 32, 20)
ok("decimation keeps a closed oriented manifold", good)
ok("decimation approaches the target from above", 32 <= len(f2) < before)
print(f"  (decimate: {before} -> {len(f2)} faces, target 32)")
# the decimated sphere should still be roughly a unit sphere: quadric decimation
# of a smooth convex surface must not move vertices far off it
radii = [math.dist((0,0,0), q) for q in p2]
ok("decimated vertices stay in the shell the input occupied",
   max(radii) < 1.6 and min(radii) > 0.3)

print()
print("FAILURES: " + (", ".join(fails) if fails else "none"))
