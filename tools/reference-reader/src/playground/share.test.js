import test from 'node:test';
import assert from 'node:assert/strict';
import {
  encodePayload, decodePayload, editedFiles, isEdited, currentSources, groupTitle, groupPage,
  MAX_PAYLOAD_CHARS, PayloadError,
} from './share.js';

const group = { inputs: [], laws: [], files: [{ path: 'a/x.plato', source: 'one' }, { path: 'a/y.plato', source: 'two' }] };
const edits = { 'a/y.plato': 'two ✓ edited\n'.repeat(50) };

for (const compress of [true, false]) {
  test(`round trip, compress=${compress}`, async () => {
    const text = await encodePayload(edits, compress);
    assert.equal(text[0], compress ? 'z' : 'p');
    assert.match(text, /^[A-Za-z0-9_-]+$/);
    assert.deepEqual(await decodePayload(text, group), edits);
  });
}

test('compression shrinks repetitive text', async () => {
  assert.ok((await encodePayload(edits, true)).length < (await encodePayload(edits, false)).length);
});

const rejects = promise => assert.rejects(promise, PayloadError);

test('rejects malformed, unknown version, empty, oversized', async () => {
  await rejects(decodePayload('', group));
  await rejects(decodePayload('qabcd', group));
  await rejects(decodePayload('p!!!!', group));
  await rejects(decodePayload('pAAAA', group));
  await rejects(decodePayload('zAAAA', group));
  await rejects(decodePayload('p' + 'A'.repeat(MAX_PAYLOAD_CHARS), group));
});

test('rejects non-object JSON, wrong value types and unknown files', async () => {
  await rejects(decodePayload(await encodePayload([1], false), group));
  await rejects(decodePayload(await encodePayload({ 'nope.plato': 'x' }, false), group));
  await rejects(decodePayload(await encodePayload({ 'a/x.plato': 5 }, false), group));
  await rejects(decodePayload(await encodePayload({ 'nope.plato': 'x' }, true), group));
});

test('rejects an edit too large to encode', async () => {
  await rejects(encodePayload({ 'a/x.plato': 'x'.repeat(300000) }, false));
});

test('edited-file detection', () => {
  assert.equal(isEdited(group, currentSources(group, {})), false);
  const changed = currentSources(group, { 'a/x.plato': 'uno' });
  assert.deepEqual(editedFiles(group, changed), { 'a/x.plato': 'uno' });
  assert.equal(isEdited(group, currentSources(group, { 'a/x.plato': 'one' })), false);
});

test('group titles and pages', () => {
  assert.equal(groupTitle('tour'), 'Tour');
  assert.equal(groupTitle('cookbook-fit-line'), 'Cookbook: fit line');
  assert.equal(groupTitle('sums'), 'Sums');
  assert.equal(groupPage('tour'), '#/docs/reference/start/tour');
  assert.equal(groupPage('cookbook-fit-line'), '#/docs/reference/cookbook/fit-line');
  assert.equal(groupPage('loops'), '#/docs/reference/implementation/examples/README');
});
