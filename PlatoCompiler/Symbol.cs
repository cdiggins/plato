using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Plato.Compiler
{
    public abstract class Symbol
    {
        public override string ToString() => $"{GetType().Name}";
        public abstract IReadOnlyList<Symbol> Children { get; }
        public int Id { get; } = NextId++;  
        public static int NextId = 0;
        public override bool Equals(object obj) => obj is Symbol s && s.Id == Id;
        public override int GetHashCode() => Id.GetHashCode();
    }

    public class MemberGroupSymbol : DefSymbol
    {
        public IReadOnlyList<MemberDefSymbol> Members { get;  }
        
        public MemberGroupSymbol(IReadOnlyList<MemberDefSymbol> members, string name)
            : base(null, name)
        {
            if (members.Count == 0) throw new Exception("Expected at least one function in group");
            Members = members;

            foreach (var m in Members)
            {
                if (m == null)
                    throw new Exception("Null member");
                if (m.Function == null)
                    throw new Exception("Member without function");
                if (m.Name != name)
                    throw new Exception($"All members in group must have the name \"{name}\" not \"{m.Name}\"");
                var sig = m.Function.Signature;
                if (Members.Count(m2 => m2.Function.Signature == sig) > 1)
                    throw new Exception($"More than one member has signature {sig}");
            }
        }

        public MemberGroupSymbol Add(MemberDefSymbol function)
            => new MemberGroupSymbol(Members.Append(function).ToList(), Name);

        public override IReadOnlyList<Symbol> Children 
            => Array.Empty<Symbol>();

        public string DebugString => 
            string.Join(";", Members.Select(m => m?.Function?.Signature));
    }

    public abstract class DefSymbol : Symbol
    {
        protected DefSymbol(TypeRefSymbol type, string name)
        {
            Type = type;
            Name = name;
        }

        public TypeRefSymbol Type { get; }
        public string Name { get; }
        public string UniqueName => Name + "_" + Id;

        public override string ToString() => $"{GetType().Name}={Name}${Id}:{Type}";
    }

    public class RefSymbol : Symbol
    {
        public DefSymbol Def { get; }
        public TypeRefSymbol Type => Def.Type;
        public string Name => Def.Name;

        public RefSymbol(DefSymbol def)
        {
            if (def is TypeDefSymbol tds)
                throw new Exception("Unexpected type definition reference");
            Def = def ?? throw new Exception("No definition provided");
        }

        public override string ToString() => $"Ref->{Def}";
        public override IReadOnlyList<Symbol> Children => Array.Empty<Symbol>();
    }

    public class PredefinedSymbol : DefSymbol
    {
        public PredefinedSymbol(TypeRefSymbol typeRef, string name)
            : base(typeRef, name)
        { }

        public override IReadOnlyList<Symbol> Children => Array.Empty<Symbol>();
    }

    public static class PrimitiveTypes
    {
        public static TypeDefSymbol Lambda = Create("Lambda");
        public static TypeDefSymbol Function = Create("Function");
        public static TypeDefSymbol Self = Create("Self");
        public static TypeDefSymbol Tuple = Create("Tuple");
        public static TypeDefSymbol Error = Create("Error");

        public static TypeDefSymbol Create(string name)
            => new TypeDefSymbol(TypeKind.Primitive, name);

        public static string GetPrimNameFromType(Type type)
        {
            if (type == null) return "Any";
            if (type.Equals(typeof(int))) return "Integer";
            if (type.Equals(typeof(float))) return "Number";
            if (type.Equals(typeof(double))) return "Number";
            if (type.Equals(typeof(bool))) return "Boolean";
            if (type.Equals(typeof(string))) return "String";
            throw new NotSupportedException(type.Name);
        }
    }

    public class TypeDefSymbol : DefSymbol
    {
        public TypeKind Kind { get; }

        public IEnumerable<FunctionSymbol> Functions => Enumerable.Empty<FunctionSymbol>()
            .Concat(Methods.Select(m => m.Function))
            .Concat(Fields.Select(f => f.Function));

        public List<MethodDefSymbol> Methods { get; } = new List<MethodDefSymbol>();
        public List<FieldDefSymbol> Fields { get; } = new List<FieldDefSymbol>();
        public List<TypeParameterDefSymbol> TypeParameters { get; } = new List<TypeParameterDefSymbol>();
        public List<TypeRefSymbol> Inherits { get; } = new List<TypeRefSymbol>();
        public List<TypeRefSymbol> Implements { get; } = new List<TypeRefSymbol>();
        public Dictionary<string, AstNode> Lookup { get; } = new Dictionary<string, AstNode>();

        public TypeDefSymbol(TypeKind kind, string name)
            : base(null, name)
        {
            Kind = kind;
        }

        public IEnumerable<TypeDefSymbol> GetSelfAndAllInheritedTypes()
            => Inherits.SelectMany(c => c.Def.GetSelfAndAllInheritedTypes()).Append(this);

        public IEnumerable<TypeRefSymbol> GetAllImplementedConcepts()
        {
            foreach (var tmp in Implements)
            {
                if (tmp == null)
                {
                    // TODO: move to semantic checker 
                    Debug.WriteLine("TODO: Implements should not have null types");
                    continue;
                }

                yield return tmp;

                if (tmp.Def != null)
                    foreach (var tmp2 in tmp.Def.GetAllImplementedConcepts())
                        yield return tmp2;
            }

            foreach (var tmp in Inherits)
            {
                if (tmp == null)
                {
                    // TODO: move to semantic checker 
                    Debug.WriteLine("TODO: Inherits should not have null types");
                    continue;
                }

                yield return tmp;
                if (tmp.Def != null)
                    foreach (var tmp2 in tmp.Def.GetAllImplementedConcepts())
                        yield return tmp2;
            }
        }

        public override IReadOnlyList<Symbol> Children 
            => Array.Empty<Symbol>().Concat(Methods).Concat(Fields).Concat(TypeParameters).ToList();

        public IEnumerable<MemberDefSymbol> Members => Enumerable.Empty<MemberDefSymbol>()
            .Concat(Methods).Concat(Fields);

        public IEnumerable<MethodDefSymbol> GetConceptMethods()
            => GetAllImplementedConcepts().SelectMany(c => c?.Def?.Methods ?? Enumerable.Empty<MethodDefSymbol>());

        public TypeRefSymbol ToRef() => new TypeRefSymbol(this);
    }

    public class ConditionalExpressionSymbol : Symbol
    {
        public Symbol Condition { get; }
        public Symbol IfTrue { get; }
        public Symbol IfFalse { get; }

        public ConditionalExpressionSymbol(Symbol condition, Symbol ifTrue, Symbol ifFalse)
        {
            Condition = condition;
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }

        public override IReadOnlyList<Symbol> Children => new []{ Condition, IfTrue, IfFalse};
    }

    public class FunctionSymbol : DefSymbol
    {
        public IReadOnlyList<ParameterSymbol> Parameters { get; }
        public Symbol Body { get; }

        public FunctionSymbol(string name, TypeRefSymbol returnType, Symbol body, params ParameterSymbol[] parameters)
            : base(returnType, name)
        {
            Parameters = parameters;
            Body = body;
        }

        public override IReadOnlyList<Symbol> Children
            => Body != null
                ? Array.Empty<Symbol>().Concat(Parameters).Append(Body).ToList()
                : Parameters.Cast<Symbol>().ToList();


        public string Signature => 
            $"{Name}(" 
            + string.Join(",", Parameters.Select(p => $"{p.Name}:{p.Type}")) 
            + $"):{Type};";

    }

    public class ParameterSymbol : DefSymbol
    {   
        public ParameterSymbol(string name, TypeRefSymbol type)
            : base(type, name)
        { }

        public override IReadOnlyList<Symbol> Children
            => Array.Empty<Symbol>();
    }

    public class VariableSymbol : DefSymbol
    {
        public VariableSymbol(string name, TypeRefSymbol type)
            : base(type, name) { }

        public override IReadOnlyList<Symbol> Children 
            => Array.Empty<Symbol>();
    }

    public class AssignmentSymbol : Symbol
    {
        public Symbol LValue { get; }
        public Symbol RValue { get; }

        public AssignmentSymbol(Symbol lvalue, Symbol rvalue)
            => (LValue, RValue) = (lvalue, rvalue);

        public override IReadOnlyList<Symbol> Children 
            => new [] { LValue, RValue };
    }

    public class ArgumentSymbol : Symbol
    {
        public Symbol Original { get; }
        public int Position { get; }

        public ArgumentSymbol(Symbol original, int position)
        {
            Original = original;
            Position = position;
        }

        public override IReadOnlyList<Symbol> Children => new[] { Original };
    }

    public class LiteralSymbol : Symbol
    {
        public object Value { get; }
        public LiteralTypes LiteralType { get; }
        public LiteralSymbol(LiteralTypes type, object value) => (LiteralType, Value) = (type, value);
        public override IReadOnlyList<Symbol> Children => Array.Empty<Symbol>();
    }

    public class FunctionCallSymbol : Symbol
    {
        public Symbol Function { get; }
        public IReadOnlyList<ArgumentSymbol> Args { get; }
        public FunctionCallSymbol(Symbol function, params ArgumentSymbol[] args)
        {
            Function = function; 
            if (Function == null)
                Debug.WriteLine("Unexpected empty function");
            Args = args;
        }

        public override IReadOnlyList<Symbol> Children => Args.Append(Function).ToList();
    }

    public class TypeRefSymbol : Symbol
    {
        public string Name => Def?.Name ?? "unresolved";
        public TypeDefSymbol Def { get; }
        public IReadOnlyList<TypeRefSymbol> TypeArgs { get; }

        public TypeRefSymbol(TypeDefSymbol def, params TypeRefSymbol[] args)
        {
            Def = def ?? throw new Exception("Type not found");
            TypeArgs = args;
        }

        public override IReadOnlyList<Symbol> Children => TypeArgs;

        public override string ToString()
        {
            var kind = Def?.Kind.ToString() ?? "";
            if (TypeArgs.Count > 0) 
                return kind + ":" + Name + $"<{string.Join(",", TypeArgs)}>";
            return kind + ":" + Name;
        }

        public static TypeRefSymbol CreateFunction(params TypeRefSymbol[] types)
            => new TypeRefSymbol(PrimitiveTypes.Function, types);
    }

    public abstract class MemberDefSymbol : DefSymbol
    {
        protected MemberDefSymbol(TypeDefSymbol parentType, TypeRefSymbol type, string name)
            : base(type, name)
        {
            ParentType = parentType;
        }

        public TypeDefSymbol ParentType { get; }

        public override IReadOnlyList<Symbol> Children
            => new[] { Function };

        public FunctionSymbol Function { get; set; }
    }

    public class FieldDefSymbol : MemberDefSymbol
    {
        public FieldDefSymbol(TypeDefSymbol parentType, TypeRefSymbol type, string name)
            : base(parentType, type, name)
        {
            Function = new FunctionSymbol(Name, Type, ToRefSymbol(),
                new ParameterSymbol("self", parentType.ToRef()));
        }

        public RefSymbol ToRefSymbol()
            => new RefSymbol(this);
    }

    public class MethodDefSymbol : MemberDefSymbol
    {
        public MethodDefSymbol(TypeDefSymbol parentType, TypeRefSymbol type, string name) 
            : base(parentType, type, name)
        { }
    }

    public class TypeParameterDefSymbol : TypeDefSymbol
    {
        public TypeParameterDefSymbol(string name, TypeRefSymbol constraint)
            : base(TypeKind.Variable, name)
            => Constraint = constraint;
        public TypeRefSymbol Constraint { get; }
        public override IReadOnlyList<Symbol> Children
            => Array.Empty<Symbol>();
    }
}