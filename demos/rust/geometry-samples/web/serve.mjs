// Zero-dependency static server for the WASM demo.
//
//   node web/serve.mjs [port]     (from demos/rust/geometry-samples)
//
// Serves the crate directory so the page can fetch both /web/* (the app and
// the wasm module) and /src/* (the Rust sources shown in the code panel).

import { createServer } from 'node:http';
import { readFile } from 'node:fs/promises';
import { extname, join, normalize } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = fileURLToPath(new URL('..', import.meta.url));
const port = Number(process.argv[2] ?? 8873);

const mime = {
    '.html': 'text/html; charset=utf-8',
    '.js': 'text/javascript; charset=utf-8',
    '.mjs': 'text/javascript; charset=utf-8',
    '.css': 'text/css; charset=utf-8',
    '.json': 'application/json',
    '.wasm': 'application/wasm',
    '.rs': 'text/plain; charset=utf-8',
    '.md': 'text/plain; charset=utf-8',
    '.toml': 'text/plain; charset=utf-8',
};

createServer(async (req, res) => {
    let path = decodeURIComponent(new URL(req.url, 'http://x').pathname);
    if (path === '/') {
        // Relative URLs in the page must resolve under /web/.
        res.writeHead(302, { Location: '/web/' }).end();
        return;
    }
    if (path === '/web/' || path === '/web') path = '/web/index.html';
    const file = normalize(join(root, path));
    if (!file.startsWith(normalize(root))) {
        res.writeHead(403).end('forbidden');
        return;
    }
    try {
        const body = await readFile(file);
        res.writeHead(200, { 'Content-Type': mime[extname(file)] ?? 'application/octet-stream' });
        res.end(body);
    } catch {
        res.writeHead(404).end('not found');
    }
}).listen(port, () => console.log(`geometry-samples (Rust/WASM) at http://localhost:${port}/`));
