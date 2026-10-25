"""Correctness-gated, process-block paired optimizer comparisons.

Ratios are variant/control elapsed time per invocation: below one is faster.
The interval is a Student t interval on paired log ratios, assuming independent
process blocks. Fewer than min_blocks never receive an effect classification.
"""
import math
import statistics
from collections import defaultdict


def _number(value, positive=False):
    try:
        return (isinstance(value, (int, float)) and not isinstance(value, bool)
                and math.isfinite(value) and (value > 0 if positive else value >= 0))
    except OverflowError:
        return False


def _invalid(row):
    if row.get("status") != "ok":
        return str(row.get("status", "error")) + ": " + str(row.get("error", "unsuccessful run"))
    if row.get("schema") != 1 or isinstance(row["schema"], bool):
        return "unsupported schema"
    if not _number(row.get("elapsed_ns"), True):
        return "invalid elapsed_ns"
    iterations = row.get("iterations")
    if not isinstance(iterations, int) or not _number(iterations, True):
        return "invalid iterations"
    if not _number(row.get("allocated_bytes")):
        return "invalid allocated_bytes"
    if not isinstance(row.get("digest"), str) or not row["digest"]:
        return "missing digest"
    checks = row.get("checks")
    if not isinstance(checks, dict) or not checks or any(v is not True for v in checks.values()):
        return "failed or missing correctness checks"
    if not _number(row["elapsed_ns"] / iterations, True):
        return "unrepresentable per-operation time"
    if "family" in row and (not isinstance(row["family"], str) or not row["family"]):
        return "invalid family"
    if "kind" in row and row["kind"] not in ("stage", "model"):
        return "invalid kind"
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
                    control_sample, candidate = pair
                    if control_sample["digest"] != candidate["digest"] or control_sample["checks"] != candidate["checks"]:
                        problems.append({"block": block, "reason": "digest or checks mismatch"})
                        continue
                    ratios.append((candidate["elapsed_ns"] / candidate["iterations"]) /
                                  (control_sample["elapsed_ns"] / control_sample["iterations"]))
                    allocations.append(candidate["allocated_bytes"] / candidate["iterations"])
                    controls.append(control_sample["allocated_bytes"] / control_sample["iterations"])
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
    recommendations = []
    for statuses, action in (({"invalid"}, "repair_or_investigate"),
                             ({"slower", "inconclusive"}, "confirm"),
                             ({"unmeasured"}, "measure")):
        affected = [r for r in comparisons if r["status"] in statuses]
        if affected:
            recommendations.append(dict(action=action,
                case_ids=sorted({r["case_id"] for r in affected}),
                workloads=sorted({r["workload"] for r in affected}),
                comparisons=[{k: r[k] for k in ("variant", "control", "case_id", "workload", "status")}
                             for r in affected]))
    return dict(schema=1, baseline=baseline, practical_threshold=practical_threshold, min_blocks=min_blocks,
                uncertainty="95% Student t interval on paired log time ratios; independent process blocks assumed",
                malformed_rows=malformed, comparisons=comparisons, summaries=summaries,
                recommendations=recommendations,
                model_totals=[r for r in comparisons if r.get("kind") == "model"],
                worst_regressions=sorted([r for r in comparisons if r.get("status") == "slower"],
                                         key=lambda r: r["time_ratio"], reverse=True),
                limitations=["Aggregate ratios are descriptive, not confidence intervals.",
                             "Quick-suite prediction on held-out corpus has not been established."])


def render_markdown(report):
    """Render all comparisons, including invalid and unmeasured results."""
    lines = ["Time ratio is variant/control; below 1 is faster.", "", report["uncertainty"], ""]
    coverage = report.get("coverage")
    if coverage:
        lines += ["Selection coverage:", "",
                  "- Selected case IDs: " + ", ".join(c["id"] for c in coverage["cases"]),
                  "- Reserved holdout IDs: " + (", ".join(coverage.get("holdout", [])) or "none"),
                  "- Uncovered dimensions: " + (", ".join(coverage.get("uncovered_dimensions", [])) or "none"),
                  "- Cost basis: " + ("discovery estimates from measured process costs" if coverage.get("estimated_costs")
                                      else "uniform coverage weights; no runtime measurements"),
                  "- Prediction validated: " + str(coverage.get("prediction_validated", False)).lower(), ""]
    lines += [
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
        complete = summary["status"] == "descriptive"
        lines.append(f'- {summary["variant"]} / {summary["control"]}: {summary["status"]}; '
                     + (f'time ratio {ratio:.4f}; ' if ratio is not None and complete else "")
                     + f'{summary["measured_workloads"]}/{summary["expected_workloads"]} stage workloads.')
        if not complete:
            lines.append("  Incomplete or unmeasured coverage; no suite performance conclusion is supported.")
    lines += ["", "Recommended follow-up:", ""]
    descriptions = {"repair_or_investigate": "Investigate failed correctness checks, builds, timeouts or invalid pairs before timing",
                    "confirm": "Confirm regressions or inconclusive results with more independent process blocks",
                    "measure": "Measure missing or budget-skipped cases"}
    for recommendation in report.get("recommendations", []):
        lines.append("- " + descriptions[recommendation["action"]] + ": case IDs "
                     + ", ".join(recommendation["case_ids"]) + "; workloads "
                     + ", ".join(recommendation["workloads"]) + ".")
    if not report.get("recommendations"):
        lines.append("No affected cases identified by these comparisons.")
    lines += ["", *report["limitations"]]
    if report["malformed_rows"]:
        lines.append(f'Invalid pairing identities: {len(report["malformed_rows"])}; see JSON for details.')
    return "\n".join(lines) + "\n"


def validate_prediction(quick_report, corpus_report, *, control="shipping"):
    """Compare a specific quick run with its reserved, measured holdout.

    This is descriptive evidence on supplied cases, not proof of general corpus
    prediction. Ranking uses only variants with complete, classified results in
    both selected and reserved cases and common workload families.
    """
    coverage = quick_report.get("coverage", {})
    selected = {c["id"] for c in coverage.get("cases", [])}
    reserved = set(coverage.get("holdout", []))
    result = dict(schema=1, control=control, status="insufficient_evidence", observations=[],
                  selected_ids=sorted(selected), reserved_ids=sorted(reserved),
                  prediction_validated=False,
                  limitation="Descriptive comparison of supplied runs only; independent cases, equivalent inputs and measurement conditions must be verified by the caller.")
    if not selected or not reserved or selected.intersection(reserved):
        result["reason"] = "missing or overlapping selected and reserved case IDs"
        return result
    qualified = {"faster", "slower", "equivalent"}
    variants = sorted({r["variant"] for r in quick_report["comparisons"] if r["control"] == control})
    for variant in variants:
        quick = [r for r in quick_report["comparisons"] if r["control"] == control
                 and r["variant"] == variant and r["case_id"] in selected and r.get("kind") != "model"]
        heldout = [r for r in corpus_report["comparisons"] if r["control"] == control
                   and r["variant"] == variant and r["case_id"] in reserved and r.get("kind") != "model"]
        observation = dict(variant=variant, status="insufficient_evidence")
        if (not quick or not heldout or {r["case_id"] for r in quick} != selected
                or {r["case_id"] for r in heldout} != reserved
                or any(r["status"] not in qualified for r in quick+heldout)):
            observation["reason"] = "missing, invalid, unmeasured or inconclusive selected/holdout comparisons"
        else:
            common = {r["family"] for r in quick}.intersection(r["family"] for r in heldout)
            def aggregate(rows):
                return _geomean(_geomean(r["time_ratio"] for r in rows if r["family"] == family)
                                for family in sorted(common))
            if not common:
                observation["reason"] = "no shared workload families"
            else:
                a, b = aggregate(quick), aggregate(heldout)
                threshold = max(quick_report["practical_threshold"], corpus_report["practical_threshold"])
                def direction(ratio):
                    return "slower" if ratio > 1+threshold else "faster" if ratio < 1-threshold else "equivalent"
                quick_regressions = {r["family"] for r in quick if r["status"] == "slower"}
                holdout_regressions = {r["family"] for r in heldout if r["status"] == "slower"}
                observation.update(status="descriptive", families=sorted(common), selected_time_ratio=a,
                    holdout_time_ratio=b, selected_direction=direction(a), holdout_direction=direction(b),
                    direction_agrees=direction(a) == direction(b),
                    selected_regression_families=sorted(quick_regressions),
                    holdout_regression_families=sorted(holdout_regressions),
                    unexpected_holdout_regression_families=sorted(holdout_regressions-quick_regressions))
        result["observations"].append(observation)
    valid = [r for r in result["observations"] if r["status"] == "descriptive"]
    if valid:
        result["status"] = "descriptive" if len(valid) == len(variants) else "incomplete"
        result["selected_ranking"] = [r["variant"] for r in sorted(valid, key=lambda r: (r["selected_time_ratio"], r["variant"]))]
        result["holdout_ranking"] = [r["variant"] for r in sorted(valid, key=lambda r: (r["holdout_time_ratio"], r["variant"]))]
        result["ranking_agrees"] = result["selected_ranking"] == result["holdout_ranking"]
    return result
