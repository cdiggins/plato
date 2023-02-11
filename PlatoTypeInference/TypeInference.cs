using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

// A Type Inference Algorithm that provides support for full inference 
// of non-recursive higher rank polymorphic types.

namespace PlatoTypeInference
{
    public abstract class BaseType
    {
        public abstract string Name { get; }
        public override string ToString() => Name;
    }

    public class TypeVar : BaseType
    {
        public override string Name { get; }
        public TypeVar(string name) => Name = name;
    }
  
    public class TypeList : BaseType
    {
        public TypeList(IEnumerable<BaseType> types, params string[] typeParameterNames)
        {
            Types = types.ToList();
            TypeParameterNames = new HashSet<string>(typeParameterNames);
            foreach (var t in Types)
            {
                switch (t)
                {
                    case TypeVar tv:
                        TypeVariables.Add(tv.Name);
                        break;
                    case TypeList tl:
                        foreach (var fv in tl.TypeFreeVariables)
                            TypeVariables.Add(fv);
                        break;
                }
            }

            TypeFreeVariables = TypeVariables.Keys.Except(typeParameterNames).ToList();
        }
        
        public override string Name => "[]";
        public int Count => Types.Count;
        public BaseType this[int index] => Types[index];
        public IReadOnlyList<BaseType> Types { get; }
        public HashSet<string> TypeVariables { get; } = new HashSet<string>();
        public HashSet<string> TypeParameterNames { get; } = 
        public HashSet<string> TypeFreeVariables;

        public bool IsPolyType => TypeParameterNames.Count > 0;
        public string TypeParametersString => IsPolyType ? "!" + string.Join("!", TypeParameterNames) : "";
        public override string ToString() => $"{TypeParametersString}.[{string.Join(",", Types)}]";


        public static IReadOnlyList<BaseType> GatherParameters(IEnumerable<BaseType> types)
        {
            var result = new Dictionary<string, TypeVar>();
            foreach (var type in types)
            {
                if (type is TypeList list)
                {
                    foreach (var param in list.TypeParameters)
                    {
                        if (!result.Contains(param))
                            result.Add(param);
                    }
                }
            }
            return result;
        }

    public class TypeConstant : BaseType
    {
        public override string Name { get; }
        public TypeConstant(string name) => Name = name;
        public override string ToString() => Name;
    }

    public class UnificationException : Exception
    {
        public BaseType Type1 { get; }
        public BaseType Type2 { get; }

        public UnificationException(BaseType type1, BaseType type2, string message)
            : base($"Unification error between {type1} and {type2}: {message}")
        {
            Type1 = type1;
            Type2 = type2;
        }   
    }
    public class TypeUnifier
    {
        public Dictionary<string, int> VariableToUnifierId = new Dictionary<string, int>();
        public Dictionary<int, BaseType> Unifiers = new Dictionary<int, BaseType>();

        public TypeList UnifyLists(TypeList t1, TypeList t2, int depth)
        {
            if (t1.Count != t2.Count) throw new UnificationException(t1, t2, "Lists different sized");
            for (var i = 0; i < t1.Count; i++)
                Unify(t1[i], t2[i], depth + 1);
            return t1;
        }

        public BaseType ChooseBestUnifier(BaseType t1, BaseType t2, int depth)
        {
            if (t1 is TypeVar tv1)
            {
                if (t2 is TypeVar tv2)
                    return string.CompareOrdinal(tv1.Name, tv2.Name) 
                           <= 0 ? tv1 : tv2;

                return t2;
            }

            if (t1 is TypeConstant tc1)
            {
                if (t2 is TypeVar)
                    return t1;

                if (!(t2 is TypeConstant tc2)) 
                    throw new UnificationException(t1, t2, "Expected a type constant");
                
                if (tc1.Name != tc2.Name)
                    throw new UnificationException(t1, t2, "Unifying different type constants");

                return t1;
            }

            if (t1 is TypeList tl1)
            {
                if (t2 is TypeVar)
                    return tl1;

                if (t2 is TypeList tl2)
                    return UnifyLists(tl1, tl2, depth);

                throw new UnificationException(t1, t2, $"Expected a type list");
            }

            throw new UnificationException(t1, t2, "Internal error: unrecognized type");
        }

        public void UpdateUnifier(TypeVar tv, BaseType u)
        {
            if (!VariableToUnifierId.ContainsKey(tv.Name)) throw new Exception("Internal error: no existing unifier found");
            var id = VariableToUnifierId[tv.Name];
            if (!Unifiers.ContainsKey(id)) throw new Exception("Internal error: could not find unified associated with id");
            Unifiers[id] = u;
        }

        public void SanityCheckTypeVariableIsUnified(TypeVar tv, int id)
        {
            if (!VariableToUnifierId.ContainsKey(tv.Name)) 
                throw new Exception("Internal error: type variables as unifiers should have ids");
            if (VariableToUnifierId[tv.Name] != id)
                throw new Exception("Internal error: type variable does not unify to itself");
        }

        public BaseType GetUnifier(TypeVar tv)
        {
            if (!VariableToUnifierId.ContainsKey(tv.Name)) return null;
            var id = VariableToUnifierId[tv.Name];
            if (!Unifiers.ContainsKey(id)) throw new Exception("Internal error: unifier id created without id");
            var r = Unifiers[id];
            if (r is TypeVar tvOther) SanityCheckTypeVariableIsUnified(tvOther, id);
            return r;
        }

        public BaseType GetOrAddUnifier(TypeVar tv)
        {
            var u = GetUnifier(tv);
            if (u != null) return u;
            var id = Unifiers.Count;
            if (Unifiers.ContainsKey(id)) throw new Exception("Internal error: unifier id already exists");
            Unifiers.Add(id, tv);
            if (VariableToUnifierId.ContainsKey(tv.Name)) throw new Exception("Internal error: variable already unified");
            VariableToUnifierId.Add(tv.Name, id);
            return tv;
        }

        public BaseType Unify(BaseType t1, BaseType t2, int depth = 0)
        {
            if (t1 is TypeVar tv1)
            {
                var u = GetOrAddUnifier(tv1);
                if (t2 is TypeVar tv2)
                {
                    var u2 = GetOrAddUnifier(tv2);
                    var ubest = ChooseBestUnifier(u, u2, depth);
                    UpdateUnifier(tv1, ubest);
                    UpdateUnifier(tv2, ubest);
                    return ubest;
                }
                else
                {
                    var ubest = ChooseBestUnifier(u, t2, depth);
                    UpdateUnifier(tv1, ubest);
                    return ubest;
                }
            }

            if (t2 is TypeVar tv)
            {
                var u = GetOrAddUnifier(tv);
                var ubest = ChooseBestUnifier(u, t1, depth);
                UpdateUnifier(tv, ubest);
                return ubest;
            }

            if (t1 is TypeConstant tc1)
            {
                if (t2 is TypeConstant tc2 && tc1.Name == tc2.Name)
                    return tc1;

                throw new UnificationException(t1, t2, "Failed");
            }

            if (t1 is TypeList tl1 && t2 is TypeList tl2)
            {
                return UnifyLists(tl1, tl2, depth);
            }

            throw new UnificationException(t1, t2, "Failed");
        }
    }
}
