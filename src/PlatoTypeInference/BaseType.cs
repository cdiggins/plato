using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoTypeInference
{
    public abstract class BaseType
    {
        public abstract string Name { get; }
        public override string ToString() => Name;

        private static int TypeListEnd(string s)
        {
            int nesting = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '[')
                    nesting++;
                if (s[i] == ']')
                    if (--nesting == 0)
                        return i;
            }

            throw new Exception("End of type list not found");
        }

        public static BaseType Parse(string s)
        {
            if (s.StartsWith("$"))
                return new TypeVariable(s);

            if (s.StartsWith("\\"))
            {
                if (s[1] != '(') 
                    throw new Exception("Expected '('");
                var n = s.IndexOf(')');
                if (n == -1) 
                    throw new Exception("Could not find ')'");
                var tmp = s.Substring(2, n - 2);
                var varNames = tmp.Split(',');
                var typeVars = varNames.Select(Parse).Cast<TypeVariable>();
                var typeList = (TypeList)Parse(s.Substring(n + 1));
                return new PolyType(typeVars, typeList);
            }

            if (s.StartsWith("["))
            {
                if (!s.EndsWith("]"))
                    throw new Exception("Could not find ']'");
                s = s.Substring(1, s.Length - 2);
                var r = new List<BaseType>();
                while (s.Length > 0)
                {
                    if (s.StartsWith("[") || s.StartsWith("\\"))
                    {
                        var end = TypeListEnd(s);
                        var tmp = s.Substring(0, end + 1);
                        r.Add(Parse(tmp));
                        if ((end < s.Length - 1) && (s[end + 1] == ','))
                            end += 1;
                        s = s.Substring(end + 1);
                    }
                    else
                    {
                        var n = s.IndexOf(',');
                        var tmp = s.Substring(0, n < 0 ? s.Length : n);
                        r.Add(Parse(tmp));
                        if (n < 0)
                            break;
                        s = s.Substring(n + 1);
                    }
                }
                return new TypeList(r);
            }

            return new TypeConstant(s);
        }

        public override bool Equals(object obj) 
            => ReferenceEquals(this, obj) || ToString() == obj?.ToString();

        public override int GetHashCode() 
            => ToString().GetHashCode();

        public static implicit operator BaseType(string s)
            => Parse(s);
    }
}