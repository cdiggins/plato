using System.Threading;

namespace Plato
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var dir = @"C:\dev\repos\vim2020\src\Vim.Desktop.Labs\Vim.Desktop.Labs.Scripts";
            var watcher = new DirectoryCompiler(dir, null);
            while (true)
            {
                Thread.Sleep(500);
            }
        }
    }
}  