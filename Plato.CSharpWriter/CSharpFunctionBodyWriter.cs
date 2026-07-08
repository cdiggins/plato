using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;

namespace Ara3D.Geometry.CSharpWriter;

public class CSharpFunctionBodyWriter : CodeBuilder<CSharpFunctionBodyWriter>
{
    public CSharpTypeWriter TypeWriter { get; }
    public CSharpWriter Writer => TypeWriter.Writer;
    public bool IsStaticOrLambda { get; }
    public CSharpFunctionInfo Function { get; }     
    public string SelfType => TypeWriter.SelfType;

    public string Namespace => Writer.Namespace;

    // "propOverride" forces (or suppresses) the property-getter wrapper independently of the
    // isStatic/NumParameters heuristic. Used by the extension-block writer (ExtensionStyleWriter),
    // where the body is written in "static" mode (parameter names, no "this") but the emitted
    // member is an instance-style extension property. Null preserves the original behavior.
    public CSharpFunctionBodyWriter(CSharpTypeWriter typeWriter, CSharpFunctionInfo fi, bool isStatic, bool isLambda, bool? propOverride = null)
    {
        IndentLevel = typeWriter.IndentLevel;
        TypeWriter = typeWriter;
        IsStaticOrLambda = isStatic;

        Function = fi;

        if (fi.Body == null)
        {
            WriteLine(" => throw new NotImplementedException();");
            return;
        }

        var isProp = propOverride ?? (isStatic ? fi.NumParameters == 0 : fi.NumParameters == 1);
        if (isProp)
            Write($" {{ {CSharpTypeWriter.Annotation} get ");

        var body = fi.Body?.RewriteLambdasCapturingVars();
        if (body is Expression)
        {
            Write($" => ").Write(body, fi.ReturnType);
            if (!isLambda)
                Write(";");
        }
        else if (body is BlockStatement)
            Write(body);
        if (isProp)
            Write(" } ");
        if (!isLambda)
            WriteLine();
    }

    public CSharpFunctionBodyWriter Write(IEnumerable<Symbol> symbols)
        => symbols.Aggregate(this, (writer, symbol) => writer.Write(symbol));

    public CSharpFunctionBodyWriter WriteCommaList(IEnumerable<Symbol> symbols)
        => WriteCommaList(symbols, (w, s) => w.Write(s));

    public CSharpFunctionBodyWriter WriteCommaList(IEnumerable<string> symbols)
        => WriteCommaList(symbols, (w, s) => w.Write(s));

    public CSharpFunctionBodyWriter Write(Symbol symbol, string type = null)
    {
        switch (symbol)
        {
            case TypeExpression typeExpression:
                return Write(typeExpression).Write(".Default");

            case Expression expression:
                return Write(expression, type);

            case Statement statement:
                return Write(statement);
            
            case null:
                return Write("null");
            
            case FieldDef fieldDef:
                return Write("public ").Write(fieldDef.Type).Write(fieldDef.Name).Write(" { get; }").WriteLine();
            
            case FunctionGroupDef memberGroup:
                return Write(memberGroup.Name);
            
            case ParameterDef parameter:
                return Write(parameter.Type).Write(parameter.Name);
            
            case VariableDef variable:
                return Write("var ").Write(variable.Name).Write(" = ").Write(variable.Value).WriteLine(";");
            
            default:
                throw new ArgumentOutOfRangeException(nameof(symbol));
        }
    }

    public static string GetLiteralType(Literal literal) 
        => literal.TypeEnum.ToString();

    public static string GetLiteralValue(Literal literal) 
        => literal.Value.ToLiteralString();

    public CSharpFunctionBodyWriter WriteLambdaBody(Lambda lambda)
    {
        var def = lambda.Function;
        var fi = TypeWriter.ToFunctionInfo(def, null, FunctionInstanceKind.Lambda);       
        var tmp = new CSharpFunctionBodyWriter(TypeWriter, fi, true, true);
        Write(tmp.ToString());
        return this;
    }

    public CSharpFunctionBodyWriter Write(Statement st)
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
                Write("while (").Write(loopSymbol.Condition).WriteLine(")");
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
                Write(ifStatement.Condition);
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

    public CSharpFunctionBodyWriter WriteFunctionCall(FunctionCall functionCall)
    {
        // If there are no arguments, it is a constant.
        if (functionCall.Args.Count == 0)
        {
            if (functionCall.Function is KeywordRefSymbol krs)
            {
                if (krs.Name != "default")
                    throw new Exception("Only default keyword supported");
                return Write("default");
            }
            else
            {
                return Write("Constants.").Write(functionCall.Function);
            }
        }


        // Calling a parameter, or variable 
        if (functionCall.Function is ParameterRefSymbol 
            || functionCall.Function is VariableRefSymbol
            || functionCall.Function is FunctionCall)
        {
            return Write(functionCall.Function).Write(".Invoke(").WriteCommaList(functionCall.Args).Write(")");
        }

        var f = functionCall.Function;
        if (f.Name.StartsWith("Tuple"))
            return Write("(").WriteCommaList(functionCall.Args).Write(")");

        var arg = functionCall.Args[0];

        if (f.Name == "At")
        {
            return Write(arg).Write("[").WriteCommaList(functionCall.Args.Skip(1)).Write("]");
        }

        Write(arg).Write(".");
        // Member-access position: the name is resolved against the receiver, so the
        // extension-style static qualifier must never apply here.
        if (TypeWriter.ExtensionReceiverName != null && functionCall.Function is FunctionGroupRefSymbol memberRef)
            Write(memberRef.Name);
        else
            Write(functionCall.Function);

        if (functionCall.Args.Count == 1 && !functionCall.HasArgList)
            return this;
            // TEMPORARY: this will route to the extension methods now instead of the properties.
            //return Write("()");
            
        return Write("(").WriteCommaList(functionCall.Args.Skip(1)).Write(")");
    }

    public CSharpFunctionBodyWriter Write(Expression expr, string type = null)
    {
        if (expr == null)
            return this;    
            
        switch (expr)
        {
            case TypeExpression typeExpression:
                if (typeExpression.TypeArgs.Count > 0)
                    throw new NotSupportedException();
                return Write(Namespace + "." + typeExpression.Name);
    
            case NewExpression newExpression:
                return Write($"new {Function.ToCSharpType(newExpression.Type)}(").WriteCommaList(newExpression.Args).Write(")");

            case ParameterRefSymbol pr:
                return pr.Def.Index == 0 && !IsStaticOrLambda
                    ? Write("this") 
                    : Write(pr.Name);

            case FunctionGroupRefSymbol fgr:
                // HACK: check if it is a constant.
                // TODO: I need to have all function calls properly resolved to generate better quality code. 
                if (fgr.Def.Functions.Count == 1 &&
                    fgr.Def.Functions[0].NumParameters == 0)
                    return Write($"Constants.{fgr.Name}");
                // Extension-style mode: bare names bound implicitly inside the partial struct
                // in the default mode; inside an extension block they must be re-qualified.
                if (TypeWriter.ExtensionReceiverName != null)
                {
                    if (TypeWriter.ExtensionInstanceNames.Contains(fgr.Name))
                        return Write($"{TypeWriter.ExtensionReceiverName}.{fgr.Name}");
                    if (TypeWriter.ExtensionStaticNames.Contains(fgr.Name))
                        return Write($"{TypeWriter.ExtensionStaticQualifier}.{fgr.Name}");
                    // Type references (e.g. "Integer2" in "Integer2.CreateFromComponents"):
                    // namespace-qualified, because members of the enclosing static library
                    // class can shadow type names.
                    if (Writer.AllTypeNames.Contains(fgr.Name))
                        return Write($"{Namespace}.{fgr.Name}");
                    // Unknown to the compiler: a handwritten member of the receiver type in
                    // Plato.Intrinsics (e.g. Number.Zero). In the default mode it bound inside
                    // the enclosing struct; statics are the only observed case.
                    return Write($"{TypeWriter.ExtensionStaticQualifier}.{fgr.Name}");
                }
                return Write(fgr.Name);

            case RefSymbol refSymbol:
                return Write(refSymbol.Name == "Self" ? SelfType : refSymbol.Name);
                
                //if (refSymbol.Name == "Self") throw new Exception("Self type references no longer supported");
            
                //return Write(refSymbol.Name);

            case Assignment assignment:
                return Write(assignment.LValue)
                    .Write(" = ")
                    .Write(assignment.RValue);

            case ConditionalExpression conditional:
                return Write(conditional.Condition)
                    .Write(" ? ").Write(conditional.IfTrue)
                    .Write(" : ").Write(conditional.IfFalse);

            case FunctionCall functionCall:
                return WriteFunctionCall(functionCall);

            case Literal literal:
                // TODO: once validating that the cost is superfluous, we can remove this. 
                return Write($"(({GetLiteralType(literal)}){GetLiteralValue(literal)})");
            
            case Lambda lambda:
                return Write("(")
                    .WriteCommaList(lambda.Function.Parameters.Select(p => p.Name))
                    .Write(") ")
                    .WriteLambdaBody(lambda);

            case ArrayLiteral arrayLiteral:
            {
                var arg = "";
                if (type != null && type.StartsWith("IArray"))
                    arg = type.Substring("IArray".Length);
                return Write($"Intrinsics.MakeArray{arg}(")
                    .WriteCommaList(arrayLiteral.Expressions)
                    .Write(")");
            }
        }

        throw new ArgumentOutOfRangeException(nameof(expr));
    }
}