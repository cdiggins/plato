using System;

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
    public class MethodIR { }
    public class MethodInstanceIR { }
    public class LambdaIR : ExpressionIR {  }
    public class DeclarationIR { }
    public class StatementIR : DeclarationIR { }
    public class WhileStatementIR : StatementIR { }
    public class ExpressionIR { }
    public class InvocationIR : ExpressionIR { }
    public class ConditionalExpressionIR : ExpressionIR { }
    public class AssignmentIR : StatementIR { }
    public class IfStatementIR : StatementIR { }
    public class ReturnStatementIR : StatementIR { }
    public class MultiStatementIR : StatementIR { }
    public class TypeIR : DeclarationIR {  }
    public class LocalIR : StatementIR { }
    public class EndScopeIR : StatementIR { }
    public class ParameterIR : DeclarationIR { }
    public class FieldIR : DeclarationIR { }
    public class GenericTypeInstanceIR : TypeIR { }
    public class GenericTypeIR : TypeIR { }
    public class PrimitiveTypeIR : TypeIR { }
    public class LiteralIR : ExpressionIR { }
}
