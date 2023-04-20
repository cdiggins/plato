using System;
using System.Collections.Generic;
using System.Text;
using Parakeet;
using Parakeet.Demos.CSharp;

namespace PlatoAst
{
    public class AstFromCst
    {
        public AstNode FromCst(CstNode node)
        {
            switch (node)
            {
                case CstAccessSpecifier cstAccessSpecifier:
                    break;
                case CstArrayInitializationValue cstArrayInitializationValue:
                    break;
                case CstArrayRankSpecifier cstArrayRankSpecifier:
                    break;
                case CstArrayRankSpecifiers cstArrayRankSpecifiers:
                    break;
                case CstArraySizeSpecifier cstArraySizeSpecifier:
                    break;
                case CstAsOperation cstAsOperation:
                    break;
                case CstAttribute cstAttribute:
                    break;
                case CstAttributeList cstAttributeList:
                    break;
                case CstBaseCall cstBaseCall:
                    break;
                case CstBaseClassList cstBaseClassList:
                    break;
                case CstBaseOrThisCall cstBaseOrThisCall:
                    break;
                case CstBinaryLiteral cstBinaryLiteral:
                    break;
                case CstBinaryOperation cstBinaryOperation:
                    break;
                case CstBinaryOperator cstBinaryOperator:
                    break;
                case CstBooleanLiteral cstBooleanLiteral:
                    break;
                case CstBracedStructure cstBracedStructure:
                    break;
                case CstBracketedStructure cstBracketedStructure:
                    break;
                case CstBreakStatement cstBreakStatement:
                    break;
                case CstCaseClause cstCaseClause:
                    break;
                case CstCastExpression cstCastExpression:
                    break;
                case CstCatchClause cstCatchClause:
                    break;
                case CstCharLiteral cstCharLiteral:
                    break;
                case CstCompoundStatement cstCompoundStatement:
                    break;
                case CstCompoundTypeExpr cstCompoundTypeExpr:
                    break;
                case CstConditionalMemberAccess cstConditionalMemberAccess:
                    break;
                case CstConstraint cstConstraint:
                    break;
                case CstConstraintClause cstConstraintClause:
                    break;
                case CstConstraintList cstConstraintList:
                    break;
                case CstConstructorDeclaration cstConstructorDeclaration:
                    break;
                case CstContinueStatement cstContinueStatement:
                    break;
                case CstConverterDeclaration cstConverterDeclaration:
                    break;
                case CstDeclarationPreamble cstDeclarationPreamble:
                    break;
                case CstDefault cstDefault:
                    break;
                case CstDefaultValue cstDefaultValue:
                    break;
                case CstDoWhileStatement cstDoWhileStatement:
                    break;
                case CstElement cstElement:
                    break;
                case CstElseClause cstElseClause:
                    break;
                case CstExpression cstExpression:
                        break;
                case CstExpressionBody cstExpressionBody:
                    break;
                case CstExpressionStatement cstExpressionStatement:
                    break;
                case CstFieldDeclaration cstFieldDeclaration:
                    break;
                case CstFile cstFile:
                    break;
                case CstFileStructure cstFileStructure:
                    break;
                case CstFinallyClause cstFinallyClause:
                    break;
                case CstFloatLiteral cstFloatLiteral:
                    break;
                case CstForEachStatement cstForEachStatement:
                    break;
                case CstForStatement cstForStatement:
                    break;
                case CstFunctionArg cstFunctionArg:
                    break;
                case CstFunctionArgKeyword cstFunctionArgKeyword:
                    break;
                case CstFunctionArgs cstFunctionArgs:
                    break;
                case CstFunctionBody cstFunctionBody:
                    break;
                case CstFunctionParameter cstFunctionParameter:
                    break;
                case CstFunctionParameterKeywords cstFunctionParameterKeywords:
                    break;
                case CstFunctionParameterList cstFunctionParameterList:
                    break;
                case CstGetter cstGetter:
                    break;
                case CstHexLiteral cstHexLiteral:
                    break;
                case CstIdentifier cstIdentifier:
                    break;
                case CstIfStatement cstIfStatement:
                    break;
                case CstImplicitOrExplicit cstImplicitOrExplicit:
                    break;
                case CstIndexer cstIndexer:
                    break;
                case CstIndexerDeclaration cstIndexerDeclaration:
                    break;
                case CstInitialization cstInitialization:
                    break;
                case CstInitializationClause cstInitializationClause:
                    break;
                case CstInitializationValue cstInitializationValue:
                    break;
                case CstInitializer cstInitializer:
                    break;
                case CstInitializerClause cstInitializerClause:
                    break;
                case CstInitter cstInitter:
                    break;
                case CstInnerTypeExpr cstInnerTypeExpr:
                    break;
                case CstIntegerLiteral cstIntegerLiteral:
                    break;
                case CstInvariantClause cstInvariantClause:
                    break;
                case CstIsOperation cstIsOperation:
                    break;
                case CstKind cstKind:
                    break;
                case CstLambdaBody cstLambdaBody:
                    break;
                case CstLambdaExpr cstLambdaExpr:
                    break;
                case CstLambdaParameter cstLambdaParameter:
                    break;
                case CstLambdaParameters cstLambdaParameters:
                    break;
                case CstLeafExpression cstLeafExpression:
                    break;
                case CstLiteral cstLiteral:
                    break;
                case CstMemberAccess cstMemberAccess:
                    break;
                case CstMemberDeclaration cstMemberDeclaration:
                    break;
                case CstMethodDeclaration cstMethodDeclaration:
                    break;
                case CstModifier cstModifier:
                    break;
                case CstNameOf cstNameOf:
                    break;
                case CstNamespaceDeclaration cstNamespaceDeclaration:
                    break;
                case CstNewOperation cstNewOperation:
                    break;
                case CstNullable cstNullable:
                    break;
                case CstNullLiteral cstNullLiteral:
                    break;
                case CstOperatorDeclaration cstOperatorDeclaration:
                    break;
                case CstOverloadableOperator cstOverloadableOperator:
                    break;
                case CstParenthesizedExpression cstParenthesizedExpression:
                    break;
                case CstParenthesizedStructure cstParenthesizedStructure:
                    break;
                case CstPostfixOperator cstPostfixOperator:
                    break;
                case CstPropertyBody cstPropertyBody:
                    break;
                case CstPropertyClauses cstPropertyClauses:
                    break;
                case CstPropertyDeclaration cstPropertyDeclaration:
                    break;
                case CstPropertyExpression cstPropertyExpression:
                    break;
                case CstPropertyWithClauses cstPropertyWithClauses:
                    break;
                case CstQualifiedIdentifier cstQualifiedIdentifier:
                    break;
                case CstReturnStatement cstReturnStatement:
                    break;
                case CstSeparator cstSeparator:
                    break;
                case CstSetter cstSetter:
                    break;
                case CstStatement cstStatement:
                    break;
                case CstStatementKeyword cstStatementKeyword:
                    break;
                case CstStatementStructure cstStatementStructure:
                    break;
                case CstStatic cstStatic:
                    break;
                case CstStringInterpolation cstStringInterpolation:
                    break;
                case CstStringInterpolationContent cstStringInterpolationContent:
                    break;
                case CstStringLiteral cstStringLiteral:
                    break;
                case CstStructure cstStructure:
                    break;
                case CstSwitchStatement cstSwitchStatement:
                    break;
                case CstTernaryOperation cstTernaryOperation:
                    break;
                case CstThisCall cstThisCall:
                    break;
                case CstThrowExpression cstThrowExpression:
                    break;
                case CstToken cstToken:
                    break;
                case CstTokenGroup cstTokenGroup:
                    break;
                case CstTryStatement cstTryStatement:
                    break;
                case CstTypeArgList cstTypeArgList:
                    break;
                case CstTypeDeclaration cstTypeDeclaration:
                    break;
                case CstTypeDeclarationWithPreamble cstTypeDeclarationWithPreamble:
                    break;
                case CstTypeExpr cstTypeExpr:
                    break;
                case CstTypeKeyword cstTypeKeyword:
                    break;
                case CstTypeOf cstTypeOf:
                    break;
                case CstTypeParameter cstTypeParameter:
                    break;
                case CstTypeParameterList cstTypeParameterList:
                    break;
                case CstTypeStructure cstTypeStructure:
                    break;
                case CstTypeVariance cstTypeVariance:
                    break;
                case CstUnaryOperator cstUnaryOperator:
                    break;
                case CstUsingDirective cstUsingDirective:
                    break;
                case CstValueLiteral cstValueLiteral:
                    break;
                case CstVarDecl cstVarDecl:
                    break;
                case CstVarDeclStatement cstVarDeclStatement:
                    break;
                case CstVariantClause cstVariantClause:
                    break;
                case CstWhileStatement cstWhileStatement:
                    break;
                case CstYieldBreak cstYieldBreak:
                    break;
                case CstYieldReturn cstYieldReturn:
                    break;
                case CstYieldStatement cstYieldStatement:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(node));
            }

            throw new NotSupportedException($"The concrete syntax node {node.GetType().Name} is not supported");
        }
    }

    public class TypeLibrary
    {

    }
}
