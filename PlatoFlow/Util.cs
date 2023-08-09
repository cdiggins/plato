using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Peacock;

namespace Emu
{
    [Mutable]
    public class Profiler
    {
        public Stopwatch Stopwatch = new();
        public List<TimeSpan> Measurements = new();

        public Profiler Start()
        {
            Stopwatch.Reset();
            Stopwatch.Start();
            return this;
        }

        public Profiler Stop()
        {
            Stopwatch.Stop();
            Measurements.Add(Stopwatch.Elapsed);
            return this;
        }

        public Profiler TimeIt(Action action)
        {
            Start();
            action();
            Stop();
            return this;
        }

        public double AverageMsec()
            => Measurements.Average(m => m.TotalMilliseconds);
    }


    public static class Util
    {
        public static Dictionary<string, Profiler> Profiles = new();

        public static Profiler GetProfiler(string name)
        {
            if (Profiles.ContainsKey(name))
                return Profiles[name];
            return Profiles[name] = new Profiler();
        }

        public static Profiler TimeIt(string name, Action action)
            => GetProfiler(name).TimeIt(action);
    }
}
