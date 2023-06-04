using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoTypeInference
{
    public static class LegacyUnificationAlgorithm
    {
        public static void GatherConstraints(this BaseType a, BaseType b, HashSet<Constraint> constraints)
        {
            if (a is TypeList tla)
            {
                if (b is TypeList tlb)
                {
                    if (tla.Types.Count != tlb.Types.Count)
                    {
                        constraints.Add(new ConstraintError(a, b, "Type list lengths are different"));
                    }
                    else
                    {
                        for (var i = 0; i < tla.Types.Count; i++)
                        {
                            GatherConstraints(tla.Types[i], tlb.Types[i], constraints);
                        }
                    }
                }
                else if (b is PolyType ptb)
                {
                    GatherConstraints(a, ptb.TypeList, constraints);
                }
                else if (b is TypeConstant tcb)
                {
                    constraints.Add(new ConstraintError(a, b, "Constraint between constant and list"));
                }
                else if (b is TypeVariable tv)
                {
                    constraints.Add(new Constraint(tv, a));
                }
                else
                {
                    constraints.Add(new ConstraintError(a, b, "Unhandled type"));
                }
            }
            else if (a is PolyType pta)
            {
                GatherConstraints(pta.TypeList, b, constraints);
            }
            else if (a is TypeVariable tva)
            {
                if (b is PolyType ptb)
                {
                    constraints.Add(new Constraint(tva, ptb.TypeList));
                }
                if (b is TypeVariable tvb)
                {
                    constraints.Add(new Constraint(tva, tvb));
                    constraints.Add(new Constraint(tvb, tva));
                }
                else
                {
                    constraints.Add(new Constraint(tva, b));
                }
            } 
            else if (a is TypeConstant tca)
            {
                if (b is TypeVariable tv)
                {
                    constraints.Add(new Constraint(tv, b));
                }
                else if (b is TypeList _)
                {
                    constraints.Add(new ConstraintError(a, b, "Cannot constrain constant with list"));
                }
                else if (b is PolyType _)
                {
                    constraints.Add(new ConstraintError(a, b, "Cannot constrain constant with PolyType"));
                }
                else
                {
                    constraints.Add(new ConstraintError(a, b, "Unhandled type"));
                }
            }
        }

        public static Dictionary<string, HashSet<BaseType>> ComputeConstraintClasses(this IEnumerable<Constraint> constraints)
        {
            var d = new Dictionary<string, HashSet<BaseType>>();
            foreach (var constraint in constraints)
            {
                var name = constraint.Variable.Name;
                if (!d.ContainsKey(name))
                    d.Add(name, new HashSet<BaseType>());
                d[name].Add(constraint.Second);
            }

            return d;
        }

        public static bool GatherCandidates(string name, Dictionary<string, HashSet<BaseType>> constraintClasses, HashSet<BaseType> candidates)
        {
            if (!constraintClasses.ContainsKey(name))
                return false;
            var result = false;

            foreach (var t in constraintClasses[name])
            {
                if (!candidates.Contains(t))
                {
                    candidates.Add(t);
                    result = true;
                }

                if (t is TypeVariable tv)
                {
                    GatherCandidates(tv.Name, constraintClasses, candidates);
                }
            }

            return result;
        }

        public static BaseType ChooseUnifier(IEnumerable<BaseType> bt)
        {
            var constants = bt.OfType<TypeConstant>();
            var vars = bt.OfType<TypeVariable>();
            var lists = bt.OfType<TypeList>();
            var polytypes = bt.OfType<PolyType>();
            throw new NotImplementedException();
        }

        public static BaseType Unify(string name, Dictionary<string, HashSet<BaseType>> constraintClasses) 
        {
            var candidates = new HashSet<BaseType>();
            GatherCandidates(name, constraintClasses, candidates);
            return ChooseUnifier(candidates);
        }

        public static string GetConstraints(Dictionary<string, List<BaseType>> constraintClasses)
        {
            // TODO: every right hand side that is a variable means that it has a constraint class.
            // The constraint classes of the two are going to be merged 
            // I want to make sure I don't merge too often. 
            // Maybe I can keep just one of them? 
            // Maybe I just make a list of pairs for merging 
            // 
            throw new NotImplementedException();
        }

        public static IEnumerable<Substitution> ComputeSubstitutions(this IEnumerable<Constraint> constraints)
        {
            throw new NotImplementedException();
        }

        public static BaseType ApplySubstitutions(this BaseType type, IEnumerable<Substitution> substitutions)
        {
            throw new NotImplementedException();
        }

        // This function has to lift the type variables up to the highest level that they are used. 

        public static PolyType InferQualifiers(this TypeList input)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<BaseType> GetChildren(this BaseType t)
        {
            if (t is PolyType pt)
                return GetChildren(pt.TypeList).Concat(pt.Parameters);

            if (t is TypeList tl)
                return tl.Types;

            return Enumerable.Empty<BaseType>();
        }

        public static BaseType ReplaceTypes(this BaseType t, IReadOnlyDictionary<string, BaseType> replace)
        {
            if (replace.ContainsKey(t.Name)) 
                return replace[t.Name];
            if (t is TypeList typeList)
                return new TypeList(typeList.Types.Select(t1 => ReplaceTypes(t1, replace)));
            if (t is PolyType pt)
                return new PolyType(
                    pt.Parameters.Select(t1 => (TypeVariable)ReplaceTypes(t1, replace)),
                    (TypeList)ReplaceTypes(pt.TypeList, replace));
            return t;
        }

        // Type variable with a name xxx`y or xxx are renamed xxx`n 
        public static TypeVariable UniquelyRename(this TypeVariable tv, int n)
        {
            var index = tv.Name.IndexOf('`');
            var name = index > 0 ? tv.Name.Substring(0, n) : tv.Name;
            return new TypeVariable($"{name}`{n}");
        }

        public static IReadOnlyDictionary<string, BaseType> 
            GetNewUniqueNamedVariables(this BaseType type, int n)
            => GetAllParameterDeclarations(type)
                .ToDictionary(tv => tv.Name, tv => (BaseType)UniquelyRename(tv, n));

        public static int RenameIndex = 0;

        public static BaseType UniqelyRenameVariables(this BaseType type)
        {
            var names = GetNewUniqueNamedVariables(type, RenameIndex++);
            return ReplaceTypes(type, names);
        }

        public static IEnumerable<BaseType> GetDescendantsAndSelf(this BaseType self)
        {
            yield return self;
            foreach (var t in GetChildren(self).SelectMany(GetDescendantsAndSelf))
                yield return t;
        }

        public static IEnumerable<TypeVariable> GetAllParameterDeclarations(this BaseType self)
            => GetDescendantsAndSelf(self).OfType<PolyType>().SelectMany(pt => pt.Parameters);

        public static bool AreDistinct<T>(this IReadOnlyList<T> self)
            => self.Count == self.Distinct().Count();

        public static bool AreVariablesDistinct(this BaseType t) 
            => AreDistinct(GetAllParameterDeclarations(t).Select(tv => tv.Name).ToList());

        public static void Unify(this BaseType type1, BaseType type2)
        {
            // Gather constraints
            // How do I make sure there is no confusion? 
            // Ideally types would be assigned un

            // TODO: make sure that the type variables in type 1 and those in type 2 
            // are different
        }
    }
}