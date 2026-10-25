using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

public static partial class TirInliner
{
    private sealed partial class StatementExpansion
    {
        private readonly bool _simplify;
        private readonly Dictionary<VariableDef, TirNode> _values = new();

        private bool Discardable(VariableDef def, TirNode value)
            => _simplify && Equals(def.Type, value.Type) && value is TirLiteral or TirParameter;

        private TirNode RewriteRegion(TirInlineRegion region)
        {
            var outer = _ancestry.ToArray();
            if (region.Ancestry != null)
            {
                _ancestry.Clear();
                _ancestry.UnionWith(region.Ancestry);
            }
            var body = Rewrite(region.Body);
            _ancestry.Clear();
            _ancestry.UnionWith(outer);
            return new TirInlineRegion(region.Result, body, region.Ancestry);
        }

        private void RememberValue(VariableDef def, TirNode value)
        {
            if (!_simplify || _assigned.Contains(def))
                return;
            value = Simplify(value);
            if (value is TirLiteral && Equals(value.Type, def.Type)
                || value is TirParameter p && !_assigned.Contains(p.Def) && Equals(value.Type, def.Type))
                _values[def] = value;
        }

        private TirNode Simplify(TirNode node)
        {
            var result = SimplifyCore(node);
            if (!ReferenceEquals(result, node)) Version++;
            return result;
        }

        private TirNode SimplifyCore(TirNode node)
        {
            if (!_simplify)
                return node;
            if (node is TirVariable v && _values.TryGetValue(v.Def, out var value))
                return value;
            if (node is TirConditional conditional && BooleanValue(conditional.Condition) is { } selected)
                return Simplify(selected ? conditional.IfTrue : conditional.IfFalse);
            if (node is not TirCall call)
                return node;
            if (call.Callee?.Body != null
                || call.Callee?.OwnerType?.Name != "Intrinsics" || call.Args.Count != 2
                || call.Callee.Parameters.Any(p => !ReferenceEquals(p.Type.Def, _host.Compilation.GetTypeDef("Integer"))))
                return node;
            var left = IntegerValue(call.Args[0]);
            var right = IntegerValue(call.Args[1]);
            if (left == null || right == null)
                return node;
            var a = left.Value;
            var b = right.Value;
            bool? boolean = call.Name switch
            {
                "Equals" => a == b, "NotEquals" => a != b, "LessThan" => a < b,
                "GreaterThan" => a > b, "LessThanOrEquals" => a <= b, "GreaterThanOrEquals" => a >= b,
                _ => null
            };
            if (boolean != null && call.Type?.Name == "Boolean")
                return new TirLiteral(boolean.Value, LiteralTypesEnum.Boolean, call.Type, call.Origin);
            long? integer = call.Name switch
            {
                "Add" => (long)a + b, "Subtract" => (long)a - b, "Multiply" => (long)a * b,
                _ => null
            };
            return integer is >= int.MinValue and <= int.MaxValue && call.Type?.Name == "Integer"
                ? new TirLiteral((int)integer.Value, LiteralTypesEnum.Integer, call.Type, call.Origin) : node;
        }

        private int? IntegerValue(TirNode node)
        {
            node = Simplify(node);
            return node is TirLiteral l && l.Type?.Name == "Integer"
                && int.TryParse(l.Value?.ToString(), out var value) ? value : null;
        }

        private bool? BooleanValue(TirNode node)
        {
            node = node is TirVariable v && _values.TryGetValue(v.Def, out var value) ? value : node;
            return node is TirLiteral l && l.Type?.Name == "Boolean"
                && bool.TryParse(l.Value?.ToString(), out var result) ? result : null;
        }
    }
}
