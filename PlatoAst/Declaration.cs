using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace PlatoAst
{
    /// <summary>
    /// This is similar to a list of all of the extension methods.
    /// Maybe an Operation should be listed differently.
    /// When looking for a function/property on a type the type is
    /// looked at first. If not found then the Operations lookup
    /// is considered. 
    /// </summary>
    public class Operations
    {
        public Dictionary<string, List<AstMethodDeclaration>> OperationsFromTypes { get; }
            = new Dictionary<string, List<AstMethodDeclaration>>();

        public void AddOperation(AstMethodDeclaration method)
        {
            if (method.Parameters.Count == 0)
            {
                Debug.WriteLine($"TODO: operation {method.Name} should not have zero parameters");
                return;
            }

            var typeName = method.Parameters[0].Type.Name;
            if (!OperationsFromTypes.ContainsKey(typeName))
            {
                OperationsFromTypes.Add(typeName, new List<AstMethodDeclaration>());
            }
            
            OperationsFromTypes[typeName].Add(method);
        }

        public static Operations Create(
            IEnumerable<AstTypeDeclaration> types)
        {
            var ops = new Operations();
            foreach (var t in types)
            {
                if (!t.HasAttribute("Operations"))
                    continue;
                foreach (var m in t.Members.OfType<AstMethodDeclaration>())
                    ops.AddOperation(m);
            }

            return ops;
        }

        public static Operations Create(IEnumerable<AstNamespace> namespaces)
        {
            return Create(namespaces.SelectMany(ns => ns.GetAllTypes()));
        }
    }   

    public class ReifiedTypes
    {
        public IReadOnlyList<AstTypeDeclaration> TypeDeclarations { get; }

        public Dictionary<string, List<ReifiedType>> Lookup { get; } 
            = new Dictionary<string, List<ReifiedType>>();

        public ReifiedType GetType(string name, params ReifiedType[] typeArguments)
        {
            throw new NotImplementedException();
        }

        public void AddType(AstTypeDeclaration type)
        {
            throw new NotImplementedException();
            // If no type parameters, it is already reified. Just add it. 
            // If type parameters then we have to wait. 
        }
    }

    public class ReifiedType : AbstractValue
    {
        public AstTypeDeclaration Declaration { get; }
        public Dictionary<string, ReifiedType> TypeLookup { get; } = new Dictionary<string, ReifiedType>();

        public ReifiedType(AstTypeDeclaration declaration, params ReifiedType[] typeArguments)
            : base(declaration, null, TypeRef.MetaType, declaration.Name.Text) 
        {
            Declaration = declaration;
            if (typeArguments.Length != Declaration.TypeParameters.Count)
                throw new Exception("Insufficient number of types");
            
            for (var i = 0; i < typeArguments.Length; ++i)
            {
                TypeLookup[Declaration.TypeParameters[i].Text] = typeArguments[i];
            }
        }
    }

    public class MethodGroup
    {
        public string Name { get; }
        public IReadOnlyList<AstMethodDeclaration> Methods { get; }
    }

    public class ReifiedMethod
    {
        public AstMethodDeclaration Declaration { get; }
        public ReifiedMethod(AstMethodDeclaration declaration, params ReifiedType[] typeArguments)
        { }
    }

    public class DeclarationLookup
    {
        public AstDeclaration Declaration { get; }
        public DeclarationLookup Parent { get; }
        public string Name { get; }
        public string QualifiedName { get; }
        public Dictionary<string, List<DeclarationLookup>> Children { get; }
            = new Dictionary<string, List<DeclarationLookup>>();

        public DeclarationLookup(AstDeclaration decl, DeclarationLookup parentLookup = null)
        {
            Declaration = decl;
            Parent = parentLookup;
            var prefix = Parent?.QualifiedName;
            if (!string.IsNullOrWhiteSpace(prefix))
                prefix += ".";
            Name = Declaration?.Name?.Text ?? "";
            QualifiedName = prefix + Name;
            foreach (var child in decl.Children.OfType<AstDeclaration>())
            {
                if (!Children.ContainsKey(child.Name))
                {
                    Children.Add(child.Name, new List<DeclarationLookup>());
                }

                var tmp = new DeclarationLookup(child, this);
                Children[child.Name].Add(tmp);
            }
        }

        public IEnumerable<(string, AstDeclaration)> GetDeclarations()
        {
            yield return (QualifiedName, Declaration);
            foreach (var pair in Children.Values
                         .SelectMany(v => v.SelectMany(dl => dl.GetDeclarations())))
                yield return pair;
        }
     }
}