"""Generate Greenhouse source evidence; require explicit review before accepting new inputs."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys
from urllib.parse import unquote

ROOT = Path(__file__).resolve().parents[2]
HERE = ROOT / "tools/optimizer-docs"
DOCS = ROOT / "docs/optimizer"


def read_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def write_json(path, value):
    path.write_text(json.dumps(value, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def digest(path):
    # Git checkouts may use CRLF; line-ending conversion must not invalidate review.
    return hashlib.sha256(path.read_text(encoding="utf-8-sig").replace("\r\n", "\n").encode()).hexdigest()


def inputs(root, manifest):
    paths = set()
    for pattern in manifest["source_globs"]:
        matches = list(root.glob(pattern))
        if not matches:
            raise ValueError(f"Source pattern has no matches: {pattern}")
        paths.update(p for p in matches if p.is_file())
    return {p.relative_to(root).as_posix(): digest(p) for p in sorted(paths)}


def changed(old, new):
    return [key for key in sorted(old.keys() | new.keys()) if old.get(key) != new.get(key)]


def extract(payload, root, manifest):
    result = []
    for item in payload["types"]:
        path = Path(item["FilePath"]).resolve().relative_to(root.resolve()).as_posix()
        if not path.startswith(manifest["symbol_directory"]):
            continue
        members = []
        for member in item["members"]:
            member_path = Path(member["FilePath"]).resolve().relative_to(root.resolve()).as_posix()
            members.append({"signature": member["Signature"], "path": member_path, "line": member["StartLine"]})
        result.append({"name": item["TypeName"], "qualified_name": item["FullyQualifiedName"],
                       "path": path, "line": item["StartLine"],
                       "members": sorted(members, key=lambda m: (m["signature"], m["path"], m["line"]))})
    missing = set(manifest["required_types"]) - {item["name"] for item in result}
    if missing:
        raise ValueError(f"Greenhouse export is missing required types: {sorted(missing)}")
    return sorted(result, key=lambda item: item["qualified_name"])


def render(symbols):
    lines = ["# Optimizer source map", "", "[Overview](README.md)", "",
             "Generated from dotnet-greenhouse's Roslyn documentation export. These are public",
             "symbols, not a complete call graph or a substitute for the component explanations.",
             "Private algorithms are reached through the linked implementation files. Source line",
             "links refer to this checkout's reviewed snapshot.", ""]
    for item in symbols:
        lines.extend([f"## {item['name']}", "",
                      f"[`{item['qualified_name']}`](../../{item['path']}#L{item['line']})", ""])
        for member in item["members"]:
            signature = member["signature"].replace("`", "'")
            lines.append(f"- [`{signature}`](../../{member['path']}#L{member['line']})")
        lines.append("")
    return "\n".join(lines)


def link_errors(pages):
    errors = []
    for page in pages:
        text = page.read_text(encoding="utf-8")
        for target in re.findall(r"\]\(([^)]+)\)", text):
            if re.match(r"[a-z]+://", target):
                continue
            path, _, fragment = unquote(target).partition("#")
            destination = (page.parent / path).resolve() if path else page
            if not destination.exists():
                errors.append(f"{page.name}: missing {target}")
            elif fragment and destination.is_file():
                contents = destination.read_text(encoding="utf-8-sig")
                if re.fullmatch(r"L\d+", fragment):
                    if not 1 <= int(fragment[1:]) <= len(contents.splitlines()):
                        errors.append(f"{page.name}: invalid line {target}")
                elif destination.suffix == ".md":
                    headings = re.findall(r"^#+\s+(.+)$", contents, re.M)
                    anchors = {re.sub(r"[^\w\- ]", "", h.lower()).replace(" ", "-") for h in headings}
                    if fragment not in anchors:
                        errors.append(f"{page.name}: missing heading {target}")
    return errors


def check(manifest):
    evidence = read_json(HERE / "evidence.json")
    review = read_json(HERE / "review.json")
    current = inputs(ROOT, manifest)
    errors = []
    for label, previous in [("Greenhouse evidence", evidence["inputs"]), ("Prose review", review["inputs"])]:
        differences = changed(previous, current)
        if differences:
            errors.append(label + " is stale:\n  " + "\n  ".join(differences))
    if evidence["manifest_hash"] != digest(HERE / "manifest.json"):
        errors.append("Manifest changed: regenerate Greenhouse evidence.")
    if review["evidence_hash"] != digest(HERE / "evidence.json"):
        errors.append("Evidence changed: review the guides and accept the review.")
    if review["generator_hash"] != digest(HERE / "update.py"):
        errors.append("Generator changed: regenerate and review its output.")
    if (DOCS / "source-map.md").read_text(encoding="utf-8") != render(evidence["symbols"]):
        errors.append("source-map.md differs from its generated evidence; run render.")
    pages = [DOCS / name for name in manifest["pages"]] + [DOCS / "source-map.md", HERE / "MAINTENANCE.md"]
    errors.extend(link_errors(pages))
    if errors:
        raise ValueError("\n".join(errors) + "\nSee tools/optimizer-docs/MAINTENANCE.md.")
    print("Optimizer documentation: source evidence, review, generated map and local links are current.")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("command", choices=["refresh", "render", "accept-review", "check"])
    parser.add_argument("--greenhouse", type=Path, help="Path to a built Greenhouse.Cli.dll")
    parser.add_argument("--note", help="What changed in the guides, or why no prose change was needed")
    args = parser.parse_args()
    manifest = read_json(HERE / "manifest.json")
    if args.command == "refresh":
        if not args.greenhouse or not args.greenhouse.is_file():
            parser.error("refresh requires --greenhouse pointing to a built Greenhouse.Cli.dll")
        before = inputs(ROOT, manifest)
        scratch = ROOT / ".temp/optimizer-docs"
        scratch.mkdir(parents=True, exist_ok=True)
        command = ["dotnet", str(args.greenhouse.resolve()), "docs"]
        subprocess.run(command + ["build", manifest["project"], str(scratch / "api.db")], cwd=ROOT, check=True)
        subprocess.run(command + ["export", str(scratch / "api.db"), str(scratch / "api.json"), "--format", "json"], cwd=ROOT, check=True)
        symbols = extract(read_json(scratch / "api.json"), ROOT, manifest)
        if inputs(ROOT, manifest) != before:
            raise ValueError("Source changed during export; retry before accepting evidence.")
        write_json(HERE / "evidence.json", {"producer": "dotnet-greenhouse docs build/export --format json",
                   "tool_sha256": hashlib.sha256(args.greenhouse.read_bytes()).hexdigest(),
                   "manifest_hash": digest(HERE / "manifest.json"), "inputs": before, "symbols": symbols})
        (DOCS / "source-map.md").write_text(render(symbols), encoding="utf-8")
        print("Refreshed source map. Review component prose, then run accept-review --note ...")
    elif args.command == "render":
        (DOCS / "source-map.md").write_text(render(read_json(HERE / "evidence.json")["symbols"]), encoding="utf-8")
    elif args.command == "accept-review":
        if not args.note or not args.note.strip():
            parser.error("accept-review requires a meaningful --note")
        evidence = read_json(HERE / "evidence.json")
        current = inputs(ROOT, manifest)
        if current != evidence["inputs"] or evidence["manifest_hash"] != digest(HERE / "manifest.json"):
            raise ValueError("Refresh Greenhouse evidence before accepting a prose review.")
        errors = link_errors([DOCS / name for name in manifest["pages"]])
        if errors:
            raise ValueError("\n".join(errors))
        write_json(HERE / "review.json", {"note": args.note, "inputs": current,
                   "evidence_hash": digest(HERE / "evidence.json"), "generator_hash": digest(HERE / "update.py")})
        check(manifest)
    else:
        check(manifest)


if __name__ == "__main__":
    try:
        main()
    except (ValueError, OSError, KeyError, subprocess.CalledProcessError) as error:
        print(str(error), file=sys.stderr)
        sys.exit(1)
