# Plato sum types with exhaustive matching — design

**Date:** 2026-07-27
**Tracker:** [plato-232](../../tracker/issues/plato-232.md) (executes the RFC idea [plato-077](../../tracker/issues/plato-077.md))
**Status:** implemented historical design, with superseded choices identified below. This is not an open implementation plan or the language specification. Refreshed under [docs-527](../../tracker/issues/docs-527.md).
**Companion:** test corpus in [`plato-test-sum/`](../../tests/plato-test-sum/README.md); v3 migration survey in [`reports/plato-sum-types-v3-survey.md`](../reports/plato-sum-types-v3-survey.md).

This document records the July design rationale. Sum types and exhaustive `match`
are implemented, including generic sums. Read [SEMANTICS.md](../SEMANTICS.md#sum-types)
for the language contract and the [October 23 generic-sum decision](../../tracker/decisions/2026-10-23-generic-sums-monomorphize-per-use.md)
for the change from the original restricted implementation. Historical proposals and
examples below do not override those authorities.

Implementation evidence lives in the [sum checker](../../src/Plato.Compiler/Checking/SumTypeChecker.cs),
[C# writer](../../writers/Plato.CSharpWriter/CSharpConcreteTypeWriter.cs),
[checking tests](../../tests/PlatoTests/Checking/SumTypeCheckingTests.cs),
[C# emission and execution tests](../../tests/PlatoTests/Emission/SumTypeCodegenTests.cs),
and [generic-sum interpreter and differential tests](../../tests/Plato.Interpreter.Tests/GenericSumTests.cs).
Those tests cover supported behavior; their existence is not a claim that they were
run during this documentation refresh. Direct recursive sums are discussed separately
in section 7; nested generic instances are not the same thing as recursive declarations.

---

## 1. Motivation

At the time of the July proposal, Plato had no way to say "one of these shapes."
The stdlib worked around the gap
with a **kind-pattern hand-encoding**: an `Integer`-wrapping `XxxKind` type used as a
discriminant field on a carrier type whose remaining fields are *conditionally
meaningful* — valid for some tag values, junk for others. The canonical example is
`stdlib/40-paths.plato`, verbatim:

```plato
type PathSegmentKind
{
    Value: Integer;
}

// One drawing verb of a contour. Every segment starts at the previous
// segment's endpoint, and the on-curve endpoint is always P3. P1 and P2 are
// control points when Kind uses them (Quadratic: P1; Cubic: P1 then P2). The
// arc fields follow the SVG elliptical-arc form (x/y radii, rotation of the
// ellipse axes, large-arc and sweep flags) and apply only when Kind is Arc.
// Fields a Kind does not use hold zeroes and are ignored.
type PathSegment2D
    implements Value
{
    Kind: PathSegmentKind;
    P1: Point2D;
    P2: Point2D;
    P3: Point2D;
    Radii: Vector2;
    AxisRotation: Angle;
    LargeArc: Boolean;
    Sweep: Boolean;
}
```

Everything wrong with this pattern is in its own doc-comment: *"P1 and P2 are control
points when Kind uses them"*, *"apply only when Kind is Arc"*, *"Fields a Kind does not
use hold zeroes and are ignored."* The type system does not know any of this. Nothing
stops a caller reading `P1` on a `Move` segment, and nothing forces a `switch` over
`Kind` to handle `Arc`. The field names (`P1`, `P2`, `P3`) are positional and untyped
by role; the meaning lives in prose. This is exactly the *partiality* weakness that
[plato-077](../../tracker/issues/plato-077.md) and `plato-overview.md` name as the
weakest part of Plato's type story.

A sum type says the same thing precisely, and the compiler checks it:

```plato
type PathSegment2D implements Value
    = Move(EndPoint: Point2D)
    | Line(EndPoint: Point2D)
    | Quadratic(Control: Point2D, EndPoint: Point2D)
    | Cubic(Control1: Point2D, Control2: Point2D, EndPoint: Point2D)
    | Arc(Radii: Vector2, AxisRotation: Angle, LargeArc: Boolean, Sweep: Boolean, EndPoint: Point2D)
    | Close;
```

Now `Control` exists only on `Quadratic`/`Cubic`, the arc fields exist only on `Arc`,
`Close` carries nothing, and any consumer must `match` all six verbs or be rejected.
The historical v3 survey (companion doc) classifies the kind-pattern types and
identifies the true sums that motivated migration. Inventory counts belong in that survey.

Sum types were also a prerequisite for the Gratify kernel port
([plato-076](../../tracker/issues/plato-076.md)) and the stdlib
partiality cleanup ([plato-079](../../tracker/issues/plato-079.md), which proposed
`Option`/`Result` in place of `Tuple2<T, Boolean>` and sentinel values).

---

## 2. Surface syntax

A `type` declaration gains a second body form: an `=` **case list**, as an alternative
to the existing braced field body. The two bodies are mutually exclusive — a type is
either a product (braces) or a sum (`= ... ;`).

### 2.1 Declaration

```
type FillRule = NonZero | EvenOdd;          // enum: degenerate sum, all cases payload-free

type Option<T> = None | Some(Value: T);     // generic sum (supported by default; see §3.5)

type PathSegment2D implements Value         // implements clause allowed, precedes the body
    = Move(EndPoint: Point2D)
    | Line(EndPoint: Point2D)
    | Quadratic(Control: Point2D, EndPoint: Point2D)
    | Cubic(Control1: Point2D, Control2: Point2D, EndPoint: Point2D)
    | Arc(Radii: Vector2, AxisRotation: Angle, LargeArc: Boolean, Sweep: Boolean, EndPoint: Point2D)
    | Close;
```

Each **case** is a name, optionally followed by a parenthesized list of named,
typed **case fields**. A payload-free case (`Close`, `NonZero`) omits the parentheses.

### 2.2 Match

`match` is an **expression** (it has a value and a type), exhaustive, with positional
binders in case-field declaration order and **no default arm** in v1:

```plato
EndPoint(seg: PathSegment2D, start: Point2D): Point2D =>
    match (seg) {
        Move(p)                    => p;
        Line(p)                    => p;
        Quadratic(c, p)            => p;
        Cubic(c1, c2, p)           => p;
        Arc(r, rot, large, sweep, p) => p;
        Close                      => start;
    };
```

A case pattern binds exactly one name per case field, positionally
(`Quadratic(c, p)` binds `c = Control`, `p = EndPoint`). A payload-free case uses a
bare pattern with no parens (`Close => ...`). Because `match` is an expression it may
appear anywhere an expression may — as a call argument, an operand, or an arm of an
enclosing `match` (see `plato-test-sum/match-expression.plato` and `nested-match.plato`).

### 2.3 Grammar sketch

Additions to the existing type/expression grammar (EBNF-ish; existing productions
referenced by name):

```
TypeDecl    := 'type' Ident TypeParams? ImplementsClause? TypeBody
TypeBody    := BracedFieldBody                       (* existing *)
             | SumBody                                (* new *)
SumBody     := '=' CaseList ';'
CaseList    := Case ('|' Case)*
Case        := Ident CaseFields?
CaseFields  := '(' FieldDecl (',' FieldDecl)* ')'
FieldDecl   := Ident ':' TypeExpr                    (* existing shape *)

MatchExpr   := 'match' '(' Expr ')' '{' MatchArm+ '}'
MatchArm    := CasePattern '=>' Expr ';'
CasePattern := Ident CaseBinders?
CaseBinders := '(' Ident (',' Ident)* ')'
```

Notes for the parser implementer:
- `=` immediately after the type header (and optional `implements`) selects `SumBody`;
  `{` selects the existing braced body. One token of lookahead disambiguates.
- `match` is a new keyword. It is contextual-friendly (no stdlib identifier is named
  `match`), but making it a reserved word is simplest.
- Case fields reuse the existing `FieldDecl` production, so per-case field parsing is
  the field parser already in place.
- The trailing `;` terminates the sum body exactly like other declarations.

---

## 3. Static semantics

### 3.1 Declaration rules

- A sum type has **≥ 1** case. (A zero-case type is rejected; it is uninhabited and
  serves no purpose in v1.)
- Case fields obey the ordinary field rules. In particular the **10-field tuple cap**
  ([plato-230](../../tracker/issues/plato-230.md)) applies **per case**, not to the
  flattened struct: each case synthesizes a constructor of its own arity, and no case
  may exceed 10 fields. The flattened struct may hold far more than 10 fields in total
  (PathSegment2D flattens to 12) — that is fine, because no single *constructor* takes
  them all as positional tuple elements (the all-fields ctor is private and named-only;
  see §5).
- A sum type is a product **xor** a sum: it may not have both a braced body and a case
  list.

### 3.2 Name rules

- **Case names are unique within their sum type** (→ CHK305). They become the C# tag
  constants and the `Case_Field` prefixes, so collisions are fatal.
- **Case-field names are unique within their case** (ordinary field rule).
- Case-field names **may repeat across cases** (`Move.EndPoint`, `Line.EndPoint`,
  `Quadratic.EndPoint` all coexist) — they are disambiguated by the `Case_` prefix in
  the flattened struct. This is intentional and is what makes the SVG-verb modelling
  read well.
- Case names live in the **sum type's namespace**: the qualified constructor form is
  `PathSegment2D.Move(...)`. Whether a bare `Move(...)` is also offered is an open
  resolver question (§4.4).

### 3.3 Exhaustiveness

A `match` is checked against the subject's sum type:
- **Every case must appear exactly once.** A missing case is CHK300; an unknown case
  (a pattern naming a non-case) is CHK301; a repeated case is CHK302.
- **No default/wildcard arm in v1** (CHK307 if one is written). Exhaustiveness is by
  explicit enumeration only. This is deliberate: it makes adding a case a compile-time
  fan-out to every consumer (the whole point), and keeps the checker trivial. A default
  arm is a candidate for a later version.
- **Binder arity** must equal the case's field count (→ CHK303). Payload-free cases
  take no binders.
- The **result type** of a `match` is the least upper bound of its arm types under the
  existing type rules; in practice every arm must have the same type (Plato has no
  subtyping join beyond interface unification, so "same type" is the operative rule).

### 3.4 The subject must be a sum

`match` on a non-sum value is CHK304. Product types (records), primitives, arrays, and
interfaces are all rejected as subjects.

### 3.5 Generics: original choice superseded

The July proposal made generic sums conditional on implementation risk and sketched
separate C# types such as `Option_Number`. The first implementation restricted generic
sums with CHK306. The [October 23 decision](../../tracker/decisions/2026-10-23-generic-sums-monomorphize-per-use.md)
replaced that restriction: generic sums are supported by default and emit as one generic
C# struct, such as `Option<T>`. CHK306 remains only for hosts explicitly requesting the
monomorphic-only stance.

See [SEMANTICS.md](../SEMANTICS.md#sum-types) for declaration parameters, `$T` in library
signatures, and per-use case instantiation. The decision also records the remaining
limitations; the July fallback is not a statement of present language support.

---

## 4. Dynamic semantics and lowering

### 4.1 Values

A sum value is a tagged product: a discriminant `Kind` (the case's 0-based declaration
index) plus the flattened per-case fields, with the inactive cases' fields holding their
zero value. It is a **concrete value type** — no heap allocation, no boxing, copy
semantics. Equality and hashing use the tag and the active case's fields only (§5.3).
The original all-fields comparison was superseded; inactive payloads carry no meaning.

### 4.2 Match lowering — no new TIR node

`match` is lowered **during elaboration** into a tag-conditional chain built entirely
from **existing** Typed-IR nodes — `Conditional`, integer `Equals`, `FieldAccess`,
integer literals, and `Let`. No new TIR node type is introduced, so every downstream
TIR consumer can handle the lowered control flow. This is a representation
choice, not a claim that implementing sums required no downstream changes.
Concretely, `match (e) { C0(bs0) => a0; …; Cn(bsn) => an; }`
elaborates to:

```
let __subject = e in
    __subject.Kind == 0 ? «a0 with bs0 bound to __subject.C0_field projections»
  : __subject.Kind == 1 ? «a1 with bs1 bound»
  : …
  :                        «an»          // last case: unconditional else (exhaustive)
```

Each binder is bound to a field projection on `__subject` — either as a `Let`
(`let c = __subject.Quadratic_Control in …`) or, since projections are pure and
side-effect-free, inlined directly. The **last case needs no tag test**: exhaustiveness
guarantees the subject is one of the cases, so the final arm is the terminal `else`.
This both avoids a redundant comparison and sidesteps C#'s "not all paths return"
concern (§5.4).

### 4.3 Case construction

A per-case constructor function is synthesized for each case. `PathSegment2D.Move(p)`
builds the value with `Kind = 0`, `Move_EndPoint = p`, and every other flattened field
defaulted. These map one-to-one onto the emitted static factories (§5.2).

### 4.4 Historical question: bare vs qualified constructor names

Baseline surface is the **qualified** form `TypeName.Case(args)`, which is unambiguous
and always available. A **bare** `Case(args)` is offered **only if** the resolver's
existing overloading makes it unambiguous at the use site (no other visible symbol named
`Case` with a compatible signature). This is the implementer's call and is documented as
an open point — start qualified-only; add bare resolution if it drops out cheaply.

---

## 5. C# representation

A sum type is emitted as **one `readonly` `partial struct`** in the house style of
`writers/Plato.CSharpWriter/CSharpConcreteTypeWriter.cs` (see `generated/.../_Ring.g.cs` for a
product-type reference). Shape:

1. an `int Kind` discriminant (0-based, declaration order);
2. `public const int` tag constants, one per case;
3. the flattened per-case fields, each named `Case_Field`, `[DataMember] public readonly`;
4. one **private** all-fields constructor (named args, so the 10-field *tuple* cap is
   irrelevant — this is not a tuple constructor);
5. one **public static factory per case** that sets its own fields and defaults the
   rest;
6. equality and hashing over `Kind` plus the active case's fields; serialization
   (including `ToString`) describes the tag and flattened fields. See the writer and
   emission tests linked above for the generated API.

### 5.1 LangVersion constraint

Generated code must compile with the **default LangVersion on net8.0 (C# 12)** —
hard rule 3, "no C# 14 features." The existing writer stays conservative: expression-
bodied members, `readonly` fields, `[MethodImpl(AggressiveInlining)]`, ordinary
constructors, `??`-free bodies. The emission below uses only long-stable constructs
(ternary chain, `default`, static factories). It deliberately **avoids** a `switch`
expression: a `switch` over `int Kind` cannot be proven exhaustive by C#, so it would
force a `_ => throw` arm (CS8509) — the ternary chain with an unconditional last arm is
cleaner and is what the match lowering already produces. The old scalar-erased sketch
below is historical: scalar erasure was retired by the
[wrapper-scalar decision](../../tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md).
It must not be used as a generation recipe.

### 5.2 Historical hand-written target sketch for `PathSegment2D`

The following July sketch is preserved to explain the tagged layout. It is **not the
current emission contract**: its native scalar payloads, all-fields equality/hash and
case-name `ToString` are superseded. For executable examples of wrapper payloads,
active-case equality, JSON `ToString`, and generic structs, use the linked C# emission tests.

```csharp
// Autogenerated file: DO NOT EDIT

using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using static System.Runtime.CompilerServices.MethodImplOptions;
using Ara3D.Collections;

namespace Ara3D.Geometry
{
    [DataContract, StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial struct PathSegment2D: IValue
    {
        // Discriminant (0-based, declaration order)
        [DataMember] public readonly int Kind;

        // Case tags
        public const int Kind_Move = 0;
        public const int Kind_Line = 1;
        public const int Kind_Quadratic = 2;
        public const int Kind_Cubic = 3;
        public const int Kind_Arc = 4;
        public const int Kind_Close = 5;

        // Flattened per-case fields (Case_Field). Inactive cases' fields hold default.
        [DataMember] public readonly Point2D Move_EndPoint;
        [DataMember] public readonly Point2D Line_EndPoint;
        [DataMember] public readonly Point2D Quadratic_Control;
        [DataMember] public readonly Point2D Quadratic_EndPoint;
        [DataMember] public readonly Point2D Cubic_Control1;
        [DataMember] public readonly Point2D Cubic_Control2;
        [DataMember] public readonly Point2D Cubic_EndPoint;
        [DataMember] public readonly Vector2 Arc_Radii;
        [DataMember] public readonly Angle   Arc_AxisRotation;
        [DataMember] public readonly bool    Arc_LargeArc;
        [DataMember] public readonly bool    Arc_Sweep;
        [DataMember] public readonly Point2D Arc_EndPoint;

        // All-fields constructor (private: build via the per-case factories)
        [MethodImpl(AggressiveInlining)]
        private PathSegment2D(int kind,
            Point2D move_EndPoint, Point2D line_EndPoint,
            Point2D quadratic_Control, Point2D quadratic_EndPoint,
            Point2D cubic_Control1, Point2D cubic_Control2, Point2D cubic_EndPoint,
            Vector2 arc_Radii, Angle arc_AxisRotation, bool arc_LargeArc, bool arc_Sweep, Point2D arc_EndPoint)
        {
            Kind = kind;
            Move_EndPoint = move_EndPoint; Line_EndPoint = line_EndPoint;
            Quadratic_Control = quadratic_Control; Quadratic_EndPoint = quadratic_EndPoint;
            Cubic_Control1 = cubic_Control1; Cubic_Control2 = cubic_Control2; Cubic_EndPoint = cubic_EndPoint;
            Arc_Radii = arc_Radii; Arc_AxisRotation = arc_AxisRotation;
            Arc_LargeArc = arc_LargeArc; Arc_Sweep = arc_Sweep; Arc_EndPoint = arc_EndPoint;
        }

        // Per-case static factories: set own fields, zero the rest.
        [MethodImpl(AggressiveInlining)] public static PathSegment2D Move(Point2D endPoint)
            => new PathSegment2D(Kind_Move, endPoint, default, default, default, default, default, default, default, default, default, default, default);
        [MethodImpl(AggressiveInlining)] public static PathSegment2D Line(Point2D endPoint)
            => new PathSegment2D(Kind_Line, default, endPoint, default, default, default, default, default, default, default, default, default, default);
        [MethodImpl(AggressiveInlining)] public static PathSegment2D Quadratic(Point2D control, Point2D endPoint)
            => new PathSegment2D(Kind_Quadratic, default, default, control, endPoint, default, default, default, default, default, default, default, default);
        [MethodImpl(AggressiveInlining)] public static PathSegment2D Cubic(Point2D control1, Point2D control2, Point2D endPoint)
            => new PathSegment2D(Kind_Cubic, default, default, default, default, control1, control2, endPoint, default, default, default, default, default);
        [MethodImpl(AggressiveInlining)] public static PathSegment2D Arc(Vector2 radii, Angle axisRotation, bool largeArc, bool sweep, Point2D endPoint)
            => new PathSegment2D(Kind_Arc, default, default, default, default, default, default, default, radii, axisRotation, largeArc, sweep, endPoint);
        [MethodImpl(AggressiveInlining)] public static PathSegment2D Close()
            => new PathSegment2D(Kind_Close, default, default, default, default, default, default, default, default, default, default, default, default);

        // Static default implementation
        public static readonly PathSegment2D Default = default;

        // Case predicates (convenience; optional — see open questions)
        public bool IsMove      { [MethodImpl(AggressiveInlining)] get => Kind == Kind_Move; }
        public bool IsLine      { [MethodImpl(AggressiveInlining)] get => Kind == Kind_Line; }
        public bool IsQuadratic { [MethodImpl(AggressiveInlining)] get => Kind == Kind_Quadratic; }
        public bool IsCubic     { [MethodImpl(AggressiveInlining)] get => Kind == Kind_Cubic; }
        public bool IsArc       { [MethodImpl(AggressiveInlining)] get => Kind == Kind_Arc; }
        public bool IsClose     { [MethodImpl(AggressiveInlining)] get => Kind == Kind_Close; }

        // Object virtual function overrides: Equals, GetHashCode, ToString
        [MethodImpl(AggressiveInlining)] public bool Equals(PathSegment2D other)
            => Kind == other.Kind
            && Move_EndPoint.Equals(other.Move_EndPoint) && Line_EndPoint.Equals(other.Line_EndPoint)
            && Quadratic_Control.Equals(other.Quadratic_Control) && Quadratic_EndPoint.Equals(other.Quadratic_EndPoint)
            && Cubic_Control1.Equals(other.Cubic_Control1) && Cubic_Control2.Equals(other.Cubic_Control2) && Cubic_EndPoint.Equals(other.Cubic_EndPoint)
            && Arc_Radii.Equals(other.Arc_Radii) && Arc_AxisRotation.Equals(other.Arc_AxisRotation)
            && Arc_LargeArc.Equals(other.Arc_LargeArc) && Arc_Sweep.Equals(other.Arc_Sweep) && Arc_EndPoint.Equals(other.Arc_EndPoint);
        [MethodImpl(AggressiveInlining)] public bool NotEquals(PathSegment2D other) => !Equals(other);
        [MethodImpl(AggressiveInlining)] public override bool Equals(object obj) => obj is PathSegment2D other ? Equals(other) : false;
        [MethodImpl(AggressiveInlining)] public static bool operator==(PathSegment2D a, PathSegment2D b) => a.Equals(b);
        [MethodImpl(AggressiveInlining)] public static bool operator!=(PathSegment2D a, PathSegment2D b) => !a.Equals(b);
        [MethodImpl(AggressiveInlining)] public override int GetHashCode()
            => Intrinsics.CombineHashCodes(Kind,
                 Move_EndPoint, Line_EndPoint,
                 Quadratic_Control, Quadratic_EndPoint,
                 Cubic_Control1, Cubic_Control2, Cubic_EndPoint,
                 Arc_Radii, Arc_AxisRotation, Arc_LargeArc, Arc_Sweep, Arc_EndPoint);
        [MethodImpl(AggressiveInlining)] public override string ToString()
            => Kind == Kind_Move       ? $"Move({Move_EndPoint})"
             : Kind == Kind_Line       ? $"Line({Line_EndPoint})"
             : Kind == Kind_Quadratic  ? $"Quadratic({Quadratic_Control}, {Quadratic_EndPoint})"
             : Kind == Kind_Cubic      ? $"Cubic({Cubic_Control1}, {Cubic_Control2}, {Cubic_EndPoint})"
             : Kind == Kind_Arc        ? $"Arc({Arc_Radii}, {Arc_AxisRotation}, {Arc_LargeArc}, {Arc_Sweep}, {Arc_EndPoint})"
             :                           "Close";
    }
    // Extension methods for the type
    public static class PathSegment2DExtensions
    {
    }
}
```

### 5.3 Inactive fields and equality

Factories still default inactive payload fields, but equality and hashing must not
read them. An inactive array field has a null C# default and comparing it previously
threw. [plato-458](../../tracker/issues/plato-458.md) records the correction; the
`SumEquality_ComparesOnlyTheActiveCase` test in the linked C# emission suite pins it.
The all-fields implementation in the historical sketch is not the sum equality contract.

### 5.4 Match lowering in C#

The elaboration of §4.2 reaches the C# body writer as an ordinary conditional/let tree
and renders as a nested ternary. For example:

```plato
EndPoint(seg: PathSegment2D, start: Point2D): Point2D =>
    match (seg) {
        Move(p) => p; Line(p) => p; Quadratic(c, p) => p;
        Cubic(c1, c2, p) => p; Arc(r, rot, large, sweep, p) => p; Close => start;
    };
```

has the following schematic C# meaning (the writer uses generated `IsCase()`
predicates rather than spelling these tag comparisons directly):

```csharp
public static Point2D EndPoint(this PathSegment2D seg, Point2D start)
    => seg.Kind == 0 ? seg.Move_EndPoint
     : seg.Kind == 1 ? seg.Line_EndPoint
     : seg.Kind == 2 ? seg.Quadratic_EndPoint
     : seg.Kind == 3 ? seg.Cubic_EndPoint
     : seg.Kind == 4 ? seg.Arc_EndPoint
     :                 start;   // Close
```

No `default`/`throw` fall-through is generated because the match is exhaustive and the
final case is the `else`. If a binder is used more than once, the projection is repeated
(a cheap, pure field read) or the writer may bind a local — either is correct.

---

## 6. Historical diagnostics catalog

The July proposal allocated the following codes and suggested message text.
[SEMANTICS.md](../SEMANTICS.md#7-match), the sum checker, and the negative fixtures
are the authorities for implemented diagnostics; this table is not a promise that
every proposed backend or parser diagnostic landed with this exact text. CHK306 is
restricted-host behavior only, not the default generics policy.

| Code   | When | Proposed message |
|--------|------|------------------|
| CHK300 | match omits ≥ 1 case | `non-exhaustive match on sum type '<Sum>': missing case(s) '<C1>', '<C2>', … — add an arm for each (v1 has no default arm)` |
| CHK301 | arm names a non-case | `unknown case '<Name>' in match on sum type '<Sum>' (cases: <C1>, <C2>, …)` |
| CHK302 | case matched twice | `duplicate match arm for case '<Case>' of sum type '<Sum>'` |
| CHK303 | binder count ≠ field count | `case '<Case>' of sum type '<Sum>' binds <N> field(s) (<F1>, <F2>, …) but the pattern supplies <M>` |
| CHK304 | subject not a sum | `cannot match on value of type '<Type>': match requires a sum type` |
| CHK305 | duplicate case name in decl | `sum type '<Sum>' declares case '<Case>' more than once` |
| CHK306 | generic sum, stance = restricted | `generic sum type '<Sum><…>' is not supported in v1; declare a monomorphic sum or await the generics follow-up` |
| CHK307 | default/wildcard arm written | `default match arms are not supported in v1; list every case of '<Sum>' explicitly` |

The proposal also reserved a backend rejection diagnostic. See SEMANTICS.md for
the supported targets and the TypeScript/Rust rejection behavior:

| Code   | When | Proposed message |
|--------|------|------------------|
| CHK320 | sum type reaches GLSL/TS/Rust writer | `sum type '<Sum>' cannot be emitted to the <target> target; sum types are C#-only in v1` |

Each negative corpus fixture pins exactly one of CHK300–CHK305 (see
`plato-test-sum/README.md`).

---

## 7. Original scope boundaries

These were the July scope boundaries, with qualifications where source evidence
matters. Consult SEMANTICS.md before treating a historical deferral as a language rule:

- **GLSL / TypeScript / Rust emission.** The original target was C# only; see
  SEMANTICS.md for backend support rather than inferring it from this proposal.
- **Nested patterns.** `Some(Circle(r))` — only one level of case pattern; bind then
  `match` again (see `nested-match.plato`).
- **Guards.** No `when`/`if` on arms.
- **Default / wildcard arm.** Exhaustive by enumeration only (CHK307).
- **Recursive sum types.** A case field of the sum's own type
  (`type Tree = Leaf(Value: Integer) | Branch(Left: Tree, Right: Tree);`) would make
  the flat C# value struct contain itself. The writer emits payload fields directly
  and adds no automatic reference indirection. Thus the expected C# limitation is a
  CS0523 layout cycle; this is a representation inference, not a freshly executed
  recursive-program test or a verified front-end diagnostic. The sum checker has no
  dedicated recursive-layout check. `Array<Tree>` or integer IDs can break the layout
  cycle in principle, but the existing array-payload and nested-sum fixtures do not
  establish an end-to-end recursive-tree guarantee. Validate a concrete recursive
  program on its intended backend before relying on it.
- **Interaction with the `unique` affine modifier.** This was unspecified in the
  proposal; see [SEMANTICS.md](../SEMANTICS.md#8-not-in-the-language) for the restriction
  to intrinsic builders.
- **Payload-sharing / common fields across cases.** No "every case also has field X";
  model it as an outer product wrapping the sum (e.g. `FillStyle { Paint: Paint; Opacity }`).
- **Exhaustiveness across `Boolean`/enum-of-two as a sum.** `Boolean` stays a primitive;
  it is not retrofitted into the sum machinery.

---

## 8. Historical implementation questions

These questions record choices considered in July, not an outstanding work queue.
Generic support and C# representation were settled by the October 23 decision;
case predicates are emitted and used for matching; `ToString` uses serialization.
Use the authorities linked above before acting on any recommendation here.

1. **Bare vs qualified constructors (§4.4).** Ship qualified `TypeName.Case(args)` for
   certain; add bare `Case(args)` only if the resolver disambiguates it for free.
   *Recommendation: qualified-only in the first landing.*
2. **Generics (§3.5).** Default is supported (monomorphized). Confirm the monomorphizer
   keys sums per type argument with no unifier change; if not, fall back to CHK306 and
   ship monomorphic-only. `option.plato` is the decider.
3. **Binder projection: inline vs let.** Inlining field projections is simplest and
   matches the pure-value model; a `let` local avoids repeated reads when a binder is
   used many times. Either is correct; pick per the body writer's existing habits.
4. **`ToString` shape.** The example emits a tag-aware, per-case `ToString`
   (`"Quadratic(…, …)"`). A simpler all-fields dump (matching the product writer) is
   less writer work but less readable. *Recommendation: tag-aware, as shown.*
5. **Case predicates (`IsMove`, …).** Shown as a convenience. Drop them if they add
   surface without a consumer; the tag constants alone suffice for the lowering.
6. **`match` keyword reservation.** Reserved word vs contextual keyword — reserved is
   simplest and collides with nothing in the current stdlib.
7. **Field ordering in the flattened struct.** The example flattens in case-then-field
   declaration order. Any stable order works for correctness; declaration order is the
   least surprising and keeps the `[DataContract]` serialization deterministic.
8. **`unique`/affine interaction.** Out of scope, but decide early whether a sum *case
   field* may be `unique`, since it affects the struct's move semantics if that modifier
   lands.

---

## 9. Companion: v3 migration survey

The historical stdlib survey — kind-pattern types classified pure-enum vs true-sum,
the recommended wave-3 migrations, and the drafted "after" of
`40-paths.plato` — is in [`reports/plato-sum-types-v3-survey.md`](../reports/plato-sum-types-v3-survey.md).
Headline: the large majority are degenerate enums (a `= A | B | …` collapse); a minority
are true sums (a `Kind` discriminant plus conditionally-meaningful carrier fields) and
those are the migration value.
