
/* TODO: these should be sequences. 
 * 
/// <summary>
/// Creates a readonly array from a seed value, by applying a function
/// </summary>
public static IArray<T> Build<T>(T init, Func<T, T> next, Func<T, bool> hasNext)
{
    var r = new List<T>();
    while (hasNext(init))
    {
        r.Add(init);
        init = next(init);
    }
    return r.ToIArray();
}

/// <summary>
/// Creates a readonly array from a seed value, by applying a function
/// </summary>
public static IArray<T> Build<T>(T init, Func<T, int, T> next, Func<T, int, bool> hasNext)
{
    var i = 0;
    var r = new List<T>();
    while (hasNext(init, i))
    {
        r.Add(init);
        init = next(init, ++i);
    }
    return r.ToIArray();
}*/