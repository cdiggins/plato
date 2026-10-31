import React, { useEffect, useMemo, useState } from 'react';
import { createRoot } from 'react-dom/client';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { highlightCode, languageLabel } from './highlight.js';
import {
  adjacentPages, buildPageTree, crumbTarget, extractHeadings, githubBlobUrl, groupAncestors,
  humanizeSegment, parseHash, resolveSymbol, routeFor, searchPagesDetailed, slugifyHeading,
  stripLeadingTitle,
} from './navigation.js';
import '@fontsource/spectral/600.css';
import '@fontsource/ibm-plex-sans/400.css';
import '@fontsource/ibm-plex-sans/500.css';
import '@fontsource/ibm-plex-sans/600.css';
import '@fontsource/ibm-plex-mono/400.css';
import './style.css';

/** The root spiral from docs/assets/icon, copied beside tokens.css by the builder. */
function Mark({ className, size }) {
  return <picture className={className}>
    <source srcSet="/brand/root-spiral-light.svg" media="(prefers-color-scheme: light)" />
    <img src="/brand/root-spiral.svg" alt="" width={size} height={size} />
  </picture>;
}

/** The brand tokens as the page computed them, for code that cannot read CSS. */
function tokens(...names) {
  const style = getComputedStyle(document.documentElement);
  return Object.fromEntries(names.map(name => [name, style.getPropertyValue(`--${name}`).trim()]));
}

function useRoute() {
  const [route, setRoute] = useState(() => parseHash(location.hash));
  useEffect(() => {
    const update = () => setRoute(parseHash(location.hash));
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
    a: ({ href = '', children, ...props }) => {
      const routeLink = href.startsWith('#/') || href.startsWith('#');
      return <a href={href} {...props} onClick={event => {
        if (routeLink) { event.preventDefault(); location.hash = href.slice(1).startsWith('/') ? href.slice(1) : `/${page.id}@${href.slice(1)}`; }
      }}>{children}</a>;
    },
    code: ({ className, children, ...props }) => {
      const language = /language-(\w+)/.exec(className ?? '')?.[1];
      const source = String(children).replace(/\n$/, '');
      if (language === 'mermaid') return <Mermaid source={source} />;
      if (language || String(children).includes('\n')) {
        return <CodeBlock language={language} source={source} symbols={symbols} />;
      }
      return <InlineCode symbols={symbols} className={className} {...props}>{children}</InlineCode>;
    },
    pre: ({ children }) => <>{children}</>,
  };
}

function TreeBranch({ node, activeId, navigate, expanded, toggle, depth = 0 }) {
  const childNodes = [...node.children.values()];
  return <>
    {childNodes.map(child => {
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
    {node.pages.map(page => <button type="button" key={page.id} className={`tree-page${page.id === activeId ? ' active' : ''}`} style={{ '--depth': depth }} onClick={() => navigate(page.id)}>{page.title}</button>)}
  </>;
}

function App() {
  const route = useRoute();
  const shortcut = useSearchShortcutLabel();
  const [data, setData] = useState(null);
  const [loadError, setLoadError] = useState('');
  const [query, setQuery] = useState('');
  const [expanded, setExpanded] = useState(() => new Set());
  const [mobileOpen, setMobileOpen] = useState(false);
  const [activeHeading, setActiveHeading] = useState(route.heading ?? '');
  useEffect(() => {
    fetch('/reference.json').then(response => {
      if (!response.ok) throw new Error(`Reference data returned ${response.status}. Run the reference builder first.`);
      return response.json();
    }).then(value => {
      if (value.schemaVersion !== 1 || !Array.isArray(value.pages)) throw new Error('Reference data has an unsupported schema.');
      setData(value);
    }).catch(error => setLoadError(error.message));
  }, []);
  const pages = data?.pages ?? [];
  const symbols = data?.symbols ?? {};
  const tree = useMemo(() => buildPageTree(pages), [pages]);
  const filtered = useMemo(() => query.trim() ? searchPagesDetailed(pages, query) : null, [pages, query]);
  const homeId = pages.find(page => page.id === 'docs/reference/README')?.id
    ?? pages.find(page => page.id === 'docs/reference/start/what-is-plato')?.id
    ?? pages[0]?.id;
  const page = pages.find(candidate => candidate.id === route.id) ?? pages.find(candidate => candidate.id === homeId) ?? pages[0];
  const markdownComponents = useMemo(() => page ? makeMarkdownComponents(page, symbols) : {}, [page, symbols]);
  const headings = useMemo(() => extractHeadings(page?.markdown ?? ''), [page]);
  const neighbors = useMemo(() => adjacentPages(pages, page), [pages, page]);
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
    if (!targets.length) return;
    const observer = new IntersectionObserver(entries => {
      const visible = entries.filter(entry => entry.isIntersecting).sort((a, b) => a.boundingClientRect.top - b.boundingClientRect.top)[0];
      if (visible) setActiveHeading(visible.target.id);
    }, { rootMargin: '-10% 0px -75% 0px' });
    targets.forEach(target => observer.observe(target));
    return () => observer.disconnect();
  }, [page, headings, route.heading]);
  const navigate = id => { location.hash = routeFor(id); setMobileOpen(false); };
  const toggle = key => setExpanded(current => new Set(current.has(key) ? [...current].filter(item => item !== key) : [...current, key]));
  const download = () => {
    const url = URL.createObjectURL(new Blob([page.rawMarkdown ?? page.markdown], { type: 'text/markdown;charset=utf-8' }));
    const anchor = document.createElement('a'); anchor.href = url; anchor.download = `${page.path.split('/').at(-1)}`; anchor.click(); URL.revokeObjectURL(url);
  };
  useEffect(() => {
    if (!route.heading) { window.scrollTo(0, 0); return; }
    requestAnimationFrame(() => document.getElementById(route.heading)?.scrollIntoView({ block: 'start' }));
  }, [route.id, route.heading, page]);
  useEffect(() => { if (page) document.title = page.id === homeId ? 'Plato reference' : `${page.title} · Plato reference`; }, [page, homeId]);
  useEffect(() => {
    // The browser chrome takes the page ground, in whichever theme the viewer has.
    const meta = document.querySelector('meta[name="theme-color"]');
    const apply = () => { if (meta) meta.content = tokens('ground').ground; };
    apply();
    const scheme = matchMedia('(prefers-color-scheme: light)');
    scheme.addEventListener('change', apply);
    return () => scheme.removeEventListener('change', apply);
  }, []);
  if (!data && !loadError) return <div className="center-state"><span className="loader" />Loading the Plato reference</div>;
  if (loadError) return <div className="center-state error-state"><Mark className="mark" size={48} /><h1>Reference data unavailable</h1><p>{loadError}</p><code>python tools/reference/build.py</code></div>;
  if (pages.length === 0) return <div className="center-state error-state"><Mark className="mark" size={48} /><h1>No reference pages found</h1><p>The generated reference data is valid but contains no pages.</p></div>;
  if (route.id && !pages.some(candidate => candidate.id === route.id)) return <div className="center-state error-state"><Mark className="mark" size={48} /><h1>Page not found</h1><p>The reference has no page with this address.</p><button className="button" onClick={() => navigate(homeId ?? '')}>Return to the reference</button></div>;

  const kicker = page.group.length ? page.group.map(humanizeSegment).join(' / ') : 'Home';
  const sourceHref = githubBlobUrl(data.sourceRevision, page.path);

  const navPanel = <>
    <a className="brand lockup" href={routeFor(homeId)} onClick={() => setQuery('')}><Mark size={22} /><span>Plato<small>Reference</small></span></a>
    <div className="search-wrap"><span className="search-icon">⌕</span><input type="search" placeholder="Search the reference" value={query} onChange={event => setQuery(event.target.value)} aria-label="Search the reference" />{query && <button type="button" onClick={() => setQuery('')} aria-label="Clear search">×</button>}<kbd>{shortcut}</kbd></div>
    <div className="nav-label">{filtered ? `${filtered.length} results` : 'Contents'}</div>
    <nav className="page-nav" aria-label="Reference pages">
      {filtered ? filtered.length ? filtered.map(result => <button type="button" key={result.page.id} className={`tree-page search-result${result.page.id === route.id ? ' active' : ''}`} onClick={() => { navigate(result.page.id); setQuery(''); }}>
        <span>{result.page.title}</span>
        <small>{result.page.group.map(humanizeSegment).join(' / ') || 'Reference'}</small>
        {result.snippet && <small className="search-snippet">{result.snippet}</small>}
      </button>) : <p className="empty-search">No pages match “{query}”.</p> : <TreeBranch node={tree} activeId={route.id} navigate={navigate} expanded={expanded} toggle={toggle} />}
    </nav>
    <div className="sidebar-foot"><span className="status-dot" />{pages.length} pages <span className="foot-sep">·</span> source {data.sourceRevision.slice(0, 7)}</div>
  </>;

  return <div className="app-shell">
    <aside className="sidebar">{navPanel}</aside>
    {mobileOpen && <div className="mobile-overlay" onClick={() => setMobileOpen(false)}><aside onClick={event => event.stopPropagation()}>{navPanel}</aside></div>}
    <main className="main-area">
      <header className="topbar">
        <button type="button" className="mobile-menu" onClick={() => setMobileOpen(true)} aria-label="Open navigation">☰</button>
        <div className="breadcrumbs">
          {page.group.map((part, index) => {
            const target = crumbTarget(pages, page.group, index);
            return <React.Fragment key={`${part}-${index}`}>
              {target ? <a href={routeFor(target)} onClick={event => { event.preventDefault(); navigate(target); }}>{humanizeSegment(part)}</a> : <span>{humanizeSegment(part)}</span>}
              <b>/</b>
            </React.Fragment>;
          })}
          <strong>{page.title}</strong>
        </div>
        <button type="button" className="button download-button" onClick={download} aria-label="Download Markdown"><span aria-hidden="true">↓</span><em>Download Markdown</em></button>
      </header>
      <div className="reading-layout"><article className={`article${page.id === homeId ? ' home' : ''}`}>
        <div className="article-kicker"><span className="kicker-dot" />Plato reference <span>/</span> {kicker}</div>
        <h1 id={headings[0]?.slug ?? slugifyHeading(page.title)}>{page.title}</h1>
        <p className="source-path"><a href={sourceHref} target="_blank" rel="noreferrer">{page.path}</a></p>
        <div className="markdown-body"><ReactMarkdown remarkPlugins={[remarkGfm]} components={markdownComponents}>{stripLeadingTitle(page.markdown)}</ReactMarkdown></div>
        {(neighbors.previous || neighbors.next) && <nav className="page-turn" aria-label="Adjacent pages">
          {neighbors.previous ? <a className="page-turn-link prev" href={routeFor(neighbors.previous.id)} onClick={event => { event.preventDefault(); navigate(neighbors.previous.id); }}><span>Previous</span><strong>{neighbors.previous.title}</strong></a> : <span />}
          {neighbors.next ? <a className="page-turn-link next" href={routeFor(neighbors.next.id)} onClick={event => { event.preventDefault(); navigate(neighbors.next.id); }}><span>Next</span><strong>{neighbors.next.title}</strong></a> : <span />}
        </nav>}
        <footer className="article-footer"><span>Source revision <code>{data.sourceRevision}</code></span><button type="button" onClick={download}>Download this page <span>↓</span></button></footer>
      </article>
      {headings.length > 1 && <aside className="toc"><div className="toc-title">On this page</div>{headings.slice(1).map(item => <a key={`${item.slug}-${item.text}`} className={`toc-link level-${item.level}${activeHeading === item.slug ? ' toc-active' : ''}`} href={routeFor(page.id, item.slug)}>{item.text}</a>)}</aside>}
      </div>
    </main>
  </div>;
}

createRoot(document.getElementById('root')).render(<App />);
