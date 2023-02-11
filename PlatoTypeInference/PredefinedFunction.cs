using System.Collections.Generic;
using PlatoTypeInference;


namespace PlatoAbstractSyntax
{
    public class AbstractSyntax { }
    public class FunctionDeclaration : AbstractSyntax { }
    public class Expression : AbstractSyntax { }
    public class Statement : Expression { }
    public class FunctionCall : Expression { }
    public class Identifier : Expression { }
    public class VariableDeclaration : Statement { }
    public class Assignment : Statement { }
    public class TypeExpr : Expression { }

    /*
     * Every statement is either a:
     * - Variable declaration 
     * - Assignment
     * - Compound statement
     * - Expression 
     * - Conditional
     *
     * Every expression is either:
     * - function invocation
     * - type expression
     *
     * Desirable operations:
     * - check for equivalence
     * - 
     */

    public class PredefinedFunction : AbstractSyntax
    {
        public string Name { get; }
        public IReadOnlyList<TypeParameter> TypeParameters { get; }
        public IReadOnlyList<Parameter> Parameters { get; }
        public TypeExpr ReturnType { get; }
        public AbstractSyntax Body { get; }

        // Many expressions can be converted into a function call.
        // A function call 
        public static IEnumerable<PredefinedFunction> PredefinedFunctions()
        {
            // Numeric operators (binary, unary)
            // Boolean operators (binary, unary)
            // Overloaded operators
            // Literals (string, number, boolean, null)
            // Bitwise operators 
            // new operators 
            // Methods
            // typeof
            // nameof
            // Indexers 
            // Abstract function calls
            // Property accessors
            // Closures 
            // Conditionals 
            // Null coalescing operator 
            // Loops: ?? (while loop, etc. can all be expressed in a function form, should I?) 
            // Compound assignment
            // throw 
            // using? 
            // Try-catch-finally? 
            // Tuple
            // string interpolation
            // indexing 
            yield break;
        }

        // TODO: why convert into an abstract syntax / intermediate representation? 
        // Simplify analysis, type inference, optimization, and code generation.

        
    }
}