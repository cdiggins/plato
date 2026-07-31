using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Utils;

namespace Ara3D.Geometry.RustWriter;

/// <summary>
/// Writes the body of a function by recursively translating the Plato symbol tree
/// into Rust. Analog of TypeScriptFunctionBodyWriter.
///
/// Notable differences from the TypeScript version:
/// - Expression bodies become "{ expr }" blocks (Rust expression bodies).
/// - Conditional expressions become "if c { a } else { b }" (a valid expression).
/// - Number literals are forced to have a decimal point ("1" -> "1.0"): Rust has
///   no implicit int-to-float conversion, so this matters (TypeScript did not care).
/// - Integer literals used as receivers get an i64 suffix ("(1i64).Foo()") so
///   method resolution finds the IntegerExt trait.
/// - Zero-argument constants become "Constants::Pi()" (function call, "::" path).
/// - Static calls become "Type::F(x)".
/// - Constructor calls become "T::new(args)".
/// - Lambdas become closures: "|a, b| body".
/// - A type used as a value becomes "T::default()" (or a native default literal).
/// </summary>
public class RustFunctionBodyWriter : CodeBuilder<RustFunctionBodyWriter>
{
    public RustTypeWriter TypeWriter { get; }
    public RustWriter Writer => TypeWriter.Writer;
    public bool IsStaticOrLambda { get; }
    public RustFunctionInfo Function { get; }
    public string SelfType => TypeWriter.SelfType;

    public RustFunctionBodyWriter(RustTypeWriter typeWriter, RustFunctionInfo fi, bool isStatic, bool isLambda)
    {
        IndentLevel = typeWriter.IndentLevel;
        TypeWriter = typeWriter;
        IsStaticOrLambda = isStatic || isLambda;

        Function = fi;

        if (fi.Body == null)
        {
            WriteLine($" {{ unimplemented!(\"{fi.Name}\") }}");
            return;
        }

        var body = fi.Body?.RewriteLambdasCapturingVars();
        if (body is Expression expression)
        {
            if (isLambda)
                Write(expression);
            else
                Write(" { ").Write(expression, fi.ReturnType).WriteLine(" }");
        }
        else if (body is BlockStatement)
        {
            if (!isLambda)
                WriteLine();
            Write(body);
        }
    }

    public RustFunctionBodyWriter Write(IEnumerable<Symbol> symbols)
        => symbols.Aggregate(this, (writer, symbol) => writer.Write(symbol));

    public RustFunctionBodyWriter WriteCommaList(IEnumerable<Symbol> symbols)
        => WriteCommaList(symbols, (w, s) => w.Write(s));

    public RustFunctionBodyWriter WriteCommaList(IEnumerable<string> symbols)
        => WriteCommaList(symbols, (w, s) => w.Write(s));

    public RustFunctionBodyWriter Write(Symbol symbol, string type = null)
    {
        switch (symbol)
        {
            case TypeExpression typeExpression:
            {
                // A type used as a value: refers to the default instance.
                var name = TypeName(typeExpression);
                if (RustWriter.NativeDefaults.TryGetValue(name, out var native))
                    return Write(native);
                return Write(name).Write("::default()");
            }

            case Expression expression:
                return Write(expression, type);

            case Statement statement:
                return Write(statement);

            case null:
                return Write("()");

            case FunctionGroupDef memberGroup:
                return Write(memberGroup.Name);

            case ParameterDef parameter:
                return Write(RustTypeWriter.EscapeName(parameter.Name));

            case VariableDef variable:
                return Write("let mut ").Write(RustTypeWriter.EscapeName(variable.Name))
                    .Write(" = ").Write(variable.Value).WriteLine(";");
        }

        throw new NotSupportedException();
    }

    public string TypeName(TypeExpression typeExpression)
        => TypeWriter.ToRustTypeName(TypeInstance.Create(typeExpression));

    public static string GetLiteralType(Literal literal)
        => literal.TypeEnum.ToString();

    public static string GetLiteralValue(Literal literal)
        => literal.Value.ToLiteralString();

    /// <summary>
    /// Rust has no implicit int-to-float conversion, so Number literals must
    /// have a decimal point ("1" -> "1.0").
    /// </summary>
    public static string ForceDecimal(string value)
        => value.Contains('.') || value.Contains('e') || value.Contains('E')
            ? value
            : value + ".0";

    public RustFunctionBodyWriter WriteLambdaBody(Lambda lambda)
    {
        var def = lambda.Function;
        var fi = TypeWriter.ToFunctionInfo(def, null, FunctionInstanceKind.Lambda);
        var tmp = new RustFunctionBodyWriter(TypeWriter, fi, true, true);
        Write(tmp.ToString());
        return this;
    }

    public RustFunctionBodyWriter WriteCondition(Symbol condition)
        => Write(condition);

    public RustFunctionBodyWriter Write(Statement st)
    {
        switch (st)
        {
            case ReturnStatement returnSymbol:
                Write("return ");
                if (returnSymbol.Expression != null)
                    Write(returnSymbol.Expression);
                WriteLine(";");
                return this;

            case LoopStatement loopSymbol:
                Write("while ").WriteCondition(loopSymbol.Condition).WriteLine();
                WriteStartBlock();
                Write(loopSymbol.Body);
                WriteEndBlock();
                WriteLine();
                return this;

            case MultiStatement multiStatement:
                foreach (var child in multiStatement.Symbols)
                {
                    Write(child);
                    if (child is Expression)
                        WriteLine(";");
                }
                return this;

            case BlockStatement block:
            {
                WriteStartBlock();
                foreach (var x in block.Symbols)
                {
                    Write(x);
                    if (x is Expression)
                        WriteLine(";");
                }

                return WriteEndBlock();
            }

            case CommentStatement commentStatement:
                return Write($"/* {commentStatement.Comment} */");

            case IfStatement ifStatement:
                Write("if ");
                WriteCondition(ifStatement.Condition);
                WriteLine();
                Write(ifStatement.IfTrue);
                if (ifStatement.IfFalse != null)
                {
                    WriteLine("else");
                    Write(ifStatement.IfFalse);
                }

                return this;
        }

        return this;
    }

    public RustFunctionBodyWriter WriteFunctionCall(FunctionCall functionCall)
    {
        // If there are no arguments, it is a constant.
        if (functionCall.Args.Count == 0)
        {
            if (functionCall.Function is KeywordRefSymbol krs)
            {
                if (krs.Name != "default")
                    throw new Exception("Only default keyword supported");
                return Write("Default::default()");
            }
            else
            {
                return Write("Constants::").Write(functionCall.Function).Write("()");
            }
        }

        // Calling a parameter, or variable: a direct function invocation.
        if (functionCall.Function is ParameterRefSymbol
            || functionCall.Function is VariableRefSymbol
            || functionCall.Function is FunctionCall)
        {
            return Write(functionCall.Function).Write("(").WriteCommaList(functionCall.Args).Write(")");
        }

        var f = functionCall.Function;
        if (f.Name.StartsWith("Tuple"))
            return Write($"{f.Name}::new(").WriteCommaList(functionCall.Args).Write(")");

        var arg = functionCall.Args[0];

        // A type as the receiver is a static (associated function) call: Type::F(x).
        if (arg is TypeExpression typeExpr)
        {
            return Write(TypeName(typeExpr)).Write("::").Write(functionCall.Function)
                .Write("(").WriteCommaList(functionCall.Args.Skip(1)).Write(")");
        }

        // Some receivers must be parenthesized: "1.Subtract" would not parse, and
        // integer literals additionally need an i64 suffix so that method lookup
        // finds the IntegerExt trait ("(1i64).Compare(x)"). An if-else expression
        // or lambda receiver also needs the parentheses.
        if (arg is Literal literal)
            Write("(").WriteLiteral(literal, true).Write(")");
        else if (arg is ConditionalExpression || arg is Lambda || arg is Assignment)
            Write("(").Write(arg).Write(")");
        else
            Write(arg);
        Write(".").Write(functionCall.Function);

        // Field access stays a field read (fields are public struct fields);
        // every actual function call is parenthesized, including zero-argument ones.
        if (functionCall.Args.Count == 1 && !functionCall.HasArgList
            && Writer.AllFieldNames.Contains(f.Name))
            return this;

        return Write("(").WriteCommaList(functionCall.Args.Skip(1)).Write(")");
    }

    public RustFunctionBodyWriter Write(Expression expr, string type = null)
    {
        if (expr == null)
            return this;

        switch (expr)
        {
            case TypeExpression typeExpression:
                if (typeExpression.TypeArgs.Count > 0)
                    throw new NotSupportedException();
                return Write(TypeName(typeExpression));

            case NewExpression newExpression:
            {
                // Use the bare type name: generic arguments are inferred in Rust
                // expression position (turbofish is not required).
                var typeName = TypeName(newExpression.Type);
                // "new Number(x)" on a native type is just the value itself.
                if (RustWriter.NativeDefaults.ContainsKey(typeName))
                    return Write("(").WriteCommaList(newExpression.Args).Write(")");
                return Write($"{typeName}::new(").WriteCommaList(newExpression.Args).Write(")");
            }

            case ParameterRefSymbol pr:
                return pr.Def.Index == 0 && !IsStaticOrLambda
                    ? Write("self")
                    : Write(RustTypeWriter.EscapeName(pr.Name));

            case FunctionGroupRefSymbol fgr:
                // HACK: check if it is a constant.
                // TODO: I need to have all function calls properly resolved to generate better quality code.
                if (fgr.Def.Functions.Count == 1 &&
                    fgr.Def.Functions[0].NumParameters == 0)
                    return Write($"Constants::{fgr.Name}()");
                return Write(fgr.Name);

            case RefSymbol refSymbol:
                return Write(refSymbol.Name == "Self" ? SelfType : RustTypeWriter.EscapeName(refSymbol.Name));

            case Assignment assignment:
                return Write(assignment.LValue)
                    .Write(" = ")
                    .Write(assignment.RValue);

            case ConditionalExpression conditional:
                return Write("if ").WriteCondition(conditional.Condition)
                    .Write(" { ").Write(conditional.IfTrue)
                    .Write(" } else { ").Write(conditional.IfFalse)
                    .Write(" }");

            case FunctionCall functionCall:
                return WriteFunctionCall(functionCall);

            case Literal literal:
                return WriteLiteral(literal, false);

            case Lambda lambda:
                return Write("|")
                    .WriteCommaList(lambda.Function.Parameters.Select(p => RustTypeWriter.EscapeName(p.Name)))
                    .Write("| ")
                    .WriteLambdaBody(lambda);

            case ArrayLiteral arrayLiteral:
                return Write("Intrinsics::MakeArray(vec![")
                    .WriteCommaList(arrayLiteral.Expressions)
                    .Write("])");
        }

        throw new ArgumentOutOfRangeException(nameof(expr));
    }

    public RustFunctionBodyWriter WriteLiteral(Literal literal, bool asReceiver)
    {
        var type = GetLiteralType(literal);
        var value = GetLiteralValue(literal);
        switch (type)
        {
            case "Character":
                return Write($"'{value}'");
            case "Number":
                return Write(ForceDecimal(value));
            case "Integer":
                // As a receiver, the literal needs an explicit i64 type so that
                // method resolution finds the IntegerExt trait.
                return Write(asReceiver ? $"{value}i64" : value);
            case "String":
                return Write($"{value}.to_string()");
            default:
                // Booleans are native literals.
                return Write(value);
        }
    }
}
