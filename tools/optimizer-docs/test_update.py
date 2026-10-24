"""Exercise the freshness contract without requiring a compiler or a Greenhouse install."""
import importlib.util
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

spec = importlib.util.spec_from_file_location("optimizer_docs", Path(__file__).with_name("update.py"))
docs = importlib.util.module_from_spec(spec)
spec.loader.exec_module(docs)


class FreshnessTests(unittest.TestCase):
    def test_check_rejects_stale_evidence_even_after_map_render(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            here = root / "tools"
            pages = root / "docs"
            here.mkdir()
            pages.mkdir()
            (root / "Pass.cs").write_text("class Pass {}\n")
            (here / "manifest.json").write_text("{}\n")
            (here / "update.py").write_text("# generator\n")
            (here / "MAINTENANCE.md").write_text("# Runbook\n")
            (pages / "README.md").write_text("# Guide\n")
            (pages / "source-map.md").write_text(docs.render([]), encoding="utf-8")
            manifest = {"source_globs": ["*.cs"], "pages": ["README.md"]}
            hashes = docs.inputs(root, manifest)
            docs.write_json(here / "evidence.json", {"inputs": hashes, "symbols": [], "manifest_hash": docs.digest(here / "manifest.json")})
            docs.write_json(here / "review.json", {"inputs": hashes, "evidence_hash": docs.digest(here / "evidence.json"), "generator_hash": docs.digest(here / "update.py")})
            with patch.object(docs, "ROOT", root), patch.object(docs, "HERE", here), patch.object(docs, "DOCS", pages):
                docs.check(manifest)
                (root / "Pass.cs").write_text("class Pass { int Run() => 2; }\n")
                (pages / "source-map.md").write_text(docs.render([]), encoding="utf-8")
                with self.assertRaisesRegex(ValueError, "Greenhouse evidence is stale"):
                    docs.check(manifest)

    def test_body_edits_additions_and_deletions_invalidate_inputs(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / "Pass.cs"
            source.write_text("public int Run() => 1;\n")
            manifest = {"source_globs": ["*.cs"]}
            before = docs.inputs(root, manifest)
            source.write_text("public int Run() => 2;\n")
            self.assertEqual(["Pass.cs"], docs.changed(before, docs.inputs(root, manifest)))
            added = root / "NewPass.cs"
            added.write_text("class NewPass {}\n")
            self.assertEqual(["NewPass.cs", "Pass.cs"], docs.changed(before, docs.inputs(root, manifest)))
            source.unlink()
            self.assertEqual(["NewPass.cs", "Pass.cs"], docs.changed(before, docs.inputs(root, manifest)))

    def test_line_endings_do_not_invalidate_review(self):
        with tempfile.TemporaryDirectory() as directory:
            source = Path(directory) / "Pass.cs"
            source.write_bytes(b"class A {}\r\n")
            before = docs.digest(source)
            source.write_bytes(b"class A {}\n")
            self.assertEqual(before, docs.digest(source))

    def test_missing_scope_and_incomplete_export_fail_closed(self):
        with tempfile.TemporaryDirectory() as directory:
            with self.assertRaisesRegex(ValueError, "no matches"):
                docs.inputs(Path(directory), {"source_globs": ["*.cs"]})
            with self.assertRaisesRegex(ValueError, "missing required types"):
                docs.extract({"types": []}, Path(directory),
                             {"symbol_directory": "src/", "required_types": ["Pass"]})

    def test_export_normalizes_paths_and_omits_volatile_metadata(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            payload = {"generatedAt": "volatile", "types": [{"FilePath": str(root / "src/Pass.cs"),
                       "TypeName": "Pass", "FullyQualifiedName": "N.Pass", "StartLine": 2,
                       "members": [{"Signature": "N.Pass.Run()", "FilePath": str(root / "src/Pass.cs"), "StartLine": 3}]}]}
            symbols = docs.extract(payload, root, {"symbol_directory": "src/", "required_types": ["Pass"]})
            rendered = docs.render(symbols)
            self.assertIn("../../src/Pass.cs#L3", rendered)
            self.assertNotIn(directory, rendered)
            self.assertNotIn("volatile", rendered)

    def test_broken_links_and_anchors_are_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            page = root / "guide.md"
            page.write_text("# Valid heading\n[ok](#valid-heading) [bad](missing.cs) [bad](#absent)\n")
            self.assertEqual(2, len(docs.link_errors([page])))


if __name__ == "__main__":
    unittest.main()
