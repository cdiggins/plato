using System.Collections.Generic;

namespace PlatoParser
{
    /// <summary>
    /// Wraps the input string to a parser, providing utility
    /// functions find the column and row (line) from a character index.
    /// </summary>
    public class ParserInput
    {
        public string File { get; }
        public string Text { get; }

        public IReadOnlyList<int> LineToChar { get; }

        public IReadOnlyList<int> CharToLine { get; }

        public ParserInput(string s, string file = "")
        {
            File = file;
            Text = s;
            var curLine = 0;
            var lineToChar = new List<int>();
            var charToLine = new List<int>();
            lineToChar.Add(0);
            for (var i=0; i < s.Length; i++)
            {
                charToLine.Add(curLine);
                if (s[i] == '\n')
                {
                    curLine++;
                    if (i < s.Length - 1)
                        lineToChar.Add(i + 1);
                }
            }
            charToLine.Add(curLine);
            LineToChar = lineToChar;
            CharToLine = charToLine;
        }

        public int Length => Text.Length;

        public char this[int index] => Text[index];

        public int GetLineLength(int lineIndex)
            => lineIndex >= LineToChar.Count - 1
                ? Text.Length - LineToChar[lineIndex]
                : LineToChar[lineIndex+1] - 1 - LineToChar[lineIndex];

        public string GetLine(int lineIndex)
            => Text.Substring(LineToChar[lineIndex], GetLineLength(lineIndex));

        public int GetLineBegin(int charIndex)
            => LineToChar[CharToLine[charIndex]];

        public int GetLineIndex(int charIndex)
            => CharToLine[charIndex];

        public int GetColumn(int charIndex)
            => charIndex - GetLineBegin(charIndex);

        public string GetIndicator(int charIndex)
            => new string(' ', GetColumn(charIndex)) + '^';

        public static implicit operator ParserInput(string s)
            => new ParserInput(s);

        public static ParserInput FromFile(string filePath)
            => new ParserInput(System.IO.File.ReadAllText(filePath), filePath);
    }
}