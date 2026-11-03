// Attributes of an emitted function (a hook for purity or calling-convention annotations).
using System.Collections.Generic;

namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>Supplies the text written between <c>PLATO_FN</c> and the return type of a user function.</summary>
    public interface IFunctionAttributes
    {
        /// <summary>The attributes, each followed by a space, or an empty string for none.</summary>
        string For(string name, IReadOnlyList<string> parameterTypes, string returnType);
    }

    /// <summary>The current behaviour: no attributes.</summary>
    public sealed class NoFunctionAttributes : IFunctionAttributes
    {
        public string For(string name, IReadOnlyList<string> parameterTypes, string returnType) => "";
    }
}
