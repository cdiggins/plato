using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PlatoTypeInference
{
    public class Constraints
    {
        public Dictionary<string, HashSet<BaseType>> Sets { get; } 
            = new Dictionary<string, HashSet<BaseType>>();

        private int _depth = 0;

        public IEnumerable<IEnumerable<string>> GetVariableGroups()
            => Sets.GroupBy(kv => kv.Value).Select(group => group.Select(kv => kv.Key));

        public void AddConstraint(TypeList tl1, TypeList tl2)
        {
            if (tl1.Types.Count != tl2.Types.Count)
                throw new Exception("Cannot constrain different sized type lists");
            _depth += 1;
            try
            {
                for (var i = 0; i < tl1.Types.Count; i++)
                    AddConstraint(tl1.Types[i], tl2.Types[i]);
            }
            finally
            {
                _depth -= 1;
            }
        }

        public void AddConstraint(BaseType bt1, BaseType bt2)
        {
            if (bt1.Equals(bt2))
                return;

            // Poly types are constrained as if they are type lists 
            if (bt1 is PolyType pt1) bt1 = pt1.TypeList;
            if (bt2 is PolyType pt2) bt2 = pt2.TypeList;

            if (bt1 is TypeVariable tv1)
            {
                AddConstraint(tv1, bt2);
            }
            else if (bt2 is TypeVariable tv2)
            {
                AddConstraint(tv2, bt1);
            }
            else if (bt1 is TypeList tl1)
            {
                if (bt2 is TypeList tl2)
                {
                    AddConstraint(tl1, tl2);
                }
                else if (bt2 is TypeConstant tc)
                {
                    throw new Exception("Cannot constraint a type list to a type constant");
                }
                else
                {
                    throw new Exception("Internal error");
                }
            }
            else if (bt1 is TypeConstant tc)
            {
                if (bt2 is TypeList tl2)
                {
                    throw new Exception("Cannot constrain a type list to a type constant");
                }

                if (bt2 is TypeConstant tc2)
                {
                    throw new Exception("Constraining two different type constants");
                }
                else
                {
                    throw new Exception("Internal error");
                }
            }
            else
            {
                throw new Exception("Internal error");
            }
        }

        /// <summary>
        /// Every element of the tv2 types will be added to the set of tv1 types. 
        /// Both variables will point to the same set afterwards.
        /// </summary>
        public void MergeConstraintSets(TypeVariable tv1, TypeVariable tv2)
        {
            if (tv1.Equals(tv2))
                return;

            var current = Sets[tv1.Name];
            var other = Sets[tv2.Name];
            
            if (ReferenceEquals(other, current))
                return;
            
            Sets[tv2.Name] = current;

            foreach (var t in other)
            {
                AddConstraint(tv1, t);
            }
        }

        public void AddConstraint(TypeVariable tv, BaseType bt)
        {
            if (!Sets.ContainsKey(tv.Name))
            {
                Sets.Add(tv.Name, new HashSet<BaseType>());
            }
            
            if (bt is TypeVariable tv2)
            {
                if (!Sets.ContainsKey(tv2.Name))
                {
                    Sets.Add(tv2.Name, new HashSet<BaseType>());
                }
                MergeConstraintSets(tv, tv2);
                return;
            }

            var set = Sets[tv.Name];

            if (set.Contains(bt))
                return;

            foreach (var t in set)
            {
                Debug.Assert(!(t is TypeVariable));
                AddConstraint(t, bt);
            }

            set.Add(bt);
        }
    }
}