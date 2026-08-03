import { readFileSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = join(dirname(fileURLToPath(import.meta.url)), '..');
const path = join(root, 'src', 'plato', 'plato.g.ts');
const stamp = '// @ts-nocheck\n';
const text = readFileSync(path, 'utf8');
if (!text.startsWith(stamp) && !text.includes('@ts-nocheck')) {
  writeFileSync(path, stamp + text);
  console.log('stamped @ts-nocheck on', path);
} else {
  console.log('already stamped', path);
}
