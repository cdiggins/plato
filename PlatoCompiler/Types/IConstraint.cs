﻿using System;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Utilities;

namespace Plato.Compiler.Types
{
    public interface IConstraint 
    { }

    public class TypeVariableConstraint : IConstraint
    {
        public TypeVariable Variable { get; }

        public TypeVariableConstraint(TypeVariable tv)
            => Variable = tv;
    }

    public class CastTypeVariableConstraint : TypeVariableConstraint
    {
        public CastTypeVariableConstraint(TypeVariable tv, IType target)
            : base(tv)
            => Target = target;

        public IType Target { get; }

        public override string ToString()
            => $"{Variable} implements {Target}";

        public override bool Equals(object obj)
            => obj is CastTypeVariableConstraint ic
               && Target.Equals(ic.Target)
               && Variable.Equals(ic.Variable);

        public override int GetHashCode()
            => Hasher.Combine(Target.GetHashCode(), Variable.GetHashCode());
    }

    public class CastToConstraint : IConstraint
    {
        public IType Source { get; }
        public IType Target { get; }

        public CastToConstraint(IType source, IType target)
        {
            Source = source;
            Target = target;
        }

        public override string ToString()
            => $"{Source} casts to {Target}";

        public override bool Equals(object obj)
            => obj is CastToConstraint ctc
               && Target.Equals(ctc.Target)
               && Source.Equals(ctc.Source);

        public override int GetHashCode()
            => Hasher.Combine(Target.GetHashCode(), Source.GetHashCode());
    }

    public class CallableConstraint : IConstraint
    {
        public IType Source { get; }
        public IReadOnlyList<IType> Args { get; }

        public CallableConstraint(IType source, IReadOnlyList<IType> args)
        {
            Source = source;
            Args = args;
        }

        public override string ToString()
            => $"{Source} is callable with ({string.Join(",", Args)})";

        public override bool Equals(object obj)
            => obj is CallableConstraint cc
               && Source.Equals(cc.Source)
               && Args.SequenceEqual(cc.Args);

        public override int GetHashCode()
            => Hasher.Combine(Hasher.Combine(Args.Select(a => a.GetHashCode())), Source.GetHashCode());
    }

    public class GenericBaseConstraint : IConstraint
    {
        public IType Source { get; }
        public TypeConstant Base { get; }

        public GenericBaseConstraint(IType source, TypeConstant constant)
        {
            Source = source;
            Base = constant;
        }
    }
}