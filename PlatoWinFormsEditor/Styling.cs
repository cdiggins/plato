namespace PlatoWinFormsEditor;

public class Styling
{
    public Dictionary<string, Style> Styles = new()
    {
        //{ "Identifier", new(Color.Blue) },
        { "Literal", new(Color.Green) },
        { "Separator", new(Color.BlueViolet) },
        { "Comment", new(Color.DarkGray, false, true) },
        { "HexLiteral", new(Color.Chocolate) },
        { "BinaryLiteral", new(Color.Chocolate) },
        { "FloatLiteral", new(Color.Chocolate) },
        { "Operator", new(Color.CornflowerBlue) },
        { "IntegerLiteral", new(Color.DarkRed) },
        { "StringLiteral", new(Color.GreenYellow) },
        { "CharLiteral", new(Color.GreenYellow) },
        { "BooleanLiteral", new(Color.DodgerBlue) },
        { "NullLiteral", new(Color.DodgerBlue) },
        { "Unknown", new(Color.DarkOrange) },
        { "TypeKeyword", new(Color.MediumVioletRed, true) },
        { "StatementKeyword", new(Color.MediumAquamarine, true) },
        { "ParameterName", new(Color.BlueViolet) },
        { "TypeName", new(Color.DarkOliveGreen)},
        { "FunctionName", new(Color.DarkCyan) },
        { "FieldName", new(Color.DarkSeaGreen) },
        { "ERROR", new (Color.Crimson, true, false, true )}
    };
}