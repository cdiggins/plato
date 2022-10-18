using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlatoGenerator
{
    public class PropertyDefinition : Expression
    {
        public bool IsStatic => throw new NotImplementedException();

        public FunctionDefinition Getter { get; set; }
        public bool IsAutoProperty => Getter == null || Getter.Body == null;

        public static PropertyDefinition Create(PropertyDeclarationSyntax property, SemanticModel model)
        {
            var symbol = ModelExtensions.GetDeclaredSymbol(model, property) as IPropertySymbol;
            var result = new Expression("#result", symbol?.Type);
            var @this = symbol?.GetMethod?.ReceiverType == null ? null : new Expression("#this", symbol.GetMethod.ReceiverType);

            var getter = property.GetFunctionDefinition(model);

            return new PropertyDefinition()
            {
                Getter = getter,
                Syntax = property,
                Symbol = symbol,
                Model = model,
                This = @this
            };
        }
    }

    public static class PropertyExtensions
    {
        public static FunctionDefinition GetFunctionDefinition(this IndexerDeclarationSyntax property, SemanticModel model)
        {
            var symbol = ModelExtensions.GetDeclaredSymbol(model, property) as IPropertySymbol;
            var result = new Expression("#result", symbol?.Type);
            var @this = symbol?.GetMethod?.ReceiverType == null ? null : new Expression("#this", symbol.GetMethod.ReceiverType);

            Statement body = null;
            if (property.AccessorList != null)
            {
                foreach (var acc in property.AccessorList.Accessors)
                {
                    if (acc.Kind() == SyntaxKind.GetAccessorDeclaration)
                    {
                        // Only if there is an actual body.
                        if (acc.ExpressionBody?.Expression != null)
                        {
                            body = acc.Body.CreateStatement(acc.ExpressionBody?.Expression, model);
                        }
                    }
                }
            }
            // TODO: do I ever actually get here? 
            if (body == null)
            {
                // Check for an arrow 
                if (property.ExpressionBody?.Expression != null)
                {
                    body = property.ExpressionBody.Expression.CreateReturnStatement(model);
                }

                // TODO: check for a backing field
            }

            return new FunctionDefinition
            {
                Name = "op_Subscript",
                Body = body,
                Syntax = null,
                Symbol = null,
                Model = model,
                Result = result,
                This = @this,
                Parameters = new List<Expression>(),
            };

        }

        public static FunctionDefinition GetFunctionDefinition(this PropertyDeclarationSyntax property, SemanticModel model)
        {
            var symbol = ModelExtensions.GetDeclaredSymbol(model, property) as IPropertySymbol;
            var result = new Expression("#result", symbol?.Type);
            var @this = symbol?.GetMethod?.ReceiverType == null ? null : new Expression("#this", symbol.GetMethod.ReceiverType);

            Statement body = null;
            if (property.AccessorList != null)
            {
                foreach (var acc in property.AccessorList.Accessors)
                {
                    if (acc.Kind() == SyntaxKind.GetAccessorDeclaration)
                    {
                        // Only if there is an actual body.
                        if (acc.ExpressionBody?.Expression != null)
                        {
                            body = acc.Body.CreateStatement(acc.ExpressionBody?.Expression, model);
                        }
                    }
                }
            }
            // TODO: do I ever actually get here? 
            if (body == null)
            {
                // Check for an arrow 
                if (property.ExpressionBody?.Expression != null)
                {
                    body = property.ExpressionBody.Expression.CreateReturnStatement(model);
                }

                // TODO: check for a backing field
            }

            return new FunctionDefinition
            {
                Name = property.Identifier.ToString(),
                Body = body,
                Syntax = null,
                Symbol = null,
                Model = model,
                Result = result,
                This = @this,
                Parameters = new List<Expression>(),
            };
        }
    }
}