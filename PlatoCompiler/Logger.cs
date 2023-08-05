using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Plato.Compiler
{
    public class Logger
    {
        public Stopwatch Stopwatch = Stopwatch.StartNew();

        public List<string> Messages = new List<string>();

        public static string PrettyPrintTimeElapsed(Stopwatch sw)
            => $"{(int)(Math.Floor(sw.Elapsed.TotalMinutes))}:{sw.Elapsed.Seconds:00}.{sw.Elapsed.Milliseconds:000}";

        public void Log(string message)
        {
            var timeStr = PrettyPrintTimeElapsed(Stopwatch);
            Messages.Add($"[{timeStr}] {message}");
        }
    }
}