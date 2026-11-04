import test from 'node:test';
import assert from 'node:assert/strict';
import { checkExamples, checkIndex, checkSearch, createJsonLoader } from './data.js';

const reply = (ok, body, status = 200) => Promise.resolve({ ok, status, json: () => Promise.resolve(body) });

test('the loader fetches a url once and forgets a failure', async () => {
  const calls = [];
  const responses = [reply(false, null, 500), reply(true, { a: 1 })];
  const load = createJsonLoader(url => { calls.push(url); return responses.shift(); });
  await assert.rejects(load('/x.json'), /500/);
  assert.deepEqual(await load('/x.json'), { a: 1 });
  assert.deepEqual(await load('/x.json'), { a: 1 });
  assert.deepEqual(calls, ['/x.json', '/x.json']);
});

test('schema checks accept only version 2', () => {
  assert.ok(checkIndex({ schemaVersion: 2, sourceRevision: 'r', pages: [] }));
  assert.throws(() => checkIndex({ schemaVersion: 1, sourceRevision: 'r', pages: [] }), /unsupported/);
  assert.ok(checkSearch({ schemaVersion: 2, pages: [] }));
  assert.throws(() => checkSearch({ schemaVersion: 2 }), /unsupported/);
  assert.ok(checkExamples({ schemaVersion: 2, groups: {} }));
  assert.throws(() => checkExamples({ schemaVersion: 2 }), /unsupported/);
});
