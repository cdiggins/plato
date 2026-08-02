import * as vscode from "vscode";
import * as path from "path";
import * as fs from "fs";
import { spawnSync } from "child_process";
import { NavigationClient, NavLocation, HoverContent, HoverResult } from "./navigationClient";

let client: NavigationClient | undefined;
let output: vscode.OutputChannel;
let launch: { command: string; baseArgs: string[] } | undefined;
let activeRoots: string[] = [];

/** Well-known Plato corpora — used only when walking up from an open file. */
const CORPUS_DIR_NAMES = new Set([
  "stdlib",
  "stdlib-tests",
  "stdlib-legacy",
  "stdlib-legacy-tests",
]);

export async function activate(context: vscode.ExtensionContext): Promise<void> {
  output = vscode.window.createOutputChannel("Plato Navigation");
  context.subscriptions.push(output);

  try {
    launch = resolveLaunch(context);
  } catch (e) {
    const msg = e instanceof Error ? e.message : String(e);
    output.appendLine(`Failed to locate navigation CLI: ${msg}`);
    void vscode.window.showWarningMessage(`Plato navigation unavailable: ${msg}`);
    return;
  }

  const selector: vscode.DocumentSelector = { language: "plato", scheme: "file" };

  context.subscriptions.push(
    vscode.languages.registerDefinitionProvider(selector, {
      provideDefinition: (doc, pos) => provideDefinition(doc, pos),
    }),
    vscode.languages.registerReferenceProvider(selector, {
      provideReferences: (doc, pos, ctx) => provideReferences(doc, pos, ctx),
    }),
    vscode.languages.registerHoverProvider(selector, {
      provideHover: (doc, pos) => provideHover(doc, pos),
    }),
    vscode.commands.registerCommand("plato.navigation.reload", async () => {
      try {
        const doc = vscode.window.activeTextEditor?.document;
        await ensureClient(doc?.languageId === "plato" ? doc : undefined, true);
        if (!client) return;
        const status = await client.reload();
        void vscode.window.showInformationMessage(
          `Plato index reloaded: ${status.defs} defs, ${status.refs} refs (${Math.round(status.lastUpdate?.totalMs ?? 0)} ms)`
        );
      } catch (e) {
        const msg = e instanceof Error ? e.message : String(e);
        output.appendLine(`reload failed: ${msg}`);
        void vscode.window.showErrorMessage(`Plato reload failed: ${msg}`);
      }
    }),
    vscode.workspace.onDidSaveTextDocument(async (doc) => {
      if (doc.languageId !== "plato") return;
      try {
        await ensureClient(doc);
        await client?.update([{ path: doc.uri.fsPath, text: doc.getText() }]);
      } catch (e) {
        output.appendLine(`update after save failed: ${e}`);
      }
    }),
    { dispose: () => client?.dispose() }
  );

  output.appendLine("Plato navigation providers registered (index starts on first use).");
}

export function deactivate(): void {
  client?.dispose();
  client = undefined;
}

async function provideDefinition(
  doc: vscode.TextDocument,
  pos: vscode.Position
): Promise<vscode.Location[] | undefined> {
  try {
    await ensureClient(doc);
    if (!client) return undefined;
    await syncDirty(doc);
    const locs = await client.definition(doc.uri.fsPath, pos.line, pos.character);
    output.appendLine(`definition ${doc.uri.fsPath}:${pos.line}:${pos.character} → ${locs.length} hit(s)`);
    return locs.map(toVscodeLocation);
  } catch (e) {
    const msg = e instanceof Error ? e.message : String(e);
    output.appendLine(`definition failed: ${msg}`);
    void vscode.window.setStatusBarMessage(`Plato: ${msg}`, 5000);
    return undefined;
  }
}

async function provideReferences(
  doc: vscode.TextDocument,
  pos: vscode.Position,
  ctx: vscode.ReferenceContext
): Promise<vscode.Location[] | undefined> {
  try {
    await ensureClient(doc);
    if (!client) return undefined;
    await syncDirty(doc);
    const locs = await client.references(
      doc.uri.fsPath,
      pos.line,
      pos.character,
      ctx.includeDeclaration
    );
    output.appendLine(`references ${doc.uri.fsPath}:${pos.line}:${pos.character} → ${locs.length} hit(s)`);
    return locs.map(toVscodeLocation);
  } catch (e) {
    const msg = e instanceof Error ? e.message : String(e);
    output.appendLine(`references failed: ${msg}`);
    void vscode.window.setStatusBarMessage(`Plato: ${msg}`, 5000);
    return undefined;
  }
}

async function provideHover(
  doc: vscode.TextDocument,
  pos: vscode.Position
): Promise<vscode.Hover | undefined> {
  try {
    await ensureClient(doc);
    if (!client) return undefined;
    await syncDirty(doc);
    const result = await client.hover(doc.uri.fsPath, pos.line, pos.character);
    if (!result.contents.length) return undefined;
    const md = formatHover(result);
    const range = result.range
      ? new vscode.Range(
          result.range.line,
          result.range.character,
          result.range.endLine,
          result.range.endCharacter
        )
      : undefined;
    return new vscode.Hover(md, range);
  } catch (e) {
    const msg = e instanceof Error ? e.message : String(e);
    output.appendLine(`hover failed: ${msg}`);
    return undefined;
  }
}

function formatHover(result: HoverResult): vscode.MarkdownString {
  const parts: string[] = [];
  for (const c of result.contents) {
    parts.push(formatHoverContent(c));
  }
  if (result.contents.length > 1)
    parts.unshift(`_${result.contents.length} overloads_`);

  const md = new vscode.MarkdownString(parts.join("\n\n---\n\n"));
  md.isTrusted = false;
  md.supportHtml = false;
  return md;
}

function formatHoverContent(c: HoverContent): string {
  const header = c.signature ? `**${c.kind}** \`${c.signature}\`` : `**${c.kind}** \`${c.name}\``;
  const loc =
    c.file != null && c.line != null
      ? `\n\n*${path.basename(c.file)}:${c.line + 1}*`
      : "";
  const code = c.code ? `\n\n\`\`\`plato\n${c.code}\n\`\`\`` : "";
  return header + loc + code;
}

async function syncDirty(doc: vscode.TextDocument): Promise<void> {
  if (!client || !doc.isDirty) return;
  await client.update([{ path: doc.uri.fsPath, text: doc.getText() }]);
}

function toVscodeLocation(loc: NavLocation): vscode.Location {
  return new vscode.Location(
    vscode.Uri.file(loc.file),
    new vscode.Range(loc.line, loc.character, loc.endLine, loc.endCharacter)
  );
}

/** Start or restart serve so it indexes only the root(s) for this document. */
async function ensureClient(doc: vscode.TextDocument | undefined, forceReload = false): Promise<void> {
  if (!launch) throw new Error("navigation CLI was not resolved");

  const roots = resolveRootsFor(doc);
  if (roots.length === 0)
    throw new Error(
      "No Plato source root for this file. Open a folder workspace or set plato.navigation.roots."
    );

  const same =
    roots.length === activeRoots.length &&
    roots.every((r, i) => pathsEqual(r, activeRoots[i]));

  if (client && same && !forceReload) return;

  client?.dispose();
  client = undefined;
  activeRoots = roots;

  const args = [...launch.baseArgs];
  for (const root of roots) {
    args.push("--root", root);
  }

  output.appendLine(`Starting serve with roots:\n  ${roots.join("\n  ")}`);
  client = new NavigationClient(launch.command, args, output);
  await client.start();
}

function resolveRootsFor(doc: vscode.TextDocument | undefined): string[] {
  const config = vscode.workspace.getConfiguration("plato.navigation");
  const configured = config.get<string[]>("roots") ?? [];
  const folders = vscode.workspace.workspaceFolders?.map((f) => f.uri.fsPath) ?? [];

  if (configured.length > 0) {
    return configured.map((r) =>
      path.isAbsolute(r) ? r : folders.length ? path.resolve(folders[0], r) : path.resolve(r)
    );
  }

  if (doc?.uri.scheme === "file") {
    const root = rootForFile(doc.uri.fsPath, folders);
    return root ? [root] : [];
  }

  // No open file yet — index each workspace folder (that folder + its subfolders).
  return folders;
}

/**
 * Index scope for an open file: nearest corpus directory when under one, otherwise the
 * workspace folder that contains the file. Sibling corpora are never mixed.
 */
function rootForFile(filePath: string, workspaceFolders: string[]): string | undefined {
  const containing = workspaceFolders.find((f) => isUnder(filePath, f));
  let dir = path.dirname(filePath);
  const stopAt = containing ?? path.parse(dir).root;

  while (true) {
    if (CORPUS_DIR_NAMES.has(path.basename(dir))) return dir;
    if (containing && pathsEqual(dir, containing)) return containing;
    if (pathsEqual(dir, stopAt)) return containing ?? dir;

    const parent = path.dirname(dir);
    if (pathsEqual(parent, dir)) return containing ?? dir;
    dir = parent;
  }
}

function isUnder(filePath: string, folder: string): boolean {
  const rel = path.relative(folder, filePath);
  return rel === "" || (!rel.startsWith("..") && !path.isAbsolute(rel));
}

function pathsEqual(a: string, b: string): boolean {
  return path.normalize(a).toLowerCase() === path.normalize(b).toLowerCase();
}

function resolveLaunch(context: vscode.ExtensionContext): { command: string; baseArgs: string[] } {
  const config = vscode.workspace.getConfiguration("plato.navigation");
  const dotnet = config.get<string>("dotnetPath") || "dotnet";

  const searchRoots = [
    ...(vscode.workspace.workspaceFolders?.map((f) => f.uri.fsPath) ?? []),
    // Installed-from-location: vscode-plato/ sits next to src/ in the Plato repo.
    context.extensionPath,
  ];
  const configured = config.get<string>("cliProject");
  const cliProject =
    (configured
      ? path.isAbsolute(configured)
        ? configured
        : path.resolve(searchRoots[0] ?? context.extensionPath, configured)
      : undefined) || findCliProject(searchRoots);
  if (!cliProject)
    throw new Error("Could not find Plato.Navigation.CLI.csproj. Set plato.navigation.cliProject.");

  output.appendLine(`Navigation CLI project: ${cliProject}`);

  const outDir = path.join(path.dirname(cliProject), "bin", "Release", "net8.0");
  const dll = path.join(outDir, "Plato.Navigation.CLI.dll");
  const exe = path.join(outDir, "Plato.Navigation.CLI.exe");

  if (!fs.existsSync(dll) && !fs.existsSync(exe)) {
    output.appendLine(`Building ${cliProject} (dll missing)`);
    const built = spawnSync(dotnet, ["build", cliProject, "-c", "Release", "-v", "q"], {
      cwd: context.extensionPath,
      encoding: "utf8",
      windowsHide: true,
    });
    if (built.status !== 0)
      throw new Error(`dotnet build failed (${built.status}): ${built.stderr || built.stdout}`);
  } else {
    output.appendLine(`Using existing CLI under ${outDir}`);
  }
  if (!fs.existsSync(dll) && !fs.existsSync(exe))
    throw new Error(`Build succeeded but CLI not found under ${outDir}`);

  if (fs.existsSync(exe)) return { command: exe, baseArgs: ["serve"] };
  return { command: dotnet, baseArgs: [dll, "serve"] };
}

/** Relative locations of Plato.Navigation.CLI.csproj under a Plato or studio root. */
const CLI_PROJECT_RELATIVE = [
  // Current layout (post folder restructure): src/Plato.Navigation.CLI/
  path.join("src", "Plato.Navigation.CLI", "Plato.Navigation.CLI.csproj"),
  // Pre-restructure layout at the Plato repo root
  path.join("Plato.Navigation.CLI", "Plato.Navigation.CLI.csproj"),
];

function findCliProject(starts: string[]): string | undefined {
  for (const start of starts) {
    let dir = start;
    for (let i = 0; i < 8; i++) {
      for (const rel of CLI_PROJECT_RELATIVE) {
        // Studio checkout: …/studio/submodules/Plato/<rel>
        const viaSubmodule = path.join(dir, "submodules", "Plato", rel);
        if (fs.existsSync(viaSubmodule)) return viaSubmodule;
        // Plato repo (or vscode-plato parent walk): …/plato/<rel>
        const direct = path.join(dir, rel);
        if (fs.existsSync(direct)) return direct;
      }
      const parent = path.dirname(dir);
      if (parent === dir) break;
      dir = parent;
    }
  }
  return undefined;
}
