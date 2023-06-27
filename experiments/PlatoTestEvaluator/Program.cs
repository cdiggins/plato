using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

public static class PrimitiveOperations
{
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/

    public static (string, Func<dynamic, dynamic>)[] UnaryOperators =
    {
        ("!", x => !x),
        ("-", x => -x),
        ("+", x => +x),
        ("~", x => ~x),
        ("throw", x => throw x),
    };

    public static (string, Func<dynamic, dynamic, dynamic>)[] BinaryOperators =
    {
        ("+", (x, y) => x + y),
        ("-", (x, y) => x - y),
        ("*", (x, y) => x * y),
        ("/", (x, y) => x / y),
        ("%", (x, y) => x % y),
        
        (">", (x, y) => x > y),
        ("<", (x, y) => x < y),
        (">=", (x, y) => x >= y),
        ("<=", (x, y) => x <= y),
        
        ("==", (x, y) => x == y),
        ("!=", (x, y) => x != y),

        ("&&", (x, y) => x && y),
        ("||", (x, y) => x || y),
       
        ("&", (x, y) => x & y),
        ("|", (x, y) => x | y),
        ("^", (x, y) => x ^ y),

        (">>", (x, y) => x >> y),
        ("<<", (x, y) => x << y),

        ("??", (x, y) => x ?? y),
        ("[]", (x, y) => x[y]),
        ("()", (x, y) => x(y)),
        
        (".", (x, y) => x.y),
        ("(,)", (x, y) => (x, y)),
    };

    public static (string, Func<dynamic, dynamic, dynamic, dynamic>)[] TernaryOperators =
    {
        ("?", (x, y, z) => x ? y : z),
        ("(,,)", (x, y, z) => (x,y,z)),
    };
}

public class Evaluator
{
    public Stack<dynamic>? Stack { get; }
    public Dictionary<string, dynamic?> Environment = new();
    
    public dynamic Pop() => Stack.Pop();
    public void Push(dynamic? d) => Stack.Push(d);
    public dynamic Peek() => Stack.Peek();
    
    public int QuotationDepth = 0;
    
    public void CallDelegate(Delegate d)
    {
        var args = new object[d.Method.GetParameters().Length];
        for (var i = args.Length - 1; i >= 0; i--)
        {
            args[i] = Pop();
        }
        var r = args.Length > 0 
            ? d.DynamicInvoke(args) 
            : d.DynamicInvoke();
        
        if (d.Method.ReturnType != typeof(void))
        {
            Push(r);
        }
    }

    /*
     * Dissassembler 
     * https://github.com/jbevain/cecil/blob/master/Mono.Cecil.Cil/OpCodes.cs
     * https://stackoverflow.com/questions/7212255/cecil-instruction-operand-types-corresponding-to-instruction-opcode-code-value
     * https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.operandtype?view=net-7.0
     * https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcode?view=net-7.0
     * https://www.codeproject.com/Articles/14058/Parsing-the-IL-of-a-Method-Body   
     */


    public Evaluator Eval(string token)
    {
        if (token == "[")
        {
            QuotationDepth++;
            Push(new List<string>());
        }    
        else if (token == "]")
        {
            if (QuotationDepth == 0)
                throw new Exception("Not in a quotation");
            QuotationDepth--;
        }
        else
        {
            if (QuotationDepth > 0)
            {
                var q = Peek() as List<string>;
                if (q == null) 
                    throw new Exception("Expected quotation on stack");
                q.Add(token);
            }
            else
            {
                
            }
        }

        return this;
    }
}

public static class Program
{
    public static void Main()
    {
        ConsoleKeyInfo cki;
        // Prevent example from ending if CTL+C is pressed.
        Console.TreatControlCAsInput = true;

        Console.WriteLine("Press any combination of CTL, ALT, and SHIFT, and a console key.");
        Console.WriteLine("Press the Escape (Esc) key to quit: \n");
        do
        {
            cki = Console.ReadKey();
            Console.Write(" --- You pressed ");
            if ((cki.Modifiers & ConsoleModifiers.Alt) != 0) Console.Write("ALT+");
            if ((cki.Modifiers & ConsoleModifiers.Shift) != 0) Console.Write("SHIFT+");
            if ((cki.Modifiers & ConsoleModifiers.Control) != 0) Console.Write("CTL+");
            Console.Write(cki.Key.ToString());

            Console.WriteLine($"Pos = {Console.WindowLeft}x{Console.WindowTop} Dim = {Console.WindowWidth}x{Console.WindowHeight} Cursor = {Console.CursorLeft} {Console.CursorTop}");
        } 
        while (cki.Key != ConsoleKey.Escape);
    }
}