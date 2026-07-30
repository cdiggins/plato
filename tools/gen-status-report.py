#!/usr/bin/env python3
"""Generate docs/status-report.html for the Plato repo.

Live sections (always refreshed): git status, commits, dirty files, worktrees,
submodules, studio pointer drift, tracker issues.

Cached sections (docs/status-report-snapshot.json): gate/lint/nav probe results.
Update the snapshot after a gate run with:
  python tools/gen-status-report.py --write-snapshot-from-defaults
or edit the JSON by hand / via --set-gate.

Intended to run from a pre-commit hook so every Plato commit refreshes the report.
Requires the studio monorepo layout for tracker issue links (../../tracker).
"""
from __future__ import annotations

import argparse
import collections
import html
import json
import pathlib
import re
import subprocess
from datetime import datetime, timedelta

PLATO = pathlib.Path(__file__).resolve().parents[1]
OUT = PLATO / "docs" / "status-report.html"
SNAPSHOT = PLATO / "docs" / "status-report-snapshot.json"


def find_studio(plato: pathlib.Path) -> pathlib.Path | None:
    for parent in [plato.parent.parent, *plato.parents]:
        if (parent / "tracker" / "issues").is_dir():
            return parent
    return None


STUDIO = find_studio(PLATO)


def run(args: list[str], cwd: pathlib.Path) -> tuple[str, int]:
    r = subprocess.run(
        args, cwd=cwd, capture_output=True, text=True, encoding="utf-8", errors="replace"
    )
    return (r.stdout or "").strip(), r.returncode


def git(args: list[str], cwd: pathlib.Path = PLATO) -> str:
    return run(["git", *args], cwd=cwd)[0]


def e(s: object) -> str:
    return html.escape(str(s) if s is not None else "")


def badge(kind: str, text: str) -> str:
    return f'<span class="badge {e(kind)}">{e(text)}</span>'


def issue_href(issue_id: str) -> str:
    if STUDIO is None:
        return f"#{issue_id}"
    return (STUDIO / "tracker" / "issues" / f"{issue_id}.md").resolve().as_uri()


def issue_a(issue_id: str, label: str | None = None) -> str:
    lab = label if label is not None else issue_id
    return f'<a href="{e(issue_href(issue_id))}"><code>{e(lab)}</code></a>'


def link_issue_ids(text: str) -> str:
    parts: list[str] = []
    last = 0
    for m in re.finditer(r"plato-\d+", text):
        parts.append(e(text[last : m.start()]))
        parts.append(issue_a(m.group(0)))
        last = m.end()
    parts.append(e(text[last:]))
    return "".join(parts)


def rows_issues(items: list[dict]) -> str:
    if not items:
        return '<tr><td colspan="6" class="muted">None</td></tr>'
    out = []
    for i in items:
        out.append(
            "<tr>"
            f"<td>{issue_a(i['id'])}</td>"
            f'<td><a href="{e(issue_href(i["id"]))}">{e(i["title"])}</a></td>'
            f"<td>{badge('type', i['type'])}</td>"
            f"<td>{badge('prio', i['priority'])}</td>"
            f"<td>{badge('status', i['status'])}</td>"
            f"<td>{e(i.get('effort', '?'))} / {e(i.get('risk', '?'))}</td>"
            "</tr>"
        )
    return "\n".join(out)


# Descriptions from PlatoCompiler/Analysis/Linter.cs; counts come from the snapshot.
LINT_DEFS: list[tuple[str, str, str, str]] = [
    (
        "LINT001",
        "Warning",
        "Unimplemented interface obligation",
        "Concrete type claims a concept member but has no body; generated C# throws NotImplementedException.",
    ),
    (
        "LINT002",
        "Error",
        "Where-clause on undeclared type var",
        "'where' constrains a type variable that is not declared on the generic.",
    ),
    (
        "LINT003",
        "Info",
        "Declared-but-unused field",
        "Field on a concrete type is never read by any library function, concept impl, or generated member.",
    ),
    (
        "LINT004",
        "Error",
        "Duplicate library signature",
        "Two functions in the same library share name + parameter types.",
    ),
    (
        "LINT005",
        "Error/Warn",
        "Generic type-var misuse",
        "Type variable used only once, or appears in the return type but is not inferable from parameters.",
    ),
    (
        "LINT006",
        "Error",
        "Affine builder stored in a field",
        "Unique/builder type used as a field type (builders must not be stored).",
    ),
    (
        "LINT007",
        "Error",
        "Affine builder as type argument",
        "Unique/builder type used as a generic argument (no containers of builders).",
    ),
    (
        "LINT008",
        "Info",
        "Concept with no implementer",
        "Concept exists but no concrete type implements it — possibly unfinished vocabulary.",
    ),
    (
        "LINT009",
        "Info",
        "Unmentioned concept",
        "Concept is never implemented, referenced, or used as a where-bound — stronger than LINT008.",
    ),
    (
        "LINT010",
        "Info",
        "Orphan concrete type",
        "Concrete type implements no concept and is mentioned by no function signature or field.",
    ),
    (
        "LINT011",
        "Info",
        "Redundant implements clause",
        "'implements' already implied by another implemented concept on the same type.",
    ),
    (
        "LINT012",
        "Warning",
        "Receiver-marker mismatch",
        "Obligation and implementation disagree on the '_' receiver marker (static vs instance).",
    ),
]

DEFAULT_SNAPSHOT: dict = {
    "updated_at": "2026-07-29T18:00:00-04:00",
    "note": "Captured during initial status-report landing; refresh after gate runs.",
    "gates": [
        {
            "name": "stdlib fast gate (lint + checker ratchet)",
            "result": "PASS",
            "seconds": "538s",
            "detail": "2026-07-29 17:54 ET",
        },
        {
            "name": "lint --strict forward stdlib",
            "result": "PASS",
            "seconds": "14s",
            "detail": "0 parse / 0 resolution errors; warning/info findings present",
        },
        {
            "name": "frozen-v1 tripwire",
            "result": "PASS",
            "seconds": "<1s",
            "detail": "210 files unchanged",
        },
        {
            "name": "Plato.CLI + Compiler build (Release)",
            "result": "PASS",
            "seconds": "~17s",
            "detail": "file-lock noise possible when Navigation MCP is live",
        },
        {
            "name": "Plato.sln full build",
            "result": "BLOCKED",
            "seconds": "101s",
            "detail": "MSB3027 when Plato.Navigation.CLI holds DLL locks",
        },
        {
            "name": "PlatoTests",
            "result": "PASS",
            "seconds": "77s",
            "detail": "161 passed / 0 failed",
        },
        {
            "name": "Plato.Navigation.Tests",
            "result": "PASS",
            "seconds": "42s",
            "detail": "30 passed / 0 failed",
        },
        {
            "name": "conformance suite",
            "result": "NOT RUN",
            "seconds": "—",
            "detail": "expected 0 fail when run; refresh snapshot after regen-conformance.ps1 -Test",
        },
        {
            "name": "regen-generated diff gate",
            "result": "NOT RUN",
            "seconds": "—",
            "detail": "refresh after regen-generated.ps1",
        },
        {
            "name": "check-all.ps1 full battery",
            "result": "NOT RUN",
            "seconds": "—",
            "detail": "run before merge / mission end",
        },
    ],
    "lint": {
        "parse_errors": 0,
        "resolution_errors": 0,
        "strict_exit": 0,
        "warnings": 262,
        "infos": 2404,
        "errors": 0,
        "counts": {
            "LINT001": 260,
            "LINT002": 0,
            "LINT003": 2279,
            "LINT004": 0,
            "LINT005": 0,
            "LINT006": 0,
            "LINT007": 0,
            "LINT008": 37,
            "LINT009": 8,
            "LINT010": 59,
            "LINT011": 21,
            "LINT012": 2,
        },
    },
    "nav": {
        "files": 363,
        "definitions": 13512,
        "references": 33693,
        "diagnostics": 0,
        "builtUtc": "2026-07-29T21:46:46Z",
        "roots": "stdlib + stdlib-tests",
    },
}


def load_snapshot() -> dict:
    if SNAPSHOT.is_file():
        data = json.loads(SNAPSHOT.read_text(encoding="utf-8"))
        # Fill missing keys from defaults
        for k, v in DEFAULT_SNAPSHOT.items():
            data.setdefault(k, v)
        return data
    return json.loads(json.dumps(DEFAULT_SNAPSHOT))


def parse_issues() -> list[dict]:
    issues: list[dict] = []
    if STUDIO is None:
        return issues
    for p in sorted((STUDIO / "tracker" / "issues").glob("plato-*.md")):
        text = p.read_text(encoding="utf-8")
        fm: dict[str, str] = {}
        if text.startswith("---"):
            body = text.split("---", 2)[1]
            for line in body.splitlines():
                if ":" in line:
                    k, v = line.split(":", 1)
                    fm[k.strip()] = v.strip().strip('"')
        issues.append(
            {
                "id": p.stem,
                "title": fm.get("title", ""),
                "type": fm.get("type", "?"),
                "status": fm.get("status", "?"),
                "priority": fm.get("priority", "?"),
                "effort": fm.get("effort", "?"),
                "risk": fm.get("risk", "?"),
                "created": fm.get("created", ""),
                "closed": fm.get("closed", ""),
            }
        )
    return issues


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument(
        "--write-snapshot-from-defaults",
        action="store_true",
        help="Write docs/status-report-snapshot.json from built-in defaults (bootstrap).",
    )
    args = ap.parse_args()

    if args.write_snapshot_from_defaults:
        SNAPSHOT.parent.mkdir(parents=True, exist_ok=True)
        snap = dict(DEFAULT_SNAPSHOT)
        snap["updated_at"] = datetime.now().astimezone().isoformat(timespec="seconds")
        SNAPSHOT.write_text(json.dumps(snap, indent=2) + "\n", encoding="utf-8")
        print(f"wrote {SNAPSHOT}")

    snapshot = load_snapshot()
    now = datetime.now().astimezone()
    today_start = now.replace(hour=0, minute=0, second=0, microsecond=0)
    week_ago = (now - timedelta(days=7)).strftime("%Y-%m-%d")

    head = git(["rev-parse", "HEAD"])
    origin = git(["rev-parse", "origin/main"])
    branch = git(["branch", "--show-current"])
    status_sb = git(["status", "-sb"])
    ahead = [l for l in git(["log", "origin/main..HEAD", "--oneline"]).splitlines() if l]
    today = [
        l
        for l in git(
            ["log", f"--since={today_start.isoformat()}", "--pretty=format:%h|%ci|%s"]
        ).splitlines()
        if l
    ]
    recent = [l for l in git(["log", "-20", "--pretty=format:%h|%ci|%s"]).splitlines() if l]
    dirty = [
        l
        for l in git(["status", "--porcelain"]).splitlines()
        if not l.startswith("?? .claude/")
        and "docs/status-report.html" not in l  # ignore self while regenerating
        and "docs/status-report-snapshot.json" not in l
    ]
    # Full dirty for display (including report if modified after write — use porcelain after)
    dirty_display = [
        l
        for l in git(["status", "--porcelain"]).splitlines()
        if not l.startswith("?? .claude/")
    ]
    diffstat = git(["diff", "--stat", "--", ".", ":(exclude)docs/status-report.html"])
    worktrees = git(["worktree", "list"]).splitlines()
    submods = git(["submodule", "status"]).splitlines()
    commits_7d = git(["rev-list", "--count", f"--since={week_ago}", "HEAD"]) or "0"

    studio_sb = ""
    studio_ptr = "?"
    studio_dirty: list[str] = []
    if STUDIO is not None:
        studio_sb = run(["git", "status", "-sb"], cwd=STUDIO)[0]
        ptr_line = run(["git", "ls-tree", "HEAD", "submodules/Plato"], cwd=STUDIO)[0].split()
        studio_ptr = ptr_line[2] if len(ptr_line) > 2 else "?"
        studio_dirty = [
            l
            for l in run(["git", "status", "--porcelain"], cwd=STUDIO)[0].splitlines()
            if any(x in l for x in ("Plato", "plato", "polyhedra"))
        ]

    issues = parse_issues()
    by_status = collections.Counter(i["status"] for i in issues)
    in_progress = [i for i in issues if i["status"] == "in-progress"]
    ready = [i for i in issues if i["status"] == "ready"]
    ideas = [i for i in issues if i["status"] == "idea"]
    done_recent = sorted(
        [i for i in issues if i["status"] == "done" and i.get("closed")],
        key=lambda x: x.get("closed", ""),
        reverse=True,
    )[:15]
    open_bugs = [i for i in issues if i["type"] == "bug" and i["status"] not in ("done", "dropped")]
    open_problems = [
        i for i in issues if i["type"] == "problem" and i["status"] not in ("done", "dropped")
    ]

    gates = snapshot.get("gates") or DEFAULT_SNAPSHOT["gates"]
    lint = snapshot.get("lint") or DEFAULT_SNAPSHOT["lint"]
    nav = snapshot.get("nav") or DEFAULT_SNAPSHOT["nav"]
    csharp_builds = snapshot.get("csharp_builds") or {}
    lint_counts = lint.get("counts") or {}

    pointer_ok = studio_ptr == head
    pointer_badge = badge("pass", "in sync") if pointer_ok else badge("blocked", "DRIFT")

    overall = "ATTENTION" if (ahead or dirty_display or not pointer_ok) else "HEALTHY-ISH"
    if dirty_display:
        dirty_bit = f"{len(dirty_display)} dirty Plato paths; "
    elif ahead:
        dirty_bit = f"Plato ahead of origin by {len(ahead)}; "
    else:
        dirty_bit = "Plato working tree clean vs origin/main; "
    overall_note = (
        dirty_bit
        + (
            "studio submodule pointer drifts from HEAD; "
            if not pointer_ok
            else "studio pointer matches HEAD; "
        )
        + f"gate snapshot from {snapshot.get('updated_at', '?')}; "
        "live git/tracker refreshed this generation."
    )

    today_rows = []
    for line in today:
        h, d, s = line.split("|", 2)
        today_rows.append(
            f"<tr><td><code>{e(h)}</code></td><td>{e(d)}</td><td>{link_issue_ids(s)}</td></tr>"
        )

    recent_rows = []
    today_hashes = {t.split("|", 1)[0] for t in today}
    for line in recent:
        h, d, s = line.split("|", 2)
        cls = "today" if h in today_hashes else ""
        recent_rows.append(
            f'<tr class="{cls}"><td><code>{e(h)}</code></td><td>{e(d)}</td><td>{link_issue_ids(s)}</td></tr>'
        )

    lint_rows = []
    lint_total = 0
    lint_nonzero = 0
    lint_ratchet = 0  # Error + Warning only; Info is vocabulary shape, not burn-down
    for code, sev, short, desc in LINT_DEFS:
        count = int(lint_counts.get(code, 0))
        lint_total += count
        if count:
            lint_nonzero += 1
        if sev == "Warning" or sev.startswith("Error"):
            lint_ratchet += count
        sev_class = "fail" if sev.startswith("Error") else ("warn" if sev == "Warning" else "idea")
        zero = " muted-row" if count == 0 else ""
        lint_rows.append(
            f'<tr class="{zero}">'
            f"<td><code>{e(code)}</code></td>"
            f"<td>{badge(sev_class, sev)}</td>"
            f'<td class="num">{count:,}</td>'
            f"<td><strong>{e(short)}</strong><div class=\"lint-desc\">{e(desc)}</div></td>"
            f"</tr>"
        )

    gate_rows = []
    for g in gates:
        result = g.get("result", "?")
        gate_rows.append(
            f"<tr><td>{e(g.get('name'))}</td><td>{badge(result.lower().replace(' ', '.'), result)}</td>"
            f"<td>{e(g.get('seconds', '—'))}</td><td>{e(g.get('detail', ''))}</td></tr>"
        )

    latest_cs = csharp_builds.get("latest") or {}
    cs_targets = csharp_builds.get("targets") or {}
    cs_category_rows = []
    cs_code_rows = []
    cs_target_rows = []
    for tname, trec in sorted(
        cs_targets.items(),
        key=lambda kv: kv[1].get("built_at") or "",
        reverse=True,
    ):
        te = trec.get("total_errors", 0)
        tw = trec.get("total_warnings", 0)
        cs_target_rows.append(
            f"<tr><td><code>{e(tname)}</code></td>"
            f"<td class=\"num\">{int(te):,}</td>"
            f"<td class=\"num\">{int(tw):,}</td>"
            f"<td>{badge('pass' if te == 0 else 'fail', 'OK' if te == 0 else 'ERRORS')}</td>"
            f"<td>{e(trec.get('built_at', '?'))}</td></tr>"
        )
    by_cat = (cs_targets.get(latest_cs.get("target") or "", {}) or {}).get("by_category") or {}
    if not by_cat and latest_cs.get("by_category"):
        # flattened latest view
        by_cat = {
            k: {"count": v, "label": k, "codes": []}
            for k, v in (latest_cs.get("by_category") or {}).items()
        }
    for cat, info in sorted(
        by_cat.items(),
        key=lambda kv: -(kv[1].get("count") if isinstance(kv[1], dict) else int(kv[1] or 0)),
    ):
        if isinstance(info, dict):
            count = int(info.get("count") or 0)
            label = info.get("label") or cat
            codes = ", ".join(info.get("codes") or [])
        else:
            count = int(info or 0)
            label = cat
            codes = ""
        cs_category_rows.append(
            f"<tr><td>{e(label)}</td><td><code>{e(cat)}</code></td>"
            f"<td class=\"num\">{count:,}</td><td><code>{e(codes)}</code></td></tr>"
        )
    by_code = (cs_targets.get(latest_cs.get("target") or "", {}) or {}).get("by_code") or latest_cs.get(
        "by_code_top"
    ) or {}
    for code, n in list(by_code.items())[:20]:
        cs_code_rows.append(
            f"<tr><td><code>{e(code)}</code></td><td class=\"num\">{int(n):,}</td></tr>"
        )
    if not cs_target_rows:
        cs_target_rows.append(
            '<tr><td colspan="5" class="muted">No C# builds recorded yet. '
            "Use <code>tools/dotnet-build-record.ps1</code> or "
            "<code>regen-forward-conformance.ps1 -Codegen</code>.</td></tr>"
        )
    if not cs_category_rows:
        cs_category_rows.append('<tr><td colspan="4" class="muted">No error categories for latest build.</td></tr>')
    if not cs_code_rows:
        cs_code_rows.append('<tr><td colspan="2" class="muted">No CS codes for latest build.</td></tr>')

    latest_err = int(latest_cs.get("total_errors") or 0)
    latest_warn = int(latest_cs.get("total_warnings") or 0)
    latest_target = latest_cs.get("target") or "—"
    latest_built = latest_cs.get("built_at") or csharp_builds.get("updated_at") or "—"
    cs_latest_badge = badge("pass", "0 errors") if latest_err == 0 else badge("fail", f"{latest_err:,} errors")

    dirty_html = (
        "<br>".join(e(l) for l in dirty_display) if dirty_display else '<span class="muted">clean</span>'
    )
    studio_dirty_html = (
        "<br>".join(e(l) for l in studio_dirty) if studio_dirty else '<span class="muted">none</span>'
    )
    worktree_html = "<br>".join(e(l) for l in worktrees) if worktrees else "—"
    submod_html = (
        "<br>".join(e(l) for l in submods) if submods else '<span class="muted">none listed</span>'
    )
    ahead_html = (
        "<br>".join(e(l) for l in ahead)
        if ahead
        else '<span class="muted">in sync with origin/main</span>'
    )
    diff_html = f"<pre>{e(diffstat)}</pre>" if diffstat else '<span class="muted">no unstaged diff</span>'

    status_cards = "".join(
        f'<div class="card"><div class="n">{by_status.get(k, 0)}</div><div class="l">{lab}</div></div>'
        for k, lab in (
            ("in-progress", "In progress"),
            ("ready", "Ready"),
            ("idea", "Ideas"),
            ("done", "Done"),
        )
    )

    studio_submods = (
        run(["git", "submodule", "status"], cwd=STUDIO)[0] if STUDIO is not None else "(studio not found)"
    )

    git_note = (
        f"Untracked <code>.claude/</code> under Plato is ignored in agent hygiene rules — do not stage it. "
        f"Report regenerated at commit time via <code>tools/githooks/pre-commit</code>."
    )
    if dirty_display:
        git_note = (
            f"{len(dirty_display)} dirty path(s) at generation time (see list). " + git_note
        )
    else:
        git_note = "Tracked tree matched the index aside from this report refresh. " + git_note

    tracker_note = (
        f"{len(ideas)} ideas remain open (not listed)."
        if ideas
        else "No open ideas."
    )
    if STUDIO is None:
        tracker_note = "Studio tracker not found — issue tables empty (standalone Plato clone)."

    doc = f"""<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1"/>
<title>Plato Status Report — {e(now.strftime('%Y-%m-%d %H:%M %Z'))}</title>
<style>
:root {{
  --bg: #0f1419; --panel: #1a222c; --panel2: #232d3a; --text: #e7ecf2;
  --muted: #8b9aab; --line: #2e3a48; --accent: #3db8a8; --warn: #d4a017;
}}
* {{ box-sizing: border-box; }}
body {{
  margin: 0; font-family: "Segoe UI", "IBM Plex Sans", system-ui, sans-serif;
  background: radial-gradient(1200px 600px at 10% -10%, #1b3a36 0%, transparent 55%),
              radial-gradient(900px 500px at 100% 0%, #243048 0%, transparent 50%), var(--bg);
  color: var(--text); line-height: 1.45;
}}
header {{
  padding: 2rem 2rem 1.25rem; border-bottom: 1px solid var(--line);
  background: linear-gradient(180deg, rgba(61,184,168,.12), transparent);
}}
header h1 {{ margin: 0 0 .35rem; font-size: 1.75rem; letter-spacing: -.02em; }}
header .meta {{ color: var(--muted); font-size: .95rem; }}
.overall {{
  display: inline-flex; gap: .75rem; align-items: center; margin-top: 1rem;
  padding: .65rem 1rem; background: var(--panel); border: 1px solid var(--line); border-radius: 8px;
}}
main {{ padding: 1.25rem 2rem 3rem; max-width: 1200px; margin: 0 auto; }}
section {{
  background: var(--panel); border: 1px solid var(--line); border-radius: 10px;
  padding: 1.1rem 1.25rem; margin: 1rem 0;
}}
h2 {{ margin: 0 0 .85rem; font-size: 1.15rem; }}
h3 {{ margin: 1rem 0 .5rem; font-size: 1rem; color: var(--muted); font-weight: 600; }}
.grid {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(140px, 1fr)); gap: .75rem; }}
.card {{ background: var(--panel2); border: 1px solid var(--line); border-radius: 8px; padding: .85rem; }}
.card .n {{ font-size: 1.6rem; font-weight: 700; color: var(--accent); }}
.card .l {{ color: var(--muted); font-size: .85rem; }}
table {{ width: 100%; border-collapse: collapse; font-size: .9rem; }}
th, td {{ text-align: left; padding: .45rem .5rem; border-bottom: 1px solid var(--line); vertical-align: top; }}
th {{ color: var(--muted); font-weight: 600; font-size: .8rem; text-transform: uppercase; letter-spacing: .04em; }}
tr.today {{ background: rgba(61,184,168,.08); }}
code, pre {{ font-family: "Cascadia Code", "Consolas", monospace; font-size: .82rem; }}
pre {{
  background: var(--panel2); border: 1px solid var(--line); border-radius: 6px;
  padding: .75rem; overflow: auto; white-space: pre-wrap;
}}
.badge {{
  display: inline-block; padding: .1rem .45rem; border-radius: 999px; font-size: .75rem;
  font-weight: 600; background: #334155; color: #e2e8f0;
}}
.badge.pass {{ background: rgba(47,158,107,.25); color: #8fdfb5; }}
.badge.fail, .badge.blocked {{ background: rgba(212,91,91,.25); color: #f0a0a0; }}
.badge.not.run, .badge.warn, .badge.attention {{ background: rgba(212,160,23,.22); color: #f0d78a; }}
.badge.in-progress {{ background: rgba(74,143,212,.25); color: #9bc4f0; }}
.badge.ready {{ background: rgba(61,184,168,.22); color: #8fe0d4; }}
.badge.idea {{ background: rgba(148,163,184,.2); color: #cbd5e1; }}
.badge.done {{ background: rgba(47,158,107,.2); color: #8fdfb5; }}
.badge.bug {{ background: rgba(212,91,91,.2); color: #f0a0a0; }}
.badge.feature {{ background: rgba(74,143,212,.2); color: #9bc4f0; }}
.badge.debt {{ background: rgba(212,160,23,.2); color: #f0d78a; }}
.badge.problem {{ background: rgba(168,85,247,.22); color: #d8b4fe; }}
.badge.p1 {{ background: rgba(212,91,91,.25); color: #f0a0a0; }}
.badge.p2 {{ background: rgba(212,160,23,.22); color: #f0d78a; }}
.badge.p3 {{ background: rgba(148,163,184,.2); color: #cbd5e1; }}
.muted {{ color: var(--muted); }}
.kv {{ display: grid; grid-template-columns: 180px 1fr; gap: .35rem .75rem; font-size: .92rem; }}
.kv .k {{ color: var(--muted); }}
.note {{
  margin-top: .75rem; padding: .65rem .8rem; border-left: 3px solid var(--accent);
  background: rgba(61,184,168,.08); border-radius: 0 6px 6px 0; font-size: .9rem;
}}
footer {{ color: var(--muted); font-size: .8rem; padding: 0 2rem 2rem; max-width: 1200px; margin: 0 auto; }}
a {{ color: var(--accent); text-decoration: none; }}
a:hover {{ text-decoration: underline; }}
td.num {{ font-variant-numeric: tabular-nums; text-align: right; font-weight: 600; }}
tr.muted-row {{ opacity: .55; }}
.lint-desc {{ color: var(--muted); font-size: .85rem; font-weight: 400; margin-top: .2rem; }}
</style>
</head>
<body>
<header>
  <h1>Plato status report</h1>
  <div class="meta">Generated {e(now.strftime('%Y-%m-%d %H:%M:%S %Z'))} · <code>docs/status-report.html</code> · HEAD <code>{e(head[:7])}</code></div>
  <div class="overall">
    <span>{badge('attention' if overall != 'HEALTHY-ISH' else 'pass', overall)}</span>
    <span>{e(overall_note)}</span>
  </div>
</header>
<main>

<section>
  <h2>Snapshot</h2>
  <div class="grid">
    <div class="card"><div class="n">{e(branch)}</div><div class="l">Branch · {badge('warn' if ahead else 'pass', f'ahead {len(ahead)}' if ahead else 'synced')}</div></div>
    <div class="card"><div class="n">{e(head[:7])}</div><div class="l">HEAD</div></div>
    <div class="card"><div class="n">{e(len(dirty_display))}</div><div class="l">Dirty paths</div></div>
    <div class="card"><div class="n">{e(len(today))}</div><div class="l">Commits today</div></div>
    <div class="card"><div class="n">{e(commits_7d)}</div><div class="l">Commits last 7 days</div></div>
    <div class="card"><div class="n">{e(by_status.get('in-progress', 0))}</div><div class="l">Issues in progress</div></div>
    <div class="card"><div class="n">{latest_err:,}</div><div class="l">Latest C# errors ({e(latest_target)})</div></div>
  </div>
</section>

<section>
  <h2>Git — Plato</h2>
  <div class="kv">
    <div class="k">Status</div><div><code>{e(status_sb)}</code></div>
    <div class="k">HEAD</div><div><code>{e(head)}</code></div>
    <div class="k">origin/main</div><div><code>{e(origin)}</code></div>
    <div class="k">Unpushed</div><div>{ahead_html}</div>
    <div class="k">Dirty (excl. .claude/)</div><div><code>{dirty_html}</code></div>
  </div>
  <h3>Diffstat (unstaged, excl. this report)</h3>
  {diff_html}
  <div class="note">{git_note}</div>
</section>

<section>
  <h2>Git — studio parent</h2>
  <div class="kv">
    <div class="k">Status</div><div><code>{e(studio_sb or '(studio not found)')}</code></div>
    <div class="k">Committed Plato pointer</div><div><code>{e(studio_ptr)}</code> {pointer_badge}</div>
    <div class="k">Plato HEAD</div><div><code>{e(head)}</code></div>
    <div class="k">Plato-related dirty</div><div><code>{studio_dirty_html}</code></div>
  </div>
  <div class="note">If drifted, bump with an explicit <code>git commit -- submodules/Plato</code> in studio after Plato pushes.</div>
</section>

<section>
  <h2>Submodules / worktrees</h2>
  <h3>Inside Plato</h3>
  <div><code>{submod_html}</code></div>
  <p class="muted">parakeet: do not stage casually (known pre-existing dirty grammar submodule).</p>
  <h3>Worktrees</h3>
  <div><code>{worktree_html}</code></div>
  <h3>Studio-level submodules</h3>
  <pre>{e(studio_submods)}</pre>
</section>

<section>
  <h2>Navigation index (cached snapshot)</h2>
  <div class="grid">
    <div class="card"><div class="n">{nav.get('files', 0)}</div><div class="l">Files indexed</div></div>
    <div class="card"><div class="n">{int(nav.get('definitions', 0)):,}</div><div class="l">Definitions</div></div>
    <div class="card"><div class="n">{int(nav.get('references', 0)):,}</div><div class="l">References</div></div>
    <div class="card"><div class="n">{nav.get('diagnostics', 0)}</div><div class="l">Diagnostics</div></div>
  </div>
  <p class="muted" style="margin:.75rem 0 0">Roots: {e(nav.get('roots', '?'))} · built {e(nav.get('builtUtc', '?'))} · snapshot {e(snapshot.get('updated_at', '?'))}</p>
</section>

<section>
  <h2>Linter / build / tests</h2>
  <p class="muted">Gate rows and lint counts come from <code>docs/status-report-snapshot.json</code> (not re-probed on every commit). Update after meaningful gate runs.</p>
  <table>
    <thead><tr><th>Gate</th><th>Result</th><th>Time</th><th>Detail</th></tr></thead>
    <tbody>{''.join(gate_rows)}</tbody>
  </table>

  <h3>C# build errors (latest recorded)</h3>
  <div class="kv" style="margin-bottom:.85rem">
    <div class="k">Latest target</div><div><code>{e(latest_target)}</code> · {cs_latest_badge}</div>
    <div class="k">Errors / warnings</div><div>{latest_err:,} / {latest_warn:,}</div>
    <div class="k">Recorded at</div><div>{e(latest_built)}</div>
    <div class="k">How updated</div><div>Every Plato C# build via <code>tools/dotnet-build-record.ps1</code> (wired into <code>regen-forward-conformance.ps1</code>, <code>regen-generated.ps1</code>). Writes <code>docs/status-report-snapshot.json</code> → this page.</div>
  </div>
  <h3>Recorded build targets</h3>
  <table>
    <thead><tr><th>Target</th><th>Errors</th><th>Warnings</th><th>Result</th><th>When</th></tr></thead>
    <tbody>{''.join(cs_target_rows)}</tbody>
  </table>
  <h3>Latest errors by category</h3>
  <table>
    <thead><tr><th>Category</th><th>Key</th><th>Count</th><th>CS codes</th></tr></thead>
    <tbody>{''.join(cs_category_rows)}</tbody>
  </table>
  <h3>Latest errors by CS code (top 20)</h3>
  <table>
    <thead><tr><th>Code</th><th>Count</th></tr></thead>
    <tbody>{''.join(cs_code_rows)}</tbody>
  </table>

  <h3>Forward-stdlib linter catalog</h3>
  <div class="kv" style="margin-bottom:.85rem">
    <div class="k">Last --strict exit</div><div>{badge('pass' if lint.get('strict_exit', 1) == 0 else 'fail', str(lint.get('strict_exit', '?')))}</div>
    <div class="k">Parse / resolution</div><div>{lint.get('parse_errors', '?')} / {lint.get('resolution_errors', '?')}</div>
    <div class="k">Ratchet</div><div>{lint_ratchet:,} (Error + Warning only) · Warning {lint.get('warnings', '?')} · Error {lint.get('errors', '?')}</div>
    <div class="k">Info (excluded)</div><div>{lint.get('infos', '?')} · not part of the burn-down; vocabulary-shape findings (LINT003/008–011)</div>
    <div class="k">All findings</div><div>{lint_total:,} total · {lint_nonzero} codes firing</div>
    <div class="k">What --strict gates</div><div>Error-severity only (LINT002/004/005E/006/007). Warnings report incomplete work; Infos do not count toward the ratchet.</div>
  </div>
  <table>
    <thead><tr><th>Code</th><th>Severity</th><th>Count</th><th>Meaning</th></tr></thead>
    <tbody>{''.join(lint_rows)}</tbody>
  </table>
  <div class="note">{issue_a('plato-308')}: forward-stdlib generated C# compile errors are tracked above under target <code>forward-conformance</code>. Compare per-category / per-code counts across runs, not only the total. Lint catalog: <code>PlatoCompiler/Analysis/Linter.cs</code>.</div>
</section>

<section>
  <h2>Tracker — Plato issues</h2>
  <div class="grid" style="margin-bottom:1rem">{status_cards}
    <div class="card"><div class="n">{len(issues)}</div><div class="l">Total plato-*</div></div>
    <div class="card"><div class="n">{len(open_bugs)}</div><div class="l">Open bugs</div></div>
  </div>
  <h3>In progress ({len(in_progress)})</h3>
  <table>
    <thead><tr><th>Id</th><th>Title</th><th>Type</th><th>Pri</th><th>Status</th><th>Effort/Risk</th></tr></thead>
    <tbody>{rows_issues(in_progress)}</tbody>
  </table>
  <h3>Ready ({len(ready)})</h3>
  <table>
    <thead><tr><th>Id</th><th>Title</th><th>Type</th><th>Pri</th><th>Status</th><th>Effort/Risk</th></tr></thead>
    <tbody>{rows_issues(ready)}</tbody>
  </table>
  <h3>Open bugs</h3>
  <table>
    <thead><tr><th>Id</th><th>Title</th><th>Type</th><th>Pri</th><th>Status</th><th>Effort/Risk</th></tr></thead>
    <tbody>{rows_issues(open_bugs)}</tbody>
  </table>
  <h3>Open design problems</h3>
  <table>
    <thead><tr><th>Id</th><th>Title</th><th>Type</th><th>Pri</th><th>Status</th><th>Effort/Risk</th></tr></thead>
    <tbody>{rows_issues(open_problems)}</tbody>
  </table>
  <h3>Recently closed</h3>
  <table>
    <thead><tr><th>Id</th><th>Title</th><th>Type</th><th>Pri</th><th>Status</th><th>Closed</th></tr></thead>
    <tbody>
      {''.join(
        f"<tr><td>{issue_a(i['id'])}</td>"
        f'<td><a href="{e(issue_href(i["id"]))}">{e(i["title"])}</a></td>'
        f"<td>{badge('type', i['type'])}</td><td>{badge('prio', i['priority'])}</td>"
        f"<td>{badge('done', 'done')}</td><td>{e(i.get('closed', ''))}</td></tr>"
        for i in done_recent
      ) or '<tr><td colspan="6" class="muted">None</td></tr>'}
    </tbody>
  </table>
  <p class="muted">{tracker_note}</p>
</section>

<section>
  <h2>Committed work today ({len(today)} commits)</h2>
  <table>
    <thead><tr><th>Hash</th><th>When</th><th>Subject</th></tr></thead>
    <tbody>{''.join(today_rows) or '<tr><td colspan="3" class="muted">None</td></tr>'}</tbody>
  </table>
</section>

<section>
  <h2>Recent history (20)</h2>
  <table>
    <thead><tr><th>Hash</th><th>When</th><th>Subject</th></tr></thead>
    <tbody>{''.join(recent_rows)}</tbody>
  </table>
</section>

<section>
  <h2>Watchouts</h2>
  <ul>
    <li>Shared working tree: commit by explicit pathspec only; do not <code>git add -A</code>.</li>
    <li>Do not stage <code>parakeet/</code> or the Plato <code>.claude/</code> tree.</li>
    <li>Bump studio submodule pointer as its own commit after Plato pushes.</li>
    <li>Several <code>in-progress</code> ideas may lack priority/effort — triage via studio <code>tools/track.py</code>.</li>
    <li>Refresh <code>docs/status-report-snapshot.json</code> after gate runs so lint/build rows stay honest.</li>
  </ul>
</section>

</main>
<footer>
  Tracked report · <code>docs/status-report.html</code> · generator <code>tools/gen-status-report.py</code>
  · hook <code>tools/githooks/pre-commit</code> · install: <code>powershell tools/install-githooks.ps1</code>
</footer>
</body>
</html>
"""

    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(doc, encoding="utf-8")
    if not SNAPSHOT.is_file():
        snap = dict(DEFAULT_SNAPSHOT)
        snap["updated_at"] = now.isoformat(timespec="seconds")
        SNAPSHOT.write_text(json.dumps(snap, indent=2) + "\n", encoding="utf-8")

    meta = {
        "generated_at": now.isoformat(timespec="seconds"),
        "path": str(OUT),
        "head": head,
        "dirty": len(dirty_display),
        "in_progress": len(in_progress),
        "today_commits": len(today),
        "studio": str(STUDIO) if STUDIO else None,
    }
    print(json.dumps(meta))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
