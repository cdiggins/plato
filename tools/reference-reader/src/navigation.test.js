import test from 'node:test';
import assert from 'node:assert/strict';
import {
  adjacentPages, buildPageTree, crumbTarget, extractHeadings, groupAncestors, humanizeSegment,
  parseHash, resolveAlias, routeFor, searchPages, searchPagesDetailed, slugifyHeading, stripLeadingTitle,
} from './navigation.js';

const pages = [
  { id: 'docs/reference/language/types/vector', title: 'Vector Model', group: ['language', 'types'], markdown: '# Vector Model\nA vector has components.' },
  { id: 'docs/reference/language/model', title: 'Programs, values, and names', group: ['language'], markdown: '# Programs\nPrograms hold values.' },
  { id: 'docs/reference/language/diagnostics', title: 'Diagnostics', group: ['language'], markdown: '# Diagnostics\nUnsupported forms.' },
  { id: 'docs/reference/language/syntax', title: 'Reading and writing source', group: ['language'], markdown: '# Syntax\nTokens and punctuation.' },
  { id: 'docs/reference/library/README', title: 'Library guide', group: ['library'], markdown: '# Library\nDomains.' },
  { id: 'docs/reference/api/foundation/numbers.library', title: 'numbers.library', group: ['api', 'foundation'], markdown: '# numbers.library\nOps.' },
  { id: 'docs/reference/api/foundation/numbers.library/001-add', title: 'numbers.library: Add', group: ['api', 'foundation', 'numbers.library'], markdown: '# Add\nAdd numbers.' },
  { id: 'docs/reference/start/what-is-plato', title: 'What is Plato', group: ['start'], markdown: '# What is Plato\nA language.' },
  { id: 'docs/reference/start/tour', title: 'Quick tour', group: ['start'], markdown: '# Quick tour\nMeasurement example.' },
];

test('routes retain page ids and heading slugs', () => {
  assert.deepEqual(parseHash('#/docs/reference/model@coordinate-frames'), { id: 'docs/reference/model', heading: 'coordinate-frames' });
  assert.deepEqual(parseHash('#/docs/a'), { id: 'docs/a', heading: null });
  assert.deepEqual(parseHash('#/bad%ZZ@heading%ZZ'), { id: 'bad%ZZ', heading: 'heading%ZZ' });
  assert.equal(routeFor('docs/a', 'coordinate-frames'), '#/docs/a@coordinate-frames');
  assert.equal(slugifyHeading('Coordinate Frames & Units'), 'coordinate-frames-units');
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

test('tree uses teaching section order and attaches API index pages', () => {
  const tree = buildPageTree(pages);
  assert.deepEqual([...tree.children.keys()], ['start', 'language', 'library', 'api']);
  assert.equal(humanizeSegment('api'), 'API catalog');
  const language = tree.children.get('language');
  assert.deepEqual(language.pages.map(page => page.id), [
    'docs/reference/language/model',
    'docs/reference/language/syntax',
    'docs/reference/language/diagnostics',
  ]);
  const numbers = tree.children.get('api').children.get('foundation').children.get('numbers.library');
  assert.equal(numbers.indexPage.id, 'docs/reference/api/foundation/numbers.library');
  assert.deepEqual(numbers.pages.map(page => page.id), ['docs/reference/api/foundation/numbers.library/001-add']);
  assert.deepEqual(groupAncestors(pages[0]), ['language', 'language/types']);
});

test('search matches every term and returns snippets', () => {
  assert.deepEqual(searchPages(pages, 'vector components').map(page => page.id), ['docs/reference/language/types/vector']);
  const detailed = searchPagesDetailed(pages, 'Measurement');
  assert.equal(detailed[0].page.id, 'docs/reference/start/tour');
  assert.match(detailed[0].snippet, /Measurement/i);
  assert.deepEqual(searchPages(pages, '   '), []);
});

test('adjacent pages follow language teaching order', () => {
  const syntax = pages.find(page => page.id.endsWith('/syntax'));
  const { previous, next } = adjacentPages(pages, syntax);
  assert.equal(previous.id, 'docs/reference/language/model');
  assert.equal(next.id, 'docs/reference/language/diagnostics');
});

test('breadcrumb targets resolve section indexes', () => {
  assert.equal(crumbTarget(pages, ['language'], 0), 'docs/reference/language/model');
  assert.equal(crumbTarget(pages, ['start'], 0), 'docs/reference/start/what-is-plato');
  assert.equal(crumbTarget(pages, ['api', 'foundation'], 1), 'docs/reference/api/foundation/numbers.library');
});

test('retired page ids resolve through aliases only to existing pages', () => {
  const aliases = {
    'docs/reference/api/foundation/numbers.library/000-old': 'docs/reference/api/foundation/numbers.library/001-add',
    'docs/reference/api/foundation/numbers.library/000-gone': 'docs/reference/api/foundation/numbers.library/missing',
  };
  assert.equal(resolveAlias(pages, aliases, 'docs/reference/api/foundation/numbers.library/000-old'),
    'docs/reference/api/foundation/numbers.library/001-add');
  assert.equal(resolveAlias(pages, aliases, 'docs/reference/api/foundation/numbers.library/000-gone'),
    'docs/reference/api/foundation/numbers.library/000-gone');
  assert.equal(resolveAlias(pages, aliases, 'docs/reference/start/tour'), 'docs/reference/start/tour');
  assert.equal(resolveAlias(pages, undefined, 'docs/reference/nowhere'), 'docs/reference/nowhere');
  assert.equal(resolveAlias(pages, aliases, null), null);
});
