using System;

using System.Text;

namespace Vim.G3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct G3dHeader
    {
        // A public instance method named ToBytes with a type byte[]
        // operation kind is Block and type 
        // member references = magicA, magicB, unitA, unitB, upAxis, forwardVector, handedness, padding
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public byte[] ToBytes()
        {
            return new[] { magicA, magicB, unitA, unitB, upAxis, forwardVector, handedness, padding };
        }

        // A public static method named FromBytes with a type Vim.G3d.G3dHeader
        // operation kind is Block and type 
        // member references = magicA, magicB, unitA, unitB, upAxis, forwardVector, handedness
        // assignments = ArrayElementReference, ArrayElementReference, ArrayElementReference, ArrayElementReference, ArrayElementReference, ArrayElementReference, ArrayElementReference
        // Written symbols are 
        // Read symbols are (Name=bytes Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public static G3dHeader FromBytes(byte[] bytes)
        {
            return new G3dHeader
                {
                    magicA = bytes[0],
                    magicB = bytes[1],
                    unitA = bytes[2],
                    unitB = bytes[3],
                    upAxis = bytes[4],
                    forwardVector = bytes[5],
                    handedness = bytes[6]
                }
                .Validate();
        }

        // A public instance method named Validate with a type Vim.G3d.G3dHeader
        // operation kind is Block and type 
        // member references = magicA, magicA, magicB, magicB, SupportedUnits, Unit, Unit, SupportedUnits, upAxis, upAxis, forwardVector, forwardVector, handedness, handedness
        // assignments = 
        // Written symbols are (Name=this Kind=Parameter)
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public G3dHeader Validate()
        {
            if (magicA != 0x63) throw new Exception($"First magic number must be 0x63 not {magicA}");
            if (magicB != 0xD0) throw new Exception($"Second magic number must be 0xD0 not {magicB}");
            if (Array.IndexOf(SupportedUnits, Unit) < 0)
                throw new Exception($"Unit {Unit} is not a supported unit: {string.Join(", ", SupportedUnits)}");
            if (upAxis < 0 || upAxis > 2) throw new Exception("Up axis must be 0(x), 1(y), or 2(z)");
            if (forwardVector < 0 || forwardVector > 5)
                throw new Exception("Front vector must be 0 (x), 1(y), 2(z), 3(-x), 4(-y), or 5(-z)");
            if (handedness < 0 || handedness > 1) throw new Exception("Handedness must be 0 (left) or 1 (right");
            return this;
        }

        // A public instance field named MagicA with a type byte
        // No associated operation
        // No data-flow analysis could be created
                public const byte MagicA = 0x63;

        // A public instance field named MagicB with a type byte
        // No associated operation
        // No data-flow analysis could be created
                public const byte MagicB = 0xD0;

        // A public instance field named magicA with a type byte
        // No associated operation
        // No data-flow analysis could be created
        
        public byte magicA; // 0x63

        // A public instance field named magicB with a type byte
        // No associated operation
        // No data-flow analysis could be created
                public byte magicB; // 0xD0

        // A public instance field named unitA with a type byte
        // No associated operation
        // No data-flow analysis could be created
                public byte unitA; // with unitB could be: 'ft', 'yd', 'mi', 'km', 'mm', 'in', 'cm', 'm',

        // A public instance field named unitB with a type byte
        // No associated operation
        // No data-flow analysis could be created
                public byte unitB;

        // A public instance field named upAxis with a type byte
        // No associated operation
        // No data-flow analysis could be created
                public byte upAxis; // e.g. 1=y or 2=z (could be 0=x, if you hate people)

        // A public instance field named forwardVector with a type byte
        // No associated operation
        // No data-flow analysis could be created
                public byte forwardVector; // e.g. 0=x, 1=y, 2=z, 3=-x, 4=-y, 5=-z

        // A public instance field named handedness with a type byte
        // No associated operation
        // No data-flow analysis could be created
                public byte handedness; // 0=left-handed, 1=right-handed

        // A public instance field named padding with a type byte
        // No associated operation
        // No data-flow analysis could be created
                public byte padding; // 0

        // A public static field named Default with a type Vim.G3d.G3dHeader
        // No associated operation
        // No data-flow analysis could be created
        
        public static G3dHeader Default
            = new G3dHeader
            {
                magicA = 0x63,
                magicB = 0xD0,
                unitA = (byte)'m',
                unitB = 0,
                upAxis = 2,
                forwardVector = 0,
                handedness = 0,
                padding = 0
            };

        // A public static field named SupportedUnits with a type string[]
        // No associated operation
        // No data-flow analysis could be created
        
        public static readonly string[] SupportedUnits = { "mm", "cm", "m\0", "km", "in", "ft", "yd", "mi" };

        // A public instance property named Unit with a type string
        // operation kind is Invocation and type string
        // member references = ASCII, unitA, unitB
        // assignments = 
        // Written symbols are 
        // Read symbols are (Name=this Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
        
        public string Unit => Encoding.ASCII.GetString(new[] { unitA, unitB });

    } // type
} // namespace
