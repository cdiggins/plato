// MIT License - Copyright 2019 (C) VIMaec, LLC.
// MIT License - Copyright 2018 (C) Ara 3D, Inc.
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

using System.Linq;

namespace Vim.LinqArray
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public interface ILookup
    <TKey, TValue>
    {
        // A private instance method named Contains with a type bool
        // No associated operation
        // No data-flow analysis could be created
                bool Contains(TKey key);

        // A private instance property named Keys with a type Vim.LinqArray.IArray<TKey>
        // No associated operation
        // No data-flow analysis could be created
                IArray<TKey> Keys { get; }

        // A private instance property named Values with a type Vim.LinqArray.IArray<TValue>
        // No associated operation
        // No data-flow analysis could be created
                IArray<TValue> Values { get; }

        // A private instance member named this with a type TValue
        // No associated operation
        // No data-flow analysis could be created
                TValue this[TKey key] { get; }

    } // type
} // namespace
namespace Vim.LinqArray
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public class EmptyLookup
    <TKey, TValue>
    : ILookup<TKey, TValue>

    {
        // A public instance method named Contains with a type bool
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are 
        // Captured symbols are 
        // Variables declared are 
        
        public bool Contains(TKey key)
        {
            return false;
        }

        // A public instance property named Keys with a type Vim.LinqArray.IArray<TKey>
        // operation kind is Invocation and type Vim.LinqArray.IArray<TKey>
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are 
        // Captured symbols are 
        // Variables declared are 
                public IArray<TKey> Keys => LinqArray.Empty<TKey>();

        // A public instance property named Values with a type Vim.LinqArray.IArray<TValue>
        // operation kind is Invocation and type Vim.LinqArray.IArray<TValue>
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are 
        // Captured symbols are 
        // Variables declared are 
                public IArray<TValue> Values => LinqArray.Empty<TValue>();

        // A public instance member named this with a type TValue
        // No associated operation
        // No data-flow analysis could be created
        
        public TValue this[TKey key] => default;

    } // type
} // namespace
namespace Vim.LinqArray
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public class LookupFromDictionary
    <TKey, TValue>
    : ILookup<TKey, TValue>

    {
        // A public instance method named .ctor with a type void
        // operation kind is Block and type 
        // member references = Dictionary, _default, Keys, Keys, Values, Values
        // assignments = Coalesce, ParameterReference, Invocation, Invocation
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=d Kind=Parameter), (Name=defaultValue Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public LookupFromDictionary(IDictionary<TKey, TValue> d = null, TValue defaultValue = default)
        {
            Dictionary = d ?? new Dictionary<TKey, TValue>();
            // TODO: sort?
            _default = defaultValue;
            Keys = d.Keys.ToIArray();
            Values = d.Values.ToIArray();
        }

        // A public instance method named Contains with a type bool
        // operation kind is Block and type 
        // member references = Dictionary
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=key Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public bool Contains(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        // A private instance field named _default with a type TValue
        // No associated operation
        // No data-flow analysis could be created
                private readonly TValue _default;

        // A public instance field named Dictionary with a type System.Collections.Generic.IDictionary<TKey, TValue>
        // No associated operation
        // No data-flow analysis could be created
                public IDictionary<TKey, TValue> Dictionary;

        // A public instance property named Keys with a type Vim.LinqArray.IArray<TKey>
        // No associated operation
        // No data-flow analysis could be created
        
        public IArray<TKey> Keys { get; }

        // A public instance property named Values with a type Vim.LinqArray.IArray<TValue>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<TValue> Values { get; }

        // A public instance member named this with a type TValue
        // No associated operation
        // No data-flow analysis could be created
                public TValue this[TKey key] => Contains(key) ? Dictionary[key] : _default;

    } // type
} // namespace
namespace Vim.LinqArray
{
    // Type has fields True
    // Type has writable fields False
    // Type has public setters False
    public class LookupFromArray
    <TValue>
    : ILookup<int, TValue>

    {
        // A public instance method named .ctor with a type void
        // operation kind is Block and type 
        // member references = array, Keys, array, Values, array
        // assignments = ParameterReference, Invocation, FieldReference
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=xs Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public LookupFromArray(IArray<TValue> xs)
        {
            array = xs;
            Keys = array.Indices();
            Values = array;
        }

        // A public instance method named Contains with a type bool
        // operation kind is Block and type 
        // member references = Count, array
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=key Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public bool Contains(int key)
        {
            return key >= 0 && key <= array.Count;
        }

        // A private instance field named array with a type Vim.LinqArray.IArray<TValue>
        // No associated operation
        // No data-flow analysis could be created
                private readonly IArray<TValue> array;

        // A public instance property named Keys with a type Vim.LinqArray.IArray<int>
        // No associated operation
        // No data-flow analysis could be created
        
        public IArray<int> Keys { get; }

        // A public instance property named Values with a type Vim.LinqArray.IArray<TValue>
        // No associated operation
        // No data-flow analysis could be created
                public IArray<TValue> Values { get; }

        // A public instance member named this with a type TValue
        // No associated operation
        // No data-flow analysis could be created
                public TValue this[int key] => array[key];

    } // type
} // namespace
namespace Vim.LinqArray
{
    // Type has fields False
    // Type has writable fields False
    // Type has public setters False
    public static class LookupExtensions
    {
        // A public static method named ToLookup with a type Vim.LinqArray.ILookup<TKey, TValue>
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=d Kind=Parameter), (Name=defaultValue Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public static ILookup<TKey, TValue> ToLookup<TKey, TValue>(this IDictionary<TKey, TValue> d,
            TValue defaultValue = default)
        {
            return new LookupFromDictionary<TKey, TValue>(d, defaultValue);
        }

        // A public static method named GetOrDefault with a type TValue
        // operation kind is Block and type 
        // member references = this[]
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=lookup Kind=Parameter), (Name=key Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static TValue GetOrDefault<TKey, TValue>(this ILookup<TKey, TValue> lookup, TKey key)
        {
            return lookup.Contains(key) ? lookup[key] : default;
        }

        // A public static method named GetValues with a type System.Collections.Generic.IEnumerable<TValue>
        // operation kind is Block and type 
        // member references = Keys, this[]
        // assignments = 
        // Written symbols are (Name=k Kind=Parameter)
        // Read symbols are (Name=lookup Kind=Parameter), (Name=k Kind=Parameter)
        // Captured symbols are (Name=lookup Kind=Parameter)
        // Variables declared are (Name=k Kind=Parameter)
        
        public static IEnumerable<TValue> GetValues<TKey, TValue>(this ILookup<TKey, TValue> lookup)
        {
            return lookup.Keys.ToEnumerable().Select(k => lookup[k]);
        }

    } // type
} // namespace
