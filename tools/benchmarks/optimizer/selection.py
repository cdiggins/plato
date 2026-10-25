"""Deterministic bounded coverage selection, with a reserved holdout."""
import hashlib
import math


def _dimensions(case):
    return {"tag:" + tag for tag in case.get("tags", [])} | {
        "workload:" + workload for workload in case.get("workloads", [])}


def select_cases(suite, *, mode="quick", max_cases=6, budget_ms=None, discovery=None,
                 affected=None, holdout_fraction=0.2, seed="plato-optimizer-v1"):
    """Select fixtures, never claim held-out predictive validity.

    discovery maps case ids to positive estimated total runtime in milliseconds.
    affected is a collection of case ids or workload names for confirmation.
    Greedy selection maximizes inverse-frequency coverage gain per runtime cost.
    Tags should include family and scale dimensions provided by preparation.
    """
    if mode not in ("quick", "confirm", "corpus"):
        raise ValueError("mode must be quick, confirm or corpus")
    if not isinstance(max_cases, int) or isinstance(max_cases, bool) or max_cases < 1:
        raise ValueError("max_cases must be a positive integer")
    if not 0 <= holdout_fraction < 1:
        raise ValueError("holdout_fraction must be in [0,1)")
    if budget_ms is not None and (not math.isfinite(budget_ms) or budget_ms <= 0):
        raise ValueError("budget_ms must be positive and finite")
    cases = list(suite.get("cases", []))
    ids = [c["id"] for c in cases]
    if budget_ms is not None and any(i not in (discovery or {}) for i in ids):
        raise ValueError("runtime budget requires discovery costs for every case")
    if len(set(ids)) != len(ids):
        raise ValueError("duplicate case id")
    for c in cases:
        if not isinstance(c["id"], str) or not c["id"]:
            raise ValueError("case id must be nonempty")
        for key in ("tags", "workloads"):
            if not isinstance(c.get(key), list) or any(not isinstance(v, str) or not v for v in c[key]):
                raise ValueError(f"invalid {key}")
    costs = {c["id"]: (discovery or {}).get(c["id"], 1.0) for c in cases}
    if any(not isinstance(v, (int, float)) or isinstance(v, bool) or not math.isfinite(v) or v <= 0
           for v in costs.values()):
        raise ValueError("discovery costs must be positive and finite")
    order = sorted(cases, key=lambda c: (hashlib.sha256((seed+":"+c["id"]).encode()).hexdigest(), c["id"]))
    reserve_count = min(len(cases)-1, max(1, int(len(cases)*holdout_fraction))) if len(cases)>1 and holdout_fraction else 0
    holdout = order[:reserve_count]
    candidates = order[reserve_count:]
    all_dimensions = set().union(*(_dimensions(c) for c in cases)) if cases else set()
    selected, covered, spent = [], set(), 0.0
    if mode == "corpus":
        selected = sorted(cases, key=lambda c: c["id"])
        covered = all_dimensions.copy()
        spent = sum(costs.values())
    else:
        if mode == "confirm":
            if not affected:
                raise ValueError("confirm requires affected case ids or workloads")
            affected = set(affected)
            # Confirmation may revisit a reserved case explicitly reported affected.
            candidates = [{**c, "workloads": list(c["workloads"]) if c["id"] in affected else
                            [w for w in c["workloads"] if w in affected]}
                          for c in cases if c["id"] in affected or affected.intersection(c["workloads"])]
        frequencies = {d: sum(d in _dimensions(c) for c in cases) for d in all_dimensions}
        while candidates and len(selected) < max_cases:
            affordable = [c for c in candidates if budget_ms is None or spent+costs[c["id"]] <= budget_ms]
            if not affordable:
                break
            def score(c):
                gain = sum(1/frequencies[d] for d in _dimensions(c)-covered)
                return (-gain/costs[c["id"]], c["id"])
            best = min(affordable, key=score)
            if mode == "quick" and not (_dimensions(best)-covered):
                break
            selected.append(best)
            candidates.remove(best)
            covered.update(_dimensions(best))
            spent += costs[best["id"]]
    return dict(schema=1, mode=mode, cases=selected, holdout=[c["id"] for c in holdout],
                covered_dimensions=sorted(covered), uncovered_dimensions=sorted(all_dimensions-covered),
                estimated_ms=spent if discovery and all(i in discovery for i in ids) else None,
                cost_basis="discovery_ms" if discovery else "uniform_coverage_cost", estimated_costs=bool(discovery),
                prediction_validated=False,
                limitation="Selection covers supplied tags and workloads; corpus prediction requires independent held-out measurements.")
