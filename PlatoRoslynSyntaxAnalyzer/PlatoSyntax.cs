using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace PlatoRoslynSyntaxAnalyzer
{
    public interface INamed
    {
        string Name { get; }
    }

    public abstract class PlatoSyntax
    {
        public abstract SyntaxNode GetNode();
    }

    public class PlatoSyntax<T> : PlatoSyntax where T : SyntaxNode
    {
        public PlatoSyntax(T node) => Node = node;
        public T Node { get; }
        public override SyntaxNode GetNode() => Node;
    }

    public class PlatoExpressionSyntax : PlatoSyntax<ExpressionSyntax>
    {
        public PlatoExpressionSyntax(ExpressionSyntax node) : base(node) 
        {
            var r = new PlatoExpressionSyntax(node);
            ChildExpressions = Enumerable.ToList<PlatoExpressionSyntax>(node.ChildNodes().OfType<ExpressionSyntax>().Select(Create));
            Variables = Enumerable.ToList<PlatoVariableSyntax>(node.ChildNodes().OfType<VariableDeclaratorSyntax>().Select(PlatoVariableSyntax.Create));
        }
        public readonly List<PlatoExpressionSyntax> ChildExpressions;
        public readonly List<PlatoVariableSyntax> Variables;        
        public static PlatoExpressionSyntax Create(ExpressionSyntax node) => new PlatoExpressionSyntax(node);
    }

    public class PlatoVariableSyntax : PlatoSyntax<VariableDeclaratorSyntax>, INamed
    {
        public readonly List<PlatoArgSyntax> Arguments;
        public readonly PlatoExpressionSyntax Initializer;
        public readonly PlatoTypeRefSyntax Type;

        public string Name => Node.Identifier.ToString();

        public static PlatoVariableSyntax Create(VariableDeclaratorSyntax node) => new PlatoVariableSyntax(node);

        public PlatoVariableSyntax(VariableDeclaratorSyntax node) : base(node)
        {
            Arguments = Enumerable.ToList<PlatoArgSyntax>(node.ArgumentList?.Arguments.Select(PlatoArgSyntax.Create) ?? new List<PlatoArgSyntax>());
            Type = PlatoTypeRefSyntax.Create((node.Parent as VariableDeclarationSyntax)?.Type);
            Initializer = PlatoExpressionSyntax.Create(node.Initializer?.Value);
        }
    }

    public class PlatoStatementSyntax : PlatoSyntax<StatementSyntax>
    {
        public List<PlatoStatementSyntax> ChildStatements;
        public List<PlatoExpressionSyntax> ChildExpressions;
        public List<PlatoTypeSyntax> ChildTypes;
        public List<PlatoVariableSyntax> Variables;

        public static PlatoStatementSyntax Create(StatementSyntax node)
        {
            var r = new PlatoStatementSyntax();
            r.ChildStatements = Enumerable.ToList<PlatoStatementSyntax>(node.ChildNodes().OfType<StatementSyntax>().Select(Create));
            r.ChildExpressions = Enumerable.ToList<PlatoExpressionSyntax>(node.ChildNodes().OfType<ExpressionSyntax>().Select(PlatoExpressionSyntax.Create));
            r.ChildTypes = Enumerable.ToList<PlatoTypeSyntax>(node.ChildNodes().OfType<TypeDeclarationSyntax>().Select(PlatoTypeSyntax.Create));
            r.Variables = Enumerable.ToList<PlatoVariableSyntax>(node.ChildNodes().OfType<VariableDeclaratorSyntax>().Select(PlatoVariableSyntax.Create));
            // TODO: special processing of LocalFunctionStatementSyntax, which basically needs to be converted into a lambda
            //
            return r;
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
            Variables = Enumerable.ToList<PlatoVariableSyntax>(Node.Declaration.Variables.Select(PlatoVariableSyntax.Create));
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

    public class PlatoOperatorSyntax : PlatoSyntax<OperatorDeclarationSyntax, PlatoOperatorSyntax>, INamed
    {
        public string Name => Node.OperatorToken.ToString();
        public PlatoStatementSyntax StatementBody;
        public PlatoExpressionSyntax ExpressionBody;
        public PlatoTypeRefSyntax ReturnType;
        public List<PlatoParamSyntax> Parameters;

        public override void OnInit()
        {
            base.OnInit();
            ReturnType = PlatoTypeRefSyntax.Create(Node.ReturnType);
            Parameters = Enumerable.ToList<PlatoParamSyntax>(Node.ParameterList.Parameters.Select(PlatoParamSyntax.Create));
            StatementBody = PlatoStatementSyntax.Create(Node.Body);
            ExpressionBody = PlatoExpressionSyntax.Create(Node.ExpressionBody?.Expression);
        }
    }

    public class PlatoConversionSyntax : PlatoSyntax<ConversionOperatorDeclarationSyntax, PlatoConversionSyntax>, INamed
    {
        public string Name => "#castoperator";
        public bool IsImplicit => Node.ImplicitOrExplicitKeyword.ToString() == "implicit";
        public PlatoStatementSyntax StatementBody;
        public PlatoExpressionSyntax ExpressionBody;
        public PlatoTypeRefSyntax ReturnType;
        public List<PlatoParamSyntax> Parameters;

        public override void OnInit()
        {
            base.OnInit();
            ReturnType = PlatoTypeRefSyntax.Create(Node.Type);
            Parameters = Enumerable.ToList<PlatoParamSyntax>(Node.ParameterList.Parameters.Select(PlatoParamSyntax.Create));
            StatementBody = PlatoStatementSyntax.Create(Node.Body);
            ExpressionBody = PlatoExpressionSyntax.Create(Node.ExpressionBody?.Expression);
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

        public bool IsExtensionMethod =>
            IsStatic && Parameters.Any(p => p.Node.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword)));

        public override void OnInit()
        {
            base.OnInit();
            ReturnType = PlatoTypeRefSyntax.Create(Node.ReturnType);
            Parameters = Enumerable.ToList<PlatoParamSyntax>(Node.ParameterList.Parameters.Select(PlatoParamSyntax.Create));
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
            Parameters = Enumerable.ToList<PlatoParamSyntax>(Node.ParameterList.Parameters.Select(PlatoParamSyntax.Create));
            StatementBody = PlatoStatementSyntax.Create(Node.Body);
            ExpressionBody = PlatoExpressionSyntax.Create(Node.ExpressionBody?.Expression);
        }
    }

    public class PlatoTypeSyntax : PlatoSyntax<TypeDeclarationSyntax, PlatoTypeSyntax>, INamed
    {
        public string Name => Node.Identifier.ToString();
        public string Kind => Node.Keyword.ToString();

        public List<PlatoFieldSyntax> Fields;
        public List<PlatoOperatorSyntax> Operators;
        public List<PlatoPropertySyntax> Properties;
        public List<PlatoMethodSyntax> Methods;
        public List<PlatoConstructorSyntax> Ctors;
        public List<PlatoIndexerSyntax> Indexers;
        public List<PlatoConversionSyntax> Converters;

        public override void OnInit()
        {
            base.OnInit();
            Ctors = Enumerable.ToList<PlatoConstructorSyntax>(Node.Members.OfType<ConstructorDeclarationSyntax>().Select(PlatoConstructorSyntax.Create));
            Methods = Enumerable.ToList<PlatoMethodSyntax>(Node.Members.OfType<MethodDeclarationSyntax>().Select(PlatoMethodSyntax.Create));
            Fields = Enumerable.ToList<PlatoFieldSyntax>(Node.Members.OfType<FieldDeclarationSyntax>().Select(PlatoFieldSyntax.Create));
            Operators = Enumerable.ToList<PlatoOperatorSyntax>(Node.Members.OfType<OperatorDeclarationSyntax>().Select(PlatoOperatorSyntax.Create));
            Properties = Enumerable.ToList<PlatoPropertySyntax>(Node.Members.OfType<PropertyDeclarationSyntax>().Select(PlatoPropertySyntax.Create));
            Indexers = Enumerable.ToList<PlatoIndexerSyntax>(Node.Members.OfType<IndexerDeclarationSyntax>().Select(PlatoIndexerSyntax.Create));
            Converters = Enumerable.ToList<PlatoConversionSyntax>(Node.Members.OfType<ConversionOperatorDeclarationSyntax>().Select(PlatoConversionSyntax.Create));
        }
    }
}
