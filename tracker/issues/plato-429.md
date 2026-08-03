---
id: plato-429
title: TypeScript writer emits calls to array-receiver library functions it never defines
type: bug
status: ready
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-03
closed:
links: [plato-419, plato-425]
---

## What and why

**This is `plato-419` defect 1, split out.** That issue is an umbrella over
thirteen TypeScript-writer defects and says in its own words to split when someone
starts work; this is the split, with the evidence a fixer needs (the exact emitted
preamble, the empty placeholder section) rather than a line in a list. Fix it here;
tick defect 1 there. The remaining twelve stay in the umbrella.

The TypeScript writer emits **calls** to library functions whose first parameter is
an `Array<T>`, but never emits a **definition** for any of them, so the generated
`plato.g.ts` references methods that do not exist on `IArray<T>`.

The runtime preamble the writer emits declares only four array members:

```ts
export interface IArray<T>
{
    At(n: number): T;
    Count(): number;
    Map<TR>(f: (x: T) => TR): IArray<TR>;
    Reduce<TAcc>(init: TAcc, f: (acc: TAcc, x: T) => TAcc): TAcc;
}
```

plus `MapRange` installed on `Number.prototype`. Everything else in the array
surface — `FlatMap`, `Concatenate`, `Append`, `Prepend`, `Slice`, `Repeat`,
`EveryNth`, `All`, `Any`, `AtModulo`, and every user-defined function with an array
first parameter — is emitted as a call site and nowhere as a definition. The
writer even emits a placeholder comment, `// Array functions over concrete element
types`, with nothing under it.

This is not new and is not confined to one tier. The **shipped** SDF demo's checked-in
bindings, `demos/typescript/sdf/src/plato/plato.g.ts`, call `.FlatMap(...)` in
several places against exactly that four-member `IArray<T>`.

## Impact

Any browser demo that drives Plato code touching the array surface must hand-write a
shim before the generated file will type-check or run. Each missing function is a
one-liner over `Map` / `Reduce` / `MapRange`, so the shim is mechanical — but it is
invisible until a demo fails, it must be rewritten per demo, and it silently drifts
from the Plato definitions it is standing in for.

It also blocks the plato-425 rigid-body solver from being driven from TypeScript
without one: `ReplacedAt` (the array-with-one-element-changed primitive the
sequential-impulse fold is built on), `Flatten`, `WarmStartFrom`, `BallSceneManifolds`
and the joint passes are all array-receiver functions.

## Fix approaches

1. **Emit the monomorphized array functions.** The writer already knows the concrete
   element types a generic array function is instantiated at — that is what the
   empty "Array functions over concrete element types" section was for. Emitting
   them as `Arr<T>` prototype installs, the way `MapRange` is installed on
   `Number.prototype`, matches the existing shape.
2. **Widen the emitted `IArray<T>` / `Arr<T>` preamble** to the full derivable array
   surface, hand-written once in the writer's preamble rather than derived per
   compilation. Cheaper, and correct for the foundation functions; it does not help
   a user-defined array function.
3. Check whether the Rust / GLSL / C++ writers have the same hole before fixing only
   TypeScript.

## Done means

- [ ] A generated `plato.g.ts` over all four tiers type-checks against its own preamble
- [ ] `FlatMap` / `Concatenate` / `Append` and a user-defined array function all resolve
- [ ] The other POC writers checked for the same gap
