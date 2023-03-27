using System;
using System.Text;

namespace Vim.G3d
{
    // http://docs.autodesk.com/FBX/2014/ENU/FBX-SDK-Documentation/index.html?url=files/GUID-CC93340E-C4A1-49EE-B048-E898F856CFBF.htm,topicNumber=d30e8478
    // https://twitter.com/FreyaHolmer/status/644881436982575104
    // https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#coordinate-system-and-units

    // The header is 7 bytes + 1 bytes padding. 
    public struct G3dHeader
    {
        public const byte MagicA = 0x63;
        public const byte MagicB = 0xD0;

        public byte magicA; // 0x63
        public byte magicB; // 0xD0
        public byte unitA; // with unitB could be: 'ft', 'yd', 'mi', 'km', 'mm', 'in', 'cm', 'm',
        public byte unitB;
        public byte upAxis; // e.g. 1=y or 2=z (could be 0=x, if you hate people)
        public byte forwardVector; // e.g. 0=x, 1=y, 2=z, 3=-x, 4=-y, 5=-z
        public byte handedness; // 0=left-handed, 1=right-handed
        public byte padding; // 0

        public string Unit => Encoding.ASCII.GetString(new[] { unitA, unitB });

        public byte[] ToBytes()
        {
            return new[] { magicA, magicB, unitA, unitB, upAxis, forwardVector, handedness, padding };
        }

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

        public static readonly string[] SupportedUnits = { "mm", "cm", "m\0", "km", "in", "ft", "yd", "mi" };

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
    }
}