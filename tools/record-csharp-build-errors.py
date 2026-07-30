#!/usr/bin/env python3
"""Parse a `dotnet build` log, categorize C# errors, update status-report-snapshot.json.

Usage:
  python tools/record-csharp-build-errors.py --target NAME --log PATH [--project PATH]
  python tools/record-csharp-build-errors.py --target NAME --project PATH.csproj
      # runs `dotnet build` itself, then records

Also invoked by tools/dotnet-build-record.ps1 after every recorded Plato C# build.
"""
from __future__ import annotations

import argparse
import json
import pathlib
import re
import subprocess
import sys
from collections import Counter
from datetime import datetime

PLATO = pathlib.Path(__file__).resolve().parents[1]
SNAPSHOT = PLATO / "docs" / "status-report-snapshot.json"

# Human categories for the status report. Unknown codes fall into "other".
CODE_CATEGORY: dict[str, str] = {
    "CS0315": "concept_self_bound",
    "CS0305": "generic_arity",
    "CS0535": "missing_interface_member",
    "CS0557": "duplicate_conversion",
    "CS0736": "static_vs_instance",
    "CS1061": "missing_member",
    "CS0030": "cannot_convert",
    "CS0029": "cannot_implicitly_convert",
    "CS0019": "operator_type_mismatch",
    "CS1503": "argument_type",
    "CS1929": "extension_receiver",
    "CS0246": "type_not_found",
    "CS0101": "duplicate_definition",
    "CS0111": "duplicate_member",
    "CS0102": "duplicate_type_member",
    "CS0540": "inaccessible_interface_member",
    "CS0534": "does_not_implement_inherited",
    "CS0051": "inconsistent_accessibility",
    "CS0053": "inconsistent_accessibility",
    "CS1615": "argument_must_be_variable",
    "CS1620": "ref_out_mismatch",
    "CS0411": "type_inference_failed",
    "CS0828": "anonymous_type",
    "CS8147": "property_must_have_body",
    "CS0103": "name_not_in_context",
    "CS0428": "method_group_conversion",
    "MSB4019": "msbuild_import_missing",
    "MSB3027": "msbuild_file_lock",
    "MSB3021": "msbuild_file_lock",
}

CATEGORY_LABELS: dict[str, str] = {
    "concept_self_bound": "Concept Self-bound (concrete where F-bounded Self required)",
    "generic_arity": "Wrong generic arity",
    "missing_interface_member": "Missing interface member on runtime/type",
    "duplicate_conversion": "Duplicate user-defined conversion",
    "static_vs_instance": "Static vs instance interface mismatch",
    "missing_member": "Missing member on receiver (CS1061)",
    "cannot_convert": "Cannot convert type",
    "cannot_implicitly_convert": "Cannot implicitly convert",
    "operator_type_mismatch": "Operator type mismatch",
    "argument_type": "Argument type mismatch",
    "extension_receiver": "Extension method receiver type",
    "type_not_found": "Type or namespace not found",
    "duplicate_definition": "Duplicate definition in namespace",
    "duplicate_member": "Duplicate member",
    "duplicate_type_member": "Type already defines member",
    "inaccessible_interface_member": "Inaccessible interface member",
    "does_not_implement_inherited": "Does not implement inherited abstract member",
    "inconsistent_accessibility": "Inconsistent accessibility",
    "argument_must_be_variable": "Argument must be variable",
    "ref_out_mismatch": "ref/out mismatch",
    "type_inference_failed": "Type inference failed",
    "anonymous_type": "Anonymous type issue",
    "property_must_have_body": "Property must have body",
    "name_not_in_context": "Name does not exist in current context",
    "method_group_conversion": "Method group must be invoked / cannot convert",
    "msbuild_import_missing": "MSBuild import / props missing",
    "msbuild_file_lock": "MSBuild output file locked",
    "other": "Other / uncategorized",
}

# Matches: error CS1061: / error MSB4019:
ERROR_RE = re.compile(
    r"(?:^|\s)error\s+([A-Z]{2,}\d+)\s*:",
    re.IGNORECASE | re.MULTILINE,
)
WARNING_RE = re.compile(
    r"(?:^|\s)warning\s+([A-Z]{2,}\d+)\s*:",
    re.IGNORECASE | re.MULTILINE,
)
SUMMARY_ERRORS_RE = re.compile(r"(\d+)\s+Error\(s\)", re.IGNORECASE)
SUMMARY_WARNINGS_RE = re.compile(r"(\d+)\s+Warning\(s\)", re.IGNORECASE)


def parse_log(text: str) -> dict:
    # Deduplicate: MSBuild often prints each error twice (inline + summary block).
    seen_lines: set[str] = set()
    codes: list[str] = []
    warn_codes: list[str] = []
    for line in text.splitlines():
        key = line.strip()
        if not key or key in seen_lines:
            continue
        em = ERROR_RE.search(line)
        if em:
            seen_lines.add(key)
            codes.append(em.group(1).upper())
            continue
        wm = WARNING_RE.search(line)
        if wm:
            seen_lines.add(key)
            warn_codes.append(wm.group(1).upper())

    by_code = Counter(codes)
    warn_by_code = Counter(warn_codes)
    by_category: Counter[str] = Counter()
    for code, n in by_code.items():
        by_category[CODE_CATEGORY.get(code, "other")] += n

    summary_errors = None
    summary_warnings = None
    m = SUMMARY_ERRORS_RE.search(text)
    if m:
        summary_errors = int(m.group(1))
    m = SUMMARY_WARNINGS_RE.search(text)
    if m:
        summary_warnings = int(m.group(1))

    counted = sum(by_code.values())
    # Prefer the line-level unique count; fall back to MSBuild summary if we found nothing.
    total_errors = counted if counted else (summary_errors or 0)
    if summary_errors is not None and counted and summary_errors > counted:
        # Summary can count duplicates; keep unique line count but note summary.
        pass
    total_warnings = sum(warn_by_code.values()) if warn_by_code else (summary_warnings or 0)

    return {
        "total_errors": total_errors,
        "total_warnings": total_warnings,
        "summary_errors": summary_errors,
        "summary_warnings": summary_warnings,
        "by_code": dict(sorted(by_code.items(), key=lambda kv: (-kv[1], kv[0]))),
        "by_category": {
            cat: {
                "count": n,
                "label": CATEGORY_LABELS.get(cat, cat),
                "codes": sorted(
                    [c for c, cc in by_code.items() if CODE_CATEGORY.get(c, "other") == cat],
                    key=lambda c: -by_code[c],
                ),
            }
            for cat, n in sorted(by_category.items(), key=lambda kv: (-kv[1], kv[0]))
        },
        "warning_by_code": dict(sorted(warn_by_code.items(), key=lambda kv: (-kv[1], kv[0]))),
    }


def read_log_text(path: pathlib.Path) -> str:
    raw = path.read_bytes()
    if raw.startswith(b"\xff\xfe") or raw.startswith(b"\xfe\xff"):
        return raw.decode("utf-16")
    if raw.startswith(b"\xef\xbb\xbf"):
        return raw.decode("utf-8-sig")
    # PowerShell sometimes writes UTF-16 without a reliable BOM on empty; sniff NULs.
    if b"\x00" in raw[:200]:
        return raw.decode("utf-16", errors="replace")
    return raw.decode("utf-8", errors="replace")

    log_path.parent.mkdir(parents=True, exist_ok=True)
    cmd = [
        "dotnet",
        "build",
        str(project),
        "-c",
        configuration,
        "--nologo",
        "-v",
        "minimal",
    ]
    with log_path.open("w", encoding="utf-8", errors="replace") as f:
        proc = subprocess.run(cmd, stdout=f, stderr=subprocess.STDOUT, text=True)
    return proc.returncode


def update_snapshot(target: str, record: dict) -> None:
    if SNAPSHOT.is_file():
        data = json.loads(SNAPSHOT.read_text(encoding="utf-8"))
    else:
        data = {}
    builds = data.setdefault("csharp_builds", {})
    builds["updated_at"] = datetime.now().astimezone().isoformat(timespec="seconds")
    targets = builds.setdefault("targets", {})
    targets[target] = record
    # Convenience: point "latest" at the most recently recorded target.
    builds["latest_target"] = target
    builds["latest"] = {
        "target": target,
        "total_errors": record.get("total_errors", 0),
        "total_warnings": record.get("total_warnings", 0),
        "built_at": record.get("built_at"),
        "exit_code": record.get("exit_code"),
        "by_category": {
            k: v.get("count", 0) for k, v in (record.get("by_category") or {}).items()
        },
        "by_code_top": dict(list((record.get("by_code") or {}).items())[:12]),
    }
    data["updated_at"] = builds["updated_at"]
    SNAPSHOT.parent.mkdir(parents=True, exist_ok=True)
    SNAPSHOT.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("--target", required=True, help="Logical build name (e.g. forward-conformance)")
    ap.add_argument("--log", type=pathlib.Path, help="Existing dotnet build log to parse")
    ap.add_argument("--project", type=pathlib.Path, help="csproj to build (if --log omitted, builds it)")
    ap.add_argument("--configuration", default="Release")
    ap.add_argument(
        "--build-exit",
        type=int,
        default=None,
        help="Exit code from the preceding dotnet build (when --log is supplied)",
    )
    ap.add_argument(
        "--exit-with-build",
        action="store_true",
        help="Exit with the dotnet build exit code (default: 0 after successful record)",
    )
    args = ap.parse_args()

    log_path = args.log
    exit_code = 0 if args.build_exit is None else args.build_exit
    project = args.project.resolve() if args.project else None

    if log_path is None:
        if project is None:
            print("error: provide --log or --project", file=sys.stderr)
            return 2
        log_path = PLATO / ".temp" / "csharp-build-logs" / f"{args.target}.log"
        exit_code = run_build(project, args.configuration, log_path)
    else:
        log_path = log_path.resolve()
        if not log_path.is_file():
            print(f"error: log not found: {log_path}", file=sys.stderr)
            return 2

    text = read_log_text(log_path)
    parsed = parse_log(text)
    if args.build_exit is None and args.log is not None:
        exit_code = 0 if parsed["total_errors"] == 0 else 1

    record = {
        "built_at": datetime.now().astimezone().isoformat(timespec="seconds"),
        "project": str(project) if project else None,
        "log": str(log_path),
        "configuration": args.configuration,
        "exit_code": exit_code,
        **parsed,
    }

    update_snapshot(args.target, record)
    print(
        json.dumps(
            {
                "target": args.target,
                "total_errors": record["total_errors"],
                "total_warnings": record["total_warnings"],
                "by_category": {k: v["count"] for k, v in record["by_category"].items()},
                "by_code_top": dict(list(record["by_code"].items())[:8]),
                "snapshot": str(SNAPSHOT),
                "exit_code": record["exit_code"],
            },
            indent=2,
        )
    )
    if args.exit_with_build:
        return exit_code if args.log is None else record["exit_code"]
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
