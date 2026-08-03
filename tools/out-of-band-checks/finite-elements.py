"""Numeric transcription of the Plato bodies planned for stdlib/future/finite-elements.*
Checks them against closed-form solutions. Same formulas, same order of operations.
"""
import math

# ---------------- shared: COO assembly + projected Jacobi PCG ----------------

def matvec(n, entries, x):
    y = [0.0] * n
    for (r, c, v) in entries:
        y[r] += v * x[c]
    return y

def dot(a, b): return sum(x * y for x, y in zip(a, b))
def addscaled(a, b, t): return [x + y * t for x, y in zip(a, b)]
def norm(a): return math.sqrt(dot(a, a))
def project(a, f): return [x * y for x, y in zip(a, f)]
def prod(a, b): return [x * y for x, y in zip(a, b)]

def jacobi(n, entries):
    d = [0.0] * n
    for (r, c, v) in entries:
        if r == c:
            d[r] += v
    return [1.0 if x == 0.0 else 1.0 / x for x in d]

def solve(n, entries, loads, constraints, maxit=20000, tol=1e-12):
    free = [1.0] * n
    u = [0.0] * n
    for (d, val) in constraints:
        free[d] = 0.0
        u[d] = val
    M = jacobi(n, entries)
    r = project([f - k for f, k in zip(loads, matvec(n, entries, u))], free)
    r0 = norm(r)
    z = project(prod(M, r), free)
    p = z[:]
    rz = dot(r, z)
    it = 0
    residual = r0
    running = r0 > 0.0
    while running and it < maxit:
        q = project(matvec(n, entries, p), free)
        pq = dot(p, q)
        if pq <= 0.0:
            running = False
        else:
            alpha = rz / pq
            u = addscaled(u, p, alpha)
            r = addscaled(r, q, -alpha)
            residual = norm(r)
            it += 1
            if residual <= tol * r0:
                running = False
            else:
                z = project(prod(M, r), free)
                rzn = dot(r, z)
                p = addscaled(z, p, rzn / rz)
                rz = rzn
    return u, it, (residual / r0 if r0 > 0 else 0.0), (r0 == 0.0 or residual <= tol * r0)

# ---------------- material ----------------

def lame3d(E, nu):
    mu = E / (2.0 * (1.0 + nu))
    lam = E * nu / ((1.0 + nu) * (1.0 - 2.0 * nu))
    return lam, mu

def lame_plane(E, nu, condition):
    mu = E / (2.0 * (1.0 + nu))
    if condition == "stress":
        return E * nu / (1.0 - nu * nu), mu
    return E * nu / ((1.0 + nu) * (1.0 - 2.0 * nu)), mu

# ---------------- tetrahedron ----------------

def sub(a, b): return (b[0]-a[0], b[1]-a[1], b[2]-a[2])
def cross(a, b): return (a[1]*b[2]-a[2]*b[1], a[2]*b[0]-a[0]*b[2], a[0]*b[1]-a[1]*b[0])
def dot3(a, b): return a[0]*b[0]+a[1]*b[1]+a[2]*b[2]
def scale3(a, t): return (a[0]*t, a[1]*t, a[2]*t)

def six_volume(a, b, c, d):
    return dot3(cross(sub(a, b), sub(a, c)), sub(a, d))

def tet_gradients(p0, p1, p2, p3):
    """grad of the four barycentric coordinates. Verified against the reference tet."""
    six = six_volume(p0, p1, p2, p3)
    return [
        scale3(cross(sub(p1, p3), sub(p1, p2)), 1.0 / six),
        scale3(cross(sub(p0, p2), sub(p0, p3)), 1.0 / six),
        scale3(cross(sub(p1, p0), sub(p1, p3)), 1.0 / six),
        scale3(cross(sub(p1, p2), sub(p1, p0)), 1.0 / six),
    ]

def comp(v, i): return v[i]

def stiffness_term(gi, gj, r, s, lam, mu, measure):
    return measure * (lam * comp(gi, r) * comp(gj, s)
                      + mu * (comp(gi, s) * comp(gj, r) + (dot3(gi, gj) if r == s else 0.0)))

def tet_entries(positions, cell, lam, mu):
    p = [positions[i] for i in cell]
    g = tet_gradients(*p)
    v = abs(six_volume(*p)) / 6.0
    out = []
    for k in range(144):
        i, r, j, s = k // 36, (k // 12) % 3, (k // 3) % 4, k % 3
        out.append((cell[i]*3+r, cell[j]*3+s, stiffness_term(g[i], g[j], r, s, lam, mu, v)))
    return out

def tet_strain(positions, cell, u, gradients=None):
    p = [positions[i] for i in cell]
    g = gradients or tet_gradients(*p)
    exx = sum(u[cell[i]*3+0] * g[i][0] for i in range(4))
    eyy = sum(u[cell[i]*3+1] * g[i][1] for i in range(4))
    ezz = sum(u[cell[i]*3+2] * g[i][2] for i in range(4))
    exy = 0.5 * sum(u[cell[i]*3+0] * g[i][1] + u[cell[i]*3+1] * g[i][0] for i in range(4))
    eyz = 0.5 * sum(u[cell[i]*3+1] * g[i][2] + u[cell[i]*3+2] * g[i][1] for i in range(4))
    ezx = 0.5 * sum(u[cell[i]*3+2] * g[i][0] + u[cell[i]*3+0] * g[i][2] for i in range(4))
    return (exx, eyy, ezz, exy, eyz, ezx)

def stress_from_strain(e, lam, mu):
    tr = e[0] + e[1] + e[2]
    return (2*mu*e[0] + lam*tr, 2*mu*e[1] + lam*tr, 2*mu*e[2] + lam*tr,
            2*mu*e[3], 2*mu*e[4], 2*mu*e[5])

def von_mises(s):
    xx, yy, zz, xy, yz, zx = s
    return math.sqrt(0.5*((xx-yy)**2 + (yy-zz)**2 + (zz-xx)**2) + 3.0*(xy*xy + yz*yz + zx*zx))

# ---- check 1: unit cube, 6 tets, uniaxial tension along X --------------------

# unit cube corners, index = bit pattern (x, y, z)
CUBE = [(x, y, z) for x in (0.0, 1.0) for y in (0.0, 1.0) for z in (0.0, 1.0)]
def cidx(x, y, z): return x*4 + y*2 + z
# Freudenthal 6-tet split of the unit cube (all positive volume checked below)
CELLS = [
    (cidx(0,0,0), cidx(1,0,0), cidx(1,1,0), cidx(1,1,1)),
    (cidx(0,0,0), cidx(1,1,0), cidx(0,1,0), cidx(1,1,1)),
    (cidx(0,0,0), cidx(0,1,0), cidx(0,1,1), cidx(1,1,1)),
    (cidx(0,0,0), cidx(0,1,1), cidx(0,0,1), cidx(1,1,1)),
    (cidx(0,0,0), cidx(0,0,1), cidx(1,0,1), cidx(1,1,1)),
    (cidx(0,0,0), cidx(1,0,1), cidx(1,0,0), cidx(1,1,1)),
]

E, nu = 200e9, 0.3
lam, mu = lame3d(E, nu)
for c in CELLS:
    assert six_volume(*[CUBE[i] for i in c]) > 0, c

entries = []
for c in CELLS:
    entries += tet_entries(CUBE, c, lam, mu)

n = 8 * 3
# fix x=0 face in X; kill rigid body motion minimally
cons = []
for i, p in enumerate(CUBE):
    if p[0] == 0.0:
        cons.append((i*3+0, 0.0))
cons.append((cidx(0,0,0)*3+1, 0.0))
cons.append((cidx(0,0,0)*3+2, 0.0))
cons.append((cidx(0,1,0)*3+2, 0.0))   # block rotation about X
sigma = 1e6  # 1 MPa uniform traction on x=1 face, area 1
# traction lumped: 2 triangles on x=1 face, each area 0.5, each node gets A/3
face_tris = [(cidx(1,0,0), cidx(1,1,0), cidx(1,1,1)), (cidx(1,0,0), cidx(1,1,1), cidx(1,0,1))]
loads = [0.0]*n
for t in face_tris:
    a, b, c = [CUBE[i] for i in t]
    area = 0.5 * math.sqrt(sum(x*x for x in cross(sub(a,b), sub(a,c))))
    for i in t:
        loads[i*3+0] += sigma * area / 3.0

u, it, res, ok = solve(n, entries, loads, cons)
ux = [u[i*3] for i, p in enumerate(CUBE) if p[0] == 1.0]
print("=== unit cube, 6 tets, uniaxial 1 MPa ===")
print("  converged:", ok, "iterations:", it, "relative residual: %.2e" % res)
print("  ux on x=1 face:", ["%.6e" % v for v in ux], " closed form sigma*L/E = %.6e" % (sigma/E))
vm = [von_mises(stress_from_strain(tet_strain(CUBE, c, u), lam, mu)) for c in CELLS]
print("  von Mises per tet:", ["%.6e" % v for v in vm], " expected %.6e" % sigma)
# transverse contraction: uy at y=1 should be -nu*sigma/E
uy = [u[i*3+1] for i, p in enumerate(CUBE) if p[1] == 1.0]
print("  uy on y=1 face:", ["%.6e" % v for v in uy], " closed form -nu*sigma/E = %.6e" % (-nu*sigma/E))

# ---------------- linear triangle (CST) ----------------

def tri_gradients(p0, p1, p2):
    twice = (p1[0]-p0[0])*(p2[1]-p0[1]) - (p2[0]-p0[0])*(p1[1]-p0[1])
    return [
        ((p1[1]-p2[1]) / twice, (p2[0]-p1[0]) / twice, 0.0),
        ((p2[1]-p0[1]) / twice, (p0[0]-p2[0]) / twice, 0.0),
        ((p0[1]-p1[1]) / twice, (p1[0]-p0[0]) / twice, 0.0),
    ], abs(twice) * 0.5

def tri_entries(positions, face, lam, mu, thickness):
    p = [positions[i] for i in face]
    g, area = tri_gradients(*p)
    measure = area * thickness
    out = []
    for k in range(36):
        i, r, j, s = k // 12, (k // 6) % 2, (k // 2) % 3, k % 2
        out.append((face[i]*2+r, face[j]*2+s, stiffness_term(g[i], g[j], r, s, lam, mu, measure)))
    return out

def tri_strain(positions, face, u):
    p = [positions[i] for i in face]
    g, _ = tri_gradients(*p)
    exx = sum(u[face[i]*2+0] * g[i][0] for i in range(3))
    eyy = sum(u[face[i]*2+1] * g[i][1] for i in range(3))
    exy = 0.5 * sum(u[face[i]*2+0] * g[i][1] + u[face[i]*2+1] * g[i][0] for i in range(3))
    return (exx, eyy, exy)

def plane_stress_state(e, lam, mu, condition):
    sxx = (lam + 2*mu) * e[0] + lam * e[1]
    syy = lam * e[0] + (lam + 2*mu) * e[1]
    sxy = 2 * mu * e[2]
    szz = 0.0 if condition == "stress" else nu * (sxx + syy)
    return (sxx, syy, szz, sxy, 0.0, 0.0)

print()
print("=== unit square, 2 CST triangles, plane stress, uniaxial 1 MPa ===")
SQ = [(0.0,0.0), (1.0,0.0), (1.0,1.0), (0.0,1.0)]
FACES = [(0,1,2), (0,2,3)]
thickness = 0.01
lam2, mu2 = lame_plane(E, nu, "stress")
entries2 = []
for f in FACES:
    entries2 += tri_entries(SQ, f, lam2, mu2, thickness)
n2 = 8
cons2 = [(0*2+0, 0.0), (3*2+0, 0.0), (0*2+1, 0.0)]
loads2 = [0.0]*n2
# traction sigma on x=1 edge (nodes 1 and 2), edge length 1, thickness t
for i in (1, 2):
    loads2[i*2+0] += sigma * 1.0 * thickness / 2.0
u2, it2, res2, ok2 = solve(n2, entries2, loads2, cons2)
print("  converged:", ok2, "iterations:", it2, "relative residual: %.2e" % res2)
print("  ux at nodes 1,2: %.6e %.6e  closed form %.6e" % (u2[2], u2[4], sigma/E))
print("  uy at node 3 (y=1): %.6e  closed form %.6e" % (u2[7], -nu*sigma/E))
vm2 = [von_mises(plane_stress_state(tri_strain(SQ, f, u2), lam2, mu2, "stress")) for f in FACES]
print("  von Mises per triangle:", ["%.6e" % v for v in vm2], " expected %.6e" % sigma)

# ---------------- Euler-Bernoulli beam ----------------

def beam_element_stiffness(EI, L):
    c = EI / (L*L*L)
    return [c*12.0,   c*6.0*L,     c*-12.0,   c*6.0*L,
            c*6.0*L,  c*4.0*L*L,   c*-6.0*L,  c*2.0*L*L,
            c*-12.0,  c*-6.0*L,    c*12.0,    c*-6.0*L,
            c*6.0*L,  c*2.0*L*L,   c*-6.0*L,  c*4.0*L*L]

def beam_entries(EI, length, ne):
    L = length / ne
    ke = beam_element_stiffness(EI, L)
    out = []
    for e in range(ne):
        for k in range(16):
            out.append((e*2 + k//4, e*2 + k%4, ke[k]))
    return out

def near_node(length, ne, pos):
    return min(max(int(round(pos / length * ne)), 0), ne)

# Hermite antiderivatives in the local parameter, so a distributed load covering
# only part of an element is still exact.
def h_near_deflection(x): return x - x**3 + x**4/2.0
def h_near_slope(x):      return x*x/2.0 - 2.0*x**3/3.0 + x**4/4.0
def h_far_deflection(x):  return x**3 - x**4/2.0
def h_far_slope(x):       return -x**3/3.0 + x**4/4.0

def beam_loads(length, ne, point_forces, moments, udls):
    L = length / ne
    f = [0.0] * ((ne+1)*2)
    for (pos, mag) in point_forces:
        f[near_node(length, ne, pos)*2] += mag
    for (pos, mag) in moments:
        f[near_node(length, ne, pos)*2+1] += mag
    for (s, e_, w) in udls:
        for el in range(ne):
            x0, x1 = el*L, (el+1)*L
            a = min(max((s - x0)/L, 0.0), 1.0)
            b = min(max((e_ - x0)/L, 0.0), 1.0)
            if b <= a: continue
            f[el*2+0] += w*L*(h_near_deflection(b) - h_near_deflection(a))
            f[el*2+1] += w*L*L*(h_near_slope(b) - h_near_slope(a))
            f[el*2+2] += w*L*(h_far_deflection(b) - h_far_deflection(a))
            f[el*2+3] += w*L*L*(h_far_slope(b) - h_far_slope(a))
    return f

def beam_solve(EI, length, ne, supports, point_forces=(), moments=(), udls=()):
    n = (ne+1)*2
    ent = beam_entries(EI, length, ne)
    loads = beam_loads(length, ne, point_forces, moments, udls)
    cons = []
    for (pos, kind) in supports:
        node = near_node(length, ne, pos)
        count = {"Fixed": 2, "Pinned": 1, "Roller": 1, "Free": 0}[kind]
        for i in range(count):
            cons.append((node*2+i, 0.0))
    u, it, res, ok = solve(n, ent, loads, cons)
    return [u[i*2] for i in range(ne+1)], [u[i*2+1] for i in range(ne+1)], it, res, ok

print()
print("=== Euler-Bernoulli beam, closed-form checks ===")
Eb, I, Lb = 200e9, 8.333333333333333e-6, 3.0   # 0.1 x 0.1 m square section: b h^3/12
EI = Eb * I
P, w = 1000.0, 500.0
for ne in (4, 16):
    d, rot, it, res, ok = beam_solve(EI, Lb, ne, [(0.0, "Fixed")], point_forces=[(Lb, P)])
    exact = P*Lb**3/(3*EI)
    print(f"  cantilever tip load,   ne={ne:2d}: tip {d[-1]:.9e}  exact {exact:.9e}  err {abs(d[-1]-exact)/exact:.2e}  it={it}")
for ne in (4, 16):
    d, rot, it, res, ok = beam_solve(EI, Lb, ne, [(0.0, "Fixed")], udls=[(0.0, Lb, w)])
    exact = w*Lb**4/(8*EI)
    print(f"  cantilever UDL,        ne={ne:2d}: tip {d[-1]:.9e}  exact {exact:.9e}  err {abs(d[-1]-exact)/exact:.2e}  it={it}")
for ne in (4, 16):
    d, rot, it, res, ok = beam_solve(EI, Lb, ne, [(0.0, "Pinned"), (Lb, "Roller")], point_forces=[(Lb/2, P)])
    exact = P*Lb**3/(48*EI)
    mid = d[ne//2]
    print(f"  simply supported P mid, ne={ne:2d}: mid {mid:.9e}  exact {exact:.9e}  err {abs(mid-exact)/exact:.2e}  it={it}")
for ne in (4, 16):
    d, rot, it, res, ok = beam_solve(EI, Lb, ne, [(0.0, "Pinned"), (Lb, "Roller")], udls=[(0.0, Lb, w)])
    exact = 5*w*Lb**4/(384*EI)
    mid = d[ne//2]
    print(f"  simply supported UDL,   ne={ne:2d}: mid {mid:.9e}  exact {exact:.9e}  err {abs(mid-exact)/exact:.2e}  it={it}")
# partial-span UDL over the outer half of a cantilever, checked against the
# closed form for a UDL from a to L: delta = w/(24 EI) * (3 L^4 - 4 a^3 L + a^4)
a = Lb/2
d, rot, it, res, ok = beam_solve(EI, Lb, 8, [(0.0, "Fixed")], udls=[(a, Lb, w)])
exact = w/(24*EI) * (3*Lb**4 - 4*a**3*Lb + a**4)
print(f"  cantilever partial UDL,  ne= 8: tip {d[-1]:.9e}  exact {exact:.9e}  err {abs(d[-1]-exact)/exact:.2e}  it={it}")

# gravity check on the tet mesh: cube hanging under its own weight, fixed top face
print()
print("=== gravity body force sanity (unit cube, fixed z=1 face, g = -9.81 z) ===")
rho, g = 7850.0, -9.81
loads3 = [0.0]*n
for c in CELLS:
    v = abs(six_volume(*[CUBE[i] for i in c])) / 6.0
    for i in c:
        loads3[i*3+2] += rho * v * g / 4.0
print("  total z load %.6f N   expected rho*V*g = %.6f N" % (sum(loads3[i*3+2] for i in range(8)), rho*1.0*g))
