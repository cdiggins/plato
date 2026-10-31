// Pure helpers for the playground: share-payload codec, edit detection, group titles and links.
// Payload text is one version character followed by base64url. Version 'z' holds deflate-raw
// bytes, version 'p' holds the bytes uncompressed. The bytes are UTF-8 JSON: an object that maps
// the path of each edited file to its full edited source.

export const MAX_PAYLOAD_CHARS = 60000;
export const MAX_JSON_BYTES = 200000;

const encoder = new TextEncoder();
const decoder = new TextDecoder('utf-8', { fatal: true });

export class PayloadError extends Error {}

export function toBase64Url(bytes) {
  let binary = '';
  for (let i = 0; i < bytes.length; i += 0x8000) {
    binary += String.fromCharCode(...bytes.subarray(i, i + 0x8000));
  }
  return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

export function fromBase64Url(text) {
  if (!/^[A-Za-z0-9_-]*$/.test(text) || text.length % 4 === 1) throw new PayloadError('The shared link is not valid base64.');
  const padded = text.replace(/-/g, '+').replace(/_/g, '/') + '='.repeat((4 - (text.length % 4)) % 4);
  const binary = atob(padded);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
  return bytes;
}

export const canCompress = () => typeof CompressionStream === 'function' && typeof DecompressionStream === 'function';

async function pipe(bytes, transform, limit) {
  const reader = new Blob([bytes]).stream().pipeThrough(transform).getReader();
  const chunks = [];
  let total = 0;
  for (;;) {
    const { done, value } = await reader.read();
    if (done) break;
    total += value.length;
    if (total > limit) {
      await reader.cancel();
      throw new PayloadError('The shared edit is too large.');
    }
    chunks.push(value);
  }
  const out = new Uint8Array(total);
  let at = 0;
  for (const chunk of chunks) { out.set(chunk, at); at += chunk.length; }
  return out;
}

/** Encode an object {path: source} of edited files. `compress` defaults to what the runtime supports. */
export async function encodePayload(edits, compress = canCompress()) {
  const json = encoder.encode(JSON.stringify(edits));
  const tooLarge = 'The edit is too large to share by link. Download the file instead.';
  if (json.length > MAX_JSON_BYTES) throw new PayloadError(tooLarge);
  const text = compress
    ? 'z' + toBase64Url(await pipe(json, new CompressionStream('deflate-raw'), MAX_JSON_BYTES * 2))
    : 'p' + toBase64Url(json);
  if (text.length > MAX_PAYLOAD_CHARS) throw new PayloadError(tooLarge);
  return text;
}

/** Decode a payload against a group {files:[{path}]}. Returns {path: source}; throws PayloadError. */
export async function decodePayload(payload, group) {
  if (typeof payload !== 'string' || payload.length < 1) throw new PayloadError('The shared link is empty.');
  if (payload.length > MAX_PAYLOAD_CHARS) throw new PayloadError('The shared link is too large.');
  const version = payload[0];
  if (version !== 'z' && version !== 'p') throw new PayloadError('The shared link has an unknown format version.');
  let bytes;
  try {
    bytes = fromBase64Url(payload.slice(1));
    if (version === 'z') {
      if (!canCompress()) throw new PayloadError('This browser cannot open a compressed shared link.');
      bytes = await pipe(bytes, new DecompressionStream('deflate-raw'), MAX_JSON_BYTES);
    }
    if (bytes.length > MAX_JSON_BYTES) throw new PayloadError('The shared edit is too large.');
  } catch (error) {
    if (error instanceof PayloadError) throw error;
    throw new PayloadError('The shared link is damaged.');
  }
  let edits;
  try {
    edits = JSON.parse(decoder.decode(bytes));
  } catch {
    throw new PayloadError('The shared link is damaged.');
  }
  if (edits === null || typeof edits !== 'object' || Array.isArray(edits)) throw new PayloadError('The shared link is damaged.');
  const known = new Set(group.files.map(file => file.path));
  for (const [path, source] of Object.entries(edits)) {
    if (!known.has(path)) throw new PayloadError(`The shared link names a file this example does not have: ${path}`);
    if (typeof source !== 'string') throw new PayloadError('The shared link is damaged.');
  }
  return edits;
}

/** Current sources by path: the originals with edits laid over them. */
export function currentSources(group, edits) {
  return Object.fromEntries(group.files.map(file => [file.path, edits[file.path] ?? file.source]));
}

/** Only the files whose current text differs from the original. */
export function editedFiles(group, sources) {
  const edited = {};
  for (const file of group.files) {
    if (sources[file.path] !== undefined && sources[file.path] !== file.source) edited[file.path] = sources[file.path];
  }
  return edited;
}

export const isEdited = (group, sources) => Object.keys(editedFiles(group, sources)).length > 0;

/** `tour` becomes Tour, `cookbook-fit-line` becomes "Cookbook: fit line". */
export function groupTitle(name) {
  const cookbook = /^cookbook-(.+)$/.exec(name);
  const words = (cookbook ? cookbook[1] : name).replace(/-/g, ' ');
  if (cookbook) return `Cookbook: ${words}`;
  return words.charAt(0).toUpperCase() + words.slice(1);
}

/** The reader route of the page that uses a group. */
export function groupPage(name) {
  if (name === 'tour') return '#/docs/reference/start/tour';
  const cookbook = /^cookbook-(.+)$/.exec(name);
  if (cookbook) return `#/docs/reference/cookbook/${cookbook[1]}`;
  return '#/docs/reference/implementation/examples/README';
}

export const fileName = path => path.slice(path.lastIndexOf('/') + 1);
