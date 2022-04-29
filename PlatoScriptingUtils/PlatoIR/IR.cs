using System;
using System.Collections.Generic;

/*
 *  PlatoIR code is an intermediate language used to generate outputs in different languages (e.g. .NET IL, 
 *  JavaScript, and Emu graphs)
 *
 *  Break and continue statements require a new variable to be introduced to control the
 *  loop structure. 
 *  
 *  Early return statements (e.g. in a loop) are similar, they require variables to be added.
 *  How do I remove them? In generated code it shouldn't be necessary. It doesn't really matter ...
 *
 *  Switch statements can be converted to switch expressions. They can be rewritten using 
 *  lambdas. Getting performance back might be tricky. 
 *  
 *  This format should be easy to create and modify. 
 *  
 *  How do I analyze it? Ideally analysus is separate. 
 */

namespace PlatoIR
{

    public class IR
    { 
        public static Dictionary<int, IR> Lookup { get; } = new Dictionary<int, IR>();
        public int Id { get; }
        public IR()
        {
            Id = Lookup.Count;
            Lookup.Add(Id, this);
        }
        public SourceLocation Source { get; }
    }

    /// <summary>
    /// Basically anything that isn't an expression. 
    /// </summary>
    public class DeclarationIR : IR
    { }

    public class NamedIR : DeclarationIR
    {
        public string Name { get; set; }
    }

    public class TypedNamedIR : NamedIR
    {
        public TypeDefinitionIR Type { get; }
    }

    public class FunctionIR : TypedNamedIR
    {
        public List<ParameterIR> Parameters { get; } = new List<ParameterIR>();
        public StatementIR Body { get; set; }
        public List<TypeParameterIR> TypeParameters { get; } = new List<TypeParameterIR>();
        public bool IsStatic { get; set; }
        public TypeDefinitionIR ReturnType { get; set; }
    }

    public class MethodIR : FunctionIR
    {
    }

    public class ConstructorIR : FunctionIR
    {
        public TypeDefinitionIR Type { get; set; }
    }

    public class LocalFunctionIR : FunctionIR
    { }

    public class ClassIR : TypeDefinitionIR
    {
        public ClassIR BaseClass { get; set; }
        public List<InterfaceIR> Interface { get; } = new List<InterfaceIR>(); 
        public List<FieldIR> Fields { get; } = new List<FieldIR>();
        public List<MethodIR> Methods { get; } = new List<MethodIR>();
        public List<ConstructorIR> Constructors { get; } = new List<ConstructorIR>();
        public List<PropertyIR> Properties { get; } = new List<PropertyIR>();
        public List<IndexerIR> Indexers { get; } = new List<IndexerIR>();
    }

    public class InterfaceIR : TypeDefinitionIR
    {
        public List<InterfaceIR> Interfaces { get; } = new List<InterfaceIR>();
    }

    public class FieldIR : TypedNamedIR
    {
        public bool IsStatic { get; set; }
    }

    public class PropertyIR : TypedNamedIR 
    {
        public MethodIR Getter { get; set; }
        public bool HasInit { get; set; }
        public bool IsStatic { get; set; }
    }

    public class IndexerIR : TypedNamedIR 
    {
        public MethodIR Getter { get; set; }
    }

    public class ConverterIR : TypedNamedIR
    {
        public MethodIR Method { get; set; }
        public bool IsImplicit { get; set; }
    }
    
    public class StatementIR : DeclarationIR { }

    public class WhileStatementIR : StatementIR 
    {
        public StatementIR Body { get; set; }
        public ExpressionIR Condition { get; set; }

    }
    public class ExpressionIR : IR 
    {
        public TypeDefinitionIR Type { get; set; }
    }

    public class ThrowExpressionIR : IR
    {
        public ExpressionIR Expression { get; set; }
    }

    public class InvocationIR : ExpressionIR 
    {
        public ExpressionIR Method { get; set; }
        public List<ExpressionIR> Arguments { get; } = new List<ExpressionIR>();
    }
    
    public class ConditionalExpressionIR : ExpressionIR 
    {
        public ExpressionIR Condition { get; set; }
        public ExpressionIR OnTrue { get; set; }
        public ExpressionIR OnFalse { get; set; }
    }

    public class LocalIR : TypedNamedIR
    {
        public ExpressionIR Value { get; set; }
    }

    public abstract class MemberReferenceIR : IR
    {
        public ExpressionIR ThisObject { get; set; }
        public abstract string Name { get; }
    }

    public class PropertyReferenceIR : MemberReferenceIR
    {
        public override string Name => Property.Name;
        public PropertyIR Property { get; set; }
    }

    public class FieldReferenceIR : MemberReferenceIR
    {
        public override string Name => Field.Name;
        public FieldIR Field { get; set; }
    }

    public class MethodReferenceIR : MemberReferenceIR
    {
        public override string Name => Method.Name;
        public MethodIR Method { get; set; }
        public List<TypeDefinitionIR> TypeArguments { get; } = new List<TypeDefinitionIR>();
    }

    public class TypeReferenceIR : ExpressionIR
    {
        public List<TypeDefinitionIR> TypeArguments { get; } = new List<TypeDefinitionIR>();
    }

    public class LocalReferenceIR : ExpressionIR
    {
        public LocalIR Local { get; set; }
    }

    public class ParameterReferenceIR : ExpressionIR
    {
        public ParameterIR Paramter { get; set; }
    }

    public class LValueIR : IR
    {
        public ParameterReferenceIR Parameter { get; set; }
        public LocalReferenceIR Local { get; set; }
        public FieldReferenceIR Field { get; set; }
    }
    
    public class AssignmentIR : StatementIR 
    {
        public LValueIR LValue;
        public ExpressionIR RValue;
    }
    
    public class ArgumentIR : IR
    {
        public string Name { get; set; }
        public ExpressionIR Value { get; }
    }

    public class NewExpression : ExpressionIR
    {
        public ConstructorIR Constructor { get; set; }
        public List<ArgumentIR> InitializerArguments { get; }
        public List<ArgumentIR> CtorArguments { get; }
    }

    public class ThrowStatement : StatementIR 
    {
        public ThrowExpressionIR ThrowExpresion;
    }

    public class IfStatementIR : StatementIR 
    {
        public ExpressionIR Condition { get; set; }
        public StatementIR OnTrue { get; set; }
        public StatementIR OnFalse { get; set; }
    }
   
    public class ReturnStatementIR : StatementIR 
    { 
        public ExpressionIR Expression { get; set; }
    }

    public class MultiStatementIR : StatementIR 
    {
        public List<StatementIR> Statements { get; } = new List<StatementIR>();
    }

    public class TypeDefinitionIR : NamedIR
    {
        public List<TypeParameterIR> TypeParameters { get; } = new List<TypeParameterIR>();
    }

    public class TypeParameterIR : NamedIR 
    { }

    public class ParameterIR : TypedNamedIR 
    { }

    public class PrimitiveTypeIR : TypeDefinitionIR { }
    
    public class LiteralIR : ExpressionIR 
    {
        public object Value;
    }

    public class TupleIR : ExpressionIR { }
    public class InterpolatedStringIR : ExpressionIR { }
    public class CastIR : ExpressionIR { }
    public class ArrayIR : ExpressionIR { }

    public class LambdaIR : ExpressionIR
    {
        public List<ParameterIR> CapturedVariables { get; } = new List<ParameterIR>();

        public MethodReferenceIR Method { get; set; }
    }

}
