using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Utils;
using Plato.Compiler;
using Plato.Compiler.Analysis;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.CSharpWriter
{
    public class DocWriter
    {
        public StringBuilder Sb { get; set; } = new StringBuilder();

        public DocWriter(Compilation compilation)
        {
            var types = compilation.AllTypeAndLibraryDefinitions.ToList();
            WriteConcepts(types);
            WriteTypes(types);
        }

        public override string ToString()
        {
            return Sb.ToString();
        }


        public DocWriter WriteTypes(IReadOnlyList<TypeDef> typeDefs)
        {
            var types = typeDefs.Where(t => t.IsConcrete()).OrderBy(t => t.Name).ToList();
            Sb.AppendLine($"# Types").AppendLine();
            Sb.AppendLine($"Types are implemented as structs.");

            foreach (var t in types)
                WriteType(t);
            return this;
        }

        public DocWriter WriteConcepts(IReadOnlyList<TypeDef> typeDefs)
        {
            var concepts = typeDefs.Where(t => t.IsConcept()).OrderBy(t => t.Name).ToList();

            Sb.AppendLine($"# Concepts");
            Sb.AppendLine();
            
            Sb.AppendLine($"Concepts are implemented as interfaces. Functions defined on a concept are available on every type that implements the concept.");
            Sb.AppendLine();

            foreach (var c in concepts)
                WriteConcept(typeDefs, c);

            return this;
        }

        public DocWriter WriteConcept(IReadOnlyList<TypeDef> allTypes, TypeDef concept)
        {
            Sb.AppendLine($"## Concept {concept.Name}");
            Sb.AppendLine();

            var inheritsList = concept.GetSelfAndAllInheritedTypes().Where(t => t.Name != concept.Name).OrderBy(t => t.Name).ToList();
            var inheritsStr = inheritsList.Select(t1 => t1.Name).JoinStringsWithComma();
            Sb.AppendLine($"Inherits: {inheritsStr}.");
            Sb.AppendLine();

            var implementingTypes = allTypes.Where(t => t.Implements(concept) && t.IsConcrete()).ToList(); 
            var implementingStr = implementingTypes.Select(t1 => t1.Name).JoinStringsWithComma();
            Sb.AppendLine($"Implemented by: {implementingStr}.");
            Sb.AppendLine();

            var inheritingTypes = allTypes.Where(t => t.Implements(concept) && t.IsConcept()).ToList();
            var inheritingStr = inheritingTypes.Select(t1 => t1.Name).JoinStringsWithComma();
            Sb.AppendLine($"Inherited by: {inheritingStr}.");
            Sb.AppendLine();

            var functions = concept.Methods.OrderBy(m => m.Name).ToList();
            var functionsStr = functions.Select(f => f.Name).JoinStringsWithComma();
            Sb.AppendLine($"Functions: {functionsStr}.");
            Sb.AppendLine();

            var fields = concept.Fields.OrderBy(m => m.Name).ToList();
            var fieldsStr = fields.Select(f => f.Name).JoinStringsWithComma();
            Sb.AppendLine($"Fields: {fieldsStr}.");
            Sb.AppendLine();

            // TODO: list all inherited members 
            // TODO: format the thing better. 

            return this;
        }

        public DocWriter WriteType(TypeDef type)
        {
            Sb.AppendLine($"## Type {type.Name}");
            Sb.AppendLine();

            var fieldsStr = type.Fields.OrderBy(f => f.Name).Select(f => $"{f.Name}:{f.Type}").JoinStringsWithComma();
            Sb.AppendLine($"Fields: {fieldsStr}.");
            Sb.AppendLine();

            var implementsType = type.GetAllImplementedConcepts().OrderBy(t => t.Name).ToList();
            var implementsStr = implementsType.Select(t1 => t1.Name).JoinStringsWithComma();
            Sb.AppendLine($"Implements: {implementsStr}.");
            Sb.AppendLine();

            return this;
        }
    }
}
