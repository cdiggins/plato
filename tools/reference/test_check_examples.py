"""Unit tests for fixture discovery and input resolution; they do not need dotnet."""
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import check_examples as ce


class FixtureTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.addCleanup(self._tmp.cleanup)
        self.root = Path(self._tmp.name).resolve()
        self.fixtures = self.root / 'examples'
        (self.fixtures / 'prelude').mkdir(parents=True)

    def group(self, name, source='function Law_ReferenceA() => true;'):
        folder = self.fixtures / name
        folder.mkdir()
        if source is not None:
            (folder / 'a.plato').write_text(source, encoding='utf-8')
        return folder

    def test_discovery_skips_prelude_and_files_and_sorts(self):
        self.group('b')
        self.group('a')
        (self.fixtures / 'README.md').write_text('x')
        self.assertEqual(ce.discover_groups(self.fixtures), ['a', 'b'])

    def test_prelude_is_the_default_input(self):
        folder = self.group('a')
        self.assertEqual(ce.resolve_inputs(folder, self.fixtures, self.root), [self.fixtures / 'prelude'])

    def test_inputs_txt_overrides_prelude(self):
        (self.root / 'lib').mkdir()
        folder = self.group('a')
        (folder / 'inputs.txt').write_text('# note\n\nlib\n', encoding='utf-8')
        self.assertEqual(ce.resolve_inputs(folder, self.fixtures, self.root), [self.root / 'lib'])

    def test_missing_input_fails(self):
        folder = self.group('a')
        (folder / 'inputs.txt').write_text('nope\n', encoding='utf-8')
        with self.assertRaisesRegex(ValueError, 'does not exist'):
            ce.resolve_inputs(folder, self.fixtures, self.root)

    def test_input_outside_repo_fails(self):
        folder = self.group('a')
        (folder / 'inputs.txt').write_text('..\n', encoding='utf-8')
        with self.assertRaisesRegex(ValueError, 'outside'):
            ce.resolve_inputs(folder, self.fixtures, self.root)

    def test_empty_group_fails(self):
        with self.assertRaisesRegex(ValueError, 'no .plato file'):
            ce.count_laws(self.group('a', source=None))

    def test_group_without_laws_fails(self):
        with self.assertRaisesRegex(ValueError, 'no reference laws'):
            ce.count_laws(self.group('a', source='function F() => 1;'))

    def test_laws_are_counted(self):
        self.assertEqual(ce.count_laws(self.group('a')), 1)


if __name__ == '__main__':
    unittest.main()
