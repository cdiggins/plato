Contract R1 acknowledged; parallel-wave and platonic-coder read and applied.
State: verified; analysis and selection APIs sent to supervisor.
Owned: analysis.py, selection.py, test_analysis.py, test_selection.py, this checkpoint.
Analysis uses per-invocation normalization; zero allocation allowed; checks require true values.
Checks: python -B -m unittest discover -s tools/benchmarks/optimizer -p test_analysis.py -v (11 pass).
Checks: python -B -m unittest discover -s tools/benchmarks/optimizer -p test_selection.py -v (5 pass).
Outstanding: serialized commit turn; supervisor integration verification.
No builds, background processes, external outputs, or shared tracker changes.
