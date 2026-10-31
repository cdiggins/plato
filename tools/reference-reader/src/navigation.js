/** Preferred root section order for teaching, not filesystem order. */
export const SECTION_ORDER = [
  'start', 'compare', 'language', 'library', 'api', 'cookbook', 'design', 'formal', 'implementation', 'evidence', 'review',
];

export const SECTION_LABELS = {
  start: 'Start',
  compare: 'Compare',
  cookbook: 'Cookbook',
  design: 'Design',
  language: 'Language',
  library: 'Library',
  api: 'API catalog',
  implementation: 'Implementation',
  formal: 'Formal model',
  evidence: 'Evidence',
  review: 'Review',
};

/** Language chapter order matching docs/reference/language/README.md. */
export const LANGUAGE_PAGE_ORDER = [
  'README',
  'model',
  'syntax',
  'functions',
  'products',
  'sums',
  'interfaces',
  'generics',
  'expressions',
  'state-and-builders',
  'evaluation',
  'numbers',
  'equality',
  'diagnostics',
  'glossary',
  'cheatsheet',
];

export const START_PAGE_ORDER = [
  'what-is-plato',
  'tour',
  'getting-started',
];

/** Library chapter order matching docs/reference/library/README.md. */
export const LIBRARY_PAGE_ORDER = [
  'README',
  'scalars-algebra',
  'collections',
  'functions',
  'quantities-time',
  'angles',
  'points-vectors',
  'matrices-transforms',
  'color',
  'probability',
  'statistics',
  'polynomials',
  'graphs',
  'primitives',
  'curves',
  'surfaces',
  'meshes',
  'triangulation',
  'solids-brep',
  'fields',
  'sampling-voxels',
  'noise-lattices',
  'search-remeshing',
  'cameras-lights',
  'images-textures',
  'paths-text',
  'motion',
  'scenes-rendering',
  'domains',
  'animation-signals',
  'dynamics',
  'engineering',
  'geospatial',
  'scientific-data',
  'verification-legacy',
];

export const LIBRARY_GROUP_ORDER = ['foundation', 'geometry', 'graphics', 'future'];

export const API_TIER_ORDER = ['foundation', 'geometry', 'graphics', 'future', 'legacy'];

/** Implementation chapter order matching docs/reference/implementation/README.md. */
export const IMPLEMENTATION_PAGE_ORDER = [
  'README',
  'grammar',
  'type-checking',
  'typed-ir',
  'csharp',
  'verification',
  'discrepancies',
  'verification-2026-10-25',
];

export const FORMAL_PAGE_ORDER = [
  'README',
  'core-language',
  'typing-and-evaluation',
  'rewriting',
  'implementation-bridge',
  'verification-2026-10-25',
];

export function slugifyHeading(text) {
  return text.toLowerCase().replace(/[^\p{L}\p{N}]+/gu, '-').replace(/^-|-$/g, '');
}

export function parseHash(hash) {
  const value = hash.replace(/^#\/?/, '');
  if (!value) return { id: null, heading: null };
  const at = value.indexOf('@');
  const decode = part => { try { return decodeURIComponent(part); } catch { return part; } };
  return at < 0
    ? { id: decode(value), heading: null }
    : { id: decode(value.slice(0, at)), heading: decode(value.slice(at + 1)) };
}

const PLAYGROUND = 'playground';

/**
 * Parse a hash into a page route or a playground route. The playground payload is the raw text
 * after the first `~`; only the playground module decodes it.
 */
export function parseRoute(hash) {
  const value = hash.replace(/^#\/?/, '');
  if (value === PLAYGROUND || value.startsWith(`${PLAYGROUND}/`)) {
    const rest = value.slice(PLAYGROUND.length + 1);
    const tilde = rest.indexOf('~');
    const name = tilde < 0 ? rest : rest.slice(0, tilde);
    const payload = tilde < 0 ? '' : rest.slice(tilde + 1);
    let group = name;
    try { group = decodeURIComponent(name); } catch { /* keep the raw text */ }
    return { kind: 'playground', group: group || null, payload: payload || null };
  }
  return { kind: 'page', ...parseHash(hash) };
}

/** The hash for a playground group, with an optional opaque payload. */
export function playgroundRoute(group, payload) {
  if (!group) return `#/${PLAYGROUND}`;
  return `#/${PLAYGROUND}/${encodeURIComponent(group)}${payload ? `~${payload}` : ''}`;
}

/** The example group a `plato` fence on this page belongs to, or null (contract: Playground). */
export function playgroundGroupFor(pageId) {
  const root = 'docs/reference/';
  if (!pageId?.startsWith(root)) return null;
  const rest = pageId.slice(root.length);
  if (rest === 'start/tour' || rest.startsWith('start/tour/')) return 'tour';
  const recipe = /^cookbook\/([^/]+)$/.exec(rest)?.[1];
  return recipe && recipe !== 'README' ? `cookbook-${recipe}` : null;
}

/** The URL of one page's data file; every path segment is encoded. */
export function pageDataUrl(base, id) {
  return `${base}pages/${id.split('/').map(encodeURIComponent).join('/')}.json`;
}

/** The five doors in header order. `landing` is a page id, or null for the playground route. */
export const DOORS = [
  { key: 'tour', label: 'Tour', landing: 'docs/reference/start/tour' },
  { key: 'reference', label: 'Reference', landing: 'docs/reference/language/README' },
  { key: 'cookbook', label: 'Cookbook', landing: 'docs/reference/cookbook/README' },
  { key: 'design', label: 'Design', landing: 'docs/reference/design/README' },
  { key: 'playground', label: 'Playground', landing: null },
];

export function doorRoute(door) {
  return door.landing ? routeFor(door.landing) : playgroundRoute();
}

/** The door a route sits in, or null on the home page. */
export function currentDoor(route, page) {
  if (route.kind === 'playground') return 'playground';
  return page && page.door !== 'home' ? page.door : null;
}

/** What each status means, from the reference contract's Status table. */
export const STATUS_MEANING = {
  Supported: 'In a shipping tier. The lint, type-check and code-generation gates cover it. No release promises its signature yet.',
  Experimental: 'Aspirational vocabulary. It parses and type-checks; it is not linted or converted to C#.',
  Legacy: 'The older library, compiled separately. It is writable, but new vocabulary does not go there.',
};

export function shortRevision(revision) {
  return String(revision ?? '').slice(0, 7);
}

export function commitUrl(revision) {
  return `https://github.com/cdiggins/plato/commit/${encodeURIComponent(revision)}`;
}

// A retired page id resolves to its replacement when the replacement exists; any other id is
// returned unchanged, so an unknown page still reports as missing.
export function resolveAlias(pages, aliases, id) {
  if (!id || pages.some(page => page.id === id)) return id;
  const target = aliases?.[id];
  return target && pages.some(page => page.id === target) ? target : id;
}

export function routeFor(pageId, heading) {
  return `#/${pageId}${heading ? `@${heading}` : ''}`;
}

export function humanizeSegment(name) {
  if (SECTION_LABELS[name]) return SECTION_LABELS[name];
  return String(name)
    .split(/[.-]/)
    .filter(Boolean)
    .map(part => part.charAt(0).toUpperCase() + part.slice(1))
    .join(' ');
}

function pageStem(page) {
  return page.id.split('/').at(-1);
}

function orderIndex(list, key) {
  const index = list.indexOf(key);
  return index < 0 ? list.length + 1 : index;
}

const SECTION_PAGE_ORDERS = {
  language: LANGUAGE_PAGE_ORDER,
  start: START_PAGE_ORDER,
  library: LIBRARY_PAGE_ORDER,
  implementation: IMPLEMENTATION_PAGE_ORDER,
  formal: FORMAL_PAGE_ORDER,
};

function comparePages(a, b, groupPath) {
  const order = SECTION_PAGE_ORDERS[groupPath[0]];
  if (order) {
    const delta = orderIndex(order, pageStem(a)) - orderIndex(order, pageStem(b));
    if (delta) return delta;
  }
  return a.title.localeCompare(b.title) || a.id.localeCompare(b.id);
}

function compareChildKeys(a, b, parentPath) {
  if (parentPath.length === 0) {
    const delta = orderIndex(SECTION_ORDER, a) - orderIndex(SECTION_ORDER, b);
    if (delta) return delta;
  }
  if (parentPath.length === 1 && parentPath[0] === 'library') {
    const delta = orderIndex(LIBRARY_GROUP_ORDER, a) - orderIndex(LIBRARY_GROUP_ORDER, b);
    if (delta) return delta;
  }
  if (parentPath.length === 1 && parentPath[0] === 'api') {
    const delta = orderIndex(API_TIER_ORDER, a) - orderIndex(API_TIER_ORDER, b);
    if (delta) return delta;
  }
  return a.localeCompare(b);
}

/**
 * Build a nav tree. When a folder name matches a sibling page stem (API catalog
 * pattern: numbers.library page + numbers.library/ children), attach the page as
 * the folder's index so the name appears once.
 */
export function buildPageTree(pages) {
  const root = { name: '', path: [], children: new Map(), pages: [], indexPage: null };
  for (const page of pages) {
    let node = root;
    for (const name of page.group ?? []) {
      if (!node.children.has(name)) {
        node.children.set(name, { name, path: [...node.path, name], children: new Map(), pages: [], indexPage: null });
      }
      node = node.children.get(name);
    }
    node.pages.push(page);
  }

  const finalize = (node, parentPath = []) => {
    for (const [name, child] of [...node.children.entries()]) {
      finalize(child, [...parentPath, name]);
    }
    const remaining = [];
    for (const page of node.pages) {
      const stem = pageStem(page);
      const child = node.children.get(stem);
      if (child && !child.indexPage) {
        child.indexPage = page;
      } else {
        remaining.push(page);
      }
    }
    node.pages = remaining.sort((a, b) => comparePages(a, b, parentPath));
    const ordered = [...node.children.entries()].sort(([a], [b]) => compareChildKeys(a, b, parentPath));
    node.children = new Map(ordered);
    return node;
  };

  return finalize(root);
}

/**
 * One folder's rows in display order. A section with a teaching order interleaves folders and
 * pages by it (a folder that the order does not list stays first); other folders list their
 * sub-folders, then their pages.
 */
export function treeEntries(node) {
  const entries = [
    ...[...node.children.values()].map(child => ({ kind: 'folder', key: child.name, child })),
    ...node.pages.map(page => ({ kind: 'page', key: pageStem(page), page })),
  ];
  const order = SECTION_PAGE_ORDERS[node.path[0]];
  if (!order) return entries;
  const rank = entry => {
    const index = order.indexOf(entry.key);
    return index >= 0 ? index : entry.kind === 'folder' ? -1 : order.length + 1;
  };
  return entries.map((entry, position) => ({ entry, position }))
    .sort((a, b) => rank(a.entry) - rank(b.entry) || a.position - b.position)
    .map(item => item.entry);
}

/** The page tree grouped under each door, in header order; doors without pages are omitted. */
export function buildDoorTrees(pages) {
  return DOORS.filter(door => door.key !== 'playground').map(door => ({
    door,
    tree: buildPageTree(pages.filter(page => page.door === door.key)),
  })).filter(item => item.tree.children.size || item.tree.pages.length);
}

function flattenTree(node, out = []) {
  for (const entry of treeEntries(node)) {
    if (entry.kind === 'page') { out.push(entry.page); continue; }
    if (entry.child.indexPage) out.push(entry.child.indexPage);
    flattenTree(entry.child, out);
  }
  return out;
}

const norm = text => text.toLocaleLowerCase().replace(/\s+/g, ' ').trim();

/** Lower-cased page text for body search, keyed by page id. Build it once per search.json. */
export function buildSearchIndex(entries) {
  return new Map(entries.map(entry => [entry.id, { text: entry.text, lower: entry.text.toLocaleLowerCase() }]));
}

/**
 * Pages matching every term in the title or, once an index is given, the body. A title match
 * outranks a body match, and a query equal to a title (or its part after the colon) ranks first.
 */
export function searchPagesDetailed(pages, query, index = null) {
  const terms = norm(query).split(' ').filter(Boolean);
  if (!terms.length) return [];
  const whole = terms.join(' ');
  const results = [];
  pages.forEach((page, position) => {
    const title = page.title.toLocaleLowerCase();
    const body = index?.get(page.id);
    let score = 0;
    for (const term of terms) {
      const inTitle = title.includes(term);
      const inBody = body ? body.lower.includes(term) : false;
      if (!inTitle && !inBody) return;
      score += (inTitle ? 4 : 0) + (inBody ? 1 : 0);
    }
    if (title === whole || title.endsWith(`: ${whole}`)) score += 100;
    results.push({ page, score, position, snippet: body ? makeSnippet(body.text, terms) : '' });
  });
  return results.sort((a, b) => b.score - a.score || a.page.title.localeCompare(b.page.title) || a.position - b.position);
}

export function searchPages(pages, query, index = null) {
  return searchPagesDetailed(pages, query, index).map(result => result.page);
}

/** Every symbol page as a row with its name and normalised declarations, for repeated queries. */
export function buildSymbolIndex(symbols) {
  const rows = [];
  for (const [name, entries] of Object.entries(symbols ?? {})) {
    for (const entry of entries) {
      rows.push({ name, lower: name.toLocaleLowerCase(), entry, declarations: entry.declarations ?? [], normalised: (entry.declarations ?? []).map(norm) });
    }
  }
  return rows;
}

/**
 * Symbols whose name matches (exact, then prefix, then substring, ignoring case) or whose
 * declarations contain the query as a whitespace-normalised substring, in that order.
 */
export function searchSymbols(rows, query, limit = 30) {
  const q = norm(query);
  if (!q) return { total: 0, results: [] };
  const found = [];
  for (const row of rows) {
    let rank = -1;
    let declaration = row.declarations[0] ?? '';
    if (row.lower === q) rank = 0;
    else if (row.lower.startsWith(q)) rank = 1;
    else if (row.lower.includes(q)) rank = 2;
    else {
      const at = row.normalised.findIndex(text => text.includes(q));
      if (at >= 0) { rank = 3; declaration = row.declarations[at]; }
    }
    if (rank < 0) continue;
    found.push({ name: row.name, entry: row.entry, rank, declaration, order: rank * 100 + symbolRank(row.entry, row.name) });
  }
  found.sort((a, b) => a.order - b.order || a.name.localeCompare(b.name) || a.entry.id.localeCompare(b.entry.id));
  return { total: found.length, results: found.slice(0, limit) };
}

function makeSnippet(markdown, terms) {
  const plain = markdown
    .replace(/^```[\s\S]*?^```/gm, ' ')
    .replace(/\[([^\]]+)\]\([^)]+\)/g, '$1')
    .replace(/[#>*_`|-]/g, ' ')
    .replace(/\s+/g, ' ')
    .trim();
  const lower = plain.toLocaleLowerCase();
  let at = -1;
  let term = terms[0];
  for (const candidate of terms) {
    const found = lower.indexOf(candidate);
    if (found >= 0 && (at < 0 || found < at)) {
      at = found;
      term = candidate;
    }
  }
  if (at < 0) return plain.slice(0, 120);
  const start = Math.max(0, at - 40);
  const end = Math.min(plain.length, at + term.length + 80);
  const slice = `${start > 0 ? '…' : ''}${plain.slice(start, end)}${end < plain.length ? '…' : ''}`;
  return slice;
}

export function extractHeadings(markdown, maxLevel = 3) {
  let fence = null;
  return markdown.split('\n').flatMap(line => {
    const fenceMatch = /^\s*(`{3,}|~{3,})/.exec(line);
    if (fenceMatch) {
      const marker = fenceMatch[1];
      if (!fence) fence = { character: marker[0], length: marker.length };
      else if (marker[0] === fence.character && marker.length >= fence.length && !line.trim().slice(marker.length).trim()) fence = null;
      return [];
    }
    if (fence) return [];
    const match = /^(#{1,6})\s+(.+?)\s*#*\s*$/.exec(line);
    if (!match || match[1].length > maxLevel) return [];
    const text = match[2].replace(/\s+#+\s*$/, '').trim();
    return [{ level: match[1].length, text, slug: slugifyHeading(text) }];
  });
}

export function stripLeadingTitle(markdown) {
  const lines = markdown.split('\n');
  let index = 0;
  while (index < lines.length) {
    const line = lines[index].trim();
    if (!line) { index += 1; continue; }
    if (line.startsWith('<!--')) {
      while (index < lines.length && !lines[index].includes('-->')) index += 1;
      index += 1;
      continue;
    }
    if (/^#\s+/.test(line)) index += 1;
    break;
  }
  while (index < lines.length && !lines[index].trim()) index += 1;
  return lines.slice(index).join('\n');
}

export function groupAncestors(page) {
  return (page?.group ?? []).map((_, index, group) => group.slice(0, index + 1).join('/'));
}

/**
 * Neighbours in reading order inside the page's door. The API catalog has none: its pages are
 * reached from the symbol index, not read in sequence.
 */
export function adjacentPages(pages, page) {
  const none = { previous: null, next: null };
  if (!page || page.door === 'home' || page.group?.[0] === 'api') return none;
  const sequence = flattenTree(buildPageTree(pages.filter(candidate => candidate.door === page.door && candidate.group?.[0] !== 'api')));
  const index = sequence.findIndex(candidate => candidate.id === page.id);
  if (index < 0) return none;
  return { previous: sequence[index - 1] ?? null, next: sequence[index + 1] ?? null };
}

/** Resolve a breadcrumb segment to a page id when possible. */
export function crumbTarget(pages, groupParts, index) {
  const path = groupParts.slice(0, index + 1);
  const readme = pages.find(page => page.id === `docs/reference/${path.join('/')}/README`);
  if (readme) return readme.id;
  const indexPage = pages.find(page => page.group?.length === path.length - 1 && pageStem(page) === path.at(-1));
  if (indexPage) return indexPage.id;
  const under = pages
    .filter(page => path.every((part, i) => page.group?.[i] === part))
    .sort((a, b) => comparePages(a, b, path))[0];
  return under?.id ?? null;
}

export function githubBlobUrl(revision, path) {
  return `https://github.com/cdiggins/plato/blob/${revision}/${path.split('/').map(encodeURIComponent).join('/')}`;
}

function symbolRank(entry, name) {
  const titled = entry.title === name || entry.title.endsWith(`: ${name}`);
  let rank = titled ? 0 : 20;
  if (entry.id.includes('/api/foundation/')) rank -= 2;
  if (entry.id.includes('/api/legacy/')) rank += 5;
  return rank;
}

/** Prefer a page titled for the name, then foundation over legacy. */
export function resolveSymbol(symbols, name) {
  if (!symbols || !name) return null;
  const entries = symbols[name];
  if (!entries?.length) return null;
  return [...entries].sort((a, b) => symbolRank(a, name) - symbolRank(b, name) || a.id.localeCompare(b.id))[0];
}
