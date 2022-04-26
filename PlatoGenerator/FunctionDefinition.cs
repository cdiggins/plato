using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlatoGenerator
{
    public class FunctionDefinition : Expression
    {
        public TypeDeclarationSyntax ParentType { get; set; }
        public bool IsStatic { get; set; } 
        public bool IsExtension { get; set; }
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

        public static FunctionDefinition Create(OperatorDeclarationSyntax op, SemanticModel model)
        {
            var symbol = ModelExtensions.GetDeclaredSymbol(model, op) as IMethodSymbol;
            if (symbol == null)
                throw new Exception($"Could not find method symbol for operator {op.OperatorToken}");
            var result = new Expression("#result", symbol?.ReturnType);
            var @this = symbol?.ReceiverType == null ? null : new Expression("#this", symbol?.ReceiverType);

            var r = new FunctionDefinition
            {
                ParentType = op.Parent as TypeDeclarationSyntax,
                Name = symbol?.Name,
                IsStatic = op.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                IsExtension = op.ParameterList.Parameters.Any(p => p.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword))),
                Body = op.Body.CreateStatement(op.ExpressionBody?.Expression, model),
                Syntax = op,
                Symbol = symbol,
                Model = model,
                Result = result,
                This = @this,
                Parameters = op.ParameterList.Parameters.Select(p => p.CreateExpression(model)).ToList()
            };

            return r;
        }

        public static FunctionDefinition Create(MethodDeclarationSyntax method, SemanticModel model)
        {
            var symbol = ModelExtensions.GetDeclaredSymbol(model, method) as IMethodSymbol;
            var result = new Expression("#result", symbol?.ReturnType);
            var @this = symbol?.ReceiverType == null ? null : new Expression("#this", symbol?.ReceiverType);

            var r = new FunctionDefinition
            {
                ParentType = method.Parent as TypeDeclarationSyntax, 
                Name = method.Identifier.ToString(),
                IsStatic = method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                IsExtension = method.ParameterList.Parameters.Any(p => p.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword))),
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

        public static Statement CreateConstructorBody(BlockSyntax block, ExpressionSyntax expr, SemanticModel model)
        {
            if (block != null) return block.CreateStatement(model);
            return new ExpressionStatement
            {
                Expression = expr.CreateExpression(model),
            };
        }

        public static FunctionDefinition Create(ConstructorDeclarationSyntax method, SemanticModel model)
        {
            var symbol = ModelExtensions.GetDeclaredSymbol(model, method) as IMethodSymbol;
            var result = new Expression("#result", symbol?.ReturnType);
            var @this = symbol?.ReceiverType == null ? null : new Expression("#this", symbol?.ReceiverType);

            var r = new FunctionDefinition
            {
                Name = "#ctor",
                ParentType = method.Parent as TypeDeclarationSyntax,
                IsStatic = method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                Body = CreateConstructorBody(method.Body, method.ExpressionBody?.Expression, model),
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