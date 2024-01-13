using System.Collections.Generic;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Symbols;

namespace Plato.CSharpWriter
{
    public class TypeConverter 
    {
        public TypeDefinition CurrentType { get; set; }
        public TypeExpression ImplementedConcept { get; }
        public TypeSubstitutions Substitutions { get; }
        public string DefaultType { get; } = "var";

        public TypeConverter(TypeDefinition currentType, TypeExpression implementedConcept, TypeSubstitutions subs)
        {
            CurrentType = currentType;
            ImplementedConcept = implementedConcept;
            Substitutions = subs;
        }

        public string Convert(TypeExpression typeExpression)
        {
            // TODO: replace any references to the concept with the type name 
            // TODO: replace the type variables with the name of the type paramer as appropriate. 

            if (typeExpression == null) return DefaultType;
            var name = ConvertTypeVariable(typeExpression.Name);

            // TODO: what to do about the "Self", not sure. 

            var argsJoined = typeExpression.TypeArgs.Select(Convert).JoinStrings(", ");
            return $"{name}<{argsJoined}>";
        }

        public string ConvertTypeVariable(string name)
            => name.StartsWith("$") ? "T" + name.Substring(1) : name;
    
        public static string GetTypeName(TypeExpression expr, TypeDefinition parentType)
            => GetTypeName(expr, new TypeSubstitutions(parentType, expr));

        public static string GetTypeName(TypeExpression expr, TypeSubstitutions subs)
        {
            var r = expr.Name;
            if (expr.Name == "Self")
                r = subs.Self.Name;
            if (expr.Definition is TypeParameterDefinition tpd && subs.Lookup.ContainsKey(tpd))
                r = GetTypeName(subs.Lookup[tpd], subs);
            var args = expr.TypeArgs.Select(ta => GetTypeName(ta, subs)).ToList();
            if (args.Count == 0)
                return r;
            return $"{r}<{args.JoinStringsWithComma()}>";
        }

        // TODO: make a "converter" for the implemented concept. 
        public static IEnumerable<TypeSubstitutions> GetAllImplementedConcepts(TypeDefinition def)
        {
            var r = new HashSet<TypeSubstitutions>();
            foreach (var impl in def.Implements)
            {
                var sub = new TypeSubstitutions(def, impl);
                r.Add(sub);
                if (impl.Definition != null)
                {
                    foreach (var tmp2 in impl.Definition.GetAllImplementedConcepts())
                    {
                        r.Add(new TypeSubstitutions(def, tmp2, sub.Lookup));
                    }
                }
            }
            return r;
        }


    }
}