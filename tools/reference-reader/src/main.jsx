import React, { Suspense, lazy, useCallback, useDeferredValue, useEffect, useMemo, useState } from 'react';
import { createRoot } from 'react-dom/client';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { highlightCode, languageLabel } from './highlight.js';
import { checkExamples, checkIndex, checkSearch, createJsonLoader } from './data.js';
import {
  DOORS, STATUS_MEANING, adjacentPages, buildDoorTrees, buildSearchIndex, buildSymbolIndex, commitUrl,
  crumbTarget, currentDoor, doorRoute, extractHeadings, githubBlobUrl, groupAncestors, humanizeSegment,
  pageDataUrl, parseRoute, playgroundGroupFor, playgroundRoute, resolveAlias, resolveSymbol, routeFor,
  searchPagesDetailed, searchSymbols, shortRevision, slugifyHeading, stripLeadingTitle, treeEntries,
} from './navigation.js';
import '@fontsource/spectral/600.css';
import '@fontsource/ibm-plex-sans/400.css';
import '@fontsource/ibm-plex-sans/500.css';
import '@fontsource/ibm-plex-sans/600.css';
import '@fontsource/ibm-plex-mono/400.css';
import './style.css';

/** Where the site is served from: "/" in development, the Pages subfolder in a `--base` build. */
const BASE = import.meta.env.BASE_URL;

/** The playground is its own module, fetched when the route is first opened. */
const Playground = lazy(() => import('./playground/Playground.jsx'));

const loadJson = createJsonLoader();

/** The root spiral from docs/assets/icon, copied beside tokens.css by the builder. */
function Mark({ className, size }) {
  return <picture className={className}>
    <source srcSet={`${BASE}brand/root-spiral-light.svg`} media="(prefers-color-scheme: light)" />
    <img src={`${BASE}brand/root-spiral.svg`} alt="" width={size} height={size} />
  </picture>;
}

/** The brand tokens as the page computed them, for code that cannot read CSS. */
function tokens(...names) {
  const style = getComputedStyle(document.documentElement);
  return Object.fromEntries(names.map(name => [name, style.getPropertyValue(`--${name}`).trim()]));
}

function useRoute() {
  const [route, setRoute] = useState(() => parseRoute(location.hash));
  useEffect(() => {
    const update = () => setRoute(parseRoute(location.hash));
    addEventListener('hashchange', update);
    return () => removeEventListener('hashchange', update);
  }, []);
  return route;
}

function useSearchShortcutLabel() {
  const [label, setLabel] = useState('Ctrl K');
  useEffect(() => {
    const apple = /Mac|iPhone|iPad|iPod/.test(navigator.platform) || navigator.userAgent.includes('Mac OS X');
    setLabel(apple ? '⌘ K' : 'Ctrl K');
  }, []);
  return label;
}

function Mermaid({ source }) {
  const [markup, setMarkup] = useState('');
  const [failed, setFailed] = useState(false);
  useEffect(() => {
    let live = true;
    import('mermaid').then(({ default: mermaid }) => {
      const t = tokens('ground', 'ground-raised', 'rule', 'ink', 'ink-muted', 'blue', 'font-sans');
      mermaid.initialize({
        startOnLoad: false, securityLevel: 'strict', theme: 'base', fontFamily: t['font-sans'],
        themeVariables: {
          fontFamily: t['font-sans'], background: t['ground-raised'], textColor: t.ink,
          primaryColor: t.ground, primaryTextColor: t.ink, primaryBorderColor: t.rule,
          secondaryColor: t['ground-raised'], secondaryTextColor: t.ink, secondaryBorderColor: t.rule,
          tertiaryColor: t['ground-raised'], tertiaryTextColor: t.ink, tertiaryBorderColor: t.rule,
          lineColor: t['ink-muted'], edgeLabelBackground: t['ground-raised'],
          clusterBkg: t['ground-raised'], clusterBorder: t.rule, noteBkgColor: t.ground, noteTextColor: t.ink, noteBorderColor: t.rule,
          mainBkg: t.ground, nodeBorder: t.rule, titleColor: t.ink, actorBkg: t.ground, actorBorder: t.rule, actorTextColor: t.ink,
          signalColor: t['ink-muted'], signalTextColor: t.ink, labelBoxBkgColor: t.ground, labelTextColor: t.ink, loopTextColor: t.ink,
          activationBkgColor: t['ground-raised'], activationBorderColor: t.blue,
        },
      });
      return mermaid.render(`diagram-${crypto.randomUUID()}`, source);
    }).then(({ svg }) => {
      if (live) setMarkup(svg);
    }).catch(() => { if (live) setFailed(true); });
    return () => { live = false; };
  }, [source]);
  if (failed) return <pre className="diagram-error"><code>{source}</code></pre>;
  return <div className="diagram" aria-label="Diagram" dangerouslySetInnerHTML={{ __html: markup }} />;
}

function CodeBlock({ language, source, symbols }) {
  const [copied, setCopied] = useState(false);
  const html = useMemo(() => highlightCode(source, language, symbols), [source, language, symbols]);
  const label = languageLabel(language);
  const copy = async () => {
    try {
      await navigator.clipboard.writeText(source);
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    } catch {
      setCopied(false);
    }
  };
  return <div className="code-block">
    <div className="code-toolbar">
      {label ? <span className="code-lang">{label}</span> : <span />}
      <button type="button" className="copy-button" onClick={copy}>{copied ? 'Copied' : 'Copy'}</button>
    </div>
    <pre><code className={language ? `language-${language}` : undefined} dangerouslySetInnerHTML={{ __html: html }} /></pre>
  </div>;
}

function InlineCode({ children, symbols, ...props }) {
  const text = String(children);
  const target = resolveSymbol(symbols, text);
  if (target) {
    return <code {...props}><a className="symbol-link" href={routeFor(target.id)}>{text}</a></code>;
  }
  return <code {...props}>{children}</code>;
}

function makeMarkdownComponents(page, symbols) {
  const playgroundGroup = playgroundGroupFor(page.id);
  const heading = level => ({ children, ...props }) => {
    const label = React.Children.toArray(children).map(child => typeof child === 'string' ? child : child?.props?.children ?? '').join('');
    const slug = slugifyHeading(String(label));
    const Tag = `h${level}`;
    return <Tag id={slug} {...props}>
      <a className="heading-anchor" href={routeFor(page.id, slug)} aria-label={`Link to ${label}`}>
        <span aria-hidden="true">#</span>
      </a>
      {children}
    </Tag>;
  };
  return {
    h1: heading(1), h2: heading(2), h3: heading(3), h4: heading(4), h5: heading(5), h6: heading(6),
    a: ({ href = '', children, node, ...props }) => {
      void node;
      const routeLink = href.startsWith('#/') || href.startsWith('#');
      return <a href={href} {...props} onClick={event => {
        if (routeLink) { event.preventDefault(); location.hash = href.slice(1).startsWith('/') ? href.slice(1) : `/${page.id}@${href.slice(1)}`; }
      }}>{children}</a>;
    },
    code: ({ className, children, node, ...props }) => {
      const language = /language-(\w+)/.exec(className ?? '')?.[1];
      const source = String(children).replace(/\n$/, '');
      if (language === 'mermaid') return <Mermaid source={source} />;
      if (language || String(children).includes('\n')) {
        const block = <CodeBlock language={language} source={source} symbols={symbols} />;
        // A `plato unchecked` fence is in no example group, so it has nowhere to open.
        if (language === 'plato' && playgroundGroup && !node?.data?.meta) {
          return <>
            {block}
            <p className="fence-link"><a href={playgroundRoute(playgroundGroup)}>Open in playground</a></p>
          </>;
        }
        return block;
      }
      return <InlineCode symbols={symbols} className={className} {...props}>{children}</InlineCode>;
    },
    pre: ({ children }) => <>{children}</>,
  };
}

/** Fetch one JSON resource when `url` is set. Reports loading, error (with a retry) or the value. */
function useResource(url, validate) {
  const [state, setState] = useState({ url: null, status: 'idle', value: null, error: '' });
  const [attempt, setAttempt] = useState(0);
  useEffect(() => {
    if (!url) return undefined;
    let live = true;
    loadJson(url).then(value => {
      if (validate) validate(value);
      if (live) setState({ url, status: 'ready', value, error: '' });
    }).catch(error => {
      if (live) setState({ url, status: 'error', value: null, error: error.message });
    });
    return () => { live = false; };
  }, [url, attempt, validate]);
  const retry = useCallback(() => {
    setState({ url: null, status: 'idle', value: null, error: '' });
    setAttempt(count => count + 1);
  }, []);
  if (!url) return { status: 'idle', value: null, error: '', retry };
  if (state.url !== url) return { status: 'loading', value: null, error: '', retry };
  return { ...state, retry };
}

function TreeBranch({ node, activeId, navigate, expanded, toggle, depth = 0 }) {
  return <>
    {treeEntries(node).map(entry => {
      if (entry.kind === 'page') {
        const page = entry.page;
        return <button type="button" key={page.id} className={`tree-page${page.id === activeId ? ' active' : ''}`} style={{ '--depth': depth }} onClick={() => navigate(page.id)}>{page.title}</button>;
      }
      const child = entry.child;
      const key = child.path.join('/');
      const open = expanded.has(key);
      const label = humanizeSegment(child.name);
      const hasKids = child.children.size > 0 || child.pages.length > 0;
      if (child.indexPage) {
        return <div className="tree-group" key={key}>
          <div className="tree-index-row" style={{ '--depth': depth }}>
            {hasKids ? <button type="button" className="tree-chevron-btn" aria-expanded={open} aria-label={`${open ? 'Collapse' : 'Expand'} ${label}`} onClick={() => toggle(key)}>
              <span className="chevron" aria-hidden="true">{open ? '⌄' : '›'}</span>
            </button> : <span className="chevron-spacer" />}
            <button type="button" className={`tree-page tree-index${child.indexPage.id === activeId ? ' active' : ''}`} onClick={() => navigate(child.indexPage.id)}>{label}</button>
          </div>
          {open && hasKids && <TreeBranch node={child} activeId={activeId} navigate={navigate} expanded={expanded} toggle={toggle} depth={depth + 1} />}
        </div>;
      }
      return <div className="tree-group" key={key}>
        <button type="button" className="tree-folder" style={{ '--depth': depth }} aria-expanded={open} onClick={() => toggle(key)}>
          <span className="chevron" aria-hidden="true">{open ? '⌄' : '›'}</span>{label}
        </button>
        {open && <TreeBranch node={child} activeId={activeId} navigate={navigate} expanded={expanded} toggle={toggle} depth={depth + 1} />}
      </div>;
    })}
  </>;
}

function StateScreen({ title, children, action }) {
  return <div className="center-state error-state"><Mark className="mark" size={48} /><h1>{title}</h1>{children}{action}</div>;
}

function Badges({ page }) {
  const door = DOORS.find(item => item.key === page.door);
  return <div className="badges">
    {door && <span className="badge badge-door">{door.label}</span>}
    {page.status && <span className="badge badge-status" data-status={page.status} title={STATUS_MEANING[page.status]}>{page.status}</span>}
  </div>;
}

function SearchResults({ pageResults, symbolResults, textState, query, openPage, activeId }) {
  const symbols = symbolResults.results.length > 0 && <section key="symbols" aria-label="Symbols">
    <div className="nav-label">Symbols · {symbolResults.total}</div>
    {symbolResults.results.map(result => <button type="button" key={result.entry.id} className={`tree-page search-result${result.entry.id === activeId ? ' active' : ''}`} onClick={() => openPage(result.entry.id)}>
      <span>{result.name}</span>
      <small>{result.entry.title}</small>
      {result.declaration && <small className="search-snippet"><code>{result.declaration}</code></small>}
    </button>)}
    {symbolResults.total > symbolResults.results.length && <p className="empty-search">{symbolResults.total - symbolResults.results.length} more symbols. Narrow the query.</p>}
  </section>;
  const shown = pageResults.slice(0, 40);
  const pages = <section key="pages" aria-label="Pages">
    <div className="nav-label">Pages · {pageResults.length}</div>
    {textState.status === 'loading' && <p className="empty-search">Searching page text…</p>}
    {textState.status === 'error' && <p className="empty-search">Page text search is unavailable: {textState.error} <button type="button" className="inline-retry" onClick={textState.retry}>Retry</button></p>}
    {shown.map(result => <button type="button" key={result.page.id} className={`tree-page search-result${result.page.id === activeId ? ' active' : ''}`} onClick={() => openPage(result.page.id)}>
      <span>{result.page.title}</span>
      <small>{result.page.group.map(humanizeSegment).join(' / ') || 'Reference'}</small>
      {result.snippet && <small className="search-snippet">{result.snippet}</small>}
    </button>)}
    {pageResults.length > shown.length && <p className="empty-search">{pageResults.length - shown.length} more pages. Narrow the query.</p>}
    {pageResults.length === 0 && textState.status !== 'loading' && <p className="empty-search">No pages match “{query}”.</p>}
  </section>;
  // A symbol named exactly as typed comes first; otherwise pages do.
  return symbolResults.results[0]?.rank === 0 ? <>{symbols}{pages}</> : <>{pages}{symbols}</>;
}

function App() {
  const route = useRoute();
  const shortcut = useSearchShortcutLabel();
  const index = useResource(`${BASE}reference-index.json`, checkIndex);
  const data = index.value;
  const [query, setQuery] = useState('');
  const deferredQuery = useDeferredValue(query);
  const [wantText, setWantText] = useState(false);
  const [wantExamples, setWantExamples] = useState(false);
  const [expanded, setExpanded] = useState(() => new Set());
  const [mobileOpen, setMobileOpen] = useState(false);
  const [activeHeading, setActiveHeading] = useState(route.heading ?? '');
  const pages = useMemo(() => data?.pages ?? [], [data]);
  const symbols = useMemo(() => data?.symbols ?? {}, [data]);
  const isPlayground = route.kind === 'playground';
  const routeId = isPlayground ? null : route.id;
  const aliasTarget = data ? resolveAlias(pages, data.aliases, routeId) : routeId;
  useEffect(() => {
    if (!isPlayground && aliasTarget !== routeId) location.replace(routeFor(aliasTarget, route.heading));
  }, [aliasTarget, routeId, route.heading, isPlayground]);
  const homeId = pages.find(candidate => candidate.door === 'home')?.id ?? pages[0]?.id;
  const page = isPlayground ? null : pages.find(candidate => candidate.id === (routeId ?? homeId));
  const missing = Boolean(data) && !isPlayground && !page && aliasTarget === routeId;

  const pageData = useResource(page ? pageDataUrl(BASE, page.id) : null);
  const body = pageData.status === 'ready' ? pageData.value : null;
  const searching = deferredQuery.trim().length > 0;
  useEffect(() => { if (query.trim()) setWantText(true); }, [query]);
  useEffect(() => { if (isPlayground) setWantExamples(true); }, [isPlayground]);
  const searchText = useResource(wantText ? `${BASE}search.json` : null, checkSearch);
  const examples = useResource(wantExamples ? `${BASE}examples.json` : null, checkExamples);
  const searchIndex = useMemo(() => searchText.value ? buildSearchIndex(searchText.value.pages) : null, [searchText.value]);
  const symbolIndex = useMemo(() => searching ? buildSymbolIndex(symbols) : [], [searching, symbols]);
  const pageResults = useMemo(() => searching ? searchPagesDetailed(pages, deferredQuery, searchIndex) : [], [searching, pages, deferredQuery, searchIndex]);
  const symbolResults = useMemo(() => searching ? searchSymbols(symbolIndex, deferredQuery) : { total: 0, results: [] }, [searching, symbolIndex, deferredQuery]);

  const doorTrees = useMemo(() => buildDoorTrees(pages), [pages]);
  const markdownComponents = useMemo(() => page ? makeMarkdownComponents(page, symbols) : {}, [page, symbols]);
  const headings = useMemo(() => extractHeadings(body?.markdown ?? ''), [body]);
  const neighbors = useMemo(() => adjacentPages(pages, page), [pages, page]);
  const door = currentDoor(route, page);
  useEffect(() => {
    const onKeyDown = event => {
      if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === 'k') {
        event.preventDefault(); document.querySelector('input[type="search"]')?.focus();
      }
    };
    addEventListener('keydown', onKeyDown);
    return () => removeEventListener('keydown', onKeyDown);
  }, []);
  useEffect(() => {
    if (!page) return;
    const ancestors = groupAncestors(page);
    setExpanded(current => new Set([...current, ...ancestors]));
  }, [page]);
  useEffect(() => {
    setActiveHeading(route.heading ?? '');
    const targets = headings.slice(1).map(item => document.getElementById(item.slug)).filter(Boolean);
    if (!targets.length) return undefined;
    const observer = new IntersectionObserver(entries => {
      const visible = entries.filter(entry => entry.isIntersecting).sort((a, b) => a.boundingClientRect.top - b.boundingClientRect.top)[0];
      if (visible) setActiveHeading(visible.target.id);
    }, { rootMargin: '-10% 0px -75% 0px' });
    targets.forEach(target => observer.observe(target));
    return () => observer.disconnect();
  }, [page, headings, route.heading]);
  // Takes a page id from the sidebar and the header; the playground gets full hash routes.
  const openPage = id => { location.hash = routeFor(id); setMobileOpen(false); setQuery(''); };
  const navigateRoute = hash => { location.hash = hash; setMobileOpen(false); };
  const toggle = key => setExpanded(current => new Set(current.has(key) ? [...current].filter(item => item !== key) : [...current, key]));
  const download = () => {
    if (!body) return;
    const url = URL.createObjectURL(new Blob([body.rawMarkdown ?? body.markdown], { type: 'text/markdown;charset=utf-8' }));
    const anchor = document.createElement('a'); anchor.href = url; anchor.download = `${page.path.split('/').at(-1)}`; anchor.click(); URL.revokeObjectURL(url);
  };
  useEffect(() => {
    if (!route.heading || !body) { window.scrollTo(0, 0); return; }
    requestAnimationFrame(() => document.getElementById(route.heading)?.scrollIntoView({ block: 'start' }));
  }, [route.kind, routeId, route.heading, body]);
  useEffect(() => {
    if (isPlayground) document.title = 'Playground · Plato reference';
    else if (page) document.title = page.id === homeId ? 'Plato reference' : `${page.title} · Plato reference`;
  }, [page, homeId, isPlayground]);
  useEffect(() => {
    // The browser chrome takes the page ground, in whichever theme the viewer has.
    const meta = document.querySelector('meta[name="theme-color"]');
    const apply = () => { if (meta) meta.content = tokens('ground').ground; };
    apply();
    const scheme = matchMedia('(prefers-color-scheme: light)');
    scheme.addEventListener('change', apply);
    return () => scheme.removeEventListener('change', apply);
  }, []);

  if (index.status === 'error') return <StateScreen title="Reference data unavailable" action={<button className="button" onClick={index.retry}>Try again</button>}><p>{index.error}</p><code>python tools/reference/build.py</code></StateScreen>;
  if (!data) return <div className="center-state"><span className="loader" />Loading the Plato reference</div>;
  if (pages.length === 0) return <StateScreen title="No reference pages found"><p>The generated reference data is valid but contains no pages.</p></StateScreen>;
  if (missing) return <StateScreen title="Page not found" action={<button className="button" onClick={() => openPage(homeId)}>Return to the reference</button>}><p>The reference has no page with this address.</p></StateScreen>;

  const activeId = page?.id ?? null;
  const revision = shortRevision(data.sourceRevision);
  const navPanel = <>
    <a className="brand lockup" href={routeFor(homeId)} onClick={() => setQuery('')}><Mark size={22} /><span>Plato<small>Reference</small></span></a>
    <div className="search-wrap"><span className="search-icon">⌕</span><input type="search" placeholder="Search pages and symbols" value={query} onChange={event => setQuery(event.target.value)} aria-label="Search pages and symbols" />{query && <button type="button" onClick={() => setQuery('')} aria-label="Clear search">×</button>}<kbd>{shortcut}</kbd></div>
    <nav className="page-nav" aria-label="Reference pages">
      {searching
        ? <SearchResults pageResults={pageResults} symbolResults={symbolResults} textState={searchText} query={deferredQuery} openPage={openPage} activeId={activeId} />
        : <>
          <button type="button" className={`tree-page home-link${activeId === homeId ? ' active' : ''}`} style={{ '--depth': 0 }} onClick={() => openPage(homeId)}>Home</button>
          {doorTrees.map(item => <section key={item.door.key} aria-label={item.door.label}>
            <div className={`nav-label door-label${door === item.door.key ? ' current' : ''}`}>{item.door.label}</div>
            <TreeBranch node={item.tree} activeId={activeId} navigate={openPage} expanded={expanded} toggle={toggle} />
          </section>)}
          <div className="nav-label door-label">Playground</div>
          <a className={`tree-page${isPlayground ? ' active' : ''}`} style={{ '--depth': 0 }} href={playgroundRoute()} onClick={() => setMobileOpen(false)}>Examples</a>
        </>}
    </nav>
    <div className="sidebar-foot">{pages.length} pages <span className="foot-sep">·</span> source {revision}</div>
  </>;

  const doorBar = <nav className="doors" aria-label="Doors">
    {DOORS.map(item => <a key={item.key} href={doorRoute(item)} className={`door-link${door === item.key ? ' current' : ''}`} aria-current={door === item.key ? 'page' : undefined} onClick={() => setMobileOpen(false)}>{item.label}</a>)}
  </nav>;

  let content;
  if (isPlayground) {
    content = <div className="playground-surface">
      {examples.status === 'error' && <div className="load-state"><p>The examples could not be loaded: {examples.error}</p><button type="button" className="button" onClick={examples.retry}>Try again</button></div>}
      {examples.status !== 'error' && <Suspense fallback={<p className="load-state">Loading the playground…</p>}>
        <Playground examples={examples.value?.groups ?? null} group={route.group} payload={route.payload} navigate={navigateRoute} />
      </Suspense>}
    </div>;
  } else {
    const kickerParts = page.group.map(humanizeSegment).join(' / ') || 'Home';
    content = <div className="reading-layout"><article className={`article${page.id === homeId ? ' home' : ''}`}>
      <div className="article-kicker"><span className="kicker-dot" />Plato reference <span>/</span> {kickerParts}</div>
      <h1 id={headings[0]?.slug ?? slugifyHeading(page.title)}>{page.title}</h1>
      <div className="page-meta">
        <Badges page={page} />
        <a className="revision" href={commitUrl(data.sourceRevision)} target="_blank" rel="noreferrer" title="The source revision this page was built from">source {revision}</a>
      </div>
      {page.status && <p className="status-caption">{STATUS_MEANING[page.status]}</p>}
      <p className="source-path"><a href={githubBlobUrl(data.sourceRevision, page.path)} target="_blank" rel="noreferrer">{page.path}</a></p>
      {pageData.status === 'error' && <div className="load-state"><p>This page could not be loaded: {pageData.error}</p><button type="button" className="button" onClick={pageData.retry}>Try again</button></div>}
      {pageData.status !== 'error' && !body && <div className="load-state" role="status"><span className="loader" />Loading page</div>}
      {body && <>
        <div className="markdown-body"><ReactMarkdown remarkPlugins={[remarkGfm]} components={markdownComponents}>{stripLeadingTitle(body.markdown)}</ReactMarkdown></div>
        {(neighbors.previous || neighbors.next) && <nav className="page-turn" aria-label="Adjacent pages">
          {neighbors.previous ? <a className="page-turn-link prev" href={routeFor(neighbors.previous.id)} onClick={event => { event.preventDefault(); openPage(neighbors.previous.id); }}><span>Previous</span><strong>{neighbors.previous.title}</strong></a> : <span />}
          {neighbors.next ? <a className="page-turn-link next" href={routeFor(neighbors.next.id)} onClick={event => { event.preventDefault(); openPage(neighbors.next.id); }}><span>Next</span><strong>{neighbors.next.title}</strong></a> : <span />}
        </nav>}
      </>}
      <footer className="article-footer"><span>Source revision <code>{data.sourceRevision}</code></span><button type="button" onClick={download} disabled={!body}>Download this page <span>↓</span></button></footer>
    </article>
    {headings.length > 1 && <aside className="toc"><div className="toc-title">On this page</div>{headings.slice(1).map(item => <a key={`${item.slug}-${item.text}`} className={`toc-link level-${item.level}${activeHeading === item.slug ? ' toc-active' : ''}`} href={routeFor(page.id, item.slug)}>{item.text}</a>)}</aside>}
    </div>;
  }

  return <div className="app-shell">
    <aside className="sidebar">{navPanel}</aside>
    {mobileOpen && <div className="mobile-overlay" onClick={() => setMobileOpen(false)}><aside onClick={event => event.stopPropagation()}>{navPanel}</aside></div>}
    <main className="main-area">
      <header className="site-header">
        <button type="button" className="mobile-menu" onClick={() => setMobileOpen(true)} aria-label="Open navigation">☰</button>
        {doorBar}
      </header>
      {!isPlayground && <div className="topbar">
        <div className="breadcrumbs">
          {page.group.map((part, position) => {
            const target = crumbTarget(pages, page.group, position);
            return <React.Fragment key={`${part}-${position}`}>
              {target ? <a href={routeFor(target)} onClick={event => { event.preventDefault(); openPage(target); }}>{humanizeSegment(part)}</a> : <span>{humanizeSegment(part)}</span>}
              <b>/</b>
            </React.Fragment>;
          })}
          <strong>{page.title}</strong>
        </div>
        <button type="button" className="button download-button" onClick={download} disabled={!body} aria-label="Download Markdown"><span aria-hidden="true">↓</span><em>Download Markdown</em></button>
      </div>}
      {content}
    </main>
  </div>;
}

createRoot(document.getElementById('root')).render(<App />);
