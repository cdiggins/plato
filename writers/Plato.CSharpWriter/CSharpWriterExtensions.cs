using System;
using Ara3D.Utils;

namespace Ara3D.Geometry.CSharpWriter
{
    public static class CSharpWriterExtensions
    {
        // CodeBuilder.Write appends a multi-line fragment (the output of a nested builder)
        // verbatim and leaves AtNewLine false, so whatever is written NEXT misses its
        // indentation - the cause of the historical indentation inconsistencies in the
        // generated output (roadmap "Phase 2 revision" item 5). Fragments produced by nested
        // builders always end in a newline; re-emitting that final newline through WriteLine
        // keeps the builder's line-start state in sync so the following line is indented.
        // Only the extension style calls this: the default mode's output is a byte-identity
        // gate and keeps the quirk until V1 is retired.
        public static T WriteWithLineStateSync<T>(this T builder, string fragment) where T : CodeBuilder<T>
        {
            if (string.IsNullOrEmpty(fragment))
                return builder;
            if (!fragment.EndsWith("\n"))
                return builder.Write(fragment);
            var cut = fragment.EndsWith("\r\n") ? fragment.Length - 2 : fragment.Length - 1;
            if (cut > 0)
                builder.Write(fragment.Substring(0, cut));
            return builder.WriteLine();
        }

        // extensionStyle = false: original writer, byte-identical output (production default).
        // extensionStyle = true : classic-extension-method output (--csharp-style=extensions, roadmap P2.2).
        // optimize = true: component-op unrolling (--optimize, roadmap P3.1; see ComponentUnroller).
        // Scalars are always WRAPPER structs: Number / Integer / Boolean / Character / String stay
        // distinct types. Erasure to native primitives was retired 2026-08-01 (see that day's ADR).
        // Function bodies emit from the monomorphized TIR (Elaborate → Monomorphize → Emit) — the
        // sole C# body writer since the legacy CSharpFunctionBodyWriter was retired (C4).
        // optimizeArrays = true: loop-into-buffer lowering of multi-consumed Map/MapRange results
        //                (--optimize-arrays, optimizer stage 2 increment 1; see TirArrayMaterializer).
        // The emitted C# is always property-free: a no-arg member emits as a METHOD unless the
        // handwritten runtime spells it as a struct field/property (see the 2026-08-01 ADR).
        public static CSharpWriter ToCSharp(this Compiler.Compilation compilation, DirectoryPath outputFolder, bool extensionStyle = false, bool optimize = false, bool optimizeArrays = false, bool inlineCalls = false, bool lowerLoops = false, string tirDumpDir = null, bool inlineReport = false, bool staticAbstract = false)
        {
            var writer = new CSharpWriter(compilation, outputFolder) { ExtensionStyle = extensionStyle, Optimize = optimize, OptimizeArrays = optimizeArrays, InlineCalls = inlineCalls, LowerLoops = lowerLoops, StaticAbstract = staticAbstract, TirDumpDir = string.IsNullOrEmpty(tirDumpDir) ? null : tirDumpDir };
            if (inlineReport)
                writer.InlineReport = new InlineReport();
            writer.WriteAll("float");
            if (writer.InlineReport != null)
                Console.Error.WriteLine(writer.InlineReport.ToTable());

            // Output documentation
            var docWriter = new DocWriter(compilation);
            var fp = outputFolder.RelativeFile("docs.html");
            fp.WriteAllText(docWriter.ToString());

            //Analyze(compilation, outputFolder);

            return writer;
        }

        public static TotalAnalysis Analyze(this Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            return new TotalAnalysis(compilation, outputFolder);
        }
    }
}
