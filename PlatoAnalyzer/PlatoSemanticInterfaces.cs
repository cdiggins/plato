using System.Collections.Generic;

namespace PlatoAnalyzer
{
    public interface ISemantic
    {
        ISemantic Parent { get; set; }
        IList<ISemantic> Children { get; set; }
    }

    public interface IRef : ISemantic
    {
        IDef Def { get; set; }
    }

    public interface IDef : ISemantic
    {
        IList<IRef> Refs { get; set; }
    }

    public interface INameRef : IRef
    {
        string Name { get; set; }
    }

    public interface ITypeRef : INameRef
    {
        IList<ITypeRef> TypeArgs { get; set; }
    }

    public interface INameDef : ISemantic
    {
        string Name { get; set; }
    }

    public interface ITypedNameDef : INameDef
    {
        ITypeRef Type { get; }
    }

    public interface ITypeDef : INameDef
    {
        IList<ITypeParam> TypeParams { get; }
    }

    public interface ITypeParam : INameDef
    {
    }

    public interface IExpression : ISemantic
    {
        ITypeRef Type { get; set; }
    }

    public interface IVarRef : IExpression, INameRef
    {
    }

    public interface IArray : IExpression
    {
        ITypeRef ElementType { get; set; }
        IExpression Size { get; set; }
        IList<IExpression> Elements { get; set; }
    }

    public interface IOperation : IExpression
    {
        string Operator { get; set; }
        IExpression this[int index] { get; }
        IList<IExpression> Operands { get; set; }
    }

    public interface IAssignmentOp : IOperation
    {
    }

    public interface IPrefixOp : IOperation
    {
    }

    public interface IPostfixOp : IOperation
    {
    }

    public interface IBinaryOp : IOperation
    {
    }

    public interface IConditionalOp : IOperation
    {
    }

    public interface ICastOp : IOperation
    {
    }

    public interface ITypeOfOp : IOperation
    {
    }

    public interface IDefaultOp : IOperation
    {
    }

    public interface IThisOp : IOperation
    {
    }

    public interface IIndexOp : IOperation
    {
    }

    public interface ITupleOp : IOperation 
    { }

    public interface IParenthesizedOp : IOperation
    { }

    public interface INewOp : IOperation
    {
        IFunctionRef ConstructorRef { get; set; }
        IList<IExpression> Args { get; set; }
        IList<IArg> Initializers { get; set; }
    }

    public interface IThrowOp : IOperation
    { }

    public interface ILiteral : IExpression
    {
        object Value { get; set; }
    }

    public interface IArg : ISemantic
    {
        IVarRef Parameter { get; set; }
        string Name { get; set; }
        IExpression Value { get; set; }
    }

    public interface IMemberRef : IExpression, INameRef
    {
        IExpression Reciever { get; set; }
    }

    public interface IParameter : IVarDef
    {
        int Index { get; set; }
    }

    public interface IFunctionDef : ITypeDef, INameDef
    {
        ITypeRef ReceiverType { get; set; }
        ITypeRef ReturnType { get; set; }
        IList<IParameter> Parameters { get; set; }
        IStatement Body { get; set; }
    }

    public interface IFunctionRef : IRef, IExpression
    {
        IList<ITypeRef> TypeArgs { get; set; }
    }

    public interface IInvocation : IExpression
    {
        IExpression Function { get; set; }
        IList<IExpression> Args { get; set; }
        IExpression Receiver { get; set; }
    }

    public interface IVarDef : ITypedNameDef
    {
    }

    public interface ILambda : IExpression
    {
        IFunctionDef Function { get; set; }
        IList<IVarRef> CapturedVariables { get; set; }
    }

    public interface IStatement : ISemantic
    {
        IList<IVarDef> DeclaredVariables { get; set; }
        IList<IVarRef> ReferencedVariables { get; set; }
    }

    public interface IBlockStatement : IStatement
    {
        IList<IStatement> Statements { get; set; }
    }

    public interface ICompoundStatement : IStatement
    {
        IList<IStatement> Statements { get; set; }
    }

    public interface IVarDeclStatement : IStatement
    {
        IVarDeclaration Declaration { get; set; }
    }

    public interface IExpressionStatement : IStatement
    {
        IExpression Expression { get; set; }
    }

    public interface IReturnStatement : IExpressionStatement
    {
    }

    public interface IBreakStatement : IStatement { }
    public interface IContinueStatement : IStatement { }
    public interface IEmptyStatement : IStatement { }

    public interface IConditionalStatement : IStatement
    {
        IExpression Condition { get; set; }
        IStatement OnTrue { get; set; }
        IStatement OnFalse { get; set; }
    }

    public interface IWhileStatement : IStatement
    {
        IExpression Conditional { get; set; }
        IStatement Body { get; set; }
    }

    public interface IVarDeclaration : IVarDef
    {
        IExpression Initializer { get; set; }
    }

    public interface IMemberDef : INameDef
    {
        ITypeRef Type { get; set; }
    }

    public interface IClass : ITypeDef
    {
        IList<IMemberDef> Members { get; set; }
    }

    public interface IMethod : IMemberDef
    {
        IFunctionDef Function { get; set; }
    }

    public interface IField : IMemberDef
    {
        IVarDeclaration Variable { get; set; }
    }

    public interface IProperty : IMemberDef
    {
        IFunctionDef Getter { get; set; }
        IFunctionDef Setter { get; set; }
    }

    public static class ISemanticExtensions
    {
        public static IExpression Left(this IBinaryOp self)
            => self[0];

        public static IExpression Right(this IBinaryOp self)
            => self[1];
    }
}