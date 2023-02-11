using System.Collections.Generic;
using System;

namespace PlatoTypeInference
{
    public interface IStack<T> 
    {
        IStack<T> Push(T element);
        IStack<T> Pop();
        T Peek();
        bool IsEmpty { get; }
    }
    
    public class EmptyStack<T> : IStack<T>
    {
        public IStack<T> Push(T element)
            => new PushStack<T>(element, this);

        public IStack<T> Pop()
            => throw new InvalidOperationException("Cannot pop an empty stack");

        public T Peek()
            => throw new InvalidOperationException("Cannot peek an empty stack");

        public bool IsEmpty
            => true;

        public static EmptyStack<T> Default { get; }
            = new EmptyStack<T>();
    }
    
    public class PushStack<T>
        : IStack<T>
    {
        public T Head { get; }
        public IStack<T> Tail { get; }

        public PushStack(T head, IStack<T> tail)
            => (Head, Tail) = (head, tail);

        public IStack<T> Push(T element)
            => new PushStack<T>(element, this);

        public IStack<T> Pop()
            => Tail;

        public T Peek()
            => Head;

        public bool IsEmpty
            => false;
    }

    public class ListStack<T>
        : IStack<T>
    {
        public IReadOnlyList<T> Elements { get; }
        public int Count { get; }

        public ListStack(IReadOnlyList<T> list)
            : this(list, list.Count)
        { }

        public ListStack(IReadOnlyList<T> list, int count)
        {
            if (count <= 0)
                return;
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (count > list.Count)
                throw new ArgumentOutOfRangeException(nameof(count));
            Elements = list;
            Count = count;
        }

        public IStack<T> Push(T element)
        {
            if (Count < Elements.Count && element.Equals(Elements[Count]))
                return new ListStack<T>(Elements, Count + 1);
            return new ListStack<T>(Elements, Count + 1);
        }

        public IStack<T> Pop()
        {
            if (Count <= 0)
                throw new InvalidOperationException("Cannot pop an empty stack");
            if (Count == 1)
                return EmptyStack<T>.Default;
            return new ListStack<T>(Elements, Count - 1);
        }

        public T Peek()
            => Count > 0
                ? Elements[Count - 1]
                : throw new InvalidOperationException("Cannot peek an empty stack");

        public bool IsEmpty
            => Count > 0;
    }
}