// The constant-cache policies: function-local statics, or nothing.
using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>Caches closed expressions whose result holds no arena memory (see <c>CppWriter.HoldsNoArenaMemory</c>) in function-local statics.</summary>
    public sealed class FunctionLocalStaticConstants : IConstantCachePolicy
    {
        public bool Enabled => true;

        private const int Depth = 3;

        public bool Worthwhile(TirNode expression, Func<FunctionDef, TirFunction> bodyOf)
            => expression.Descendants().Append(expression).OfType<TirCall>().Any(c => Iterates(c.Callee, bodyOf, Depth, new HashSet<FunctionDef>()));

        // A function iterates when its body has a loop or a lambda (a higher-order call), or it calls one that does.
        private static bool Iterates(FunctionDef f, Func<FunctionDef, TirFunction> bodyOf, int depth, HashSet<FunctionDef> seen)
        {
            if (f == null || depth == 0 || !seen.Add(f))
                return false;
            var body = bodyOf(f)?.Body;
            if (body == null)
                return false;
            var nodes = body.Descendants().Append(body).ToList();
            return nodes.Any(n => n is TirLoop or TirLambda)
                || nodes.OfType<TirCall>().Any(c => Iterates(c.Callee, bodyOf, depth - 1, seen));
        }

        public (string Open, string Close) WholeBody(string cppType)
            => ($" {{ static const {cppType} _c = ", "; return _c; }");

        public (string Open, string Close) Site(string cppType)
            => ($"([]() -> const {cppType}& {{ static const {cppType} _c = ", "; return _c; }())");
    }

    /// <summary>The output before --cache-constants: nothing is cached.</summary>
    public sealed class NoConstantCaching : IConstantCachePolicy
    {
        public bool Enabled => false;
        public bool Worthwhile(TirNode expression, Func<FunctionDef, TirFunction> bodyOf) => false;
        public (string Open, string Close) WholeBody(string cppType) => (null, null);
        public (string Open, string Close) Site(string cppType) => (null, null);
    }
}
