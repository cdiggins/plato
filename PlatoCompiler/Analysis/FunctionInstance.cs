using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ara3D.Utils;
using System.Linq;
using Plato.AST;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.Compiler.Analysis
{
    public enum FunctionInstanceKind
    {
        InterfaceDeclared,
        InterfaceImplemented,
        InterfaceExtension,
        ConcreteType,
        FieldType,
        Constant,
        Lambda,
    }

    /// <summary>
    /// Represents an instance of a function. Used for analyzing library functions and generating code.
    /// </summary>
    public class FunctionInstance
    {
        public FunctionDef Implementation { get; }
        public TypeSubstitutions Substitutions { get; private set; }
        public TypeDef Interface { get; }
        public ConcreteType ConcreteType { get; }
        public TypeExpression ConcreteTypeExpression { get; }
        public string InterfaceName => Interface?.Name ?? "";
        public IReadOnlyList<string> ParameterNames { get; }
        public string Name => Implementation.Name;
        public TypeInstance ReturnType { get; }
        public string SignatureId => $"{Name}({ParameterTypes.JoinStringsWithComma()}):{ReturnType}";
        public IReadOnlyList<TypeInstance> ParameterTypes { get; }
        public override string ToString() => $"{SignatureId}:{ReturnType}";
        public bool IsImplicitCast => Name == ReturnType.Name && ParameterNames.Count == 1 && !ReturnType.Def.IsInterface();
        public InterfaceImplementation InterfaceImplementation { get; }
        public FunctionInstanceKind Kind { get; }
        public FunctionTypeVariableAnalysis TypeVariableAnalysis { get; }
        public IReadOnlyList<string> TypeVariables => TypeVariableAnalysis.GetTypeVariableNames();
        public IReadOnlyList<ConstrainedTypeVariable> ConstrainedTypeVariables => TypeVariableAnalysis.TypeVariables;
        public bool IsInterfaceDeclaration => Kind == FunctionInstanceKind.InterfaceDeclared;
        public bool IsInterfaceExtension => Kind == FunctionInstanceKind.InterfaceExtension;
            
        public FunctionInstance(FunctionDef implementation, ConcreteType type, InterfaceImplementation ii, FunctionInstanceKind kind, TypeSubstitutions substitutions = null)
        {
            Implementation = implementation;
            ConcreteType = type;
            ConcreteTypeExpression = type?.TypeDef.ToTypeExpression();
            InterfaceImplementation = ii;
            Kind = kind;

            Substitutions = substitutions;
            ParameterNames = Implementation.Parameters.Select(p => p.Name).ToList();

            // - Replace "Self" type with the concrete type (if provided)
            // - If the first type is an interface, replace it, and every example of it, with the concrete type ... 
            //      - If it has generic arguments containing type-variables, replacing with the appropriate type from the implementing class .
            //      - That might require consulting the inherited interface.
            //      - However if the inherited interface has a type-parameter and maps to something else we have to consider that as well (I assume I can using the TypeSubstitutions)
            // - If the first type is a type-variable, replace it with the concrete type. 
            // - If the first type is a concrete type with generics then we have to do something similar ... there might be type-variables to replace.
            // - If we have other interfaces, they need to become type-variables as well. 
            //
            // What is missing:
            // - finding out what interface is implementing the function. 
            // - An effective and reliable way to do type substitutions.
            // - The interfaceImplementation class does not work the way we would like, or in a reliable way. I expected it to be actually tell me where this function was coming from. 
            //
            // Questions:
            // - Where do TypeSubstitutions come from?
            // - Are TypeSubstitutions reliable?
            // - How can I improve the TypeSubstitutions class, so that it is more than just a naive "name" lookup. 
            //   - OR is a naive name lookup all that is needed?
            // - Should I really have a separate class for Type Variable analysis?
            // - Can I simplify TypeSubstitutions? 
            // - Do I have too many FunctionInstanceKind kinds?
            // - Should there really be any type Substitions performed outside of this class? 
            //      - MAYBE: everything really should be here.
            // - Aren't functions the most important thing?
            // 
            // Thoughts:
            // - What I notice is that sometimes functions should be providing an interface implementation, but they aren't. 
            // - The whole "Interface Implementation" seems accurate. 
            // - The TypeSubsititions seems accurate. 
            // - The whole "interface implementation" can be a bit confusing. 
            // One of the question marks is related to "type substitutions" and when they should be applied. 
            // It might make sense to apply them first. 
            // I think the FIRST thing to do is. 
            // 2) Figure out if there are Interface substitutions that need to be made.
            // 3) Figure out if there are SelfType substitutions that need to be made.
            // 4) Perform the substitutions.
            // 5) Analyze the function for remaining type variables.
            // 6) Consider that interfaces might need to converted to type variables (if not in the first position) 
            // 7) Create the final version of the function types  
            // We are matching a function to particular concrete type, or interface implementation, or neither. 


            // The first thing we do is figure out if there are TypeVariable substitutions that need to be made.
            var first = Implementation.Parameters.FirstOrDefault();
            if (first != null)
            {
                if (first.Type.Def.IsInterface())
                {
                    if (Kind == FunctionInstanceKind.InterfaceDeclared || Kind == FunctionInstanceKind.InterfaceExtension)
                    {
                        // Nothing happens.
                        Debug.Assert(ii == null);
                        Debug.Assert(ConcreteType == null);
                    }
                    else if (Kind == FunctionInstanceKind.InterfaceImplemented)
                    {
                        Debug.Assert(ii != null, "We expect there to be a valid interface implementation");
                        Debug.Assert(ConcreteType != null, "We expect there to be a concrete type");
                        GatherTypeVariableSubstitutions(first.Type, ii.TypeExpression);
                    }
                    else
                    {
                        throw new NotImplementedException("We have absolutely no idea what to do");
                    }
                }
                else if (first.Type.IsSelfType())
                {
                    Debug.Assert(first.Type.Def is SelfType);
                    if (!IsInterfaceDeclaration)
                    {
                        // There are no Self substitutions if this is just an interface declaration.
                        Debug.Assert(Substitutions != null);
                        var tmp = Substitutions?.Replace(first.Type);
                        Debug.Assert(tmp != null);
                        Debug.Assert(tmp.Def.Equals(ConcreteType.TypeDef));
                    }
                }
                else if (first.Type.Def.IsTypeVariable())
                {
                    throw new Exception("We never expect the first type to be a type-variable");
                    //GatherTypeVariableSubstitutions(first.Type, concreteTypeExpr);
                }
                else if (first.Type.Def.IsConcrete() || first.Type.Def.IsPrimitive())
                {
                    // NOTE: just do nothing. I don't think there are type-variables to replace.
                    Debug.Assert(CountTypeVars(first.Type) == 0);
                }
                else 
                {
                    throw new Exception($"Unexpected first type {first.Type} of kind {first.Type.Def.Kind}");
                }
            }

            // Create an initial version of the return type, and parameter types
            ReturnType = ToInstance(Implementation.ReturnType);
            ParameterTypes = Implementation.Parameters.Select(p => ToInstance(p.Type)).ToList();

            //if (SignatureId == "Linear(Number, INumerical, INumerical):INumerical") Debugger.Break();
            //if (Name == "WithNext") Debugger.Break();
            //if (Name == "FlatMap") Debugger.Break();

            // Generate and gather the type variables 
            TypeVariableAnalysis = new FunctionTypeVariableAnalysis(ParameterTypes, ReturnType, !IsInterfaceDeclaration && !IsInterfaceExtension);

            if (ParameterTypes.Count <= 1)
            {
                if (TypeVariableAnalysis.TypeVariables.Count > 0 && Kind != FunctionInstanceKind.InterfaceExtension)
                    throw new Exception("We expect there to be no type variables in functions with only one or less parameters");
            }

            // Create the final version of the return type, and parameter types
            ReturnType = TypeVariableAnalysis.ReturnType;
            ParameterTypes = TypeVariableAnalysis.ParameterTypes;

            /*
            if (ReturnType.ContainsRawTypeVariable())
                throw new Exception($"Return type contains an unreplaced type variable");
            if (ParameterTypes.Any(t => t.ContainsRawTypeVariable()))
                throw new Exception($"Parameters types contains an unreplaced type variable");
            */
        }

        public void GatherTypeVariableSubstitutions(TypeExpression original, TypeExpression replace)
        {
            if (original.Name.StartsWith("$"))
            {
                Substitutions = new TypeSubstitutions(original.Name, replace, Substitutions);
                Debug.Assert(original.TypeArgs.Count == 0);
                return;
            }
            Debug.Assert(original.TypeArgs.Count == replace.TypeArgs.Count);
            for (var i = 0; i < original.TypeArgs.Count; ++i)
            {
                GatherTypeVariableSubstitutions(original.TypeArgs[i], replace.TypeArgs[i]);
            }
        }

        public TypeInstance ToInstance(TypeExpression expr)
        {
            // Repeat until we don't change anymore. 
            while (true)
            {
                var r = Substitutions?.Replace(expr) ?? expr;

                if (ReferenceEquals(r, expr)) 
                    return new TypeInstance(r, r.TypeArgs.Select(ToInstance));

                expr = r;
            }
        }

        public static int CountTypeVars(TypeExpression te) 
            => te.IsTypeVariable ? 1 : te.TypeArgs.Sum(CountTypeVars);
    }
}