using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace PlatoGenerator
{
    public interface INamed
    {
        string Name { get; }
    }

    public abstract class PlatoSyntax
    {
        public readonly int Id = Lookup.Count;

        public abstract SyntaxNode GetNode();

        public static Dictionary<(TextSpan, SyntaxKind), PlatoSyntax> Lookup 
            = new Dictionary<(TextSpan, SyntaxKind), PlatoSyntax>();

        public static PlatoSyntax GetSyntax(SyntaxNode node)
            => node == null ? null : Lookup.TryGetValue((node.Span, node.Kind()), out var r) ? r : null;
    }

    public class PlatoSyntax<T, TSelf> 
        : PlatoSyntax
        where T : SyntaxNode
        where TSelf: PlatoSyntax<T, TSelf>, new()
    {
        public T Node { get; set; }

        public override SyntaxNode GetNode()
            => Node;

        public virtual void OnInit()
        { }

        public static TSelf Create(T syntax)
        {
            if (syntax == null)
                return null;

            var tmp = GetSyntax(syntax);
            if (tmp != null)
            {
                var r = (TSelf)tmp;
                Debug.Assert(r.Node.ToString() == syntax.ToString());
                return r;
            }
            else
            {
                var r = new TSelf { Node = syntax };
                r.OnInit();
                Lookup.Add((syntax.Span, syntax.Kind()), r);
                return r;
            }
        }

        public string Text
            => Node.ToString();

        public override int GetHashCode()
            => Id;

        public override bool Equals(object obj)
            => obj is PlatoSyntax syn && Id == syn.Id;

        public PlatoSyntax Parent
            => FindParent(Node);

        public PlatoSyntax FindParent(SyntaxNode syntax)
        {
            if (syntax == null) return null;
            var tmp = GetSyntax(syntax);
            if (tmp != null) return tmp;
            return FindParent(syntax.Parent);
        }
    }

    public class PlatoExpressionSyntax : PlatoSyntax<ExpressionSyntax, PlatoExpressionSyntax>
    {
        public List<PlatoExpressionSyntax> ChildExpressions;
        public List<PlatoVariableSyntax> Variables;

        public override void OnInit()
        {
            base.OnInit();
            ChildExpressions = Node.ChildNodes().OfType<ExpressionSyntax>().Select(Create).ToList();
            Variables = Node.ChildNodes().OfType<VariableDeclaratorSyntax>().Select(PlatoVariableSyntax.Create).ToList();
        }
    }

    public class PlatoVariableSyntax : PlatoSyntax<VariableDeclaratorSyntax, PlatoVariableSyntax>, INamed
    {
        public List<PlatoArgSyntax> Arguments;
        public PlatoExpressionSyntax Initializer;
        public PlatoTypeRefSyntax Type;

        public string Name => Node.Identifier.ToString();

        public override void OnInit()
        {
            base.OnInit();
            Arguments = Node.ArgumentList?.Arguments.Select(PlatoArgSyntax.Create).ToList() ?? new List<PlatoArgSyntax>();
            Type = PlatoTypeRefSyntax.Create((Node.Parent as VariableDeclarationSyntax)?.Type);
            Initializer = PlatoExpressionSyntax.Create(Node.Initializer?.Value);
        }
    }

    public class PlatoStatementSyntax : PlatoSyntax<StatementSyntax, PlatoStatementSyntax>
    {
        public List<PlatoStatementSyntax> ChildStatements;
        public List<PlatoExpressionSyntax> ChildExpressions;
        public List<PlatoTypeSyntax> ChildTypes;
        public List<PlatoVariableSyntax> Variables;

        public override void OnInit()
        {
            base.OnInit();
            ChildStatements = Node.ChildNodes().OfType<StatementSyntax>().Select(Create).ToList();
            ChildExpressions = Node.ChildNodes().OfType<ExpressionSyntax>().Select(PlatoExpressionSyntax.Create).ToList();
            ChildTypes = Node.ChildNodes().OfType<TypeDeclarationSyntax>().Select(PlatoTypeSyntax.Create).ToList();
            Variables = Node.ChildNodes().OfType<VariableDeclaratorSyntax>().Select(PlatoVariableSyntax.Create).ToList();
            // TODO: special processing of LocalFunctionStatementSyntax, which basically needs to be converted into a lambda
            // 
        }
    }

    public class PlatoTypeRefSyntax : PlatoSyntax<TypeSyntax, PlatoTypeRefSyntax>
    {
    }

    public class PlatoArgSyntax : PlatoSyntax<ArgumentSyntax, PlatoArgSyntax>
    {
        public PlatoExpressionSyntax Expression;
        public string Label => Node.NameColon?.Name.ToString();

        public override void OnInit()
        {
            base.OnInit();
            Expression = PlatoExpressionSyntax.Create(Node.Expression);
        }
    }

    public class PlatoParamSyntax : PlatoSyntax<ParameterSyntax, PlatoParamSyntax>, INamed
    {
        public PlatoTypeRefSyntax Type;
        public string Name => Node.Identifier.ToString();
        
        public override void OnInit()
        {
            base.OnInit();
            Type = PlatoTypeRefSyntax.Create(Node.Type);
        }
    }

    public class PlatoFieldSyntax : PlatoSyntax<FieldDeclarationSyntax, PlatoFieldSyntax>
    {
        public IReadOnlyList<PlatoVariableSyntax> Variables;

        public bool IsStatic => Node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

        public override void OnInit()
        {
            base.OnInit();
            Variables = Node.Declaration.Variables.Select(PlatoVariableSyntax.Create).ToList();
        }
    }

    public class PlatoAccessorSyntax : PlatoSyntax<AccessorDeclarationSyntax, PlatoAccessorSyntax>
    {
        public PlatoExpressionSyntax ArrowExpression;
        public PlatoStatementSyntax BlockStatement;

        public override void OnInit()
        {
            base.OnInit();
            ArrowExpression = PlatoExpressionSyntax.Create(Node.ExpressionBody?.Expression);
            BlockStatement = PlatoStatementSyntax.Create(Node.Body);
        }
    }


    public class PlatoPropertySyntax : PlatoSyntax<PropertyDeclarationSyntax, PlatoPropertySyntax>, INamed
    {
        public string Name => Node.Identifier.ToString();
        public PlatoExpressionSyntax InitializerExpression;
        public PlatoExpressionSyntax ArrowExpression;
        public PlatoTypeRefSyntax Type;
        public PlatoAccessorSyntax Getter;
        public PlatoAccessorSyntax Setter;

        public bool IsStatic => Node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

        public override void OnInit()
        {
            base.OnInit();

            var getAccessor = Node.AccessorList?.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.GetAccessorDeclaration);
            Getter = PlatoAccessorSyntax.Create(getAccessor);

            var setAccessor = Node.AccessorList?.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.SetAccessorDeclaration);
            Setter = PlatoAccessorSyntax.Create(setAccessor);

            InitializerExpression = PlatoExpressionSyntax.Create(Node.Initializer?.Value);
            ArrowExpression = PlatoExpressionSyntax.Create(Node.ExpressionBody?.Expression);
            Type = PlatoTypeRefSyntax.Create(Node.Type);
        }
    }


    public class PlatoIndexerSyntax : PlatoSyntax<IndexerDeclarationSyntax, PlatoIndexerSyntax>
    {
        public PlatoExpressionSyntax ArrowExpression;
        public PlatoTypeRefSyntax Type;
        public PlatoAccessorSyntax Getter;

        public override void OnInit()
        {
            base.OnInit();
            if (Node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
                throw new Exception("Static indexers not allowed");

            var getAccessor = Node.AccessorList?.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.GetAccessorDeclaration);
            Getter = PlatoAccessorSyntax.Create(getAccessor);

            var setAccessor = Node.AccessorList?.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.SetAccessorDeclaration);
            if (setAccessor != null)
                throw new Exception("No setters allowed");

            ArrowExpression = PlatoExpressionSyntax.Create(Node.ExpressionBody?.Expression);
            Type = PlatoTypeRefSyntax.Create(Node.Type);
        }
    }

    public class PlatoMethodSyntax : PlatoSyntax<MethodDeclarationSyntax, PlatoMethodSyntax>, INamed
    {
        public string Name => Node.Identifier.ToString();
        public PlatoStatementSyntax StatementBody;
        public PlatoExpressionSyntax ExpressionBody;
        public PlatoTypeRefSyntax ReturnType;
        public List<PlatoParamSyntax> Parameters;

        public bool IsStatic => Node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

        public override void OnInit()
        {
            base.OnInit();
            ReturnType = PlatoTypeRefSyntax.Create(Node.ReturnType);
            Parameters = Node.ParameterList.Parameters.Select(PlatoParamSyntax.Create).ToList();
            StatementBody = PlatoStatementSyntax.Create(Node.Body);
            ExpressionBody = PlatoExpressionSyntax.Create(Node.ExpressionBody?.Expression);
        }
    }


    public class PlatoConstructorSyntax : PlatoSyntax<ConstructorDeclarationSyntax, PlatoConstructorSyntax>
    {
        public string Name => Node.Identifier.ToString();
        public PlatoStatementSyntax StatementBody;
        public PlatoExpressionSyntax ExpressionBody;
        public PlatoTypeRefSyntax ReturnType;
        public List<PlatoParamSyntax> Parameters;

        public bool IsStatic => Node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

        public override void OnInit()
        {
            base.OnInit();
            Parameters = Node.ParameterList.Parameters.Select(PlatoParamSyntax.Create).ToList();
            StatementBody = PlatoStatementSyntax.Create(Node.Body);
            ExpressionBody = PlatoExpressionSyntax.Create(Node.ExpressionBody?.Expression);
        }
    }


    public class PlatoTypeSyntax: PlatoSyntax<TypeDeclarationSyntax, PlatoTypeSyntax>, INamed
    {
        public string Name => Node.Identifier.ToString();
        public string Kind => Node.Keyword.ToString();

        public List<PlatoFieldSyntax> Fields;
        public List<PlatoPropertySyntax> Properties;
        public List<PlatoMethodSyntax> Methods;
        public List<PlatoConstructorSyntax> Ctors;
        public List<PlatoIndexerSyntax> Indexers;

        public override void OnInit()
        {
            base.OnInit();
            Ctors = Node.Members.OfType<ConstructorDeclarationSyntax>().Select(PlatoConstructorSyntax.Create).ToList();
            Methods = Node.Members.OfType<MethodDeclarationSyntax>().Select(PlatoMethodSyntax.Create).ToList();
            Fields = Node.Members.OfType<FieldDeclarationSyntax>().Select(PlatoFieldSyntax.Create).ToList();
            Properties = Node.Members.OfType<PropertyDeclarationSyntax>().Select(PlatoPropertySyntax.Create).ToList();
            Indexers = Node.Members.OfType<IndexerDeclarationSyntax>().Select(PlatoIndexerSyntax.Create).ToList();
        }
    }

    public static class Extensions
    {
        public static List<PlatoTypeSyntax> GetPlatoTypes(this IEnumerable<SyntaxTree> trees)
            => trees.SelectMany(tree =>
                tree.GetRoot().DescendantNodes(node => !(node is TypeDeclarationSyntax))
                    .OfType<TypeDeclarationSyntax>().Select(PlatoTypeSyntax.Create)).ToList();
    }
}
