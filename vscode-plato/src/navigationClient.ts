import { ChildProcessWithoutNullStreams, spawn } from "child_process";
import * as readline from "readline";
import type * as vscode from "vscode";

export interface NavLocation {
  file: string;
  line: number;
  character: number;
  endLine: number;
  endCharacter: number;
  name?: string;
  kind?: string;
  id?: number;
}

export interface HoverContent {
  kind: string;
  name: string;
  signature?: string;
  code?: string;
  file?: string;
  line?: number;
}

export interface HoverResult {
  contents: HoverContent[];
  range?: {
    line: number;
    character: number;
    endLine: number;
    endCharacter: number;
  };
  name?: string;
}

interface StatusResult {
  generation: string;
  files: number;
  defs: number;
  refs: number;
  diagnostics?: number;
  lastUpdate?: {
    filesParsed: number;
    filesReused: number;
    parseMs: number;
    bindMs: number;
    totalMs: number;
  };
}

interface Pending {
  resolve: (value: any) => void;
  reject: (reason: Error) => void;
  timer: NodeJS.Timeout;
}

const REQUEST_TIMEOUT_MS = 60_000;

/** NDJSON client for Plato.Navigation.CLI `serve`. Restarts the process if it dies. */
export class NavigationClient {
  private child: ChildProcessWithoutNullStreams | undefined;
  private readonly pending = new Map<number, Pending>();
  private nextId = 1;
  private generation = 0;
  private startPromise: Promise<void> | undefined;

  constructor(
    private readonly command: string,
    private readonly args: string[],
    private readonly output: vscode.OutputChannel
  ) {}

  start(): Promise<void> {
    return this.ensureStarted();
  }

  dispose(): void {
    this.failAll(new Error("disposed"));
    this.killChild();
  }

  definition(file: string, line: number, column: number): Promise<NavLocation[]> {
    return this.request("definition", { file, line, column }).then((r) => r.locations ?? []);
  }

  references(
    file: string,
    line: number,
    column: number,
    includeDeclaration: boolean
  ): Promise<NavLocation[]> {
    return this.request("references", { file, line, column, includeDeclaration }).then(
      (r) => r.locations ?? []
    );
  }

  hover(file: string, line: number, column: number): Promise<HoverResult> {
    return this.request("hover", { file, line, column }).then((r) => ({
      contents: r.contents ?? [],
      range: r.range,
      name: r.name,
    }));
  }

  reload(): Promise<StatusResult> {
    return this.request("reload", {});
  }

  update(files: { path: string; text: string }[]): Promise<StatusResult> {
    return this.request("update", { files });
  }

  private ensureStarted(): Promise<void> {
    if (this.child?.stdin.writable) return Promise.resolve();
    if (this.startPromise) return this.startPromise;

    this.startPromise = this.spawnServe().finally(() => {
      this.startPromise = undefined;
    });
    return this.startPromise;
  }

  private spawnServe(): Promise<void> {
    this.killChild();
    const gen = ++this.generation;

    return new Promise<void>((resolve, reject) => {
      this.output.appendLine(`spawn: ${this.command} ${this.args.join(" ")}`);
      const child = spawn(this.command, this.args, {
        stdio: ["pipe", "pipe", "pipe"],
        windowsHide: true,
      });
      this.child = child;

      let settled = false;
      const succeed = () => {
        if (settled) return;
        settled = true;
        resolve();
      };
      const fail = (e: Error) => {
        if (settled) return;
        settled = true;
        reject(e);
      };

      child.on("error", (e) => {
        this.output.appendLine(`serve error: ${e.message}`);
        if (this.generation === gen) this.child = undefined;
        this.failAll(e);
        fail(e);
      });

      child.on("close", (code) => {
        const err = new Error(`navigation serve exited (${code})`);
        this.output.appendLine(err.message);
        if (this.generation === gen) this.child = undefined;
        this.failAll(err);
        fail(err);
      });

      child.stderr.setEncoding("utf8");
      child.stderr.on("data", (chunk: string) => {
        for (const line of chunk.split(/\r?\n/)) if (line.trim()) this.output.appendLine(`[stderr] ${line}`);
      });

      const rl = readline.createInterface({ input: child.stdout });
      rl.on("line", (line) => {
        if (this.generation !== gen) return;
        this.onLine(line, succeed);
      });
    });
  }

  private killChild(): void {
    const child = this.child;
    this.child = undefined;
    if (!child) return;
    try {
      child.stdin.end();
    } catch {
      /* ignore */
    }
    child.kill();
  }

  private request(op: string, body: Record<string, unknown>): Promise<any> {
    return this.ensureStarted().then(
      () =>
        new Promise((resolve, reject) => {
          if (!this.child?.stdin.writable) {
            reject(new Error("navigation serve is not running"));
            return;
          }
          const id = this.nextId++;
          const timer = setTimeout(() => {
            if (!this.pending.has(id)) return;
            this.pending.delete(id);
            reject(new Error(`navigation ${op} timed out after ${REQUEST_TIMEOUT_MS}ms`));
          }, REQUEST_TIMEOUT_MS);
          this.pending.set(id, { resolve, reject, timer });
          const payload = JSON.stringify({ id, op, ...body });
          this.child.stdin.write(payload + "\n", (err) => {
            if (err) {
              this.pending.delete(id);
              clearTimeout(timer);
              reject(err);
            }
          });
        })
    );
  }

  private onLine(line: string, onReady: () => void): void {
    let msg: any;
    try {
      msg = JSON.parse(line);
    } catch {
      this.output.appendLine(`bad NDJSON: ${line}`);
      return;
    }

    if (msg.op === "ready") {
      this.output.appendLine(
        `indexed ${msg.files} files, ${msg.defs} defs, ${msg.refs} refs (gen ${String(msg.generation).slice(0, 12)})`
      );
      onReady();
      return;
    }

    const id = msg.id as number | undefined;
    if (id == null) {
      this.output.appendLine(`message without id: ${line}`);
      return;
    }

    const pending = this.pending.get(id);
    if (!pending) return;
    this.pending.delete(id);
    clearTimeout(pending.timer);

    if (msg.ok === false) pending.reject(new Error(msg.error ?? "request failed"));
    else pending.resolve(msg.result ?? msg);
  }

  private failAll(err: Error): void {
    for (const [, p] of this.pending) {
      clearTimeout(p.timer);
      p.reject(err);
    }
    this.pending.clear();
  }
}
