using Vim.Math3d;

namespace Sandbox;

class Program
{
    static void Main(string[] args)
    {
        var min = 10;
        var max = 100;
        Console.WriteLine(23.Clamp(min, max)); // 23
        Console.WriteLine(-2.Clamp(min, max)); // 10
        Console.WriteLine(999.Clamp(min, max)); // 100
    }
}