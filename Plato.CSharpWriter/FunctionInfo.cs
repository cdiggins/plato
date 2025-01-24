using System.Collections.Generic;
using System.Linq;
using Ara3D.Utils;
using Plato.AST;
using Plato.Compiler.Analysis;
using Plato.Compiler.Symbols;

namespace Plato.CSharpWriter
{
    public class FunctionInfo
    {
        public FunctionInfo(string name,
            ConcreteType owner, // Could be null 
            string returnType,
            IEnumerable<string> genericParameters,
            IEnumerable<string> parameterNames,
            IEnumerable<string> parameterTypes,
            Symbol body,
            bool isImplicit)
        {
            Name = name;
            ConcreteType = owner;
            ReturnType = returnType;
            Generics = genericParameters.ToList();
            ParameterNames = parameterNames.ToList();
            ParameterTypes = parameterTypes.ToList();
            Body = body;
            IsImplicit = isImplicit;
            if (ParameterNames.Count != ParameterTypes.Count)
                throw new System.Exception("Parameter names and types must have the same length");
        }

        public string Name { get; }
        public string ReturnType { get; }
        public ConcreteType ConcreteType { get; }
        public Symbol Body { get; }
        public bool IsImplicit { get; }
        public IReadOnlyList<string> Generics { get; }
        public int NumParameters => ParameterNames.Count;
        public IReadOnlyList<string> ParameterNames { get; }
        public string FirstParameterName => ParameterNames[0];
        public IReadOnlyList<string> ParameterTypes { get; }
        public IEnumerable<string> MethodParameterNames => ParameterNames.Skip(1);
        public IEnumerable<string> MethodParameterTypes => ParameterTypes.Skip(1);
        public IEnumerable<string> Parameters => ParameterNames.Zip(ParameterTypes, (n, t) => $"{t} {n}");
        public IEnumerable<string> MethodParameters => MethodParameterNames.Zip(MethodParameterTypes, (n, t) => $"{t} {n}");
        public string GenericsString => Generics.Count > 0 ? $"<{Generics.JoinStringsWithComma()}>" : "";
        public string StaticParametersString => NumParameters > 0 ? $"({Parameters.JoinStringsWithComma()})" : "";
        public string ExtensionParametersString => NumParameters > 0 ? $"(this {Parameters.JoinStringsWithComma()})" : "";
        public string MethodParametersString => NumParameters > 1 ? $"({MethodParameters.JoinStringsWithComma()})" : "";
        public string StaticSignature => $"public static {ReturnType} {Name}{GenericsString}{StaticParametersString}";
        public string ExtensionSignature => $"public static {ReturnType} {Name}{GenericsString}{ExtensionParametersString}";
        
        public bool IsStatic => ParameterNames.Count == 0 || ParameterNames[0] == "_";
        public string StaticKeyword => IsStatic ? "static " : "";
        public bool IsProperty => ParameterNames.Count <= 1;
        public static string Annotation => "[MethodImpl(AggressiveInlining)] ";
        public string FunctionAnnotation => IsProperty ? "" : $"{Annotation} "; 
        public string MethodSignature => $"{FunctionAnnotation}public {StaticKeyword}{ReturnType} {Name}{GenericsString}{MethodParametersString}";
        public string StaticArgsString => NumParameters > 0 ? $"({ParameterNames.JoinStringsWithComma()})" : "";
        public string MethodArgsString => NumParameters > 1 ? $"({ParameterNames.Skip(1).JoinStringsWithComma()})" : "";
        public string IntrinsicsArgsString => NumParameters > 0 ? $"({ParameterNames.Skip(1).Prepend("this").JoinStringsWithComma()})" : "";
        public bool IsMethodOf(string typeName) => ParameterTypes.Count > 0 && ParameterTypes[0] == typeName;

        public bool IsMember => ConcreteType != null;

        public string OperatorName
        {
            get
            {
                var op = ParameterNames.Count == 1 ? Operators.NameToUnaryOperator(Name) :
                    ParameterNames.Count == 2 ? Operators.NameToBinaryOperator(Name) : null;
                // NOTE: this is because in C#, the "||" and "&&" operators cannot be overridden.
                if (op == "&&") op = "&";
                if (op == "||") op = "|";
                return op;
            }
        }

        public bool IsIndexer => Name == "At";
        public string IndexerSig => $"public {ReturnType} this[{MethodParameters.JoinStringsWithComma()}]";
        public string IndexerImpl => $"{IndexerSig} {{ {Annotation} get => {Name}{MethodArgsString}; }}";
        public string IndexerInterface => $"{ReturnType} this[{MethodParameters.JoinStringsWithComma()}] {{ get; }}";

        public string ImplicitImpl => $"{Annotation} public static implicit operator {ReturnType}{StaticParametersString} => {FirstParameterName}.{Name}{MethodArgsString};";

        public bool IsOperator => OperatorName != null;
        public string OperatorImpl => $"{Annotation} public static {ReturnType} operator {OperatorName}{StaticParametersString} => {FirstParameterName}.{Name}{MethodArgsString};";

        public string MethodInterface => $"{ReturnType} {Name}{MethodParametersString}" + (NumParameters > 1 ? ";" : " { get; }");

    }
}