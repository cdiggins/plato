
public enum TokenType
{
    Unknown,
    Identifier,
    Number,
    //String,
    //Char,
    Operator,
    Separator,
    WhiteSpace,
}

public static class Tokenizer
{
    public class Token
    {
        public TokenType Type { get; }
        public int Start { get; }
        public int End { get; }

        public Token(TokenType type, int start, int end)
            => (Type, Start, End) = (type, start, end);
    }

    public static bool IsIdentifierChar(char c)
        => char.IsLetterOrDigit(c) || c == '_';

    public static bool IsLetter(char c)
        => char.IsLetter(c);

    public static bool IsDigit(char c)
        => char.IsDigit(c);

    public static bool IsWhiteSpace(char c)
        => char.IsWhiteSpace(c);

    public static bool IsOperator(char c)
        => ":.!@#$%^~&*-+=<>/?".Contains(c);

    public static bool IsSeparator(char c)
        => "{}[]();,".Contains(c);

    public static Token AdvanceWhile(TokenType type, Func<char, bool> func, string input, int start, int pos)
    {
        while (pos < input.Length && func(input[pos]))
            pos++;
        return new Token(type, start, pos);
    }

    public static Token GetToken(string input, int pos)
    {
        var start = pos;
        var c = input[pos++];
        if (IsWhiteSpace(c))
            return AdvanceWhile(TokenType.WhiteSpace, IsWhiteSpace, input, start, pos);
        if (IsIdentifierChar(c))
            return AdvanceWhile(TokenType.Identifier, IsIdentifierChar, input, start, pos);
        if (IsDigit(c))
            return AdvanceWhile(TokenType.Identifier, IsIdentifierChar, input, start, pos);
        if (IsOperator(c))
            return AdvanceWhile(TokenType.Operator, IsOperator, input, start, pos);
        if (IsSeparator(c))
            return new Token(TokenType.Separator, start, pos);
        return new Token(TokenType.Unknown, start, pos);
    }
}
