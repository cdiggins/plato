using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;

namespace PlatoAnalyzer
{
    /// <summary>
    /// Lambda with Block, Lambda Expression, Method, Local Function, 
    /// </summary>
    public class MethodSymbolAnalysis
    {
        public IMethodSymbol Symbol;
        public string Name => Symbol.Name;

        public ITypeSymbol ReceiverType => Symbol.ReceiverType;
        public ITypeSymbol ReturnType => Symbol.ReturnType;
        public ImmutableArray<ITypeSymbol> TypeArguments => Symbol.TypeArguments;
        public ImmutableArray<IParameterSymbol> Parameters => Symbol.Parameters;

        public bool IsValid => NotValidReasons.Count == 0;
        public List<string> NotValidReasons = new List<string>();
        public SyntaxReference SyntaxReference;

        public MethodSymbolAnalysis(IMethodSymbol method)
        {
            Symbol = method;

            try
            {
                CheckValidity(!Symbol.IsPartialDefinition, "Partial definitions not supported");
                CheckValidity(!Symbol.IsAbstract, "Abstract definitions not supported");
                CheckValidity(!Symbol.IsVirtual, "Virtual definitions not supported");
                CheckValidity(!Symbol.IsAsync, "Async functions not supported");
                CheckValidity(!Symbol.IsVararg, "Vararg functions not supported");
                CheckValidity(!Symbol.ReturnsByRef, "Return by reference functions not supported");
                CheckValidity(!Symbol.ReturnsVoid, "Void functions not supported");
                CheckValidity(!Symbol.IsExtern, "Extern functions not supported");
                CheckValidity(Symbol.CallingConvention != SignatureCallingConvention.Default, "Only default calling convention functions supported");

                CheckValidity(Symbol.MethodKind != MethodKind.DelegateInvoke, "Delegate invoke not supported");
                CheckValidity(Symbol.MethodKind != MethodKind.Destructor, "Destructors not supported");
                CheckValidity(Symbol.MethodKind != MethodKind.EventAdd, "Event adds not supported");
                CheckValidity(Symbol.MethodKind != MethodKind.EventRaise, "Event raises not supported");
                CheckValidity(Symbol.MethodKind != MethodKind.EventRemove, "Event removes not supported");
                CheckValidity(Symbol.MethodKind != MethodKind.FunctionPointerSignature, "Function pointers not supported");
                CheckValidity(Symbol.MethodKind != MethodKind.ReducedExtension, "Reduced extensions not supported");
                CheckValidity(Symbol.MethodKind != MethodKind.SharedConstructor, "Shared constructors not supported");
                CheckValidity(Symbol.MethodKind != MethodKind.StaticConstructor, "Static constructors not supported");

                CheckValidity(!Symbol.IsSealed, "Sealed functions not supported");

                CheckValidity(Symbol.ReturnsVoid, "Void functions not supported");
                CheckValidity(Symbol.ReturnsVoid, "Void functions not supported");

                SyntaxReference = Symbol.DeclaringSyntaxReferences.FirstOrDefault();
            }
            catch (Exception e)
            {
                NotValidReasons.Add(e.Message);
            }
        }

        public void CheckValidity(bool value, string reason)
        {
            if (!value)
                NotValidReasons.Add(reason);
        }
    }
}