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


class NamePageTests(unittest.TestCase):
    def test_reordering_declarations_keeps_name_page_names(self):
        one = 'type Alpha { X: Integer; }\ntype Beta { Y: Integer; }\n'
        two = 'type Beta { Y: Integer; }\ntype Alpha { X: Integer; }\n'
        names = lambda text: sorted(build.by_name(build.slices(text)))
        self.assertEqual(names(one), names(two))

    def test_reordering_declarations_keeps_generated_paths(self):
        # Page paths come only from the lower-cased name, never from position.
        for text in ('type Alpha { X: Integer; }\ntype Beta { Y: Integer; }\n',
                     'type Beta { Y: Integer; }\ntype Alpha { X: Integer; }\n'):
            paths = sorted(f'{name.lower()}.md' for name in build.by_name(build.slices(text)))
            self.assertEqual(paths, ['alpha.md', 'beta.md'])

    def test_overloads_share_one_name(self):
        text = 'library L {\n    F(x: Integer): Integer => x;\n    F(x: Number): Number => x;\n}\n'
        grouped = build.by_name(build.slices(text))
        self.assertEqual(list(grouped), ['F'])
        self.assertEqual(len(grouped['F']), 2)

    def test_case_only_collision_fails_naming_file_and_names(self):
        with self.assertRaisesRegex(ValueError, r'a\.plato.*Foo.*FOO'):
            build.check_names('a.plato', ['Foo', 'FOO'])

    def test_reserved_names_fail(self):
        for name in ('Readme', 'Index', 'CON', 'Nul', 'Com1', 'LPT9'):
            with self.assertRaisesRegex(ValueError, 'reserved'):
                build.check_names('a.plato', [name])

    def test_ordinary_names_pass(self):
        build.check_names('a.plato', ['Vector', 'Vec3', 'Console'])

    def test_dangling_alias_fails(self):
        original = build.ALIASES
        with tempfile.TemporaryDirectory() as folder:
            build.ALIASES = Path(folder) / 'aliases.json'
            try:
                build.ALIASES.write_text('{"docs/reference/api/x/old/001-a": "docs/reference/api/x/new/a"}')
                with self.assertRaisesRegex(ValueError, 'alias targets'):
                    build.load_aliases({})
                page = build.ROOT / 'docs/reference/api/x/new/a.md'
                self.assertEqual(len(build.load_aliases({page: ''})), 1)
            finally:
                build.ALIASES = original


class FixtureCheckTests(unittest.TestCase):
    def tree(self, folder, page, fixture):
        docs = Path(folder)
        (docs / 'cookbook').mkdir(parents=True)
        (docs / 'implementation/examples/cookbook-r').mkdir(parents=True)
        (docs / 'cookbook/r.md').write_text(page, encoding='utf-8')
        (docs / 'implementation/examples/cookbook-r/main.plato').write_bytes(fixture.encode('utf-8'))
        return docs

    def test_copied_fence_passes_despite_line_endings_and_trailing_space(self):
        with tempfile.TemporaryDirectory() as folder:
            docs = self.tree(folder, '# R\n\n```plato\nF(x: Integer): Integer => x;  \n```\n',
                             'library L {\r\n}\r\nF(x: Integer): Integer => x;\r\n')
            self.assertEqual(build.fixture_violations(docs), [])

    def test_altered_fence_fails_with_page_and_line(self):
        with tempfile.TemporaryDirectory() as folder:
            docs = self.tree(folder, '# R\n\n```plato\nF(x: Integer): Integer => x + 1;\n```\n',
                             'F(x: Integer): Integer => x;\n')
            problems = build.fixture_violations(docs)
            self.assertEqual(len(problems), 1)
            self.assertIn('cookbook/r.md:3', problems[0])

    def test_unchecked_fence_and_other_languages_are_exempt(self):
        with tempfile.TemporaryDirectory() as folder:
            docs = self.tree(folder, '# R\n\n```plato unchecked\nanything\n```\n\n```csharp\nvar x;\n```\n', '')
            self.assertEqual(build.fixture_violations(docs), [])

    def test_start_pages_are_checked_only_when_tour_exists(self):
        with tempfile.TemporaryDirectory() as folder:
            docs = Path(folder)
            (docs / 'start').mkdir()
            (docs / 'start/a.md').write_text('# A\n\n```plato\nnot a fixture\n```\n', encoding='utf-8')
            self.assertEqual(build.fixture_violations(docs), [])
            (docs / 'implementation/examples/tour').mkdir(parents=True)
            self.assertEqual(len(build.fixture_violations(docs)), 1)



class ReaderDataTests(unittest.TestCase):
    def test_doors_and_status_follow_the_folder(self):
        self.assertEqual(build.page_door([]), 'home')
        self.assertEqual(build.page_door(['start', 'tour']), 'tour')
        self.assertEqual(build.page_door(['compare']), 'tour')
        self.assertEqual(build.page_door(['api', 'foundation', 'x']), 'reference')
        self.assertEqual(build.page_door(['cookbook']), 'cookbook')
        self.assertEqual(build.page_door(['formal']), 'design')
        self.assertEqual(build.page_status(['api', 'future', 'x']), 'Experimental')
        self.assertEqual(build.page_status(['library', 'legacy']), 'Legacy')
        self.assertEqual(build.page_status(['library']), None)
        self.assertEqual(build.page_status(['language']), None)

    def test_split_keeps_every_page_and_attaches_declarations(self):
        bundle = {'sourceRevision': 'abc', 'aliases': {'old': 'new'},
                  'pages': [{'id': 'docs/reference/api/foundation/f/abs', 'path': 'p.md', 'title': 'f: Abs',
                             'group': ['api', 'foundation', 'f'], 'markdown': 'm', 'rawMarkdown': 'r'}],
                  'symbols': {'Abs': [{'id': 'docs/reference/api/foundation/f/abs', 'title': 'f: Abs'}]}}
        records = [{'page': 'docs/reference/api/foundation/f/abs',
                    'declarations': [{'line': 1, 'declaration': 'Abs(x: Number): Number'}]}]
        files = build.split_bundle(bundle, records)
        index = files['reference-index.json']
        self.assertEqual(index['schemaVersion'], 2)
        self.assertEqual(index['pages'][0]['door'], 'reference')
        self.assertEqual(index['pages'][0]['status'], 'Supported')
        self.assertNotIn('markdown', index['pages'][0])
        self.assertEqual(index['symbols']['Abs'][0]['declarations'], ['Abs(x: Number): Number'])
        self.assertEqual(files['pages/docs/reference/api/foundation/f/abs.json']['markdown'], 'm')
        self.assertEqual(files['search.json']['pages'][0]['text'], 'r')
        self.assertIn('groups', files['examples.json'])

if __name__ == '__main__':
    unittest.main()
