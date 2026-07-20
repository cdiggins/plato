#!/usr/bin/env bash
# Re-vendor Gratify's built ESM + shared widgets into ./ for the static GLSL gallery.
set -e
G=../../../../gratify            # submodules/gratify from this dir
DEST="$(cd "$(dirname "$0")" && pwd)"
rm -rf "$DEST/gratify"; mkdir -p "$DEST/gratify"
( cd "$G/dist" && find . -name '*.js' -exec cp --parents {} "$DEST/gratify/" \; )
node -e 'const fs=require("fs"),path=require("path");(function w(d){for(const e of fs.readdirSync(d,{withFileTypes:true})){const p=path.join(d,e.name);if(e.isDirectory())w(p);else if(e.name.endsWith(".js")){let s=fs.readFileSync(p,"utf8");s=s.replace(/(from\s*")(\.\.?\/[^"]+?)(")/g,(m,a,q,z)=>q.endsWith(".js")?m:a+q+".js"+z);fs.writeFileSync(p,s);}}})(process.argv[1])' "$DEST/gratify"
"$G/node_modules/.bin/esbuild" "$G/examples/shared/widgets.ts" --format=esm --target=es2020 --outfile="$DEST/widgets.js"
echo "re-vendored gratify + widgets"
