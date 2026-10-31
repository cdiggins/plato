/** Preferred root section order for teaching, not filesystem order. */
export const SECTION_ORDER = [
  'start', 'language', 'library', 'api', 'implementation', 'formal', 'evidence', 'review',
];

export const SECTION_LABELS = {
  start: 'Start',
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

export function sortedChildEntries(node) {
  return [...node.children.entries()];
}

export function searchPages(pages, query) {
  return searchPagesDetailed(pages, query).map(result => result.page);
}

export function searchPagesDetailed(pages, query) {
  const terms = query.trim().toLocaleLowerCase().split(/\s+/).filter(Boolean);
  if (!terms.length) return [];
  return pages.map((page, index) => {
    const title = page.title.toLocaleLowerCase();
    const body = page.markdown.toLocaleLowerCase();
    const score = terms.reduce((sum, term) => sum + (title.includes(term) ? 4 : 0) + (body.includes(term) ? 1 : 0), 0);
    const matchesEveryTerm = terms.every(term => title.includes(term) || body.includes(term));
    const snippet = matchesEveryTerm ? makeSnippet(page.markdown, terms) : '';
    return { page, score, index, matchesEveryTerm, snippet };
  }).filter(result => result.matchesEveryTerm)
    .sort((a, b) => b.score - a.score || a.page.title.localeCompare(b.page.title) || a.index - b.index);
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

/** Sequential neighbors for guide chapters (start, language, library, implementation). */
export function adjacentPages(pages, page) {
  if (!page) return { previous: null, next: null };
  const section = page.group?.[0];
  if (!['start', 'language', 'library', 'implementation'].includes(section)) {
    return { previous: null, next: null };
  }
  const siblings = pages
    .filter(candidate => (candidate.group?.[0] === section) && (candidate.group?.length === page.group.length))
    .sort((a, b) => comparePages(a, b, page.group));
  const index = siblings.findIndex(candidate => candidate.id === page.id);
  if (index < 0) return { previous: null, next: null };
  return { previous: siblings[index - 1] ?? null, next: siblings[index + 1] ?? null };
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
