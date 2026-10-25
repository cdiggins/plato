using System.Text.Json;
using Microsoft.Diagnostics.Tracing.Etlx;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace Plato.Experiments;

public static class Program
{
    public static void Main(string[] args)
    {
        var etlx = TraceLog.CreateFromEventPipeDataFile(args[0]);
        using var trace = new TraceLog(etlx);
        var rows = new Dictionary<(int Run, string Stage, string Type, string Site), (long Bytes, int Samples)>();
        var stage = "startup";
        var run = 0;
        var missingStacks = 0;
        foreach (var item in trace.Events)
        {
            if (item.ProviderName == "Plato-Experiment-Phases" && (int)item.ID == 1)
            {
                stage = item.PayloadString(0);
                if (stage == "parse") ++run;
            }
            if (item is not GCAllocationTickTraceData allocation) continue;
            var frames = new List<string>();
            for (var stack = item.CallStack(); stack != null; stack = stack.Caller)
                frames.Add(stack.CodeAddress.FullMethodName);
            if (frames.Count == 0) ++missingStacks;
            var site = string.Join(" <- ", frames.Where(f => f.Contains("Ara3D.StepBrep") ||
                f.Contains("Ara3D.Step.Mesher") || f.Contains("Ara3D.Step.CorpusHarness")).Take(8));
            if (site.Length == 0) site = string.Join(" <- ", frames.Take(4));
            var key = (run, stage, allocation.TypeName, site);
            var old = rows.GetValueOrDefault(key);
            rows[key] = (old.Bytes + (long)allocation.AllocationAmount64, old.Samples + 1);
        }
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            eventsLost = trace.EventsLost, missingStacks, runs = run,
            rows = rows.OrderByDescending(x => x.Value.Bytes).Select(x => new
            {
                run = x.Key.Run, stage = x.Key.Stage, type = x.Key.Type, site = x.Key.Site,
                sampledBytes = x.Value.Bytes, samples = x.Value.Samples
            })
        }));
    }
}
