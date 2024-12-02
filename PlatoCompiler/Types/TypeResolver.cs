﻿using Plato.AST;
using Plato.Compiler.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plato.Compiler.Types
{
    public class TypeResolver
    {
        public Compilation Compilation { get; }
        
        public Dictionary<Expression, TypeExpression> Types { get; } 
            = new Dictionary<Expression, TypeExpression>();

        public TypeExpression CreateType(string name, params TypeExpression[] args)
            => CreateType(Compilation.GetTypeDefinition(name), args);

        public TypeExpression CreateType(TypeDef def, params TypeExpression[] args)
        {
            if (def == null)
                throw new ArgumentNullException(nameof(def));
            if (args.Any(a => a == null))
                throw new Exception("One of the args is null");
            return new TypeExpression(def, args);
        }

        public TypeExpression CreateArrayType(IReadOnlyList<Expression> expressions)
            => CreateType("Array", expressions.Select(GetType).ToArray());

        public TypeExpression CreateFunctionType(IReadOnlyList<TypeExpression> args, TypeExpression returnType)
            => CreateType($"Function{args.Count}", args.Append(returnType).ToArray());

        public TypeExpression CreateTypeVariable(int i)
            => CreateType(new TypeVariable(null, $"$T{i}"));

        public TypeExpression CreateFunctionType(int n)
            => CreateType($"Function{n}", Enumerable.Range(0, n + 1)
                .Select(CreateTypeVariable).ToArray());

        public TypeExpression CreateTuple(params TypeExpression[] children)
            => CreateType($"Tuple{children.Length}", children);

        public TypeExpression CreateType(LiteralTypesEnum typeEnum)
        {
            switch (typeEnum)
            {
                case LiteralTypesEnum.Integer:
                    return CreateType("Integer");
                case LiteralTypesEnum.Number:
                    return CreateType("Number");
                case LiteralTypesEnum.Boolean:
                    return CreateType("Boolean");
                case LiteralTypesEnum.String:
                    return CreateType("String");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public TypeExpression Unify(TypeExpression a, TypeExpression b)
            => a;

        public TypeExpression GetType(Expression expr)
        {
            if (Types.TryGetValue(expr, out var type))
                return type;

            TypeExpression r = null;
            switch (expr)
            {
                case Assignment assignment:
                    throw new Exception("Assignment not supported");

                case ConditionalExpression conditionalExpression:
                    
                    r = Unify(
                        GetType(conditionalExpression.IfTrue), 
                        GetType(conditionalExpression.IfFalse));
                    break;

                case FunctionCall functionCall:
                    //r = Compilation.GetOrComputeFunctionCallAnalysis(functionCall).;
                    break;

                case FunctionGroupRefSymbol functionGroupReference:
                    r = CreateFunctionType(functionGroupReference.Def.Functions[0].NumParameters);
                    break;

                case Lambda lambda:
                    r = CreateFunctionType(lambda.Function.NumParameters);
                    break;

                case Literal literal:
                    r = CreateType(literal.TypeEnum);
                    break;

                case ParameterRefSymbol parameterReference:
                    r = parameterReference.Def.Type;
                    break;

                case Parenthesized parenthesized:
                    r = GetType(parenthesized.Expression);
                    break;

                case VariableRefSymbol variableReference:
                    r = variableReference.Def.Type;
                    break;

                case TypeRefSymbol typeReference:
                    r = CreateType("Type");
                    break;

                case RefSymbol reference:
                    throw new NotImplementedException();

                case ArrayLiteral arrayLiteral:
                    r = CreateArrayType(arrayLiteral.Expressions);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(expr));
            }

            if (r == null)
                throw new Exception($"No type expression was able to be determined for {expr}");

            Types.Add(expr, r);
            return r;
        }
    }
}
