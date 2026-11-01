// Writes the span twins of --span-params into BorrowedTwins.g.cs (TKT-1152 chunk C8a).
using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>A body whose original was written, and so may have a twin: the library class it went into, the function and
/// the concrete type it was emitted at (null for a static library function), its signature record, and a factory for the
/// type writer that carries the extension re-qualification context the original body was written under.</summary>
internal sealed record TwinSite(string ClassName, FunctionDef Function, TypeDef Type, CSharpFunctionInfo Info,
    Func<CSharpTypeWriter> NewWriter);

/// <summary>
/// Writes <c>BorrowedTwins.g.cs</c>, last, from the memoised optimised TIR of each recorded site whose
/// <see cref="BorrowedParameters"/> mask is not empty. A twin's body is the original's, with each borrowed parameter
/// planned <see cref="ArrayStorage.Borrowed"/> so the body writer reads it as <c>x[i]</c> and <c>x.Length</c>; the plan is
/// verified by <see cref="StorageChecker"/> like every other, and a twin whose plan is refused is not written. Nothing
/// calls a twin yet (chunk C8b routes calls), and the originals are untouched.
/// </summary>
internal static class BorrowedTwinWriter
{
    public static void WriteFile(CSharpWriter writer)
        => writer.WriteFile("BorrowedTwins.g.cs", () => WriteClass(writer));

    private static CSharpWriter WriteClass(CSharpWriter writer)
    {
        writer.WriteLine($"public static class {BorrowedParameters.TwinClass}");
        writer.WriteStartBlock();
        var signatures = new HashSet<string>();
        foreach (var site in writer.TwinSites.ToList())
        {
            var mask = writer.Borrowed.Of(site.Function, site.Type, site.Info);
            if (!BorrowedParameters.Any(mask))
                continue;
            var name = BorrowedParameters.TwinName(site.ClassName, site.Info.Name);
            var signature = site.Info.TwinSignature(name, mask);
            if (signature == null)
            {
                writer.Borrowed.Refuse($"{site.ClassName}.{site.Info.Name}: a borrowed parameter is not an IReadOnlyList");
                continue;
            }
            // One twin per C# signature, as the originals are.
            if (!signatures.Add($"{name}({string.Join(",", site.Info.TwinParameterTypes(mask))})"))
                continue;
            var tir = writer.OptimizedTirForEmission(site.Function, site.Type, site.Info);
            if (tir == null)
                continue;

            var tw = site.NewWriter();
            tw.Write(signature);
            if (!TryWriteBody(writer, tw, tir, site, mask))
                continue;
            writer.WriteWithLineStateSync(tw.ToString());
            writer.Borrowed.CountWritten(mask);
        }
        return writer.WriteEndBlock();
    }

    // The body writer resolves storage at its entry through writer.StorageResolver; for the one body, the resolver is
    // the usual one with the borrowed parameters added to its plan.
    private static bool TryWriteBody(CSharpWriter writer, CSharpTypeWriter tw, TirFunction tir, TwinSite site, bool[] mask)
    {
        var usual = writer.StorageResolver;
        var refusals = writer.StorageRefusals;
        writer.StorageResolver = (body, owner) =>
        {
            var scope = (usual ?? writer.DefaultStorageResolver)(body, owner);
            BorrowedParameters.Plan(scope.Plan, scope.Body, tir.Parameters.Where((_, i) => mask[i]));
            return scope;
        };
        try
        {
            tw.Write(new TirCSharpBodyWriter(tw, tir, isStatic: true, site.Info).ToString());
        }
        finally
        {
            writer.StorageResolver = usual;
        }
        if (writer.StorageRefusals == refusals)
            return true;
        // The checker refused the plan and the body printed with List parameters, which no span satisfies.
        writer.Borrowed.Refuse($"{site.ClassName}.{site.Info.Name}: {writer.StorageRefusalLog[^1]}");
        return false;
    }
}
