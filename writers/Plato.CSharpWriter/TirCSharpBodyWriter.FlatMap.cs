using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

public partial class TirCSharpBodyWriter
{
    private static bool IsConcreteFlatMapType(TypeExpression type)
        => TirLoopLowerer.IsRenderableConcrete(type) && !type.IsTypeParameter
            && !type.IsSelfType() && type.TypeArgs.All(IsConcreteFlatMapType);

    private bool TryWriteFlatMapConditional(TirLoweredLoop loop, string list, string part, string index, string elem)
    {
        // Do not strip coercions: their original target context can perform a conversion.
        if (loop.Fn is not TirLambda { Body: TirConditional conditional } lambda
            || lambda.Type?.Name != "Function1" || lambda.Type.TypeArgs.Count != 2
            || !TirRewrite.IsListType(conditional.Type) || conditional.Type.TypeArgs.Count != 1
            || !IsConcreteFlatMapType(loop.ElemType)
            || !loop.ElemType.Equals(conditional.Type.TypeArgs[0])
            || !conditional.Type.Equals(loop.Type)
            || !conditional.Type.Equals(lambda.Type.TypeArgs[1])
            || !conditional.Type.Equals(conditional.IfTrue.Type)
            || !conditional.Type.Equals(conditional.IfFalse.Type)
            || conditional.IfTrue is TirCoerce || conditional.IfFalse is TirCoerce)
            return false;

        bool IsSingleton(TirNode node)
            => node is TirArray { Elements.Count: 1 } array
                && loop.ElemType.Equals(array.Elements[0].Type);

        bool IsEmpty(TirNode node) => TryGetEmptyTakeReceiver(node, conditional.Type, out _);

        if (!IsSingleton(conditional.IfTrue) && !IsSingleton(conditional.IfFalse)
            && !IsEmpty(conditional.IfTrue) && !IsEmpty(conditional.IfFalse))
            return false;

        void WriteArm(TirNode arm)
        {
            WriteStartBlock();
            if (IsSingleton(arm))
            {
                // List<T>.Add supplies the same element conversion context as MakeArray<T>.
                Write($"{list}.Add(");
                WriteNode(((TirArray)arm).Elements[0]);
                WriteLine(");");
            }
            else if (TryGetEmptyTakeReceiver(arm, conditional.Type, out var receiver))
            {
                // Take(0) appends nothing. Its receiver still runs unless it provably cannot fault
                // or diverge; a discarded value keeps the selected arm's evaluation order.
                if (!IsTotalAndCheap(receiver))
                {
                    Write($"IReadOnlyList<{elem}> {part} = ");
                    WriteNode(receiver);
                    WriteLine(";");
                }
            }
            else
            {
                Write($"IReadOnlyList<{elem}> {part} = ");
                WriteNode(arm);
                WriteLine(";");
                WriteLine($"for (var {index} = 0; {index} < {part}.Count; {index}++)");
                WriteStartBlock();
                WriteLine($"{list}.Add({part}[{index}]);");
                WriteEndBlock();
            }
            WriteEndBlock();
        }

        // An arm that appends nothing and evaluates nothing leaves no statement: its branch is dropped
        // (the condition still runs).
        bool AppendsNothing(TirNode arm)
            => TryGetEmptyTakeReceiver(arm, conditional.Type, out var receiver) && IsTotalAndCheap(receiver);

        var skipTrue = AppendsNothing(conditional.IfTrue);
        var skipFalse = AppendsNothing(conditional.IfFalse);
        _lambdaDepth++;
        Write(skipTrue && !skipFalse ? "if (!(bool)(" : "if ((bool)(");
        WriteNode(conditional.Condition);
        WriteLine("))");
        WriteArm(skipTrue && !skipFalse ? conditional.IfFalse : conditional.IfTrue);
        if (!skipTrue && !skipFalse)
        {
            WriteLine("else");
            WriteArm(conditional.IfFalse);
        }
        _lambdaDepth--;
        return true;
    }

    // The receiver of a call to the standard Primitives.Take whose count is the literal Integer 0.
    // The call must be the arm itself (no coercion around it), have the callback's result type, and
    // take a receiver of that same type, so the receiver can be bound as that array type.
    private bool TryGetEmptyTakeReceiver(TirNode arm, TypeExpression resultType, out TirNode receiver)
    {
        receiver = null;
        if (arm is not TirCall { Name: "Take", Args.Count: 2 } call
            || !IsStandardTake(call.Callee)
            || call.Args[1] is not TirLiteral { LiteralType: Ara3D.Geometry.AST.LiteralTypesEnum.Integer } zero
            || zero.Type?.Name != "Integer" || !(zero.Value is 0 or 0L)
            || !resultType.Equals(call.Type) || !resultType.Equals(call.Args[0].Type))
            return false;
        receiver = call.Args[0];
        return true;
    }

    // The library function declared as Take in the stdlib's Primitives library, not a same-named one.
    private bool IsStandardTake(FunctionDef function)
        => function is { Name: "Take", FunctionType: FunctionType.Library, Body: not null, NumParameters: 2 }
           && _tw.Writer.Compilation.LibraryDefinitionsByName.TryGetValue("Primitives", out var library)
           && ReferenceEquals(function.OwnerType, library)
           && library.Functions.Any(candidate => ReferenceEquals(candidate, function));

    // Evaluating the node can neither fault nor fail to terminate, and costs a few field reads and
    // constructions at most: a parameter, local or literal, an array literal of such, a field read of
    // such, or a constructor call of such. Anything else (a call to a function value, a library
    // function, a conditional, a lambda) may fault, so a skipped Take(0) must still evaluate it.
    private static bool IsTotalAndCheap(TirNode node)
        => node switch
        {
            TirParameter or TirVariable or TirLiteral or TirTypeRef or TirDefault or TirTempRef => true,
            TirArray array => array.Elements.All(IsTotalAndCheap),
            TirNew created => created.Args.All(IsTotalAndCheap),
            TirConstructorCall created => created.Args.All(IsTotalAndCheap),
            TirComponentAccess access => IsTotalAndCheap(access.Receiver),
            TirCoerce coerce => IsPlainConversion(coerce.ConversionFn) && IsTotalAndCheap(coerce.Inner),
            TirCall { Callee: { Body: null, FunctionType: FunctionType.Constructor or FunctionType.Field } } call
                => call.Args.All(IsTotalAndCheap),
            _ => false,
        };

    // No conversion function, or a bodyless constructor or cast: a conversion with a Plato body is a call.
    private static bool IsPlainConversion(IFunction conversion)
        => conversion is null or FunctionDef { Body: null, FunctionType: FunctionType.Constructor or FunctionType.Cast };
}
