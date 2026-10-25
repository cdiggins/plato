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

export function routeFor(pageId, heading) {
  return `#/${pageId}${heading ? `@${heading}` : ''}`;
}

export function buildPageTree(pages) {
  const root = { name: '', path: [], children: new Map(), pages: [] };
  for (const page of pages) {
    let node = root;
    for (const name of page.group ?? []) {
      if (!node.children.has(name)) node.children.set(name, { name, path: [...node.path, name], children: new Map(), pages: [] });
      node = node.children.get(name);
    }
    node.pages.push(page);
  }
  const sort = node => {
    node.pages.sort((a, b) => a.title.localeCompare(b.title));
    for (const child of node.children.values()) sort(child);
    return node;
  };
  return sort(root);
}

export function searchPages(pages, query) {
  const terms = query.trim().toLocaleLowerCase().split(/\s+/).filter(Boolean);
  if (!terms.length) return [];
  return pages.map((page, index) => {
    const title = page.title.toLocaleLowerCase();
    const body = page.markdown.toLocaleLowerCase();
    const score = terms.reduce((sum, term) => sum + (title.includes(term) ? 4 : 0) + (body.includes(term) ? 1 : 0), 0);
    const matchesEveryTerm = terms.every(term => title.includes(term) || body.includes(term));
    return { page, score, index, matchesEveryTerm };
  }).filter(result => result.matchesEveryTerm)
    .sort((a, b) => b.score - a.score || a.page.title.localeCompare(b.page.title) || a.index - b.index)
    .map(result => result.page);
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
