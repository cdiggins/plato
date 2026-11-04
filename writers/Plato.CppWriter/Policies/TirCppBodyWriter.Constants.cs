// --cache-constants for the Cpu dialect (TKT-1183): closed expressions are evaluated once, in function-local statics.
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CppWriter.Policies;

namespace Ara3D.Geometry.CppWriter
{
    public partial class TirCppBodyWriter
    {
        private int _constantDepth;

        private IConstantCachePolicy Constants => _w.Policies.Constants;

        /// <summary>
        /// A function with no parameters (or only the ignored type-level receiver <c>_</c>) whose body is a closed
        /// expression that calls into the program becomes <c>{ static const T _c = expr; return _c; }</c>. Refused: a Law,
        /// a result type that is not a function value and holds arena memory (an Array, List, Buffer, Bytes, generic
        /// instance), and a bare literal or arithmetic on literals (the compiler folds those).
        /// </summary>
        private bool TryWriteCachedConstantBody(TirNode body)
        {
            if (!UsesCpuLambdas || !Constants.Enabled || _lambdaReturns.Count > 0 || _constantDepth > 0)
                return false;
            var original = _tir.Original;
            if (original == null || original.NumParameters > 1 || original.NumParameters == 1 && original.GetParameterName(0) != "_")
                return false;
            if (original.Name.StartsWith("Law_") || !IsClosedConstant(body) || !HasCallIntoProgram(body)
                || !Constants.Worthwhile(body, _w.TryGetStaticTir))
                return false;
            var type = _tir.ZonkedReturnType;
            var cpp = _retCpp ?? _w.CppTypeNameOrNull(type);
            if (type == null || cpp == null || cpp.Contains("Func<"))
                return false;
            if (!_w.HoldsNoArenaMemory(type))
            {
                C1bCounts.Bump(ref C1bCounts.RefusedArenaType);
                return false;
            }
            var (open, close) = Constants.WholeBody(cpp);
            Write(open);
            _constantDepth++;
            WriteReturnValue(body);
            _constantDepth--;
            WriteLine(close);
            C1bCounts.Bump(ref C1bCounts.ConstantBodies);
            return true;
        }

        /// <summary>
        /// A call whose arguments are all closed has the same value wherever it stands (a loop body, a lambda, a function
        /// called per row). The largest such call is written once as a static and read from a lambda that returns it.
        /// Refused as in the body form, and for a bare nullary call (the callee caches its own body).
        /// </summary>
        private bool TryWriteCachedClosedCall(TirCall call)
        {
            if (!UsesCpuLambdas || !Constants.Enabled || _constantDepth > 0 || call.Type == null || call.Args.Count == 0)
                return false;
            if (_tir.Original?.Name.StartsWith("Law_") == true)
                return false;
            if (!IsClosedConstant(call) || !call.Descendants().Any(d => d is TirCall { Callee.Body: not null })
                || !Constants.Worthwhile(call, _w.TryGetStaticTir))
                return false;
            var cpp = _w.CppTypeNameOrNull(call.Type);
            if (cpp == null || cpp.Contains("Func<") || cpp.Contains('$'))
                return false;
            if (!_w.HoldsNoArenaMemory(call.Type))
            {
                C1bCounts.Bump(ref C1bCounts.RefusedArenaType);
                return false;
            }
            var (open, close) = Constants.Site(cpp);
            Write(open);
            _constantDepth++;
            WriteNode(call);
            _constantDepth--;
            Write(close);
            C1bCounts.Bump(ref C1bCounts.ConstantSites);
            return true;
        }

        private static bool HasCallIntoProgram(TirNode body) => body.Descendants().Any(n => n is TirCall { Callee.Body: not null });

        // Only value-forming expression nodes: anything that binds or reads a name, builds a closure or an array, or is a
        // statement disqualifies the expression.
        private static bool IsClosedConstant(TirNode n)
            => n switch
            {
                null => true,
                TirLiteral or TirTypeRef or TirName or TirDefault => true,
                TirCoerce or TirCall or TirConditional or TirNew
                    or TirComponentAccess or TirConstructorCall or TirBooleanChain => n.Children.All(IsClosedConstant),
                _ => false,
            };
    }
}
