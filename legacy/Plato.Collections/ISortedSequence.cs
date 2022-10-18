using System.Diagnostics;

namespace Plato
{
}

/*
public readonly record struct SortedSequence<T>(IComparer<T> Ordering, ISequence<T> Items) 
    : ISortedSequence<T>
{
}

public readonly record struct MergedSequence<T>
    : ISortedSequence<T>
{
    public MergedSequence(ISortedSequence<T> seq1, ISortedSequence<T> seq2)
    {
        Debug.Assert(seq1.HasValue);
        Debug.Assert(seq2.HasValue);
        Debug.Assert(seq1.Ordering == seq2.Ordering);
        Ordering = seq1.Ordering;
        if (Ordering.Compare(seq1.Value, seq2.Value) <= 0)
        {
            Left = seq1;
            Right = seq2;
        }
        else
        {
            Left = seq2;
            Right = seq1;
        }
    }

    IComparer<T> Ordering { get; }
    ISortedSequence<T> Left { get; }
    ISortedSequence<T> Right { get; }
    T IIterator<T>.Value => Left.Value;
    bool IIterator<T>.HasValue => Left.HasValue;
    IIterator<T> IIterator<T>.Next => Next;

    ISortedSequence<T> Next
    { 
        get 
        {
            var tmp = Left.Next;
            if (!tmp.HasValue)
                return Right;
            return new MergedSequence<T>(tmp, Right);
        }
    }
}

public partial interface ISortedSequence<T>
{
    ISortedSequence<T> Next => Default;

    IIterator<T> IIterator<T>.Next => Next;

    ISortedSequence<T> ISequence<T>.OrderBy(IComparer<T> ordering)
        => ordering == Ordering
            ? this
            : IsOrderedBy(ordering)
                ? new SortedSequence<T>(ordering, this)
                : ExplicitSort(ordering);

    // TODO: BUG: I don't think calling the base class in this manner is legal.
    bool ISequence<T>.IsOrderedBy(IComparer<T> ordering)
        => ordering == Ordering || ((ISequence<T>)this).IsOrderedBy(ordering);

    ISortedSequence<T> Merge(ISequence<T> seq)
        => new MergedSequence<T>(this, seq.OrderBy(Ordering));

    public static ISortedSequence<T> Default = new SortedSequence<T>(NoOrder<T>.Instance, EmptyGenerator<T>.Instance);
}
*/
