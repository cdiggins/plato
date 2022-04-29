using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlatoRoslynSyntaxAnalyzer
{
    public interface INamed
    {
        string Name { get; }
    }

    public abstract class PlatoSyntax
    {
        public abstract SyntaxNode GetNode();

        /*
        public static PlatoSyntax Create(SyntaxNode node)
        {
            switch (node)
            {
            }
        }*/
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
            ChildExpressions = node.ChildNodes().OfType<ExpressionSyntax>().Select(Create).ToList();
            Variables = node.ChildNodes().OfType<VariableDeclaratorSyntax>().Select(PlatoVariableSyntax.Create).ToList();
        }
        public IReadOnlyList<PlatoExpressionSyntax> ChildExpressions { get; }
        public IReadOnlyList<PlatoVariableSyntax> Variables { get; }  

        public static PlatoExpressionSyntax Create(ExpressionSyntax node) => 
            node == null ? null : new PlatoExpressionSyntax(node);
    }

    public class PlatoVariableSyntax : PlatoSyntax<VariableDeclaratorSyntax>, INamed
    {
        public IReadOnlyList<PlatoArgSyntax> Arguments { get; }
        public PlatoExpressionSyntax Initializer { get; }
        public PlatoTypeRefSyntax Type { get; }

        public string Name => Node.Identifier.ToString();

        public static PlatoVariableSyntax Create(VariableDeclaratorSyntax node) 
            => new PlatoVariableSyntax(node);

        public PlatoVariableSyntax(VariableDeclaratorSyntax node) : base(node)
        {
            Arguments = node.ArgumentList?.Arguments.Select(PlatoArgSyntax.Create).ToList() ?? new List<PlatoArgSyntax>();
            Type = PlatoTypeRefSyntax.Create((node.Parent as VariableDeclarationSyntax)?.Type);
            Initializer = PlatoExpressionSyntax.Create(node.Initializer?.Value);
        }
    }

    public class PlatoStatementSyntax : PlatoSyntax<StatementSyntax>
    {
        public IReadOnlyList<PlatoStatementSyntax> ChildStatements { get; }
        public IReadOnlyList<PlatoExpressionSyntax> ChildExpressions { get; }
        public IReadOnlyList<PlatoTypeSyntax> ChildTypes { get; }
        public IReadOnlyList<PlatoVariableSyntax> Variables { get; }

        public static PlatoStatementSyntax Create(StatementSyntax node) 
            => node == null ? null : new PlatoStatementSyntax(node);

        public PlatoStatementSyntax(StatementSyntax node) : base(node)
        {
            ChildStatements = node.ChildNodes().OfType<StatementSyntax>().Select(Create).ToList();
            ChildExpressions = node.ChildNodes().OfType<ExpressionSyntax>().Select(PlatoExpressionSyntax.Create).ToList();
            ChildTypes = node.ChildNodes().OfType<TypeDeclarationSyntax>().Select(PlatoTypeSyntax.Create).ToList();
            Variables = node.ChildNodes().OfType<VariableDeclaratorSyntax>().Select(PlatoVariableSyntax.Create).ToList();
            
            // TODO: special processing of LocalFunctionStatementSyntax, which basically needs to be converted into a lambda
            //
        }
    }

    public class PlatoTypeRefSyntax : PlatoSyntax<TypeSyntax>
    {
        public PlatoTypeRefSyntax(TypeSyntax node) : base(node) {}
        public static PlatoTypeRefSyntax Create(TypeSyntax node) => new PlatoTypeRefSyntax(node);
    }

    public class PlatoArgSyntax : PlatoSyntax<ArgumentSyntax>
    {
        public PlatoExpressionSyntax Expression;
        public string Label => Node.NameColon?.Name.ToString().Trim() ?? "";
        public bool IsRef => Node.RefOrOutKeyword.Text == "ref";
        public bool IsOut => Node.RefOrOutKeyword.Text == "out";

        public PlatoArgSyntax(ArgumentSyntax node) : base(node)
        {
            Expression = PlatoExpressionSyntax.Create(node.Expression);
        }

        public static PlatoArgSyntax Create(ArgumentSyntax node) => new PlatoArgSyntax(node);
    }

    public class PlatoParamSyntax : PlatoSyntax<ParameterSyntax>, INamed
    {
        public PlatoTypeRefSyntax Type { get; }
        public string Name => Node.Identifier.ToString();

        public bool IsRef => Node.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword));
        public bool IsOut => Node.Modifiers.Any(m => m.IsKind(SyntaxKind.OutKeyword));
        public bool IsParams => Node.Modifiers.Any(m => m.IsKind(SyntaxKind.ParamKeyword));

        public PlatoParamSyntax(ParameterSyntax node) : base(node)
        {
            Type = PlatoTypeRefSyntax.Create(node.Type);
            node.NoOutParameters(!IsOut);
            node.NoRefParameters(!IsRef);
            node.NoRestParameters(!IsParams);
        }

        public static PlatoParamSyntax Create(ParameterSyntax node)
            => new PlatoParamSyntax(node);
    }

    public class PlatoMemberSyntax<SyntaxType> : PlatoSyntax<SyntaxType>
        where SyntaxType : MemberDeclarationSyntax
    {
        public PlatoMemberSyntax(SyntaxType node) : base(node) { }
        public bool IsStatic => Node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
    }

    public class PlatoFieldSyntax : PlatoMemberSyntax<FieldDeclarationSyntax>
    {
        public IReadOnlyList<PlatoVariableSyntax> Variables { get; }
        public PlatoFieldSyntax(FieldDeclarationSyntax node) : base(node) 
        {
            Variables = Node.Declaration.Variables.Select(PlatoVariableSyntax.Create).ToList();
        }

        public static PlatoFieldSyntax Create(FieldDeclarationSyntax node) => new PlatoFieldSyntax(node);
    }

    public class PlatoAccessorSyntax : PlatoSyntax<AccessorDeclarationSyntax>
    {
        public PlatoExpressionSyntax ArrowExpression { get; }
        public PlatoStatementSyntax BlockStatement { get; }

        public PlatoAccessorSyntax(AccessorDeclarationSyntax node) :base(node)
        {
            ArrowExpression = PlatoExpressionSyntax.Create(Node.ExpressionBody?.Expression);
            BlockStatement = PlatoStatementSyntax.Create(Node.Body);
        }

        public static PlatoAccessorSyntax Create(AccessorDeclarationSyntax node)
            => node == null ? null : new PlatoAccessorSyntax(node);
    }

    public class PlatoPropertySyntax : PlatoMemberSyntax<PropertyDeclarationSyntax>, INamed
    {
        public string Name => Node.Identifier.ToString();
        public PlatoExpressionSyntax InitializerExpression { get; }
        public PlatoExpressionSyntax ArrowExpression { get; }
        public PlatoTypeRefSyntax Type { get; }
        public PlatoAccessorSyntax Getter { get; }

        public PlatoPropertySyntax(PropertyDeclarationSyntax node) :base(node)
        {
            var getAccessor = Node.AccessorList?.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.GetAccessorDeclaration);
            Getter = PlatoAccessorSyntax.Create(getAccessor);

            var setAccessor = Node.AccessorList?.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.SetAccessorDeclaration);
            node.NoSetter(setAccessor== null);

            var initAccessor = Node.AccessorList?.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.InitAccessorDeclaration);
            node.NoInitOnlySetter(initAccessor == null);

            InitializerExpression = PlatoExpressionSyntax.Create(Node.Initializer?.Value);
            ArrowExpression = PlatoExpressionSyntax.Create(Node.ExpressionBody?.Expression);
            Type = PlatoTypeRefSyntax.Create(Node.Type);
        }

        public static PlatoPropertySyntax Create(PropertyDeclarationSyntax node)
            => new PlatoPropertySyntax(node);
    }

    public class PlatoIndexerSyntax : PlatoMemberSyntax<IndexerDeclarationSyntax>
    {
        public PlatoExpressionSyntax ArrowExpression { get; }
        public PlatoTypeRefSyntax Type { get; }
        public PlatoAccessorSyntax Getter { get; }
        public PlatoParamSyntax Parameter { get; }

        public PlatoIndexerSyntax(IndexerDeclarationSyntax node) : base(node)
        {
            node.NoStaticIndexer(!IsStatic);

            var getAccessor = Node.AccessorList?.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.GetAccessorDeclaration);
            Getter = PlatoAccessorSyntax.Create(getAccessor);

            var parameters = node.ParameterList.Parameters.Select(PlatoParamSyntax.Create).ToList();
            node.IndexerHasSingleParameter(parameters.Count == 1);
            Parameter = parameters[0];

            var setAccessor = Node.AccessorList?.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.SetAccessorDeclaration);
            node.NoSetter(setAccessor == null);

            ArrowExpression = PlatoExpressionSyntax.Create(Node.ExpressionBody?.Expression);
            node.MustHaveGetter(Getter != null || ArrowExpression != null);
            Type = PlatoTypeRefSyntax.Create(Node.Type);
        }

        public static PlatoIndexerSyntax Create(IndexerDeclarationSyntax node)
            => new PlatoIndexerSyntax(node);
    }

    public class PlatoOperatorSyntax : PlatoMemberSyntax<OperatorDeclarationSyntax>, INamed
    {
        public string Name => Node.OperatorToken.ToString();
        public PlatoStatementSyntax StatementBody { get; }
        public PlatoExpressionSyntax ExpressionBody { get; }
        public PlatoTypeRefSyntax ReturnType { get; }
        public IReadOnlyList<PlatoParamSyntax> Parameters { get; }

        public PlatoOperatorSyntax(OperatorDeclarationSyntax node) : base(node)
        {
            ReturnType = PlatoTypeRefSyntax.Create(Node.ReturnType);
            Parameters = Node.ParameterList.Parameters.Select(PlatoParamSyntax.Create).ToList();
            StatementBody = PlatoStatementSyntax.Create(Node.Body);
            ExpressionBody = PlatoExpressionSyntax.Create(Node.ExpressionBody?.Expression);
        }

        public static PlatoOperatorSyntax Create(OperatorDeclarationSyntax node)
            => new PlatoOperatorSyntax(node);
    }

    public class PlatoConversionSyntax : PlatoMemberSyntax<ConversionOperatorDeclarationSyntax>, INamed
    {
        public string Name => "#castoperator";
        public bool IsImplicit => Node.ImplicitOrExplicitKeyword.ToString() == "implicit";
        public PlatoStatementSyntax StatementBody { get; }
        public PlatoExpressionSyntax ExpressionBody { get; }
        public PlatoTypeRefSyntax ReturnType { get; }
        public IReadOnlyList<PlatoParamSyntax> Parameters { get; }

        public PlatoConversionSyntax(ConversionOperatorDeclarationSyntax node) : base(node)
        {
            ReturnType = PlatoTypeRefSyntax.Create(Node.Type);
            Parameters = Node.ParameterList.Parameters.Select(PlatoParamSyntax.Create).ToList();
            StatementBody = PlatoStatementSyntax.Create(Node.Body);
            ExpressionBody = PlatoExpressionSyntax.Create(Node.ExpressionBody?.Expression);
        }

        public static PlatoConversionSyntax Create(ConversionOperatorDeclarationSyntax node) 
            => new PlatoConversionSyntax(node);
    }

    public class PlatoMethodSyntax : PlatoMemberSyntax<MethodDeclarationSyntax>, INamed
    {
        public string Name => Node.Identifier.ToString();
        public PlatoStatementSyntax StatementBody { get; }
        public PlatoExpressionSyntax ExpressionBody { get; }
        public PlatoTypeRefSyntax ReturnType { get; }
        public IReadOnlyList<PlatoParamSyntax> Parameters { get; }

        public bool IsExtensionMethod =>
            IsStatic && Parameters.Any(p => p.Node.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword)));

        public PlatoMethodSyntax(MethodDeclarationSyntax node) :base(node)
        {
            ReturnType = PlatoTypeRefSyntax.Create(Node.ReturnType);
            Parameters = Node.ParameterList.Parameters.Select(PlatoParamSyntax.Create).ToList();
            StatementBody = PlatoStatementSyntax.Create(Node.Body);
            ExpressionBody = PlatoExpressionSyntax.Create(Node.ExpressionBody?.Expression);
        }

        public static PlatoMethodSyntax Create(MethodDeclarationSyntax node) 
            => new PlatoMethodSyntax(node);
    }


    public class PlatoConstructorSyntax : PlatoMemberSyntax<ConstructorDeclarationSyntax>
    {
        public PlatoStatementSyntax StatementBody { get; }
        public PlatoExpressionSyntax ExpressionBody { get; }
        public IReadOnlyList<PlatoParamSyntax> Parameters { get; }

        public PlatoConstructorSyntax(ConstructorDeclarationSyntax node): base(node)
        {
            Parameters = Node.ParameterList.Parameters.Select(PlatoParamSyntax.Create).ToList();
            StatementBody = PlatoStatementSyntax.Create(Node.Body);
            ExpressionBody = PlatoExpressionSyntax.Create(Node.ExpressionBody?.Expression);
        }

        public static PlatoConstructorSyntax Create(ConstructorDeclarationSyntax node)
            => new PlatoConstructorSyntax(node);
    }

    public class PlatoTypeSyntax : PlatoSyntax<TypeDeclarationSyntax>, INamed
    {
        public string Name => Node.Identifier.ToString();
        public string Kind => Node.Keyword.ToString();

        public IReadOnlyList<PlatoFieldSyntax> Fields { get; }
        public IReadOnlyList<PlatoOperatorSyntax> Operators { get; }
        public IReadOnlyList<PlatoPropertySyntax> Properties { get; }
        public IReadOnlyList<PlatoMethodSyntax> Methods { get; }
        public IReadOnlyList<PlatoConstructorSyntax> Ctors { get; }
        public IReadOnlyList<PlatoIndexerSyntax> Indexers { get; }
        public IReadOnlyList<PlatoConversionSyntax> Converters { get; }

        public PlatoTypeSyntax(TypeDeclarationSyntax node) : base(node)
        {
            Ctors = Node.Members.OfType<ConstructorDeclarationSyntax>().Select(PlatoConstructorSyntax.Create).ToList();
            Methods = Node.Members.OfType<MethodDeclarationSyntax>().Select(PlatoMethodSyntax.Create).ToList();
            Fields = Node.Members.OfType<FieldDeclarationSyntax>().Select(PlatoFieldSyntax.Create).ToList();
            Operators = Node.Members.OfType<OperatorDeclarationSyntax>().Select(PlatoOperatorSyntax.Create).ToList();
            Properties = Node.Members.OfType<PropertyDeclarationSyntax>().Select(PlatoPropertySyntax.Create).ToList();
            Indexers = Node.Members.OfType<IndexerDeclarationSyntax>().Select(PlatoIndexerSyntax.Create).ToList();
            Converters = Node.Members.OfType<ConversionOperatorDeclarationSyntax>().Select(PlatoConversionSyntax.Create).ToList();
        }

        public static PlatoTypeSyntax Create(TypeDeclarationSyntax node) 
            => new PlatoTypeSyntax(node);
    }
}
