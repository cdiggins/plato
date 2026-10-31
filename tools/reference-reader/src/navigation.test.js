import test from 'node:test';
import assert from 'node:assert/strict';
import {
  DOORS, adjacentPages, buildDoorTrees, buildPageTree, buildSearchIndex, buildSymbolIndex, commitUrl, crumbTarget,
  currentDoor, doorRoute, extractHeadings, groupAncestors, humanizeSegment, pageDataUrl, parseHash, parseRoute,
  playgroundGroupFor, playgroundRoute, resolveAlias, routeFor, searchPages, searchPagesDetailed, searchSymbols,
  shortRevision, slugifyHeading, stripLeadingTitle, treeEntries,
} from './navigation.js';

const page = (id, title, group, door, status = null) => ({ id, path: `${id}.md`, title, group, door, status });
const root = 'docs/reference';
const pages = [
  page(`${root}/README`, 'Plato reference', [], 'home'),
  page(`${root}/language/types/vector`, 'Vector Model', ['language', 'types'], 'reference'),
  page(`${root}/language/model`, 'Programs, values, and names', ['language'], 'reference'),
  page(`${root}/language/diagnostics`, 'Diagnostics', ['language'], 'reference'),
  page(`${root}/language/syntax`, 'Reading and writing source', ['language'], 'reference'),
  page(`${root}/library/README`, 'Library guide', ['library'], 'reference', 'Supported'),
  page(`${root}/api/foundation/vectors.library`, 'vectors.library', ['api', 'foundation'], 'reference', 'Supported'),
  page(`${root}/api/foundation/vectors.library/length`, 'vectors.library: Length', ['api', 'foundation', 'vectors.library'], 'reference', 'Supported'),
  page(`${root}/start/getting-started`, 'Getting started', ['start'], 'tour'),
  page(`${root}/start/tour`, 'Quick tour', ['start'], 'tour'),
  page(`${root}/start/tour/functions`, 'Functions', ['start', 'tour'], 'tour'),
  page(`${root}/start/what-is-plato`, 'What is Plato', ['start'], 'tour'),
  page(`${root}/cookbook/README`, 'Cookbook', ['cookbook'], 'cookbook'),
  page(`${root}/cookbook/fit-line`, 'Fit a line', ['cookbook'], 'cookbook'),
  page(`${root}/design/README`, 'Design', ['design'], 'design'),
  page(`${root}/evidence/grammar-source`, 'Grammar source', ['evidence'], 'design'),
];
const text = [
  { id: `${root}/language/types/vector`, text: '# Vector Model\nA vector has components.' },
  { id: `${root}/start/tour`, text: '# Quick tour\nMeasurement example.' },
];

test('routes retain page ids and heading slugs', () => {
  assert.deepEqual(parseHash('#/docs/reference/model@coordinate-frames'), { id: 'docs/reference/model', heading: 'coordinate-frames' });
  assert.deepEqual(parseHash('#/bad%ZZ@heading%ZZ'), { id: 'bad%ZZ', heading: 'heading%ZZ' });
  assert.equal(routeFor('docs/a', 'coordinate-frames'), '#/docs/a@coordinate-frames');
  assert.equal(slugifyHeading('Coordinate Frames & Units'), 'coordinate-frames-units');
  assert.deepEqual(parseRoute('#/docs/a@b'), { kind: 'page', id: 'docs/a', heading: 'b' });
  assert.deepEqual(parseRoute(''), { kind: 'page', id: null, heading: null });
});

test('playground routes carry a group and an opaque payload', () => {
  assert.deepEqual(parseRoute('#/playground'), { kind: 'playground', group: null, payload: null });
  assert.deepEqual(parseRoute('#/playground/'), { kind: 'playground', group: null, payload: null });
  assert.deepEqual(parseRoute('#/playground/tour'), { kind: 'playground', group: 'tour', payload: null });
  assert.deepEqual(parseRoute('#/playground/cookbook-fit-line~a%7Eb~c'), { kind: 'playground', group: 'cookbook-fit-line', payload: 'a%7Eb~c' });
  assert.deepEqual(parseRoute('#/playgroundx/y'), { kind: 'page', id: 'playgroundx/y', heading: null });
  assert.equal(playgroundRoute(), '#/playground');
  assert.equal(playgroundRoute('tour'), '#/playground/tour');
  assert.equal(playgroundRoute('tour', 'abc'), '#/playground/tour~abc');
});

test('fences open the group the contract names', () => {
  assert.equal(playgroundGroupFor(`${root}/start/tour`), 'tour');
  assert.equal(playgroundGroupFor(`${root}/start/tour/sums`), 'tour');
  assert.equal(playgroundGroupFor(`${root}/cookbook/fit-line`), 'cookbook-fit-line');
  assert.equal(playgroundGroupFor(`${root}/cookbook/README`), null);
  assert.equal(playgroundGroupFor(`${root}/start/what-is-plato`), null);
  assert.equal(playgroundGroupFor(`${root}/language/model`), null);
});

test('doors land on the pages the guidance names', () => {
  assert.deepEqual(DOORS.map(door => door.label), ['Tour', 'Reference', 'Cookbook', 'Design', 'Playground']);
  assert.deepEqual(DOORS.map(doorRoute), [
    '#/docs/reference/start/tour', '#/docs/reference/language/README', '#/docs/reference/cookbook/README',
    '#/docs/reference/design/README', '#/playground',
  ]);
  assert.equal(currentDoor({ kind: 'playground' }, null), 'playground');
  assert.equal(currentDoor({ kind: 'page' }, pages[1]), 'reference');
  assert.equal(currentDoor({ kind: 'page' }, pages[0]), null);
});

test('page data urls encode each segment and revisions link to the commit', () => {
  assert.equal(pageDataUrl('/x/', 'docs/reference/a b/c#d'), '/x/pages/docs/reference/a%20b/c%23d.json');
  assert.equal(shortRevision('c33c50760ef81c63'), 'c33c507');
  assert.equal(commitUrl('abc'), 'https://github.com/cdiggins/plato/commit/abc');
});

test('heading outline skips fenced examples and title removal handles generated comments', () => {
  const markdown = '<!-- generated page -->\n# Page title\n\n## Visible\n\n```plato\n# Not a heading\n```\n\n### Nested\n';
  assert.deepEqual(extractHeadings(markdown), [
    { level: 1, text: 'Page title', slug: 'page-title' },
    { level: 2, text: 'Visible', slug: 'visible' },
    { level: 3, text: 'Nested', slug: 'nested' },
  ]);
  assert.equal(stripLeadingTitle(markdown), '## Visible\n\n```plato\n# Not a heading\n```\n\n### Nested\n');
});

test('the tree groups pages under doors in header order', () => {
  const trees = buildDoorTrees(pages);
  assert.deepEqual(trees.map(item => item.door.key), ['tour', 'reference', 'cookbook', 'design']);
  const reference = trees[1].tree;
  assert.deepEqual([...reference.children.keys()], ['language', 'library', 'api']);
  assert.equal(humanizeSegment('api'), 'API catalog');
  assert.deepEqual(reference.children.get('language').pages.map(p => p.id.split('/').at(-1)), ['model', 'syntax', 'diagnostics']);
  const vectors = reference.children.get('api').children.get('foundation').children.get('vectors.library');
  assert.equal(vectors.indexPage.id, `${root}/api/foundation/vectors.library`);
  assert.deepEqual(groupAncestors(pages[1]), ['language', 'language/types']);
});

test('a folder sits at its place in the section order', () => {
  const start = buildPageTree(pages.filter(p => p.door === 'tour')).children.get('start');
  assert.deepEqual(treeEntries(start).map(entry => entry.key), ['what-is-plato', 'tour', 'getting-started']);
});

test('title search is immediate; body search joins once the text arrives', () => {
  assert.deepEqual(searchPages(pages, 'measurement').map(p => p.id), []);
  const index = buildSearchIndex(text);
  assert.deepEqual(searchPages(pages, 'vector components', index).map(p => p.id), [`${root}/language/types/vector`]);
  const detailed = searchPagesDetailed(pages, 'Measurement', index);
  assert.equal(detailed[0].page.id, `${root}/start/tour`);
  assert.match(detailed[0].snippet, /Measurement/i);
  assert.deepEqual(searchPages(pages, '   '), []);
  assert.equal(searchPages(pages, 'length')[0].id, `${root}/api/foundation/vectors.library/length`);
});

const symbols = {
  Length: [
    { id: 'a/length', title: 'vectors.library: Length', declarations: ['library Vectors', 'Length(self:   Vector3D): Number', 'Length(self: Number2): Number'] },
    { id: 'legacy/length', title: 'vectors: Length', declarations: ['Length(a: IVector): Number'] },
  ],
  LengthSquared: [{ id: 'a/lengthsquared', title: 'vectors.library: LengthSquared', declarations: ['LengthSquared(self: Vector3D): Number'] }],
  Unlength: [{ id: 'a/unlength', title: 'x: Unlength', declarations: ['Unlength(a: Number): Number'] }],
  Cross: [{ id: 'a/cross', title: 'vectors.library: Cross', declarations: ['Cross(a: Vector3D, b: Vector3D): Vector3D'] }],
};

test('symbol search ranks exact names, then prefixes, then substrings', () => {
  const rows = buildSymbolIndex(symbols);
  const byName = searchSymbols(rows, 'length').results;
  assert.deepEqual(byName.map(r => [r.name, r.rank]), [['Length', 0], ['Length', 0], ['LengthSquared', 1], ['Unlength', 2]]);
  assert.equal(byName[0].entry.id, 'a/length');
});

test('symbol search finds a signature shape through normalised whitespace', () => {
  const rows = buildSymbolIndex(symbols);
  const found = searchSymbols(rows, 'Vector3D):   Number');
  assert.deepEqual(found.results.map(r => r.entry.id), ['a/length', 'a/lengthsquared']);
  assert.ok(found.results.every(r => r.rank === 3));
  assert.equal(found.results[0].declaration, 'Length(self:   Vector3D): Number');
  assert.deepEqual(searchSymbols(rows, '  ').results, []);
  assert.equal(searchSymbols(rows, 'length', 2).total, 4);
});

test('adjacent pages stay within a door', () => {
  const syntax = pages.find(p => p.id.endsWith('/syntax'));
  const { previous, next } = adjacentPages(pages, syntax);
  assert.equal(previous.id, `${root}/language/model`);
  assert.equal(next.id, `${root}/language/diagnostics`);
  const tour = pages.find(p => p.id.endsWith('/start/tour'));
  assert.equal(adjacentPages(pages, tour).previous.id, `${root}/start/what-is-plato`);
  assert.equal(adjacentPages(pages, tour).next.id, `${root}/start/tour/functions`);
  const last = pages.find(p => p.id.endsWith('/getting-started'));
  assert.equal(adjacentPages(pages, last).next, null);
  const design = pages.find(p => p.id.endsWith('/design/README'));
  assert.equal(adjacentPages(pages, design).previous, null);
  assert.equal(adjacentPages(pages, design).next.id, `${root}/evidence/grammar-source`);
  assert.deepEqual(adjacentPages(pages, pages[7]), { previous: null, next: null });
  assert.deepEqual(adjacentPages(pages, pages[0]), { previous: null, next: null });
});

test('breadcrumb targets resolve section indexes', () => {
  assert.equal(crumbTarget(pages, ['library'], 0), `${root}/library/README`);
  assert.equal(crumbTarget(pages, ['api', 'foundation'], 1), `${root}/api/foundation/vectors.library`);
});

test('retired page ids resolve through aliases only to existing pages', () => {
  const retired = `${root}/api/foundation/vectors.library/000-old`;
  const gone = `${root}/api/foundation/vectors.library/000-gone`;
  const aliases = { [retired]: `${root}/api/foundation/vectors.library/length`, [gone]: `${root}/api/foundation/vectors.library/missing` };
  assert.equal(resolveAlias(pages, aliases, retired), `${root}/api/foundation/vectors.library/length`);
  assert.equal(resolveAlias(pages, aliases, gone), gone);
  assert.equal(resolveAlias(pages, aliases, `${root}/start/tour`), `${root}/start/tour`);
  assert.equal(resolveAlias(pages, undefined, 'docs/reference/nowhere'), 'docs/reference/nowhere');
  assert.equal(resolveAlias(pages, aliases, null), null);
});
