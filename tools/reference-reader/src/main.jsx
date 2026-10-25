import React, { useEffect, useMemo, useState } from 'react';
import { createRoot } from 'react-dom/client';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { buildPageTree, extractHeadings, groupAncestors, parseHash, routeFor, searchPages, slugifyHeading, stripLeadingTitle } from './navigation.js';
import './style.css';
import './toc.css';

function useRoute() {
  const [route, setRoute] = useState(() => parseHash(location.hash));
  useEffect(() => {
    const update = () => setRoute(parseHash(location.hash));
    addEventListener('hashchange', update);
    return () => removeEventListener('hashchange', update);
  }, []);
  return route;
}

function Mermaid({ source }) {
  const [markup, setMarkup] = useState('');
  const [failed, setFailed] = useState(false);
  useEffect(() => {
    let live = true;
    import('mermaid').then(({ default: mermaid }) => {
      mermaid.initialize({ startOnLoad: false, securityLevel: 'strict', theme: 'neutral', fontFamily: 'system-ui, sans-serif' });
      return mermaid.render(`diagram-${crypto.randomUUID()}`, source);
    }).then(({ svg }) => {
      if (live) setMarkup(svg);
    }).catch(() => { if (live) setFailed(true); });
    return () => { live = false; };
  }, [source]);
  if (failed) return <pre className="diagram-error"><code>{source}</code></pre>;
  return <div className="diagram" aria-label="Diagram" dangerouslySetInnerHTML={{ __html: markup }} />;
}

function makeMarkdownComponents(page) {
  const heading = level => ({ children, ...props }) => {
    const label = React.Children.toArray(children).map(child => typeof child === 'string' ? child : child?.props?.children ?? '').join('');
    const slug = slugifyHeading(String(label));
    const Tag = `h${level}`;
    return <Tag id={slug} {...props}><a className="heading-anchor" href={routeFor(page.id, slug)} aria-label={`Link to ${label}`}>#</a>{children}</Tag>;
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
      if (language === 'mermaid') return <Mermaid source={String(children).replace(/\n$/, '')} />;
      return <code className={className} {...props}>{children}</code>;
    },
  };
}

function TreeBranch({ node, activeId, navigate, expanded, toggle, depth = 0 }) {
  const childNodes = [...node.children.values()];
  return <>
    {childNodes.map(child => {
      const key = child.path.join('/');
      const open = expanded.has(key);
      return <div className="tree-group" key={key}>
        <button className="tree-folder" style={{ '--depth': depth }} aria-expanded={open} onClick={() => toggle(key)}>
          <span className="chevron" aria-hidden="true">{open ? '⌄' : '›'}</span>{child.name}
        </button>
        {open && <TreeBranch node={child} activeId={activeId} navigate={navigate} expanded={expanded} toggle={toggle} depth={depth + 1} />}
      </div>;
    })}
    {node.pages.map(page => <button key={page.id} className={`tree-page${page.id === activeId ? ' active' : ''}`} style={{ '--depth': depth }} onClick={() => navigate(page.id)}>{page.title}</button>)}
  </>;
}

function App() {
  const route = useRoute();
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
  const tree = useMemo(() => buildPageTree(pages), [pages]);
  const filtered = useMemo(() => query.trim() ? searchPages(pages, query) : null, [pages, query]);
  const page = pages.find(candidate => candidate.id === route.id) ?? pages[0];
  const markdownComponents = useMemo(() => page ? makeMarkdownComponents(page) : {}, [page]);
  const headings = useMemo(() => extractHeadings(page?.markdown ?? ''), [page]);
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
    const url = URL.createObjectURL(new Blob([page.markdown], { type: 'text/markdown;charset=utf-8' }));
    const anchor = document.createElement('a'); anchor.href = url; anchor.download = `${page.path.split('/').at(-1)}`; anchor.click(); URL.revokeObjectURL(url);
  };
  useEffect(() => {
    if (!route.heading) { window.scrollTo(0, 0); return; }
    requestAnimationFrame(() => document.getElementById(route.heading)?.scrollIntoView({ block: 'start' }));
  }, [route.id, route.heading, page]);
  useEffect(() => { if (page) document.title = `${page.title} · Plato Reference`; }, [page]);
  if (!data && !loadError) return <div className="center-state"><span className="loader" />Loading the Plato reference…</div>;
  if (loadError) return <div className="center-state error-state"><div className="mark">P</div><h1>Reference data unavailable</h1><p>{loadError}</p><code>python tools/reference/build.py</code></div>;
  if (pages.length === 0) return <div className="center-state error-state"><div className="mark">P</div><h1>No reference pages found</h1><p>The generated reference data is valid but contains no pages.</p></div>;
  if (route.id && !pages.some(candidate => candidate.id === route.id)) return <div className="center-state error-state"><div className="mark">P</div><h1>Page not found</h1><p>The reference has no page with this address.</p><button className="download-button" onClick={() => navigate(pages[0]?.id ?? '')}>Return to the reference</button></div>;

  const navPanel = <>
    <a className="brand" href="#/" onClick={() => setQuery('')}><span className="brand-mark">P</span><span><strong>Plato</strong><small>REFERENCE LIBRARY</small></span></a>
    <div className="search-wrap"><span className="search-icon">⌕</span><input type="search" placeholder="Search the reference" value={query} onChange={event => setQuery(event.target.value)} aria-label="Search the reference" />{query && <button onClick={() => setQuery('')} aria-label="Clear search">×</button>}<kbd>⌘ K</kbd></div>
    <div className="nav-label">{filtered ? `${filtered.length} RESULTS` : 'REFERENCE'}</div>
    <nav className="page-nav" aria-label="Reference pages">
      {filtered ? filtered.length ? filtered.map(result => <button key={result.id} className={`tree-page search-result${result.id === route.id ? ' active' : ''}`} onClick={() => { navigate(result.id); setQuery(''); }}><span>{result.title}</span><small>{result.group.join(' / ') || 'Reference'}</small></button>) : <p className="empty-search">No pages match “{query}”.</p> : <TreeBranch node={tree} activeId={route.id} navigate={navigate} expanded={expanded} toggle={toggle} />}
    </nav>
    <div className="sidebar-foot"><span className="status-dot" />{pages.length} pages <span className="foot-sep">·</span> source {data.sourceRevision.slice(0, 7)}</div>
  </>;
  return <div className="app-shell">
    <aside className="sidebar">{navPanel}</aside>
    {mobileOpen && <div className="mobile-overlay" onClick={() => setMobileOpen(false)}><aside onClick={event => event.stopPropagation()}>{navPanel}</aside></div>}
    <main className="main-area">
      <header className="topbar"><button className="mobile-menu" onClick={() => setMobileOpen(true)} aria-label="Open navigation">☰</button><div className="breadcrumbs">{page.group.map((part, index) => <React.Fragment key={part}><span>{part}</span><b>/</b></React.Fragment>)}<strong>{page.title}</strong></div><button className="download-button" onClick={download}><span>↓</span> Download Markdown</button></header>
      <div className="reading-layout"><article className="article">
        <div className="article-kicker"><span className="kicker-dot" />PLATO REFERENCE <span>/</span> {page.group.join(' / ') || 'GUIDE'}</div>
        <h1 id={headings[0]?.slug ?? slugifyHeading(page.title)}>{page.title}</h1><p className="source-path">{page.path}</p>
        <div className="markdown-body"><ReactMarkdown remarkPlugins={[remarkGfm]} components={markdownComponents}>{stripLeadingTitle(page.markdown)}</ReactMarkdown></div>
        <footer className="article-footer"><span>Source revision <code>{data.sourceRevision}</code></span><button onClick={download}>Download this page <span>↓</span></button></footer>
      </article>
      {headings.length > 1 && <aside className="toc"><div className="toc-title">ON THIS PAGE</div>{headings.slice(1).map(item => <a key={`${item.slug}-${item.text}`} className={`toc-link level-${item.level}${activeHeading === item.slug ? ' toc-active' : ''}`} href={routeFor(page.id, item.slug)}>{item.text}</a>)}</aside>}
      </div>
    </main>
  </div>;
}

createRoot(document.getElementById('root')).render(<App />);
