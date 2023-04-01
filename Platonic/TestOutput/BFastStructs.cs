/*
    BFAST - Binary Format for Array Streaming and Transmission
    Copyright 2019, VIMaec LLC
    Copyright 2018, Ara 3D, Inc.    
    Usage licensed under terms of MIT License
	https://github.com/vimaec/bfast

    The BFAST format is a simple, generic, and efficient representation of 
    buffers (arrays of binary data) with optional names.  
    
    It can be used in place of a zip when compression is not required, or when a simple protocol
    is required for transmitting data to/from disk, between processes, or over a network. 
*/

using System.Linq;

using System.Runtime.InteropServices;

namespace Vim.BFast
{
    // Type has fields True
    // Type has writable fields False
    // Type has public setters False
    public static class Constants
    {
        // A public instance field named Magic with a type long
        // No associated operation
        // No data-flow analysis could be created
                public const long Magic = 0xBFA5;

        // A public instance field named SameEndian with a type long
        // No associated operation
        // No data-flow analysis could be created
        
        // https://en.wikipedia.org/wiki/Endianness
        public const long SameEndian = Magic;

        // A public instance field named SwappedEndian with a type long
        // No associated operation
        // No data-flow analysis could be created
                public const long SwappedEndian = 0xA5BFL << 48;

        // A public instance field named ALIGNMENT with a type long
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Data arrays are aligned to 64 bytes, so that they can be cast directly to AVX-512 registers.
        /// This is useful for efficiently working with floating point data. 
        /// </summary>
        public const long ALIGNMENT = 64;

    } // type
} // namespace
namespace Vim.BFast
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public class BFastHeader
    {
        // A public instance method named Equals with a type bool
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=other Kind=Local)
        // Read symbols are (Name=this Kind=Parameter), (Name=o Kind=Parameter), (Name=other Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=other Kind=Local)
        
        public override bool Equals(object o)
        {
            return o is BFastHeader other && Equals(other);
        }

        // A public instance method named Equals with a type bool
        // operation kind is Block and type 
        // member references = Preamble, Preamble, Length, Ranges, Length, Ranges, Ranges, Ranges, Names, Names
        // assignments = 
        // Written symbols are (Name=x Kind=Parameter), (Name=y Kind=Parameter), (Name=x Kind=Parameter), (Name=x Kind=Parameter), (Name=y Kind=Parameter), (Name=x Kind=Parameter)
        // Read symbols are (Name=this Kind=Parameter), (Name=other Kind=Parameter), (Name=x Kind=Parameter), (Name=y Kind=Parameter), (Name=x Kind=Parameter), (Name=x Kind=Parameter), (Name=y Kind=Parameter), (Name=x Kind=Parameter)
        // Captured symbols are 
        // Variables declared are (Name=x Kind=Parameter), (Name=y Kind=Parameter), (Name=x Kind=Parameter), (Name=x Kind=Parameter), (Name=y Kind=Parameter), (Name=x Kind=Parameter)
        
        public bool Equals(BFastHeader other)
        {
            return Preamble.Equals(other.Preamble) &&
                   Ranges.Length == other.Ranges.Length &&
                   Ranges.Zip(other.Ranges, (x, y) => x.Equals(y)).All(x => x) &&
                   Names.Zip(other.Names, (x, y) => x.Equals(y)).All(x => x);
        }

        // A public instance field named Names with a type string[]
        // No associated operation
        // No data-flow analysis could be created
                public string[] Names;

        // A public instance field named Preamble with a type Vim.BFast.BFastPreamble
        // No associated operation
        // No data-flow analysis could be created
                public BFastPreamble Preamble;

        // A public instance field named Ranges with a type Vim.BFast.BFastRange[]
        // No associated operation
        // No data-flow analysis could be created
                public BFastRange[] Ranges;

    } // type
} // namespace
namespace Vim.BFast
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct BFastRange
    {
        // A public instance method named Equals with a type bool
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=this Kind=Parameter), (Name=other Kind=Local)
        // Read symbols are (Name=this Kind=Parameter), (Name=x Kind=Parameter), (Name=other Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=other Kind=Local)
        
        public override bool Equals(object x)
        {
            return x is BFastRange other && Equals(other);
        }

        // A public instance method named Equals with a type bool
        // operation kind is Block and type 
        // member references = Begin, Begin, End, End
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=other Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public bool Equals(BFastRange other)
        {
            return Begin == other.Begin && End == other.End;
        }

        // A public instance field named Begin with a type long
        // No associated operation
        // No data-flow analysis could be created
                public long Begin;

        // A public instance field named End with a type long
        // No associated operation
        // No data-flow analysis could be created
                public long End;

        // A public static field named Size with a type long
        // No associated operation
        // No data-flow analysis could be created
                public static long Size = 16;

        // A public instance property named Count with a type long
        // operation kind is Binary and type long
        // member references = End, Begin
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public long Count => End - Begin;

    } // type
} // namespace
namespace Vim.BFast
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct BFastPreamble
    {
        // A public instance method named Equals with a type bool
        // operation kind is Block and type 
        // member references = 
        // assignments = 
        // Written symbols are (Name=this Kind=Parameter), (Name=other Kind=Local)
        // Read symbols are (Name=this Kind=Parameter), (Name=x Kind=Parameter), (Name=other Kind=Local)
        // Captured symbols are 
        // Variables declared are (Name=other Kind=Local)
        
        public override bool Equals(object x)
        {
            return x is BFastPreamble other && Equals(other);
        }

        // A public instance method named Equals with a type bool
        // operation kind is Block and type 
        // member references = Magic, Magic, DataStart, DataStart, DataEnd, DataEnd, NumArrays, NumArrays
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter), (Name=other Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public bool Equals(BFastPreamble other)
        {
            return Magic == other.Magic && DataStart == other.DataStart && DataEnd == other.DataEnd &&
                   NumArrays == other.NumArrays;
        }

        // A public instance field named Magic with a type long
        // No associated operation
        // No data-flow analysis could be created
                public long
            Magic; // Either Constants.SameEndian or Constants.SwappedEndian depending on endianess of writer compared to reader. 

        // A public instance field named DataStart with a type long
        // No associated operation
        // No data-flow analysis could be created
        
        public long DataStart; // <= file size and >= ArrayRangesEnd and >= FileHeader.ByteCount

        // A public instance field named DataEnd with a type long
        // No associated operation
        // No data-flow analysis could be created
                public long DataEnd; // >= DataStart and <= file size

        // A public instance field named NumArrays with a type long
        // No associated operation
        // No data-flow analysis could be created
                public long NumArrays; // number of arrays 

        // A public static field named Size with a type long
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// The size of the FileHeader structure 
        /// </summary>
        public static long Size = 32;

        // A public instance property named RangesEnd with a type long
        // operation kind is Binary and type long
        // member references = Size, NumArrays
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        /// <summary>
        /// This is where the array ranges are finished. 
        /// Must be less than or equal to DataStart.
        /// Must be greater than or equal to FileHeader.ByteCount
        /// </summary>
        public long RangesEnd => Size + NumArrays * 16;

        // A public instance property named SameEndian with a type bool
        // operation kind is Binary and type bool
        // member references = Magic, SameEndian
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        /// <summary>
        /// Returns true if the producer of the BFast file has the same endianness as the current library
        /// </summary>
        public bool SameEndian => Magic == Constants.SameEndian;

    } // type
} // namespace
