# Playground feasibility — 2026-10-31

Question: can the reference site offer a browser playground that runs Plato programs, and what
is the cheapest sound way to do it? The site is static (`tools/reference-reader/`, fed by
`tools/reference/build.py`), and [the site guidance](../design/reference-site-guidance.md) asks
for a Playground door and a site that is "fast and static, no login, works offline".

Evidence labels: **ran** (observed in the spike below), **source** (read in the code),
**docs** (repository or vendor documentation), **inferred** (reasoned from the others; the basis
is stated).

## Recommendation

1. **Now:** ship the Playground door as editable examples with precomputed, build-checked
   output (option 3). Share links carry the source in the URL fragment. Once a reader edits an
   example, the page labels the shown output as belonging to the original. This keeps the site
   static and offline. It reuses the fixture runner that already checks examples
   (`tools/reference/check_examples.py`).
2. **Next:** make in-browser execution with .NET WebAssembly (option 1) the path to a real
   playground. The spike shows that the real parser, type checker, and interpreter build for
   `browser-wasm` without any workload. They ran a small program and its `Law_*` functions
   end to end. Before it can be the default, two measured blockers have to be fixed: stack
   overflow on deep stdlib files, and slow parsing under the Mono interpreter.
3. **Do not** build an execution server (option 2). It breaks the static, offline, no-tracking
   characteristic, and its only gain is speed, which AOT may also deliver. A TypeScript writer
   path (option 4) does not avoid shipping the compiler, and its semantics diverge.

Confidence: **medium**. Feasibility of option 1 is high (it ran). How fast a foundation-sized
compile can be made is the open number: the throughput figure below was taken on a contended
machine, and AOT was not measured because the `wasm-tools` workload is not installed.

## The spike

Scratch folder: `C:/Users/cdigg/AppData/Local/Temp/claude/C--Users-cdigg-git-plato/258e19d8-ec88-48a4-82fe-d854169c45e9/scratchpad/spike-s/`
(thrown away). It contains copies of `src/Plato.AST`, `src/Plato.Compiler`,
`src/Plato.Interpreter`, `parakeet/{Parakeet,Parakeet.Cst,Parakeet.Grammars}`, and the two
source files of `parakeet/Parakeet.Parsers`, taken from the clean checkout at `cfdf669d`. A
`Microsoft.NET.Sdk.WebAssembly` project (net10.0, SDK 10.0.400) embeds the `.plato` inputs as
resources, parses them, builds a `Compilation`, and calls functions through `Interpreter.Call`.
It was published with `dotnet publish -c Release` and run under Node 22.13.1 via the published
`dotnet.js`. A desktop (`Microsoft.NET.Sdk`) project with the same source was the control.
Installed workloads: `android`, `ios`, `maccatalyst`, `maui-windows`; no `wasm-tools`.

Other agents were building on the same machine throughout, so every timing below is an
upper bound and only the order of magnitude should be relied on.

| Run | Result |
|---|---|
| Publish, no workload | Succeeds. Prints "Publishing without optimizations… we strongly recommend using `wasm-tools`". |
| Download, trimmed, foundation source embedded | 6.2 MB uncompressed; 1.9 MB as the published `.br` files. The embedded foundation source is about 0.12 MB of that (brotli). |
| Runtime start under Node | 0.1–0.25 s |
| Fixture `functions` + reference prelude (2 files, 1,303 chars) | Parse 1.9 s, compile 3.5 s (29 functions), 3 laws evaluated True in 3.2 s; 8.7 s total. The native `Plato.CLI run … --laws` on the same files reports 0.5 s. |
| Foundation (59 files) + one user file | Parsed 27 files (234 KB) in about 150 s, then a fatal `StackOverflowException` on `numbers.library.plato` |
| Desktop control, foundation + user file | Parse 2.8 s, compile 2.2 s, call 0.13 s (net10, JIT) |
| Native CLI, foundation + user file, warm | `compiled 59 files, 3106 functions (3.8s)` |

## Options

### 1. Compiler and interpreter in the browser with .NET WebAssembly

What works:

- **The input does not have to come from a file system.** `Compilation` takes `IEnumerable<AstNode>`,
  not paths [source: `src/Plato.Compiler/Compilation.cs:24`]. Folder enumeration lives only in
  the CLI (`SourceFiles.cs`, `LawProgram.SourceFiles`). The spike fed embedded resources
  instead [ran].
- **Dependencies are portable.** The interpreter references only `Plato.Compiler` and
  `Ara3D.SDK.Core` [source: `src/Plato.Interpreter/Plato.Interpreter.csproj`]. Roslyn sits only in
  `Plato.Laws`, which the C# law backend uses and the interpreter path does not. Nothing
  Windows-only was pulled in, and the trimmed output contains small Ara3D assemblies only [ran].
- **No threads are needed.** With the interpreter backend, the law runner evaluates inline
  (`SupportsParallel => false`) and starts no thread [source:
  `src/Plato.Interpreter/Verification/LawBackend.cs:49`, `LawRunner.cs:260`]. The interpreter has
  a `Deadline` and a `MaxDepth`, which a page can set to bound a run [source:
  `Interpreter.cs:29,33`].
- **Laws run in the browser.** In the fixture run, three `Law_*` functions evaluated True under wasm [ran].
- **Startup and size are acceptable.** About 2 MB compressed is in line with a documentation
  site that loads the runtime only when Run is pressed [ran; "acceptable" is inferred].

Blockers, ranked:

1. **Throughput.** Parsing under the wasm Mono interpreter ran at roughly 1.5 KB/s, against
   about 210 KB/s on the desktop JIT: roughly two orders of magnitude slower [ran, contended].
   At that rate a program that compiles with all of `stdlib/foundation` (600 KB of source)
   would take minutes before it runs. Possible fixes, none measured here:
   - AOT compilation needs `wasm-tools`. Its speed-up and size cost are the one measurement
     that would settle this option.
   - A smaller prelude per example. Most reference fixtures already compile against a 612-byte
     prelude or against foundation, and only one uses `stdlib/geometry` [source:
     `docs/reference/implementation/examples/*/inputs.txt`].
   - Parsing the stdlib once per page load, or at build time, and keeping the trees in
     memory. No serialised form of a `Compilation` or its ASTs exists today [source;
     inferred from the absence of one].
2. **Stack depth.** CST construction overflows the stack on the deepest foundation file
   (parse-tree depth 83, the largest in foundation; the median is 15) [ran; depth measured
   on the desktop control]:
   - The generated `CstNodeFactory.Create` is a 126-case string switch that recurses through
     LINQ. Replacing the LINQ with a loop did not stop the overflow [ran]. The file is
     generated by parakeet's `CstCodeBuilder` [source:
     `parakeet/Parakeet.Cst/PlatoGrammarCstFactory.cs`, "DO NOT EDIT"].
   - A larger V8 stack (`node --stack-size`) made no difference, so the limit belongs to the
     .NET wasm runtime [ran]. Raising it needs a native relink, which needs `wasm-tools`
     [inferred from the runtime pack shipping static `.a` libraries].
   - The desktop parser is also stack-hungry: nested parentheses overflow its default 1 MB
     main-thread stack between depth 150 and 175 [ran].
   - The CLI runs the interpreter on a 512 MB thread, and the interpreter allows 20,000
     activations [source: `RunCommand.cs`, `Interpreter.cs:29`]. A browser without the
     multithreaded runtime cannot start that thread (CA1416 warning at build) [ran]. Deep
     Plato recursion will therefore need a lower `MaxDepth`, and probably a less recursive
     evaluator.
3. **Trimming.** Parakeet resolves recursive grammar rules by name through
   `GetType().GetProperties()` and hashes rules with `GetMethodBody()` [source:
   `parakeet/Parakeet/Grammar.cs:21`, `Rule.cs:204-234`]. With default trimming, the grammar
   exposed 96 properties instead of 195 and parsing failed with a null reference [ran].
   Listing the three Parakeet assemblies as `TrimmerRootAssembly` fixed it [ran].
4. **Build layout.** `Ara3D.Parakeet.Parsers.csproj` references `../../../../ara3d-sdk` as a
   sibling checkout rather than as a package [source]. The spike compiled its two source files
   directly instead [ran]. A site build would need the same workaround, or a fix in parakeet.

### 2. A small server that runs the CLI on request

- **It works today.** `Plato.CLI run <folders> --call=<Name>` / `--laws` already does the
  job [source; ran natively]. A warm process compiles foundation in about 4 s [ran]. A
  long-lived process that keeps the stdlib trees parsed would be faster [inferred from
  `WarmLawRunner` in `src/Plato.Navigation`].
- **The attack surface is mostly denial of service.** The interpreter's host intrinsics are
  arithmetic and collection operations with no file, process, or network access [source:
  `src/Plato.Interpreter/Evaluation/HostIntrinsics.cs`]. The risk is CPU, memory, and stack
  exhaustion, plus crashes in the parser or compiler on hostile input. The server would need
  a process per request or a recycled worker, the existing `--budget`, a memory cap, rate
  limits, and a container [inferred].
- **Cost.** One small always-on container or VM, plus someone to patch it and watch abuse
  [inferred; prices not checked].
- **It breaks the stated characteristics.** It needs a network, so it does not work offline.
  It makes the site no longer static. Request logs are tracking unless deliberately
  discarded [inferred from the guidance text].

### 3. No execution: editable examples with precomputed, build-checked output

- **The machinery exists.** `tools/reference/check_examples.py` runs every fixture group
  through `Plato.CLI run … --laws` and keeps the raw evidence [source]. The guidance already
  requires a build check for every runnable example [docs: reference-site-guidance.md,
  "Constraints"].
- **What it gives.** The original program, its checked output, a copy button, the command to
  run it locally, and a share link with the source in the URL fragment (not the query
  string, so it never reaches a server log) [inferred].
- **The limit.** An edited program cannot be run. The page must say so plainly, or it breaks
  the guidance's "honest about status" rule [docs].

### 4. Other paths

- **TypeScript writer.** It is real: about 2,600 lines in `writers/Plato.TypeScriptWriter`,
  with browser demos under `demos/typescript/` [source]. It does not help a playground:
  - It runs after the compiler, so user code still needs the compiler in the page, which is
    the hard part [source: the writer consumes a `Compilation`].
  - Its semantics diverge: `Number` is 64-bit where SEMANTICS.md says 32 (plato-449),
    overloads are dropped silently (plato-441), and mesh, polygon, and CSG paths do not run
    (plato-419) [source: tracker issues].
  - The reference contract says not to use non-C# writers as semantic evidence [source:
    `tools/reference/CONTRACT.md`].
  - A narrow use remains: interactive widgets over fixed, precompiled stdlib functions
    [inferred].
- **A TypeScript reimplementation of the parser and interpreter.** This would be a second
  implementation of the language, and keeping its semantics in step with the C# one is the
  failure the TypeScript writer already shows. Rejected [inferred].
- **AOT wasm.** This is option 1 with `wasm-tools`. It is the most likely fix for blocker 1,
  and native relinking also allows a larger stack for blocker 2 [inferred; not measured].

## What would change the answer

- **An AOT build compiles foundation plus a user file in a few seconds in a browser, with
  stack room for the deepest stdlib file.** Option 1 becomes the playground, with option 3
  as the no-JavaScript fallback. Settling this needs `dotnet workload install wasm-tools`
  and a rerun of this spike on an idle machine.
- **AOT is still too slow and the parser cannot be made less recursive without reworking
  parakeet.** Option 3 stays the playground. Option 1 shrinks to small programs over a
  minimal prelude, which the fixture run shows already works.
- **The project accepts a hosted service.** Option 2 becomes viable quickly. The guidance's
  "fast and static, no login, works offline" line would have to change first.

## Not checked

- Real browsers: Chrome, Firefox, Safari, and a Web Worker (only Node was used).
- Jiterpreter settings.
- The interpreter's deepest-recursion limit under wasm (the foundation run never got past
  parsing).
- Memory use.
- Hosting prices.
- Whether `Compilation` could be cached or serialised.

## Next

On an idle machine with `wasm-tools` installed, rebuild the spike with
`<RunAOTCompilation>true</RunAOTCompilation>` and a raised `EmccStackSize`. Record the parse,
compile, and run times for the fixture program and for foundation, and the compressed
download size.
