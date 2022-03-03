namespace BetterCore;

using System;
using System.Diagnostics.Contracts;

public class Dictionary<TKey, TValue>
{

    private struct Entry
    {
        public int hashCode; // Lower 31 bits of hash code, -1 if unused
        public int next; // Index of next entry, -1 if last
        public TKey key; // Key of entry
        public TValue value; // Value of entry
    }

    private int[] buckets;
    private Entry[] entries;
    private int count;
    private int version;
    private int freeList;
    private int freeCount;
    private IEqualityComparer<TKey> comparer;


    public Dictionary()
        => Initialize(2);

    public Dictionary(IEqualityComparer<TKey> comparer) : this(null, comparer)
    {
    }

    public Dictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null)
    {
    }

    public Dictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
    {
        // NOTE: really? REALLY???!
        foreach (var pair in dictionary)
        {
            Add(pair.Key, pair.Value);
        }
    }

    public IEqualityComparer<TKey> Comparer => Comparer;

    public int Count => count - freeCount;

    public TValue this[TKey key]
    {
        get
        {
            var i = FindEntry(key);
            if (i >= 0) return entries[i].value;
            throw new ArgumentException();
        }
        set { Insert(key, value, false); }
    }

    public void Add(TKey key, TValue value)
    {
        Insert(key, value, true);
    }

    public void Clear()
    {
        if (count > 0)
        {
            for (var i = 0; i < buckets.Length; i++) buckets[i] = -1;
            Array.Clear(entries, 0, count);
            freeList = -1;
            count = 0;
            freeCount = 0;
            version++;
        }
    }

    public bool ContainsKey(TKey key)
    {
        return FindEntry(key) >= 0;
    }

    public bool ContainsValue(TValue value)
    {
        if (value == null)
        {
            for (var i = 0; i < count; i++)
            {
                if (entries[i].hashCode >= 0 && entries[i].value == null) return true;
            }
        }
        else
        {
            var c = EqualityComparer<TValue>.Default;
            for (var i = 0; i < count; i++)
            {
                if (entries[i].hashCode >= 0 && c.Equals(entries[i].value, value)) return true;
            }
        }

        return false;
    }

    private int FindEntry(TKey key)
    {
        if (buckets != null)
        {
            var hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
            // TODO: fix linear hashing.
            for (var i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].next)
            {
                if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key)) return i;
            }
        }

        return -1;
    }

    private void Initialize(int capacity)
    {
        // TODO: optimize this 
        var size = capacity * 2;

        buckets = new int[size];
        for (var i = 0; i < buckets.Length; i++) buckets[i] = -1;
        entries = new Entry[size];
        freeList = -1;
    }

    private void Insert(TKey key, TValue value, bool add)
    {
        var hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
        var targetBucket = hashCode % buckets.Length;


        for (var i = buckets[targetBucket]; i >= 0; i = entries[i].next)
        {
            if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
            {
                if (add)
                {
                    throw new ArgumentException();
                }

                entries[i].value = value;
                version++;
                return;
            }
        }

        int index;
        if (freeCount > 0)
        {
            index = freeList;
            freeList = entries[index].next;
            freeCount--;
        }
        else
        {
            if (count == entries.Length)
            {
                Resize();
                targetBucket = hashCode % buckets.Length;
            }

            index = count;
            count++;
        }

        entries[index].hashCode = hashCode;
        entries[index].next = buckets[targetBucket];
        entries[index].key = key;
        entries[index].value = value;
        buckets[targetBucket] = index;
        version++;

    }

    private void Resize()
    {
        //Resize(HashHelpers.ExpandPrime(count), false);
    }

    private void Resize(int newSize, bool forceNewHashCodes)
    {
        Contract.Assert(newSize >= entries.Length);
        var newBuckets = new int[newSize];
        for (var i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
        var newEntries = new Entry[newSize];
        Array.Copy(entries, 0, newEntries, 0, count);
        if (forceNewHashCodes)
        {
            for (var i = 0; i < count; i++)
            {
                if (newEntries[i].hashCode != -1)
                {
                    newEntries[i].hashCode = (comparer.GetHashCode(newEntries[i].key) & 0x7FFFFFFF);
                }
            }
        }

        for (var i = 0; i < count; i++)
        {
            if (newEntries[i].hashCode >= 0)
            {
                var bucket = newEntries[i].hashCode % newSize;
                newEntries[i].next = newBuckets[bucket];
                newBuckets[bucket] = i;
            }
        }

        buckets = newBuckets;
        entries = newEntries;
    }

    public bool Remove(TKey key)
    {
        if (buckets != null)
        {
            var hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
            var bucket = hashCode % buckets.Length;
            var last = -1;
            for (var i = buckets[bucket]; i >= 0; last = i, i = entries[i].next)
            {
                if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
                {
                    if (last < 0)
                    {
                        buckets[bucket] = entries[i].next;
                    }
                    else
                    {
                        entries[last].next = entries[i].next;
                    }

                    entries[i].hashCode = -1;
                    entries[i].next = freeList;
                    entries[i].key = default(TKey);
                    entries[i].value = default(TValue);
                    freeList = i;
                    freeCount++;
                    version++;
                    return true;
                }
            }
        }

        return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        var i = FindEntry(key);
        if (i >= 0)
        {
            value = entries[i].value;
            return true;
        }

        value = default(TValue);
        return false;
    }
}