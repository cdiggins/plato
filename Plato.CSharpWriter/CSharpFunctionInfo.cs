using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Ara3D.Utils;
using Plato.AST;
using Plato.Compiler.Analysis;
using Plato.Compiler.Symbols;

namespace Plato.CSharpWriter
{
    public class CSharpFunctionInfo : ITypeToCSharp
    {
        public CSharpFunctionInfo(FunctionInstance fi, ConcreteType owner, ITypeToCSharp typeToCSharp)
        {
            Function = fi;
            ConcreteType = owner;
            TypeToCSharp = typeToCSharp;

            ReturnType = this.ToCSharpType(fi.ReturnType);
            ParameterTypes = Function.ParameterTypes.Select(this.ToCSharpType).ToList();

            var funcTypeParams = fi.UsedTypeParameters;
            if (ConcreteType != null)
            {
                var typeParameterNames = ConcreteType.TypeDef.TypeParameters.Select(tp => tp.Name).ToHashSet();
                funcTypeParams = fi.UsedTypeParameters.Where(tp => !typeParameterNames.Contains(tp.Name)).ToList();
            }
            Generics = funcTypeParams.Select(ft => ft.Name).Distinct().ToList();

            if (ParameterNames.Count != ParameterTypes.Count)
                throw new System.Exception("Parameter names and types must have the same length");
        }

        public ITypeToCSharp TypeToCSharp { get; }
        public FunctionInstance Function { get; }
        public string Name => Function.Name;

        public string ReturnType { get; }
        public ConcreteType ConcreteType { get; }
        public Symbol Body => Function.Implementation.Body;
        public bool IsImplicit => Function.IsImplicitCast;
        public IReadOnlyList<string> Generics { get; }
        public int NumParameters => ParameterNames.Count;
        public IReadOnlyList<string> ParameterNames => Function.ParameterNames;
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

        // TODO: should we not be taking into account the function type replacements?  
        public string ToCSharpTypeName(TypeInstance ti)
            => TypeToCSharp.ToCSharpTypeName(ti);
    }
}