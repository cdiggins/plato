using System.Collections.Generic;
using System.Linq;
using Ara3D.Utils;

namespace Ara3D.Geometry.AST
{
    public class PlatoDeclarationWriter : CodeBuilder<PlatoDeclarationWriter>
    {
        public static string Write(AstTypeDeclaration declaration)
            => new PlatoDeclarationWriter().WriteDeclaration(declaration).ToString();

        public static string WriteFormatted(AstTypeDeclaration declaration, PlatoFormatOptions options)
            => PlatoTextCompaction.Apply(Write(declaration), options);

        public PlatoDeclarationWriter WriteDeclaration(AstTypeDeclaration declaration)
        {
            if (declaration.Kind != TypeKind.ConcreteType && declaration.Kind != TypeKind.Interface)
                return this;

            WriteKeyword(declaration.Kind);
            Write(" ").Write(declaration.Name.Text);
            WriteTypeParameters(declaration.TypeParameters);

            if (declaration.Kind == TypeKind.Interface)
            {
                WriteConstraints(declaration.Constraints);
                WriteInheritsOrImplements("inherits", declaration.Inherits);
            }
            else
            {
                WriteInheritsOrImplements("implements", declaration.Implements);
            }

            return WriteLine().Brace(w => w.WriteMembers(declaration.Members));
        }

        void WriteKeyword(TypeKind kind)
        {
            switch (kind)
            {
                case TypeKind.ConcreteType:
                    Write("type");
                    break;
                case TypeKind.Interface:
                    Write("concept");
                    break;
                default:
                    Write(kind.ToString());
                    break;
            }
        }

        PlatoDeclarationWriter WriteTypeParameters(IReadOnlyList<AstTypeParameter> typeParameters)
        {
            if (typeParameters.Count == 0)
                return this;
            return Write("<")
                .WriteCommaList(typeParameters, (w, tp) => w.Write(tp.Name.Text))
                .Write(">");
        }

        PlatoDeclarationWriter WriteInheritsOrImplements(string keyword, IReadOnlyList<AstTypeNode> types)
        {
            if (types.Count == 0)
                return this;
            return WriteLine()
                .Write("    ")
                .Write(keyword)
                .Write(" ")
                .WriteCommaList(types, WriteTypeNode);
        }

        PlatoDeclarationWriter WriteConstraints(IReadOnlyList<AstConstraint> constraints)
        {
            if (constraints.Count == 0)
                return this;

            var r = WriteLine().Write("    where ");
            for (var i = 0; i < constraints.Count; i++)
            {
                if (i > 0)
                    r = r.Write(", ");
                var c = constraints[i];
                r = r.Write(c.Name.Text).Write(": ").WriteTypeNode(c.Constraint);
            }
            return r;
        }

        PlatoDeclarationWriter WriteMembers(IReadOnlyList<AstMemberDeclaration> members)
        {
            var r = this;
            foreach (var member in members)
                r = r.WriteMember(member);
            return r;
        }

        PlatoDeclarationWriter WriteMember(AstMemberDeclaration member)
        {
            return member switch
            {
                AstFieldDeclaration field => WriteField(field),
                AstMethodDeclaration method => WriteMethod(method),
                _ => this
            };
        }

        PlatoDeclarationWriter WriteField(AstFieldDeclaration field)
            => Write("    ")
                .Write(field.Name.Text)
                .Write(": ")
                .WriteTypeNode(field.Type)
                .WriteLine(";");

        PlatoDeclarationWriter WriteMethod(AstMethodDeclaration method)
        {
            var r = Write("    ")
                .Write(method.Name.Text)
                .Write("(")
                .WriteCommaList(method.Parameters, WriteParameter)
                .Write("): ")
                .WriteTypeNode(method.Type)
                .WriteLine(";");
            return r;
        }

        PlatoDeclarationWriter WriteParameter(PlatoDeclarationWriter w, AstParameterDeclaration parameter)
            => w.Write(parameter.Name.Text).Write(": ").WriteTypeNode(parameter.Type);

        PlatoDeclarationWriter WriteTypeNode(AstTypeNode typeNode)
            => WriteTypeNode(this, typeNode);

        static PlatoDeclarationWriter WriteTypeNode(PlatoDeclarationWriter w, AstTypeNode typeNode)
        {
            if (typeNode == null)
                return w;

            var name = typeNode.Name.Text;
            if (name.StartsWith('$'))
                name = name.Substring(1);

            w = w.Write(name);
            if (typeNode.TypeArguments.Count == 0)
                return w;

            return w.Write("<")
                .WriteCommaList(typeNode.TypeArguments, WriteTypeNode)
                .Write(">");
        }
    }
}
