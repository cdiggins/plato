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
    static Cos_2321 = function (x_2320/* : Angle_84 */) /* : Number_30 */{ return null; };
    static Sin_2324 = function (x_2323/* : Angle_84 */) /* : Number_30 */{ return null; };
    static Tan_2327 = function (x_2326/* : Angle_84 */) /* : Number_30 */{ return null; };
    static Acos_2330 = function (x_2329/* : Number_30 */) /* : Angle_84 */{ return null; };
    static Asin_2333 = function (x_2332/* : Number_30 */) /* : Angle_84 */{ return null; };
    static Atan_2336 = function (x_2335/* : Number_30 */) /* : Angle_84 */{ return null; };
    static Pow_2341 = function (x_2338/* : Number_30 */, y_2340/* : Number_30 */) /* : Number_30 */{ return null; };
    static Log_2346 = function (x_2343/* : Number_30 */, y_2345/* : Number_30 */) /* : Number_30 */{ return null; };
    static NaturalLog_2349 = function (x_2348/* : Number_30 */) /* : Number_30 */{ return null; };
    static NaturalPower_2352 = function (x_2351/* : Number_30 */) /* : Number_30 */{ return null; };
    static Interpolate_2355 = function (xs_2354/* : Array_15 */) /* : String_8 */{ return null; };
    static Throw_2358 = function (x_2357/* : Any_14 */) /* : Any_14 */{ return null; };
    static TypeOf_2361 = function (x_2360/* : Any_14 */) /* : Type_12 */{ return null; };
    static Add_2366 = function (x_2363/* : Number_30 */, y_2365/* : Number_30 */) /* : Number_30 */{ return null; };
    static Subtract_2371 = function (x_2368/* : Number_30 */, y_2370/* : Number_30 */) /* : Number_30 */{ return null; };
    static Divide_2376 = function (x_2373/* : Number_30 */, y_2375/* : Number_30 */) /* : Number_30 */{ return null; };
    static Multiply_2381 = function (x_2378/* : Number_30 */, y_2380/* : Number_30 */) /* : Number_30 */{ return null; };
    static Modulo_2386 = function (x_2383/* : Number_30 */, y_2385/* : Number_30 */) /* : Number_30 */{ return null; };
    static Negative_2389 = function (x_2388/* : Number_30 */) /* : Number_30 */{ return null; };
    static Add_2394 = function (x_2391/* : Integer_27 */, y_2393/* : Integer_27 */) /* : Integer_27 */{ return null; };
    static Subtract_2399 = function (x_2396/* : Integer_27 */, y_2398/* : Integer_27 */) /* : Integer_27 */{ return null; };
    static Divide_2404 = function (x_2401/* : Integer_27 */, y_2403/* : Integer_27 */) /* : Integer_27 */{ return null; };
    static Multiply_2409 = function (x_2406/* : Integer_27 */, y_2408/* : Integer_27 */) /* : Integer_27 */{ return null; };
    static Modulo_2414 = function (x_2411/* : Integer_27 */, y_2413/* : Integer_27 */) /* : Integer_27 */{ return null; };
    static Negative_2417 = function (x_2416/* : Integer_27 */) /* : Integer_27 */{ return null; };
    static And_2422 = function (x_2419/* : Boolean_25 */, y_2421/* : Boolean_25 */) /* : Boolean_25 */{ return null; };
    static Or_2427 = function (x_2424/* : Boolean_25 */, y_2426/* : Boolean_25 */) /* : Boolean_25 */{ return null; };
    static Not_2430 = function (x_2429/* : Boolean_25 */) /* : Boolean_25 */{ return null; };
    static FieldTypes_2433 = function (x_2432/* : Any_14 */) /* : Array_15 */{ return null; };
    static FieldNames_2436 = function (x_2435/* : Any_14 */) /* : Array_15 */{ return null; };
    static FieldValues_2439 = function (x_2438/* : Any_14 */) /* : Array_15 */{ return null; };
}
class Array_135_Library
{
    static Map_2802 = function (xs_2778/* : Array_15 */, f_2780/* : Function_4 */) /* : Array_15 */{ return Tuple_7_Primitive/* : Tuple_7 */(Count_28_Type/* : UnknownType */(xs_2778/* : UnknownType */)/* : UnknownType */, function (i_2787/* : UnknownType */) /* : Lambda_3 */{ return f_2780/* : UnknownType */(At_256/* : UnknownType */(xs_2778/* : UnknownType */, i_2787/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Tuple_7 */; };
    static Reverse_2839 = function (xs_2804/* : Array_15 */) /* : Array_15 */{ return Tuple_7_Primitive/* : Tuple_7 */(Count_28_Type/* : UnknownType */(xs_2804/* : UnknownType */)/* : UnknownType */, function (i_2811/* : UnknownType */) /* : Lambda_3 */{ return f_2780/* : UnknownType */(At_256/* : UnknownType */(xs_2804/* : UnknownType */, Subtract_187/* : UnknownType */(Count_28_Type/* : UnknownType */(xs_2804/* : UnknownType */)/* : UnknownType */, Subtract_187/* : UnknownType */(1/* : Int_10 */, i_2811/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Tuple_7 */; };
    static Zip_2872 = function (xs_2841/* : Array_15 */, ys_2843/* : Array_15 */, f_2845/* : Function_4 */) /* : Array_15 */{ return Tuple_7_Primitive/* : Tuple_7 */(Count_28_Type/* : UnknownType */(xs_2841/* : UnknownType */)/* : UnknownType */, function (i_2852/* : UnknownType */) /* : Lambda_3 */{ return f_2845/* : UnknownType */(At_256/* : UnknownType */(i_2852/* : UnknownType */)/* : UnknownType */, At_256/* : UnknownType */(ys_2843/* : UnknownType */, i_2852/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Tuple_7 */; };
    static Zip_2914 = function (xs_2874/* : Array_15 */, ys_2876/* : Array_15 */, zs_2878/* : Array_15 */, f_2880/* : Function_4 */) /* : Array_15 */{ return Tuple_7_Primitive/* : Tuple_7 */(Count_28_Type/* : UnknownType */(xs_2874/* : UnknownType */)/* : UnknownType */, function (i_2887/* : UnknownType */) /* : Lambda_3 */{ return f_2880/* : UnknownType */(At_256/* : UnknownType */(i_2887/* : UnknownType */)/* : UnknownType */, At_256/* : UnknownType */(ys_2876/* : UnknownType */, i_2887/* : UnknownType */)/* : UnknownType */, At_256/* : UnknownType */(zs_2878/* : UnknownType */, i_2887/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Tuple_7 */; };
    static Skip_2942 = function (xs_2916/* : Array_15 */, n_2918/* : Count_28 */) /* : Array_15 */{ return Tuple_7_Primitive/* : Tuple_7 */(Subtract_187/* : UnknownType */(Count_28_Type/* : UnknownType */, n_2918/* : UnknownType */)/* : UnknownType */, function (i_2927/* : UnknownType */) /* : Lambda_3 */{ return At_256/* : UnknownType */(Subtract_187/* : UnknownType */(i_2927/* : UnknownType */, n_2918/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Tuple_7 */; };
    static Take_2960 = function (xs_2944/* : Array_15 */, n_2946/* : Count_28 */) /* : Array_15 */{ return Tuple_7_Primitive/* : Tuple_7 */(n_2946/* : UnknownType */, function (i_2950/* : UnknownType */) /* : Lambda_3 */{ return At_256/* : UnknownType */(i_2950/* : UnknownType */)/* : UnknownType */; })/* : Tuple_7 */; };
    static Aggregate_2985 = function (xs_2962/* : Array_15 */, init_2964/* : Any_14 */, f_2966/* : Function_4 */) /* : Any_14 */{ return IsEmpty_1945/* : UnknownType */(xs_2962/* : UnknownType */)/* : UnknownType */
        ? init_2964/* : Any_14 */
        : f_2966/* : Function_4 */(init_2964/* : UnknownType */, f_2966/* : UnknownType */(Rest_1942/* : UnknownType */(xs_2962/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Function_4 */
    ; };
    static Rest_2994 = function (xs_2987/* : Array_15 */) /* : Array_15 */{ return Skip_1933/* : Array_15 */(xs_2987/* : UnknownType */, 1/* : Int_10 */)/* : Array_15 */; };
    static IsEmpty_3006 = function (xs_2996/* : Array_15 */) /* : Boolean_25 */{ return Equals_294/* : Boolean_25 */(Count_28_Type/* : UnknownType */(xs_2996/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_25 */; };
    static First_3015 = function (xs_3008/* : Array_15 */) /* : Any_14 */{ return At_256/* : T_251 */(xs_3008/* : UnknownType */, 0/* : Int_10 */)/* : T_251 */; };
    static Last_3032 = function (xs_3017/* : Array_15 */) /* : Any_14 */{ return At_256/* : T_251 */(xs_3017/* : UnknownType */, Subtract_187/* : UnknownType */(Count_28_Type/* : UnknownType */(xs_3017/* : UnknownType */)/* : UnknownType */, 1/* : Int_10 */)/* : UnknownType */)/* : T_251 */; };
    static Slice_3050 = function (xs_3034/* : Array_15 */, from_3036/* : Index_29 */, count_3038/* : Count_28 */) /* : Array_15 */{ return Take_1936/* : Array_15 */(Skip_1933/* : UnknownType */(xs_3034/* : UnknownType */, from_3036/* : UnknownType */)/* : UnknownType */, count_3038/* : UnknownType */)/* : Array_15 */; };
    static Join_3096 = function (xs_3052/* : Array_15 */, sep_3054/* : String_8 */) /* : String_8 */{ return IsEmpty_1945/* : UnknownType */(xs_3052/* : UnknownType */)/* : UnknownType */
        ? ""/* : String_8 */
        : Add_184/* : Number_30 */(ToString_2029/* : UnknownType */(First_1948/* : UnknownType */(xs_3052/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Aggregate_1939/* : UnknownType */(Rest_1942/* : UnknownType */(xs_3052/* : UnknownType */)/* : UnknownType */, ""/* : String_8 */, function (acc_3076/* : UnknownType */, cur_3078/* : UnknownType */) /* : Lambda_3 */{ return Interpolate_175/* : UnknownType */(acc_3076/* : UnknownType */, sep_3054/* : UnknownType */, cur_3078/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */)/* : Number_30 */
    ; };
    static All_3125 = function (xs_3098/* : Array_15 */, f_3100/* : Function_4 */) /* : Boolean_25 */{ return IsEmpty_1945/* : UnknownType */(xs_3098/* : UnknownType */)/* : UnknownType */
        ? True/* : Bool_9 */
        : And_220/* : Boolean_25 */(f_3100/* : UnknownType */(First_1948/* : UnknownType */(xs_3098/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, f_3100/* : UnknownType */(Rest_1942/* : UnknownType */(xs_3098/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Boolean_25 */
    ; };
}
class Interval_136_Library
{
    static Size_3140 = function (x_3127/* : Interval_26 */) /* : Numerical_19 */{ return Subtract_187/* : Number_30 */(Max_343/* : UnknownType */(x_3127/* : UnknownType */)/* : UnknownType */, Min_340/* : UnknownType */(x_3127/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static IsEmpty_3155 = function (x_3142/* : Interval_26 */) /* : Boolean_25 */{ return GreaterThanOrEquals_2203/* : Boolean_25 */(Min_340/* : UnknownType */(x_3142/* : UnknownType */)/* : UnknownType */, Max_343/* : UnknownType */(x_3142/* : UnknownType */)/* : UnknownType */)/* : Boolean_25 */; };
    static Lerp_3187 = function (x_3157/* : Interval_26 */, amount_3159/* : Unit_31 */) /* : Numerical_19 */{ return Multiply_193/* : Number_30 */(Min_340/* : UnknownType */(x_3157/* : UnknownType */)/* : UnknownType */, Add_184/* : UnknownType */(Subtract_187/* : UnknownType */(1/* : Float_11 */, amount_3159/* : UnknownType */)/* : UnknownType */, Multiply_193/* : UnknownType */(Max_343/* : UnknownType */(x_3157/* : UnknownType */)/* : UnknownType */, amount_3159/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static InverseLerp_3209 = function (x_3189/* : Interval_26 */, value_3191/* : Numerical_19 */) /* : Unit_31 */{ return Divide_190/* : Number_30 */(Subtract_187/* : UnknownType */(value_3191/* : UnknownType */, Min_340/* : UnknownType */(x_3189/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Size_1214/* : UnknownType */(x_3189/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static Negate_3230 = function (x_3211/* : Interval_26 */) /* : Interval_26 */{ return Tuple_7_Primitive/* : Tuple_7 */(Negative_199/* : UnknownType */(Max_343/* : UnknownType */(x_3211/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Negative_199/* : UnknownType */(Min_340/* : UnknownType */(x_3211/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static Reverse_3245 = function (x_3232/* : Interval_26 */) /* : Interval_26 */{ return Tuple_7_Primitive/* : Tuple_7 */(Max_343/* : UnknownType */(x_3232/* : UnknownType */)/* : UnknownType */, Min_340/* : UnknownType */(x_3232/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static Center_3254 = function (x_3247/* : Interval_26 */) /* : Numerical_19 */{ return Lerp_1969/* : Numerical_19 */(x_3247/* : UnknownType */, 0.5/* : Float_11 */)/* : Numerical_19 */; };
    static Contains_3281 = function (x_3256/* : Interval_26 */, value_3258/* : Numerical_19 */) /* : Boolean_25 */{ return LessThanOrEquals_2197/* : Boolean_25 */(Min_340/* : UnknownType */(x_3256/* : UnknownType */)/* : UnknownType */, And_220/* : UnknownType */(value_3258/* : UnknownType */, LessThanOrEquals_2197/* : UnknownType */(value_3258/* : UnknownType */, Max_343/* : UnknownType */(x_3256/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Boolean_25 */; };
    static Contains_3311 = function (x_3283/* : Interval_26 */, other_3285/* : Interval_26 */) /* : Boolean_25 */{ return LessThanOrEquals_2197/* : Boolean_25 */(Min_340/* : UnknownType */(x_3283/* : UnknownType */)/* : UnknownType */, And_220/* : UnknownType */(Min_340/* : UnknownType */(other_3285/* : UnknownType */)/* : UnknownType */, GreaterThanOrEquals_2203/* : UnknownType */(Max_343/* : UnknownType */, Max_343/* : UnknownType */(other_3285/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Boolean_25 */; };
    static Overlaps_3328 = function (x_3313/* : Interval_26 */, y_3315/* : Interval_26 */) /* : Boolean_25 */{ return Not_226/* : Boolean_25 */(IsEmpty_1945/* : UnknownType */(Clamp_2020/* : UnknownType */(x_3313/* : UnknownType */, y_3315/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Boolean_25 */; };
    static Split_3349 = function (x_3330/* : Interval_26 */, t_3332/* : Unit_31 */) /* : Tuple_7 */{ return Tuple_7_Primitive/* : Tuple_7 */(Left_1999/* : UnknownType */(x_3330/* : UnknownType */, t_3332/* : UnknownType */)/* : UnknownType */, Right_2002/* : UnknownType */(x_3330/* : UnknownType */, t_3332/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static Split_3358 = function (x_3351/* : Interval_26 */) /* : Tuple_7 */{ return Split_1993/* : Tuple_7 */(x_3351/* : UnknownType */, 0.5/* : Float_11 */)/* : Tuple_7 */; };
    static Left_3377 = function (x_3360/* : Interval_26 */, t_3362/* : Unit_31 */) /* : Interval_26 */{ return Tuple_7_Primitive/* : Tuple_7 */(Min_340/* : UnknownType */(x_3360/* : UnknownType */)/* : UnknownType */, Lerp_1969/* : UnknownType */(x_3360/* : UnknownType */, t_3362/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static Right_3396 = function (x_3379/* : Interval_26 */, t_3381/* : Unit_31 */) /* : Interval_26 */{ return Tuple_7_Primitive/* : Tuple_7 */(Lerp_1969/* : UnknownType */(x_3379/* : UnknownType */, t_3381/* : UnknownType */)/* : UnknownType */, Max_343/* : UnknownType */(x_3379/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static MoveTo_3415 = function (x_3398/* : Interval_26 */, v_3400/* : Numerical_19 */) /* : Interval_26 */{ return Tuple_7_Primitive/* : Tuple_7 */(v_3400/* : UnknownType */, Add_184/* : UnknownType */(v_3400/* : UnknownType */, Size_1214/* : UnknownType */(x_3398/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static LeftHalf_3424 = function (x_3417/* : Interval_26 */) /* : Interval_26 */{ return Left_1999/* : Interval_26 */(x_3417/* : UnknownType */, 0.5/* : Float_11 */)/* : Interval_26 */; };
    static RightHalf_3433 = function (x_3426/* : Interval_26 */) /* : Interval_26 */{ return Right_2002/* : Interval_26 */(x_3426/* : UnknownType */, 0.5/* : Float_11 */)/* : Interval_26 */; };
    static HalfSize_3443 = function (x_3435/* : Interval_26 */) /* : Numerical_19 */{ return Half_2095/* : Numerical_19 */(Size_1214/* : UnknownType */(x_3435/* : UnknownType */)/* : UnknownType */)/* : Numerical_19 */; };
    static Recenter_3470 = function (x_3445/* : Interval_26 */, c_3447/* : Numerical_19 */) /* : Interval_26 */{ return Tuple_7_Primitive/* : Tuple_7 */(Subtract_187/* : UnknownType */(c_3447/* : UnknownType */, HalfSize_2014/* : UnknownType */(x_3445/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Add_184/* : UnknownType */(c_3447/* : UnknownType */, HalfSize_2014/* : UnknownType */(x_3445/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static Clamp_3497 = function (x_3472/* : Interval_26 */, y_3474/* : Interval_26 */) /* : Interval_26 */{ return Tuple_7_Primitive/* : Tuple_7 */(Clamp_2020/* : UnknownType */(x_3472/* : UnknownType */, Min_340/* : UnknownType */(y_3474/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Clamp_2020/* : UnknownType */(x_3472/* : UnknownType */, Max_343/* : UnknownType */(y_3474/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Tuple_7 */; };
    static Clamp_3531 = function (x_3499/* : Interval_26 */, value_3501/* : Numerical_19 */) /* : Numerical_19 */{ return LessThan_2194/* : Boolean_25 */(value_3501/* : UnknownType */, Min_340/* : UnknownType */(x_3499/* : UnknownType */)/* : UnknownType */
        ? Min_340/* : UnknownType */(x_3499/* : UnknownType */)/* : UnknownType */
        : GreaterThan_2200/* : UnknownType */(value_3501/* : UnknownType */, Max_343/* : UnknownType */(x_3499/* : UnknownType */)/* : UnknownType */
            ? Max_343/* : UnknownType */(x_3499/* : UnknownType */)/* : UnknownType */
            : value_3501/* : UnknownType */
        )/* : UnknownType */
    )/* : Boolean_25 */; };
    static Within_3558 = function (x_3533/* : Interval_26 */, value_3535/* : Numerical_19 */) /* : Boolean_25 */{ return GreaterThanOrEquals_2203/* : Boolean_25 */(value_3535/* : UnknownType */, And_220/* : UnknownType */(Min_340/* : UnknownType */(x_3533/* : UnknownType */)/* : UnknownType */, LessThanOrEquals_2197/* : UnknownType */(value_3535/* : UnknownType */, Max_343/* : UnknownType */(x_3533/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Boolean_25 */; };
}
class Value_137_Library
{
    static ToString_3570 = function (x_3560/* : Value_16 */) /* : String_8 */{ return Join_1957/* : String_8 */(FieldValues_238/* : UnknownType */(x_3560/* : UnknownType */)/* : UnknownType */, ", "/* : String_8 */)/* : String_8 */; };
}
class Vector_138_Library
{
    static Sum_3581 = function (v_3572/* : Vector_17 */) /* : Number_30 */{ return Aggregate_1939/* : Any_14 */(v_3572/* : UnknownType */, 0/* : Int_10 */, Add_184/* : UnknownType */)/* : Any_14 */; };
    static SumSquares_3595 = function (v_3583/* : Vector_17 */) /* : Number_30 */{ return Aggregate_1939/* : Any_14 */(Square_2053/* : UnknownType */(v_3583/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */, Add_184/* : UnknownType */)/* : Any_14 */; };
    static LengthSquared_3602 = function (v_3597/* : Vector_17 */) /* : Number_30 */{ return SumSquares_2035/* : Number_30 */(v_3597/* : UnknownType */)/* : Number_30 */; };
    static Length_3612 = function (v_3604/* : Vector_17 */) /* : Number_30 */{ return SquareRoot_2050/* : Numerical_19 */(LengthSquared_2038/* : UnknownType */(v_3604/* : UnknownType */)/* : UnknownType */)/* : Numerical_19 */; };
    static Dot_3626 = function (v1_3614/* : Vector_17 */, v2_3616/* : Vector_17 */) /* : Number_30 */{ return Sum_2032/* : Number_30 */(Multiply_193/* : UnknownType */(v1_3614/* : UnknownType */, v2_3616/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static Normal_3638 = function (v_3628/* : Vector_17 */) /* : Vector_17 */{ return Divide_190/* : Number_30 */(v_3628/* : UnknownType */, Length_2041/* : UnknownType */(v_3628/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
}
class Numerical_139_Library
{
    static SquareRoot_3647 = function (x_3640/* : Numerical_19 */) /* : Numerical_19 */{ return Pow_163/* : Number_30 */(x_3640/* : UnknownType */, 0.5/* : Float_11 */)/* : Number_30 */; };
    static Square_3656 = function (x_3649/* : Numerical_19 */) /* : Numerical_19 */{ return Multiply_193/* : Number_30 */(x_3649/* : UnknownType */, x_3649/* : UnknownType */)/* : Number_30 */; };
    static Clamp_3667 = function (x_3658/* : Numerical_19 */, i_3660/* : Interval_26 */) /* : Numerical_19 */{ return Clamp_2020/* : Interval_26 */(i_3660/* : UnknownType */, x_3658/* : UnknownType */)/* : Interval_26 */; };
    static Clamp_3681 = function (x_3669/* : Numerical_19 */) /* : Numerical_19 */{ return Clamp_2020/* : Interval_26 */(x_3669/* : UnknownType */, Tuple_7_Primitive/* : UnknownType */(0/* : Int_10 */, 1/* : Int_10 */)/* : UnknownType */)/* : Interval_26 */; };
    static PlusOne_3693 = function (x_3683/* : Numerical_19 */) /* : Numerical_19 */{ return Add_184/* : Number_30 */(x_3683/* : UnknownType */, One_279/* : UnknownType */(x_3683/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static MinusOne_3705 = function (x_3695/* : Numerical_19 */) /* : Numerical_19 */{ return Subtract_187/* : Number_30 */(x_3695/* : UnknownType */, One_279/* : UnknownType */(x_3695/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static FromOne_3717 = function (x_3707/* : Numerical_19 */) /* : Numerical_19 */{ return Subtract_187/* : Number_30 */(One_279/* : UnknownType */(x_3707/* : UnknownType */)/* : UnknownType */, x_3707/* : UnknownType */)/* : Number_30 */; };
    static IsPositive_3726 = function (x_3719/* : Numerical_19 */) /* : Boolean_25 */{ return GreaterThanOrEquals_2203/* : Boolean_25 */(x_3719/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_25 */; };
    static GtZ_3735 = function (x_3728/* : Numerical_19 */) /* : Boolean_25 */{ return GreaterThan_2200/* : Boolean_25 */(x_3728/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_25 */; };
    static LtZ_3744 = function (x_3737/* : Numerical_19 */) /* : Boolean_25 */{ return LessThan_2194/* : Boolean_25 */(x_3737/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_25 */; };
    static GtEqZ_3753 = function (x_3746/* : Numerical_19 */) /* : Boolean_25 */{ return GreaterThanOrEquals_2203/* : Boolean_25 */(x_3746/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_25 */; };
    static LtEqZ_3762 = function (x_3755/* : Numerical_19 */) /* : Boolean_25 */{ return LessThanOrEquals_2197/* : Boolean_25 */(x_3755/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_25 */; };
    static IsNegative_3771 = function (x_3764/* : Numerical_19 */) /* : Boolean_25 */{ return LessThan_2194/* : Boolean_25 */(x_3764/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_25 */; };
    static Sign_3799 = function (x_3773/* : Numerical_19 */) /* : Numerical_19 */{ return LtZ_2077/* : UnknownType */(x_3773/* : UnknownType */)/* : UnknownType */
        ? Negative_199/* : Number_30 */(One_279/* : UnknownType */(x_3773/* : UnknownType */)/* : UnknownType */)/* : Number_30 */
        : GtZ_2074/* : UnknownType */(x_3773/* : UnknownType */)/* : UnknownType */
            ? One_279/* : Self_6 */(x_3773/* : UnknownType */)/* : Self_6 */
            : Zero_276/* : Self_6 */(x_3773/* : UnknownType */)/* : Self_6 */

    ; };
    static Abs_3812 = function (x_3801/* : Numerical_19 */) /* : Numerical_19 */{ return LtZ_2077/* : UnknownType */(x_3801/* : UnknownType */)/* : UnknownType */
        ? Negative_199/* : Number_30 */(x_3801/* : UnknownType */)/* : Number_30 */
        : x_3801/* : Numerical_19 */
    ; };
    static Half_3821 = function (x_3814/* : Numerical_19 */) /* : Numerical_19 */{ return Divide_190/* : Number_30 */(x_3814/* : UnknownType */, 2/* : Int_10 */)/* : Number_30 */; };
    static Third_3830 = function (x_3823/* : Numerical_19 */) /* : Numerical_19 */{ return Divide_190/* : Number_30 */(x_3823/* : UnknownType */, 3/* : Int_10 */)/* : Number_30 */; };
    static Quarter_3839 = function (x_3832/* : Numerical_19 */) /* : Numerical_19 */{ return Divide_190/* : Number_30 */(x_3832/* : UnknownType */, 4/* : Int_10 */)/* : Number_30 */; };
    static Fifth_3848 = function (x_3841/* : Numerical_19 */) /* : Numerical_19 */{ return Divide_190/* : Number_30 */(x_3841/* : UnknownType */, 5/* : Int_10 */)/* : Number_30 */; };
    static Sixth_3857 = function (x_3850/* : Numerical_19 */) /* : Numerical_19 */{ return Divide_190/* : Number_30 */(x_3850/* : UnknownType */, 6/* : Int_10 */)/* : Number_30 */; };
    static Seventh_3866 = function (x_3859/* : Numerical_19 */) /* : Numerical_19 */{ return Divide_190/* : Number_30 */(x_3859/* : UnknownType */, 7/* : Int_10 */)/* : Number_30 */; };
    static Eighth_3875 = function (x_3868/* : Numerical_19 */) /* : Numerical_19 */{ return Divide_190/* : Number_30 */(x_3868/* : UnknownType */, 8/* : Int_10 */)/* : Number_30 */; };
    static Ninth_3884 = function (x_3877/* : Numerical_19 */) /* : Numerical_19 */{ return Divide_190/* : Number_30 */(x_3877/* : UnknownType */, 9/* : Int_10 */)/* : Number_30 */; };
    static Tenth_3893 = function (x_3886/* : Numerical_19 */) /* : Numerical_19 */{ return Divide_190/* : Number_30 */(x_3886/* : UnknownType */, 10/* : Int_10 */)/* : Number_30 */; };
    static Sixteenth_3902 = function (x_3895/* : Numerical_19 */) /* : Numerical_19 */{ return Divide_190/* : Number_30 */(x_3895/* : UnknownType */, 16/* : Int_10 */)/* : Number_30 */; };
    static Hundredth_3911 = function (x_3904/* : Numerical_19 */) /* : Numerical_19 */{ return Divide_190/* : Number_30 */(x_3904/* : UnknownType */, 100/* : Int_10 */)/* : Number_30 */; };
    static Thousandth_3920 = function (x_3913/* : Numerical_19 */) /* : Numerical_19 */{ return Divide_190/* : Number_30 */(x_3913/* : UnknownType */, 1000/* : Int_10 */)/* : Number_30 */; };
    static Millionth_3934 = function (x_3922/* : Numerical_19 */) /* : Numerical_19 */{ return Divide_190/* : Number_30 */(x_3922/* : UnknownType */, Divide_190/* : UnknownType */(1000/* : Int_10 */, 1000/* : Int_10 */)/* : UnknownType */)/* : Number_30 */; };
    static Billionth_3953 = function (x_3936/* : Numerical_19 */) /* : Numerical_19 */{ return Divide_190/* : Number_30 */(x_3936/* : UnknownType */, Divide_190/* : UnknownType */(1000/* : Int_10 */, Divide_190/* : UnknownType */(1000/* : Int_10 */, 1000/* : Int_10 */)/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static Hundred_3962 = function (x_3955/* : Numerical_19 */) /* : Numerical_19 */{ return Multiply_193/* : Number_30 */(x_3955/* : UnknownType */, 100/* : Int_10 */)/* : Number_30 */; };
    static Thousand_3971 = function (x_3964/* : Numerical_19 */) /* : Numerical_19 */{ return Multiply_193/* : Number_30 */(x_3964/* : UnknownType */, 1000/* : Int_10 */)/* : Number_30 */; };
    static Million_3985 = function (x_3973/* : Numerical_19 */) /* : Numerical_19 */{ return Multiply_193/* : Number_30 */(x_3973/* : UnknownType */, Multiply_193/* : UnknownType */(1000/* : Int_10 */, 1000/* : Int_10 */)/* : UnknownType */)/* : Number_30 */; };
    static Billion_4004 = function (x_3987/* : Numerical_19 */) /* : Numerical_19 */{ return Multiply_193/* : Number_30 */(x_3987/* : UnknownType */, Multiply_193/* : UnknownType */(1000/* : Int_10 */, Multiply_193/* : UnknownType */(1000/* : Int_10 */, 1000/* : Int_10 */)/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static Twice_4013 = function (x_4006/* : Numerical_19 */) /* : Numerical_19 */{ return Multiply_193/* : Number_30 */(x_4006/* : UnknownType */, 2/* : Int_10 */)/* : Number_30 */; };
    static Thrice_4022 = function (x_4015/* : Numerical_19 */) /* : Numerical_19 */{ return Multiply_193/* : Number_30 */(x_4015/* : UnknownType */, 3/* : Int_10 */)/* : Number_30 */; };
    static SmoothStep_4042 = function (x_4024/* : Numerical_19 */) /* : Numerical_19 */{ return Multiply_193/* : Number_30 */(Square_2053/* : UnknownType */(x_4024/* : UnknownType */)/* : UnknownType */, Subtract_187/* : UnknownType */(3/* : Int_10 */, Twice_2149/* : UnknownType */(x_4024/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static Pow2_4051 = function (x_4044/* : Numerical_19 */) /* : Numerical_19 */{ return Multiply_193/* : Number_30 */(x_4044/* : UnknownType */, x_4044/* : UnknownType */)/* : Number_30 */; };
    static Pow3_4063 = function (x_4053/* : Numerical_19 */) /* : Numerical_19 */{ return Multiply_193/* : Number_30 */(Pow2_2158/* : UnknownType */(x_4053/* : UnknownType */)/* : UnknownType */, x_4053/* : UnknownType */)/* : Number_30 */; };
    static Pow4_4075 = function (x_4065/* : Numerical_19 */) /* : Numerical_19 */{ return Multiply_193/* : Number_30 */(Pow3_2161/* : UnknownType */(x_4065/* : UnknownType */)/* : UnknownType */, x_4065/* : UnknownType */)/* : Number_30 */; };
    static Pow5_4087 = function (x_4077/* : Numerical_19 */) /* : Numerical_19 */{ return Multiply_193/* : Number_30 */(Pow4_2164/* : UnknownType */(x_4077/* : UnknownType */)/* : UnknownType */, x_4077/* : UnknownType */)/* : Number_30 */; };
    static Pi_4089 = function () /* : Number_30 */{ return 3.1415926535897/* : Float_11 */; };
    static AlmostZero_4101 = function (x_4091/* : Numerical_19 */) /* : Boolean_25 */{ return LessThan_2194/* : Boolean_25 */(Abs_2092/* : UnknownType */(x_4091/* : UnknownType */)/* : UnknownType */, 1E-08/* : Float_11 */)/* : Boolean_25 */; };
    static Lerp_4129 = function (a_4103/* : Numerical_19 */, b_4105/* : Numerical_19 */, t_4107/* : Unit_31 */) /* : Numerical_19 */{ return Multiply_193/* : Number_30 */(Subtract_187/* : UnknownType */(1/* : Int_10 */, t_4107/* : UnknownType */)/* : UnknownType */, Add_184/* : UnknownType */(a_4103/* : UnknownType */, Multiply_193/* : UnknownType */(t_4107/* : UnknownType */, b_4105/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static Between_4170 = function (self_4131/* : Numerical_19 */, min_4133/* : Numerical_19 */, max_4135/* : Numerical_19 */) /* : Boolean_25 */{ return Zip_1927/* : Array_15 */(FieldValues_238/* : UnknownType */(self_4131/* : UnknownType */)/* : UnknownType */, FieldValues_238/* : UnknownType */(min_4133/* : UnknownType */)/* : UnknownType */, FieldValues_238/* : UnknownType */(max_4135/* : UnknownType */)/* : UnknownType */, function (x_4152/* : UnknownType */, y_4154/* : UnknownType */, z_4156/* : UnknownType */) /* : Lambda_3 */{ return Between_2179/* : UnknownType */(x_4152/* : UnknownType */, y_4154/* : UnknownType */, z_4156/* : UnknownType */)/* : UnknownType */; })/* : Array_15 */; };
}
class Angles_140_Library
{
    static Radians_4174 = function (x_4172/* : Number_30 */) /* : Angle_84 */{ return x_4172/* : Number_30 */; };
    static Degrees_4188 = function (x_4176/* : Number_30 */) /* : Angle_84 */{ return Multiply_193/* : Number_30 */(x_4176/* : UnknownType */, Divide_190/* : UnknownType */(Pi_2170/* : UnknownType */, 180/* : Int_10 */)/* : UnknownType */)/* : Number_30 */; };
    static Turns_4202 = function (x_4190/* : Number_30 */) /* : Angle_84 */{ return Multiply_193/* : Number_30 */(x_4190/* : UnknownType */, Multiply_193/* : UnknownType */(2/* : Int_10 */, Pi_2170/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
}
class Comparable_141_Library
{
    static Equals_4218 = function (a_4204/* : Comparable_21 */, b_4206/* : Comparable_21 */) /* : Boolean_25 */{ return Equals_294/* : Boolean_25 */(Compare_291/* : UnknownType */(a_4204/* : UnknownType */, b_4206/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_25 */; };
    static LessThan_4234 = function (a_4220/* : Comparable_21 */, b_4222/* : Comparable_21 */) /* : Boolean_25 */{ return LessThan_2194/* : Boolean_25 */(Compare_291/* : UnknownType */(a_4220/* : UnknownType */, b_4222/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_25 */; };
    static LessThanOrEquals_4250 = function (a_4236/* : Comparable_21 */, b_4238/* : Comparable_21 */) /* : Boolean_25 */{ return LessThanOrEquals_2197/* : Boolean_25 */(Compare_291/* : UnknownType */(a_4236/* : UnknownType */, b_4238/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_25 */; };
    static GreaterThan_4266 = function (a_4252/* : Comparable_21 */, b_4254/* : Comparable_21 */) /* : Boolean_25 */{ return GreaterThan_2200/* : Boolean_25 */(Compare_291/* : UnknownType */(a_4252/* : UnknownType */, b_4254/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_25 */; };
    static GreaterThanOrEquals_4282 = function (a_4268/* : Comparable_21 */, b_4270/* : Comparable_21 */) /* : Boolean_25 */{ return GreaterThanOrEquals_2203/* : Boolean_25 */(Compare_291/* : UnknownType */(a_4268/* : UnknownType */, b_4270/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : Boolean_25 */; };
    static Between_4305 = function (v_4284/* : Comparable_21 */, a_4286/* : Comparable_21 */, b_4288/* : Comparable_21 */) /* : Value_16 */{ return GreaterThanOrEquals_2203/* : Boolean_25 */(v_4284/* : UnknownType */, And_220/* : UnknownType */(a_4286/* : UnknownType */, LessThanOrEquals_2197/* : UnknownType */(v_4284/* : UnknownType */, b_4288/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Boolean_25 */; };
    static Between_4316 = function (v_4307/* : Value_16 */, i_4309/* : Interval_26 */) /* : Interval_26 */{ return Contains_1984/* : Boolean_25 */(i_4309/* : UnknownType */, v_4307/* : UnknownType */)/* : Boolean_25 */; };
    static Min_4330 = function (a_4318/* : Comparable_21 */, b_4320/* : Comparable_21 */) /* : Comparable_21 */{ return LessThanOrEquals_2197/* : Boolean_25 */(a_4318/* : UnknownType */, b_4320/* : UnknownType */
        ? a_4318/* : UnknownType */
        : b_4320/* : UnknownType */
    )/* : Boolean_25 */; };
    static Max_4344 = function (a_4332/* : Comparable_21 */, b_4334/* : Comparable_21 */) /* : Comparable_21 */{ return GreaterThanOrEquals_2203/* : Boolean_25 */(a_4332/* : UnknownType */, b_4334/* : UnknownType */
        ? a_4332/* : UnknownType */
        : b_4334/* : UnknownType */
    )/* : Boolean_25 */; };
}
class Equatable_142_Library
{
    static NotEquals_4358 = function (x_4346/* : Equatable_22 */, y_4348/* : Equatable_22 */) /* : Boolean_25 */{ return Not_226/* : Boolean_25 */(Equals_294/* : UnknownType */(x_4346/* : UnknownType */, y_4348/* : UnknownType */)/* : UnknownType */)/* : Boolean_25 */; };
}
class Easings_143_Library
{
    static BlendEaseFunc_4410 = function (p_4360/* : Number_30 */, easeIn_4362/* : Function_4 */, easeOut_4364/* : Function_4 */) /* : Number_30 */{ return LessThan_2194/* : Boolean_25 */(p_4360/* : UnknownType */, 0.5/* : Float_11 */
        ? Multiply_193/* : UnknownType */(0.5/* : Float_11 */, easeIn_4362/* : UnknownType */(Multiply_193/* : UnknownType */(p_4360/* : UnknownType */, 2/* : Int_10 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
        : Multiply_193/* : UnknownType */(0.5/* : Float_11 */, Add_184/* : UnknownType */(easeOut_4364/* : UnknownType */(Multiply_193/* : UnknownType */(p_4360/* : UnknownType */, Subtract_187/* : UnknownType */(2/* : Int_10 */, 1/* : Int_10 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, 0.5/* : Float_11 */)/* : UnknownType */)/* : UnknownType */
    )/* : Boolean_25 */; };
    static InvertEaseFunc_4429 = function (p_4412/* : Number_30 */, easeIn_4414/* : Function_4 */) /* : Number_30 */{ return Subtract_187/* : Number_30 */(1/* : Int_10 */, easeIn_4414/* : UnknownType */(Subtract_187/* : UnknownType */(1/* : Int_10 */, p_4412/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static Linear_4433 = function (p_4431/* : Number_30 */) /* : Number_30 */{ return p_4431/* : Number_30 */; };
    static QuadraticEaseIn_4440 = function (p_4435/* : Number_30 */) /* : Number_30 */{ return Pow2_2158/* : Numerical_19 */(p_4435/* : UnknownType */)/* : Numerical_19 */; };
    static QuadraticEaseOut_4449 = function (p_4442/* : Number_30 */) /* : Number_30 */{ return InvertEaseFunc_2224/* : Number_30 */(p_4442/* : UnknownType */, QuadraticEaseIn_2230/* : UnknownType */)/* : Number_30 */; };
    static QuadraticEaseInOut_4460 = function (p_4451/* : Number_30 */) /* : Number_30 */{ return BlendEaseFunc_2221/* : Number_30 */(p_4451/* : UnknownType */, QuadraticEaseIn_2230/* : UnknownType */, QuadraticEaseOut_2233/* : UnknownType */)/* : Number_30 */; };
    static CubicEaseIn_4467 = function (p_4462/* : Number_30 */) /* : Number_30 */{ return Pow3_2161/* : Numerical_19 */(p_4462/* : UnknownType */)/* : Numerical_19 */; };
    static CubicEaseOut_4476 = function (p_4469/* : Number_30 */) /* : Number_30 */{ return InvertEaseFunc_2224/* : Number_30 */(p_4469/* : UnknownType */, CubicEaseIn_2239/* : UnknownType */)/* : Number_30 */; };
    static CubicEaseInOut_4487 = function (p_4478/* : Number_30 */) /* : Number_30 */{ return BlendEaseFunc_2221/* : Number_30 */(p_4478/* : UnknownType */, CubicEaseIn_2239/* : UnknownType */, CubicEaseOut_2242/* : UnknownType */)/* : Number_30 */; };
    static QuarticEaseIn_4494 = function (p_4489/* : Number_30 */) /* : Number_30 */{ return Pow4_2164/* : Numerical_19 */(p_4489/* : UnknownType */)/* : Numerical_19 */; };
    static QuarticEaseOut_4503 = function (p_4496/* : Number_30 */) /* : Number_30 */{ return InvertEaseFunc_2224/* : Number_30 */(p_4496/* : UnknownType */, QuarticEaseIn_2248/* : UnknownType */)/* : Number_30 */; };
    static QuarticEaseInOut_4514 = function (p_4505/* : Number_30 */) /* : Number_30 */{ return BlendEaseFunc_2221/* : Number_30 */(p_4505/* : UnknownType */, QuarticEaseIn_2248/* : UnknownType */, QuarticEaseOut_2251/* : UnknownType */)/* : Number_30 */; };
    static QuinticEaseIn_4521 = function (p_4516/* : Number_30 */) /* : Number_30 */{ return Pow5_2167/* : Numerical_19 */(p_4516/* : UnknownType */)/* : Numerical_19 */; };
    static QuinticEaseOut_4530 = function (p_4523/* : Number_30 */) /* : Number_30 */{ return InvertEaseFunc_2224/* : Number_30 */(p_4523/* : UnknownType */, QuinticEaseIn_2257/* : UnknownType */)/* : Number_30 */; };
    static QuinticEaseInOut_4541 = function (p_4532/* : Number_30 */) /* : Number_30 */{ return BlendEaseFunc_2221/* : Number_30 */(p_4532/* : UnknownType */, QuinticEaseIn_2257/* : UnknownType */, QuinticEaseOut_2260/* : UnknownType */)/* : Number_30 */; };
    static SineEaseIn_4550 = function (p_4543/* : Number_30 */) /* : Number_30 */{ return InvertEaseFunc_2224/* : Number_30 */(p_4543/* : UnknownType */, SineEaseOut_2269/* : UnknownType */)/* : Number_30 */; };
    static SineEaseOut_4563 = function (p_4552/* : Number_30 */) /* : Number_30 */{ return Sin_148/* : Number_30 */(Turns_2188/* : UnknownType */(Quarter_2101/* : UnknownType */(p_4552/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static SineEaseInOut_4574 = function (p_4565/* : Number_30 */) /* : Number_30 */{ return BlendEaseFunc_2221/* : Number_30 */(p_4565/* : UnknownType */, SineEaseIn_2266/* : UnknownType */, SineEaseOut_2269/* : UnknownType */)/* : Number_30 */; };
    static CircularEaseIn_4590 = function (p_4576/* : Number_30 */) /* : Number_30 */{ return FromOne_2068/* : Numerical_19 */(SquareRoot_2050/* : UnknownType */(FromOne_2068/* : UnknownType */(Pow2_2158/* : UnknownType */(p_4576/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Numerical_19 */; };
    static CircularEaseOut_4599 = function (p_4592/* : Number_30 */) /* : Number_30 */{ return InvertEaseFunc_2224/* : Number_30 */(p_4592/* : UnknownType */, CircularEaseIn_2275/* : UnknownType */)/* : Number_30 */; };
    static CircularEaseInOut_4610 = function (p_4601/* : Number_30 */) /* : Number_30 */{ return BlendEaseFunc_2221/* : Number_30 */(p_4601/* : UnknownType */, CircularEaseIn_2275/* : UnknownType */, CircularEaseOut_2278/* : UnknownType */)/* : Number_30 */; };
    static ExponentialEaseIn_4633 = function (p_4612/* : Number_30 */) /* : Number_30 */{ return AlmostZero_2173/* : UnknownType */(p_4612/* : UnknownType */)/* : UnknownType */
        ? p_4612/* : Number_30 */
        : Pow_163/* : Number_30 */(2/* : Int_10 */, Multiply_193/* : UnknownType */(10/* : Int_10 */, MinusOne_2065/* : UnknownType */(p_4612/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_30 */
    ; };
    static ExponentialEaseOut_4642 = function (p_4635/* : Number_30 */) /* : Number_30 */{ return InvertEaseFunc_2224/* : Number_30 */(p_4635/* : UnknownType */, ExponentialEaseIn_2284/* : UnknownType */)/* : Number_30 */; };
    static ExponentialEaseInOut_4653 = function (p_4644/* : Number_30 */) /* : Number_30 */{ return BlendEaseFunc_2221/* : Number_30 */(p_4644/* : UnknownType */, ExponentialEaseIn_2284/* : UnknownType */, ExponentialEaseOut_2287/* : UnknownType */)/* : Number_30 */; };
    static ElasticEaseIn_4692 = function (p_4655/* : Number_30 */) /* : Number_30 */{ return Multiply_193/* : Number_30 */(13/* : Int_10 */, Multiply_193/* : UnknownType */(Turns_2188/* : UnknownType */(Quarter_2101/* : UnknownType */(p_4655/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Sin_148/* : UnknownType */(Radians_1242/* : UnknownType */(Pow_163/* : UnknownType */(2/* : Int_10 */, Multiply_193/* : UnknownType */(10/* : Int_10 */, MinusOne_2065/* : UnknownType */(p_4655/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static ElasticEaseOut_4701 = function (p_4694/* : Number_30 */) /* : Number_30 */{ return InvertEaseFunc_2224/* : Number_30 */(p_4694/* : UnknownType */, ElasticEaseIn_2293/* : UnknownType */)/* : Number_30 */; };
    static ElasticEaseInOut_4712 = function (p_4703/* : Number_30 */) /* : Number_30 */{ return BlendEaseFunc_2221/* : Number_30 */(p_4703/* : UnknownType */, ElasticEaseIn_2293/* : UnknownType */, ElasticEaseOut_2296/* : UnknownType */)/* : Number_30 */; };
    static BackEaseIn_4738 = function (p_4714/* : Number_30 */) /* : Number_30 */{ return Subtract_187/* : Number_30 */(Pow3_2161/* : UnknownType */(p_4714/* : UnknownType */)/* : UnknownType */, Multiply_193/* : UnknownType */(p_4714/* : UnknownType */, Sin_148/* : UnknownType */(Turns_2188/* : UnknownType */(Half_2095/* : UnknownType */(p_4714/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static BackEaseOut_4747 = function (p_4740/* : Number_30 */) /* : Number_30 */{ return InvertEaseFunc_2224/* : Number_30 */(p_4740/* : UnknownType */, BackEaseIn_2302/* : UnknownType */)/* : Number_30 */; };
    static BackEaseInOut_4758 = function (p_4749/* : Number_30 */) /* : Number_30 */{ return BlendEaseFunc_2221/* : Number_30 */(p_4749/* : UnknownType */, BackEaseIn_2302/* : UnknownType */, BackEaseOut_2305/* : UnknownType */)/* : Number_30 */; };
    static BounceEaseIn_4767 = function (p_4760/* : Number_30 */) /* : Number_30 */{ return InvertEaseFunc_2224/* : Number_30 */(p_4760/* : UnknownType */, BounceEaseOut_2314/* : UnknownType */)/* : Number_30 */; };
    static BounceEaseOut_4937 = function (p_4769/* : Number_30 */) /* : Number_30 */{ return LessThan_2194/* : UnknownType */(p_4769/* : UnknownType */, Divide_190/* : UnknownType */(4/* : Int_10 */, 11/* : Float_11 */)/* : UnknownType */)/* : UnknownType */
        ? Multiply_193/* : Number_30 */(121/* : Float_11 */, Divide_190/* : UnknownType */(Pow2_2158/* : UnknownType */(p_4769/* : UnknownType */)/* : UnknownType */, 16/* : Float_11 */)/* : UnknownType */)/* : Number_30 */
        : LessThan_2194/* : UnknownType */(p_4769/* : UnknownType */, Divide_190/* : UnknownType */(8/* : Int_10 */, 11/* : Float_11 */)/* : UnknownType */)/* : UnknownType */
            ? Divide_190/* : Number_30 */(363/* : Float_11 */, Multiply_193/* : UnknownType */(40/* : Float_11 */, Subtract_187/* : UnknownType */(Pow2_2158/* : UnknownType */(p_4769/* : UnknownType */)/* : UnknownType */, Divide_190/* : UnknownType */(99/* : Float_11 */, Multiply_193/* : UnknownType */(10/* : Float_11 */, Add_184/* : UnknownType */(p_4769/* : UnknownType */, Divide_190/* : UnknownType */(17/* : Float_11 */, 5/* : Float_11 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_30 */
            : LessThan_2194/* : UnknownType */(p_4769/* : UnknownType */, Divide_190/* : UnknownType */(9/* : Int_10 */, 10/* : Float_11 */)/* : UnknownType */)/* : UnknownType */
                ? Divide_190/* : Number_30 */(4356/* : Float_11 */, Multiply_193/* : UnknownType */(361/* : Float_11 */, Subtract_187/* : UnknownType */(Pow2_2158/* : UnknownType */(p_4769/* : UnknownType */)/* : UnknownType */, Divide_190/* : UnknownType */(35442/* : Float_11 */, Multiply_193/* : UnknownType */(1805/* : Float_11 */, Add_184/* : UnknownType */(p_4769/* : UnknownType */, Divide_190/* : UnknownType */(16061/* : Float_11 */, 1805/* : Float_11 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_30 */
                : Divide_190/* : Number_30 */(54/* : Float_11 */, Multiply_193/* : UnknownType */(5/* : Float_11 */, Subtract_187/* : UnknownType */(Pow2_2158/* : UnknownType */(p_4769/* : UnknownType */)/* : UnknownType */, Divide_190/* : UnknownType */(513/* : Float_11 */, Multiply_193/* : UnknownType */(25/* : Float_11 */, Add_184/* : UnknownType */(p_4769/* : UnknownType */, Divide_190/* : UnknownType */(268/* : Float_11 */, 25/* : Float_11 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_30 */


    ; };
    static BounceEaseInOut_4948 = function (p_4939/* : Number_30 */) /* : Number_30 */{ return BlendEaseFunc_2221/* : Number_30 */(p_4939/* : UnknownType */, BounceEaseIn_2311/* : UnknownType */, BounceEaseOut_2314/* : UnknownType */)/* : Number_30 */; };
}
class Any_14_Concept
{
    constructor(self) { this.Self = self; };
    static FieldNames_2440 = function () /* : Array_15 */{ return null; };
    static FieldValues_2443 = function (x_2442/* : Self_6 */) /* : Array_15 */{ return null; };
    static TypeOf_2444 = function () /* : Type_12 */{ return null; };
}
class Array_15_Concept
{
    constructor(self) { this.Self = self; };
    static Count_2447 = function (xs_2446/* : Self_6 */) /* : Count_28 */{ return null; };
    static At_2452 = function (xs_2449/* : Self_6 */, n_2451/* : Index_29 */) /* : T_251 */{ return null; };
}
class Value_16_Concept
{
    constructor(self) { this.Self = self; };
    static Default_2460 = function () /* : Self_6 */{ return Default_259/* : Self_6 */(FieldValues_238/* : UnknownType */(Self_6_Primitive/* : UnknownType */)/* : UnknownType */)/* : Self_6 */; };
}
class Vector_17_Concept
{
    constructor(self) { this.Self = self; };
    static Count_2470 = function (v_2462/* : Vector_17 */) /* : Count_28 */{ return Count_28_Type/* : Count_28 */(FieldTypes_230/* : UnknownType */(Self_6_Primitive/* : UnknownType */)/* : UnknownType */)/* : Count_28 */; };
    static At_2484 = function (v_2472/* : Vector_17 */, n_2474/* : Index_29 */) /* : T_261 */{ return At_256/* : T_251 */(FieldValues_238/* : UnknownType */(v_2472/* : UnknownType */)/* : UnknownType */, n_2474/* : UnknownType */)/* : T_251 */; };
}
class Measure_18_Concept
{
    constructor(self) { this.Self = self; };
    static Value_2496 = function (x_2486/* : Self_6 */) /* : Number_30 */{ return At_256/* : T_251 */(FieldValues_238/* : UnknownType */(x_2486/* : UnknownType */)/* : UnknownType */, 0/* : Int_10 */)/* : T_251 */; };
}
class Numerical_19_Concept
{
    constructor(self) { this.Self = self; };
    static FieldTypes_2497 = function () /* : Array_15 */{ return null; };
    static Zero_2505 = function () /* : Self_6 */{ return Zero_276/* : Self_6 */(FieldTypes_230/* : UnknownType */(Self_6_Primitive/* : UnknownType */)/* : UnknownType */)/* : Self_6 */; };
    static One_2513 = function () /* : Self_6 */{ return One_279/* : Self_6 */(FieldTypes_230/* : UnknownType */(Self_6_Primitive/* : UnknownType */)/* : UnknownType */)/* : Self_6 */; };
    static MinValue_2521 = function () /* : Self_6 */{ return MinValue_282/* : Self_6 */(FieldTypes_230/* : UnknownType */(Self_6_Primitive/* : UnknownType */)/* : UnknownType */)/* : Self_6 */; };
    static MaxValue_2529 = function () /* : Self_6 */{ return MaxValue_285/* : Self_6 */(FieldTypes_230/* : UnknownType */(Self_6_Primitive/* : UnknownType */)/* : UnknownType */)/* : Self_6 */; };
}
class Magnitudinal_20_Concept
{
    constructor(self) { this.Self = self; };
    static Magnitude_2545 = function (x_2531/* : Self_6 */) /* : Number_30 */{ return SquareRoot_2050/* : Numerical_19 */(Sum_2032/* : UnknownType */(Square_2053/* : UnknownType */(FieldValues_238/* : UnknownType */(x_2531/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Numerical_19 */; };
}
class Comparable_21_Concept
{
    constructor(self) { this.Self = self; };
    static Compare_2548 = function (x_2547/* : Self_6 */) /* : Integer_27 */{ return null; };
}
class Equatable_22_Concept
{
    constructor(self) { this.Self = self; };
    static Equals_2568 = function (a_2550/* : Self_6 */, b_2552/* : Self_6 */) /* : Boolean_25 */{ return All_1960/* : Boolean_25 */(Equals_294/* : UnknownType */(FieldValues_238/* : UnknownType */(a_2550/* : UnknownType */)/* : UnknownType */, FieldValues_238/* : UnknownType */(b_2552/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Boolean_25 */; };
}
class Arithmetic_23_Concept
{
    constructor(self) { this.Self = self; };
    static Add_2585 = function (self_2570/* : Self_6 */, other_2572/* : Self_6 */) /* : Self_6 */{ return Add_184/* : Number_30 */(FieldValues_238/* : UnknownType */(self_2570/* : UnknownType */)/* : UnknownType */, FieldValues_238/* : UnknownType */(other_2572/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static Negative_2595 = function (self_2587/* : Self_6 */) /* : Self_6 */{ return Negative_199/* : Number_30 */(FieldValues_238/* : UnknownType */(self_2587/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static Reciprocal_2605 = function (self_2597/* : Self_6 */) /* : Self_6 */{ return Reciprocal_303/* : Self_6 */(FieldValues_238/* : UnknownType */(self_2597/* : UnknownType */)/* : UnknownType */)/* : Self_6 */; };
    static Multiply_2622 = function (self_2607/* : Self_6 */, other_2609/* : Self_6 */) /* : Self_6 */{ return Add_184/* : Number_30 */(FieldValues_238/* : UnknownType */(self_2607/* : UnknownType */)/* : UnknownType */, FieldValues_238/* : UnknownType */(other_2609/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static Divide_2639 = function (self_2624/* : Self_6 */, other_2626/* : Self_6 */) /* : Self_6 */{ return Divide_190/* : Number_30 */(FieldValues_238/* : UnknownType */(self_2624/* : UnknownType */)/* : UnknownType */, FieldValues_238/* : UnknownType */(other_2626/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static Modulo_2656 = function (self_2641/* : Self_6 */, other_2643/* : Self_6 */) /* : Self_6 */{ return Modulo_196/* : Number_30 */(FieldValues_238/* : UnknownType */(self_2641/* : UnknownType */)/* : UnknownType */, FieldValues_238/* : UnknownType */(other_2643/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
}
class ScalarArithmetic_24_Concept
{
    constructor(self) { this.Self = self; };
    static Add_2670 = function (self_2658/* : Self_6 */, scalar_2660/* : Number_30 */) /* : Self_6 */{ return Add_184/* : Number_30 */(FieldValues_238/* : UnknownType */(self_2658/* : UnknownType */)/* : UnknownType */, scalar_2660/* : UnknownType */)/* : Number_30 */; };
    static Subtract_2684 = function (self_2672/* : Self_6 */, scalar_2674/* : Number_30 */) /* : Self_6 */{ return Add_184/* : Number_30 */(self_2672/* : UnknownType */, Negative_199/* : UnknownType */(scalar_2674/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static Multiply_2698 = function (self_2686/* : Self_6 */, scalar_2688/* : Number_30 */) /* : Self_6 */{ return Multiply_193/* : Number_30 */(FieldValues_238/* : UnknownType */(self_2686/* : UnknownType */)/* : UnknownType */, scalar_2688/* : UnknownType */)/* : Number_30 */; };
    static Divide_2712 = function (self_2700/* : Self_6 */, scalar_2702/* : Number_30 */) /* : Self_6 */{ return Multiply_193/* : Number_30 */(self_2700/* : UnknownType */, Reciprocal_303/* : UnknownType */(scalar_2702/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static Modulo_2726 = function (self_2714/* : Self_6 */, scalar_2716/* : Number_30 */) /* : Self_6 */{ return Modulo_196/* : Number_30 */(FieldValues_238/* : UnknownType */(self_2714/* : UnknownType */)/* : UnknownType */, scalar_2716/* : UnknownType */)/* : Number_30 */; };
}
class Boolean_25_Concept
{
    constructor(self) { this.Self = self; };
    static And_2743 = function (a_2728/* : Self_6 */, b_2730/* : Self_6 */) /* : Self_6 */{ return And_220/* : Boolean_25 */(FieldValues_238/* : UnknownType */(a_2728/* : UnknownType */)/* : UnknownType */, FieldValues_238/* : UnknownType */(b_2730/* : UnknownType */)/* : UnknownType */)/* : Boolean_25 */; };
    static Or_2760 = function (a_2745/* : Self_6 */, b_2747/* : Self_6 */) /* : Self_6 */{ return Or_223/* : Boolean_25 */(FieldValues_238/* : UnknownType */(a_2745/* : UnknownType */)/* : UnknownType */, FieldValues_238/* : UnknownType */(b_2747/* : UnknownType */)/* : UnknownType */)/* : Boolean_25 */; };
    static Not_2770 = function (a_2762/* : Self_6 */) /* : Self_6 */{ return Not_226/* : Boolean_25 */(FieldValues_238/* : UnknownType */(a_2762/* : UnknownType */)/* : UnknownType */)/* : Boolean_25 */; };
}
class Interval_26_Concept
{
    constructor(self) { this.Self = self; };
    static Min_2773 = function (x_2772/* : Self_6 */) /* : T_338 */{ return null; };
    static Max_2776 = function (x_2775/* : Self_6 */) /* : T_338 */{ return null; };
}
class Integer_27_Type
{
    constructor(Value_346)
    {
        // field initialization 
        this.Value_346 = Value_346;
        this.Default_2460 = Integer_27_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = Integer_27_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = Integer_27_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = Integer_27_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = Integer_27_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = Integer_27_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = Integer_27_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = Integer_27_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Integer_27_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Integer_27_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = Integer_27_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Integer_27_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Integer_27_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Integer_27_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Integer_27_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = Integer_27_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = Integer_27_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = Integer_27_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = Integer_27_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = Integer_27_Type.Numerical_19_Concept.MaxValue_2529;
    }
    // field accessors
    static Value_346 = function(self) { return self.Value_346; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Integer_27_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(Integer_27_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Integer_27_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Integer_27_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Integer_27_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Integer_27_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(Integer_27_Type);
    static Implements = [Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept];
}
class Count_28_Type
{
    constructor(Value_353)
    {
        // field initialization 
        this.Value_353 = Value_353;
        this.Default_2460 = Count_28_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = Count_28_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = Count_28_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = Count_28_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = Count_28_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = Count_28_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = Count_28_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = Count_28_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Count_28_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Count_28_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = Count_28_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Count_28_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Count_28_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Count_28_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Count_28_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = Count_28_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = Count_28_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = Count_28_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = Count_28_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = Count_28_Type.Numerical_19_Concept.MaxValue_2529;
    }
    // field accessors
    static Value_353 = function(self) { return self.Value_353; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Count_28_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(Count_28_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Count_28_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Count_28_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Count_28_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Count_28_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(Count_28_Type);
    static Implements = [Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept];
}
class Index_29_Type
{
    constructor(Value_360)
    {
        // field initialization 
        this.Value_360 = Value_360;
        this.Default_2460 = Index_29_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Value_360 = function(self) { return self.Value_360; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Index_29_Type);
    static Implements = [Value_16_Concept];
}
class Number_30_Type
{
    constructor(Value_367)
    {
        // field initialization 
        this.Value_367 = Value_367;
        this.Default_2460 = Number_30_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = Number_30_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = Number_30_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = Number_30_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = Number_30_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = Number_30_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = Number_30_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = Number_30_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Number_30_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Number_30_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = Number_30_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Number_30_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Number_30_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Number_30_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Number_30_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = Number_30_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = Number_30_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = Number_30_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = Number_30_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = Number_30_Type.Numerical_19_Concept.MaxValue_2529;
    }
    // field accessors
    static Value_367 = function(self) { return self.Value_367; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Number_30_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(Number_30_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Number_30_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Number_30_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Number_30_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Number_30_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(Number_30_Type);
    static Implements = [Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept];
}
class Unit_31_Type
{
    constructor(Value_374)
    {
        // field initialization 
        this.Value_374 = Value_374;
        this.Default_2460 = Unit_31_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = Unit_31_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = Unit_31_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = Unit_31_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = Unit_31_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = Unit_31_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = Unit_31_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = Unit_31_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Unit_31_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Unit_31_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = Unit_31_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Unit_31_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Unit_31_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Unit_31_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Unit_31_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = Unit_31_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = Unit_31_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = Unit_31_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = Unit_31_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = Unit_31_Type.Numerical_19_Concept.MaxValue_2529;
    }
    // field accessors
    static Value_374 = function(self) { return self.Value_374; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Unit_31_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(Unit_31_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Unit_31_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Unit_31_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Unit_31_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Unit_31_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(Unit_31_Type);
    static Implements = [Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept];
}
class Percent_32_Type
{
    constructor(Value_381)
    {
        // field initialization 
        this.Value_381 = Value_381;
        this.Default_2460 = Percent_32_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = Percent_32_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = Percent_32_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = Percent_32_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = Percent_32_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = Percent_32_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = Percent_32_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = Percent_32_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Percent_32_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Percent_32_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = Percent_32_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Percent_32_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Percent_32_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Percent_32_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Percent_32_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = Percent_32_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = Percent_32_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = Percent_32_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = Percent_32_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = Percent_32_Type.Numerical_19_Concept.MaxValue_2529;
    }
    // field accessors
    static Value_381 = function(self) { return self.Value_381; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Percent_32_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(Percent_32_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Percent_32_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Percent_32_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Percent_32_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Percent_32_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(Percent_32_Type);
    static Implements = [Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept];
}
class Quaternion_33_Type
{
    constructor(X_388, Y_395, Z_402, W_409)
    {
        // field initialization 
        this.X_388 = X_388;
        this.Y_395 = Y_395;
        this.Z_402 = Z_402;
        this.W_409 = W_409;
        this.Default_2460 = Quaternion_33_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static X_388 = function(self) { return self.X_388; }
    static Y_395 = function(self) { return self.Y_395; }
    static Z_402 = function(self) { return self.Z_402; }
    static W_409 = function(self) { return self.W_409; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Quaternion_33_Type);
    static Implements = [Value_16_Concept];
}
class Unit2D_34_Type
{
    constructor(X_416, Y_423)
    {
        // field initialization 
        this.X_416 = X_416;
        this.Y_423 = Y_423;
        this.Default_2460 = Unit2D_34_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static X_416 = function(self) { return self.X_416; }
    static Y_423 = function(self) { return self.Y_423; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Unit2D_34_Type);
    static Implements = [Value_16_Concept];
}
class Unit3D_35_Type
{
    constructor(X_430, Y_437, Z_444)
    {
        // field initialization 
        this.X_430 = X_430;
        this.Y_437 = Y_437;
        this.Z_444 = Z_444;
        this.Default_2460 = Unit3D_35_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static X_430 = function(self) { return self.X_430; }
    static Y_437 = function(self) { return self.Y_437; }
    static Z_444 = function(self) { return self.Z_444; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Unit3D_35_Type);
    static Implements = [Value_16_Concept];
}
class Direction3D_36_Type
{
    constructor(Value_451)
    {
        // field initialization 
        this.Value_451 = Value_451;
        this.Default_2460 = Direction3D_36_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Value_451 = function(self) { return self.Value_451; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Direction3D_36_Type);
    static Implements = [Value_16_Concept];
}
class AxisAngle_37_Type
{
    constructor(Axis_458, Angle_465)
    {
        // field initialization 
        this.Axis_458 = Axis_458;
        this.Angle_465 = Angle_465;
        this.Default_2460 = AxisAngle_37_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Axis_458 = function(self) { return self.Axis_458; }
    static Angle_465 = function(self) { return self.Angle_465; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(AxisAngle_37_Type);
    static Implements = [Value_16_Concept];
}
class EulerAngles_38_Type
{
    constructor(Yaw_472, Pitch_479, Roll_486)
    {
        // field initialization 
        this.Yaw_472 = Yaw_472;
        this.Pitch_479 = Pitch_479;
        this.Roll_486 = Roll_486;
        this.Default_2460 = EulerAngles_38_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Yaw_472 = function(self) { return self.Yaw_472; }
    static Pitch_479 = function(self) { return self.Pitch_479; }
    static Roll_486 = function(self) { return self.Roll_486; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(EulerAngles_38_Type);
    static Implements = [Value_16_Concept];
}
class Rotation3D_39_Type
{
    constructor(Quaternion_493)
    {
        // field initialization 
        this.Quaternion_493 = Quaternion_493;
        this.Default_2460 = Rotation3D_39_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Quaternion_493 = function(self) { return self.Quaternion_493; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Rotation3D_39_Type);
    static Implements = [Value_16_Concept];
}
class Vector2D_40_Type
{
    constructor(X_500, Y_507)
    {
        // field initialization 
        this.X_500 = X_500;
        this.Y_507 = Y_507;
        this.Count_2447 = Vector2D_40_Type.Array_15_Concept.Count_2447;
        this.At_2452 = Vector2D_40_Type.Array_15_Concept.At_2452;
        this.Default_2460 = Vector2D_40_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = Vector2D_40_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = Vector2D_40_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = Vector2D_40_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = Vector2D_40_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = Vector2D_40_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = Vector2D_40_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = Vector2D_40_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Vector2D_40_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Vector2D_40_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = Vector2D_40_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Vector2D_40_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Vector2D_40_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Vector2D_40_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Vector2D_40_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = Vector2D_40_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = Vector2D_40_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = Vector2D_40_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = Vector2D_40_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = Vector2D_40_Type.Numerical_19_Concept.MaxValue_2529;
        this.Count_2470 = Vector2D_40_Type.Vector_17_Concept.Count_2470;
        this.At_2484 = Vector2D_40_Type.Vector_17_Concept.At_2484;
    }
    // field accessors
    static X_500 = function(self) { return self.X_500; }
    static Y_507 = function(self) { return self.Y_507; }
    // implemented concepts 
    static Array_15_Concept = new Array_15_Concept(Vector2D_40_Type);
    static Value_16_Concept = new Value_16_Concept(Vector2D_40_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(Vector2D_40_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Vector2D_40_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Vector2D_40_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Vector2D_40_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Vector2D_40_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(Vector2D_40_Type);
    static Vector_17_Concept = new Vector_17_Concept(Vector2D_40_Type);
    static Implements = [Array_15_Concept,Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept,Vector_17_Concept];
}
class Vector3D_41_Type
{
    constructor(X_514, Y_521, Z_528)
    {
        // field initialization 
        this.X_514 = X_514;
        this.Y_521 = Y_521;
        this.Z_528 = Z_528;
        this.Count_2447 = Vector3D_41_Type.Array_15_Concept.Count_2447;
        this.At_2452 = Vector3D_41_Type.Array_15_Concept.At_2452;
        this.Default_2460 = Vector3D_41_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = Vector3D_41_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = Vector3D_41_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = Vector3D_41_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = Vector3D_41_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = Vector3D_41_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = Vector3D_41_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = Vector3D_41_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Vector3D_41_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Vector3D_41_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = Vector3D_41_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Vector3D_41_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Vector3D_41_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Vector3D_41_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Vector3D_41_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = Vector3D_41_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = Vector3D_41_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = Vector3D_41_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = Vector3D_41_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = Vector3D_41_Type.Numerical_19_Concept.MaxValue_2529;
        this.Count_2470 = Vector3D_41_Type.Vector_17_Concept.Count_2470;
        this.At_2484 = Vector3D_41_Type.Vector_17_Concept.At_2484;
    }
    // field accessors
    static X_514 = function(self) { return self.X_514; }
    static Y_521 = function(self) { return self.Y_521; }
    static Z_528 = function(self) { return self.Z_528; }
    // implemented concepts 
    static Array_15_Concept = new Array_15_Concept(Vector3D_41_Type);
    static Value_16_Concept = new Value_16_Concept(Vector3D_41_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(Vector3D_41_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Vector3D_41_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Vector3D_41_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Vector3D_41_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Vector3D_41_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(Vector3D_41_Type);
    static Vector_17_Concept = new Vector_17_Concept(Vector3D_41_Type);
    static Implements = [Array_15_Concept,Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept,Vector_17_Concept];
}
class Vector4D_42_Type
{
    constructor(X_535, Y_542, Z_549, W_556)
    {
        // field initialization 
        this.X_535 = X_535;
        this.Y_542 = Y_542;
        this.Z_549 = Z_549;
        this.W_556 = W_556;
        this.Count_2447 = Vector4D_42_Type.Array_15_Concept.Count_2447;
        this.At_2452 = Vector4D_42_Type.Array_15_Concept.At_2452;
        this.Default_2460 = Vector4D_42_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = Vector4D_42_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = Vector4D_42_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = Vector4D_42_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = Vector4D_42_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = Vector4D_42_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = Vector4D_42_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = Vector4D_42_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Vector4D_42_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Vector4D_42_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = Vector4D_42_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Vector4D_42_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Vector4D_42_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Vector4D_42_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Vector4D_42_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = Vector4D_42_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = Vector4D_42_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = Vector4D_42_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = Vector4D_42_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = Vector4D_42_Type.Numerical_19_Concept.MaxValue_2529;
        this.Count_2470 = Vector4D_42_Type.Vector_17_Concept.Count_2470;
        this.At_2484 = Vector4D_42_Type.Vector_17_Concept.At_2484;
    }
    // field accessors
    static X_535 = function(self) { return self.X_535; }
    static Y_542 = function(self) { return self.Y_542; }
    static Z_549 = function(self) { return self.Z_549; }
    static W_556 = function(self) { return self.W_556; }
    // implemented concepts 
    static Array_15_Concept = new Array_15_Concept(Vector4D_42_Type);
    static Value_16_Concept = new Value_16_Concept(Vector4D_42_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(Vector4D_42_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Vector4D_42_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Vector4D_42_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Vector4D_42_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Vector4D_42_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(Vector4D_42_Type);
    static Vector_17_Concept = new Vector_17_Concept(Vector4D_42_Type);
    static Implements = [Array_15_Concept,Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept,Vector_17_Concept];
}
class Orientation3D_43_Type
{
    constructor(Value_563)
    {
        // field initialization 
        this.Value_563 = Value_563;
        this.Default_2460 = Orientation3D_43_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Value_563 = function(self) { return self.Value_563; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Orientation3D_43_Type);
    static Implements = [Value_16_Concept];
}
class Pose2D_44_Type
{
    constructor(Position_570, Orientation_577)
    {
        // field initialization 
        this.Position_570 = Position_570;
        this.Orientation_577 = Orientation_577;
        this.Default_2460 = Pose2D_44_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Position_570 = function(self) { return self.Position_570; }
    static Orientation_577 = function(self) { return self.Orientation_577; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Pose2D_44_Type);
    static Implements = [Value_16_Concept];
}
class Pose3D_45_Type
{
    constructor(Position_584, Orientation_591)
    {
        // field initialization 
        this.Position_584 = Position_584;
        this.Orientation_591 = Orientation_591;
        this.Default_2460 = Pose3D_45_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Position_584 = function(self) { return self.Position_584; }
    static Orientation_591 = function(self) { return self.Orientation_591; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Pose3D_45_Type);
    static Implements = [Value_16_Concept];
}
class Transform3D_46_Type
{
    constructor(Translation_598, Rotation_605, Scale_612)
    {
        // field initialization 
        this.Translation_598 = Translation_598;
        this.Rotation_605 = Rotation_605;
        this.Scale_612 = Scale_612;
        this.Default_2460 = Transform3D_46_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Translation_598 = function(self) { return self.Translation_598; }
    static Rotation_605 = function(self) { return self.Rotation_605; }
    static Scale_612 = function(self) { return self.Scale_612; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Transform3D_46_Type);
    static Implements = [Value_16_Concept];
}
class Transform2D_47_Type
{
    constructor(Translation_619, Rotation_626, Scale_633)
    {
        // field initialization 
        this.Translation_619 = Translation_619;
        this.Rotation_626 = Rotation_626;
        this.Scale_633 = Scale_633;
        this.Default_2460 = Transform2D_47_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Translation_619 = function(self) { return self.Translation_619; }
    static Rotation_626 = function(self) { return self.Rotation_626; }
    static Scale_633 = function(self) { return self.Scale_633; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Transform2D_47_Type);
    static Implements = [Value_16_Concept];
}
class AlignedBox2D_48_Type
{
    constructor(A_640, B_647)
    {
        // field initialization 
        this.A_640 = A_640;
        this.B_647 = B_647;
        this.Count_2447 = AlignedBox2D_48_Type.Array_15_Concept.Count_2447;
        this.At_2452 = AlignedBox2D_48_Type.Array_15_Concept.At_2452;
        this.Default_2460 = AlignedBox2D_48_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = AlignedBox2D_48_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = AlignedBox2D_48_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = AlignedBox2D_48_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = AlignedBox2D_48_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = AlignedBox2D_48_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = AlignedBox2D_48_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = AlignedBox2D_48_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = AlignedBox2D_48_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = AlignedBox2D_48_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = AlignedBox2D_48_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = AlignedBox2D_48_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = AlignedBox2D_48_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = AlignedBox2D_48_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = AlignedBox2D_48_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = AlignedBox2D_48_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = AlignedBox2D_48_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = AlignedBox2D_48_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = AlignedBox2D_48_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = AlignedBox2D_48_Type.Numerical_19_Concept.MaxValue_2529;
        this.Count_2470 = AlignedBox2D_48_Type.Vector_17_Concept.Count_2470;
        this.At_2484 = AlignedBox2D_48_Type.Vector_17_Concept.At_2484;
        this.Min_2773 = AlignedBox2D_48_Type.Interval_26_Concept.Min_2773;
        this.Max_2776 = AlignedBox2D_48_Type.Interval_26_Concept.Max_2776;
    }
    // field accessors
    static A_640 = function(self) { return self.A_640; }
    static B_647 = function(self) { return self.B_647; }
    // implemented concepts 
    static Array_15_Concept = new Array_15_Concept(AlignedBox2D_48_Type);
    static Value_16_Concept = new Value_16_Concept(AlignedBox2D_48_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(AlignedBox2D_48_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(AlignedBox2D_48_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(AlignedBox2D_48_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(AlignedBox2D_48_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(AlignedBox2D_48_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(AlignedBox2D_48_Type);
    static Vector_17_Concept = new Vector_17_Concept(AlignedBox2D_48_Type);
    static Interval_26_Concept = new Interval_26_Concept(AlignedBox2D_48_Type);
    static Implements = [Array_15_Concept,Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept,Vector_17_Concept,Interval_26_Concept];
}
class AlignedBox3D_49_Type
{
    constructor(A_654, B_661)
    {
        // field initialization 
        this.A_654 = A_654;
        this.B_661 = B_661;
        this.Count_2447 = AlignedBox3D_49_Type.Array_15_Concept.Count_2447;
        this.At_2452 = AlignedBox3D_49_Type.Array_15_Concept.At_2452;
        this.Default_2460 = AlignedBox3D_49_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = AlignedBox3D_49_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = AlignedBox3D_49_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = AlignedBox3D_49_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = AlignedBox3D_49_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = AlignedBox3D_49_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = AlignedBox3D_49_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = AlignedBox3D_49_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = AlignedBox3D_49_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = AlignedBox3D_49_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = AlignedBox3D_49_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = AlignedBox3D_49_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = AlignedBox3D_49_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = AlignedBox3D_49_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = AlignedBox3D_49_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = AlignedBox3D_49_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = AlignedBox3D_49_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = AlignedBox3D_49_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = AlignedBox3D_49_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = AlignedBox3D_49_Type.Numerical_19_Concept.MaxValue_2529;
        this.Count_2470 = AlignedBox3D_49_Type.Vector_17_Concept.Count_2470;
        this.At_2484 = AlignedBox3D_49_Type.Vector_17_Concept.At_2484;
        this.Min_2773 = AlignedBox3D_49_Type.Interval_26_Concept.Min_2773;
        this.Max_2776 = AlignedBox3D_49_Type.Interval_26_Concept.Max_2776;
    }
    // field accessors
    static A_654 = function(self) { return self.A_654; }
    static B_661 = function(self) { return self.B_661; }
    // implemented concepts 
    static Array_15_Concept = new Array_15_Concept(AlignedBox3D_49_Type);
    static Value_16_Concept = new Value_16_Concept(AlignedBox3D_49_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(AlignedBox3D_49_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(AlignedBox3D_49_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(AlignedBox3D_49_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(AlignedBox3D_49_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(AlignedBox3D_49_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(AlignedBox3D_49_Type);
    static Vector_17_Concept = new Vector_17_Concept(AlignedBox3D_49_Type);
    static Interval_26_Concept = new Interval_26_Concept(AlignedBox3D_49_Type);
    static Implements = [Array_15_Concept,Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept,Vector_17_Concept,Interval_26_Concept];
}
class Complex_50_Type
{
    constructor(Real_668, Imaginary_675)
    {
        // field initialization 
        this.Real_668 = Real_668;
        this.Imaginary_675 = Imaginary_675;
        this.Count_2447 = Complex_50_Type.Array_15_Concept.Count_2447;
        this.At_2452 = Complex_50_Type.Array_15_Concept.At_2452;
        this.Default_2460 = Complex_50_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = Complex_50_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = Complex_50_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = Complex_50_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = Complex_50_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = Complex_50_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = Complex_50_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = Complex_50_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Complex_50_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Complex_50_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = Complex_50_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Complex_50_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Complex_50_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Complex_50_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Complex_50_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = Complex_50_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = Complex_50_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = Complex_50_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = Complex_50_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = Complex_50_Type.Numerical_19_Concept.MaxValue_2529;
        this.Count_2470 = Complex_50_Type.Vector_17_Concept.Count_2470;
        this.At_2484 = Complex_50_Type.Vector_17_Concept.At_2484;
    }
    // field accessors
    static Real_668 = function(self) { return self.Real_668; }
    static Imaginary_675 = function(self) { return self.Imaginary_675; }
    // implemented concepts 
    static Array_15_Concept = new Array_15_Concept(Complex_50_Type);
    static Value_16_Concept = new Value_16_Concept(Complex_50_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(Complex_50_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Complex_50_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Complex_50_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Complex_50_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Complex_50_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(Complex_50_Type);
    static Vector_17_Concept = new Vector_17_Concept(Complex_50_Type);
    static Implements = [Array_15_Concept,Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept,Vector_17_Concept];
}
class Ray3D_51_Type
{
    constructor(Direction_682, Position_689)
    {
        // field initialization 
        this.Direction_682 = Direction_682;
        this.Position_689 = Position_689;
        this.Default_2460 = Ray3D_51_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Direction_682 = function(self) { return self.Direction_682; }
    static Position_689 = function(self) { return self.Position_689; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Ray3D_51_Type);
    static Implements = [Value_16_Concept];
}
class Ray2D_52_Type
{
    constructor(Direction_696, Position_703)
    {
        // field initialization 
        this.Direction_696 = Direction_696;
        this.Position_703 = Position_703;
        this.Default_2460 = Ray2D_52_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Direction_696 = function(self) { return self.Direction_696; }
    static Position_703 = function(self) { return self.Position_703; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Ray2D_52_Type);
    static Implements = [Value_16_Concept];
}
class Sphere_53_Type
{
    constructor(Center_710, Radius_717)
    {
        // field initialization 
        this.Center_710 = Center_710;
        this.Radius_717 = Radius_717;
        this.Default_2460 = Sphere_53_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Center_710 = function(self) { return self.Center_710; }
    static Radius_717 = function(self) { return self.Radius_717; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Sphere_53_Type);
    static Implements = [Value_16_Concept];
}
class Plane_54_Type
{
    constructor(Normal_724, D_731)
    {
        // field initialization 
        this.Normal_724 = Normal_724;
        this.D_731 = D_731;
        this.Default_2460 = Plane_54_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Normal_724 = function(self) { return self.Normal_724; }
    static D_731 = function(self) { return self.D_731; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Plane_54_Type);
    static Implements = [Value_16_Concept];
}
class Triangle3D_55_Type
{
    constructor(A_738, B_745, C_752)
    {
        // field initialization 
        this.A_738 = A_738;
        this.B_745 = B_745;
        this.C_752 = C_752;
        this.Default_2460 = Triangle3D_55_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static A_738 = function(self) { return self.A_738; }
    static B_745 = function(self) { return self.B_745; }
    static C_752 = function(self) { return self.C_752; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Triangle3D_55_Type);
    static Implements = [Value_16_Concept];
}
class Triangle2D_56_Type
{
    constructor(A_759, B_766, C_773)
    {
        // field initialization 
        this.A_759 = A_759;
        this.B_766 = B_766;
        this.C_773 = C_773;
        this.Default_2460 = Triangle2D_56_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static A_759 = function(self) { return self.A_759; }
    static B_766 = function(self) { return self.B_766; }
    static C_773 = function(self) { return self.C_773; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Triangle2D_56_Type);
    static Implements = [Value_16_Concept];
}
class Quad3D_57_Type
{
    constructor(A_780, B_787, C_794, D_801)
    {
        // field initialization 
        this.A_780 = A_780;
        this.B_787 = B_787;
        this.C_794 = C_794;
        this.D_801 = D_801;
        this.Default_2460 = Quad3D_57_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static A_780 = function(self) { return self.A_780; }
    static B_787 = function(self) { return self.B_787; }
    static C_794 = function(self) { return self.C_794; }
    static D_801 = function(self) { return self.D_801; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Quad3D_57_Type);
    static Implements = [Value_16_Concept];
}
class Quad2D_58_Type
{
    constructor(A_808, B_815, C_822, D_829)
    {
        // field initialization 
        this.A_808 = A_808;
        this.B_815 = B_815;
        this.C_822 = C_822;
        this.D_829 = D_829;
        this.Default_2460 = Quad2D_58_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static A_808 = function(self) { return self.A_808; }
    static B_815 = function(self) { return self.B_815; }
    static C_822 = function(self) { return self.C_822; }
    static D_829 = function(self) { return self.D_829; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Quad2D_58_Type);
    static Implements = [Value_16_Concept];
}
class Point3D_59_Type
{
    constructor(Value_836)
    {
        // field initialization 
        this.Value_836 = Value_836;
        this.Default_2460 = Point3D_59_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Value_836 = function(self) { return self.Value_836; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Point3D_59_Type);
    static Implements = [Value_16_Concept];
}
class Point2D_60_Type
{
    constructor(Value_843)
    {
        // field initialization 
        this.Value_843 = Value_843;
        this.Default_2460 = Point2D_60_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Value_843 = function(self) { return self.Value_843; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Point2D_60_Type);
    static Implements = [Value_16_Concept];
}
class Line3D_61_Type
{
    constructor(A_850, B_857)
    {
        // field initialization 
        this.A_850 = A_850;
        this.B_857 = B_857;
        this.Count_2447 = Line3D_61_Type.Array_15_Concept.Count_2447;
        this.At_2452 = Line3D_61_Type.Array_15_Concept.At_2452;
        this.Default_2460 = Line3D_61_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = Line3D_61_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = Line3D_61_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = Line3D_61_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = Line3D_61_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = Line3D_61_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = Line3D_61_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = Line3D_61_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Line3D_61_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Line3D_61_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = Line3D_61_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Line3D_61_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Line3D_61_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Line3D_61_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Line3D_61_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = Line3D_61_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = Line3D_61_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = Line3D_61_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = Line3D_61_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = Line3D_61_Type.Numerical_19_Concept.MaxValue_2529;
        this.Count_2470 = Line3D_61_Type.Vector_17_Concept.Count_2470;
        this.At_2484 = Line3D_61_Type.Vector_17_Concept.At_2484;
        this.Min_2773 = Line3D_61_Type.Interval_26_Concept.Min_2773;
        this.Max_2776 = Line3D_61_Type.Interval_26_Concept.Max_2776;
    }
    // field accessors
    static A_850 = function(self) { return self.A_850; }
    static B_857 = function(self) { return self.B_857; }
    // implemented concepts 
    static Array_15_Concept = new Array_15_Concept(Line3D_61_Type);
    static Value_16_Concept = new Value_16_Concept(Line3D_61_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(Line3D_61_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Line3D_61_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Line3D_61_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Line3D_61_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Line3D_61_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(Line3D_61_Type);
    static Vector_17_Concept = new Vector_17_Concept(Line3D_61_Type);
    static Interval_26_Concept = new Interval_26_Concept(Line3D_61_Type);
    static Implements = [Array_15_Concept,Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept,Vector_17_Concept,Interval_26_Concept];
}
class Line2D_62_Type
{
    constructor(A_864, B_871)
    {
        // field initialization 
        this.A_864 = A_864;
        this.B_871 = B_871;
        this.Count_2447 = Line2D_62_Type.Array_15_Concept.Count_2447;
        this.At_2452 = Line2D_62_Type.Array_15_Concept.At_2452;
        this.Default_2460 = Line2D_62_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = Line2D_62_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = Line2D_62_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = Line2D_62_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = Line2D_62_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = Line2D_62_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = Line2D_62_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = Line2D_62_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Line2D_62_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Line2D_62_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = Line2D_62_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Line2D_62_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Line2D_62_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Line2D_62_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Line2D_62_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = Line2D_62_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = Line2D_62_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = Line2D_62_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = Line2D_62_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = Line2D_62_Type.Numerical_19_Concept.MaxValue_2529;
        this.Count_2470 = Line2D_62_Type.Vector_17_Concept.Count_2470;
        this.At_2484 = Line2D_62_Type.Vector_17_Concept.At_2484;
        this.Min_2773 = Line2D_62_Type.Interval_26_Concept.Min_2773;
        this.Max_2776 = Line2D_62_Type.Interval_26_Concept.Max_2776;
    }
    // field accessors
    static A_864 = function(self) { return self.A_864; }
    static B_871 = function(self) { return self.B_871; }
    // implemented concepts 
    static Array_15_Concept = new Array_15_Concept(Line2D_62_Type);
    static Value_16_Concept = new Value_16_Concept(Line2D_62_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(Line2D_62_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Line2D_62_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Line2D_62_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Line2D_62_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Line2D_62_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(Line2D_62_Type);
    static Vector_17_Concept = new Vector_17_Concept(Line2D_62_Type);
    static Interval_26_Concept = new Interval_26_Concept(Line2D_62_Type);
    static Implements = [Array_15_Concept,Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept,Vector_17_Concept,Interval_26_Concept];
}
class Color_63_Type
{
    constructor(R_878, G_885, B_892, A_899)
    {
        // field initialization 
        this.R_878 = R_878;
        this.G_885 = G_885;
        this.B_892 = B_892;
        this.A_899 = A_899;
        this.Default_2460 = Color_63_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static R_878 = function(self) { return self.R_878; }
    static G_885 = function(self) { return self.G_885; }
    static B_892 = function(self) { return self.B_892; }
    static A_899 = function(self) { return self.A_899; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Color_63_Type);
    static Implements = [Value_16_Concept];
}
class ColorLUV_64_Type
{
    constructor(Lightness_906, U_913, V_920)
    {
        // field initialization 
        this.Lightness_906 = Lightness_906;
        this.U_913 = U_913;
        this.V_920 = V_920;
        this.Default_2460 = ColorLUV_64_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Lightness_906 = function(self) { return self.Lightness_906; }
    static U_913 = function(self) { return self.U_913; }
    static V_920 = function(self) { return self.V_920; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ColorLUV_64_Type);
    static Implements = [Value_16_Concept];
}
class ColorLAB_65_Type
{
    constructor(Lightness_927, A_934, B_941)
    {
        // field initialization 
        this.Lightness_927 = Lightness_927;
        this.A_934 = A_934;
        this.B_941 = B_941;
        this.Default_2460 = ColorLAB_65_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Lightness_927 = function(self) { return self.Lightness_927; }
    static A_934 = function(self) { return self.A_934; }
    static B_941 = function(self) { return self.B_941; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ColorLAB_65_Type);
    static Implements = [Value_16_Concept];
}
class ColorLCh_66_Type
{
    constructor(Lightness_948, ChromaHue_955)
    {
        // field initialization 
        this.Lightness_948 = Lightness_948;
        this.ChromaHue_955 = ChromaHue_955;
        this.Default_2460 = ColorLCh_66_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Lightness_948 = function(self) { return self.Lightness_948; }
    static ChromaHue_955 = function(self) { return self.ChromaHue_955; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ColorLCh_66_Type);
    static Implements = [Value_16_Concept];
}
class ColorHSV_67_Type
{
    constructor(Hue_962, S_969, V_976)
    {
        // field initialization 
        this.Hue_962 = Hue_962;
        this.S_969 = S_969;
        this.V_976 = V_976;
        this.Default_2460 = ColorHSV_67_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Hue_962 = function(self) { return self.Hue_962; }
    static S_969 = function(self) { return self.S_969; }
    static V_976 = function(self) { return self.V_976; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ColorHSV_67_Type);
    static Implements = [Value_16_Concept];
}
class ColorHSL_68_Type
{
    constructor(Hue_983, Saturation_990, Luminance_997)
    {
        // field initialization 
        this.Hue_983 = Hue_983;
        this.Saturation_990 = Saturation_990;
        this.Luminance_997 = Luminance_997;
        this.Default_2460 = ColorHSL_68_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Hue_983 = function(self) { return self.Hue_983; }
    static Saturation_990 = function(self) { return self.Saturation_990; }
    static Luminance_997 = function(self) { return self.Luminance_997; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ColorHSL_68_Type);
    static Implements = [Value_16_Concept];
}
class ColorYCbCr_69_Type
{
    constructor(Y_1004, Cb_1011, Cr_1018)
    {
        // field initialization 
        this.Y_1004 = Y_1004;
        this.Cb_1011 = Cb_1011;
        this.Cr_1018 = Cr_1018;
        this.Default_2460 = ColorYCbCr_69_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Y_1004 = function(self) { return self.Y_1004; }
    static Cb_1011 = function(self) { return self.Cb_1011; }
    static Cr_1018 = function(self) { return self.Cr_1018; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ColorYCbCr_69_Type);
    static Implements = [Value_16_Concept];
}
class SphericalCoordinate_70_Type
{
    constructor(Radius_1025, Azimuth_1032, Polar_1039)
    {
        // field initialization 
        this.Radius_1025 = Radius_1025;
        this.Azimuth_1032 = Azimuth_1032;
        this.Polar_1039 = Polar_1039;
        this.Default_2460 = SphericalCoordinate_70_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Radius_1025 = function(self) { return self.Radius_1025; }
    static Azimuth_1032 = function(self) { return self.Azimuth_1032; }
    static Polar_1039 = function(self) { return self.Polar_1039; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(SphericalCoordinate_70_Type);
    static Implements = [Value_16_Concept];
}
class PolarCoordinate_71_Type
{
    constructor(Radius_1046, Angle_1053)
    {
        // field initialization 
        this.Radius_1046 = Radius_1046;
        this.Angle_1053 = Angle_1053;
        this.Default_2460 = PolarCoordinate_71_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Radius_1046 = function(self) { return self.Radius_1046; }
    static Angle_1053 = function(self) { return self.Angle_1053; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(PolarCoordinate_71_Type);
    static Implements = [Value_16_Concept];
}
class LogPolarCoordinate_72_Type
{
    constructor(Rho_1060, Azimuth_1067)
    {
        // field initialization 
        this.Rho_1060 = Rho_1060;
        this.Azimuth_1067 = Azimuth_1067;
        this.Default_2460 = LogPolarCoordinate_72_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Rho_1060 = function(self) { return self.Rho_1060; }
    static Azimuth_1067 = function(self) { return self.Azimuth_1067; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(LogPolarCoordinate_72_Type);
    static Implements = [Value_16_Concept];
}
class CylindricalCoordinate_73_Type
{
    constructor(RadialDistance_1074, Azimuth_1081, Height_1088)
    {
        // field initialization 
        this.RadialDistance_1074 = RadialDistance_1074;
        this.Azimuth_1081 = Azimuth_1081;
        this.Height_1088 = Height_1088;
        this.Default_2460 = CylindricalCoordinate_73_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static RadialDistance_1074 = function(self) { return self.RadialDistance_1074; }
    static Azimuth_1081 = function(self) { return self.Azimuth_1081; }
    static Height_1088 = function(self) { return self.Height_1088; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(CylindricalCoordinate_73_Type);
    static Implements = [Value_16_Concept];
}
class HorizontalCoordinate_74_Type
{
    constructor(Radius_1095, Azimuth_1102, Height_1109)
    {
        // field initialization 
        this.Radius_1095 = Radius_1095;
        this.Azimuth_1102 = Azimuth_1102;
        this.Height_1109 = Height_1109;
        this.Default_2460 = HorizontalCoordinate_74_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Radius_1095 = function(self) { return self.Radius_1095; }
    static Azimuth_1102 = function(self) { return self.Azimuth_1102; }
    static Height_1109 = function(self) { return self.Height_1109; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(HorizontalCoordinate_74_Type);
    static Implements = [Value_16_Concept];
}
class GeoCoordinate_75_Type
{
    constructor(Latitude_1116, Longitude_1123)
    {
        // field initialization 
        this.Latitude_1116 = Latitude_1116;
        this.Longitude_1123 = Longitude_1123;
        this.Default_2460 = GeoCoordinate_75_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Latitude_1116 = function(self) { return self.Latitude_1116; }
    static Longitude_1123 = function(self) { return self.Longitude_1123; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(GeoCoordinate_75_Type);
    static Implements = [Value_16_Concept];
}
class GeoCoordinateWithAltitude_76_Type
{
    constructor(Coordinate_1130, Altitude_1137)
    {
        // field initialization 
        this.Coordinate_1130 = Coordinate_1130;
        this.Altitude_1137 = Altitude_1137;
        this.Default_2460 = GeoCoordinateWithAltitude_76_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Coordinate_1130 = function(self) { return self.Coordinate_1130; }
    static Altitude_1137 = function(self) { return self.Altitude_1137; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(GeoCoordinateWithAltitude_76_Type);
    static Implements = [Value_16_Concept];
}
class Circle_77_Type
{
    constructor(Center_1144, Radius_1151)
    {
        // field initialization 
        this.Center_1144 = Center_1144;
        this.Radius_1151 = Radius_1151;
        this.Default_2460 = Circle_77_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Center_1144 = function(self) { return self.Center_1144; }
    static Radius_1151 = function(self) { return self.Radius_1151; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Circle_77_Type);
    static Implements = [Value_16_Concept];
}
class Chord_78_Type
{
    constructor(Circle_1158, Arc_1165)
    {
        // field initialization 
        this.Circle_1158 = Circle_1158;
        this.Arc_1165 = Arc_1165;
        this.Default_2460 = Chord_78_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Circle_1158 = function(self) { return self.Circle_1158; }
    static Arc_1165 = function(self) { return self.Arc_1165; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Chord_78_Type);
    static Implements = [Value_16_Concept];
}
class Size2D_79_Type
{
    constructor(Width_1172, Height_1179)
    {
        // field initialization 
        this.Width_1172 = Width_1172;
        this.Height_1179 = Height_1179;
        this.Default_2460 = Size2D_79_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Width_1172 = function(self) { return self.Width_1172; }
    static Height_1179 = function(self) { return self.Height_1179; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Size2D_79_Type);
    static Implements = [Value_16_Concept];
}
class Size3D_80_Type
{
    constructor(Width_1186, Height_1193, Depth_1200)
    {
        // field initialization 
        this.Width_1186 = Width_1186;
        this.Height_1193 = Height_1193;
        this.Depth_1200 = Depth_1200;
        this.Default_2460 = Size3D_80_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Width_1186 = function(self) { return self.Width_1186; }
    static Height_1193 = function(self) { return self.Height_1193; }
    static Depth_1200 = function(self) { return self.Depth_1200; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Size3D_80_Type);
    static Implements = [Value_16_Concept];
}
class Rectangle2D_81_Type
{
    constructor(Center_1207, Size_1214)
    {
        // field initialization 
        this.Center_1207 = Center_1207;
        this.Size_1214 = Size_1214;
        this.Default_2460 = Rectangle2D_81_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Center_1207 = function(self) { return self.Center_1207; }
    static Size_1214 = function(self) { return self.Size_1214; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Rectangle2D_81_Type);
    static Implements = [Value_16_Concept];
}
class Proportion_82_Type
{
    constructor(Value_1221)
    {
        // field initialization 
        this.Value_1221 = Value_1221;
        this.Default_2460 = Proportion_82_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = Proportion_82_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = Proportion_82_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = Proportion_82_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = Proportion_82_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = Proportion_82_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = Proportion_82_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = Proportion_82_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Proportion_82_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Proportion_82_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = Proportion_82_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Proportion_82_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Proportion_82_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Proportion_82_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Proportion_82_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = Proportion_82_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = Proportion_82_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = Proportion_82_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = Proportion_82_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = Proportion_82_Type.Numerical_19_Concept.MaxValue_2529;
    }
    // field accessors
    static Value_1221 = function(self) { return self.Value_1221; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Proportion_82_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(Proportion_82_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Proportion_82_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Proportion_82_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Proportion_82_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Proportion_82_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(Proportion_82_Type);
    static Implements = [Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept];
}
class Fraction_83_Type
{
    constructor(Numerator_1228, Denominator_1235)
    {
        // field initialization 
        this.Numerator_1228 = Numerator_1228;
        this.Denominator_1235 = Denominator_1235;
        this.Default_2460 = Fraction_83_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Numerator_1228 = function(self) { return self.Numerator_1228; }
    static Denominator_1235 = function(self) { return self.Denominator_1235; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Fraction_83_Type);
    static Implements = [Value_16_Concept];
}
class Angle_84_Type
{
    constructor(Radians_1242)
    {
        // field initialization 
        this.Radians_1242 = Radians_1242;
        this.Default_2460 = Angle_84_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = Angle_84_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Angle_84_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Angle_84_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Angle_84_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Angle_84_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = Angle_84_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Angle_84_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Angle_84_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = Angle_84_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Radians_1242 = function(self) { return self.Radians_1242; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Angle_84_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Angle_84_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Angle_84_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Angle_84_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Angle_84_Type);
    static Measure_18_Concept = new Measure_18_Concept(Angle_84_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class Length_85_Type
{
    constructor(Meters_1249)
    {
        // field initialization 
        this.Meters_1249 = Meters_1249;
        this.Default_2460 = Length_85_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = Length_85_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Length_85_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Length_85_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Length_85_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Length_85_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = Length_85_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Length_85_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Length_85_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = Length_85_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Meters_1249 = function(self) { return self.Meters_1249; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Length_85_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Length_85_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Length_85_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Length_85_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Length_85_Type);
    static Measure_18_Concept = new Measure_18_Concept(Length_85_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class Mass_86_Type
{
    constructor(Kilograms_1256)
    {
        // field initialization 
        this.Kilograms_1256 = Kilograms_1256;
        this.Default_2460 = Mass_86_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = Mass_86_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Mass_86_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Mass_86_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Mass_86_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Mass_86_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = Mass_86_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Mass_86_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Mass_86_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = Mass_86_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Kilograms_1256 = function(self) { return self.Kilograms_1256; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Mass_86_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Mass_86_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Mass_86_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Mass_86_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Mass_86_Type);
    static Measure_18_Concept = new Measure_18_Concept(Mass_86_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class Temperature_87_Type
{
    constructor(Celsius_1263)
    {
        // field initialization 
        this.Celsius_1263 = Celsius_1263;
        this.Default_2460 = Temperature_87_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = Temperature_87_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Temperature_87_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Temperature_87_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Temperature_87_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Temperature_87_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = Temperature_87_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Temperature_87_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Temperature_87_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = Temperature_87_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Celsius_1263 = function(self) { return self.Celsius_1263; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Temperature_87_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Temperature_87_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Temperature_87_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Temperature_87_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Temperature_87_Type);
    static Measure_18_Concept = new Measure_18_Concept(Temperature_87_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class TimeSpan_88_Type
{
    constructor(Seconds_1270)
    {
        // field initialization 
        this.Seconds_1270 = Seconds_1270;
        this.Default_2460 = TimeSpan_88_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = TimeSpan_88_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = TimeSpan_88_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = TimeSpan_88_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = TimeSpan_88_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = TimeSpan_88_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = TimeSpan_88_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = TimeSpan_88_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = TimeSpan_88_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = TimeSpan_88_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Seconds_1270 = function(self) { return self.Seconds_1270; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(TimeSpan_88_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(TimeSpan_88_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(TimeSpan_88_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(TimeSpan_88_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(TimeSpan_88_Type);
    static Measure_18_Concept = new Measure_18_Concept(TimeSpan_88_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class TimeRange_89_Type
{
    constructor(Min_1277, Max_1284)
    {
        // field initialization 
        this.Min_1277 = Min_1277;
        this.Max_1284 = Max_1284;
        this.Count_2447 = TimeRange_89_Type.Array_15_Concept.Count_2447;
        this.At_2452 = TimeRange_89_Type.Array_15_Concept.At_2452;
        this.Default_2460 = TimeRange_89_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = TimeRange_89_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = TimeRange_89_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = TimeRange_89_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = TimeRange_89_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = TimeRange_89_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = TimeRange_89_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = TimeRange_89_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = TimeRange_89_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = TimeRange_89_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = TimeRange_89_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = TimeRange_89_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = TimeRange_89_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = TimeRange_89_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = TimeRange_89_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = TimeRange_89_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = TimeRange_89_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = TimeRange_89_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = TimeRange_89_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = TimeRange_89_Type.Numerical_19_Concept.MaxValue_2529;
        this.Count_2470 = TimeRange_89_Type.Vector_17_Concept.Count_2470;
        this.At_2484 = TimeRange_89_Type.Vector_17_Concept.At_2484;
        this.Min_2773 = TimeRange_89_Type.Interval_26_Concept.Min_2773;
        this.Max_2776 = TimeRange_89_Type.Interval_26_Concept.Max_2776;
    }
    // field accessors
    static Min_1277 = function(self) { return self.Min_1277; }
    static Max_1284 = function(self) { return self.Max_1284; }
    // implemented concepts 
    static Array_15_Concept = new Array_15_Concept(TimeRange_89_Type);
    static Value_16_Concept = new Value_16_Concept(TimeRange_89_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(TimeRange_89_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(TimeRange_89_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(TimeRange_89_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(TimeRange_89_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(TimeRange_89_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(TimeRange_89_Type);
    static Vector_17_Concept = new Vector_17_Concept(TimeRange_89_Type);
    static Interval_26_Concept = new Interval_26_Concept(TimeRange_89_Type);
    static Implements = [Array_15_Concept,Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept,Vector_17_Concept,Interval_26_Concept];
}
class DateTime_90_Type
{
    constructor()
    {
        // field initialization 
        this.Default_2460 = DateTime_90_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(DateTime_90_Type);
    static Implements = [Value_16_Concept];
}
class AnglePair_91_Type
{
    constructor(Start_1291, End_1298)
    {
        // field initialization 
        this.Start_1291 = Start_1291;
        this.End_1298 = End_1298;
        this.Count_2447 = AnglePair_91_Type.Array_15_Concept.Count_2447;
        this.At_2452 = AnglePair_91_Type.Array_15_Concept.At_2452;
        this.Default_2460 = AnglePair_91_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = AnglePair_91_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = AnglePair_91_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = AnglePair_91_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = AnglePair_91_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = AnglePair_91_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = AnglePair_91_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = AnglePair_91_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = AnglePair_91_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = AnglePair_91_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = AnglePair_91_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = AnglePair_91_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = AnglePair_91_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = AnglePair_91_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = AnglePair_91_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = AnglePair_91_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = AnglePair_91_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = AnglePair_91_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = AnglePair_91_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = AnglePair_91_Type.Numerical_19_Concept.MaxValue_2529;
        this.Count_2470 = AnglePair_91_Type.Vector_17_Concept.Count_2470;
        this.At_2484 = AnglePair_91_Type.Vector_17_Concept.At_2484;
        this.Min_2773 = AnglePair_91_Type.Interval_26_Concept.Min_2773;
        this.Max_2776 = AnglePair_91_Type.Interval_26_Concept.Max_2776;
    }
    // field accessors
    static Start_1291 = function(self) { return self.Start_1291; }
    static End_1298 = function(self) { return self.End_1298; }
    // implemented concepts 
    static Array_15_Concept = new Array_15_Concept(AnglePair_91_Type);
    static Value_16_Concept = new Value_16_Concept(AnglePair_91_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(AnglePair_91_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(AnglePair_91_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(AnglePair_91_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(AnglePair_91_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(AnglePair_91_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(AnglePair_91_Type);
    static Vector_17_Concept = new Vector_17_Concept(AnglePair_91_Type);
    static Interval_26_Concept = new Interval_26_Concept(AnglePair_91_Type);
    static Implements = [Array_15_Concept,Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept,Vector_17_Concept,Interval_26_Concept];
}
class Ring_92_Type
{
    constructor(Circle_1305, InnerRadius_1312)
    {
        // field initialization 
        this.Circle_1305 = Circle_1305;
        this.InnerRadius_1312 = InnerRadius_1312;
        this.Default_2460 = Ring_92_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = Ring_92_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = Ring_92_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = Ring_92_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = Ring_92_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = Ring_92_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = Ring_92_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = Ring_92_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Ring_92_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Ring_92_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = Ring_92_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Ring_92_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Ring_92_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Ring_92_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Ring_92_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = Ring_92_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = Ring_92_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = Ring_92_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = Ring_92_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = Ring_92_Type.Numerical_19_Concept.MaxValue_2529;
    }
    // field accessors
    static Circle_1305 = function(self) { return self.Circle_1305; }
    static InnerRadius_1312 = function(self) { return self.InnerRadius_1312; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Ring_92_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(Ring_92_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Ring_92_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Ring_92_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Ring_92_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Ring_92_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(Ring_92_Type);
    static Implements = [Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept];
}
class Arc_93_Type
{
    constructor(Angles_1319, Cirlce_1326)
    {
        // field initialization 
        this.Angles_1319 = Angles_1319;
        this.Cirlce_1326 = Cirlce_1326;
        this.Default_2460 = Arc_93_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Angles_1319 = function(self) { return self.Angles_1319; }
    static Cirlce_1326 = function(self) { return self.Cirlce_1326; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Arc_93_Type);
    static Implements = [Value_16_Concept];
}
class TimeInterval_94_Type
{
    constructor(Start_1333, End_1340)
    {
        // field initialization 
        this.Start_1333 = Start_1333;
        this.End_1340 = End_1340;
        this.Count_2447 = TimeInterval_94_Type.Array_15_Concept.Count_2447;
        this.At_2452 = TimeInterval_94_Type.Array_15_Concept.At_2452;
        this.Default_2460 = TimeInterval_94_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = TimeInterval_94_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = TimeInterval_94_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = TimeInterval_94_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = TimeInterval_94_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = TimeInterval_94_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = TimeInterval_94_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = TimeInterval_94_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = TimeInterval_94_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = TimeInterval_94_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = TimeInterval_94_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = TimeInterval_94_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = TimeInterval_94_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = TimeInterval_94_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = TimeInterval_94_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = TimeInterval_94_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = TimeInterval_94_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = TimeInterval_94_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = TimeInterval_94_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = TimeInterval_94_Type.Numerical_19_Concept.MaxValue_2529;
        this.Count_2470 = TimeInterval_94_Type.Vector_17_Concept.Count_2470;
        this.At_2484 = TimeInterval_94_Type.Vector_17_Concept.At_2484;
        this.Min_2773 = TimeInterval_94_Type.Interval_26_Concept.Min_2773;
        this.Max_2776 = TimeInterval_94_Type.Interval_26_Concept.Max_2776;
    }
    // field accessors
    static Start_1333 = function(self) { return self.Start_1333; }
    static End_1340 = function(self) { return self.End_1340; }
    // implemented concepts 
    static Array_15_Concept = new Array_15_Concept(TimeInterval_94_Type);
    static Value_16_Concept = new Value_16_Concept(TimeInterval_94_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(TimeInterval_94_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(TimeInterval_94_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(TimeInterval_94_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(TimeInterval_94_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(TimeInterval_94_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(TimeInterval_94_Type);
    static Vector_17_Concept = new Vector_17_Concept(TimeInterval_94_Type);
    static Interval_26_Concept = new Interval_26_Concept(TimeInterval_94_Type);
    static Implements = [Array_15_Concept,Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept,Vector_17_Concept,Interval_26_Concept];
}
class RealInterval_95_Type
{
    constructor(A_1347, B_1354)
    {
        // field initialization 
        this.A_1347 = A_1347;
        this.B_1354 = B_1354;
        this.Count_2447 = RealInterval_95_Type.Array_15_Concept.Count_2447;
        this.At_2452 = RealInterval_95_Type.Array_15_Concept.At_2452;
        this.Default_2460 = RealInterval_95_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = RealInterval_95_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = RealInterval_95_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = RealInterval_95_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = RealInterval_95_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = RealInterval_95_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = RealInterval_95_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = RealInterval_95_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = RealInterval_95_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = RealInterval_95_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = RealInterval_95_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = RealInterval_95_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = RealInterval_95_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = RealInterval_95_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = RealInterval_95_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = RealInterval_95_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = RealInterval_95_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = RealInterval_95_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = RealInterval_95_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = RealInterval_95_Type.Numerical_19_Concept.MaxValue_2529;
        this.Count_2470 = RealInterval_95_Type.Vector_17_Concept.Count_2470;
        this.At_2484 = RealInterval_95_Type.Vector_17_Concept.At_2484;
        this.Min_2773 = RealInterval_95_Type.Interval_26_Concept.Min_2773;
        this.Max_2776 = RealInterval_95_Type.Interval_26_Concept.Max_2776;
    }
    // field accessors
    static A_1347 = function(self) { return self.A_1347; }
    static B_1354 = function(self) { return self.B_1354; }
    // implemented concepts 
    static Array_15_Concept = new Array_15_Concept(RealInterval_95_Type);
    static Value_16_Concept = new Value_16_Concept(RealInterval_95_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(RealInterval_95_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(RealInterval_95_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(RealInterval_95_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(RealInterval_95_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(RealInterval_95_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(RealInterval_95_Type);
    static Vector_17_Concept = new Vector_17_Concept(RealInterval_95_Type);
    static Interval_26_Concept = new Interval_26_Concept(RealInterval_95_Type);
    static Implements = [Array_15_Concept,Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept,Vector_17_Concept,Interval_26_Concept];
}
class Interval2D_96_Type
{
    constructor(A_1361, B_1368)
    {
        // field initialization 
        this.A_1361 = A_1361;
        this.B_1368 = B_1368;
        this.Count_2447 = Interval2D_96_Type.Array_15_Concept.Count_2447;
        this.At_2452 = Interval2D_96_Type.Array_15_Concept.At_2452;
        this.Default_2460 = Interval2D_96_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = Interval2D_96_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = Interval2D_96_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = Interval2D_96_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = Interval2D_96_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = Interval2D_96_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = Interval2D_96_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = Interval2D_96_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Interval2D_96_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Interval2D_96_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = Interval2D_96_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Interval2D_96_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Interval2D_96_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Interval2D_96_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Interval2D_96_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = Interval2D_96_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = Interval2D_96_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = Interval2D_96_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = Interval2D_96_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = Interval2D_96_Type.Numerical_19_Concept.MaxValue_2529;
        this.Count_2470 = Interval2D_96_Type.Vector_17_Concept.Count_2470;
        this.At_2484 = Interval2D_96_Type.Vector_17_Concept.At_2484;
        this.Min_2773 = Interval2D_96_Type.Interval_26_Concept.Min_2773;
        this.Max_2776 = Interval2D_96_Type.Interval_26_Concept.Max_2776;
    }
    // field accessors
    static A_1361 = function(self) { return self.A_1361; }
    static B_1368 = function(self) { return self.B_1368; }
    // implemented concepts 
    static Array_15_Concept = new Array_15_Concept(Interval2D_96_Type);
    static Value_16_Concept = new Value_16_Concept(Interval2D_96_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(Interval2D_96_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Interval2D_96_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Interval2D_96_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Interval2D_96_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Interval2D_96_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(Interval2D_96_Type);
    static Vector_17_Concept = new Vector_17_Concept(Interval2D_96_Type);
    static Interval_26_Concept = new Interval_26_Concept(Interval2D_96_Type);
    static Implements = [Array_15_Concept,Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept,Vector_17_Concept,Interval_26_Concept];
}
class Interval3D_97_Type
{
    constructor(A_1375, B_1382)
    {
        // field initialization 
        this.A_1375 = A_1375;
        this.B_1382 = B_1382;
        this.Count_2447 = Interval3D_97_Type.Array_15_Concept.Count_2447;
        this.At_2452 = Interval3D_97_Type.Array_15_Concept.At_2452;
        this.Default_2460 = Interval3D_97_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = Interval3D_97_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = Interval3D_97_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = Interval3D_97_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = Interval3D_97_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = Interval3D_97_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = Interval3D_97_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = Interval3D_97_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Interval3D_97_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Interval3D_97_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = Interval3D_97_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Interval3D_97_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Interval3D_97_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Interval3D_97_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Interval3D_97_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = Interval3D_97_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = Interval3D_97_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = Interval3D_97_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = Interval3D_97_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = Interval3D_97_Type.Numerical_19_Concept.MaxValue_2529;
        this.Count_2470 = Interval3D_97_Type.Vector_17_Concept.Count_2470;
        this.At_2484 = Interval3D_97_Type.Vector_17_Concept.At_2484;
        this.Min_2773 = Interval3D_97_Type.Interval_26_Concept.Min_2773;
        this.Max_2776 = Interval3D_97_Type.Interval_26_Concept.Max_2776;
    }
    // field accessors
    static A_1375 = function(self) { return self.A_1375; }
    static B_1382 = function(self) { return self.B_1382; }
    // implemented concepts 
    static Array_15_Concept = new Array_15_Concept(Interval3D_97_Type);
    static Value_16_Concept = new Value_16_Concept(Interval3D_97_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(Interval3D_97_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Interval3D_97_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Interval3D_97_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Interval3D_97_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Interval3D_97_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(Interval3D_97_Type);
    static Vector_17_Concept = new Vector_17_Concept(Interval3D_97_Type);
    static Interval_26_Concept = new Interval_26_Concept(Interval3D_97_Type);
    static Implements = [Array_15_Concept,Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept,Vector_17_Concept,Interval_26_Concept];
}
class Capsule_98_Type
{
    constructor(Line_1389, Radius_1396)
    {
        // field initialization 
        this.Line_1389 = Line_1389;
        this.Radius_1396 = Radius_1396;
        this.Default_2460 = Capsule_98_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Line_1389 = function(self) { return self.Line_1389; }
    static Radius_1396 = function(self) { return self.Radius_1396; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Capsule_98_Type);
    static Implements = [Value_16_Concept];
}
class Matrix3D_99_Type
{
    constructor(Column1_1403, Column2_1410, Column3_1417, Column4_1424)
    {
        // field initialization 
        this.Column1_1403 = Column1_1403;
        this.Column2_1410 = Column2_1410;
        this.Column3_1417 = Column3_1417;
        this.Column4_1424 = Column4_1424;
        this.Default_2460 = Matrix3D_99_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Column1_1403 = function(self) { return self.Column1_1403; }
    static Column2_1410 = function(self) { return self.Column2_1410; }
    static Column3_1417 = function(self) { return self.Column3_1417; }
    static Column4_1424 = function(self) { return self.Column4_1424; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Matrix3D_99_Type);
    static Implements = [Value_16_Concept];
}
class Cylinder_100_Type
{
    constructor(Line_1431, Radius_1438)
    {
        // field initialization 
        this.Line_1431 = Line_1431;
        this.Radius_1438 = Radius_1438;
        this.Default_2460 = Cylinder_100_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Line_1431 = function(self) { return self.Line_1431; }
    static Radius_1438 = function(self) { return self.Radius_1438; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Cylinder_100_Type);
    static Implements = [Value_16_Concept];
}
class Cone_101_Type
{
    constructor(Line_1445, Radius_1452)
    {
        // field initialization 
        this.Line_1445 = Line_1445;
        this.Radius_1452 = Radius_1452;
        this.Default_2460 = Cone_101_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Line_1445 = function(self) { return self.Line_1445; }
    static Radius_1452 = function(self) { return self.Radius_1452; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Cone_101_Type);
    static Implements = [Value_16_Concept];
}
class Tube_102_Type
{
    constructor(Line_1459, InnerRadius_1466, OuterRadius_1473)
    {
        // field initialization 
        this.Line_1459 = Line_1459;
        this.InnerRadius_1466 = InnerRadius_1466;
        this.OuterRadius_1473 = OuterRadius_1473;
        this.Default_2460 = Tube_102_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Line_1459 = function(self) { return self.Line_1459; }
    static InnerRadius_1466 = function(self) { return self.InnerRadius_1466; }
    static OuterRadius_1473 = function(self) { return self.OuterRadius_1473; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Tube_102_Type);
    static Implements = [Value_16_Concept];
}
class ConeSegment_103_Type
{
    constructor(Line_1480, Radius1_1487, Radius2_1494)
    {
        // field initialization 
        this.Line_1480 = Line_1480;
        this.Radius1_1487 = Radius1_1487;
        this.Radius2_1494 = Radius2_1494;
        this.Default_2460 = ConeSegment_103_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Line_1480 = function(self) { return self.Line_1480; }
    static Radius1_1487 = function(self) { return self.Radius1_1487; }
    static Radius2_1494 = function(self) { return self.Radius2_1494; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ConeSegment_103_Type);
    static Implements = [Value_16_Concept];
}
class Box2D_104_Type
{
    constructor(Center_1501, Rotation_1508, Extent_1515)
    {
        // field initialization 
        this.Center_1501 = Center_1501;
        this.Rotation_1508 = Rotation_1508;
        this.Extent_1515 = Extent_1515;
        this.Default_2460 = Box2D_104_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Center_1501 = function(self) { return self.Center_1501; }
    static Rotation_1508 = function(self) { return self.Rotation_1508; }
    static Extent_1515 = function(self) { return self.Extent_1515; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Box2D_104_Type);
    static Implements = [Value_16_Concept];
}
class Box3D_105_Type
{
    constructor(Center_1522, Rotation_1529, Extent_1536)
    {
        // field initialization 
        this.Center_1522 = Center_1522;
        this.Rotation_1529 = Rotation_1529;
        this.Extent_1536 = Extent_1536;
        this.Default_2460 = Box3D_105_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Center_1522 = function(self) { return self.Center_1522; }
    static Rotation_1529 = function(self) { return self.Rotation_1529; }
    static Extent_1536 = function(self) { return self.Extent_1536; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Box3D_105_Type);
    static Implements = [Value_16_Concept];
}
class CubicBezierTriangle3D_106_Type
{
    constructor(A_1543, B_1550, C_1557, A2B_1564, AB2_1571, B2C_1578, BC2_1585, AC2_1592, A2C_1599, ABC_1606)
    {
        // field initialization 
        this.A_1543 = A_1543;
        this.B_1550 = B_1550;
        this.C_1557 = C_1557;
        this.A2B_1564 = A2B_1564;
        this.AB2_1571 = AB2_1571;
        this.B2C_1578 = B2C_1578;
        this.BC2_1585 = BC2_1585;
        this.AC2_1592 = AC2_1592;
        this.A2C_1599 = A2C_1599;
        this.ABC_1606 = ABC_1606;
        this.Default_2460 = CubicBezierTriangle3D_106_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static A_1543 = function(self) { return self.A_1543; }
    static B_1550 = function(self) { return self.B_1550; }
    static C_1557 = function(self) { return self.C_1557; }
    static A2B_1564 = function(self) { return self.A2B_1564; }
    static AB2_1571 = function(self) { return self.AB2_1571; }
    static B2C_1578 = function(self) { return self.B2C_1578; }
    static BC2_1585 = function(self) { return self.BC2_1585; }
    static AC2_1592 = function(self) { return self.AC2_1592; }
    static A2C_1599 = function(self) { return self.A2C_1599; }
    static ABC_1606 = function(self) { return self.ABC_1606; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(CubicBezierTriangle3D_106_Type);
    static Implements = [Value_16_Concept];
}
class CubicBezier2D_107_Type
{
    constructor(A_1613, B_1620, C_1627, D_1634)
    {
        // field initialization 
        this.A_1613 = A_1613;
        this.B_1620 = B_1620;
        this.C_1627 = C_1627;
        this.D_1634 = D_1634;
        this.Default_2460 = CubicBezier2D_107_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static A_1613 = function(self) { return self.A_1613; }
    static B_1620 = function(self) { return self.B_1620; }
    static C_1627 = function(self) { return self.C_1627; }
    static D_1634 = function(self) { return self.D_1634; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(CubicBezier2D_107_Type);
    static Implements = [Value_16_Concept];
}
class UV_108_Type
{
    constructor(U_1641, V_1648)
    {
        // field initialization 
        this.U_1641 = U_1641;
        this.V_1648 = V_1648;
        this.Count_2447 = UV_108_Type.Array_15_Concept.Count_2447;
        this.At_2452 = UV_108_Type.Array_15_Concept.At_2452;
        this.Default_2460 = UV_108_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = UV_108_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = UV_108_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = UV_108_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = UV_108_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = UV_108_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = UV_108_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = UV_108_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = UV_108_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = UV_108_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = UV_108_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = UV_108_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = UV_108_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = UV_108_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = UV_108_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = UV_108_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = UV_108_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = UV_108_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = UV_108_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = UV_108_Type.Numerical_19_Concept.MaxValue_2529;
        this.Count_2470 = UV_108_Type.Vector_17_Concept.Count_2470;
        this.At_2484 = UV_108_Type.Vector_17_Concept.At_2484;
    }
    // field accessors
    static U_1641 = function(self) { return self.U_1641; }
    static V_1648 = function(self) { return self.V_1648; }
    // implemented concepts 
    static Array_15_Concept = new Array_15_Concept(UV_108_Type);
    static Value_16_Concept = new Value_16_Concept(UV_108_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(UV_108_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(UV_108_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(UV_108_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(UV_108_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(UV_108_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(UV_108_Type);
    static Vector_17_Concept = new Vector_17_Concept(UV_108_Type);
    static Implements = [Array_15_Concept,Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept,Vector_17_Concept];
}
class UVW_109_Type
{
    constructor(U_1655, V_1662, W_1669)
    {
        // field initialization 
        this.U_1655 = U_1655;
        this.V_1662 = V_1662;
        this.W_1669 = W_1669;
        this.Count_2447 = UVW_109_Type.Array_15_Concept.Count_2447;
        this.At_2452 = UVW_109_Type.Array_15_Concept.At_2452;
        this.Default_2460 = UVW_109_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = UVW_109_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = UVW_109_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = UVW_109_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = UVW_109_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = UVW_109_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = UVW_109_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = UVW_109_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = UVW_109_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = UVW_109_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = UVW_109_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = UVW_109_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = UVW_109_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = UVW_109_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = UVW_109_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = UVW_109_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = UVW_109_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = UVW_109_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = UVW_109_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = UVW_109_Type.Numerical_19_Concept.MaxValue_2529;
        this.Count_2470 = UVW_109_Type.Vector_17_Concept.Count_2470;
        this.At_2484 = UVW_109_Type.Vector_17_Concept.At_2484;
    }
    // field accessors
    static U_1655 = function(self) { return self.U_1655; }
    static V_1662 = function(self) { return self.V_1662; }
    static W_1669 = function(self) { return self.W_1669; }
    // implemented concepts 
    static Array_15_Concept = new Array_15_Concept(UVW_109_Type);
    static Value_16_Concept = new Value_16_Concept(UVW_109_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(UVW_109_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(UVW_109_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(UVW_109_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(UVW_109_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(UVW_109_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(UVW_109_Type);
    static Vector_17_Concept = new Vector_17_Concept(UVW_109_Type);
    static Implements = [Array_15_Concept,Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept,Vector_17_Concept];
}
class CubicBezier3D_110_Type
{
    constructor(A_1676, B_1683, C_1690, D_1697)
    {
        // field initialization 
        this.A_1676 = A_1676;
        this.B_1683 = B_1683;
        this.C_1690 = C_1690;
        this.D_1697 = D_1697;
        this.Default_2460 = CubicBezier3D_110_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static A_1676 = function(self) { return self.A_1676; }
    static B_1683 = function(self) { return self.B_1683; }
    static C_1690 = function(self) { return self.C_1690; }
    static D_1697 = function(self) { return self.D_1697; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(CubicBezier3D_110_Type);
    static Implements = [Value_16_Concept];
}
class QuadraticBezier2D_111_Type
{
    constructor(A_1704, B_1711, C_1718)
    {
        // field initialization 
        this.A_1704 = A_1704;
        this.B_1711 = B_1711;
        this.C_1718 = C_1718;
        this.Default_2460 = QuadraticBezier2D_111_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static A_1704 = function(self) { return self.A_1704; }
    static B_1711 = function(self) { return self.B_1711; }
    static C_1718 = function(self) { return self.C_1718; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(QuadraticBezier2D_111_Type);
    static Implements = [Value_16_Concept];
}
class QuadraticBezier3D_112_Type
{
    constructor(A_1725, B_1732, C_1739)
    {
        // field initialization 
        this.A_1725 = A_1725;
        this.B_1732 = B_1732;
        this.C_1739 = C_1739;
        this.Default_2460 = QuadraticBezier3D_112_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static A_1725 = function(self) { return self.A_1725; }
    static B_1732 = function(self) { return self.B_1732; }
    static C_1739 = function(self) { return self.C_1739; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(QuadraticBezier3D_112_Type);
    static Implements = [Value_16_Concept];
}
class Area_113_Type
{
    constructor(MetersSquared_1746)
    {
        // field initialization 
        this.MetersSquared_1746 = MetersSquared_1746;
        this.Default_2460 = Area_113_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = Area_113_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Area_113_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Area_113_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Area_113_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Area_113_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = Area_113_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Area_113_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Area_113_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = Area_113_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static MetersSquared_1746 = function(self) { return self.MetersSquared_1746; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Area_113_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Area_113_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Area_113_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Area_113_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Area_113_Type);
    static Measure_18_Concept = new Measure_18_Concept(Area_113_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class Volume_114_Type
{
    constructor(MetersCubed_1753)
    {
        // field initialization 
        this.MetersCubed_1753 = MetersCubed_1753;
        this.Default_2460 = Volume_114_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = Volume_114_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Volume_114_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Volume_114_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Volume_114_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Volume_114_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = Volume_114_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Volume_114_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Volume_114_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = Volume_114_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static MetersCubed_1753 = function(self) { return self.MetersCubed_1753; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Volume_114_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Volume_114_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Volume_114_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Volume_114_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Volume_114_Type);
    static Measure_18_Concept = new Measure_18_Concept(Volume_114_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class Velocity_115_Type
{
    constructor(MetersPerSecond_1760)
    {
        // field initialization 
        this.MetersPerSecond_1760 = MetersPerSecond_1760;
        this.Default_2460 = Velocity_115_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = Velocity_115_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Velocity_115_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Velocity_115_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Velocity_115_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Velocity_115_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = Velocity_115_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Velocity_115_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Velocity_115_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = Velocity_115_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static MetersPerSecond_1760 = function(self) { return self.MetersPerSecond_1760; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Velocity_115_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Velocity_115_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Velocity_115_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Velocity_115_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Velocity_115_Type);
    static Measure_18_Concept = new Measure_18_Concept(Velocity_115_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class Acceleration_116_Type
{
    constructor(MetersPerSecondSquared_1767)
    {
        // field initialization 
        this.MetersPerSecondSquared_1767 = MetersPerSecondSquared_1767;
        this.Default_2460 = Acceleration_116_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = Acceleration_116_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Acceleration_116_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Acceleration_116_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Acceleration_116_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Acceleration_116_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = Acceleration_116_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Acceleration_116_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Acceleration_116_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = Acceleration_116_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static MetersPerSecondSquared_1767 = function(self) { return self.MetersPerSecondSquared_1767; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Acceleration_116_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Acceleration_116_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Acceleration_116_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Acceleration_116_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Acceleration_116_Type);
    static Measure_18_Concept = new Measure_18_Concept(Acceleration_116_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class Force_117_Type
{
    constructor(Newtons_1774)
    {
        // field initialization 
        this.Newtons_1774 = Newtons_1774;
        this.Default_2460 = Force_117_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = Force_117_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Force_117_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Force_117_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Force_117_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Force_117_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = Force_117_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Force_117_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Force_117_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = Force_117_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Newtons_1774 = function(self) { return self.Newtons_1774; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Force_117_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Force_117_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Force_117_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Force_117_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Force_117_Type);
    static Measure_18_Concept = new Measure_18_Concept(Force_117_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class Pressure_118_Type
{
    constructor(Pascals_1781)
    {
        // field initialization 
        this.Pascals_1781 = Pascals_1781;
        this.Default_2460 = Pressure_118_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = Pressure_118_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Pressure_118_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Pressure_118_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Pressure_118_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Pressure_118_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = Pressure_118_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Pressure_118_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Pressure_118_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = Pressure_118_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Pascals_1781 = function(self) { return self.Pascals_1781; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Pressure_118_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Pressure_118_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Pressure_118_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Pressure_118_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Pressure_118_Type);
    static Measure_18_Concept = new Measure_18_Concept(Pressure_118_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class Energy_119_Type
{
    constructor(Joules_1788)
    {
        // field initialization 
        this.Joules_1788 = Joules_1788;
        this.Default_2460 = Energy_119_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = Energy_119_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Energy_119_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Energy_119_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Energy_119_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Energy_119_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = Energy_119_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Energy_119_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Energy_119_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = Energy_119_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Joules_1788 = function(self) { return self.Joules_1788; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Energy_119_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Energy_119_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Energy_119_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Energy_119_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Energy_119_Type);
    static Measure_18_Concept = new Measure_18_Concept(Energy_119_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class Memory_120_Type
{
    constructor(Bytes_1795)
    {
        // field initialization 
        this.Bytes_1795 = Bytes_1795;
        this.Default_2460 = Memory_120_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = Memory_120_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Memory_120_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Memory_120_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Memory_120_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Memory_120_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = Memory_120_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Memory_120_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Memory_120_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = Memory_120_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Bytes_1795 = function(self) { return self.Bytes_1795; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Memory_120_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Memory_120_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Memory_120_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Memory_120_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Memory_120_Type);
    static Measure_18_Concept = new Measure_18_Concept(Memory_120_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class Frequency_121_Type
{
    constructor(Hertz_1802)
    {
        // field initialization 
        this.Hertz_1802 = Hertz_1802;
        this.Default_2460 = Frequency_121_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = Frequency_121_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Frequency_121_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Frequency_121_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Frequency_121_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Frequency_121_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = Frequency_121_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Frequency_121_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Frequency_121_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = Frequency_121_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Hertz_1802 = function(self) { return self.Hertz_1802; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Frequency_121_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Frequency_121_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Frequency_121_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Frequency_121_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Frequency_121_Type);
    static Measure_18_Concept = new Measure_18_Concept(Frequency_121_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class Loudness_122_Type
{
    constructor(Decibels_1809)
    {
        // field initialization 
        this.Decibels_1809 = Decibels_1809;
        this.Default_2460 = Loudness_122_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = Loudness_122_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Loudness_122_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Loudness_122_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Loudness_122_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Loudness_122_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = Loudness_122_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Loudness_122_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Loudness_122_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = Loudness_122_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Decibels_1809 = function(self) { return self.Decibels_1809; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Loudness_122_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Loudness_122_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Loudness_122_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Loudness_122_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Loudness_122_Type);
    static Measure_18_Concept = new Measure_18_Concept(Loudness_122_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class LuminousIntensity_123_Type
{
    constructor(Candelas_1816)
    {
        // field initialization 
        this.Candelas_1816 = Candelas_1816;
        this.Default_2460 = LuminousIntensity_123_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = LuminousIntensity_123_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = LuminousIntensity_123_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = LuminousIntensity_123_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = LuminousIntensity_123_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = LuminousIntensity_123_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = LuminousIntensity_123_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = LuminousIntensity_123_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = LuminousIntensity_123_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = LuminousIntensity_123_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Candelas_1816 = function(self) { return self.Candelas_1816; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(LuminousIntensity_123_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(LuminousIntensity_123_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(LuminousIntensity_123_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(LuminousIntensity_123_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(LuminousIntensity_123_Type);
    static Measure_18_Concept = new Measure_18_Concept(LuminousIntensity_123_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class ElectricPotential_124_Type
{
    constructor(Volts_1823)
    {
        // field initialization 
        this.Volts_1823 = Volts_1823;
        this.Default_2460 = ElectricPotential_124_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = ElectricPotential_124_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = ElectricPotential_124_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = ElectricPotential_124_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = ElectricPotential_124_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = ElectricPotential_124_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = ElectricPotential_124_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = ElectricPotential_124_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = ElectricPotential_124_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = ElectricPotential_124_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Volts_1823 = function(self) { return self.Volts_1823; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ElectricPotential_124_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(ElectricPotential_124_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(ElectricPotential_124_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(ElectricPotential_124_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(ElectricPotential_124_Type);
    static Measure_18_Concept = new Measure_18_Concept(ElectricPotential_124_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class ElectricCharge_125_Type
{
    constructor(Columbs_1830)
    {
        // field initialization 
        this.Columbs_1830 = Columbs_1830;
        this.Default_2460 = ElectricCharge_125_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = ElectricCharge_125_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = ElectricCharge_125_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = ElectricCharge_125_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = ElectricCharge_125_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = ElectricCharge_125_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = ElectricCharge_125_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = ElectricCharge_125_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = ElectricCharge_125_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = ElectricCharge_125_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Columbs_1830 = function(self) { return self.Columbs_1830; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ElectricCharge_125_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(ElectricCharge_125_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(ElectricCharge_125_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(ElectricCharge_125_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(ElectricCharge_125_Type);
    static Measure_18_Concept = new Measure_18_Concept(ElectricCharge_125_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class ElectricCurrent_126_Type
{
    constructor(Amperes_1837)
    {
        // field initialization 
        this.Amperes_1837 = Amperes_1837;
        this.Default_2460 = ElectricCurrent_126_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = ElectricCurrent_126_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = ElectricCurrent_126_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = ElectricCurrent_126_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = ElectricCurrent_126_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = ElectricCurrent_126_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = ElectricCurrent_126_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = ElectricCurrent_126_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = ElectricCurrent_126_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = ElectricCurrent_126_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Amperes_1837 = function(self) { return self.Amperes_1837; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ElectricCurrent_126_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(ElectricCurrent_126_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(ElectricCurrent_126_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(ElectricCurrent_126_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(ElectricCurrent_126_Type);
    static Measure_18_Concept = new Measure_18_Concept(ElectricCurrent_126_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class ElectricResistance_127_Type
{
    constructor(Ohms_1844)
    {
        // field initialization 
        this.Ohms_1844 = Ohms_1844;
        this.Default_2460 = ElectricResistance_127_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = ElectricResistance_127_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = ElectricResistance_127_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = ElectricResistance_127_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = ElectricResistance_127_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = ElectricResistance_127_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = ElectricResistance_127_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = ElectricResistance_127_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = ElectricResistance_127_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = ElectricResistance_127_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Ohms_1844 = function(self) { return self.Ohms_1844; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ElectricResistance_127_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(ElectricResistance_127_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(ElectricResistance_127_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(ElectricResistance_127_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(ElectricResistance_127_Type);
    static Measure_18_Concept = new Measure_18_Concept(ElectricResistance_127_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class Power_128_Type
{
    constructor(Watts_1851)
    {
        // field initialization 
        this.Watts_1851 = Watts_1851;
        this.Default_2460 = Power_128_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = Power_128_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Power_128_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Power_128_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Power_128_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Power_128_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = Power_128_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Power_128_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Power_128_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = Power_128_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static Watts_1851 = function(self) { return self.Watts_1851; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Power_128_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Power_128_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Power_128_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Power_128_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Power_128_Type);
    static Measure_18_Concept = new Measure_18_Concept(Power_128_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class Density_129_Type
{
    constructor(KilogramsPerMeterCubed_1858)
    {
        // field initialization 
        this.KilogramsPerMeterCubed_1858 = KilogramsPerMeterCubed_1858;
        this.Default_2460 = Density_129_Type.Value_16_Concept.Default_2460;
        this.Add_2670 = Density_129_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Density_129_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Density_129_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Density_129_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Density_129_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.Equals_2568 = Density_129_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Density_129_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Density_129_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Value_2496 = Density_129_Type.Measure_18_Concept.Value_2496;
    }
    // field accessors
    static KilogramsPerMeterCubed_1858 = function(self) { return self.KilogramsPerMeterCubed_1858; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Density_129_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Density_129_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Density_129_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Density_129_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Density_129_Type);
    static Measure_18_Concept = new Measure_18_Concept(Density_129_Type);
    static Implements = [Value_16_Concept,ScalarArithmetic_24_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,Measure_18_Concept];
}
class NormalDistribution_130_Type
{
    constructor(Mean_1865, StandardDeviation_1872)
    {
        // field initialization 
        this.Mean_1865 = Mean_1865;
        this.StandardDeviation_1872 = StandardDeviation_1872;
        this.Default_2460 = NormalDistribution_130_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Mean_1865 = function(self) { return self.Mean_1865; }
    static StandardDeviation_1872 = function(self) { return self.StandardDeviation_1872; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(NormalDistribution_130_Type);
    static Implements = [Value_16_Concept];
}
class PoissonDistribution_131_Type
{
    constructor(Expected_1879, Occurrences_1886)
    {
        // field initialization 
        this.Expected_1879 = Expected_1879;
        this.Occurrences_1886 = Occurrences_1886;
        this.Default_2460 = PoissonDistribution_131_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Expected_1879 = function(self) { return self.Expected_1879; }
    static Occurrences_1886 = function(self) { return self.Occurrences_1886; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(PoissonDistribution_131_Type);
    static Implements = [Value_16_Concept];
}
class BernoulliDistribution_132_Type
{
    constructor(P_1893)
    {
        // field initialization 
        this.P_1893 = P_1893;
        this.Default_2460 = BernoulliDistribution_132_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static P_1893 = function(self) { return self.P_1893; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(BernoulliDistribution_132_Type);
    static Implements = [Value_16_Concept];
}
class Probability_133_Type
{
    constructor(Value_1900)
    {
        // field initialization 
        this.Value_1900 = Value_1900;
        this.Default_2460 = Probability_133_Type.Value_16_Concept.Default_2460;
        this.Add_2585 = Probability_133_Type.Arithmetic_23_Concept.Add_2585;
        this.Negative_2595 = Probability_133_Type.Arithmetic_23_Concept.Negative_2595;
        this.Reciprocal_2605 = Probability_133_Type.Arithmetic_23_Concept.Reciprocal_2605;
        this.Multiply_2622 = Probability_133_Type.Arithmetic_23_Concept.Multiply_2622;
        this.Divide_2639 = Probability_133_Type.Arithmetic_23_Concept.Divide_2639;
        this.Modulo_2656 = Probability_133_Type.Arithmetic_23_Concept.Modulo_2656;
        this.Equals_2568 = Probability_133_Type.Equatable_22_Concept.Equals_2568;
        this.Compare_2548 = Probability_133_Type.Comparable_21_Concept.Compare_2548;
        this.Magnitude_2545 = Probability_133_Type.Magnitudinal_20_Concept.Magnitude_2545;
        this.Add_2670 = Probability_133_Type.ScalarArithmetic_24_Concept.Add_2670;
        this.Subtract_2684 = Probability_133_Type.ScalarArithmetic_24_Concept.Subtract_2684;
        this.Multiply_2698 = Probability_133_Type.ScalarArithmetic_24_Concept.Multiply_2698;
        this.Divide_2712 = Probability_133_Type.ScalarArithmetic_24_Concept.Divide_2712;
        this.Modulo_2726 = Probability_133_Type.ScalarArithmetic_24_Concept.Modulo_2726;
        this.FieldTypes_2497 = Probability_133_Type.Numerical_19_Concept.FieldTypes_2497;
        this.Zero_2505 = Probability_133_Type.Numerical_19_Concept.Zero_2505;
        this.One_2513 = Probability_133_Type.Numerical_19_Concept.One_2513;
        this.MinValue_2521 = Probability_133_Type.Numerical_19_Concept.MinValue_2521;
        this.MaxValue_2529 = Probability_133_Type.Numerical_19_Concept.MaxValue_2529;
    }
    // field accessors
    static Value_1900 = function(self) { return self.Value_1900; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Probability_133_Type);
    static Arithmetic_23_Concept = new Arithmetic_23_Concept(Probability_133_Type);
    static Equatable_22_Concept = new Equatable_22_Concept(Probability_133_Type);
    static Comparable_21_Concept = new Comparable_21_Concept(Probability_133_Type);
    static Magnitudinal_20_Concept = new Magnitudinal_20_Concept(Probability_133_Type);
    static ScalarArithmetic_24_Concept = new ScalarArithmetic_24_Concept(Probability_133_Type);
    static Numerical_19_Concept = new Numerical_19_Concept(Probability_133_Type);
    static Implements = [Value_16_Concept,Arithmetic_23_Concept,Equatable_22_Concept,Comparable_21_Concept,Magnitudinal_20_Concept,ScalarArithmetic_24_Concept,Numerical_19_Concept];
}
class BinomialDistribution_134_Type
{
    constructor(Trials_1907, P_1914)
    {
        // field initialization 
        this.Trials_1907 = Trials_1907;
        this.P_1914 = P_1914;
        this.Default_2460 = BinomialDistribution_134_Type.Value_16_Concept.Default_2460;
    }
    // field accessors
    static Trials_1907 = function(self) { return self.Trials_1907; }
    static P_1914 = function(self) { return self.P_1914; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(BinomialDistribution_134_Type);
    static Implements = [Value_16_Concept];
}

// This is appended to every JavaScript program generated from Plato