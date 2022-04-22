using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlatoGenerator
{
    public class PropertyDefinition : Expression
    {
        public bool IsStatic => throw new NotImplementedException();

        public FunctionDefinition Getter { get; set; }

        public static PropertyDefinition Create(PropertyDeclarationSyntax property, SemanticModel model)
        {
            var symbol = ModelExtensions.GetDeclaredSymbol(model, property) as IPropertySymbol;
            var result = new Expression("#result", symbol?.Type);
            var @this = symbol?.GetMethod?.ReceiverType == null ? null : new Expression("#this", symbol.GetMethod.ReceiverType);

            // Find the getter body. 
            Statement body = null;
            if (property.AccessorList != null)
            {
                foreach (var acc in property.AccessorList.Accessors)
                {
                    if (acc.Kind() == SyntaxKind.GetAccessorDeclaration)
                    {

                        body = acc.Body.CreateStatement(acc.ExpressionBody?.Expression, model);
                    }
                }
            }
            // TODO: do I ever actually get here? 
            if (body == null)
            {
                // Check for an arrow 
                if (property.ExpressionBody?.Expression != null)
                {
                    body = property.ExpressionBody.Expression.CreateStatement(model);
                }

                // TODO: check for a backing field
            }

            var getter = new FunctionDefinition
            {
                Name = property.Identifier.ToString(),

                Body = body,
                Syntax = null,
                Symbol = null,
                Model = model,
                Result = result,
                This = @this,
                Parameters = new List<Expression>(), // TODO: What about indexed properties? 
            };

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


    public class FunctionDefinition : Expression
    {
        public bool IsStatic { get; set; } 
        public List<Expression> Parameters = new List<Expression>();
        public Expression Result; 
        public Statement Body;

        // TODO: add these for lambdas and local functions 
        //public List<Expression> Captured = new List<Expression>();
        //public bool IsClosure => Captured.Count > 0;

        // TODO: get captured variables 
        public static FunctionDefinition Create(LambdaExpressionSyntax syntax, SemanticModel model)
        {
            // TODO: assure that this is always an IMethodSymbol, if not, what is it? 
            var symbol = ModelExtensions.GetSymbolInfo(model, syntax).Symbol as IMethodSymbol;

            /*
            var result = new Expression("#result", symbol?.ReturnType);
            var @this = symbol?.ReceiverType == null ? null : new Expression("#this", symbol?.ReceiverType);
            */

            var tmp1 = syntax as SimpleLambdaExpressionSyntax;
            var tmp2 = syntax as ParenthesizedLambdaExpressionSyntax;

            var parameters = new List<ParameterSyntax>();
            if (tmp1 != null) parameters.Add(tmp1.Parameter);
            if (tmp2 != null) parameters.AddRange(tmp2.ParameterList.Parameters);

            var r = new FunctionDefinition
            {
                IsStatic = false,
                Name = "#lambda",
                Body = syntax.Block.CreateStatement(syntax.ExpressionBody, model),
                Syntax = syntax,
                Symbol = symbol,
                Model = model,
                //Result = result,
                Parameters = parameters.Select(p => ExpressionExtensions.CreateExpression(p, model)).ToList()
            };

            return r;
        }

        // TODO: should I really be getting the function definition from a "PlatoMethodSyntax" 
        public static FunctionDefinition Create(MethodDeclarationSyntax method, SemanticModel model)
        {
            var symbol = ModelExtensions.GetDeclaredSymbol(model, method) as IMethodSymbol;
            var result = new Expression("#result", symbol?.ReturnType);
            var @this = symbol?.ReceiverType == null ? null : new Expression("#this", symbol?.ReceiverType);

            var r = new FunctionDefinition
            {
                Name = method.Identifier.ToString(),
                IsStatic = method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                Body = method.Body.CreateStatement(method.ExpressionBody?.Expression, model),
                Syntax = method,
                Symbol = symbol,
                Model = model,
                Result = result,
                This = @this,
                Parameters = method.ParameterList.Parameters.Select(p => p.CreateExpression(model)).ToList()
            };

            return r;
        }

        // TODO: get captured variables! 
        public static FunctionDefinition Create(LocalFunctionStatementSyntax method, SemanticModel model)
        {
            var symbol = model.GetDeclaredSymbol(method) as IMethodSymbol;
            var result = new Expression("#result", symbol?.ReturnType);
            var @this = symbol?.ReceiverType == null ? null : new Expression("#this", symbol.ReceiverType);

            var r = new FunctionDefinition
            {
                Name = method.Identifier.ToString(),
                IsStatic = false,
                Body = method.Body.CreateStatement(method.ExpressionBody?.Expression, model),
                Syntax = method,
                Symbol = symbol,
                Model = model,
                Result = result,
                This = @this,
                Parameters = method.ParameterList.Parameters.Select(p => p.CreateExpression(model)).ToList()
            };

            return r;
        }

        public List<Expression> GetReturnExpressions()
        {
            throw new NotImplementedException("TODO: get the list of all of the returned expressions.");
        }
    }
}