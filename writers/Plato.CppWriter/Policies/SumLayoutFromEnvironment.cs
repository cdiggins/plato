// A stand-in for the CLI rows of the sum layouts, until the switch table has them.
using System;
using System.Linq;

namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// <c>PLATO_CPP_SUMS</c>, a comma-separated list read when an options record is created: <c>switch</c> sets
    /// <see cref="CppCpuOptions.SwitchSumDispatch"/>, <c>compact</c> sets <see cref="CppCpuOptions.CompactSums"/>. It lets a full
    /// library generation select the layouts without a CLI flag; an explicit value in an object initializer wins. Unset (the
    /// default) it changes nothing. Delete it when P0.4's switch table carries <c>--cpp-sum-switch</c> and <c>--cpp-sum-compact</c>.
    /// </summary>
    internal static class SumLayoutFromEnvironment
    {
        public static bool Has(string name)
            => (Environment.GetEnvironmentVariable("PLATO_CPP_SUMS") ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Contains(name);
    }
}
