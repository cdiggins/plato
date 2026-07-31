using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ara3D.Logging;

namespace Ara3D.Geometry.Compiler
{
    public class Logger : ILogger
    {
        public Stopwatch Stopwatch = Stopwatch.StartNew();

        public List<string> Messages = new List<string>();

        public static string PrettyPrintTimeElapsed(Stopwatch sw)
            => $"{(int)(Math.Floor(sw.Elapsed.TotalMinutes))}:{sw.Elapsed.Seconds:00}.{sw.Elapsed.Milliseconds:000}";

        public ILogger Log(string message, LogLevel level)
        {
            var timeStr = PrettyPrintTimeElapsed(Stopwatch);
            Messages.Add($"[{timeStr}] {message}");
            return this; 
        }

        public string Name => "Compilation";
    }
}