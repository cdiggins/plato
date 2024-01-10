public delegate TResult Function1<in T, out TResult>(T arg);
public delegate TResult Function2<in T1, in T2, out TResult>(T1 arg1, T2 arg2);
public class Tuple2<T1, T2>
{
    public T1 Item1; 
    public T2 Item2;
}