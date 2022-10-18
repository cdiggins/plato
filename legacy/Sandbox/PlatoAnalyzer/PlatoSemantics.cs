using System.Collections.Generic;

namespace PlatoAnalyzer
{
    public class Semantic : ISemantic
    {
        public ISemantic Parent { get; set; }
        public IList<ISemantic> Children { get; set; } = new List<ISemantic>();
    }

    public class Ref : Semantic, IRef
    {
        public IDef Def { get; set; }
    }

    public class Def : Semantic, IDef
    {
        public IList<IRef> Refs { get; set; } = new List<IRef>();
    }

    public class TypeRef : NameRef, ITypeRef
    {
        public IList<ITypeRef> TypeArgs { get; set; } = new List<ITypeRef>();
    }

    public class NameRef : Ref, INameRef
    {
        public string Name { get; set; }
    }

    public class NameDef : Def, INameDef
    {
        public string Name { get; set; }
    }

    public class TypedNameDef : NameDef, ITypedNameDef
    {
        public ITypeRef Type { get; set; }
    }

    public class TypeDef : NameDef, ITypeDef
    {
        public IList<ITypeParam> TypeParams { get; } = new List<ITypeParam>();
    }

    public class TypeParam : NameDef, ITypeParam
    {
    }

    public class Expression : Semantic, IExpression
    {
        public ITypeRef Type { get; set; }
    }

    public class VarRef : NameRef, IVarRef
    {
        public ITypeRef Type { get; set; }
    }

    public class Array : Expression, IArray
    {
        public ITypeRef ElementType { get; set; }
        public IExpression Size { get; set; }
        public IList<IExpression> Elements { get; set; } = new List<IExpression>();
    }

    public class Op : Expression, IOp
    {
        public string Operator { get; set; }
        public IList<IExpression> Operands { get; set; } = new List<IExpression>();
        public IExpression this[int index] => Operands[index];

        public Op(string op = null)
            => Operator = op ?? "_NOOP_";
    }

    public class Assignment : Op, IAssignmentOp
    {
    }

    public class PrefixOp : Op, IPrefixOp
    {
    }

    public class PostfixOp : Op, IPostfixOp
    {
    }

    public class BinaryOp : Op, IBinaryOp
    {
    }

    public class ConditionalOp : Op, IConditionalOp
    {
        public ConditionalOp() : base("?")
        {
        }
    }

    public class CastOp : Op, ICastOp
    {
        public CastOp() : base("as")
        {
        }
    }

    public class TypeOfOp : Op, ITypeOfOp
    {
        public TypeOfOp() : base("typeof")
        {
        }
    }

    public class DefaultOp : Op, IDefaultOp
    {
        public DefaultOp() : base("default")
        {
        }
    }

    public class ThisOp : Op, IThisOp
    {
        public ThisOp() : base("this")
        {
        }
    }

    public class IndexOp : Op, IIndexOp
    {
        public IndexOp() : base("[]")
        {
        }
    }

    public class TupleOp : Op, ITupleOp
    {
        public TupleOp() : base("(,)")
        {
        }
    }

    public class ParenthesizedOp : Op, IParenthesizedOp
    {
        public ParenthesizedOp() : base("()")
        {
        }
    }

    public class NewOp : Op, INewOp
    {
        public NewOp() : base("new")
        { }

        public IFunctionRef ConstructorRef { get; set; }
        public IList<IExpression> Args { get; set; }
        public IExpression Receiver { get; set; }
        public IList<IArg> Initializers { get; set; }
    }

    public class ThrowOp : Op, IThrowOp
    {
        public ThrowOp() : base("throw")
        {
        }
    }

    public class Literal : Expression, ILiteral
    {
        public object Value { get; set; }
    }

    public class Arg : Semantic, IArg
    {
        public IExpression Value { get; set; }
        public IVarRef Parameter { get; set; }
        public string Name { get; set; }
    }

    public class MemberRef : NameRef, IMemberRef
    {
        public IExpression Reciever { get; set; }
        public ITypeRef Type { get; set; }
    }

    public class Parameter : VarDef, IParameter
    {
        public int Index { get; set; }
    }

    public class FunctionDef : TypeDef, INameDef, IFunctionDef
    {
        public IList<IParameter> Parameters { get; set; }
        public IStatement Body { get; set; }
        public ITypeRef ReceiverType { get; set; }
        public ITypeRef ReturnType { get; set; }
    }

    public class FunctionRef : NameRef, IFunctionRef
    {
        public IList<ITypeRef> TypeArgs { get; set; } = new List<ITypeRef>();
        public ITypeRef Type { get; set; }
    }

    public class Invocation : Expression, IInvocation
    {
        public IExpression Function { get; set; }
        public IList<IExpression> Args { get; set; } = new List<IExpression>();
        public IExpression Receiver { get; set; }
    }

    public class VarDef : TypedNameDef
    {
    }

    public class Lambda : Expression, ILambda
    {
        public IFunctionDef Function { get; set; }

        // NOTE: we have to make sure lambda don't capture arguments  
        public IList<IVarRef> CapturedVariables { get; set; } = new List<IVarRef>();
    }

    public class Statement : Semantic, IStatement
    {
        public IList<IVarDef> DeclaredVariables { get; set; } = new List<IVarDef>();
        public IList<IVarRef> ReferencedVariables { get; set; } = new List<IVarRef>();
    }

    public class BlockStatement : Statement, IBlockStatement
    {
        public IList<IStatement> Statements { get; set; } = new List<IStatement>();
    }

    public class CompoundStatement : Statement, ICompoundStatement
    {
        public IList<IStatement> Statements { get; set; } = new List<IStatement>();
    }

    public class VarDeclStatement : Statement, IVarDeclStatement
    {
        public IVarDeclaration Declaration { get; set; }
    }

    public class ExpressionStatement : Statement, IExpressionStatement
    {
        public IExpression Expression { get; set; }
    }

    public class ReturnStatement : ExpressionStatement, IReturnStatement
    {
    }

    public class BreakStatement : Statement, IBreakStatement
    {
    }

    public class ContinueStatement : Statement, IContinueStatement
    {
    }

    public class EmptyStatement : Statement, IEmptyStatement
    {
    }

    public class ConditionalStatement : Statement, IConditionalStatement
    {
        public IExpression Condition { get; set; }
        public IStatement OnTrue { get; set; }
        public IStatement OnFalse { get; set; }
    }

    public class WhileStatement : Statement, IWhileStatement
    {
        public IExpression Condition { get; set; }
        public IStatement Body { get; set; }
    }

    public class VarDeclaration : VarDef, IVarDeclaration
    {
        public IExpression Initializer { get; set; }
    }

    public class MemberDef : TypedNameDef, IMemberDef
    {
    }

    public class Class : TypeDef, IClass
    {
        public IList<IMemberDef> Members { get; set; } = new List<IMemberDef>();
    }

    public class Method : MemberDef, IMethod
    {
        public IFunctionDef Function { get; set; }
    }

    public class Field : MemberDef, IField
    {
        public IVarDeclaration Variable { get; set; }
    }

    public class Property : MemberDef, IProperty
    {
        public IFunctionDef Getter { get; set; }
        public IFunctionDef Setter { get; set; }
    }
}
