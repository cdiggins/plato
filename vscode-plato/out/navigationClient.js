"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.NavigationClient = void 0;
const child_process_1 = require("child_process");
const readline = __importStar(require("readline"));
const REQUEST_TIMEOUT_MS = 60000;
/** NDJSON client for Plato.Navigation.CLI `serve`. Restarts the process if it dies. */
class NavigationClient {
    constructor(command, args, output) {
        this.command = command;
        this.args = args;
        this.output = output;
        this.pending = new Map();
        this.nextId = 1;
        this.generation = 0;
    }
    start() {
        return this.ensureStarted();
    }
    dispose() {
        this.failAll(new Error("disposed"));
        this.killChild();
    }
    definition(file, line, column) {
        return this.request("definition", { file, line, column }).then((r) => r.locations ?? []);
    }
    references(file, line, column, includeDeclaration) {
        return this.request("references", { file, line, column, includeDeclaration }).then((r) => r.locations ?? []);
    }
    hover(file, line, column) {
        return this.request("hover", { file, line, column }).then((r) => ({
            contents: r.contents ?? [],
            range: r.range,
            name: r.name,
        }));
    }
    reload() {
        return this.request("reload", {});
    }
    update(files) {
        return this.request("update", { files });
    }
    ensureStarted() {
        if (this.child?.stdin.writable)
            return Promise.resolve();
        if (this.startPromise)
            return this.startPromise;
        this.startPromise = this.spawnServe().finally(() => {
            this.startPromise = undefined;
        });
        return this.startPromise;
    }
    spawnServe() {
        this.killChild();
        const gen = ++this.generation;
        return new Promise((resolve, reject) => {
            this.output.appendLine(`spawn: ${this.command} ${this.args.join(" ")}`);
            const child = (0, child_process_1.spawn)(this.command, this.args, {
                stdio: ["pipe", "pipe", "pipe"],
                windowsHide: true,
            });
            this.child = child;
            let settled = false;
            const succeed = () => {
                if (settled)
                    return;
                settled = true;
                resolve();
            };
            const fail = (e) => {
                if (settled)
                    return;
                settled = true;
                reject(e);
            };
            child.on("error", (e) => {
                this.output.appendLine(`serve error: ${e.message}`);
                if (this.generation === gen)
                    this.child = undefined;
                this.failAll(e);
                fail(e);
            });
            child.on("close", (code) => {
                const err = new Error(`navigation serve exited (${code})`);
                this.output.appendLine(err.message);
                if (this.generation === gen)
                    this.child = undefined;
                this.failAll(err);
                fail(err);
            });
            child.stderr.setEncoding("utf8");
            child.stderr.on("data", (chunk) => {
                for (const line of chunk.split(/\r?\n/))
                    if (line.trim())
                        this.output.appendLine(`[stderr] ${line}`);
            });
            const rl = readline.createInterface({ input: child.stdout });
            rl.on("line", (line) => {
                if (this.generation !== gen)
                    return;
                this.onLine(line, succeed);
            });
        });
    }
    killChild() {
        const child = this.child;
        this.child = undefined;
        if (!child)
            return;
        try {
            child.stdin.end();
        }
        catch {
            /* ignore */
        }
        child.kill();
    }
    request(op, body) {
        return this.ensureStarted().then(() => new Promise((resolve, reject) => {
            if (!this.child?.stdin.writable) {
                reject(new Error("navigation serve is not running"));
                return;
            }
            const id = this.nextId++;
            const timer = setTimeout(() => {
                if (!this.pending.has(id))
                    return;
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
        }));
    }
    onLine(line, onReady) {
        let msg;
        try {
            msg = JSON.parse(line);
        }
        catch {
            this.output.appendLine(`bad NDJSON: ${line}`);
            return;
        }
        if (msg.op === "ready") {
            this.output.appendLine(`indexed ${msg.files} files, ${msg.defs} defs, ${msg.refs} refs (gen ${String(msg.generation).slice(0, 12)})`);
            onReady();
            return;
        }
        const id = msg.id;
        if (id == null) {
            this.output.appendLine(`message without id: ${line}`);
            return;
        }
        const pending = this.pending.get(id);
        if (!pending)
            return;
        this.pending.delete(id);
        clearTimeout(pending.timer);
        if (msg.ok === false)
            pending.reject(new Error(msg.error ?? "request failed"));
        else
            pending.resolve(msg.result ?? msg);
    }
    failAll(err) {
        for (const [, p] of this.pending) {
            clearTimeout(p.timer);
            p.reject(err);
        }
        this.pending.clear();
    }
}
exports.NavigationClient = NavigationClient;
//# sourceMappingURL=navigationClient.js.map