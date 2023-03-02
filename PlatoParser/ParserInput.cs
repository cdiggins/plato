using System.Collections.Generic;

namespace PlatoParser
{
    public class ParserInput
    {
        public string Input { get; }

        public IReadOnlyList<int> LineToChar { get; }

        public IReadOnlyList<int> CharToLine { get; }

        public ParserInput(string s)
        {
            Input = s;
            var curLine = 0;
            var lineToChar = new List<int>();
            var charToLine = new List<int>();
            for (var i=0; i < s.Length; i++)
            {
                charToLine.Add(curLine);
                if (s[i] == '\n')
                {
                    curLine++;
                    lineToChar.Add(i + 1);
                }
            }
            LineToChar = lineToChar;
            CharToLine = charToLine;
        }

        public int Length { get; }

        public char this[int index] => Input[index];

        public int GetLineLength(int lineIndex)
            => lineIndex >= LineToChar.Count
                ? Input.Length - LineToChar[lineIndex]
                : LineToChar[lineIndex+1] - 1 - LineToChar[lineIndex];

        public string GetLine(int lineIndex)
            => Input.Substring(LineToChar[lineIndex], GetLineLength(lineIndex));

        public int GetLineBegin(int charIndex)
            => LineToChar[CharToLine[charIndex]];

        public int GetLineIndex(int charIndex)
            => CharToLine[charIndex];

        public int GetColumn(int charIndex)
            => charIndex - GetLineBegin(charIndex);

        public string GetCaretPos(int charIndex)
            => new string(' ', GetColumn(charIndex)) + '^';

        public static implicit operator ParserInput(string s)
            => new ParserInput(s);
    }
}