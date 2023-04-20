using System;
using Parakeet;

namespace PlatoIntrinsics
{
    public class Generator
    {
        public static (string, string)[] UnaryMathOperators = new[]
        {
            ("-", "Negate")
        };

        public static (string, string)[] BinaryMathOperators = new[]
        {
            ("+", "Add"),
            ("-", "Subtract"),
            ("*", "Multiply"),
            ("/", "Divide"),
            ("%", "Modulo"),
        };

        public static (string, string)[] ComparisonOperators = new[]
        {
            ("<", "LessThan"),
            (">", "GreaterThan"),
            ("<=", "LessThanOrEquals"),
            (">=", "GreaterThanOrEquals"),
        };

        public static  (string, string)[] BinaryBooleanOperators = new[]
        {
            ("&&", "And"),
            ("||", "Or"),
            ("^", "XOr"),
        };

        public static (string, string)[] UnaryBooleanOperators = new[]
        {
            ("!", "Not")
        };

        public static (string, string)[] EqualityOperators = new[]
        {
            ("==", "Equals"),
            ("!=", "NotEquals"),
        };

        // C# (System.Math.), JavaScript (math.), C++ (include <cmath>, std::)
        // https://www.w3schools.com/cpp/cpp_math.asp
        // https://www.w3schools.com/js/js_math.asp
        // https://learn.microsoft.com/en-us/dotnet/api/system.math?view=net-8.0

        public static (string, string)[] UnaryMathFunctions = new[]
        {
            ("Abs", "math.abs"),
            ("Sqrt", "math.sqrt"),
        };

        public static (string, string)[] BinaryMathFunctions = new[]
        {
            ("Min", "math.min"),
            ("Max", "math.max"),
            ("Pow", "math.pow"),
        };

        // TODO: assign to a field. 

        public static void OutputJavascriptFunc(CodeBuilder cb, int argCount, string impl, string name, bool isOp)
        {
            cb.Write("function ").Write(name).Write("(");

            for (var i = 0; i < argCount; ++i)
            {
                if (i > 0)
                {
                    cb.Write(", ");
                }

                cb.Write($"a{i}");
            }

            cb.WriteLine(")");
            cb.WriteLine("{").Indent();
            cb.Write("return ");
            if (isOp)
            {
                if (argCount == 1)
                {
                    cb.Write($"{impl}a0");
                }
                else if (argCount == 2)
                {
                    cb.Write($"a0 {impl} a1");
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                cb.Write(impl);
                cb.Write("(");
                for (var i = 0; i < argCount; ++i)
                {
                    if (i > 0) cb.Write(", ");
                    cb.Write($"a{i}");
                }
                cb.Write(")");
            }

            cb.WriteLine(";");
            cb.Dedent().WriteLine("}");
        }

        public static CodeBuilder OutputOperators(CodeBuilder cb = null)
        {
            cb = cb ?? new CodeBuilder();

            foreach (var tuple in UnaryMathOperators)
            {
                OutputJavascriptFunc(cb, 1, tuple.Item1, tuple.Item2, true);
            }

            foreach (var tuple in BinaryMathOperators)
            {
                OutputJavascriptFunc(cb, 2, tuple.Item1, tuple.Item2, true);
            }

            foreach (var tuple in ComparisonOperators)
            {
                OutputJavascriptFunc(cb, 2, tuple.Item1, tuple.Item2, true);
            }

            foreach (var tuple in EqualityOperators)
            {
                OutputJavascriptFunc(cb, 2, tuple.Item1, tuple.Item2, true);
            }

            foreach (var tuple in BinaryBooleanOperators)
            {
                OutputJavascriptFunc(cb, 2, tuple.Item1, tuple.Item2, true);
            }

            foreach (var tuple in UnaryBooleanOperators)
            {
                OutputJavascriptFunc(cb, 1, tuple.Item1, tuple.Item2, true);
            }

            // Tuple reversed. 

            foreach (var tuple in BinaryMathFunctions)
            {
                OutputJavascriptFunc(cb, 2, tuple.Item2, tuple.Item1, false);
            }

            foreach (var tuple in UnaryMathFunctions)
            {
                OutputJavascriptFunc(cb, 1, tuple.Item2, tuple.Item1, false);
            }

            return cb;
        }
    }
}
