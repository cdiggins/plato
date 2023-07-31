using System.Linq;

namespace Plato.Compiler
{
    public class TypedFunction
    {
        public TypedFunction(TypeDefSymbol type, FunctionSymbol func)
        {
            ParentType = type;
            Function = func;
            ParameterTypes = new TypeDefSymbol[func.Parameters.Count];
            for (var i=0; i < func.Parameters.Count; ++i)
            {
                var p = func.Parameters[i];
                ParameterTypes[i]  = p.Type?.Def;
            }

            ReturnType = func.Type?.Def;
        }
        public TypeDefSymbol ParentType { get; }
        public FunctionSymbol Function { get; }
        public string Name => Function.Name;
        public TypeDefSymbol ReturnType { get; set; }
        public int ParameterCount => Function.Parameters.Count;
        public TypeDefSymbol[] ParameterTypes { get; }
        public string ParameterListString => string.Join(", ", ParameterTypes.Select(p => p?.Name ?? "?"));
        public string Id => $"{Name}({ParameterListString})";
        public override string ToString() => $"{ParentType.Type}.{Id}";
    }
}