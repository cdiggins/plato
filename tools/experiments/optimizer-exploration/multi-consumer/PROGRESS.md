compiler-506: isolated multiple-consumer experiment; owner multi_consumer.
Read optimizer handbook, BREP plan, runtime Reduce, lowerer/emitter; shared compiler unchanged.
Prototype compares real Number wrapper folds, preserves per-accumulator order.
Counterexamples cover callback fault ordering, stateful host source, reassociation.
Verified: recorded net8 Release build; 209 bitwise cases; three unsafe counterexamples.
Warmed paired kernels show traversal benefit, zero allocation savings; see dated report.
Design/review/ordered plan written. Next: read-only TIR candidate census; production unimplemented.
Supervisor owns tracker, integration and commits.
