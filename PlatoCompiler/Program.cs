using PlatoAnalyzer;
using Ptarmigan.Utils.Roslyn;

var inputFolder = @"C:\\GitHub\\platosoft\\plato\\PlatoStandardLibrary\\";

var typesFile = Path.Combine(inputFolder, "math.types.plato.cs");
    var inputFiles = new[]
{
    typesFile
};

var outputFile = Path.Combine(inputFolder, "math.types.plato.g.cs");
var compilation = Compilation.Create(inputFiles);
PlatoToCSharp.OutputTypes(compilation.Compiler, outputFile);

var inputFiles2 = new[]
{
    typesFile,
    outputFile,
    Path.Combine(inputFolder, "math.funcs.plato.cs"),
};

var dllPath = @"C:\GitHub\platosoft\plato\PlatoCollections\bin\Debug\netstandard2.0\Plato.Collections.dll";

var outputFile2 = Path.Combine(inputFolder, "math.funcs.plato.g.cs");
var compilation2 = Compilation.Create(inputFiles2);
compilation2 = compilation2.AddReferences(new[] { dllPath });

PlatoToCSharp.OutputOperations(compilation2.Compiler, outputFile2);
