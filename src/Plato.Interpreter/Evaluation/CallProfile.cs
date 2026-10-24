// Collects per-function call counts and execution time during interpreter evaluation.
using System.Diagnostics;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Interpreter;

/// <summary>
/// Where an evaluation spends its time, per bodied function: how many activations it had, its
/// inclusive time (counted once per outermost activation, so a recursive function is not counted
/// twice), and its self time (inclusive minus the time spent in the bodied functions it called).
/// Attach one to <see cref="Interpreter.Profiler"/> to collect; leave it null to pay nothing.
/// </summary>
public sealed class CallProfile
{
    public sealed class Entry
    {
        public FunctionDef Function { get; init; }
        public long Calls { get; internal set; }
        public long InclusiveTicks { get; internal set; }
        public long SelfTicks { get; internal set; }
        internal int Active;

        public TimeSpan Inclusive => TimeSpan.FromSeconds((double)InclusiveTicks / Stopwatch.Frequency);
        public TimeSpan Self => TimeSpan.FromSeconds((double)SelfTicks / Stopwatch.Frequency);
        public string Name => $"{Function.OwnerType?.Name}.{Function.Name}";
    }

    private readonly Dictionary<FunctionDef, Entry> _entries = new Dictionary<FunctionDef, Entry>();
    private readonly Stack<(Entry Entry, long Start, long Children)> _frames = new Stack<(Entry, long, long)>();

    public IEnumerable<Entry> Entries => _entries.Values;

    internal void Enter(FunctionDef f)
    {
        if (!_entries.TryGetValue(f, out var e))
            _entries[f] = e = new Entry { Function = f };
        e.Calls++;
        e.Active++;
        _frames.Push((e, Stopwatch.GetTimestamp(), 0));
    }

    internal void Exit()
    {
        var (e, start, children) = _frames.Pop();
        var elapsed = Stopwatch.GetTimestamp() - start;
        e.SelfTicks += elapsed - children;
        if (--e.Active == 0)
            e.InclusiveTicks += elapsed;
        if (_frames.Count > 0)
        {
            var parent = _frames.Pop();
            _frames.Push((parent.Entry, parent.Start, parent.Children + elapsed));
        }
    }

    /// <summary>The entries with the most self time first, one line each, for a report.</summary>
    public IEnumerable<string> Report(int top = 30)
        => Entries.OrderByDescending(e => e.SelfTicks).Take(top)
            .Select(e => $"{e.Self.TotalSeconds,8:F3}s self {e.Inclusive.TotalSeconds,8:F3}s incl {e.Calls,10} calls  {e.Name}");
}
