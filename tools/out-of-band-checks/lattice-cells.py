"""Verify the lattice unit-cell tables and the welding (ownership) rule."""
import itertools, math
from fractions import Fraction as F

def P(*a): return tuple(F(x) for x in a)

corners = [P(0,0,0), P(1,0,0), P(1,1,0), P(0,1,0),
           P(0,0,1), P(1,0,1), P(1,1,1), P(0,1,1)]

# ---- simple cubic
sc_nodes = corners
sc_struts = [(0,1),(1,2),(2,3),(3,0),(4,5),(5,6),(6,7),(7,4),(0,4),(1,5),(2,6),(3,7)]

# ---- body-centred cubic
bcc_nodes = corners + [P(F(1,2),F(1,2),F(1,2))]
bcc_struts = [(8,i) for i in range(8)]

# ---- face-centred cubic / octet (same node set)
half = F(1,2)
fcc_nodes = corners + [P(half,half,0), P(half,half,1), P(half,0,half),
                       P(half,1,half), P(0,half,half), P(1,half,half)]
fcc_struts = [(8,0),(8,1),(8,2),(8,3),
              (9,4),(9,5),(9,6),(9,7),
              (10,0),(10,1),(10,5),(10,4),
              (11,3),(11,2),(11,6),(11,7),
              (12,0),(12,3),(12,7),(12,4),
              (13,1),(13,2),(13,6),(13,5)]
octa = [(8,10),(8,11),(8,12),(8,13),(9,10),(9,11),(9,12),(9,13),
        (10,12),(10,13),(11,12),(11,13)]
octet_nodes = fcc_nodes
octet_struts = fcc_struts + octa

# ---- diamond
q = F(1,4)
dia_nodes = corners + [P(half,half,0), P(half,half,1), P(half,0,half),
                       P(half,1,half), P(0,half,half), P(1,half,half),
                       P(q,q,q), P(3*q,3*q,q), P(3*q,q,3*q), P(q,3*q,3*q)]
# indices: 0-7 corners, 8=(.5,.5,0) 9=(.5,.5,1) 10=(.5,0,.5) 11=(.5,1,.5)
#          12=(0,.5,.5) 13=(1,.5,.5), 14..17 interior
def find(nodes, p):
    for i, n in enumerate(nodes):
        if n == p: return i
    raise KeyError(p)
dia_struts = []
for k, c in enumerate([P(q,q,q), P(3*q,3*q,q), P(3*q,q,3*q), P(q,3*q,3*q)]):
    for d in itertools.product([-q, q], repeat=3):
        if sum(1 for s in d if s > 0) % 2 != 0:
            continue
        nb = tuple(a+b for a, b in zip(c, d))
        dia_struts.append((14+k, find(dia_nodes, nb)))

# ---- Kelvin (truncated octahedron), vertices = centre + perms of (0, +-1/4, +-1/2)
kel_set = set()
for perm in itertools.permutations(range(3)):
    for sa in (-1, 1):
        for sb in (-1, 1):
            off = [0, 0, 0]
            off[perm[0]] = F(0)
            off[perm[1]] = sa*F(1,4)
            off[perm[2]] = sb*F(1,2)
            kel_set.add(tuple(F(1,2)+o for o in off))
kel_nodes = sorted(kel_set)
def d2(a, b): return sum((x-y)**2 for x, y in zip(a, b))
edge2 = F(1,8)   # (1/4 sqrt2)^2
kel_struts = [(i, j) for i in range(len(kel_nodes)) for j in range(i+1, len(kel_nodes))
              if d2(kel_nodes[i], kel_nodes[j]) == edge2]

# ---- re-entrant auxetic: cube corners + one inward chevron node per cube edge
def reentrant(r):
    nodes = list(corners)
    struts = []
    for (i, j) in sc_struts:
        u, v = corners[i], corners[j]
        axis = [k for k in range(3) if u[k] != v[k]][0]
        m = []
        for k in range(3):
            if k == axis: m.append(F(1,2))
            else: m.append(r if u[k] == 0 else 1-r)
        nodes.append(tuple(m))
        struts.append((i, len(nodes)-1))
        struts.append((j, len(nodes)-1))
    return nodes, struts

CELLS = {
    "SimpleCubic": (sc_nodes, sc_struts),
    "BodyCenteredCubic": (bcc_nodes, bcc_struts),
    "FaceCenteredCubic": (fcc_nodes, fcc_struts),
    "OctetTruss": (octet_nodes, octet_struts),
    "Diamond": (dia_nodes, dia_struts),
    "Kelvin": (kel_nodes, kel_struts),
    "ReentrantAuxetic(1/4)": reentrant(F(1,4)),
}

def far(u): return u >= 1
def wrap(p): return tuple(x-1 if x >= 1 else x for x in p)

def owns_strut(counts, cell, a, b):
    for k in range(3):
        if far(a[k]) and far(b[k]) and cell[k]+1 < counts[k]:
            return False
    return True

def owns_node(counts, cell, p):
    for k in range(3):
        if far(p[k]) and cell[k]+1 < counts[k]:
            return False
    return True

def world(cell, p):  # unit cell size, origin 0
    return tuple(F(cell[k]) + p[k] for k in range(3))

def check(name, nodes, struts, counts=(3,3,3)):
    cells = list(itertools.product(range(counts[0]), range(counts[1]), range(counts[2])))
    owned, allsegs = [], set()
    for c in cells:
        for (i, j) in struts:
            a, b = nodes[i], nodes[j]
            wa, wb = world(c, a), world(c, b)
            seg = tuple(sorted([wa, wb]))
            allsegs.add(seg)
            if owns_strut(counts, c, a, b):
                owned.append(seg)
    dup = len(owned) - len(set(owned))
    missing = allsegs - set(owned)
    # node ownership
    ownednodes, allnodes = [], set()
    for c in cells:
        for p in nodes:
            wp = world(c, p)
            allnodes.add(wp)
            if owns_node(counts, c, p):
                ownednodes.append(wp)
    ndup = len(ownednodes) - len(set(ownednodes))
    nmissing = allnodes - set(ownednodes)
    # periodic valence from the cell alone
    vals = {}
    for ni, p in enumerate(nodes):
        wp = wrap(p)
        v = 0
        for (i, j) in struts:
            a, b = nodes[i], nodes[j]
            if not all(not (far(a[k]) and far(b[k])) for k in range(3)):
                continue
            v += (wrap(a) == wp) + (wrap(b) == wp)
        vals[ni] = v
    lens = sorted(set(math.sqrt(float(d2(nodes[i], nodes[j]))) for i, j in struts))
    print(f"{name}: nodes={len(nodes)} struts={len(struts)} "
          f"ownedStruts/cell={len(owned)/len(cells):.3f} "
          f"dupStruts={dup} missingStruts={len(missing)} "
          f"dupNodes={ndup} missingNodes={len(nmissing)}")
    print(f"    valences={sorted(set(vals.values()))} "
          f"strutLengths={[round(x,5) for x in lens]}")
    if dup or missing or ndup or nmissing:
        print("    *** FAIL ***")

for name, (n, s) in CELLS.items():
    check(name, n, s)

# a non-cubic count to exercise the mixed-boundary case
print("\nmixed counts (3,1,2):")
for name, (n, s) in CELLS.items():
    check(name, n, s, counts=(3, 1, 2))

print("\nKelvin node/strut tables:")
for i, p in enumerate(kel_nodes):
    print(f"  {i}: {tuple(str(x) for x in p)}")
print("  struts:", kel_struts)
