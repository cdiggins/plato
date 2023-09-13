﻿using System.Collections.Generic;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public class ReifiedFunction : IFunction
    {
        public IReadOnlyList<TypeExpression> ParameterTypes { get; }
        
        public string GetParameterName(int n)
            => Original.GetParameterName(n);

        public TypeExpression GetParameterType(int n)
            => ParameterTypes[n];

        public TypeExpression ReturnType { get; }
        public TypeDefinition OwnerType => Original.OwnerType;
        public FunctionDefinition Original { get; }
        public ReifiedType ReifiedType { get; }
        public string Name => Original.Name;
        public int NumParameters => ParameterTypes.Count;

        public Expression Body => Original.Body;

        public ReifiedFunction(FunctionDefinition original, 
            ReifiedType reifiedType, 
            IReadOnlyList<TypeExpression> parameterTypes, 
            TypeExpression returnType)
        {
            Original = original;
            ReifiedType = reifiedType;
            ParameterTypes = parameterTypes;
            ReturnType = returnType;
        }

        public void Verify()
        {
            ReturnType.VerifyIsReified();
            foreach (var p in ParameterTypes)
                p.VerifyIsReified();
        }

        public override string ToString()
            => this.GetSignature();

        public override bool Equals(object obj) 
            => obj is ReifiedFunction rf
            && ReferenceEquals(ReifiedType, rf.ReifiedType)
            && ToString() == rf.ToString();

        public override int GetHashCode() 
            => ToString().GetHashCode();
    }
}