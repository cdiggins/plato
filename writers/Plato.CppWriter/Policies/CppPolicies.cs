// The set of code-shape decisions the C++ writer consults; the default set is today's behaviour.
namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// One implementation of each decision. A new idea is a new implementation of one interface,
    /// selected by replacing that property; <see cref="For"/> builds the default set for the options.
    /// </summary>
    public sealed class CppPolicies
    {
        public IParameterPolicy Parameters { get; init; }
        public IReadBorrowPolicy Reads { get; init; }
        public ILambdaPolicy Lambdas { get; init; }
        public ISumLayout Sums { get; init; }
        public IStringLiteralPolicy Strings { get; init; }
        public IUnitPartitioner Units { get; init; }
        public IFunctionAttributes Attributes { get; init; }
        public LoweredLoopEmitters Loops { get; init; }

        public static CppPolicies For(CppCpuOptions options) => new CppPolicies
        {
            Parameters = new BorrowRecordParameters(options.BorrowLargeParameters),
            Reads = options.BorrowReads ? new ConstReferenceReads() : new CopyingReads(),
            Lambdas = new CaptureByValueLambdas(),
            Sums = options.CompactSums ? new CompactUnionSumLayout(options.SwitchSumDispatch)
                : options.SwitchSumDispatch ? new SwitchKindSumLayout() : new FlattenedKindSumLayout(),
            Strings = new MakeStringLiterals(),
            Units = new LineLimitPartitioner(),
            Attributes = new NoFunctionAttributes(),
            Loops = BuiltInLoopEmitters.Create(),
        };
    }
}
