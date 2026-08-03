using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Utils;

namespace Ara3D.Geometry.TypeScriptWriter;

/// <summary>
/// Writes the body of a function by recursively translating the Plato symbol tree
/// into TypeScript. Analog of CSharpFunctionBodyWriter.
///
/// Notable differences from the C# version:
/// - Expression bodies become "{ return expr; }" blocks (TypeScript has no expression-bodied members).
/// - Literals and conditions are native (the Plato primitives map to native types).
/// - Every function call is parenthesized ("v.Length()"), except field access,
///   which stays a property read ("v.X"). This mirrors extension methods in C#.
/// - Tuples become constructor calls on the generated TupleN classes.
/// - "At" calls stay method calls (TypeScript has no indexers).
/// </summary>
public class TypeScriptFunctionBodyWriter : CodeBuilder<TypeScriptFunctionBodyWriter>
{
    public TypeScriptTypeWriter TypeWriter { get; }
    public TypeScriptWriter Writer => TypeWriter.Writer;
    public bool IsStaticOrLambda { get; }
    public bool IsLambda { get; }
    public TypeScriptFunctionInfo Function { get; }
    public string SelfType => TypeWriter.SelfType;

    public TypeScriptFunctionBodyWriter(TypeScriptTypeWriter typeWriter, TypeScriptFunctionInfo fi, bool isStatic, bool isLambda)
    {
        IndentLevel = typeWriter.IndentLevel;
        TypeWriter = typeWriter;
        IsStaticOrLambda = isStatic || isLambda;
        IsLambda = isLambda;

        Function = fi;

        if (fi.Body == null)
        {
            WriteLine($" {{ return Intrinsics.ThrowNotImplemented('{fi.Name}'); }}");
            return;
        }

        var body = fi.Body?.RewriteLambdasCapturingVars();
        if (body is Expression expression)
        {
            if (isLambda)
                Write(expression);
            else
                Write(" { return ").Write(expression, fi.ReturnType).WriteLine("; }");
        }
        else if (body is BlockStatement)
        {
            if (!isLambda)
                WriteLine();
            Write(body);
        }
    }

    public TypeScriptFunctionBodyWriter Write(IEnumerable<Symbol> symbols)
        => symbols.Aggregate(this, (writer, symbol) => writer.Write(symbol));

    public TypeScriptFunctionBodyWriter WriteCommaList(IEnumerable<Symbol> symbols)
        => WriteCommaList(symbols, (w, s) => w.Write(s));

    public TypeScriptFunctionBodyWriter WriteCommaList(IEnumerable<string> symbols)
        => WriteCommaList(symbols, (w, s) => w.Write(s));

    public TypeScriptFunctionBodyWriter Write(Symbol symbol, string type = null)
    {
        switch (symbol)
        {
            case TypeExpression typeExpression:
            {
                // A type used as a value: refers to the default instance.
                var name = TypeName(typeExpression);
                if (TypeScriptWriter.NativeDefaults.TryGetValue(name, out var native))
                    return Write(native);
                return Write(name).Write(".Default");
            }

            case Expression expression:
                return Write(expression, type);

            case Statement statement:
                return Write(statement);

            case null:
                return Write("null");

            case FunctionGroupDef memberGroup:
                return Write(memberGroup.Name);

            case ParameterDef parameter:
                return Write(TypeScriptTypeWriter.EscapeName(parameter.Name));

            case VariableDef variable:
                return Write("let ").Write(TypeScriptTypeWriter.EscapeName(variable.Name))
                    .Write(" = ").Write(variable.Value).WriteLine(";");
        }

        throw new NotSupportedException();
    }

    public string TypeName(TypeExpression typeExpression)
        => TypeWriter.ToTypeScriptTypeName(TypeInstance.Create(typeExpression));

    public static string GetLiteralType(Literal literal)
        => literal.TypeEnum.ToString();

    public static string GetLiteralValue(Literal literal)
        => literal.Value.ToLiteralString();

    public TypeScriptFunctionBodyWriter WriteLambdaBody(Lambda lambda)
    {
        var def = lambda.Function;
        var fi = TypeWriter.ToFunctionInfo(def, null, FunctionInstanceKind.Lambda);
        var tmp = new TypeScriptFunctionBodyWriter(TypeWriter, fi, true, true);
        Write(tmp.ToString());
        return this;
    }

    public TypeScriptFunctionBodyWriter WriteCondition(Symbol condition)
        => Write("(").Write(condition).Write(")");

    public TypeScriptFunctionBodyWriter Write(Statement st)
    {
        switch (st)
        {
            case ReturnStatement returnSymbol:
                Write("return ");
                if (returnSymbol.Expression != null)
                    // The declared return type renames a returned TupleN constructor
                    // (a lambda's return belongs to the lambda, not the outer function).
                    Write(returnSymbol.Expression, IsLambda ? null : Function?.ReturnType);
                WriteLine(";");
                return this;

            case LoopStatement loopSymbol:
                Write("while (").WriteCondition(loopSymbol.Condition).WriteLine(")");
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
                Write("if (");
                WriteCondition(ifStatement.Condition);
                WriteLine(")");
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

    /// <summary>
    /// Best-effort receiver type: parameters and variables carry their declared type,
    /// and chains of no-argument field reads (this.Min.X) are followed field-by-field.
    /// Anything else (real method calls, operators) resolves to null and the caller
    /// falls back to the current concrete type — mirroring HasFieldNamed on the TIR path.
    /// </summary>
    private TypeDef ResolveReceiverTypeDef(Expression e)
    {
        switch (e)
        {
            case ParameterRefSymbol p when p.Def?.Index == 0 && !IsStaticOrLambda:
            {
                // "this": the declared self type may be a concept; the ground type is
                // the concrete type currently being written.
                var declared = p.Def.Type?.Def;
                return declared != null && declared.Fields.Count > 0 ? declared : TypeWriter.TypeDef;
            }
            case VariableRefSymbol v:
            {
                // A let-binding's declared type is rarely useful (usually Any);
                // resolve through the initializer when it carries no fields.
                var declared = v.Def?.Type?.Def;
                if (declared != null && declared.Fields.Count > 0)
                    return declared;
                return ResolveReceiverTypeDef(v.Def?.Value);
            }
            case RefSymbol r:
                return r.Type?.Def;
            case Parenthesized paren:
                return ResolveReceiverTypeDef(paren.Expression);
            case FunctionCall fc:
            {
                var recv = fc.Args.Count >= 1 ? ResolveReceiverTypeDef(fc.Args[0]) : null;
                if (fc.Args.Count == 1 && !fc.HasArgList)
                {
                    var fieldName = (fc.Function as RefSymbol)?.Name;
                    var fd = recv?.Fields?.FirstOrDefault(x => x.Name == fieldName)?.Type?.Def;
                    if (fd != null)
                        return fd;
                }
                // Overload resolution lite: same arity, preferring overloads whose
                // first parameter matches the receiver; a unique concrete return
                // type among the survivors resolves the call.
                var group = (fc.Function as FunctionGroupRefSymbol)?.Def;
                if (group != null)
                {
                    var candidates = group.Functions
                        .Where(fn => fn.Parameters.Count == fc.Args.Count).ToList();
                    if (recv != null)
                    {
                        var byReceiver = candidates
                            .Where(fn => fn.Parameters.Count > 0 && fn.Parameters[0].Type?.Def == recv).ToList();
                        if (byReceiver.Count > 0)
                            candidates = byReceiver;
                    }
                    var returnDefs = candidates.Select(fn => fn.ReturnType?.Def).Distinct().ToList();
                    if (returnDefs.Count == 1 && returnDefs[0] != null)
                        return returnDefs[0];
                }
                // The arithmetic operators preserve the type of their first operand.
                var opName = (fc.Function as RefSymbol)?.Name;
                if (fc.Args.Count >= 1
                    && (opName == "Multiply" || opName == "Divide" || opName == "Add"
                        || opName == "Subtract" || opName == "Negative" || opName == "Modulo"))
                    return recv;
                return null;
            }
            default:
                return null;
        }
    }

    public TypeScriptFunctionBodyWriter WriteFunctionCall(FunctionCall functionCall, string type = null)
    {
        // If there are no arguments, it is a constant.
        if (functionCall.Args.Count == 0)
        {
            if (functionCall.Function is KeywordRefSymbol krs)
            {
                if (krs.Name != "default")
                    throw new Exception("Only default keyword supported");
                return Write("(undefined as any)");
            }
            else
            {
                return Write("Constants.").Write(functionCall.Function);
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
        {
            var ctorName = !string.IsNullOrEmpty(type) && type != f.Name
                ? type
                : f.Name;
            return Write($"new {ctorName}(").WriteCommaList(functionCall.Args).Write(")");
        }

        var arg = functionCall.Args[0];

        // Some receivers must be parenthesized: "1.Subtract" is a syntax error
        // ("(1).Subtract" is not), and a ternary or lambda receiver would
        // otherwise bind the member access to its last operand only.
        if (arg is Literal || arg is ConditionalExpression || arg is Lambda || arg is Assignment)
            Write("(").Write(arg).Write(")");
        else
            Write(arg);
        Write(".").Write(functionCall.Function);

        // Field access stays a property read only when the receiver type, or the
        // current concrete type as a fallback, actually declares that field.
        if (functionCall.Args.Count == 1 && !functionCall.HasArgList
            && (TypeWriter.HasFieldNamed(ResolveReceiverTypeDef(functionCall.Args[0]), f.Name)
                || TypeWriter.HasFieldNamed(f.Name)))
            return this;

        return Write("(").WriteCommaList(functionCall.Args.Skip(1)).Write(")");
    }

    public TypeScriptFunctionBodyWriter Write(Expression expr, string type = null)
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
                var typeName = Function.ToTypeScriptType(newExpression.Type);
                if (typeName.StartsWith("Tuple") && !string.IsNullOrEmpty(type) && type != typeName)
                    typeName = type;
                // "new Number(x)" on a native type is just the value itself.
                if (TypeScriptWriter.NativeDefaults.ContainsKey(typeName))
                    return Write("(").WriteCommaList(newExpression.Args).Write(")");
                return Write($"new {typeName}(").WriteCommaList(newExpression.Args).Write(")");
            }

            case ParameterRefSymbol pr:
                return pr.Def.Index == 0 && !IsStaticOrLambda
                    ? Write("this")
                    : Write(TypeScriptTypeWriter.EscapeName(pr.Name));

            case FunctionGroupRefSymbol fgr:
                // HACK: check if it is a constant.
                // TODO: I need to have all function calls properly resolved to generate better quality code.
                if (fgr.Def.Functions.Count == 1 &&
                    fgr.Def.Functions[0].NumParameters == 0)
                    return Write($"Constants.{fgr.Name}");
                return Write(fgr.Name);

            case RefSymbol refSymbol:
                return Write(refSymbol.Name == "Self" ? SelfType : TypeScriptTypeWriter.EscapeName(refSymbol.Name));

            case Assignment assignment:
                return Write(assignment.LValue)
                    .Write(" = ")
                    .Write(assignment.RValue);

            case ConditionalExpression conditional:
                return WriteCondition(conditional.Condition)
                    .Write(" ? ").Write(conditional.IfTrue)
                    .Write(" : ").Write(conditional.IfFalse);

            case FunctionCall functionCall:
                return WriteFunctionCall(functionCall, type);

            case Literal literal:
                return WriteLiteral(literal);

            case Lambda lambda:
                return Write("(")
                    .WriteCommaList(lambda.Function.Parameters.Select(p => TypeScriptTypeWriter.EscapeName(p.Name)))
                    .Write(") => ")
                    .WriteLambdaBody(lambda);

            case ArrayLiteral arrayLiteral:
                return Write("Intrinsics.MakeArray(")
                    .WriteCommaList(arrayLiteral.Expressions)
                    .Write(")");
        }

        throw new ArgumentOutOfRangeException(nameof(expr));
    }

    public TypeScriptFunctionBodyWriter WriteLiteral(Literal literal)
    {
        var type = GetLiteralType(literal);
        var value = GetLiteralValue(literal);
        switch (type)
        {
            case "Character":
                return Write($"'{value}'");
            default:
                // Numbers, integers, booleans, and strings are all native literals.
                return Write(value);
        }
    }
}
