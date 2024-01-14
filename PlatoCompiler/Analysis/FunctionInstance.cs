using Plato.Compiler.Symbols;

namespace Plato.Compiler.Analysis
{
    /// <summary>
    /// Represents an instance of a function. Used for analyzing library functions and generating code.
    /// </summary>
    public class FunctionInstance
    {
        public TypeDefinition ConcreteType { get; }
        public FunctionDefinition Implementation { get; }
        public TypeSubstitutions Substitutions { get; }

        public FunctionInstance(
            TypeDefinition concreteType,
            FunctionDefinition implementation,
            TypeSubstitutions substitutions)
        {
            ConcreteType = concreteType;
            Implementation = implementation;
            Substitutions = substitutions;
        }
    }
}