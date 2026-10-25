"""Correctness-gated, process-block paired optimizer comparisons.

Ratios are variant/control elapsed time per invocation: below one is faster.
The interval is a Student t interval on paired log ratios, assuming independent
process blocks. Fewer than min_blocks never receive an effect classification.
"""
import math
import statistics
from collections import defaultdict


def _number(value, positive=False):
    return (isinstance(value, (int, float)) and not isinstance(value, bool)
            and math.isfinite(value) and (value > 0 if positive else value >= 0))


def _invalid(row):
    if row.get("status") != "ok":
        return row.get("status", "error") + ": " + str(row.get("error", "unsuccessful run"))
    if row.get("schema") != 1:
        return "unsupported schema"
    if not _number(row.get("elapsed_ns"), True):
        return "invalid elapsed_ns"
    iterations = row.get("iterations")
    if not isinstance(iterations, int) or isinstance(iterations, bool) or iterations <= 0:
        return "invalid iterations"
    if not _number(row.get("allocated_bytes")):
        return "invalid allocated_bytes"
    if not isinstance(row.get("digest"), str) or not row["digest"]:
        return "missing digest"
    checks = row.get("checks")
    if not isinstance(checks, dict) or not checks or any(v is not True for v in checks.values()):
        return "failed or missing correctness checks"
    return None


def _geomean(values):
    return math.exp(statistics.mean(math.log(v) for v in values))


def _interval(logs):
    # Conservative 95% two-sided critical values (df capped down to table key).
    critical = {1: 12.7063, 2: 4.3027, 3: 3.1825, 4: 2.7765, 5: 2.5706,
                6: 2.4469, 7: 2.3646, 8: 2.3061, 9: 2.2622, 10: 2.2282,
                15: 2.1315, 20: 2.0860, 30: 2.0423, 60: 2.0003, 120: 1.9799}
    df = len(logs) - 1
    t = critical[max(k for k in critical if k <= df)]
    mean = statistics.mean(logs)
    margin = t * statistics.stdev(logs) / math.sqrt(len(logs))
    try:
        return [math.exp(mean - margin), math.exp(mean + margin)]
    except OverflowError:
        return None


def analyze(rows, variants=None, *, baseline="full", practical_threshold=0.03, min_blocks=5):
    """Analyze R1 raw rows. Invalid/missing blocks invalidate that comparison.

    Optional row family identifies the coverage family; model cases use
    workload='model' or kind='model'. Aggregates balance families and exclude
    model totals, preventing stage/model double counting.
    """
    if not 0 < practical_threshold < 1 or min_blocks < 3:
        raise ValueError("threshold must be between zero and one; min_blocks >= 3")
    rows = list(rows)
    names = list(dict.fromkeys(v["name"] if isinstance(v, dict) else v
                               for v in (variants or [])))
    groups = defaultdict(list)
    malformed = []
    cases = set()
    for row in rows:
        if (not isinstance(row, dict) or any(not isinstance(row.get(k), str) or not row[k]
                                           for k in ("variant", "case_id", "workload"))
                or not isinstance(row.get("block"), (str, int)) or isinstance(row.get("block"), bool)):
            malformed.append({"reason": "missing or malformed pairing identity", "row": repr(row)})
            continue
        if row["variant"] not in names:
            names.append(row["variant"])
        cases.add((row["case_id"], row["workload"]))
        groups[(row["variant"], row["case_id"], row["workload"], row["block"])].append(row)
    comparisons = []
    control_names = list(dict.fromkeys((baseline, "shipping")))
    for control in control_names:
        for variant in names:
            if variant == control:
                continue
            for case_id, workload in sorted(cases):
                blocks = sorted({key[3] for key in groups if key[:3] in
                                 ((control, case_id, workload), (variant, case_id, workload))}, key=str)
                result = dict(variant=variant, control=control, case_id=case_id,
                              workload=workload, status="unmeasured", blocks=0,
                              kind="model" if workload == "model" else "stage")
                problems, ratios, allocations, controls, samples = [], [], [], [], []
                for block in blocks:
                    pair = []
                    for name in (control, variant):
                        found = groups.get((name, case_id, workload, block), [])
                        if len(found) != 1:
                            problems.append({"block": block, "variant": name,
                                             "reason": "missing pair" if not found else "duplicate pair"})
                            continue
                        error = _invalid(found[0])
                        if error:
                            problems.append({"block": block, "variant": name, "reason": error})
                        pair.append(found[0])
                    if len(pair) != 2 or any(_invalid(r) for r in pair):
                        continue
                    baseline, candidate = pair
                    if baseline["digest"] != candidate["digest"] or baseline["checks"] != candidate["checks"]:
                        problems.append({"block": block, "reason": "digest or checks mismatch"})
                        continue
                    ratios.append((candidate["elapsed_ns"] / candidate["iterations"]) /
                                  (baseline["elapsed_ns"] / baseline["iterations"]))
                    allocations.append(candidate["allocated_bytes"] / candidate["iterations"])
                    controls.append(baseline["allocated_bytes"] / baseline["iterations"])
                    samples.append(candidate)
                if len({r["digest"] for r in samples}) > 1:
                    problems.append({"reason": "digest changed across process blocks"})
                result["blocks"] = len(ratios)
                if problems or malformed:
                    if malformed:
                        problems.append({"reason": "malformed pairing identities in input"})
                    result.update(status="invalid", problems=problems)
                    reasons = [p["reason"] for p in problems]
                    if not malformed and not ratios and all(r.startswith("not_measured:") or r == "missing pair" for r in reasons):
                        result["status"] = "unmeasured"
                    result["observed_states"] = sorted({r["status"] for key, rs in groups.items()
                                                         if key[:3] in ((control, case_id, workload),
                                                                       (variant, case_id, workload))
                                                         for r in rs if isinstance(r.get("status"), str)})
                elif ratios:
                    if any(not _number(r, True) for r in ratios):
                        result.update(status="invalid", problems=[{"reason": "unrepresentable time ratio"}])
                        comparisons.append(result)
                        continue
                    ratio = _geomean(ratios)
                    result.update(status="inconclusive", time_ratio=ratio,
                                  speedup=1 / ratio, allocated_bytes_per_operation=statistics.mean(allocations),
                                  control_allocated_bytes_per_operation=statistics.mean(controls),
                                  allocation_delta_bytes=statistics.mean(allocations)-statistics.mean(controls),
                                  family=samples[0].get("family", workload),
                                  kind="model" if workload == "model" else samples[0].get("kind", "stage"))
                    if len(ratios) >= min_blocks:
                        interval = _interval([math.log(r) for r in ratios])
                        result["time_ratio_ci95"] = interval
                        if interval:
                            low, high = interval
                            if high < 1 - practical_threshold:
                                result["status"] = "faster"
                            elif low > 1 + practical_threshold:
                                result["status"] = "slower"
                            elif low >= 1-practical_threshold and high <= 1+practical_threshold:
                                result["status"] = "equivalent"
                    else:
                        result["reason"] = "too few independent process blocks"
                comparisons.append(result)
    summaries = []
    for control in control_names:
        for variant in names:
            subset = [r for r in comparisons if r["control"] == control and r["variant"] == variant]
            if not subset:
                continue
            stages = [r for r in subset if r.get("kind") != "model"]
            valid = [r for r in stages if "time_ratio" in r]
            families = defaultdict(list)
            for result in valid:
                families[result["family"]].append(result["time_ratio"])
            summary = dict(variant=variant, control=control, status="unmeasured",
                           measured_workloads=len(valid), expected_workloads=len(stages),
                           family_ratios={f: _geomean(rs) for f, rs in sorted(families.items())})
            if valid:
                summary.update(status="descriptive" if len(valid) == len(stages) and not malformed else "incomplete",
                               balanced_time_ratio=_geomean(summary["family_ratios"].values()))
            summaries.append(summary)
    return dict(schema=1, baseline=baseline, practical_threshold=practical_threshold, min_blocks=min_blocks,
                uncertainty="95% Student t interval on paired log time ratios; independent process blocks assumed",
                malformed_rows=malformed, comparisons=comparisons, summaries=summaries,
                model_totals=[r for r in comparisons if r.get("kind") == "model"],
                worst_regressions=sorted([r for r in comparisons if r.get("status") == "slower"],
                                         key=lambda r: r["time_ratio"], reverse=True),
                limitations=["Aggregate ratios are descriptive, not confidence intervals.",
                             "Quick-suite prediction on held-out corpus has not been established."])


def render_markdown(report):
    """Render all comparisons, including invalid and unmeasured results."""
    lines = ["Time ratio is variant/control; below 1 is faster.", "", report["uncertainty"], "",
             "| Variant / control | Case / workload | State | Blocks | Time ratio (95% interval) | Bytes/op (control) |",
             "|---|---|---|---:|---|---:|"]
    for row in report["comparisons"]:
        ratio = f'{row["time_ratio"]:.4f}' if "time_ratio" in row else "unavailable"
        if row.get("time_ratio_ci95"):
            ratio += " (" + ", ".join(f"{v:.4f}" for v in row["time_ratio_ci95"]) + ")"
        allocation = (f'{row["allocated_bytes_per_operation"]:.2f} ({row["control_allocated_bytes_per_operation"]:.2f})'
                      if "allocated_bytes_per_operation" in row else "unavailable")
        cells = [row['variant'] + ' / ' + row['control'], row['case_id'] + ' / ' + row['workload'],
                 row['status'], str(row['blocks']), ratio, allocation]
        lines.append("| " + " | ".join(c.replace("|", "\\|").replace("\n", " ") for c in cells) + " |")
        if row.get("problems"):
            lines.append("\nProblems: " + "; ".join(p["reason"] for p in row["problems"]) + "\n")
    lines += ["", "Family-balanced stage summaries (descriptive; model totals are separate rows):", ""]
    for summary in report["summaries"]:
        ratio = summary.get("balanced_time_ratio")
        lines.append(f'- {summary["variant"]} / {summary["control"]}: {summary["status"]}; '
                     + (f'time ratio {ratio:.4f}; ' if ratio is not None else "")
                     + f'{summary["measured_workloads"]}/{summary["expected_workloads"]} stage workloads.')
    lines += ["", *report["limitations"]]
    if report["malformed_rows"]:
        lines.append(f'Invalid pairing identities: {len(report["malformed_rows"])}; see JSON for details.')
    return "\n".join(lines) + "\n"
