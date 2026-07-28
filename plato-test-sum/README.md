# plato-test-sum — sum types + matching test corpus

Small, self-contained `.plato` fixtures for the sum-type / exhaustive-`match`
feature (tracker **plato-232**, design doc
[`docs/plato-sum-types-design-2026-07-27.md`](../docs/plato-sum-types-design-2026-07-27.md)).
These are **fixtures, not stdlib** — never merge into `stdlib-legacy`, `plato-src-v2`,
or `stdlib`. They exercise syntax the current front end does not yet accept;
the wave-2 parser/AST implementer wires them into the test suite as the feature lands.

## Layout

```
plato-test-sum/
  pathsegment.plato        flagship sum (SVG path verbs) + match functions
  fillrule.plato           degenerate sum (enum, payload-free cases)
  option.plato             generic sum (Option<T>) — also the generics-stance fixture
  match-expression.plato   match used in expression position (call arg, nested)
  nested-match.plato       match inside a match arm (non-recursive)
  negatives/               one file per diagnostic, each with an EXPECT line
    missing-case.plato
    unknown-case.plato
    duplicate-case.plato
    wrong-binder-arity.plato
    match-non-sum.plato
    duplicate-case-names.plato
```

## Conventions

- **Self-contained.** Each file declares the minimal primitives it needs (`type
  Number { }`, `type Point2D { X: Number; Y: Number; }`, …), mirroring how
  `stdlib/00-primitives.plato` self-declares. `implements`/`concept`
  clauses are intentionally omitted — they are orthogonal to sum types and would
  drag in the concept lattice. The design doc shows the faithful `implements Value`
  form; the fixtures stay minimal.
- **EXPECT convention (negatives only).** The first line of every negative fixture
  is a comment of the form:

  ```
  // EXPECT: <CODE> <human-readable diagnostic text>
  ```

  The test harness compiles the file and asserts that a diagnostic with that code
  is raised (matching on `<CODE>` is the stable contract; the prose is illustrative
  and may be reworded — see the diagnostics catalog in the design doc). Exactly one
  diagnostic is expected per negative file, and each file isolates a single failure
  mode. Positive fixtures carry no EXPECT line and must compile clean once the
  feature lands.
- **One concern per file.** Positive fixtures each demonstrate one capability;
  negatives each trip one diagnostic. Keep them short.

## Diagnostics referenced

| Code   | Fixture                        | Meaning                                            |
|--------|--------------------------------|----------------------------------------------------|
| CHK300 | negatives/missing-case         | non-exhaustive match (missing case[s])             |
| CHK301 | negatives/unknown-case         | match arm names a case the sum type lacks          |
| CHK302 | negatives/duplicate-case       | same case matched twice in one match               |
| CHK303 | negatives/wrong-binder-arity   | binder count ≠ case field count                    |
| CHK304 | negatives/match-non-sum        | match subject is not a sum type                    |
| CHK305 | negatives/duplicate-case-names | sum declares the same case name twice              |
| CHK306 | option.plato (if v1 restricts) | generic sum type unsupported in v1 (stance-gated)  |

Codes are proposed; final numbers are the implementer's call (the checker's CHK2xx
block is the family). See the design doc's diagnostics catalog for exact texts.
