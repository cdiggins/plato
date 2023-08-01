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
    static Cos_2184 = function (x_2183/* : Float_11 */) /* : Float_11 */{ return null; };
    static Sin_2187 = function (x_2186/* : Float_11 */) /* : Float_11 */{ return null; };
    static Tan_2190 = function (x_2189/* : Float_11 */) /* : Float_11 */{ return null; };
    static Acos_2193 = function (x_2192/* : Float_11 */) /* : Float_11 */{ return null; };
    static Asin_2196 = function (x_2195/* : Float_11 */) /* : Float_11 */{ return null; };
    static Atan_2199 = function (x_2198/* : Float_11 */) /* : Float_11 */{ return null; };
    static Cosh_2202 = function (x_2201/* : Float_11 */) /* : Float_11 */{ return null; };
    static Sinh_2205 = function (x_2204/* : Float_11 */) /* : Float_11 */{ return null; };
    static Tanh_2208 = function (x_2207/* : Float_11 */) /* : Float_11 */{ return null; };
    static Acosh_2211 = function (x_2210/* : Float_11 */) /* : Float_11 */{ return null; };
    static Asinh_2214 = function (x_2213/* : Float_11 */) /* : Float_11 */{ return null; };
    static Atanh_2217 = function (x_2216/* : Float_11 */) /* : Float_11 */{ return null; };
    static Pow_2222 = function (x_2219/* : Float_11 */, y_2221/* : Float_11 */) /* : Float_11 */{ return null; };
    static Log_2227 = function (x_2224/* : Float_11 */, y_2226/* : Float_11 */) /* : Float_11 */{ return null; };
    static NaturalLog_2230 = function (x_2229/* : Float_11 */) /* : Float_11 */{ return null; };
    static NaturalPower_2233 = function (x_2232/* : Float_11 */) /* : Float_11 */{ return null; };
    static Interpolate_2237 = function (xs_2236/* : Array_25 */) /* : String_8 */{ return null; };
    static Throw_2240 = function (x_2239/* : Any_5 */) /* : Any_5 */{ return null; };
    static TypeOf_2243 = function (x_2242/* : Any_5 */) /* : Type_12 */{ return null; };
    static Add_2248 = function (x_2245/* : Float_11 */, y_2247/* : Float_11 */) /* : Float_11 */{ return null; };
    static Subtract_2253 = function (x_2250/* : Float_11 */, y_2252/* : Float_11 */) /* : Float_11 */{ return null; };
    static Divide_2258 = function (x_2255/* : Float_11 */, y_2257/* : Float_11 */) /* : Float_11 */{ return null; };
    static Multiply_2263 = function (x_2260/* : Float_11 */, y_2262/* : Float_11 */) /* : Float_11 */{ return null; };
    static Modulo_2268 = function (x_2265/* : Float_11 */, y_2267/* : Float_11 */) /* : Float_11 */{ return null; };
    static Negative_2271 = function (x_2270/* : Float_11 */) /* : Float_11 */{ return null; };
    static Add_2276 = function (x_2273/* : Int_10 */, y_2275/* : Int_10 */) /* : Int_10 */{ return null; };
    static Subtract_2281 = function (x_2278/* : Int_10 */, y_2280/* : Int_10 */) /* : Int_10 */{ return null; };
    static Divide_2286 = function (x_2283/* : Int_10 */, y_2285/* : Int_10 */) /* : Int_10 */{ return null; };
    static Multiply_2291 = function (x_2288/* : Int_10 */, y_2290/* : Int_10 */) /* : Int_10 */{ return null; };
    static Modulo_2296 = function (x_2293/* : Int_10 */, y_2295/* : Int_10 */) /* : Int_10 */{ return null; };
    static Negative_2299 = function (x_2298/* : Int_10 */) /* : Int_10 */{ return null; };
    static And_2304 = function (x_2301/* : Bool_9 */, y_2303/* : Bool_9 */) /* : Bool_9 */{ return null; };
    static Or_2309 = function (x_2306/* : Bool_9 */, y_2308/* : Bool_9 */) /* : Bool_9 */{ return null; };
    static Not_2312 = function (x_2311/* : Bool_9 */) /* : Bool_9 */{ return null; };
}
class Interval_134_Library
{
    static Size_2699 = function (x_2686/* : Interval_24 */) /* : UnknownType */{ return Subtract_203/* : Float_11 */(Max_344/* : UnknownType */(x_2686/* : UnknownType */)/* : UnknownType */, Min_341/* : UnknownType */(x_2686/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static IsEmpty_2713 = function (x_2700/* : Interval_24 */) /* : UnknownType */{ return GreaterThanOrEquals_2072/* : UnknownType */(Min_341/* : UnknownType */(x_2700/* : UnknownType */)/* : UnknownType */, Max_344/* : UnknownType */(x_2700/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Lerp_2743 = function (x_2714/* : Interval_24 */, amount_2715/* : UnknownType */) /* : UnknownType */{ return Multiply_209/* : Float_11 */(Min_341/* : UnknownType */(x_2714/* : UnknownType */)/* : UnknownType */, Add_200/* : UnknownType */(Subtract_203/* : UnknownType */(1/* : Float_11 */, amount_2715/* : UnknownType */)/* : UnknownType */, Multiply_209/* : UnknownType */(Max_344/* : UnknownType */(x_2714/* : UnknownType */)/* : UnknownType */, amount_2715/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static InverseLerp_2763 = function (x_2744/* : Interval_24 */, value_2745/* : UnknownType */) /* : UnknownType */{ return Divide_206/* : Float_11 */(Subtract_203/* : UnknownType */(value_2745/* : UnknownType */, Min_341/* : UnknownType */(x_2744/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Size_1222/* : UnknownType */(x_2744/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static Negate_2783 = function (x_2764/* : Interval_24 */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(Negative_215/* : UnknownType */(Max_344/* : UnknownType */(x_2764/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Negative_215/* : UnknownType */(Min_341/* : UnknownType */(x_2764/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Reverse_2797 = function (x_2784/* : Interval_24 */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(Max_344/* : UnknownType */(x_2784/* : UnknownType */)/* : UnknownType */, Min_341/* : UnknownType */(x_2784/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Resize_2817 = function (x_2798/* : Interval_24 */, size_2799/* : UnknownType */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(Min_341/* : UnknownType */(x_2798/* : UnknownType */)/* : UnknownType */, Add_200/* : UnknownType */(Min_341/* : UnknownType */(x_2798/* : UnknownType */)/* : UnknownType */, size_2799/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Center_2825 = function (x_2818/* : Interval_24 */) /* : UnknownType */{ return Lerp_1932/* : Float_11 */(x_2818/* : UnknownType */, 0.5/* : Float_11 */)/* : Float_11 */; };
    static Contains_2850 = function (x_2826/* : Interval_24 */, value_2827/* : UnknownType */) /* : UnknownType */{ return LessThanOrEquals_2068/* : UnknownType */(Min_341/* : UnknownType */(x_2826/* : UnknownType */)/* : UnknownType */, And_236/* : UnknownType */(value_2827/* : UnknownType */, LessThanOrEquals_2068/* : UnknownType */(value_2827/* : UnknownType */, Max_344/* : UnknownType */(x_2826/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Contains_2878 = function (x_2851/* : Interval_24 */, other_2852/* : UnknownType */) /* : UnknownType */{ return LessThanOrEquals_2068/* : UnknownType */(Min_341/* : UnknownType */(x_2851/* : UnknownType */)/* : UnknownType */, And_236/* : UnknownType */(Min_341/* : UnknownType */(other_2852/* : UnknownType */)/* : UnknownType */, GreaterThanOrEquals_2072/* : UnknownType */(Max_344/* : UnknownType */, Max_344/* : UnknownType */(other_2852/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Overlaps_2893 = function (x_2879/* : Interval_24 */, y_2880/* : UnknownType */) /* : UnknownType */{ return Not_242/* : Bool_9 */(IsEmpty_1930/* : UnknownType */(Clamp_1968/* : UnknownType */(x_2879/* : UnknownType */, y_2880/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Bool_9 */; };
    static Split_2912 = function (x_2894/* : Interval_24 */, t_2895/* : Interval_134 */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(Left_1954/* : UnknownType */(x_2894/* : UnknownType */, t_2895/* : UnknownType */)/* : UnknownType */, Right_1956/* : UnknownType */(x_2894/* : UnknownType */, t_2895/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Split_2920 = function (x_2913/* : Interval_24 */) /* : UnknownType */{ return Split_1950/* : UnknownType */(x_2913/* : UnknownType */, 0.5/* : Float_11 */)/* : UnknownType */; };
    static Left_2937 = function (x_2921/* : Interval_24 */, t_2922/* : Interval_134 */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(Min_341/* : UnknownType */(x_2921/* : UnknownType */)/* : UnknownType */, Lerp_1932/* : UnknownType */(x_2921/* : UnknownType */, t_2922/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Right_2954 = function (x_2938/* : Interval_24 */, t_2939/* : Interval_134 */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(Lerp_1932/* : UnknownType */(x_2938/* : UnknownType */, t_2939/* : UnknownType */)/* : UnknownType */, Max_344/* : UnknownType */(x_2938/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static MoveTo_2971 = function (x_2955/* : Interval_24 */, t_2956/* : UnknownType */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(t_2956/* : UnknownType */, Add_200/* : UnknownType */(t_2956/* : UnknownType */, Size_1222/* : UnknownType */(x_2955/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static LeftHalf_2979 = function (x_2972/* : Interval_24 */) /* : UnknownType */{ return Left_1954/* : UnknownType */(x_2972/* : UnknownType */, 0.5/* : Float_11 */)/* : UnknownType */; };
    static RightHalf_2987 = function (x_2980/* : Interval_24 */) /* : UnknownType */{ return Right_1956/* : UnknownType */(x_2980/* : UnknownType */, 0.5/* : Float_11 */)/* : UnknownType */; };
    static HalfSize_2996 = function (x_2988/* : Interval_24 */) /* : UnknownType */{ return Half_2006/* : UnknownType */(Size_1222/* : UnknownType */(x_2988/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Recenter_3021 = function (x_2997/* : Interval_24 */, c_2998/* : UnknownType */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(Subtract_203/* : UnknownType */(c_2998/* : UnknownType */, HalfSize_1964/* : UnknownType */(x_2997/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Add_200/* : UnknownType */(c_2998/* : UnknownType */, HalfSize_1964/* : UnknownType */(x_2997/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Clamp_3046 = function (x_3022/* : Interval_24 */, y_3023/* : UnknownType */) /* : UnknownType */{ return Tuple_1/* : UnknownType */(Clamp_1968/* : UnknownType */(x_3022/* : UnknownType */, Min_341/* : UnknownType */(y_3023/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Clamp_1968/* : UnknownType */(x_3022/* : UnknownType */, Max_344/* : UnknownType */(y_3023/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Clamp_3078 = function (x_3047/* : Interval_24 */, value_3048/* : Comparable_137 */) /* : UnknownType */{ return LessThan_2062/* : UnknownType */(value_3048/* : UnknownType */, Min_341/* : UnknownType */(x_3047/* : UnknownType */)/* : UnknownType */
        ? Min_341/* : UnknownType */(x_3047/* : UnknownType */)/* : UnknownType */
        : GreaterThan_2070/* : UnknownType */(value_3048/* : UnknownType */, Max_344/* : UnknownType */(x_3047/* : UnknownType */)/* : UnknownType */
            ? Max_344/* : UnknownType */(x_3047/* : UnknownType */)/* : UnknownType */
            : value_3048/* : UnknownType */
        )/* : UnknownType */
    )/* : UnknownType */; };
    static Between_3103 = function (x_3079/* : Interval_24 */, value_3080/* : Comparable_137 */) /* : UnknownType */{ return GreaterThanOrEquals_2072/* : UnknownType */(value_3080/* : UnknownType */, And_236/* : UnknownType */(Min_341/* : UnknownType */(x_3079/* : UnknownType */)/* : UnknownType */, LessThanOrEquals_2068/* : UnknownType */(value_3080/* : UnknownType */, Max_344/* : UnknownType */(x_3079/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Unit_3110 = function () /* : UnknownType */{ return Tuple_1/* : UnknownType */(0/* : Int_10 */, 1/* : Int_10 */)/* : UnknownType */; };
}
class Vector_135_Library
{
    static Sum_3120 = function (v_3111/* : Vector_14 */) /* : UnknownType */{ return Aggregate_2098/* : UnknownType */(v_3111/* : UnknownType */, 0/* : Int_10 */, Add_200/* : UnknownType */)/* : UnknownType */; };
    static SumSquares_3133 = function (v_3121/* : Vector_14 */) /* : UnknownType */{ return Aggregate_2098/* : UnknownType */(Square_1988/* : UnknownType */(v_3121/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */, Add_200/* : UnknownType */)/* : UnknownType */; };
    static LengthSquared_3139 = function (v_3134/* : Vector_14 */) /* : UnknownType */{ return SumSquares_1978/* : UnknownType */(v_3134/* : UnknownType */)/* : UnknownType */; };
    static Length_3148 = function (v_3140/* : Vector_14 */) /* : UnknownType */{ return SquareRoot_1986/* : UnknownType */(LengthSquared_1980/* : UnknownType */(v_3140/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Dot_3160 = function (v1_3149/* : Vector_14 */, v2_3150/* : UnknownType */) /* : UnknownType */{ return Sum_1976/* : UnknownType */(Multiply_209/* : UnknownType */(v1_3149/* : UnknownType */, v2_3150/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Numerical_136_Library
{
    static SquareRoot_3168 = function (x_3161/* : Numerical_16 */) /* : UnknownType */{ return Pow_179/* : Float_11 */(x_3161/* : UnknownType */, 0.5/* : Float_11 */)/* : Float_11 */; };
    static Square_3176 = function (x_3169/* : Numerical_16 */) /* : UnknownType */{ return Multiply_209/* : Float_11 */(Value_354/* : UnknownType */, Value_354/* : UnknownType */)/* : Float_11 */; };
    static Clamp_3191 = function (x_3177/* : Numerical_16 */, min_3178/* : Any_5 */, max_3179/* : Any_5 */) /* : UnknownType */{ return Clamp_1968/* : UnknownType */(x_3177/* : UnknownType */, Tuple_1/* : UnknownType */(min_3178/* : UnknownType */, max_3179/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Clamp_3200 = function (x_3192/* : Numerical_16 */, i_3193/* : UnknownType */) /* : UnknownType */{ return Clamp_1968/* : UnknownType */(i_3193/* : UnknownType */, x_3192/* : UnknownType */)/* : UnknownType */; };
    static Clamp_3210 = function (x_3201/* : Numerical_16 */) /* : UnknownType */{ return Clamp_1968/* : UnknownType */(x_3201/* : UnknownType */, 0/* : Int_10 */, 1/* : Int_10 */)/* : UnknownType */; };
    static PlusOne_3218 = function (x_3211/* : Numerical_16 */) /* : UnknownType */{ return Add_200/* : Float_11 */(x_3211/* : UnknownType */, 1/* : Int_10 */)/* : Float_11 */; };
    static MinusOne_3226 = function (x_3219/* : Numerical_16 */) /* : UnknownType */{ return Subtract_203/* : Float_11 */(x_3219/* : UnknownType */, 1/* : Int_10 */)/* : Float_11 */; };
    static FromOne_3234 = function (x_3227/* : Numerical_16 */) /* : UnknownType */{ return Subtract_203/* : Float_11 */(1/* : Int_10 */, x_3227/* : UnknownType */)/* : Float_11 */; };
    static Sign_3256 = function (x_3235/* : Numerical_16 */) /* : UnknownType */{ return LessThan_2062/* : UnknownType */(x_3235/* : UnknownType */, 0/* : Int_10 */
        ? Negative_215/* : UnknownType */(1/* : Int_10 */)/* : UnknownType */
        : GreaterThan_2070/* : UnknownType */(x_3235/* : UnknownType */, 0/* : Int_10 */
            ? 1/* : Int_10 */
            : 0/* : Int_10 */
        )/* : UnknownType */
    )/* : UnknownType */; };
    static Abs_3270 = function (x_3257/* : Numerical_16 */) /* : UnknownType */{ return LessThan_2062/* : UnknownType */(Value_354/* : UnknownType */, 0/* : Int_10 */
        ? Negative_215/* : UnknownType */(Value_354/* : UnknownType */)/* : UnknownType */
        : Value_354/* : UnknownType */
    )/* : UnknownType */; };
    static Half_3278 = function (x_3271/* : Numerical_16 */) /* : UnknownType */{ return Divide_206/* : Float_11 */(x_3271/* : UnknownType */, 2/* : Int_10 */)/* : Float_11 */; };
    static Third_3286 = function (x_3279/* : Numerical_16 */) /* : UnknownType */{ return Divide_206/* : Float_11 */(x_3279/* : UnknownType */, 3/* : Int_10 */)/* : Float_11 */; };
    static Quarter_3294 = function (x_3287/* : Numerical_16 */) /* : UnknownType */{ return Divide_206/* : Float_11 */(x_3287/* : UnknownType */, 4/* : Int_10 */)/* : Float_11 */; };
    static Fifth_3302 = function (x_3295/* : Numerical_16 */) /* : UnknownType */{ return Divide_206/* : Float_11 */(x_3295/* : UnknownType */, 5/* : Int_10 */)/* : Float_11 */; };
    static Sixth_3310 = function (x_3303/* : Numerical_16 */) /* : UnknownType */{ return Divide_206/* : Float_11 */(x_3303/* : UnknownType */, 6/* : Int_10 */)/* : Float_11 */; };
    static Seventh_3318 = function (x_3311/* : Numerical_16 */) /* : UnknownType */{ return Divide_206/* : Float_11 */(x_3311/* : UnknownType */, 7/* : Int_10 */)/* : Float_11 */; };
    static Eighth_3326 = function (x_3319/* : Numerical_16 */) /* : UnknownType */{ return Divide_206/* : Float_11 */(x_3319/* : UnknownType */, 8/* : Int_10 */)/* : Float_11 */; };
    static Ninth_3334 = function (x_3327/* : Numerical_16 */) /* : UnknownType */{ return Divide_206/* : Float_11 */(x_3327/* : UnknownType */, 9/* : Int_10 */)/* : Float_11 */; };
    static Tenth_3342 = function (x_3335/* : Numerical_16 */) /* : UnknownType */{ return Divide_206/* : Float_11 */(x_3335/* : UnknownType */, 10/* : Int_10 */)/* : Float_11 */; };
    static Sixteenth_3350 = function (x_3343/* : Numerical_16 */) /* : UnknownType */{ return Divide_206/* : Float_11 */(x_3343/* : UnknownType */, 16/* : Int_10 */)/* : Float_11 */; };
    static Hundredth_3358 = function (x_3351/* : Numerical_16 */) /* : UnknownType */{ return Divide_206/* : Float_11 */(x_3351/* : UnknownType */, 100/* : Int_10 */)/* : Float_11 */; };
    static Thousandth_3366 = function (x_3359/* : Numerical_16 */) /* : UnknownType */{ return Divide_206/* : Float_11 */(x_3359/* : UnknownType */, 1000/* : Int_10 */)/* : Float_11 */; };
    static Millionth_3379 = function (x_3367/* : Numerical_16 */) /* : UnknownType */{ return Divide_206/* : Float_11 */(x_3367/* : UnknownType */, Divide_206/* : UnknownType */(1000/* : Int_10 */, 1000/* : Int_10 */)/* : UnknownType */)/* : Float_11 */; };
    static Billionth_3397 = function (x_3380/* : Numerical_16 */) /* : UnknownType */{ return Divide_206/* : Float_11 */(x_3380/* : UnknownType */, Divide_206/* : UnknownType */(1000/* : Int_10 */, Divide_206/* : UnknownType */(1000/* : Int_10 */, 1000/* : Int_10 */)/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static Hundred_3405 = function (x_3398/* : Numerical_16 */) /* : UnknownType */{ return Multiply_209/* : Float_11 */(x_3398/* : UnknownType */, 100/* : Int_10 */)/* : Float_11 */; };
    static Thousand_3413 = function (x_3406/* : Numerical_16 */) /* : UnknownType */{ return Multiply_209/* : Float_11 */(x_3406/* : UnknownType */, 1000/* : Int_10 */)/* : Float_11 */; };
    static Million_3426 = function (x_3414/* : Numerical_16 */) /* : UnknownType */{ return Multiply_209/* : Float_11 */(x_3414/* : UnknownType */, Multiply_209/* : UnknownType */(1000/* : Int_10 */, 1000/* : Int_10 */)/* : UnknownType */)/* : Float_11 */; };
    static Billion_3444 = function (x_3427/* : Numerical_16 */) /* : UnknownType */{ return Multiply_209/* : Float_11 */(x_3427/* : UnknownType */, Multiply_209/* : UnknownType */(1000/* : Int_10 */, Multiply_209/* : UnknownType */(1000/* : Int_10 */, 1000/* : Int_10 */)/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static Twice_3452 = function (x_3445/* : Numerical_16 */) /* : UnknownType */{ return Multiply_209/* : Float_11 */(x_3445/* : UnknownType */, 2/* : Int_10 */)/* : Float_11 */; };
    static Thrice_3460 = function (x_3453/* : Numerical_16 */) /* : UnknownType */{ return Multiply_209/* : Float_11 */(x_3453/* : UnknownType */, 3/* : Int_10 */)/* : Float_11 */; };
    static SmoothStep_3479 = function (x_3461/* : Numerical_16 */) /* : UnknownType */{ return Multiply_209/* : Float_11 */(Square_1988/* : UnknownType */(x_3461/* : UnknownType */)/* : UnknownType */, Subtract_203/* : UnknownType */(3/* : Int_10 */, Twice_2042/* : UnknownType */(x_3461/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static Pow2_3487 = function (x_3480/* : Numerical_16 */) /* : UnknownType */{ return Multiply_209/* : Float_11 */(x_3480/* : UnknownType */, x_3480/* : UnknownType */)/* : Float_11 */; };
    static Pow3_3498 = function (x_3488/* : Numerical_16 */) /* : UnknownType */{ return Multiply_209/* : Float_11 */(Pow2_2048/* : UnknownType */(x_3488/* : UnknownType */)/* : UnknownType */, x_3488/* : UnknownType */)/* : Float_11 */; };
    static Pow4_3509 = function (x_3499/* : Numerical_16 */) /* : UnknownType */{ return Multiply_209/* : Float_11 */(Pow3_2050/* : UnknownType */(x_3499/* : UnknownType */)/* : UnknownType */, x_3499/* : UnknownType */)/* : Float_11 */; };
    static Pow5_3520 = function (x_3510/* : Numerical_16 */) /* : UnknownType */{ return Multiply_209/* : Float_11 */(Pow4_2052/* : UnknownType */(x_3510/* : UnknownType */)/* : UnknownType */, x_3510/* : UnknownType */)/* : Float_11 */; };
    static Turns_3533 = function (x_3521/* : Numerical_16 */) /* : UnknownType */{ return Multiply_209/* : Float_11 */(x_3521/* : UnknownType */, Multiply_209/* : UnknownType */(3.1415926535897/* : Float_11 */, 2/* : Int_10 */)/* : UnknownType */)/* : Float_11 */; };
    static AlmostZero_3544 = function (x_3534/* : Numerical_16 */) /* : UnknownType */{ return LessThan_2062/* : UnknownType */(Abs_2004/* : UnknownType */(x_3534/* : UnknownType */)/* : UnknownType */, 1E-08/* : Float_11 */)/* : UnknownType */; };
}
class Comparable_137_Library
{
    static Equals_3558 = function (a_3545/* : Comparable_18 */, b_3546/* : Comparable_18 */) /* : UnknownType */{ return Equals_261/* : Boolean_22 */(Compare_258/* : UnknownType */(a_3545/* : UnknownType */, b_3546/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_22 */; };
    static LessThan_3572 = function (a_3559/* : Comparable_18 */, b_3560/* : Comparable_18 */) /* : UnknownType */{ return LessThan_2062/* : UnknownType */(Compare_258/* : UnknownType */(a_3559/* : UnknownType */, b_3560/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : UnknownType */; };
    static Lesser_3584 = function (a_3573/* : Comparable_18 */, b_3574/* : Comparable_137 */) /* : UnknownType */{ return LessThanOrEquals_2068/* : UnknownType */(a_3573/* : UnknownType */, b_3574/* : UnknownType */)/* : UnknownType */
        ? a_3573/* : Comparable_18 */
        : b_3574/* : Comparable_137 */
    ; };
    static Greater_3596 = function (a_3585/* : Comparable_18 */, b_3586/* : Comparable_137 */) /* : UnknownType */{ return GreaterThanOrEquals_2072/* : UnknownType */(a_3585/* : UnknownType */, b_3586/* : UnknownType */)/* : UnknownType */
        ? a_3585/* : Comparable_18 */
        : b_3586/* : Comparable_137 */
    ; };
    static LessThanOrEquals_3610 = function (a_3597/* : Comparable_18 */, b_3598/* : Comparable_18 */) /* : UnknownType */{ return LessThanOrEquals_2068/* : UnknownType */(Compare_258/* : UnknownType */(a_3597/* : UnknownType */, b_3598/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : UnknownType */; };
    static GreaterThan_3624 = function (a_3611/* : Comparable_18 */, b_3612/* : Comparable_18 */) /* : UnknownType */{ return GreaterThan_2070/* : UnknownType */(Compare_258/* : UnknownType */(a_3611/* : UnknownType */, b_3612/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : UnknownType */; };
    static GreaterThanOrEquals_3638 = function (a_3625/* : Comparable_18 */, b_3626/* : Comparable_18 */) /* : UnknownType */{ return GreaterThanOrEquals_2072/* : UnknownType */(Compare_258/* : UnknownType */(a_3625/* : UnknownType */, b_3626/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : UnknownType */; };
    static Min_3650 = function (a_3639/* : Comparable_18 */, b_3640/* : Comparable_137 */) /* : UnknownType */{ return LessThan_2062/* : UnknownType */(a_3639/* : UnknownType */, b_3640/* : UnknownType */)/* : UnknownType */
        ? a_3639/* : Comparable_18 */
        : b_3640/* : Comparable_137 */
    ; };
    static Max_3662 = function (a_3651/* : Comparable_18 */, b_3652/* : Comparable_137 */) /* : UnknownType */{ return GreaterThan_2070/* : UnknownType */(a_3651/* : UnknownType */, b_3652/* : UnknownType */)/* : UnknownType */
        ? a_3651/* : Comparable_18 */
        : b_3652/* : Comparable_137 */
    ; };
    static Between_3677 = function (v_3663/* : Comparable_18 */, a_3664/* : Any_5 */, b_3665/* : Any_5 */) /* : UnknownType */{ return Between_1972/* : UnknownType */(v_3663/* : UnknownType */, Interval_134_Library/* : UnknownType */(a_3664/* : UnknownType */, b_3665/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Between_3686 = function (v_3678/* : Comparable_18 */, i_3679/* : Interval_134 */) /* : UnknownType */{ return Contains_1944/* : UnknownType */(i_3679/* : UnknownType */, v_3678/* : UnknownType */)/* : UnknownType */; };
}
class Boolean_138_Library
{
    static XOr_3696 = function (a_3687/* : Boolean_22 */, b_3688/* : UnknownType */) /* : UnknownType */{ return a_3687/* : UnknownType */
        ? Not_242/* : Bool_9 */(b_3688/* : UnknownType */)/* : Bool_9 */
        : b_3688/* : UnknownType */
    ; };
    static NAnd_3708 = function (a_3697/* : Boolean_22 */, b_3698/* : UnknownType */) /* : UnknownType */{ return Not_242/* : Bool_9 */(And_236/* : UnknownType */(a_3697/* : UnknownType */, b_3698/* : UnknownType */)/* : UnknownType */)/* : Bool_9 */; };
    static NOr_3720 = function (a_3709/* : Boolean_22 */, b_3710/* : UnknownType */) /* : UnknownType */{ return Not_242/* : Bool_9 */(Or_239/* : UnknownType */(a_3709/* : UnknownType */, b_3710/* : UnknownType */)/* : UnknownType */)/* : Bool_9 */; };
}
class Equatable_139_Library
{
    static NotEquals_3729 = function (x_3721/* : Equatable_19 */) /* : UnknownType */{ return Not_242/* : Bool_9 */(Equals_261/* : UnknownType */(x_3721/* : UnknownType */)/* : UnknownType */)/* : Bool_9 */; };
}
class Array_140_Library
{
    static Map_3752 = function (xs_3730/* : Array_25 */, f_3731/* : Function_4 */) /* : UnknownType */{ return Map_2090/* : UnknownType */(Count_27_Type/* : UnknownType */(xs_3730/* : UnknownType */)/* : UnknownType */, function (i_3737/* : UnknownType */) /* : Lambda_3 */{ return f_3731/* : UnknownType */(At_249/* : UnknownType */(xs_3730/* : UnknownType */, i_3737/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */; };
    static Zip_3781 = function (xs_3753/* : Array_25 */, ys_3754/* : UnknownType */, f_3755/* : Function_4 */) /* : UnknownType */{ return Array_140_Library/* : Array_140 */(Count_27_Type/* : UnknownType */(xs_3753/* : UnknownType */)/* : UnknownType */, function (i_3761/* : UnknownType */) /* : Lambda_3 */{ return f_3755/* : UnknownType */(At_249/* : UnknownType */(i_3761/* : UnknownType */)/* : UnknownType */, At_249/* : UnknownType */(ys_3754/* : UnknownType */, i_3761/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Array_140 */; };
    static Skip_3806 = function (xs_3782/* : Array_25 */, n_3783/* : UnknownType */) /* : UnknownType */{ return Array_140_Library/* : Array_140 */(Subtract_203/* : UnknownType */(Count_27_Type/* : UnknownType */, n_3783/* : UnknownType */)/* : UnknownType */, function (i_3791/* : UnknownType */) /* : Lambda_3 */{ return At_249/* : UnknownType */(Subtract_203/* : UnknownType */(i_3791/* : UnknownType */, n_3783/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Array_140 */; };
    static Take_3818 = function (xs_3807/* : Array_25 */, n_3808/* : Any_5 */) /* : UnknownType */{ return Array_140_Library/* : Array_140 */(n_3808/* : UnknownType */, function (i_3811/* : UnknownType */) /* : Lambda_3 */{ return At_249/* : UnknownType */; })/* : Array_140 */; };
    static Aggregate_3840 = function (xs_3819/* : Array_25 */, init_3820/* : Any_5 */, f_3821/* : Function_4 */) /* : UnknownType */{ return IsEmpty_1930/* : UnknownType */(xs_3819/* : UnknownType */)/* : UnknownType */
        ? init_3820/* : Any_5 */
        : f_3821/* : Function_4 */(init_3820/* : UnknownType */, f_3821/* : UnknownType */(Rest_2100/* : UnknownType */(xs_3819/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Function_4 */
    ; };
    static Rest_3848 = function (xs_3841/* : Array_25 */) /* : UnknownType */{ return Skip_2094/* : Array_140 */(xs_3841/* : UnknownType */, 1/* : Int_10 */)/* : Array_140 */; };
    static IsEmpty_3859 = function (xs_3849/* : Array_25 */) /* : UnknownType */{ return Equals_261/* : Boolean_22 */(Count_27_Type/* : UnknownType */(xs_3849/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_22 */; };
    static First_3867 = function (xs_3860/* : Array_25 */) /* : UnknownType */{ return At_249/* : T_244 */(xs_3860/* : UnknownType */, 0/* : Int_10 */)/* : T_244 */; };
    static Last_3883 = function (xs_3868/* : Array_25 */) /* : UnknownType */{ return At_249/* : T_244 */(xs_3868/* : UnknownType */, Subtract_203/* : UnknownType */(Count_27_Type/* : UnknownType */(xs_3868/* : UnknownType */)/* : UnknownType */, 1/* : Int_10 */)/* : UnknownType */)/* : T_244 */; };
    static Slice_3898 = function (xs_3884/* : Array_25 */, from_3885/* : Array_140 */, count_3886/* : Array_140 */) /* : UnknownType */{ return Take_2096/* : Array_140 */(Skip_2094/* : UnknownType */(xs_3884/* : UnknownType */, from_3885/* : UnknownType */)/* : UnknownType */, count_3886/* : UnknownType */)/* : Array_140 */; };
    static Join_3942 = function (xs_3899/* : Array_25 */, sep_3900/* : Intrinsics_13 */) /* : UnknownType */{ return IsEmpty_1930/* : UnknownType */(xs_3899/* : UnknownType */)/* : UnknownType */
        ? ""/* : String_8 */
        : Add_200/* : Float_11 */(ToString_337/* : UnknownType */(First_2104/* : UnknownType */(xs_3899/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Aggregate_2098/* : UnknownType */(Skip_2094/* : UnknownType */(xs_3899/* : UnknownType */, 1/* : Int_10 */)/* : UnknownType */, ""/* : String_8 */, function (acc_3923/* : UnknownType */, cur_3924/* : UnknownType */) /* : Lambda_3 */{ return Interpolate_191/* : UnknownType */(acc_3923/* : UnknownType */, sep_3900/* : UnknownType */, cur_3924/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */)/* : Float_11 */
    ; };
    static All_3969 = function (xs_3943/* : Array_25 */, f_3944/* : Function_4 */) /* : UnknownType */{ return IsEmpty_1930/* : UnknownType */(xs_3943/* : UnknownType */)/* : UnknownType */
        ? True/* : Bool_9 */
        : And_236/* : Bool_9 */(f_3944/* : UnknownType */(First_2104/* : UnknownType */(xs_3943/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, f_3944/* : UnknownType */(Rest_2100/* : UnknownType */(xs_3943/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Bool_9 */
    ; };
    static JoinStrings_4014 = function (xs_3970/* : Array_25 */, sep_3971/* : Any_5 */) /* : UnknownType */{ return IsEmpty_1930/* : UnknownType */(xs_3970/* : UnknownType */)/* : UnknownType */
        ? ""/* : String_8 */
        : Add_200/* : Float_11 */(First_2104/* : UnknownType */(xs_3970/* : UnknownType */)/* : UnknownType */, Aggregate_2098/* : UnknownType */(Rest_2100/* : UnknownType */(xs_3970/* : UnknownType */)/* : UnknownType */, ""/* : String_8 */, function (x_3989/* : UnknownType */, acc_3990/* : UnknownType */) /* : Lambda_3 */{ return Add_200/* : UnknownType */(acc_3990/* : UnknownType */, Add_200/* : UnknownType */(", "/* : String_8 */, ToString_337/* : UnknownType */(x_3989/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */)/* : Float_11 */
    ; };
}
class Easings_141_Library
{
    static BlendEaseFunc_4063 = function (p_4015/* : UnknownType */, easeIn_4016/* : Function_4 */, easeOut_4017/* : Function_4 */) /* : UnknownType */{ return LessThan_2062/* : UnknownType */(p_4015/* : UnknownType */, 0.5/* : Float_11 */
        ? Multiply_209/* : UnknownType */(0.5/* : Float_11 */, easeIn_4016/* : UnknownType */(Multiply_209/* : UnknownType */(p_4015/* : UnknownType */, 2/* : Int_10 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
        : Multiply_209/* : UnknownType */(0.5/* : Float_11 */, Add_200/* : UnknownType */(easeOut_4017/* : UnknownType */(Multiply_209/* : UnknownType */(p_4015/* : UnknownType */, Subtract_203/* : UnknownType */(2/* : Int_10 */, 1/* : Int_10 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, 0.5/* : Float_11 */)/* : UnknownType */)/* : UnknownType */
    )/* : UnknownType */; };
    static InvertEaseFunc_4080 = function (p_4064/* : UnknownType */, easeIn_4065/* : Function_4 */) /* : UnknownType */{ return Subtract_203/* : Float_11 */(1/* : Int_10 */, easeIn_4065/* : UnknownType */(Subtract_203/* : UnknownType */(1/* : Int_10 */, p_4064/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static Linear_4083 = function (p_4081/* : Any_5 */) /* : UnknownType */{ return p_4081/* : Any_5 */; };
    static QuadraticEaseIn_4089 = function (p_4084/* : Numerical_136 */) /* : UnknownType */{ return Pow2_2048/* : Float_11 */(p_4084/* : UnknownType */)/* : Float_11 */; };
    static QuadraticEaseOut_4097 = function (p_4090/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2118/* : Float_11 */(p_4090/* : UnknownType */, QuadraticEaseIn_2122/* : UnknownType */)/* : Float_11 */; };
    static QuadraticEaseInOut_4107 = function (p_4098/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2116/* : UnknownType */(p_4098/* : UnknownType */, QuadraticEaseIn_2122/* : UnknownType */, QuadraticEaseOut_2124/* : UnknownType */)/* : UnknownType */; };
    static CubicEaseIn_4113 = function (p_4108/* : Numerical_136 */) /* : UnknownType */{ return Pow3_2050/* : Float_11 */(p_4108/* : UnknownType */)/* : Float_11 */; };
    static CubicEaseOut_4121 = function (p_4114/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2118/* : Float_11 */(p_4114/* : UnknownType */, CubicEaseIn_2128/* : UnknownType */)/* : Float_11 */; };
    static CubicEaseInOut_4131 = function (p_4122/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2116/* : UnknownType */(p_4122/* : UnknownType */, CubicEaseIn_2128/* : UnknownType */, CubicEaseOut_2130/* : UnknownType */)/* : UnknownType */; };
    static QuarticEaseIn_4137 = function (p_4132/* : Numerical_136 */) /* : UnknownType */{ return Pow4_2052/* : Float_11 */(p_4132/* : UnknownType */)/* : Float_11 */; };
    static QuarticEaseOut_4145 = function (p_4138/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2118/* : Float_11 */(p_4138/* : UnknownType */, QuarticEaseIn_2134/* : UnknownType */)/* : Float_11 */; };
    static QuarticEaseInOut_4155 = function (p_4146/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2116/* : UnknownType */(p_4146/* : UnknownType */, QuarticEaseIn_2134/* : UnknownType */, QuarticEaseOut_2136/* : UnknownType */)/* : UnknownType */; };
    static QuinticEaseIn_4161 = function (p_4156/* : Numerical_136 */) /* : UnknownType */{ return Pow5_2054/* : Float_11 */(p_4156/* : UnknownType */)/* : Float_11 */; };
    static QuinticEaseOut_4169 = function (p_4162/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2118/* : Float_11 */(p_4162/* : UnknownType */, QuinticEaseIn_2140/* : UnknownType */)/* : Float_11 */; };
    static QuinticEaseInOut_4179 = function (p_4170/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2116/* : UnknownType */(p_4170/* : UnknownType */, QuinticEaseIn_2140/* : UnknownType */, QuinticEaseOut_2142/* : UnknownType */)/* : UnknownType */; };
    static SineEaseIn_4187 = function (p_4180/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2118/* : Float_11 */(p_4180/* : UnknownType */, SineEaseOut_2148/* : UnknownType */)/* : Float_11 */; };
    static SineEaseOut_4199 = function (p_4188/* : Numerical_136 */) /* : UnknownType */{ return Sin_146/* : Float_11 */(Turns_2056/* : UnknownType */(Quarter_2010/* : UnknownType */(p_4188/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static SineEaseInOut_4209 = function (p_4200/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2116/* : UnknownType */(p_4200/* : UnknownType */, SineEaseIn_2146/* : UnknownType */, SineEaseOut_2148/* : UnknownType */)/* : UnknownType */; };
    static CircularEaseIn_4224 = function (p_4210/* : Numerical_136 */) /* : UnknownType */{ return FromOne_2000/* : Float_11 */(SquareRoot_1986/* : UnknownType */(FromOne_2000/* : UnknownType */(Pow2_2048/* : UnknownType */(p_4210/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static CircularEaseOut_4232 = function (p_4225/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2118/* : Float_11 */(p_4225/* : UnknownType */, CircularEaseIn_2152/* : UnknownType */)/* : Float_11 */; };
    static CircularEaseInOut_4242 = function (p_4233/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2116/* : UnknownType */(p_4233/* : UnknownType */, CircularEaseIn_2152/* : UnknownType */, CircularEaseOut_2154/* : UnknownType */)/* : UnknownType */; };
    static ExponentialEaseIn_4264 = function (p_4243/* : Numerical_136 */) /* : UnknownType */{ return AlmostZero_2058/* : UnknownType */(p_4243/* : UnknownType */)/* : UnknownType */
        ? p_4243/* : Numerical_136 */
        : Pow_179/* : Float_11 */(2/* : Int_10 */, Multiply_209/* : UnknownType */(10/* : Int_10 */, MinusOne_1998/* : UnknownType */(p_4243/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Float_11 */
    ; };
    static ExponentialEaseOut_4272 = function (p_4265/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2118/* : Float_11 */(p_4265/* : UnknownType */, ExponentialEaseIn_2158/* : UnknownType */)/* : Float_11 */; };
    static ExponentialEaseInOut_4282 = function (p_4273/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2116/* : UnknownType */(p_4273/* : UnknownType */, ExponentialEaseIn_2158/* : UnknownType */, ExponentialEaseOut_2160/* : UnknownType */)/* : UnknownType */; };
    static ElasticEaseIn_4320 = function (p_4283/* : Numerical_136 */) /* : UnknownType */{ return Multiply_209/* : Float_11 */(13/* : Int_10 */, Multiply_209/* : UnknownType */(Turns_2056/* : UnknownType */(Quarter_2010/* : UnknownType */(p_4283/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Sin_146/* : UnknownType */(Radians_1250/* : UnknownType */(Pow_179/* : UnknownType */(2/* : Int_10 */, Multiply_209/* : UnknownType */(10/* : Int_10 */, MinusOne_1998/* : UnknownType */(p_4283/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static ElasticEaseOut_4328 = function (p_4321/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2118/* : Float_11 */(p_4321/* : UnknownType */, ElasticEaseIn_2164/* : UnknownType */)/* : Float_11 */; };
    static ElasticEaseInOut_4338 = function (p_4329/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2116/* : UnknownType */(p_4329/* : UnknownType */, ElasticEaseIn_2164/* : UnknownType */, ElasticEaseOut_2166/* : UnknownType */)/* : UnknownType */; };
    static BackEaseIn_4363 = function (p_4339/* : UnknownType */) /* : UnknownType */{ return Subtract_203/* : Float_11 */(Pow3_2050/* : UnknownType */(p_4339/* : UnknownType */)/* : UnknownType */, Multiply_209/* : UnknownType */(p_4339/* : UnknownType */, Sin_146/* : UnknownType */(Turns_2056/* : UnknownType */(Half_2006/* : UnknownType */(p_4339/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static BackEaseOut_4371 = function (p_4364/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2118/* : Float_11 */(p_4364/* : UnknownType */, BackEaseIn_2170/* : UnknownType */)/* : Float_11 */; };
    static BackEaseInOut_4381 = function (p_4372/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2116/* : UnknownType */(p_4372/* : UnknownType */, BackEaseIn_2170/* : UnknownType */, BackEaseOut_2172/* : UnknownType */)/* : UnknownType */; };
    static BounceEaseIn_4389 = function (p_4382/* : Easings_141 */) /* : UnknownType */{ return InvertEaseFunc_2118/* : Float_11 */(p_4382/* : UnknownType */, BounceEaseOut_2178/* : UnknownType */)/* : Float_11 */; };
    static BounceEaseOut_4558 = function (p_4390/* : UnknownType */) /* : UnknownType */{ return LessThan_2062/* : UnknownType */(p_4390/* : UnknownType */, Divide_206/* : UnknownType */(4/* : Int_10 */, 11/* : Float_11 */)/* : UnknownType */)/* : UnknownType */
        ? Multiply_209/* : Float_11 */(121/* : Float_11 */, Divide_206/* : UnknownType */(Pow2_2048/* : UnknownType */(p_4390/* : UnknownType */)/* : UnknownType */, 16/* : Float_11 */)/* : UnknownType */)/* : Float_11 */
        : LessThan_2062/* : UnknownType */(p_4390/* : UnknownType */, Divide_206/* : UnknownType */(8/* : Int_10 */, 11/* : Float_11 */)/* : UnknownType */)/* : UnknownType */
            ? Divide_206/* : Float_11 */(363/* : Float_11 */, Multiply_209/* : UnknownType */(40/* : Float_11 */, Subtract_203/* : UnknownType */(Pow2_2048/* : UnknownType */(p_4390/* : UnknownType */)/* : UnknownType */, Divide_206/* : UnknownType */(99/* : Float_11 */, Multiply_209/* : UnknownType */(10/* : Float_11 */, Add_200/* : UnknownType */(p_4390/* : UnknownType */, Divide_206/* : UnknownType */(17/* : Float_11 */, 5/* : Float_11 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Float_11 */
            : LessThan_2062/* : UnknownType */(p_4390/* : UnknownType */, Divide_206/* : UnknownType */(9/* : Int_10 */, 10/* : Float_11 */)/* : UnknownType */)/* : UnknownType */
                ? Divide_206/* : Float_11 */(4356/* : Float_11 */, Multiply_209/* : UnknownType */(361/* : Float_11 */, Subtract_203/* : UnknownType */(Pow2_2048/* : UnknownType */(p_4390/* : UnknownType */)/* : UnknownType */, Divide_206/* : UnknownType */(35442/* : Float_11 */, Multiply_209/* : UnknownType */(1805/* : Float_11 */, Add_200/* : UnknownType */(p_4390/* : UnknownType */, Divide_206/* : UnknownType */(16061/* : Float_11 */, 1805/* : Float_11 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Float_11 */
                : Divide_206/* : Float_11 */(54/* : Float_11 */, Multiply_209/* : UnknownType */(5/* : Float_11 */, Subtract_203/* : UnknownType */(Pow2_2048/* : UnknownType */(p_4390/* : UnknownType */)/* : UnknownType */, Divide_206/* : UnknownType */(513/* : Float_11 */, Multiply_209/* : UnknownType */(25/* : Float_11 */, Add_200/* : UnknownType */(p_4390/* : UnknownType */, Divide_206/* : UnknownType */(268/* : Float_11 */, 25/* : Float_11 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Float_11 */


    ; };
    static BounceEaseInOut_4568 = function (p_4559/* : Easings_141 */) /* : UnknownType */{ return BlendEaseFunc_2116/* : UnknownType */(p_4559/* : UnknownType */, BounceEaseIn_2176/* : UnknownType */, BounceEaseOut_2178/* : UnknownType */)/* : UnknownType */; };
}
class Vector_14_Concept
{
    constructor(self) { this.Self = self; };
    static Count_2323 = function (v_2315/* : Vector_14 */) /* : Count_27 */{ return Count_27_Type/* : Count_27 */(FieldTypes_311/* : UnknownType */(Self_6_Primitive/* : UnknownType */)/* : UnknownType */)/* : Count_27 */; };
    static At_2337 = function (v_2325/* : Vector_14 */, n_2327/* : Index_28 */) /* : T_244 */{ return At_249/* : T_244 */(FieldValues_319/* : UnknownType */(v_2325/* : UnknownType */)/* : UnknownType */, n_2327/* : UnknownType */)/* : T_244 */; };
}
class Measure_15_Concept
{
    constructor(self) { this.Self = self; };
    static Value_2349 = function (x_2339/* : Self_6 */) /* : Number_29 */{ return At_249/* : T_244 */(FieldValues_319/* : UnknownType */(x_2339/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : T_244 */; };
}
class Numerical_16_Concept
{
    constructor(self) { this.Self = self; };
}
class Magnitude_17_Concept
{
    constructor(self) { this.Self = self; };
    static Magnitude_2365 = function (x_2351/* : Self_6 */) /* : Number_29 */{ return SquareRoot_1986/* : UnknownType */(Sum_1976/* : UnknownType */(Square_1988/* : UnknownType */(FieldValues_319/* : UnknownType */(x_2351/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Comparable_18_Concept
{
    constructor(self) { this.Self = self; };
    static Compare_2402 = function (a_2367/* : Self_6 */, b_2369/* : Self_6 */) /* : Integer_26 */{ return LessThan_2062/* : UnknownType */(Magnitude_255/* : UnknownType */(a_2367/* : UnknownType */)/* : UnknownType */, Magnitude_255/* : UnknownType */(b_2369/* : UnknownType */)/* : UnknownType */
        ? Negative_215/* : UnknownType */(1/* : Int_10 */)/* : UnknownType */
        : GreaterThan_2070/* : UnknownType */(Magnitude_255/* : UnknownType */(a_2367/* : UnknownType */)/* : UnknownType */, Magnitude_255/* : UnknownType */(b_2369/* : UnknownType */)/* : UnknownType */
            ? 1/* : Int_10 */
            : 0/* : Int_10 */
        )/* : UnknownType */
    )/* : UnknownType */; };
}
class Equatable_19_Concept
{
    constructor(self) { this.Self = self; };
    static Equals_2422 = function (a_2404/* : Self_6 */, b_2406/* : Self_6 */) /* : Boolean_22 */{ return All_2112/* : UnknownType */(Equals_261/* : UnknownType */(FieldValues_319/* : UnknownType */(a_2404/* : UnknownType */)/* : UnknownType */, FieldValues_319/* : UnknownType */(b_2406/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Arithmetic_20_Concept
{
    constructor(self) { this.Self = self; };
    static Add_2439 = function (self_2424/* : Self_6 */, other_2426/* : Self_6 */) /* : Self_6 */{ return Add_200/* : Float_11 */(FieldValues_319/* : UnknownType */(self_2424/* : UnknownType */)/* : UnknownType */, FieldValues_319/* : UnknownType */(other_2426/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static Negative_2449 = function (self_2441/* : Self_6 */) /* : Self_6 */{ return Negative_215/* : Float_11 */(FieldValues_319/* : UnknownType */(self_2441/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static Reciprocal_2459 = function (self_2451/* : Self_6 */) /* : Self_6 */{ return Reciprocal_270/* : Self_6 */(FieldValues_319/* : UnknownType */(self_2451/* : UnknownType */)/* : UnknownType */)/* : Self_6 */; };
    static Multiply_2476 = function (self_2461/* : Self_6 */, other_2463/* : Self_6 */) /* : Self_6 */{ return Add_200/* : Float_11 */(FieldValues_319/* : UnknownType */(self_2461/* : UnknownType */)/* : UnknownType */, FieldValues_319/* : UnknownType */(other_2463/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static Divide_2493 = function (self_2478/* : Self_6 */, other_2480/* : Self_6 */) /* : Self_6 */{ return Divide_206/* : Float_11 */(FieldValues_319/* : UnknownType */(self_2478/* : UnknownType */)/* : UnknownType */, FieldValues_319/* : UnknownType */(other_2480/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static Modulo_2510 = function (self_2495/* : Self_6 */, other_2497/* : Self_6 */) /* : Self_6 */{ return Modulo_212/* : Float_11 */(FieldValues_319/* : UnknownType */(self_2495/* : UnknownType */)/* : UnknownType */, FieldValues_319/* : UnknownType */(other_2497/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
}
class ScalarArithmetic_21_Concept
{
    constructor(self) { this.Self = self; };
    static Add_2525 = function (self_2513/* : Self_6 */, scalar_2515/* : T_2511 */) /* : Self_6 */{ return Add_200/* : Float_11 */(FieldValues_319/* : UnknownType */(self_2513/* : UnknownType */)/* : UnknownType */, scalar_2515/* : UnknownType */)/* : Float_11 */; };
    static Subtract_2539 = function (self_2527/* : Self_6 */, scalar_2529/* : T_2511 */) /* : Self_6 */{ return Add_200/* : Float_11 */(self_2527/* : UnknownType */, Negative_215/* : UnknownType */(scalar_2529/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static Multiply_2553 = function (self_2541/* : Self_6 */, scalar_2543/* : T_2511 */) /* : Self_6 */{ return Multiply_209/* : Float_11 */(FieldValues_319/* : UnknownType */(self_2541/* : UnknownType */)/* : UnknownType */, scalar_2543/* : UnknownType */)/* : Float_11 */; };
    static Divide_2567 = function (self_2555/* : Self_6 */, scalar_2557/* : T_2511 */) /* : Self_6 */{ return Multiply_209/* : Float_11 */(self_2555/* : UnknownType */, Reciprocal_270/* : UnknownType */(scalar_2557/* : UnknownType */)/* : UnknownType */)/* : Float_11 */; };
    static Modulo_2581 = function (self_2569/* : Self_6 */, scalar_2571/* : T_2511 */) /* : Self_6 */{ return Modulo_212/* : Float_11 */(FieldValues_319/* : UnknownType */(self_2569/* : UnknownType */)/* : UnknownType */, scalar_2571/* : UnknownType */)/* : Float_11 */; };
}
class Boolean_22_Concept
{
    constructor(self) { this.Self = self; };
    static And_2598 = function (a_2583/* : Self_6 */, b_2585/* : Self_6 */) /* : Self_6 */{ return And_236/* : Bool_9 */(FieldValues_319/* : UnknownType */(a_2583/* : UnknownType */)/* : UnknownType */, FieldValues_319/* : UnknownType */(b_2585/* : UnknownType */)/* : UnknownType */)/* : Bool_9 */; };
    static Or_2615 = function (a_2600/* : Self_6 */, b_2602/* : Self_6 */) /* : Self_6 */{ return Or_239/* : Bool_9 */(FieldValues_319/* : UnknownType */(a_2600/* : UnknownType */)/* : UnknownType */, FieldValues_319/* : UnknownType */(b_2602/* : UnknownType */)/* : UnknownType */)/* : Bool_9 */; };
    static Not_2625 = function (a_2617/* : Self_6 */) /* : Self_6 */{ return Not_242/* : Bool_9 */(FieldValues_319/* : UnknownType */(a_2617/* : UnknownType */)/* : UnknownType */)/* : Bool_9 */; };
}
class Value_23_Concept
{
    constructor(self) { this.Self = self; };
    static Type_2627 = function () /* : Type_12 */{ return intrinsic_0/* : UnknownType */; };
    static FieldTypes_2629 = function () /* : Array_25 */{ return intrinsic_0/* : UnknownType */; };
    static FieldNames_2631 = function () /* : Array_25 */{ return intrinsic_0/* : UnknownType */; };
    static FieldValues_2635 = function (self_2633/* : Self_6 */) /* : Array_25 */{ return intrinsic_0/* : UnknownType */; };
    static Zero_2640 = function () /* : Self_6 */{ return Zero_322/* : Self_6 */(FieldTypes_311/* : UnknownType */)/* : Self_6 */; };
    static One_2645 = function () /* : Self_6 */{ return One_325/* : Self_6 */(FieldTypes_311/* : UnknownType */)/* : Self_6 */; };
    static Default_2650 = function () /* : Self_6 */{ return Default_328/* : Self_6 */(FieldTypes_311/* : UnknownType */)/* : Self_6 */; };
    static MinValue_2655 = function () /* : Self_6 */{ return MinValue_331/* : Self_6 */(FieldTypes_311/* : UnknownType */)/* : Self_6 */; };
    static MaxValue_2660 = function () /* : Self_6 */{ return MaxValue_334/* : Self_6 */(FieldTypes_311/* : UnknownType */)/* : Self_6 */; };
    static ToString_2669 = function (x_2662/* : Self_6 */) /* : String_8 */{ return JoinStrings_2114/* : UnknownType */(FieldValues_319/* : UnknownType */, ","/* : String_8 */)/* : UnknownType */; };
}
class Interval_24_Concept
{
    constructor(self) { this.Self = self; };
    static Min_2673 = function (x_2672/* : Self_6 */) /* : T_339 */{ return null; };
    static Max_2676 = function (x_2675/* : Self_6 */) /* : T_339 */{ return null; };
}
class Array_25_Concept
{
    constructor(self) { this.Self = self; };
    static Count_2680 = function (xs_2679/* : Self_6 */) /* : Count_27 */{ return null; };
    static At_2685 = function (xs_2682/* : Self_6 */, n_2684/* : Index_28 */) /* : T_346 */{ return null; };
}
class Integer_26_Type
{
    constructor(Value_354)
    {
        // field initialization 
        this.Value_354 = Value_354;
        this.Type_2627 = Integer_26_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Integer_26_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Integer_26_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Integer_26_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Integer_26_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Integer_26_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Integer_26_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Integer_26_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Integer_26_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Integer_26_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = Integer_26_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = Integer_26_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = Integer_26_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = Integer_26_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = Integer_26_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = Integer_26_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = Integer_26_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Integer_26_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Integer_26_Type.Magnitude_17_Concept.Magnitude_2365;
    }
    // field accessors
    static Value_354 = function(self) { return self.Value_354; }
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
    constructor(Value_361)
    {
        // field initialization 
        this.Value_361 = Value_361;
        this.Type_2627 = Count_27_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Count_27_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Count_27_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Count_27_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Count_27_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Count_27_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Count_27_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Count_27_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Count_27_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Count_27_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = Count_27_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = Count_27_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = Count_27_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = Count_27_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = Count_27_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = Count_27_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = Count_27_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Count_27_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Count_27_Type.Magnitude_17_Concept.Magnitude_2365;
    }
    // field accessors
    static Value_361 = function(self) { return self.Value_361; }
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
    constructor(Value_368)
    {
        // field initialization 
        this.Value_368 = Value_368;
        this.Type_2627 = Index_28_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Index_28_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Index_28_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Index_28_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Index_28_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Index_28_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Index_28_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Index_28_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Index_28_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Index_28_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Value_368 = function(self) { return self.Value_368; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Index_28_Type);
    static Implements = [Value_23_Concept];
}
class Number_29_Type
{
    constructor(Value_375)
    {
        // field initialization 
        this.Value_375 = Value_375;
        this.Type_2627 = Number_29_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Number_29_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Number_29_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Number_29_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Number_29_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Number_29_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Number_29_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Number_29_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Number_29_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Number_29_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = Number_29_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = Number_29_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = Number_29_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = Number_29_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = Number_29_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = Number_29_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = Number_29_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Number_29_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Number_29_Type.Magnitude_17_Concept.Magnitude_2365;
    }
    // field accessors
    static Value_375 = function(self) { return self.Value_375; }
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
    constructor(Value_382)
    {
        // field initialization 
        this.Value_382 = Value_382;
        this.Type_2627 = Unit_30_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Unit_30_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Unit_30_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Unit_30_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Unit_30_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Unit_30_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Unit_30_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Unit_30_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Unit_30_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Unit_30_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = Unit_30_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = Unit_30_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = Unit_30_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = Unit_30_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = Unit_30_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = Unit_30_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = Unit_30_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Unit_30_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Unit_30_Type.Magnitude_17_Concept.Magnitude_2365;
    }
    // field accessors
    static Value_382 = function(self) { return self.Value_382; }
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
    constructor(Value_389)
    {
        // field initialization 
        this.Value_389 = Value_389;
        this.Type_2627 = Percent_31_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Percent_31_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Percent_31_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Percent_31_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Percent_31_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Percent_31_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Percent_31_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Percent_31_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Percent_31_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Percent_31_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = Percent_31_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = Percent_31_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = Percent_31_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = Percent_31_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = Percent_31_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = Percent_31_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = Percent_31_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Percent_31_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Percent_31_Type.Magnitude_17_Concept.Magnitude_2365;
    }
    // field accessors
    static Value_389 = function(self) { return self.Value_389; }
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
    constructor(X_396, Y_403, Z_410, W_417)
    {
        // field initialization 
        this.X_396 = X_396;
        this.Y_403 = Y_403;
        this.Z_410 = Z_410;
        this.W_417 = W_417;
        this.Type_2627 = Quaternion_32_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Quaternion_32_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Quaternion_32_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Quaternion_32_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Quaternion_32_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Quaternion_32_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Quaternion_32_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Quaternion_32_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Quaternion_32_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Quaternion_32_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static X_396 = function(self) { return self.X_396; }
    static Y_403 = function(self) { return self.Y_403; }
    static Z_410 = function(self) { return self.Z_410; }
    static W_417 = function(self) { return self.W_417; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Quaternion_32_Type);
    static Implements = [Value_23_Concept];
}
class Unit2D_33_Type
{
    constructor(X_424, Y_431)
    {
        // field initialization 
        this.X_424 = X_424;
        this.Y_431 = Y_431;
        this.Type_2627 = Unit2D_33_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Unit2D_33_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Unit2D_33_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Unit2D_33_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Unit2D_33_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Unit2D_33_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Unit2D_33_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Unit2D_33_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Unit2D_33_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Unit2D_33_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static X_424 = function(self) { return self.X_424; }
    static Y_431 = function(self) { return self.Y_431; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Unit2D_33_Type);
    static Implements = [Value_23_Concept];
}
class Unit3D_34_Type
{
    constructor(X_438, Y_445, Z_452)
    {
        // field initialization 
        this.X_438 = X_438;
        this.Y_445 = Y_445;
        this.Z_452 = Z_452;
        this.Type_2627 = Unit3D_34_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Unit3D_34_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Unit3D_34_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Unit3D_34_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Unit3D_34_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Unit3D_34_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Unit3D_34_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Unit3D_34_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Unit3D_34_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Unit3D_34_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static X_438 = function(self) { return self.X_438; }
    static Y_445 = function(self) { return self.Y_445; }
    static Z_452 = function(self) { return self.Z_452; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Unit3D_34_Type);
    static Implements = [Value_23_Concept];
}
class Direction3D_35_Type
{
    constructor(Value_459)
    {
        // field initialization 
        this.Value_459 = Value_459;
        this.Type_2627 = Direction3D_35_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Direction3D_35_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Direction3D_35_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Direction3D_35_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Direction3D_35_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Direction3D_35_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Direction3D_35_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Direction3D_35_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Direction3D_35_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Direction3D_35_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Value_459 = function(self) { return self.Value_459; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Direction3D_35_Type);
    static Implements = [Value_23_Concept];
}
class AxisAngle_36_Type
{
    constructor(Axis_466, Angle_473)
    {
        // field initialization 
        this.Axis_466 = Axis_466;
        this.Angle_473 = Angle_473;
        this.Type_2627 = AxisAngle_36_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = AxisAngle_36_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = AxisAngle_36_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = AxisAngle_36_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = AxisAngle_36_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = AxisAngle_36_Type.Value_23_Concept.One_2645;
        this.Default_2650 = AxisAngle_36_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = AxisAngle_36_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = AxisAngle_36_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = AxisAngle_36_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Axis_466 = function(self) { return self.Axis_466; }
    static Angle_473 = function(self) { return self.Angle_473; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(AxisAngle_36_Type);
    static Implements = [Value_23_Concept];
}
class EulerAngles_37_Type
{
    constructor(Yaw_480, Pitch_487, Roll_494)
    {
        // field initialization 
        this.Yaw_480 = Yaw_480;
        this.Pitch_487 = Pitch_487;
        this.Roll_494 = Roll_494;
        this.Type_2627 = EulerAngles_37_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = EulerAngles_37_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = EulerAngles_37_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = EulerAngles_37_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = EulerAngles_37_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = EulerAngles_37_Type.Value_23_Concept.One_2645;
        this.Default_2650 = EulerAngles_37_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = EulerAngles_37_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = EulerAngles_37_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = EulerAngles_37_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Yaw_480 = function(self) { return self.Yaw_480; }
    static Pitch_487 = function(self) { return self.Pitch_487; }
    static Roll_494 = function(self) { return self.Roll_494; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(EulerAngles_37_Type);
    static Implements = [Value_23_Concept];
}
class Rotation3D_38_Type
{
    constructor(Quaternion_501)
    {
        // field initialization 
        this.Quaternion_501 = Quaternion_501;
        this.Type_2627 = Rotation3D_38_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Rotation3D_38_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Rotation3D_38_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Rotation3D_38_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Rotation3D_38_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Rotation3D_38_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Rotation3D_38_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Rotation3D_38_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Rotation3D_38_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Rotation3D_38_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Quaternion_501 = function(self) { return self.Quaternion_501; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Rotation3D_38_Type);
    static Implements = [Value_23_Concept];
}
class Vector2D_39_Type
{
    constructor(X_508, Y_515)
    {
        // field initialization 
        this.X_508 = X_508;
        this.Y_515 = Y_515;
        this.Count_2680 = Vector2D_39_Type.Array_25_Concept.Count_2680;
        this.At_2685 = Vector2D_39_Type.Array_25_Concept.At_2685;
        this.Type_2627 = Vector2D_39_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Vector2D_39_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Vector2D_39_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Vector2D_39_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Vector2D_39_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Vector2D_39_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Vector2D_39_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Vector2D_39_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Vector2D_39_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Vector2D_39_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = Vector2D_39_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = Vector2D_39_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = Vector2D_39_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = Vector2D_39_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = Vector2D_39_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = Vector2D_39_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = Vector2D_39_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Vector2D_39_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Vector2D_39_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Count_2323 = Vector2D_39_Type.Vector_14_Concept.Count_2323;
        this.At_2337 = Vector2D_39_Type.Vector_14_Concept.At_2337;
    }
    // field accessors
    static X_508 = function(self) { return self.X_508; }
    static Y_515 = function(self) { return self.Y_515; }
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
    constructor(X_522, Y_529, Z_536)
    {
        // field initialization 
        this.X_522 = X_522;
        this.Y_529 = Y_529;
        this.Z_536 = Z_536;
        this.Count_2680 = Vector3D_40_Type.Array_25_Concept.Count_2680;
        this.At_2685 = Vector3D_40_Type.Array_25_Concept.At_2685;
        this.Type_2627 = Vector3D_40_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Vector3D_40_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Vector3D_40_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Vector3D_40_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Vector3D_40_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Vector3D_40_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Vector3D_40_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Vector3D_40_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Vector3D_40_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Vector3D_40_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = Vector3D_40_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = Vector3D_40_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = Vector3D_40_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = Vector3D_40_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = Vector3D_40_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = Vector3D_40_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = Vector3D_40_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Vector3D_40_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Vector3D_40_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Count_2323 = Vector3D_40_Type.Vector_14_Concept.Count_2323;
        this.At_2337 = Vector3D_40_Type.Vector_14_Concept.At_2337;
    }
    // field accessors
    static X_522 = function(self) { return self.X_522; }
    static Y_529 = function(self) { return self.Y_529; }
    static Z_536 = function(self) { return self.Z_536; }
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
    constructor(X_543, Y_550, Z_557, W_564)
    {
        // field initialization 
        this.X_543 = X_543;
        this.Y_550 = Y_550;
        this.Z_557 = Z_557;
        this.W_564 = W_564;
        this.Count_2680 = Vector4D_41_Type.Array_25_Concept.Count_2680;
        this.At_2685 = Vector4D_41_Type.Array_25_Concept.At_2685;
        this.Type_2627 = Vector4D_41_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Vector4D_41_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Vector4D_41_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Vector4D_41_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Vector4D_41_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Vector4D_41_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Vector4D_41_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Vector4D_41_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Vector4D_41_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Vector4D_41_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = Vector4D_41_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = Vector4D_41_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = Vector4D_41_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = Vector4D_41_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = Vector4D_41_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = Vector4D_41_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = Vector4D_41_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Vector4D_41_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Vector4D_41_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Count_2323 = Vector4D_41_Type.Vector_14_Concept.Count_2323;
        this.At_2337 = Vector4D_41_Type.Vector_14_Concept.At_2337;
    }
    // field accessors
    static X_543 = function(self) { return self.X_543; }
    static Y_550 = function(self) { return self.Y_550; }
    static Z_557 = function(self) { return self.Z_557; }
    static W_564 = function(self) { return self.W_564; }
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
    constructor(Value_571)
    {
        // field initialization 
        this.Value_571 = Value_571;
        this.Type_2627 = Orientation3D_42_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Orientation3D_42_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Orientation3D_42_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Orientation3D_42_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Orientation3D_42_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Orientation3D_42_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Orientation3D_42_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Orientation3D_42_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Orientation3D_42_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Orientation3D_42_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Value_571 = function(self) { return self.Value_571; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Orientation3D_42_Type);
    static Implements = [Value_23_Concept];
}
class Pose2D_43_Type
{
    constructor(Position_578, Orientation_585)
    {
        // field initialization 
        this.Position_578 = Position_578;
        this.Orientation_585 = Orientation_585;
        this.Type_2627 = Pose2D_43_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Pose2D_43_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Pose2D_43_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Pose2D_43_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Pose2D_43_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Pose2D_43_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Pose2D_43_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Pose2D_43_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Pose2D_43_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Pose2D_43_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Position_578 = function(self) { return self.Position_578; }
    static Orientation_585 = function(self) { return self.Orientation_585; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Pose2D_43_Type);
    static Implements = [Value_23_Concept];
}
class Pose3D_44_Type
{
    constructor(Position_592, Orientation_599)
    {
        // field initialization 
        this.Position_592 = Position_592;
        this.Orientation_599 = Orientation_599;
        this.Type_2627 = Pose3D_44_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Pose3D_44_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Pose3D_44_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Pose3D_44_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Pose3D_44_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Pose3D_44_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Pose3D_44_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Pose3D_44_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Pose3D_44_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Pose3D_44_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Position_592 = function(self) { return self.Position_592; }
    static Orientation_599 = function(self) { return self.Orientation_599; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Pose3D_44_Type);
    static Implements = [Value_23_Concept];
}
class Transform3D_45_Type
{
    constructor(Translation_606, Rotation_613, Scale_620)
    {
        // field initialization 
        this.Translation_606 = Translation_606;
        this.Rotation_613 = Rotation_613;
        this.Scale_620 = Scale_620;
        this.Type_2627 = Transform3D_45_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Transform3D_45_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Transform3D_45_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Transform3D_45_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Transform3D_45_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Transform3D_45_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Transform3D_45_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Transform3D_45_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Transform3D_45_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Transform3D_45_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Translation_606 = function(self) { return self.Translation_606; }
    static Rotation_613 = function(self) { return self.Rotation_613; }
    static Scale_620 = function(self) { return self.Scale_620; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Transform3D_45_Type);
    static Implements = [Value_23_Concept];
}
class Transform2D_46_Type
{
    constructor(Translation_627, Rotation_634, Scale_641)
    {
        // field initialization 
        this.Translation_627 = Translation_627;
        this.Rotation_634 = Rotation_634;
        this.Scale_641 = Scale_641;
        this.Type_2627 = Transform2D_46_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Transform2D_46_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Transform2D_46_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Transform2D_46_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Transform2D_46_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Transform2D_46_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Transform2D_46_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Transform2D_46_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Transform2D_46_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Transform2D_46_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Translation_627 = function(self) { return self.Translation_627; }
    static Rotation_634 = function(self) { return self.Rotation_634; }
    static Scale_641 = function(self) { return self.Scale_641; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Transform2D_46_Type);
    static Implements = [Value_23_Concept];
}
class AlignedBox2D_47_Type
{
    constructor(A_648, B_655)
    {
        // field initialization 
        this.A_648 = A_648;
        this.B_655 = B_655;
        this.Count_2680 = AlignedBox2D_47_Type.Array_25_Concept.Count_2680;
        this.At_2685 = AlignedBox2D_47_Type.Array_25_Concept.At_2685;
        this.Type_2627 = AlignedBox2D_47_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = AlignedBox2D_47_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = AlignedBox2D_47_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = AlignedBox2D_47_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = AlignedBox2D_47_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = AlignedBox2D_47_Type.Value_23_Concept.One_2645;
        this.Default_2650 = AlignedBox2D_47_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = AlignedBox2D_47_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = AlignedBox2D_47_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = AlignedBox2D_47_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = AlignedBox2D_47_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = AlignedBox2D_47_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = AlignedBox2D_47_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = AlignedBox2D_47_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Count_2323 = AlignedBox2D_47_Type.Vector_14_Concept.Count_2323;
        this.At_2337 = AlignedBox2D_47_Type.Vector_14_Concept.At_2337;
        this.Min_2673 = AlignedBox2D_47_Type.Interval_24_Concept.Min_2673;
        this.Max_2676 = AlignedBox2D_47_Type.Interval_24_Concept.Max_2676;
    }
    // field accessors
    static A_648 = function(self) { return self.A_648; }
    static B_655 = function(self) { return self.B_655; }
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
    constructor(A_662, B_669)
    {
        // field initialization 
        this.A_662 = A_662;
        this.B_669 = B_669;
        this.Count_2680 = AlignedBox3D_48_Type.Array_25_Concept.Count_2680;
        this.At_2685 = AlignedBox3D_48_Type.Array_25_Concept.At_2685;
        this.Type_2627 = AlignedBox3D_48_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = AlignedBox3D_48_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = AlignedBox3D_48_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = AlignedBox3D_48_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = AlignedBox3D_48_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = AlignedBox3D_48_Type.Value_23_Concept.One_2645;
        this.Default_2650 = AlignedBox3D_48_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = AlignedBox3D_48_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = AlignedBox3D_48_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = AlignedBox3D_48_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = AlignedBox3D_48_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = AlignedBox3D_48_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = AlignedBox3D_48_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = AlignedBox3D_48_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Count_2323 = AlignedBox3D_48_Type.Vector_14_Concept.Count_2323;
        this.At_2337 = AlignedBox3D_48_Type.Vector_14_Concept.At_2337;
        this.Min_2673 = AlignedBox3D_48_Type.Interval_24_Concept.Min_2673;
        this.Max_2676 = AlignedBox3D_48_Type.Interval_24_Concept.Max_2676;
    }
    // field accessors
    static A_662 = function(self) { return self.A_662; }
    static B_669 = function(self) { return self.B_669; }
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
    constructor(Real_676, Imaginary_683)
    {
        // field initialization 
        this.Real_676 = Real_676;
        this.Imaginary_683 = Imaginary_683;
        this.Count_2680 = Complex_49_Type.Array_25_Concept.Count_2680;
        this.At_2685 = Complex_49_Type.Array_25_Concept.At_2685;
        this.Type_2627 = Complex_49_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Complex_49_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Complex_49_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Complex_49_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Complex_49_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Complex_49_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Complex_49_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Complex_49_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Complex_49_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Complex_49_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = Complex_49_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = Complex_49_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = Complex_49_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = Complex_49_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = Complex_49_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = Complex_49_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = Complex_49_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Complex_49_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Complex_49_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Count_2323 = Complex_49_Type.Vector_14_Concept.Count_2323;
        this.At_2337 = Complex_49_Type.Vector_14_Concept.At_2337;
    }
    // field accessors
    static Real_676 = function(self) { return self.Real_676; }
    static Imaginary_683 = function(self) { return self.Imaginary_683; }
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
    constructor(Direction_690, Position_697)
    {
        // field initialization 
        this.Direction_690 = Direction_690;
        this.Position_697 = Position_697;
        this.Type_2627 = Ray3D_50_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Ray3D_50_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Ray3D_50_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Ray3D_50_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Ray3D_50_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Ray3D_50_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Ray3D_50_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Ray3D_50_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Ray3D_50_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Ray3D_50_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Direction_690 = function(self) { return self.Direction_690; }
    static Position_697 = function(self) { return self.Position_697; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Ray3D_50_Type);
    static Implements = [Value_23_Concept];
}
class Ray2D_51_Type
{
    constructor(Direction_704, Position_711)
    {
        // field initialization 
        this.Direction_704 = Direction_704;
        this.Position_711 = Position_711;
        this.Type_2627 = Ray2D_51_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Ray2D_51_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Ray2D_51_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Ray2D_51_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Ray2D_51_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Ray2D_51_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Ray2D_51_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Ray2D_51_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Ray2D_51_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Ray2D_51_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Direction_704 = function(self) { return self.Direction_704; }
    static Position_711 = function(self) { return self.Position_711; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Ray2D_51_Type);
    static Implements = [Value_23_Concept];
}
class Sphere_52_Type
{
    constructor(Center_718, Radius_725)
    {
        // field initialization 
        this.Center_718 = Center_718;
        this.Radius_725 = Radius_725;
        this.Type_2627 = Sphere_52_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Sphere_52_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Sphere_52_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Sphere_52_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Sphere_52_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Sphere_52_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Sphere_52_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Sphere_52_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Sphere_52_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Sphere_52_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Center_718 = function(self) { return self.Center_718; }
    static Radius_725 = function(self) { return self.Radius_725; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Sphere_52_Type);
    static Implements = [Value_23_Concept];
}
class Plane_53_Type
{
    constructor(Normal_732, D_739)
    {
        // field initialization 
        this.Normal_732 = Normal_732;
        this.D_739 = D_739;
        this.Type_2627 = Plane_53_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Plane_53_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Plane_53_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Plane_53_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Plane_53_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Plane_53_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Plane_53_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Plane_53_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Plane_53_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Plane_53_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Normal_732 = function(self) { return self.Normal_732; }
    static D_739 = function(self) { return self.D_739; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Plane_53_Type);
    static Implements = [Value_23_Concept];
}
class Triangle3D_54_Type
{
    constructor(A_746, B_753, C_760)
    {
        // field initialization 
        this.A_746 = A_746;
        this.B_753 = B_753;
        this.C_760 = C_760;
        this.Type_2627 = Triangle3D_54_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Triangle3D_54_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Triangle3D_54_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Triangle3D_54_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Triangle3D_54_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Triangle3D_54_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Triangle3D_54_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Triangle3D_54_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Triangle3D_54_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Triangle3D_54_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static A_746 = function(self) { return self.A_746; }
    static B_753 = function(self) { return self.B_753; }
    static C_760 = function(self) { return self.C_760; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Triangle3D_54_Type);
    static Implements = [Value_23_Concept];
}
class Triangle2D_55_Type
{
    constructor(A_767, B_774, C_781)
    {
        // field initialization 
        this.A_767 = A_767;
        this.B_774 = B_774;
        this.C_781 = C_781;
        this.Type_2627 = Triangle2D_55_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Triangle2D_55_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Triangle2D_55_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Triangle2D_55_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Triangle2D_55_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Triangle2D_55_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Triangle2D_55_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Triangle2D_55_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Triangle2D_55_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Triangle2D_55_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static A_767 = function(self) { return self.A_767; }
    static B_774 = function(self) { return self.B_774; }
    static C_781 = function(self) { return self.C_781; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Triangle2D_55_Type);
    static Implements = [Value_23_Concept];
}
class Quad3D_56_Type
{
    constructor(A_788, B_795, C_802, D_809)
    {
        // field initialization 
        this.A_788 = A_788;
        this.B_795 = B_795;
        this.C_802 = C_802;
        this.D_809 = D_809;
        this.Type_2627 = Quad3D_56_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Quad3D_56_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Quad3D_56_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Quad3D_56_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Quad3D_56_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Quad3D_56_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Quad3D_56_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Quad3D_56_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Quad3D_56_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Quad3D_56_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static A_788 = function(self) { return self.A_788; }
    static B_795 = function(self) { return self.B_795; }
    static C_802 = function(self) { return self.C_802; }
    static D_809 = function(self) { return self.D_809; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Quad3D_56_Type);
    static Implements = [Value_23_Concept];
}
class Quad2D_57_Type
{
    constructor(A_816, B_823, C_830, D_837)
    {
        // field initialization 
        this.A_816 = A_816;
        this.B_823 = B_823;
        this.C_830 = C_830;
        this.D_837 = D_837;
        this.Type_2627 = Quad2D_57_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Quad2D_57_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Quad2D_57_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Quad2D_57_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Quad2D_57_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Quad2D_57_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Quad2D_57_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Quad2D_57_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Quad2D_57_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Quad2D_57_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static A_816 = function(self) { return self.A_816; }
    static B_823 = function(self) { return self.B_823; }
    static C_830 = function(self) { return self.C_830; }
    static D_837 = function(self) { return self.D_837; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Quad2D_57_Type);
    static Implements = [Value_23_Concept];
}
class Point3D_58_Type
{
    constructor(Value_844)
    {
        // field initialization 
        this.Value_844 = Value_844;
        this.Type_2627 = Point3D_58_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Point3D_58_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Point3D_58_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Point3D_58_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Point3D_58_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Point3D_58_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Point3D_58_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Point3D_58_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Point3D_58_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Point3D_58_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Value_844 = function(self) { return self.Value_844; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Point3D_58_Type);
    static Implements = [Value_23_Concept];
}
class Point2D_59_Type
{
    constructor(Value_851)
    {
        // field initialization 
        this.Value_851 = Value_851;
        this.Type_2627 = Point2D_59_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Point2D_59_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Point2D_59_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Point2D_59_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Point2D_59_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Point2D_59_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Point2D_59_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Point2D_59_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Point2D_59_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Point2D_59_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Value_851 = function(self) { return self.Value_851; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Point2D_59_Type);
    static Implements = [Value_23_Concept];
}
class Line3D_60_Type
{
    constructor(A_858, B_865)
    {
        // field initialization 
        this.A_858 = A_858;
        this.B_865 = B_865;
        this.Count_2680 = Line3D_60_Type.Array_25_Concept.Count_2680;
        this.At_2685 = Line3D_60_Type.Array_25_Concept.At_2685;
        this.Type_2627 = Line3D_60_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Line3D_60_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Line3D_60_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Line3D_60_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Line3D_60_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Line3D_60_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Line3D_60_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Line3D_60_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Line3D_60_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Line3D_60_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = Line3D_60_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = Line3D_60_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = Line3D_60_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = Line3D_60_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = Line3D_60_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = Line3D_60_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = Line3D_60_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Line3D_60_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Line3D_60_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Count_2323 = Line3D_60_Type.Vector_14_Concept.Count_2323;
        this.At_2337 = Line3D_60_Type.Vector_14_Concept.At_2337;
        this.Min_2673 = Line3D_60_Type.Interval_24_Concept.Min_2673;
        this.Max_2676 = Line3D_60_Type.Interval_24_Concept.Max_2676;
    }
    // field accessors
    static A_858 = function(self) { return self.A_858; }
    static B_865 = function(self) { return self.B_865; }
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
    constructor(A_872, B_879)
    {
        // field initialization 
        this.A_872 = A_872;
        this.B_879 = B_879;
        this.Count_2680 = Line2D_61_Type.Array_25_Concept.Count_2680;
        this.At_2685 = Line2D_61_Type.Array_25_Concept.At_2685;
        this.Type_2627 = Line2D_61_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Line2D_61_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Line2D_61_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Line2D_61_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Line2D_61_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Line2D_61_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Line2D_61_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Line2D_61_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Line2D_61_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Line2D_61_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = Line2D_61_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = Line2D_61_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = Line2D_61_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = Line2D_61_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = Line2D_61_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = Line2D_61_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = Line2D_61_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Line2D_61_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Line2D_61_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Count_2323 = Line2D_61_Type.Vector_14_Concept.Count_2323;
        this.At_2337 = Line2D_61_Type.Vector_14_Concept.At_2337;
        this.Min_2673 = Line2D_61_Type.Interval_24_Concept.Min_2673;
        this.Max_2676 = Line2D_61_Type.Interval_24_Concept.Max_2676;
    }
    // field accessors
    static A_872 = function(self) { return self.A_872; }
    static B_879 = function(self) { return self.B_879; }
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
    constructor(R_886, G_893, B_900, A_907)
    {
        // field initialization 
        this.R_886 = R_886;
        this.G_893 = G_893;
        this.B_900 = B_900;
        this.A_907 = A_907;
        this.Type_2627 = Color_62_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Color_62_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Color_62_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Color_62_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Color_62_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Color_62_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Color_62_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Color_62_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Color_62_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Color_62_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static R_886 = function(self) { return self.R_886; }
    static G_893 = function(self) { return self.G_893; }
    static B_900 = function(self) { return self.B_900; }
    static A_907 = function(self) { return self.A_907; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Color_62_Type);
    static Implements = [Value_23_Concept];
}
class ColorLUV_63_Type
{
    constructor(Lightness_914, U_921, V_928)
    {
        // field initialization 
        this.Lightness_914 = Lightness_914;
        this.U_921 = U_921;
        this.V_928 = V_928;
        this.Type_2627 = ColorLUV_63_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = ColorLUV_63_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = ColorLUV_63_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = ColorLUV_63_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = ColorLUV_63_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = ColorLUV_63_Type.Value_23_Concept.One_2645;
        this.Default_2650 = ColorLUV_63_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = ColorLUV_63_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = ColorLUV_63_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = ColorLUV_63_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Lightness_914 = function(self) { return self.Lightness_914; }
    static U_921 = function(self) { return self.U_921; }
    static V_928 = function(self) { return self.V_928; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorLUV_63_Type);
    static Implements = [Value_23_Concept];
}
class ColorLAB_64_Type
{
    constructor(Lightness_935, A_942, B_949)
    {
        // field initialization 
        this.Lightness_935 = Lightness_935;
        this.A_942 = A_942;
        this.B_949 = B_949;
        this.Type_2627 = ColorLAB_64_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = ColorLAB_64_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = ColorLAB_64_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = ColorLAB_64_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = ColorLAB_64_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = ColorLAB_64_Type.Value_23_Concept.One_2645;
        this.Default_2650 = ColorLAB_64_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = ColorLAB_64_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = ColorLAB_64_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = ColorLAB_64_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Lightness_935 = function(self) { return self.Lightness_935; }
    static A_942 = function(self) { return self.A_942; }
    static B_949 = function(self) { return self.B_949; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorLAB_64_Type);
    static Implements = [Value_23_Concept];
}
class ColorLCh_65_Type
{
    constructor(Lightness_956, ChromaHue_963)
    {
        // field initialization 
        this.Lightness_956 = Lightness_956;
        this.ChromaHue_963 = ChromaHue_963;
        this.Type_2627 = ColorLCh_65_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = ColorLCh_65_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = ColorLCh_65_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = ColorLCh_65_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = ColorLCh_65_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = ColorLCh_65_Type.Value_23_Concept.One_2645;
        this.Default_2650 = ColorLCh_65_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = ColorLCh_65_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = ColorLCh_65_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = ColorLCh_65_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Lightness_956 = function(self) { return self.Lightness_956; }
    static ChromaHue_963 = function(self) { return self.ChromaHue_963; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorLCh_65_Type);
    static Implements = [Value_23_Concept];
}
class ColorHSV_66_Type
{
    constructor(Hue_970, S_977, V_984)
    {
        // field initialization 
        this.Hue_970 = Hue_970;
        this.S_977 = S_977;
        this.V_984 = V_984;
        this.Type_2627 = ColorHSV_66_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = ColorHSV_66_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = ColorHSV_66_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = ColorHSV_66_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = ColorHSV_66_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = ColorHSV_66_Type.Value_23_Concept.One_2645;
        this.Default_2650 = ColorHSV_66_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = ColorHSV_66_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = ColorHSV_66_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = ColorHSV_66_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Hue_970 = function(self) { return self.Hue_970; }
    static S_977 = function(self) { return self.S_977; }
    static V_984 = function(self) { return self.V_984; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorHSV_66_Type);
    static Implements = [Value_23_Concept];
}
class ColorHSL_67_Type
{
    constructor(Hue_991, Saturation_998, Luminance_1005)
    {
        // field initialization 
        this.Hue_991 = Hue_991;
        this.Saturation_998 = Saturation_998;
        this.Luminance_1005 = Luminance_1005;
        this.Type_2627 = ColorHSL_67_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = ColorHSL_67_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = ColorHSL_67_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = ColorHSL_67_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = ColorHSL_67_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = ColorHSL_67_Type.Value_23_Concept.One_2645;
        this.Default_2650 = ColorHSL_67_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = ColorHSL_67_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = ColorHSL_67_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = ColorHSL_67_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Hue_991 = function(self) { return self.Hue_991; }
    static Saturation_998 = function(self) { return self.Saturation_998; }
    static Luminance_1005 = function(self) { return self.Luminance_1005; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorHSL_67_Type);
    static Implements = [Value_23_Concept];
}
class ColorYCbCr_68_Type
{
    constructor(Y_1012, Cb_1019, Cr_1026)
    {
        // field initialization 
        this.Y_1012 = Y_1012;
        this.Cb_1019 = Cb_1019;
        this.Cr_1026 = Cr_1026;
        this.Type_2627 = ColorYCbCr_68_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = ColorYCbCr_68_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = ColorYCbCr_68_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = ColorYCbCr_68_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = ColorYCbCr_68_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = ColorYCbCr_68_Type.Value_23_Concept.One_2645;
        this.Default_2650 = ColorYCbCr_68_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = ColorYCbCr_68_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = ColorYCbCr_68_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = ColorYCbCr_68_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Y_1012 = function(self) { return self.Y_1012; }
    static Cb_1019 = function(self) { return self.Cb_1019; }
    static Cr_1026 = function(self) { return self.Cr_1026; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ColorYCbCr_68_Type);
    static Implements = [Value_23_Concept];
}
class SphericalCoordinate_69_Type
{
    constructor(Radius_1033, Azimuth_1040, Polar_1047)
    {
        // field initialization 
        this.Radius_1033 = Radius_1033;
        this.Azimuth_1040 = Azimuth_1040;
        this.Polar_1047 = Polar_1047;
        this.Type_2627 = SphericalCoordinate_69_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = SphericalCoordinate_69_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = SphericalCoordinate_69_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = SphericalCoordinate_69_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = SphericalCoordinate_69_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = SphericalCoordinate_69_Type.Value_23_Concept.One_2645;
        this.Default_2650 = SphericalCoordinate_69_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = SphericalCoordinate_69_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = SphericalCoordinate_69_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = SphericalCoordinate_69_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Radius_1033 = function(self) { return self.Radius_1033; }
    static Azimuth_1040 = function(self) { return self.Azimuth_1040; }
    static Polar_1047 = function(self) { return self.Polar_1047; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(SphericalCoordinate_69_Type);
    static Implements = [Value_23_Concept];
}
class PolarCoordinate_70_Type
{
    constructor(Radius_1054, Angle_1061)
    {
        // field initialization 
        this.Radius_1054 = Radius_1054;
        this.Angle_1061 = Angle_1061;
        this.Type_2627 = PolarCoordinate_70_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = PolarCoordinate_70_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = PolarCoordinate_70_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = PolarCoordinate_70_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = PolarCoordinate_70_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = PolarCoordinate_70_Type.Value_23_Concept.One_2645;
        this.Default_2650 = PolarCoordinate_70_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = PolarCoordinate_70_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = PolarCoordinate_70_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = PolarCoordinate_70_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Radius_1054 = function(self) { return self.Radius_1054; }
    static Angle_1061 = function(self) { return self.Angle_1061; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(PolarCoordinate_70_Type);
    static Implements = [Value_23_Concept];
}
class LogPolarCoordinate_71_Type
{
    constructor(Rho_1068, Azimuth_1075)
    {
        // field initialization 
        this.Rho_1068 = Rho_1068;
        this.Azimuth_1075 = Azimuth_1075;
        this.Type_2627 = LogPolarCoordinate_71_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = LogPolarCoordinate_71_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = LogPolarCoordinate_71_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = LogPolarCoordinate_71_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = LogPolarCoordinate_71_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = LogPolarCoordinate_71_Type.Value_23_Concept.One_2645;
        this.Default_2650 = LogPolarCoordinate_71_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = LogPolarCoordinate_71_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = LogPolarCoordinate_71_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = LogPolarCoordinate_71_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Rho_1068 = function(self) { return self.Rho_1068; }
    static Azimuth_1075 = function(self) { return self.Azimuth_1075; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(LogPolarCoordinate_71_Type);
    static Implements = [Value_23_Concept];
}
class CylindricalCoordinate_72_Type
{
    constructor(RadialDistance_1082, Azimuth_1089, Height_1096)
    {
        // field initialization 
        this.RadialDistance_1082 = RadialDistance_1082;
        this.Azimuth_1089 = Azimuth_1089;
        this.Height_1096 = Height_1096;
        this.Type_2627 = CylindricalCoordinate_72_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = CylindricalCoordinate_72_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = CylindricalCoordinate_72_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = CylindricalCoordinate_72_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = CylindricalCoordinate_72_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = CylindricalCoordinate_72_Type.Value_23_Concept.One_2645;
        this.Default_2650 = CylindricalCoordinate_72_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = CylindricalCoordinate_72_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = CylindricalCoordinate_72_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = CylindricalCoordinate_72_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static RadialDistance_1082 = function(self) { return self.RadialDistance_1082; }
    static Azimuth_1089 = function(self) { return self.Azimuth_1089; }
    static Height_1096 = function(self) { return self.Height_1096; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(CylindricalCoordinate_72_Type);
    static Implements = [Value_23_Concept];
}
class HorizontalCoordinate_73_Type
{
    constructor(Radius_1103, Azimuth_1110, Height_1117)
    {
        // field initialization 
        this.Radius_1103 = Radius_1103;
        this.Azimuth_1110 = Azimuth_1110;
        this.Height_1117 = Height_1117;
        this.Type_2627 = HorizontalCoordinate_73_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = HorizontalCoordinate_73_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = HorizontalCoordinate_73_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = HorizontalCoordinate_73_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = HorizontalCoordinate_73_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = HorizontalCoordinate_73_Type.Value_23_Concept.One_2645;
        this.Default_2650 = HorizontalCoordinate_73_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = HorizontalCoordinate_73_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = HorizontalCoordinate_73_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = HorizontalCoordinate_73_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Radius_1103 = function(self) { return self.Radius_1103; }
    static Azimuth_1110 = function(self) { return self.Azimuth_1110; }
    static Height_1117 = function(self) { return self.Height_1117; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(HorizontalCoordinate_73_Type);
    static Implements = [Value_23_Concept];
}
class GeoCoordinate_74_Type
{
    constructor(Latitude_1124, Longitude_1131)
    {
        // field initialization 
        this.Latitude_1124 = Latitude_1124;
        this.Longitude_1131 = Longitude_1131;
        this.Type_2627 = GeoCoordinate_74_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = GeoCoordinate_74_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = GeoCoordinate_74_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = GeoCoordinate_74_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = GeoCoordinate_74_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = GeoCoordinate_74_Type.Value_23_Concept.One_2645;
        this.Default_2650 = GeoCoordinate_74_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = GeoCoordinate_74_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = GeoCoordinate_74_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = GeoCoordinate_74_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Latitude_1124 = function(self) { return self.Latitude_1124; }
    static Longitude_1131 = function(self) { return self.Longitude_1131; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(GeoCoordinate_74_Type);
    static Implements = [Value_23_Concept];
}
class GeoCoordinateWithAltitude_75_Type
{
    constructor(Coordinate_1138, Altitude_1145)
    {
        // field initialization 
        this.Coordinate_1138 = Coordinate_1138;
        this.Altitude_1145 = Altitude_1145;
        this.Type_2627 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.One_2645;
        this.Default_2650 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = GeoCoordinateWithAltitude_75_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Coordinate_1138 = function(self) { return self.Coordinate_1138; }
    static Altitude_1145 = function(self) { return self.Altitude_1145; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(GeoCoordinateWithAltitude_75_Type);
    static Implements = [Value_23_Concept];
}
class Circle_76_Type
{
    constructor(Center_1152, Radius_1159)
    {
        // field initialization 
        this.Center_1152 = Center_1152;
        this.Radius_1159 = Radius_1159;
        this.Type_2627 = Circle_76_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Circle_76_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Circle_76_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Circle_76_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Circle_76_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Circle_76_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Circle_76_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Circle_76_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Circle_76_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Circle_76_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Center_1152 = function(self) { return self.Center_1152; }
    static Radius_1159 = function(self) { return self.Radius_1159; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Circle_76_Type);
    static Implements = [Value_23_Concept];
}
class Chord_77_Type
{
    constructor(Circle_1166, Arc_1173)
    {
        // field initialization 
        this.Circle_1166 = Circle_1166;
        this.Arc_1173 = Arc_1173;
        this.Type_2627 = Chord_77_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Chord_77_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Chord_77_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Chord_77_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Chord_77_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Chord_77_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Chord_77_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Chord_77_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Chord_77_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Chord_77_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Circle_1166 = function(self) { return self.Circle_1166; }
    static Arc_1173 = function(self) { return self.Arc_1173; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Chord_77_Type);
    static Implements = [Value_23_Concept];
}
class Size2D_78_Type
{
    constructor(Width_1180, Height_1187)
    {
        // field initialization 
        this.Width_1180 = Width_1180;
        this.Height_1187 = Height_1187;
        this.Type_2627 = Size2D_78_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Size2D_78_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Size2D_78_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Size2D_78_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Size2D_78_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Size2D_78_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Size2D_78_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Size2D_78_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Size2D_78_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Size2D_78_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Width_1180 = function(self) { return self.Width_1180; }
    static Height_1187 = function(self) { return self.Height_1187; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Size2D_78_Type);
    static Implements = [Value_23_Concept];
}
class Size3D_79_Type
{
    constructor(Width_1194, Height_1201, Depth_1208)
    {
        // field initialization 
        this.Width_1194 = Width_1194;
        this.Height_1201 = Height_1201;
        this.Depth_1208 = Depth_1208;
        this.Type_2627 = Size3D_79_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Size3D_79_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Size3D_79_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Size3D_79_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Size3D_79_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Size3D_79_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Size3D_79_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Size3D_79_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Size3D_79_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Size3D_79_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Width_1194 = function(self) { return self.Width_1194; }
    static Height_1201 = function(self) { return self.Height_1201; }
    static Depth_1208 = function(self) { return self.Depth_1208; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Size3D_79_Type);
    static Implements = [Value_23_Concept];
}
class Rectangle2D_80_Type
{
    constructor(Center_1215, Size_1222)
    {
        // field initialization 
        this.Center_1215 = Center_1215;
        this.Size_1222 = Size_1222;
        this.Type_2627 = Rectangle2D_80_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Rectangle2D_80_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Rectangle2D_80_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Rectangle2D_80_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Rectangle2D_80_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Rectangle2D_80_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Rectangle2D_80_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Rectangle2D_80_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Rectangle2D_80_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Rectangle2D_80_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Center_1215 = function(self) { return self.Center_1215; }
    static Size_1222 = function(self) { return self.Size_1222; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Rectangle2D_80_Type);
    static Implements = [Value_23_Concept];
}
class Proportion_81_Type
{
    constructor(Value_1229)
    {
        // field initialization 
        this.Value_1229 = Value_1229;
        this.Type_2627 = Proportion_81_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Proportion_81_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Proportion_81_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Proportion_81_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Proportion_81_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Proportion_81_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Proportion_81_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Proportion_81_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Proportion_81_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Proportion_81_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = Proportion_81_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = Proportion_81_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = Proportion_81_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = Proportion_81_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = Proportion_81_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = Proportion_81_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = Proportion_81_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Proportion_81_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Proportion_81_Type.Magnitude_17_Concept.Magnitude_2365;
    }
    // field accessors
    static Value_1229 = function(self) { return self.Value_1229; }
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
    constructor(Numerator_1236, Denominator_1243)
    {
        // field initialization 
        this.Numerator_1236 = Numerator_1236;
        this.Denominator_1243 = Denominator_1243;
        this.Type_2627 = Fraction_82_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Fraction_82_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Fraction_82_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Fraction_82_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Fraction_82_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Fraction_82_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Fraction_82_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Fraction_82_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Fraction_82_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Fraction_82_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Numerator_1236 = function(self) { return self.Numerator_1236; }
    static Denominator_1243 = function(self) { return self.Denominator_1243; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Fraction_82_Type);
    static Implements = [Value_23_Concept];
}
class Angle_83_Type
{
    constructor(Radians_1250)
    {
        // field initialization 
        this.Radians_1250 = Radians_1250;
        this.Type_2627 = Angle_83_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Angle_83_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Angle_83_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Angle_83_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Angle_83_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Angle_83_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Angle_83_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Angle_83_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Angle_83_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Angle_83_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = Angle_83_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = Angle_83_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = Angle_83_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = Angle_83_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = Angle_83_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = Angle_83_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Angle_83_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Angle_83_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = Angle_83_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Radians_1250 = function(self) { return self.Radians_1250; }
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
    constructor(Meters_1257)
    {
        // field initialization 
        this.Meters_1257 = Meters_1257;
        this.Type_2627 = Length_84_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Length_84_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Length_84_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Length_84_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Length_84_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Length_84_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Length_84_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Length_84_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Length_84_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Length_84_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = Length_84_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = Length_84_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = Length_84_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = Length_84_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = Length_84_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = Length_84_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Length_84_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Length_84_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = Length_84_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Meters_1257 = function(self) { return self.Meters_1257; }
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
    constructor(Kilograms_1264)
    {
        // field initialization 
        this.Kilograms_1264 = Kilograms_1264;
        this.Type_2627 = Mass_85_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Mass_85_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Mass_85_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Mass_85_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Mass_85_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Mass_85_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Mass_85_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Mass_85_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Mass_85_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Mass_85_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = Mass_85_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = Mass_85_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = Mass_85_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = Mass_85_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = Mass_85_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = Mass_85_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Mass_85_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Mass_85_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = Mass_85_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Kilograms_1264 = function(self) { return self.Kilograms_1264; }
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
    constructor(Celsius_1271)
    {
        // field initialization 
        this.Celsius_1271 = Celsius_1271;
        this.Type_2627 = Temperature_86_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Temperature_86_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Temperature_86_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Temperature_86_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Temperature_86_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Temperature_86_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Temperature_86_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Temperature_86_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Temperature_86_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Temperature_86_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = Temperature_86_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = Temperature_86_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = Temperature_86_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = Temperature_86_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = Temperature_86_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = Temperature_86_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Temperature_86_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Temperature_86_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = Temperature_86_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Celsius_1271 = function(self) { return self.Celsius_1271; }
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
    constructor(Seconds_1278)
    {
        // field initialization 
        this.Seconds_1278 = Seconds_1278;
        this.Type_2627 = TimeSpan_87_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = TimeSpan_87_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = TimeSpan_87_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = TimeSpan_87_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = TimeSpan_87_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = TimeSpan_87_Type.Value_23_Concept.One_2645;
        this.Default_2650 = TimeSpan_87_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = TimeSpan_87_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = TimeSpan_87_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = TimeSpan_87_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = TimeSpan_87_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = TimeSpan_87_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = TimeSpan_87_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = TimeSpan_87_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = TimeSpan_87_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = TimeSpan_87_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = TimeSpan_87_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = TimeSpan_87_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = TimeSpan_87_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Seconds_1278 = function(self) { return self.Seconds_1278; }
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
    constructor(Min_1285, Max_1292)
    {
        // field initialization 
        this.Min_1285 = Min_1285;
        this.Max_1292 = Max_1292;
        this.Count_2680 = TimeRange_88_Type.Array_25_Concept.Count_2680;
        this.At_2685 = TimeRange_88_Type.Array_25_Concept.At_2685;
        this.Type_2627 = TimeRange_88_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = TimeRange_88_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = TimeRange_88_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = TimeRange_88_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = TimeRange_88_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = TimeRange_88_Type.Value_23_Concept.One_2645;
        this.Default_2650 = TimeRange_88_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = TimeRange_88_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = TimeRange_88_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = TimeRange_88_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = TimeRange_88_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = TimeRange_88_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = TimeRange_88_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = TimeRange_88_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = TimeRange_88_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = TimeRange_88_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = TimeRange_88_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = TimeRange_88_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = TimeRange_88_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Count_2323 = TimeRange_88_Type.Vector_14_Concept.Count_2323;
        this.At_2337 = TimeRange_88_Type.Vector_14_Concept.At_2337;
        this.Min_2673 = TimeRange_88_Type.Interval_24_Concept.Min_2673;
        this.Max_2676 = TimeRange_88_Type.Interval_24_Concept.Max_2676;
    }
    // field accessors
    static Min_1285 = function(self) { return self.Min_1285; }
    static Max_1292 = function(self) { return self.Max_1292; }
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
        this.Type_2627 = DateTime_89_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = DateTime_89_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = DateTime_89_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = DateTime_89_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = DateTime_89_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = DateTime_89_Type.Value_23_Concept.One_2645;
        this.Default_2650 = DateTime_89_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = DateTime_89_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = DateTime_89_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = DateTime_89_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(DateTime_89_Type);
    static Implements = [Value_23_Concept];
}
class AnglePair_90_Type
{
    constructor(Start_1299, End_1306)
    {
        // field initialization 
        this.Start_1299 = Start_1299;
        this.End_1306 = End_1306;
        this.Count_2680 = AnglePair_90_Type.Array_25_Concept.Count_2680;
        this.At_2685 = AnglePair_90_Type.Array_25_Concept.At_2685;
        this.Type_2627 = AnglePair_90_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = AnglePair_90_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = AnglePair_90_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = AnglePair_90_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = AnglePair_90_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = AnglePair_90_Type.Value_23_Concept.One_2645;
        this.Default_2650 = AnglePair_90_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = AnglePair_90_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = AnglePair_90_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = AnglePair_90_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = AnglePair_90_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = AnglePair_90_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = AnglePair_90_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = AnglePair_90_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = AnglePair_90_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = AnglePair_90_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = AnglePair_90_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = AnglePair_90_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = AnglePair_90_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Count_2323 = AnglePair_90_Type.Vector_14_Concept.Count_2323;
        this.At_2337 = AnglePair_90_Type.Vector_14_Concept.At_2337;
        this.Min_2673 = AnglePair_90_Type.Interval_24_Concept.Min_2673;
        this.Max_2676 = AnglePair_90_Type.Interval_24_Concept.Max_2676;
    }
    // field accessors
    static Start_1299 = function(self) { return self.Start_1299; }
    static End_1306 = function(self) { return self.End_1306; }
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
    constructor(Circle_1313, InnerRadius_1320)
    {
        // field initialization 
        this.Circle_1313 = Circle_1313;
        this.InnerRadius_1320 = InnerRadius_1320;
        this.Type_2627 = Ring_91_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Ring_91_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Ring_91_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Ring_91_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Ring_91_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Ring_91_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Ring_91_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Ring_91_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Ring_91_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Ring_91_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = Ring_91_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = Ring_91_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = Ring_91_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = Ring_91_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = Ring_91_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = Ring_91_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = Ring_91_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Ring_91_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Ring_91_Type.Magnitude_17_Concept.Magnitude_2365;
    }
    // field accessors
    static Circle_1313 = function(self) { return self.Circle_1313; }
    static InnerRadius_1320 = function(self) { return self.InnerRadius_1320; }
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
    constructor(Angles_1327, Cirlce_1334)
    {
        // field initialization 
        this.Angles_1327 = Angles_1327;
        this.Cirlce_1334 = Cirlce_1334;
        this.Type_2627 = Arc_92_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Arc_92_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Arc_92_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Arc_92_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Arc_92_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Arc_92_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Arc_92_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Arc_92_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Arc_92_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Arc_92_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Angles_1327 = function(self) { return self.Angles_1327; }
    static Cirlce_1334 = function(self) { return self.Cirlce_1334; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Arc_92_Type);
    static Implements = [Value_23_Concept];
}
class TimeInterval_93_Type
{
    constructor(Start_1341, End_1348)
    {
        // field initialization 
        this.Start_1341 = Start_1341;
        this.End_1348 = End_1348;
        this.Count_2680 = TimeInterval_93_Type.Array_25_Concept.Count_2680;
        this.At_2685 = TimeInterval_93_Type.Array_25_Concept.At_2685;
        this.Type_2627 = TimeInterval_93_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = TimeInterval_93_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = TimeInterval_93_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = TimeInterval_93_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = TimeInterval_93_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = TimeInterval_93_Type.Value_23_Concept.One_2645;
        this.Default_2650 = TimeInterval_93_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = TimeInterval_93_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = TimeInterval_93_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = TimeInterval_93_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = TimeInterval_93_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = TimeInterval_93_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = TimeInterval_93_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = TimeInterval_93_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = TimeInterval_93_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = TimeInterval_93_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = TimeInterval_93_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = TimeInterval_93_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = TimeInterval_93_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Count_2323 = TimeInterval_93_Type.Vector_14_Concept.Count_2323;
        this.At_2337 = TimeInterval_93_Type.Vector_14_Concept.At_2337;
        this.Min_2673 = TimeInterval_93_Type.Interval_24_Concept.Min_2673;
        this.Max_2676 = TimeInterval_93_Type.Interval_24_Concept.Max_2676;
    }
    // field accessors
    static Start_1341 = function(self) { return self.Start_1341; }
    static End_1348 = function(self) { return self.End_1348; }
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
    constructor(A_1355, B_1362)
    {
        // field initialization 
        this.A_1355 = A_1355;
        this.B_1362 = B_1362;
        this.Count_2680 = RealInterval_94_Type.Array_25_Concept.Count_2680;
        this.At_2685 = RealInterval_94_Type.Array_25_Concept.At_2685;
        this.Type_2627 = RealInterval_94_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = RealInterval_94_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = RealInterval_94_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = RealInterval_94_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = RealInterval_94_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = RealInterval_94_Type.Value_23_Concept.One_2645;
        this.Default_2650 = RealInterval_94_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = RealInterval_94_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = RealInterval_94_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = RealInterval_94_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = RealInterval_94_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = RealInterval_94_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = RealInterval_94_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = RealInterval_94_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = RealInterval_94_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = RealInterval_94_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = RealInterval_94_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = RealInterval_94_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = RealInterval_94_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Count_2323 = RealInterval_94_Type.Vector_14_Concept.Count_2323;
        this.At_2337 = RealInterval_94_Type.Vector_14_Concept.At_2337;
        this.Min_2673 = RealInterval_94_Type.Interval_24_Concept.Min_2673;
        this.Max_2676 = RealInterval_94_Type.Interval_24_Concept.Max_2676;
    }
    // field accessors
    static A_1355 = function(self) { return self.A_1355; }
    static B_1362 = function(self) { return self.B_1362; }
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
    constructor(A_1369, B_1376)
    {
        // field initialization 
        this.A_1369 = A_1369;
        this.B_1376 = B_1376;
        this.Count_2680 = Interval2D_95_Type.Array_25_Concept.Count_2680;
        this.At_2685 = Interval2D_95_Type.Array_25_Concept.At_2685;
        this.Type_2627 = Interval2D_95_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Interval2D_95_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Interval2D_95_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Interval2D_95_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Interval2D_95_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Interval2D_95_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Interval2D_95_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Interval2D_95_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Interval2D_95_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Interval2D_95_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = Interval2D_95_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = Interval2D_95_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = Interval2D_95_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = Interval2D_95_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = Interval2D_95_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = Interval2D_95_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = Interval2D_95_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Interval2D_95_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Interval2D_95_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Count_2323 = Interval2D_95_Type.Vector_14_Concept.Count_2323;
        this.At_2337 = Interval2D_95_Type.Vector_14_Concept.At_2337;
        this.Min_2673 = Interval2D_95_Type.Interval_24_Concept.Min_2673;
        this.Max_2676 = Interval2D_95_Type.Interval_24_Concept.Max_2676;
    }
    // field accessors
    static A_1369 = function(self) { return self.A_1369; }
    static B_1376 = function(self) { return self.B_1376; }
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
    constructor(A_1383, B_1390)
    {
        // field initialization 
        this.A_1383 = A_1383;
        this.B_1390 = B_1390;
        this.Count_2680 = Interval3D_96_Type.Array_25_Concept.Count_2680;
        this.At_2685 = Interval3D_96_Type.Array_25_Concept.At_2685;
        this.Type_2627 = Interval3D_96_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Interval3D_96_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Interval3D_96_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Interval3D_96_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Interval3D_96_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Interval3D_96_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Interval3D_96_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Interval3D_96_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Interval3D_96_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Interval3D_96_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = Interval3D_96_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = Interval3D_96_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = Interval3D_96_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = Interval3D_96_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = Interval3D_96_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = Interval3D_96_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = Interval3D_96_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Interval3D_96_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Interval3D_96_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Count_2323 = Interval3D_96_Type.Vector_14_Concept.Count_2323;
        this.At_2337 = Interval3D_96_Type.Vector_14_Concept.At_2337;
        this.Min_2673 = Interval3D_96_Type.Interval_24_Concept.Min_2673;
        this.Max_2676 = Interval3D_96_Type.Interval_24_Concept.Max_2676;
    }
    // field accessors
    static A_1383 = function(self) { return self.A_1383; }
    static B_1390 = function(self) { return self.B_1390; }
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
    constructor(Line_1397, Radius_1404)
    {
        // field initialization 
        this.Line_1397 = Line_1397;
        this.Radius_1404 = Radius_1404;
        this.Type_2627 = Capsule_97_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Capsule_97_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Capsule_97_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Capsule_97_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Capsule_97_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Capsule_97_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Capsule_97_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Capsule_97_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Capsule_97_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Capsule_97_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Line_1397 = function(self) { return self.Line_1397; }
    static Radius_1404 = function(self) { return self.Radius_1404; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Capsule_97_Type);
    static Implements = [Value_23_Concept];
}
class Matrix3D_98_Type
{
    constructor(Column1_1411, Column2_1418, Column3_1425, Column4_1432)
    {
        // field initialization 
        this.Column1_1411 = Column1_1411;
        this.Column2_1418 = Column2_1418;
        this.Column3_1425 = Column3_1425;
        this.Column4_1432 = Column4_1432;
        this.Type_2627 = Matrix3D_98_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Matrix3D_98_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Matrix3D_98_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Matrix3D_98_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Matrix3D_98_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Matrix3D_98_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Matrix3D_98_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Matrix3D_98_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Matrix3D_98_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Matrix3D_98_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Column1_1411 = function(self) { return self.Column1_1411; }
    static Column2_1418 = function(self) { return self.Column2_1418; }
    static Column3_1425 = function(self) { return self.Column3_1425; }
    static Column4_1432 = function(self) { return self.Column4_1432; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Matrix3D_98_Type);
    static Implements = [Value_23_Concept];
}
class Cylinder_99_Type
{
    constructor(Line_1439, Radius_1446)
    {
        // field initialization 
        this.Line_1439 = Line_1439;
        this.Radius_1446 = Radius_1446;
        this.Type_2627 = Cylinder_99_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Cylinder_99_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Cylinder_99_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Cylinder_99_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Cylinder_99_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Cylinder_99_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Cylinder_99_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Cylinder_99_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Cylinder_99_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Cylinder_99_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Line_1439 = function(self) { return self.Line_1439; }
    static Radius_1446 = function(self) { return self.Radius_1446; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Cylinder_99_Type);
    static Implements = [Value_23_Concept];
}
class Cone_100_Type
{
    constructor(Line_1453, Radius_1460)
    {
        // field initialization 
        this.Line_1453 = Line_1453;
        this.Radius_1460 = Radius_1460;
        this.Type_2627 = Cone_100_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Cone_100_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Cone_100_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Cone_100_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Cone_100_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Cone_100_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Cone_100_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Cone_100_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Cone_100_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Cone_100_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Line_1453 = function(self) { return self.Line_1453; }
    static Radius_1460 = function(self) { return self.Radius_1460; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Cone_100_Type);
    static Implements = [Value_23_Concept];
}
class Tube_101_Type
{
    constructor(Line_1467, InnerRadius_1474, OuterRadius_1481)
    {
        // field initialization 
        this.Line_1467 = Line_1467;
        this.InnerRadius_1474 = InnerRadius_1474;
        this.OuterRadius_1481 = OuterRadius_1481;
        this.Type_2627 = Tube_101_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Tube_101_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Tube_101_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Tube_101_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Tube_101_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Tube_101_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Tube_101_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Tube_101_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Tube_101_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Tube_101_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Line_1467 = function(self) { return self.Line_1467; }
    static InnerRadius_1474 = function(self) { return self.InnerRadius_1474; }
    static OuterRadius_1481 = function(self) { return self.OuterRadius_1481; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Tube_101_Type);
    static Implements = [Value_23_Concept];
}
class ConeSegment_102_Type
{
    constructor(Line_1488, Radius1_1495, Radius2_1502)
    {
        // field initialization 
        this.Line_1488 = Line_1488;
        this.Radius1_1495 = Radius1_1495;
        this.Radius2_1502 = Radius2_1502;
        this.Type_2627 = ConeSegment_102_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = ConeSegment_102_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = ConeSegment_102_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = ConeSegment_102_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = ConeSegment_102_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = ConeSegment_102_Type.Value_23_Concept.One_2645;
        this.Default_2650 = ConeSegment_102_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = ConeSegment_102_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = ConeSegment_102_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = ConeSegment_102_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Line_1488 = function(self) { return self.Line_1488; }
    static Radius1_1495 = function(self) { return self.Radius1_1495; }
    static Radius2_1502 = function(self) { return self.Radius2_1502; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(ConeSegment_102_Type);
    static Implements = [Value_23_Concept];
}
class Box2D_103_Type
{
    constructor(Center_1509, Rotation_1516, Extent_1523)
    {
        // field initialization 
        this.Center_1509 = Center_1509;
        this.Rotation_1516 = Rotation_1516;
        this.Extent_1523 = Extent_1523;
        this.Type_2627 = Box2D_103_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Box2D_103_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Box2D_103_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Box2D_103_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Box2D_103_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Box2D_103_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Box2D_103_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Box2D_103_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Box2D_103_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Box2D_103_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Center_1509 = function(self) { return self.Center_1509; }
    static Rotation_1516 = function(self) { return self.Rotation_1516; }
    static Extent_1523 = function(self) { return self.Extent_1523; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Box2D_103_Type);
    static Implements = [Value_23_Concept];
}
class Box3D_104_Type
{
    constructor(Center_1530, Rotation_1537, Extent_1544)
    {
        // field initialization 
        this.Center_1530 = Center_1530;
        this.Rotation_1537 = Rotation_1537;
        this.Extent_1544 = Extent_1544;
        this.Type_2627 = Box3D_104_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Box3D_104_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Box3D_104_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Box3D_104_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Box3D_104_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Box3D_104_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Box3D_104_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Box3D_104_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Box3D_104_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Box3D_104_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Center_1530 = function(self) { return self.Center_1530; }
    static Rotation_1537 = function(self) { return self.Rotation_1537; }
    static Extent_1544 = function(self) { return self.Extent_1544; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(Box3D_104_Type);
    static Implements = [Value_23_Concept];
}
class CubicBezierTriangle3D_105_Type
{
    constructor(A_1551, B_1558, C_1565, A2B_1572, AB2_1579, B2C_1586, BC2_1593, AC2_1600, A2C_1607, ABC_1614)
    {
        // field initialization 
        this.A_1551 = A_1551;
        this.B_1558 = B_1558;
        this.C_1565 = C_1565;
        this.A2B_1572 = A2B_1572;
        this.AB2_1579 = AB2_1579;
        this.B2C_1586 = B2C_1586;
        this.BC2_1593 = BC2_1593;
        this.AC2_1600 = AC2_1600;
        this.A2C_1607 = A2C_1607;
        this.ABC_1614 = ABC_1614;
        this.Type_2627 = CubicBezierTriangle3D_105_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = CubicBezierTriangle3D_105_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = CubicBezierTriangle3D_105_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = CubicBezierTriangle3D_105_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = CubicBezierTriangle3D_105_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = CubicBezierTriangle3D_105_Type.Value_23_Concept.One_2645;
        this.Default_2650 = CubicBezierTriangle3D_105_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = CubicBezierTriangle3D_105_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = CubicBezierTriangle3D_105_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = CubicBezierTriangle3D_105_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static A_1551 = function(self) { return self.A_1551; }
    static B_1558 = function(self) { return self.B_1558; }
    static C_1565 = function(self) { return self.C_1565; }
    static A2B_1572 = function(self) { return self.A2B_1572; }
    static AB2_1579 = function(self) { return self.AB2_1579; }
    static B2C_1586 = function(self) { return self.B2C_1586; }
    static BC2_1593 = function(self) { return self.BC2_1593; }
    static AC2_1600 = function(self) { return self.AC2_1600; }
    static A2C_1607 = function(self) { return self.A2C_1607; }
    static ABC_1614 = function(self) { return self.ABC_1614; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(CubicBezierTriangle3D_105_Type);
    static Implements = [Value_23_Concept];
}
class CubicBezier2D_106_Type
{
    constructor(A_1621, B_1628, C_1635, D_1642)
    {
        // field initialization 
        this.A_1621 = A_1621;
        this.B_1628 = B_1628;
        this.C_1635 = C_1635;
        this.D_1642 = D_1642;
        this.Type_2627 = CubicBezier2D_106_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = CubicBezier2D_106_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = CubicBezier2D_106_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = CubicBezier2D_106_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = CubicBezier2D_106_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = CubicBezier2D_106_Type.Value_23_Concept.One_2645;
        this.Default_2650 = CubicBezier2D_106_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = CubicBezier2D_106_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = CubicBezier2D_106_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = CubicBezier2D_106_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static A_1621 = function(self) { return self.A_1621; }
    static B_1628 = function(self) { return self.B_1628; }
    static C_1635 = function(self) { return self.C_1635; }
    static D_1642 = function(self) { return self.D_1642; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(CubicBezier2D_106_Type);
    static Implements = [Value_23_Concept];
}
class UV_107_Type
{
    constructor(U_1649, V_1656)
    {
        // field initialization 
        this.U_1649 = U_1649;
        this.V_1656 = V_1656;
        this.Count_2680 = UV_107_Type.Array_25_Concept.Count_2680;
        this.At_2685 = UV_107_Type.Array_25_Concept.At_2685;
        this.Type_2627 = UV_107_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = UV_107_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = UV_107_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = UV_107_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = UV_107_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = UV_107_Type.Value_23_Concept.One_2645;
        this.Default_2650 = UV_107_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = UV_107_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = UV_107_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = UV_107_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = UV_107_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = UV_107_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = UV_107_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = UV_107_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = UV_107_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = UV_107_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = UV_107_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = UV_107_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = UV_107_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Count_2323 = UV_107_Type.Vector_14_Concept.Count_2323;
        this.At_2337 = UV_107_Type.Vector_14_Concept.At_2337;
    }
    // field accessors
    static U_1649 = function(self) { return self.U_1649; }
    static V_1656 = function(self) { return self.V_1656; }
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
    constructor(U_1663, V_1670, W_1677)
    {
        // field initialization 
        this.U_1663 = U_1663;
        this.V_1670 = V_1670;
        this.W_1677 = W_1677;
        this.Count_2680 = UVW_108_Type.Array_25_Concept.Count_2680;
        this.At_2685 = UVW_108_Type.Array_25_Concept.At_2685;
        this.Type_2627 = UVW_108_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = UVW_108_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = UVW_108_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = UVW_108_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = UVW_108_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = UVW_108_Type.Value_23_Concept.One_2645;
        this.Default_2650 = UVW_108_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = UVW_108_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = UVW_108_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = UVW_108_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = UVW_108_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = UVW_108_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = UVW_108_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = UVW_108_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = UVW_108_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = UVW_108_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = UVW_108_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = UVW_108_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = UVW_108_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Count_2323 = UVW_108_Type.Vector_14_Concept.Count_2323;
        this.At_2337 = UVW_108_Type.Vector_14_Concept.At_2337;
    }
    // field accessors
    static U_1663 = function(self) { return self.U_1663; }
    static V_1670 = function(self) { return self.V_1670; }
    static W_1677 = function(self) { return self.W_1677; }
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
    constructor(A_1684, B_1691, C_1698, D_1705)
    {
        // field initialization 
        this.A_1684 = A_1684;
        this.B_1691 = B_1691;
        this.C_1698 = C_1698;
        this.D_1705 = D_1705;
        this.Type_2627 = CubicBezier3D_109_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = CubicBezier3D_109_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = CubicBezier3D_109_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = CubicBezier3D_109_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = CubicBezier3D_109_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = CubicBezier3D_109_Type.Value_23_Concept.One_2645;
        this.Default_2650 = CubicBezier3D_109_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = CubicBezier3D_109_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = CubicBezier3D_109_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = CubicBezier3D_109_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static A_1684 = function(self) { return self.A_1684; }
    static B_1691 = function(self) { return self.B_1691; }
    static C_1698 = function(self) { return self.C_1698; }
    static D_1705 = function(self) { return self.D_1705; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(CubicBezier3D_109_Type);
    static Implements = [Value_23_Concept];
}
class QuadraticBezier2D_110_Type
{
    constructor(A_1712, B_1719, C_1726)
    {
        // field initialization 
        this.A_1712 = A_1712;
        this.B_1719 = B_1719;
        this.C_1726 = C_1726;
        this.Type_2627 = QuadraticBezier2D_110_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = QuadraticBezier2D_110_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = QuadraticBezier2D_110_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = QuadraticBezier2D_110_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = QuadraticBezier2D_110_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = QuadraticBezier2D_110_Type.Value_23_Concept.One_2645;
        this.Default_2650 = QuadraticBezier2D_110_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = QuadraticBezier2D_110_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = QuadraticBezier2D_110_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = QuadraticBezier2D_110_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static A_1712 = function(self) { return self.A_1712; }
    static B_1719 = function(self) { return self.B_1719; }
    static C_1726 = function(self) { return self.C_1726; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(QuadraticBezier2D_110_Type);
    static Implements = [Value_23_Concept];
}
class QuadraticBezier3D_111_Type
{
    constructor(A_1733, B_1740, C_1747)
    {
        // field initialization 
        this.A_1733 = A_1733;
        this.B_1740 = B_1740;
        this.C_1747 = C_1747;
        this.Type_2627 = QuadraticBezier3D_111_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = QuadraticBezier3D_111_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = QuadraticBezier3D_111_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = QuadraticBezier3D_111_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = QuadraticBezier3D_111_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = QuadraticBezier3D_111_Type.Value_23_Concept.One_2645;
        this.Default_2650 = QuadraticBezier3D_111_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = QuadraticBezier3D_111_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = QuadraticBezier3D_111_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = QuadraticBezier3D_111_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static A_1733 = function(self) { return self.A_1733; }
    static B_1740 = function(self) { return self.B_1740; }
    static C_1747 = function(self) { return self.C_1747; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(QuadraticBezier3D_111_Type);
    static Implements = [Value_23_Concept];
}
class Area_112_Type
{
    constructor(MetersSquared_1754)
    {
        // field initialization 
        this.MetersSquared_1754 = MetersSquared_1754;
        this.Type_2627 = Area_112_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Area_112_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Area_112_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Area_112_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Area_112_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Area_112_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Area_112_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Area_112_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Area_112_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Area_112_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = Area_112_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = Area_112_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = Area_112_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = Area_112_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = Area_112_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = Area_112_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Area_112_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Area_112_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = Area_112_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static MetersSquared_1754 = function(self) { return self.MetersSquared_1754; }
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
    constructor(MetersCubed_1761)
    {
        // field initialization 
        this.MetersCubed_1761 = MetersCubed_1761;
        this.Type_2627 = Volume_113_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Volume_113_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Volume_113_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Volume_113_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Volume_113_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Volume_113_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Volume_113_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Volume_113_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Volume_113_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Volume_113_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = Volume_113_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = Volume_113_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = Volume_113_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = Volume_113_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = Volume_113_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = Volume_113_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Volume_113_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Volume_113_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = Volume_113_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static MetersCubed_1761 = function(self) { return self.MetersCubed_1761; }
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
    constructor(MetersPerSecond_1768)
    {
        // field initialization 
        this.MetersPerSecond_1768 = MetersPerSecond_1768;
        this.Type_2627 = Velocity_114_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Velocity_114_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Velocity_114_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Velocity_114_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Velocity_114_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Velocity_114_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Velocity_114_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Velocity_114_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Velocity_114_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Velocity_114_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = Velocity_114_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = Velocity_114_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = Velocity_114_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = Velocity_114_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = Velocity_114_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = Velocity_114_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Velocity_114_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Velocity_114_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = Velocity_114_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static MetersPerSecond_1768 = function(self) { return self.MetersPerSecond_1768; }
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
    constructor(MetersPerSecondSquared_1775)
    {
        // field initialization 
        this.MetersPerSecondSquared_1775 = MetersPerSecondSquared_1775;
        this.Type_2627 = Acceleration_115_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Acceleration_115_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Acceleration_115_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Acceleration_115_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Acceleration_115_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Acceleration_115_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Acceleration_115_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Acceleration_115_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Acceleration_115_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Acceleration_115_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = Acceleration_115_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = Acceleration_115_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = Acceleration_115_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = Acceleration_115_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = Acceleration_115_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = Acceleration_115_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Acceleration_115_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Acceleration_115_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = Acceleration_115_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static MetersPerSecondSquared_1775 = function(self) { return self.MetersPerSecondSquared_1775; }
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
    constructor(Newtons_1782)
    {
        // field initialization 
        this.Newtons_1782 = Newtons_1782;
        this.Type_2627 = Force_116_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Force_116_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Force_116_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Force_116_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Force_116_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Force_116_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Force_116_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Force_116_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Force_116_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Force_116_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = Force_116_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = Force_116_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = Force_116_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = Force_116_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = Force_116_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = Force_116_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Force_116_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Force_116_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = Force_116_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Newtons_1782 = function(self) { return self.Newtons_1782; }
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
    constructor(Pascals_1789)
    {
        // field initialization 
        this.Pascals_1789 = Pascals_1789;
        this.Type_2627 = Pressure_117_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Pressure_117_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Pressure_117_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Pressure_117_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Pressure_117_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Pressure_117_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Pressure_117_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Pressure_117_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Pressure_117_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Pressure_117_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = Pressure_117_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = Pressure_117_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = Pressure_117_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = Pressure_117_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = Pressure_117_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = Pressure_117_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Pressure_117_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Pressure_117_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = Pressure_117_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Pascals_1789 = function(self) { return self.Pascals_1789; }
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
    constructor(Joules_1796)
    {
        // field initialization 
        this.Joules_1796 = Joules_1796;
        this.Type_2627 = Energy_118_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Energy_118_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Energy_118_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Energy_118_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Energy_118_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Energy_118_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Energy_118_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Energy_118_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Energy_118_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Energy_118_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = Energy_118_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = Energy_118_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = Energy_118_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = Energy_118_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = Energy_118_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = Energy_118_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Energy_118_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Energy_118_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = Energy_118_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Joules_1796 = function(self) { return self.Joules_1796; }
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
    constructor(Bytes_1803)
    {
        // field initialization 
        this.Bytes_1803 = Bytes_1803;
        this.Type_2627 = Memory_119_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Memory_119_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Memory_119_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Memory_119_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Memory_119_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Memory_119_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Memory_119_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Memory_119_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Memory_119_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Memory_119_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = Memory_119_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = Memory_119_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = Memory_119_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = Memory_119_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = Memory_119_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = Memory_119_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Memory_119_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Memory_119_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = Memory_119_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Bytes_1803 = function(self) { return self.Bytes_1803; }
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
    constructor(Hertz_1810)
    {
        // field initialization 
        this.Hertz_1810 = Hertz_1810;
        this.Type_2627 = Frequency_120_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Frequency_120_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Frequency_120_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Frequency_120_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Frequency_120_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Frequency_120_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Frequency_120_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Frequency_120_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Frequency_120_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Frequency_120_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = Frequency_120_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = Frequency_120_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = Frequency_120_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = Frequency_120_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = Frequency_120_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = Frequency_120_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Frequency_120_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Frequency_120_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = Frequency_120_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Hertz_1810 = function(self) { return self.Hertz_1810; }
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
    constructor(Decibels_1817)
    {
        // field initialization 
        this.Decibels_1817 = Decibels_1817;
        this.Type_2627 = Loudness_121_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Loudness_121_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Loudness_121_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Loudness_121_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Loudness_121_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Loudness_121_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Loudness_121_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Loudness_121_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Loudness_121_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Loudness_121_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = Loudness_121_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = Loudness_121_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = Loudness_121_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = Loudness_121_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = Loudness_121_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = Loudness_121_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Loudness_121_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Loudness_121_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = Loudness_121_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Decibels_1817 = function(self) { return self.Decibels_1817; }
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
    constructor(Candelas_1824)
    {
        // field initialization 
        this.Candelas_1824 = Candelas_1824;
        this.Type_2627 = LuminousIntensity_122_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = LuminousIntensity_122_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = LuminousIntensity_122_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = LuminousIntensity_122_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = LuminousIntensity_122_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = LuminousIntensity_122_Type.Value_23_Concept.One_2645;
        this.Default_2650 = LuminousIntensity_122_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = LuminousIntensity_122_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = LuminousIntensity_122_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = LuminousIntensity_122_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = LuminousIntensity_122_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = LuminousIntensity_122_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = LuminousIntensity_122_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = LuminousIntensity_122_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = LuminousIntensity_122_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = LuminousIntensity_122_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = LuminousIntensity_122_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = LuminousIntensity_122_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = LuminousIntensity_122_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Candelas_1824 = function(self) { return self.Candelas_1824; }
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
    constructor(Volts_1831)
    {
        // field initialization 
        this.Volts_1831 = Volts_1831;
        this.Type_2627 = ElectricPotential_123_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = ElectricPotential_123_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = ElectricPotential_123_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = ElectricPotential_123_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = ElectricPotential_123_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = ElectricPotential_123_Type.Value_23_Concept.One_2645;
        this.Default_2650 = ElectricPotential_123_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = ElectricPotential_123_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = ElectricPotential_123_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = ElectricPotential_123_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = ElectricPotential_123_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = ElectricPotential_123_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = ElectricPotential_123_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = ElectricPotential_123_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = ElectricPotential_123_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = ElectricPotential_123_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = ElectricPotential_123_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = ElectricPotential_123_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = ElectricPotential_123_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Volts_1831 = function(self) { return self.Volts_1831; }
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
    constructor(Columbs_1838)
    {
        // field initialization 
        this.Columbs_1838 = Columbs_1838;
        this.Type_2627 = ElectricCharge_124_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = ElectricCharge_124_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = ElectricCharge_124_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = ElectricCharge_124_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = ElectricCharge_124_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = ElectricCharge_124_Type.Value_23_Concept.One_2645;
        this.Default_2650 = ElectricCharge_124_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = ElectricCharge_124_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = ElectricCharge_124_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = ElectricCharge_124_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = ElectricCharge_124_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = ElectricCharge_124_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = ElectricCharge_124_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = ElectricCharge_124_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = ElectricCharge_124_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = ElectricCharge_124_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = ElectricCharge_124_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = ElectricCharge_124_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = ElectricCharge_124_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Columbs_1838 = function(self) { return self.Columbs_1838; }
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
    constructor(Amperes_1845)
    {
        // field initialization 
        this.Amperes_1845 = Amperes_1845;
        this.Type_2627 = ElectricCurrent_125_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = ElectricCurrent_125_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = ElectricCurrent_125_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = ElectricCurrent_125_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = ElectricCurrent_125_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = ElectricCurrent_125_Type.Value_23_Concept.One_2645;
        this.Default_2650 = ElectricCurrent_125_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = ElectricCurrent_125_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = ElectricCurrent_125_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = ElectricCurrent_125_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = ElectricCurrent_125_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = ElectricCurrent_125_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = ElectricCurrent_125_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = ElectricCurrent_125_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = ElectricCurrent_125_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = ElectricCurrent_125_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = ElectricCurrent_125_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = ElectricCurrent_125_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = ElectricCurrent_125_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Amperes_1845 = function(self) { return self.Amperes_1845; }
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
    constructor(Ohms_1852)
    {
        // field initialization 
        this.Ohms_1852 = Ohms_1852;
        this.Type_2627 = ElectricResistance_126_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = ElectricResistance_126_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = ElectricResistance_126_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = ElectricResistance_126_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = ElectricResistance_126_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = ElectricResistance_126_Type.Value_23_Concept.One_2645;
        this.Default_2650 = ElectricResistance_126_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = ElectricResistance_126_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = ElectricResistance_126_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = ElectricResistance_126_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = ElectricResistance_126_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = ElectricResistance_126_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = ElectricResistance_126_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = ElectricResistance_126_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = ElectricResistance_126_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = ElectricResistance_126_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = ElectricResistance_126_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = ElectricResistance_126_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = ElectricResistance_126_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Ohms_1852 = function(self) { return self.Ohms_1852; }
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
    constructor(Watts_1859)
    {
        // field initialization 
        this.Watts_1859 = Watts_1859;
        this.Type_2627 = Power_127_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Power_127_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Power_127_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Power_127_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Power_127_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Power_127_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Power_127_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Power_127_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Power_127_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Power_127_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = Power_127_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = Power_127_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = Power_127_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = Power_127_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = Power_127_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = Power_127_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Power_127_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Power_127_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = Power_127_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static Watts_1859 = function(self) { return self.Watts_1859; }
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
    constructor(KilogramsPerMeterCubed_1866)
    {
        // field initialization 
        this.KilogramsPerMeterCubed_1866 = KilogramsPerMeterCubed_1866;
        this.Type_2627 = Density_128_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Density_128_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Density_128_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Density_128_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Density_128_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Density_128_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Density_128_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Density_128_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Density_128_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Density_128_Type.Value_23_Concept.ToString_2669;
        this.Add_2525 = Density_128_Type.ScalarArithmetic_21_Concept.Add_2525;
        this.Subtract_2539 = Density_128_Type.ScalarArithmetic_21_Concept.Subtract_2539;
        this.Multiply_2553 = Density_128_Type.ScalarArithmetic_21_Concept.Multiply_2553;
        this.Divide_2567 = Density_128_Type.ScalarArithmetic_21_Concept.Divide_2567;
        this.Modulo_2581 = Density_128_Type.ScalarArithmetic_21_Concept.Modulo_2581;
        this.Equals_2422 = Density_128_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Density_128_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Density_128_Type.Magnitude_17_Concept.Magnitude_2365;
        this.Value_2349 = Density_128_Type.Measure_15_Concept.Value_2349;
    }
    // field accessors
    static KilogramsPerMeterCubed_1866 = function(self) { return self.KilogramsPerMeterCubed_1866; }
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
    constructor(Mean_1873, StandardDeviation_1880)
    {
        // field initialization 
        this.Mean_1873 = Mean_1873;
        this.StandardDeviation_1880 = StandardDeviation_1880;
        this.Type_2627 = NormalDistribution_129_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = NormalDistribution_129_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = NormalDistribution_129_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = NormalDistribution_129_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = NormalDistribution_129_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = NormalDistribution_129_Type.Value_23_Concept.One_2645;
        this.Default_2650 = NormalDistribution_129_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = NormalDistribution_129_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = NormalDistribution_129_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = NormalDistribution_129_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Mean_1873 = function(self) { return self.Mean_1873; }
    static StandardDeviation_1880 = function(self) { return self.StandardDeviation_1880; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(NormalDistribution_129_Type);
    static Implements = [Value_23_Concept];
}
class PoissonDistribution_130_Type
{
    constructor(Expected_1887, Occurrences_1894)
    {
        // field initialization 
        this.Expected_1887 = Expected_1887;
        this.Occurrences_1894 = Occurrences_1894;
        this.Type_2627 = PoissonDistribution_130_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = PoissonDistribution_130_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = PoissonDistribution_130_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = PoissonDistribution_130_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = PoissonDistribution_130_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = PoissonDistribution_130_Type.Value_23_Concept.One_2645;
        this.Default_2650 = PoissonDistribution_130_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = PoissonDistribution_130_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = PoissonDistribution_130_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = PoissonDistribution_130_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Expected_1887 = function(self) { return self.Expected_1887; }
    static Occurrences_1894 = function(self) { return self.Occurrences_1894; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(PoissonDistribution_130_Type);
    static Implements = [Value_23_Concept];
}
class BernoulliDistribution_131_Type
{
    constructor(P_1901)
    {
        // field initialization 
        this.P_1901 = P_1901;
        this.Type_2627 = BernoulliDistribution_131_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = BernoulliDistribution_131_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = BernoulliDistribution_131_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = BernoulliDistribution_131_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = BernoulliDistribution_131_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = BernoulliDistribution_131_Type.Value_23_Concept.One_2645;
        this.Default_2650 = BernoulliDistribution_131_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = BernoulliDistribution_131_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = BernoulliDistribution_131_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = BernoulliDistribution_131_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static P_1901 = function(self) { return self.P_1901; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(BernoulliDistribution_131_Type);
    static Implements = [Value_23_Concept];
}
class Probability_132_Type
{
    constructor(Value_1908)
    {
        // field initialization 
        this.Value_1908 = Value_1908;
        this.Type_2627 = Probability_132_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = Probability_132_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = Probability_132_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = Probability_132_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = Probability_132_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = Probability_132_Type.Value_23_Concept.One_2645;
        this.Default_2650 = Probability_132_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = Probability_132_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = Probability_132_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = Probability_132_Type.Value_23_Concept.ToString_2669;
        this.Add_2439 = Probability_132_Type.Arithmetic_20_Concept.Add_2439;
        this.Negative_2449 = Probability_132_Type.Arithmetic_20_Concept.Negative_2449;
        this.Reciprocal_2459 = Probability_132_Type.Arithmetic_20_Concept.Reciprocal_2459;
        this.Multiply_2476 = Probability_132_Type.Arithmetic_20_Concept.Multiply_2476;
        this.Divide_2493 = Probability_132_Type.Arithmetic_20_Concept.Divide_2493;
        this.Modulo_2510 = Probability_132_Type.Arithmetic_20_Concept.Modulo_2510;
        this.Equals_2422 = Probability_132_Type.Equatable_19_Concept.Equals_2422;
        this.Compare_2402 = Probability_132_Type.Comparable_18_Concept.Compare_2402;
        this.Magnitude_2365 = Probability_132_Type.Magnitude_17_Concept.Magnitude_2365;
    }
    // field accessors
    static Value_1908 = function(self) { return self.Value_1908; }
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
    constructor(Trials_1915, P_1922)
    {
        // field initialization 
        this.Trials_1915 = Trials_1915;
        this.P_1922 = P_1922;
        this.Type_2627 = BinomialDistribution_133_Type.Value_23_Concept.Type_2627;
        this.FieldTypes_2629 = BinomialDistribution_133_Type.Value_23_Concept.FieldTypes_2629;
        this.FieldNames_2631 = BinomialDistribution_133_Type.Value_23_Concept.FieldNames_2631;
        this.FieldValues_2635 = BinomialDistribution_133_Type.Value_23_Concept.FieldValues_2635;
        this.Zero_2640 = BinomialDistribution_133_Type.Value_23_Concept.Zero_2640;
        this.One_2645 = BinomialDistribution_133_Type.Value_23_Concept.One_2645;
        this.Default_2650 = BinomialDistribution_133_Type.Value_23_Concept.Default_2650;
        this.MinValue_2655 = BinomialDistribution_133_Type.Value_23_Concept.MinValue_2655;
        this.MaxValue_2660 = BinomialDistribution_133_Type.Value_23_Concept.MaxValue_2660;
        this.ToString_2669 = BinomialDistribution_133_Type.Value_23_Concept.ToString_2669;
    }
    // field accessors
    static Trials_1915 = function(self) { return self.Trials_1915; }
    static P_1922 = function(self) { return self.P_1922; }
    // implemented concepts 
    static Value_23_Concept = new Value_23_Concept(BinomialDistribution_133_Type);
    static Implements = [Value_23_Concept];
}

// This is appended to every JavaScript program generated from Plato