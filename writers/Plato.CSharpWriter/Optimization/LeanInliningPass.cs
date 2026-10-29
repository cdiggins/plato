// Keeps [MethodImpl(AggressiveInlining)] only on one-line forwarding members of the emitted C# (--lean-inlining).
using System.Text;
using System.Text.RegularExpressions;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// TKT-1091. The writer marks every generated member AggressiveInlining. Forcing all of them inline fills
/// the JIT's per-method local budget, after which it refuses to inline even the small Number, Integer and
/// Boolean wrappers ("too many locals"). Under <see cref="CSharpWriter.LeanInlining"/> the attribute stays
/// only on a one-line member whose whole body is a forwarding expression: a member access or call chain
/// whose arguments are plain names (<c>a.Name(b, c)</c>, <c>x.MagnitudeSquared().LessThanOrEquals(t)</c>,
/// <c>new T(a, b)</c>, <c>self.Field</c>), or one binary operator over two plain names (<c>this * b</c>).
/// Everything else, statement bodies included, is left to the JIT's own heuristics. The handwritten
/// intrinsics are not written by the writer and keep their attributes.
/// The rule reads the emitted C#, because the JIT's decision depends on the shape of that text and every
/// emission site (TIR bodies, synthesized operators, conversions, field accessors) reaches it there. It
/// only ever removes the attribute, so the program's meaning cannot change.
/// </summary>
public static class LeanInliningPass
{
    public const string Attribute = "[MethodImpl(AggressiveInlining)]";

    private const string Name = @"[A-Za-z_][\w.]*";
    private const string Args = @"\((?:\s*" + Name + @"\s*(?:,\s*" + Name + @"\s*)*)?\)";
    private static readonly Regex Forwarding = new(
        @"^(?:new\s+)?" + Name + @"(?:<[\w, .]+>)?(?:" + Args + @")?(?:\." + Name + @"(?:" + Args + @")?)*;\s*(?:}\s*)?$"
        + @"|^" + Name + @"\s*[-+*/%<>=!&|^]{1,2}\s*" + Name + @";\s*(?:}\s*)?$",
        RegexOptions.CultureInvariant);

    /// <summary>True when <paramref name="body"/>, the text after a member's <c>=&gt;</c> to the end of its
    /// line, is a forwarding expression.</summary>
    public static bool IsForwarding(string body) => Forwarding.IsMatch(body.Trim());

    /// <summary>Removes the attribute from every line of <paramref name="text"/> whose member is not a
    /// one-line forwarding member. Line ends are kept as they are.</summary>
    public static string Apply(string text)
    {
        if (!text.Contains(Attribute))
            return text;
        var lines = text.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var at = line.IndexOf(Attribute, System.StringComparison.Ordinal);
            if (at < 0)
                continue;
            var rest = line.Substring(at + Attribute.Length);
            var arrow = rest.IndexOf("=>", System.StringComparison.Ordinal);
            if (arrow >= 0 && IsForwarding(rest.Substring(arrow + 2)))
                continue;
            lines[i] = line.Substring(0, at) + (rest.StartsWith(" ") ? rest.Substring(1) : rest);
        }
        return string.Join("\n", lines);
    }

    /// <summary>Applies <see cref="Apply(string)"/> to every file the writer produced.</summary>
    public static void Apply(System.Collections.Generic.Dictionary<string, StringBuilder> files)
    {
        foreach (var name in new System.Collections.Generic.List<string>(files.Keys))
        {
            var text = files[name].ToString();
            var lean = Apply(text);
            if (!ReferenceEquals(lean, text))
                files[name] = new StringBuilder(lean);
        }
    }
}
