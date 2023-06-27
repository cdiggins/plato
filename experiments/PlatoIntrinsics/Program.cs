using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PlatoIntrinsics
{
    public static class Program
    {
        public static void Main()
        {
            var cb = Generator.OutputOperators();
            Console.WriteLine(cb.ToString());
        }
    }
}
