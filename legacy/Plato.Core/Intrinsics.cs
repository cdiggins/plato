using System;

namespace Plato
{
    // Temperature: C, K, F
    // Area: Square Distance 
    // Volume: Cube Distance 
    public static partial class Intrinsics
    {
        public static double ToDouble(this double self) => self;
        public static double ToDouble(this float self) => self;
        public static double ToDouble(this int self) => self;
        public static double ToDouble(this uint self) => self;
        public static double ToDouble(this short self) => self;
        public static double ToDouble(this ushort self) => self;
        public static double ToDouble(this byte self) => self;
        public static double ToDouble(this sbyte self) => self;
    
        public static Angle ToDegrees(this double self) => Angle.FromDegrees(self);
        public static Angle ToRevolutions(this double self) => Angle.FromRevolutions(self);
        public static Angle ToRadians(this double self) => self;

        public static Percent ToPercent(this double self) => self;
        public static Proportion ToProportion(this double self) => self;
    }

}