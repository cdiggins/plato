using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;

namespace Ara3D.Geometry.Compiler.Types
{
    public class FunctionAnalysis
    {
        public FunctionAnalysis(Compilation compilation, FunctionDef def)
        {
            Compilation = compilation;
            Def = def;
            TypeResolver = new TypeResolver(this);
        }

        public TypeResolver TypeResolver { get; }

        public List<TypeVariable> TypeVariables { get; } 
            = new List<TypeVariable>();

        public Compilation Compilation { get; }
        public FunctionDef Def { get; }
        
        public TypeDef OwnerType 
            => Def.OwnerType;
        
        public bool IsConceptFunction 
            => OwnerType.IsInterface();

        public bool IsLibraryFunction
            => OwnerType.IsLibrary();

        public bool IsGenericLibraryFunction 
            => IsLibraryFunction && TypeVariables.Count > 0;

        public bool IsConceptExtension 
            => IsLibraryFunction 
                && Def.Parameters.Count > 0 
                && Def.Parameters[0].Type.Def.IsInterface();

        public TypeExpression DeclaredReturnType
            => Def.ReturnType;

        public string ParametersString()
            => string.Join(", ", Def.Parameters.JoinStringsWithComma());

        public string Signature
            => $"{Def.OwnerType}.{Def.Name}({ParametersString()}): {DeclaredReturnType}";
    }
}