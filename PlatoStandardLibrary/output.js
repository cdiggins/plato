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




class Intrinsics_12_Library
{
    static Cos_3043 = function (x_3042/* : Angle_83 */) /* : Number_29 */{ return null; };
    static Sin_3046 = function (x_3045/* : Angle_83 */) /* : Number_29 */{ return null; };
    static Tan_3049 = function (x_3048/* : Angle_83 */) /* : Number_29 */{ return null; };
    static Acos_3052 = function (x_3051/* : Number_29 */) /* : Angle_83 */{ return null; };
    static Asin_3055 = function (x_3054/* : Number_29 */) /* : Angle_83 */{ return null; };
    static Atan_3058 = function (x_3057/* : Number_29 */) /* : Angle_83 */{ return null; };
    static Pow_3063 = function (x_3060/* : Number_29 */, y_3062/* : Number_29 */) /* : Number_29 */{ return null; };
    static Log_3068 = function (x_3065/* : Number_29 */, y_3067/* : Number_29 */) /* : Number_29 */{ return null; };
    static NaturalLog_3071 = function (x_3070/* : Number_29 */) /* : Number_29 */{ return null; };
    static NaturalPower_3074 = function (x_3073/* : Number_29 */) /* : Number_29 */{ return null; };
    static Interpolate_3077 = function (xs_3076/* : Array_14 */) /* : String_7 */{ return null; };
    static Throw_3080 = function (x_3079/* : Any_13 */) /* : Any_13 */{ return null; };
    static TypeOf_3083 = function (x_3082/* : Any_13 */) /* : Type_11 */{ return null; };
    static Add_3088 = function (x_3085/* : Number_29 */, y_3087/* : Number_29 */) /* : Number_29 */{ return null; };
    static Subtract_3093 = function (x_3090/* : Number_29 */, y_3092/* : Number_29 */) /* : Number_29 */{ return null; };
    static Divide_3098 = function (x_3095/* : Number_29 */, y_3097/* : Number_29 */) /* : Number_29 */{ return null; };
    static Multiply_3103 = function (x_3100/* : Number_29 */, y_3102/* : Number_29 */) /* : Number_29 */{ return null; };
    static Modulo_3108 = function (x_3105/* : Number_29 */, y_3107/* : Number_29 */) /* : Number_29 */{ return null; };
    static Negative_3111 = function (x_3110/* : Number_29 */) /* : Number_29 */{ return null; };
    static Add_3116 = function (x_3113/* : Integer_26 */, y_3115/* : Integer_26 */) /* : Integer_26 */{ return null; };
    static Subtract_3121 = function (x_3118/* : Integer_26 */, y_3120/* : Integer_26 */) /* : Integer_26 */{ return null; };
    static Divide_3126 = function (x_3123/* : Integer_26 */, y_3125/* : Integer_26 */) /* : Integer_26 */{ return null; };
    static Multiply_3131 = function (x_3128/* : Integer_26 */, y_3130/* : Integer_26 */) /* : Integer_26 */{ return null; };
    static Modulo_3136 = function (x_3133/* : Integer_26 */, y_3135/* : Integer_26 */) /* : Integer_26 */{ return null; };
    static Negative_3139 = function (x_3138/* : Integer_26 */) /* : Integer_26 */{ return null; };
    static And_3144 = function (x_3141/* : Boolean_24 */, y_3143/* : Boolean_24 */) /* : Boolean_24 */{ return null; };
    static Or_3149 = function (x_3146/* : Boolean_24 */, y_3148/* : Boolean_24 */) /* : Boolean_24 */{ return null; };
    static Not_3152 = function (x_3151/* : Boolean_24 */) /* : Boolean_24 */{ return null; };
}
class Array_134_Library
{
    static Map_3905 = function (xs_3881/* : Array_14 */, f_3883/* : Function_3 */) /* : Array_14 */{ return Tuple_1/* : UnknownType */(Count_362/* : UnknownType */(xs_3881/* : UnknownType */)/* : UnknownType */, function (i_3890/* : UnknownType */) /* : Lambda_2 */{ return f_3883/* : UnknownType */(At_368/* : UnknownType */(xs_3881/* : UnknownType */, i_3890/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */; };
    static Reverse_3942 = function (xs_3907/* : Array_14 */) /* : Array_14 */{ return Tuple_1/* : UnknownType */(Count_362/* : UnknownType */(xs_3907/* : UnknownType */)/* : UnknownType */, function (i_3914/* : UnknownType */) /* : Lambda_2 */{ return f_3883/* : UnknownType */(At_368/* : UnknownType */(xs_3907/* : UnknownType */, Subtract_234/* : UnknownType */(Count_362/* : UnknownType */(xs_3907/* : UnknownType */)/* : UnknownType */, Subtract_234/* : UnknownType */(1/* : Integer_9 */, i_3914/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */; };
    static Zip_3975 = function (xs_3944/* : Array_14 */, ys_3946/* : Array_14 */, f_3948/* : Function_3 */) /* : Array_14 */{ return Tuple_1/* : UnknownType */(Count_362/* : UnknownType */(xs_3944/* : UnknownType */)/* : UnknownType */, function (i_3955/* : UnknownType */) /* : Lambda_2 */{ return f_3948/* : UnknownType */(At_368/* : UnknownType */(i_3955/* : UnknownType */)/* : UnknownType */, At_368/* : UnknownType */(ys_3946/* : UnknownType */, i_3955/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */; };
    static Zip_4017 = function (xs_3977/* : Array_14 */, ys_3979/* : Array_14 */, zs_3981/* : Array_14 */, f_3983/* : Function_3 */) /* : Array_14 */{ return Tuple_1/* : UnknownType */(Count_362/* : UnknownType */(xs_3977/* : UnknownType */)/* : UnknownType */, function (i_3990/* : UnknownType */) /* : Lambda_2 */{ return f_3983/* : UnknownType */(At_368/* : UnknownType */(i_3990/* : UnknownType */)/* : UnknownType */, At_368/* : UnknownType */(ys_3979/* : UnknownType */, i_3990/* : UnknownType */)/* : UnknownType */, At_368/* : UnknownType */(zs_3981/* : UnknownType */, i_3990/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */; };
    static Skip_4045 = function (xs_4019/* : Array_14 */, n_4021/* : Count_27 */) /* : Array_14 */{ return Tuple_1/* : UnknownType */(Subtract_234/* : UnknownType */(Count_362/* : UnknownType */, n_4021/* : UnknownType */)/* : UnknownType */, function (i_4030/* : UnknownType */) /* : Lambda_2 */{ return At_368/* : UnknownType */(Subtract_234/* : UnknownType */(i_4030/* : UnknownType */, n_4021/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */; };
    static Take_4063 = function (xs_4047/* : Array_14 */, n_4049/* : Count_27 */) /* : Array_14 */{ return Tuple_1/* : UnknownType */(n_4049/* : UnknownType */, function (i_4053/* : UnknownType */) /* : Lambda_2 */{ return At_368/* : UnknownType */(i_4053/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */; };
    static Aggregate_4088 = function (xs_4065/* : Array_14 */, init_4067/* : Any_13 */, f_4069/* : Function_3 */) /* : Any_13 */{ return IsEmpty_2218/* : UnknownType */(xs_4065/* : UnknownType */)/* : UnknownType */
        ? init_4067/* : Any_13 */
        : f_4069/* : Function_3 */(init_4067/* : UnknownType */, f_4069/* : UnknownType */(Rest_2212/* : UnknownType */(xs_4065/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Function_3 */
    ; };
    static Rest_4097 = function (xs_4090/* : Array_14 */) /* : Array_14 */{ return Skip_2186/* : UnknownType */(xs_4090/* : UnknownType */, 1/* : Integer_9 */)/* : UnknownType */; };
    static IsEmpty_4109 = function (xs_4099/* : Array_14 */) /* : Boolean_24 */{ return Equals_447/* : UnknownType */(Count_362/* : UnknownType */(xs_4099/* : UnknownType */)/* : UnknownType */, 0/* : Integer_9 */)/* : UnknownType */; };
    static First_4118 = function (xs_4111/* : Array_14 */) /* : Any_13 */{ return At_368/* : UnknownType */(xs_4111/* : UnknownType */, 0/* : Integer_9 */)/* : UnknownType */; };
    static Last_4135 = function (xs_4120/* : Array_14 */) /* : Any_13 */{ return At_368/* : UnknownType */(xs_4120/* : UnknownType */, Subtract_234/* : UnknownType */(Count_362/* : UnknownType */(xs_4120/* : UnknownType */)/* : UnknownType */, 1/* : Integer_9 */)/* : UnknownType */)/* : UnknownType */; };
    static Slice_4153 = function (xs_4137/* : Array_14 */, from_4139/* : Index_28 */, count_4141/* : Count_27 */) /* : Array_14 */{ return Take_2194/* : UnknownType */(Skip_2186/* : UnknownType */(xs_4137/* : UnknownType */, from_4139/* : UnknownType */)/* : UnknownType */, count_4141/* : UnknownType */)/* : UnknownType */; };
    static Join_4199 = function (xs_4155/* : Array_14 */, sep_4157/* : String_7 */) /* : String_7 */{ return IsEmpty_2218/* : UnknownType */(xs_4155/* : UnknownType */)/* : UnknownType */
        ? ""/* : String_7 */
        : Add_226/* : UnknownType */(ToString_2420/* : UnknownType */(First_2224/* : UnknownType */(xs_4155/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Aggregate_2202/* : UnknownType */(Rest_2212/* : UnknownType */(xs_4155/* : UnknownType */)/* : UnknownType */, ""/* : String_7 */, function (acc_4179/* : UnknownType */, cur_4181/* : UnknownType */) /* : Lambda_2 */{ return Interpolate_208/* : UnknownType */(acc_4179/* : UnknownType */, sep_4157/* : UnknownType */, cur_4181/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */)/* : UnknownType */
    ; };
    static All_4228 = function (xs_4201/* : Array_14 */, f_4203/* : Function_3 */) /* : Boolean_24 */{ return IsEmpty_2218/* : UnknownType */(xs_4201/* : UnknownType */)/* : UnknownType */
        ? True/* : Boolean_8 */
        : And_318/* : UnknownType */(f_4203/* : UnknownType */(First_2224/* : UnknownType */(xs_4201/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, f_4203/* : UnknownType */(Rest_2212/* : UnknownType */(xs_4201/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
    ; };
}
class Interval_135_Library
{
    static Size_4243 = function (x_4230/* : Interval_25 */) /* : Numerical_18 */{ return Subtract_234/* : UnknownType */(Max_569/* : UnknownType */(x_4230/* : UnknownType */)/* : UnknownType */, Min_563/* : UnknownType */(x_4230/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static IsEmpty_4258 = function (x_4245/* : Interval_25 */) /* : Boolean_24 */{ return GreaterThanOrEquals_2788/* : UnknownType */(Min_563/* : UnknownType */(x_4245/* : UnknownType */)/* : UnknownType */, Max_569/* : UnknownType */(x_4245/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Lerp_4290 = function (x_4260/* : Interval_25 */, amount_4262/* : Unit_30 */) /* : Numerical_18 */{ return Multiply_250/* : UnknownType */(Min_563/* : UnknownType */(x_4260/* : UnknownType */)/* : UnknownType */, Add_226/* : UnknownType */(Subtract_234/* : UnknownType */(1/* : Float_10 */, amount_4262/* : UnknownType */)/* : UnknownType */, Multiply_250/* : UnknownType */(Max_569/* : UnknownType */(x_4260/* : UnknownType */)/* : UnknownType */, amount_4262/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static InverseLerp_4312 = function (x_4292/* : Interval_25 */, value_4294/* : Numerical_18 */) /* : Unit_30 */{ return Divide_242/* : UnknownType */(Subtract_234/* : UnknownType */(value_4294/* : UnknownType */, Min_563/* : UnknownType */(x_4292/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Size_1443/* : UnknownType */(x_4292/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Negate_4333 = function (x_4314/* : Interval_25 */) /* : Interval_25 */{ return Tuple_1/* : UnknownType */(Negative_266/* : UnknownType */(Max_569/* : UnknownType */(x_4314/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Negative_266/* : UnknownType */(Min_563/* : UnknownType */(x_4314/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Reverse_4348 = function (x_4335/* : Interval_25 */) /* : Interval_25 */{ return Tuple_1/* : UnknownType */(Max_569/* : UnknownType */(x_4335/* : UnknownType */)/* : UnknownType */, Min_563/* : UnknownType */(x_4335/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Center_4357 = function (x_4350/* : Interval_25 */) /* : Numerical_18 */{ return Lerp_2274/* : UnknownType */(x_4350/* : UnknownType */, 0.5/* : Float_10 */)/* : UnknownType */; };
    static Contains_4384 = function (x_4359/* : Interval_25 */, value_4361/* : Numerical_18 */) /* : Boolean_24 */{ return LessThanOrEquals_2772/* : UnknownType */(Min_563/* : UnknownType */(x_4359/* : UnknownType */)/* : UnknownType */, And_318/* : UnknownType */(value_4361/* : UnknownType */, LessThanOrEquals_2772/* : UnknownType */(value_4361/* : UnknownType */, Max_569/* : UnknownType */(x_4359/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Contains_4414 = function (x_4386/* : Interval_25 */, other_4388/* : Interval_25 */) /* : Boolean_24 */{ return LessThanOrEquals_2772/* : UnknownType */(Min_563/* : UnknownType */(x_4386/* : UnknownType */)/* : UnknownType */, And_318/* : UnknownType */(Min_563/* : UnknownType */(other_4388/* : UnknownType */)/* : UnknownType */, GreaterThanOrEquals_2788/* : UnknownType */(Max_569/* : UnknownType */, Max_569/* : UnknownType */(other_4388/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Overlaps_4431 = function (x_4416/* : Interval_25 */, y_4418/* : Interval_25 */) /* : Boolean_24 */{ return Not_334/* : UnknownType */(IsEmpty_2218/* : UnknownType */(Clamp_2396/* : UnknownType */(x_4416/* : UnknownType */, y_4418/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Split_4452 = function (x_4433/* : Interval_25 */, t_4435/* : Unit_30 */) /* : Tuple_6 */{ return Tuple_1/* : UnknownType */(Left_2346/* : UnknownType */(x_4433/* : UnknownType */, t_4435/* : UnknownType */)/* : UnknownType */, Right_2354/* : UnknownType */(x_4433/* : UnknownType */, t_4435/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Split_4461 = function (x_4454/* : Interval_25 */) /* : Tuple_6 */{ return Split_2332/* : UnknownType */(x_4454/* : UnknownType */, 0.5/* : Float_10 */)/* : UnknownType */; };
    static Left_4480 = function (x_4463/* : Interval_25 */, t_4465/* : Unit_30 */) /* : Interval_25 */{ return Tuple_1/* : UnknownType */(Min_563/* : UnknownType */(x_4463/* : UnknownType */)/* : UnknownType */, Lerp_2274/* : UnknownType */(x_4463/* : UnknownType */, t_4465/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Right_4499 = function (x_4482/* : Interval_25 */, t_4484/* : Unit_30 */) /* : Interval_25 */{ return Tuple_1/* : UnknownType */(Lerp_2274/* : UnknownType */(x_4482/* : UnknownType */, t_4484/* : UnknownType */)/* : UnknownType */, Max_569/* : UnknownType */(x_4482/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static MoveTo_4518 = function (x_4501/* : Interval_25 */, v_4503/* : Numerical_18 */) /* : Interval_25 */{ return Tuple_1/* : UnknownType */(v_4503/* : UnknownType */, Add_226/* : UnknownType */(v_4503/* : UnknownType */, Size_1443/* : UnknownType */(x_4501/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static LeftHalf_4527 = function (x_4520/* : Interval_25 */) /* : Interval_25 */{ return Left_2346/* : UnknownType */(x_4520/* : UnknownType */, 0.5/* : Float_10 */)/* : UnknownType */; };
    static RightHalf_4536 = function (x_4529/* : Interval_25 */) /* : Interval_25 */{ return Right_2354/* : UnknownType */(x_4529/* : UnknownType */, 0.5/* : Float_10 */)/* : UnknownType */; };
    static HalfSize_4546 = function (x_4538/* : Interval_25 */) /* : Numerical_18 */{ return Half_2556/* : UnknownType */(Size_1443/* : UnknownType */(x_4538/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Recenter_4573 = function (x_4548/* : Interval_25 */, c_4550/* : Numerical_18 */) /* : Interval_25 */{ return Tuple_1/* : UnknownType */(Subtract_234/* : UnknownType */(c_4550/* : UnknownType */, HalfSize_2382/* : UnknownType */(x_4548/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Add_226/* : UnknownType */(c_4550/* : UnknownType */, HalfSize_2382/* : UnknownType */(x_4548/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Clamp_4600 = function (x_4575/* : Interval_25 */, y_4577/* : Interval_25 */) /* : Interval_25 */{ return Tuple_1/* : UnknownType */(Clamp_2396/* : UnknownType */(x_4575/* : UnknownType */, Min_563/* : UnknownType */(y_4577/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Clamp_2396/* : UnknownType */(x_4575/* : UnknownType */, Max_569/* : UnknownType */(y_4577/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Clamp_4634 = function (x_4602/* : Interval_25 */, value_4604/* : Numerical_18 */) /* : Numerical_18 */{ return LessThan_2764/* : UnknownType */(value_4604/* : UnknownType */, Min_563/* : UnknownType */(x_4602/* : UnknownType */)/* : UnknownType */
        ? Min_563/* : UnknownType */(x_4602/* : UnknownType */)/* : UnknownType */
        : GreaterThan_2780/* : UnknownType */(value_4604/* : UnknownType */, Max_569/* : UnknownType */(x_4602/* : UnknownType */)/* : UnknownType */
            ? Max_569/* : UnknownType */(x_4602/* : UnknownType */)/* : UnknownType */
            : value_4604/* : UnknownType */
        )/* : UnknownType */
    )/* : UnknownType */; };
    static Within_4661 = function (x_4636/* : Interval_25 */, value_4638/* : Numerical_18 */) /* : Boolean_24 */{ return GreaterThanOrEquals_2788/* : UnknownType */(value_4638/* : UnknownType */, And_318/* : UnknownType */(Min_563/* : UnknownType */(x_4636/* : UnknownType */)/* : UnknownType */, LessThanOrEquals_2772/* : UnknownType */(value_4638/* : UnknownType */, Max_569/* : UnknownType */(x_4636/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Value_136_Library
{
    static ToString_4673 = function (x_4663/* : Value_15 */) /* : String_7 */{ return Join_2246/* : UnknownType */(FieldValues_348/* : UnknownType */(x_4663/* : UnknownType */)/* : UnknownType */, ", "/* : String_7 */)/* : UnknownType */; };
}
class Vector_137_Library
{
    static Sum_4684 = function (v_4675/* : Vector_16 */) /* : Number_29 */{ return Aggregate_2202/* : UnknownType */(v_4675/* : UnknownType */, 0/* : Integer_9 */, Add_226/* : UnknownType */)/* : UnknownType */; };
    static SumSquares_4698 = function (v_4686/* : Vector_16 */) /* : Number_29 */{ return Aggregate_2202/* : UnknownType */(Square_2470/* : UnknownType */(v_4686/* : UnknownType */)/* : UnknownType */, 0/* : Integer_9 */, Add_226/* : UnknownType */)/* : UnknownType */; };
    static LengthSquared_4705 = function (v_4700/* : Vector_16 */) /* : Number_29 */{ return SumSquares_2432/* : UnknownType */(v_4700/* : UnknownType */)/* : UnknownType */; };
    static Length_4715 = function (v_4707/* : Vector_16 */) /* : Number_29 */{ return SquareRoot_2464/* : UnknownType */(LengthSquared_2438/* : UnknownType */(v_4707/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Dot_4729 = function (v1_4717/* : Vector_16 */, v2_4719/* : Vector_16 */) /* : Number_29 */{ return Sum_2426/* : UnknownType */(Multiply_250/* : UnknownType */(v1_4717/* : UnknownType */, v2_4719/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Normal_4741 = function (v_4731/* : Vector_16 */) /* : Vector_16 */{ return Divide_242/* : UnknownType */(v_4731/* : UnknownType */, Length_2444/* : UnknownType */(v_4731/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Numerical_138_Library
{
    static SquareRoot_4750 = function (x_4743/* : Numerical_18 */) /* : Numerical_18 */{ return Pow_180/* : UnknownType */(x_4743/* : UnknownType */, 0.5/* : Float_10 */)/* : UnknownType */; };
    static Square_4759 = function (x_4752/* : Numerical_18 */) /* : Numerical_18 */{ return Multiply_250/* : UnknownType */(x_4752/* : UnknownType */, x_4752/* : UnknownType */)/* : UnknownType */; };
    static Clamp_4770 = function (x_4761/* : Numerical_18 */, i_4763/* : Interval_25 */) /* : Numerical_18 */{ return Clamp_2396/* : UnknownType */(i_4763/* : UnknownType */, x_4761/* : UnknownType */)/* : UnknownType */; };
    static Clamp_4784 = function (x_4772/* : Numerical_18 */) /* : Numerical_18 */{ return Clamp_2396/* : UnknownType */(x_4772/* : UnknownType */, Tuple_1/* : UnknownType */(0/* : Integer_9 */, 1/* : Integer_9 */)/* : UnknownType */)/* : UnknownType */; };
    static PlusOne_4796 = function (x_4786/* : Numerical_18 */) /* : Numerical_18 */{ return Add_226/* : UnknownType */(x_4786/* : UnknownType */, One_417/* : UnknownType */(x_4786/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static MinusOne_4808 = function (x_4798/* : Numerical_18 */) /* : Numerical_18 */{ return Subtract_234/* : UnknownType */(x_4798/* : UnknownType */, One_417/* : UnknownType */(x_4798/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static FromOne_4820 = function (x_4810/* : Numerical_18 */) /* : Numerical_18 */{ return Subtract_234/* : UnknownType */(One_417/* : UnknownType */(x_4810/* : UnknownType */)/* : UnknownType */, x_4810/* : UnknownType */)/* : UnknownType */; };
    static IsPositive_4829 = function (x_4822/* : Numerical_18 */) /* : Boolean_24 */{ return GreaterThanOrEquals_2788/* : UnknownType */(x_4822/* : UnknownType */, 0/* : Integer_9 */)/* : UnknownType */; };
    static GtZ_4838 = function (x_4831/* : Numerical_18 */) /* : Boolean_24 */{ return GreaterThan_2780/* : UnknownType */(x_4831/* : UnknownType */, 0/* : Integer_9 */)/* : UnknownType */; };
    static LtZ_4847 = function (x_4840/* : Numerical_18 */) /* : Boolean_24 */{ return LessThan_2764/* : UnknownType */(x_4840/* : UnknownType */, 0/* : Integer_9 */)/* : UnknownType */; };
    static GtEqZ_4856 = function (x_4849/* : Numerical_18 */) /* : Boolean_24 */{ return GreaterThanOrEquals_2788/* : UnknownType */(x_4849/* : UnknownType */, 0/* : Integer_9 */)/* : UnknownType */; };
    static LtEqZ_4865 = function (x_4858/* : Numerical_18 */) /* : Boolean_24 */{ return LessThanOrEquals_2772/* : UnknownType */(x_4858/* : UnknownType */, 0/* : Integer_9 */)/* : UnknownType */; };
    static IsNegative_4874 = function (x_4867/* : Numerical_18 */) /* : Boolean_24 */{ return LessThan_2764/* : UnknownType */(x_4867/* : UnknownType */, 0/* : Integer_9 */)/* : UnknownType */; };
    static Sign_4902 = function (x_4876/* : Numerical_18 */) /* : Numerical_18 */{ return LtZ_2520/* : UnknownType */(x_4876/* : UnknownType */)/* : UnknownType */
        ? Negative_266/* : UnknownType */(One_417/* : UnknownType */(x_4876/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
        : GtZ_2514/* : UnknownType */(x_4876/* : UnknownType */)/* : UnknownType */
            ? One_417/* : UnknownType */(x_4876/* : UnknownType */)/* : UnknownType */
            : Zero_411/* : UnknownType */(x_4876/* : UnknownType */)/* : UnknownType */

    ; };
    static Abs_4915 = function (x_4904/* : Numerical_18 */) /* : Numerical_18 */{ return LtZ_2520/* : UnknownType */(x_4904/* : UnknownType */)/* : UnknownType */
        ? Negative_266/* : UnknownType */(x_4904/* : UnknownType */)/* : UnknownType */
        : x_4904/* : Numerical_18 */
    ; };
    static Half_4924 = function (x_4917/* : Numerical_18 */) /* : Numerical_18 */{ return Divide_242/* : UnknownType */(x_4917/* : UnknownType */, 2/* : Integer_9 */)/* : UnknownType */; };
    static Third_4933 = function (x_4926/* : Numerical_18 */) /* : Numerical_18 */{ return Divide_242/* : UnknownType */(x_4926/* : UnknownType */, 3/* : Integer_9 */)/* : UnknownType */; };
    static Quarter_4942 = function (x_4935/* : Numerical_18 */) /* : Numerical_18 */{ return Divide_242/* : UnknownType */(x_4935/* : UnknownType */, 4/* : Integer_9 */)/* : UnknownType */; };
    static Fifth_4951 = function (x_4944/* : Numerical_18 */) /* : Numerical_18 */{ return Divide_242/* : UnknownType */(x_4944/* : UnknownType */, 5/* : Integer_9 */)/* : UnknownType */; };
    static Sixth_4960 = function (x_4953/* : Numerical_18 */) /* : Numerical_18 */{ return Divide_242/* : UnknownType */(x_4953/* : UnknownType */, 6/* : Integer_9 */)/* : UnknownType */; };
    static Seventh_4969 = function (x_4962/* : Numerical_18 */) /* : Numerical_18 */{ return Divide_242/* : UnknownType */(x_4962/* : UnknownType */, 7/* : Integer_9 */)/* : UnknownType */; };
    static Eighth_4978 = function (x_4971/* : Numerical_18 */) /* : Numerical_18 */{ return Divide_242/* : UnknownType */(x_4971/* : UnknownType */, 8/* : Integer_9 */)/* : UnknownType */; };
    static Ninth_4987 = function (x_4980/* : Numerical_18 */) /* : Numerical_18 */{ return Divide_242/* : UnknownType */(x_4980/* : UnknownType */, 9/* : Integer_9 */)/* : UnknownType */; };
    static Tenth_4996 = function (x_4989/* : Numerical_18 */) /* : Numerical_18 */{ return Divide_242/* : UnknownType */(x_4989/* : UnknownType */, 10/* : Integer_9 */)/* : UnknownType */; };
    static Sixteenth_5005 = function (x_4998/* : Numerical_18 */) /* : Numerical_18 */{ return Divide_242/* : UnknownType */(x_4998/* : UnknownType */, 16/* : Integer_9 */)/* : UnknownType */; };
    static Hundredth_5014 = function (x_5007/* : Numerical_18 */) /* : Numerical_18 */{ return Divide_242/* : UnknownType */(x_5007/* : UnknownType */, 100/* : Integer_9 */)/* : UnknownType */; };
    static Thousandth_5023 = function (x_5016/* : Numerical_18 */) /* : Numerical_18 */{ return Divide_242/* : UnknownType */(x_5016/* : UnknownType */, 1000/* : Integer_9 */)/* : UnknownType */; };
    static Millionth_5037 = function (x_5025/* : Numerical_18 */) /* : Numerical_18 */{ return Divide_242/* : UnknownType */(x_5025/* : UnknownType */, Divide_242/* : UnknownType */(1000/* : Integer_9 */, 1000/* : Integer_9 */)/* : UnknownType */)/* : UnknownType */; };
    static Billionth_5056 = function (x_5039/* : Numerical_18 */) /* : Numerical_18 */{ return Divide_242/* : UnknownType */(x_5039/* : UnknownType */, Divide_242/* : UnknownType */(1000/* : Integer_9 */, Divide_242/* : UnknownType */(1000/* : Integer_9 */, 1000/* : Integer_9 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Hundred_5065 = function (x_5058/* : Numerical_18 */) /* : Numerical_18 */{ return Multiply_250/* : UnknownType */(x_5058/* : UnknownType */, 100/* : Integer_9 */)/* : UnknownType */; };
    static Thousand_5074 = function (x_5067/* : Numerical_18 */) /* : Numerical_18 */{ return Multiply_250/* : UnknownType */(x_5067/* : UnknownType */, 1000/* : Integer_9 */)/* : UnknownType */; };
    static Million_5088 = function (x_5076/* : Numerical_18 */) /* : Numerical_18 */{ return Multiply_250/* : UnknownType */(x_5076/* : UnknownType */, Multiply_250/* : UnknownType */(1000/* : Integer_9 */, 1000/* : Integer_9 */)/* : UnknownType */)/* : UnknownType */; };
    static Billion_5107 = function (x_5090/* : Numerical_18 */) /* : Numerical_18 */{ return Multiply_250/* : UnknownType */(x_5090/* : UnknownType */, Multiply_250/* : UnknownType */(1000/* : Integer_9 */, Multiply_250/* : UnknownType */(1000/* : Integer_9 */, 1000/* : Integer_9 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Twice_5116 = function (x_5109/* : Numerical_18 */) /* : Numerical_18 */{ return Multiply_250/* : UnknownType */(x_5109/* : UnknownType */, 2/* : Integer_9 */)/* : UnknownType */; };
    static Thrice_5125 = function (x_5118/* : Numerical_18 */) /* : Numerical_18 */{ return Multiply_250/* : UnknownType */(x_5118/* : UnknownType */, 3/* : Integer_9 */)/* : UnknownType */; };
    static SmoothStep_5145 = function (x_5127/* : Numerical_18 */) /* : Numerical_18 */{ return Multiply_250/* : UnknownType */(Square_2470/* : UnknownType */(x_5127/* : UnknownType */)/* : UnknownType */, Subtract_234/* : UnknownType */(3/* : Integer_9 */, Twice_2664/* : UnknownType */(x_5127/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Pow2_5154 = function (x_5147/* : Numerical_18 */) /* : Numerical_18 */{ return Multiply_250/* : UnknownType */(x_5147/* : UnknownType */, x_5147/* : UnknownType */)/* : UnknownType */; };
    static Pow3_5166 = function (x_5156/* : Numerical_18 */) /* : Numerical_18 */{ return Multiply_250/* : UnknownType */(Pow2_2682/* : UnknownType */(x_5156/* : UnknownType */)/* : UnknownType */, x_5156/* : UnknownType */)/* : UnknownType */; };
    static Pow4_5178 = function (x_5168/* : Numerical_18 */) /* : Numerical_18 */{ return Multiply_250/* : UnknownType */(Pow3_2688/* : UnknownType */(x_5168/* : UnknownType */)/* : UnknownType */, x_5168/* : UnknownType */)/* : UnknownType */; };
    static Pow5_5190 = function (x_5180/* : Numerical_18 */) /* : Numerical_18 */{ return Multiply_250/* : UnknownType */(Pow4_2694/* : UnknownType */(x_5180/* : UnknownType */)/* : UnknownType */, x_5180/* : UnknownType */)/* : UnknownType */; };
    static Pi_5194 = function (self_5192/* : Numerical_138 */) /* : Number_29 */{ return 3.1415926535897/* : Float_10 */; };
    static AlmostZero_5206 = function (x_5196/* : Numerical_18 */) /* : Boolean_24 */{ return LessThan_2764/* : UnknownType */(Abs_2550/* : UnknownType */(x_5196/* : UnknownType */)/* : UnknownType */, 1E-08/* : Float_10 */)/* : UnknownType */; };
    static Lerp_5234 = function (a_5208/* : Numerical_18 */, b_5210/* : Numerical_18 */, t_5212/* : Unit_30 */) /* : Numerical_18 */{ return Multiply_250/* : UnknownType */(Subtract_234/* : UnknownType */(1/* : Integer_9 */, t_5212/* : UnknownType */)/* : UnknownType */, Add_226/* : UnknownType */(a_5208/* : UnknownType */, Multiply_250/* : UnknownType */(t_5212/* : UnknownType */, b_5210/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Between_5275 = function (self_5236/* : Numerical_18 */, min_5238/* : Numerical_18 */, max_5240/* : Numerical_18 */) /* : Boolean_24 */{ return Zip_2164/* : UnknownType */(FieldValues_348/* : UnknownType */(self_5236/* : UnknownType */)/* : UnknownType */, FieldValues_348/* : UnknownType */(min_5238/* : UnknownType */)/* : UnknownType */, FieldValues_348/* : UnknownType */(max_5240/* : UnknownType */)/* : UnknownType */, function (x_5257/* : UnknownType */, y_5259/* : UnknownType */, z_5261/* : UnknownType */) /* : Lambda_2 */{ return Between_2728/* : UnknownType */(x_5257/* : UnknownType */, y_5259/* : UnknownType */, z_5261/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */; };
}
class Angles_139_Library
{
    static Radians_5279 = function (x_5277/* : Number_29 */) /* : Angle_83 */{ return x_5277/* : Number_29 */; };
    static Degrees_5293 = function (x_5281/* : Number_29 */) /* : Angle_83 */{ return Multiply_250/* : UnknownType */(x_5281/* : UnknownType */, Divide_242/* : UnknownType */(Pi_2706/* : UnknownType */, 180/* : Integer_9 */)/* : UnknownType */)/* : UnknownType */; };
    static Turns_5307 = function (x_5295/* : Number_29 */) /* : Angle_83 */{ return Multiply_250/* : UnknownType */(x_5295/* : UnknownType */, Multiply_250/* : UnknownType */(2/* : Integer_9 */, Pi_2706/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Comparable_140_Library
{
    static Equals_5323 = function (a_5309/* : Comparable_20 */, b_5311/* : Comparable_20 */) /* : Boolean_24 */{ return Equals_447/* : UnknownType */(Compare_441/* : UnknownType */(a_5309/* : UnknownType */, b_5311/* : UnknownType */)/* : UnknownType */, 0/* : Integer_9 */)/* : UnknownType */; };
    static LessThan_5339 = function (a_5325/* : Comparable_20 */, b_5327/* : Comparable_20 */) /* : Boolean_24 */{ return LessThan_2764/* : UnknownType */(Compare_441/* : UnknownType */(a_5325/* : UnknownType */, b_5327/* : UnknownType */)/* : UnknownType */, 0/* : Integer_9 */)/* : UnknownType */; };
    static LessThanOrEquals_5355 = function (a_5341/* : Comparable_20 */, b_5343/* : Comparable_20 */) /* : Boolean_24 */{ return LessThanOrEquals_2772/* : UnknownType */(Compare_441/* : UnknownType */(a_5341/* : UnknownType */, b_5343/* : UnknownType */)/* : UnknownType */, 0/* : Integer_9 */)/* : UnknownType */; };
    static GreaterThan_5371 = function (a_5357/* : Comparable_20 */, b_5359/* : Comparable_20 */) /* : Boolean_24 */{ return GreaterThan_2780/* : UnknownType */(Compare_441/* : UnknownType */(a_5357/* : UnknownType */, b_5359/* : UnknownType */)/* : UnknownType */, 0/* : Integer_9 */)/* : UnknownType */; };
    static GreaterThanOrEquals_5387 = function (a_5373/* : Comparable_20 */, b_5375/* : Comparable_20 */) /* : Boolean_24 */{ return GreaterThanOrEquals_2788/* : UnknownType */(Compare_441/* : UnknownType */(a_5373/* : UnknownType */, b_5375/* : UnknownType */)/* : UnknownType */, 0/* : Integer_9 */)/* : UnknownType */; };
    static Between_5410 = function (v_5389/* : Comparable_20 */, a_5391/* : Comparable_20 */, b_5393/* : Comparable_20 */) /* : Value_15 */{ return GreaterThanOrEquals_2788/* : UnknownType */(v_5389/* : UnknownType */, And_318/* : UnknownType */(a_5391/* : UnknownType */, LessThanOrEquals_2772/* : UnknownType */(v_5389/* : UnknownType */, b_5393/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Between_5421 = function (v_5412/* : Value_15 */, i_5414/* : Interval_25 */) /* : Interval_25 */{ return Contains_2308/* : UnknownType */(i_5414/* : UnknownType */, v_5412/* : UnknownType */)/* : UnknownType */; };
    static Min_5435 = function (a_5423/* : Comparable_20 */, b_5425/* : Comparable_20 */) /* : Comparable_20 */{ return LessThanOrEquals_2772/* : UnknownType */(a_5423/* : UnknownType */, b_5425/* : UnknownType */
        ? a_5423/* : UnknownType */
        : b_5425/* : UnknownType */
    )/* : UnknownType */; };
    static Max_5449 = function (a_5437/* : Comparable_20 */, b_5439/* : Comparable_20 */) /* : Comparable_20 */{ return GreaterThanOrEquals_2788/* : UnknownType */(a_5437/* : UnknownType */, b_5439/* : UnknownType */
        ? a_5437/* : UnknownType */
        : b_5439/* : UnknownType */
    )/* : UnknownType */; };
}
class Equatable_141_Library
{
    static NotEquals_5463 = function (x_5451/* : Equatable_21 */, y_5453/* : Equatable_21 */) /* : Boolean_24 */{ return Not_334/* : UnknownType */(Equals_447/* : UnknownType */(x_5451/* : UnknownType */, y_5453/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Easings_142_Library
{
    static BlendEaseFunc_5515 = function (p_5465/* : Number_29 */, easeIn_5467/* : Function_3 */, easeOut_5469/* : Function_3 */) /* : Number_29 */{ return LessThan_2764/* : UnknownType */(p_5465/* : UnknownType */, 0.5/* : Float_10 */
        ? Multiply_250/* : UnknownType */(0.5/* : Float_10 */, easeIn_5467/* : UnknownType */(Multiply_250/* : UnknownType */(p_5465/* : UnknownType */, 2/* : Integer_9 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
        : Multiply_250/* : UnknownType */(0.5/* : Float_10 */, Add_226/* : UnknownType */(easeOut_5469/* : UnknownType */(Multiply_250/* : UnknownType */(p_5465/* : UnknownType */, Subtract_234/* : UnknownType */(2/* : Integer_9 */, 1/* : Integer_9 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, 0.5/* : Float_10 */)/* : UnknownType */)/* : UnknownType */
    )/* : UnknownType */; };
    static InvertEaseFunc_5534 = function (p_5517/* : Number_29 */, easeIn_5519/* : Function_3 */) /* : Number_29 */{ return Subtract_234/* : UnknownType */(1/* : Integer_9 */, easeIn_5519/* : UnknownType */(Subtract_234/* : UnknownType */(1/* : Integer_9 */, p_5517/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Linear_5538 = function (p_5536/* : Number_29 */) /* : Number_29 */{ return p_5536/* : Number_29 */; };
    static QuadraticEaseIn_5545 = function (p_5540/* : Number_29 */) /* : Number_29 */{ return Pow2_2682/* : UnknownType */(p_5540/* : UnknownType */)/* : UnknownType */; };
    static QuadraticEaseOut_5554 = function (p_5547/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2848/* : UnknownType */(p_5547/* : UnknownType */, QuadraticEaseIn_2862/* : UnknownType */)/* : UnknownType */; };
    static QuadraticEaseInOut_5565 = function (p_5556/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2838/* : UnknownType */(p_5556/* : UnknownType */, QuadraticEaseIn_2862/* : UnknownType */, QuadraticEaseOut_2868/* : UnknownType */)/* : UnknownType */; };
    static CubicEaseIn_5572 = function (p_5567/* : Number_29 */) /* : Number_29 */{ return Pow3_2688/* : UnknownType */(p_5567/* : UnknownType */)/* : UnknownType */; };
    static CubicEaseOut_5581 = function (p_5574/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2848/* : UnknownType */(p_5574/* : UnknownType */, CubicEaseIn_2880/* : UnknownType */)/* : UnknownType */; };
    static CubicEaseInOut_5592 = function (p_5583/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2838/* : UnknownType */(p_5583/* : UnknownType */, CubicEaseIn_2880/* : UnknownType */, CubicEaseOut_2886/* : UnknownType */)/* : UnknownType */; };
    static QuarticEaseIn_5599 = function (p_5594/* : Number_29 */) /* : Number_29 */{ return Pow4_2694/* : UnknownType */(p_5594/* : UnknownType */)/* : UnknownType */; };
    static QuarticEaseOut_5608 = function (p_5601/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2848/* : UnknownType */(p_5601/* : UnknownType */, QuarticEaseIn_2898/* : UnknownType */)/* : UnknownType */; };
    static QuarticEaseInOut_5619 = function (p_5610/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2838/* : UnknownType */(p_5610/* : UnknownType */, QuarticEaseIn_2898/* : UnknownType */, QuarticEaseOut_2904/* : UnknownType */)/* : UnknownType */; };
    static QuinticEaseIn_5626 = function (p_5621/* : Number_29 */) /* : Number_29 */{ return Pow5_2700/* : UnknownType */(p_5621/* : UnknownType */)/* : UnknownType */; };
    static QuinticEaseOut_5635 = function (p_5628/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2848/* : UnknownType */(p_5628/* : UnknownType */, QuinticEaseIn_2916/* : UnknownType */)/* : UnknownType */; };
    static QuinticEaseInOut_5646 = function (p_5637/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2838/* : UnknownType */(p_5637/* : UnknownType */, QuinticEaseIn_2916/* : UnknownType */, QuinticEaseOut_2922/* : UnknownType */)/* : UnknownType */; };
    static SineEaseIn_5655 = function (p_5648/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2848/* : UnknownType */(p_5648/* : UnknownType */, SineEaseOut_2940/* : UnknownType */)/* : UnknownType */; };
    static SineEaseOut_5668 = function (p_5657/* : Number_29 */) /* : Number_29 */{ return Sin_150/* : UnknownType */(Turns_2750/* : UnknownType */(Quarter_2568/* : UnknownType */(p_5657/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static SineEaseInOut_5679 = function (p_5670/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2838/* : UnknownType */(p_5670/* : UnknownType */, SineEaseIn_2934/* : UnknownType */, SineEaseOut_2940/* : UnknownType */)/* : UnknownType */; };
    static CircularEaseIn_5695 = function (p_5681/* : Number_29 */) /* : Number_29 */{ return FromOne_2502/* : UnknownType */(SquareRoot_2464/* : UnknownType */(FromOne_2502/* : UnknownType */(Pow2_2682/* : UnknownType */(p_5681/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static CircularEaseOut_5704 = function (p_5697/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2848/* : UnknownType */(p_5697/* : UnknownType */, CircularEaseIn_2952/* : UnknownType */)/* : UnknownType */; };
    static CircularEaseInOut_5715 = function (p_5706/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2838/* : UnknownType */(p_5706/* : UnknownType */, CircularEaseIn_2952/* : UnknownType */, CircularEaseOut_2958/* : UnknownType */)/* : UnknownType */; };
    static ExponentialEaseIn_5738 = function (p_5717/* : Number_29 */) /* : Number_29 */{ return AlmostZero_2712/* : UnknownType */(p_5717/* : UnknownType */)/* : UnknownType */
        ? p_5717/* : Number_29 */
        : Pow_180/* : UnknownType */(2/* : Integer_9 */, Multiply_250/* : UnknownType */(10/* : Integer_9 */, MinusOne_2496/* : UnknownType */(p_5717/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
    ; };
    static ExponentialEaseOut_5747 = function (p_5740/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2848/* : UnknownType */(p_5740/* : UnknownType */, ExponentialEaseIn_2970/* : UnknownType */)/* : UnknownType */; };
    static ExponentialEaseInOut_5758 = function (p_5749/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2838/* : UnknownType */(p_5749/* : UnknownType */, ExponentialEaseIn_2970/* : UnknownType */, ExponentialEaseOut_2976/* : UnknownType */)/* : UnknownType */; };
    static ElasticEaseIn_5797 = function (p_5760/* : Number_29 */) /* : Number_29 */{ return Multiply_250/* : UnknownType */(13/* : Integer_9 */, Multiply_250/* : UnknownType */(Turns_2750/* : UnknownType */(Quarter_2568/* : UnknownType */(p_5760/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Sin_150/* : UnknownType */(Radians_1471/* : UnknownType */(Pow_180/* : UnknownType */(2/* : Integer_9 */, Multiply_250/* : UnknownType */(10/* : Integer_9 */, MinusOne_2496/* : UnknownType */(p_5760/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static ElasticEaseOut_5806 = function (p_5799/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2848/* : UnknownType */(p_5799/* : UnknownType */, ElasticEaseIn_2988/* : UnknownType */)/* : UnknownType */; };
    static ElasticEaseInOut_5817 = function (p_5808/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2838/* : UnknownType */(p_5808/* : UnknownType */, ElasticEaseIn_2988/* : UnknownType */, ElasticEaseOut_2994/* : UnknownType */)/* : UnknownType */; };
    static BackEaseIn_5843 = function (p_5819/* : Number_29 */) /* : Number_29 */{ return Subtract_234/* : UnknownType */(Pow3_2688/* : UnknownType */(p_5819/* : UnknownType */)/* : UnknownType */, Multiply_250/* : UnknownType */(p_5819/* : UnknownType */, Sin_150/* : UnknownType */(Turns_2750/* : UnknownType */(Half_2556/* : UnknownType */(p_5819/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static BackEaseOut_5852 = function (p_5845/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2848/* : UnknownType */(p_5845/* : UnknownType */, BackEaseIn_3006/* : UnknownType */)/* : UnknownType */; };
    static BackEaseInOut_5863 = function (p_5854/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2838/* : UnknownType */(p_5854/* : UnknownType */, BackEaseIn_3006/* : UnknownType */, BackEaseOut_3012/* : UnknownType */)/* : UnknownType */; };
    static BounceEaseIn_5872 = function (p_5865/* : Number_29 */) /* : Number_29 */{ return InvertEaseFunc_2848/* : UnknownType */(p_5865/* : UnknownType */, BounceEaseOut_3030/* : UnknownType */)/* : UnknownType */; };
    static BounceEaseOut_6042 = function (p_5874/* : Number_29 */) /* : Number_29 */{ return LessThan_2764/* : UnknownType */(p_5874/* : UnknownType */, Divide_242/* : UnknownType */(4/* : Integer_9 */, 11/* : Float_10 */)/* : UnknownType */)/* : UnknownType */
        ? Multiply_250/* : UnknownType */(121/* : Float_10 */, Divide_242/* : UnknownType */(Pow2_2682/* : UnknownType */(p_5874/* : UnknownType */)/* : UnknownType */, 16/* : Float_10 */)/* : UnknownType */)/* : UnknownType */
        : LessThan_2764/* : UnknownType */(p_5874/* : UnknownType */, Divide_242/* : UnknownType */(8/* : Integer_9 */, 11/* : Float_10 */)/* : UnknownType */)/* : UnknownType */
            ? Divide_242/* : UnknownType */(363/* : Float_10 */, Multiply_250/* : UnknownType */(40/* : Float_10 */, Subtract_234/* : UnknownType */(Pow2_2682/* : UnknownType */(p_5874/* : UnknownType */)/* : UnknownType */, Divide_242/* : UnknownType */(99/* : Float_10 */, Multiply_250/* : UnknownType */(10/* : Float_10 */, Add_226/* : UnknownType */(p_5874/* : UnknownType */, Divide_242/* : UnknownType */(17/* : Float_10 */, 5/* : Float_10 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
            : LessThan_2764/* : UnknownType */(p_5874/* : UnknownType */, Divide_242/* : UnknownType */(9/* : Integer_9 */, 10/* : Float_10 */)/* : UnknownType */)/* : UnknownType */
                ? Divide_242/* : UnknownType */(4356/* : Float_10 */, Multiply_250/* : UnknownType */(361/* : Float_10 */, Subtract_234/* : UnknownType */(Pow2_2682/* : UnknownType */(p_5874/* : UnknownType */)/* : UnknownType */, Divide_242/* : UnknownType */(35442/* : Float_10 */, Multiply_250/* : UnknownType */(1805/* : Float_10 */, Add_226/* : UnknownType */(p_5874/* : UnknownType */, Divide_242/* : UnknownType */(16061/* : Float_10 */, 1805/* : Float_10 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
                : Divide_242/* : UnknownType */(54/* : Float_10 */, Multiply_250/* : UnknownType */(5/* : Float_10 */, Subtract_234/* : UnknownType */(Pow2_2682/* : UnknownType */(p_5874/* : UnknownType */)/* : UnknownType */, Divide_242/* : UnknownType */(513/* : Float_10 */, Multiply_250/* : UnknownType */(25/* : Float_10 */, Add_226/* : UnknownType */(p_5874/* : UnknownType */, Divide_242/* : UnknownType */(268/* : Float_10 */, 25/* : Float_10 */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */


    ; };
    static BounceEaseInOut_6053 = function (p_6044/* : Number_29 */) /* : Number_29 */{ return BlendEaseFunc_2838/* : UnknownType */(p_6044/* : UnknownType */, BounceEaseIn_3024/* : UnknownType */, BounceEaseOut_3030/* : UnknownType */)/* : UnknownType */; };
}
class Any_13_Concept
{
    constructor(self) { this.Self = self; };
    static FieldNames_3157 = function (self_3156/* : Any_13 */) /* : Array_14 */{ return null; };
    static FieldValues_3160 = function (x_3159/* : Any_13 */) /* : Array_14 */{ return null; };
    static TypeOf_3163 = function (self_3162/* : Any_13 */) /* : Type_11 */{ return null; };
}
class Array_14_Concept
{
    constructor(self) { this.Self = self; };
    static Count_3168 = function (xs_3167/* : Array_14 */) /* : Count_27 */{ return null; };
    static At_3173 = function (xs_3170/* : Array_14 */, n_3172/* : Index_28 */) /* : Any_13 */{ return null; };
}
class Value_15_Concept
{
    constructor(self) { this.Self = self; };
    static Default_3185 = function (self_3177/* : Value_15 */) /* : Value_15 */{ return Default_376/* : UnknownType */(FieldValues_348/* : UnknownType */(Self_3175/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Vector_16_Concept
{
    constructor(self) { this.Self = self; };
    static Count_3203 = function (v_3195/* : Vector_16 */) /* : Count_27 */{ return Count_362/* : UnknownType */(FieldTypes_405/* : UnknownType */(Self_3193/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static At_3217 = function (v_3205/* : Vector_16 */, n_3207/* : Index_28 */) /* : Numerical_18 */{ return At_368/* : UnknownType */(FieldValues_348/* : UnknownType */(v_3205/* : UnknownType */)/* : UnknownType */, n_3207/* : UnknownType */)/* : UnknownType */; };
}
class Measure_17_Concept
{
    constructor(self) { this.Self = self; };
    static Value_3236 = function (x_3226/* : Measure_17 */) /* : Number_29 */{ return At_368/* : UnknownType */(FieldValues_348/* : UnknownType */(x_3226/* : UnknownType */)/* : UnknownType */, 0/* : Integer_9 */)/* : UnknownType */; };
}
class Numerical_18_Concept
{
    constructor(self) { this.Self = self; };
    static FieldTypes_3247 = function (self_3246/* : Numerical_18 */) /* : Array_14 */{ return null; };
    static Zero_3257 = function (self_3249/* : Numerical_18 */) /* : Numerical_18 */{ return Zero_411/* : UnknownType */(FieldTypes_405/* : UnknownType */(Self_3244/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static One_3267 = function (self_3259/* : Numerical_18 */) /* : Numerical_18 */{ return One_417/* : UnknownType */(FieldTypes_405/* : UnknownType */(Self_3244/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static MinValue_3277 = function (self_3269/* : Numerical_18 */) /* : Numerical_18 */{ return MinValue_423/* : UnknownType */(FieldTypes_405/* : UnknownType */(Self_3244/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static MaxValue_3287 = function (self_3279/* : Numerical_18 */) /* : Numerical_18 */{ return MaxValue_429/* : UnknownType */(FieldTypes_405/* : UnknownType */(Self_3244/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Magnitudinal_19_Concept
{
    constructor(self) { this.Self = self; };
    static Magnitude_3306 = function (x_3292/* : Magnitudinal_19 */) /* : Number_29 */{ return SquareRoot_2464/* : UnknownType */(Sum_2426/* : UnknownType */(Square_2470/* : UnknownType */(FieldValues_348/* : UnknownType */(x_3292/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Comparable_20_Concept
{
    constructor(self) { this.Self = self; };
    static Compare_3312 = function (x_3311/* : Comparable_20 */) /* : Integer_26 */{ return null; };
}
class Equatable_21_Concept
{
    constructor(self) { this.Self = self; };
    static Equals_3335 = function (a_3317/* : Equatable_21 */, b_3319/* : Equatable_21 */) /* : Boolean_24 */{ return All_2254/* : UnknownType */(Equals_447/* : UnknownType */(FieldValues_348/* : UnknownType */(a_3317/* : UnknownType */)/* : UnknownType */, FieldValues_348/* : UnknownType */(b_3319/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Arithmetic_22_Concept
{
    constructor(self) { this.Self = self; };
    static Add_3355 = function (self_3340/* : Arithmetic_22 */, other_3342/* : Arithmetic_22 */) /* : Arithmetic_22 */{ return Add_226/* : UnknownType */(FieldValues_348/* : UnknownType */(self_3340/* : UnknownType */)/* : UnknownType */, FieldValues_348/* : UnknownType */(other_3342/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Negative_3365 = function (self_3357/* : Arithmetic_22 */) /* : Arithmetic_22 */{ return Negative_266/* : UnknownType */(FieldValues_348/* : UnknownType */(self_3357/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Reciprocal_3375 = function (self_3367/* : Arithmetic_22 */) /* : Arithmetic_22 */{ return Reciprocal_469/* : UnknownType */(FieldValues_348/* : UnknownType */(self_3367/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Multiply_3392 = function (self_3377/* : Arithmetic_22 */, other_3379/* : Arithmetic_22 */) /* : Arithmetic_22 */{ return Add_226/* : UnknownType */(FieldValues_348/* : UnknownType */(self_3377/* : UnknownType */)/* : UnknownType */, FieldValues_348/* : UnknownType */(other_3379/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Divide_3409 = function (self_3394/* : Arithmetic_22 */, other_3396/* : Arithmetic_22 */) /* : Arithmetic_22 */{ return Divide_242/* : UnknownType */(FieldValues_348/* : UnknownType */(self_3394/* : UnknownType */)/* : UnknownType */, FieldValues_348/* : UnknownType */(other_3396/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Modulo_3426 = function (self_3411/* : Arithmetic_22 */, other_3413/* : Arithmetic_22 */) /* : Arithmetic_22 */{ return Modulo_258/* : UnknownType */(FieldValues_348/* : UnknownType */(self_3411/* : UnknownType */)/* : UnknownType */, FieldValues_348/* : UnknownType */(other_3413/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class ScalarArithmetic_23_Concept
{
    constructor(self) { this.Self = self; };
    static Add_3443 = function (self_3431/* : ScalarArithmetic_23 */, scalar_3433/* : Number_29 */) /* : ScalarArithmetic_23 */{ return Add_226/* : UnknownType */(FieldValues_348/* : UnknownType */(self_3431/* : UnknownType */)/* : UnknownType */, scalar_3433/* : UnknownType */)/* : UnknownType */; };
    static Subtract_3457 = function (self_3445/* : ScalarArithmetic_23 */, scalar_3447/* : Number_29 */) /* : ScalarArithmetic_23 */{ return Add_226/* : UnknownType */(self_3445/* : UnknownType */, Negative_266/* : UnknownType */(scalar_3447/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Multiply_3471 = function (self_3459/* : ScalarArithmetic_23 */, scalar_3461/* : Number_29 */) /* : ScalarArithmetic_23 */{ return Multiply_250/* : UnknownType */(FieldValues_348/* : UnknownType */(self_3459/* : UnknownType */)/* : UnknownType */, scalar_3461/* : UnknownType */)/* : UnknownType */; };
    static Divide_3485 = function (self_3473/* : ScalarArithmetic_23 */, scalar_3475/* : Number_29 */) /* : ScalarArithmetic_23 */{ return Multiply_250/* : UnknownType */(self_3473/* : UnknownType */, Reciprocal_469/* : UnknownType */(scalar_3475/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Modulo_3499 = function (self_3487/* : ScalarArithmetic_23 */, scalar_3489/* : Number_29 */) /* : ScalarArithmetic_23 */{ return Modulo_258/* : UnknownType */(FieldValues_348/* : UnknownType */(self_3487/* : UnknownType */)/* : UnknownType */, scalar_3489/* : UnknownType */)/* : UnknownType */; };
}
class Boolean_24_Concept
{
    constructor(self) { this.Self = self; };
    static And_3518 = function (a_3503/* : Boolean_24 */, b_3505/* : Boolean_24 */) /* : Boolean_24 */{ return And_318/* : UnknownType */(FieldValues_348/* : UnknownType */(a_3503/* : UnknownType */)/* : UnknownType */, FieldValues_348/* : UnknownType */(b_3505/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Or_3535 = function (a_3520/* : Boolean_24 */, b_3522/* : Boolean_24 */) /* : Boolean_24 */{ return Or_326/* : UnknownType */(FieldValues_348/* : UnknownType */(a_3520/* : UnknownType */)/* : UnknownType */, FieldValues_348/* : UnknownType */(b_3522/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Not_3545 = function (a_3537/* : Boolean_24 */) /* : Boolean_24 */{ return Not_334/* : UnknownType */(FieldValues_348/* : UnknownType */(a_3537/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Interval_25_Concept
{
    constructor(self) { this.Self = self; };
    static Min_3552 = function (x_3551/* : Interval_25 */) /* : Numerical_18 */{ return null; };
    static Max_3555 = function (x_3554/* : Interval_25 */) /* : Numerical_18 */{ return null; };
}
class Integer_26_Type
{
    constructor(Value_575)
    {
        // field initialization 
        this.Value_575 = Value_575;
        this.FieldTypes_3247 = Integer_26_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = Integer_26_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = Integer_26_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = Integer_26_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = Integer_26_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = Integer_26_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Integer_26_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Integer_26_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Integer_26_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Integer_26_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Integer_26_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Integer_26_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Integer_26_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Integer_26_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Integer_26_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Integer_26_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Integer_26_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Integer_26_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Integer_26_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Integer_26_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Integer_26_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Integer_26_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Integer_26_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Integer_26_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Integer_26_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Value_575 = function(self) { return self.Value_575; }
    // implemented concepts 
    static Numerical_18_Concept = new Numerical_18_Concept(Integer_26_Type);
    static Value_15_Concept = new Value_15_Concept(Integer_26_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Integer_26_Type);
    static Value_15_Concept = new Value_15_Concept(Integer_26_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Integer_26_Type);
    static Value_15_Concept = new Value_15_Concept(Integer_26_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Integer_26_Type);
    static Value_15_Concept = new Value_15_Concept(Integer_26_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Integer_26_Type);
    static Value_15_Concept = new Value_15_Concept(Integer_26_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Integer_26_Type);
    static Value_15_Concept = new Value_15_Concept(Integer_26_Type);
    static Implements = [Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept];
}
class Count_27_Type
{
    constructor(Value_582)
    {
        // field initialization 
        this.Value_582 = Value_582;
        this.FieldTypes_3247 = Count_27_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = Count_27_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = Count_27_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = Count_27_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = Count_27_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = Count_27_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Count_27_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Count_27_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Count_27_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Count_27_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Count_27_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Count_27_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Count_27_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Count_27_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Count_27_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Count_27_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Count_27_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Count_27_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Count_27_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Count_27_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Count_27_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Count_27_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Count_27_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Count_27_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Count_27_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Value_582 = function(self) { return self.Value_582; }
    // implemented concepts 
    static Numerical_18_Concept = new Numerical_18_Concept(Count_27_Type);
    static Value_15_Concept = new Value_15_Concept(Count_27_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Count_27_Type);
    static Value_15_Concept = new Value_15_Concept(Count_27_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Count_27_Type);
    static Value_15_Concept = new Value_15_Concept(Count_27_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Count_27_Type);
    static Value_15_Concept = new Value_15_Concept(Count_27_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Count_27_Type);
    static Value_15_Concept = new Value_15_Concept(Count_27_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Count_27_Type);
    static Value_15_Concept = new Value_15_Concept(Count_27_Type);
    static Implements = [Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept];
}
class Index_28_Type
{
    constructor(Value_589)
    {
        // field initialization 
        this.Value_589 = Value_589;
        this.Default_3185 = Index_28_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Value_589 = function(self) { return self.Value_589; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Index_28_Type);
    static Implements = [Value_15_Concept];
}
class Number_29_Type
{
    constructor(Value_596)
    {
        // field initialization 
        this.Value_596 = Value_596;
        this.FieldTypes_3247 = Number_29_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = Number_29_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = Number_29_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = Number_29_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = Number_29_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = Number_29_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Number_29_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Number_29_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Number_29_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Number_29_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Number_29_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Number_29_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Number_29_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Number_29_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Number_29_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Number_29_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Number_29_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Number_29_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Number_29_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Number_29_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Number_29_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Number_29_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Number_29_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Number_29_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Number_29_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Value_596 = function(self) { return self.Value_596; }
    // implemented concepts 
    static Numerical_18_Concept = new Numerical_18_Concept(Number_29_Type);
    static Value_15_Concept = new Value_15_Concept(Number_29_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Number_29_Type);
    static Value_15_Concept = new Value_15_Concept(Number_29_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Number_29_Type);
    static Value_15_Concept = new Value_15_Concept(Number_29_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Number_29_Type);
    static Value_15_Concept = new Value_15_Concept(Number_29_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Number_29_Type);
    static Value_15_Concept = new Value_15_Concept(Number_29_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Number_29_Type);
    static Value_15_Concept = new Value_15_Concept(Number_29_Type);
    static Implements = [Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept];
}
class Unit_30_Type
{
    constructor(Value_603)
    {
        // field initialization 
        this.Value_603 = Value_603;
        this.FieldTypes_3247 = Unit_30_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = Unit_30_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = Unit_30_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = Unit_30_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = Unit_30_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = Unit_30_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Unit_30_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Unit_30_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Unit_30_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Unit_30_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Unit_30_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Unit_30_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Unit_30_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Unit_30_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Unit_30_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Unit_30_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Unit_30_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Unit_30_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Unit_30_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Unit_30_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Unit_30_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Unit_30_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Unit_30_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Unit_30_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Unit_30_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Value_603 = function(self) { return self.Value_603; }
    // implemented concepts 
    static Numerical_18_Concept = new Numerical_18_Concept(Unit_30_Type);
    static Value_15_Concept = new Value_15_Concept(Unit_30_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Unit_30_Type);
    static Value_15_Concept = new Value_15_Concept(Unit_30_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Unit_30_Type);
    static Value_15_Concept = new Value_15_Concept(Unit_30_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Unit_30_Type);
    static Value_15_Concept = new Value_15_Concept(Unit_30_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Unit_30_Type);
    static Value_15_Concept = new Value_15_Concept(Unit_30_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Unit_30_Type);
    static Value_15_Concept = new Value_15_Concept(Unit_30_Type);
    static Implements = [Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept];
}
class Percent_31_Type
{
    constructor(Value_610)
    {
        // field initialization 
        this.Value_610 = Value_610;
        this.FieldTypes_3247 = Percent_31_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = Percent_31_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = Percent_31_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = Percent_31_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = Percent_31_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = Percent_31_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Percent_31_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Percent_31_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Percent_31_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Percent_31_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Percent_31_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Percent_31_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Percent_31_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Percent_31_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Percent_31_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Percent_31_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Percent_31_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Percent_31_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Percent_31_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Percent_31_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Percent_31_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Percent_31_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Percent_31_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Percent_31_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Percent_31_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Value_610 = function(self) { return self.Value_610; }
    // implemented concepts 
    static Numerical_18_Concept = new Numerical_18_Concept(Percent_31_Type);
    static Value_15_Concept = new Value_15_Concept(Percent_31_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Percent_31_Type);
    static Value_15_Concept = new Value_15_Concept(Percent_31_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Percent_31_Type);
    static Value_15_Concept = new Value_15_Concept(Percent_31_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Percent_31_Type);
    static Value_15_Concept = new Value_15_Concept(Percent_31_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Percent_31_Type);
    static Value_15_Concept = new Value_15_Concept(Percent_31_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Percent_31_Type);
    static Value_15_Concept = new Value_15_Concept(Percent_31_Type);
    static Implements = [Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept];
}
class Quaternion_32_Type
{
    constructor(X_617, Y_624, Z_631, W_638)
    {
        // field initialization 
        this.X_617 = X_617;
        this.Y_624 = Y_624;
        this.Z_631 = Z_631;
        this.W_638 = W_638;
        this.Default_3185 = Quaternion_32_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static X_617 = function(self) { return self.X_617; }
    static Y_624 = function(self) { return self.Y_624; }
    static Z_631 = function(self) { return self.Z_631; }
    static W_638 = function(self) { return self.W_638; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Quaternion_32_Type);
    static Implements = [Value_15_Concept];
}
class Unit2D_33_Type
{
    constructor(X_645, Y_652)
    {
        // field initialization 
        this.X_645 = X_645;
        this.Y_652 = Y_652;
        this.Default_3185 = Unit2D_33_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static X_645 = function(self) { return self.X_645; }
    static Y_652 = function(self) { return self.Y_652; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Unit2D_33_Type);
    static Implements = [Value_15_Concept];
}
class Unit3D_34_Type
{
    constructor(X_659, Y_666, Z_673)
    {
        // field initialization 
        this.X_659 = X_659;
        this.Y_666 = Y_666;
        this.Z_673 = Z_673;
        this.Default_3185 = Unit3D_34_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static X_659 = function(self) { return self.X_659; }
    static Y_666 = function(self) { return self.Y_666; }
    static Z_673 = function(self) { return self.Z_673; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Unit3D_34_Type);
    static Implements = [Value_15_Concept];
}
class Direction3D_35_Type
{
    constructor(Value_680)
    {
        // field initialization 
        this.Value_680 = Value_680;
        this.Default_3185 = Direction3D_35_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Value_680 = function(self) { return self.Value_680; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Direction3D_35_Type);
    static Implements = [Value_15_Concept];
}
class AxisAngle_36_Type
{
    constructor(Axis_687, Angle_694)
    {
        // field initialization 
        this.Axis_687 = Axis_687;
        this.Angle_694 = Angle_694;
        this.Default_3185 = AxisAngle_36_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Axis_687 = function(self) { return self.Axis_687; }
    static Angle_694 = function(self) { return self.Angle_694; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(AxisAngle_36_Type);
    static Implements = [Value_15_Concept];
}
class EulerAngles_37_Type
{
    constructor(Yaw_701, Pitch_708, Roll_715)
    {
        // field initialization 
        this.Yaw_701 = Yaw_701;
        this.Pitch_708 = Pitch_708;
        this.Roll_715 = Roll_715;
        this.Default_3185 = EulerAngles_37_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Yaw_701 = function(self) { return self.Yaw_701; }
    static Pitch_708 = function(self) { return self.Pitch_708; }
    static Roll_715 = function(self) { return self.Roll_715; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(EulerAngles_37_Type);
    static Implements = [Value_15_Concept];
}
class Rotation3D_38_Type
{
    constructor(Quaternion_722)
    {
        // field initialization 
        this.Quaternion_722 = Quaternion_722;
        this.Default_3185 = Rotation3D_38_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Quaternion_722 = function(self) { return self.Quaternion_722; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Rotation3D_38_Type);
    static Implements = [Value_15_Concept];
}
class Vector2D_39_Type
{
    constructor(X_729, Y_736)
    {
        // field initialization 
        this.X_729 = X_729;
        this.Y_736 = Y_736;
        this.Count_3203 = Vector2D_39_Type.Vector_16_Concept.Count_3203;
        this.At_3217 = Vector2D_39_Type.Vector_16_Concept.At_3217;
        this.Count_3168 = Vector2D_39_Type.Array_14_Concept.Count_3168;
        this.At_3173 = Vector2D_39_Type.Array_14_Concept.At_3173;
        this.Default_3185 = Vector2D_39_Type.Value_15_Concept.Default_3185;
        this.FieldTypes_3247 = Vector2D_39_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = Vector2D_39_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = Vector2D_39_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = Vector2D_39_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = Vector2D_39_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = Vector2D_39_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Vector2D_39_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Vector2D_39_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Vector2D_39_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Vector2D_39_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Vector2D_39_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Vector2D_39_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Vector2D_39_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Vector2D_39_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Vector2D_39_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Vector2D_39_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Vector2D_39_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Vector2D_39_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Vector2D_39_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Vector2D_39_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Vector2D_39_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Vector2D_39_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Vector2D_39_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Vector2D_39_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Vector2D_39_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Vector2D_39_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Vector2D_39_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Vector2D_39_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Vector2D_39_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Vector2D_39_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Vector2D_39_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Vector2D_39_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static X_729 = function(self) { return self.X_729; }
    static Y_736 = function(self) { return self.Y_736; }
    // implemented concepts 
    static Vector_16_Concept = new Vector_16_Concept(Vector2D_39_Type);
    static Array_14_Concept = new Array_14_Concept(Vector2D_39_Type);
    static Value_15_Concept = new Value_15_Concept(Vector2D_39_Type);
    static Numerical_18_Concept = new Numerical_18_Concept(Vector2D_39_Type);
    static Value_15_Concept = new Value_15_Concept(Vector2D_39_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Vector2D_39_Type);
    static Value_15_Concept = new Value_15_Concept(Vector2D_39_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Vector2D_39_Type);
    static Value_15_Concept = new Value_15_Concept(Vector2D_39_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Vector2D_39_Type);
    static Value_15_Concept = new Value_15_Concept(Vector2D_39_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Vector2D_39_Type);
    static Value_15_Concept = new Value_15_Concept(Vector2D_39_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Vector2D_39_Type);
    static Value_15_Concept = new Value_15_Concept(Vector2D_39_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Vector2D_39_Type);
    static Value_15_Concept = new Value_15_Concept(Vector2D_39_Type);
    static Implements = [Vector_16_Concept,Array_14_Concept,Value_15_Concept,Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept];
}
class Vector3D_40_Type
{
    constructor(X_743, Y_750, Z_757)
    {
        // field initialization 
        this.X_743 = X_743;
        this.Y_750 = Y_750;
        this.Z_757 = Z_757;
        this.Count_3203 = Vector3D_40_Type.Vector_16_Concept.Count_3203;
        this.At_3217 = Vector3D_40_Type.Vector_16_Concept.At_3217;
        this.Count_3168 = Vector3D_40_Type.Array_14_Concept.Count_3168;
        this.At_3173 = Vector3D_40_Type.Array_14_Concept.At_3173;
        this.Default_3185 = Vector3D_40_Type.Value_15_Concept.Default_3185;
        this.FieldTypes_3247 = Vector3D_40_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = Vector3D_40_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = Vector3D_40_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = Vector3D_40_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = Vector3D_40_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = Vector3D_40_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Vector3D_40_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Vector3D_40_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Vector3D_40_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Vector3D_40_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Vector3D_40_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Vector3D_40_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Vector3D_40_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Vector3D_40_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Vector3D_40_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Vector3D_40_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Vector3D_40_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Vector3D_40_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Vector3D_40_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Vector3D_40_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Vector3D_40_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Vector3D_40_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Vector3D_40_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Vector3D_40_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Vector3D_40_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Vector3D_40_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Vector3D_40_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Vector3D_40_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Vector3D_40_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Vector3D_40_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Vector3D_40_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Vector3D_40_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static X_743 = function(self) { return self.X_743; }
    static Y_750 = function(self) { return self.Y_750; }
    static Z_757 = function(self) { return self.Z_757; }
    // implemented concepts 
    static Vector_16_Concept = new Vector_16_Concept(Vector3D_40_Type);
    static Array_14_Concept = new Array_14_Concept(Vector3D_40_Type);
    static Value_15_Concept = new Value_15_Concept(Vector3D_40_Type);
    static Numerical_18_Concept = new Numerical_18_Concept(Vector3D_40_Type);
    static Value_15_Concept = new Value_15_Concept(Vector3D_40_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Vector3D_40_Type);
    static Value_15_Concept = new Value_15_Concept(Vector3D_40_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Vector3D_40_Type);
    static Value_15_Concept = new Value_15_Concept(Vector3D_40_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Vector3D_40_Type);
    static Value_15_Concept = new Value_15_Concept(Vector3D_40_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Vector3D_40_Type);
    static Value_15_Concept = new Value_15_Concept(Vector3D_40_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Vector3D_40_Type);
    static Value_15_Concept = new Value_15_Concept(Vector3D_40_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Vector3D_40_Type);
    static Value_15_Concept = new Value_15_Concept(Vector3D_40_Type);
    static Implements = [Vector_16_Concept,Array_14_Concept,Value_15_Concept,Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept];
}
class Vector4D_41_Type
{
    constructor(X_764, Y_771, Z_778, W_785)
    {
        // field initialization 
        this.X_764 = X_764;
        this.Y_771 = Y_771;
        this.Z_778 = Z_778;
        this.W_785 = W_785;
        this.Count_3203 = Vector4D_41_Type.Vector_16_Concept.Count_3203;
        this.At_3217 = Vector4D_41_Type.Vector_16_Concept.At_3217;
        this.Count_3168 = Vector4D_41_Type.Array_14_Concept.Count_3168;
        this.At_3173 = Vector4D_41_Type.Array_14_Concept.At_3173;
        this.Default_3185 = Vector4D_41_Type.Value_15_Concept.Default_3185;
        this.FieldTypes_3247 = Vector4D_41_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = Vector4D_41_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = Vector4D_41_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = Vector4D_41_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = Vector4D_41_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = Vector4D_41_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Vector4D_41_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Vector4D_41_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Vector4D_41_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Vector4D_41_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Vector4D_41_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Vector4D_41_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Vector4D_41_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Vector4D_41_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Vector4D_41_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Vector4D_41_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Vector4D_41_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Vector4D_41_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Vector4D_41_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Vector4D_41_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Vector4D_41_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Vector4D_41_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Vector4D_41_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Vector4D_41_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Vector4D_41_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Vector4D_41_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Vector4D_41_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Vector4D_41_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Vector4D_41_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Vector4D_41_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Vector4D_41_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Vector4D_41_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static X_764 = function(self) { return self.X_764; }
    static Y_771 = function(self) { return self.Y_771; }
    static Z_778 = function(self) { return self.Z_778; }
    static W_785 = function(self) { return self.W_785; }
    // implemented concepts 
    static Vector_16_Concept = new Vector_16_Concept(Vector4D_41_Type);
    static Array_14_Concept = new Array_14_Concept(Vector4D_41_Type);
    static Value_15_Concept = new Value_15_Concept(Vector4D_41_Type);
    static Numerical_18_Concept = new Numerical_18_Concept(Vector4D_41_Type);
    static Value_15_Concept = new Value_15_Concept(Vector4D_41_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Vector4D_41_Type);
    static Value_15_Concept = new Value_15_Concept(Vector4D_41_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Vector4D_41_Type);
    static Value_15_Concept = new Value_15_Concept(Vector4D_41_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Vector4D_41_Type);
    static Value_15_Concept = new Value_15_Concept(Vector4D_41_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Vector4D_41_Type);
    static Value_15_Concept = new Value_15_Concept(Vector4D_41_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Vector4D_41_Type);
    static Value_15_Concept = new Value_15_Concept(Vector4D_41_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Vector4D_41_Type);
    static Value_15_Concept = new Value_15_Concept(Vector4D_41_Type);
    static Implements = [Vector_16_Concept,Array_14_Concept,Value_15_Concept,Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept];
}
class Orientation3D_42_Type
{
    constructor(Value_792)
    {
        // field initialization 
        this.Value_792 = Value_792;
        this.Default_3185 = Orientation3D_42_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Value_792 = function(self) { return self.Value_792; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Orientation3D_42_Type);
    static Implements = [Value_15_Concept];
}
class Pose2D_43_Type
{
    constructor(Position_799, Orientation_806)
    {
        // field initialization 
        this.Position_799 = Position_799;
        this.Orientation_806 = Orientation_806;
        this.Default_3185 = Pose2D_43_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Position_799 = function(self) { return self.Position_799; }
    static Orientation_806 = function(self) { return self.Orientation_806; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Pose2D_43_Type);
    static Implements = [Value_15_Concept];
}
class Pose3D_44_Type
{
    constructor(Position_813, Orientation_820)
    {
        // field initialization 
        this.Position_813 = Position_813;
        this.Orientation_820 = Orientation_820;
        this.Default_3185 = Pose3D_44_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Position_813 = function(self) { return self.Position_813; }
    static Orientation_820 = function(self) { return self.Orientation_820; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Pose3D_44_Type);
    static Implements = [Value_15_Concept];
}
class Transform3D_45_Type
{
    constructor(Translation_827, Rotation_834, Scale_841)
    {
        // field initialization 
        this.Translation_827 = Translation_827;
        this.Rotation_834 = Rotation_834;
        this.Scale_841 = Scale_841;
        this.Default_3185 = Transform3D_45_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Translation_827 = function(self) { return self.Translation_827; }
    static Rotation_834 = function(self) { return self.Rotation_834; }
    static Scale_841 = function(self) { return self.Scale_841; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Transform3D_45_Type);
    static Implements = [Value_15_Concept];
}
class Transform2D_46_Type
{
    constructor(Translation_848, Rotation_855, Scale_862)
    {
        // field initialization 
        this.Translation_848 = Translation_848;
        this.Rotation_855 = Rotation_855;
        this.Scale_862 = Scale_862;
        this.Default_3185 = Transform2D_46_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Translation_848 = function(self) { return self.Translation_848; }
    static Rotation_855 = function(self) { return self.Rotation_855; }
    static Scale_862 = function(self) { return self.Scale_862; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Transform2D_46_Type);
    static Implements = [Value_15_Concept];
}
class AlignedBox2D_47_Type
{
    constructor(A_869, B_876)
    {
        // field initialization 
        this.A_869 = A_869;
        this.B_876 = B_876;
        this.Min_3552 = AlignedBox2D_47_Type.Interval_25_Concept.Min_3552;
        this.Max_3555 = AlignedBox2D_47_Type.Interval_25_Concept.Max_3555;
        this.Count_3203 = AlignedBox2D_47_Type.Vector_16_Concept.Count_3203;
        this.At_3217 = AlignedBox2D_47_Type.Vector_16_Concept.At_3217;
        this.Count_3168 = AlignedBox2D_47_Type.Array_14_Concept.Count_3168;
        this.At_3173 = AlignedBox2D_47_Type.Array_14_Concept.At_3173;
        this.Default_3185 = AlignedBox2D_47_Type.Value_15_Concept.Default_3185;
        this.FieldTypes_3247 = AlignedBox2D_47_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = AlignedBox2D_47_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = AlignedBox2D_47_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = AlignedBox2D_47_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = AlignedBox2D_47_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = AlignedBox2D_47_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = AlignedBox2D_47_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = AlignedBox2D_47_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = AlignedBox2D_47_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = AlignedBox2D_47_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = AlignedBox2D_47_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = AlignedBox2D_47_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = AlignedBox2D_47_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = AlignedBox2D_47_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = AlignedBox2D_47_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = AlignedBox2D_47_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = AlignedBox2D_47_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = AlignedBox2D_47_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = AlignedBox2D_47_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = AlignedBox2D_47_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = AlignedBox2D_47_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = AlignedBox2D_47_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = AlignedBox2D_47_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = AlignedBox2D_47_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = AlignedBox2D_47_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = AlignedBox2D_47_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = AlignedBox2D_47_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = AlignedBox2D_47_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = AlignedBox2D_47_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = AlignedBox2D_47_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = AlignedBox2D_47_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = AlignedBox2D_47_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static A_869 = function(self) { return self.A_869; }
    static B_876 = function(self) { return self.B_876; }
    // implemented concepts 
    static Interval_25_Concept = new Interval_25_Concept(AlignedBox2D_47_Type);
    static Vector_16_Concept = new Vector_16_Concept(AlignedBox2D_47_Type);
    static Array_14_Concept = new Array_14_Concept(AlignedBox2D_47_Type);
    static Value_15_Concept = new Value_15_Concept(AlignedBox2D_47_Type);
    static Numerical_18_Concept = new Numerical_18_Concept(AlignedBox2D_47_Type);
    static Value_15_Concept = new Value_15_Concept(AlignedBox2D_47_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(AlignedBox2D_47_Type);
    static Value_15_Concept = new Value_15_Concept(AlignedBox2D_47_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(AlignedBox2D_47_Type);
    static Value_15_Concept = new Value_15_Concept(AlignedBox2D_47_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(AlignedBox2D_47_Type);
    static Value_15_Concept = new Value_15_Concept(AlignedBox2D_47_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(AlignedBox2D_47_Type);
    static Value_15_Concept = new Value_15_Concept(AlignedBox2D_47_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(AlignedBox2D_47_Type);
    static Value_15_Concept = new Value_15_Concept(AlignedBox2D_47_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(AlignedBox2D_47_Type);
    static Value_15_Concept = new Value_15_Concept(AlignedBox2D_47_Type);
    static Implements = [Interval_25_Concept,Vector_16_Concept,Array_14_Concept,Value_15_Concept,Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept];
}
class AlignedBox3D_48_Type
{
    constructor(A_883, B_890)
    {
        // field initialization 
        this.A_883 = A_883;
        this.B_890 = B_890;
        this.Min_3552 = AlignedBox3D_48_Type.Interval_25_Concept.Min_3552;
        this.Max_3555 = AlignedBox3D_48_Type.Interval_25_Concept.Max_3555;
        this.Count_3203 = AlignedBox3D_48_Type.Vector_16_Concept.Count_3203;
        this.At_3217 = AlignedBox3D_48_Type.Vector_16_Concept.At_3217;
        this.Count_3168 = AlignedBox3D_48_Type.Array_14_Concept.Count_3168;
        this.At_3173 = AlignedBox3D_48_Type.Array_14_Concept.At_3173;
        this.Default_3185 = AlignedBox3D_48_Type.Value_15_Concept.Default_3185;
        this.FieldTypes_3247 = AlignedBox3D_48_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = AlignedBox3D_48_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = AlignedBox3D_48_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = AlignedBox3D_48_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = AlignedBox3D_48_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = AlignedBox3D_48_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = AlignedBox3D_48_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = AlignedBox3D_48_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = AlignedBox3D_48_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = AlignedBox3D_48_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = AlignedBox3D_48_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = AlignedBox3D_48_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = AlignedBox3D_48_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = AlignedBox3D_48_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = AlignedBox3D_48_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = AlignedBox3D_48_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = AlignedBox3D_48_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = AlignedBox3D_48_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = AlignedBox3D_48_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = AlignedBox3D_48_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = AlignedBox3D_48_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = AlignedBox3D_48_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = AlignedBox3D_48_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = AlignedBox3D_48_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = AlignedBox3D_48_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = AlignedBox3D_48_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = AlignedBox3D_48_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = AlignedBox3D_48_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = AlignedBox3D_48_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = AlignedBox3D_48_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = AlignedBox3D_48_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = AlignedBox3D_48_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static A_883 = function(self) { return self.A_883; }
    static B_890 = function(self) { return self.B_890; }
    // implemented concepts 
    static Interval_25_Concept = new Interval_25_Concept(AlignedBox3D_48_Type);
    static Vector_16_Concept = new Vector_16_Concept(AlignedBox3D_48_Type);
    static Array_14_Concept = new Array_14_Concept(AlignedBox3D_48_Type);
    static Value_15_Concept = new Value_15_Concept(AlignedBox3D_48_Type);
    static Numerical_18_Concept = new Numerical_18_Concept(AlignedBox3D_48_Type);
    static Value_15_Concept = new Value_15_Concept(AlignedBox3D_48_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(AlignedBox3D_48_Type);
    static Value_15_Concept = new Value_15_Concept(AlignedBox3D_48_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(AlignedBox3D_48_Type);
    static Value_15_Concept = new Value_15_Concept(AlignedBox3D_48_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(AlignedBox3D_48_Type);
    static Value_15_Concept = new Value_15_Concept(AlignedBox3D_48_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(AlignedBox3D_48_Type);
    static Value_15_Concept = new Value_15_Concept(AlignedBox3D_48_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(AlignedBox3D_48_Type);
    static Value_15_Concept = new Value_15_Concept(AlignedBox3D_48_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(AlignedBox3D_48_Type);
    static Value_15_Concept = new Value_15_Concept(AlignedBox3D_48_Type);
    static Implements = [Interval_25_Concept,Vector_16_Concept,Array_14_Concept,Value_15_Concept,Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept];
}
class Complex_49_Type
{
    constructor(Real_897, Imaginary_904)
    {
        // field initialization 
        this.Real_897 = Real_897;
        this.Imaginary_904 = Imaginary_904;
        this.Count_3203 = Complex_49_Type.Vector_16_Concept.Count_3203;
        this.At_3217 = Complex_49_Type.Vector_16_Concept.At_3217;
        this.Count_3168 = Complex_49_Type.Array_14_Concept.Count_3168;
        this.At_3173 = Complex_49_Type.Array_14_Concept.At_3173;
        this.Default_3185 = Complex_49_Type.Value_15_Concept.Default_3185;
        this.FieldTypes_3247 = Complex_49_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = Complex_49_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = Complex_49_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = Complex_49_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = Complex_49_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = Complex_49_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Complex_49_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Complex_49_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Complex_49_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Complex_49_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Complex_49_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Complex_49_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Complex_49_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Complex_49_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Complex_49_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Complex_49_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Complex_49_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Complex_49_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Complex_49_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Complex_49_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Complex_49_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Complex_49_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Complex_49_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Complex_49_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Complex_49_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Complex_49_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Complex_49_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Complex_49_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Complex_49_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Complex_49_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Complex_49_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Complex_49_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Real_897 = function(self) { return self.Real_897; }
    static Imaginary_904 = function(self) { return self.Imaginary_904; }
    // implemented concepts 
    static Vector_16_Concept = new Vector_16_Concept(Complex_49_Type);
    static Array_14_Concept = new Array_14_Concept(Complex_49_Type);
    static Value_15_Concept = new Value_15_Concept(Complex_49_Type);
    static Numerical_18_Concept = new Numerical_18_Concept(Complex_49_Type);
    static Value_15_Concept = new Value_15_Concept(Complex_49_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Complex_49_Type);
    static Value_15_Concept = new Value_15_Concept(Complex_49_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Complex_49_Type);
    static Value_15_Concept = new Value_15_Concept(Complex_49_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Complex_49_Type);
    static Value_15_Concept = new Value_15_Concept(Complex_49_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Complex_49_Type);
    static Value_15_Concept = new Value_15_Concept(Complex_49_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Complex_49_Type);
    static Value_15_Concept = new Value_15_Concept(Complex_49_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Complex_49_Type);
    static Value_15_Concept = new Value_15_Concept(Complex_49_Type);
    static Implements = [Vector_16_Concept,Array_14_Concept,Value_15_Concept,Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept];
}
class Ray3D_50_Type
{
    constructor(Direction_911, Position_918)
    {
        // field initialization 
        this.Direction_911 = Direction_911;
        this.Position_918 = Position_918;
        this.Default_3185 = Ray3D_50_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Direction_911 = function(self) { return self.Direction_911; }
    static Position_918 = function(self) { return self.Position_918; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Ray3D_50_Type);
    static Implements = [Value_15_Concept];
}
class Ray2D_51_Type
{
    constructor(Direction_925, Position_932)
    {
        // field initialization 
        this.Direction_925 = Direction_925;
        this.Position_932 = Position_932;
        this.Default_3185 = Ray2D_51_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Direction_925 = function(self) { return self.Direction_925; }
    static Position_932 = function(self) { return self.Position_932; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Ray2D_51_Type);
    static Implements = [Value_15_Concept];
}
class Sphere_52_Type
{
    constructor(Center_939, Radius_946)
    {
        // field initialization 
        this.Center_939 = Center_939;
        this.Radius_946 = Radius_946;
        this.Default_3185 = Sphere_52_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Center_939 = function(self) { return self.Center_939; }
    static Radius_946 = function(self) { return self.Radius_946; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Sphere_52_Type);
    static Implements = [Value_15_Concept];
}
class Plane_53_Type
{
    constructor(Normal_953, D_960)
    {
        // field initialization 
        this.Normal_953 = Normal_953;
        this.D_960 = D_960;
        this.Default_3185 = Plane_53_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Normal_953 = function(self) { return self.Normal_953; }
    static D_960 = function(self) { return self.D_960; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Plane_53_Type);
    static Implements = [Value_15_Concept];
}
class Triangle3D_54_Type
{
    constructor(A_967, B_974, C_981)
    {
        // field initialization 
        this.A_967 = A_967;
        this.B_974 = B_974;
        this.C_981 = C_981;
        this.Default_3185 = Triangle3D_54_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static A_967 = function(self) { return self.A_967; }
    static B_974 = function(self) { return self.B_974; }
    static C_981 = function(self) { return self.C_981; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Triangle3D_54_Type);
    static Implements = [Value_15_Concept];
}
class Triangle2D_55_Type
{
    constructor(A_988, B_995, C_1002)
    {
        // field initialization 
        this.A_988 = A_988;
        this.B_995 = B_995;
        this.C_1002 = C_1002;
        this.Default_3185 = Triangle2D_55_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static A_988 = function(self) { return self.A_988; }
    static B_995 = function(self) { return self.B_995; }
    static C_1002 = function(self) { return self.C_1002; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Triangle2D_55_Type);
    static Implements = [Value_15_Concept];
}
class Quad3D_56_Type
{
    constructor(A_1009, B_1016, C_1023, D_1030)
    {
        // field initialization 
        this.A_1009 = A_1009;
        this.B_1016 = B_1016;
        this.C_1023 = C_1023;
        this.D_1030 = D_1030;
        this.Default_3185 = Quad3D_56_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static A_1009 = function(self) { return self.A_1009; }
    static B_1016 = function(self) { return self.B_1016; }
    static C_1023 = function(self) { return self.C_1023; }
    static D_1030 = function(self) { return self.D_1030; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Quad3D_56_Type);
    static Implements = [Value_15_Concept];
}
class Quad2D_57_Type
{
    constructor(A_1037, B_1044, C_1051, D_1058)
    {
        // field initialization 
        this.A_1037 = A_1037;
        this.B_1044 = B_1044;
        this.C_1051 = C_1051;
        this.D_1058 = D_1058;
        this.Default_3185 = Quad2D_57_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static A_1037 = function(self) { return self.A_1037; }
    static B_1044 = function(self) { return self.B_1044; }
    static C_1051 = function(self) { return self.C_1051; }
    static D_1058 = function(self) { return self.D_1058; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Quad2D_57_Type);
    static Implements = [Value_15_Concept];
}
class Point3D_58_Type
{
    constructor(Value_1065)
    {
        // field initialization 
        this.Value_1065 = Value_1065;
        this.Default_3185 = Point3D_58_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Value_1065 = function(self) { return self.Value_1065; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Point3D_58_Type);
    static Implements = [Value_15_Concept];
}
class Point2D_59_Type
{
    constructor(Value_1072)
    {
        // field initialization 
        this.Value_1072 = Value_1072;
        this.Default_3185 = Point2D_59_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Value_1072 = function(self) { return self.Value_1072; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Point2D_59_Type);
    static Implements = [Value_15_Concept];
}
class Line3D_60_Type
{
    constructor(A_1079, B_1086)
    {
        // field initialization 
        this.A_1079 = A_1079;
        this.B_1086 = B_1086;
        this.Min_3552 = Line3D_60_Type.Interval_25_Concept.Min_3552;
        this.Max_3555 = Line3D_60_Type.Interval_25_Concept.Max_3555;
        this.Count_3203 = Line3D_60_Type.Vector_16_Concept.Count_3203;
        this.At_3217 = Line3D_60_Type.Vector_16_Concept.At_3217;
        this.Count_3168 = Line3D_60_Type.Array_14_Concept.Count_3168;
        this.At_3173 = Line3D_60_Type.Array_14_Concept.At_3173;
        this.Default_3185 = Line3D_60_Type.Value_15_Concept.Default_3185;
        this.FieldTypes_3247 = Line3D_60_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = Line3D_60_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = Line3D_60_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = Line3D_60_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = Line3D_60_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = Line3D_60_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Line3D_60_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Line3D_60_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Line3D_60_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Line3D_60_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Line3D_60_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Line3D_60_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Line3D_60_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Line3D_60_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Line3D_60_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Line3D_60_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Line3D_60_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Line3D_60_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Line3D_60_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Line3D_60_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Line3D_60_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Line3D_60_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Line3D_60_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Line3D_60_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Line3D_60_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Line3D_60_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Line3D_60_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Line3D_60_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Line3D_60_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Line3D_60_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Line3D_60_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Line3D_60_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static A_1079 = function(self) { return self.A_1079; }
    static B_1086 = function(self) { return self.B_1086; }
    // implemented concepts 
    static Interval_25_Concept = new Interval_25_Concept(Line3D_60_Type);
    static Vector_16_Concept = new Vector_16_Concept(Line3D_60_Type);
    static Array_14_Concept = new Array_14_Concept(Line3D_60_Type);
    static Value_15_Concept = new Value_15_Concept(Line3D_60_Type);
    static Numerical_18_Concept = new Numerical_18_Concept(Line3D_60_Type);
    static Value_15_Concept = new Value_15_Concept(Line3D_60_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Line3D_60_Type);
    static Value_15_Concept = new Value_15_Concept(Line3D_60_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Line3D_60_Type);
    static Value_15_Concept = new Value_15_Concept(Line3D_60_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Line3D_60_Type);
    static Value_15_Concept = new Value_15_Concept(Line3D_60_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Line3D_60_Type);
    static Value_15_Concept = new Value_15_Concept(Line3D_60_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Line3D_60_Type);
    static Value_15_Concept = new Value_15_Concept(Line3D_60_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Line3D_60_Type);
    static Value_15_Concept = new Value_15_Concept(Line3D_60_Type);
    static Implements = [Interval_25_Concept,Vector_16_Concept,Array_14_Concept,Value_15_Concept,Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept];
}
class Line2D_61_Type
{
    constructor(A_1093, B_1100)
    {
        // field initialization 
        this.A_1093 = A_1093;
        this.B_1100 = B_1100;
        this.Min_3552 = Line2D_61_Type.Interval_25_Concept.Min_3552;
        this.Max_3555 = Line2D_61_Type.Interval_25_Concept.Max_3555;
        this.Count_3203 = Line2D_61_Type.Vector_16_Concept.Count_3203;
        this.At_3217 = Line2D_61_Type.Vector_16_Concept.At_3217;
        this.Count_3168 = Line2D_61_Type.Array_14_Concept.Count_3168;
        this.At_3173 = Line2D_61_Type.Array_14_Concept.At_3173;
        this.Default_3185 = Line2D_61_Type.Value_15_Concept.Default_3185;
        this.FieldTypes_3247 = Line2D_61_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = Line2D_61_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = Line2D_61_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = Line2D_61_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = Line2D_61_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = Line2D_61_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Line2D_61_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Line2D_61_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Line2D_61_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Line2D_61_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Line2D_61_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Line2D_61_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Line2D_61_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Line2D_61_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Line2D_61_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Line2D_61_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Line2D_61_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Line2D_61_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Line2D_61_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Line2D_61_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Line2D_61_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Line2D_61_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Line2D_61_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Line2D_61_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Line2D_61_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Line2D_61_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Line2D_61_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Line2D_61_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Line2D_61_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Line2D_61_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Line2D_61_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Line2D_61_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static A_1093 = function(self) { return self.A_1093; }
    static B_1100 = function(self) { return self.B_1100; }
    // implemented concepts 
    static Interval_25_Concept = new Interval_25_Concept(Line2D_61_Type);
    static Vector_16_Concept = new Vector_16_Concept(Line2D_61_Type);
    static Array_14_Concept = new Array_14_Concept(Line2D_61_Type);
    static Value_15_Concept = new Value_15_Concept(Line2D_61_Type);
    static Numerical_18_Concept = new Numerical_18_Concept(Line2D_61_Type);
    static Value_15_Concept = new Value_15_Concept(Line2D_61_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Line2D_61_Type);
    static Value_15_Concept = new Value_15_Concept(Line2D_61_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Line2D_61_Type);
    static Value_15_Concept = new Value_15_Concept(Line2D_61_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Line2D_61_Type);
    static Value_15_Concept = new Value_15_Concept(Line2D_61_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Line2D_61_Type);
    static Value_15_Concept = new Value_15_Concept(Line2D_61_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Line2D_61_Type);
    static Value_15_Concept = new Value_15_Concept(Line2D_61_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Line2D_61_Type);
    static Value_15_Concept = new Value_15_Concept(Line2D_61_Type);
    static Implements = [Interval_25_Concept,Vector_16_Concept,Array_14_Concept,Value_15_Concept,Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept];
}
class Color_62_Type
{
    constructor(R_1107, G_1114, B_1121, A_1128)
    {
        // field initialization 
        this.R_1107 = R_1107;
        this.G_1114 = G_1114;
        this.B_1121 = B_1121;
        this.A_1128 = A_1128;
        this.Default_3185 = Color_62_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static R_1107 = function(self) { return self.R_1107; }
    static G_1114 = function(self) { return self.G_1114; }
    static B_1121 = function(self) { return self.B_1121; }
    static A_1128 = function(self) { return self.A_1128; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Color_62_Type);
    static Implements = [Value_15_Concept];
}
class ColorLUV_63_Type
{
    constructor(Lightness_1135, U_1142, V_1149)
    {
        // field initialization 
        this.Lightness_1135 = Lightness_1135;
        this.U_1142 = U_1142;
        this.V_1149 = V_1149;
        this.Default_3185 = ColorLUV_63_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Lightness_1135 = function(self) { return self.Lightness_1135; }
    static U_1142 = function(self) { return self.U_1142; }
    static V_1149 = function(self) { return self.V_1149; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(ColorLUV_63_Type);
    static Implements = [Value_15_Concept];
}
class ColorLAB_64_Type
{
    constructor(Lightness_1156, A_1163, B_1170)
    {
        // field initialization 
        this.Lightness_1156 = Lightness_1156;
        this.A_1163 = A_1163;
        this.B_1170 = B_1170;
        this.Default_3185 = ColorLAB_64_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Lightness_1156 = function(self) { return self.Lightness_1156; }
    static A_1163 = function(self) { return self.A_1163; }
    static B_1170 = function(self) { return self.B_1170; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(ColorLAB_64_Type);
    static Implements = [Value_15_Concept];
}
class ColorLCh_65_Type
{
    constructor(Lightness_1177, ChromaHue_1184)
    {
        // field initialization 
        this.Lightness_1177 = Lightness_1177;
        this.ChromaHue_1184 = ChromaHue_1184;
        this.Default_3185 = ColorLCh_65_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Lightness_1177 = function(self) { return self.Lightness_1177; }
    static ChromaHue_1184 = function(self) { return self.ChromaHue_1184; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(ColorLCh_65_Type);
    static Implements = [Value_15_Concept];
}
class ColorHSV_66_Type
{
    constructor(Hue_1191, S_1198, V_1205)
    {
        // field initialization 
        this.Hue_1191 = Hue_1191;
        this.S_1198 = S_1198;
        this.V_1205 = V_1205;
        this.Default_3185 = ColorHSV_66_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Hue_1191 = function(self) { return self.Hue_1191; }
    static S_1198 = function(self) { return self.S_1198; }
    static V_1205 = function(self) { return self.V_1205; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(ColorHSV_66_Type);
    static Implements = [Value_15_Concept];
}
class ColorHSL_67_Type
{
    constructor(Hue_1212, Saturation_1219, Luminance_1226)
    {
        // field initialization 
        this.Hue_1212 = Hue_1212;
        this.Saturation_1219 = Saturation_1219;
        this.Luminance_1226 = Luminance_1226;
        this.Default_3185 = ColorHSL_67_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Hue_1212 = function(self) { return self.Hue_1212; }
    static Saturation_1219 = function(self) { return self.Saturation_1219; }
    static Luminance_1226 = function(self) { return self.Luminance_1226; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(ColorHSL_67_Type);
    static Implements = [Value_15_Concept];
}
class ColorYCbCr_68_Type
{
    constructor(Y_1233, Cb_1240, Cr_1247)
    {
        // field initialization 
        this.Y_1233 = Y_1233;
        this.Cb_1240 = Cb_1240;
        this.Cr_1247 = Cr_1247;
        this.Default_3185 = ColorYCbCr_68_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Y_1233 = function(self) { return self.Y_1233; }
    static Cb_1240 = function(self) { return self.Cb_1240; }
    static Cr_1247 = function(self) { return self.Cr_1247; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(ColorYCbCr_68_Type);
    static Implements = [Value_15_Concept];
}
class SphericalCoordinate_69_Type
{
    constructor(Radius_1254, Azimuth_1261, Polar_1268)
    {
        // field initialization 
        this.Radius_1254 = Radius_1254;
        this.Azimuth_1261 = Azimuth_1261;
        this.Polar_1268 = Polar_1268;
        this.Default_3185 = SphericalCoordinate_69_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Radius_1254 = function(self) { return self.Radius_1254; }
    static Azimuth_1261 = function(self) { return self.Azimuth_1261; }
    static Polar_1268 = function(self) { return self.Polar_1268; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(SphericalCoordinate_69_Type);
    static Implements = [Value_15_Concept];
}
class PolarCoordinate_70_Type
{
    constructor(Radius_1275, Angle_1282)
    {
        // field initialization 
        this.Radius_1275 = Radius_1275;
        this.Angle_1282 = Angle_1282;
        this.Default_3185 = PolarCoordinate_70_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Radius_1275 = function(self) { return self.Radius_1275; }
    static Angle_1282 = function(self) { return self.Angle_1282; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(PolarCoordinate_70_Type);
    static Implements = [Value_15_Concept];
}
class LogPolarCoordinate_71_Type
{
    constructor(Rho_1289, Azimuth_1296)
    {
        // field initialization 
        this.Rho_1289 = Rho_1289;
        this.Azimuth_1296 = Azimuth_1296;
        this.Default_3185 = LogPolarCoordinate_71_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Rho_1289 = function(self) { return self.Rho_1289; }
    static Azimuth_1296 = function(self) { return self.Azimuth_1296; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(LogPolarCoordinate_71_Type);
    static Implements = [Value_15_Concept];
}
class CylindricalCoordinate_72_Type
{
    constructor(RadialDistance_1303, Azimuth_1310, Height_1317)
    {
        // field initialization 
        this.RadialDistance_1303 = RadialDistance_1303;
        this.Azimuth_1310 = Azimuth_1310;
        this.Height_1317 = Height_1317;
        this.Default_3185 = CylindricalCoordinate_72_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static RadialDistance_1303 = function(self) { return self.RadialDistance_1303; }
    static Azimuth_1310 = function(self) { return self.Azimuth_1310; }
    static Height_1317 = function(self) { return self.Height_1317; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(CylindricalCoordinate_72_Type);
    static Implements = [Value_15_Concept];
}
class HorizontalCoordinate_73_Type
{
    constructor(Radius_1324, Azimuth_1331, Height_1338)
    {
        // field initialization 
        this.Radius_1324 = Radius_1324;
        this.Azimuth_1331 = Azimuth_1331;
        this.Height_1338 = Height_1338;
        this.Default_3185 = HorizontalCoordinate_73_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Radius_1324 = function(self) { return self.Radius_1324; }
    static Azimuth_1331 = function(self) { return self.Azimuth_1331; }
    static Height_1338 = function(self) { return self.Height_1338; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(HorizontalCoordinate_73_Type);
    static Implements = [Value_15_Concept];
}
class GeoCoordinate_74_Type
{
    constructor(Latitude_1345, Longitude_1352)
    {
        // field initialization 
        this.Latitude_1345 = Latitude_1345;
        this.Longitude_1352 = Longitude_1352;
        this.Default_3185 = GeoCoordinate_74_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Latitude_1345 = function(self) { return self.Latitude_1345; }
    static Longitude_1352 = function(self) { return self.Longitude_1352; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(GeoCoordinate_74_Type);
    static Implements = [Value_15_Concept];
}
class GeoCoordinateWithAltitude_75_Type
{
    constructor(Coordinate_1359, Altitude_1366)
    {
        // field initialization 
        this.Coordinate_1359 = Coordinate_1359;
        this.Altitude_1366 = Altitude_1366;
        this.Default_3185 = GeoCoordinateWithAltitude_75_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Coordinate_1359 = function(self) { return self.Coordinate_1359; }
    static Altitude_1366 = function(self) { return self.Altitude_1366; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(GeoCoordinateWithAltitude_75_Type);
    static Implements = [Value_15_Concept];
}
class Circle_76_Type
{
    constructor(Center_1373, Radius_1380)
    {
        // field initialization 
        this.Center_1373 = Center_1373;
        this.Radius_1380 = Radius_1380;
        this.Default_3185 = Circle_76_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Center_1373 = function(self) { return self.Center_1373; }
    static Radius_1380 = function(self) { return self.Radius_1380; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Circle_76_Type);
    static Implements = [Value_15_Concept];
}
class Chord_77_Type
{
    constructor(Circle_1387, Arc_1394)
    {
        // field initialization 
        this.Circle_1387 = Circle_1387;
        this.Arc_1394 = Arc_1394;
        this.Default_3185 = Chord_77_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Circle_1387 = function(self) { return self.Circle_1387; }
    static Arc_1394 = function(self) { return self.Arc_1394; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Chord_77_Type);
    static Implements = [Value_15_Concept];
}
class Size2D_78_Type
{
    constructor(Width_1401, Height_1408)
    {
        // field initialization 
        this.Width_1401 = Width_1401;
        this.Height_1408 = Height_1408;
        this.Default_3185 = Size2D_78_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Width_1401 = function(self) { return self.Width_1401; }
    static Height_1408 = function(self) { return self.Height_1408; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Size2D_78_Type);
    static Implements = [Value_15_Concept];
}
class Size3D_79_Type
{
    constructor(Width_1415, Height_1422, Depth_1429)
    {
        // field initialization 
        this.Width_1415 = Width_1415;
        this.Height_1422 = Height_1422;
        this.Depth_1429 = Depth_1429;
        this.Default_3185 = Size3D_79_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Width_1415 = function(self) { return self.Width_1415; }
    static Height_1422 = function(self) { return self.Height_1422; }
    static Depth_1429 = function(self) { return self.Depth_1429; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Size3D_79_Type);
    static Implements = [Value_15_Concept];
}
class Rectangle2D_80_Type
{
    constructor(Center_1436, Size_1443)
    {
        // field initialization 
        this.Center_1436 = Center_1436;
        this.Size_1443 = Size_1443;
        this.Default_3185 = Rectangle2D_80_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Center_1436 = function(self) { return self.Center_1436; }
    static Size_1443 = function(self) { return self.Size_1443; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Rectangle2D_80_Type);
    static Implements = [Value_15_Concept];
}
class Proportion_81_Type
{
    constructor(Value_1450)
    {
        // field initialization 
        this.Value_1450 = Value_1450;
        this.FieldTypes_3247 = Proportion_81_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = Proportion_81_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = Proportion_81_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = Proportion_81_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = Proportion_81_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = Proportion_81_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Proportion_81_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Proportion_81_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Proportion_81_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Proportion_81_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Proportion_81_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Proportion_81_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Proportion_81_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Proportion_81_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Proportion_81_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Proportion_81_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Proportion_81_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Proportion_81_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Proportion_81_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Proportion_81_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Proportion_81_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Proportion_81_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Proportion_81_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Proportion_81_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Proportion_81_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Value_1450 = function(self) { return self.Value_1450; }
    // implemented concepts 
    static Numerical_18_Concept = new Numerical_18_Concept(Proportion_81_Type);
    static Value_15_Concept = new Value_15_Concept(Proportion_81_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Proportion_81_Type);
    static Value_15_Concept = new Value_15_Concept(Proportion_81_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Proportion_81_Type);
    static Value_15_Concept = new Value_15_Concept(Proportion_81_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Proportion_81_Type);
    static Value_15_Concept = new Value_15_Concept(Proportion_81_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Proportion_81_Type);
    static Value_15_Concept = new Value_15_Concept(Proportion_81_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Proportion_81_Type);
    static Value_15_Concept = new Value_15_Concept(Proportion_81_Type);
    static Implements = [Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept];
}
class Fraction_82_Type
{
    constructor(Numerator_1457, Denominator_1464)
    {
        // field initialization 
        this.Numerator_1457 = Numerator_1457;
        this.Denominator_1464 = Denominator_1464;
        this.Default_3185 = Fraction_82_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Numerator_1457 = function(self) { return self.Numerator_1457; }
    static Denominator_1464 = function(self) { return self.Denominator_1464; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Fraction_82_Type);
    static Implements = [Value_15_Concept];
}
class Angle_83_Type
{
    constructor(Radians_1471)
    {
        // field initialization 
        this.Radians_1471 = Radians_1471;
        this.Value_3236 = Angle_83_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = Angle_83_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Angle_83_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Angle_83_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Angle_83_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Angle_83_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Angle_83_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Angle_83_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Angle_83_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Angle_83_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Angle_83_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Angle_83_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Angle_83_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Angle_83_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Radians_1471 = function(self) { return self.Radians_1471; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(Angle_83_Type);
    static Value_15_Concept = new Value_15_Concept(Angle_83_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Angle_83_Type);
    static Value_15_Concept = new Value_15_Concept(Angle_83_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Angle_83_Type);
    static Value_15_Concept = new Value_15_Concept(Angle_83_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Angle_83_Type);
    static Value_15_Concept = new Value_15_Concept(Angle_83_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Angle_83_Type);
    static Value_15_Concept = new Value_15_Concept(Angle_83_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class Length_84_Type
{
    constructor(Meters_1478)
    {
        // field initialization 
        this.Meters_1478 = Meters_1478;
        this.Value_3236 = Length_84_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = Length_84_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Length_84_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Length_84_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Length_84_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Length_84_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Length_84_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Length_84_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Length_84_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Length_84_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Length_84_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Length_84_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Length_84_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Length_84_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Meters_1478 = function(self) { return self.Meters_1478; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(Length_84_Type);
    static Value_15_Concept = new Value_15_Concept(Length_84_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Length_84_Type);
    static Value_15_Concept = new Value_15_Concept(Length_84_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Length_84_Type);
    static Value_15_Concept = new Value_15_Concept(Length_84_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Length_84_Type);
    static Value_15_Concept = new Value_15_Concept(Length_84_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Length_84_Type);
    static Value_15_Concept = new Value_15_Concept(Length_84_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class Mass_85_Type
{
    constructor(Kilograms_1485)
    {
        // field initialization 
        this.Kilograms_1485 = Kilograms_1485;
        this.Value_3236 = Mass_85_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = Mass_85_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Mass_85_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Mass_85_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Mass_85_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Mass_85_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Mass_85_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Mass_85_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Mass_85_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Mass_85_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Mass_85_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Mass_85_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Mass_85_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Mass_85_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Kilograms_1485 = function(self) { return self.Kilograms_1485; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(Mass_85_Type);
    static Value_15_Concept = new Value_15_Concept(Mass_85_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Mass_85_Type);
    static Value_15_Concept = new Value_15_Concept(Mass_85_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Mass_85_Type);
    static Value_15_Concept = new Value_15_Concept(Mass_85_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Mass_85_Type);
    static Value_15_Concept = new Value_15_Concept(Mass_85_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Mass_85_Type);
    static Value_15_Concept = new Value_15_Concept(Mass_85_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class Temperature_86_Type
{
    constructor(Celsius_1492)
    {
        // field initialization 
        this.Celsius_1492 = Celsius_1492;
        this.Value_3236 = Temperature_86_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = Temperature_86_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Temperature_86_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Temperature_86_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Temperature_86_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Temperature_86_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Temperature_86_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Temperature_86_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Temperature_86_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Temperature_86_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Temperature_86_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Temperature_86_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Temperature_86_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Temperature_86_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Celsius_1492 = function(self) { return self.Celsius_1492; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(Temperature_86_Type);
    static Value_15_Concept = new Value_15_Concept(Temperature_86_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Temperature_86_Type);
    static Value_15_Concept = new Value_15_Concept(Temperature_86_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Temperature_86_Type);
    static Value_15_Concept = new Value_15_Concept(Temperature_86_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Temperature_86_Type);
    static Value_15_Concept = new Value_15_Concept(Temperature_86_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Temperature_86_Type);
    static Value_15_Concept = new Value_15_Concept(Temperature_86_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class TimeSpan_87_Type
{
    constructor(Seconds_1499)
    {
        // field initialization 
        this.Seconds_1499 = Seconds_1499;
        this.Value_3236 = TimeSpan_87_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = TimeSpan_87_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = TimeSpan_87_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = TimeSpan_87_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = TimeSpan_87_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = TimeSpan_87_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = TimeSpan_87_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = TimeSpan_87_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = TimeSpan_87_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = TimeSpan_87_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = TimeSpan_87_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = TimeSpan_87_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = TimeSpan_87_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = TimeSpan_87_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Seconds_1499 = function(self) { return self.Seconds_1499; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(TimeSpan_87_Type);
    static Value_15_Concept = new Value_15_Concept(TimeSpan_87_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(TimeSpan_87_Type);
    static Value_15_Concept = new Value_15_Concept(TimeSpan_87_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(TimeSpan_87_Type);
    static Value_15_Concept = new Value_15_Concept(TimeSpan_87_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(TimeSpan_87_Type);
    static Value_15_Concept = new Value_15_Concept(TimeSpan_87_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(TimeSpan_87_Type);
    static Value_15_Concept = new Value_15_Concept(TimeSpan_87_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class TimeRange_88_Type
{
    constructor(Min_1506, Max_1513)
    {
        // field initialization 
        this.Min_1506 = Min_1506;
        this.Max_1513 = Max_1513;
        this.Min_3552 = TimeRange_88_Type.Interval_25_Concept.Min_3552;
        this.Max_3555 = TimeRange_88_Type.Interval_25_Concept.Max_3555;
        this.Count_3203 = TimeRange_88_Type.Vector_16_Concept.Count_3203;
        this.At_3217 = TimeRange_88_Type.Vector_16_Concept.At_3217;
        this.Count_3168 = TimeRange_88_Type.Array_14_Concept.Count_3168;
        this.At_3173 = TimeRange_88_Type.Array_14_Concept.At_3173;
        this.Default_3185 = TimeRange_88_Type.Value_15_Concept.Default_3185;
        this.FieldTypes_3247 = TimeRange_88_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = TimeRange_88_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = TimeRange_88_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = TimeRange_88_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = TimeRange_88_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = TimeRange_88_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = TimeRange_88_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = TimeRange_88_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = TimeRange_88_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = TimeRange_88_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = TimeRange_88_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = TimeRange_88_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = TimeRange_88_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = TimeRange_88_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = TimeRange_88_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = TimeRange_88_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = TimeRange_88_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = TimeRange_88_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = TimeRange_88_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = TimeRange_88_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = TimeRange_88_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = TimeRange_88_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = TimeRange_88_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = TimeRange_88_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = TimeRange_88_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = TimeRange_88_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = TimeRange_88_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = TimeRange_88_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = TimeRange_88_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = TimeRange_88_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = TimeRange_88_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = TimeRange_88_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Min_1506 = function(self) { return self.Min_1506; }
    static Max_1513 = function(self) { return self.Max_1513; }
    // implemented concepts 
    static Interval_25_Concept = new Interval_25_Concept(TimeRange_88_Type);
    static Vector_16_Concept = new Vector_16_Concept(TimeRange_88_Type);
    static Array_14_Concept = new Array_14_Concept(TimeRange_88_Type);
    static Value_15_Concept = new Value_15_Concept(TimeRange_88_Type);
    static Numerical_18_Concept = new Numerical_18_Concept(TimeRange_88_Type);
    static Value_15_Concept = new Value_15_Concept(TimeRange_88_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(TimeRange_88_Type);
    static Value_15_Concept = new Value_15_Concept(TimeRange_88_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(TimeRange_88_Type);
    static Value_15_Concept = new Value_15_Concept(TimeRange_88_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(TimeRange_88_Type);
    static Value_15_Concept = new Value_15_Concept(TimeRange_88_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(TimeRange_88_Type);
    static Value_15_Concept = new Value_15_Concept(TimeRange_88_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(TimeRange_88_Type);
    static Value_15_Concept = new Value_15_Concept(TimeRange_88_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(TimeRange_88_Type);
    static Value_15_Concept = new Value_15_Concept(TimeRange_88_Type);
    static Implements = [Interval_25_Concept,Vector_16_Concept,Array_14_Concept,Value_15_Concept,Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept];
}
class DateTime_89_Type
{
    constructor()
    {
        // field initialization 
        this.Default_3185 = DateTime_89_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(DateTime_89_Type);
    static Implements = [Value_15_Concept];
}
class AnglePair_90_Type
{
    constructor(Start_1520, End_1527)
    {
        // field initialization 
        this.Start_1520 = Start_1520;
        this.End_1527 = End_1527;
        this.Min_3552 = AnglePair_90_Type.Interval_25_Concept.Min_3552;
        this.Max_3555 = AnglePair_90_Type.Interval_25_Concept.Max_3555;
        this.Count_3203 = AnglePair_90_Type.Vector_16_Concept.Count_3203;
        this.At_3217 = AnglePair_90_Type.Vector_16_Concept.At_3217;
        this.Count_3168 = AnglePair_90_Type.Array_14_Concept.Count_3168;
        this.At_3173 = AnglePair_90_Type.Array_14_Concept.At_3173;
        this.Default_3185 = AnglePair_90_Type.Value_15_Concept.Default_3185;
        this.FieldTypes_3247 = AnglePair_90_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = AnglePair_90_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = AnglePair_90_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = AnglePair_90_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = AnglePair_90_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = AnglePair_90_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = AnglePair_90_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = AnglePair_90_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = AnglePair_90_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = AnglePair_90_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = AnglePair_90_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = AnglePair_90_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = AnglePair_90_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = AnglePair_90_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = AnglePair_90_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = AnglePair_90_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = AnglePair_90_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = AnglePair_90_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = AnglePair_90_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = AnglePair_90_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = AnglePair_90_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = AnglePair_90_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = AnglePair_90_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = AnglePair_90_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = AnglePair_90_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = AnglePair_90_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = AnglePair_90_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = AnglePair_90_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = AnglePair_90_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = AnglePair_90_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = AnglePair_90_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = AnglePair_90_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Start_1520 = function(self) { return self.Start_1520; }
    static End_1527 = function(self) { return self.End_1527; }
    // implemented concepts 
    static Interval_25_Concept = new Interval_25_Concept(AnglePair_90_Type);
    static Vector_16_Concept = new Vector_16_Concept(AnglePair_90_Type);
    static Array_14_Concept = new Array_14_Concept(AnglePair_90_Type);
    static Value_15_Concept = new Value_15_Concept(AnglePair_90_Type);
    static Numerical_18_Concept = new Numerical_18_Concept(AnglePair_90_Type);
    static Value_15_Concept = new Value_15_Concept(AnglePair_90_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(AnglePair_90_Type);
    static Value_15_Concept = new Value_15_Concept(AnglePair_90_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(AnglePair_90_Type);
    static Value_15_Concept = new Value_15_Concept(AnglePair_90_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(AnglePair_90_Type);
    static Value_15_Concept = new Value_15_Concept(AnglePair_90_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(AnglePair_90_Type);
    static Value_15_Concept = new Value_15_Concept(AnglePair_90_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(AnglePair_90_Type);
    static Value_15_Concept = new Value_15_Concept(AnglePair_90_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(AnglePair_90_Type);
    static Value_15_Concept = new Value_15_Concept(AnglePair_90_Type);
    static Implements = [Interval_25_Concept,Vector_16_Concept,Array_14_Concept,Value_15_Concept,Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept];
}
class Ring_91_Type
{
    constructor(Circle_1534, InnerRadius_1541)
    {
        // field initialization 
        this.Circle_1534 = Circle_1534;
        this.InnerRadius_1541 = InnerRadius_1541;
        this.FieldTypes_3247 = Ring_91_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = Ring_91_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = Ring_91_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = Ring_91_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = Ring_91_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = Ring_91_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Ring_91_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Ring_91_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Ring_91_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Ring_91_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Ring_91_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Ring_91_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Ring_91_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Ring_91_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Ring_91_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Ring_91_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Ring_91_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Ring_91_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Ring_91_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Ring_91_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Ring_91_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Ring_91_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Ring_91_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Ring_91_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Ring_91_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Circle_1534 = function(self) { return self.Circle_1534; }
    static InnerRadius_1541 = function(self) { return self.InnerRadius_1541; }
    // implemented concepts 
    static Numerical_18_Concept = new Numerical_18_Concept(Ring_91_Type);
    static Value_15_Concept = new Value_15_Concept(Ring_91_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Ring_91_Type);
    static Value_15_Concept = new Value_15_Concept(Ring_91_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Ring_91_Type);
    static Value_15_Concept = new Value_15_Concept(Ring_91_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Ring_91_Type);
    static Value_15_Concept = new Value_15_Concept(Ring_91_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Ring_91_Type);
    static Value_15_Concept = new Value_15_Concept(Ring_91_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Ring_91_Type);
    static Value_15_Concept = new Value_15_Concept(Ring_91_Type);
    static Implements = [Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept];
}
class Arc_92_Type
{
    constructor(Angles_1548, Cirlce_1555)
    {
        // field initialization 
        this.Angles_1548 = Angles_1548;
        this.Cirlce_1555 = Cirlce_1555;
        this.Default_3185 = Arc_92_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Angles_1548 = function(self) { return self.Angles_1548; }
    static Cirlce_1555 = function(self) { return self.Cirlce_1555; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Arc_92_Type);
    static Implements = [Value_15_Concept];
}
class TimeInterval_93_Type
{
    constructor(Start_1562, End_1569)
    {
        // field initialization 
        this.Start_1562 = Start_1562;
        this.End_1569 = End_1569;
        this.Min_3552 = TimeInterval_93_Type.Interval_25_Concept.Min_3552;
        this.Max_3555 = TimeInterval_93_Type.Interval_25_Concept.Max_3555;
        this.Count_3203 = TimeInterval_93_Type.Vector_16_Concept.Count_3203;
        this.At_3217 = TimeInterval_93_Type.Vector_16_Concept.At_3217;
        this.Count_3168 = TimeInterval_93_Type.Array_14_Concept.Count_3168;
        this.At_3173 = TimeInterval_93_Type.Array_14_Concept.At_3173;
        this.Default_3185 = TimeInterval_93_Type.Value_15_Concept.Default_3185;
        this.FieldTypes_3247 = TimeInterval_93_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = TimeInterval_93_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = TimeInterval_93_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = TimeInterval_93_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = TimeInterval_93_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = TimeInterval_93_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = TimeInterval_93_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = TimeInterval_93_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = TimeInterval_93_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = TimeInterval_93_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = TimeInterval_93_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = TimeInterval_93_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = TimeInterval_93_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = TimeInterval_93_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = TimeInterval_93_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = TimeInterval_93_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = TimeInterval_93_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = TimeInterval_93_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = TimeInterval_93_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = TimeInterval_93_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = TimeInterval_93_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = TimeInterval_93_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = TimeInterval_93_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = TimeInterval_93_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = TimeInterval_93_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = TimeInterval_93_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = TimeInterval_93_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = TimeInterval_93_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = TimeInterval_93_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = TimeInterval_93_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = TimeInterval_93_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = TimeInterval_93_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Start_1562 = function(self) { return self.Start_1562; }
    static End_1569 = function(self) { return self.End_1569; }
    // implemented concepts 
    static Interval_25_Concept = new Interval_25_Concept(TimeInterval_93_Type);
    static Vector_16_Concept = new Vector_16_Concept(TimeInterval_93_Type);
    static Array_14_Concept = new Array_14_Concept(TimeInterval_93_Type);
    static Value_15_Concept = new Value_15_Concept(TimeInterval_93_Type);
    static Numerical_18_Concept = new Numerical_18_Concept(TimeInterval_93_Type);
    static Value_15_Concept = new Value_15_Concept(TimeInterval_93_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(TimeInterval_93_Type);
    static Value_15_Concept = new Value_15_Concept(TimeInterval_93_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(TimeInterval_93_Type);
    static Value_15_Concept = new Value_15_Concept(TimeInterval_93_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(TimeInterval_93_Type);
    static Value_15_Concept = new Value_15_Concept(TimeInterval_93_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(TimeInterval_93_Type);
    static Value_15_Concept = new Value_15_Concept(TimeInterval_93_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(TimeInterval_93_Type);
    static Value_15_Concept = new Value_15_Concept(TimeInterval_93_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(TimeInterval_93_Type);
    static Value_15_Concept = new Value_15_Concept(TimeInterval_93_Type);
    static Implements = [Interval_25_Concept,Vector_16_Concept,Array_14_Concept,Value_15_Concept,Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept];
}
class RealInterval_94_Type
{
    constructor(A_1576, B_1583)
    {
        // field initialization 
        this.A_1576 = A_1576;
        this.B_1583 = B_1583;
        this.Min_3552 = RealInterval_94_Type.Interval_25_Concept.Min_3552;
        this.Max_3555 = RealInterval_94_Type.Interval_25_Concept.Max_3555;
        this.Count_3203 = RealInterval_94_Type.Vector_16_Concept.Count_3203;
        this.At_3217 = RealInterval_94_Type.Vector_16_Concept.At_3217;
        this.Count_3168 = RealInterval_94_Type.Array_14_Concept.Count_3168;
        this.At_3173 = RealInterval_94_Type.Array_14_Concept.At_3173;
        this.Default_3185 = RealInterval_94_Type.Value_15_Concept.Default_3185;
        this.FieldTypes_3247 = RealInterval_94_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = RealInterval_94_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = RealInterval_94_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = RealInterval_94_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = RealInterval_94_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = RealInterval_94_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = RealInterval_94_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = RealInterval_94_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = RealInterval_94_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = RealInterval_94_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = RealInterval_94_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = RealInterval_94_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = RealInterval_94_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = RealInterval_94_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = RealInterval_94_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = RealInterval_94_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = RealInterval_94_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = RealInterval_94_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = RealInterval_94_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = RealInterval_94_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = RealInterval_94_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = RealInterval_94_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = RealInterval_94_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = RealInterval_94_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = RealInterval_94_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = RealInterval_94_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = RealInterval_94_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = RealInterval_94_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = RealInterval_94_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = RealInterval_94_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = RealInterval_94_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = RealInterval_94_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static A_1576 = function(self) { return self.A_1576; }
    static B_1583 = function(self) { return self.B_1583; }
    // implemented concepts 
    static Interval_25_Concept = new Interval_25_Concept(RealInterval_94_Type);
    static Vector_16_Concept = new Vector_16_Concept(RealInterval_94_Type);
    static Array_14_Concept = new Array_14_Concept(RealInterval_94_Type);
    static Value_15_Concept = new Value_15_Concept(RealInterval_94_Type);
    static Numerical_18_Concept = new Numerical_18_Concept(RealInterval_94_Type);
    static Value_15_Concept = new Value_15_Concept(RealInterval_94_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(RealInterval_94_Type);
    static Value_15_Concept = new Value_15_Concept(RealInterval_94_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(RealInterval_94_Type);
    static Value_15_Concept = new Value_15_Concept(RealInterval_94_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(RealInterval_94_Type);
    static Value_15_Concept = new Value_15_Concept(RealInterval_94_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(RealInterval_94_Type);
    static Value_15_Concept = new Value_15_Concept(RealInterval_94_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(RealInterval_94_Type);
    static Value_15_Concept = new Value_15_Concept(RealInterval_94_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(RealInterval_94_Type);
    static Value_15_Concept = new Value_15_Concept(RealInterval_94_Type);
    static Implements = [Interval_25_Concept,Vector_16_Concept,Array_14_Concept,Value_15_Concept,Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept];
}
class Interval2D_95_Type
{
    constructor(A_1590, B_1597)
    {
        // field initialization 
        this.A_1590 = A_1590;
        this.B_1597 = B_1597;
        this.Min_3552 = Interval2D_95_Type.Interval_25_Concept.Min_3552;
        this.Max_3555 = Interval2D_95_Type.Interval_25_Concept.Max_3555;
        this.Count_3203 = Interval2D_95_Type.Vector_16_Concept.Count_3203;
        this.At_3217 = Interval2D_95_Type.Vector_16_Concept.At_3217;
        this.Count_3168 = Interval2D_95_Type.Array_14_Concept.Count_3168;
        this.At_3173 = Interval2D_95_Type.Array_14_Concept.At_3173;
        this.Default_3185 = Interval2D_95_Type.Value_15_Concept.Default_3185;
        this.FieldTypes_3247 = Interval2D_95_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = Interval2D_95_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = Interval2D_95_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = Interval2D_95_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = Interval2D_95_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = Interval2D_95_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Interval2D_95_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Interval2D_95_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Interval2D_95_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Interval2D_95_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Interval2D_95_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Interval2D_95_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Interval2D_95_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Interval2D_95_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Interval2D_95_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Interval2D_95_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Interval2D_95_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Interval2D_95_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Interval2D_95_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Interval2D_95_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Interval2D_95_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Interval2D_95_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Interval2D_95_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Interval2D_95_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Interval2D_95_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Interval2D_95_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Interval2D_95_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Interval2D_95_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Interval2D_95_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Interval2D_95_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Interval2D_95_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Interval2D_95_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static A_1590 = function(self) { return self.A_1590; }
    static B_1597 = function(self) { return self.B_1597; }
    // implemented concepts 
    static Interval_25_Concept = new Interval_25_Concept(Interval2D_95_Type);
    static Vector_16_Concept = new Vector_16_Concept(Interval2D_95_Type);
    static Array_14_Concept = new Array_14_Concept(Interval2D_95_Type);
    static Value_15_Concept = new Value_15_Concept(Interval2D_95_Type);
    static Numerical_18_Concept = new Numerical_18_Concept(Interval2D_95_Type);
    static Value_15_Concept = new Value_15_Concept(Interval2D_95_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Interval2D_95_Type);
    static Value_15_Concept = new Value_15_Concept(Interval2D_95_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Interval2D_95_Type);
    static Value_15_Concept = new Value_15_Concept(Interval2D_95_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Interval2D_95_Type);
    static Value_15_Concept = new Value_15_Concept(Interval2D_95_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Interval2D_95_Type);
    static Value_15_Concept = new Value_15_Concept(Interval2D_95_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Interval2D_95_Type);
    static Value_15_Concept = new Value_15_Concept(Interval2D_95_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Interval2D_95_Type);
    static Value_15_Concept = new Value_15_Concept(Interval2D_95_Type);
    static Implements = [Interval_25_Concept,Vector_16_Concept,Array_14_Concept,Value_15_Concept,Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept];
}
class Interval3D_96_Type
{
    constructor(A_1604, B_1611)
    {
        // field initialization 
        this.A_1604 = A_1604;
        this.B_1611 = B_1611;
        this.Min_3552 = Interval3D_96_Type.Interval_25_Concept.Min_3552;
        this.Max_3555 = Interval3D_96_Type.Interval_25_Concept.Max_3555;
        this.Count_3203 = Interval3D_96_Type.Vector_16_Concept.Count_3203;
        this.At_3217 = Interval3D_96_Type.Vector_16_Concept.At_3217;
        this.Count_3168 = Interval3D_96_Type.Array_14_Concept.Count_3168;
        this.At_3173 = Interval3D_96_Type.Array_14_Concept.At_3173;
        this.Default_3185 = Interval3D_96_Type.Value_15_Concept.Default_3185;
        this.FieldTypes_3247 = Interval3D_96_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = Interval3D_96_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = Interval3D_96_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = Interval3D_96_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = Interval3D_96_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = Interval3D_96_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Interval3D_96_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Interval3D_96_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Interval3D_96_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Interval3D_96_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Interval3D_96_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Interval3D_96_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Interval3D_96_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Interval3D_96_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Interval3D_96_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Interval3D_96_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Interval3D_96_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Interval3D_96_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Interval3D_96_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Interval3D_96_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Interval3D_96_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Interval3D_96_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Interval3D_96_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Interval3D_96_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Interval3D_96_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Interval3D_96_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Interval3D_96_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Interval3D_96_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Interval3D_96_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Interval3D_96_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Interval3D_96_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Interval3D_96_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static A_1604 = function(self) { return self.A_1604; }
    static B_1611 = function(self) { return self.B_1611; }
    // implemented concepts 
    static Interval_25_Concept = new Interval_25_Concept(Interval3D_96_Type);
    static Vector_16_Concept = new Vector_16_Concept(Interval3D_96_Type);
    static Array_14_Concept = new Array_14_Concept(Interval3D_96_Type);
    static Value_15_Concept = new Value_15_Concept(Interval3D_96_Type);
    static Numerical_18_Concept = new Numerical_18_Concept(Interval3D_96_Type);
    static Value_15_Concept = new Value_15_Concept(Interval3D_96_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Interval3D_96_Type);
    static Value_15_Concept = new Value_15_Concept(Interval3D_96_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Interval3D_96_Type);
    static Value_15_Concept = new Value_15_Concept(Interval3D_96_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Interval3D_96_Type);
    static Value_15_Concept = new Value_15_Concept(Interval3D_96_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Interval3D_96_Type);
    static Value_15_Concept = new Value_15_Concept(Interval3D_96_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Interval3D_96_Type);
    static Value_15_Concept = new Value_15_Concept(Interval3D_96_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Interval3D_96_Type);
    static Value_15_Concept = new Value_15_Concept(Interval3D_96_Type);
    static Implements = [Interval_25_Concept,Vector_16_Concept,Array_14_Concept,Value_15_Concept,Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept];
}
class Capsule_97_Type
{
    constructor(Line_1618, Radius_1625)
    {
        // field initialization 
        this.Line_1618 = Line_1618;
        this.Radius_1625 = Radius_1625;
        this.Default_3185 = Capsule_97_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Line_1618 = function(self) { return self.Line_1618; }
    static Radius_1625 = function(self) { return self.Radius_1625; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Capsule_97_Type);
    static Implements = [Value_15_Concept];
}
class Matrix3D_98_Type
{
    constructor(Column1_1632, Column2_1639, Column3_1646, Column4_1653)
    {
        // field initialization 
        this.Column1_1632 = Column1_1632;
        this.Column2_1639 = Column2_1639;
        this.Column3_1646 = Column3_1646;
        this.Column4_1653 = Column4_1653;
        this.Default_3185 = Matrix3D_98_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Column1_1632 = function(self) { return self.Column1_1632; }
    static Column2_1639 = function(self) { return self.Column2_1639; }
    static Column3_1646 = function(self) { return self.Column3_1646; }
    static Column4_1653 = function(self) { return self.Column4_1653; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Matrix3D_98_Type);
    static Implements = [Value_15_Concept];
}
class Cylinder_99_Type
{
    constructor(Line_1660, Radius_1667)
    {
        // field initialization 
        this.Line_1660 = Line_1660;
        this.Radius_1667 = Radius_1667;
        this.Default_3185 = Cylinder_99_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Line_1660 = function(self) { return self.Line_1660; }
    static Radius_1667 = function(self) { return self.Radius_1667; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Cylinder_99_Type);
    static Implements = [Value_15_Concept];
}
class Cone_100_Type
{
    constructor(Line_1674, Radius_1681)
    {
        // field initialization 
        this.Line_1674 = Line_1674;
        this.Radius_1681 = Radius_1681;
        this.Default_3185 = Cone_100_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Line_1674 = function(self) { return self.Line_1674; }
    static Radius_1681 = function(self) { return self.Radius_1681; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Cone_100_Type);
    static Implements = [Value_15_Concept];
}
class Tube_101_Type
{
    constructor(Line_1688, InnerRadius_1695, OuterRadius_1702)
    {
        // field initialization 
        this.Line_1688 = Line_1688;
        this.InnerRadius_1695 = InnerRadius_1695;
        this.OuterRadius_1702 = OuterRadius_1702;
        this.Default_3185 = Tube_101_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Line_1688 = function(self) { return self.Line_1688; }
    static InnerRadius_1695 = function(self) { return self.InnerRadius_1695; }
    static OuterRadius_1702 = function(self) { return self.OuterRadius_1702; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Tube_101_Type);
    static Implements = [Value_15_Concept];
}
class ConeSegment_102_Type
{
    constructor(Line_1709, Radius1_1716, Radius2_1723)
    {
        // field initialization 
        this.Line_1709 = Line_1709;
        this.Radius1_1716 = Radius1_1716;
        this.Radius2_1723 = Radius2_1723;
        this.Default_3185 = ConeSegment_102_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Line_1709 = function(self) { return self.Line_1709; }
    static Radius1_1716 = function(self) { return self.Radius1_1716; }
    static Radius2_1723 = function(self) { return self.Radius2_1723; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(ConeSegment_102_Type);
    static Implements = [Value_15_Concept];
}
class Box2D_103_Type
{
    constructor(Center_1730, Rotation_1737, Extent_1744)
    {
        // field initialization 
        this.Center_1730 = Center_1730;
        this.Rotation_1737 = Rotation_1737;
        this.Extent_1744 = Extent_1744;
        this.Default_3185 = Box2D_103_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Center_1730 = function(self) { return self.Center_1730; }
    static Rotation_1737 = function(self) { return self.Rotation_1737; }
    static Extent_1744 = function(self) { return self.Extent_1744; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Box2D_103_Type);
    static Implements = [Value_15_Concept];
}
class Box3D_104_Type
{
    constructor(Center_1751, Rotation_1758, Extent_1765)
    {
        // field initialization 
        this.Center_1751 = Center_1751;
        this.Rotation_1758 = Rotation_1758;
        this.Extent_1765 = Extent_1765;
        this.Default_3185 = Box3D_104_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Center_1751 = function(self) { return self.Center_1751; }
    static Rotation_1758 = function(self) { return self.Rotation_1758; }
    static Extent_1765 = function(self) { return self.Extent_1765; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(Box3D_104_Type);
    static Implements = [Value_15_Concept];
}
class CubicBezierTriangle3D_105_Type
{
    constructor(A_1772, B_1779, C_1786, A2B_1793, AB2_1800, B2C_1807, BC2_1814, AC2_1821, A2C_1828, ABC_1835)
    {
        // field initialization 
        this.A_1772 = A_1772;
        this.B_1779 = B_1779;
        this.C_1786 = C_1786;
        this.A2B_1793 = A2B_1793;
        this.AB2_1800 = AB2_1800;
        this.B2C_1807 = B2C_1807;
        this.BC2_1814 = BC2_1814;
        this.AC2_1821 = AC2_1821;
        this.A2C_1828 = A2C_1828;
        this.ABC_1835 = ABC_1835;
        this.Default_3185 = CubicBezierTriangle3D_105_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static A_1772 = function(self) { return self.A_1772; }
    static B_1779 = function(self) { return self.B_1779; }
    static C_1786 = function(self) { return self.C_1786; }
    static A2B_1793 = function(self) { return self.A2B_1793; }
    static AB2_1800 = function(self) { return self.AB2_1800; }
    static B2C_1807 = function(self) { return self.B2C_1807; }
    static BC2_1814 = function(self) { return self.BC2_1814; }
    static AC2_1821 = function(self) { return self.AC2_1821; }
    static A2C_1828 = function(self) { return self.A2C_1828; }
    static ABC_1835 = function(self) { return self.ABC_1835; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(CubicBezierTriangle3D_105_Type);
    static Implements = [Value_15_Concept];
}
class CubicBezier2D_106_Type
{
    constructor(A_1842, B_1849, C_1856, D_1863)
    {
        // field initialization 
        this.A_1842 = A_1842;
        this.B_1849 = B_1849;
        this.C_1856 = C_1856;
        this.D_1863 = D_1863;
        this.Default_3185 = CubicBezier2D_106_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static A_1842 = function(self) { return self.A_1842; }
    static B_1849 = function(self) { return self.B_1849; }
    static C_1856 = function(self) { return self.C_1856; }
    static D_1863 = function(self) { return self.D_1863; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(CubicBezier2D_106_Type);
    static Implements = [Value_15_Concept];
}
class UV_107_Type
{
    constructor(U_1870, V_1877)
    {
        // field initialization 
        this.U_1870 = U_1870;
        this.V_1877 = V_1877;
        this.Count_3203 = UV_107_Type.Vector_16_Concept.Count_3203;
        this.At_3217 = UV_107_Type.Vector_16_Concept.At_3217;
        this.Count_3168 = UV_107_Type.Array_14_Concept.Count_3168;
        this.At_3173 = UV_107_Type.Array_14_Concept.At_3173;
        this.Default_3185 = UV_107_Type.Value_15_Concept.Default_3185;
        this.FieldTypes_3247 = UV_107_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = UV_107_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = UV_107_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = UV_107_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = UV_107_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = UV_107_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = UV_107_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = UV_107_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = UV_107_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = UV_107_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = UV_107_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = UV_107_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = UV_107_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = UV_107_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = UV_107_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = UV_107_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = UV_107_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = UV_107_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = UV_107_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = UV_107_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = UV_107_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = UV_107_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = UV_107_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = UV_107_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = UV_107_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = UV_107_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = UV_107_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = UV_107_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = UV_107_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = UV_107_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = UV_107_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = UV_107_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static U_1870 = function(self) { return self.U_1870; }
    static V_1877 = function(self) { return self.V_1877; }
    // implemented concepts 
    static Vector_16_Concept = new Vector_16_Concept(UV_107_Type);
    static Array_14_Concept = new Array_14_Concept(UV_107_Type);
    static Value_15_Concept = new Value_15_Concept(UV_107_Type);
    static Numerical_18_Concept = new Numerical_18_Concept(UV_107_Type);
    static Value_15_Concept = new Value_15_Concept(UV_107_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(UV_107_Type);
    static Value_15_Concept = new Value_15_Concept(UV_107_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(UV_107_Type);
    static Value_15_Concept = new Value_15_Concept(UV_107_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(UV_107_Type);
    static Value_15_Concept = new Value_15_Concept(UV_107_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(UV_107_Type);
    static Value_15_Concept = new Value_15_Concept(UV_107_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(UV_107_Type);
    static Value_15_Concept = new Value_15_Concept(UV_107_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(UV_107_Type);
    static Value_15_Concept = new Value_15_Concept(UV_107_Type);
    static Implements = [Vector_16_Concept,Array_14_Concept,Value_15_Concept,Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept];
}
class UVW_108_Type
{
    constructor(U_1884, V_1891, W_1898)
    {
        // field initialization 
        this.U_1884 = U_1884;
        this.V_1891 = V_1891;
        this.W_1898 = W_1898;
        this.Count_3203 = UVW_108_Type.Vector_16_Concept.Count_3203;
        this.At_3217 = UVW_108_Type.Vector_16_Concept.At_3217;
        this.Count_3168 = UVW_108_Type.Array_14_Concept.Count_3168;
        this.At_3173 = UVW_108_Type.Array_14_Concept.At_3173;
        this.Default_3185 = UVW_108_Type.Value_15_Concept.Default_3185;
        this.FieldTypes_3247 = UVW_108_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = UVW_108_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = UVW_108_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = UVW_108_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = UVW_108_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = UVW_108_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = UVW_108_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = UVW_108_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = UVW_108_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = UVW_108_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = UVW_108_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = UVW_108_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = UVW_108_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = UVW_108_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = UVW_108_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = UVW_108_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = UVW_108_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = UVW_108_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = UVW_108_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = UVW_108_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = UVW_108_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = UVW_108_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = UVW_108_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = UVW_108_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = UVW_108_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = UVW_108_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = UVW_108_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = UVW_108_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = UVW_108_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = UVW_108_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = UVW_108_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = UVW_108_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static U_1884 = function(self) { return self.U_1884; }
    static V_1891 = function(self) { return self.V_1891; }
    static W_1898 = function(self) { return self.W_1898; }
    // implemented concepts 
    static Vector_16_Concept = new Vector_16_Concept(UVW_108_Type);
    static Array_14_Concept = new Array_14_Concept(UVW_108_Type);
    static Value_15_Concept = new Value_15_Concept(UVW_108_Type);
    static Numerical_18_Concept = new Numerical_18_Concept(UVW_108_Type);
    static Value_15_Concept = new Value_15_Concept(UVW_108_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(UVW_108_Type);
    static Value_15_Concept = new Value_15_Concept(UVW_108_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(UVW_108_Type);
    static Value_15_Concept = new Value_15_Concept(UVW_108_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(UVW_108_Type);
    static Value_15_Concept = new Value_15_Concept(UVW_108_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(UVW_108_Type);
    static Value_15_Concept = new Value_15_Concept(UVW_108_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(UVW_108_Type);
    static Value_15_Concept = new Value_15_Concept(UVW_108_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(UVW_108_Type);
    static Value_15_Concept = new Value_15_Concept(UVW_108_Type);
    static Implements = [Vector_16_Concept,Array_14_Concept,Value_15_Concept,Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept];
}
class CubicBezier3D_109_Type
{
    constructor(A_1905, B_1912, C_1919, D_1926)
    {
        // field initialization 
        this.A_1905 = A_1905;
        this.B_1912 = B_1912;
        this.C_1919 = C_1919;
        this.D_1926 = D_1926;
        this.Default_3185 = CubicBezier3D_109_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static A_1905 = function(self) { return self.A_1905; }
    static B_1912 = function(self) { return self.B_1912; }
    static C_1919 = function(self) { return self.C_1919; }
    static D_1926 = function(self) { return self.D_1926; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(CubicBezier3D_109_Type);
    static Implements = [Value_15_Concept];
}
class QuadraticBezier2D_110_Type
{
    constructor(A_1933, B_1940, C_1947)
    {
        // field initialization 
        this.A_1933 = A_1933;
        this.B_1940 = B_1940;
        this.C_1947 = C_1947;
        this.Default_3185 = QuadraticBezier2D_110_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static A_1933 = function(self) { return self.A_1933; }
    static B_1940 = function(self) { return self.B_1940; }
    static C_1947 = function(self) { return self.C_1947; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(QuadraticBezier2D_110_Type);
    static Implements = [Value_15_Concept];
}
class QuadraticBezier3D_111_Type
{
    constructor(A_1954, B_1961, C_1968)
    {
        // field initialization 
        this.A_1954 = A_1954;
        this.B_1961 = B_1961;
        this.C_1968 = C_1968;
        this.Default_3185 = QuadraticBezier3D_111_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static A_1954 = function(self) { return self.A_1954; }
    static B_1961 = function(self) { return self.B_1961; }
    static C_1968 = function(self) { return self.C_1968; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(QuadraticBezier3D_111_Type);
    static Implements = [Value_15_Concept];
}
class Area_112_Type
{
    constructor(MetersSquared_1975)
    {
        // field initialization 
        this.MetersSquared_1975 = MetersSquared_1975;
        this.Value_3236 = Area_112_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = Area_112_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Area_112_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Area_112_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Area_112_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Area_112_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Area_112_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Area_112_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Area_112_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Area_112_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Area_112_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Area_112_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Area_112_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Area_112_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static MetersSquared_1975 = function(self) { return self.MetersSquared_1975; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(Area_112_Type);
    static Value_15_Concept = new Value_15_Concept(Area_112_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Area_112_Type);
    static Value_15_Concept = new Value_15_Concept(Area_112_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Area_112_Type);
    static Value_15_Concept = new Value_15_Concept(Area_112_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Area_112_Type);
    static Value_15_Concept = new Value_15_Concept(Area_112_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Area_112_Type);
    static Value_15_Concept = new Value_15_Concept(Area_112_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class Volume_113_Type
{
    constructor(MetersCubed_1982)
    {
        // field initialization 
        this.MetersCubed_1982 = MetersCubed_1982;
        this.Value_3236 = Volume_113_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = Volume_113_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Volume_113_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Volume_113_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Volume_113_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Volume_113_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Volume_113_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Volume_113_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Volume_113_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Volume_113_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Volume_113_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Volume_113_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Volume_113_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Volume_113_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static MetersCubed_1982 = function(self) { return self.MetersCubed_1982; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(Volume_113_Type);
    static Value_15_Concept = new Value_15_Concept(Volume_113_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Volume_113_Type);
    static Value_15_Concept = new Value_15_Concept(Volume_113_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Volume_113_Type);
    static Value_15_Concept = new Value_15_Concept(Volume_113_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Volume_113_Type);
    static Value_15_Concept = new Value_15_Concept(Volume_113_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Volume_113_Type);
    static Value_15_Concept = new Value_15_Concept(Volume_113_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class Velocity_114_Type
{
    constructor(MetersPerSecond_1989)
    {
        // field initialization 
        this.MetersPerSecond_1989 = MetersPerSecond_1989;
        this.Value_3236 = Velocity_114_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = Velocity_114_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Velocity_114_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Velocity_114_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Velocity_114_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Velocity_114_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Velocity_114_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Velocity_114_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Velocity_114_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Velocity_114_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Velocity_114_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Velocity_114_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Velocity_114_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Velocity_114_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static MetersPerSecond_1989 = function(self) { return self.MetersPerSecond_1989; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(Velocity_114_Type);
    static Value_15_Concept = new Value_15_Concept(Velocity_114_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Velocity_114_Type);
    static Value_15_Concept = new Value_15_Concept(Velocity_114_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Velocity_114_Type);
    static Value_15_Concept = new Value_15_Concept(Velocity_114_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Velocity_114_Type);
    static Value_15_Concept = new Value_15_Concept(Velocity_114_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Velocity_114_Type);
    static Value_15_Concept = new Value_15_Concept(Velocity_114_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class Acceleration_115_Type
{
    constructor(MetersPerSecondSquared_1996)
    {
        // field initialization 
        this.MetersPerSecondSquared_1996 = MetersPerSecondSquared_1996;
        this.Value_3236 = Acceleration_115_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = Acceleration_115_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Acceleration_115_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Acceleration_115_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Acceleration_115_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Acceleration_115_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Acceleration_115_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Acceleration_115_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Acceleration_115_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Acceleration_115_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Acceleration_115_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Acceleration_115_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Acceleration_115_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Acceleration_115_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static MetersPerSecondSquared_1996 = function(self) { return self.MetersPerSecondSquared_1996; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(Acceleration_115_Type);
    static Value_15_Concept = new Value_15_Concept(Acceleration_115_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Acceleration_115_Type);
    static Value_15_Concept = new Value_15_Concept(Acceleration_115_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Acceleration_115_Type);
    static Value_15_Concept = new Value_15_Concept(Acceleration_115_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Acceleration_115_Type);
    static Value_15_Concept = new Value_15_Concept(Acceleration_115_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Acceleration_115_Type);
    static Value_15_Concept = new Value_15_Concept(Acceleration_115_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class Force_116_Type
{
    constructor(Newtons_2003)
    {
        // field initialization 
        this.Newtons_2003 = Newtons_2003;
        this.Value_3236 = Force_116_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = Force_116_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Force_116_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Force_116_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Force_116_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Force_116_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Force_116_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Force_116_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Force_116_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Force_116_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Force_116_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Force_116_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Force_116_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Force_116_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Newtons_2003 = function(self) { return self.Newtons_2003; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(Force_116_Type);
    static Value_15_Concept = new Value_15_Concept(Force_116_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Force_116_Type);
    static Value_15_Concept = new Value_15_Concept(Force_116_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Force_116_Type);
    static Value_15_Concept = new Value_15_Concept(Force_116_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Force_116_Type);
    static Value_15_Concept = new Value_15_Concept(Force_116_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Force_116_Type);
    static Value_15_Concept = new Value_15_Concept(Force_116_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class Pressure_117_Type
{
    constructor(Pascals_2010)
    {
        // field initialization 
        this.Pascals_2010 = Pascals_2010;
        this.Value_3236 = Pressure_117_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = Pressure_117_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Pressure_117_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Pressure_117_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Pressure_117_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Pressure_117_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Pressure_117_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Pressure_117_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Pressure_117_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Pressure_117_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Pressure_117_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Pressure_117_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Pressure_117_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Pressure_117_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Pascals_2010 = function(self) { return self.Pascals_2010; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(Pressure_117_Type);
    static Value_15_Concept = new Value_15_Concept(Pressure_117_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Pressure_117_Type);
    static Value_15_Concept = new Value_15_Concept(Pressure_117_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Pressure_117_Type);
    static Value_15_Concept = new Value_15_Concept(Pressure_117_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Pressure_117_Type);
    static Value_15_Concept = new Value_15_Concept(Pressure_117_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Pressure_117_Type);
    static Value_15_Concept = new Value_15_Concept(Pressure_117_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class Energy_118_Type
{
    constructor(Joules_2017)
    {
        // field initialization 
        this.Joules_2017 = Joules_2017;
        this.Value_3236 = Energy_118_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = Energy_118_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Energy_118_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Energy_118_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Energy_118_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Energy_118_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Energy_118_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Energy_118_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Energy_118_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Energy_118_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Energy_118_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Energy_118_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Energy_118_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Energy_118_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Joules_2017 = function(self) { return self.Joules_2017; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(Energy_118_Type);
    static Value_15_Concept = new Value_15_Concept(Energy_118_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Energy_118_Type);
    static Value_15_Concept = new Value_15_Concept(Energy_118_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Energy_118_Type);
    static Value_15_Concept = new Value_15_Concept(Energy_118_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Energy_118_Type);
    static Value_15_Concept = new Value_15_Concept(Energy_118_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Energy_118_Type);
    static Value_15_Concept = new Value_15_Concept(Energy_118_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class Memory_119_Type
{
    constructor(Bytes_2024)
    {
        // field initialization 
        this.Bytes_2024 = Bytes_2024;
        this.Value_3236 = Memory_119_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = Memory_119_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Memory_119_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Memory_119_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Memory_119_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Memory_119_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Memory_119_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Memory_119_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Memory_119_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Memory_119_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Memory_119_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Memory_119_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Memory_119_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Memory_119_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Bytes_2024 = function(self) { return self.Bytes_2024; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(Memory_119_Type);
    static Value_15_Concept = new Value_15_Concept(Memory_119_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Memory_119_Type);
    static Value_15_Concept = new Value_15_Concept(Memory_119_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Memory_119_Type);
    static Value_15_Concept = new Value_15_Concept(Memory_119_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Memory_119_Type);
    static Value_15_Concept = new Value_15_Concept(Memory_119_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Memory_119_Type);
    static Value_15_Concept = new Value_15_Concept(Memory_119_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class Frequency_120_Type
{
    constructor(Hertz_2031)
    {
        // field initialization 
        this.Hertz_2031 = Hertz_2031;
        this.Value_3236 = Frequency_120_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = Frequency_120_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Frequency_120_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Frequency_120_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Frequency_120_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Frequency_120_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Frequency_120_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Frequency_120_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Frequency_120_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Frequency_120_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Frequency_120_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Frequency_120_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Frequency_120_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Frequency_120_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Hertz_2031 = function(self) { return self.Hertz_2031; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(Frequency_120_Type);
    static Value_15_Concept = new Value_15_Concept(Frequency_120_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Frequency_120_Type);
    static Value_15_Concept = new Value_15_Concept(Frequency_120_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Frequency_120_Type);
    static Value_15_Concept = new Value_15_Concept(Frequency_120_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Frequency_120_Type);
    static Value_15_Concept = new Value_15_Concept(Frequency_120_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Frequency_120_Type);
    static Value_15_Concept = new Value_15_Concept(Frequency_120_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class Loudness_121_Type
{
    constructor(Decibels_2038)
    {
        // field initialization 
        this.Decibels_2038 = Decibels_2038;
        this.Value_3236 = Loudness_121_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = Loudness_121_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Loudness_121_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Loudness_121_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Loudness_121_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Loudness_121_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Loudness_121_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Loudness_121_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Loudness_121_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Loudness_121_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Loudness_121_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Loudness_121_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Loudness_121_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Loudness_121_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Decibels_2038 = function(self) { return self.Decibels_2038; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(Loudness_121_Type);
    static Value_15_Concept = new Value_15_Concept(Loudness_121_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Loudness_121_Type);
    static Value_15_Concept = new Value_15_Concept(Loudness_121_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Loudness_121_Type);
    static Value_15_Concept = new Value_15_Concept(Loudness_121_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Loudness_121_Type);
    static Value_15_Concept = new Value_15_Concept(Loudness_121_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Loudness_121_Type);
    static Value_15_Concept = new Value_15_Concept(Loudness_121_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class LuminousIntensity_122_Type
{
    constructor(Candelas_2045)
    {
        // field initialization 
        this.Candelas_2045 = Candelas_2045;
        this.Value_3236 = LuminousIntensity_122_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = LuminousIntensity_122_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = LuminousIntensity_122_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = LuminousIntensity_122_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = LuminousIntensity_122_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = LuminousIntensity_122_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = LuminousIntensity_122_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = LuminousIntensity_122_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = LuminousIntensity_122_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = LuminousIntensity_122_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = LuminousIntensity_122_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = LuminousIntensity_122_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = LuminousIntensity_122_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = LuminousIntensity_122_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Candelas_2045 = function(self) { return self.Candelas_2045; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(LuminousIntensity_122_Type);
    static Value_15_Concept = new Value_15_Concept(LuminousIntensity_122_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(LuminousIntensity_122_Type);
    static Value_15_Concept = new Value_15_Concept(LuminousIntensity_122_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(LuminousIntensity_122_Type);
    static Value_15_Concept = new Value_15_Concept(LuminousIntensity_122_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(LuminousIntensity_122_Type);
    static Value_15_Concept = new Value_15_Concept(LuminousIntensity_122_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(LuminousIntensity_122_Type);
    static Value_15_Concept = new Value_15_Concept(LuminousIntensity_122_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class ElectricPotential_123_Type
{
    constructor(Volts_2052)
    {
        // field initialization 
        this.Volts_2052 = Volts_2052;
        this.Value_3236 = ElectricPotential_123_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = ElectricPotential_123_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = ElectricPotential_123_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = ElectricPotential_123_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = ElectricPotential_123_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = ElectricPotential_123_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = ElectricPotential_123_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = ElectricPotential_123_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = ElectricPotential_123_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = ElectricPotential_123_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = ElectricPotential_123_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = ElectricPotential_123_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = ElectricPotential_123_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = ElectricPotential_123_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Volts_2052 = function(self) { return self.Volts_2052; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(ElectricPotential_123_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricPotential_123_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(ElectricPotential_123_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricPotential_123_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(ElectricPotential_123_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricPotential_123_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(ElectricPotential_123_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricPotential_123_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(ElectricPotential_123_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricPotential_123_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class ElectricCharge_124_Type
{
    constructor(Columbs_2059)
    {
        // field initialization 
        this.Columbs_2059 = Columbs_2059;
        this.Value_3236 = ElectricCharge_124_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = ElectricCharge_124_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = ElectricCharge_124_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = ElectricCharge_124_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = ElectricCharge_124_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = ElectricCharge_124_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = ElectricCharge_124_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = ElectricCharge_124_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = ElectricCharge_124_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = ElectricCharge_124_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = ElectricCharge_124_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = ElectricCharge_124_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = ElectricCharge_124_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = ElectricCharge_124_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Columbs_2059 = function(self) { return self.Columbs_2059; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(ElectricCharge_124_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricCharge_124_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(ElectricCharge_124_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricCharge_124_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(ElectricCharge_124_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricCharge_124_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(ElectricCharge_124_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricCharge_124_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(ElectricCharge_124_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricCharge_124_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class ElectricCurrent_125_Type
{
    constructor(Amperes_2066)
    {
        // field initialization 
        this.Amperes_2066 = Amperes_2066;
        this.Value_3236 = ElectricCurrent_125_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = ElectricCurrent_125_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = ElectricCurrent_125_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = ElectricCurrent_125_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = ElectricCurrent_125_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = ElectricCurrent_125_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = ElectricCurrent_125_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = ElectricCurrent_125_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = ElectricCurrent_125_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = ElectricCurrent_125_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = ElectricCurrent_125_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = ElectricCurrent_125_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = ElectricCurrent_125_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = ElectricCurrent_125_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Amperes_2066 = function(self) { return self.Amperes_2066; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(ElectricCurrent_125_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricCurrent_125_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(ElectricCurrent_125_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricCurrent_125_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(ElectricCurrent_125_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricCurrent_125_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(ElectricCurrent_125_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricCurrent_125_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(ElectricCurrent_125_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricCurrent_125_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class ElectricResistance_126_Type
{
    constructor(Ohms_2073)
    {
        // field initialization 
        this.Ohms_2073 = Ohms_2073;
        this.Value_3236 = ElectricResistance_126_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = ElectricResistance_126_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = ElectricResistance_126_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = ElectricResistance_126_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = ElectricResistance_126_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = ElectricResistance_126_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = ElectricResistance_126_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = ElectricResistance_126_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = ElectricResistance_126_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = ElectricResistance_126_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = ElectricResistance_126_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = ElectricResistance_126_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = ElectricResistance_126_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = ElectricResistance_126_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Ohms_2073 = function(self) { return self.Ohms_2073; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(ElectricResistance_126_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricResistance_126_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(ElectricResistance_126_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricResistance_126_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(ElectricResistance_126_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricResistance_126_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(ElectricResistance_126_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricResistance_126_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(ElectricResistance_126_Type);
    static Value_15_Concept = new Value_15_Concept(ElectricResistance_126_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class Power_127_Type
{
    constructor(Watts_2080)
    {
        // field initialization 
        this.Watts_2080 = Watts_2080;
        this.Value_3236 = Power_127_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = Power_127_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Power_127_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Power_127_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Power_127_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Power_127_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Power_127_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Power_127_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Power_127_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Power_127_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Power_127_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Power_127_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Power_127_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Power_127_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Watts_2080 = function(self) { return self.Watts_2080; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(Power_127_Type);
    static Value_15_Concept = new Value_15_Concept(Power_127_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Power_127_Type);
    static Value_15_Concept = new Value_15_Concept(Power_127_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Power_127_Type);
    static Value_15_Concept = new Value_15_Concept(Power_127_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Power_127_Type);
    static Value_15_Concept = new Value_15_Concept(Power_127_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Power_127_Type);
    static Value_15_Concept = new Value_15_Concept(Power_127_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class Density_128_Type
{
    constructor(KilogramsPerMeterCubed_2087)
    {
        // field initialization 
        this.KilogramsPerMeterCubed_2087 = KilogramsPerMeterCubed_2087;
        this.Value_3236 = Density_128_Type.Measure_17_Concept.Value_3236;
        this.Default_3185 = Density_128_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Density_128_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Density_128_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Density_128_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Density_128_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Density_128_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Density_128_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Density_128_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Density_128_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Density_128_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Density_128_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Density_128_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Density_128_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static KilogramsPerMeterCubed_2087 = function(self) { return self.KilogramsPerMeterCubed_2087; }
    // implemented concepts 
    static Measure_17_Concept = new Measure_17_Concept(Density_128_Type);
    static Value_15_Concept = new Value_15_Concept(Density_128_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Density_128_Type);
    static Value_15_Concept = new Value_15_Concept(Density_128_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Density_128_Type);
    static Value_15_Concept = new Value_15_Concept(Density_128_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Density_128_Type);
    static Value_15_Concept = new Value_15_Concept(Density_128_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Density_128_Type);
    static Value_15_Concept = new Value_15_Concept(Density_128_Type);
    static Implements = [Measure_17_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept];
}
class NormalDistribution_129_Type
{
    constructor(Mean_2094, StandardDeviation_2101)
    {
        // field initialization 
        this.Mean_2094 = Mean_2094;
        this.StandardDeviation_2101 = StandardDeviation_2101;
        this.Default_3185 = NormalDistribution_129_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Mean_2094 = function(self) { return self.Mean_2094; }
    static StandardDeviation_2101 = function(self) { return self.StandardDeviation_2101; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(NormalDistribution_129_Type);
    static Implements = [Value_15_Concept];
}
class PoissonDistribution_130_Type
{
    constructor(Expected_2108, Occurrences_2115)
    {
        // field initialization 
        this.Expected_2108 = Expected_2108;
        this.Occurrences_2115 = Occurrences_2115;
        this.Default_3185 = PoissonDistribution_130_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Expected_2108 = function(self) { return self.Expected_2108; }
    static Occurrences_2115 = function(self) { return self.Occurrences_2115; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(PoissonDistribution_130_Type);
    static Implements = [Value_15_Concept];
}
class BernoulliDistribution_131_Type
{
    constructor(P_2122)
    {
        // field initialization 
        this.P_2122 = P_2122;
        this.Default_3185 = BernoulliDistribution_131_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static P_2122 = function(self) { return self.P_2122; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(BernoulliDistribution_131_Type);
    static Implements = [Value_15_Concept];
}
class Probability_132_Type
{
    constructor(Value_2129)
    {
        // field initialization 
        this.Value_2129 = Value_2129;
        this.FieldTypes_3247 = Probability_132_Type.Numerical_18_Concept.FieldTypes_3247;
        this.Zero_3257 = Probability_132_Type.Numerical_18_Concept.Zero_3257;
        this.One_3267 = Probability_132_Type.Numerical_18_Concept.One_3267;
        this.MinValue_3277 = Probability_132_Type.Numerical_18_Concept.MinValue_3277;
        this.MaxValue_3287 = Probability_132_Type.Numerical_18_Concept.MaxValue_3287;
        this.Default_3185 = Probability_132_Type.Value_15_Concept.Default_3185;
        this.Add_3355 = Probability_132_Type.Arithmetic_22_Concept.Add_3355;
        this.Negative_3365 = Probability_132_Type.Arithmetic_22_Concept.Negative_3365;
        this.Reciprocal_3375 = Probability_132_Type.Arithmetic_22_Concept.Reciprocal_3375;
        this.Multiply_3392 = Probability_132_Type.Arithmetic_22_Concept.Multiply_3392;
        this.Divide_3409 = Probability_132_Type.Arithmetic_22_Concept.Divide_3409;
        this.Modulo_3426 = Probability_132_Type.Arithmetic_22_Concept.Modulo_3426;
        this.Default_3185 = Probability_132_Type.Value_15_Concept.Default_3185;
        this.Equals_3335 = Probability_132_Type.Equatable_21_Concept.Equals_3335;
        this.Default_3185 = Probability_132_Type.Value_15_Concept.Default_3185;
        this.Compare_3312 = Probability_132_Type.Comparable_20_Concept.Compare_3312;
        this.Default_3185 = Probability_132_Type.Value_15_Concept.Default_3185;
        this.Magnitude_3306 = Probability_132_Type.Magnitudinal_19_Concept.Magnitude_3306;
        this.Default_3185 = Probability_132_Type.Value_15_Concept.Default_3185;
        this.Add_3443 = Probability_132_Type.ScalarArithmetic_23_Concept.Add_3443;
        this.Subtract_3457 = Probability_132_Type.ScalarArithmetic_23_Concept.Subtract_3457;
        this.Multiply_3471 = Probability_132_Type.ScalarArithmetic_23_Concept.Multiply_3471;
        this.Divide_3485 = Probability_132_Type.ScalarArithmetic_23_Concept.Divide_3485;
        this.Modulo_3499 = Probability_132_Type.ScalarArithmetic_23_Concept.Modulo_3499;
        this.Default_3185 = Probability_132_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Value_2129 = function(self) { return self.Value_2129; }
    // implemented concepts 
    static Numerical_18_Concept = new Numerical_18_Concept(Probability_132_Type);
    static Value_15_Concept = new Value_15_Concept(Probability_132_Type);
    static Arithmetic_22_Concept = new Arithmetic_22_Concept(Probability_132_Type);
    static Value_15_Concept = new Value_15_Concept(Probability_132_Type);
    static Equatable_21_Concept = new Equatable_21_Concept(Probability_132_Type);
    static Value_15_Concept = new Value_15_Concept(Probability_132_Type);
    static Comparable_20_Concept = new Comparable_20_Concept(Probability_132_Type);
    static Value_15_Concept = new Value_15_Concept(Probability_132_Type);
    static Magnitudinal_19_Concept = new Magnitudinal_19_Concept(Probability_132_Type);
    static Value_15_Concept = new Value_15_Concept(Probability_132_Type);
    static ScalarArithmetic_23_Concept = new ScalarArithmetic_23_Concept(Probability_132_Type);
    static Value_15_Concept = new Value_15_Concept(Probability_132_Type);
    static Implements = [Numerical_18_Concept,Value_15_Concept,Arithmetic_22_Concept,Value_15_Concept,Equatable_21_Concept,Value_15_Concept,Comparable_20_Concept,Value_15_Concept,Magnitudinal_19_Concept,Value_15_Concept,ScalarArithmetic_23_Concept,Value_15_Concept];
}
class BinomialDistribution_133_Type
{
    constructor(Trials_2136, P_2143)
    {
        // field initialization 
        this.Trials_2136 = Trials_2136;
        this.P_2143 = P_2143;
        this.Default_3185 = BinomialDistribution_133_Type.Value_15_Concept.Default_3185;
    }
    // field accessors
    static Trials_2136 = function(self) { return self.Trials_2136; }
    static P_2143 = function(self) { return self.P_2143; }
    // implemented concepts 
    static Value_15_Concept = new Value_15_Concept(BinomialDistribution_133_Type);
    static Implements = [Value_15_Concept];
}

// This is appended to every JavaScript program generated from Plato