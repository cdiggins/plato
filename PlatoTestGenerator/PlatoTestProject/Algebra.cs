namespace Plato;

// Concepts are declared in Plato as interfaces that take the type as a first parameter. 
public class ConceptAttribute : Attribute
{ }

// https://en.wikipedia.org/wiki/Associative_property
// Declares an operation as being associative 
public class AssociativeAttribute : Attribute
{ }

// https://en.wikipedia.org/wiki/Commutative_property
// Declares an operation as being commutative, changing order does not change meaning
public class CommutativeAttribute : Attribute
{ }

// https://en.wikipedia.org/wiki/Anticommutative_property
// Swapping arguments yield inverse 
public class AnticommutativeAttribute : Attribute
{ }

public interface IBinaryOperation<TSelf>
    where TSelf: IBinaryOperation<TSelf>
{
    TSelf Operator(TSelf self);
}

public interface IAssociativeOperation<TSelf>
    : IBinaryOperation<TSelf>
    where TSelf : IAssociativeOperation<TSelf>
{
    [Associative]
    new TSelf Operator(TSelf self);
}

public interface ICommutativeOperation<TSelf>
    : IBinaryOperation<TSelf>
    where TSelf : ICommutativeOperation<TSelf>
{
}

public interface IIdentity<TSelf>
    where TSelf : IIdentity<TSelf>
{
    [Static]
    TSelf Identity { get; }
}

public interface IAdditiveIdentity<TSelf>
    : IIdentity<TSelf>
    where TSelf : IAdditiveIdentity<TSelf>
{
    [Static]
    TSelf Zero { get; }
}

public interface IMultiplicativeIdentity<TSelf>
    : IIdentity<TSelf>
    where TSelf : IMultiplicativeIdentity<TSelf>
{
    [Static]
    TSelf One { get; }
}

public interface IInvertible<TSelf>
    where TSelf : IInvertible<TSelf>
{
    TSelf Inverse();
}

public interface IAdditiveInverse<TSelf>
    : IInvertible<TSelf>
    where TSelf : IAdditiveInverse<TSelf>
{
    TSelf Negate();
}

public interface IMultiplicativeInverse<TSelf>
    : IInvertible<TSelf>
    where TSelf : IMultiplicativeInverse<TSelf>
{
    TSelf Reciprocal();
}

public interface IAddition<TSelf>
    : IAssociativeOperation<TSelf>
    where TSelf : IAddition<TSelf>
{
    TSelf Add(TSelf self);
}

public interface IAdditionSemigroup<TSelf>
    : IAddition<TSelf>, ISemiGroup<TSelf, IAddition<TSelf>>
    where TSelf : IAdditionSemigroup<TSelf>
{
}

public interface IAdditionMonoid<TSelf>
    : IMonoid<TSelf, IAddition<TSelf>>, IAddition<TSelf>
    where TSelf : IAdditionMonoid<TSelf>
{
}

public interface IAdditionGroup<TSelf>
    : IAddition<TSelf>, IAdditiveInverse<TSelf>, IAdditiveIdentity<TSelf> 
    where TSelf : IAdditionGroup<TSelf>
{
}

public interface IMultiplication<TSelf>
    : IAssociativeOperation<TSelf>
    where TSelf : IMultiplication<TSelf>
{
    TSelf Multiply(TSelf self);
}

public interface IMultiplicationGroup<TSelf>
    : IMultiplication<TSelf>, IMultiplicativeInverse<TSelf>, IMultiplicativeIdentity<TSelf>
    where TSelf : IMultiplicationGroup<TSelf>
{
}

public interface IDivision<TSelf>
    : IBinaryOperation<TSelf>
    where TSelf : IDivision<TSelf>
{
    TSelf Divide(TSelf self);
}

// A static attribute will return the same regardless of the type 
public class StaticAttribute : Attribute
{ }

public static class AlgebraicExtension
{
}

// https://en.wikipedia.org/wiki/Magma_(algebra)
// A set with a binary operation 
[Concept]
public interface IMagma<TSelf, TOperation>
    where TOperation : IBinaryOperation<TSelf>
    where TSelf : IMagma<TSelf, TOperation>, TOperation
{
}

// https://en.wikipedia.org/wiki/Quasigroup
public interface IQuasiGroup<TSelf, TOperation>
    : IMagma<TSelf, TOperation>
    where TOperation : IBinaryOperation<TSelf>
    where TSelf : IQuasiGroup<TSelf, TOperation>, TOperation
{
    TSelf Divide(TSelf self);
    TSelf LeftDivide(TSelf self);
}

// https://en.wikipedia.org/wiki/Quasigroup
public interface ILoop<TSelf, TOperation>
    : IQuasiGroup<TSelf, TOperation>, IUnitalMagma<TSelf, TOperation>
    where TOperation : IBinaryOperation<TSelf>
    where TSelf : ILoop<TSelf, TOperation>, TOperation
{
}

public interface IUnital<out TSelf>
{
    [Static]
    TSelf Identity { get; }
}

// https://en.wikipedia.org/wiki/Magma_(algebra)#unital
// A magma with identity 
[Concept]
public interface IUnitalMagma<TSelf, TOperation>
    : IMagma<TSelf, TOperation>, IUnital<TSelf>
    where TOperation: IBinaryOperation<TSelf>
    where TSelf : IUnitalMagma<TSelf, TOperation>, IUnital<TSelf>, TOperation
{
}

// https://en.wikipedia.org/wiki/Semigroup
// A magma where the operation is associative 
// A group without the requirement for an inverse or identity 
[Concept]
public interface ISemiGroup<TSelf, TOperation>
    : IMagma<TSelf, TOperation>
    where TOperation : IAssociativeOperation<TSelf>
    where TSelf : ISemiGroup<TSelf, TOperation>, TOperation
{
}

// https://en.wikipedia.org/wiki/Inverse_semigroup
// A magma where the operation is associative, an inverse exists, but no identity 
[Concept]
public interface IInverseSemiGroup<TSelf, TOperation>
    : IQuasiGroup<TSelf, TOperation>, ISemiGroup<TSelf, TOperation>, IInvertible<TSelf>
    where TOperation : IAssociativeOperation<TSelf>
    where TSelf : IInverseSemiGroup<TSelf, TOperation>, TOperation
{
}

// https://en.wikipedia.org/wiki/Monoid
// A semigroup with identity. 
// Example: non-negative integers with addition
[Concept]
public interface IMonoid<TSelf, TOperation>
    : IUnitalMagma<TSelf, TOperation>, ISemiGroup<TSelf, TOperation>
    where TOperation : IAssociativeOperation<TSelf>
    where TSelf : IMonoid<TSelf, TOperation>, TOperation
{
}

// https://en.wikipedia.org/wiki/Monoid#Commutative_monoid
// 
public interface ICommuatativeMonoid<TSelf, TOperation>
    : IMonoid<TSelf, TOperation>
    where TOperation : IAssociativeOperation<TSelf>
    where TSelf : ICommuatativeMonoid<TSelf, TOperation>, TOperation
{
    [Commutative]
    TSelf Operation(TSelf self);
}


// An algebraic group over a field is an algebraic variety G over k, together with an identity element, and
// regular maps and the inversion operator which satisfy the group axioms.
// Equivalently it can be described as a Monoid that is invertable, an Inverse Semigroup with identity, or an associative loop. 
// https://en.wikipedia.org/wiki/Algebraic_group
[Concept]
public interface IGroup<TSelf, TOperation>
    : IMonoid<TSelf, TOperation>, IInverseSemiGroup<TSelf, TOperation>, ILoop<TSelf, TOperation>
    where TOperation : IAssociativeOperation<TSelf>
    where TSelf : IGroup<TSelf, TOperation>, TOperation
{
}

// https://en.wikipedia.org/wiki/Abelian_group
// Also known as a Commutative group. 
[Concept]
public interface IAbelianGroup<TSelf, TOperation>
    : IGroup<TSelf, TOperation>
    where TOperation : IAssociativeOperation<TSelf>
    where TSelf : IAbelianGroup<TSelf, TOperation>, TOperation
{
}

[Concept]
public interface IAdditiveSemiGroup<TSelf>
    : ISemiGroup<TSelf, IAddition<TSelf>>, IAddition<TSelf>
    where TSelf: IAdditiveSemiGroup<TSelf>
{
    //     TSelf Zero { get; } // AKA Additive Identity 

}

[Concept]
public interface IAdditiveInverseSemiGroup<TSelf>
    : IAdditiveSemiGroup<TSelf>, IInverseSemiGroup<TSelf, IAddition<TSelf>>
    where TSelf : IAdditiveInverseSemiGroup<TSelf>
{
    TSelf Negate(); // AKA Additive Inverse
    //TSelf Subtract(TSelf x); Trivially derived. = Add(x.Negate())
}

// https://en.wikipedia.org/wiki/Additive_group
[Concept]
public interface IAdditiveGroup<TSelf>
    : IAbelianGroup<TSelf, IAddition<TSelf>>, IAdditiveInverseSemiGroup<TSelf>
    where TSelf : IAdditiveGroup<TSelf>
{
}

// https://en.wikipedia.org/wiki/Multiplicative_group
[Concept]
public interface IMultiplicativeSemiGroup<TSelf>
    : ISemiGroup<TSelf, IMultiplication<TSelf>>, IMultiplication<TSelf>
    where TSelf : IMultiplicativeSemiGroup<TSelf>
{
}

// https://en.wikipedia.org/wiki/Multiplicative_group
[Concept]
public interface IMultiplicativeGroup<TSelf>
    : IAbelianGroup<TSelf, IMultiplication<TSelf>>, IMultiplicativeSemiGroup<TSelf>
    where TSelf : IMultiplicativeGroup<TSelf>
{
    TSelf One { get; } // AKA Multiplicative Identity 
    TSelf Reciprocal(); // AKA Multiplicative Inverse
}

// https://en.wikipedia.org/wiki/Semiring
// Do not have an additive inverse
[Concept]
public interface ISemiRing<TSelf>
    : IAdditiveInverseSemiGroup<TSelf>, IMultiplicativeSemiGroup<TSelf>
    where TSelf : ISemiRing<TSelf>
{
}

// https://en.wikipedia.org/wiki/Ring_(mathematics)
// Rings do not necessarily have a multiplicative inverse, so divide is not necessarily well-defined
[Concept]
public interface IRing<TSelf>
    : IAdditiveGroup<TSelf>, IMultiplicativeSemiGroup<TSelf>
    where TSelf : IRing<TSelf>
{
}

// https://en.wikipedia.org/wiki/Division_ring
[Concept]
public interface IDivisionRing<TSelf>
    : IRing<TSelf>
    where TSelf : IDivisionRing<TSelf>
{
    TSelf Divide(TSelf x);
    TSelf Reciprocal(); // AKA Multiplicative Inverse  
}

// https://en.wikipedia.org/wiki/Division_ring
public interface ICommutativeDivisionRing<TSelf>
    : IDivisionRing<TSelf>
    where TSelf : ICommutativeDivisionRing<TSelf>
{
}

// https://en.wikipedia.org/wiki/Quaternion
// Quaternions are non-commutative division rings 
[Concept]
public interface IQuaternion<TSelf, TElement> : IDivisionRing<TSelf>
    where TSelf : IQuaternion<TSelf, TElement>
    where TElement : IReal<TElement>
{
    TElement A { get; }
    TElement B { get; }
    TElement C { get; }
    TElement D { get; }
    IQuaternion<TSelf, TElement> BasisI { get; }
    IQuaternion<TSelf, TElement> BasisJ { get; }
    IQuaternion<TSelf, TElement> BasisK { get; } }


// https://en.wikipedia.org/wiki/Field_(mathematics)
// Fields support commutative division 
[Concept]
public interface IField<TSelf>
    : ICommutativeDivisionRing<TSelf>
    where TSelf : IField<TSelf>
{
}

[Concept]
public interface IStrictOrder<TSelf>
    where TSelf : IStrictOrder<TSelf>
{
    bool LessThan(TSelf x, TSelf y);
}

// https://en.wikipedia.org/wiki/Ordered_field
[Concept]
public interface IOrderedField<TSelf>
    : IField<TSelf>, IStrictOrder<TSelf>
    where TSelf : IOrderedField<TSelf>
{
}

// https://en.wikipedia.org/wiki/Real_number
[Concept]
public interface IReal<TSelf>
    : IOrderedField<TSelf>
    where TSelf : IReal<TSelf>
{
}

// https://en.wikipedia.org/wiki/Rational_number
[Concept]
public interface IRational<TSelf, TInteger>
    : IField<TSelf>
    where TSelf : IRational<TSelf, TInteger>
{
    TInteger Numerator { get; }
    TInteger Denominator { get; }
}

// https://en.wikipedia.org/wiki/Complex_number
[Concept]
public interface IComplex<TSelf, TElement>
    : IField<TSelf>, IVector<TSelf, TElement>
    where TSelf : IComplex<TSelf, TElement>
    where TElement : IReal<TElement>
{
    TElement A { get; }
    TElement B { get; }
    IComplex<TSelf, TElement> BasisI { get; }
}

[Concept]
public interface IJoinSemiLattice<TSelf>
    : IPartiallyOrdered<TSelf>
    where TSelf : IJoinSemiLattice<TSelf>
{
    TSelf Join(TSelf self);
}

[Concept]
public interface IMeetSemiLattice<TSelf>
    : IPartiallyOrdered<TSelf>
    where TSelf : IMeetSemiLattice<TSelf>
{
    TSelf Meet(TSelf self);
}

// https://en.wikipedia.org/wiki/Partially_ordered_group
[Concept]
public interface IPartiallyOrdered<TSelf> 
    // TODO: is it really an additiver group? Or is the LessThanOrEqualTo
    : IAdditiveGroup<TSelf>
    where TSelf : IPartiallyOrdered<TSelf>
{
    bool LessThanOrEqualTo(TSelf x);
    // bool Positive() => Zero.LessThanOrEqualTo(self);
    // bool Negative() => self.LessThanOrEqualTo(self.Zero);
}

// https://en.wikipedia.org/wiki/Lattice_(order)
// https://mathworld.wolfram.com/Lattice.html
[Concept]
public interface ILattice<TSelf>
    : IPartiallyOrdered<TSelf> 
    where TSelf : ILattice<TSelf>
{
    TSelf LeastUpperBound(TSelf other);
    TSelf GreatestLowerBOund(TSelf other);
}

[Concept]
public interface IScalable<TSelf, TScalar>
    where TSelf : IScalable<TSelf, TScalar>
{
    TSelf Multiply(TScalar scalar);
    TSelf Divide(TScalar scalar);
}

// https://en.wikipedia.org/wiki/Module_(mathematics)
[Concept]
public interface IModule<TSelf, TElement>
    : IAdditiveGroup<TSelf>, IScalable<TSelf, TElement>
    where TSelf : IModule<TSelf, TElement>
    where TElement : IRing<TElement>
{
    int Dimensionality(TSelf x);
}

[Concept]
// https://en.wikipedia.org/wiki/Vector_space
public interface IVector<TSelf, TScalar>
    : IModule<TSelf, TScalar>
    where TSelf : IVector<TSelf, TScalar>
    where TScalar : IField<TScalar>
{
    TSelf DotProduct(TSelf x);
    TScalar Length { get; }
    TSelf Normal { get; }
    IArray<TSelf> CanonicalBasis { get; }
}


[Concept]
public interface IVector3<TSelf, TScalar>
    : IModule<TSelf, TScalar>
    where TSelf : IVector<TSelf, TScalar>
    where TScalar : IField<TScalar>
{
    [Anticommutative]
    TSelf CrossProduct(TSelf other);
}

// https://en.wikipedia.org/wiki/Heyting_algebra
// Corresponds to intuitionistic logic
// https://en.wikipedia.org/wiki/Intuitionistic_logic
public interface IHeyting<TSelf>
    : ILattice<TSelf>
    where TSelf : IHeyting<TSelf>
{
    TSelf And(TSelf x);
    TSelf Or(TSelf x);
    TSelf Not();

    [Static]
    TSelf Zero { get; }

    [Static]
    TSelf One { get; }
}

// https://en.wikipedia.org/wiki/Boolean_algebra_(structure)
// Classical logic
// https://en.wikipedia.org/wiki/Classical_logic
// 1. Law of excluded middle and double negation elimination
// 2. Law of noncontradiction, and the principle of explosion
// 3. Monotonicity of entailment and idempotency of entailment
// 4. Commutativity of conjunction
// 5. De Morgan duality: every logical operator is dual to another
public interface IBoolean<TSelf>
    : IHeyting<TSelf>
    where TSelf: IBoolean<TSelf>
{
}