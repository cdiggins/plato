// The current parameter policy: assigned parameters stay owned; records borrow when --cpp-borrow-params is on.
namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// Assignment targets remain owned values. Only closed, named records are wrapped in
    /// <c>ReadOnlyParameter</c> (alias templates in deduced parameter positions would prevent
    /// generic argument deduction in C++); the size threshold lives in the prelude's wrapper.
    /// </summary>
    public sealed class BorrowRecordParameters : IParameterPolicy
    {
        private readonly bool _borrowRecords;

        public BorrowRecordParameters(bool borrowRecords) => _borrowRecords = borrowRecords;

        public ParameterPassing Classify(string cppType, bool assigned, bool isRecord)
        {
            if (assigned)
                return ParameterPassing.Owned;
            if (_borrowRecords && isRecord)
                return ParameterPassing.ReadOnly;
            return ParameterPassing.Standard;
        }
    }
}
