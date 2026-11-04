import test from 'node:test';
import assert from 'node:assert/strict';
import { highlightCode, languageLabel } from './highlight.js';

test('plato highlighting colors keywords, types, and punctuation', () => {
  const html = highlightCode('type Point { X: Number; }', 'plato');
  assert.match(html, /tok-keyword">type</);
  assert.match(html, /tok-type">Point</);
  assert.match(html, /tok-type">Number</);
  assert.match(html, /tok-punct">/);
});

test('symbol links prefer foundation API pages', () => {
  const symbols = {
    Number: [
      { id: 'docs/reference/api/legacy/primitives/001-number', title: 'primitives: Number' },
      { id: 'docs/reference/api/foundation/primitives/001-number', title: 'primitives: Number' },
    ],
  };
  const html = highlightCode('Value: Number', 'plato', symbols);
  assert.match(html, /href="#\/docs\/reference\/api\/foundation\/primitives\/001-number"/);
  assert.match(html, /tok-type">Number</);
});

test('csharp highlighting uses distinct keyword color', () => {
  const html = highlightCode('public class Box { }', 'csharp');
  assert.match(html, /tok-keyword">public</);
  assert.match(html, /tok-keyword">class</);
  assert.equal(languageLabel('csharp'), 'C#');
  assert.equal(languageLabel('plato'), 'Plato');
});
