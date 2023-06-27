﻿using Parakeet.Tests;
using Parakeet;
using Parakeet.Demos;

namespace PlatoTests
{
    public class TestMath3D
    {
        public static CSharpGrammar CSharpGrammar = new CSharpGrammar();


        [Test]
        public static void ParseFuncs()
        {
            var file = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\math.funcs.plato.cs";
            var input = ParserInput.FromFile(file);
            Assert.AreEqual(1, ParserTests.ParseTest(input, CSharpGrammar.File, false));
        }
    }
}
    