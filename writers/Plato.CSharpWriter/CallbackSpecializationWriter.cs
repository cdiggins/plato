// Writes the clones the callback specializer registered as one static class (--specialize-callbacks).
using System.Collections.Generic;
using System.Linq;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// Emits <c>CallbackSpecializations.g.cs</c>: every <see cref="CallbackClone"/> as a static method
/// <c>[MethodImpl(AggressiveInlining)] public static R Callee_hash(T0 p0, ..., Ck _capk, ...)</c>, rendered in
/// the callee's own extension context (the receiver type's plan), in static mode, through the same optimizer
/// passes as any body. A clone's passes may register further clones, which this loop reaches, so the file is
/// written after every other body and left out entirely when no site was specialized.
/// </summary>
public static class CallbackSpecializationWriter
{
    public static void WriteFile(CSharpWriter writer)
    {
        var clones = writer.Specializations.Clones;
        if (clones.Count == 0)
            return;
        writer.WriteFile(CallbackSpecializations.FileName, () =>
        {
            writer.WriteLine($"public static class {CallbackSpecializations.ClassName}");
            writer.WriteStartBlock();
            for (var i = 0; i < clones.Count; i++)
                WriteClone(writer, clones[i]);
            return writer.WriteEndBlock();
        });
    }

    private static void WriteClone(CSharpWriter writer, CallbackClone clone)
    {
        // A moved member renders in its receiver type's extension context; a static library function
        // (Array, Function1 receivers) in the default context of Extensions.g.cs.
        var tw = clone.Plan == null
            ? writer.NewDefaultTypeWriter()
            : new CSharpTypeWriter(writer, clone.ReceiverType)
            {
                ExtensionReceiverName = clone.Callee.GetParameterName(0),
                ExtensionStaticQualifier = $"{writer.Namespace}.{clone.ReceiverType.Name}",
                ExtensionInstanceNames = clone.Plan.InstanceNames,
                ExtensionStaticNames = clone.Plan.StaticNames,
            };
        writer.SpecializationDepth = clone.Depth;
        var tir = writer.RunOptimizerPasses(clone.Tir, clone.Function);
        writer.SpecializationDepth = 0;
        var modifiers = writer.LargeStructs?.ParameterModifiers(clone.ParameterTypes, tir);
        var parameters = new List<string>();
        for (var i = 0; i < clone.ParameterTypes.Count; i++)
            parameters.Add($"{(modifiers != null ? modifiers[i] : "")}{clone.ParameterTypes[i]} {CSharpTypeWriter.Identifier(tir.Parameters[i].Name)}");
        tw.Write($"// {clone.Callee.OwnerType?.Name}.{clone.Callee.Name} on {clone.ReceiverTypeName}, depth {clone.Depth}");
        tw.WriteLine();
        tw.Write($"{CSharpFunctionInfo.Annotation}public static {clone.ReturnType} {clone.Name}({string.Join(", ", parameters)})");
        tw.Write(new TirCSharpBodyWriter(tw, tir, isStatic: true, clone.Function).ToString());
        writer.WriteWithLineStateSync(tw.ToString());
    }
}
