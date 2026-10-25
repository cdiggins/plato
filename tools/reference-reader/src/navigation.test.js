import test from 'node:test';
import assert from 'node:assert/strict';
import { buildPageTree, extractHeadings, groupAncestors, parseHash, routeFor, searchPages, slugifyHeading, stripLeadingTitle } from './navigation.js';

const pages = [
  { id: 'docs/a', title: 'Vector Model', group: ['language', 'types'], markdown: '# Vector Model\nA vector has components.' },
  { id: 'docs/b', title: 'Angle', group: ['language'], markdown: 'Angles measure rotation.' },
  { id: 'docs/c', title: 'Vector API', group: ['library'], markdown: 'Vector operations.' },
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

test('tree groups pages under each hierarchy segment', () => {
  const tree = buildPageTree(pages);
  assert.deepEqual([...tree.children.keys()], ['language', 'library']);
  assert.deepEqual([...tree.children.get('language').children.keys()], ['types']);
  assert.deepEqual(tree.children.get('language').pages.map(page => page.id), ['docs/b']);
  assert.deepEqual(groupAncestors(pages[0]), ['language', 'language/types']);
});

test('search matches every term in title or body and ranks title matches first', () => {
  assert.deepEqual(searchPages(pages, 'vector components').map(page => page.id), ['docs/a']);
  assert.deepEqual(searchPages(pages, 'vector').map(page => page.id), ['docs/c', 'docs/a']);
  assert.deepEqual(searchPages(pages, '   '), []);
});
