using System.Diagnostics;
using System.Text;
using Parakeet;
using Parakeet.Demos;
using Parakeet.Demos.Plato;
using PlatoAst;
using static System.Windows.Forms.AxHost;

namespace PlatoWinFormsEditor;

public class IDE
{
    public string Input { get; set; }
    public string Output { get; set; }
    public Compilation Compilation { get; set; }

    public string ParseTree => Try(() => Compilation?.ParseTree?.ToString());
    public string CstXml => Try(() => Compilation?.CstTree?.ToXml().ToString());
    public string AstXml => Try(() => Compilation?.AstTree?.ToXml());
    public string CSharpAst => Try(() => Compilation?.AstTree?.ToCSharp());
    public string JavaScriptAst => Try(() => Compilation?.AstTree?.ToJavaScript());

    public IDE()
    {
        var inputFile1 = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\concepts.plato";
        var inputFile2 = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\types.plato";
        var inputFile3 = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\modules.plato";
        var input1 = File.ReadAllText(inputFile1);
        var input2 = File.ReadAllText(inputFile2);
        var input3 = File.ReadAllText(inputFile3);
        Input = input1 + Environment.NewLine + input2 + Environment.NewLine + input3;
        Compilation = Compile(Input);
    }

    public string Try(Func<string?> f)
    {
        try
        {
            return f() ?? "";
        }
        catch (Exception e)
        {
            return e.Message;
        };
    }

    public Compilation Compile(string input)
    {
        var c = new Compilation(input, PlatoGrammar.Instance.File, 
            CstNodeFactory.Create, AstFromPlatoCst.Convert);

        var outputBuilder = new StringBuilder();

        outputBuilder.AppendLine(c.Success ? "Compilation Success" : "Compilation Failure");

        var state = c.State;
        if (state != null)
        {
            var curError = 0;
            for (var e = state.LastError; e != null; e = e.Previous)
            {
                outputBuilder.AppendLine($"Error {curError} at {e.LastState}");
                outputBuilder.AppendLine($"failed expected rule {e.Expected}, parent state is {e.ParentState}, message is {e.Message}");
                outputBuilder.AppendLine(e.LastState.CurrentLine);
                outputBuilder.AppendLine(e.LastState.Indicator);
            }

            outputBuilder.AppendLine(c.Message);

            if (!state.AtEnd())
            {
                outputBuilder.AppendLine("Parsing did not reach the end");
            }
        }
        else
        {
            outputBuilder.AppendLine("Parsing failed");
        }

        Output = outputBuilder.ToString();
        return c;
    }
}