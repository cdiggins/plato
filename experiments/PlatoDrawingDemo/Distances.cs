/*
public class Functions
{
public class vec2
{
    public double x;
    public double y;

    public vec2(double x, double y)
    {
        this.x = x;
        this.y = y;
    }
}

double length(vec2 p)
{
    return Math.Sqrt(p.x * p.x + p.y * p.y);
}

vec2 abs(vec2 p)
{
    return a
}

public 

double sdCircle(vec2 p, double r)
{
    return length(p) - r;
}

// Rounded Box - exact   (https://www.shadertoy.com/view/4llXD7 and https://www.youtube.com/watch?v=s5NGeUV2EyU)

double sdRoundedBox(vec2 p, vec2 b, vec4 r)
{
    r.xy = (p.x > 0.0) ? r.xy : r.zw;
    r.x = (p.y > 0.0) ? r.x : r.y;
    vec2 q = abs(p) - b + r.x;
    return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r.x;
}

// Box - exact   (https://www.youtube.com/watch?v=62-pRVZuS5c)

double sdBox(vec2 p, vec2 b)
{
    vec2 d = abs(p) - b;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

// Oriented Box - exact   (https://www.shadertoy.com/view/stcfzn)

double sdOrientedBox(vec2 p, vec2 a, vec2 b, double th)
{
    double l = length(b - a);
    vec2 d = (b - a) / l;
    vec2 q = (p - (a + b) * 0.5);
    q = mat2(d.x, -d.y, d.y, d.x) * q;
    q = abs(q) - vec2(l, th) * 0.5;
    return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0);
}

// Segment - exact   (https://www.shadertoy.com/view/3tdSDj and https://www.youtube.com/watch?v=PMltMdi1Wzg)

double sdSegment(vec2 p, vec2 a, vec2 b)
{
    vec2 pa = p - a, ba = b - a;
    double h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h);
}

// Rhombus - exact   (https://www.shadertoy.com/view/XdXcRB)

double ndot(vec2 a, vec2 b)
{
    return a.x * b.x - a.y * b.y;
}

double sdRhombus(vec2 p, vec2 b)
{
    p = abs(p);
    double h = clamp(ndot(b - 2.0 * p, b) / dot(b, b), -1.0, 1.0);
    double d = length(p - 0.5 * b * vec2(1.0 - h, 1.0 + h));
    return d * sign(p.x * b.y + p.y * b.x - b.x * b.y);
}

// Isosceles Trapezoid - exact   (https://www.shadertoy.com/view/MlycD3)

double sdTrapezoid(vec2 p, double r1, double r2, double he)
{
    vec2 k1 = vec2(r2, he);
    vec2 k2 = vec2(r2 - r1, 2.0 * he);
    p.x = abs(p.x);
    vec2 ca = vec2(p.x - min(p.x, (p.y < 0.0) ? r1 : r2), abs(p.y) - he);
    vec2 cb = p - k1 + k2 * clamp(dot(k1 - p, k2) / dot2(k2), 0.0, 1.0);
    double s = (cb.x < 0.0 && ca.y < 0.0) ? -1.0 : 1.0;
    return s * sqrt(min(dot2(ca), dot2(cb)));
}

//Parallelogram - exact   (https://www.shadertoy.com/view/7dlGRf)

double sdParallelogram(vec2 p, double wi, double he, double sk)
{
    vec2 e = vec2(sk, he);
    p = (p.y < 0.0) ? -p : p;
    vec2 w = p - e;
    w.x -= clamp(w.x, -wi, wi);
    vec2 d = vec2(dot(w, w), -w.y);
    double s = p.x * e.y - p.y * e.x;
    p = (s < 0.0) ? -p : p;
    vec2 v = p - vec2(wi, 0);
    v -= e * clamp(dot(v, e) / dot(e, e), -1.0, 1.0);
    d = min(d, vec2(dot(v, v), wi * he - abs(s)));
    return sqrt(d.x) * sign(-d.y);
}

// Equilateral Triangle - exact   (https://www.shadertoy.com/view/Xl2yDW)

double sdEquilateralTriangle(vec2 p, double r)
{
    const double k = sqrt(3.0);
    p.x = abs(p.x) - r;
    p.y = p.y + r / k;
    if (p.x + k * p.y > 0.0) p = vec2(p.x - k * p.y, -k * p.x - p.y) / 2.0;
    p.x -= clamp(p.x, -2.0 * r, 0.0);
    return -length(p) * sign(p.y);
}

// Isosceles Triangle - exact   (https://www.shadertoy.com/view/MldcD7)

double sdTriangleIsosceles(vec2 p, vec2 q)
{
    p.x = abs(p.x);
    vec2 a = p - q * clamp(dot(p, q) / dot(q, q), 0.0, 1.0);
    vec2 b = p - q * vec2(clamp(p.x / q.x, 0.0, 1.0), 1.0);
    double s = -sign(q.y);
    vec2 d = min(vec2(dot(a, a), s * (p.x * q.y - p.y * q.x)),
        vec2(dot(b, b), s * (p.y - q.y)));
    return -sqrt(d.x) * sign(d.y);
}

// Triangle - exact   (https://www.shadertoy.com/view/XsXSz4)

double sdTriangle(vec2 p, vec2 p0, vec2 p1, vec2 p2)
{
    vec2 e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;
    vec2 v0 = p - p0, v1 = p - p1, v2 = p - p2;
    vec2 pq0 = v0 - e0 * clamp(dot(v0, e0) / dot(e0, e0), 0.0, 1.0);
    vec2 pq1 = v1 - e1 * clamp(dot(v1, e1) / dot(e1, e1), 0.0, 1.0);
    vec2 pq2 = v2 - e2 * clamp(dot(v2, e2) / dot(e2, e2), 0.0, 1.0);
    double s = sign(e0.x * e2.y - e0.y * e2.x);
    vec2 d = min(min(vec2(dot(pq0, pq0), s * (v0.x * e0.y - v0.y * e0.x)),
            vec2(dot(pq1, pq1), s * (v1.x * e1.y - v1.y * e1.x))),
        vec2(dot(pq2, pq2), s * (v2.x * e2.y - v2.y * e2.x)));
    return -sqrt(d.x) * sign(d.y);
}

// Uneven Capsule - exact   (https://www.shadertoy.com/view/4lcBWn)

double sdUnevenCapsule(vec2 p, double r1, double r2, double h)
{
    p.x = abs(p.x);
    double b = (r1 - r2) / h;
    double a = sqrt(1.0 - b * b);
    double k = dot(p, vec2(-b, a));
    if (k < 0.0) return length(p) - r1;
    if (k > a * h) return length(p - vec2(0.0, h)) - r2;
    return dot(p, vec2(a, b)) - r1;
}

// Regular Pentagon - exact   (https://www.shadertoy.com/view/llVyWW)

double sdPentagon(vec2 p, double r)
{
    const vec3 k = vec3(0.809016994, 0.587785252, 0.726542528);
    p.x = abs(p.x);
    p -= 2.0 * min(dot(vec2(-k.x, k.y), p), 0.0) * vec2(-k.x, k.y);
    p -= 2.0 * min(dot(vec2(k.x, k.y), p), 0.0) * vec2(k.x, k.y);
    p -= vec2(clamp(p.x, -r * k.z, r * k.z), r);
    return length(p) * sign(p.y);
}

// Regular Hexagon - exact

double sdHexagon(vec2 p, double r)
{
    const vec3 k = vec3(-0.866025404, 0.5, 0.577350269);
    p = abs(p);
    p -= 2.0 * min(dot(k.xy, p), 0.0) * k.xy;
    p -= vec2(clamp(p.x, -k.z * r, k.z * r), r);
    return length(p) * sign(p.y);
}

// Regular Octogon - exact   (https://www.shadertoy.com/view/llGfDG)

double sdOctogon(vec2 p, double r)
{
    const vec3 k = vec3(-0.9238795325, 0.3826834323, 0.4142135623);
    p = abs(p);
    p -= 2.0 * min(dot(vec2(k.x, k.y), p), 0.0) * vec2(k.x, k.y);
    p -= 2.0 * min(dot(vec2(-k.x, k.y), p), 0.0) * vec2(-k.x, k.y);
    p -= vec2(clamp(p.x, -k.z * r, k.z * r), r);
    return length(p) * sign(p.y);
}

// Hexagram - exact   (https://www.shadertoy.com/view/tt23RR)

double sdHexagram(vec2 p, double r)
{
    const vec4 k = vec4(-0.5, 0.8660254038, 0.5773502692, 1.7320508076);
    p = abs(p);
    p -= 2.0 * min(dot(k.xy, p), 0.0) * k.xy;
    p -= 2.0 * min(dot(k.yx, p), 0.0) * k.yx;
    p -= vec2(clamp(p.x, r * k.z, r * k.w), r);
    return length(p) * sign(p.y);
}

// Star 5 - exact   (https://www.shadertoy.com/view/3tSGDy)

double sdStar5(vec2 p, double r, double rf)
{
    const vec2 k1 = vec2(0.809016994375, -0.587785252292);
    const vec2 k2 = vec2(-k1.x, k1.y);
    p.x = abs(p.x);
    p -= 2.0 * max(dot(k1, p), 0.0) * k1;
    p -= 2.0 * max(dot(k2, p), 0.0) * k2;
    p.x = abs(p.x);
    p.y -= r;
    vec2 ba = rf * vec2(-k1.y, k1.x) - vec2(0, 1);
    double h = clamp(dot(p, ba) / dot(ba, ba), 0.0, r);
    return length(p - ba * h) * sign(p.y * ba.x - p.x * ba.y);
}

// Regular Star - exact   (https://www.shadertoy.com/view/3tSGDy)

double sdStar(vec2 p, double r, int n, double m)
{
    // next 4 lines can be precomputed for a given shape
    double an = 3.141593 / double(n);
    double en = 3.141593 / m; // m is between 2 and n
    vec2 acs = vec2(cos(an), sin(an));
    vec2 ecs = vec2(cos(en), sin(en)); // ecs=vec2(0,1) for regular polygon

    double bn = mod(atan(p.x, p.y), 2.0 * an) - an;
    p = length(p) * vec2(cos(bn), abs(sin(bn)));
    p -= r * acs;
    p += ecs * clamp(-dot(p, ecs), 0.0, r * acs.y / ecs.y);
    return length(p) * sign(p.x);
}

// Pie - exact   (https://www.shadertoy.com/view/3l23RK)

double sdPie(vec2 p, vec2 c, double r)
{
    p.x = abs(p.x);
    double l = length(p) - r;
    double m = length(p - c * clamp(dot(p, c), 0.0, r)); // c=sin/cos of aperture
    return max(l, m * sign(c.y * p.x - c.x * p.y));
}

// Cut Disk - exact   (https://www.shadertoy.com/view/ftVXRc)

double sdCutDisk(vec2 p, double r, double h)
{
    double w = sqrt(r * r - h * h); // constant for any given shape
    p.x = abs(p.x);
    double s = max((h - r) * p.x * p.x + w * w * (h + r - 2.0 * p.y), h * p.x - w * p.y);
    return (s < 0.0) ? length(p) - r :
        (p.x < w) ? h - p.y :
        length(p - vec2(w, h));
}

// Arc - exact   (https://www.shadertoy.com/view/wl23RK)

double sdArc(vec2 p, vec2 sc, double ra, double rb)
{
    // sc is the sin/cos of the arc's aperture
    p.x = abs(p.x);
    return ((sc.y * p.x > sc.x * p.y) ? length(p - sc * ra) : abs(length(p) - ra)) - rb;
}

// Horseshoe - exact   (https://www.shadertoy.com/view/WlSGW1)

double sdHorseshoe(vec2 p, vec2 c, double r, vec2 w)
{
    p.x = abs(p.x);
    double l = length(p);
    p = mat2(-c.x, c.y, c.y, c.x) * p;
    p = vec2((p.y > 0.0 || p.x > 0.0) ? p.x : l * sign(-c.x),
        (p.x > 0.0) ? p.y : l);
    p = vec2(p.x, abs(p.y - r)) - w;
    return length(max(p, 0.0)) + min(0.0, max(p.x, p.y));
}

// Vesica - exact   (https://www.shadertoy.com/view/XtVfRW)

double sdVesica(vec2 p, double r, double d)
{
    p = abs(p);
    double b = sqrt(r * r - d * d);
    return ((p.y - b) * d > p.x * b)
        ? length(p - vec2(0.0, b))
        : length(p - vec2(-d, 0.0)) - r;
}

// Moon - exact   (https://www.shadertoy.com/view/WtdBRS)

double sdMoon(vec2 p, double d, double ra, double rb)
{
    p.y = abs(p.y);
    double a = (ra * ra - rb * rb + d * d) / (2.0 * d);
    double b = sqrt(max(ra * ra - a * a, 0.0));
    if (d * (p.x * b - p.y * a) > d * d * max(b - p.y, 0.0))
        return length(p - vec2(a, b));
    return max((length(p) - ra),
        -(length(p - vec2(d, 0)) - rb));
}

// Circle Cross - exact   (https://www.shadertoy.com/view/NslXDM)

double sdRoundedCross(vec2 p, double h)
{
    double k = 0.5 * (h + 1.0 / h); // k should be const at modeling time
    p = abs(p);
    return (p.x < 1.0 && p.y < p.x * (k - h) + h)
        ? k - sqrt(dot2(p - vec2(1, k)))
        : sqrt(min(dot2(p - vec2(0, h)),
            dot2(p - vec2(1, 0))));
}

// Simple Egg - exact   (https://www.shadertoy.com/view/XtVfRW)

double sdEgg(vec2 p, double ra, double rb)
{
    const double k = sqrt(3.0);
    p.x = abs(p.x);
    double r = ra - rb;
    return ((p.y < 0.0) ? length(vec2(p.x, p.y)) - r :
        (k * (p.x + r) < p.y) ? length(vec2(p.x, p.y - k * r)) :
        length(vec2(p.x + r, p.y)) - 2.0 * r) - rb;
}

// Heart - exact   (https://www.shadertoy.com/view/3tyBzV)

double sdHeart(vec2 p)
{
    p.x = abs(p.x);

    if (p.y + p.x > 1.0)
        return sqrt(dot2(p - vec2(0.25, 0.75))) - sqrt(2.0) / 4.0;
    return sqrt(min(dot2(p - vec2(0.00, 1.00)),
        dot2(p - 0.5 * max(p.x + p.y, 0.0)))) * sign(p.x - p.y);
}

// Cross - exact exterior, bound interior   (https://www.shadertoy.com/view/XtGfzw)

double sdCross(vec2 p, vec2 b, double r)
{
    p = abs(p);
    p = (p.y > p.x) ? p.yx : p.xy;
    vec2 q = p - b;
    double k = max(q.y, q.x);
    vec2 w = (k > 0.0) ? q : vec2(b.y - p.x, -k);
    return sign(k) * length(max(w, 0.0)) + r;
}

// Rounded X - exact   (https://www.shadertoy.com/view/3dKSDc)

double sdRoundedX(vec2 p, double w, double r)
{
    p = abs(p);
    return length(p - min(p.x + p.y, w) * 0.5) - r;
}

// Polygon - exact   (https://www.shadertoy.com/view/wdBXRW)

double sdPolygon(vec2[N]
v, vec2 p )
{
    double d = dot(p - v[0], p - v[0]);
    double s = 1.0;
    for (int i = 0, j = N - 1; i < N; j = i, i++)
    {
        vec2 e = v[j] - v[i];
        vec2 w = p - v[i];
        vec2 b = w - e * clamp(dot(w, e) / dot(e, e), 0.0, 1.0);
        d = min(d, dot(b, b));
        bvec3 c = bvec3(p.y >= v[i].y, p.y < v[j].y, e.x * w.y > e.y * w.x);
        if (all(c) || all(not(c))) s *= -1.0;
    }

    return s * sqrt(d);
}

// Ellipse - exact   (https://www.shadertoy.com/view/4sS3zz)

double sdEllipse(vec2 p, vec2 ab)
{
    p = abs(p);
    if (p.x > p.y)
    {
        p = p.yx;
        ab = ab.yx;
    }

    double l = ab.y * ab.y - ab.x * ab.x;
    double m = ab.x * p.x / l;
    double m2 = m * m;
    double n = ab.y * p.y / l;
    double n2 = n * n;
    double c = (m2 + n2 - 1.0) / 3.0;
    double c3 = c * c * c;
    double q = c3 + m2 * n2 * 2.0;
    double d = c3 + m2 * n2;
    double g = m + m * n2;
    double co;
    if (d < 0.0)
    {
        double h = acos(q / c3) / 3.0;
        double s = cos(h);
        double t = sin(h) * sqrt(3.0);
        double rx = sqrt(-c * (s + t + 2.0) + m2);
        double ry = sqrt(-c * (s - t + 2.0) + m2);
        co = (ry + sign(l) * rx + abs(g) / (rx * ry) - m) / 2.0;
    }
    else
    {
        double h = 2.0 * m * n * sqrt(d);
        double s = sign(q + h) * pow(abs(q + h), 1.0 / 3.0);
        double u = sign(q - h) * pow(abs(q - h), 1.0 / 3.0);
        double rx = -s - u - c * 4.0 + 2.0 * m2;
        double ry = (s - u) * sqrt(3.0);
        double rm = sqrt(rx * rx + ry * ry);
        co = (ry / sqrt(rm - rx) + 2.0 * g / rm - m) / 2.0;
    }

    vec2 r = ab * vec2(co, sqrt(1.0 - co * co));
    return length(r - p) * sign(p.y - r.y);
}

// Parabola - exact   (https://www.shadertoy.com/view/ws3GD7)

double sdParabola(vec2 pos, double k)
{
    pos.x = abs(pos.x);
    double ik = 1.0 / k;
    double p = ik * (pos.y - 0.5 * ik) / 3.0;
    double q = 0.25 * ik * ik * pos.x;
    double h = q * q - p * p * p;
    double r = sqrt(abs(h));
    double x = (h > 0.0)
        ? pow(q + r, 1.0 / 3.0) - pow(abs(q - r), 1.0 / 3.0) * sign(r - q)
        : 2.0 * cos(atan(r, q) / 3.0) * sqrt(p);
    return length(pos - vec2(x, k * x * x)) * sign(pos.x - x);
}

// Parabola Segment - exact   (https://www.shadertoy.com/view/3lSczz)

double sdParabola(vec2 pos, double wi, double he)
{
    pos.x = abs(pos.x);
    double ik = wi * wi / he;
    double p = ik * (he - pos.y - 0.5 * ik) / 3.0;
    double q = pos.x * ik * ik * 0.25;
    double h = q * q - p * p * p;
    double r = sqrt(abs(h));
    double x = (h > 0.0)
        ? pow(q + r, 1.0 / 3.0) - pow(abs(q - r), 1.0 / 3.0) * sign(r - q)
        : 2.0 * cos(atan(r / q) / 3.0) * sqrt(p);
    x = min(x, wi);
    return length(pos - vec2(x, he - x * x / ik)) *
           sign(ik * (pos.y - he) + pos.x * pos.x);
}

// Quadratic Bezier - exact   (https://www.shadertoy.com/view/MlKcDD)

double sdBezier(vec2 pos, vec2 A, vec2 B, vec2 C)
{
    vec2 a = B - A;
    vec2 b = A - 2.0 * B + C;
    vec2 c = a * 2.0;
    vec2 d = A - pos;
    double kk = 1.0 / dot(b, b);
    double kx = kk * dot(a, b);
    double ky = kk * (2.0 * dot(a, a) + dot(d, b)) / 3.0;
    double kz = kk * dot(d, a);
    double res = 0.0;
    double p = ky - kx * kx;
    double p3 = p * p * p;
    double q = kx * (2.0 * kx * kx - 3.0 * ky) + kz;
    double h = q * q + 4.0 * p3;
    if (h >= 0.0)
    {
        h = sqrt(h);
        vec2 x = (vec2(h, -h) - q) / 2.0;
        vec2 uv = sign(x) * pow(abs(x), vec2(1.0 / 3.0));
        double t = clamp(uv.x + uv.y - kx, 0.0, 1.0);
        res = dot2(d + (c + b * t) * t);
    }
    else
    {
        double z = sqrt(-p);
        double v = acos(q / (p * z * 2.0)) / 3.0;
        double m = cos(v);
        double n = sin(v) * 1.732050808;
        vec3 t = clamp(vec3(m + m, -n - m, n - m) * z - kx, 0.0, 1.0);
        res = min(dot2(d + (c + b * t.x) * t.x),
            dot2(d + (c + b * t.y) * t.y));
        // the third root cannot be the closest
        // res = min(res,dot2(d+(c+b*t.z)*t.z));
    }

    return sqrt(res);
}

// Bobbly Cross - exact   (https://www.shadertoy.com/view/NssXWM)

double sdBlobbyCross(vec2 pos, double he)
{
    pos = abs(pos);
    pos = vec2(abs(pos.x - pos.y), 1.0 - pos.x - pos.y) / sqrt(2.0);

    double p = (he - pos.y - 0.25 / he) / (6.0 * he);
    double q = pos.x / (he * he * 16.0);
    double h = q * q - p * p * p;

    double x;
    if (h > 0.0)
    {
        double r = sqrt(h);
        x = pow(q + r, 1.0 / 3.0) - pow(abs(q - r), 1.0 / 3.0) * sign(r - q);
    }
    else
    {
        double r = sqrt(p);
        x = 2.0 * r * cos(acos(q / (p * r)) / 3.0);
    }

    x = min(x, sqrt(2.0) / 2.0);

    vec2 z = vec2(x, he * (1.0 - 2.0 * x * x)) - pos;
    return length(z) * sign(z.y);
}

// Tunnel - exact   (https://www.shadertoy.com/view/flSSDy)

double sdTunnel(vec2 p, vec2 wh)
{
    p.x = abs(p.x);
    p.y = -p.y;
    vec2 q = p - wh;

    double d1 = dot2(vec2(max(q.x, 0.0), q.y));
    q.x = (p.y > 0.0) ? q.x : length(p) - wh.x;
    double d2 = dot2(vec2(q.x, max(q.y, 0.0)));
    double d = sqrt(min(d1, d2));

    return (max(q.x, q.y) < 0.0) ? -d : d;
}

// Stairs - exact   (https://www.shadertoy.com/view/7tKSWt)

double sdStairs(vec2 p, vec2 wh, double n)
{
    vec2 ba = wh * n;
    double d = min(dot2(p - vec2(clamp(p.x, 0.0, ba.x), 0.0)),
        dot2(p - vec2(ba.x, clamp(p.y, 0.0, ba.y))));
    double s = sign(max(-p.y, p.x - ba.x));

    double dia = length(wh);
    p = mat2(wh.x, -wh.y, wh.y, wh.x) * p / dia;
    double id = clamp(round(p.x / dia), 0.0, n - 1.0);
    p.x = p.x - id * dia;
    p = mat2(wh.x, wh.y, -wh.y, wh.x) * p / dia;

    double hh = wh.y / 2.0;
    p.y -= hh;
    if (p.y > hh * sign(p.x)) s = 1.0;
    p = (id < 0.5 || p.x > 0.0) ? p : -p;
    d = min(d, dot2(p - vec2(0.0, clamp(p.y, -hh, hh))));
    d = min(d, dot2(p - vec2(clamp(p.x, 0.0, wh.x), hh)));

    return sqrt(d) * s;
}

// Quadratic Circle - exact   (https://www.shadertoy.com/view/Nd3cW8)

double sdQuadraticCircle(vec2 p)
{
    p = abs(p);
    if (p.y > p.x) p = p.yx;

    double a = p.x - p.y;
    double b = p.x + p.y;
    double c = (2.0 * b - 1.0) / 3.0;
    double h = a * a + c * c * c;
    double t;
    if (h >= 0.0)
    {
        h = sqrt(h);
        t = sign(h - a) * pow(abs(h - a), 1.0 / 3.0) - pow(h + a, 1.0 / 3.0);
    }
    else
    {
        double z = sqrt(-c);
        double v = acos(a / (c * z)) / 3.0;
        t = -z * (cos(v) + sin(v) * 1.732050808);
    }

    t *= 0.5;
    vec2 w = vec2(-t, t) + 0.75 - t * t - p;
    return length(w) * sign(a * a * 0.5 + b - 1.5);
}

// Hyperbola - exact   (https://www.shadertoy.com/view/DtjXDG)

double sdHyberbola(vec2 p, double k, double he) // k (0,inf)
{
    p = abs(p);
    p = vec2(p.x - p.y, p.x + p.y) / sqrt(2.0);

    double x2 = p.x * p.x / 16.0;
    double y2 = p.y * p.y / 16.0;
    double r = k * (4.0 * k - p.x * p.y) / 12.0;
    double q = (x2 - y2) * k * k;
    double h = q * q + r * r * r;
    double u;
    if (h < 0.0)
    {
        double m = sqrt(-r);
        u = m * cos(acos(q / (r * m)) / 3.0);
    }
    else
    {
        double m = pow(sqrt(h) - q, 1.0 / 3.0);
        u = (m - r / m) / 2.0;
    }

    double w = sqrt(u + x2);
    double b = k * p.y - x2 * p.x * 2.0;
    double t = p.x / 4.0 - w + sqrt(2.0 * x2 - u + b / w / 4.0);

    t = max(t, sqrt(he * he * 0.5 + k) - he / sqrt(2.0));

    double d = length(p - vec2(t, k / t));
    return p.x * p.y < k ? d : -d;
}



// Making shapes rounded

// All the shapes above can be converted into rounded shapes by subtracting a constant from their distance function. That, effectivelly moves the isosurface (isopetimeter I guess) from the level zero to one of the outter rings, which naturally are rounded, as it can be seen the yellow areas all the images above. So, basically, for any shape defined by d(x,y) = sdf(x,y), one can make it sounded by computing d(x,y) = sdf(x,y) - r. You can learn more about this this Youtube video.

double opRound(vec2 p, double r)
{
    return sdShape(p) - r;
}

// These are a few examples: rounded line, rounded triangle, rounded box and a rounded pentagon:

// Making shapes annular

// Similarly, shapes can be made annular (like a ring or the layers of an onion), but taking their absolute value and then substracting a constant from their field. So, for any shape defined by d(x,y) = sdf(x,y) compute d(x,y) = |sdf(x,y)| - r:

double opOnion(vec2 p, double r)
{
    return abs(sdShape(p)) - r;
}

// These are a few examples: annular rounded line, an annular triangle, an annular box and a annular pentagon:


}
*/