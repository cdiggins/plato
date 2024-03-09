namespace PlatoWinFormsEditor;

public class Style
{
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public Color Color { get; set; }

    public Style(Color color, bool bold = false, bool italic = false, bool underline = false)
    {
        Color = color;
        Bold = bold;
        Italic = italic;
        Underline = underline;
    }

    public FontStyle ToFontStyle()
    {
        var r = FontStyle.Regular;
        if (Bold) r |= FontStyle.Bold;
        if (Italic) r |= FontStyle.Italic;
        if (Underline) r |= FontStyle.Underline;
        return r;
    }

    public bool IsRegular()
        => ToFontStyle() == FontStyle.Regular;

    public Font ToFont(Font font)
        => new(font, ToFontStyle());
}