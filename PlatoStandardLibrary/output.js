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




class Intrinsics_13_Library
{
    static Cos_2294 = function (x_2293/* : Angle_83 */) /* : Number_29 */{ return null; };
    static Sin_2297 = function (x_2296/* : Angle_83 */) /* : Number_29 */{ return null; };
    static Tan_2300 = function (x_2299/* : Angle_83 */) /* : Number_29 */{ return null; };
    static Acos_2303 = function (x_2302/* : Number_29 */) /* : Angle_83 */{ return null; };
    static Asin_2306 = function (x_2305/* : Number_29 */) /* : Angle_83 */{ return null; };
    static Atan_2309 = function (x_2308/* : Number_29 */) /* : Angle_83 */{ return null; };
    static Pow_2314 = function (x_2311/* : Number_29 */, y_2313/* : Number_29 */) /* : Number_29 */{ return null; };
    static Log_2319 = function (x_2316/* : Number_29 */, y_2318/* : Number_29 */) /* : Number_29 */{ return null; };
    static NaturalLog_2322 = function (x_2321/* : Number_29 */) /* : Number_29 */{ return null; };
    static NaturalPower_2325 = function (x_2324/* : Number_29 */) /* : Number_29 */{ return null; };
    static Interpolate_2328 = function (xs_2327/* : Array_25 */) /* : String_8 */{ return null; };
    static Throw_2331 = function (x_2330/* : Any_5 */) /* : Any_5 */{ return null; };
    static TypeOf_2334 = function (x_2333/* : Any_5 */) /* : Type_12 */{ return null; };
    static Add_2339 = function (x_2336/* : Number_29 */, y_2338/* : Number_29 */) /* : Number_29 */{ return null; };
    static Subtract_2344 = function (x_2341/* : Number_29 */, y_2343/* : Number_29 */) /* : Number_29 */{ return null; };
    static Divide_2349 = function (x_2346/* : Number_29 */, y_2348/* : Number_29 */) /* : Number_29 */{ return null; };
    static Multiply_2354 = function (x_2351/* : Number_29 */, y_2353/* : Number_29 */) /* : Number_29 */{ return null; };
    static Modulo_2359 = function (x_2356/* : Number_29 */, y_2358/* : Number_29 */) /* : Number_29 */{ return null; };
    static Negative_2362 = function (x_2361/* : Number_29 */) /* : Number_29 */{ return null; };
    static Add_2367 = function (x_2364/* : Integer_26 */, y_2366/* : Integer_26 */) /* : Integer_26 */{ return null; };
    static Subtract_2372 = function (x_2369/* : Integer_26 */, y_2371/* : Integer_26 */) /* : Integer_26 */{ return null; };
    static Divide_2377 = function (x_2374/* : Integer_26 */, y_2376/* : Integer_26 */) /* : Integer_26 */{ return null; };
    static Multiply_2382 = function (x_2379/* : Integer_26 */, y_2381/* : Integer_26 */) /* : Integer_26 */{ return null; };
    static Modulo_2387 = function (x_2384/* : Integer_26 */, y_2386/* : Integer_26 */) /* : Integer_26 */{ return null; };
    static Negative_2390 = function (x_2389/* : Integer_26 */) /* : Integer_26 */{ return null; };
    static And_2395 = function (x_2392/* : Boolean_22 */, y_2394/* : Boolean_22 */) /* : Boolean_22 */{ return null; };
    static Or_2400 = function (x_2397/* : Boolean_22 */, y_2399/* : Boolean_22 */) /* : Boolean_22 */{ return null; };
    static Not_2403 = function (x_2402/* : Boolean_22 */) /* : Boolean_22 */{ return null; };
    static FieldTypes_2406 = function (x_2405/* : Any_5 */) /* : Array_25 */{ return null; };
    static FieldNames_2409 = function (x_2408/* : Any_5 */) /* : Array_25 */{ return null; };
    static FieldValues_2412 = function (x_2411/* : Any_5 */) /* : Array_25 */{ return null; };
}
class Interval_134_Library
{
    static Size_2804 = function (x_2791/* : Interval_24 */) /* : Value_23 */{ return Subtract_185/* : Number_29 */(Max_323/* : UnknownType */(x_2791/* : UnknownType */)/* : UnknownType */, Min_320/* : UnknownType */(x_2791/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static IsEmpty_2819 = function (x_2806/* : Interval_24 */) /* : Boolean_22 */{ return GreaterThanOrEquals_2140/* : Boolean_22 */(Min_320/* : UnknownType */(x_2806/* : UnknownType */)/* : UnknownType */, Max_323/* : UnknownType */(x_2806/* : UnknownType */)/* : UnknownType */)/* : Boolean_22 */; };
    static Lerp_2851 = function (x_2821/* : Interval_24 */, amount_2823/* : Unit_30 */) /* : Numerical_16 */{ return Multiply_191/* : Number_29 */(Min_320/* : UnknownType */(x_2821/* : UnknownType */)/* : UnknownType */, Add_182/* : UnknownType */(Subtract_185/* : UnknownType */(1/* : Float_11 */, amount_2823/* : UnknownType */)/* : UnknownType */, Multiply_191/* : UnknownType */(Max_323/* : UnknownType */(x_2821/* : UnknownType */)/* : UnknownType */, amount_2823/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static InverseLerp_2873 = function (x_2853/* : Interval_24 */, value_2855/* : Numerical_16 */) /* : Unit_30 */{ return Divide_188/* : Number_29 */(Subtract_185/* : UnknownType */(value_2855/* : UnknownType */, Min_320/* : UnknownType */(x_2853/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Size_1201/* : UnknownType */(x_2853/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static Negate_2894 = function (x_2875/* : Interval_24 */) /* : Interval_24 */{ return Tuple_7_Primitive/* : Tuple_7 */(Negative_197/* : UnknownType */(Max_323/* : UnknownType */(x_2875/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Negative_197/* : UnknownType */(Min_320/* : UnknownType */(x_2875/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static Reverse_2909 = function (x_2896/* : Interval_24 */) /* : Interval_24 */{ return Tuple_7_Primitive/* : Tuple_7 */(Max_323/* : UnknownType */(x_2896/* : UnknownType */)/* : UnknownType */, Min_320/* : UnknownType */(x_2896/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static Resize_2931 = function (x_2911/* : Interval_24 */, size_2913/* : Numerical_16 */) /* : Interval_24 */{ return Tuple_7_Primitive/* : Tuple_7 */(Min_320/* : UnknownType */(x_2911/* : UnknownType */)/* : UnknownType */, Add_182/* : UnknownType */(Min_320/* : UnknownType */(x_2911/* : UnknownType */)/* : UnknownType */, size_2913/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static Center_2940 = function (x_2933/* : Interval_24 */) /* : Numerical_16 */{ return Lerp_1914/* : Numerical_16 */(x_2933/* : UnknownType */, 0.5/* : Float_11 */)/* : Numerical_16 */; };
    static Contains_2967 = function (x_2942/* : Interval_24 */, value_2944/* : Numerical_16 */) /* : Boolean_22 */{ return LessThanOrEquals_2134/* : UnknownType */(Min_320/* : UnknownType */(x_2942/* : UnknownType */)/* : UnknownType */, And_218/* : UnknownType */(value_2944/* : UnknownType */, LessThanOrEquals_2134/* : UnknownType */(value_2944/* : UnknownType */, Max_323/* : UnknownType */(x_2942/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Contains_2997 = function (x_2969/* : Interval_24 */, other_2971/* : Interval_24 */) /* : Boolean_22 */{ return LessThanOrEquals_2134/* : UnknownType */(Min_320/* : UnknownType */(x_2969/* : UnknownType */)/* : UnknownType */, And_218/* : UnknownType */(Min_320/* : UnknownType */(other_2971/* : UnknownType */)/* : UnknownType */, GreaterThanOrEquals_2140/* : UnknownType */(Max_323/* : UnknownType */, Max_323/* : UnknownType */(other_2971/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Overlaps_3014 = function (x_2999/* : Interval_24 */, y_3001/* : Interval_24 */) /* : Boolean_22 */{ return Not_224/* : Boolean_22 */(IsEmpty_1911/* : UnknownType */(Clamp_1968/* : UnknownType */(x_2999/* : UnknownType */, y_3001/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Boolean_22 */; };
    static Split_3035 = function (x_3016/* : Interval_24 */, t_3018/* : Unit_30 */) /* : Tuple_7 */{ return Tuple_7_Primitive/* : Tuple_7 */(Left_1947/* : UnknownType */(x_3016/* : UnknownType */, t_3018/* : UnknownType */)/* : UnknownType */, Right_1950/* : UnknownType */(x_3016/* : UnknownType */, t_3018/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static Split_3044 = function (x_3037/* : Interval_24 */) /* : Tuple_7 */{ return Split_1941/* : Tuple_7 */(x_3037/* : UnknownType */, 0.5/* : Float_11 */)/* : Tuple_7 */; };
    static Left_3063 = function (x_3046/* : Interval_24 */, t_3048/* : Unit_30 */) /* : Interval_24 */{ return Tuple_7_Primitive/* : Tuple_7 */(Min_320/* : UnknownType */(x_3046/* : UnknownType */)/* : UnknownType */, Lerp_1914/* : UnknownType */(x_3046/* : UnknownType */, t_3048/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static Right_3082 = function (x_3065/* : Interval_24 */, t_3067/* : Unit_30 */) /* : Interval_24 */{ return Tuple_7_Primitive/* : Tuple_7 */(Lerp_1914/* : UnknownType */(x_3065/* : UnknownType */, t_3067/* : UnknownType */)/* : UnknownType */, Max_323/* : UnknownType */(x_3065/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static MoveTo_3101 = function (x_3084/* : Interval_24 */, v_3086/* : Numerical_16 */) /* : Interval_24 */{ return Tuple_7_Primitive/* : Tuple_7 */(v_3086/* : UnknownType */, Add_182/* : UnknownType */(v_3086/* : UnknownType */, Size_1201/* : UnknownType */(x_3084/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static LeftHalf_3110 = function (x_3103/* : Interval_24 */) /* : Interval_24 */{ return Left_1947/* : Interval_24 */(x_3103/* : UnknownType */, 0.5/* : Float_11 */)/* : Interval_24 */; };
    static RightHalf_3119 = function (x_3112/* : Interval_24 */) /* : Interval_24 */{ return Right_1950/* : Interval_24 */(x_3112/* : UnknownType */, 0.5/* : Float_11 */)/* : Interval_24 */; };
    static HalfSize_3129 = function (x_3121/* : Interval_24 */) /* : Numerical_16 */{ return Half_2039/* : Numerical_16 */(Size_1201/* : UnknownType */(x_3121/* : UnknownType */)/* : UnknownType */)/* : Numerical_16 */; };
    static Recenter_3156 = function (x_3131/* : Interval_24 */, c_3133/* : Numerical_16 */) /* : Interval_24 */{ return Tuple_7_Primitive/* : Tuple_7 */(Subtract_185/* : UnknownType */(c_3133/* : UnknownType */, HalfSize_1962/* : UnknownType */(x_3131/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Add_182/* : UnknownType */(c_3133/* : UnknownType */, HalfSize_1962/* : UnknownType */(x_3131/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static Clamp_3183 = function (x_3158/* : Interval_24 */, y_3160/* : Interval_24 */) /* : Interval_24 */{ return Tuple_7_Primitive/* : Tuple_7 */(Clamp_1968/* : UnknownType */(x_3158/* : UnknownType */, Min_320/* : UnknownType */(y_3160/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Clamp_1968/* : UnknownType */(x_3158/* : UnknownType */, Max_323/* : UnknownType */(y_3160/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static Clamp_3217 = function (x_3185/* : Interval_24 */, value_3187/* : Numerical_16 */) /* : Numerical_16 */{ return LessThan_2132/* : Boolean_22 */(value_3187/* : UnknownType */, Min_320/* : UnknownType */(x_3185/* : UnknownType */)/* : UnknownType */
        ? Min_320/* : UnknownType */(x_3185/* : UnknownType */)/* : UnknownType */
        : GreaterThan_2137/* : UnknownType */(value_3187/* : UnknownType */, Max_323/* : UnknownType */(x_3185/* : UnknownType */)/* : UnknownType */
            ? Max_323/* : UnknownType */(x_3185/* : UnknownType */)/* : UnknownType */
            : value_3187/* : UnknownType */
        )/* : UnknownType */
    )/* : Boolean_22 */; };
    static Within_3244 = function (x_3219/* : Interval_24 */, value_3221/* : Numerical_16 */) /* : Boolean_22 */{ return GreaterThanOrEquals_2140/* : Boolean_22 */(value_3221/* : UnknownType */, And_218/* : UnknownType */(Min_320/* : UnknownType */(x_3219/* : UnknownType */)/* : UnknownType */, LessThanOrEquals_2134/* : UnknownType */(value_3221/* : UnknownType */, Max_323/* : UnknownType */(x_3219/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Boolean_22 */; };
}
class Vector_135_Library
{
    static Sum_3255 = function (v_3246/* : Vector_14 */) /* : Number_29 */{ return Aggregate_2170/* : Any_5 */(v_3246/* : UnknownType */, 0/* : Int_10 */, Add_182/* : UnknownType */)/* : Any_5 */; };
    static SumSquares_3269 = function (v_3257/* : Vector_14 */) /* : Number_29 */{ return Aggregate_2170/* : Any_5 */(Square_1998/* : UnknownType */(v_3257/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */, Add_182/* : UnknownType */)/* : Any_5 */; };
    static LengthSquared_3276 = function (v_3271/* : Vector_14 */) /* : Number_29 */{ return SumSquares_1980/* : Number_29 */(v_3271/* : UnknownType */)/* : Number_29 */; };
    static Length_3286 = function (v_3278/* : Vector_14 */) /* : Number_29 */{ return SquareRoot_1995/* : Numerical_16 */(LengthSquared_1983/* : UnknownType */(v_3278/* : UnknownType */)/* : UnknownType */)/* : Numerical_16 */; };
    static Dot_3300 = function (v1_3288/* : Vector_14 */, v2_3290/* : Vector_14 */) /* : Number_29 */{ return Sum_1977/* : Number_29 */(Multiply_191/* : UnknownType */(v1_3288/* : UnknownType */, v2_3290/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static Normal_3312 = function (v_3302/* : Vector_14 */) /* : Vector_14 */{ return Divide_188/* : Number_29 */(v_3302/* : UnknownType */, Length_1986/* : UnknownType */(v_3302/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
}
class Numerical_136_Library
{
    static SquareRoot_3321 = function (x_3314/* : Numerical_16 */) /* : Numerical_16 */{ return Pow_161/* : Number_29 */(x_3314/* : UnknownType */, 0.5/* : Float_11 */)/* : Number_29 */; };
    static Square_3330 = function (x_3323/* : Numerical_16 */) /* : Numerical_16 */{ return Multiply_191/* : Number_29 */(x_3323/* : UnknownType */, x_3323/* : UnknownType */)/* : Number_29 */; };
    static Clamp_3341 = function (x_3332/* : Numerical_16 */, i_3334/* : Interval_24 */) /* : Numerical_16 */{ return Clamp_1968/* : Interval_24 */(i_3334/* : UnknownType */, x_3332/* : UnknownType */)/* : Interval_24 */; };
    static Clamp_3355 = function (x_3343/* : Numerical_16 */) /* : Numerical_16 */{ return Clamp_1968/* : Interval_24 */(x_3343/* : UnknownType */, Tuple_7_Primitive/* : UnknownType */(0/* : Int_10 */, 1/* : Int_10 */)/* : UnknownType */)/* : Interval_24 */; };
    static PlusOne_3367 = function (x_3357/* : Numerical_16 */) /* : Numerical_16 */{ return Add_182/* : Number_29 */(x_3357/* : UnknownType */, One_304/* : UnknownType */(x_3357/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static MinusOne_3379 = function (x_3369/* : Numerical_16 */) /* : UnknownType */{ return Subtract_185/* : Number_29 */(x_3369/* : UnknownType */, One_304/* : UnknownType */(x_3369/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static FromOne_3391 = function (x_3381/* : Numerical_16 */) /* : Numerical_16 */{ return Subtract_185/* : Number_29 */(One_304/* : UnknownType */(x_3381/* : UnknownType */)/* : UnknownType */, x_3381/* : UnknownType */)/* : Number_29 */; };
    static IsPositive_3400 = function (x_3393/* : Numerical_16 */) /* : Boolean_22 */{ return GreaterThanOrEquals_2140/* : Boolean_22 */(x_3393/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_22 */; };
    static GtZ_3409 = function (x_3402/* : Numerical_16 */) /* : Boolean_22 */{ return GreaterThan_2137/* : Boolean_22 */(x_3402/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_22 */; };
    static LtZ_3418 = function (x_3411/* : Numerical_16 */) /* : Boolean_22 */{ return LessThan_2132/* : Boolean_22 */(x_3411/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_22 */; };
    static GtEqZ_3427 = function (x_3420/* : Numerical_16 */) /* : Boolean_22 */{ return GreaterThanOrEquals_2140/* : Boolean_22 */(x_3420/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_22 */; };
    static LtEqZ_3436 = function (x_3429/* : Numerical_16 */) /* : Boolean_22 */{ return LessThanOrEquals_2134/* : UnknownType */(x_3429/* : UnknownType */, 0/* : Int_10 */)/* : UnknownType */; };
    static IsNegative_3445 = function (x_3438/* : Numerical_16 */) /* : Boolean_22 */{ return LessThan_2132/* : Boolean_22 */(x_3438/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_22 */; };
    static Sign_3473 = function (x_3447/* : Numerical_16 */) /* : Numerical_16 */{ return LtZ_2021/* : UnknownType */(x_3447/* : UnknownType */)/* : UnknownType */
        ? Negative_197/* : Number_29 */(One_304/* : UnknownType */(x_3447/* : UnknownType */)/* : UnknownType */)/* : Number_29 */
        : GtZ_2018/* : UnknownType */(x_3447/* : UnknownType */)/* : UnknownType */
            ? One_304/* : Self_6 */(x_3447/* : UnknownType */)/* : Self_6 */
            : Zero_301/* : Self_6 */(x_3447/* : UnknownType */)/* : Self_6 */

    ; };
    static Abs_3486 = function (x_3475/* : Numerical_16 */) /* : Numerical_16 */{ return LtZ_2021/* : UnknownType */(x_3475/* : UnknownType */)/* : UnknownType */
        ? Negative_197/* : Number_29 */(x_3475/* : UnknownType */)/* : Number_29 */
        : x_3475/* : Numerical_16 */
    ; };
    static Half_3495 = function (x_3488/* : Numerical_16 */) /* : Numerical_16 */{ return Divide_188/* : Number_29 */(x_3488/* : UnknownType */, 2/* : Int_10 */)/* : Number_29 */; };
    static Third_3504 = function (x_3497/* : Numerical_16 */) /* : Numerical_16 */{ return Divide_188/* : Number_29 */(x_3497/* : UnknownType */, 3/* : Int_10 */)/* : Number_29 */; };
    static Quarter_3513 = function (x_3506/* : Numerical_16 */) /* : Numerical_16 */{ return Divide_188/* : Number_29 */(x_3506/* : UnknownType */, 4/* : Int_10 */)/* : Number_29 */; };
    static Fifth_3522 = function (x_3515/* : Numerical_16 */) /* : Numerical_16 */{ return Divide_188/* : Number_29 */(x_3515/* : UnknownType */, 5/* : Int_10 */)/* : Number_29 */; };
    static Sixth_3531 = function (x_3524/* : Numerical_16 */) /* : Numerical_16 */{ return Divide_188/* : Number_29 */(x_3524/* : UnknownType */, 6/* : Int_10 */)/* : Number_29 */; };
    static Seventh_3540 = function (x_3533/* : Numerical_16 */) /* : Numerical_16 */{ return Divide_188/* : Number_29 */(x_3533/* : UnknownType */, 7/* : Int_10 */)/* : Number_29 */; };
    static Eighth_3549 = function (x_3542/* : Numerical_16 */) /* : Numerical_16 */{ return Divide_188/* : Number_29 */(x_3542/* : UnknownType */, 8/* : Int_10 */)/* : Number_29 */; };
    static Ninth_3558 = function (x_3551/* : Numerical_16 */) /* : Numerical_16 */{ return Divide_188/* : Number_29 */(x_3551/* : UnknownType */, 9/* : Int_10 */)/* : Number_29 */; };
    static Tenth_3567 = function (x_3560/* : Numerical_16 */) /* : Numerical_16 */{ return Divide_188/* : Number_29 */(x_3560/* : UnknownType */, 10/* : Int_10 */)/* : Number_29 */; };
    static Sixteenth_3576 = function (x_3569/* : Numerical_16 */) /* : Numerical_16 */{ return Divide_188/* : Number_29 */(x_3569/* : UnknownType */, 16/* : Int_10 */)/* : Number_29 */; };
    static Hundredth_3585 = function (x_3578/* : Numerical_16 */) /* : Numerical_16 */{ return Divide_188/* : Number_29 */(x_3578/* : UnknownType */, 100/* : Int_10 */)/* : Number_29 */; };
    static Thousandth_3594 = function (x_3587/* : Numerical_16 */) /* : Numerical_16 */{ return Divide_188/* : Number_29 */(x_3587/* : UnknownType */, 1000/* : Int_10 */)/* : Number_29 */; };
    static Millionth_3608 = function (x_3596/* : Numerical_16 */) /* : Numerical_16 */{ return Divide_188/* : Number_29 */(x_3596/* : UnknownType */, Divide_188/* : UnknownType */(1000/* : Int_10 */, 1000/* : Int_10 */)/* : UnknownType */)/* : Number_29 */; };
    static Billionth_3627 = function (x_3610/* : Numerical_16 */) /* : Numerical_16 */{ return Divide_188/* : Number_29 */(x_3610/* : UnknownType */, Divide_188/* : UnknownType */(1000/* : Int_10 */, Divide_188/* : UnknownType */(1000/* : Int_10 */, 1000/* : Int_10 */)/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static Hundred_3636 = function (x_3629/* : Numerical_16 */) /* : Numerical_16 */{ return Multiply_191/* : Number_29 */(x_3629/* : UnknownType */, 100/* : Int_10 */)/* : Number_29 */; };
    static Thousand_3645 = function (x_3638/* : Numerical_16 */) /* : Numerical_16 */{ return Multiply_191/* : Number_29 */(x_3638/* : UnknownType */, 1000/* : Int_10 */)/* : Number_29 */; };
    static Million_3659 = function (x_3647/* : Numerical_16 */) /* : Numerical_16 */{ return Multiply_191/* : Number_29 */(x_3647/* : UnknownType */, Multiply_191/* : UnknownType */(1000/* : Int_10 */, 1000/* : Int_10 */)/* : UnknownType */)/* : Number_29 */; };
    static Billion_3678 = function (x_3661/* : Numerical_16 */) /* : Numerical_16 */{ return Multiply_191/* : Number_29 */(x_3661/* : UnknownType */, Multiply_191/* : UnknownType */(1000/* : Int_10 */, Multiply_191/* : UnknownType */(1000/* : Int_10 */, 1000/* : Int_10 */)/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static Twice_3687 = function (x_3680/* : Numerical_16 */) /* : Numerical_16 */{ return Multiply_191/* : Number_29 */(x_3680/* : UnknownType */, 2/* : Int_10 */)/* : Number_29 */; };
    static Thrice_3696 = function (x_3689/* : Numerical_16 */) /* : Numerical_16 */{ return Multiply_191/* : Number_29 */(x_3689/* : UnknownType */, 3/* : Int_10 */)/* : Number_29 */; };
    static SmoothStep_3716 = function (x_3698/* : Numerical_16 */) /* : Numerical_16 */{ return Multiply_191/* : Number_29 */(Square_1998/* : UnknownType */(x_3698/* : UnknownType */)/* : UnknownType */, Subtract_185/* : UnknownType */(3/* : Int_10 */, Twice_2093/* : UnknownType */(x_3698/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static Pow2_3725 = function (x_3718/* : Numerical_16 */) /* : Numerical_16 */{ return Multiply_191/* : Number_29 */(x_3718/* : UnknownType */, x_3718/* : UnknownType */)/* : Number_29 */; };
    static Pow3_3737 = function (x_3727/* : Numerical_16 */) /* : Numerical_16 */{ return Multiply_191/* : Number_29 */(Pow2_2102/* : UnknownType */(x_3727/* : UnknownType */)/* : UnknownType */, x_3727/* : UnknownType */)/* : Number_29 */; };
    static Pow4_3749 = function (x_3739/* : Numerical_16 */) /* : Numerical_16 */{ return Multiply_191/* : Number_29 */(Pow3_2105/* : UnknownType */(x_3739/* : UnknownType */)/* : UnknownType */, x_3739/* : UnknownType */)/* : Number_29 */; };
    static Pow5_3761 = function (x_3751/* : Numerical_16 */) /* : Numerical_16 */{ return Multiply_191/* : Number_29 */(Pow4_2108/* : UnknownType */(x_3751/* : UnknownType */)/* : UnknownType */, x_3751/* : UnknownType */)/* : Number_29 */; };
    static Pi_3763 = function () /* : Number_29 */{ return 3.1415926535897/* : Float_11 */; };
    static AlmostZero_3775 = function (x_3765/* : Numerical_16 */) /* : Boolean_22 */{ return LessThan_2132/* : Boolean_22 */(Abs_2036/* : UnknownType */(x_3765/* : UnknownType */)/* : UnknownType */, 1E-08/* : Float_11 */)/* : Boolean_22 */; };
}
class Angles_137_Library
{
    static Radians_3779 = function (x_3777/* : Number_29 */) /* : Angle_83 */{ return x_3777/* : Number_29 */; };
    static Degrees_3793 = function (x_3781/* : Number_29 */) /* : Angle_83 */{ return Multiply_191/* : Number_29 */(x_3781/* : UnknownType */, Divide_188/* : UnknownType */(Pi_2114/* : UnknownType */, 180/* : Int_10 */)/* : UnknownType */)/* : Number_29 */; };
    static Turns_3807 = function (x_3795/* : Number_29 */) /* : Angle_83 */{ return Multiply_191/* : Number_29 */(x_3795/* : UnknownType */, Multiply_191/* : UnknownType */(2/* : Int_10 */, Pi_2114/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
}
class Comparable_138_Library
{
    static Equals_3823 = function (a_3809/* : Comparable_18 */, b_3811/* : Comparable_18 */) /* : Boolean_22 */{ return Equals_255/* : Boolean_22 */(Compare_252/* : UnknownType */(a_3809/* : UnknownType */, b_3811/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_22 */; };
    static LessThan_3839 = function (a_3825/* : Comparable_18 */, b_3827/* : Comparable_18 */) /* : Boolean_22 */{ return LessThan_2132/* : Boolean_22 */(Compare_252/* : UnknownType */(a_3825/* : UnknownType */, b_3827/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_22 */; };
    static LessThanOrEquals_3855 = function (a_3841/* : Comparable_18 */, b_3843/* : Comparable_18 */) /* : UnknownType */{ return LessThanOrEquals_2134/* : UnknownType */(Compare_252/* : UnknownType */(a_3841/* : UnknownType */, b_3843/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : UnknownType */; };
    static GreaterThan_3871 = function (a_3857/* : Comparable_18 */, b_3859/* : Comparable_18 */) /* : Boolean_22 */{ return GreaterThan_2137/* : Boolean_22 */(Compare_252/* : UnknownType */(a_3857/* : UnknownType */, b_3859/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_22 */; };
    static GreaterThanOrEquals_3887 = function (a_3873/* : Comparable_18 */, b_3875/* : Comparable_18 */) /* : Boolean_22 */{ return GreaterThanOrEquals_2140/* : Boolean_22 */(Compare_252/* : UnknownType */(a_3873/* : UnknownType */, b_3875/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_22 */; };
    static Between_3905 = function (v_3889/* : Value_23 */, a_3891/* : Value_23 */, b_3893/* : Value_23 */) /* : Value_23 */{ return Between_2143/* : Value_23 */(v_3889/* : UnknownType */, Tuple_7_Primitive/* : UnknownType */(a_3891/* : UnknownType */, b_3893/* : UnknownType */)/* : UnknownType */)/* : Value_23 */; };
    static Between_3916 = function (v_3907/* : Value_23 */, i_3909/* : Interval_24 */) /* : Interval_24 */{ return Contains_1932/* : Boolean_22 */(i_3909/* : UnknownType */, v_3907/* : UnknownType */)/* : Boolean_22 */; };
    static Min_3930 = function (a_3918/* : Comparable_18 */, b_3920/* : Comparable_18 */) /* : Comparable_18 */{ return LessThanOrEquals_2134/* : UnknownType */(a_3918/* : UnknownType */, b_3920/* : UnknownType */)/* : UnknownType */
        ? a_3918/* : Comparable_18 */
        : b_3920/* : Comparable_18 */
    ; };
    static Max_3944 = function (a_3932/* : Comparable_18 */, b_3934/* : Comparable_18 */) /* : Comparable_18 */{ return GreaterThanOrEquals_2140/* : UnknownType */(a_3932/* : UnknownType */, b_3934/* : UnknownType */)/* : UnknownType */
        ? a_3932/* : Comparable_18 */
        : b_3934/* : Comparable_18 */
    ; };
}
class Equatable_139_Library
{
    static NotEquals_3958 = function (x_3946/* : Equatable_19 */, y_3948/* : Equatable_19 */) /* : Boolean_22 */{ return Not_224/* : Boolean_22 */(Equals_255/* : UnknownType */(x_3946/* : UnknownType */, y_3948/* : UnknownType */)/* : UnknownType */)/* : Boolean_22 */; };
}
class Array_140_Library
{
    static Map_3983 = function (xs_3960/* : Array_25 */, f_3962/* : Function_4 */) /* : Array_25 */{ return Map_2158/* : Array_25 */(Count_27_Type/* : UnknownType */(xs_3960/* : UnknownType */)/* : UnknownType */, function (i_3968/* : UnknownType */) /* : Lambda_3 */{ return f_3962/* : UnknownType */(At_243/* : UnknownType */(xs_3960/* : UnknownType */, i_3968/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Array_25 */; };
    static Zip_4015 = function (xs_3985/* : Array_25 */, ys_3987/* : Array_25 */, f_3989/* : Function_4 */) /* : Array_25 */{ return Tuple_7_Primitive/* : Tuple_7 */(Count_27_Type/* : UnknownType */(xs_3985/* : UnknownType */)/* : UnknownType */, function (i_3995/* : UnknownType */) /* : Lambda_3 */{ return f_3989/* : UnknownType */(At_243/* : UnknownType */(i_3995/* : UnknownType */)/* : UnknownType */, At_243/* : UnknownType */(ys_3987/* : UnknownType */, i_3995/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Tuple_7 */; };
    static Skip_4042 = function (xs_4017/* : Array_25 */, n_4019/* : Count_27 */) /* : Array_25 */{ return Tuple_7_Primitive/* : Tuple_7 */(Subtract_185/* : UnknownType */(Count_27_Type/* : UnknownType */, n_4019/* : UnknownType */)/* : UnknownType */, function (i_4027/* : UnknownType */) /* : Lambda_3 */{ return At_243/* : UnknownType */(Subtract_185/* : UnknownType */(i_4027/* : UnknownType */, n_4019/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Tuple_7 */; };
    static Take_4056 = function (xs_4044/* : Array_25 */, n_4046/* : Count_27 */) /* : Array_25 */{ return Array_140_Library/* : Array_140 */(n_4046/* : UnknownType */, function (i_4049/* : UnknownType */) /* : Lambda_3 */{ return At_243/* : UnknownType */; })/* : Array_140 */; };
    static Aggregate_4081 = function (xs_4058/* : Array_25 */, init_4060/* : Any_5 */, f_4062/* : Function_4 */) /* : Any_5 */{ return IsEmpty_1911/* : UnknownType */(xs_4058/* : UnknownType */)/* : UnknownType */
        ? init_4060/* : Any_5 */
        : f_4062/* : Function_4 */(init_4060/* : UnknownType */, f_4062/* : UnknownType */(Rest_2173/* : UnknownType */(xs_4058/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Function_4 */
    ; };
    static Rest_4090 = function (xs_4083/* : Array_25 */) /* : Array_25 */{ return Skip_2164/* : Array_25 */(xs_4083/* : UnknownType */, 1/* : Int_10 */)/* : Array_25 */; };
    static IsEmpty_4102 = function (xs_4092/* : Array_25 */) /* : Boolean_22 */{ return Equals_255/* : Boolean_22 */(Count_27_Type/* : UnknownType */(xs_4092/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_22 */; };
    static First_4111 = function (xs_4104/* : Array_25 */) /* : Any_5 */{ return At_243/* : T_238 */(xs_4104/* : UnknownType */, 0/* : Int_10 */)/* : T_238 */; };
    static Last_4128 = function (xs_4113/* : Array_25 */) /* : Any_5 */{ return At_243/* : T_238 */(xs_4113/* : UnknownType */, Subtract_185/* : UnknownType */(Count_27_Type/* : UnknownType */(xs_4113/* : UnknownType */)/* : UnknownType */, 1/* : Int_10 */)/* : UnknownType */)/* : T_238 */; };
    static Slice_4146 = function (xs_4130/* : Array_25 */, from_4132/* : Index_28 */, count_4134/* : Count_27 */) /* : Array_25 */{ return Take_2167/* : Array_25 */(Skip_2164/* : UnknownType */(xs_4130/* : UnknownType */, from_4132/* : UnknownType */)/* : UnknownType */, count_4134/* : UnknownType */)/* : Array_25 */; };
    static Join_4190 = function (xs_4148/* : Array_25 */, sep_4150/* : String_8 */) /* : String_8 */{ return IsEmpty_1911/* : UnknownType */(xs_4148/* : UnknownType */)/* : UnknownType */
        ? ""/* : String_8 */
        : Add_182/* : Number_29 */(ToString_316/* : UnknownType */(First_2179/* : UnknownType */(xs_4148/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Aggregate_2170/* : UnknownType */(Rest_2173/* : UnknownType */(xs_4148/* : UnknownType */)/* : UnknownType */, ""/* : String_8 */, function (acc_4171/* : UnknownType */, cur_4172/* : UnknownType */) /* : Lambda_3 */{ return Interpolate_173/* : UnknownType */(acc_4171/* : UnknownType */, sep_4150/* : UnknownType */, cur_4172/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */)/* : Number_29 */
    ; };
    static All_4219 = function (xs_4192/* : Array_25 */, f_4194/* : Function_4 */) /* : Boolean_22 */{ return IsEmpty_1911/* : UnknownType */(xs_4192/* : UnknownType */)/* : UnknownType */
        ? True/* : Bool_9 */
        : And_218/* : Boolean_22 */(f_4194/* : UnknownType */(First_2179/* : UnknownType */(xs_4192/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, f_4194/* : UnknownType */(Rest_2173/* : UnknownType */(xs_4192/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Boolean_22 */
    ; };
}
class Easings_141_Library
{
    static BlendEaseFunc_4271 = function (p_4221/* : Number_29 */, easeIn_4223/* : Function_4 */, easeOut_4225/* : Function_4 */) /* : Number_29 */{ return LessThan_2132/* : Boolean_22 */(p_4221/* : UnknownType */, 0.5/* : Float_11 */
        ? Multiply_191/* : UnknownType */(0.5/* : Float_11 */, easeIn_4223/* : UnknownType */(Multiply_191/* : UnknownType */(p_4221/* : UnknownType */, 2/* : Int_10 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
        : Multiply_191/* : UnknownType */(0.5/* : Float_11 */, Add_182/* : UnknownType */(easeOut_4225/* : UnknownType */(Multiply_191/* : UnknownType */(p_4221/* : UnknownType */, Subtract_185/* : UnknownType */(2/* : Int_10 */, 1/* : Int_10 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, 0.5/* : Float_11 */)/* : UnknownType */)/* : UnknownType */
    )/* : Boolean_22 */; };
    static InvertEaseFunc_4290 = function (p_4273/* : Number_29 */, easeIn_4275/* : Function_4 */) /* : Number_29 */{ return Subtract_185/* : Number_29 */(1/* : Int_10 */, easeIn_4275/* : UnknownType */(Subtract_185/* : UnknownType */(1/* : Int_10 */, p_4273/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static Linear_4294 = function (p_4292/* : Number_29 */) /* : Number_29 */{ return p_4292/* : Number_29 */; };
    static QuadraticEaseIn_4301 = function (p_4296/* : Number_29 */) /* : Number_29 */{ return Pow2_2102/* : Numerical_16 */(p_4296/* : UnknownType */)/* : Numerical_16 */; };
    static QuadraticEaseOut_4310 = function (p_4303/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2197/* : Number_29 */(p_4303/* : UnknownType */, QuadraticEaseIn_2203/* : UnknownType */)/* : Number_29 */; };
    static QuadraticEaseInOut_4321 = function (p_4312/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2194/* : Number_29 */(p_4312/* : UnknownType */, QuadraticEaseIn_2203/* : UnknownType */, QuadraticEaseOut_2206/* : UnknownType */)/* : Number_29 */; };
    static CubicEaseIn_4328 = function (p_4323/* : Number_29 */) /* : Number_29 */{ return Pow3_2105/* : Numerical_16 */(p_4323/* : UnknownType */)/* : Numerical_16 */; };
    static CubicEaseOut_4337 = function (p_4330/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2197/* : Number_29 */(p_4330/* : UnknownType */, CubicEaseIn_2212/* : UnknownType */)/* : Number_29 */; };
    static CubicEaseInOut_4348 = function (p_4339/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2194/* : Number_29 */(p_4339/* : UnknownType */, CubicEaseIn_2212/* : UnknownType */, CubicEaseOut_2215/* : UnknownType */)/* : Number_29 */; };
    static QuarticEaseIn_4355 = function (p_4350/* : Number_29 */) /* : Number_29 */{ return Pow4_2108/* : Numerical_16 */(p_4350/* : UnknownType */)/* : Numerical_16 */; };
    static QuarticEaseOut_4364 = function (p_4357/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2197/* : Number_29 */(p_4357/* : UnknownType */, QuarticEaseIn_2221/* : UnknownType */)/* : Number_29 */; };
    static QuarticEaseInOut_4375 = function (p_4366/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2194/* : Number_29 */(p_4366/* : UnknownType */, QuarticEaseIn_2221/* : UnknownType */, QuarticEaseOut_2224/* : UnknownType */)/* : Number_29 */; };
    static QuinticEaseIn_4382 = function (p_4377/* : Number_29 */) /* : Number_29 */{ return Pow5_2111/* : Numerical_16 */(p_4377/* : UnknownType */)/* : Numerical_16 */; };
    static QuinticEaseOut_4391 = function (p_4384/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2197/* : Number_29 */(p_4384/* : UnknownType */, QuinticEaseIn_2230/* : UnknownType */)/* : Number_29 */; };
    static QuinticEaseInOut_4402 = function (p_4393/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2194/* : Number_29 */(p_4393/* : UnknownType */, QuinticEaseIn_2230/* : UnknownType */, QuinticEaseOut_2233/* : UnknownType */)/* : Number_29 */; };
    static SineEaseIn_4411 = function (p_4404/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2197/* : Number_29 */(p_4404/* : UnknownType */, SineEaseOut_2242/* : UnknownType */)/* : Number_29 */; };
    static SineEaseOut_4424 = function (p_4413/* : Number_29 */) /* : Number_29 */{ return Sin_146/* : Number_29 */(Turns_2126/* : UnknownType */(Quarter_2045/* : UnknownType */(p_4413/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static SineEaseInOut_4435 = function (p_4426/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2194/* : Number_29 */(p_4426/* : UnknownType */, SineEaseIn_2239/* : UnknownType */, SineEaseOut_2242/* : UnknownType */)/* : Number_29 */; };
    static CircularEaseIn_4451 = function (p_4437/* : Number_29 */) /* : Number_29 */{ return FromOne_2012/* : Numerical_16 */(SquareRoot_1995/* : UnknownType */(FromOne_2012/* : UnknownType */(Pow2_2102/* : UnknownType */(p_4437/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Numerical_16 */; };
    static CircularEaseOut_4460 = function (p_4453/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2197/* : Number_29 */(p_4453/* : UnknownType */, CircularEaseIn_2248/* : UnknownType */)/* : Number_29 */; };
    static CircularEaseInOut_4471 = function (p_4462/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2194/* : Number_29 */(p_4462/* : UnknownType */, CircularEaseIn_2248/* : UnknownType */, CircularEaseOut_2251/* : UnknownType */)/* : Number_29 */; };
    static ExponentialEaseIn_4494 = function (p_4473/* : Number_29 */) /* : Number_29 */{ return AlmostZero_2117/* : UnknownType */(p_4473/* : UnknownType */)/* : UnknownType */
        ? p_4473/* : Number_29 */
        : Pow_161/* : Number_29 */(2/* : Int_10 */, Multiply_191/* : UnknownType */(10/* : Int_10 */, MinusOne_2009/* : UnknownType */(p_4473/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_29 */
    ; };
    static ExponentialEaseOut_4503 = function (p_4496/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2197/* : Number_29 */(p_4496/* : UnknownType */, ExponentialEaseIn_2257/* : UnknownType */)/* : Number_29 */; };
    static ExponentialEaseInOut_4514 = function (p_4505/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2194/* : Number_29 */(p_4505/* : UnknownType */, ExponentialEaseIn_2257/* : UnknownType */, ExponentialEaseOut_2260/* : UnknownType */)/* : Number_29 */; };
    static ElasticEaseIn_4553 = function (p_4516/* : Number_29 */) /* : Number_29 */{ return Multiply_191/* : Number_29 */(13/* : Int_10 */, Multiply_191/* : UnknownType */(Turns_2126/* : UnknownType */(Quarter_2045/* : UnknownType */(p_4516/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Sin_146/* : UnknownType */(Radians_1229/* : UnknownType */(Pow_161/* : UnknownType */(2/* : Int_10 */, Multiply_191/* : UnknownType */(10/* : Int_10 */, MinusOne_2009/* : UnknownType */(p_4516/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static ElasticEaseOut_4562 = function (p_4555/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2197/* : Number_29 */(p_4555/* : UnknownType */, ElasticEaseIn_2266/* : UnknownType */)/* : Number_29 */; };
    static ElasticEaseInOut_4573 = function (p_4564/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2194/* : Number_29 */(p_4564/* : UnknownType */, ElasticEaseIn_2266/* : UnknownType */, ElasticEaseOut_2269/* : UnknownType */)/* : Number_29 */; };
    static BackEaseIn_4599 = function (p_4575/* : Number_29 */) /* : Number_29 */{ return Subtract_185/* : Number_29 */(Pow3_2105/* : UnknownType */(p_4575/* : UnknownType */)/* : UnknownType */, Multiply_191/* : UnknownType */(p_4575/* : UnknownType */, Sin_146/* : UnknownType */(Turns_2126/* : UnknownType */(Half_2039/* : UnknownType */(p_4575/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static BackEaseOut_4608 = function (p_4601/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2197/* : Number_29 */(p_4601/* : UnknownType */, BackEaseIn_2275/* : UnknownType */)/* : Number_29 */; };
    static BackEaseInOut_4619 = function (p_4610/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2194/* : Number_29 */(p_4610/* : UnknownType */, BackEaseIn_2275/* : UnknownType */, BackEaseOut_2278/* : UnknownType */)/* : Number_29 */; };
    static BounceEaseIn_4628 = function (p_4621/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2197/* : Number_29 */(p_4621/* : UnknownType */, BounceEaseOut_2287/* : UnknownType */)/* : Number_29 */; };
    static BounceEaseOut_4798 = function (p_4630/* : Number_29 */) /* : Number_29 */{ return LessThan_2132/* : UnknownType */(p_4630/* : UnknownType */, Divide_188/* : UnknownType */(4/* : Int_10 */, 11/* : Float_11 */)/* : UnknownType */)/* : UnknownType */
        ? Multiply_191/* : Number_29 */(121/* : Float_11 */, Divide_188/* : UnknownType */(Pow2_2102/* : UnknownType */(p_4630/* : UnknownType */)/* : UnknownType */, 16/* : Float_11 */)/* : UnknownType */)/* : Number_29 */
        : LessThan_2132/* : UnknownType */(p_4630/* : UnknownType */, Divide_188/* : UnknownType */(8/* : Int_10 */, 11/* : Float_11 */)/* : UnknownType */)/* : UnknownType */
            ? Divide_188/* : Number_29 */(363/* : Float_11 */, Multiply_191/* : UnknownType */(40/* : Float_11 */, Subtract_185/* : UnknownType */(Pow2_2102/* : UnknownType */(p_4630/* : UnknownType */)/* : UnknownType */, Divide_188/* : UnknownType */(99/* : Float_11 */, Multiply_191/* : UnknownType */(10/* : Float_11 */, Add_182/* : UnknownType */(p_4630/* : UnknownType */, Divide_188/* : UnknownType */(17/* : Float_11 */, 5/* : Float_11 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_29 */
            : LessThan_2132/* : UnknownType */(p_4630/* : UnknownType */, Divide_188/* : UnknownType */(9/* : Int_10 */, 10/* : Float_11 */)/* : UnknownType */)/* : UnknownType */
                ? Divide_188/* : Number_29 */(4356/* : Float_11 */, Multiply_191/* : UnknownType */(361/* : Float_11 */, Subtract_185/* : UnknownType */(Pow2_2102/* : UnknownType */(p_4630/* : UnknownType */)/* : UnknownType */, Divide_188/* : UnknownType */(35442/* : Float_11 */, Multiply_191/* : UnknownType */(1805/* : Float_11 */, Add_182/* : UnknownType */(p_4630/* : UnknownType */, Divide_188/* : UnknownType */(16061/* : Float_11 */, 1805/* : Float_11 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_29 */
                : Divide_188/* : Number_29 */(54/* : Float_11 */, Multiply_191/* : UnknownType */(5/* : Float_11 */, Subtract_185/* : UnknownType */(Pow2_2102/* : UnknownType */(p_4630/* : UnknownType */)/* : UnknownType */, Divide_188/* : UnknownType */(513/* : Float_11 */, Multiply_191/* : UnknownType */(25/* : Float_11 */, Add_182/* : UnknownType */(p_4630/* : UnknownType */, Divide_188/* : UnknownType */(268/* : Float_11 */, 25/* : Float_11 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_29 */


    ; };
    static BounceEaseInOut_4809 = function (p_4800/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2194/* : Number_29 */(p_4800/* : UnknownType */, BounceEaseIn_2284/* : UnknownType */, BounceEaseOut_2287/* : UnknownType */)/* : Number_29 */; };
}
class Vector_14_Concept
{
    constructor(self) { this.Self = self; };
    static Count_2422 = function (v_2414/* : Vector_14 */) /* : Count_27 */{ return Count_27_Type/* : Count_27 */(FieldTypes_228/* : UnknownType */(Self_6_Primitive/* : UnknownType */)/* : UnknownType */)/* : Count_27 */; };
    static At_2436 = function (v_2424/* : Vector_14 */, n_2426/* : Index_28 */) /* : T_238 */{ return At_243/* : T_238 */(FieldValues_236/* : UnknownType */(v_2424/* : UnknownType */)/* : UnknownType */, n_2426/* : UnknownType */)/* : T_238 */; };
}
class Measure_15_Concept
{
    constructor(self) { this.Self = self; };
    static Value_2448 = function (x_2438/* : Self_6 */) /* : Number_29 */{ return At_243/* : T_238 */(FieldValues_236/* : UnknownType */(x_2438/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : T_238 */; };
}
class Numerical_16_Concept
{
    constructor(self) { this.Self = self; };
}
class Magnitude_17_Concept
{
    constructor(self) { this.Self = self; };
    static Magnitude_2464 = function (x_2450/* : Self_6 */) /* : Number_29 */{ return SquareRoot_1995/* : Numerical_16 */(Sum_1977/* : UnknownType */(Square_1998/* : UnknownType */(FieldValues_236/* : UnknownType */(x_2450/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Numerical_16 */; };
}
class Comparable_18_Concept
{
    constructor(self) { this.Self = self; };
    static Compare_2501 = function (a_2466/* : Self_6 */, b_2468/* : Self_6 */) /* : Integer_26 */{ return LessThan_2132/* : Boolean_22 */(Magnitude_249/* : UnknownType */(a_2466/* : UnknownType */)/* : UnknownType */, Magnitude_249/* : UnknownType */(b_2468/* : UnknownType */)/* : UnknownType */
        ? Negative_197/* : UnknownType */(1/* : Int_10 */)/* : UnknownType */
        : GreaterThan_2137/* : UnknownType */(Magnitude_249/* : UnknownType */(a_2466/* : UnknownType */)/* : UnknownType */, Magnitude_249/* : UnknownType */(b_2468/* : UnknownType */)/* : UnknownType */
            ? 1/* : Int_10 */
            : 0/* : Int_10 */
        )/* : UnknownType */
    )/* : Boolean_22 */; };
}
class Equatable_19_Concept
{
    constructor(self) { this.Self = self; };
    static Equals_2521 = function (a_2503/* : Self_6 */, b_2505/* : Self_6 */) /* : Boolean_22 */{ return All_2191/* : Boolean_22 */(Equals_255/* : UnknownType */(FieldValues_236/* : UnknownType */(a_2503/* : UnknownType */)/* : UnknownType */, FieldValues_236/* : UnknownType */(b_2505/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Boolean_22 */; };
}
class Arithmetic_20_Concept
{
    constructor(self) { this.Self = self; };
    static Add_2538 = function (self_2523/* : Self_6 */, other_2525/* : Self_6 */) /* : Self_6 */{ return Add_182/* : Number_29 */(FieldValues_236/* : UnknownType */(self_2523/* : UnknownType */)/* : UnknownType */, FieldValues_236/* : UnknownType */(other_2525/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static Negative_2548 = function (self_2540/* : Self_6 */) /* : Self_6 */{ return Negative_197/* : Number_29 */(FieldValues_236/* : UnknownType */(self_2540/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static Reciprocal_2558 = function (self_2550/* : Self_6 */) /* : Self_6 */{ return Reciprocal_264/* : Self_6 */(FieldValues_236/* : UnknownType */(self_2550/* : UnknownType */)/* : UnknownType */)/* : Self_6 */; };
    static Multiply_2575 = function (self_2560/* : Self_6 */, other_2562/* : Self_6 */) /* : Self_6 */{ return Add_182/* : Number_29 */(FieldValues_236/* : UnknownType */(self_2560/* : UnknownType */)/* : UnknownType */, FieldValues_236/* : UnknownType */(other_2562/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static Divide_2592 = function (self_2577/* : Self_6 */, other_2579/* : Self_6 */) /* : Self_6 */{ return Divide_188/* : Number_29 */(FieldValues_236/* : UnknownType */(self_2577/* : UnknownType */)/* : UnknownType */, FieldValues_236/* : UnknownType */(other_2579/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static Modulo_2609 = function (self_2594/* : Self_6 */, other_2596/* : Self_6 */) /* : Self_6 */{ return Modulo_194/* : Number_29 */(FieldValues_236/* : UnknownType */(self_2594/* : UnknownType */)/* : UnknownType */, FieldValues_236/* : UnknownType */(other_2596/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
}
class ScalarArithmetic_21_Concept
{
    constructor(self) { this.Self = self; };
    static Add_2623 = function (self_2611/* : Self_6 */, scalar_2613/* : T_325 */) /* : Self_6 */{ return Add_182/* : Number_29 */(FieldValues_236/* : UnknownType */(self_2611/* : UnknownType */)/* : UnknownType */, scalar_2613/* : UnknownType */)/* : Number_29 */; };
    static Subtract_2637 = function (self_2625/* : Self_6 */, scalar_2627/* : T_325 */) /* : Self_6 */{ return Add_182/* : Number_29 */(self_2625/* : UnknownType */, Negative_197/* : UnknownType */(scalar_2627/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static Multiply_2651 = function (self_2639/* : Self_6 */, scalar_2641/* : T_325 */) /* : Self_6 */{ return Multiply_191/* : Number_29 */(FieldValues_236/* : UnknownType */(self_2639/* : UnknownType */)/* : UnknownType */, scalar_2641/* : UnknownType */)/* : Number_29 */; };
    static Divide_2665 = function (self_2653/* : Self_6 */, scalar_2655/* : T_325 */) /* : Self_6 */{ return Multiply_191/* : Number_29 */(self_2653/* : UnknownType */, Reciprocal_264/* : UnknownType */(scalar_2655/* : UnknownType */)/* : UnknownType */)/* : Number_29 */; };
    static Modulo_2679 = function (self_2667/* : Self_6 */, scalar_2669/* : T_325 */) /* : Self_6 */{ return Modulo_194/* : Number_29 */(FieldValues_236/* : UnknownType */(self_2667/* : UnknownType */)/* : UnknownType */, scalar_2669/* : UnknownType */)/* : Number_29 */; };
}
class Boolean_22_Concept
{
    constructor(self) { this.Self = self; };
    static And_2696 = function (a_2681/* : Self_6 */, b_2683/* : Self_6 */) /* : Self_6 */{ return And_218/* : Boolean_22 */(FieldValues_236/* : UnknownType */(a_2681/* : UnknownType */)/* : UnknownType */, FieldValues_236/* : UnknownType */(b_2683/* : UnknownType */)/* : UnknownType */)/* : Boolean_22 */; };
    static Or_2713 = function (a_2698/* : Self_6 */, b_2700/* : Self_6 */) /* : Self_6 */{ return Or_221/* : Boolean_22 */(FieldValues_236/* : UnknownType */(a_2698/* : UnknownType */)/* : UnknownType */, FieldValues_236/* : UnknownType */(b_2700/* : UnknownType */)/* : UnknownType */)/* : Boolean_22 */; };
    static Not_2723 = function (a_2715/* : Self_6 */) /* : Self_6 */{ return Not_224/* : Boolean_22 */(FieldValues_236/* : UnknownType */(a_2715/* : UnknownType */)/* : UnknownType */)/* : Boolean_22 */; };
}
class Value_23_Concept
{
    constructor(self) { this.Self = self; };
    static Zero_2731 = function () /* : Self_6 */{ return Zero_301/* : Self_6 */(FieldTypes_228/* : UnknownType */(Self_6_Primitive/* : UnknownType */)/* : UnknownType */)/* : Self_6 */; };
    static One_2739 = function () /* : Self_6 */{ return One_304/* : Self_6 */(FieldTypes_228/* : UnknownType */(Self_6_Primitive/* : UnknownType */)/* : UnknownType */)/* : Self_6 */; };
    static Default_2747 = function () /* : Self_6 */{ return Default_307/* : Self_6 */(FieldTypes_228/* : UnknownType */(Self_6_Primitive/* : UnknownType */)/* : UnknownType */)/* : Self_6 */; };
    static MinValue_2755 = function () /* : Self_6 */{ return MinValue_310/* : Self_6 */(FieldTypes_228/* : UnknownType */(Self_6_Primitive/* : UnknownType */)/* : UnknownType */)/* : Self_6 */; };
    static MaxValue_2763 = function () /* : Self_6 */{ return MaxValue_313/* : Self_6 */(FieldTypes_228/* : UnknownType */(Self_6_Primitive/* : UnknownType */)/* : UnknownType */)/* : Self_6 */; };
    static ToString_2775 = function (x_2765/* : Self_6 */) /* : String_8 */{ return Join_2188/* : String_8 */(FieldValues_236/* : UnknownType */(x_2765/* : UnknownType */)/* : UnknownType */, ","/* : String_8 */)/* : String_8 */; };
}
class Interval_24_Concept
{
    constructor(self) { this.Self = self; };
    static Min_2778 = function (x_2777/* : Self_6 */) /* : T_318 */{ return null; };
    static Max_2781 = function (x_2780/* : Self_6 */) /* : T_318 */{ return null; };
}
class Array_25_Concept
{
    constructor(self) { this.Self = self; };
    static Count_2784 = function (xs_2783/* : Self_6 */) /* : Count_27 */{ return null; };
    static At_2789 = function (xs_2786/* : Self_6 */, n_2788/* : Index_28 */) /* : T_325 */{ return null; };
}
class Integer_26_Type
{
    constructor(Value_333)
    {
        // field initialization 
        this.Value_333 = Value_333;
        this.Zero_2731 = Integer_26_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Integer_26_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Integer_26_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Integer_26_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Integer_26_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Integer_26_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = Integer_26_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = Integer_26_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = Integer_26_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = Integer_26_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = Integer_26_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = Integer_26_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = Integer_26_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Integer_26_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Integer_26_Type.Magnitude_17_Concept.Magnitude_2464;
    }
    // field accessors
    static Value_333 = function(self) { return self.Value_333; }
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
    constructor(Value_340)
    {
        // field initialization 
        this.Value_340 = Value_340;
        this.Zero_2731 = Count_27_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Count_27_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Count_27_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Count_27_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Count_27_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Count_27_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = Count_27_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = Count_27_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = Count_27_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = Count_27_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = Count_27_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = Count_27_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = Count_27_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Count_27_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Count_27_Type.Magnitude_17_Concept.Magnitude_2464;
    }
    // field accessors
    static Value_340 = function(self) { return self.Value_340; }
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
    constructor(Value_347)
    {
        // field initialization 
        this.Value_347 = Value_347;
        this.Zero_2731 = Index_28_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Index_28_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Index_28_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Index_28_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Index_28_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Index_28_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Value_347 = function(self) { return self.Value_347; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Index_28_Type);
    static Implements = [Value_23_Concept];
}
class Number_29_Type
{
    constructor(Value_354)
    {
        // field initialization 
        this.Value_354 = Value_354;
        this.Zero_2731 = Number_29_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Number_29_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Number_29_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Number_29_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Number_29_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Number_29_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = Number_29_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = Number_29_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = Number_29_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = Number_29_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = Number_29_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = Number_29_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = Number_29_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Number_29_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Number_29_Type.Magnitude_17_Concept.Magnitude_2464;
    }
    // field accessors
    static Value_354 = function(self) { return self.Value_354; }
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
    constructor(Value_361)
    {
        // field initialization 
        this.Value_361 = Value_361;
        this.Zero_2731 = Unit_30_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Unit_30_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Unit_30_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Unit_30_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Unit_30_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Unit_30_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = Unit_30_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = Unit_30_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = Unit_30_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = Unit_30_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = Unit_30_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = Unit_30_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = Unit_30_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Unit_30_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Unit_30_Type.Magnitude_17_Concept.Magnitude_2464;
    }
    // field accessors
    static Value_361 = function(self) { return self.Value_361; }
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
    constructor(Value_368)
    {
        // field initialization 
        this.Value_368 = Value_368;
        this.Zero_2731 = Percent_31_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Percent_31_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Percent_31_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Percent_31_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Percent_31_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Percent_31_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = Percent_31_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = Percent_31_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = Percent_31_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = Percent_31_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = Percent_31_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = Percent_31_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = Percent_31_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Percent_31_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Percent_31_Type.Magnitude_17_Concept.Magnitude_2464;
    }
    // field accessors
    static Value_368 = function(self) { return self.Value_368; }
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
    constructor(X_375, Y_382, Z_389, W_396)
    {
        // field initialization 
        this.X_375 = X_375;
        this.Y_382 = Y_382;
        this.Z_389 = Z_389;
        this.W_396 = W_396;
        this.Zero_2731 = Quaternion_32_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Quaternion_32_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Quaternion_32_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Quaternion_32_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Quaternion_32_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Quaternion_32_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static X_375 = function(self) { return self.X_375; }
    static Y_382 = function(self) { return self.Y_382; }
    static Z_389 = function(self) { return self.Z_389; }
    static W_396 = function(self) { return self.W_396; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Quaternion_32_Type);
    static Implements = [Value_23_Concept];
}
class Unit2D_33_Type
{
    constructor(X_403, Y_410)
    {
        // field initialization 
        this.X_403 = X_403;
        this.Y_410 = Y_410;
        this.Zero_2731 = Unit2D_33_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Unit2D_33_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Unit2D_33_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Unit2D_33_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Unit2D_33_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Unit2D_33_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static X_403 = function(self) { return self.X_403; }
    static Y_410 = function(self) { return self.Y_410; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Unit2D_33_Type);
    static Implements = [Value_23_Concept];
}
class Unit3D_34_Type
{
    constructor(X_417, Y_424, Z_431)
    {
        // field initialization 
        this.X_417 = X_417;
        this.Y_424 = Y_424;
        this.Z_431 = Z_431;
        this.Zero_2731 = Unit3D_34_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Unit3D_34_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Unit3D_34_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Unit3D_34_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Unit3D_34_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Unit3D_34_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static X_417 = function(self) { return self.X_417; }
    static Y_424 = function(self) { return self.Y_424; }
    static Z_431 = function(self) { return self.Z_431; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Unit3D_34_Type);
    static Implements = [Value_23_Concept];
}
class Direction3D_35_Type
{
    constructor(Value_438)
    {
        // field initialization 
        this.Value_438 = Value_438;
        this.Zero_2731 = Direction3D_35_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Direction3D_35_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Direction3D_35_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Direction3D_35_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Direction3D_35_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Direction3D_35_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Value_438 = function(self) { return self.Value_438; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Direction3D_35_Type);
    static Implements = [Value_23_Concept];
}
class AxisAngle_36_Type
{
    constructor(Axis_445, Angle_452)
    {
        // field initialization 
        this.Axis_445 = Axis_445;
        this.Angle_452 = Angle_452;
        this.Zero_2731 = AxisAngle_36_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = AxisAngle_36_Type.Value_23_Concept.One_2739;
        this.Default_2747 = AxisAngle_36_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = AxisAngle_36_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = AxisAngle_36_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = AxisAngle_36_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Axis_445 = function(self) { return self.Axis_445; }
    static Angle_452 = function(self) { return self.Angle_452; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(AxisAngle_36_Type);
    static Implements = [Value_23_Concept];
}
class EulerAngles_37_Type
{
    constructor(Yaw_459, Pitch_466, Roll_473)
    {
        // field initialization 
        this.Yaw_459 = Yaw_459;
        this.Pitch_466 = Pitch_466;
        this.Roll_473 = Roll_473;
        this.Zero_2731 = EulerAngles_37_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = EulerAngles_37_Type.Value_23_Concept.One_2739;
        this.Default_2747 = EulerAngles_37_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = EulerAngles_37_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = EulerAngles_37_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = EulerAngles_37_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Yaw_459 = function(self) { return self.Yaw_459; }
    static Pitch_466 = function(self) { return self.Pitch_466; }
    static Roll_473 = function(self) { return self.Roll_473; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(EulerAngles_37_Type);
    static Implements = [Value_23_Concept];
}
class Rotation3D_38_Type
{
    constructor(Quaternion_480)
    {
        // field initialization 
        this.Quaternion_480 = Quaternion_480;
        this.Zero_2731 = Rotation3D_38_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Rotation3D_38_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Rotation3D_38_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Rotation3D_38_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Rotation3D_38_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Rotation3D_38_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Quaternion_480 = function(self) { return self.Quaternion_480; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Rotation3D_38_Type);
    static Implements = [Value_23_Concept];
}
class Vector2D_39_Type
{
    constructor(X_487, Y_494)
    {
        // field initialization 
        this.X_487 = X_487;
        this.Y_494 = Y_494;
        this.Count_2784 = Vector2D_39_Type.Array_25_Concept.Count_2784;
        this.At_2789 = Vector2D_39_Type.Array_25_Concept.At_2789;
        this.Zero_2731 = Vector2D_39_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Vector2D_39_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Vector2D_39_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Vector2D_39_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Vector2D_39_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Vector2D_39_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = Vector2D_39_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = Vector2D_39_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = Vector2D_39_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = Vector2D_39_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = Vector2D_39_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = Vector2D_39_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = Vector2D_39_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Vector2D_39_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Vector2D_39_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Count_2422 = Vector2D_39_Type.Vector_14_Concept.Count_2422;
        this.At_2436 = Vector2D_39_Type.Vector_14_Concept.At_2436;
    }
    // field accessors
    static X_487 = function(self) { return self.X_487; }
    static Y_494 = function(self) { return self.Y_494; }
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
    constructor(X_501, Y_508, Z_515)
    {
        // field initialization 
        this.X_501 = X_501;
        this.Y_508 = Y_508;
        this.Z_515 = Z_515;
        this.Count_2784 = Vector3D_40_Type.Array_25_Concept.Count_2784;
        this.At_2789 = Vector3D_40_Type.Array_25_Concept.At_2789;
        this.Zero_2731 = Vector3D_40_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Vector3D_40_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Vector3D_40_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Vector3D_40_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Vector3D_40_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Vector3D_40_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = Vector3D_40_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = Vector3D_40_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = Vector3D_40_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = Vector3D_40_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = Vector3D_40_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = Vector3D_40_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = Vector3D_40_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Vector3D_40_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Vector3D_40_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Count_2422 = Vector3D_40_Type.Vector_14_Concept.Count_2422;
        this.At_2436 = Vector3D_40_Type.Vector_14_Concept.At_2436;
    }
    // field accessors
    static X_501 = function(self) { return self.X_501; }
    static Y_508 = function(self) { return self.Y_508; }
    static Z_515 = function(self) { return self.Z_515; }
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
    constructor(X_522, Y_529, Z_536, W_543)
    {
        // field initialization 
        this.X_522 = X_522;
        this.Y_529 = Y_529;
        this.Z_536 = Z_536;
        this.W_543 = W_543;
        this.Count_2784 = Vector4D_41_Type.Array_25_Concept.Count_2784;
        this.At_2789 = Vector4D_41_Type.Array_25_Concept.At_2789;
        this.Zero_2731 = Vector4D_41_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Vector4D_41_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Vector4D_41_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Vector4D_41_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Vector4D_41_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Vector4D_41_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = Vector4D_41_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = Vector4D_41_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = Vector4D_41_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = Vector4D_41_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = Vector4D_41_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = Vector4D_41_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = Vector4D_41_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Vector4D_41_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Vector4D_41_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Count_2422 = Vector4D_41_Type.Vector_14_Concept.Count_2422;
        this.At_2436 = Vector4D_41_Type.Vector_14_Concept.At_2436;
    }
    // field accessors
    static X_522 = function(self) { return self.X_522; }
    static Y_529 = function(self) { return self.Y_529; }
    static Z_536 = function(self) { return self.Z_536; }
    static W_543 = function(self) { return self.W_543; }
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
    constructor(Value_550)
    {
        // field initialization 
        this.Value_550 = Value_550;
        this.Zero_2731 = Orientation3D_42_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Orientation3D_42_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Orientation3D_42_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Orientation3D_42_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Orientation3D_42_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Orientation3D_42_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Value_550 = function(self) { return self.Value_550; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Orientation3D_42_Type);
    static Implements = [Value_23_Concept];
}
class Pose2D_43_Type
{
    constructor(Position_557, Orientation_564)
    {
        // field initialization 
        this.Position_557 = Position_557;
        this.Orientation_564 = Orientation_564;
        this.Zero_2731 = Pose2D_43_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Pose2D_43_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Pose2D_43_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Pose2D_43_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Pose2D_43_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Pose2D_43_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Position_557 = function(self) { return self.Position_557; }
    static Orientation_564 = function(self) { return self.Orientation_564; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Pose2D_43_Type);
    static Implements = [Value_23_Concept];
}
class Pose3D_44_Type
{
    constructor(Position_571, Orientation_578)
    {
        // field initialization 
        this.Position_571 = Position_571;
        this.Orientation_578 = Orientation_578;
        this.Zero_2731 = Pose3D_44_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Pose3D_44_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Pose3D_44_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Pose3D_44_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Pose3D_44_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Pose3D_44_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Position_571 = function(self) { return self.Position_571; }
    static Orientation_578 = function(self) { return self.Orientation_578; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Pose3D_44_Type);
    static Implements = [Value_23_Concept];
}
class Transform3D_45_Type
{
    constructor(Translation_585, Rotation_592, Scale_599)
    {
        // field initialization 
        this.Translation_585 = Translation_585;
        this.Rotation_592 = Rotation_592;
        this.Scale_599 = Scale_599;
        this.Zero_2731 = Transform3D_45_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Transform3D_45_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Transform3D_45_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Transform3D_45_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Transform3D_45_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Transform3D_45_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Translation_585 = function(self) { return self.Translation_585; }
    static Rotation_592 = function(self) { return self.Rotation_592; }
    static Scale_599 = function(self) { return self.Scale_599; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Transform3D_45_Type);
    static Implements = [Value_23_Concept];
}
class Transform2D_46_Type
{
    constructor(Translation_606, Rotation_613, Scale_620)
    {
        // field initialization 
        this.Translation_606 = Translation_606;
        this.Rotation_613 = Rotation_613;
        this.Scale_620 = Scale_620;
        this.Zero_2731 = Transform2D_46_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Transform2D_46_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Transform2D_46_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Transform2D_46_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Transform2D_46_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Transform2D_46_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Translation_606 = function(self) { return self.Translation_606; }
    static Rotation_613 = function(self) { return self.Rotation_613; }
    static Scale_620 = function(self) { return self.Scale_620; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Transform2D_46_Type);
    static Implements = [Value_23_Concept];
}
class AlignedBox2D_47_Type
{
    constructor(A_627, B_634)
    {
        // field initialization 
        this.A_627 = A_627;
        this.B_634 = B_634;
        this.Count_2784 = AlignedBox2D_47_Type.Array_25_Concept.Count_2784;
        this.At_2789 = AlignedBox2D_47_Type.Array_25_Concept.At_2789;
        this.Zero_2731 = AlignedBox2D_47_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = AlignedBox2D_47_Type.Value_23_Concept.One_2739;
        this.Default_2747 = AlignedBox2D_47_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = AlignedBox2D_47_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = AlignedBox2D_47_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = AlignedBox2D_47_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = AlignedBox2D_47_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = AlignedBox2D_47_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = AlignedBox2D_47_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Count_2422 = AlignedBox2D_47_Type.Vector_14_Concept.Count_2422;
        this.At_2436 = AlignedBox2D_47_Type.Vector_14_Concept.At_2436;
        this.Min_2778 = AlignedBox2D_47_Type.Interval_24_Concept.Min_2778;
        this.Max_2781 = AlignedBox2D_47_Type.Interval_24_Concept.Max_2781;
    }
    // field accessors
    static A_627 = function(self) { return self.A_627; }
    static B_634 = function(self) { return self.B_634; }
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
    constructor(A_641, B_648)
    {
        // field initialization 
        this.A_641 = A_641;
        this.B_648 = B_648;
        this.Count_2784 = AlignedBox3D_48_Type.Array_25_Concept.Count_2784;
        this.At_2789 = AlignedBox3D_48_Type.Array_25_Concept.At_2789;
        this.Zero_2731 = AlignedBox3D_48_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = AlignedBox3D_48_Type.Value_23_Concept.One_2739;
        this.Default_2747 = AlignedBox3D_48_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = AlignedBox3D_48_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = AlignedBox3D_48_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = AlignedBox3D_48_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = AlignedBox3D_48_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = AlignedBox3D_48_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = AlignedBox3D_48_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Count_2422 = AlignedBox3D_48_Type.Vector_14_Concept.Count_2422;
        this.At_2436 = AlignedBox3D_48_Type.Vector_14_Concept.At_2436;
        this.Min_2778 = AlignedBox3D_48_Type.Interval_24_Concept.Min_2778;
        this.Max_2781 = AlignedBox3D_48_Type.Interval_24_Concept.Max_2781;
    }
    // field accessors
    static A_641 = function(self) { return self.A_641; }
    static B_648 = function(self) { return self.B_648; }
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
    constructor(Real_655, Imaginary_662)
    {
        // field initialization 
        this.Real_655 = Real_655;
        this.Imaginary_662 = Imaginary_662;
        this.Count_2784 = Complex_49_Type.Array_25_Concept.Count_2784;
        this.At_2789 = Complex_49_Type.Array_25_Concept.At_2789;
        this.Zero_2731 = Complex_49_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Complex_49_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Complex_49_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Complex_49_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Complex_49_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Complex_49_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = Complex_49_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = Complex_49_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = Complex_49_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = Complex_49_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = Complex_49_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = Complex_49_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = Complex_49_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Complex_49_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Complex_49_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Count_2422 = Complex_49_Type.Vector_14_Concept.Count_2422;
        this.At_2436 = Complex_49_Type.Vector_14_Concept.At_2436;
    }
    // field accessors
    static Real_655 = function(self) { return self.Real_655; }
    static Imaginary_662 = function(self) { return self.Imaginary_662; }
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
    constructor(Direction_669, Position_676)
    {
        // field initialization 
        this.Direction_669 = Direction_669;
        this.Position_676 = Position_676;
        this.Zero_2731 = Ray3D_50_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Ray3D_50_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Ray3D_50_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Ray3D_50_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Ray3D_50_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Ray3D_50_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Direction_669 = function(self) { return self.Direction_669; }
    static Position_676 = function(self) { return self.Position_676; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Ray3D_50_Type);
    static Implements = [Value_23_Concept];
}
class Ray2D_51_Type
{
    constructor(Direction_683, Position_690)
    {
        // field initialization 
        this.Direction_683 = Direction_683;
        this.Position_690 = Position_690;
        this.Zero_2731 = Ray2D_51_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Ray2D_51_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Ray2D_51_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Ray2D_51_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Ray2D_51_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Ray2D_51_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Direction_683 = function(self) { return self.Direction_683; }
    static Position_690 = function(self) { return self.Position_690; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Ray2D_51_Type);
    static Implements = [Value_23_Concept];
}
class Sphere_52_Type
{
    constructor(Center_697, Radius_704)
    {
        // field initialization 
        this.Center_697 = Center_697;
        this.Radius_704 = Radius_704;
        this.Zero_2731 = Sphere_52_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Sphere_52_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Sphere_52_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Sphere_52_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Sphere_52_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Sphere_52_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Center_697 = function(self) { return self.Center_697; }
    static Radius_704 = function(self) { return self.Radius_704; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Sphere_52_Type);
    static Implements = [Value_23_Concept];
}
class Plane_53_Type
{
    constructor(Normal_711, D_718)
    {
        // field initialization 
        this.Normal_711 = Normal_711;
        this.D_718 = D_718;
        this.Zero_2731 = Plane_53_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Plane_53_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Plane_53_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Plane_53_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Plane_53_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Plane_53_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Normal_711 = function(self) { return self.Normal_711; }
    static D_718 = function(self) { return self.D_718; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Plane_53_Type);
    static Implements = [Value_23_Concept];
}
class Triangle3D_54_Type
{
    constructor(A_725, B_732, C_739)
    {
        // field initialization 
        this.A_725 = A_725;
        this.B_732 = B_732;
        this.C_739 = C_739;
        this.Zero_2731 = Triangle3D_54_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Triangle3D_54_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Triangle3D_54_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Triangle3D_54_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Triangle3D_54_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Triangle3D_54_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static A_725 = function(self) { return self.A_725; }
    static B_732 = function(self) { return self.B_732; }
    static C_739 = function(self) { return self.C_739; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Triangle3D_54_Type);
    static Implements = [Value_23_Concept];
}
class Triangle2D_55_Type
{
    constructor(A_746, B_753, C_760)
    {
        // field initialization 
        this.A_746 = A_746;
        this.B_753 = B_753;
        this.C_760 = C_760;
        this.Zero_2731 = Triangle2D_55_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Triangle2D_55_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Triangle2D_55_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Triangle2D_55_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Triangle2D_55_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Triangle2D_55_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static A_746 = function(self) { return self.A_746; }
    static B_753 = function(self) { return self.B_753; }
    static C_760 = function(self) { return self.C_760; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Triangle2D_55_Type);
    static Implements = [Value_23_Concept];
}
class Quad3D_56_Type
{
    constructor(A_767, B_774, C_781, D_788)
    {
        // field initialization 
        this.A_767 = A_767;
        this.B_774 = B_774;
        this.C_781 = C_781;
        this.D_788 = D_788;
        this.Zero_2731 = Quad3D_56_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Quad3D_56_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Quad3D_56_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Quad3D_56_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Quad3D_56_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Quad3D_56_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static A_767 = function(self) { return self.A_767; }
    static B_774 = function(self) { return self.B_774; }
    static C_781 = function(self) { return self.C_781; }
    static D_788 = function(self) { return self.D_788; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Quad3D_56_Type);
    static Implements = [Value_23_Concept];
}
class Quad2D_57_Type
{
    constructor(A_795, B_802, C_809, D_816)
    {
        // field initialization 
        this.A_795 = A_795;
        this.B_802 = B_802;
        this.C_809 = C_809;
        this.D_816 = D_816;
        this.Zero_2731 = Quad2D_57_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Quad2D_57_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Quad2D_57_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Quad2D_57_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Quad2D_57_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Quad2D_57_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static A_795 = function(self) { return self.A_795; }
    static B_802 = function(self) { return self.B_802; }
    static C_809 = function(self) { return self.C_809; }
    static D_816 = function(self) { return self.D_816; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Quad2D_57_Type);
    static Implements = [Value_23_Concept];
}
class Point3D_58_Type
{
    constructor(Value_823)
    {
        // field initialization 
        this.Value_823 = Value_823;
        this.Zero_2731 = Point3D_58_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Point3D_58_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Point3D_58_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Point3D_58_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Point3D_58_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Point3D_58_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Value_823 = function(self) { return self.Value_823; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Point3D_58_Type);
    static Implements = [Value_23_Concept];
}
class Point2D_59_Type
{
    constructor(Value_830)
    {
        // field initialization 
        this.Value_830 = Value_830;
        this.Zero_2731 = Point2D_59_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Point2D_59_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Point2D_59_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Point2D_59_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Point2D_59_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Point2D_59_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Value_830 = function(self) { return self.Value_830; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Point2D_59_Type);
    static Implements = [Value_23_Concept];
}
class Line3D_60_Type
{
    constructor(A_837, B_844)
    {
        // field initialization 
        this.A_837 = A_837;
        this.B_844 = B_844;
        this.Count_2784 = Line3D_60_Type.Array_25_Concept.Count_2784;
        this.At_2789 = Line3D_60_Type.Array_25_Concept.At_2789;
        this.Zero_2731 = Line3D_60_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Line3D_60_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Line3D_60_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Line3D_60_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Line3D_60_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Line3D_60_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = Line3D_60_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = Line3D_60_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = Line3D_60_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = Line3D_60_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = Line3D_60_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = Line3D_60_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = Line3D_60_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Line3D_60_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Line3D_60_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Count_2422 = Line3D_60_Type.Vector_14_Concept.Count_2422;
        this.At_2436 = Line3D_60_Type.Vector_14_Concept.At_2436;
        this.Min_2778 = Line3D_60_Type.Interval_24_Concept.Min_2778;
        this.Max_2781 = Line3D_60_Type.Interval_24_Concept.Max_2781;
    }
    // field accessors
    static A_837 = function(self) { return self.A_837; }
    static B_844 = function(self) { return self.B_844; }
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
    constructor(A_851, B_858)
    {
        // field initialization 
        this.A_851 = A_851;
        this.B_858 = B_858;
        this.Count_2784 = Line2D_61_Type.Array_25_Concept.Count_2784;
        this.At_2789 = Line2D_61_Type.Array_25_Concept.At_2789;
        this.Zero_2731 = Line2D_61_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Line2D_61_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Line2D_61_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Line2D_61_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Line2D_61_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Line2D_61_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = Line2D_61_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = Line2D_61_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = Line2D_61_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = Line2D_61_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = Line2D_61_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = Line2D_61_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = Line2D_61_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Line2D_61_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Line2D_61_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Count_2422 = Line2D_61_Type.Vector_14_Concept.Count_2422;
        this.At_2436 = Line2D_61_Type.Vector_14_Concept.At_2436;
        this.Min_2778 = Line2D_61_Type.Interval_24_Concept.Min_2778;
        this.Max_2781 = Line2D_61_Type.Interval_24_Concept.Max_2781;
    }
    // field accessors
    static A_851 = function(self) { return self.A_851; }
    static B_858 = function(self) { return self.B_858; }
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
    constructor(R_865, G_872, B_879, A_886)
    {
        // field initialization 
        this.R_865 = R_865;
        this.G_872 = G_872;
        this.B_879 = B_879;
        this.A_886 = A_886;
        this.Zero_2731 = Color_62_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Color_62_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Color_62_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Color_62_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Color_62_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Color_62_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static R_865 = function(self) { return self.R_865; }
    static G_872 = function(self) { return self.G_872; }
    static B_879 = function(self) { return self.B_879; }
    static A_886 = function(self) { return self.A_886; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Color_62_Type);
    static Implements = [Value_23_Concept];
}
class ColorLUV_63_Type
{
    constructor(Lightness_893, U_900, V_907)
    {
        // field initialization 
        this.Lightness_893 = Lightness_893;
        this.U_900 = U_900;
        this.V_907 = V_907;
        this.Zero_2731 = ColorLUV_63_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = ColorLUV_63_Type.Value_23_Concept.One_2739;
        this.Default_2747 = ColorLUV_63_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = ColorLUV_63_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = ColorLUV_63_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = ColorLUV_63_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Lightness_893 = function(self) { return self.Lightness_893; }
    static U_900 = function(self) { return self.U_900; }
    static V_907 = function(self) { return self.V_907; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorLUV_63_Type);
    static Implements = [Value_23_Concept];
}
class ColorLAB_64_Type
{
    constructor(Lightness_914, A_921, B_928)
    {
        // field initialization 
        this.Lightness_914 = Lightness_914;
        this.A_921 = A_921;
        this.B_928 = B_928;
        this.Zero_2731 = ColorLAB_64_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = ColorLAB_64_Type.Value_23_Concept.One_2739;
        this.Default_2747 = ColorLAB_64_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = ColorLAB_64_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = ColorLAB_64_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = ColorLAB_64_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Lightness_914 = function(self) { return self.Lightness_914; }
    static A_921 = function(self) { return self.A_921; }
    static B_928 = function(self) { return self.B_928; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorLAB_64_Type);
    static Implements = [Value_23_Concept];
}
class ColorLCh_65_Type
{
    constructor(Lightness_935, ChromaHue_942)
    {
        // field initialization 
        this.Lightness_935 = Lightness_935;
        this.ChromaHue_942 = ChromaHue_942;
        this.Zero_2731 = ColorLCh_65_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = ColorLCh_65_Type.Value_23_Concept.One_2739;
        this.Default_2747 = ColorLCh_65_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = ColorLCh_65_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = ColorLCh_65_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = ColorLCh_65_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Lightness_935 = function(self) { return self.Lightness_935; }
    static ChromaHue_942 = function(self) { return self.ChromaHue_942; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorLCh_65_Type);
    static Implements = [Value_23_Concept];
}
class ColorHSV_66_Type
{
    constructor(Hue_949, S_956, V_963)
    {
        // field initialization 
        this.Hue_949 = Hue_949;
        this.S_956 = S_956;
        this.V_963 = V_963;
        this.Zero_2731 = ColorHSV_66_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = ColorHSV_66_Type.Value_23_Concept.One_2739;
        this.Default_2747 = ColorHSV_66_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = ColorHSV_66_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = ColorHSV_66_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = ColorHSV_66_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Hue_949 = function(self) { return self.Hue_949; }
    static S_956 = function(self) { return self.S_956; }
    static V_963 = function(self) { return self.V_963; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorHSV_66_Type);
    static Implements = [Value_23_Concept];
}
class ColorHSL_67_Type
{
    constructor(Hue_970, Saturation_977, Luminance_984)
    {
        // field initialization 
        this.Hue_970 = Hue_970;
        this.Saturation_977 = Saturation_977;
        this.Luminance_984 = Luminance_984;
        this.Zero_2731 = ColorHSL_67_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = ColorHSL_67_Type.Value_23_Concept.One_2739;
        this.Default_2747 = ColorHSL_67_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = ColorHSL_67_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = ColorHSL_67_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = ColorHSL_67_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Hue_970 = function(self) { return self.Hue_970; }
    static Saturation_977 = function(self) { return self.Saturation_977; }
    static Luminance_984 = function(self) { return self.Luminance_984; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorHSL_67_Type);
    static Implements = [Value_23_Concept];
}
class ColorYCbCr_68_Type
{
    constructor(Y_991, Cb_998, Cr_1005)
    {
        // field initialization 
        this.Y_991 = Y_991;
        this.Cb_998 = Cb_998;
        this.Cr_1005 = Cr_1005;
        this.Zero_2731 = ColorYCbCr_68_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = ColorYCbCr_68_Type.Value_23_Concept.One_2739;
        this.Default_2747 = ColorYCbCr_68_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = ColorYCbCr_68_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = ColorYCbCr_68_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = ColorYCbCr_68_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Y_991 = function(self) { return self.Y_991; }
    static Cb_998 = function(self) { return self.Cb_998; }
    static Cr_1005 = function(self) { return self.Cr_1005; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorYCbCr_68_Type);
    static Implements = [Value_23_Concept];
}
class SphericalCoordinate_69_Type
{
    constructor(Radius_1012, Azimuth_1019, Polar_1026)
    {
        // field initialization 
        this.Radius_1012 = Radius_1012;
        this.Azimuth_1019 = Azimuth_1019;
        this.Polar_1026 = Polar_1026;
        this.Zero_2731 = SphericalCoordinate_69_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = SphericalCoordinate_69_Type.Value_23_Concept.One_2739;
        this.Default_2747 = SphericalCoordinate_69_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = SphericalCoordinate_69_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = SphericalCoordinate_69_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = SphericalCoordinate_69_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Radius_1012 = function(self) { return self.Radius_1012; }
    static Azimuth_1019 = function(self) { return self.Azimuth_1019; }
    static Polar_1026 = function(self) { return self.Polar_1026; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(SphericalCoordinate_69_Type);
    static Implements = [Value_23_Concept];
}
class PolarCoordinate_70_Type
{
    constructor(Radius_1033, Angle_1040)
    {
        // field initialization 
        this.Radius_1033 = Radius_1033;
        this.Angle_1040 = Angle_1040;
        this.Zero_2731 = PolarCoordinate_70_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = PolarCoordinate_70_Type.Value_23_Concept.One_2739;
        this.Default_2747 = PolarCoordinate_70_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = PolarCoordinate_70_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = PolarCoordinate_70_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = PolarCoordinate_70_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Radius_1033 = function(self) { return self.Radius_1033; }
    static Angle_1040 = function(self) { return self.Angle_1040; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(PolarCoordinate_70_Type);
    static Implements = [Value_23_Concept];
}
class LogPolarCoordinate_71_Type
{
    constructor(Rho_1047, Azimuth_1054)
    {
        // field initialization 
        this.Rho_1047 = Rho_1047;
        this.Azimuth_1054 = Azimuth_1054;
        this.Zero_2731 = LogPolarCoordinate_71_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = LogPolarCoordinate_71_Type.Value_23_Concept.One_2739;
        this.Default_2747 = LogPolarCoordinate_71_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = LogPolarCoordinate_71_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = LogPolarCoordinate_71_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = LogPolarCoordinate_71_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Rho_1047 = function(self) { return self.Rho_1047; }
    static Azimuth_1054 = function(self) { return self.Azimuth_1054; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(LogPolarCoordinate_71_Type);
    static Implements = [Value_23_Concept];
}
class CylindricalCoordinate_72_Type
{
    constructor(RadialDistance_1061, Azimuth_1068, Height_1075)
    {
        // field initialization 
        this.RadialDistance_1061 = RadialDistance_1061;
        this.Azimuth_1068 = Azimuth_1068;
        this.Height_1075 = Height_1075;
        this.Zero_2731 = CylindricalCoordinate_72_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = CylindricalCoordinate_72_Type.Value_23_Concept.One_2739;
        this.Default_2747 = CylindricalCoordinate_72_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = CylindricalCoordinate_72_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = CylindricalCoordinate_72_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = CylindricalCoordinate_72_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static RadialDistance_1061 = function(self) { return self.RadialDistance_1061; }
    static Azimuth_1068 = function(self) { return self.Azimuth_1068; }
    static Height_1075 = function(self) { return self.Height_1075; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(CylindricalCoordinate_72_Type);
    static Implements = [Value_23_Concept];
}
class HorizontalCoordinate_73_Type
{
    constructor(Radius_1082, Azimuth_1089, Height_1096)
    {
        // field initialization 
        this.Radius_1082 = Radius_1082;
        this.Azimuth_1089 = Azimuth_1089;
        this.Height_1096 = Height_1096;
        this.Zero_2731 = HorizontalCoordinate_73_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = HorizontalCoordinate_73_Type.Value_23_Concept.One_2739;
        this.Default_2747 = HorizontalCoordinate_73_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = HorizontalCoordinate_73_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = HorizontalCoordinate_73_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = HorizontalCoordinate_73_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Radius_1082 = function(self) { return self.Radius_1082; }
    static Azimuth_1089 = function(self) { return self.Azimuth_1089; }
    static Height_1096 = function(self) { return self.Height_1096; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(HorizontalCoordinate_73_Type);
    static Implements = [Value_23_Concept];
}
class GeoCoordinate_74_Type
{
    constructor(Latitude_1103, Longitude_1110)
    {
        // field initialization 
        this.Latitude_1103 = Latitude_1103;
        this.Longitude_1110 = Longitude_1110;
        this.Zero_2731 = GeoCoordinate_74_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = GeoCoordinate_74_Type.Value_23_Concept.One_2739;
        this.Default_2747 = GeoCoordinate_74_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = GeoCoordinate_74_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = GeoCoordinate_74_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = GeoCoordinate_74_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Latitude_1103 = function(self) { return self.Latitude_1103; }
    static Longitude_1110 = function(self) { return self.Longitude_1110; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(GeoCoordinate_74_Type);
    static Implements = [Value_23_Concept];
}
class GeoCoordinateWithAltitude_75_Type
{
    constructor(Coordinate_1117, Altitude_1124)
    {
        // field initialization 
        this.Coordinate_1117 = Coordinate_1117;
        this.Altitude_1124 = Altitude_1124;
        this.Zero_2731 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.One_2739;
        this.Default_2747 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Coordinate_1117 = function(self) { return self.Coordinate_1117; }
    static Altitude_1124 = function(self) { return self.Altitude_1124; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(GeoCoordinateWithAltitude_75_Type);
    static Implements = [Value_23_Concept];
}
class Circle_76_Type
{
    constructor(Center_1131, Radius_1138)
    {
        // field initialization 
        this.Center_1131 = Center_1131;
        this.Radius_1138 = Radius_1138;
        this.Zero_2731 = Circle_76_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Circle_76_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Circle_76_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Circle_76_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Circle_76_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Circle_76_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Center_1131 = function(self) { return self.Center_1131; }
    static Radius_1138 = function(self) { return self.Radius_1138; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Circle_76_Type);
    static Implements = [Value_23_Concept];
}
class Chord_77_Type
{
    constructor(Circle_1145, Arc_1152)
    {
        // field initialization 
        this.Circle_1145 = Circle_1145;
        this.Arc_1152 = Arc_1152;
        this.Zero_2731 = Chord_77_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Chord_77_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Chord_77_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Chord_77_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Chord_77_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Chord_77_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Circle_1145 = function(self) { return self.Circle_1145; }
    static Arc_1152 = function(self) { return self.Arc_1152; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Chord_77_Type);
    static Implements = [Value_23_Concept];
}
class Size2D_78_Type
{
    constructor(Width_1159, Height_1166)
    {
        // field initialization 
        this.Width_1159 = Width_1159;
        this.Height_1166 = Height_1166;
        this.Zero_2731 = Size2D_78_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Size2D_78_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Size2D_78_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Size2D_78_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Size2D_78_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Size2D_78_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Width_1159 = function(self) { return self.Width_1159; }
    static Height_1166 = function(self) { return self.Height_1166; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Size2D_78_Type);
    static Implements = [Value_23_Concept];
}
class Size3D_79_Type
{
    constructor(Width_1173, Height_1180, Depth_1187)
    {
        // field initialization 
        this.Width_1173 = Width_1173;
        this.Height_1180 = Height_1180;
        this.Depth_1187 = Depth_1187;
        this.Zero_2731 = Size3D_79_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Size3D_79_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Size3D_79_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Size3D_79_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Size3D_79_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Size3D_79_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Width_1173 = function(self) { return self.Width_1173; }
    static Height_1180 = function(self) { return self.Height_1180; }
    static Depth_1187 = function(self) { return self.Depth_1187; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Size3D_79_Type);
    static Implements = [Value_23_Concept];
}
class Rectangle2D_80_Type
{
    constructor(Center_1194, Size_1201)
    {
        // field initialization 
        this.Center_1194 = Center_1194;
        this.Size_1201 = Size_1201;
        this.Zero_2731 = Rectangle2D_80_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Rectangle2D_80_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Rectangle2D_80_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Rectangle2D_80_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Rectangle2D_80_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Rectangle2D_80_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Center_1194 = function(self) { return self.Center_1194; }
    static Size_1201 = function(self) { return self.Size_1201; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Rectangle2D_80_Type);
    static Implements = [Value_23_Concept];
}
class Proportion_81_Type
{
    constructor(Value_1208)
    {
        // field initialization 
        this.Value_1208 = Value_1208;
        this.Zero_2731 = Proportion_81_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Proportion_81_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Proportion_81_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Proportion_81_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Proportion_81_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Proportion_81_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = Proportion_81_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = Proportion_81_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = Proportion_81_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = Proportion_81_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = Proportion_81_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = Proportion_81_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = Proportion_81_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Proportion_81_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Proportion_81_Type.Magnitude_17_Concept.Magnitude_2464;
    }
    // field accessors
    static Value_1208 = function(self) { return self.Value_1208; }
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
    constructor(Numerator_1215, Denominator_1222)
    {
        // field initialization 
        this.Numerator_1215 = Numerator_1215;
        this.Denominator_1222 = Denominator_1222;
        this.Zero_2731 = Fraction_82_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Fraction_82_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Fraction_82_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Fraction_82_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Fraction_82_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Fraction_82_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Numerator_1215 = function(self) { return self.Numerator_1215; }
    static Denominator_1222 = function(self) { return self.Denominator_1222; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Fraction_82_Type);
    static Implements = [Value_23_Concept];
}
class Angle_83_Type
{
    constructor(Radians_1229)
    {
        // field initialization 
        this.Radians_1229 = Radians_1229;
        this.Zero_2731 = Angle_83_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Angle_83_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Angle_83_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Angle_83_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Angle_83_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Angle_83_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = Angle_83_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = Angle_83_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = Angle_83_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = Angle_83_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = Angle_83_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = Angle_83_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Angle_83_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Angle_83_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = Angle_83_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Radians_1229 = function(self) { return self.Radians_1229; }
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
    constructor(Meters_1236)
    {
        // field initialization 
        this.Meters_1236 = Meters_1236;
        this.Zero_2731 = Length_84_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Length_84_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Length_84_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Length_84_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Length_84_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Length_84_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = Length_84_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = Length_84_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = Length_84_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = Length_84_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = Length_84_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = Length_84_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Length_84_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Length_84_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = Length_84_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Meters_1236 = function(self) { return self.Meters_1236; }
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
    constructor(Kilograms_1243)
    {
        // field initialization 
        this.Kilograms_1243 = Kilograms_1243;
        this.Zero_2731 = Mass_85_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Mass_85_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Mass_85_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Mass_85_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Mass_85_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Mass_85_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = Mass_85_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = Mass_85_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = Mass_85_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = Mass_85_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = Mass_85_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = Mass_85_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Mass_85_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Mass_85_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = Mass_85_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Kilograms_1243 = function(self) { return self.Kilograms_1243; }
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
    constructor(Celsius_1250)
    {
        // field initialization 
        this.Celsius_1250 = Celsius_1250;
        this.Zero_2731 = Temperature_86_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Temperature_86_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Temperature_86_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Temperature_86_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Temperature_86_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Temperature_86_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = Temperature_86_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = Temperature_86_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = Temperature_86_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = Temperature_86_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = Temperature_86_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = Temperature_86_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Temperature_86_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Temperature_86_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = Temperature_86_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Celsius_1250 = function(self) { return self.Celsius_1250; }
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
    constructor(Seconds_1257)
    {
        // field initialization 
        this.Seconds_1257 = Seconds_1257;
        this.Zero_2731 = TimeSpan_87_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = TimeSpan_87_Type.Value_23_Concept.One_2739;
        this.Default_2747 = TimeSpan_87_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = TimeSpan_87_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = TimeSpan_87_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = TimeSpan_87_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = TimeSpan_87_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = TimeSpan_87_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = TimeSpan_87_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = TimeSpan_87_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = TimeSpan_87_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = TimeSpan_87_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = TimeSpan_87_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = TimeSpan_87_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = TimeSpan_87_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Seconds_1257 = function(self) { return self.Seconds_1257; }
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
    constructor(Min_1264, Max_1271)
    {
        // field initialization 
        this.Min_1264 = Min_1264;
        this.Max_1271 = Max_1271;
        this.Count_2784 = TimeRange_88_Type.Array_25_Concept.Count_2784;
        this.At_2789 = TimeRange_88_Type.Array_25_Concept.At_2789;
        this.Zero_2731 = TimeRange_88_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = TimeRange_88_Type.Value_23_Concept.One_2739;
        this.Default_2747 = TimeRange_88_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = TimeRange_88_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = TimeRange_88_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = TimeRange_88_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = TimeRange_88_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = TimeRange_88_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = TimeRange_88_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = TimeRange_88_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = TimeRange_88_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = TimeRange_88_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = TimeRange_88_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = TimeRange_88_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = TimeRange_88_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Count_2422 = TimeRange_88_Type.Vector_14_Concept.Count_2422;
        this.At_2436 = TimeRange_88_Type.Vector_14_Concept.At_2436;
        this.Min_2778 = TimeRange_88_Type.Interval_24_Concept.Min_2778;
        this.Max_2781 = TimeRange_88_Type.Interval_24_Concept.Max_2781;
    }
    // field accessors
    static Min_1264 = function(self) { return self.Min_1264; }
    static Max_1271 = function(self) { return self.Max_1271; }
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
        this.Zero_2731 = DateTime_89_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = DateTime_89_Type.Value_23_Concept.One_2739;
        this.Default_2747 = DateTime_89_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = DateTime_89_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = DateTime_89_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = DateTime_89_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(DateTime_89_Type);
    static Implements = [Value_23_Concept];
}
class AnglePair_90_Type
{
    constructor(Start_1278, End_1285)
    {
        // field initialization 
        this.Start_1278 = Start_1278;
        this.End_1285 = End_1285;
        this.Count_2784 = AnglePair_90_Type.Array_25_Concept.Count_2784;
        this.At_2789 = AnglePair_90_Type.Array_25_Concept.At_2789;
        this.Zero_2731 = AnglePair_90_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = AnglePair_90_Type.Value_23_Concept.One_2739;
        this.Default_2747 = AnglePair_90_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = AnglePair_90_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = AnglePair_90_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = AnglePair_90_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = AnglePair_90_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = AnglePair_90_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = AnglePair_90_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = AnglePair_90_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = AnglePair_90_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = AnglePair_90_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = AnglePair_90_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = AnglePair_90_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = AnglePair_90_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Count_2422 = AnglePair_90_Type.Vector_14_Concept.Count_2422;
        this.At_2436 = AnglePair_90_Type.Vector_14_Concept.At_2436;
        this.Min_2778 = AnglePair_90_Type.Interval_24_Concept.Min_2778;
        this.Max_2781 = AnglePair_90_Type.Interval_24_Concept.Max_2781;
    }
    // field accessors
    static Start_1278 = function(self) { return self.Start_1278; }
    static End_1285 = function(self) { return self.End_1285; }
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
    constructor(Circle_1292, InnerRadius_1299)
    {
        // field initialization 
        this.Circle_1292 = Circle_1292;
        this.InnerRadius_1299 = InnerRadius_1299;
        this.Zero_2731 = Ring_91_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Ring_91_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Ring_91_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Ring_91_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Ring_91_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Ring_91_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = Ring_91_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = Ring_91_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = Ring_91_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = Ring_91_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = Ring_91_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = Ring_91_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = Ring_91_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Ring_91_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Ring_91_Type.Magnitude_17_Concept.Magnitude_2464;
    }
    // field accessors
    static Circle_1292 = function(self) { return self.Circle_1292; }
    static InnerRadius_1299 = function(self) { return self.InnerRadius_1299; }
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
    constructor(Angles_1306, Cirlce_1313)
    {
        // field initialization 
        this.Angles_1306 = Angles_1306;
        this.Cirlce_1313 = Cirlce_1313;
        this.Zero_2731 = Arc_92_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Arc_92_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Arc_92_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Arc_92_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Arc_92_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Arc_92_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Angles_1306 = function(self) { return self.Angles_1306; }
    static Cirlce_1313 = function(self) { return self.Cirlce_1313; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Arc_92_Type);
    static Implements = [Value_23_Concept];
}
class TimeInterval_93_Type
{
    constructor(Start_1320, End_1327)
    {
        // field initialization 
        this.Start_1320 = Start_1320;
        this.End_1327 = End_1327;
        this.Count_2784 = TimeInterval_93_Type.Array_25_Concept.Count_2784;
        this.At_2789 = TimeInterval_93_Type.Array_25_Concept.At_2789;
        this.Zero_2731 = TimeInterval_93_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = TimeInterval_93_Type.Value_23_Concept.One_2739;
        this.Default_2747 = TimeInterval_93_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = TimeInterval_93_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = TimeInterval_93_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = TimeInterval_93_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = TimeInterval_93_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = TimeInterval_93_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = TimeInterval_93_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = TimeInterval_93_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = TimeInterval_93_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = TimeInterval_93_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = TimeInterval_93_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = TimeInterval_93_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = TimeInterval_93_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Count_2422 = TimeInterval_93_Type.Vector_14_Concept.Count_2422;
        this.At_2436 = TimeInterval_93_Type.Vector_14_Concept.At_2436;
        this.Min_2778 = TimeInterval_93_Type.Interval_24_Concept.Min_2778;
        this.Max_2781 = TimeInterval_93_Type.Interval_24_Concept.Max_2781;
    }
    // field accessors
    static Start_1320 = function(self) { return self.Start_1320; }
    static End_1327 = function(self) { return self.End_1327; }
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
    constructor(A_1334, B_1341)
    {
        // field initialization 
        this.A_1334 = A_1334;
        this.B_1341 = B_1341;
        this.Count_2784 = RealInterval_94_Type.Array_25_Concept.Count_2784;
        this.At_2789 = RealInterval_94_Type.Array_25_Concept.At_2789;
        this.Zero_2731 = RealInterval_94_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = RealInterval_94_Type.Value_23_Concept.One_2739;
        this.Default_2747 = RealInterval_94_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = RealInterval_94_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = RealInterval_94_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = RealInterval_94_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = RealInterval_94_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = RealInterval_94_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = RealInterval_94_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = RealInterval_94_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = RealInterval_94_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = RealInterval_94_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = RealInterval_94_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = RealInterval_94_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = RealInterval_94_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Count_2422 = RealInterval_94_Type.Vector_14_Concept.Count_2422;
        this.At_2436 = RealInterval_94_Type.Vector_14_Concept.At_2436;
        this.Min_2778 = RealInterval_94_Type.Interval_24_Concept.Min_2778;
        this.Max_2781 = RealInterval_94_Type.Interval_24_Concept.Max_2781;
    }
    // field accessors
    static A_1334 = function(self) { return self.A_1334; }
    static B_1341 = function(self) { return self.B_1341; }
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
    constructor(A_1348, B_1355)
    {
        // field initialization 
        this.A_1348 = A_1348;
        this.B_1355 = B_1355;
        this.Count_2784 = Interval2D_95_Type.Array_25_Concept.Count_2784;
        this.At_2789 = Interval2D_95_Type.Array_25_Concept.At_2789;
        this.Zero_2731 = Interval2D_95_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Interval2D_95_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Interval2D_95_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Interval2D_95_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Interval2D_95_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Interval2D_95_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = Interval2D_95_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = Interval2D_95_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = Interval2D_95_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = Interval2D_95_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = Interval2D_95_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = Interval2D_95_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = Interval2D_95_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Interval2D_95_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Interval2D_95_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Count_2422 = Interval2D_95_Type.Vector_14_Concept.Count_2422;
        this.At_2436 = Interval2D_95_Type.Vector_14_Concept.At_2436;
        this.Min_2778 = Interval2D_95_Type.Interval_24_Concept.Min_2778;
        this.Max_2781 = Interval2D_95_Type.Interval_24_Concept.Max_2781;
    }
    // field accessors
    static A_1348 = function(self) { return self.A_1348; }
    static B_1355 = function(self) { return self.B_1355; }
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
    constructor(A_1362, B_1369)
    {
        // field initialization 
        this.A_1362 = A_1362;
        this.B_1369 = B_1369;
        this.Count_2784 = Interval3D_96_Type.Array_25_Concept.Count_2784;
        this.At_2789 = Interval3D_96_Type.Array_25_Concept.At_2789;
        this.Zero_2731 = Interval3D_96_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Interval3D_96_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Interval3D_96_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Interval3D_96_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Interval3D_96_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Interval3D_96_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = Interval3D_96_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = Interval3D_96_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = Interval3D_96_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = Interval3D_96_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = Interval3D_96_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = Interval3D_96_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = Interval3D_96_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Interval3D_96_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Interval3D_96_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Count_2422 = Interval3D_96_Type.Vector_14_Concept.Count_2422;
        this.At_2436 = Interval3D_96_Type.Vector_14_Concept.At_2436;
        this.Min_2778 = Interval3D_96_Type.Interval_24_Concept.Min_2778;
        this.Max_2781 = Interval3D_96_Type.Interval_24_Concept.Max_2781;
    }
    // field accessors
    static A_1362 = function(self) { return self.A_1362; }
    static B_1369 = function(self) { return self.B_1369; }
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
    constructor(Line_1376, Radius_1383)
    {
        // field initialization 
        this.Line_1376 = Line_1376;
        this.Radius_1383 = Radius_1383;
        this.Zero_2731 = Capsule_97_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Capsule_97_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Capsule_97_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Capsule_97_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Capsule_97_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Capsule_97_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Line_1376 = function(self) { return self.Line_1376; }
    static Radius_1383 = function(self) { return self.Radius_1383; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Capsule_97_Type);
    static Implements = [Value_23_Concept];
}
class Matrix3D_98_Type
{
    constructor(Column1_1390, Column2_1397, Column3_1404, Column4_1411)
    {
        // field initialization 
        this.Column1_1390 = Column1_1390;
        this.Column2_1397 = Column2_1397;
        this.Column3_1404 = Column3_1404;
        this.Column4_1411 = Column4_1411;
        this.Zero_2731 = Matrix3D_98_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Matrix3D_98_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Matrix3D_98_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Matrix3D_98_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Matrix3D_98_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Matrix3D_98_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Column1_1390 = function(self) { return self.Column1_1390; }
    static Column2_1397 = function(self) { return self.Column2_1397; }
    static Column3_1404 = function(self) { return self.Column3_1404; }
    static Column4_1411 = function(self) { return self.Column4_1411; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Matrix3D_98_Type);
    static Implements = [Value_23_Concept];
}
class Cylinder_99_Type
{
    constructor(Line_1418, Radius_1425)
    {
        // field initialization 
        this.Line_1418 = Line_1418;
        this.Radius_1425 = Radius_1425;
        this.Zero_2731 = Cylinder_99_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Cylinder_99_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Cylinder_99_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Cylinder_99_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Cylinder_99_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Cylinder_99_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Line_1418 = function(self) { return self.Line_1418; }
    static Radius_1425 = function(self) { return self.Radius_1425; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Cylinder_99_Type);
    static Implements = [Value_23_Concept];
}
class Cone_100_Type
{
    constructor(Line_1432, Radius_1439)
    {
        // field initialization 
        this.Line_1432 = Line_1432;
        this.Radius_1439 = Radius_1439;
        this.Zero_2731 = Cone_100_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Cone_100_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Cone_100_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Cone_100_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Cone_100_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Cone_100_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Line_1432 = function(self) { return self.Line_1432; }
    static Radius_1439 = function(self) { return self.Radius_1439; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Cone_100_Type);
    static Implements = [Value_23_Concept];
}
class Tube_101_Type
{
    constructor(Line_1446, InnerRadius_1453, OuterRadius_1460)
    {
        // field initialization 
        this.Line_1446 = Line_1446;
        this.InnerRadius_1453 = InnerRadius_1453;
        this.OuterRadius_1460 = OuterRadius_1460;
        this.Zero_2731 = Tube_101_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Tube_101_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Tube_101_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Tube_101_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Tube_101_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Tube_101_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Line_1446 = function(self) { return self.Line_1446; }
    static InnerRadius_1453 = function(self) { return self.InnerRadius_1453; }
    static OuterRadius_1460 = function(self) { return self.OuterRadius_1460; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Tube_101_Type);
    static Implements = [Value_23_Concept];
}
class ConeSegment_102_Type
{
    constructor(Line_1467, Radius1_1474, Radius2_1481)
    {
        // field initialization 
        this.Line_1467 = Line_1467;
        this.Radius1_1474 = Radius1_1474;
        this.Radius2_1481 = Radius2_1481;
        this.Zero_2731 = ConeSegment_102_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = ConeSegment_102_Type.Value_23_Concept.One_2739;
        this.Default_2747 = ConeSegment_102_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = ConeSegment_102_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = ConeSegment_102_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = ConeSegment_102_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Line_1467 = function(self) { return self.Line_1467; }
    static Radius1_1474 = function(self) { return self.Radius1_1474; }
    static Radius2_1481 = function(self) { return self.Radius2_1481; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ConeSegment_102_Type);
    static Implements = [Value_23_Concept];
}
class Box2D_103_Type
{
    constructor(Center_1488, Rotation_1495, Extent_1502)
    {
        // field initialization 
        this.Center_1488 = Center_1488;
        this.Rotation_1495 = Rotation_1495;
        this.Extent_1502 = Extent_1502;
        this.Zero_2731 = Box2D_103_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Box2D_103_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Box2D_103_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Box2D_103_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Box2D_103_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Box2D_103_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Center_1488 = function(self) { return self.Center_1488; }
    static Rotation_1495 = function(self) { return self.Rotation_1495; }
    static Extent_1502 = function(self) { return self.Extent_1502; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Box2D_103_Type);
    static Implements = [Value_23_Concept];
}
class Box3D_104_Type
{
    constructor(Center_1509, Rotation_1516, Extent_1523)
    {
        // field initialization 
        this.Center_1509 = Center_1509;
        this.Rotation_1516 = Rotation_1516;
        this.Extent_1523 = Extent_1523;
        this.Zero_2731 = Box3D_104_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Box3D_104_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Box3D_104_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Box3D_104_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Box3D_104_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Box3D_104_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Center_1509 = function(self) { return self.Center_1509; }
    static Rotation_1516 = function(self) { return self.Rotation_1516; }
    static Extent_1523 = function(self) { return self.Extent_1523; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Box3D_104_Type);
    static Implements = [Value_23_Concept];
}
class CubicBezierTriangle3D_105_Type
{
    constructor(A_1530, B_1537, C_1544, A2B_1551, AB2_1558, B2C_1565, BC2_1572, AC2_1579, A2C_1586, ABC_1593)
    {
        // field initialization 
        this.A_1530 = A_1530;
        this.B_1537 = B_1537;
        this.C_1544 = C_1544;
        this.A2B_1551 = A2B_1551;
        this.AB2_1558 = AB2_1558;
        this.B2C_1565 = B2C_1565;
        this.BC2_1572 = BC2_1572;
        this.AC2_1579 = AC2_1579;
        this.A2C_1586 = A2C_1586;
        this.ABC_1593 = ABC_1593;
        this.Zero_2731 = CubicBezierTriangle3D_105_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = CubicBezierTriangle3D_105_Type.Value_23_Concept.One_2739;
        this.Default_2747 = CubicBezierTriangle3D_105_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = CubicBezierTriangle3D_105_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = CubicBezierTriangle3D_105_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = CubicBezierTriangle3D_105_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static A_1530 = function(self) { return self.A_1530; }
    static B_1537 = function(self) { return self.B_1537; }
    static C_1544 = function(self) { return self.C_1544; }
    static A2B_1551 = function(self) { return self.A2B_1551; }
    static AB2_1558 = function(self) { return self.AB2_1558; }
    static B2C_1565 = function(self) { return self.B2C_1565; }
    static BC2_1572 = function(self) { return self.BC2_1572; }
    static AC2_1579 = function(self) { return self.AC2_1579; }
    static A2C_1586 = function(self) { return self.A2C_1586; }
    static ABC_1593 = function(self) { return self.ABC_1593; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(CubicBezierTriangle3D_105_Type);
    static Implements = [Value_23_Concept];
}
class CubicBezier2D_106_Type
{
    constructor(A_1600, B_1607, C_1614, D_1621)
    {
        // field initialization 
        this.A_1600 = A_1600;
        this.B_1607 = B_1607;
        this.C_1614 = C_1614;
        this.D_1621 = D_1621;
        this.Zero_2731 = CubicBezier2D_106_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = CubicBezier2D_106_Type.Value_23_Concept.One_2739;
        this.Default_2747 = CubicBezier2D_106_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = CubicBezier2D_106_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = CubicBezier2D_106_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = CubicBezier2D_106_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static A_1600 = function(self) { return self.A_1600; }
    static B_1607 = function(self) { return self.B_1607; }
    static C_1614 = function(self) { return self.C_1614; }
    static D_1621 = function(self) { return self.D_1621; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(CubicBezier2D_106_Type);
    static Implements = [Value_23_Concept];
}
class UV_107_Type
{
    constructor(U_1628, V_1635)
    {
        // field initialization 
        this.U_1628 = U_1628;
        this.V_1635 = V_1635;
        this.Count_2784 = UV_107_Type.Array_25_Concept.Count_2784;
        this.At_2789 = UV_107_Type.Array_25_Concept.At_2789;
        this.Zero_2731 = UV_107_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = UV_107_Type.Value_23_Concept.One_2739;
        this.Default_2747 = UV_107_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = UV_107_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = UV_107_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = UV_107_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = UV_107_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = UV_107_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = UV_107_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = UV_107_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = UV_107_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = UV_107_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = UV_107_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = UV_107_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = UV_107_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Count_2422 = UV_107_Type.Vector_14_Concept.Count_2422;
        this.At_2436 = UV_107_Type.Vector_14_Concept.At_2436;
    }
    // field accessors
    static U_1628 = function(self) { return self.U_1628; }
    static V_1635 = function(self) { return self.V_1635; }
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
    constructor(U_1642, V_1649, W_1656)
    {
        // field initialization 
        this.U_1642 = U_1642;
        this.V_1649 = V_1649;
        this.W_1656 = W_1656;
        this.Count_2784 = UVW_108_Type.Array_25_Concept.Count_2784;
        this.At_2789 = UVW_108_Type.Array_25_Concept.At_2789;
        this.Zero_2731 = UVW_108_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = UVW_108_Type.Value_23_Concept.One_2739;
        this.Default_2747 = UVW_108_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = UVW_108_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = UVW_108_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = UVW_108_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = UVW_108_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = UVW_108_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = UVW_108_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = UVW_108_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = UVW_108_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = UVW_108_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = UVW_108_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = UVW_108_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = UVW_108_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Count_2422 = UVW_108_Type.Vector_14_Concept.Count_2422;
        this.At_2436 = UVW_108_Type.Vector_14_Concept.At_2436;
    }
    // field accessors
    static U_1642 = function(self) { return self.U_1642; }
    static V_1649 = function(self) { return self.V_1649; }
    static W_1656 = function(self) { return self.W_1656; }
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
    constructor(A_1663, B_1670, C_1677, D_1684)
    {
        // field initialization 
        this.A_1663 = A_1663;
        this.B_1670 = B_1670;
        this.C_1677 = C_1677;
        this.D_1684 = D_1684;
        this.Zero_2731 = CubicBezier3D_109_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = CubicBezier3D_109_Type.Value_23_Concept.One_2739;
        this.Default_2747 = CubicBezier3D_109_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = CubicBezier3D_109_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = CubicBezier3D_109_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = CubicBezier3D_109_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static A_1663 = function(self) { return self.A_1663; }
    static B_1670 = function(self) { return self.B_1670; }
    static C_1677 = function(self) { return self.C_1677; }
    static D_1684 = function(self) { return self.D_1684; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(CubicBezier3D_109_Type);
    static Implements = [Value_23_Concept];
}
class QuadraticBezier2D_110_Type
{
    constructor(A_1691, B_1698, C_1705)
    {
        // field initialization 
        this.A_1691 = A_1691;
        this.B_1698 = B_1698;
        this.C_1705 = C_1705;
        this.Zero_2731 = QuadraticBezier2D_110_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = QuadraticBezier2D_110_Type.Value_23_Concept.One_2739;
        this.Default_2747 = QuadraticBezier2D_110_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = QuadraticBezier2D_110_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = QuadraticBezier2D_110_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = QuadraticBezier2D_110_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static A_1691 = function(self) { return self.A_1691; }
    static B_1698 = function(self) { return self.B_1698; }
    static C_1705 = function(self) { return self.C_1705; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(QuadraticBezier2D_110_Type);
    static Implements = [Value_23_Concept];
}
class QuadraticBezier3D_111_Type
{
    constructor(A_1712, B_1719, C_1726)
    {
        // field initialization 
        this.A_1712 = A_1712;
        this.B_1719 = B_1719;
        this.C_1726 = C_1726;
        this.Zero_2731 = QuadraticBezier3D_111_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = QuadraticBezier3D_111_Type.Value_23_Concept.One_2739;
        this.Default_2747 = QuadraticBezier3D_111_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = QuadraticBezier3D_111_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = QuadraticBezier3D_111_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = QuadraticBezier3D_111_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static A_1712 = function(self) { return self.A_1712; }
    static B_1719 = function(self) { return self.B_1719; }
    static C_1726 = function(self) { return self.C_1726; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(QuadraticBezier3D_111_Type);
    static Implements = [Value_23_Concept];
}
class Area_112_Type
{
    constructor(MetersSquared_1733)
    {
        // field initialization 
        this.MetersSquared_1733 = MetersSquared_1733;
        this.Zero_2731 = Area_112_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Area_112_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Area_112_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Area_112_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Area_112_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Area_112_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = Area_112_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = Area_112_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = Area_112_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = Area_112_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = Area_112_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = Area_112_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Area_112_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Area_112_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = Area_112_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static MetersSquared_1733 = function(self) { return self.MetersSquared_1733; }
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
    constructor(MetersCubed_1740)
    {
        // field initialization 
        this.MetersCubed_1740 = MetersCubed_1740;
        this.Zero_2731 = Volume_113_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Volume_113_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Volume_113_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Volume_113_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Volume_113_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Volume_113_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = Volume_113_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = Volume_113_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = Volume_113_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = Volume_113_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = Volume_113_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = Volume_113_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Volume_113_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Volume_113_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = Volume_113_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static MetersCubed_1740 = function(self) { return self.MetersCubed_1740; }
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
    constructor(MetersPerSecond_1747)
    {
        // field initialization 
        this.MetersPerSecond_1747 = MetersPerSecond_1747;
        this.Zero_2731 = Velocity_114_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Velocity_114_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Velocity_114_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Velocity_114_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Velocity_114_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Velocity_114_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = Velocity_114_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = Velocity_114_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = Velocity_114_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = Velocity_114_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = Velocity_114_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = Velocity_114_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Velocity_114_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Velocity_114_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = Velocity_114_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static MetersPerSecond_1747 = function(self) { return self.MetersPerSecond_1747; }
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
    constructor(MetersPerSecondSquared_1754)
    {
        // field initialization 
        this.MetersPerSecondSquared_1754 = MetersPerSecondSquared_1754;
        this.Zero_2731 = Acceleration_115_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Acceleration_115_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Acceleration_115_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Acceleration_115_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Acceleration_115_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Acceleration_115_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = Acceleration_115_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = Acceleration_115_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = Acceleration_115_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = Acceleration_115_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = Acceleration_115_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = Acceleration_115_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Acceleration_115_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Acceleration_115_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = Acceleration_115_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static MetersPerSecondSquared_1754 = function(self) { return self.MetersPerSecondSquared_1754; }
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
    constructor(Newtons_1761)
    {
        // field initialization 
        this.Newtons_1761 = Newtons_1761;
        this.Zero_2731 = Force_116_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Force_116_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Force_116_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Force_116_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Force_116_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Force_116_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = Force_116_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = Force_116_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = Force_116_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = Force_116_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = Force_116_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = Force_116_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Force_116_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Force_116_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = Force_116_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Newtons_1761 = function(self) { return self.Newtons_1761; }
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
    constructor(Pascals_1768)
    {
        // field initialization 
        this.Pascals_1768 = Pascals_1768;
        this.Zero_2731 = Pressure_117_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Pressure_117_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Pressure_117_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Pressure_117_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Pressure_117_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Pressure_117_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = Pressure_117_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = Pressure_117_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = Pressure_117_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = Pressure_117_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = Pressure_117_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = Pressure_117_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Pressure_117_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Pressure_117_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = Pressure_117_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Pascals_1768 = function(self) { return self.Pascals_1768; }
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
    constructor(Joules_1775)
    {
        // field initialization 
        this.Joules_1775 = Joules_1775;
        this.Zero_2731 = Energy_118_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Energy_118_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Energy_118_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Energy_118_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Energy_118_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Energy_118_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = Energy_118_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = Energy_118_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = Energy_118_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = Energy_118_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = Energy_118_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = Energy_118_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Energy_118_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Energy_118_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = Energy_118_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Joules_1775 = function(self) { return self.Joules_1775; }
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
    constructor(Bytes_1782)
    {
        // field initialization 
        this.Bytes_1782 = Bytes_1782;
        this.Zero_2731 = Memory_119_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Memory_119_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Memory_119_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Memory_119_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Memory_119_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Memory_119_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = Memory_119_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = Memory_119_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = Memory_119_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = Memory_119_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = Memory_119_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = Memory_119_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Memory_119_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Memory_119_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = Memory_119_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Bytes_1782 = function(self) { return self.Bytes_1782; }
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
    constructor(Hertz_1789)
    {
        // field initialization 
        this.Hertz_1789 = Hertz_1789;
        this.Zero_2731 = Frequency_120_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Frequency_120_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Frequency_120_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Frequency_120_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Frequency_120_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Frequency_120_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = Frequency_120_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = Frequency_120_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = Frequency_120_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = Frequency_120_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = Frequency_120_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = Frequency_120_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Frequency_120_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Frequency_120_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = Frequency_120_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Hertz_1789 = function(self) { return self.Hertz_1789; }
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
    constructor(Decibels_1796)
    {
        // field initialization 
        this.Decibels_1796 = Decibels_1796;
        this.Zero_2731 = Loudness_121_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Loudness_121_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Loudness_121_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Loudness_121_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Loudness_121_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Loudness_121_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = Loudness_121_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = Loudness_121_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = Loudness_121_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = Loudness_121_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = Loudness_121_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = Loudness_121_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Loudness_121_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Loudness_121_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = Loudness_121_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Decibels_1796 = function(self) { return self.Decibels_1796; }
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
    constructor(Candelas_1803)
    {
        // field initialization 
        this.Candelas_1803 = Candelas_1803;
        this.Zero_2731 = LuminousIntensity_122_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = LuminousIntensity_122_Type.Value_23_Concept.One_2739;
        this.Default_2747 = LuminousIntensity_122_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = LuminousIntensity_122_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = LuminousIntensity_122_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = LuminousIntensity_122_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = LuminousIntensity_122_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = LuminousIntensity_122_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = LuminousIntensity_122_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = LuminousIntensity_122_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = LuminousIntensity_122_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = LuminousIntensity_122_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = LuminousIntensity_122_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = LuminousIntensity_122_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = LuminousIntensity_122_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Candelas_1803 = function(self) { return self.Candelas_1803; }
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
    constructor(Volts_1810)
    {
        // field initialization 
        this.Volts_1810 = Volts_1810;
        this.Zero_2731 = ElectricPotential_123_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = ElectricPotential_123_Type.Value_23_Concept.One_2739;
        this.Default_2747 = ElectricPotential_123_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = ElectricPotential_123_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = ElectricPotential_123_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = ElectricPotential_123_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = ElectricPotential_123_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = ElectricPotential_123_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = ElectricPotential_123_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = ElectricPotential_123_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = ElectricPotential_123_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = ElectricPotential_123_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = ElectricPotential_123_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = ElectricPotential_123_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = ElectricPotential_123_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Volts_1810 = function(self) { return self.Volts_1810; }
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
    constructor(Columbs_1817)
    {
        // field initialization 
        this.Columbs_1817 = Columbs_1817;
        this.Zero_2731 = ElectricCharge_124_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = ElectricCharge_124_Type.Value_23_Concept.One_2739;
        this.Default_2747 = ElectricCharge_124_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = ElectricCharge_124_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = ElectricCharge_124_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = ElectricCharge_124_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = ElectricCharge_124_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = ElectricCharge_124_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = ElectricCharge_124_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = ElectricCharge_124_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = ElectricCharge_124_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = ElectricCharge_124_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = ElectricCharge_124_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = ElectricCharge_124_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = ElectricCharge_124_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Columbs_1817 = function(self) { return self.Columbs_1817; }
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
    constructor(Amperes_1824)
    {
        // field initialization 
        this.Amperes_1824 = Amperes_1824;
        this.Zero_2731 = ElectricCurrent_125_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = ElectricCurrent_125_Type.Value_23_Concept.One_2739;
        this.Default_2747 = ElectricCurrent_125_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = ElectricCurrent_125_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = ElectricCurrent_125_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = ElectricCurrent_125_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = ElectricCurrent_125_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = ElectricCurrent_125_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = ElectricCurrent_125_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = ElectricCurrent_125_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = ElectricCurrent_125_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = ElectricCurrent_125_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = ElectricCurrent_125_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = ElectricCurrent_125_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = ElectricCurrent_125_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Amperes_1824 = function(self) { return self.Amperes_1824; }
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
    constructor(Ohms_1831)
    {
        // field initialization 
        this.Ohms_1831 = Ohms_1831;
        this.Zero_2731 = ElectricResistance_126_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = ElectricResistance_126_Type.Value_23_Concept.One_2739;
        this.Default_2747 = ElectricResistance_126_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = ElectricResistance_126_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = ElectricResistance_126_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = ElectricResistance_126_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = ElectricResistance_126_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = ElectricResistance_126_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = ElectricResistance_126_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = ElectricResistance_126_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = ElectricResistance_126_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = ElectricResistance_126_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = ElectricResistance_126_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = ElectricResistance_126_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = ElectricResistance_126_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Ohms_1831 = function(self) { return self.Ohms_1831; }
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
    constructor(Watts_1838)
    {
        // field initialization 
        this.Watts_1838 = Watts_1838;
        this.Zero_2731 = Power_127_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Power_127_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Power_127_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Power_127_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Power_127_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Power_127_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = Power_127_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = Power_127_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = Power_127_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = Power_127_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = Power_127_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = Power_127_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Power_127_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Power_127_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = Power_127_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static Watts_1838 = function(self) { return self.Watts_1838; }
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
    constructor(KilogramsPerMeterCubed_1845)
    {
        // field initialization 
        this.KilogramsPerMeterCubed_1845 = KilogramsPerMeterCubed_1845;
        this.Zero_2731 = Density_128_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Density_128_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Density_128_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Density_128_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Density_128_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Density_128_Type.Value_23_Concept.ToString_2775;
        this.Add_2623 = Density_128_Type.ScalarArithmetic_21_Concept.Add_2623;
        this.Subtract_2637 = Density_128_Type.ScalarArithmetic_21_Concept.Subtract_2637;
        this.Multiply_2651 = Density_128_Type.ScalarArithmetic_21_Concept.Multiply_2651;
        this.Divide_2665 = Density_128_Type.ScalarArithmetic_21_Concept.Divide_2665;
        this.Modulo_2679 = Density_128_Type.ScalarArithmetic_21_Concept.Modulo_2679;
        this.Equals_2521 = Density_128_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Density_128_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Density_128_Type.Magnitude_17_Concept.Magnitude_2464;
        this.Value_2448 = Density_128_Type.Measure_15_Concept.Value_2448;
    }
    // field accessors
    static KilogramsPerMeterCubed_1845 = function(self) { return self.KilogramsPerMeterCubed_1845; }
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
    constructor(Mean_1852, StandardDeviation_1859)
    {
        // field initialization 
        this.Mean_1852 = Mean_1852;
        this.StandardDeviation_1859 = StandardDeviation_1859;
        this.Zero_2731 = NormalDistribution_129_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = NormalDistribution_129_Type.Value_23_Concept.One_2739;
        this.Default_2747 = NormalDistribution_129_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = NormalDistribution_129_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = NormalDistribution_129_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = NormalDistribution_129_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Mean_1852 = function(self) { return self.Mean_1852; }
    static StandardDeviation_1859 = function(self) { return self.StandardDeviation_1859; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(NormalDistribution_129_Type);
    static Implements = [Value_23_Concept];
}
class PoissonDistribution_130_Type
{
    constructor(Expected_1866, Occurrences_1873)
    {
        // field initialization 
        this.Expected_1866 = Expected_1866;
        this.Occurrences_1873 = Occurrences_1873;
        this.Zero_2731 = PoissonDistribution_130_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = PoissonDistribution_130_Type.Value_23_Concept.One_2739;
        this.Default_2747 = PoissonDistribution_130_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = PoissonDistribution_130_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = PoissonDistribution_130_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = PoissonDistribution_130_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Expected_1866 = function(self) { return self.Expected_1866; }
    static Occurrences_1873 = function(self) { return self.Occurrences_1873; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(PoissonDistribution_130_Type);
    static Implements = [Value_23_Concept];
}
class BernoulliDistribution_131_Type
{
    constructor(P_1880)
    {
        // field initialization 
        this.P_1880 = P_1880;
        this.Zero_2731 = BernoulliDistribution_131_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = BernoulliDistribution_131_Type.Value_23_Concept.One_2739;
        this.Default_2747 = BernoulliDistribution_131_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = BernoulliDistribution_131_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = BernoulliDistribution_131_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = BernoulliDistribution_131_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static P_1880 = function(self) { return self.P_1880; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(BernoulliDistribution_131_Type);
    static Implements = [Value_23_Concept];
}
class Probability_132_Type
{
    constructor(Value_1887)
    {
        // field initialization 
        this.Value_1887 = Value_1887;
        this.Zero_2731 = Probability_132_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = Probability_132_Type.Value_23_Concept.One_2739;
        this.Default_2747 = Probability_132_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = Probability_132_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = Probability_132_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = Probability_132_Type.Value_23_Concept.ToString_2775;
        this.Add_2538 = Probability_132_Type.Arithmetic_20_Concept.Add_2538;
        this.Negative_2548 = Probability_132_Type.Arithmetic_20_Concept.Negative_2548;
        this.Reciprocal_2558 = Probability_132_Type.Arithmetic_20_Concept.Reciprocal_2558;
        this.Multiply_2575 = Probability_132_Type.Arithmetic_20_Concept.Multiply_2575;
        this.Divide_2592 = Probability_132_Type.Arithmetic_20_Concept.Divide_2592;
        this.Modulo_2609 = Probability_132_Type.Arithmetic_20_Concept.Modulo_2609;
        this.Equals_2521 = Probability_132_Type.Equatable_19_Concept.Equals_2521;
        this.Compare_2501 = Probability_132_Type.Comparable_18_Concept.Compare_2501;
        this.Magnitude_2464 = Probability_132_Type.Magnitude_17_Concept.Magnitude_2464;
    }
    // field accessors
    static Value_1887 = function(self) { return self.Value_1887; }
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
    constructor(Trials_1894, P_1901)
    {
        // field initialization 
        this.Trials_1894 = Trials_1894;
        this.P_1901 = P_1901;
        this.Zero_2731 = BinomialDistribution_133_Type.Value_23_Concept.Zero_2731;
        this.One_2739 = BinomialDistribution_133_Type.Value_23_Concept.One_2739;
        this.Default_2747 = BinomialDistribution_133_Type.Value_23_Concept.Default_2747;
        this.MinValue_2755 = BinomialDistribution_133_Type.Value_23_Concept.MinValue_2755;
        this.MaxValue_2763 = BinomialDistribution_133_Type.Value_23_Concept.MaxValue_2763;
        this.ToString_2775 = BinomialDistribution_133_Type.Value_23_Concept.ToString_2775;
    }
    // field accessors
    static Trials_1894 = function(self) { return self.Trials_1894; }
    static P_1901 = function(self) { return self.P_1901; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(BinomialDistribution_133_Type);
    static Implements = [Value_23_Concept];
}

// This is appended to every JavaScript program generated from Plato