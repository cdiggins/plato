using PlatoAnalyzer;
using Ptarmigan.Utils;
using Ptarmigan.Utils.Roslyn;

//var rootFolder = @"C:\GitHub\platosoft\plato";
var rootFolder = @"C:\Users\cdigg\git\plato";

var inputFolder = Path.Combine(rootFolder, "PlatoStandardLibrary");

void CompileTypesAndFuncs()
{
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

    // TODO:
    var dllPath = Path.Combine(rootFolder, "PlatoCollections", "bin", "Debug", "netstandard2.0", "Plato.Collections.dll");

    var outputFile2 = Path.Combine(inputFolder, "math.funcs.plato.g.cs");
    var compilation2 = Compilation.Create(inputFiles2);
    compilation2 = compilation2.AddReferences(new[] { dllPath });

    PlatoToCSharp.OutputOperations(compilation2.Compiler, outputFile2);
}

void CompileArrays()
{
    var inputFile = Path.Combine(inputFolder, "array.plato.cs");
    var inputFiles = new[] { inputFile };

    var outputFile = Path.ChangeExtension(inputFile, ".g.cs");
    var compilation = Compilation.Create(inputFiles);

    PlatoToCSharp.OutputTypes(compilation.Compiler, outputFile);

    // TODO: this needs to output something else.
    // I want to get the functions out. And do the inference.
    // The first test is just 
}

void HandleException(Exception exception)
{
    if (exception is PlatoException pe)
    {
        Console.WriteLine($"Plato error: {pe.Message}");
        if (pe.Symbol != null)
        {
            Console.WriteLine($"At symbol: {pe.Symbol.Kind}");
            var synRef = pe.Symbol.DeclaringSyntaxReferences.FirstOrDefault();
            if (synRef != null)
            {
                // TODO: maybe I want a way to output general information
                Console.WriteLine($"Defined at syntax reference {synRef}");
            }
        }
        var node = pe.Node;
        if (node != null)
        {
            
            Console.WriteLine($"At node: {pe.Node}");
            var content = pe.Node.Span.ToString();
            Console.WriteLine(content);
            var tree = pe.Node.SyntaxTree;
            if (tree != null)
            {
                var pos = tree.GetLineSpan(pe.Node.Span);
                Console.WriteLine($"In file {tree.FilePath} from line # {pos.StartLinePosition.Line} to line # {pos.EndLinePosition.Line}");
            }
        }
    }
    else
    {
        Console.WriteLine($"Unrecognized exception: {exception}");
    }
}

try
{
    CompileArrays();
}
catch (Exception e)
{
    HandleException(e);
}

