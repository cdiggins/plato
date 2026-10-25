"""Tests for evidence preservation and reference navigation integrity."""
import importlib.util
import tempfile
import unittest
from pathlib import Path

spec = importlib.util.spec_from_file_location('reference_build', Path(__file__).with_name('build.py'))
build = importlib.util.module_from_spec(spec)
spec.loader.exec_module(build)


class SourceEvidenceTests(unittest.TestCase):
    def partition(self, text):
        parts = build.slices(text)
        self.assertEqual(''.join(text[start:end] for _, start, end, _ in parts), text)
        self.assertEqual(parts[0][1], 0)
        self.assertEqual(parts[-1][2], len(text))
        return parts

    def test_comments_strings_and_nested_bodies_do_not_create_fake_members(self):
        text = '''// type Fake { }
library Real {
    // A string with } and a fake signature.
    First(x: Integer): Integer {
        var s = "not a } // comment";
        if (x == 0) { return 0; }
        return x;
    }
    // Second member's explanation belongs to it.
    Second(x: Integer): Integer => x;
}
'''
        parts = self.partition(text)
        self.assertEqual([name for name, *_ in parts], ['First', 'Second'])
        self.assertIn("Second member's explanation", text[parts[1][1]:parts[1][2]])

    def test_generic_sum_and_product_are_distinct_entries(self):
        text = '''type Maybe<T> = Just(Value: T) | Nothing;
// A record, following a sum.
type Pair<T> { A: T; B: T; }
'''
        self.assertEqual([p[0] for p in self.partition(text)], ['Maybe', 'Pair'])

    def test_multiline_function_calls_are_not_declarations(self):
        text = '''library Demo {
    First(x: Integer): Integer =>
        Second(x);
    Second(x: Integer): Integer => x;
}
'''
        self.assertEqual([p[0] for p in self.partition(text)], ['First', 'Second'])

    def test_unbalanced_braces_fail_instead_of_publishing_partial_evidence(self):
        with self.assertRaisesRegex(ValueError, 'unbalanced'):
            build.slices('type Broken { X: Integer;')

    def test_no_declarations_fails_instead_of_empty_success(self):
        with self.assertRaisesRegex(ValueError, 'no top-level'):
            build.slices('// placeholder only\n')

    def test_links_in_code_are_not_document_navigation(self):
        text = '[Real](../README.md)\n\n```plato\n[Fake](../missing.md)\n```\n'
        links = list(build.local_links(build.DOCS / 'language/model.md', text))
        self.assertEqual(len(links), 1)
        self.assertEqual(links[0][1], build.DOCS / 'README.md')

    def test_link_offsets_after_code_remain_valid_in_original_markdown(self):
        text = '# Title\n\n```plato\n[Fake](../missing.md)\n```\n\n[Real](../README.md)\n'
        links = list(build.local_links(build.DOCS / 'language/model.md', text))
        self.assertEqual(len(links), 1)
        match, target, _ = links[0]
        self.assertEqual(text[match.start():match.end()], '[Real](../README.md)')
        self.assertEqual(target, build.DOCS / 'README.md')

    def test_link_cannot_escape_repository(self):
        with self.assertRaisesRegex(ValueError, 'escapes repository'):
            list(build.local_links(build.ROOT / 'README.md', '[Bad](../../../../outside.md)'))

    def test_source_fingerprints_detect_semantic_and_comment_changes(self):
        self.assertNotEqual(build.digest('x + y'), build.digest('x - y'))
        self.assertNotEqual(build.digest('// requires x > 0'), build.digest('// requires x >= 0'))


if __name__ == '__main__':
    unittest.main()
