/**
 * A JSON loader that remembers each URL it has fetched, so a page opened twice is downloaded
 * once. A failed fetch is forgotten, so retrying asks the network again.
 */
export function createJsonLoader(fetchFn = (...args) => fetch(...args)) {
  const cache = new Map();
  return url => {
    if (!cache.has(url)) {
      const request = Promise.resolve(fetchFn(url)).then(response => {
        if (!response.ok) throw new Error(`${url} returned ${response.status}`);
        return response.json();
      });
      request.catch(() => cache.delete(url));
      cache.set(url, request);
    }
    return cache.get(url);
  };
}

/** Throws unless `value` is the v2 reference index. */
export function checkIndex(value) {
  if (value?.schemaVersion !== 2 || !Array.isArray(value.pages) || typeof value.sourceRevision !== 'string') {
    throw new Error('The reference index has an unsupported schema. Run the reference builder again.');
  }
  return value;
}

/** Throws unless `value` is the v2 body-search file. */
export function checkSearch(value) {
  if (value?.schemaVersion !== 2 || !Array.isArray(value.pages)) throw new Error('The search data has an unsupported schema.');
  return value;
}

/** Throws unless `value` is the v2 examples file. */
export function checkExamples(value) {
  if (value?.schemaVersion !== 2 || typeof value.groups !== 'object' || !value.groups) throw new Error('The examples data has an unsupported schema.');
  return value;
}
