using System.Collections.Generic;
using System.Linq;

namespace Ara3D.Geometry.AST
{
    public static class Operators
    {
        public static (string, string)[] BinaryOperators => new[]
        {
            ("+", "Add"),
            ("-", "Subtract"),
            ("*", "Multiply"),
            ("/", "Divide"),
            ("%", "Modulo"),
            ("==", "Equals"),
            ("!=", "NotEquals"),
            (">", "GreaterThan"),
            ("<", "LessThan"),
            (">=", "GreaterThanOrEquals"),
            ("<=", "LessThanOrEquals"),
            ("&&", "And"),
            ("||", "Or"),
            ("&", "BitwiseAnd"),
            ("|", "BitwiseOr"),
            ("^", "XOr")
        };

        public static (string, string)[] UnaryOperators => new[]
        {
            ("-", "Negative"),
            ("!", "Not"),
            ("~", "Complement"),
        };

        public static Dictionary<string, string> BinaryOperatorToNames
            = BinaryOperators.ToDictionary(op => op.Item1, op => op.Item2);

        public static Dictionary<string, string> NamesToBinaryOperators
            = BinaryOperators.ToDictionary(op => op.Item2, op => op.Item1);

        public static string BinaryOperatorToName(string op)
            => BinaryOperatorToNames.ContainsKey(op) ? BinaryOperatorToNames[op] : op;

        public static string NameToBinaryOperator(string name)
            => NamesToBinaryOperators.ContainsKey(name) ? NamesToBinaryOperators[name] : null;

        public static Dictionary<string, string> UnaryOperatorToNames
            = UnaryOperators.ToDictionary(op => op.Item1, op => op.Item2);

        public static Dictionary<string, string> NamesToUnaryOperators
            = UnaryOperators.ToDictionary(op => op.Item2, op => op.Item1);

        public static string UnaryOperatorToName(string op)
            => UnaryOperatorToNames.ContainsKey(op) ? UnaryOperatorToNames[op] : op;

        public static string NameToUnaryOperator(string name)
            => NamesToUnaryOperators.ContainsKey(name) ? NamesToUnaryOperators[name] : null;

        public static int BinaryOperatorPrecedence(string op)
        {
            switch (op)
            {
                case "*":
                case "/":
                case "%":
                    return 10;
                case "+":
                case "-":
                    return 9;
                case ">>":
                case "<<":
                    return 8;
                case "<":
                case ">":
                case "<=":
                case ">=":
                    return 7;
                case "==":
                case "!=":
                    return 6;
                case "&":
                    return 5;
                case "^":
                    return 4;
                case "|":
                    return 3;
                case "&&":
                    return 2;
                case "||":
                    return 1;
                default:
                    return 0;
            }
        }
    }
}