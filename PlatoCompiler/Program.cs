using PlatoAnalyzer;
using Ptarmigan.Utils.Roslyn;

var inputFolder = @"C:\\GitHub\\platosoft\\plato\\PlatoStandardLibrary\\"; 

var inputFiles = new[]
{
    Path.Combine(inputFolder, "math.types.plato.cs"),
};

var outputFile = Path.Combine(inputFolder, "math.types.plato.g.cs");

//var inputFiles = args;
var compilation = Compilation.Create(inputFiles);

PlatoToCSharp.Output(compilation.Compiler, outputFile);
