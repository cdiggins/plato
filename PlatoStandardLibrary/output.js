// This is prepended to every JavaScript program generated from Plato

class Intrinsics {
    static fieldNames(obj) {

    }

    static fieldValues(obj) {

    }

    static fieldTypes(obj) {

    }
}

class PrimitiveValue {
    constructor(value, jsClass) {
        this.Value = value;
        this.Class = jsClass;
        this.Create = function (x) {
            return new jsClass(x);
        }
    }

    Add(a, b) { return this.Create(a.Value + b.Value); }
    Subtract(a, b) { return this.Create(a.Value - b.Value); }
    Multiply(a, b) { return this.Create(a.Value * b.Value); }
    Divide(a, b) { return this.Create(a.Value / b.Value); }
    Modulo(a, b) { return this.Create(a.Value % b.Value); }
    And(a, b) { return this.Create(a.Value && b.Value); }
    Or(a, b) { return this.Create(a.Value || b.Value); }
    Negative(a) { return this.Create(-a.Value); }
    Not(a) { return this.Create(!a.Value); }
    Equals(a, b) { return this.Create(a.Value === b.Value); }
    NotEquals(a, b) { return this.Create(a.Value !== b.Value); }
    Compare(a, b) { return this.Create(a.Value < b.Value ? -1 : a.Value > b.Value ? +1 : 0); }
    LessThan(a, b) { return this.Create(a.Value < b.Value); }
    GreaterThan(a, b) { return this.Create(a.Value > b.Value); }
    LessThanOrEquals(a, b) { return this.Create(a.Value <= b.Value); }
    GreaterThanOrEquals(a, b) { return this.Create(a.Value >= b.Value); }
    ToString(a) { return new PrimitiveString(a.Value); }
}

class PrimitiveType extends PrimitiveValue {
    constructor(value) { super(value, PrimitiveType); }
}

class PrimitiveInteger extends PrimitiveValue {
    constructor(value) { super(value, PrimitiveInteger); }
    static Zero() { return new PrimitiveInteger(0); }
    static One() { return new PrimitiveInteger(1); }
    static Default() { return new PrimitiveInteger(0); }
    static MinValue() { return new PrimitiveInteger(Number.MIN_SAFE_INTEGER); }
    static MaxValue() { return new PrimitiveInteger(Number.MAX_SAFE_INTEGER); }
}

class PrimitiveBoolean extends PrimitiveValue {
    constructor(value) { super(value, PrimitiveBoolean); }
    static Zero() { return new PrimitiveBoolean(false); }
    static One() { return new PrimitiveBoolean(true); }
    static Default() { return new PrimitiveBoolean(false); }
    static MinValue() { return new PrimitiveBoolean(false); }
    static MaxValue() { return new PrimitiveBoolean(true); }
}

class PrimitiveNumber extends PrimitiveValue {
    constructor(value) { super(value, PrimitiveNumber); }
    static Zero() { return new PrimitiveNumber(0); }
    static One() { return new PrimitiveNumber(1); }
    static Default() { return new PrimitiveNumber(0); }
    static MinValue() { return new PrimitiveNumber(Number.MIN_VALUE); }
    static MaxValue() { return new PrimitiveNumber(Number.MAX_VALUE); }
}

class PrimitiveString extends PrimitiveValue {
    constructor(value) { super(value, PrimitiveArray); }
    static Zero() { return new PrimitiveString(""); }
    static One() { return new PrimitiveString(""); }
    static Default() { return new PrimitiveString(""); }    
    static MinValue() { return new PrimitiveString(""); }    
    static MaxValue() { return new PrimitiveString(""); }    
}

class TestClass {
    constructor(x) { this.value = x; }
    f(x) { return x + this.value; }
}

function PrimitiveArray(ElementType) {
    var r = class PrimitiveArrayInternal extends PrimitiveValue {
        constructor(value) {
            super(value, PrimitiveArray);
            let self = this;
            // Add a mapped version of each function in ElementType
            Object.getOwnPropertyNames(ElementType.prototype).forEach(function (key) {
                console.log("Adding " + key + " method");
                self[key] = function (args) {
                    return self.map(function (x) {
                        return x[key](args);
                    });
                }
            });
        }
        add(x) { this.Value.push(x); }
        at(index) { return this.Value[index]; }
        count() { return this.Value.length; }
        map(f) { return new PrimitiveArrayInternal(this.Value.map(f)); }
    }
    r.T = ElementType;

    return r;
} 

var t = PrimitiveArray(TestClass);
console.log(t);
var xs = new t([]);
console.log(xs);
xs.add(new TestClass(1));
xs.add(new TestClass(2));
console.log(xs);
console.log(xs.at(0));
console.log(xs.at(1));
let ys = xs.f(3);
console.log(ys);

// TODO:
// Implement an Array concept? What is interesting is that there are two parts to it.
// There is an element type, and an "implementing type".




class Interval_134_Library
{
    static Size_2509 = function (x_2496/* : Interval_24 */) /* : UnknownType */{ return Subtract_185/* : Self_7 */(Max_243/* : UnknownType */(x_2496/* : UnknownType */)/* : UnknownType */, Min_240/* : UnknownType */(x_2496/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static IsEmpty_2523 = function (x_2510/* : Interval_24 */) /* : UnknownType */{ return GreaterThanOrEquals_2005/* : UnknownType */(Min_240/* : UnknownType */(x_2510/* : UnknownType */)/* : UnknownType */, Max_243/* : UnknownType */(x_2510/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Lerp_2553 = function (x_2524/* : Interval_24 */, amount_2525/* : UnknownType */) /* : UnknownType */{ return Multiply_172/* : Self_7 */(Min_240/* : UnknownType */(x_2524/* : UnknownType */)/* : UnknownType */, Add_163/* : UnknownType */(Subtract_185/* : UnknownType */(1/* : Float64_12 */, amount_2525/* : UnknownType */)/* : UnknownType */, Multiply_172/* : UnknownType */(Max_243/* : UnknownType */(x_2524/* : UnknownType */)/* : UnknownType */, amount_2525/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static InverseLerp_2573 = function (x_2554/* : Interval_24 */, value_2555/* : ScalarArithmetic_21 */) /* : UnknownType */{ return Divide_175/* : Self_7 */(Subtract_185/* : UnknownType */(value_2555/* : UnknownType */, Min_240/* : UnknownType */(x_2554/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Size_1121/* : UnknownType */(x_2554/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static Negate_2593 = function (x_2574/* : Interval_24 */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(Negative_166/* : UnknownType */(Max_243/* : UnknownType */(x_2574/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Negative_166/* : UnknownType */(Min_240/* : UnknownType */(x_2574/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Reverse_2607 = function (x_2594/* : Interval_24 */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(Max_243/* : UnknownType */(x_2594/* : UnknownType */)/* : UnknownType */, Min_240/* : UnknownType */(x_2594/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Resize_2627 = function (x_2608/* : Interval_24 */, size_2609/* : UnknownType */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(Min_240/* : UnknownType */(x_2608/* : UnknownType */)/* : UnknownType */, Add_163/* : UnknownType */(Min_240/* : UnknownType */(x_2608/* : UnknownType */)/* : UnknownType */, size_2609/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Center_2635 = function (x_2628/* : Interval_24 */) /* : UnknownType */{ return Lerp_1831/* : Self_7 */(x_2628/* : UnknownType */, 0.5/* : Float64_12 */)/* : Self_7 */; };
    static Contains_2660 = function (x_2636/* : Interval_24 */, value_2637/* : UnknownType */) /* : UnknownType */{ return LessThanOrEquals_2001/* : UnknownType */(Min_240/* : UnknownType */(x_2636/* : UnknownType */)/* : UnknownType */, And_197/* : UnknownType */(value_2637/* : UnknownType */, LessThanOrEquals_2001/* : UnknownType */(value_2637/* : UnknownType */, Max_243/* : UnknownType */(x_2636/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Contains_2688 = function (x_2661/* : Interval_24 */, other_2662/* : UnknownType */) /* : UnknownType */{ return LessThanOrEquals_2001/* : UnknownType */(Min_240/* : UnknownType */(x_2661/* : UnknownType */)/* : UnknownType */, And_197/* : UnknownType */(Min_240/* : UnknownType */(other_2662/* : UnknownType */)/* : UnknownType */, GreaterThanOrEquals_2005/* : UnknownType */(Max_243/* : UnknownType */, Max_243/* : UnknownType */(other_2662/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Overlaps_2703 = function (x_2689/* : Interval_24 */, y_2690/* : UnknownType */) /* : UnknownType */{ return Not_203/* : Self_7 */(IsEmpty_1829/* : UnknownType */(Clamp_1867/* : UnknownType */(x_2689/* : UnknownType */, y_2690/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static Split_2722 = function (x_2704/* : Interval_24 */, t_2705/* : Interval_134 */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(Left_1853/* : UnknownType */(x_2704/* : UnknownType */, t_2705/* : UnknownType */)/* : UnknownType */, Right_1855/* : UnknownType */(x_2704/* : UnknownType */, t_2705/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Split_2730 = function (x_2723/* : Interval_24 */) /* : UnknownType */{ return Split_1849/* : UnknownType */(x_2723/* : UnknownType */, 0.5/* : Float64_12 */)/* : UnknownType */; };
    static Left_2744 = function (x_2731/* : Interval_24 */, t_2732/* : Interval_134 */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(Min_240/* : UnknownType */, Lerp_1831/* : UnknownType */(x_2731/* : UnknownType */, t_2732/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Right_2761 = function (x_2745/* : Interval_24 */, t_2746/* : Interval_134 */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(Lerp_1831/* : UnknownType */(x_2745/* : UnknownType */, t_2746/* : UnknownType */)/* : UnknownType */, Max_243/* : UnknownType */(x_2745/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static MoveTo_2778 = function (x_2762/* : Interval_24 */, t_2763/* : UnknownType */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(t_2763/* : UnknownType */, Add_163/* : UnknownType */(t_2763/* : UnknownType */, Size_1121/* : UnknownType */(x_2762/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static LeftHalf_2786 = function (x_2779/* : Interval_24 */) /* : UnknownType */{ return Left_1853/* : UnknownType */(x_2779/* : UnknownType */, 0.5/* : Float64_12 */)/* : UnknownType */; };
    static RightHalf_2794 = function (x_2787/* : Interval_24 */) /* : UnknownType */{ return Right_1855/* : UnknownType */(x_2787/* : UnknownType */, 0.5/* : Float64_12 */)/* : UnknownType */; };
    static HalfSize_2803 = function (x_2795/* : Interval_24 */) /* : UnknownType */{ return Half_1939/* : UnknownType */(Size_1121/* : UnknownType */(x_2795/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Recenter_2828 = function (x_2804/* : Interval_24 */, c_2805/* : UnknownType */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(Subtract_185/* : UnknownType */(c_2805/* : UnknownType */, HalfSize_1863/* : UnknownType */(x_2804/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Add_163/* : UnknownType */(c_2805/* : UnknownType */, HalfSize_1863/* : UnknownType */(x_2804/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Clamp_2853 = function (x_2829/* : Interval_24 */, y_2830/* : UnknownType */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(Clamp_1867/* : UnknownType */(x_2829/* : UnknownType */, Min_240/* : UnknownType */(y_2830/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Clamp_1867/* : UnknownType */(x_2829/* : UnknownType */, Max_243/* : UnknownType */(y_2830/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Clamp_2885 = function (x_2854/* : Interval_24 */, value_2855/* : Comparable_137 */) /* : UnknownType */{ return LessThan_1995/* : UnknownType */(value_2855/* : UnknownType */, Min_240/* : UnknownType */(x_2854/* : UnknownType */)/* : UnknownType */
        ? Min_240/* : UnknownType */(x_2854/* : UnknownType */)/* : UnknownType */
        : GreaterThan_2003/* : UnknownType */(value_2855/* : UnknownType */, Max_243/* : UnknownType */(x_2854/* : UnknownType */)/* : UnknownType */
            ? Max_243/* : UnknownType */(x_2854/* : UnknownType */)/* : UnknownType */
            : value_2855/* : UnknownType */
        )/* : UnknownType */
    )/* : UnknownType */; };
    static Between_2910 = function (x_2886/* : Interval_24 */, value_2887/* : Comparable_137 */) /* : UnknownType */{ return GreaterThanOrEquals_2005/* : UnknownType */(value_2887/* : UnknownType */, And_197/* : UnknownType */(Min_240/* : UnknownType */(x_2886/* : UnknownType */)/* : UnknownType */, LessThanOrEquals_2001/* : UnknownType */(value_2887/* : UnknownType */, Max_243/* : UnknownType */(x_2886/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Unit_2917 = function () /* : UnknownType */{ return Tuple_1/* : UnknownType */(0/* : Integer_11 */, 1/* : Integer_11 */)/* : UnknownType */; };
}
class Vector_135_Library
{
    static Sum_2927 = function (v_2918/* : Vector_14 */) /* : UnknownType */{ return Aggregate_2031/* : UnknownType */(v_2918/* : UnknownType */, 0/* : Integer_11 */, Add_163/* : UnknownType */)/* : UnknownType */; };
    static SumSquares_2940 = function (v_2928/* : Vector_14 */) /* : UnknownType */{ return Aggregate_2031/* : UnknownType */(Square_1921/* : UnknownType */(v_2928/* : UnknownType */)/* : UnknownType */, 0/* : Integer_11 */, Add_163/* : UnknownType */)/* : UnknownType */; };
    static LengthSquared_2946 = function (v_2941/* : Vector_14 */) /* : UnknownType */{ return SumSquares_1877/* : UnknownType */(v_2941/* : UnknownType */)/* : UnknownType */; };
    static Length_2955 = function (v_2947/* : Vector_14 */) /* : UnknownType */{ return SquareRoot_1917/* : UnknownType */(LengthSquared_1879/* : UnknownType */(v_2947/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Dot_2967 = function (v1_2956/* : Vector_14 */, v2_2957/* : UnknownType */) /* : UnknownType */{ return Sum_1875/* : UnknownType */(Multiply_172/* : UnknownType */(v1_2956/* : UnknownType */, v2_2957/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Numerical_136_Library
{
    static Cos_2970 = function (x_2968/* : Numerical_16 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static Sin_2973 = function (x_2971/* : Numerical_16 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static Tan_2976 = function (x_2974/* : Numerical_16 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static Acos_2979 = function (x_2977/* : Numerical_16 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static Asin_2982 = function (x_2980/* : Numerical_16 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static Atan_2985 = function (x_2983/* : Numerical_16 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static Cosh_2988 = function (x_2986/* : Numerical_16 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static Sinh_2991 = function (x_2989/* : Numerical_16 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static Tanh_2994 = function (x_2992/* : Numerical_16 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static Acosh_2997 = function (x_2995/* : Numerical_16 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static Asinh_3000 = function (x_2998/* : Numerical_16 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static Atanh_3003 = function (x_3001/* : Numerical_16 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static Pow_3007 = function (x_3004/* : Numerical_16 */, y_3005/* : Any_6 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static Log_3011 = function (x_3008/* : Numerical_16 */, y_3009/* : Any_6 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static NaturalLog_3014 = function (x_3012/* : Numerical_16 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static NaturalPower_3017 = function (x_3015/* : Numerical_16 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static SquareRoot_3025 = function (x_3018/* : Numerical_16 */) /* : UnknownType */{ return Pow_1909/* : UnknownType */(x_3018/* : UnknownType */, 0.5/* : Float64_12 */)/* : UnknownType */; };
    static CubeRoot_3033 = function (x_3026/* : Numerical_16 */) /* : UnknownType */{ return Pow_1909/* : UnknownType */(x_3026/* : UnknownType */, 0.5/* : Float64_12 */)/* : UnknownType */; };
    static Square_3041 = function (x_3034/* : Numerical_16 */) /* : UnknownType */{ return Multiply_172/* : Self_7 */(Value_253/* : UnknownType */, Value_253/* : UnknownType */)/* : Self_7 */; };
    static Clamp_3056 = function (x_3042/* : Numerical_16 */, min_3043/* : Any_6 */, max_3044/* : Any_6 */) /* : UnknownType */{ return Clamp_1867/* : UnknownType */(x_3042/* : UnknownType */, Interval_134_Library/* : UnknownType */(min_3043/* : UnknownType */, max_3044/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Clamp_3065 = function (x_3057/* : Numerical_16 */, i_3058/* : UnknownType */) /* : UnknownType */{ return Clamp_1867/* : UnknownType */(i_3058/* : UnknownType */, x_3057/* : UnknownType */)/* : UnknownType */; };
    static Clamp_3075 = function (x_3066/* : Numerical_16 */) /* : UnknownType */{ return Clamp_1867/* : UnknownType */(x_3066/* : UnknownType */, 0/* : Integer_11 */, 1/* : Integer_11 */)/* : UnknownType */; };
    static PlusOne_3083 = function (x_3076/* : Numerical_16 */) /* : UnknownType */{ return Add_163/* : Self_7 */(x_3076/* : UnknownType */, 1/* : Integer_11 */)/* : Self_7 */; };
    static MinusOne_3091 = function (x_3084/* : Numerical_16 */) /* : UnknownType */{ return Subtract_185/* : Self_7 */(x_3084/* : UnknownType */, 1/* : Integer_11 */)/* : Self_7 */; };
    static FromOne_3099 = function (x_3092/* : Numerical_16 */) /* : UnknownType */{ return Subtract_185/* : Self_7 */(1/* : Integer_11 */, x_3092/* : UnknownType */)/* : Self_7 */; };
    static Sign_3121 = function (x_3100/* : Numerical_16 */) /* : UnknownType */{ return LessThan_1995/* : UnknownType */(x_3100/* : UnknownType */, 0/* : Integer_11 */
        ? Negative_166/* : UnknownType */(1/* : Integer_11 */)/* : UnknownType */
        : GreaterThan_2003/* : UnknownType */(x_3100/* : UnknownType */, 0/* : Integer_11 */
            ? 1/* : Integer_11 */
            : 0/* : Integer_11 */
        )/* : UnknownType */
    )/* : UnknownType */; };
    static Abs_3135 = function (x_3122/* : Numerical_16 */) /* : UnknownType */{ return LessThan_1995/* : UnknownType */(Value_253/* : UnknownType */, 0/* : Integer_11 */
        ? Negative_166/* : UnknownType */(Value_253/* : UnknownType */)/* : UnknownType */
        : Value_253/* : UnknownType */
    )/* : UnknownType */; };
    static Half_3143 = function (x_3136/* : Numerical_16 */) /* : UnknownType */{ return Divide_175/* : Self_7 */(x_3136/* : UnknownType */, 2/* : Integer_11 */)/* : Self_7 */; };
    static Third_3151 = function (x_3144/* : Numerical_16 */) /* : UnknownType */{ return Divide_175/* : Self_7 */(x_3144/* : UnknownType */, 3/* : Integer_11 */)/* : Self_7 */; };
    static Quarter_3159 = function (x_3152/* : Numerical_16 */) /* : UnknownType */{ return Divide_175/* : Self_7 */(x_3152/* : UnknownType */, 4/* : Integer_11 */)/* : Self_7 */; };
    static Fifth_3167 = function (x_3160/* : Numerical_16 */) /* : UnknownType */{ return Divide_175/* : Self_7 */(x_3160/* : UnknownType */, 5/* : Integer_11 */)/* : Self_7 */; };
    static Sixth_3175 = function (x_3168/* : Numerical_16 */) /* : UnknownType */{ return Divide_175/* : Self_7 */(x_3168/* : UnknownType */, 6/* : Integer_11 */)/* : Self_7 */; };
    static Seventh_3183 = function (x_3176/* : Numerical_16 */) /* : UnknownType */{ return Divide_175/* : Self_7 */(x_3176/* : UnknownType */, 7/* : Integer_11 */)/* : Self_7 */; };
    static Eighth_3191 = function (x_3184/* : Numerical_16 */) /* : UnknownType */{ return Divide_175/* : Self_7 */(x_3184/* : UnknownType */, 8/* : Integer_11 */)/* : Self_7 */; };
    static Ninth_3199 = function (x_3192/* : Numerical_16 */) /* : UnknownType */{ return Divide_175/* : Self_7 */(x_3192/* : UnknownType */, 9/* : Integer_11 */)/* : Self_7 */; };
    static Tenth_3207 = function (x_3200/* : Numerical_16 */) /* : UnknownType */{ return Divide_175/* : Self_7 */(x_3200/* : UnknownType */, 10/* : Integer_11 */)/* : Self_7 */; };
    static Sixteenth_3215 = function (x_3208/* : Numerical_16 */) /* : UnknownType */{ return Divide_175/* : Self_7 */(x_3208/* : UnknownType */, 16/* : Integer_11 */)/* : Self_7 */; };
    static Hundredth_3223 = function (x_3216/* : Numerical_16 */) /* : UnknownType */{ return Divide_175/* : Self_7 */(x_3216/* : UnknownType */, 100/* : Integer_11 */)/* : Self_7 */; };
    static Thousandth_3231 = function (x_3224/* : Numerical_16 */) /* : UnknownType */{ return Divide_175/* : Self_7 */(x_3224/* : UnknownType */, 1000/* : Integer_11 */)/* : Self_7 */; };
    static Millionth_3244 = function (x_3232/* : Numerical_16 */) /* : UnknownType */{ return Divide_175/* : Self_7 */(x_3232/* : UnknownType */, Divide_175/* : UnknownType */(1000/* : Integer_11 */, 1000/* : Integer_11 */)/* : UnknownType */)/* : Self_7 */; };
    static Billionth_3262 = function (x_3245/* : Numerical_16 */) /* : UnknownType */{ return Divide_175/* : Self_7 */(x_3245/* : UnknownType */, Divide_175/* : UnknownType */(1000/* : Integer_11 */, Divide_175/* : UnknownType */(1000/* : Integer_11 */, 1000/* : Integer_11 */)/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static Hundred_3270 = function (x_3263/* : Numerical_16 */) /* : UnknownType */{ return Multiply_172/* : Self_7 */(x_3263/* : UnknownType */, 100/* : Integer_11 */)/* : Self_7 */; };
    static Thousand_3278 = function (x_3271/* : Numerical_16 */) /* : UnknownType */{ return Multiply_172/* : Self_7 */(x_3271/* : UnknownType */, 1000/* : Integer_11 */)/* : Self_7 */; };
    static Million_3291 = function (x_3279/* : Numerical_16 */) /* : UnknownType */{ return Multiply_172/* : Self_7 */(x_3279/* : UnknownType */, Multiply_172/* : UnknownType */(1000/* : Integer_11 */, 1000/* : Integer_11 */)/* : UnknownType */)/* : Self_7 */; };
    static Billion_3309 = function (x_3292/* : Numerical_16 */) /* : UnknownType */{ return Multiply_172/* : Self_7 */(x_3292/* : UnknownType */, Multiply_172/* : UnknownType */(1000/* : Integer_11 */, Multiply_172/* : UnknownType */(1000/* : Integer_11 */, 1000/* : Integer_11 */)/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static Twice_3317 = function (x_3310/* : Numerical_16 */) /* : UnknownType */{ return Multiply_172/* : Self_7 */(x_3310/* : UnknownType */, 2/* : Integer_11 */)/* : Self_7 */; };
    static Thrice_3325 = function (x_3318/* : Numerical_16 */) /* : UnknownType */{ return Multiply_172/* : Self_7 */(x_3318/* : UnknownType */, 3/* : Integer_11 */)/* : Self_7 */; };
    static SmoothStep_3344 = function (x_3326/* : Numerical_16 */) /* : UnknownType */{ return Multiply_172/* : Self_7 */(Square_1921/* : UnknownType */(x_3326/* : UnknownType */)/* : UnknownType */, Subtract_185/* : UnknownType */(3/* : Integer_11 */, Twice_1975/* : UnknownType */(x_3326/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static Pow2_3352 = function (x_3345/* : Numerical_16 */) /* : UnknownType */{ return Multiply_172/* : Self_7 */(x_3345/* : UnknownType */, x_3345/* : UnknownType */)/* : Self_7 */; };
    static Pow3_3363 = function (x_3353/* : Numerical_16 */) /* : UnknownType */{ return Multiply_172/* : Self_7 */(Pow2_1981/* : UnknownType */(x_3353/* : UnknownType */)/* : UnknownType */, x_3353/* : UnknownType */)/* : Self_7 */; };
    static Pow4_3374 = function (x_3364/* : Numerical_16 */) /* : UnknownType */{ return Multiply_172/* : Self_7 */(Pow3_1983/* : UnknownType */(x_3364/* : UnknownType */)/* : UnknownType */, x_3364/* : UnknownType */)/* : Self_7 */; };
    static Pow5_3385 = function (x_3375/* : Numerical_16 */) /* : UnknownType */{ return Multiply_172/* : Self_7 */(Pow4_1985/* : UnknownType */(x_3375/* : UnknownType */)/* : UnknownType */, x_3375/* : UnknownType */)/* : Self_7 */; };
    static Turns_3398 = function (x_3386/* : Numerical_16 */) /* : UnknownType */{ return Multiply_172/* : Self_7 */(x_3386/* : UnknownType */, Multiply_172/* : UnknownType */(3.1415926535897/* : Float64_12 */, 2/* : Integer_11 */)/* : UnknownType */)/* : Self_7 */; };
    static AlmostZero_3409 = function (x_3399/* : Numerical_16 */) /* : UnknownType */{ return LessThan_1995/* : UnknownType */(Abs_1937/* : UnknownType */(x_3399/* : UnknownType */)/* : UnknownType */, 1E-08/* : Float64_12 */)/* : UnknownType */; };
}
class Comparable_137_Library
{
    static Equals_3423 = function (a_3410/* : Comparable_18 */, b_3411/* : Comparable_18 */) /* : UnknownType */{ return Equals_160/* : Boolean_22 */(Compare_157/* : UnknownType */(a_3410/* : UnknownType */, b_3411/* : UnknownType */)/* : UnknownType */, 0/* : Integer_11 */)/* : Boolean_22 */; };
    static LessThan_3437 = function (a_3424/* : Comparable_18 */, b_3425/* : Comparable_18 */) /* : UnknownType */{ return LessThan_1995/* : UnknownType */(Compare_157/* : UnknownType */(a_3424/* : UnknownType */, b_3425/* : UnknownType */)/* : UnknownType */, 0/* : Integer_11 */)/* : UnknownType */; };
    static Lesser_3449 = function (a_3438/* : Comparable_18 */, b_3439/* : Comparable_137 */) /* : UnknownType */{ return LessThanOrEquals_2001/* : UnknownType */(a_3438/* : UnknownType */, b_3439/* : UnknownType */)/* : UnknownType */
        ? a_3438/* : Comparable_18 */
        : b_3439/* : Comparable_137 */
    ; };
    static Greater_3461 = function (a_3450/* : Comparable_18 */, b_3451/* : Comparable_137 */) /* : UnknownType */{ return GreaterThanOrEquals_2005/* : UnknownType */(a_3450/* : UnknownType */, b_3451/* : UnknownType */)/* : UnknownType */
        ? a_3450/* : Comparable_18 */
        : b_3451/* : Comparable_137 */
    ; };
    static LessThanOrEquals_3475 = function (a_3462/* : Comparable_18 */, b_3463/* : Comparable_18 */) /* : UnknownType */{ return LessThanOrEquals_2001/* : UnknownType */(Compare_157/* : UnknownType */(a_3462/* : UnknownType */, b_3463/* : UnknownType */)/* : UnknownType */, 0/* : Integer_11 */)/* : UnknownType */; };
    static GreaterThan_3489 = function (a_3476/* : Comparable_18 */, b_3477/* : Comparable_18 */) /* : UnknownType */{ return GreaterThan_2003/* : UnknownType */(Compare_157/* : UnknownType */(a_3476/* : UnknownType */, b_3477/* : UnknownType */)/* : UnknownType */, 0/* : Integer_11 */)/* : UnknownType */; };
    static GreaterThanOrEquals_3503 = function (a_3490/* : Comparable_18 */, b_3491/* : Comparable_18 */) /* : UnknownType */{ return GreaterThanOrEquals_2005/* : UnknownType */(Compare_157/* : UnknownType */(a_3490/* : UnknownType */, b_3491/* : UnknownType */)/* : UnknownType */, 0/* : Integer_11 */)/* : UnknownType */; };
    static Min_3515 = function (a_3504/* : Comparable_18 */, b_3505/* : Comparable_137 */) /* : UnknownType */{ return LessThan_1995/* : UnknownType */(a_3504/* : UnknownType */, b_3505/* : UnknownType */)/* : UnknownType */
        ? a_3504/* : Comparable_18 */
        : b_3505/* : Comparable_137 */
    ; };
    static Max_3527 = function (a_3516/* : Comparable_18 */, b_3517/* : Comparable_137 */) /* : UnknownType */{ return GreaterThan_2003/* : UnknownType */(a_3516/* : UnknownType */, b_3517/* : UnknownType */)/* : UnknownType */
        ? a_3516/* : Comparable_18 */
        : b_3517/* : Comparable_137 */
    ; };
    static Between_3542 = function (v_3528/* : Comparable_18 */, a_3529/* : Any_6 */, b_3530/* : Any_6 */) /* : UnknownType */{ return Between_1871/* : UnknownType */(v_3528/* : UnknownType */, Interval_134_Library/* : UnknownType */(a_3529/* : UnknownType */, b_3530/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Between_3551 = function (v_3543/* : Comparable_18 */, i_3544/* : Interval_134 */) /* : UnknownType */{ return Contains_1843/* : UnknownType */(i_3544/* : UnknownType */, v_3543/* : UnknownType */)/* : UnknownType */; };
}
class Boolean_138_Library
{
    static XOr_3561 = function (a_3552/* : Boolean_22 */, b_3553/* : Boolean_22 */) /* : UnknownType */{ return a_3552/* : UnknownType */
        ? Not_203/* : Self_7 */(b_3553/* : UnknownType */)/* : Self_7 */
        : b_3553/* : Boolean_22 */
    ; };
    static NAnd_3573 = function (a_3562/* : Boolean_22 */, b_3563/* : Boolean_22 */) /* : UnknownType */{ return Not_203/* : Self_7 */(And_197/* : UnknownType */(a_3562/* : UnknownType */, b_3563/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static NOr_3585 = function (a_3574/* : Boolean_22 */, b_3575/* : Boolean_22 */) /* : UnknownType */{ return Not_203/* : Self_7 */(Or_200/* : UnknownType */(a_3574/* : UnknownType */, b_3575/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
}
class Equatable_139_Library
{
    static NotEquals_3594 = function (x_3586/* : Equatable_19 */) /* : UnknownType */{ return Not_203/* : Self_7 */(Equals_160/* : UnknownType */(x_3586/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
}
class Array_140_Library
{
    static Map_3617 = function (xs_3595/* : Array_25 */, f_3596/* : Function_5 */) /* : UnknownType */{ return Map_2023/* : UnknownType */(Count_27_Type/* : UnknownType */(xs_3595/* : UnknownType */)/* : UnknownType */, function (i_3602/* : UnknownType */) /* : Lambda_4 */{ return f_3596/* : UnknownType */(At_148/* : UnknownType */(xs_3595/* : UnknownType */, i_3602/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */; };
    static Zip_3646 = function (xs_3618/* : Array_25 */, ys_3619/* : UnknownType */, f_3620/* : Function_5 */) /* : UnknownType */{ return Array_140_Library/* : Array_140 */(Count_27_Type/* : UnknownType */(xs_3618/* : UnknownType */)/* : UnknownType */, function (i_3626/* : UnknownType */) /* : Lambda_4 */{ return f_3620/* : UnknownType */(At_148/* : UnknownType */(i_3626/* : UnknownType */)/* : UnknownType */, At_148/* : UnknownType */(ys_3619/* : UnknownType */, i_3626/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Array_140 */; };
    static Skip_3671 = function (xs_3647/* : Array_25 */, n_3648/* : ScalarArithmetic_21 */) /* : UnknownType */{ return Array_140_Library/* : Array_140 */(Subtract_185/* : UnknownType */(Count_27_Type/* : UnknownType */, n_3648/* : UnknownType */)/* : UnknownType */, function (i_3656/* : UnknownType */) /* : Lambda_4 */{ return At_148/* : UnknownType */(Subtract_185/* : UnknownType */(i_3656/* : UnknownType */, n_3648/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Array_140 */; };
    static Take_3683 = function (xs_3672/* : Array_25 */, n_3673/* : Any_6 */) /* : UnknownType */{ return Array_140_Library/* : Array_140 */(n_3673/* : UnknownType */, function (i_3676/* : UnknownType */) /* : Lambda_4 */{ return At_148/* : UnknownType */; })/* : Array_140 */; };
    static Aggregate_3705 = function (xs_3684/* : Array_25 */, init_3685/* : Any_6 */, f_3686/* : Function_5 */) /* : UnknownType */{ return IsEmpty_1829/* : UnknownType */(xs_3684/* : UnknownType */)/* : UnknownType */
        ? init_3685/* : Any_6 */
        : f_3686/* : Function_5 */(init_3685/* : UnknownType */, f_3686/* : UnknownType */(Rest_2033/* : UnknownType */(xs_3684/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Function_5 */
    ; };
    static Rest_3711 = function (xs_3706/* : Array_25 */) /* : UnknownType */{ return Skip_2027/* : Array_140 */(1/* : Integer_11 */)/* : Array_140 */; };
    static IsEmpty_3722 = function (xs_3712/* : Array_25 */) /* : UnknownType */{ return Equals_160/* : Boolean_22 */(Count_27_Type/* : UnknownType */(xs_3712/* : UnknownType */)/* : UnknownType */, 0/* : Integer_11 */)/* : Boolean_22 */; };
    static First_3730 = function (xs_3723/* : Array_25 */) /* : UnknownType */{ return At_148/* : T_143 */(xs_3723/* : UnknownType */, 0/* : Integer_11 */)/* : T_143 */; };
    static Last_3746 = function (xs_3731/* : Array_25 */) /* : UnknownType */{ return At_148/* : T_143 */(xs_3731/* : UnknownType */, Subtract_185/* : UnknownType */(Count_27_Type/* : UnknownType */(xs_3731/* : UnknownType */)/* : UnknownType */, 1/* : Integer_11 */)/* : UnknownType */)/* : T_143 */; };
    static Slice_3761 = function (xs_3747/* : Array_25 */, from_3748/* : Array_140 */, count_3749/* : Array_140 */) /* : UnknownType */{ return Take_2029/* : Array_140 */(Skip_2027/* : UnknownType */(xs_3747/* : UnknownType */, from_3748/* : UnknownType */)/* : UnknownType */, count_3749/* : UnknownType */)/* : Array_140 */; };
    static Join_3805 = function (xs_3762/* : Array_25 */, sep_3763/* : Intrinsics_142 */) /* : UnknownType */{ return IsEmpty_1829/* : UnknownType */(xs_3762/* : UnknownType */)/* : UnknownType */
        ? ""/* : String_9 */
        : Add_163/* : Self_7 */(ToString_236/* : UnknownType */(First_2037/* : UnknownType */(xs_3762/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Aggregate_2031/* : UnknownType */(Skip_2027/* : UnknownType */(xs_3762/* : UnknownType */, 1/* : Integer_11 */)/* : UnknownType */, ""/* : String_9 */, function (acc_3786/* : UnknownType */, cur_3787/* : UnknownType */) /* : Lambda_4 */{ return Interpolate_2115/* : UnknownType */(acc_3786/* : UnknownType */, sep_3763/* : UnknownType */, cur_3787/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */)/* : Self_7 */
    ; };
    static All_3832 = function (xs_3806/* : Array_25 */, f_3807/* : Function_5 */) /* : UnknownType */{ return IsEmpty_1829/* : UnknownType */(xs_3806/* : UnknownType */)/* : UnknownType */
        ? True/* : Boolean_10 */
        : And_197/* : Self_7 */(f_3807/* : UnknownType */(First_2037/* : UnknownType */(xs_3806/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, f_3807/* : UnknownType */(Rest_2033/* : UnknownType */(xs_3806/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Self_7 */
    ; };
    static JoinStrings_3877 = function (xs_3833/* : Array_25 */, sep_3834/* : Any_6 */) /* : UnknownType */{ return IsEmpty_1829/* : UnknownType */(xs_3833/* : UnknownType */)/* : UnknownType */
        ? ""/* : String_9 */
        : Add_163/* : Self_7 */(First_2037/* : UnknownType */(xs_3833/* : UnknownType */)/* : UnknownType */, Aggregate_2031/* : UnknownType */(Rest_2033/* : UnknownType */(xs_3833/* : UnknownType */)/* : UnknownType */, ""/* : String_9 */, function (x_3852/* : UnknownType */, acc_3853/* : UnknownType */) /* : Lambda_4 */{ return Add_163/* : UnknownType */(acc_3853/* : UnknownType */, Add_163/* : UnknownType */(", "/* : String_9 */, ToString_236/* : UnknownType */(x_3852/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */)/* : Self_7 */
    ; };
}
class Easings_141_Library
{
    static BlendEaseFunc_3926 = function (p_3878/* : UnknownType */, easeIn_3879/* : Function_5 */, easeOut_3880/* : Function_5 */) /* : UnknownType */{ return LessThan_1995/* : UnknownType */(p_3878/* : UnknownType */, 0.5/* : Float64_12 */
        ? Multiply_172/* : UnknownType */(0.5/* : Float64_12 */, easeIn_3879/* : UnknownType */(Multiply_172/* : UnknownType */(p_3878/* : UnknownType */, 2/* : Integer_11 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
        : Multiply_172/* : UnknownType */(0.5/* : Float64_12 */, Add_163/* : UnknownType */(easeOut_3880/* : UnknownType */(Multiply_172/* : UnknownType */(p_3878/* : UnknownType */, Subtract_185/* : UnknownType */(2/* : Integer_11 */, 1/* : Integer_11 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, 0.5/* : Float64_12 */)/* : UnknownType */)/* : UnknownType */
    )/* : UnknownType */; };
    static InvertEaseFunc_3943 = function (p_3927/* : ScalarArithmetic_21 */, easeIn_3928/* : Function_5 */) /* : UnknownType */{ return Subtract_185/* : Self_7 */(1/* : Integer_11 */, easeIn_3928/* : UnknownType */(Subtract_185/* : UnknownType */(1/* : Integer_11 */, p_3927/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static Linear_3946 = function (p_3944/* : Any_6 */) /* : UnknownType */{ return p_3944/* : Any_6 */; };
    static QuadraticEaseIn_3952 = function (p_3947/* : Numerical_136 */) /* : UnknownType */{ return Pow2_1981/* : Self_7 */(p_3947/* : UnknownType */)/* : Self_7 */; };
    static QuadraticEaseOut_3960 = function (p_3953/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2051/* : Self_7 */(p_3953/* : UnknownType */, QuadraticEaseIn_2055/* : UnknownType */)/* : Self_7 */; };
    static QuadraticEaseInOut_3970 = function (p_3961/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2049/* : UnknownType */(p_3961/* : UnknownType */, QuadraticEaseIn_2055/* : UnknownType */, QuadraticEaseOut_2057/* : UnknownType */)/* : UnknownType */; };
    static CubicEaseIn_3976 = function (p_3971/* : Numerical_136 */) /* : UnknownType */{ return Pow3_1983/* : Self_7 */(p_3971/* : UnknownType */)/* : Self_7 */; };
    static CubicEaseOut_3984 = function (p_3977/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2051/* : Self_7 */(p_3977/* : UnknownType */, CubicEaseIn_2061/* : UnknownType */)/* : Self_7 */; };
    static CubicEaseInOut_3994 = function (p_3985/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2049/* : UnknownType */(p_3985/* : UnknownType */, CubicEaseIn_2061/* : UnknownType */, CubicEaseOut_2063/* : UnknownType */)/* : UnknownType */; };
    static QuarticEaseIn_4000 = function (p_3995/* : Numerical_136 */) /* : UnknownType */{ return Pow4_1985/* : Self_7 */(p_3995/* : UnknownType */)/* : Self_7 */; };
    static QuarticEaseOut_4008 = function (p_4001/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2051/* : Self_7 */(p_4001/* : UnknownType */, QuarticEaseIn_2067/* : UnknownType */)/* : Self_7 */; };
    static QuarticEaseInOut_4018 = function (p_4009/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2049/* : UnknownType */(p_4009/* : UnknownType */, QuarticEaseIn_2067/* : UnknownType */, QuarticEaseOut_2069/* : UnknownType */)/* : UnknownType */; };
    static QuinticEaseIn_4024 = function (p_4019/* : Numerical_136 */) /* : UnknownType */{ return Pow5_1987/* : Self_7 */(p_4019/* : UnknownType */)/* : Self_7 */; };
    static QuinticEaseOut_4032 = function (p_4025/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2051/* : Self_7 */(p_4025/* : UnknownType */, QuinticEaseIn_2073/* : UnknownType */)/* : Self_7 */; };
    static QuinticEaseInOut_4042 = function (p_4033/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2049/* : UnknownType */(p_4033/* : UnknownType */, QuinticEaseIn_2073/* : UnknownType */, QuinticEaseOut_2075/* : UnknownType */)/* : UnknownType */; };
    static SineEaseIn_4050 = function (p_4043/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2051/* : Self_7 */(p_4043/* : UnknownType */, SineEaseOut_2081/* : UnknownType */)/* : Self_7 */; };
    static SineEaseOut_4062 = function (p_4051/* : Numerical_136 */) /* : UnknownType */{ return Sin_1887/* : UnknownType */(Turns_1989/* : UnknownType */(Quarter_1943/* : UnknownType */(p_4051/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static SineEaseInOut_4072 = function (p_4063/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2049/* : UnknownType */(p_4063/* : UnknownType */, SineEaseIn_2079/* : UnknownType */, SineEaseOut_2081/* : UnknownType */)/* : UnknownType */; };
    static CircularEaseIn_4087 = function (p_4073/* : Numerical_136 */) /* : UnknownType */{ return FromOne_1933/* : Self_7 */(SquareRoot_1917/* : UnknownType */(FromOne_1933/* : UnknownType */(Pow2_1981/* : UnknownType */(p_4073/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static CircularEaseOut_4095 = function (p_4088/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2051/* : Self_7 */(p_4088/* : UnknownType */, CircularEaseIn_2085/* : UnknownType */)/* : Self_7 */; };
    static CircularEaseInOut_4105 = function (p_4096/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2049/* : UnknownType */(p_4096/* : UnknownType */, CircularEaseIn_2085/* : UnknownType */, CircularEaseOut_2087/* : UnknownType */)/* : UnknownType */; };
    static ExponentialEaseIn_4127 = function (p_4106/* : Numerical_136 */) /* : UnknownType */{ return AlmostZero_1991/* : UnknownType */(p_4106/* : UnknownType */)/* : UnknownType */
        ? p_4106/* : Numerical_136 */
        : Pow_1909/* : UnknownType */(2/* : Integer_11 */, Multiply_172/* : UnknownType */(10/* : Integer_11 */, MinusOne_1931/* : UnknownType */(p_4106/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
    ; };
    static ExponentialEaseOut_4135 = function (p_4128/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2051/* : Self_7 */(p_4128/* : UnknownType */, ExponentialEaseIn_2091/* : UnknownType */)/* : Self_7 */; };
    static ExponentialEaseInOut_4145 = function (p_4136/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2049/* : UnknownType */(p_4136/* : UnknownType */, ExponentialEaseIn_2091/* : UnknownType */, ExponentialEaseOut_2093/* : UnknownType */)/* : UnknownType */; };
    static ElasticEaseIn_4183 = function (p_4146/* : Numerical_136 */) /* : UnknownType */{ return Multiply_172/* : Self_7 */(13/* : Integer_11 */, Multiply_172/* : UnknownType */(Turns_1989/* : UnknownType */(Quarter_1943/* : UnknownType */(p_4146/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Sin_1887/* : UnknownType */(Radians_1149/* : UnknownType */(Pow_1909/* : UnknownType */(2/* : Integer_11 */, Multiply_172/* : UnknownType */(10/* : Integer_11 */, MinusOne_1931/* : UnknownType */(p_4146/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static ElasticEaseOut_4191 = function (p_4184/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2051/* : Self_7 */(p_4184/* : UnknownType */, ElasticEaseIn_2097/* : UnknownType */)/* : Self_7 */; };
    static ElasticEaseInOut_4201 = function (p_4192/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2049/* : UnknownType */(p_4192/* : UnknownType */, ElasticEaseIn_2097/* : UnknownType */, ElasticEaseOut_2099/* : UnknownType */)/* : UnknownType */; };
    static BackEaseIn_4226 = function (p_4202/* : UnknownType */) /* : UnknownType */{ return Subtract_185/* : Self_7 */(Pow3_1983/* : UnknownType */(p_4202/* : UnknownType */)/* : UnknownType */, Multiply_172/* : UnknownType */(p_4202/* : UnknownType */, Sin_1887/* : UnknownType */(Turns_1989/* : UnknownType */(Half_1939/* : UnknownType */(p_4202/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static BackEaseOut_4234 = function (p_4227/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2051/* : Self_7 */(p_4227/* : UnknownType */, BackEaseIn_2103/* : UnknownType */)/* : Self_7 */; };
    static BackEaseInOut_4244 = function (p_4235/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2049/* : UnknownType */(p_4235/* : UnknownType */, BackEaseIn_2103/* : UnknownType */, BackEaseOut_2105/* : UnknownType */)/* : UnknownType */; };
    static BounceEaseIn_4252 = function (p_4245/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2051/* : Self_7 */(p_4245/* : UnknownType */, BounceEaseOut_2111/* : UnknownType */)/* : Self_7 */; };
    static BounceEaseOut_4421 = function (p_4253/* : UnknownType */) /* : UnknownType */{ return LessThan_1995/* : UnknownType */(p_4253/* : UnknownType */, Divide_175/* : UnknownType */(4/* : Integer_11 */, 11/* : Float64_12 */)/* : UnknownType */)/* : UnknownType */
        ? Multiply_172/* : Self_7 */(121/* : Float64_12 */, Divide_175/* : UnknownType */(Pow2_1981/* : UnknownType */(p_4253/* : UnknownType */)/* : UnknownType */, 16/* : Float64_12 */)/* : UnknownType */)/* : Self_7 */
        : LessThan_1995/* : UnknownType */(p_4253/* : UnknownType */, Divide_175/* : UnknownType */(8/* : Integer_11 */, 11/* : Float64_12 */)/* : UnknownType */)/* : UnknownType */
            ? Divide_175/* : Self_7 */(363/* : Float64_12 */, Multiply_172/* : UnknownType */(40/* : Float64_12 */, Subtract_185/* : UnknownType */(Pow2_1981/* : UnknownType */(p_4253/* : UnknownType */)/* : UnknownType */, Divide_175/* : UnknownType */(99/* : Float64_12 */, Multiply_172/* : UnknownType */(10/* : Float64_12 */, Add_163/* : UnknownType */(p_4253/* : UnknownType */, Divide_175/* : UnknownType */(17/* : Float64_12 */, 5/* : Float64_12 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Self_7 */
            : LessThan_1995/* : UnknownType */(p_4253/* : UnknownType */, Divide_175/* : UnknownType */(9/* : Integer_11 */, 10/* : Float64_12 */)/* : UnknownType */)/* : UnknownType */
                ? Divide_175/* : Self_7 */(4356/* : Float64_12 */, Multiply_172/* : UnknownType */(361/* : Float64_12 */, Subtract_185/* : UnknownType */(Pow2_1981/* : UnknownType */(p_4253/* : UnknownType */)/* : UnknownType */, Divide_175/* : UnknownType */(35442/* : Float64_12 */, Multiply_172/* : UnknownType */(1805/* : Float64_12 */, Add_163/* : UnknownType */(p_4253/* : UnknownType */, Divide_175/* : UnknownType */(16061/* : Float64_12 */, 1805/* : Float64_12 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Self_7 */
                : Divide_175/* : Self_7 */(54/* : Float64_12 */, Multiply_172/* : UnknownType */(5/* : Float64_12 */, Subtract_185/* : UnknownType */(Pow2_1981/* : UnknownType */(p_4253/* : UnknownType */)/* : UnknownType */, Divide_175/* : UnknownType */(513/* : Float64_12 */, Multiply_172/* : UnknownType */(25/* : Float64_12 */, Add_163/* : UnknownType */(p_4253/* : UnknownType */, Divide_175/* : UnknownType */(268/* : Float64_12 */, 25/* : Float64_12 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Self_7 */


    ; };
    static BounceEaseInOut_4431 = function (p_4422/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2049/* : UnknownType */(p_4422/* : UnknownType */, BounceEaseIn_2109/* : UnknownType */, BounceEaseOut_2111/* : UnknownType */)/* : UnknownType */; };
}
class Intrinsics_142_Library
{
    static Interpolate_4434 = function (xs_4432/* : Any_6 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static Throw_4437 = function (x_4435/* : Any_6 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static TypeOf_4440 = function (x_4438/* : Any_6 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
    static New_4443 = function (x_4441/* : Any_6 */) /* : UnknownType */{ return intrinsic_0/* : UnknownType */; };
}
class Vector_14_Concept
{
    constructor(self) { this.Self = self; };
    static Count_2133 = function (v_2125/* : Vector_14 */) /* : Count_27 */{ return Count_27_Type/* : Count_27 */(FieldTypes_210/* : UnknownType */(Self_7_Primitive/* : UnknownType */)/* : UnknownType */)/* : Count_27 */; };
    static At_2147 = function (v_2135/* : Vector_14 */, n_2137/* : Index_28 */) /* : T_143 */{ return At_148/* : T_143 */(FieldValues_218/* : UnknownType */(v_2135/* : UnknownType */)/* : UnknownType */, n_2137/* : UnknownType */)/* : T_143 */; };
}
class Measure_15_Concept
{
    constructor(self) { this.Self = self; };
    static Value_2159 = function (x_2149/* : Self_7 */) /* : Number_29 */{ return At_148/* : T_143 */(FieldValues_218/* : UnknownType */(x_2149/* : UnknownType */)/* : UnknownType */, 0/* : Integer_11 */)/* : T_143 */; };
}
class Numerical_16_Concept
{
    constructor(self) { this.Self = self; };
}
class Magnitude_17_Concept
{
    constructor(self) { this.Self = self; };
    static Magnitude_2175 = function (x_2161/* : Self_7 */) /* : Number_29 */{ return SquareRoot_1917/* : UnknownType */(Sum_1875/* : UnknownType */(Square_1921/* : UnknownType */(FieldValues_218/* : UnknownType */(x_2161/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Comparable_18_Concept
{
    constructor(self) { this.Self = self; };
    static Compare_2212 = function (a_2177/* : Self_7 */, b_2179/* : Self_7 */) /* : Integer_26 */{ return LessThan_1995/* : UnknownType */(Magnitude_154/* : UnknownType */(a_2177/* : UnknownType */)/* : UnknownType */, Magnitude_154/* : UnknownType */(b_2179/* : UnknownType */)/* : UnknownType */
        ? Negative_166/* : UnknownType */(1/* : Integer_11 */)/* : UnknownType */
        : GreaterThan_2003/* : UnknownType */(Magnitude_154/* : UnknownType */(a_2177/* : UnknownType */)/* : UnknownType */, Magnitude_154/* : UnknownType */(b_2179/* : UnknownType */)/* : UnknownType */
            ? 1/* : Integer_11 */
            : 0/* : Integer_11 */
        )/* : UnknownType */
    )/* : UnknownType */; };
}
class Equatable_19_Concept
{
    constructor(self) { this.Self = self; };
    static Equals_2232 = function (a_2214/* : Self_7 */, b_2216/* : Self_7 */) /* : Boolean_22 */{ return All_2045/* : UnknownType */(Equals_160/* : UnknownType */(FieldValues_218/* : UnknownType */(a_2214/* : UnknownType */)/* : UnknownType */, FieldValues_218/* : UnknownType */(b_2216/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Arithmetic_20_Concept
{
    constructor(self) { this.Self = self; };
    static Add_2249 = function (self_2234/* : Self_7 */, other_2236/* : Self_7 */) /* : Self_7 */{ return Add_163/* : Self_7 */(FieldValues_218/* : UnknownType */(self_2234/* : UnknownType */)/* : UnknownType */, FieldValues_218/* : UnknownType */(other_2236/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static Negative_2259 = function (self_2251/* : Self_7 */) /* : Self_7 */{ return Negative_166/* : Self_7 */(FieldValues_218/* : UnknownType */(self_2251/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static Reciprocal_2269 = function (self_2261/* : Self_7 */) /* : Self_7 */{ return Reciprocal_169/* : Self_7 */(FieldValues_218/* : UnknownType */(self_2261/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static Multiply_2286 = function (self_2271/* : Self_7 */, other_2273/* : Self_7 */) /* : Self_7 */{ return Add_163/* : Self_7 */(FieldValues_218/* : UnknownType */(self_2271/* : UnknownType */)/* : UnknownType */, FieldValues_218/* : UnknownType */(other_2273/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static Divide_2303 = function (self_2288/* : Self_7 */, other_2290/* : Self_7 */) /* : Self_7 */{ return Divide_175/* : Self_7 */(FieldValues_218/* : UnknownType */(self_2288/* : UnknownType */)/* : UnknownType */, FieldValues_218/* : UnknownType */(other_2290/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static Modulo_2320 = function (self_2305/* : Self_7 */, other_2307/* : Self_7 */) /* : Self_7 */{ return Modulo_178/* : Self_7 */(FieldValues_218/* : UnknownType */(self_2305/* : UnknownType */)/* : UnknownType */, FieldValues_218/* : UnknownType */(other_2307/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
}
class ScalarArithmetic_21_Concept
{
    constructor(self) { this.Self = self; };
    static Add_2335 = function (self_2323/* : Self_7 */, scalar_2325/* : T_2321 */) /* : Self_7 */{ return Add_163/* : Self_7 */(FieldValues_218/* : UnknownType */(self_2323/* : UnknownType */)/* : UnknownType */, scalar_2325/* : UnknownType */)/* : Self_7 */; };
    static Subtract_2349 = function (self_2337/* : Self_7 */, scalar_2339/* : T_2321 */) /* : Self_7 */{ return Add_163/* : Self_7 */(self_2337/* : UnknownType */, Negative_166/* : UnknownType */(scalar_2339/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static Multiply_2363 = function (self_2351/* : Self_7 */, scalar_2353/* : T_2321 */) /* : Self_7 */{ return Multiply_172/* : Self_7 */(FieldValues_218/* : UnknownType */(self_2351/* : UnknownType */)/* : UnknownType */, scalar_2353/* : UnknownType */)/* : Self_7 */; };
    static Divide_2377 = function (self_2365/* : Self_7 */, scalar_2367/* : T_2321 */) /* : Self_7 */{ return Multiply_172/* : Self_7 */(self_2365/* : UnknownType */, Reciprocal_169/* : UnknownType */(scalar_2367/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static Modulo_2391 = function (self_2379/* : Self_7 */, scalar_2381/* : T_2321 */) /* : Self_7 */{ return Modulo_178/* : Self_7 */(FieldValues_218/* : UnknownType */(self_2379/* : UnknownType */)/* : UnknownType */, scalar_2381/* : UnknownType */)/* : Self_7 */; };
}
class Boolean_22_Concept
{
    constructor(self) { this.Self = self; };
    static And_2408 = function (a_2393/* : Self_7 */, b_2395/* : Self_7 */) /* : Self_7 */{ return And_197/* : Self_7 */(FieldValues_218/* : UnknownType */(a_2393/* : UnknownType */)/* : UnknownType */, FieldValues_218/* : UnknownType */(b_2395/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static Or_2425 = function (a_2410/* : Self_7 */, b_2412/* : Self_7 */) /* : Self_7 */{ return Or_200/* : Self_7 */(FieldValues_218/* : UnknownType */(a_2410/* : UnknownType */)/* : UnknownType */, FieldValues_218/* : UnknownType */(b_2412/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
    static Not_2435 = function (a_2427/* : Self_7 */) /* : Self_7 */{ return Not_203/* : Self_7 */(FieldValues_218/* : UnknownType */(a_2427/* : UnknownType */)/* : UnknownType */)/* : Self_7 */; };
}
class Value_23_Concept
{
    constructor(self) { this.Self = self; };
    static Type_2437 = function () /* : Type_13 */{ return intrinsic_0/* : UnknownType */; };
    static FieldTypes_2439 = function () /* : Array_25 */{ return intrinsic_0/* : UnknownType */; };
    static FieldNames_2441 = function () /* : Array_25 */{ return intrinsic_0/* : UnknownType */; };
    static FieldValues_2445 = function (self_2443/* : Self_7 */) /* : Array_25 */{ return intrinsic_0/* : UnknownType */; };
    static Zero_2450 = function () /* : Self_7 */{ return Zero_221/* : Self_7 */(FieldTypes_210/* : UnknownType */)/* : Self_7 */; };
    static One_2455 = function () /* : Self_7 */{ return One_224/* : Self_7 */(FieldTypes_210/* : UnknownType */)/* : Self_7 */; };
    static Default_2460 = function () /* : Self_7 */{ return Default_227/* : Self_7 */(FieldTypes_210/* : UnknownType */)/* : Self_7 */; };
    static MinValue_2465 = function () /* : Self_7 */{ return MinValue_230/* : Self_7 */(FieldTypes_210/* : UnknownType */)/* : Self_7 */; };
    static MaxValue_2470 = function () /* : Self_7 */{ return MaxValue_233/* : Self_7 */(FieldTypes_210/* : UnknownType */)/* : Self_7 */; };
    static ToString_2479 = function (x_2472/* : Self_7 */) /* : String_9 */{ return JoinStrings_2047/* : UnknownType */(FieldValues_218/* : UnknownType */, ","/* : String_9 */)/* : UnknownType */; };
}
class Interval_24_Concept
{
    constructor(self) { this.Self = self; };
    static Min_2483 = function (x_2482/* : Self_7 */) /* : T_238 */{ return null; };
    static Max_2486 = function (x_2485/* : Self_7 */) /* : T_238 */{ return null; };
}
class Array_25_Concept
{
    constructor(self) { this.Self = self; };
    static Count_2490 = function (xs_2489/* : Self_7 */) /* : Count_27 */{ return null; };
    static At_2495 = function (xs_2492/* : Self_7 */, n_2494/* : Index_28 */) /* : T_245 */{ return null; };
}
class Integer_26_Type
{
    constructor(Value_253)
    {
        // field initialization 
        this.Value_253 = Value_253;
        this.Type_2437 = Integer_26_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Integer_26_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Integer_26_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Integer_26_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Integer_26_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Integer_26_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Integer_26_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Integer_26_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Integer_26_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Integer_26_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = Integer_26_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = Integer_26_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = Integer_26_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = Integer_26_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = Integer_26_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = Integer_26_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = Integer_26_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Integer_26_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Integer_26_Type.Magnitude_17_Concept.Magnitude_2175;
    }
    // field accessors
    static Value_253 = function(self) { return self.Value_253; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Integer_26_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(Integer_26_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Integer_26_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Integer_26_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Integer_26_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(Integer_26_Type);
    static Implements = [Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept];
}
class Count_27_Type
{
    constructor(Value_260)
    {
        // field initialization 
        this.Value_260 = Value_260;
        this.Type_2437 = Count_27_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Count_27_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Count_27_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Count_27_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Count_27_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Count_27_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Count_27_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Count_27_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Count_27_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Count_27_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = Count_27_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = Count_27_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = Count_27_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = Count_27_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = Count_27_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = Count_27_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = Count_27_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Count_27_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Count_27_Type.Magnitude_17_Concept.Magnitude_2175;
    }
    // field accessors
    static Value_260 = function(self) { return self.Value_260; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Count_27_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(Count_27_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Count_27_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Count_27_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Count_27_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(Count_27_Type);
    static Implements = [Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept];
}
class Index_28_Type
{
    constructor(Value_267)
    {
        // field initialization 
        this.Value_267 = Value_267;
        this.Type_2437 = Index_28_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Index_28_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Index_28_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Index_28_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Index_28_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Index_28_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Index_28_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Index_28_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Index_28_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Index_28_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Value_267 = function(self) { return self.Value_267; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Index_28_Type);
    static Implements = [Value_23_Concept];
}
class Number_29_Type
{
    constructor(Value_274)
    {
        // field initialization 
        this.Value_274 = Value_274;
        this.Type_2437 = Number_29_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Number_29_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Number_29_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Number_29_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Number_29_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Number_29_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Number_29_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Number_29_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Number_29_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Number_29_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = Number_29_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = Number_29_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = Number_29_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = Number_29_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = Number_29_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = Number_29_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = Number_29_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Number_29_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Number_29_Type.Magnitude_17_Concept.Magnitude_2175;
    }
    // field accessors
    static Value_274 = function(self) { return self.Value_274; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Number_29_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(Number_29_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Number_29_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Number_29_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Number_29_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(Number_29_Type);
    static Implements = [Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept];
}
class Unit_30_Type
{
    constructor(Value_281)
    {
        // field initialization 
        this.Value_281 = Value_281;
        this.Type_2437 = Unit_30_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Unit_30_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Unit_30_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Unit_30_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Unit_30_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Unit_30_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Unit_30_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Unit_30_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Unit_30_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Unit_30_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = Unit_30_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = Unit_30_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = Unit_30_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = Unit_30_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = Unit_30_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = Unit_30_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = Unit_30_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Unit_30_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Unit_30_Type.Magnitude_17_Concept.Magnitude_2175;
    }
    // field accessors
    static Value_281 = function(self) { return self.Value_281; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Unit_30_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(Unit_30_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Unit_30_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Unit_30_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Unit_30_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(Unit_30_Type);
    static Implements = [Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept];
}
class Percent_31_Type
{
    constructor(Value_288)
    {
        // field initialization 
        this.Value_288 = Value_288;
        this.Type_2437 = Percent_31_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Percent_31_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Percent_31_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Percent_31_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Percent_31_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Percent_31_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Percent_31_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Percent_31_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Percent_31_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Percent_31_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = Percent_31_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = Percent_31_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = Percent_31_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = Percent_31_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = Percent_31_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = Percent_31_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = Percent_31_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Percent_31_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Percent_31_Type.Magnitude_17_Concept.Magnitude_2175;
    }
    // field accessors
    static Value_288 = function(self) { return self.Value_288; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Percent_31_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(Percent_31_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Percent_31_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Percent_31_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Percent_31_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(Percent_31_Type);
    static Implements = [Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept];
}
class Quaternion_32_Type
{
    constructor(X_295, Y_302, Z_309, W_316)
    {
        // field initialization 
        this.X_295 = X_295;
        this.Y_302 = Y_302;
        this.Z_309 = Z_309;
        this.W_316 = W_316;
        this.Type_2437 = Quaternion_32_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Quaternion_32_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Quaternion_32_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Quaternion_32_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Quaternion_32_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Quaternion_32_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Quaternion_32_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Quaternion_32_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Quaternion_32_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Quaternion_32_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static X_295 = function(self) { return self.X_295; }
    static Y_302 = function(self) { return self.Y_302; }
    static Z_309 = function(self) { return self.Z_309; }
    static W_316 = function(self) { return self.W_316; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Quaternion_32_Type);
    static Implements = [Value_23_Concept];
}
class Unit2D_33_Type
{
    constructor(X_323, Y_330)
    {
        // field initialization 
        this.X_323 = X_323;
        this.Y_330 = Y_330;
        this.Type_2437 = Unit2D_33_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Unit2D_33_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Unit2D_33_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Unit2D_33_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Unit2D_33_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Unit2D_33_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Unit2D_33_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Unit2D_33_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Unit2D_33_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Unit2D_33_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static X_323 = function(self) { return self.X_323; }
    static Y_330 = function(self) { return self.Y_330; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Unit2D_33_Type);
    static Implements = [Value_23_Concept];
}
class Unit3D_34_Type
{
    constructor(X_337, Y_344, Z_351)
    {
        // field initialization 
        this.X_337 = X_337;
        this.Y_344 = Y_344;
        this.Z_351 = Z_351;
        this.Type_2437 = Unit3D_34_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Unit3D_34_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Unit3D_34_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Unit3D_34_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Unit3D_34_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Unit3D_34_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Unit3D_34_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Unit3D_34_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Unit3D_34_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Unit3D_34_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static X_337 = function(self) { return self.X_337; }
    static Y_344 = function(self) { return self.Y_344; }
    static Z_351 = function(self) { return self.Z_351; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Unit3D_34_Type);
    static Implements = [Value_23_Concept];
}
class Direction3D_35_Type
{
    constructor(Value_358)
    {
        // field initialization 
        this.Value_358 = Value_358;
        this.Type_2437 = Direction3D_35_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Direction3D_35_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Direction3D_35_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Direction3D_35_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Direction3D_35_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Direction3D_35_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Direction3D_35_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Direction3D_35_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Direction3D_35_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Direction3D_35_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Value_358 = function(self) { return self.Value_358; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Direction3D_35_Type);
    static Implements = [Value_23_Concept];
}
class AxisAngle_36_Type
{
    constructor(Axis_365, Angle_372)
    {
        // field initialization 
        this.Axis_365 = Axis_365;
        this.Angle_372 = Angle_372;
        this.Type_2437 = AxisAngle_36_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = AxisAngle_36_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = AxisAngle_36_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = AxisAngle_36_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = AxisAngle_36_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = AxisAngle_36_Type.Value_23_Concept.One_2455;
        this.Default_2460 = AxisAngle_36_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = AxisAngle_36_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = AxisAngle_36_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = AxisAngle_36_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Axis_365 = function(self) { return self.Axis_365; }
    static Angle_372 = function(self) { return self.Angle_372; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(AxisAngle_36_Type);
    static Implements = [Value_23_Concept];
}
class EulerAngles_37_Type
{
    constructor(Yaw_379, Pitch_386, Roll_393)
    {
        // field initialization 
        this.Yaw_379 = Yaw_379;
        this.Pitch_386 = Pitch_386;
        this.Roll_393 = Roll_393;
        this.Type_2437 = EulerAngles_37_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = EulerAngles_37_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = EulerAngles_37_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = EulerAngles_37_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = EulerAngles_37_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = EulerAngles_37_Type.Value_23_Concept.One_2455;
        this.Default_2460 = EulerAngles_37_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = EulerAngles_37_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = EulerAngles_37_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = EulerAngles_37_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Yaw_379 = function(self) { return self.Yaw_379; }
    static Pitch_386 = function(self) { return self.Pitch_386; }
    static Roll_393 = function(self) { return self.Roll_393; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(EulerAngles_37_Type);
    static Implements = [Value_23_Concept];
}
class Rotation3D_38_Type
{
    constructor(Quaternion_400)
    {
        // field initialization 
        this.Quaternion_400 = Quaternion_400;
        this.Type_2437 = Rotation3D_38_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Rotation3D_38_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Rotation3D_38_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Rotation3D_38_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Rotation3D_38_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Rotation3D_38_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Rotation3D_38_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Rotation3D_38_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Rotation3D_38_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Rotation3D_38_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Quaternion_400 = function(self) { return self.Quaternion_400; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Rotation3D_38_Type);
    static Implements = [Value_23_Concept];
}
class Vector2D_39_Type
{
    constructor(X_407, Y_414)
    {
        // field initialization 
        this.X_407 = X_407;
        this.Y_414 = Y_414;
        this.Count_2490 = Vector2D_39_Type.Array_25_Concept.Count_2490;
        this.At_2495 = Vector2D_39_Type.Array_25_Concept.At_2495;
        this.Type_2437 = Vector2D_39_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Vector2D_39_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Vector2D_39_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Vector2D_39_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Vector2D_39_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Vector2D_39_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Vector2D_39_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Vector2D_39_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Vector2D_39_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Vector2D_39_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = Vector2D_39_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = Vector2D_39_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = Vector2D_39_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = Vector2D_39_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = Vector2D_39_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = Vector2D_39_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = Vector2D_39_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Vector2D_39_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Vector2D_39_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Count_2133 = Vector2D_39_Type.Vector_14_Concept.Count_2133;
        this.At_2147 = Vector2D_39_Type.Vector_14_Concept.At_2147;
    }
    // field accessors
    static X_407 = function(self) { return self.X_407; }
    static Y_414 = function(self) { return self.Y_414; }
    // implemented concepts 
    static Array_25_Concept = new Array_25_Concept(Vector2D_39_Type);
    static Value_23_Concept = new Value_23_Concept(Vector2D_39_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(Vector2D_39_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Vector2D_39_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Vector2D_39_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Vector2D_39_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(Vector2D_39_Type);
    static Vector_14_Concept = new Vector_14_Concept(Vector2D_39_Type);
    static Implements = [Array_25_Concept,Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept,Vector_14_Concept];
}
class Vector3D_40_Type
{
    constructor(X_421, Y_428, Z_435)
    {
        // field initialization 
        this.X_421 = X_421;
        this.Y_428 = Y_428;
        this.Z_435 = Z_435;
        this.Count_2490 = Vector3D_40_Type.Array_25_Concept.Count_2490;
        this.At_2495 = Vector3D_40_Type.Array_25_Concept.At_2495;
        this.Type_2437 = Vector3D_40_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Vector3D_40_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Vector3D_40_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Vector3D_40_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Vector3D_40_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Vector3D_40_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Vector3D_40_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Vector3D_40_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Vector3D_40_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Vector3D_40_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = Vector3D_40_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = Vector3D_40_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = Vector3D_40_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = Vector3D_40_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = Vector3D_40_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = Vector3D_40_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = Vector3D_40_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Vector3D_40_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Vector3D_40_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Count_2133 = Vector3D_40_Type.Vector_14_Concept.Count_2133;
        this.At_2147 = Vector3D_40_Type.Vector_14_Concept.At_2147;
    }
    // field accessors
    static X_421 = function(self) { return self.X_421; }
    static Y_428 = function(self) { return self.Y_428; }
    static Z_435 = function(self) { return self.Z_435; }
    // implemented concepts 
    static Array_25_Concept = new Array_25_Concept(Vector3D_40_Type);
    static Value_23_Concept = new Value_23_Concept(Vector3D_40_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(Vector3D_40_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Vector3D_40_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Vector3D_40_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Vector3D_40_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(Vector3D_40_Type);
    static Vector_14_Concept = new Vector_14_Concept(Vector3D_40_Type);
    static Implements = [Array_25_Concept,Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept,Vector_14_Concept];
}
class Vector4D_41_Type
{
    constructor(X_442, Y_449, Z_456, W_463)
    {
        // field initialization 
        this.X_442 = X_442;
        this.Y_449 = Y_449;
        this.Z_456 = Z_456;
        this.W_463 = W_463;
        this.Count_2490 = Vector4D_41_Type.Array_25_Concept.Count_2490;
        this.At_2495 = Vector4D_41_Type.Array_25_Concept.At_2495;
        this.Type_2437 = Vector4D_41_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Vector4D_41_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Vector4D_41_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Vector4D_41_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Vector4D_41_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Vector4D_41_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Vector4D_41_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Vector4D_41_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Vector4D_41_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Vector4D_41_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = Vector4D_41_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = Vector4D_41_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = Vector4D_41_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = Vector4D_41_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = Vector4D_41_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = Vector4D_41_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = Vector4D_41_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Vector4D_41_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Vector4D_41_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Count_2133 = Vector4D_41_Type.Vector_14_Concept.Count_2133;
        this.At_2147 = Vector4D_41_Type.Vector_14_Concept.At_2147;
    }
    // field accessors
    static X_442 = function(self) { return self.X_442; }
    static Y_449 = function(self) { return self.Y_449; }
    static Z_456 = function(self) { return self.Z_456; }
    static W_463 = function(self) { return self.W_463; }
    // implemented concepts 
    static Array_25_Concept = new Array_25_Concept(Vector4D_41_Type);
    static Value_23_Concept = new Value_23_Concept(Vector4D_41_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(Vector4D_41_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Vector4D_41_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Vector4D_41_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Vector4D_41_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(Vector4D_41_Type);
    static Vector_14_Concept = new Vector_14_Concept(Vector4D_41_Type);
    static Implements = [Array_25_Concept,Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept,Vector_14_Concept];
}
class Orientation3D_42_Type
{
    constructor(Value_470)
    {
        // field initialization 
        this.Value_470 = Value_470;
        this.Type_2437 = Orientation3D_42_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Orientation3D_42_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Orientation3D_42_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Orientation3D_42_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Orientation3D_42_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Orientation3D_42_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Orientation3D_42_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Orientation3D_42_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Orientation3D_42_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Orientation3D_42_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Value_470 = function(self) { return self.Value_470; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Orientation3D_42_Type);
    static Implements = [Value_23_Concept];
}
class Pose2D_43_Type
{
    constructor(Position_477, Orientation_484)
    {
        // field initialization 
        this.Position_477 = Position_477;
        this.Orientation_484 = Orientation_484;
        this.Type_2437 = Pose2D_43_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Pose2D_43_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Pose2D_43_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Pose2D_43_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Pose2D_43_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Pose2D_43_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Pose2D_43_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Pose2D_43_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Pose2D_43_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Pose2D_43_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Position_477 = function(self) { return self.Position_477; }
    static Orientation_484 = function(self) { return self.Orientation_484; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Pose2D_43_Type);
    static Implements = [Value_23_Concept];
}
class Pose3D_44_Type
{
    constructor(Position_491, Orientation_498)
    {
        // field initialization 
        this.Position_491 = Position_491;
        this.Orientation_498 = Orientation_498;
        this.Type_2437 = Pose3D_44_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Pose3D_44_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Pose3D_44_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Pose3D_44_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Pose3D_44_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Pose3D_44_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Pose3D_44_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Pose3D_44_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Pose3D_44_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Pose3D_44_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Position_491 = function(self) { return self.Position_491; }
    static Orientation_498 = function(self) { return self.Orientation_498; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Pose3D_44_Type);
    static Implements = [Value_23_Concept];
}
class Transform3D_45_Type
{
    constructor(Translation_505, Rotation_512, Scale_519)
    {
        // field initialization 
        this.Translation_505 = Translation_505;
        this.Rotation_512 = Rotation_512;
        this.Scale_519 = Scale_519;
        this.Type_2437 = Transform3D_45_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Transform3D_45_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Transform3D_45_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Transform3D_45_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Transform3D_45_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Transform3D_45_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Transform3D_45_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Transform3D_45_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Transform3D_45_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Transform3D_45_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Translation_505 = function(self) { return self.Translation_505; }
    static Rotation_512 = function(self) { return self.Rotation_512; }
    static Scale_519 = function(self) { return self.Scale_519; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Transform3D_45_Type);
    static Implements = [Value_23_Concept];
}
class Transform2D_46_Type
{
    constructor(Translation_526, Rotation_533, Scale_540)
    {
        // field initialization 
        this.Translation_526 = Translation_526;
        this.Rotation_533 = Rotation_533;
        this.Scale_540 = Scale_540;
        this.Type_2437 = Transform2D_46_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Transform2D_46_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Transform2D_46_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Transform2D_46_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Transform2D_46_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Transform2D_46_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Transform2D_46_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Transform2D_46_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Transform2D_46_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Transform2D_46_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Translation_526 = function(self) { return self.Translation_526; }
    static Rotation_533 = function(self) { return self.Rotation_533; }
    static Scale_540 = function(self) { return self.Scale_540; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Transform2D_46_Type);
    static Implements = [Value_23_Concept];
}
class AlignedBox2D_47_Type
{
    constructor(A_547, B_554)
    {
        // field initialization 
        this.A_547 = A_547;
        this.B_554 = B_554;
        this.Count_2490 = AlignedBox2D_47_Type.Array_25_Concept.Count_2490;
        this.At_2495 = AlignedBox2D_47_Type.Array_25_Concept.At_2495;
        this.Type_2437 = AlignedBox2D_47_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = AlignedBox2D_47_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = AlignedBox2D_47_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = AlignedBox2D_47_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = AlignedBox2D_47_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = AlignedBox2D_47_Type.Value_23_Concept.One_2455;
        this.Default_2460 = AlignedBox2D_47_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = AlignedBox2D_47_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = AlignedBox2D_47_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = AlignedBox2D_47_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = AlignedBox2D_47_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = AlignedBox2D_47_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = AlignedBox2D_47_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Count_2133 = AlignedBox2D_47_Type.Vector_14_Concept.Count_2133;
        this.At_2147 = AlignedBox2D_47_Type.Vector_14_Concept.At_2147;
        this.Min_2483 = AlignedBox2D_47_Type.Interval_24_Concept.Min_2483;
        this.Max_2486 = AlignedBox2D_47_Type.Interval_24_Concept.Max_2486;
    }
    // field accessors
    static A_547 = function(self) { return self.A_547; }
    static B_554 = function(self) { return self.B_554; }
    // implemented concepts 
    static Array_25_Concept = new Array_25_Concept(AlignedBox2D_47_Type);
    static Value_23_Concept = new Value_23_Concept(AlignedBox2D_47_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(AlignedBox2D_47_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(AlignedBox2D_47_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(AlignedBox2D_47_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(AlignedBox2D_47_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(AlignedBox2D_47_Type);
    static Vector_14_Concept = new Vector_14_Concept(AlignedBox2D_47_Type);
    static Interval_24_Concept = new Interval_24_Concept(AlignedBox2D_47_Type);
    static Implements = [Array_25_Concept,Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept,Vector_14_Concept,Interval_24_Concept];
}
class AlignedBox3D_48_Type
{
    constructor(A_561, B_568)
    {
        // field initialization 
        this.A_561 = A_561;
        this.B_568 = B_568;
        this.Count_2490 = AlignedBox3D_48_Type.Array_25_Concept.Count_2490;
        this.At_2495 = AlignedBox3D_48_Type.Array_25_Concept.At_2495;
        this.Type_2437 = AlignedBox3D_48_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = AlignedBox3D_48_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = AlignedBox3D_48_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = AlignedBox3D_48_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = AlignedBox3D_48_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = AlignedBox3D_48_Type.Value_23_Concept.One_2455;
        this.Default_2460 = AlignedBox3D_48_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = AlignedBox3D_48_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = AlignedBox3D_48_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = AlignedBox3D_48_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = AlignedBox3D_48_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = AlignedBox3D_48_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = AlignedBox3D_48_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Count_2133 = AlignedBox3D_48_Type.Vector_14_Concept.Count_2133;
        this.At_2147 = AlignedBox3D_48_Type.Vector_14_Concept.At_2147;
        this.Min_2483 = AlignedBox3D_48_Type.Interval_24_Concept.Min_2483;
        this.Max_2486 = AlignedBox3D_48_Type.Interval_24_Concept.Max_2486;
    }
    // field accessors
    static A_561 = function(self) { return self.A_561; }
    static B_568 = function(self) { return self.B_568; }
    // implemented concepts 
    static Array_25_Concept = new Array_25_Concept(AlignedBox3D_48_Type);
    static Value_23_Concept = new Value_23_Concept(AlignedBox3D_48_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(AlignedBox3D_48_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(AlignedBox3D_48_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(AlignedBox3D_48_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(AlignedBox3D_48_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(AlignedBox3D_48_Type);
    static Vector_14_Concept = new Vector_14_Concept(AlignedBox3D_48_Type);
    static Interval_24_Concept = new Interval_24_Concept(AlignedBox3D_48_Type);
    static Implements = [Array_25_Concept,Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept,Vector_14_Concept,Interval_24_Concept];
}
class Complex_49_Type
{
    constructor(Real_575, Imaginary_582)
    {
        // field initialization 
        this.Real_575 = Real_575;
        this.Imaginary_582 = Imaginary_582;
        this.Count_2490 = Complex_49_Type.Array_25_Concept.Count_2490;
        this.At_2495 = Complex_49_Type.Array_25_Concept.At_2495;
        this.Type_2437 = Complex_49_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Complex_49_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Complex_49_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Complex_49_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Complex_49_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Complex_49_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Complex_49_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Complex_49_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Complex_49_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Complex_49_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = Complex_49_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = Complex_49_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = Complex_49_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = Complex_49_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = Complex_49_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = Complex_49_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = Complex_49_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Complex_49_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Complex_49_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Count_2133 = Complex_49_Type.Vector_14_Concept.Count_2133;
        this.At_2147 = Complex_49_Type.Vector_14_Concept.At_2147;
    }
    // field accessors
    static Real_575 = function(self) { return self.Real_575; }
    static Imaginary_582 = function(self) { return self.Imaginary_582; }
    // implemented concepts 
    static Array_25_Concept = new Array_25_Concept(Complex_49_Type);
    static Value_23_Concept = new Value_23_Concept(Complex_49_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(Complex_49_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Complex_49_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Complex_49_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Complex_49_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(Complex_49_Type);
    static Vector_14_Concept = new Vector_14_Concept(Complex_49_Type);
    static Implements = [Array_25_Concept,Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept,Vector_14_Concept];
}
class Ray3D_50_Type
{
    constructor(Direction_589, Position_596)
    {
        // field initialization 
        this.Direction_589 = Direction_589;
        this.Position_596 = Position_596;
        this.Type_2437 = Ray3D_50_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Ray3D_50_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Ray3D_50_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Ray3D_50_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Ray3D_50_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Ray3D_50_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Ray3D_50_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Ray3D_50_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Ray3D_50_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Ray3D_50_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Direction_589 = function(self) { return self.Direction_589; }
    static Position_596 = function(self) { return self.Position_596; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Ray3D_50_Type);
    static Implements = [Value_23_Concept];
}
class Ray2D_51_Type
{
    constructor(Direction_603, Position_610)
    {
        // field initialization 
        this.Direction_603 = Direction_603;
        this.Position_610 = Position_610;
        this.Type_2437 = Ray2D_51_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Ray2D_51_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Ray2D_51_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Ray2D_51_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Ray2D_51_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Ray2D_51_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Ray2D_51_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Ray2D_51_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Ray2D_51_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Ray2D_51_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Direction_603 = function(self) { return self.Direction_603; }
    static Position_610 = function(self) { return self.Position_610; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Ray2D_51_Type);
    static Implements = [Value_23_Concept];
}
class Sphere_52_Type
{
    constructor(Center_617, Radius_624)
    {
        // field initialization 
        this.Center_617 = Center_617;
        this.Radius_624 = Radius_624;
        this.Type_2437 = Sphere_52_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Sphere_52_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Sphere_52_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Sphere_52_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Sphere_52_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Sphere_52_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Sphere_52_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Sphere_52_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Sphere_52_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Sphere_52_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Center_617 = function(self) { return self.Center_617; }
    static Radius_624 = function(self) { return self.Radius_624; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Sphere_52_Type);
    static Implements = [Value_23_Concept];
}
class Plane_53_Type
{
    constructor(Normal_631, D_638)
    {
        // field initialization 
        this.Normal_631 = Normal_631;
        this.D_638 = D_638;
        this.Type_2437 = Plane_53_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Plane_53_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Plane_53_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Plane_53_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Plane_53_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Plane_53_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Plane_53_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Plane_53_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Plane_53_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Plane_53_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Normal_631 = function(self) { return self.Normal_631; }
    static D_638 = function(self) { return self.D_638; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Plane_53_Type);
    static Implements = [Value_23_Concept];
}
class Triangle3D_54_Type
{
    constructor(A_645, B_652, C_659)
    {
        // field initialization 
        this.A_645 = A_645;
        this.B_652 = B_652;
        this.C_659 = C_659;
        this.Type_2437 = Triangle3D_54_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Triangle3D_54_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Triangle3D_54_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Triangle3D_54_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Triangle3D_54_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Triangle3D_54_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Triangle3D_54_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Triangle3D_54_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Triangle3D_54_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Triangle3D_54_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static A_645 = function(self) { return self.A_645; }
    static B_652 = function(self) { return self.B_652; }
    static C_659 = function(self) { return self.C_659; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Triangle3D_54_Type);
    static Implements = [Value_23_Concept];
}
class Triangle2D_55_Type
{
    constructor(A_666, B_673, C_680)
    {
        // field initialization 
        this.A_666 = A_666;
        this.B_673 = B_673;
        this.C_680 = C_680;
        this.Type_2437 = Triangle2D_55_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Triangle2D_55_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Triangle2D_55_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Triangle2D_55_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Triangle2D_55_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Triangle2D_55_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Triangle2D_55_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Triangle2D_55_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Triangle2D_55_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Triangle2D_55_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static A_666 = function(self) { return self.A_666; }
    static B_673 = function(self) { return self.B_673; }
    static C_680 = function(self) { return self.C_680; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Triangle2D_55_Type);
    static Implements = [Value_23_Concept];
}
class Quad3D_56_Type
{
    constructor(A_687, B_694, C_701, D_708)
    {
        // field initialization 
        this.A_687 = A_687;
        this.B_694 = B_694;
        this.C_701 = C_701;
        this.D_708 = D_708;
        this.Type_2437 = Quad3D_56_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Quad3D_56_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Quad3D_56_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Quad3D_56_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Quad3D_56_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Quad3D_56_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Quad3D_56_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Quad3D_56_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Quad3D_56_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Quad3D_56_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static A_687 = function(self) { return self.A_687; }
    static B_694 = function(self) { return self.B_694; }
    static C_701 = function(self) { return self.C_701; }
    static D_708 = function(self) { return self.D_708; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Quad3D_56_Type);
    static Implements = [Value_23_Concept];
}
class Quad2D_57_Type
{
    constructor(A_715, B_722, C_729, D_736)
    {
        // field initialization 
        this.A_715 = A_715;
        this.B_722 = B_722;
        this.C_729 = C_729;
        this.D_736 = D_736;
        this.Type_2437 = Quad2D_57_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Quad2D_57_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Quad2D_57_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Quad2D_57_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Quad2D_57_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Quad2D_57_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Quad2D_57_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Quad2D_57_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Quad2D_57_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Quad2D_57_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static A_715 = function(self) { return self.A_715; }
    static B_722 = function(self) { return self.B_722; }
    static C_729 = function(self) { return self.C_729; }
    static D_736 = function(self) { return self.D_736; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Quad2D_57_Type);
    static Implements = [Value_23_Concept];
}
class Point3D_58_Type
{
    constructor(Value_743)
    {
        // field initialization 
        this.Value_743 = Value_743;
        this.Type_2437 = Point3D_58_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Point3D_58_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Point3D_58_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Point3D_58_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Point3D_58_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Point3D_58_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Point3D_58_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Point3D_58_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Point3D_58_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Point3D_58_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Value_743 = function(self) { return self.Value_743; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Point3D_58_Type);
    static Implements = [Value_23_Concept];
}
class Point2D_59_Type
{
    constructor(Value_750)
    {
        // field initialization 
        this.Value_750 = Value_750;
        this.Type_2437 = Point2D_59_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Point2D_59_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Point2D_59_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Point2D_59_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Point2D_59_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Point2D_59_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Point2D_59_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Point2D_59_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Point2D_59_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Point2D_59_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Value_750 = function(self) { return self.Value_750; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Point2D_59_Type);
    static Implements = [Value_23_Concept];
}
class Line3D_60_Type
{
    constructor(A_757, B_764)
    {
        // field initialization 
        this.A_757 = A_757;
        this.B_764 = B_764;
        this.Count_2490 = Line3D_60_Type.Array_25_Concept.Count_2490;
        this.At_2495 = Line3D_60_Type.Array_25_Concept.At_2495;
        this.Type_2437 = Line3D_60_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Line3D_60_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Line3D_60_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Line3D_60_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Line3D_60_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Line3D_60_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Line3D_60_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Line3D_60_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Line3D_60_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Line3D_60_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = Line3D_60_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = Line3D_60_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = Line3D_60_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = Line3D_60_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = Line3D_60_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = Line3D_60_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = Line3D_60_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Line3D_60_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Line3D_60_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Count_2133 = Line3D_60_Type.Vector_14_Concept.Count_2133;
        this.At_2147 = Line3D_60_Type.Vector_14_Concept.At_2147;
        this.Min_2483 = Line3D_60_Type.Interval_24_Concept.Min_2483;
        this.Max_2486 = Line3D_60_Type.Interval_24_Concept.Max_2486;
    }
    // field accessors
    static A_757 = function(self) { return self.A_757; }
    static B_764 = function(self) { return self.B_764; }
    // implemented concepts 
    static Array_25_Concept = new Array_25_Concept(Line3D_60_Type);
    static Value_23_Concept = new Value_23_Concept(Line3D_60_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(Line3D_60_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Line3D_60_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Line3D_60_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Line3D_60_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(Line3D_60_Type);
    static Vector_14_Concept = new Vector_14_Concept(Line3D_60_Type);
    static Interval_24_Concept = new Interval_24_Concept(Line3D_60_Type);
    static Implements = [Array_25_Concept,Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept,Vector_14_Concept,Interval_24_Concept];
}
class Line2D_61_Type
{
    constructor(A_771, B_778)
    {
        // field initialization 
        this.A_771 = A_771;
        this.B_778 = B_778;
        this.Count_2490 = Line2D_61_Type.Array_25_Concept.Count_2490;
        this.At_2495 = Line2D_61_Type.Array_25_Concept.At_2495;
        this.Type_2437 = Line2D_61_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Line2D_61_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Line2D_61_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Line2D_61_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Line2D_61_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Line2D_61_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Line2D_61_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Line2D_61_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Line2D_61_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Line2D_61_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = Line2D_61_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = Line2D_61_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = Line2D_61_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = Line2D_61_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = Line2D_61_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = Line2D_61_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = Line2D_61_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Line2D_61_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Line2D_61_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Count_2133 = Line2D_61_Type.Vector_14_Concept.Count_2133;
        this.At_2147 = Line2D_61_Type.Vector_14_Concept.At_2147;
        this.Min_2483 = Line2D_61_Type.Interval_24_Concept.Min_2483;
        this.Max_2486 = Line2D_61_Type.Interval_24_Concept.Max_2486;
    }
    // field accessors
    static A_771 = function(self) { return self.A_771; }
    static B_778 = function(self) { return self.B_778; }
    // implemented concepts 
    static Array_25_Concept = new Array_25_Concept(Line2D_61_Type);
    static Value_23_Concept = new Value_23_Concept(Line2D_61_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(Line2D_61_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Line2D_61_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Line2D_61_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Line2D_61_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(Line2D_61_Type);
    static Vector_14_Concept = new Vector_14_Concept(Line2D_61_Type);
    static Interval_24_Concept = new Interval_24_Concept(Line2D_61_Type);
    static Implements = [Array_25_Concept,Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept,Vector_14_Concept,Interval_24_Concept];
}
class Color_62_Type
{
    constructor(R_785, G_792, B_799, A_806)
    {
        // field initialization 
        this.R_785 = R_785;
        this.G_792 = G_792;
        this.B_799 = B_799;
        this.A_806 = A_806;
        this.Type_2437 = Color_62_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Color_62_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Color_62_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Color_62_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Color_62_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Color_62_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Color_62_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Color_62_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Color_62_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Color_62_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static R_785 = function(self) { return self.R_785; }
    static G_792 = function(self) { return self.G_792; }
    static B_799 = function(self) { return self.B_799; }
    static A_806 = function(self) { return self.A_806; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Color_62_Type);
    static Implements = [Value_23_Concept];
}
class ColorLUV_63_Type
{
    constructor(Lightness_813, U_820, V_827)
    {
        // field initialization 
        this.Lightness_813 = Lightness_813;
        this.U_820 = U_820;
        this.V_827 = V_827;
        this.Type_2437 = ColorLUV_63_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = ColorLUV_63_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = ColorLUV_63_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = ColorLUV_63_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = ColorLUV_63_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = ColorLUV_63_Type.Value_23_Concept.One_2455;
        this.Default_2460 = ColorLUV_63_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = ColorLUV_63_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = ColorLUV_63_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = ColorLUV_63_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Lightness_813 = function(self) { return self.Lightness_813; }
    static U_820 = function(self) { return self.U_820; }
    static V_827 = function(self) { return self.V_827; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorLUV_63_Type);
    static Implements = [Value_23_Concept];
}
class ColorLAB_64_Type
{
    constructor(Lightness_834, A_841, B_848)
    {
        // field initialization 
        this.Lightness_834 = Lightness_834;
        this.A_841 = A_841;
        this.B_848 = B_848;
        this.Type_2437 = ColorLAB_64_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = ColorLAB_64_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = ColorLAB_64_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = ColorLAB_64_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = ColorLAB_64_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = ColorLAB_64_Type.Value_23_Concept.One_2455;
        this.Default_2460 = ColorLAB_64_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = ColorLAB_64_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = ColorLAB_64_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = ColorLAB_64_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Lightness_834 = function(self) { return self.Lightness_834; }
    static A_841 = function(self) { return self.A_841; }
    static B_848 = function(self) { return self.B_848; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorLAB_64_Type);
    static Implements = [Value_23_Concept];
}
class ColorLCh_65_Type
{
    constructor(Lightness_855, ChromaHue_862)
    {
        // field initialization 
        this.Lightness_855 = Lightness_855;
        this.ChromaHue_862 = ChromaHue_862;
        this.Type_2437 = ColorLCh_65_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = ColorLCh_65_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = ColorLCh_65_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = ColorLCh_65_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = ColorLCh_65_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = ColorLCh_65_Type.Value_23_Concept.One_2455;
        this.Default_2460 = ColorLCh_65_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = ColorLCh_65_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = ColorLCh_65_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = ColorLCh_65_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Lightness_855 = function(self) { return self.Lightness_855; }
    static ChromaHue_862 = function(self) { return self.ChromaHue_862; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorLCh_65_Type);
    static Implements = [Value_23_Concept];
}
class ColorHSV_66_Type
{
    constructor(Hue_869, S_876, V_883)
    {
        // field initialization 
        this.Hue_869 = Hue_869;
        this.S_876 = S_876;
        this.V_883 = V_883;
        this.Type_2437 = ColorHSV_66_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = ColorHSV_66_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = ColorHSV_66_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = ColorHSV_66_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = ColorHSV_66_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = ColorHSV_66_Type.Value_23_Concept.One_2455;
        this.Default_2460 = ColorHSV_66_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = ColorHSV_66_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = ColorHSV_66_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = ColorHSV_66_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Hue_869 = function(self) { return self.Hue_869; }
    static S_876 = function(self) { return self.S_876; }
    static V_883 = function(self) { return self.V_883; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorHSV_66_Type);
    static Implements = [Value_23_Concept];
}
class ColorHSL_67_Type
{
    constructor(Hue_890, Saturation_897, Luminance_904)
    {
        // field initialization 
        this.Hue_890 = Hue_890;
        this.Saturation_897 = Saturation_897;
        this.Luminance_904 = Luminance_904;
        this.Type_2437 = ColorHSL_67_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = ColorHSL_67_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = ColorHSL_67_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = ColorHSL_67_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = ColorHSL_67_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = ColorHSL_67_Type.Value_23_Concept.One_2455;
        this.Default_2460 = ColorHSL_67_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = ColorHSL_67_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = ColorHSL_67_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = ColorHSL_67_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Hue_890 = function(self) { return self.Hue_890; }
    static Saturation_897 = function(self) { return self.Saturation_897; }
    static Luminance_904 = function(self) { return self.Luminance_904; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorHSL_67_Type);
    static Implements = [Value_23_Concept];
}
class ColorYCbCr_68_Type
{
    constructor(Y_911, Cb_918, Cr_925)
    {
        // field initialization 
        this.Y_911 = Y_911;
        this.Cb_918 = Cb_918;
        this.Cr_925 = Cr_925;
        this.Type_2437 = ColorYCbCr_68_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = ColorYCbCr_68_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = ColorYCbCr_68_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = ColorYCbCr_68_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = ColorYCbCr_68_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = ColorYCbCr_68_Type.Value_23_Concept.One_2455;
        this.Default_2460 = ColorYCbCr_68_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = ColorYCbCr_68_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = ColorYCbCr_68_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = ColorYCbCr_68_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Y_911 = function(self) { return self.Y_911; }
    static Cb_918 = function(self) { return self.Cb_918; }
    static Cr_925 = function(self) { return self.Cr_925; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorYCbCr_68_Type);
    static Implements = [Value_23_Concept];
}
class SphericalCoordinate_69_Type
{
    constructor(Radius_932, Azimuth_939, Polar_946)
    {
        // field initialization 
        this.Radius_932 = Radius_932;
        this.Azimuth_939 = Azimuth_939;
        this.Polar_946 = Polar_946;
        this.Type_2437 = SphericalCoordinate_69_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = SphericalCoordinate_69_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = SphericalCoordinate_69_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = SphericalCoordinate_69_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = SphericalCoordinate_69_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = SphericalCoordinate_69_Type.Value_23_Concept.One_2455;
        this.Default_2460 = SphericalCoordinate_69_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = SphericalCoordinate_69_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = SphericalCoordinate_69_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = SphericalCoordinate_69_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Radius_932 = function(self) { return self.Radius_932; }
    static Azimuth_939 = function(self) { return self.Azimuth_939; }
    static Polar_946 = function(self) { return self.Polar_946; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(SphericalCoordinate_69_Type);
    static Implements = [Value_23_Concept];
}
class PolarCoordinate_70_Type
{
    constructor(Radius_953, Angle_960)
    {
        // field initialization 
        this.Radius_953 = Radius_953;
        this.Angle_960 = Angle_960;
        this.Type_2437 = PolarCoordinate_70_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = PolarCoordinate_70_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = PolarCoordinate_70_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = PolarCoordinate_70_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = PolarCoordinate_70_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = PolarCoordinate_70_Type.Value_23_Concept.One_2455;
        this.Default_2460 = PolarCoordinate_70_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = PolarCoordinate_70_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = PolarCoordinate_70_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = PolarCoordinate_70_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Radius_953 = function(self) { return self.Radius_953; }
    static Angle_960 = function(self) { return self.Angle_960; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(PolarCoordinate_70_Type);
    static Implements = [Value_23_Concept];
}
class LogPolarCoordinate_71_Type
{
    constructor(Rho_967, Azimuth_974)
    {
        // field initialization 
        this.Rho_967 = Rho_967;
        this.Azimuth_974 = Azimuth_974;
        this.Type_2437 = LogPolarCoordinate_71_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = LogPolarCoordinate_71_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = LogPolarCoordinate_71_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = LogPolarCoordinate_71_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = LogPolarCoordinate_71_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = LogPolarCoordinate_71_Type.Value_23_Concept.One_2455;
        this.Default_2460 = LogPolarCoordinate_71_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = LogPolarCoordinate_71_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = LogPolarCoordinate_71_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = LogPolarCoordinate_71_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Rho_967 = function(self) { return self.Rho_967; }
    static Azimuth_974 = function(self) { return self.Azimuth_974; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(LogPolarCoordinate_71_Type);
    static Implements = [Value_23_Concept];
}
class CylindricalCoordinate_72_Type
{
    constructor(RadialDistance_981, Azimuth_988, Height_995)
    {
        // field initialization 
        this.RadialDistance_981 = RadialDistance_981;
        this.Azimuth_988 = Azimuth_988;
        this.Height_995 = Height_995;
        this.Type_2437 = CylindricalCoordinate_72_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = CylindricalCoordinate_72_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = CylindricalCoordinate_72_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = CylindricalCoordinate_72_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = CylindricalCoordinate_72_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = CylindricalCoordinate_72_Type.Value_23_Concept.One_2455;
        this.Default_2460 = CylindricalCoordinate_72_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = CylindricalCoordinate_72_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = CylindricalCoordinate_72_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = CylindricalCoordinate_72_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static RadialDistance_981 = function(self) { return self.RadialDistance_981; }
    static Azimuth_988 = function(self) { return self.Azimuth_988; }
    static Height_995 = function(self) { return self.Height_995; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(CylindricalCoordinate_72_Type);
    static Implements = [Value_23_Concept];
}
class HorizontalCoordinate_73_Type
{
    constructor(Radius_1002, Azimuth_1009, Height_1016)
    {
        // field initialization 
        this.Radius_1002 = Radius_1002;
        this.Azimuth_1009 = Azimuth_1009;
        this.Height_1016 = Height_1016;
        this.Type_2437 = HorizontalCoordinate_73_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = HorizontalCoordinate_73_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = HorizontalCoordinate_73_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = HorizontalCoordinate_73_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = HorizontalCoordinate_73_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = HorizontalCoordinate_73_Type.Value_23_Concept.One_2455;
        this.Default_2460 = HorizontalCoordinate_73_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = HorizontalCoordinate_73_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = HorizontalCoordinate_73_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = HorizontalCoordinate_73_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Radius_1002 = function(self) { return self.Radius_1002; }
    static Azimuth_1009 = function(self) { return self.Azimuth_1009; }
    static Height_1016 = function(self) { return self.Height_1016; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(HorizontalCoordinate_73_Type);
    static Implements = [Value_23_Concept];
}
class GeoCoordinate_74_Type
{
    constructor(Latitude_1023, Longitude_1030)
    {
        // field initialization 
        this.Latitude_1023 = Latitude_1023;
        this.Longitude_1030 = Longitude_1030;
        this.Type_2437 = GeoCoordinate_74_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = GeoCoordinate_74_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = GeoCoordinate_74_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = GeoCoordinate_74_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = GeoCoordinate_74_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = GeoCoordinate_74_Type.Value_23_Concept.One_2455;
        this.Default_2460 = GeoCoordinate_74_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = GeoCoordinate_74_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = GeoCoordinate_74_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = GeoCoordinate_74_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Latitude_1023 = function(self) { return self.Latitude_1023; }
    static Longitude_1030 = function(self) { return self.Longitude_1030; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(GeoCoordinate_74_Type);
    static Implements = [Value_23_Concept];
}
class GeoCoordinateWithAltitude_75_Type
{
    constructor(Coordinate_1037, Altitude_1044)
    {
        // field initialization 
        this.Coordinate_1037 = Coordinate_1037;
        this.Altitude_1044 = Altitude_1044;
        this.Type_2437 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.One_2455;
        this.Default_2460 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Coordinate_1037 = function(self) { return self.Coordinate_1037; }
    static Altitude_1044 = function(self) { return self.Altitude_1044; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(GeoCoordinateWithAltitude_75_Type);
    static Implements = [Value_23_Concept];
}
class Circle_76_Type
{
    constructor(Center_1051, Radius_1058)
    {
        // field initialization 
        this.Center_1051 = Center_1051;
        this.Radius_1058 = Radius_1058;
        this.Type_2437 = Circle_76_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Circle_76_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Circle_76_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Circle_76_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Circle_76_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Circle_76_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Circle_76_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Circle_76_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Circle_76_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Circle_76_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Center_1051 = function(self) { return self.Center_1051; }
    static Radius_1058 = function(self) { return self.Radius_1058; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Circle_76_Type);
    static Implements = [Value_23_Concept];
}
class Chord_77_Type
{
    constructor(Circle_1065, Arc_1072)
    {
        // field initialization 
        this.Circle_1065 = Circle_1065;
        this.Arc_1072 = Arc_1072;
        this.Type_2437 = Chord_77_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Chord_77_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Chord_77_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Chord_77_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Chord_77_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Chord_77_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Chord_77_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Chord_77_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Chord_77_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Chord_77_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Circle_1065 = function(self) { return self.Circle_1065; }
    static Arc_1072 = function(self) { return self.Arc_1072; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Chord_77_Type);
    static Implements = [Value_23_Concept];
}
class Size2D_78_Type
{
    constructor(Width_1079, Height_1086)
    {
        // field initialization 
        this.Width_1079 = Width_1079;
        this.Height_1086 = Height_1086;
        this.Type_2437 = Size2D_78_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Size2D_78_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Size2D_78_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Size2D_78_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Size2D_78_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Size2D_78_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Size2D_78_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Size2D_78_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Size2D_78_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Size2D_78_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Width_1079 = function(self) { return self.Width_1079; }
    static Height_1086 = function(self) { return self.Height_1086; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Size2D_78_Type);
    static Implements = [Value_23_Concept];
}
class Size3D_79_Type
{
    constructor(Width_1093, Height_1100, Depth_1107)
    {
        // field initialization 
        this.Width_1093 = Width_1093;
        this.Height_1100 = Height_1100;
        this.Depth_1107 = Depth_1107;
        this.Type_2437 = Size3D_79_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Size3D_79_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Size3D_79_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Size3D_79_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Size3D_79_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Size3D_79_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Size3D_79_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Size3D_79_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Size3D_79_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Size3D_79_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Width_1093 = function(self) { return self.Width_1093; }
    static Height_1100 = function(self) { return self.Height_1100; }
    static Depth_1107 = function(self) { return self.Depth_1107; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Size3D_79_Type);
    static Implements = [Value_23_Concept];
}
class Rectangle2D_80_Type
{
    constructor(Center_1114, Size_1121)
    {
        // field initialization 
        this.Center_1114 = Center_1114;
        this.Size_1121 = Size_1121;
        this.Type_2437 = Rectangle2D_80_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Rectangle2D_80_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Rectangle2D_80_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Rectangle2D_80_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Rectangle2D_80_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Rectangle2D_80_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Rectangle2D_80_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Rectangle2D_80_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Rectangle2D_80_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Rectangle2D_80_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Center_1114 = function(self) { return self.Center_1114; }
    static Size_1121 = function(self) { return self.Size_1121; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Rectangle2D_80_Type);
    static Implements = [Value_23_Concept];
}
class Proportion_81_Type
{
    constructor(Value_1128)
    {
        // field initialization 
        this.Value_1128 = Value_1128;
        this.Type_2437 = Proportion_81_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Proportion_81_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Proportion_81_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Proportion_81_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Proportion_81_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Proportion_81_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Proportion_81_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Proportion_81_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Proportion_81_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Proportion_81_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = Proportion_81_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = Proportion_81_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = Proportion_81_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = Proportion_81_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = Proportion_81_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = Proportion_81_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = Proportion_81_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Proportion_81_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Proportion_81_Type.Magnitude_17_Concept.Magnitude_2175;
    }
    // field accessors
    static Value_1128 = function(self) { return self.Value_1128; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Proportion_81_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(Proportion_81_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Proportion_81_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Proportion_81_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Proportion_81_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(Proportion_81_Type);
    static Implements = [Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept];
}
class Fraction_82_Type
{
    constructor(Numerator_1135, Denominator_1142)
    {
        // field initialization 
        this.Numerator_1135 = Numerator_1135;
        this.Denominator_1142 = Denominator_1142;
        this.Type_2437 = Fraction_82_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Fraction_82_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Fraction_82_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Fraction_82_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Fraction_82_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Fraction_82_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Fraction_82_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Fraction_82_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Fraction_82_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Fraction_82_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Numerator_1135 = function(self) { return self.Numerator_1135; }
    static Denominator_1142 = function(self) { return self.Denominator_1142; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Fraction_82_Type);
    static Implements = [Value_23_Concept];
}
class Angle_83_Type
{
    constructor(Radians_1149)
    {
        // field initialization 
        this.Radians_1149 = Radians_1149;
        this.Type_2437 = Angle_83_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Angle_83_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Angle_83_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Angle_83_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Angle_83_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Angle_83_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Angle_83_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Angle_83_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Angle_83_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Angle_83_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = Angle_83_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = Angle_83_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = Angle_83_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = Angle_83_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = Angle_83_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = Angle_83_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Angle_83_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Angle_83_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = Angle_83_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Radians_1149 = function(self) { return self.Radians_1149; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Angle_83_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(Angle_83_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Angle_83_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Angle_83_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Angle_83_Type);
    static Measure_15_Concept = new Measure_15_Concept(Angle_83_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class Length_84_Type
{
    constructor(Meters_1156)
    {
        // field initialization 
        this.Meters_1156 = Meters_1156;
        this.Type_2437 = Length_84_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Length_84_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Length_84_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Length_84_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Length_84_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Length_84_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Length_84_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Length_84_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Length_84_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Length_84_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = Length_84_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = Length_84_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = Length_84_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = Length_84_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = Length_84_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = Length_84_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Length_84_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Length_84_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = Length_84_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Meters_1156 = function(self) { return self.Meters_1156; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Length_84_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(Length_84_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Length_84_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Length_84_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Length_84_Type);
    static Measure_15_Concept = new Measure_15_Concept(Length_84_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class Mass_85_Type
{
    constructor(Kilograms_1163)
    {
        // field initialization 
        this.Kilograms_1163 = Kilograms_1163;
        this.Type_2437 = Mass_85_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Mass_85_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Mass_85_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Mass_85_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Mass_85_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Mass_85_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Mass_85_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Mass_85_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Mass_85_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Mass_85_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = Mass_85_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = Mass_85_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = Mass_85_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = Mass_85_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = Mass_85_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = Mass_85_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Mass_85_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Mass_85_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = Mass_85_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Kilograms_1163 = function(self) { return self.Kilograms_1163; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Mass_85_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(Mass_85_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Mass_85_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Mass_85_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Mass_85_Type);
    static Measure_15_Concept = new Measure_15_Concept(Mass_85_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class Temperature_86_Type
{
    constructor(Celsius_1170)
    {
        // field initialization 
        this.Celsius_1170 = Celsius_1170;
        this.Type_2437 = Temperature_86_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Temperature_86_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Temperature_86_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Temperature_86_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Temperature_86_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Temperature_86_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Temperature_86_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Temperature_86_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Temperature_86_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Temperature_86_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = Temperature_86_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = Temperature_86_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = Temperature_86_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = Temperature_86_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = Temperature_86_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = Temperature_86_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Temperature_86_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Temperature_86_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = Temperature_86_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Celsius_1170 = function(self) { return self.Celsius_1170; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Temperature_86_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(Temperature_86_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Temperature_86_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Temperature_86_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Temperature_86_Type);
    static Measure_15_Concept = new Measure_15_Concept(Temperature_86_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class TimeSpan_87_Type
{
    constructor(Seconds_1177)
    {
        // field initialization 
        this.Seconds_1177 = Seconds_1177;
        this.Type_2437 = TimeSpan_87_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = TimeSpan_87_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = TimeSpan_87_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = TimeSpan_87_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = TimeSpan_87_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = TimeSpan_87_Type.Value_23_Concept.One_2455;
        this.Default_2460 = TimeSpan_87_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = TimeSpan_87_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = TimeSpan_87_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = TimeSpan_87_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = TimeSpan_87_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = TimeSpan_87_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = TimeSpan_87_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = TimeSpan_87_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = TimeSpan_87_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = TimeSpan_87_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = TimeSpan_87_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = TimeSpan_87_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = TimeSpan_87_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Seconds_1177 = function(self) { return self.Seconds_1177; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(TimeSpan_87_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(TimeSpan_87_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(TimeSpan_87_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(TimeSpan_87_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(TimeSpan_87_Type);
    static Measure_15_Concept = new Measure_15_Concept(TimeSpan_87_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class TimeRange_88_Type
{
    constructor(Min_1184, Max_1191)
    {
        // field initialization 
        this.Min_1184 = Min_1184;
        this.Max_1191 = Max_1191;
        this.Count_2490 = TimeRange_88_Type.Array_25_Concept.Count_2490;
        this.At_2495 = TimeRange_88_Type.Array_25_Concept.At_2495;
        this.Type_2437 = TimeRange_88_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = TimeRange_88_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = TimeRange_88_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = TimeRange_88_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = TimeRange_88_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = TimeRange_88_Type.Value_23_Concept.One_2455;
        this.Default_2460 = TimeRange_88_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = TimeRange_88_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = TimeRange_88_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = TimeRange_88_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = TimeRange_88_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = TimeRange_88_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = TimeRange_88_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = TimeRange_88_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = TimeRange_88_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = TimeRange_88_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = TimeRange_88_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = TimeRange_88_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = TimeRange_88_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Count_2133 = TimeRange_88_Type.Vector_14_Concept.Count_2133;
        this.At_2147 = TimeRange_88_Type.Vector_14_Concept.At_2147;
        this.Min_2483 = TimeRange_88_Type.Interval_24_Concept.Min_2483;
        this.Max_2486 = TimeRange_88_Type.Interval_24_Concept.Max_2486;
    }
    // field accessors
    static Min_1184 = function(self) { return self.Min_1184; }
    static Max_1191 = function(self) { return self.Max_1191; }
    // implemented concepts 
    static Array_25_Concept = new Array_25_Concept(TimeRange_88_Type);
    static Value_23_Concept = new Value_23_Concept(TimeRange_88_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(TimeRange_88_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(TimeRange_88_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(TimeRange_88_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(TimeRange_88_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(TimeRange_88_Type);
    static Vector_14_Concept = new Vector_14_Concept(TimeRange_88_Type);
    static Interval_24_Concept = new Interval_24_Concept(TimeRange_88_Type);
    static Implements = [Array_25_Concept,Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept,Vector_14_Concept,Interval_24_Concept];
}
class DateTime_89_Type
{
    constructor()
    {
        // field initialization 
        this.Type_2437 = DateTime_89_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = DateTime_89_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = DateTime_89_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = DateTime_89_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = DateTime_89_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = DateTime_89_Type.Value_23_Concept.One_2455;
        this.Default_2460 = DateTime_89_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = DateTime_89_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = DateTime_89_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = DateTime_89_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(DateTime_89_Type);
    static Implements = [Value_23_Concept];
}
class AnglePair_90_Type
{
    constructor(Start_1198, End_1205)
    {
        // field initialization 
        this.Start_1198 = Start_1198;
        this.End_1205 = End_1205;
        this.Count_2490 = AnglePair_90_Type.Array_25_Concept.Count_2490;
        this.At_2495 = AnglePair_90_Type.Array_25_Concept.At_2495;
        this.Type_2437 = AnglePair_90_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = AnglePair_90_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = AnglePair_90_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = AnglePair_90_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = AnglePair_90_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = AnglePair_90_Type.Value_23_Concept.One_2455;
        this.Default_2460 = AnglePair_90_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = AnglePair_90_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = AnglePair_90_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = AnglePair_90_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = AnglePair_90_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = AnglePair_90_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = AnglePair_90_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = AnglePair_90_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = AnglePair_90_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = AnglePair_90_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = AnglePair_90_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = AnglePair_90_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = AnglePair_90_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Count_2133 = AnglePair_90_Type.Vector_14_Concept.Count_2133;
        this.At_2147 = AnglePair_90_Type.Vector_14_Concept.At_2147;
        this.Min_2483 = AnglePair_90_Type.Interval_24_Concept.Min_2483;
        this.Max_2486 = AnglePair_90_Type.Interval_24_Concept.Max_2486;
    }
    // field accessors
    static Start_1198 = function(self) { return self.Start_1198; }
    static End_1205 = function(self) { return self.End_1205; }
    // implemented concepts 
    static Array_25_Concept = new Array_25_Concept(AnglePair_90_Type);
    static Value_23_Concept = new Value_23_Concept(AnglePair_90_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(AnglePair_90_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(AnglePair_90_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(AnglePair_90_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(AnglePair_90_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(AnglePair_90_Type);
    static Vector_14_Concept = new Vector_14_Concept(AnglePair_90_Type);
    static Interval_24_Concept = new Interval_24_Concept(AnglePair_90_Type);
    static Implements = [Array_25_Concept,Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept,Vector_14_Concept,Interval_24_Concept];
}
class Ring_91_Type
{
    constructor(Circle_1212, InnerRadius_1219)
    {
        // field initialization 
        this.Circle_1212 = Circle_1212;
        this.InnerRadius_1219 = InnerRadius_1219;
        this.Type_2437 = Ring_91_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Ring_91_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Ring_91_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Ring_91_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Ring_91_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Ring_91_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Ring_91_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Ring_91_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Ring_91_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Ring_91_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = Ring_91_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = Ring_91_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = Ring_91_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = Ring_91_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = Ring_91_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = Ring_91_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = Ring_91_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Ring_91_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Ring_91_Type.Magnitude_17_Concept.Magnitude_2175;
    }
    // field accessors
    static Circle_1212 = function(self) { return self.Circle_1212; }
    static InnerRadius_1219 = function(self) { return self.InnerRadius_1219; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Ring_91_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(Ring_91_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Ring_91_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Ring_91_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Ring_91_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(Ring_91_Type);
    static Implements = [Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept];
}
class Arc_92_Type
{
    constructor(Angles_1226, Cirlce_1233)
    {
        // field initialization 
        this.Angles_1226 = Angles_1226;
        this.Cirlce_1233 = Cirlce_1233;
        this.Type_2437 = Arc_92_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Arc_92_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Arc_92_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Arc_92_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Arc_92_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Arc_92_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Arc_92_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Arc_92_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Arc_92_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Arc_92_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Angles_1226 = function(self) { return self.Angles_1226; }
    static Cirlce_1233 = function(self) { return self.Cirlce_1233; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Arc_92_Type);
    static Implements = [Value_23_Concept];
}
class TimeInterval_93_Type
{
    constructor(Start_1240, End_1247)
    {
        // field initialization 
        this.Start_1240 = Start_1240;
        this.End_1247 = End_1247;
        this.Count_2490 = TimeInterval_93_Type.Array_25_Concept.Count_2490;
        this.At_2495 = TimeInterval_93_Type.Array_25_Concept.At_2495;
        this.Type_2437 = TimeInterval_93_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = TimeInterval_93_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = TimeInterval_93_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = TimeInterval_93_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = TimeInterval_93_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = TimeInterval_93_Type.Value_23_Concept.One_2455;
        this.Default_2460 = TimeInterval_93_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = TimeInterval_93_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = TimeInterval_93_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = TimeInterval_93_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = TimeInterval_93_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = TimeInterval_93_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = TimeInterval_93_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = TimeInterval_93_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = TimeInterval_93_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = TimeInterval_93_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = TimeInterval_93_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = TimeInterval_93_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = TimeInterval_93_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Count_2133 = TimeInterval_93_Type.Vector_14_Concept.Count_2133;
        this.At_2147 = TimeInterval_93_Type.Vector_14_Concept.At_2147;
        this.Min_2483 = TimeInterval_93_Type.Interval_24_Concept.Min_2483;
        this.Max_2486 = TimeInterval_93_Type.Interval_24_Concept.Max_2486;
    }
    // field accessors
    static Start_1240 = function(self) { return self.Start_1240; }
    static End_1247 = function(self) { return self.End_1247; }
    // implemented concepts 
    static Array_25_Concept = new Array_25_Concept(TimeInterval_93_Type);
    static Value_23_Concept = new Value_23_Concept(TimeInterval_93_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(TimeInterval_93_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(TimeInterval_93_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(TimeInterval_93_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(TimeInterval_93_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(TimeInterval_93_Type);
    static Vector_14_Concept = new Vector_14_Concept(TimeInterval_93_Type);
    static Interval_24_Concept = new Interval_24_Concept(TimeInterval_93_Type);
    static Implements = [Array_25_Concept,Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept,Vector_14_Concept,Interval_24_Concept];
}
class RealInterval_94_Type
{
    constructor(A_1254, B_1261)
    {
        // field initialization 
        this.A_1254 = A_1254;
        this.B_1261 = B_1261;
        this.Count_2490 = RealInterval_94_Type.Array_25_Concept.Count_2490;
        this.At_2495 = RealInterval_94_Type.Array_25_Concept.At_2495;
        this.Type_2437 = RealInterval_94_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = RealInterval_94_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = RealInterval_94_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = RealInterval_94_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = RealInterval_94_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = RealInterval_94_Type.Value_23_Concept.One_2455;
        this.Default_2460 = RealInterval_94_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = RealInterval_94_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = RealInterval_94_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = RealInterval_94_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = RealInterval_94_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = RealInterval_94_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = RealInterval_94_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = RealInterval_94_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = RealInterval_94_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = RealInterval_94_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = RealInterval_94_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = RealInterval_94_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = RealInterval_94_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Count_2133 = RealInterval_94_Type.Vector_14_Concept.Count_2133;
        this.At_2147 = RealInterval_94_Type.Vector_14_Concept.At_2147;
        this.Min_2483 = RealInterval_94_Type.Interval_24_Concept.Min_2483;
        this.Max_2486 = RealInterval_94_Type.Interval_24_Concept.Max_2486;
    }
    // field accessors
    static A_1254 = function(self) { return self.A_1254; }
    static B_1261 = function(self) { return self.B_1261; }
    // implemented concepts 
    static Array_25_Concept = new Array_25_Concept(RealInterval_94_Type);
    static Value_23_Concept = new Value_23_Concept(RealInterval_94_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(RealInterval_94_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(RealInterval_94_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(RealInterval_94_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(RealInterval_94_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(RealInterval_94_Type);
    static Vector_14_Concept = new Vector_14_Concept(RealInterval_94_Type);
    static Interval_24_Concept = new Interval_24_Concept(RealInterval_94_Type);
    static Implements = [Array_25_Concept,Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept,Vector_14_Concept,Interval_24_Concept];
}
class Interval2D_95_Type
{
    constructor(A_1268, B_1275)
    {
        // field initialization 
        this.A_1268 = A_1268;
        this.B_1275 = B_1275;
        this.Count_2490 = Interval2D_95_Type.Array_25_Concept.Count_2490;
        this.At_2495 = Interval2D_95_Type.Array_25_Concept.At_2495;
        this.Type_2437 = Interval2D_95_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Interval2D_95_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Interval2D_95_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Interval2D_95_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Interval2D_95_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Interval2D_95_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Interval2D_95_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Interval2D_95_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Interval2D_95_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Interval2D_95_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = Interval2D_95_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = Interval2D_95_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = Interval2D_95_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = Interval2D_95_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = Interval2D_95_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = Interval2D_95_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = Interval2D_95_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Interval2D_95_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Interval2D_95_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Count_2133 = Interval2D_95_Type.Vector_14_Concept.Count_2133;
        this.At_2147 = Interval2D_95_Type.Vector_14_Concept.At_2147;
        this.Min_2483 = Interval2D_95_Type.Interval_24_Concept.Min_2483;
        this.Max_2486 = Interval2D_95_Type.Interval_24_Concept.Max_2486;
    }
    // field accessors
    static A_1268 = function(self) { return self.A_1268; }
    static B_1275 = function(self) { return self.B_1275; }
    // implemented concepts 
    static Array_25_Concept = new Array_25_Concept(Interval2D_95_Type);
    static Value_23_Concept = new Value_23_Concept(Interval2D_95_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(Interval2D_95_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Interval2D_95_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Interval2D_95_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Interval2D_95_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(Interval2D_95_Type);
    static Vector_14_Concept = new Vector_14_Concept(Interval2D_95_Type);
    static Interval_24_Concept = new Interval_24_Concept(Interval2D_95_Type);
    static Implements = [Array_25_Concept,Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept,Vector_14_Concept,Interval_24_Concept];
}
class Interval3D_96_Type
{
    constructor(A_1282, B_1289)
    {
        // field initialization 
        this.A_1282 = A_1282;
        this.B_1289 = B_1289;
        this.Count_2490 = Interval3D_96_Type.Array_25_Concept.Count_2490;
        this.At_2495 = Interval3D_96_Type.Array_25_Concept.At_2495;
        this.Type_2437 = Interval3D_96_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Interval3D_96_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Interval3D_96_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Interval3D_96_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Interval3D_96_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Interval3D_96_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Interval3D_96_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Interval3D_96_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Interval3D_96_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Interval3D_96_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = Interval3D_96_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = Interval3D_96_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = Interval3D_96_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = Interval3D_96_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = Interval3D_96_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = Interval3D_96_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = Interval3D_96_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Interval3D_96_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Interval3D_96_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Count_2133 = Interval3D_96_Type.Vector_14_Concept.Count_2133;
        this.At_2147 = Interval3D_96_Type.Vector_14_Concept.At_2147;
        this.Min_2483 = Interval3D_96_Type.Interval_24_Concept.Min_2483;
        this.Max_2486 = Interval3D_96_Type.Interval_24_Concept.Max_2486;
    }
    // field accessors
    static A_1282 = function(self) { return self.A_1282; }
    static B_1289 = function(self) { return self.B_1289; }
    // implemented concepts 
    static Array_25_Concept = new Array_25_Concept(Interval3D_96_Type);
    static Value_23_Concept = new Value_23_Concept(Interval3D_96_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(Interval3D_96_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Interval3D_96_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Interval3D_96_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Interval3D_96_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(Interval3D_96_Type);
    static Vector_14_Concept = new Vector_14_Concept(Interval3D_96_Type);
    static Interval_24_Concept = new Interval_24_Concept(Interval3D_96_Type);
    static Implements = [Array_25_Concept,Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept,Vector_14_Concept,Interval_24_Concept];
}
class Capsule_97_Type
{
    constructor(Line_1296, Radius_1303)
    {
        // field initialization 
        this.Line_1296 = Line_1296;
        this.Radius_1303 = Radius_1303;
        this.Type_2437 = Capsule_97_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Capsule_97_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Capsule_97_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Capsule_97_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Capsule_97_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Capsule_97_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Capsule_97_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Capsule_97_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Capsule_97_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Capsule_97_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Line_1296 = function(self) { return self.Line_1296; }
    static Radius_1303 = function(self) { return self.Radius_1303; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Capsule_97_Type);
    static Implements = [Value_23_Concept];
}
class Matrix3D_98_Type
{
    constructor(Column1_1310, Column2_1317, Column3_1324, Column4_1331)
    {
        // field initialization 
        this.Column1_1310 = Column1_1310;
        this.Column2_1317 = Column2_1317;
        this.Column3_1324 = Column3_1324;
        this.Column4_1331 = Column4_1331;
        this.Type_2437 = Matrix3D_98_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Matrix3D_98_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Matrix3D_98_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Matrix3D_98_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Matrix3D_98_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Matrix3D_98_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Matrix3D_98_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Matrix3D_98_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Matrix3D_98_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Matrix3D_98_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Column1_1310 = function(self) { return self.Column1_1310; }
    static Column2_1317 = function(self) { return self.Column2_1317; }
    static Column3_1324 = function(self) { return self.Column3_1324; }
    static Column4_1331 = function(self) { return self.Column4_1331; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Matrix3D_98_Type);
    static Implements = [Value_23_Concept];
}
class Cylinder_99_Type
{
    constructor(Line_1338, Radius_1345)
    {
        // field initialization 
        this.Line_1338 = Line_1338;
        this.Radius_1345 = Radius_1345;
        this.Type_2437 = Cylinder_99_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Cylinder_99_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Cylinder_99_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Cylinder_99_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Cylinder_99_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Cylinder_99_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Cylinder_99_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Cylinder_99_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Cylinder_99_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Cylinder_99_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Line_1338 = function(self) { return self.Line_1338; }
    static Radius_1345 = function(self) { return self.Radius_1345; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Cylinder_99_Type);
    static Implements = [Value_23_Concept];
}
class Cone_100_Type
{
    constructor(Line_1352, Radius_1359)
    {
        // field initialization 
        this.Line_1352 = Line_1352;
        this.Radius_1359 = Radius_1359;
        this.Type_2437 = Cone_100_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Cone_100_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Cone_100_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Cone_100_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Cone_100_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Cone_100_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Cone_100_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Cone_100_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Cone_100_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Cone_100_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Line_1352 = function(self) { return self.Line_1352; }
    static Radius_1359 = function(self) { return self.Radius_1359; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Cone_100_Type);
    static Implements = [Value_23_Concept];
}
class Tube_101_Type
{
    constructor(Line_1366, InnerRadius_1373, OuterRadius_1380)
    {
        // field initialization 
        this.Line_1366 = Line_1366;
        this.InnerRadius_1373 = InnerRadius_1373;
        this.OuterRadius_1380 = OuterRadius_1380;
        this.Type_2437 = Tube_101_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Tube_101_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Tube_101_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Tube_101_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Tube_101_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Tube_101_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Tube_101_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Tube_101_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Tube_101_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Tube_101_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Line_1366 = function(self) { return self.Line_1366; }
    static InnerRadius_1373 = function(self) { return self.InnerRadius_1373; }
    static OuterRadius_1380 = function(self) { return self.OuterRadius_1380; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Tube_101_Type);
    static Implements = [Value_23_Concept];
}
class ConeSegment_102_Type
{
    constructor(Line_1387, Radius1_1394, Radius2_1401)
    {
        // field initialization 
        this.Line_1387 = Line_1387;
        this.Radius1_1394 = Radius1_1394;
        this.Radius2_1401 = Radius2_1401;
        this.Type_2437 = ConeSegment_102_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = ConeSegment_102_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = ConeSegment_102_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = ConeSegment_102_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = ConeSegment_102_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = ConeSegment_102_Type.Value_23_Concept.One_2455;
        this.Default_2460 = ConeSegment_102_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = ConeSegment_102_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = ConeSegment_102_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = ConeSegment_102_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Line_1387 = function(self) { return self.Line_1387; }
    static Radius1_1394 = function(self) { return self.Radius1_1394; }
    static Radius2_1401 = function(self) { return self.Radius2_1401; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ConeSegment_102_Type);
    static Implements = [Value_23_Concept];
}
class Box2D_103_Type
{
    constructor(Center_1408, Rotation_1415, Extent_1422)
    {
        // field initialization 
        this.Center_1408 = Center_1408;
        this.Rotation_1415 = Rotation_1415;
        this.Extent_1422 = Extent_1422;
        this.Type_2437 = Box2D_103_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Box2D_103_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Box2D_103_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Box2D_103_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Box2D_103_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Box2D_103_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Box2D_103_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Box2D_103_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Box2D_103_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Box2D_103_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Center_1408 = function(self) { return self.Center_1408; }
    static Rotation_1415 = function(self) { return self.Rotation_1415; }
    static Extent_1422 = function(self) { return self.Extent_1422; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Box2D_103_Type);
    static Implements = [Value_23_Concept];
}
class Box3D_104_Type
{
    constructor(Center_1429, Rotation_1436, Extent_1443)
    {
        // field initialization 
        this.Center_1429 = Center_1429;
        this.Rotation_1436 = Rotation_1436;
        this.Extent_1443 = Extent_1443;
        this.Type_2437 = Box3D_104_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Box3D_104_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Box3D_104_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Box3D_104_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Box3D_104_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Box3D_104_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Box3D_104_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Box3D_104_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Box3D_104_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Box3D_104_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Center_1429 = function(self) { return self.Center_1429; }
    static Rotation_1436 = function(self) { return self.Rotation_1436; }
    static Extent_1443 = function(self) { return self.Extent_1443; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Box3D_104_Type);
    static Implements = [Value_23_Concept];
}
class CubicBezierTriangle3D_105_Type
{
    constructor(A_1450, B_1457, C_1464, A2B_1471, AB2_1478, B2C_1485, BC2_1492, AC2_1499, A2C_1506, ABC_1513)
    {
        // field initialization 
        this.A_1450 = A_1450;
        this.B_1457 = B_1457;
        this.C_1464 = C_1464;
        this.A2B_1471 = A2B_1471;
        this.AB2_1478 = AB2_1478;
        this.B2C_1485 = B2C_1485;
        this.BC2_1492 = BC2_1492;
        this.AC2_1499 = AC2_1499;
        this.A2C_1506 = A2C_1506;
        this.ABC_1513 = ABC_1513;
        this.Type_2437 = CubicBezierTriangle3D_105_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = CubicBezierTriangle3D_105_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = CubicBezierTriangle3D_105_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = CubicBezierTriangle3D_105_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = CubicBezierTriangle3D_105_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = CubicBezierTriangle3D_105_Type.Value_23_Concept.One_2455;
        this.Default_2460 = CubicBezierTriangle3D_105_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = CubicBezierTriangle3D_105_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = CubicBezierTriangle3D_105_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = CubicBezierTriangle3D_105_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static A_1450 = function(self) { return self.A_1450; }
    static B_1457 = function(self) { return self.B_1457; }
    static C_1464 = function(self) { return self.C_1464; }
    static A2B_1471 = function(self) { return self.A2B_1471; }
    static AB2_1478 = function(self) { return self.AB2_1478; }
    static B2C_1485 = function(self) { return self.B2C_1485; }
    static BC2_1492 = function(self) { return self.BC2_1492; }
    static AC2_1499 = function(self) { return self.AC2_1499; }
    static A2C_1506 = function(self) { return self.A2C_1506; }
    static ABC_1513 = function(self) { return self.ABC_1513; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(CubicBezierTriangle3D_105_Type);
    static Implements = [Value_23_Concept];
}
class CubicBezier2D_106_Type
{
    constructor(A_1520, B_1527, C_1534, D_1541)
    {
        // field initialization 
        this.A_1520 = A_1520;
        this.B_1527 = B_1527;
        this.C_1534 = C_1534;
        this.D_1541 = D_1541;
        this.Type_2437 = CubicBezier2D_106_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = CubicBezier2D_106_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = CubicBezier2D_106_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = CubicBezier2D_106_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = CubicBezier2D_106_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = CubicBezier2D_106_Type.Value_23_Concept.One_2455;
        this.Default_2460 = CubicBezier2D_106_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = CubicBezier2D_106_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = CubicBezier2D_106_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = CubicBezier2D_106_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static A_1520 = function(self) { return self.A_1520; }
    static B_1527 = function(self) { return self.B_1527; }
    static C_1534 = function(self) { return self.C_1534; }
    static D_1541 = function(self) { return self.D_1541; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(CubicBezier2D_106_Type);
    static Implements = [Value_23_Concept];
}
class UV_107_Type
{
    constructor(U_1548, V_1555)
    {
        // field initialization 
        this.U_1548 = U_1548;
        this.V_1555 = V_1555;
        this.Count_2490 = UV_107_Type.Array_25_Concept.Count_2490;
        this.At_2495 = UV_107_Type.Array_25_Concept.At_2495;
        this.Type_2437 = UV_107_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = UV_107_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = UV_107_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = UV_107_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = UV_107_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = UV_107_Type.Value_23_Concept.One_2455;
        this.Default_2460 = UV_107_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = UV_107_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = UV_107_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = UV_107_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = UV_107_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = UV_107_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = UV_107_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = UV_107_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = UV_107_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = UV_107_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = UV_107_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = UV_107_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = UV_107_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Count_2133 = UV_107_Type.Vector_14_Concept.Count_2133;
        this.At_2147 = UV_107_Type.Vector_14_Concept.At_2147;
    }
    // field accessors
    static U_1548 = function(self) { return self.U_1548; }
    static V_1555 = function(self) { return self.V_1555; }
    // implemented concepts 
    static Array_25_Concept = new Array_25_Concept(UV_107_Type);
    static Value_23_Concept = new Value_23_Concept(UV_107_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(UV_107_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(UV_107_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(UV_107_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(UV_107_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(UV_107_Type);
    static Vector_14_Concept = new Vector_14_Concept(UV_107_Type);
    static Implements = [Array_25_Concept,Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept,Vector_14_Concept];
}
class UVW_108_Type
{
    constructor(U_1562, V_1569, W_1576)
    {
        // field initialization 
        this.U_1562 = U_1562;
        this.V_1569 = V_1569;
        this.W_1576 = W_1576;
        this.Count_2490 = UVW_108_Type.Array_25_Concept.Count_2490;
        this.At_2495 = UVW_108_Type.Array_25_Concept.At_2495;
        this.Type_2437 = UVW_108_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = UVW_108_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = UVW_108_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = UVW_108_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = UVW_108_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = UVW_108_Type.Value_23_Concept.One_2455;
        this.Default_2460 = UVW_108_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = UVW_108_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = UVW_108_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = UVW_108_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = UVW_108_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = UVW_108_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = UVW_108_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = UVW_108_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = UVW_108_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = UVW_108_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = UVW_108_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = UVW_108_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = UVW_108_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Count_2133 = UVW_108_Type.Vector_14_Concept.Count_2133;
        this.At_2147 = UVW_108_Type.Vector_14_Concept.At_2147;
    }
    // field accessors
    static U_1562 = function(self) { return self.U_1562; }
    static V_1569 = function(self) { return self.V_1569; }
    static W_1576 = function(self) { return self.W_1576; }
    // implemented concepts 
    static Array_25_Concept = new Array_25_Concept(UVW_108_Type);
    static Value_23_Concept = new Value_23_Concept(UVW_108_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(UVW_108_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(UVW_108_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(UVW_108_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(UVW_108_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(UVW_108_Type);
    static Vector_14_Concept = new Vector_14_Concept(UVW_108_Type);
    static Implements = [Array_25_Concept,Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept,Vector_14_Concept];
}
class CubicBezier3D_109_Type
{
    constructor(A_1583, B_1590, C_1597, D_1604)
    {
        // field initialization 
        this.A_1583 = A_1583;
        this.B_1590 = B_1590;
        this.C_1597 = C_1597;
        this.D_1604 = D_1604;
        this.Type_2437 = CubicBezier3D_109_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = CubicBezier3D_109_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = CubicBezier3D_109_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = CubicBezier3D_109_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = CubicBezier3D_109_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = CubicBezier3D_109_Type.Value_23_Concept.One_2455;
        this.Default_2460 = CubicBezier3D_109_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = CubicBezier3D_109_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = CubicBezier3D_109_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = CubicBezier3D_109_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static A_1583 = function(self) { return self.A_1583; }
    static B_1590 = function(self) { return self.B_1590; }
    static C_1597 = function(self) { return self.C_1597; }
    static D_1604 = function(self) { return self.D_1604; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(CubicBezier3D_109_Type);
    static Implements = [Value_23_Concept];
}
class QuadraticBezier2D_110_Type
{
    constructor(A_1611, B_1618, C_1625)
    {
        // field initialization 
        this.A_1611 = A_1611;
        this.B_1618 = B_1618;
        this.C_1625 = C_1625;
        this.Type_2437 = QuadraticBezier2D_110_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = QuadraticBezier2D_110_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = QuadraticBezier2D_110_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = QuadraticBezier2D_110_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = QuadraticBezier2D_110_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = QuadraticBezier2D_110_Type.Value_23_Concept.One_2455;
        this.Default_2460 = QuadraticBezier2D_110_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = QuadraticBezier2D_110_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = QuadraticBezier2D_110_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = QuadraticBezier2D_110_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static A_1611 = function(self) { return self.A_1611; }
    static B_1618 = function(self) { return self.B_1618; }
    static C_1625 = function(self) { return self.C_1625; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(QuadraticBezier2D_110_Type);
    static Implements = [Value_23_Concept];
}
class QuadraticBezier3D_111_Type
{
    constructor(A_1632, B_1639, C_1646)
    {
        // field initialization 
        this.A_1632 = A_1632;
        this.B_1639 = B_1639;
        this.C_1646 = C_1646;
        this.Type_2437 = QuadraticBezier3D_111_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = QuadraticBezier3D_111_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = QuadraticBezier3D_111_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = QuadraticBezier3D_111_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = QuadraticBezier3D_111_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = QuadraticBezier3D_111_Type.Value_23_Concept.One_2455;
        this.Default_2460 = QuadraticBezier3D_111_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = QuadraticBezier3D_111_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = QuadraticBezier3D_111_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = QuadraticBezier3D_111_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static A_1632 = function(self) { return self.A_1632; }
    static B_1639 = function(self) { return self.B_1639; }
    static C_1646 = function(self) { return self.C_1646; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(QuadraticBezier3D_111_Type);
    static Implements = [Value_23_Concept];
}
class Area_112_Type
{
    constructor(MetersSquared_1653)
    {
        // field initialization 
        this.MetersSquared_1653 = MetersSquared_1653;
        this.Type_2437 = Area_112_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Area_112_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Area_112_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Area_112_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Area_112_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Area_112_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Area_112_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Area_112_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Area_112_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Area_112_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = Area_112_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = Area_112_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = Area_112_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = Area_112_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = Area_112_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = Area_112_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Area_112_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Area_112_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = Area_112_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static MetersSquared_1653 = function(self) { return self.MetersSquared_1653; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Area_112_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(Area_112_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Area_112_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Area_112_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Area_112_Type);
    static Measure_15_Concept = new Measure_15_Concept(Area_112_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class Volume_113_Type
{
    constructor(MetersCubed_1660)
    {
        // field initialization 
        this.MetersCubed_1660 = MetersCubed_1660;
        this.Type_2437 = Volume_113_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Volume_113_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Volume_113_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Volume_113_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Volume_113_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Volume_113_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Volume_113_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Volume_113_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Volume_113_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Volume_113_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = Volume_113_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = Volume_113_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = Volume_113_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = Volume_113_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = Volume_113_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = Volume_113_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Volume_113_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Volume_113_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = Volume_113_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static MetersCubed_1660 = function(self) { return self.MetersCubed_1660; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Volume_113_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(Volume_113_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Volume_113_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Volume_113_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Volume_113_Type);
    static Measure_15_Concept = new Measure_15_Concept(Volume_113_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class Velocity_114_Type
{
    constructor(MetersPerSecond_1667)
    {
        // field initialization 
        this.MetersPerSecond_1667 = MetersPerSecond_1667;
        this.Type_2437 = Velocity_114_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Velocity_114_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Velocity_114_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Velocity_114_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Velocity_114_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Velocity_114_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Velocity_114_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Velocity_114_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Velocity_114_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Velocity_114_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = Velocity_114_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = Velocity_114_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = Velocity_114_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = Velocity_114_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = Velocity_114_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = Velocity_114_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Velocity_114_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Velocity_114_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = Velocity_114_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static MetersPerSecond_1667 = function(self) { return self.MetersPerSecond_1667; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Velocity_114_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(Velocity_114_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Velocity_114_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Velocity_114_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Velocity_114_Type);
    static Measure_15_Concept = new Measure_15_Concept(Velocity_114_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class Acceleration_115_Type
{
    constructor(MetersPerSecondSquared_1674)
    {
        // field initialization 
        this.MetersPerSecondSquared_1674 = MetersPerSecondSquared_1674;
        this.Type_2437 = Acceleration_115_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Acceleration_115_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Acceleration_115_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Acceleration_115_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Acceleration_115_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Acceleration_115_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Acceleration_115_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Acceleration_115_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Acceleration_115_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Acceleration_115_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = Acceleration_115_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = Acceleration_115_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = Acceleration_115_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = Acceleration_115_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = Acceleration_115_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = Acceleration_115_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Acceleration_115_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Acceleration_115_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = Acceleration_115_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static MetersPerSecondSquared_1674 = function(self) { return self.MetersPerSecondSquared_1674; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Acceleration_115_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(Acceleration_115_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Acceleration_115_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Acceleration_115_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Acceleration_115_Type);
    static Measure_15_Concept = new Measure_15_Concept(Acceleration_115_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class Force_116_Type
{
    constructor(Newtons_1681)
    {
        // field initialization 
        this.Newtons_1681 = Newtons_1681;
        this.Type_2437 = Force_116_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Force_116_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Force_116_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Force_116_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Force_116_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Force_116_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Force_116_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Force_116_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Force_116_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Force_116_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = Force_116_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = Force_116_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = Force_116_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = Force_116_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = Force_116_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = Force_116_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Force_116_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Force_116_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = Force_116_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Newtons_1681 = function(self) { return self.Newtons_1681; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Force_116_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(Force_116_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Force_116_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Force_116_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Force_116_Type);
    static Measure_15_Concept = new Measure_15_Concept(Force_116_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class Pressure_117_Type
{
    constructor(Pascals_1688)
    {
        // field initialization 
        this.Pascals_1688 = Pascals_1688;
        this.Type_2437 = Pressure_117_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Pressure_117_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Pressure_117_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Pressure_117_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Pressure_117_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Pressure_117_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Pressure_117_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Pressure_117_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Pressure_117_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Pressure_117_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = Pressure_117_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = Pressure_117_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = Pressure_117_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = Pressure_117_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = Pressure_117_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = Pressure_117_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Pressure_117_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Pressure_117_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = Pressure_117_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Pascals_1688 = function(self) { return self.Pascals_1688; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Pressure_117_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(Pressure_117_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Pressure_117_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Pressure_117_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Pressure_117_Type);
    static Measure_15_Concept = new Measure_15_Concept(Pressure_117_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class Energy_118_Type
{
    constructor(Joules_1695)
    {
        // field initialization 
        this.Joules_1695 = Joules_1695;
        this.Type_2437 = Energy_118_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Energy_118_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Energy_118_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Energy_118_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Energy_118_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Energy_118_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Energy_118_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Energy_118_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Energy_118_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Energy_118_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = Energy_118_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = Energy_118_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = Energy_118_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = Energy_118_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = Energy_118_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = Energy_118_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Energy_118_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Energy_118_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = Energy_118_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Joules_1695 = function(self) { return self.Joules_1695; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Energy_118_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(Energy_118_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Energy_118_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Energy_118_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Energy_118_Type);
    static Measure_15_Concept = new Measure_15_Concept(Energy_118_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class Memory_119_Type
{
    constructor(Bytes_1702)
    {
        // field initialization 
        this.Bytes_1702 = Bytes_1702;
        this.Type_2437 = Memory_119_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Memory_119_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Memory_119_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Memory_119_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Memory_119_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Memory_119_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Memory_119_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Memory_119_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Memory_119_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Memory_119_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = Memory_119_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = Memory_119_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = Memory_119_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = Memory_119_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = Memory_119_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = Memory_119_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Memory_119_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Memory_119_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = Memory_119_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Bytes_1702 = function(self) { return self.Bytes_1702; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Memory_119_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(Memory_119_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Memory_119_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Memory_119_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Memory_119_Type);
    static Measure_15_Concept = new Measure_15_Concept(Memory_119_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class Frequency_120_Type
{
    constructor(Hertz_1709)
    {
        // field initialization 
        this.Hertz_1709 = Hertz_1709;
        this.Type_2437 = Frequency_120_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Frequency_120_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Frequency_120_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Frequency_120_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Frequency_120_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Frequency_120_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Frequency_120_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Frequency_120_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Frequency_120_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Frequency_120_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = Frequency_120_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = Frequency_120_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = Frequency_120_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = Frequency_120_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = Frequency_120_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = Frequency_120_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Frequency_120_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Frequency_120_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = Frequency_120_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Hertz_1709 = function(self) { return self.Hertz_1709; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Frequency_120_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(Frequency_120_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Frequency_120_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Frequency_120_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Frequency_120_Type);
    static Measure_15_Concept = new Measure_15_Concept(Frequency_120_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class Loudness_121_Type
{
    constructor(Decibels_1716)
    {
        // field initialization 
        this.Decibels_1716 = Decibels_1716;
        this.Type_2437 = Loudness_121_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Loudness_121_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Loudness_121_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Loudness_121_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Loudness_121_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Loudness_121_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Loudness_121_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Loudness_121_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Loudness_121_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Loudness_121_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = Loudness_121_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = Loudness_121_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = Loudness_121_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = Loudness_121_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = Loudness_121_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = Loudness_121_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Loudness_121_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Loudness_121_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = Loudness_121_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Decibels_1716 = function(self) { return self.Decibels_1716; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Loudness_121_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(Loudness_121_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Loudness_121_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Loudness_121_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Loudness_121_Type);
    static Measure_15_Concept = new Measure_15_Concept(Loudness_121_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class LuminousIntensity_122_Type
{
    constructor(Candelas_1723)
    {
        // field initialization 
        this.Candelas_1723 = Candelas_1723;
        this.Type_2437 = LuminousIntensity_122_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = LuminousIntensity_122_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = LuminousIntensity_122_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = LuminousIntensity_122_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = LuminousIntensity_122_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = LuminousIntensity_122_Type.Value_23_Concept.One_2455;
        this.Default_2460 = LuminousIntensity_122_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = LuminousIntensity_122_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = LuminousIntensity_122_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = LuminousIntensity_122_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = LuminousIntensity_122_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = LuminousIntensity_122_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = LuminousIntensity_122_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = LuminousIntensity_122_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = LuminousIntensity_122_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = LuminousIntensity_122_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = LuminousIntensity_122_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = LuminousIntensity_122_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = LuminousIntensity_122_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Candelas_1723 = function(self) { return self.Candelas_1723; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(LuminousIntensity_122_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(LuminousIntensity_122_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(LuminousIntensity_122_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(LuminousIntensity_122_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(LuminousIntensity_122_Type);
    static Measure_15_Concept = new Measure_15_Concept(LuminousIntensity_122_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class ElectricPotential_123_Type
{
    constructor(Volts_1730)
    {
        // field initialization 
        this.Volts_1730 = Volts_1730;
        this.Type_2437 = ElectricPotential_123_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = ElectricPotential_123_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = ElectricPotential_123_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = ElectricPotential_123_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = ElectricPotential_123_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = ElectricPotential_123_Type.Value_23_Concept.One_2455;
        this.Default_2460 = ElectricPotential_123_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = ElectricPotential_123_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = ElectricPotential_123_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = ElectricPotential_123_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = ElectricPotential_123_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = ElectricPotential_123_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = ElectricPotential_123_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = ElectricPotential_123_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = ElectricPotential_123_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = ElectricPotential_123_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = ElectricPotential_123_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = ElectricPotential_123_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = ElectricPotential_123_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Volts_1730 = function(self) { return self.Volts_1730; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ElectricPotential_123_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(ElectricPotential_123_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(ElectricPotential_123_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(ElectricPotential_123_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(ElectricPotential_123_Type);
    static Measure_15_Concept = new Measure_15_Concept(ElectricPotential_123_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class ElectricCharge_124_Type
{
    constructor(Columbs_1737)
    {
        // field initialization 
        this.Columbs_1737 = Columbs_1737;
        this.Type_2437 = ElectricCharge_124_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = ElectricCharge_124_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = ElectricCharge_124_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = ElectricCharge_124_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = ElectricCharge_124_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = ElectricCharge_124_Type.Value_23_Concept.One_2455;
        this.Default_2460 = ElectricCharge_124_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = ElectricCharge_124_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = ElectricCharge_124_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = ElectricCharge_124_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = ElectricCharge_124_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = ElectricCharge_124_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = ElectricCharge_124_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = ElectricCharge_124_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = ElectricCharge_124_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = ElectricCharge_124_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = ElectricCharge_124_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = ElectricCharge_124_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = ElectricCharge_124_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Columbs_1737 = function(self) { return self.Columbs_1737; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ElectricCharge_124_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(ElectricCharge_124_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(ElectricCharge_124_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(ElectricCharge_124_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(ElectricCharge_124_Type);
    static Measure_15_Concept = new Measure_15_Concept(ElectricCharge_124_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class ElectricCurrent_125_Type
{
    constructor(Amperes_1744)
    {
        // field initialization 
        this.Amperes_1744 = Amperes_1744;
        this.Type_2437 = ElectricCurrent_125_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = ElectricCurrent_125_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = ElectricCurrent_125_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = ElectricCurrent_125_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = ElectricCurrent_125_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = ElectricCurrent_125_Type.Value_23_Concept.One_2455;
        this.Default_2460 = ElectricCurrent_125_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = ElectricCurrent_125_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = ElectricCurrent_125_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = ElectricCurrent_125_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = ElectricCurrent_125_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = ElectricCurrent_125_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = ElectricCurrent_125_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = ElectricCurrent_125_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = ElectricCurrent_125_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = ElectricCurrent_125_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = ElectricCurrent_125_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = ElectricCurrent_125_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = ElectricCurrent_125_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Amperes_1744 = function(self) { return self.Amperes_1744; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ElectricCurrent_125_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(ElectricCurrent_125_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(ElectricCurrent_125_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(ElectricCurrent_125_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(ElectricCurrent_125_Type);
    static Measure_15_Concept = new Measure_15_Concept(ElectricCurrent_125_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class ElectricResistance_126_Type
{
    constructor(Ohms_1751)
    {
        // field initialization 
        this.Ohms_1751 = Ohms_1751;
        this.Type_2437 = ElectricResistance_126_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = ElectricResistance_126_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = ElectricResistance_126_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = ElectricResistance_126_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = ElectricResistance_126_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = ElectricResistance_126_Type.Value_23_Concept.One_2455;
        this.Default_2460 = ElectricResistance_126_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = ElectricResistance_126_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = ElectricResistance_126_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = ElectricResistance_126_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = ElectricResistance_126_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = ElectricResistance_126_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = ElectricResistance_126_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = ElectricResistance_126_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = ElectricResistance_126_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = ElectricResistance_126_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = ElectricResistance_126_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = ElectricResistance_126_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = ElectricResistance_126_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Ohms_1751 = function(self) { return self.Ohms_1751; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ElectricResistance_126_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(ElectricResistance_126_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(ElectricResistance_126_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(ElectricResistance_126_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(ElectricResistance_126_Type);
    static Measure_15_Concept = new Measure_15_Concept(ElectricResistance_126_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class Power_127_Type
{
    constructor(Watts_1758)
    {
        // field initialization 
        this.Watts_1758 = Watts_1758;
        this.Type_2437 = Power_127_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Power_127_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Power_127_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Power_127_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Power_127_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Power_127_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Power_127_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Power_127_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Power_127_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Power_127_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = Power_127_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = Power_127_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = Power_127_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = Power_127_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = Power_127_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = Power_127_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Power_127_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Power_127_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = Power_127_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static Watts_1758 = function(self) { return self.Watts_1758; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Power_127_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(Power_127_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Power_127_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Power_127_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Power_127_Type);
    static Measure_15_Concept = new Measure_15_Concept(Power_127_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class Density_128_Type
{
    constructor(KilogramsPerMeterCubed_1765)
    {
        // field initialization 
        this.KilogramsPerMeterCubed_1765 = KilogramsPerMeterCubed_1765;
        this.Type_2437 = Density_128_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Density_128_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Density_128_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Density_128_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Density_128_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Density_128_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Density_128_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Density_128_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Density_128_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Density_128_Type.Value_23_Concept.ToString_2479;
        this.Add_2335 = Density_128_Type.ScalarArithmetic_21_Concept.Add_2335;
        this.Subtract_2349 = Density_128_Type.ScalarArithmetic_21_Concept.Subtract_2349;
        this.Multiply_2363 = Density_128_Type.ScalarArithmetic_21_Concept.Multiply_2363;
        this.Divide_2377 = Density_128_Type.ScalarArithmetic_21_Concept.Divide_2377;
        this.Modulo_2391 = Density_128_Type.ScalarArithmetic_21_Concept.Modulo_2391;
        this.Equals_2232 = Density_128_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Density_128_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Density_128_Type.Magnitude_17_Concept.Magnitude_2175;
        this.Value_2159 = Density_128_Type.Measure_15_Concept.Value_2159;
    }
    // field accessors
    static KilogramsPerMeterCubed_1765 = function(self) { return self.KilogramsPerMeterCubed_1765; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Density_128_Type);
    static ScalarArithmetic_21_Concept = new ScalarArithmetic_21_Concept(Density_128_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Density_128_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Density_128_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Density_128_Type);
    static Measure_15_Concept = new Measure_15_Concept(Density_128_Type);
    static Implements = [Value_23_Concept,ScalarArithmetic_21_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Measure_15_Concept];
}
class NormalDistribution_129_Type
{
    constructor(Mean_1772, StandardDeviation_1779)
    {
        // field initialization 
        this.Mean_1772 = Mean_1772;
        this.StandardDeviation_1779 = StandardDeviation_1779;
        this.Type_2437 = NormalDistribution_129_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = NormalDistribution_129_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = NormalDistribution_129_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = NormalDistribution_129_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = NormalDistribution_129_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = NormalDistribution_129_Type.Value_23_Concept.One_2455;
        this.Default_2460 = NormalDistribution_129_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = NormalDistribution_129_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = NormalDistribution_129_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = NormalDistribution_129_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Mean_1772 = function(self) { return self.Mean_1772; }
    static StandardDeviation_1779 = function(self) { return self.StandardDeviation_1779; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(NormalDistribution_129_Type);
    static Implements = [Value_23_Concept];
}
class PoissonDistribution_130_Type
{
    constructor(Expected_1786, Occurrences_1793)
    {
        // field initialization 
        this.Expected_1786 = Expected_1786;
        this.Occurrences_1793 = Occurrences_1793;
        this.Type_2437 = PoissonDistribution_130_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = PoissonDistribution_130_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = PoissonDistribution_130_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = PoissonDistribution_130_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = PoissonDistribution_130_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = PoissonDistribution_130_Type.Value_23_Concept.One_2455;
        this.Default_2460 = PoissonDistribution_130_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = PoissonDistribution_130_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = PoissonDistribution_130_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = PoissonDistribution_130_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Expected_1786 = function(self) { return self.Expected_1786; }
    static Occurrences_1793 = function(self) { return self.Occurrences_1793; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(PoissonDistribution_130_Type);
    static Implements = [Value_23_Concept];
}
class BernoulliDistribution_131_Type
{
    constructor(P_1800)
    {
        // field initialization 
        this.P_1800 = P_1800;
        this.Type_2437 = BernoulliDistribution_131_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = BernoulliDistribution_131_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = BernoulliDistribution_131_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = BernoulliDistribution_131_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = BernoulliDistribution_131_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = BernoulliDistribution_131_Type.Value_23_Concept.One_2455;
        this.Default_2460 = BernoulliDistribution_131_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = BernoulliDistribution_131_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = BernoulliDistribution_131_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = BernoulliDistribution_131_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static P_1800 = function(self) { return self.P_1800; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(BernoulliDistribution_131_Type);
    static Implements = [Value_23_Concept];
}
class Probability_132_Type
{
    constructor(Value_1807)
    {
        // field initialization 
        this.Value_1807 = Value_1807;
        this.Type_2437 = Probability_132_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = Probability_132_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = Probability_132_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = Probability_132_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = Probability_132_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = Probability_132_Type.Value_23_Concept.One_2455;
        this.Default_2460 = Probability_132_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = Probability_132_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = Probability_132_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = Probability_132_Type.Value_23_Concept.ToString_2479;
        this.Add_2249 = Probability_132_Type.Arithmetic_20_Concept.Add_2249;
        this.Negative_2259 = Probability_132_Type.Arithmetic_20_Concept.Negative_2259;
        this.Reciprocal_2269 = Probability_132_Type.Arithmetic_20_Concept.Reciprocal_2269;
        this.Multiply_2286 = Probability_132_Type.Arithmetic_20_Concept.Multiply_2286;
        this.Divide_2303 = Probability_132_Type.Arithmetic_20_Concept.Divide_2303;
        this.Modulo_2320 = Probability_132_Type.Arithmetic_20_Concept.Modulo_2320;
        this.Equals_2232 = Probability_132_Type.Equatable_19_Concept.Equals_2232;
        this.Compare_2212 = Probability_132_Type.Comparable_18_Concept.Compare_2212;
        this.Magnitude_2175 = Probability_132_Type.Magnitude_17_Concept.Magnitude_2175;
    }
    // field accessors
    static Value_1807 = function(self) { return self.Value_1807; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Probability_132_Type);
    static Arithmetic_20_Concept = new Arithmetic_20_Concept(Probability_132_Type);
    static Equatable_19_Concept = new Equatable_19_Concept(Probability_132_Type);
    static Comparable_18_Concept = new Comparable_18_Concept(Probability_132_Type);
    static Magnitude_17_Concept = new Magnitude_17_Concept(Probability_132_Type);
    static Numerical_16_Concept = new Numerical_16_Concept(Probability_132_Type);
    static Implements = [Value_23_Concept,Arithmetic_20_Concept,Equatable_19_Concept,Comparable_18_Concept,Magnitude_17_Concept,Numerical_16_Concept];
}
class BinomialDistribution_133_Type
{
    constructor(Trials_1814, P_1821)
    {
        // field initialization 
        this.Trials_1814 = Trials_1814;
        this.P_1821 = P_1821;
        this.Type_2437 = BinomialDistribution_133_Type.Value_23_Concept.Type_2437;
        this.FieldTypes_2439 = BinomialDistribution_133_Type.Value_23_Concept.FieldTypes_2439;
        this.FieldNames_2441 = BinomialDistribution_133_Type.Value_23_Concept.FieldNames_2441;
        this.FieldValues_2445 = BinomialDistribution_133_Type.Value_23_Concept.FieldValues_2445;
        this.Zero_2450 = BinomialDistribution_133_Type.Value_23_Concept.Zero_2450;
        this.One_2455 = BinomialDistribution_133_Type.Value_23_Concept.One_2455;
        this.Default_2460 = BinomialDistribution_133_Type.Value_23_Concept.Default_2460;
        this.MinValue_2465 = BinomialDistribution_133_Type.Value_23_Concept.MinValue_2465;
        this.MaxValue_2470 = BinomialDistribution_133_Type.Value_23_Concept.MaxValue_2470;
        this.ToString_2479 = BinomialDistribution_133_Type.Value_23_Concept.ToString_2479;
    }
    // field accessors
    static Trials_1814 = function(self) { return self.Trials_1814; }
    static P_1821 = function(self) { return self.P_1821; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(BinomialDistribution_133_Type);
    static Implements = [Value_23_Concept];
}

// This is appended to every JavaScript program generated from Plato