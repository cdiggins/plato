using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Ptarmigan.Utils;

namespace PlatoTypeInference
{
    public class Environment
    {
    }

    public class Reference
    {
        public int ScopeDistance { get; }
    }

    public class Type
    {
    } 

    public class Variable : INamed
    {
        // Variable belong to something. A type, a scope.
        public int Index { get; } 
        public string Name { get; }
        public TypeExpr TypeExpr { get; }
    }

    public class TypeParameter : Variable
    {
    }

    public class Parameter : Variable
    {
        public bool HasDefaultValue { get; }
        public object DefaultValue { get; }
    }

    public class TypeExpr : Variable
    {
        public IReadOnlyList<TypeExpr> TypeParameters { get; }
    }

    public class Function : Variable
    {
        public IReadOnlyList<TypeParameter> TypeParameters { get; }
        public IReadOnlyList<Variable> LocalVariables {get;}
        public IReadOnlyList<Variable> Parameters { get; }
        public IReadOnlyList<Variable> NonLocalVariables { get; }
        public IReadOnlyList<Function> NestedFunctions { get; }
    }

    // A literal could be a function invocation
    // An operator could be a function invocation.
    // The only thing that is not a function invocation is a conditional expression or a variable.
    // Actually a conditional expression can also be a function invocation: it's just that the arguments are conditionally evaluated.
    // A member access is a function invocation. 
    
    // What is the advantage? Well it makes implementation a bit simpler. We normalize everything into a function call. 
    // The only question becomes: what is the type of the result, when is it called, and when can it be inlined. 
    // The nice thing is that we start to get some patterns that are just function calls. 
    // And we have everything explicit: including the "this".
    
    // The last question becomes: are some functions known?
    // What about things like method groups? There are multiple functions that could be chosen.
    // They vary based on the types of the input parameters. 
    // I could omit them for now. 
    
    // Right now the last problem is: what do I do about closures?
    // Closures capture the lexical environment. So to evaluate them I need the variables they refer to.
    // They basically 

    // So a closure, is a special function that returns a function. It takes a list of variables ... 
    

    public class Expression
    {
        public Type Type { get; }
        public IReadOnlyList<Expression> Children { get; }
    }

    public class Assignment : Expression
    {
        public Variable Variable { get; }
        public Expression Expression { get; }
    }

    // The resolution of a member might not be known until run-time
    public class MemberGet : Expression
    {
        public Expression Receiver => Children[0];
        public string Name { get; }
    }

    public class MemberSet :  MemberGet
    {
        public Expression Value => Children[1];
    }

    public class Scope
    {
        public int Depth { get; }
    }

    public class LexicalEnvironment 
    {
        public IStack<EnvironmentRecord> Records { get; }
    }

    public class EnvironmentRecord
    {
        public Scope Scope { get; }
        //public IStack<Binding> Bindings { get; }
    }
    
    public class NameAnalysis
    {
        // Get the names of the types.
        // Get the names of the type parameters.
        // Scope
    }
    
}
