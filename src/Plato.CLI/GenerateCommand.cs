// Validates generation arguments and invokes the selected writer.
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Logging;
using Ara3D.Utils;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.TypeScriptWriter;
using Ara3D.Geometry.RustWriter;
using Ara3D.Geometry.GlslWriter;
using Ara3D.Geometry.CppWriter;

namespace Ara3D.Geometry.CLI;

internal static class GenerateCommand
{
    public static int Run(string[] args, ILogger logger)
    {
        var typeScript = args.Contains("--typescript");
        var rust = args.Contains("--rust");
        var glsl = args.Contains("--glsl");
        var cpp = args.Contains("--cpp");
        var cuda = args.Contains("--cuda");

        var optimize = args.Contains("--optimize");

        var optimizeArrays = args.Contains("--optimize-arrays");

        var inline = args.Contains("--inline");
        var inlinePolicy = args.LastOrDefault(a => a.StartsWith("--inline-policy="))?["--inline-policy=".Length..];
        var inlineGrowthOption = args.LastOrDefault(a => a.StartsWith("--inline-growth="));
        var inlineGrowth = inlineGrowthOption?["--inline-growth=".Length..] ?? "4096";
        if (inlinePolicy is not (null or "statements" or "lambdas" or "iterative")
            || !int.TryParse(inlineGrowth, out var growth) || growth <= 0
            || inlinePolicy == null && inlineGrowthOption != null
            || inlinePolicy != null && (typeScript || rust || glsl || cpp || cuda))
        {
            Console.Error.WriteLine("Experimental C# inlining requires --inline-policy=statements|lambdas|iterative and a positive --inline-growth value.");
            return 1;
        }
        var statementInlining = inlinePolicy == null ? null
            : new StatementInliningOptions(inlinePolicy != "statements", inlinePolicy == "iterative", growth);

        var staticAbstract = args.Contains("--static-abstract");

        var unknownNegation = WriterSwitches.UnknownNegation(args);
        if (unknownNegation != null)
        {
            Console.Error.WriteLine($"Unrecognized generation option '{unknownNegation}'.");
            return 1;
        }
        var profileArg = args.Where(a => a.StartsWith("--profile="))
            .Select(a => a.Substring("--profile=".Length)).LastOrDefault();
        // --cpp --profile=cpu64i32 selects the Cpu dialect (TKT-1097); it implies --inline.
        var cpu = cpp && !cuda && profileArg == "cpu64i32";
        var target = cpu ? WriterTarget.Cpu : typeScript || rust || glsl || cpp || cuda ? WriterTarget.Other : WriterTarget.CSharp;
        // The switch table (WriterSwitches) decides defaults, the targets each switch applies to and its dependency.
        var resolved = WriterSwitches.Resolve(args, target);
        var switchError = WriterSwitches.Check(resolved, target);
        if (switchError != null)
        {
            Console.Error.WriteLine(switchError);
            return 1;
        }
        var loops = resolved["loops"];
        var loopOptimizations = WriterSwitches.ToLoopOptions(resolved);
        if (loopOptimizations.SpanParams && statementInlining != null)
        {
            Console.Error.WriteLine("--span-params cannot be combined with --inline-policy.");
            return 1;
        }
        if (cpu)
            inline = true;

        var fuse = resolved["fuse"];

        var tirDumpDir = args.Where(a => a.StartsWith("--dump-tir="))
            .Select(a => a.Substring("--dump-tir=".Length)).LastOrDefault();

        var inlineReport = args.Contains("--inline-report");

        var csharpStyle = args.Where(a => a.StartsWith("--csharp-style="))
            .Select(a => a.Substring("--csharp-style=".Length)).LastOrDefault() ?? "extensions";
        if (csharpStyle != "extensions")
        {
            Console.Error.WriteLine($"Unsupported --csharp-style value '{csharpStyle}' (only 'extensions' is supported; the default style was retired in C4)");
            return 1;
        }

        var scalarFlag = args.FirstOrDefault(a => a.StartsWith("--scalar="));
        if (scalarFlag != null)
        {
            Console.Error.WriteLine($"'{scalarFlag}' is no longer supported: scalar erasure was retired 2026-08-01 and wrapper scalars are the only representation. Drop the flag.");
            return 1;
        }

        var unknownOption = GenerationArguments.UnknownOption(args);
        if (unknownOption != null)
        {
            Console.Error.WriteLine($"Unrecognized generation option '{unknownOption}'.");
            return 1;
        }

        // Generated code must be built with the same PlatoNumberProfile.
        var profileName = args.Where(a => a.StartsWith("--profile="))
            .Select(a => a.Substring("--profile=".Length)).LastOrDefault();
        var profile = profileName == null ? NumberProfile.Default : NumberProfile.TryParse(profileName);
        if (profile == null)
        {
            Console.Error.WriteLine($"Unknown --profile value '{profileName}' (expected one of: {NumberProfile.Names})");
            return 1;
        }
        if (profileName != null && !cpu && (typeScript || rust || glsl || cpp || cuda))
        {
            Console.Error.WriteLine("--profile= is implemented by the C# writer only; the other writers still use their fixed scalar types.");
            return 1;
        }

        // With --out, every positional folder is an input root.
        var outFlag = args.Where(a => a.StartsWith("--out="))
            .Select(a => a.Substring("--out=".Length)).LastOrDefault();

        var folders = args.Where(a => !a.StartsWith("--")).ToList();
        if (outFlag == null && folders.Count > 2)
        {
            Console.Error.WriteLine(
                "Ambiguous arguments: without --out=<outputFolder> at most two positional folders " +
                "are accepted (<inputFolder> [outputFolder]). Pass --out= to use multiple input roots.");
            return 1;
        }

        var inputFolders = outFlag != null
            ? (folders.Count > 0 ? folders : new List<string> { Config.InputFolder })
            : new List<string> { folders.Count > 0 ? folders[0] : Config.InputFolder };
        var outputFolder = new DirectoryPath(outFlag ?? (folders.Count > 1 ? folders[1] : Config.OutputFolder));

        logger.Log("Opening files");
        var (files, inputError) = SourceFiles.Discover(inputFolders);
        if (inputError != null)
        {
            Console.Error.WriteLine(inputError);
            return 1;
        }
        var docs = DocumentLoader.ParseAll(files, logger);
        var parsingSuccessful = docs.All(e => e.Parser.Succeeded);
        if (!parsingSuccessful)
        {
            Console.Error.WriteLine("Parsing failed for one of the input files, halting");
            return 1;
        }
        logger.Log("Parsing succeeded for all files");

        logger.Log("Compiling");
        var trees = docs.Select(e => e.Ast);
        var compilation = new Compilation(logger, trees);
        if (!compilation.CompletedCompilation)
        {
            Console.Error.WriteLine("Compilation was not completed");
            return 1;
        }

        if (typeScript)
        {
            logger.Log("Writing TypeScript Files");
            var output = compilation.ToTypeScript(outputFolder, true);
            GeneratedFiles.Write(output.Files, outputFolder, logger);
        }
        else if (glsl)
        {
            logger.Log("Writing GLSL Files");
            var output = compilation.ToGlsl(outputFolder);
            logger.Log($"GLSL functions emitted: {output.FunctionsEmitted}, skipped: {output.Skipped.Count}");
            GeneratedFiles.Write(output.Files, outputFolder, logger);
        }
        else if (cpp || cuda)
        {
            var dialect = cuda ? CppDialect.Cuda : cpu ? CppDialect.Cpu : CppDialect.Cpp;
            logger.Log($"Writing {dialect.DisplayName()} Files");
            var cpuOptions = cpu ? CppCpuOptions.From(resolved.Switches) : null;
            var output = compilation.ToCpp(outputFolder, dialect, inline, cpuOptions);
            logger.Log($"{dialect.DisplayName()} functions emitted: {output.FunctionsEmitted}, skipped: {output.Skipped.Count}");
            GeneratedFiles.Write(output.Files, outputFolder, logger);
            if (cpu)
            {
                var unreachable = output.UnemittedReachableFromRoots();
                if (unreachable.Count > 0)
                {
                    Console.Error.WriteLine($"{unreachable.Count} function(s) reachable from --cpp-roots were not emitted:");
                    foreach (var line in unreachable)
                        Console.Error.WriteLine("  " + line);
                    return 1;
                }
            }
        }
        else if (rust)
        {
            logger.Log("Writing Rust Files");
            var output = compilation.ToRust(outputFolder, true);
            GeneratedFiles.Write(output.Files, outputFolder, logger);
        }
        else
        {
            logger.Log("Writing C# Files");
            var output = compilation.ToCSharp(outputFolder, csharpStyle == "extensions", optimize, optimizeArrays, inline, loops, tirDumpDir, inlineReport, staticAbstract, profile, fuse, statementInlining, loopOptimizations);
            logger.Log($"TIR bodies emitted: {output.TirBodiesEmitted}");
            if (loopOptimizations.Elaboration.Any || loopOptimizations.ShareCallsWide)
            {
                var elaboration = output.ElaborationStats;
                var rewrites = output.RewriteStats;
                logger.Log($"Decoder-wave rewrites: scrutinees bound {elaboration.BoundScrutinees}, bindings sunk {elaboration.SunkBindings} at elaboration and {rewrites.SunkAfterInlining} after inlining, calls shared across statements {rewrites.SharedAcrossStatements}, in lambda bodies {rewrites.SharedInLambdas}");
            }
            if (loopOptimizations.HoistInvariants)
                logger.Log($"Loop invariants hoisted: {output.HoistedInvariants} in {output.HoistedLoops} loops ({output.HoistExposingInlines} forwarding calls expanded)");
            if (loopOptimizations.SpanParams)
                logger.Log($"Span twins: {output.Borrowed.TwinsWritten} written borrowing {output.Borrowed.ParametersBorrowed} parameters; refused: {output.Borrowed.Refusals.Count}");
            if (loopOptimizations.SpecializeCallbacks || loopOptimizations.ScalarArrays || loopOptimizations.OwnedArrays)
            {
                var s = output.Specializations;
                logger.Log($"Callbacks specialized: {s.SitesSpecialized} sites into {s.Clones.Count} clones; refused: {string.Join(", ", s.Refusals.Select(r => $"{r.Key} {r.Value}"))}");
            }
            if (loopOptimizations.OwnedArrays)
            {
                // TKT-1152: the storage layer's counts; a checker violation prints that body with List storage.
                logger.Log($"Storage: {output.StorageBodiesChecked} bodies checked, checker refusals {output.StorageRefusals}, stack slots {output.StackSlotsPlaced}, twin calls routed {output.TwinCallsRouted}; stack refused: {string.Join(", ", output.StackRefusals.Select(r => $"{r.Key} {r.Value}"))}");
                foreach (var refusal in output.StorageRefusalLog)
                    logger.Log($"  storage refused: {refusal}");
            }
            if (output.DegradedBodies.Count > 0)
            {
                logger.Log($"DEGRADED bodies (no ground TIR, emitted as throwing stubs): {output.DegradedBodies.Count}");
                foreach (var m in output.DegradedBodies)
                    logger.Log($"  degraded: {m}");
            }
            GeneratedFiles.Write(output.Files, outputFolder, logger);
        }

        logger.Log("Completed");
        return 0;
    }
}
