---
id: plato-458
title: C# writer's sum-type Equals compares inactive cases' fields
type: bug
status: ready
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-10-17
closed:
links: []
---

## Problem

A sum type is emitted as one struct holding every case's fields. Its `Equals` compares all of them, so comparing two values of a case whose sibling case has an array field dereferences a null array: `Curves[0].Equals(Curves[0])` on a `Line` throws NullReferenceException, and so do the STEP library's `SameModel` and `Law_UnitBoxIsBoxOfOnes`. Two values of the same case that differ only in an inactive field's default would also compare wrongly.

## Done means

- [ ] Sum `Equals` compares `Kind`, then only the active case's fields; `GetHashCode` agrees.
- [ ] A test compares two values of a case whose sibling case has an array field.
