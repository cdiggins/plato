// The CPU (cpu64i32) prelude: the fragments under prelude/*.hpp, embedded, written beside plato_cpu.hpp by CppWriter in dialect Cpu.
// plato_cpu.hpp #includes the fragments in order; the options are PLATO_* macros in plato_config.hpp.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ara3D.Geometry.CppWriter
{
    public static class CppCpuPrelude
    {
        private const string ConfigName = "plato_config.hpp";
        private const string MainName = "plato_cpu.hpp";

        private static readonly Lazy<IReadOnlyDictionary<string, string>> _embedded = new(ReadEmbedded);

        private static IReadOnlyDictionary<string, string> ReadEmbedded()
        {
            var assembly = typeof(CppCpuPrelude).Assembly;
            var files = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var resource in assembly.GetManifestResourceNames().Where(n => n.StartsWith("prelude/")))
            {
                using var stream = assembly.GetManifestResourceStream(resource)!;
                using var reader = new StreamReader(stream);
                files[resource.Substring("prelude/".Length)] = reader.ReadToEnd().Replace("\r\n", "\n");
            }
            if (!files.ContainsKey(MainName) || !files.ContainsKey(ConfigName))
                throw new InvalidOperationException("The prelude fragments are not embedded in Plato.CppWriter.");
            return files;
        }

        /// <summary>plato_config.hpp for these options: the embedded defaults with the option values written in. Each
        /// macro is guarded by <c>#ifndef</c>, so a -D on the compile line wins.</summary>
        public static string ConfigFor(CppCpuOptions o)
        {
            var text = _embedded.Value[ConfigName];
            if (o == null)
                return text;
            text = SetMacro(text, "PLATO_BORROW_MIN_BYTES", o.BorrowMinBytes.ToString(System.Globalization.CultureInfo.InvariantCulture));
            text = SetMacro(text, "PLATO_BORROW_NON_TRIVIAL", o.BorrowNonTrivial ? "1" : "0");
            return SetMacro(text, "PLATO_BORROW_READS", o.BorrowReads ? "1" : "0");
        }

        private static string SetMacro(string text, string name, string value)
        {
            var result = Regex.Replace(text, $@"(?m)^#define {name} .*$", $"#define {name} {value}");
            if (result == text && !text.Contains($"#define {name} {value}\n"))
                throw new InvalidOperationException($"{name} is not defined in {ConfigName}.");
            return result;
        }

        /// <summary>Every file of the prelude for these options, by name: plato_cpu.hpp, plato_config.hpp and the fragments.</summary>
        public static IEnumerable<KeyValuePair<string, string>> FilesFor(CppCpuOptions o)
        {
            foreach (var file in _embedded.Value)
                yield return file.Key == ConfigName ? new KeyValuePair<string, string>(ConfigName, ConfigFor(o)) : file;
        }

        /// <summary>The default-option text of every prelude file, joined: for scans of the names the prelude defines.</summary>
        public static string Text => string.Join("\n", _embedded.Value.Values);
    }
}
