---
id: compiler-541
title: Reuse conservative call sharing in native TIR pipeline
type: feature
status: in-progress
priority: p1
effort: S
risk: med
area: compiler
sprint: 
created: 2026-11-01
closed:
links: []
---

CppCpuOptions.ShareCalls now selects the existing conservative C# TirSharedCalls pass in the native pipeline, in the same relative order before loop lowering. API-only opt-in; CLI/shipping defaults remain unchanged pending corpus evidence. Four focused tests pass: both pipelines reduce repeated Cost calls from 2 to 1, retain 2 branch-only calls and 2 differing-argument calls, and generated native sharing laws compile and execute. The fixture uses a statement-bodied equivalent computation so prior inlining cannot erase the calls under test. Recorded Release build: zero errors/warnings. Wider sharing, CLI enablement, refusal telemetry and BREP performance validation remain outstanding; shared algorithm code was not duplicated or changed.
