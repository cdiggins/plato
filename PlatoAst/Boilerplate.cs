using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Parakeet;
using Parakeet.Demos;
using Parakeet.Demos.CSharp;

namespace PlatoAst
{
    public static class Boilerplate
    {

        // AdditiveInverse: Negate, 
        // MultiplicativeInverse: Reciprocal,  
        // Add: +
        // Subtract: -
        // Multiply: *
        // Divide: /, %, DivMod, 

        // Boolean
        // And/Or/XOr/Not

        // Bitwise
        // ShiftLeft, ShiftRight, BitAnd, BitOr, Complement, XOr : <<, >>, &, |, ~,
        // 

        // 
        // Compare: <, >, <=, >=, CompareTo,
        // Sameness: ==, !=, Equals, NotEquals,
        // String:
        // Parse: 
        // Hashable: 

        // Vector:
        // * Dot product
        // * LengthSquared
        // * Length
        // * IArray : Count, At

        // Zero, One, Default, MinValue, MaxValue  

        // WithX methods   
        // WithYZ ? (combinatorial explosion) 

        // Binary to/from conversion
        // JSON to/from conversion
        // XML to/from conversion

        // Static create function (could be during code gen step)
        // 

        // Implicit conversion: 
        // Conversion to/from tuple (regular)
        // Conversion to/from array (vector)

        // Things applied to concepts, how do they get out, and apply to types of those concepts? 

        // Meta stuff:
        // FieldDefSymbol names: IArray<string>
        // FieldDefSymbol values: IArray<string> 
        // Type name: 

        // Explicit conversion? 
        // .CastX, .AsX, ToX, 

        // Is an interval a vector of 2?
        // 
        // I am missing a lot of 

        public static CSharpGrammar Grammar = new CSharpGrammar();
        public static AstFromCSharpCst AstFromCSharpCst = new AstFromCSharpCst();

        public static AstNode Parse(string input, Rule rule)
        {
            var pi = new ParserInput(input);
            var ps = pi.Parse(rule);
            var pt = ps.Node.ToParseTree();
            var cn = CstNodeFactory.Create(pt);
            var an = AstFromCSharpCst.ToAst(cn);
            return an;
        }

        public static AstMethodDeclaration CreateMethod(string name, AstTypeNode type, string body, params AstParameterDeclaration[] parameters)
        {
            //var bodyNode = Parse(body, Grammar.Expression) as CstExpression;
            
            
            // TODO: simplify parsing ASTs. 
            // TODO: I think CST 
            // 1) find the rule,
            // 2) create the CST,
            // 3) convert the CST into an AST,
             
            throw new NotImplementedException();
            // var bodyAst = 
            return new AstMethodDeclaration(name, type, parameters, null, null);
        }

        public static AstTypeDeclaration AddMethods(this AstTypeDeclaration astType,
            IEnumerable<AstMethodDeclaration> methods)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<AstMethodDeclaration> CreateScalarOps(this AstTypeDeclaration type)
        {
            if (!type.GetFields().Any())
                throw new NotSupportedException("scalar ops require fields");

            var fieldTypes = type.GetFields().Select(f => f.Type.Name).ToList();
            var fieldType = type.GetFields().Select(f => f.Type).First();
            var fieldTypeName = fieldTypes[0];
            if (!fieldTypes.All(ft => ft == fieldTypeName))
                throw new NotSupportedException("Fields types must all be the same for scalar ops");

            var names = new[] { "Add", "Subtract", "Multiply", "Divide", "Modulo" };
            foreach (var name in names)
            {
                var typeNode = type.ToTypeNode();
                {
                    var p0 = new AstParameterDeclaration(Arg0, typeNode);
                    var p1 = new AstParameterDeclaration(Arg1, fieldType);
                    var args = BinaryOpArgs(type, name);
                    yield return CreateMethod(name, type.ToTypeNode(), $"new {type.Name}({args})", p0, p1);
                }
                {
                    var p0 = new AstParameterDeclaration(Arg0, fieldType);
                    var p1 = new AstParameterDeclaration(Arg1, typeNode);
                    var args = BinaryOpArgs(type, name);
                    yield return CreateMethod(name, type.ToTypeNode(), $"new {type.Name}({args})", p0, p1);
                }
            }
        }

        public static AstTypeDeclaration AddBoilerPlate(this AstTypeDeclaration type)
        {
            var methods = new List<AstMethodDeclaration>();

            var hasFields = type.GetFields().Any();
            
            // TODO: type name.
 
            if (hasFields)
            {
                methods.AddRange(CreateSpecialValues(type));

                // Add with functions.
                foreach (var f in type.GetFields())
                {
                    var name = f.Name;
                    var fieldType = f.Type;
                    var tmp = "_" + name;
                    var param = new AstParameterDeclaration(tmp, fieldType);
                    var argList = new List<string>();
                    foreach (var f2 in type.GetFields())
                    {
                        if (f2.Name == name)
                            argList.Add(tmp);
                        else
                            argList.Add(f2.Name);
                    }

                    var args = string.Join(", ", argList);
                    var meth = CreateMethod($"With{name}", fieldType, $"new {type.Name}({args})", param);
                    methods.Add(meth);
                }
            }

            //if (type.HasAttribute("Numerical"))
            {
                methods.AddRange(CreateMultiplicationOps(type));
                methods.AddRange(CreateAdditionOps(type));
                methods.AddRange(CreateComparisonOps(type));
            }

            //if (type.HasAttribute("Value"))
            {
                // 
            }

            //if (type.HasAttribute("Measure"))
            {
                // Comparison:
                methods.AddRange(CreateComparisonOps(type));
                methods.AddRange(CreateAdditionOps(type));
            }

            //if (type.HasAttribute("Vector"))
            {
                methods.AddRange(CreateMultiplicationOps(type));
                methods.AddRange(CreateAdditionOps(type));
                methods.AddRange(CreateScalarOps(type));
            }

            // if (type.HasAttribute("Interval"))
            {

            }

            return type.AddMethods(methods);
        }

        public const string Arg0 = "arg0";
        public const string Arg1 = "arg1";

        public static string BinaryOpArgs(this AstTypeDeclaration type, string name)
        {
            return type.GetFields().Select(f => $"{Arg0}.{f.Name}.{name}({Arg1}.{f.Name})").CommaSeparated();
        }

        public static string UnaryOpArgs(this AstTypeDeclaration type, string name)
        {
            return type.GetFields().Select(f => $"{Arg0}.{f.Name}.{name}").CommaSeparated();
        }

        public static AstMethodDeclaration CreateBinaryMethod(this AstTypeDeclaration type, string name, AstTypeNode typeNode = null)
        {
            typeNode = typeNode ?? type.ToTypeNode();
            var p0 = new AstParameterDeclaration(Arg0, typeNode);
            var p1 = new AstParameterDeclaration(Arg1, typeNode);
            var args = BinaryOpArgs(type, name);
            return CreateMethod(name, type.ToTypeNode(), $"new {type.Name}({args})", p0, p1);
        }

        public static AstMethodDeclaration CreateUnaryMethod(this AstTypeDeclaration type, string name, AstTypeNode typeNode = null)
        {
            typeNode = typeNode ?? type.ToTypeNode();
            var p0 = new AstParameterDeclaration(Arg0, typeNode);
            var args = UnaryOpArgs(type, name);
            return CreateMethod(name, type.ToTypeNode(), $"new {type.Name}({args})", p0);
        }

        public static IEnumerable<AstMethodDeclaration> CreateMultiplicationOps(this AstTypeDeclaration type)
        {
            var names = new[] { "Multiply", "Divide", "Modulo" };
            foreach (var name in names)
            {
                yield return CreateBinaryMethod(type, name);
            }

            yield return CreateUnaryMethod(type, "Inverse");
        }

        public static IEnumerable<AstMethodDeclaration> CreateComparisonOps(this AstTypeDeclaration type)
        {
            var names = new[] { "LessThan", "GreaterThan", "LessThanOrEquals", "GreaterThanOrEquals", "CompareTo" };
            foreach (var name in names)
            {
                yield return CreateBinaryMethod(type, name, new AstTypeNode("Boolean"));
            }
        }

        public static IEnumerable<AstMethodDeclaration> CreateEqualityOps(this AstTypeDeclaration type)
        {
            var names = new[] { "Equals", "NotEquals" };
            foreach (var name in names)
            {
                yield return CreateBinaryMethod(type, name, new AstTypeNode("Boolean"));
            }
        }

        public static IEnumerable<AstMethodDeclaration> CreateAdditionOps(this AstTypeDeclaration type)
        {
            var names = new[] { "Add", "Subtract" };
            foreach (var name in names)
            {
                yield return CreateBinaryMethod(type, name);
            }

            yield return CreateUnaryMethod(type, "Negate");
        }

        public static IEnumerable<AstMethodDeclaration> CreateSpecialValues(this AstTypeDeclaration type)
        {
            var names = new [] { "Min", "Max", "Zero", "One", "Default", "ToString" };
            foreach (var name in names)
            {
                var args = UnaryOpArgs(type, name);
                yield return CreateMethod(name, type.ToTypeNode(), $"new {type.Name}({args})");
            }
        }

        public static IEnumerable<AstFieldDeclaration> GetFields(this AstTypeDeclaration astType)
        {
            return astType.Members.OfType<AstFieldDeclaration>();
        }

        public static string CommaSeparated<T>(this IEnumerable<T> list)
            => string.Join(", ", list);

        public static AstTypeNode ToTypeNode(this AstTypeDeclaration astType)
            => new AstTypeNode(astType.Name);
    }
}
