const PLATO_KEYWORDS = new Set([
  'type', 'library', 'interface', 'where', 'self', 'Self', 'var', 'if', 'else',
  'switch', 'case', 'default', 'for', 'while', 'break', 'continue', 'return',
  'true', 'false', 'null', 'import', 'export', 'static', 'sealed',
]);

const CSHARP_KEYWORDS = new Set([
  'abstract', 'as', 'base', 'bool', 'break', 'byte', 'case', 'catch', 'char',
  'checked', 'class', 'const', 'continue', 'decimal', 'default', 'delegate',
  'do', 'double', 'else', 'enum', 'event', 'explicit', 'extern', 'false',
  'finally', 'fixed', 'float', 'for', 'foreach', 'goto', 'if', 'implicit',
  'in', 'int', 'interface', 'internal', 'is', 'lock', 'long', 'namespace',
  'new', 'null', 'object', 'operator', 'out', 'override', 'params', 'private',
  'protected', 'public', 'readonly', 'ref', 'return', 'sbyte', 'sealed',
  'short', 'sizeof', 'stackalloc', 'static', 'string', 'struct', 'switch',
  'this', 'throw', 'true', 'try', 'typeof', 'uint', 'ulong', 'unchecked',
  'unsafe', 'ushort', 'using', 'virtual', 'void', 'volatile', 'while', 'var',
  'record', 'init', 'required', 'nameof', 'when', 'await', 'async',
]);

function escapeHtml(text) {
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

function span(kind, text) {
  return `<span class="tok-${kind}">${escapeHtml(text)}</span>`;
}

function highlightGeneric(source, keywords) {
  let result = '';
  let index = 0;
  while (index < source.length) {
    if (source.startsWith('//', index)) {
      const end = source.indexOf('\n', index);
      const stop = end < 0 ? source.length : end;
      result += span('comment', source.slice(index, stop));
      index = stop;
      continue;
    }
    if (source[index] === '"' || source[index] === "'") {
      const quote = source[index];
      let end = index + 1;
      while (end < source.length) {
        if (source[end] === '\\') { end += 2; continue; }
        if (source[end] === quote) { end += 1; break; }
        end += 1;
      }
      result += span('string', source.slice(index, end));
      index = end;
      continue;
    }
    if (/[0-9]/.test(source[index]) && (index === 0 || !/[\w$]/.test(source[index - 1]))) {
      let end = index + 1;
      while (end < source.length && /[0-9.eExX_]/.test(source[end])) end += 1;
      result += span('number', source.slice(index, end));
      index = end;
      continue;
    }
    if (/[A-Za-z_$]/.test(source[index])) {
      let end = index + 1;
      while (end < source.length && /[\w$]/.test(source[end])) end += 1;
      const word = source.slice(index, end);
      if (keywords.has(word)) result += span('keyword', word);
      else if (/^[A-Z]/.test(word)) result += span('type', word);
      else result += span('ident', word);
      index = end;
      continue;
    }
    if (/[{}()\[\];,.<>:=+\-*/%&|!?]/.test(source[index])) {
      let end = index + 1;
      while (end < source.length && /[{}()\[\];,.<>:=+\-*/%&|!?]/.test(source[end])) end += 1;
      result += span('punct', source.slice(index, end));
      index = end;
      continue;
    }
    result += escapeHtml(source[index]);
    index += 1;
  }
  return result;
}

const HIGHLIGHTERS = {
  plato: source => highlightGeneric(source, PLATO_KEYWORDS),
  csharp: source => highlightGeneric(source, CSHARP_KEYWORDS),
  cs: source => highlightGeneric(source, CSHARP_KEYWORDS),
  c: source => highlightGeneric(source, CSHARP_KEYWORDS),
  js: source => highlightGeneric(source, new Set(['const', 'let', 'var', 'function', 'return', 'if', 'else', 'for', 'while', 'class', 'import', 'export', 'from', 'async', 'await', 'true', 'false', 'null', 'undefined'])),
  javascript: source => highlightGeneric(source, new Set(['const', 'let', 'var', 'function', 'return', 'if', 'else', 'for', 'while', 'class', 'import', 'export', 'from', 'async', 'await', 'true', 'false', 'null', 'undefined'])),
  python: source => highlightGeneric(source, new Set(['def', 'class', 'return', 'if', 'elif', 'else', 'for', 'while', 'import', 'from', 'as', 'True', 'False', 'None', 'with', 'yield', 'async', 'await', 'lambda', 'pass', 'raise', 'try', 'except', 'finally'])),
  py: source => highlightGeneric(source, new Set(['def', 'class', 'return', 'if', 'elif', 'else', 'for', 'while', 'import', 'from', 'as', 'True', 'False', 'None', 'with', 'yield', 'async', 'await', 'lambda', 'pass', 'raise', 'try', 'except', 'finally'])),
};

/**
 * Highlight source and optionally wrap known PascalCase / keyword-like symbols
 * with links. Symbols is a map name -> [{ id, title }].
 */
export function highlightCode(source, language, symbols = null) {
  const key = (language || '').toLowerCase();
  const highlighter = HIGHLIGHTERS[key];
  let html = highlighter ? highlighter(source) : escapeHtml(source);
  if (symbols) {
    html = linkSymbolsInHtml(html, symbols);
  }
  return html;
}

function linkSymbolsInHtml(html, symbols) {
  return html.replace(/<span class="tok-(?:type|ident)">([^<]+)<\/span>/g, (match, name) => {
    const target = resolvePreferred(symbols, name);
    if (!target) return match;
    return `<a class="tok-link" href="#/${target.id}">${match}</a>`;
  });
}

function resolvePreferred(symbols, name) {
  const entries = symbols[name];
  if (!entries?.length) return null;
  return [...entries].sort((a, b) => {
    const rank = entry => {
      const titled = entry.title === name || entry.title.endsWith(`: ${name}`);
      let value = titled ? 0 : 20;
      if (entry.id.includes('/api/foundation/')) value -= 2;
      if (entry.id.includes('/api/legacy/')) value += 5;
      return value;
    };
    return rank(a) - rank(b) || a.id.localeCompare(b.id);
  })[0];
}

export function languageLabel(language) {
  if (!language) return '';
  const labels = { plato: 'Plato', csharp: 'C#', cs: 'C#', js: 'JavaScript', javascript: 'JavaScript', python: 'Python', py: 'Python', text: 'Text', plain: 'Text' };
  return labels[language.toLowerCase()] ?? language;
}
