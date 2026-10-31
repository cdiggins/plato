// The playground door (reference contract, "Reader data v2" and "Playground").
//
// Props:
//   examples  — the `groups` object of examples.json, or null while it loads
//   group     — the selected group name from the route, or null for the group list
//   payload   — the opaque text after `~` in the route, or null
//   navigate  — function(routeString) that moves to a reader hash route
//
// Nothing runs in the browser. The laws describe the original files as checked at build time.
// Share moves the address bar by assigning the hash itself, so it does not depend on whether
// `navigate` takes a page id or a route string.
import { useEffect, useMemo, useState } from 'react';
import { highlightCode } from '../highlight.js';
import {
  PayloadError, currentSources, decodePayload, editedFiles, encodePayload, fileName,
  groupPage, groupTitle, isEdited,
} from './share.js';
import './playground.css';

const plural = (n, word) => `${n} ${word}${n === 1 ? '' : 's'}`;

function GroupList({ examples }) {
  return (
    <section className="playground">
      <h1>Playground</h1>
      <p className="pg-lede">
        Open a checked example, edit it, and share the edit by link. Nothing runs in the browser:
        the laws were checked when the site was built, against the original files.
      </p>
      <ul className="pg-list">
        {Object.entries(examples).map(([name, entry]) => (
          <li key={name}>
            <span>
              <a href={`#/playground/${name}`}>{groupTitle(name)}</a>
              <span className="pg-meta"> · {plural(entry.files.length, 'file')}, {plural(entry.laws.length, 'law')}</span>
            </span>
            <a href={groupPage(name)}>Page that uses it</a>
          </li>
        ))}
      </ul>
    </section>
  );
}

function download(name, text) {
  const url = URL.createObjectURL(new Blob([text], { type: 'text/plain;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = name;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}

function GroupView({ name, entry, payload }) {
  const [edits, setEdits] = useState({});
  const [active, setActive] = useState(0);
  const [error, setError] = useState(null);
  const [status, setStatus] = useState('');

  useEffect(() => {
    let cancelled = false;
    setActive(0);
    setStatus('');
    if (!payload) { setEdits({}); setError(null); return undefined; }
    decodePayload(payload, entry).then(
      decoded => { if (!cancelled) { setEdits(decoded); setError(null); } },
      failure => {
        if (cancelled) return;
        setEdits({});
        setError(failure instanceof PayloadError ? failure.message : 'The shared link could not be opened.');
      },
    );
    return () => { cancelled = true; };
  }, [name, payload, entry]);

  const sources = useMemo(() => currentSources(entry, edits), [entry, edits]);
  const edited = useMemo(() => editedFiles(entry, sources), [entry, sources]);
  const file = entry.files[Math.min(active, entry.files.length - 1)];
  const text = sources[file.path];
  const preview = useMemo(() => highlightCode(text, 'plato'), [text]);
  const anyEdit = isEdited(entry, sources);

  async function share() {
    try {
      const encoded = await encodePayload(edited);
      const route = `#/playground/${name}~${encoded}`;
      window.location.hash = route;
      await navigator.clipboard.writeText(`${window.location.href.split('#')[0]}${route}`);
      setStatus('Link copied.');
    } catch (failure) {
      setStatus(failure instanceof PayloadError ? failure.message : 'Copying the link failed; it is in the address bar.');
    }
  }

  async function copy() {
    try { await navigator.clipboard.writeText(text); setStatus('File copied.'); } catch { setStatus('Copying failed.'); }
  }

  const reset = () => {
    setEdits(previous => {
      const rest = { ...previous };
      delete rest[file.path];
      return rest;
    });
    setStatus('File reset.');
  };

  return (
    <section className="playground">
      <p className="pg-meta"><a href="#/playground">Playground</a> / {groupTitle(name)}</p>
      <h1>{groupTitle(name)}</h1>
      <p className="pg-meta">
        Used by <a href={groupPage(name)}>its page</a>. Compiled with:{' '}
        {entry.inputs.length ? entry.inputs.join(', ') : 'no extra inputs'}.
      </p>

      {error && <p className="pg-notice pg-error" role="alert">{error} Showing the original example.</p>}
      {anyEdit && (
        <p className="pg-notice" role="status">
          Edited. This program was changed and has not been re-run, so the laws below describe the
          original, not your edit.
        </p>
      )}

      <div className="pg-tabs" role="tablist">
        {entry.files.map((f, index) => (
          <button
            key={f.path}
            type="button"
            role="tab"
            className="pg-tab"
            aria-selected={f.path === file.path}
            onClick={() => setActive(index)}
            title={f.path}
          >
            {fileName(f.path)}
            {f.path in edited && <span className="pg-edited-mark" role="img" aria-label="edited">●</span>}
          </button>
        ))}
      </div>
      <p className="pg-path">{file.path}</p>

      <textarea
        className="pg-editor"
        aria-label={`Edit ${fileName(file.path)}`}
        value={text}
        spellCheck={false}
        autoCapitalize="off"
        autoCorrect="off"
        onChange={event => { const value = event.target.value; setEdits(previous => ({ ...previous, [file.path]: value })); }}
      />
      <pre className="pg-preview" aria-label="Highlighted view"><code dangerouslySetInnerHTML={{ __html: preview }} /></pre>

      <div className="pg-actions">
        <button type="button" className="button" disabled={!(file.path in edited)} onClick={reset}>Reset</button>
        <button type="button" className="button" onClick={copy}>Copy</button>
        <button type="button" className="button" onClick={() => download(fileName(file.path), text)}>Download</button>
        <button type="button" className="button" disabled={!anyEdit} onClick={share}>Share</button>
      </div>
      <p className="pg-status" role="status">{status}</p>

      <h2>Laws</h2>
      <p className="pg-meta">Checked when the site was built, against the original files. Nothing here re-runs them.</p>
      <ul className="pg-laws">
        {entry.laws.map(law => <li key={law}>{law}</li>)}
      </ul>
    </section>
  );
}

export default function Playground({ examples, group, payload }) {
  if (!examples) return <p>Loading examples…</p>;
  if (!group) return <GroupList examples={examples} />;
  const entry = Object.prototype.hasOwnProperty.call(examples, group) ? examples[group] : null;
  if (!entry) {
    return (
      <section className="playground">
        <h1>Playground</h1>
        <p className="pg-notice pg-error" role="alert">There is no example group called “{group}”.</p>
        <p><a href="#/playground">See all example groups</a></p>
      </section>
    );
  }
  return <GroupView name={group} entry={entry} payload={payload} />;
}
