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




class Intrinsics_7_Library
{
    static Cos_3041 = function (x_3040/* : Angle_82 */) /* : Number_21 */{ return null; };
    static Sin_3044 = function (x_3043/* : Angle_82 */) /* : Number_21 */{ return null; };
    static Tan_3047 = function (x_3046/* : Angle_82 */) /* : Number_21 */{ return null; };
    static Acos_3050 = function (x_3049/* : Number_21 */) /* : Angle_82 */{ return null; };
    static Asin_3053 = function (x_3052/* : Number_21 */) /* : Angle_82 */{ return null; };
    static Atan_3056 = function (x_3055/* : Number_21 */) /* : Angle_82 */{ return null; };
    static Pow_3061 = function (x_3058/* : Number_21 */, y_3060/* : Number_21 */) /* : Number_21 */{ return null; };
    static Log_3066 = function (x_3063/* : Number_21 */, y_3065/* : Number_21 */) /* : Number_21 */{ return null; };
    static NaturalLog_3069 = function (x_3068/* : Number_21 */) /* : Number_21 */{ return null; };
    static NaturalPower_3072 = function (x_3071/* : Number_21 */) /* : Number_21 */{ return null; };
    static Interpolate_3075 = function (xs_3074/* : Array_10 */) /* : String_23 */{ return null; };
    static Throw_3078 = function (x_3077/* : Any_8 */) /* : Any_8 */{ return null; };
    static TypeOf_3081 = function (x_3080/* : Any_8 */) /* : Type_25 */{ return null; };
    static Add_3086 = function (x_3083/* : Number_21 */, y_3085/* : Number_21 */) /* : Number_21 */{ return null; };
    static Subtract_3091 = function (x_3088/* : Number_21 */, y_3090/* : Number_21 */) /* : Number_21 */{ return null; };
    static Divide_3096 = function (x_3093/* : Number_21 */, y_3095/* : Number_21 */) /* : Number_21 */{ return null; };
    static Multiply_3101 = function (x_3098/* : Number_21 */, y_3100/* : Number_21 */) /* : Number_21 */{ return null; };
    static Modulo_3106 = function (x_3103/* : Number_21 */, y_3105/* : Number_21 */) /* : Number_21 */{ return null; };
    static Negative_3109 = function (x_3108/* : Number_21 */) /* : Number_21 */{ return null; };
    static Add_3114 = function (x_3111/* : Integer_22 */, y_3113/* : Integer_22 */) /* : Integer_22 */{ return null; };
    static Subtract_3119 = function (x_3116/* : Integer_22 */, y_3118/* : Integer_22 */) /* : Integer_22 */{ return null; };
    static Divide_3124 = function (x_3121/* : Integer_22 */, y_3123/* : Integer_22 */) /* : Integer_22 */{ return null; };
    static Multiply_3129 = function (x_3126/* : Integer_22 */, y_3128/* : Integer_22 */) /* : Integer_22 */{ return null; };
    static Modulo_3134 = function (x_3131/* : Integer_22 */, y_3133/* : Integer_22 */) /* : Integer_22 */{ return null; };
    static Negative_3137 = function (x_3136/* : Integer_22 */) /* : Integer_22 */{ return null; };
    static And_3142 = function (x_3139/* : Boolean_24 */, y_3141/* : Boolean_24 */) /* : Boolean_24 */{ return null; };
    static Or_3147 = function (x_3144/* : Boolean_24 */, y_3146/* : Boolean_24 */) /* : Boolean_24 */{ return null; };
    static Not_3150 = function (x_3149/* : Boolean_24 */) /* : Boolean_24 */{ return null; };
}
class Array_133_Library
{
    static Map_3882 = function (xs_3858/* : Array_10 */, f_3860/* : Function_3 */) /* : Array_10 */{ return Tuple_1/* : Error_6 */(Count_374/* : UnknownType */(xs_3858/* : UnknownType */)/* : UnknownType */, function (i_3867/* : UnknownType */) /* : Lambda_2 */{ return f_3860/* : UnknownType */(At_380/* : UnknownType */(xs_3858/* : UnknownType */, i_3867/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Error_6 */; };
    static Reverse_3919 = function (xs_3884/* : Array_10 */) /* : Array_10 */{ return Tuple_1/* : Error_6 */(Count_374/* : UnknownType */(xs_3884/* : UnknownType */)/* : UnknownType */, function (i_3891/* : UnknownType */) /* : Lambda_2 */{ return f_3860/* : UnknownType */(At_380/* : UnknownType */(xs_3884/* : UnknownType */, Subtract_233/* : UnknownType */(Count_374/* : UnknownType */(xs_3884/* : UnknownType */)/* : UnknownType */, Subtract_233/* : UnknownType */(1/* : UnknownType */, i_3891/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Error_6 */; };
    static Zip_3952 = function (xs_3921/* : Array_10 */, ys_3923/* : Array_10 */, f_3925/* : Function_3 */) /* : Array_10 */{ return Tuple_1/* : Error_6 */(Count_374/* : UnknownType */(xs_3921/* : UnknownType */)/* : UnknownType */, function (i_3932/* : UnknownType */) /* : Lambda_2 */{ return f_3925/* : UnknownType */(At_380/* : UnknownType */(i_3932/* : UnknownType */)/* : UnknownType */, At_380/* : UnknownType */(ys_3923/* : UnknownType */, i_3932/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Error_6 */; };
    static Zip_3994 = function (xs_3954/* : Array_10 */, ys_3956/* : Array_10 */, zs_3958/* : Array_10 */, f_3960/* : Function_3 */) /* : Array_10 */{ return Tuple_1/* : Error_6 */(Count_374/* : UnknownType */(xs_3954/* : UnknownType */)/* : UnknownType */, function (i_3967/* : UnknownType */) /* : Lambda_2 */{ return f_3960/* : UnknownType */(At_380/* : UnknownType */(i_3967/* : UnknownType */)/* : UnknownType */, At_380/* : UnknownType */(ys_3956/* : UnknownType */, i_3967/* : UnknownType */)/* : UnknownType */, At_380/* : UnknownType */(zs_3958/* : UnknownType */, i_3967/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Error_6 */; };
    static Skip_4022 = function (xs_3996/* : Array_10 */, n_3998/* : Count_27 */) /* : Array_10 */{ return Tuple_1/* : Error_6 */(Subtract_233/* : UnknownType */(Count_374/* : UnknownType */, n_3998/* : UnknownType */)/* : UnknownType */, function (i_4007/* : UnknownType */) /* : Lambda_2 */{ return At_380/* : UnknownType */(Subtract_233/* : UnknownType */(i_4007/* : UnknownType */, n_3998/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Error_6 */; };
    static Take_4040 = function (xs_4024/* : Array_10 */, n_4026/* : Count_27 */) /* : Array_10 */{ return Tuple_1/* : Error_6 */(n_4026/* : UnknownType */, function (i_4030/* : UnknownType */) /* : Lambda_2 */{ return At_380/* : UnknownType */(i_4030/* : UnknownType */)/* : UnknownType */; })/* : Error_6 */; };
    static Aggregate_4065 = function (xs_4042/* : Array_10 */, init_4044/* : Any_8 */, f_4046/* : Function_3 */) /* : Any_8 */{ return IsEmpty_2210/* : UnknownType */(xs_4042/* : UnknownType */)/* : UnknownType */
        ? init_4044/* : Any_8 */
        : f_4046/* : Function_3 */(init_4044/* : UnknownType */, f_4046/* : UnknownType */(Rest_2204/* : UnknownType */(xs_4042/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Function_3 */
    ; };
    static Rest_4074 = function (xs_4067/* : Array_10 */) /* : Array_10 */{ return Skip_2178/* : UnknownType */(xs_4067/* : Array_10 */, 1/* : Integer_22 */)/* : Array_10 */; };
    static IsEmpty_4086 = function (xs_4076/* : Array_10 */) /* : Boolean_24 */{ return Equals_446/* : UnknownType */(Count_374/* : UnknownType */(xs_4076/* : Array_10 */)/* : Count_27 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static First_4095 = function (xs_4088/* : Array_10 */) /* : Any_8 */{ return At_380/* : UnknownType */(xs_4088/* : Array_10 */, 0/* : Integer_22 */)/* : Any_8 */; };
    static Last_4112 = function (xs_4097/* : Array_10 */) /* : Any_8 */{ return At_380/* : UnknownType */(xs_4097/* : Array_10 */, Subtract_233/* : UnknownType */(Count_374/* : UnknownType */(xs_4097/* : Array_10 */)/* : Count_27 */, 1/* : Integer_22 */)/* : Integer_22 */)/* : Any_8 */; };
    static Slice_4130 = function (xs_4114/* : Array_10 */, from_4116/* : Index_28 */, count_4118/* : Count_27 */) /* : Array_10 */{ return Take_2186/* : UnknownType */(Skip_2178/* : UnknownType */(xs_4114/* : Array_10 */, from_4116/* : Index_28 */)/* : Array_10 */, count_4118/* : Count_27 */)/* : Array_10 */; };
    static Join_4176 = function (xs_4132/* : Array_10 */, sep_4134/* : String_23 */) /* : String_23 */{ return IsEmpty_2210/* : UnknownType */(xs_4132/* : UnknownType */)/* : UnknownType */
        ? ""/* : String_23 */
        : Add_225/* : UnknownType */(ToString_2418/* : UnknownType */(First_2216/* : UnknownType */(xs_4132/* : Array_10 */)/* : Any_8 */)/* : String_23 */, Aggregate_2194/* : UnknownType */(Rest_2204/* : UnknownType */(xs_4132/* : Array_10 */)/* : Array_10 */, ""/* : String_23 */, function (acc_4156/* : UnknownType */, cur_4158/* : UnknownType */) /* : Lambda_2 */{ return Interpolate_207/* : UnknownType */(acc_4156/* : UnknownType */, sep_4134/* : UnknownType */, cur_4158/* : UnknownType */)/* : UnknownType */; })/* : Any_8 */)/* : Number_21 */
    ; };
    static All_4205 = function (xs_4178/* : Array_10 */, f_4180/* : Function_3 */) /* : Boolean_24 */{ return IsEmpty_2210/* : UnknownType */(xs_4178/* : UnknownType */)/* : UnknownType */
        ? True/* : Boolean_24 */
        : And_317/* : UnknownType */(f_4180/* : Function_3 */(First_2216/* : UnknownType */(xs_4178/* : UnknownType */)/* : UnknownType */)/* : Function_3 */, f_4180/* : Function_3 */(Rest_2204/* : UnknownType */(xs_4178/* : UnknownType */)/* : UnknownType */)/* : Function_3 */)/* : Boolean_24 */
    ; };
    static All_4218 = function (xs_4207/* : Array_10 */) /* : Boolean_24 */{ return All_2246/* : UnknownType */(xs_4207/* : Array_10 */, function (b_4211/* : UnknownType */) /* : Lambda_2 */{ return b_4211/* : UnknownType */; })/* : Boolean_24 */; };
}
class Interval_134_Library
{
    static Size_4233 = function (x_4220/* : Interval_20 */) /* : Numerical_13 */{ return Subtract_233/* : UnknownType */(Max_568/* : UnknownType */(x_4220/* : Interval_20 */)/* : Numerical_13 */, Min_562/* : UnknownType */(x_4220/* : Interval_20 */)/* : Numerical_13 */)/* : Number_21 */; };
    static IsEmpty_4248 = function (x_4235/* : Interval_20 */) /* : Boolean_24 */{ return GreaterThanOrEquals_2786/* : UnknownType */(Min_562/* : UnknownType */(x_4235/* : Interval_20 */)/* : Numerical_13 */, Max_568/* : UnknownType */(x_4235/* : Interval_20 */)/* : Numerical_13 */)/* : Boolean_24 */; };
    static Lerp_4280 = function (x_4250/* : Interval_20 */, amount_4252/* : Unit_29 */) /* : Numerical_13 */{ return Multiply_249/* : UnknownType */(Min_562/* : UnknownType */(x_4250/* : Interval_20 */)/* : Numerical_13 */, Add_225/* : UnknownType */(Subtract_233/* : UnknownType */(1/* : Number_21 */, amount_4252/* : Unit_29 */)/* : Number_21 */, Multiply_249/* : UnknownType */(Max_568/* : UnknownType */(x_4250/* : Interval_20 */)/* : Numerical_13 */, amount_4252/* : Unit_29 */)/* : Arithmetic_17 */)/* : Arithmetic_17 */)/* : Arithmetic_17 */; };
    static InverseLerp_4302 = function (x_4282/* : Interval_20 */, value_4284/* : Numerical_13 */) /* : Unit_29 */{ return Divide_241/* : UnknownType */(Subtract_233/* : UnknownType */(value_4284/* : Numerical_13 */, Min_562/* : UnknownType */(x_4282/* : Interval_20 */)/* : Numerical_13 */)/* : Number_21 */, Size_1435/* : UnknownType */(x_4282/* : Interval_20 */)/* : Numerical_13 */)/* : Arithmetic_17 */; };
    static Negate_4323 = function (x_4304/* : Interval_20 */) /* : Interval_20 */{ return Tuple_1/* : Error_6 */(Negative_265/* : UnknownType */(Max_568/* : UnknownType */(x_4304/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Negative_265/* : UnknownType */(Min_562/* : UnknownType */(x_4304/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Error_6 */; };
    static Reverse_4338 = function (x_4325/* : Interval_20 */) /* : Interval_20 */{ return Tuple_1/* : Error_6 */(Max_568/* : UnknownType */(x_4325/* : UnknownType */)/* : UnknownType */, Min_562/* : UnknownType */(x_4325/* : UnknownType */)/* : UnknownType */)/* : Error_6 */; };
    static Center_4347 = function (x_4340/* : Interval_20 */) /* : Numerical_13 */{ return Lerp_2272/* : UnknownType */(x_4340/* : Interval_20 */, 0.5/* : Number_21 */)/* : Numerical_13 */; };
    static Contains_4374 = function (x_4349/* : Interval_20 */, value_4351/* : Numerical_13 */) /* : Boolean_24 */{ return LessThanOrEquals_2770/* : UnknownType */(Min_562/* : UnknownType */(x_4349/* : Interval_20 */)/* : Numerical_13 */, And_317/* : UnknownType */(value_4351/* : Numerical_13 */, LessThanOrEquals_2770/* : UnknownType */(value_4351/* : Numerical_13 */, Max_568/* : UnknownType */(x_4349/* : Interval_20 */)/* : Numerical_13 */)/* : Boolean_24 */)/* : Boolean_24 */)/* : Boolean_24 */; };
    static Contains_4404 = function (x_4376/* : Interval_20 */, other_4378/* : Interval_20 */) /* : Boolean_24 */{ return LessThanOrEquals_2770/* : UnknownType */(Min_562/* : UnknownType */(x_4376/* : Interval_20 */)/* : Numerical_13 */, And_317/* : UnknownType */(Min_562/* : UnknownType */(other_4378/* : Interval_20 */)/* : Numerical_13 */, GreaterThanOrEquals_2786/* : UnknownType */(Max_568/* : Error_6 */, Max_568/* : UnknownType */(other_4378/* : Interval_20 */)/* : Numerical_13 */)/* : Boolean_24 */)/* : Boolean_24 */)/* : Boolean_24 */; };
    static Overlaps_4421 = function (x_4406/* : Interval_20 */, y_4408/* : Interval_20 */) /* : Boolean_24 */{ return Not_333/* : UnknownType */(IsEmpty_2210/* : UnknownType */(Clamp_2394/* : UnknownType */(x_4406/* : Interval_20 */, y_4408/* : Interval_20 */)/* : Interval_20 */)/* : Boolean_24 */)/* : Boolean_24 */; };
    static Split_4442 = function (x_4423/* : Interval_20 */, t_4425/* : Unit_29 */) /* : Tuple_5 */{ return Tuple_1/* : Error_6 */(Left_2344/* : UnknownType */(x_4423/* : UnknownType */, t_4425/* : UnknownType */)/* : UnknownType */, Right_2352/* : UnknownType */(x_4423/* : UnknownType */, t_4425/* : UnknownType */)/* : UnknownType */)/* : Error_6 */; };
    static Split_4451 = function (x_4444/* : Interval_20 */) /* : Tuple_5 */{ return Split_2330/* : UnknownType */(x_4444/* : Interval_20 */, 0.5/* : Number_21 */)/* : Tuple_5 */; };
    static Left_4470 = function (x_4453/* : Interval_20 */, t_4455/* : Unit_29 */) /* : Interval_20 */{ return Tuple_1/* : Error_6 */(Min_562/* : UnknownType */(x_4453/* : UnknownType */)/* : UnknownType */, Lerp_2272/* : UnknownType */(x_4453/* : UnknownType */, t_4455/* : UnknownType */)/* : UnknownType */)/* : Error_6 */; };
    static Right_4489 = function (x_4472/* : Interval_20 */, t_4474/* : Unit_29 */) /* : Interval_20 */{ return Tuple_1/* : Error_6 */(Lerp_2272/* : UnknownType */(x_4472/* : UnknownType */, t_4474/* : UnknownType */)/* : UnknownType */, Max_568/* : UnknownType */(x_4472/* : UnknownType */)/* : UnknownType */)/* : Error_6 */; };
    static MoveTo_4508 = function (x_4491/* : Interval_20 */, v_4493/* : Numerical_13 */) /* : Interval_20 */{ return Tuple_1/* : Error_6 */(v_4493/* : UnknownType */, Add_225/* : UnknownType */(v_4493/* : UnknownType */, Size_1435/* : UnknownType */(x_4491/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Error_6 */; };
    static LeftHalf_4517 = function (x_4510/* : Interval_20 */) /* : Interval_20 */{ return Left_2344/* : UnknownType */(x_4510/* : Interval_20 */, 0.5/* : Number_21 */)/* : Interval_20 */; };
    static RightHalf_4526 = function (x_4519/* : Interval_20 */) /* : Interval_20 */{ return Right_2352/* : UnknownType */(x_4519/* : Interval_20 */, 0.5/* : Number_21 */)/* : Interval_20 */; };
    static HalfSize_4536 = function (x_4528/* : Interval_20 */) /* : Numerical_13 */{ return Half_2554/* : UnknownType */(Size_1435/* : UnknownType */(x_4528/* : Interval_20 */)/* : Numerical_13 */)/* : Numerical_13 */; };
    static Recenter_4563 = function (x_4538/* : Interval_20 */, c_4540/* : Numerical_13 */) /* : Interval_20 */{ return Tuple_1/* : Error_6 */(Subtract_233/* : UnknownType */(c_4540/* : UnknownType */, HalfSize_2380/* : UnknownType */(x_4538/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Add_225/* : UnknownType */(c_4540/* : UnknownType */, HalfSize_2380/* : UnknownType */(x_4538/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Error_6 */; };
    static Clamp_4590 = function (x_4565/* : Interval_20 */, y_4567/* : Interval_20 */) /* : Interval_20 */{ return Tuple_1/* : Error_6 */(Clamp_2394/* : UnknownType */(x_4565/* : UnknownType */, Min_562/* : UnknownType */(y_4567/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Clamp_2394/* : UnknownType */(x_4565/* : UnknownType */, Max_568/* : UnknownType */(y_4567/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Error_6 */; };
    static Clamp_4624 = function (x_4592/* : Interval_20 */, value_4594/* : Numerical_13 */) /* : Numerical_13 */{ return LessThan_2762/* : UnknownType */(value_4594/* : Numerical_13 */, Min_562/* : UnknownType */(x_4592/* : UnknownType */)/* : UnknownType */
        ? Min_562/* : UnknownType */(x_4592/* : Interval_20 */)/* : Numerical_13 */
        : GreaterThan_2778/* : UnknownType */(value_4594/* : Numerical_13 */, Max_568/* : UnknownType */(x_4592/* : UnknownType */)/* : UnknownType */
            ? Max_568/* : UnknownType */(x_4592/* : Interval_20 */)/* : Numerical_13 */
            : value_4594/* : Numerical_13 */
        )/* : Boolean_24 */
    )/* : Boolean_24 */; };
    static Within_4651 = function (x_4626/* : Interval_20 */, value_4628/* : Numerical_13 */) /* : Boolean_24 */{ return GreaterThanOrEquals_2786/* : UnknownType */(value_4628/* : Numerical_13 */, And_317/* : UnknownType */(Min_562/* : UnknownType */(x_4626/* : Interval_20 */)/* : Numerical_13 */, LessThanOrEquals_2770/* : UnknownType */(value_4628/* : Numerical_13 */, Max_568/* : UnknownType */(x_4626/* : Interval_20 */)/* : Numerical_13 */)/* : Boolean_24 */)/* : Boolean_24 */)/* : Boolean_24 */; };
}
class Value_135_Library
{
    static ToString_4663 = function (x_4653/* : Value_9 */) /* : String_23 */{ return Join_2238/* : UnknownType */(FieldValues_347/* : UnknownType */(x_4653/* : Value_9 */)/* : Array_10 */, ", "/* : String_23 */)/* : String_23 */; };
}
class Vector_136_Library
{
    static Sum_4674 = function (v_4665/* : Array_10 */) /* : Numerical_13 */{ return Aggregate_2194/* : UnknownType */(v_4665/* : Array_10 */, 0/* : Integer_22 */, Add_225/* : Error_6 */)/* : Any_8 */; };
    static SumSquares_4688 = function (v_4676/* : Array_10 */) /* : Numerical_13 */{ return Aggregate_2194/* : UnknownType */(Square_2468/* : UnknownType */(v_4676/* : Array_10 */)/* : Numerical_13 */, 0/* : Integer_22 */, Add_225/* : Error_6 */)/* : Any_8 */; };
    static LengthSquared_4695 = function (v_4690/* : Array_10 */) /* : Numerical_13 */{ return SumSquares_2430/* : UnknownType */(v_4690/* : Array_10 */)/* : Numerical_13 */; };
    static Length_4705 = function (v_4697/* : Array_10 */) /* : Numerical_13 */{ return SquareRoot_2462/* : UnknownType */(LengthSquared_2436/* : UnknownType */(v_4697/* : Array_10 */)/* : Numerical_13 */)/* : Numerical_13 */; };
    static Dot_4719 = function (v1_4707/* : Vector_11 */, v2_4709/* : Vector_11 */) /* : Numerical_13 */{ return Sum_2424/* : UnknownType */(Multiply_249/* : UnknownType */(v1_4707/* : Vector_11 */, v2_4709/* : Vector_11 */)/* : Arithmetic_17 */)/* : Numerical_13 */; };
    static Normal_4731 = function (v_4721/* : Vector_11 */) /* : Vector_11 */{ return Divide_241/* : UnknownType */(v_4721/* : Vector_11 */, Length_2442/* : UnknownType */(v_4721/* : Vector_11 */)/* : Numerical_13 */)/* : Arithmetic_17 */; };
}
class Numerical_137_Library
{
    static SquareRoot_4740 = function (x_4733/* : Numerical_13 */) /* : Numerical_13 */{ return Pow_179/* : UnknownType */(x_4733/* : Numerical_13 */, 0.5/* : Number_21 */)/* : Number_21 */; };
    static Square_4749 = function (x_4742/* : Numerical_13 */) /* : Numerical_13 */{ return Multiply_249/* : UnknownType */(x_4742/* : Numerical_13 */, x_4742/* : Numerical_13 */)/* : Arithmetic_17 */; };
    static Clamp_4760 = function (x_4751/* : Numerical_13 */, i_4753/* : Interval_20 */) /* : Numerical_13 */{ return Clamp_2394/* : UnknownType */(i_4753/* : Interval_20 */, x_4751/* : Numerical_13 */)/* : Numerical_13 */; };
    static Clamp_4774 = function (x_4762/* : Numerical_13 */) /* : Numerical_13 */{ return Clamp_2394/* : UnknownType */(x_4762/* : Numerical_13 */, Tuple_1/* : Error_6 */(0/* : UnknownType */, 1/* : UnknownType */)/* : Error_6 */)/* : Interval_20 */; };
    static PlusOne_4786 = function (x_4776/* : Numerical_13 */) /* : Numerical_13 */{ return Add_225/* : UnknownType */(x_4776/* : Numerical_13 */, One_416/* : UnknownType */(x_4776/* : Numerical_13 */)/* : Numerical_13 */)/* : Arithmetic_17 */; };
    static MinusOne_4798 = function (x_4788/* : Numerical_13 */) /* : Numerical_13 */{ return Subtract_233/* : UnknownType */(x_4788/* : Numerical_13 */, One_416/* : UnknownType */(x_4788/* : Numerical_13 */)/* : Numerical_13 */)/* : Number_21 */; };
    static FromOne_4810 = function (x_4800/* : Numerical_13 */) /* : Numerical_13 */{ return Subtract_233/* : UnknownType */(One_416/* : UnknownType */(x_4800/* : Numerical_13 */)/* : Numerical_13 */, x_4800/* : Numerical_13 */)/* : Number_21 */; };
    static IsPositive_4819 = function (x_4812/* : Numerical_13 */) /* : Boolean_24 */{ return GreaterThanOrEquals_2786/* : UnknownType */(x_4812/* : Numerical_13 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static GtZ_4828 = function (x_4821/* : Numerical_13 */) /* : Boolean_24 */{ return GreaterThan_2778/* : UnknownType */(x_4821/* : Numerical_13 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static LtZ_4837 = function (x_4830/* : Numerical_13 */) /* : Boolean_24 */{ return LessThan_2762/* : UnknownType */(x_4830/* : Numerical_13 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static GtEqZ_4846 = function (x_4839/* : Numerical_13 */) /* : Boolean_24 */{ return GreaterThanOrEquals_2786/* : UnknownType */(x_4839/* : Numerical_13 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static LtEqZ_4855 = function (x_4848/* : Numerical_13 */) /* : Boolean_24 */{ return LessThanOrEquals_2770/* : UnknownType */(x_4848/* : Numerical_13 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static IsNegative_4864 = function (x_4857/* : Numerical_13 */) /* : Boolean_24 */{ return LessThan_2762/* : UnknownType */(x_4857/* : Numerical_13 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static Sign_4892 = function (x_4866/* : Numerical_13 */) /* : Numerical_13 */{ return LtZ_2518/* : UnknownType */(x_4866/* : UnknownType */)/* : UnknownType */
        ? Negative_265/* : UnknownType */(One_416/* : UnknownType */(x_4866/* : Numerical_13 */)/* : Numerical_13 */)/* : Arithmetic_17 */
        : GtZ_2512/* : UnknownType */(x_4866/* : UnknownType */)/* : UnknownType */
            ? One_416/* : UnknownType */(x_4866/* : Numerical_13 */)/* : Numerical_13 */
            : Zero_410/* : UnknownType */(x_4866/* : Numerical_13 */)/* : Numerical_13 */

    ; };
    static Abs_4905 = function (x_4894/* : Numerical_13 */) /* : Numerical_13 */{ return LtZ_2518/* : UnknownType */(x_4894/* : UnknownType */)/* : UnknownType */
        ? Negative_265/* : UnknownType */(x_4894/* : Numerical_13 */)/* : Arithmetic_17 */
        : x_4894/* : Numerical_13 */
    ; };
    static Half_4914 = function (x_4907/* : Numerical_13 */) /* : Numerical_13 */{ return Divide_241/* : UnknownType */(x_4907/* : Numerical_13 */, 2/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Third_4923 = function (x_4916/* : Numerical_13 */) /* : Numerical_13 */{ return Divide_241/* : UnknownType */(x_4916/* : Numerical_13 */, 3/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Quarter_4932 = function (x_4925/* : Numerical_13 */) /* : Numerical_13 */{ return Divide_241/* : UnknownType */(x_4925/* : Numerical_13 */, 4/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Fifth_4941 = function (x_4934/* : Numerical_13 */) /* : Numerical_13 */{ return Divide_241/* : UnknownType */(x_4934/* : Numerical_13 */, 5/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Sixth_4950 = function (x_4943/* : Numerical_13 */) /* : Numerical_13 */{ return Divide_241/* : UnknownType */(x_4943/* : Numerical_13 */, 6/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Seventh_4959 = function (x_4952/* : Numerical_13 */) /* : Numerical_13 */{ return Divide_241/* : UnknownType */(x_4952/* : Numerical_13 */, 7/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Eighth_4968 = function (x_4961/* : Numerical_13 */) /* : Numerical_13 */{ return Divide_241/* : UnknownType */(x_4961/* : Numerical_13 */, 8/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Ninth_4977 = function (x_4970/* : Numerical_13 */) /* : Numerical_13 */{ return Divide_241/* : UnknownType */(x_4970/* : Numerical_13 */, 9/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Tenth_4986 = function (x_4979/* : Numerical_13 */) /* : Numerical_13 */{ return Divide_241/* : UnknownType */(x_4979/* : Numerical_13 */, 10/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Sixteenth_4995 = function (x_4988/* : Numerical_13 */) /* : Numerical_13 */{ return Divide_241/* : UnknownType */(x_4988/* : Numerical_13 */, 16/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Hundredth_5004 = function (x_4997/* : Numerical_13 */) /* : Numerical_13 */{ return Divide_241/* : UnknownType */(x_4997/* : Numerical_13 */, 100/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Thousandth_5013 = function (x_5006/* : Numerical_13 */) /* : Numerical_13 */{ return Divide_241/* : UnknownType */(x_5006/* : Numerical_13 */, 1000/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Millionth_5027 = function (x_5015/* : Numerical_13 */) /* : Numerical_13 */{ return Divide_241/* : UnknownType */(x_5015/* : Numerical_13 */, Divide_241/* : UnknownType */(1000/* : Integer_22 */, 1000/* : Integer_22 */)/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Billionth_5046 = function (x_5029/* : Numerical_13 */) /* : Numerical_13 */{ return Divide_241/* : UnknownType */(x_5029/* : Numerical_13 */, Divide_241/* : UnknownType */(1000/* : Integer_22 */, Divide_241/* : UnknownType */(1000/* : Integer_22 */, 1000/* : Integer_22 */)/* : Integer_22 */)/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Hundred_5055 = function (x_5048/* : Numerical_13 */) /* : Numerical_13 */{ return Multiply_249/* : UnknownType */(x_5048/* : Numerical_13 */, 100/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Thousand_5064 = function (x_5057/* : Numerical_13 */) /* : Numerical_13 */{ return Multiply_249/* : UnknownType */(x_5057/* : Numerical_13 */, 1000/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Million_5078 = function (x_5066/* : Numerical_13 */) /* : Numerical_13 */{ return Multiply_249/* : UnknownType */(x_5066/* : Numerical_13 */, Multiply_249/* : UnknownType */(1000/* : Integer_22 */, 1000/* : Integer_22 */)/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Billion_5097 = function (x_5080/* : Numerical_13 */) /* : Numerical_13 */{ return Multiply_249/* : UnknownType */(x_5080/* : Numerical_13 */, Multiply_249/* : UnknownType */(1000/* : Integer_22 */, Multiply_249/* : UnknownType */(1000/* : Integer_22 */, 1000/* : Integer_22 */)/* : Integer_22 */)/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Twice_5106 = function (x_5099/* : Numerical_13 */) /* : Numerical_13 */{ return Multiply_249/* : UnknownType */(x_5099/* : Numerical_13 */, 2/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Thrice_5115 = function (x_5108/* : Numerical_13 */) /* : Numerical_13 */{ return Multiply_249/* : UnknownType */(x_5108/* : Numerical_13 */, 3/* : Integer_22 */)/* : Arithmetic_17 */; };
    static SmoothStep_5135 = function (x_5117/* : Numerical_13 */) /* : Numerical_13 */{ return Multiply_249/* : UnknownType */(Square_2468/* : UnknownType */(x_5117/* : Numerical_13 */)/* : Numerical_13 */, Subtract_233/* : UnknownType */(3/* : Integer_22 */, Twice_2662/* : UnknownType */(x_5117/* : Numerical_13 */)/* : Numerical_13 */)/* : Number_21 */)/* : Arithmetic_17 */; };
    static Pow2_5144 = function (x_5137/* : Numerical_13 */) /* : Numerical_13 */{ return Multiply_249/* : UnknownType */(x_5137/* : Numerical_13 */, x_5137/* : Numerical_13 */)/* : Arithmetic_17 */; };
    static Pow3_5156 = function (x_5146/* : Numerical_13 */) /* : Numerical_13 */{ return Multiply_249/* : UnknownType */(Pow2_2680/* : UnknownType */(x_5146/* : Numerical_13 */)/* : Numerical_13 */, x_5146/* : Numerical_13 */)/* : Arithmetic_17 */; };
    static Pow4_5168 = function (x_5158/* : Numerical_13 */) /* : Numerical_13 */{ return Multiply_249/* : UnknownType */(Pow3_2686/* : UnknownType */(x_5158/* : Numerical_13 */)/* : Numerical_13 */, x_5158/* : Numerical_13 */)/* : Arithmetic_17 */; };
    static Pow5_5180 = function (x_5170/* : Numerical_13 */) /* : Numerical_13 */{ return Multiply_249/* : UnknownType */(Pow4_2692/* : UnknownType */(x_5170/* : Numerical_13 */)/* : Numerical_13 */, x_5170/* : Numerical_13 */)/* : Arithmetic_17 */; };
    static Pi_5184 = function (self_5182/* : Numerical_137 */) /* : Number_21 */{ return 3.1415926535897/* : Number_21 */; };
    static AlmostZero_5196 = function (x_5186/* : Numerical_13 */) /* : Boolean_24 */{ return LessThan_2762/* : UnknownType */(Abs_2548/* : UnknownType */(x_5186/* : Numerical_13 */)/* : Numerical_13 */, 1E-08/* : Number_21 */)/* : Boolean_24 */; };
    static Lerp_5224 = function (a_5198/* : Numerical_13 */, b_5200/* : Numerical_13 */, t_5202/* : Unit_29 */) /* : Numerical_13 */{ return Multiply_249/* : UnknownType */(Subtract_233/* : UnknownType */(1/* : Integer_22 */, t_5202/* : Unit_29 */)/* : Number_21 */, Add_225/* : UnknownType */(a_5198/* : Numerical_13 */, Multiply_249/* : UnknownType */(t_5202/* : Unit_29 */, b_5200/* : Numerical_13 */)/* : Arithmetic_17 */)/* : Arithmetic_17 */)/* : Arithmetic_17 */; };
    static Between_5265 = function (self_5226/* : Numerical_13 */, min_5228/* : Numerical_13 */, max_5230/* : Numerical_13 */) /* : Boolean_24 */{ return Zip_2156/* : UnknownType */(FieldValues_347/* : UnknownType */(self_5226/* : Numerical_13 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(min_5228/* : Numerical_13 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(max_5230/* : Numerical_13 */)/* : Array_10 */, function (x_5247/* : UnknownType */, y_5249/* : UnknownType */, z_5251/* : UnknownType */) /* : Lambda_2 */{ return Between_2726/* : UnknownType */(x_5247/* : UnknownType */, y_5249/* : UnknownType */, z_5251/* : UnknownType */)/* : UnknownType */; })/* : Array_10 */; };
}
class Angles_138_Library
{
    static Radians_5269 = function (x_5267/* : Number_21 */) /* : Angle_82 */{ return x_5267/* : Number_21 */; };
    static Degrees_5283 = function (x_5271/* : Number_21 */) /* : Angle_82 */{ return Multiply_249/* : UnknownType */(x_5271/* : Number_21 */, Divide_241/* : UnknownType */(Pi_2704/* : Error_6 */, 180/* : Integer_22 */)/* : Number_21 */)/* : Number_21 */; };
    static Turns_5297 = function (x_5285/* : Number_21 */) /* : Angle_82 */{ return Multiply_249/* : UnknownType */(x_5285/* : Number_21 */, Multiply_249/* : UnknownType */(2/* : Integer_22 */, Pi_2704/* : Error_6 */)/* : Number_21 */)/* : Number_21 */; };
}
class Comparable_139_Library
{
    static Equals_5313 = function (a_5299/* : Comparable_15 */, b_5301/* : Comparable_15 */) /* : Boolean_24 */{ return Equals_446/* : UnknownType */(Compare_440/* : UnknownType */(a_5299/* : UnknownType */, b_5301/* : UnknownType */)/* : Error_6 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static LessThan_5329 = function (a_5315/* : Comparable_15 */, b_5317/* : Comparable_15 */) /* : Boolean_24 */{ return LessThan_2762/* : UnknownType */(Compare_440/* : UnknownType */(a_5315/* : UnknownType */, b_5317/* : UnknownType */)/* : Error_6 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static LessThanOrEquals_5345 = function (a_5331/* : Comparable_15 */, b_5333/* : Comparable_15 */) /* : Boolean_24 */{ return LessThanOrEquals_2770/* : UnknownType */(Compare_440/* : UnknownType */(a_5331/* : UnknownType */, b_5333/* : UnknownType */)/* : Error_6 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static GreaterThan_5361 = function (a_5347/* : Comparable_15 */, b_5349/* : Comparable_15 */) /* : Boolean_24 */{ return GreaterThan_2778/* : UnknownType */(Compare_440/* : UnknownType */(a_5347/* : UnknownType */, b_5349/* : UnknownType */)/* : Error_6 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static GreaterThanOrEquals_5377 = function (a_5363/* : Comparable_15 */, b_5365/* : Comparable_15 */) /* : Boolean_24 */{ return GreaterThanOrEquals_2786/* : UnknownType */(Compare_440/* : UnknownType */(a_5363/* : UnknownType */, b_5365/* : UnknownType */)/* : Error_6 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static Between_5400 = function (v_5379/* : Comparable_15 */, a_5381/* : Comparable_15 */, b_5383/* : Comparable_15 */) /* : Value_9 */{ return GreaterThanOrEquals_2786/* : UnknownType */(v_5379/* : Comparable_15 */, And_317/* : UnknownType */(a_5381/* : Comparable_15 */, LessThanOrEquals_2770/* : UnknownType */(v_5379/* : Comparable_15 */, b_5383/* : Comparable_15 */)/* : Boolean_24 */)/* : Boolean_24 */)/* : Boolean_24 */; };
    static Between_5411 = function (v_5402/* : Value_9 */, i_5404/* : Interval_20 */) /* : Interval_20 */{ return Contains_2306/* : UnknownType */(i_5404/* : Interval_20 */, v_5402/* : Value_9 */)/* : Boolean_24 */; };
    static Min_5425 = function (a_5413/* : Comparable_15 */, b_5415/* : Comparable_15 */) /* : Comparable_15 */{ return LessThanOrEquals_2770/* : UnknownType */(a_5413/* : Comparable_15 */, b_5415/* : UnknownType */
        ? a_5413/* : Comparable_15 */
        : b_5415/* : Comparable_15 */
    )/* : Boolean_24 */; };
    static Max_5439 = function (a_5427/* : Comparable_15 */, b_5429/* : Comparable_15 */) /* : Comparable_15 */{ return GreaterThanOrEquals_2786/* : UnknownType */(a_5427/* : Comparable_15 */, b_5429/* : UnknownType */
        ? a_5427/* : Comparable_15 */
        : b_5429/* : Comparable_15 */
    )/* : Boolean_24 */; };
}
class Equatable_140_Library
{
    static NotEquals_5453 = function (x_5441/* : Equatable_16 */, y_5443/* : Equatable_16 */) /* : Boolean_24 */{ return Not_333/* : UnknownType */(Equals_446/* : UnknownType */(x_5441/* : Equatable_16 */, y_5443/* : Equatable_16 */)/* : Boolean_24 */)/* : Boolean_24 */; };
}
class Easings_141_Library
{
    static BlendEaseFunc_5505 = function (p_5455/* : Number_21 */, easeIn_5457/* : Function_3 */, easeOut_5459/* : Function_3 */) /* : Number_21 */{ return LessThan_2762/* : UnknownType */(p_5455/* : Number_21 */, 0.5/* : UnknownType */
        ? Multiply_249/* : UnknownType */(0.5/* : Number_21 */, easeIn_5457/* : Function_3 */(Multiply_249/* : UnknownType */(p_5455/* : UnknownType */, 2/* : UnknownType */)/* : UnknownType */)/* : Function_3 */)/* : Number_21 */
        : Multiply_249/* : UnknownType */(0.5/* : Number_21 */, Add_225/* : UnknownType */(easeOut_5459/* : Function_3 */(Multiply_249/* : UnknownType */(p_5455/* : UnknownType */, Subtract_233/* : UnknownType */(2/* : UnknownType */, 1/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Function_3 */, 0.5/* : Number_21 */)/* : Number_21 */)/* : Number_21 */
    )/* : Boolean_24 */; };
    static InvertEaseFunc_5524 = function (p_5507/* : Number_21 */, easeIn_5509/* : Function_3 */) /* : Number_21 */{ return Subtract_233/* : UnknownType */(1/* : Integer_22 */, easeIn_5509/* : Function_3 */(Subtract_233/* : UnknownType */(1/* : UnknownType */, p_5507/* : UnknownType */)/* : UnknownType */)/* : Function_3 */)/* : Number_21 */; };
    static Linear_5528 = function (p_5526/* : Number_21 */) /* : Number_21 */{ return p_5526/* : Number_21 */; };
    static QuadraticEaseIn_5535 = function (p_5530/* : Number_21 */) /* : Number_21 */{ return Pow2_2680/* : UnknownType */(p_5530/* : Number_21 */)/* : Numerical_13 */; };
    static QuadraticEaseOut_5544 = function (p_5537/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2846/* : UnknownType */(p_5537/* : Number_21 */, QuadraticEaseIn_2860/* : Error_6 */)/* : Number_21 */; };
    static QuadraticEaseInOut_5555 = function (p_5546/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2836/* : UnknownType */(p_5546/* : Number_21 */, QuadraticEaseIn_2860/* : Error_6 */, QuadraticEaseOut_2866/* : Error_6 */)/* : Number_21 */; };
    static CubicEaseIn_5562 = function (p_5557/* : Number_21 */) /* : Number_21 */{ return Pow3_2686/* : UnknownType */(p_5557/* : Number_21 */)/* : Numerical_13 */; };
    static CubicEaseOut_5571 = function (p_5564/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2846/* : UnknownType */(p_5564/* : Number_21 */, CubicEaseIn_2878/* : Error_6 */)/* : Number_21 */; };
    static CubicEaseInOut_5582 = function (p_5573/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2836/* : UnknownType */(p_5573/* : Number_21 */, CubicEaseIn_2878/* : Error_6 */, CubicEaseOut_2884/* : Error_6 */)/* : Number_21 */; };
    static QuarticEaseIn_5589 = function (p_5584/* : Number_21 */) /* : Number_21 */{ return Pow4_2692/* : UnknownType */(p_5584/* : Number_21 */)/* : Numerical_13 */; };
    static QuarticEaseOut_5598 = function (p_5591/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2846/* : UnknownType */(p_5591/* : Number_21 */, QuarticEaseIn_2896/* : Error_6 */)/* : Number_21 */; };
    static QuarticEaseInOut_5609 = function (p_5600/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2836/* : UnknownType */(p_5600/* : Number_21 */, QuarticEaseIn_2896/* : Error_6 */, QuarticEaseOut_2902/* : Error_6 */)/* : Number_21 */; };
    static QuinticEaseIn_5616 = function (p_5611/* : Number_21 */) /* : Number_21 */{ return Pow5_2698/* : UnknownType */(p_5611/* : Number_21 */)/* : Numerical_13 */; };
    static QuinticEaseOut_5625 = function (p_5618/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2846/* : UnknownType */(p_5618/* : Number_21 */, QuinticEaseIn_2914/* : Error_6 */)/* : Number_21 */; };
    static QuinticEaseInOut_5636 = function (p_5627/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2836/* : UnknownType */(p_5627/* : Number_21 */, QuinticEaseIn_2914/* : Error_6 */, QuinticEaseOut_2920/* : Error_6 */)/* : Number_21 */; };
    static SineEaseIn_5645 = function (p_5638/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2846/* : UnknownType */(p_5638/* : Number_21 */, SineEaseOut_2938/* : Error_6 */)/* : Number_21 */; };
    static SineEaseOut_5658 = function (p_5647/* : Number_21 */) /* : Number_21 */{ return Sin_149/* : UnknownType */(Turns_2748/* : UnknownType */(Quarter_2566/* : UnknownType */(p_5647/* : Number_21 */)/* : Numerical_13 */)/* : Angle_82 */)/* : Number_21 */; };
    static SineEaseInOut_5669 = function (p_5660/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2836/* : UnknownType */(p_5660/* : Number_21 */, SineEaseIn_2932/* : Error_6 */, SineEaseOut_2938/* : Error_6 */)/* : Number_21 */; };
    static CircularEaseIn_5685 = function (p_5671/* : Number_21 */) /* : Number_21 */{ return FromOne_2500/* : UnknownType */(SquareRoot_2462/* : UnknownType */(FromOne_2500/* : UnknownType */(Pow2_2680/* : UnknownType */(p_5671/* : Number_21 */)/* : Numerical_13 */)/* : Numerical_13 */)/* : Numerical_13 */)/* : Numerical_13 */; };
    static CircularEaseOut_5694 = function (p_5687/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2846/* : UnknownType */(p_5687/* : Number_21 */, CircularEaseIn_2950/* : Error_6 */)/* : Number_21 */; };
    static CircularEaseInOut_5705 = function (p_5696/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2836/* : UnknownType */(p_5696/* : Number_21 */, CircularEaseIn_2950/* : Error_6 */, CircularEaseOut_2956/* : Error_6 */)/* : Number_21 */; };
    static ExponentialEaseIn_5728 = function (p_5707/* : Number_21 */) /* : Number_21 */{ return AlmostZero_2710/* : UnknownType */(p_5707/* : UnknownType */)/* : UnknownType */
        ? p_5707/* : Number_21 */
        : Pow_179/* : UnknownType */(2/* : Integer_22 */, Multiply_249/* : UnknownType */(10/* : Integer_22 */, MinusOne_2494/* : UnknownType */(p_5707/* : Number_21 */)/* : Numerical_13 */)/* : Arithmetic_17 */)/* : Number_21 */
    ; };
    static ExponentialEaseOut_5737 = function (p_5730/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2846/* : UnknownType */(p_5730/* : Number_21 */, ExponentialEaseIn_2968/* : Error_6 */)/* : Number_21 */; };
    static ExponentialEaseInOut_5748 = function (p_5739/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2836/* : UnknownType */(p_5739/* : Number_21 */, ExponentialEaseIn_2968/* : Error_6 */, ExponentialEaseOut_2974/* : Error_6 */)/* : Number_21 */; };
    static ElasticEaseIn_5787 = function (p_5750/* : Number_21 */) /* : Number_21 */{ return Multiply_249/* : UnknownType */(13/* : Integer_22 */, Multiply_249/* : UnknownType */(Turns_2748/* : UnknownType */(Quarter_2566/* : UnknownType */(p_5750/* : Number_21 */)/* : Numerical_13 */)/* : Angle_82 */, Sin_149/* : UnknownType */(Radians_1463/* : UnknownType */(Pow_179/* : UnknownType */(2/* : Integer_22 */, Multiply_249/* : UnknownType */(10/* : Integer_22 */, MinusOne_2494/* : UnknownType */(p_5750/* : Number_21 */)/* : Numerical_13 */)/* : Arithmetic_17 */)/* : Number_21 */)/* : Angle_82 */)/* : Number_21 */)/* : ScalarArithmetic_18 */)/* : Number_21 */; };
    static ElasticEaseOut_5796 = function (p_5789/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2846/* : UnknownType */(p_5789/* : Number_21 */, ElasticEaseIn_2986/* : Error_6 */)/* : Number_21 */; };
    static ElasticEaseInOut_5807 = function (p_5798/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2836/* : UnknownType */(p_5798/* : Number_21 */, ElasticEaseIn_2986/* : Error_6 */, ElasticEaseOut_2992/* : Error_6 */)/* : Number_21 */; };
    static BackEaseIn_5833 = function (p_5809/* : Number_21 */) /* : Number_21 */{ return Subtract_233/* : UnknownType */(Pow3_2686/* : UnknownType */(p_5809/* : Number_21 */)/* : Numerical_13 */, Multiply_249/* : UnknownType */(p_5809/* : Number_21 */, Sin_149/* : UnknownType */(Turns_2748/* : UnknownType */(Half_2554/* : UnknownType */(p_5809/* : Number_21 */)/* : Numerical_13 */)/* : Angle_82 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */; };
    static BackEaseOut_5842 = function (p_5835/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2846/* : UnknownType */(p_5835/* : Number_21 */, BackEaseIn_3004/* : Error_6 */)/* : Number_21 */; };
    static BackEaseInOut_5853 = function (p_5844/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2836/* : UnknownType */(p_5844/* : Number_21 */, BackEaseIn_3004/* : Error_6 */, BackEaseOut_3010/* : Error_6 */)/* : Number_21 */; };
    static BounceEaseIn_5862 = function (p_5855/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2846/* : UnknownType */(p_5855/* : Number_21 */, BounceEaseOut_3028/* : Error_6 */)/* : Number_21 */; };
    static BounceEaseOut_6032 = function (p_5864/* : Number_21 */) /* : Number_21 */{ return LessThan_2762/* : UnknownType */(p_5864/* : UnknownType */, Divide_241/* : UnknownType */(4/* : UnknownType */, 11/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
        ? Multiply_249/* : UnknownType */(121/* : Number_21 */, Divide_241/* : UnknownType */(Pow2_2680/* : UnknownType */(p_5864/* : Number_21 */)/* : Numerical_13 */, 16/* : Number_21 */)/* : Arithmetic_17 */)/* : Arithmetic_17 */
        : LessThan_2762/* : UnknownType */(p_5864/* : UnknownType */, Divide_241/* : UnknownType */(8/* : UnknownType */, 11/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
            ? Divide_241/* : UnknownType */(363/* : Number_21 */, Multiply_249/* : UnknownType */(40/* : Number_21 */, Subtract_233/* : UnknownType */(Pow2_2680/* : UnknownType */(p_5864/* : Number_21 */)/* : Numerical_13 */, Divide_241/* : UnknownType */(99/* : Number_21 */, Multiply_249/* : UnknownType */(10/* : Number_21 */, Add_225/* : UnknownType */(p_5864/* : Number_21 */, Divide_241/* : UnknownType */(17/* : Number_21 */, 5/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */
            : LessThan_2762/* : UnknownType */(p_5864/* : UnknownType */, Divide_241/* : UnknownType */(9/* : UnknownType */, 10/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
                ? Divide_241/* : UnknownType */(4356/* : Number_21 */, Multiply_249/* : UnknownType */(361/* : Number_21 */, Subtract_233/* : UnknownType */(Pow2_2680/* : UnknownType */(p_5864/* : Number_21 */)/* : Numerical_13 */, Divide_241/* : UnknownType */(35442/* : Number_21 */, Multiply_249/* : UnknownType */(1805/* : Number_21 */, Add_225/* : UnknownType */(p_5864/* : Number_21 */, Divide_241/* : UnknownType */(16061/* : Number_21 */, 1805/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */
                : Divide_241/* : UnknownType */(54/* : Number_21 */, Multiply_249/* : UnknownType */(5/* : Number_21 */, Subtract_233/* : UnknownType */(Pow2_2680/* : UnknownType */(p_5864/* : Number_21 */)/* : Numerical_13 */, Divide_241/* : UnknownType */(513/* : Number_21 */, Multiply_249/* : UnknownType */(25/* : Number_21 */, Add_225/* : UnknownType */(p_5864/* : Number_21 */, Divide_241/* : UnknownType */(268/* : Number_21 */, 25/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */


    ; };
    static BounceEaseInOut_6043 = function (p_6034/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2836/* : UnknownType */(p_6034/* : Number_21 */, BounceEaseIn_3022/* : Error_6 */, BounceEaseOut_3028/* : Error_6 */)/* : Number_21 */; };
}
class Any_8_Concept
{
    constructor(self) { this.Self = self; };
    static FieldNames_3155 = function (self_3154/* : Any_8 */) /* : Array_10 */{ return null; };
    static FieldValues_3158 = function (x_3157/* : Any_8 */) /* : Array_10 */{ return null; };
    static FieldTypes_3161 = function (x_3160/* : Any_8 */) /* : Array_10 */{ return null; };
    static TypeOf_3164 = function (self_3163/* : Any_8 */) /* : Type_25 */{ return null; };
}
class Value_9_Concept
{
    constructor(self) { this.Self = self; };
    static Default_3170 = function (self_3169/* : Value_9 */) /* : Value_9 */{ return null; };
}
class Array_10_Concept
{
    constructor(self) { this.Self = self; };
    static Count_3176 = function (xs_3175/* : Array_10 */) /* : Count_27 */{ return null; };
    static At_3181 = function (xs_3178/* : Array_10 */, n_3180/* : Index_28 */) /* : Any_8 */{ return null; };
}
class Vector_11_Concept
{
    constructor(self) { this.Self = self; };
    static Count_3197 = function (v_3189/* : Vector_11 */) /* : Count_27 */{ return Count_374/* : UnknownType */(FieldTypes_354/* : UnknownType */(Self_3187/* : Vector_11 */)/* : Array_10 */)/* : Count_27 */; };
    static At_3211 = function (v_3199/* : Vector_11 */, n_3201/* : Index_28 */) /* : Numerical_13 */{ return At_380/* : UnknownType */(FieldValues_347/* : UnknownType */(v_3199/* : Vector_11 */)/* : Array_10 */, n_3201/* : Index_28 */)/* : Any_8 */; };
}
class Measure_12_Concept
{
    constructor(self) { this.Self = self; };
    static Value_3230 = function (x_3220/* : Measure_12 */) /* : Number_21 */{ return At_380/* : UnknownType */(FieldValues_347/* : UnknownType */(x_3220/* : Measure_12 */)/* : Array_10 */, 0/* : Integer_22 */)/* : Any_8 */; };
}
class Numerical_13_Concept
{
    constructor(self) { this.Self = self; };
    static Zero_3240 = function (x_3239/* : Numerical_13 */) /* : Numerical_13 */{ return null; };
    static One_3243 = function (x_3242/* : Numerical_13 */) /* : Numerical_13 */{ return null; };
    static MinValue_3246 = function (x_3245/* : Numerical_13 */) /* : Numerical_13 */{ return null; };
    static MaxValue_3249 = function (x_3248/* : Numerical_13 */) /* : Numerical_13 */{ return null; };
}
class Magnitudinal_14_Concept
{
    constructor(self) { this.Self = self; };
    static Magnitude_3268 = function (x_3254/* : Magnitudinal_14 */) /* : Number_21 */{ return SquareRoot_2462/* : UnknownType */(Sum_2424/* : UnknownType */(Square_2468/* : UnknownType */(FieldValues_347/* : UnknownType */(x_3254/* : Magnitudinal_14 */)/* : Array_10 */)/* : Numerical_13 */)/* : Numerical_13 */)/* : Numerical_13 */; };
}
class Comparable_15_Concept
{
    constructor(self) { this.Self = self; };
    static Compare_3274 = function (x_3273/* : Comparable_15 */) /* : Integer_22 */{ return null; };
}
class Equatable_16_Concept
{
    constructor(self) { this.Self = self; };
    static Equals_3297 = function (a_3279/* : Equatable_16 */, b_3281/* : Equatable_16 */) /* : Boolean_24 */{ return All_2246/* : UnknownType */(Equals_446/* : UnknownType */(FieldValues_347/* : UnknownType */(a_3279/* : Equatable_16 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(b_3281/* : Equatable_16 */)/* : Array_10 */)/* : Boolean_24 */)/* : Boolean_24 */; };
}
class Arithmetic_17_Concept
{
    constructor(self) { this.Self = self; };
    static Add_3317 = function (self_3302/* : Arithmetic_17 */, other_3304/* : Arithmetic_17 */) /* : Arithmetic_17 */{ return Add_225/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3302/* : Arithmetic_17 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(other_3304/* : Arithmetic_17 */)/* : Array_10 */)/* : Number_21 */; };
    static Negative_3327 = function (self_3319/* : Arithmetic_17 */) /* : Arithmetic_17 */{ return Negative_265/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3319/* : Arithmetic_17 */)/* : Array_10 */)/* : Number_21 */; };
    static Reciprocal_3337 = function (self_3329/* : Arithmetic_17 */) /* : Arithmetic_17 */{ return Reciprocal_468/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3329/* : Arithmetic_17 */)/* : Array_10 */)/* : Arithmetic_17 */; };
    static Multiply_3354 = function (self_3339/* : Arithmetic_17 */, other_3341/* : Arithmetic_17 */) /* : Arithmetic_17 */{ return Add_225/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3339/* : Arithmetic_17 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(other_3341/* : Arithmetic_17 */)/* : Array_10 */)/* : Number_21 */; };
    static Divide_3371 = function (self_3356/* : Arithmetic_17 */, other_3358/* : Arithmetic_17 */) /* : Arithmetic_17 */{ return Divide_241/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3356/* : Arithmetic_17 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(other_3358/* : Arithmetic_17 */)/* : Array_10 */)/* : Number_21 */; };
    static Modulo_3388 = function (self_3373/* : Arithmetic_17 */, other_3375/* : Arithmetic_17 */) /* : Arithmetic_17 */{ return Modulo_257/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3373/* : Arithmetic_17 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(other_3375/* : Arithmetic_17 */)/* : Array_10 */)/* : Number_21 */; };
}
class ScalarArithmetic_18_Concept
{
    constructor(self) { this.Self = self; };
    static Add_3405 = function (self_3393/* : ScalarArithmetic_18 */, scalar_3395/* : Number_21 */) /* : ScalarArithmetic_18 */{ return Add_225/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3393/* : ScalarArithmetic_18 */)/* : Array_10 */, scalar_3395/* : Number_21 */)/* : Number_21 */; };
    static Subtract_3419 = function (self_3407/* : ScalarArithmetic_18 */, scalar_3409/* : Number_21 */) /* : ScalarArithmetic_18 */{ return Add_225/* : UnknownType */(self_3407/* : ScalarArithmetic_18 */, Negative_265/* : UnknownType */(scalar_3409/* : Number_21 */)/* : Number_21 */)/* : ScalarArithmetic_18 */; };
    static Multiply_3433 = function (self_3421/* : ScalarArithmetic_18 */, scalar_3423/* : Number_21 */) /* : ScalarArithmetic_18 */{ return Multiply_249/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3421/* : ScalarArithmetic_18 */)/* : Array_10 */, scalar_3423/* : Number_21 */)/* : Number_21 */; };
    static Divide_3447 = function (self_3435/* : ScalarArithmetic_18 */, scalar_3437/* : Number_21 */) /* : ScalarArithmetic_18 */{ return Multiply_249/* : UnknownType */(self_3435/* : ScalarArithmetic_18 */, Reciprocal_468/* : UnknownType */(scalar_3437/* : Number_21 */)/* : Arithmetic_17 */)/* : Number_21 */; };
    static Modulo_3461 = function (self_3449/* : ScalarArithmetic_18 */, scalar_3451/* : Number_21 */) /* : ScalarArithmetic_18 */{ return Modulo_257/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3449/* : ScalarArithmetic_18 */)/* : Array_10 */, scalar_3451/* : Number_21 */)/* : Number_21 */; };
}
class BooleanOperations_19_Concept
{
    constructor(self) { this.Self = self; };
    static And_3480 = function (a_3465/* : BooleanOperations_19 */, b_3467/* : BooleanOperations_19 */) /* : BooleanOperations_19 */{ return And_317/* : UnknownType */(FieldValues_347/* : UnknownType */(a_3465/* : BooleanOperations_19 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(b_3467/* : BooleanOperations_19 */)/* : Array_10 */)/* : Boolean_24 */; };
    static Or_3497 = function (a_3482/* : BooleanOperations_19 */, b_3484/* : BooleanOperations_19 */) /* : BooleanOperations_19 */{ return Or_325/* : UnknownType */(FieldValues_347/* : UnknownType */(a_3482/* : BooleanOperations_19 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(b_3484/* : BooleanOperations_19 */)/* : Array_10 */)/* : Boolean_24 */; };
    static Not_3507 = function (a_3499/* : BooleanOperations_19 */) /* : BooleanOperations_19 */{ return Not_333/* : UnknownType */(FieldValues_347/* : UnknownType */(a_3499/* : BooleanOperations_19 */)/* : Array_10 */)/* : Boolean_24 */; };
}
class Interval_20_Concept
{
    constructor(self) { this.Self = self; };
    static Min_3514 = function (x_3513/* : Interval_20 */) /* : Numerical_13 */{ return null; };
    static Max_3517 = function (x_3516/* : Interval_20 */) /* : Numerical_13 */{ return null; };
}
class Number_21_Type
{
    constructor()
    {
        // field initialization 
        this.Zero_3240 = Number_21_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = Number_21_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = Number_21_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = Number_21_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = Number_21_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Number_21_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Number_21_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Number_21_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Number_21_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = Number_21_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = Number_21_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = Number_21_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = Number_21_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = Number_21_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = Number_21_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = Number_21_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Number_21_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Number_21_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Number_21_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Number_21_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Number_21_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Number_21_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Number_21_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Number_21_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Number_21_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Number_21_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Number_21_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Number_21_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Number_21_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Number_21_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Number_21_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Number_21_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Number_21_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Number_21_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Number_21_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Number_21_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Number_21_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Number_21_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    // implemented concepts 
    static Numerical_13_Concept = new Numerical_13_Concept(Number_21_Type);
    static Value_9_Concept = new Value_9_Concept(Number_21_Type);
    static Any_8_Concept = new Any_8_Concept(Number_21_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(Number_21_Type);
    static Value_9_Concept = new Value_9_Concept(Number_21_Type);
    static Any_8_Concept = new Any_8_Concept(Number_21_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Number_21_Type);
    static Value_9_Concept = new Value_9_Concept(Number_21_Type);
    static Any_8_Concept = new Any_8_Concept(Number_21_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Number_21_Type);
    static Value_9_Concept = new Value_9_Concept(Number_21_Type);
    static Any_8_Concept = new Any_8_Concept(Number_21_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Number_21_Type);
    static Value_9_Concept = new Value_9_Concept(Number_21_Type);
    static Any_8_Concept = new Any_8_Concept(Number_21_Type);
    static Implements = [Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Integer_22_Type
{
    constructor()
    {
        // field initialization 
        this.Zero_3240 = Integer_22_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = Integer_22_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = Integer_22_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = Integer_22_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = Integer_22_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Integer_22_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Integer_22_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Integer_22_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Integer_22_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = Integer_22_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = Integer_22_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = Integer_22_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = Integer_22_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = Integer_22_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = Integer_22_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = Integer_22_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Integer_22_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Integer_22_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Integer_22_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Integer_22_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Integer_22_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Integer_22_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Integer_22_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Integer_22_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Integer_22_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Integer_22_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Integer_22_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Integer_22_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Integer_22_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Integer_22_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Integer_22_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Integer_22_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Integer_22_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Integer_22_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Integer_22_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Integer_22_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Integer_22_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Integer_22_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    // implemented concepts 
    static Numerical_13_Concept = new Numerical_13_Concept(Integer_22_Type);
    static Value_9_Concept = new Value_9_Concept(Integer_22_Type);
    static Any_8_Concept = new Any_8_Concept(Integer_22_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(Integer_22_Type);
    static Value_9_Concept = new Value_9_Concept(Integer_22_Type);
    static Any_8_Concept = new Any_8_Concept(Integer_22_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Integer_22_Type);
    static Value_9_Concept = new Value_9_Concept(Integer_22_Type);
    static Any_8_Concept = new Any_8_Concept(Integer_22_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Integer_22_Type);
    static Value_9_Concept = new Value_9_Concept(Integer_22_Type);
    static Any_8_Concept = new Any_8_Concept(Integer_22_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Integer_22_Type);
    static Value_9_Concept = new Value_9_Concept(Integer_22_Type);
    static Any_8_Concept = new Any_8_Concept(Integer_22_Type);
    static Implements = [Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class String_23_Type
{
    constructor()
    {
        // field initialization 
        this.Default_3170 = String_23_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = String_23_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = String_23_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = String_23_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = String_23_Type.Any_8_Concept.TypeOf_3164;
        this.Count_3176 = String_23_Type.Array_10_Concept.Count_3176;
        this.At_3181 = String_23_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = String_23_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = String_23_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = String_23_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = String_23_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(String_23_Type);
    static Any_8_Concept = new Any_8_Concept(String_23_Type);
    static Array_10_Concept = new Array_10_Concept(String_23_Type);
    static Any_8_Concept = new Any_8_Concept(String_23_Type);
    static Implements = [Value_9_Concept,Any_8_Concept,Array_10_Concept,Any_8_Concept];
}
class Boolean_24_Type
{
    constructor()
    {
        // field initialization 
        this.Default_3170 = Boolean_24_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Boolean_24_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Boolean_24_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Boolean_24_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Boolean_24_Type.Any_8_Concept.TypeOf_3164;
        this.And_3480 = Boolean_24_Type.BooleanOperations_19_Concept.And_3480;
        this.Or_3497 = Boolean_24_Type.BooleanOperations_19_Concept.Or_3497;
        this.Not_3507 = Boolean_24_Type.BooleanOperations_19_Concept.Not_3507;
    }
    // field accessors
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Boolean_24_Type);
    static Any_8_Concept = new Any_8_Concept(Boolean_24_Type);
    static BooleanOperations_19_Concept = new BooleanOperations_19_Concept(Boolean_24_Type);
    static Implements = [Value_9_Concept,Any_8_Concept,BooleanOperations_19_Concept];
}
class Type_25_Type
{
    constructor(Name_574)
    {
        // field initialization 
        this.Name_574 = Name_574;
        this.Default_3170 = Type_25_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Type_25_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Type_25_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Type_25_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Type_25_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Name_574 = function(self) { return self.Name_574; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Type_25_Type);
    static Any_8_Concept = new Any_8_Concept(Type_25_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Character_26_Type
{
    constructor()
    {
        // field initialization 
        this.Default_3170 = Character_26_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Character_26_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Character_26_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Character_26_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Character_26_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Character_26_Type);
    static Any_8_Concept = new Any_8_Concept(Character_26_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Count_27_Type
{
    constructor(Value_581)
    {
        // field initialization 
        this.Value_581 = Value_581;
        this.Zero_3240 = Count_27_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = Count_27_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = Count_27_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = Count_27_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = Count_27_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Count_27_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Count_27_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Count_27_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Count_27_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = Count_27_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = Count_27_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = Count_27_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = Count_27_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = Count_27_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = Count_27_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = Count_27_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Count_27_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Count_27_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Count_27_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Count_27_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Count_27_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Count_27_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Count_27_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Count_27_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Count_27_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Count_27_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Count_27_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Count_27_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Count_27_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Count_27_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Count_27_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Count_27_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Count_27_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Count_27_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Count_27_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Count_27_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Count_27_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Count_27_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Value_581 = function(self) { return self.Value_581; }
    // implemented concepts 
    static Numerical_13_Concept = new Numerical_13_Concept(Count_27_Type);
    static Value_9_Concept = new Value_9_Concept(Count_27_Type);
    static Any_8_Concept = new Any_8_Concept(Count_27_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(Count_27_Type);
    static Value_9_Concept = new Value_9_Concept(Count_27_Type);
    static Any_8_Concept = new Any_8_Concept(Count_27_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Count_27_Type);
    static Value_9_Concept = new Value_9_Concept(Count_27_Type);
    static Any_8_Concept = new Any_8_Concept(Count_27_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Count_27_Type);
    static Value_9_Concept = new Value_9_Concept(Count_27_Type);
    static Any_8_Concept = new Any_8_Concept(Count_27_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Count_27_Type);
    static Value_9_Concept = new Value_9_Concept(Count_27_Type);
    static Any_8_Concept = new Any_8_Concept(Count_27_Type);
    static Implements = [Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Index_28_Type
{
    constructor(Value_588)
    {
        // field initialization 
        this.Value_588 = Value_588;
        this.Default_3170 = Index_28_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Index_28_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Index_28_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Index_28_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Index_28_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Value_588 = function(self) { return self.Value_588; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Index_28_Type);
    static Any_8_Concept = new Any_8_Concept(Index_28_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Unit_29_Type
{
    constructor(Value_595)
    {
        // field initialization 
        this.Value_595 = Value_595;
        this.Zero_3240 = Unit_29_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = Unit_29_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = Unit_29_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = Unit_29_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = Unit_29_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Unit_29_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Unit_29_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Unit_29_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Unit_29_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = Unit_29_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = Unit_29_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = Unit_29_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = Unit_29_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = Unit_29_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = Unit_29_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = Unit_29_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Unit_29_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Unit_29_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Unit_29_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Unit_29_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Unit_29_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Unit_29_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Unit_29_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Unit_29_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Unit_29_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Unit_29_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Unit_29_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Unit_29_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Unit_29_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Unit_29_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Unit_29_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Unit_29_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Unit_29_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Unit_29_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Unit_29_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Unit_29_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Unit_29_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Unit_29_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Value_595 = function(self) { return self.Value_595; }
    // implemented concepts 
    static Numerical_13_Concept = new Numerical_13_Concept(Unit_29_Type);
    static Value_9_Concept = new Value_9_Concept(Unit_29_Type);
    static Any_8_Concept = new Any_8_Concept(Unit_29_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(Unit_29_Type);
    static Value_9_Concept = new Value_9_Concept(Unit_29_Type);
    static Any_8_Concept = new Any_8_Concept(Unit_29_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Unit_29_Type);
    static Value_9_Concept = new Value_9_Concept(Unit_29_Type);
    static Any_8_Concept = new Any_8_Concept(Unit_29_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Unit_29_Type);
    static Value_9_Concept = new Value_9_Concept(Unit_29_Type);
    static Any_8_Concept = new Any_8_Concept(Unit_29_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Unit_29_Type);
    static Value_9_Concept = new Value_9_Concept(Unit_29_Type);
    static Any_8_Concept = new Any_8_Concept(Unit_29_Type);
    static Implements = [Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Percent_30_Type
{
    constructor(Value_602)
    {
        // field initialization 
        this.Value_602 = Value_602;
        this.Zero_3240 = Percent_30_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = Percent_30_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = Percent_30_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = Percent_30_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = Percent_30_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Percent_30_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Percent_30_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Percent_30_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Percent_30_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = Percent_30_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = Percent_30_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = Percent_30_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = Percent_30_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = Percent_30_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = Percent_30_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = Percent_30_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Percent_30_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Percent_30_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Percent_30_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Percent_30_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Percent_30_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Percent_30_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Percent_30_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Percent_30_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Percent_30_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Percent_30_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Percent_30_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Percent_30_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Percent_30_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Percent_30_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Percent_30_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Percent_30_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Percent_30_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Percent_30_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Percent_30_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Percent_30_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Percent_30_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Percent_30_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Value_602 = function(self) { return self.Value_602; }
    // implemented concepts 
    static Numerical_13_Concept = new Numerical_13_Concept(Percent_30_Type);
    static Value_9_Concept = new Value_9_Concept(Percent_30_Type);
    static Any_8_Concept = new Any_8_Concept(Percent_30_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(Percent_30_Type);
    static Value_9_Concept = new Value_9_Concept(Percent_30_Type);
    static Any_8_Concept = new Any_8_Concept(Percent_30_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Percent_30_Type);
    static Value_9_Concept = new Value_9_Concept(Percent_30_Type);
    static Any_8_Concept = new Any_8_Concept(Percent_30_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Percent_30_Type);
    static Value_9_Concept = new Value_9_Concept(Percent_30_Type);
    static Any_8_Concept = new Any_8_Concept(Percent_30_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Percent_30_Type);
    static Value_9_Concept = new Value_9_Concept(Percent_30_Type);
    static Any_8_Concept = new Any_8_Concept(Percent_30_Type);
    static Implements = [Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Quaternion_31_Type
{
    constructor(X_609, Y_616, Z_623, W_630)
    {
        // field initialization 
        this.X_609 = X_609;
        this.Y_616 = Y_616;
        this.Z_623 = Z_623;
        this.W_630 = W_630;
        this.Default_3170 = Quaternion_31_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Quaternion_31_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Quaternion_31_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Quaternion_31_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Quaternion_31_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static X_609 = function(self) { return self.X_609; }
    static Y_616 = function(self) { return self.Y_616; }
    static Z_623 = function(self) { return self.Z_623; }
    static W_630 = function(self) { return self.W_630; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Quaternion_31_Type);
    static Any_8_Concept = new Any_8_Concept(Quaternion_31_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Unit2D_32_Type
{
    constructor(X_637, Y_644)
    {
        // field initialization 
        this.X_637 = X_637;
        this.Y_644 = Y_644;
        this.Default_3170 = Unit2D_32_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Unit2D_32_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Unit2D_32_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Unit2D_32_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Unit2D_32_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static X_637 = function(self) { return self.X_637; }
    static Y_644 = function(self) { return self.Y_644; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Unit2D_32_Type);
    static Any_8_Concept = new Any_8_Concept(Unit2D_32_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Unit3D_33_Type
{
    constructor(X_651, Y_658, Z_665)
    {
        // field initialization 
        this.X_651 = X_651;
        this.Y_658 = Y_658;
        this.Z_665 = Z_665;
        this.Default_3170 = Unit3D_33_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Unit3D_33_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Unit3D_33_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Unit3D_33_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Unit3D_33_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static X_651 = function(self) { return self.X_651; }
    static Y_658 = function(self) { return self.Y_658; }
    static Z_665 = function(self) { return self.Z_665; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Unit3D_33_Type);
    static Any_8_Concept = new Any_8_Concept(Unit3D_33_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Direction3D_34_Type
{
    constructor(Value_672)
    {
        // field initialization 
        this.Value_672 = Value_672;
        this.Default_3170 = Direction3D_34_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Direction3D_34_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Direction3D_34_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Direction3D_34_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Direction3D_34_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Value_672 = function(self) { return self.Value_672; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Direction3D_34_Type);
    static Any_8_Concept = new Any_8_Concept(Direction3D_34_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class AxisAngle_35_Type
{
    constructor(Axis_679, Angle_686)
    {
        // field initialization 
        this.Axis_679 = Axis_679;
        this.Angle_686 = Angle_686;
        this.Default_3170 = AxisAngle_35_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AxisAngle_35_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AxisAngle_35_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AxisAngle_35_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AxisAngle_35_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Axis_679 = function(self) { return self.Axis_679; }
    static Angle_686 = function(self) { return self.Angle_686; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(AxisAngle_35_Type);
    static Any_8_Concept = new Any_8_Concept(AxisAngle_35_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class EulerAngles_36_Type
{
    constructor(Yaw_693, Pitch_700, Roll_707)
    {
        // field initialization 
        this.Yaw_693 = Yaw_693;
        this.Pitch_700 = Pitch_700;
        this.Roll_707 = Roll_707;
        this.Default_3170 = EulerAngles_36_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = EulerAngles_36_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = EulerAngles_36_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = EulerAngles_36_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = EulerAngles_36_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Yaw_693 = function(self) { return self.Yaw_693; }
    static Pitch_700 = function(self) { return self.Pitch_700; }
    static Roll_707 = function(self) { return self.Roll_707; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(EulerAngles_36_Type);
    static Any_8_Concept = new Any_8_Concept(EulerAngles_36_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Rotation3D_37_Type
{
    constructor(Quaternion_714)
    {
        // field initialization 
        this.Quaternion_714 = Quaternion_714;
        this.Default_3170 = Rotation3D_37_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Rotation3D_37_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Rotation3D_37_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Rotation3D_37_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Rotation3D_37_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Quaternion_714 = function(self) { return self.Quaternion_714; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Rotation3D_37_Type);
    static Any_8_Concept = new Any_8_Concept(Rotation3D_37_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Vector2D_38_Type
{
    constructor(X_721, Y_728)
    {
        // field initialization 
        this.X_721 = X_721;
        this.Y_728 = Y_728;
        this.Count_3197 = Vector2D_38_Type.Vector_11_Concept.Count_3197;
        this.At_3211 = Vector2D_38_Type.Vector_11_Concept.At_3211;
        this.Count_3176 = Vector2D_38_Type.Array_10_Concept.Count_3176;
        this.At_3181 = Vector2D_38_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = Vector2D_38_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector2D_38_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector2D_38_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector2D_38_Type.Any_8_Concept.TypeOf_3164;
        this.Zero_3240 = Vector2D_38_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = Vector2D_38_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = Vector2D_38_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = Vector2D_38_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = Vector2D_38_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector2D_38_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector2D_38_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector2D_38_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector2D_38_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = Vector2D_38_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = Vector2D_38_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = Vector2D_38_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = Vector2D_38_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = Vector2D_38_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = Vector2D_38_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = Vector2D_38_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector2D_38_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector2D_38_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector2D_38_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector2D_38_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Vector2D_38_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Vector2D_38_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector2D_38_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector2D_38_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector2D_38_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector2D_38_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Vector2D_38_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Vector2D_38_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector2D_38_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector2D_38_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector2D_38_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector2D_38_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Vector2D_38_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Vector2D_38_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector2D_38_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector2D_38_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector2D_38_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector2D_38_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Vector2D_38_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Vector2D_38_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Vector2D_38_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Vector2D_38_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Vector2D_38_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Vector2D_38_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector2D_38_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector2D_38_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector2D_38_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector2D_38_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static X_721 = function(self) { return self.X_721; }
    static Y_728 = function(self) { return self.Y_728; }
    // implemented concepts 
    static Vector_11_Concept = new Vector_11_Concept(Vector2D_38_Type);
    static Array_10_Concept = new Array_10_Concept(Vector2D_38_Type);
    static Any_8_Concept = new Any_8_Concept(Vector2D_38_Type);
    static Numerical_13_Concept = new Numerical_13_Concept(Vector2D_38_Type);
    static Value_9_Concept = new Value_9_Concept(Vector2D_38_Type);
    static Any_8_Concept = new Any_8_Concept(Vector2D_38_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(Vector2D_38_Type);
    static Value_9_Concept = new Value_9_Concept(Vector2D_38_Type);
    static Any_8_Concept = new Any_8_Concept(Vector2D_38_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Vector2D_38_Type);
    static Value_9_Concept = new Value_9_Concept(Vector2D_38_Type);
    static Any_8_Concept = new Any_8_Concept(Vector2D_38_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Vector2D_38_Type);
    static Value_9_Concept = new Value_9_Concept(Vector2D_38_Type);
    static Any_8_Concept = new Any_8_Concept(Vector2D_38_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Vector2D_38_Type);
    static Value_9_Concept = new Value_9_Concept(Vector2D_38_Type);
    static Any_8_Concept = new Any_8_Concept(Vector2D_38_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Vector2D_38_Type);
    static Value_9_Concept = new Value_9_Concept(Vector2D_38_Type);
    static Any_8_Concept = new Any_8_Concept(Vector2D_38_Type);
    static Implements = [Vector_11_Concept,Array_10_Concept,Any_8_Concept,Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept];
}
class Vector3D_39_Type
{
    constructor(X_735, Y_742, Z_749)
    {
        // field initialization 
        this.X_735 = X_735;
        this.Y_742 = Y_742;
        this.Z_749 = Z_749;
        this.Count_3197 = Vector3D_39_Type.Vector_11_Concept.Count_3197;
        this.At_3211 = Vector3D_39_Type.Vector_11_Concept.At_3211;
        this.Count_3176 = Vector3D_39_Type.Array_10_Concept.Count_3176;
        this.At_3181 = Vector3D_39_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = Vector3D_39_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector3D_39_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector3D_39_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector3D_39_Type.Any_8_Concept.TypeOf_3164;
        this.Zero_3240 = Vector3D_39_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = Vector3D_39_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = Vector3D_39_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = Vector3D_39_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = Vector3D_39_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector3D_39_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector3D_39_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector3D_39_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector3D_39_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = Vector3D_39_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = Vector3D_39_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = Vector3D_39_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = Vector3D_39_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = Vector3D_39_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = Vector3D_39_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = Vector3D_39_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector3D_39_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector3D_39_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector3D_39_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector3D_39_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Vector3D_39_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Vector3D_39_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector3D_39_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector3D_39_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector3D_39_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector3D_39_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Vector3D_39_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Vector3D_39_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector3D_39_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector3D_39_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector3D_39_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector3D_39_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Vector3D_39_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Vector3D_39_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector3D_39_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector3D_39_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector3D_39_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector3D_39_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Vector3D_39_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Vector3D_39_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Vector3D_39_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Vector3D_39_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Vector3D_39_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Vector3D_39_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector3D_39_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector3D_39_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector3D_39_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector3D_39_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static X_735 = function(self) { return self.X_735; }
    static Y_742 = function(self) { return self.Y_742; }
    static Z_749 = function(self) { return self.Z_749; }
    // implemented concepts 
    static Vector_11_Concept = new Vector_11_Concept(Vector3D_39_Type);
    static Array_10_Concept = new Array_10_Concept(Vector3D_39_Type);
    static Any_8_Concept = new Any_8_Concept(Vector3D_39_Type);
    static Numerical_13_Concept = new Numerical_13_Concept(Vector3D_39_Type);
    static Value_9_Concept = new Value_9_Concept(Vector3D_39_Type);
    static Any_8_Concept = new Any_8_Concept(Vector3D_39_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(Vector3D_39_Type);
    static Value_9_Concept = new Value_9_Concept(Vector3D_39_Type);
    static Any_8_Concept = new Any_8_Concept(Vector3D_39_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Vector3D_39_Type);
    static Value_9_Concept = new Value_9_Concept(Vector3D_39_Type);
    static Any_8_Concept = new Any_8_Concept(Vector3D_39_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Vector3D_39_Type);
    static Value_9_Concept = new Value_9_Concept(Vector3D_39_Type);
    static Any_8_Concept = new Any_8_Concept(Vector3D_39_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Vector3D_39_Type);
    static Value_9_Concept = new Value_9_Concept(Vector3D_39_Type);
    static Any_8_Concept = new Any_8_Concept(Vector3D_39_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Vector3D_39_Type);
    static Value_9_Concept = new Value_9_Concept(Vector3D_39_Type);
    static Any_8_Concept = new Any_8_Concept(Vector3D_39_Type);
    static Implements = [Vector_11_Concept,Array_10_Concept,Any_8_Concept,Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept];
}
class Vector4D_40_Type
{
    constructor(X_756, Y_763, Z_770, W_777)
    {
        // field initialization 
        this.X_756 = X_756;
        this.Y_763 = Y_763;
        this.Z_770 = Z_770;
        this.W_777 = W_777;
        this.Count_3197 = Vector4D_40_Type.Vector_11_Concept.Count_3197;
        this.At_3211 = Vector4D_40_Type.Vector_11_Concept.At_3211;
        this.Count_3176 = Vector4D_40_Type.Array_10_Concept.Count_3176;
        this.At_3181 = Vector4D_40_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = Vector4D_40_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector4D_40_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector4D_40_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector4D_40_Type.Any_8_Concept.TypeOf_3164;
        this.Zero_3240 = Vector4D_40_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = Vector4D_40_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = Vector4D_40_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = Vector4D_40_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = Vector4D_40_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector4D_40_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector4D_40_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector4D_40_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector4D_40_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = Vector4D_40_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = Vector4D_40_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = Vector4D_40_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = Vector4D_40_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = Vector4D_40_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = Vector4D_40_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = Vector4D_40_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector4D_40_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector4D_40_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector4D_40_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector4D_40_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Vector4D_40_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Vector4D_40_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector4D_40_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector4D_40_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector4D_40_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector4D_40_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Vector4D_40_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Vector4D_40_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector4D_40_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector4D_40_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector4D_40_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector4D_40_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Vector4D_40_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Vector4D_40_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector4D_40_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector4D_40_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector4D_40_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector4D_40_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Vector4D_40_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Vector4D_40_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Vector4D_40_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Vector4D_40_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Vector4D_40_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Vector4D_40_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Vector4D_40_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Vector4D_40_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Vector4D_40_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Vector4D_40_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static X_756 = function(self) { return self.X_756; }
    static Y_763 = function(self) { return self.Y_763; }
    static Z_770 = function(self) { return self.Z_770; }
    static W_777 = function(self) { return self.W_777; }
    // implemented concepts 
    static Vector_11_Concept = new Vector_11_Concept(Vector4D_40_Type);
    static Array_10_Concept = new Array_10_Concept(Vector4D_40_Type);
    static Any_8_Concept = new Any_8_Concept(Vector4D_40_Type);
    static Numerical_13_Concept = new Numerical_13_Concept(Vector4D_40_Type);
    static Value_9_Concept = new Value_9_Concept(Vector4D_40_Type);
    static Any_8_Concept = new Any_8_Concept(Vector4D_40_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(Vector4D_40_Type);
    static Value_9_Concept = new Value_9_Concept(Vector4D_40_Type);
    static Any_8_Concept = new Any_8_Concept(Vector4D_40_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Vector4D_40_Type);
    static Value_9_Concept = new Value_9_Concept(Vector4D_40_Type);
    static Any_8_Concept = new Any_8_Concept(Vector4D_40_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Vector4D_40_Type);
    static Value_9_Concept = new Value_9_Concept(Vector4D_40_Type);
    static Any_8_Concept = new Any_8_Concept(Vector4D_40_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Vector4D_40_Type);
    static Value_9_Concept = new Value_9_Concept(Vector4D_40_Type);
    static Any_8_Concept = new Any_8_Concept(Vector4D_40_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Vector4D_40_Type);
    static Value_9_Concept = new Value_9_Concept(Vector4D_40_Type);
    static Any_8_Concept = new Any_8_Concept(Vector4D_40_Type);
    static Implements = [Vector_11_Concept,Array_10_Concept,Any_8_Concept,Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept];
}
class Orientation3D_41_Type
{
    constructor(Value_784)
    {
        // field initialization 
        this.Value_784 = Value_784;
        this.Default_3170 = Orientation3D_41_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Orientation3D_41_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Orientation3D_41_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Orientation3D_41_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Orientation3D_41_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Value_784 = function(self) { return self.Value_784; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Orientation3D_41_Type);
    static Any_8_Concept = new Any_8_Concept(Orientation3D_41_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Pose2D_42_Type
{
    constructor(Position_791, Orientation_798)
    {
        // field initialization 
        this.Position_791 = Position_791;
        this.Orientation_798 = Orientation_798;
        this.Default_3170 = Pose2D_42_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Pose2D_42_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Pose2D_42_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Pose2D_42_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Pose2D_42_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Position_791 = function(self) { return self.Position_791; }
    static Orientation_798 = function(self) { return self.Orientation_798; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Pose2D_42_Type);
    static Any_8_Concept = new Any_8_Concept(Pose2D_42_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Pose3D_43_Type
{
    constructor(Position_805, Orientation_812)
    {
        // field initialization 
        this.Position_805 = Position_805;
        this.Orientation_812 = Orientation_812;
        this.Default_3170 = Pose3D_43_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Pose3D_43_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Pose3D_43_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Pose3D_43_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Pose3D_43_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Position_805 = function(self) { return self.Position_805; }
    static Orientation_812 = function(self) { return self.Orientation_812; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Pose3D_43_Type);
    static Any_8_Concept = new Any_8_Concept(Pose3D_43_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Transform3D_44_Type
{
    constructor(Translation_819, Rotation_826, Scale_833)
    {
        // field initialization 
        this.Translation_819 = Translation_819;
        this.Rotation_826 = Rotation_826;
        this.Scale_833 = Scale_833;
        this.Default_3170 = Transform3D_44_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Transform3D_44_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Transform3D_44_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Transform3D_44_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Transform3D_44_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Translation_819 = function(self) { return self.Translation_819; }
    static Rotation_826 = function(self) { return self.Rotation_826; }
    static Scale_833 = function(self) { return self.Scale_833; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Transform3D_44_Type);
    static Any_8_Concept = new Any_8_Concept(Transform3D_44_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Transform2D_45_Type
{
    constructor(Translation_840, Rotation_847, Scale_854)
    {
        // field initialization 
        this.Translation_840 = Translation_840;
        this.Rotation_847 = Rotation_847;
        this.Scale_854 = Scale_854;
        this.Default_3170 = Transform2D_45_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Transform2D_45_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Transform2D_45_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Transform2D_45_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Transform2D_45_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Translation_840 = function(self) { return self.Translation_840; }
    static Rotation_847 = function(self) { return self.Rotation_847; }
    static Scale_854 = function(self) { return self.Scale_854; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Transform2D_45_Type);
    static Any_8_Concept = new Any_8_Concept(Transform2D_45_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class AlignedBox2D_46_Type
{
    constructor(A_861, B_868)
    {
        // field initialization 
        this.A_861 = A_861;
        this.B_868 = B_868;
        this.Min_3514 = AlignedBox2D_46_Type.Interval_20_Concept.Min_3514;
        this.Max_3517 = AlignedBox2D_46_Type.Interval_20_Concept.Max_3517;
        this.Count_3197 = AlignedBox2D_46_Type.Vector_11_Concept.Count_3197;
        this.At_3211 = AlignedBox2D_46_Type.Vector_11_Concept.At_3211;
        this.Count_3176 = AlignedBox2D_46_Type.Array_10_Concept.Count_3176;
        this.At_3181 = AlignedBox2D_46_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = AlignedBox2D_46_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AlignedBox2D_46_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AlignedBox2D_46_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AlignedBox2D_46_Type.Any_8_Concept.TypeOf_3164;
        this.Zero_3240 = AlignedBox2D_46_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = AlignedBox2D_46_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = AlignedBox2D_46_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = AlignedBox2D_46_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = AlignedBox2D_46_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AlignedBox2D_46_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AlignedBox2D_46_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AlignedBox2D_46_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AlignedBox2D_46_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = AlignedBox2D_46_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = AlignedBox2D_46_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = AlignedBox2D_46_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = AlignedBox2D_46_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = AlignedBox2D_46_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = AlignedBox2D_46_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = AlignedBox2D_46_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AlignedBox2D_46_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AlignedBox2D_46_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AlignedBox2D_46_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AlignedBox2D_46_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = AlignedBox2D_46_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = AlignedBox2D_46_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AlignedBox2D_46_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AlignedBox2D_46_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AlignedBox2D_46_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AlignedBox2D_46_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = AlignedBox2D_46_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = AlignedBox2D_46_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AlignedBox2D_46_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AlignedBox2D_46_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AlignedBox2D_46_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AlignedBox2D_46_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = AlignedBox2D_46_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = AlignedBox2D_46_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AlignedBox2D_46_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AlignedBox2D_46_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AlignedBox2D_46_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AlignedBox2D_46_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = AlignedBox2D_46_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = AlignedBox2D_46_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = AlignedBox2D_46_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = AlignedBox2D_46_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = AlignedBox2D_46_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = AlignedBox2D_46_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AlignedBox2D_46_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AlignedBox2D_46_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AlignedBox2D_46_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AlignedBox2D_46_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static A_861 = function(self) { return self.A_861; }
    static B_868 = function(self) { return self.B_868; }
    // implemented concepts 
    static Interval_20_Concept = new Interval_20_Concept(AlignedBox2D_46_Type);
    static Vector_11_Concept = new Vector_11_Concept(AlignedBox2D_46_Type);
    static Array_10_Concept = new Array_10_Concept(AlignedBox2D_46_Type);
    static Any_8_Concept = new Any_8_Concept(AlignedBox2D_46_Type);
    static Numerical_13_Concept = new Numerical_13_Concept(AlignedBox2D_46_Type);
    static Value_9_Concept = new Value_9_Concept(AlignedBox2D_46_Type);
    static Any_8_Concept = new Any_8_Concept(AlignedBox2D_46_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(AlignedBox2D_46_Type);
    static Value_9_Concept = new Value_9_Concept(AlignedBox2D_46_Type);
    static Any_8_Concept = new Any_8_Concept(AlignedBox2D_46_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(AlignedBox2D_46_Type);
    static Value_9_Concept = new Value_9_Concept(AlignedBox2D_46_Type);
    static Any_8_Concept = new Any_8_Concept(AlignedBox2D_46_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(AlignedBox2D_46_Type);
    static Value_9_Concept = new Value_9_Concept(AlignedBox2D_46_Type);
    static Any_8_Concept = new Any_8_Concept(AlignedBox2D_46_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(AlignedBox2D_46_Type);
    static Value_9_Concept = new Value_9_Concept(AlignedBox2D_46_Type);
    static Any_8_Concept = new Any_8_Concept(AlignedBox2D_46_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(AlignedBox2D_46_Type);
    static Value_9_Concept = new Value_9_Concept(AlignedBox2D_46_Type);
    static Any_8_Concept = new Any_8_Concept(AlignedBox2D_46_Type);
    static Implements = [Interval_20_Concept,Vector_11_Concept,Array_10_Concept,Any_8_Concept,Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept];
}
class AlignedBox3D_47_Type
{
    constructor(A_875, B_882)
    {
        // field initialization 
        this.A_875 = A_875;
        this.B_882 = B_882;
        this.Min_3514 = AlignedBox3D_47_Type.Interval_20_Concept.Min_3514;
        this.Max_3517 = AlignedBox3D_47_Type.Interval_20_Concept.Max_3517;
        this.Count_3197 = AlignedBox3D_47_Type.Vector_11_Concept.Count_3197;
        this.At_3211 = AlignedBox3D_47_Type.Vector_11_Concept.At_3211;
        this.Count_3176 = AlignedBox3D_47_Type.Array_10_Concept.Count_3176;
        this.At_3181 = AlignedBox3D_47_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = AlignedBox3D_47_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AlignedBox3D_47_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AlignedBox3D_47_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AlignedBox3D_47_Type.Any_8_Concept.TypeOf_3164;
        this.Zero_3240 = AlignedBox3D_47_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = AlignedBox3D_47_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = AlignedBox3D_47_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = AlignedBox3D_47_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = AlignedBox3D_47_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AlignedBox3D_47_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AlignedBox3D_47_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AlignedBox3D_47_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AlignedBox3D_47_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = AlignedBox3D_47_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = AlignedBox3D_47_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = AlignedBox3D_47_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = AlignedBox3D_47_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = AlignedBox3D_47_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = AlignedBox3D_47_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = AlignedBox3D_47_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AlignedBox3D_47_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AlignedBox3D_47_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AlignedBox3D_47_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AlignedBox3D_47_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = AlignedBox3D_47_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = AlignedBox3D_47_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AlignedBox3D_47_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AlignedBox3D_47_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AlignedBox3D_47_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AlignedBox3D_47_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = AlignedBox3D_47_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = AlignedBox3D_47_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AlignedBox3D_47_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AlignedBox3D_47_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AlignedBox3D_47_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AlignedBox3D_47_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = AlignedBox3D_47_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = AlignedBox3D_47_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AlignedBox3D_47_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AlignedBox3D_47_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AlignedBox3D_47_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AlignedBox3D_47_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = AlignedBox3D_47_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = AlignedBox3D_47_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = AlignedBox3D_47_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = AlignedBox3D_47_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = AlignedBox3D_47_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = AlignedBox3D_47_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AlignedBox3D_47_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AlignedBox3D_47_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AlignedBox3D_47_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AlignedBox3D_47_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static A_875 = function(self) { return self.A_875; }
    static B_882 = function(self) { return self.B_882; }
    // implemented concepts 
    static Interval_20_Concept = new Interval_20_Concept(AlignedBox3D_47_Type);
    static Vector_11_Concept = new Vector_11_Concept(AlignedBox3D_47_Type);
    static Array_10_Concept = new Array_10_Concept(AlignedBox3D_47_Type);
    static Any_8_Concept = new Any_8_Concept(AlignedBox3D_47_Type);
    static Numerical_13_Concept = new Numerical_13_Concept(AlignedBox3D_47_Type);
    static Value_9_Concept = new Value_9_Concept(AlignedBox3D_47_Type);
    static Any_8_Concept = new Any_8_Concept(AlignedBox3D_47_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(AlignedBox3D_47_Type);
    static Value_9_Concept = new Value_9_Concept(AlignedBox3D_47_Type);
    static Any_8_Concept = new Any_8_Concept(AlignedBox3D_47_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(AlignedBox3D_47_Type);
    static Value_9_Concept = new Value_9_Concept(AlignedBox3D_47_Type);
    static Any_8_Concept = new Any_8_Concept(AlignedBox3D_47_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(AlignedBox3D_47_Type);
    static Value_9_Concept = new Value_9_Concept(AlignedBox3D_47_Type);
    static Any_8_Concept = new Any_8_Concept(AlignedBox3D_47_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(AlignedBox3D_47_Type);
    static Value_9_Concept = new Value_9_Concept(AlignedBox3D_47_Type);
    static Any_8_Concept = new Any_8_Concept(AlignedBox3D_47_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(AlignedBox3D_47_Type);
    static Value_9_Concept = new Value_9_Concept(AlignedBox3D_47_Type);
    static Any_8_Concept = new Any_8_Concept(AlignedBox3D_47_Type);
    static Implements = [Interval_20_Concept,Vector_11_Concept,Array_10_Concept,Any_8_Concept,Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept];
}
class Complex_48_Type
{
    constructor(Real_889, Imaginary_896)
    {
        // field initialization 
        this.Real_889 = Real_889;
        this.Imaginary_896 = Imaginary_896;
        this.Count_3197 = Complex_48_Type.Vector_11_Concept.Count_3197;
        this.At_3211 = Complex_48_Type.Vector_11_Concept.At_3211;
        this.Count_3176 = Complex_48_Type.Array_10_Concept.Count_3176;
        this.At_3181 = Complex_48_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = Complex_48_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Complex_48_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Complex_48_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Complex_48_Type.Any_8_Concept.TypeOf_3164;
        this.Zero_3240 = Complex_48_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = Complex_48_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = Complex_48_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = Complex_48_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = Complex_48_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Complex_48_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Complex_48_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Complex_48_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Complex_48_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = Complex_48_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = Complex_48_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = Complex_48_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = Complex_48_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = Complex_48_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = Complex_48_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = Complex_48_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Complex_48_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Complex_48_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Complex_48_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Complex_48_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Complex_48_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Complex_48_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Complex_48_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Complex_48_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Complex_48_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Complex_48_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Complex_48_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Complex_48_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Complex_48_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Complex_48_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Complex_48_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Complex_48_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Complex_48_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Complex_48_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Complex_48_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Complex_48_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Complex_48_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Complex_48_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Complex_48_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Complex_48_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Complex_48_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Complex_48_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Complex_48_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Complex_48_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Complex_48_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Complex_48_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Complex_48_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Complex_48_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Real_889 = function(self) { return self.Real_889; }
    static Imaginary_896 = function(self) { return self.Imaginary_896; }
    // implemented concepts 
    static Vector_11_Concept = new Vector_11_Concept(Complex_48_Type);
    static Array_10_Concept = new Array_10_Concept(Complex_48_Type);
    static Any_8_Concept = new Any_8_Concept(Complex_48_Type);
    static Numerical_13_Concept = new Numerical_13_Concept(Complex_48_Type);
    static Value_9_Concept = new Value_9_Concept(Complex_48_Type);
    static Any_8_Concept = new Any_8_Concept(Complex_48_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(Complex_48_Type);
    static Value_9_Concept = new Value_9_Concept(Complex_48_Type);
    static Any_8_Concept = new Any_8_Concept(Complex_48_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Complex_48_Type);
    static Value_9_Concept = new Value_9_Concept(Complex_48_Type);
    static Any_8_Concept = new Any_8_Concept(Complex_48_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Complex_48_Type);
    static Value_9_Concept = new Value_9_Concept(Complex_48_Type);
    static Any_8_Concept = new Any_8_Concept(Complex_48_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Complex_48_Type);
    static Value_9_Concept = new Value_9_Concept(Complex_48_Type);
    static Any_8_Concept = new Any_8_Concept(Complex_48_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Complex_48_Type);
    static Value_9_Concept = new Value_9_Concept(Complex_48_Type);
    static Any_8_Concept = new Any_8_Concept(Complex_48_Type);
    static Implements = [Vector_11_Concept,Array_10_Concept,Any_8_Concept,Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept];
}
class Ray3D_49_Type
{
    constructor(Direction_903, Position_910)
    {
        // field initialization 
        this.Direction_903 = Direction_903;
        this.Position_910 = Position_910;
        this.Default_3170 = Ray3D_49_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Ray3D_49_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Ray3D_49_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Ray3D_49_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Ray3D_49_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Direction_903 = function(self) { return self.Direction_903; }
    static Position_910 = function(self) { return self.Position_910; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Ray3D_49_Type);
    static Any_8_Concept = new Any_8_Concept(Ray3D_49_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Ray2D_50_Type
{
    constructor(Direction_917, Position_924)
    {
        // field initialization 
        this.Direction_917 = Direction_917;
        this.Position_924 = Position_924;
        this.Default_3170 = Ray2D_50_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Ray2D_50_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Ray2D_50_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Ray2D_50_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Ray2D_50_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Direction_917 = function(self) { return self.Direction_917; }
    static Position_924 = function(self) { return self.Position_924; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Ray2D_50_Type);
    static Any_8_Concept = new Any_8_Concept(Ray2D_50_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Sphere_51_Type
{
    constructor(Center_931, Radius_938)
    {
        // field initialization 
        this.Center_931 = Center_931;
        this.Radius_938 = Radius_938;
        this.Default_3170 = Sphere_51_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Sphere_51_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Sphere_51_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Sphere_51_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Sphere_51_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Center_931 = function(self) { return self.Center_931; }
    static Radius_938 = function(self) { return self.Radius_938; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Sphere_51_Type);
    static Any_8_Concept = new Any_8_Concept(Sphere_51_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Plane_52_Type
{
    constructor(Normal_945, D_952)
    {
        // field initialization 
        this.Normal_945 = Normal_945;
        this.D_952 = D_952;
        this.Default_3170 = Plane_52_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Plane_52_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Plane_52_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Plane_52_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Plane_52_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Normal_945 = function(self) { return self.Normal_945; }
    static D_952 = function(self) { return self.D_952; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Plane_52_Type);
    static Any_8_Concept = new Any_8_Concept(Plane_52_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Triangle3D_53_Type
{
    constructor(A_959, B_966, C_973)
    {
        // field initialization 
        this.A_959 = A_959;
        this.B_966 = B_966;
        this.C_973 = C_973;
        this.Default_3170 = Triangle3D_53_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Triangle3D_53_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Triangle3D_53_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Triangle3D_53_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Triangle3D_53_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static A_959 = function(self) { return self.A_959; }
    static B_966 = function(self) { return self.B_966; }
    static C_973 = function(self) { return self.C_973; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Triangle3D_53_Type);
    static Any_8_Concept = new Any_8_Concept(Triangle3D_53_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Triangle2D_54_Type
{
    constructor(A_980, B_987, C_994)
    {
        // field initialization 
        this.A_980 = A_980;
        this.B_987 = B_987;
        this.C_994 = C_994;
        this.Default_3170 = Triangle2D_54_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Triangle2D_54_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Triangle2D_54_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Triangle2D_54_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Triangle2D_54_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static A_980 = function(self) { return self.A_980; }
    static B_987 = function(self) { return self.B_987; }
    static C_994 = function(self) { return self.C_994; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Triangle2D_54_Type);
    static Any_8_Concept = new Any_8_Concept(Triangle2D_54_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Quad3D_55_Type
{
    constructor(A_1001, B_1008, C_1015, D_1022)
    {
        // field initialization 
        this.A_1001 = A_1001;
        this.B_1008 = B_1008;
        this.C_1015 = C_1015;
        this.D_1022 = D_1022;
        this.Default_3170 = Quad3D_55_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Quad3D_55_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Quad3D_55_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Quad3D_55_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Quad3D_55_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static A_1001 = function(self) { return self.A_1001; }
    static B_1008 = function(self) { return self.B_1008; }
    static C_1015 = function(self) { return self.C_1015; }
    static D_1022 = function(self) { return self.D_1022; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Quad3D_55_Type);
    static Any_8_Concept = new Any_8_Concept(Quad3D_55_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Quad2D_56_Type
{
    constructor(A_1029, B_1036, C_1043, D_1050)
    {
        // field initialization 
        this.A_1029 = A_1029;
        this.B_1036 = B_1036;
        this.C_1043 = C_1043;
        this.D_1050 = D_1050;
        this.Default_3170 = Quad2D_56_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Quad2D_56_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Quad2D_56_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Quad2D_56_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Quad2D_56_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static A_1029 = function(self) { return self.A_1029; }
    static B_1036 = function(self) { return self.B_1036; }
    static C_1043 = function(self) { return self.C_1043; }
    static D_1050 = function(self) { return self.D_1050; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Quad2D_56_Type);
    static Any_8_Concept = new Any_8_Concept(Quad2D_56_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Point3D_57_Type
{
    constructor(Value_1057)
    {
        // field initialization 
        this.Value_1057 = Value_1057;
        this.Default_3170 = Point3D_57_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Point3D_57_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Point3D_57_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Point3D_57_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Point3D_57_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Value_1057 = function(self) { return self.Value_1057; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Point3D_57_Type);
    static Any_8_Concept = new Any_8_Concept(Point3D_57_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Point2D_58_Type
{
    constructor(Value_1064)
    {
        // field initialization 
        this.Value_1064 = Value_1064;
        this.Default_3170 = Point2D_58_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Point2D_58_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Point2D_58_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Point2D_58_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Point2D_58_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Value_1064 = function(self) { return self.Value_1064; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Point2D_58_Type);
    static Any_8_Concept = new Any_8_Concept(Point2D_58_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Line3D_59_Type
{
    constructor(A_1071, B_1078)
    {
        // field initialization 
        this.A_1071 = A_1071;
        this.B_1078 = B_1078;
        this.Min_3514 = Line3D_59_Type.Interval_20_Concept.Min_3514;
        this.Max_3517 = Line3D_59_Type.Interval_20_Concept.Max_3517;
        this.Count_3197 = Line3D_59_Type.Vector_11_Concept.Count_3197;
        this.At_3211 = Line3D_59_Type.Vector_11_Concept.At_3211;
        this.Count_3176 = Line3D_59_Type.Array_10_Concept.Count_3176;
        this.At_3181 = Line3D_59_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = Line3D_59_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Line3D_59_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Line3D_59_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Line3D_59_Type.Any_8_Concept.TypeOf_3164;
        this.Zero_3240 = Line3D_59_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = Line3D_59_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = Line3D_59_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = Line3D_59_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = Line3D_59_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Line3D_59_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Line3D_59_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Line3D_59_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Line3D_59_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = Line3D_59_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = Line3D_59_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = Line3D_59_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = Line3D_59_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = Line3D_59_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = Line3D_59_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = Line3D_59_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Line3D_59_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Line3D_59_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Line3D_59_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Line3D_59_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Line3D_59_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Line3D_59_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Line3D_59_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Line3D_59_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Line3D_59_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Line3D_59_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Line3D_59_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Line3D_59_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Line3D_59_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Line3D_59_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Line3D_59_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Line3D_59_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Line3D_59_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Line3D_59_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Line3D_59_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Line3D_59_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Line3D_59_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Line3D_59_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Line3D_59_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Line3D_59_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Line3D_59_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Line3D_59_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Line3D_59_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Line3D_59_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Line3D_59_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Line3D_59_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Line3D_59_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Line3D_59_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static A_1071 = function(self) { return self.A_1071; }
    static B_1078 = function(self) { return self.B_1078; }
    // implemented concepts 
    static Interval_20_Concept = new Interval_20_Concept(Line3D_59_Type);
    static Vector_11_Concept = new Vector_11_Concept(Line3D_59_Type);
    static Array_10_Concept = new Array_10_Concept(Line3D_59_Type);
    static Any_8_Concept = new Any_8_Concept(Line3D_59_Type);
    static Numerical_13_Concept = new Numerical_13_Concept(Line3D_59_Type);
    static Value_9_Concept = new Value_9_Concept(Line3D_59_Type);
    static Any_8_Concept = new Any_8_Concept(Line3D_59_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(Line3D_59_Type);
    static Value_9_Concept = new Value_9_Concept(Line3D_59_Type);
    static Any_8_Concept = new Any_8_Concept(Line3D_59_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Line3D_59_Type);
    static Value_9_Concept = new Value_9_Concept(Line3D_59_Type);
    static Any_8_Concept = new Any_8_Concept(Line3D_59_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Line3D_59_Type);
    static Value_9_Concept = new Value_9_Concept(Line3D_59_Type);
    static Any_8_Concept = new Any_8_Concept(Line3D_59_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Line3D_59_Type);
    static Value_9_Concept = new Value_9_Concept(Line3D_59_Type);
    static Any_8_Concept = new Any_8_Concept(Line3D_59_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Line3D_59_Type);
    static Value_9_Concept = new Value_9_Concept(Line3D_59_Type);
    static Any_8_Concept = new Any_8_Concept(Line3D_59_Type);
    static Implements = [Interval_20_Concept,Vector_11_Concept,Array_10_Concept,Any_8_Concept,Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept];
}
class Line2D_60_Type
{
    constructor(A_1085, B_1092)
    {
        // field initialization 
        this.A_1085 = A_1085;
        this.B_1092 = B_1092;
        this.Min_3514 = Line2D_60_Type.Interval_20_Concept.Min_3514;
        this.Max_3517 = Line2D_60_Type.Interval_20_Concept.Max_3517;
        this.Count_3197 = Line2D_60_Type.Vector_11_Concept.Count_3197;
        this.At_3211 = Line2D_60_Type.Vector_11_Concept.At_3211;
        this.Count_3176 = Line2D_60_Type.Array_10_Concept.Count_3176;
        this.At_3181 = Line2D_60_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = Line2D_60_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Line2D_60_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Line2D_60_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Line2D_60_Type.Any_8_Concept.TypeOf_3164;
        this.Zero_3240 = Line2D_60_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = Line2D_60_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = Line2D_60_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = Line2D_60_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = Line2D_60_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Line2D_60_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Line2D_60_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Line2D_60_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Line2D_60_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = Line2D_60_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = Line2D_60_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = Line2D_60_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = Line2D_60_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = Line2D_60_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = Line2D_60_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = Line2D_60_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Line2D_60_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Line2D_60_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Line2D_60_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Line2D_60_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Line2D_60_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Line2D_60_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Line2D_60_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Line2D_60_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Line2D_60_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Line2D_60_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Line2D_60_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Line2D_60_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Line2D_60_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Line2D_60_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Line2D_60_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Line2D_60_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Line2D_60_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Line2D_60_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Line2D_60_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Line2D_60_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Line2D_60_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Line2D_60_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Line2D_60_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Line2D_60_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Line2D_60_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Line2D_60_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Line2D_60_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Line2D_60_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Line2D_60_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Line2D_60_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Line2D_60_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Line2D_60_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static A_1085 = function(self) { return self.A_1085; }
    static B_1092 = function(self) { return self.B_1092; }
    // implemented concepts 
    static Interval_20_Concept = new Interval_20_Concept(Line2D_60_Type);
    static Vector_11_Concept = new Vector_11_Concept(Line2D_60_Type);
    static Array_10_Concept = new Array_10_Concept(Line2D_60_Type);
    static Any_8_Concept = new Any_8_Concept(Line2D_60_Type);
    static Numerical_13_Concept = new Numerical_13_Concept(Line2D_60_Type);
    static Value_9_Concept = new Value_9_Concept(Line2D_60_Type);
    static Any_8_Concept = new Any_8_Concept(Line2D_60_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(Line2D_60_Type);
    static Value_9_Concept = new Value_9_Concept(Line2D_60_Type);
    static Any_8_Concept = new Any_8_Concept(Line2D_60_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Line2D_60_Type);
    static Value_9_Concept = new Value_9_Concept(Line2D_60_Type);
    static Any_8_Concept = new Any_8_Concept(Line2D_60_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Line2D_60_Type);
    static Value_9_Concept = new Value_9_Concept(Line2D_60_Type);
    static Any_8_Concept = new Any_8_Concept(Line2D_60_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Line2D_60_Type);
    static Value_9_Concept = new Value_9_Concept(Line2D_60_Type);
    static Any_8_Concept = new Any_8_Concept(Line2D_60_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Line2D_60_Type);
    static Value_9_Concept = new Value_9_Concept(Line2D_60_Type);
    static Any_8_Concept = new Any_8_Concept(Line2D_60_Type);
    static Implements = [Interval_20_Concept,Vector_11_Concept,Array_10_Concept,Any_8_Concept,Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept];
}
class Color_61_Type
{
    constructor(R_1099, G_1106, B_1113, A_1120)
    {
        // field initialization 
        this.R_1099 = R_1099;
        this.G_1106 = G_1106;
        this.B_1113 = B_1113;
        this.A_1120 = A_1120;
        this.Default_3170 = Color_61_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Color_61_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Color_61_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Color_61_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Color_61_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static R_1099 = function(self) { return self.R_1099; }
    static G_1106 = function(self) { return self.G_1106; }
    static B_1113 = function(self) { return self.B_1113; }
    static A_1120 = function(self) { return self.A_1120; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Color_61_Type);
    static Any_8_Concept = new Any_8_Concept(Color_61_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class ColorLUV_62_Type
{
    constructor(Lightness_1127, U_1134, V_1141)
    {
        // field initialization 
        this.Lightness_1127 = Lightness_1127;
        this.U_1134 = U_1134;
        this.V_1141 = V_1141;
        this.Default_3170 = ColorLUV_62_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ColorLUV_62_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ColorLUV_62_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ColorLUV_62_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ColorLUV_62_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Lightness_1127 = function(self) { return self.Lightness_1127; }
    static U_1134 = function(self) { return self.U_1134; }
    static V_1141 = function(self) { return self.V_1141; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(ColorLUV_62_Type);
    static Any_8_Concept = new Any_8_Concept(ColorLUV_62_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class ColorLAB_63_Type
{
    constructor(Lightness_1148, A_1155, B_1162)
    {
        // field initialization 
        this.Lightness_1148 = Lightness_1148;
        this.A_1155 = A_1155;
        this.B_1162 = B_1162;
        this.Default_3170 = ColorLAB_63_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ColorLAB_63_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ColorLAB_63_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ColorLAB_63_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ColorLAB_63_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Lightness_1148 = function(self) { return self.Lightness_1148; }
    static A_1155 = function(self) { return self.A_1155; }
    static B_1162 = function(self) { return self.B_1162; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(ColorLAB_63_Type);
    static Any_8_Concept = new Any_8_Concept(ColorLAB_63_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class ColorLCh_64_Type
{
    constructor(Lightness_1169, ChromaHue_1176)
    {
        // field initialization 
        this.Lightness_1169 = Lightness_1169;
        this.ChromaHue_1176 = ChromaHue_1176;
        this.Default_3170 = ColorLCh_64_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ColorLCh_64_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ColorLCh_64_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ColorLCh_64_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ColorLCh_64_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Lightness_1169 = function(self) { return self.Lightness_1169; }
    static ChromaHue_1176 = function(self) { return self.ChromaHue_1176; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(ColorLCh_64_Type);
    static Any_8_Concept = new Any_8_Concept(ColorLCh_64_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class ColorHSV_65_Type
{
    constructor(Hue_1183, S_1190, V_1197)
    {
        // field initialization 
        this.Hue_1183 = Hue_1183;
        this.S_1190 = S_1190;
        this.V_1197 = V_1197;
        this.Default_3170 = ColorHSV_65_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ColorHSV_65_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ColorHSV_65_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ColorHSV_65_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ColorHSV_65_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Hue_1183 = function(self) { return self.Hue_1183; }
    static S_1190 = function(self) { return self.S_1190; }
    static V_1197 = function(self) { return self.V_1197; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(ColorHSV_65_Type);
    static Any_8_Concept = new Any_8_Concept(ColorHSV_65_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class ColorHSL_66_Type
{
    constructor(Hue_1204, Saturation_1211, Luminance_1218)
    {
        // field initialization 
        this.Hue_1204 = Hue_1204;
        this.Saturation_1211 = Saturation_1211;
        this.Luminance_1218 = Luminance_1218;
        this.Default_3170 = ColorHSL_66_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ColorHSL_66_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ColorHSL_66_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ColorHSL_66_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ColorHSL_66_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Hue_1204 = function(self) { return self.Hue_1204; }
    static Saturation_1211 = function(self) { return self.Saturation_1211; }
    static Luminance_1218 = function(self) { return self.Luminance_1218; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(ColorHSL_66_Type);
    static Any_8_Concept = new Any_8_Concept(ColorHSL_66_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class ColorYCbCr_67_Type
{
    constructor(Y_1225, Cb_1232, Cr_1239)
    {
        // field initialization 
        this.Y_1225 = Y_1225;
        this.Cb_1232 = Cb_1232;
        this.Cr_1239 = Cr_1239;
        this.Default_3170 = ColorYCbCr_67_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ColorYCbCr_67_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ColorYCbCr_67_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ColorYCbCr_67_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ColorYCbCr_67_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Y_1225 = function(self) { return self.Y_1225; }
    static Cb_1232 = function(self) { return self.Cb_1232; }
    static Cr_1239 = function(self) { return self.Cr_1239; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(ColorYCbCr_67_Type);
    static Any_8_Concept = new Any_8_Concept(ColorYCbCr_67_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class SphericalCoordinate_68_Type
{
    constructor(Radius_1246, Azimuth_1253, Polar_1260)
    {
        // field initialization 
        this.Radius_1246 = Radius_1246;
        this.Azimuth_1253 = Azimuth_1253;
        this.Polar_1260 = Polar_1260;
        this.Default_3170 = SphericalCoordinate_68_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = SphericalCoordinate_68_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = SphericalCoordinate_68_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = SphericalCoordinate_68_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = SphericalCoordinate_68_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Radius_1246 = function(self) { return self.Radius_1246; }
    static Azimuth_1253 = function(self) { return self.Azimuth_1253; }
    static Polar_1260 = function(self) { return self.Polar_1260; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(SphericalCoordinate_68_Type);
    static Any_8_Concept = new Any_8_Concept(SphericalCoordinate_68_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class PolarCoordinate_69_Type
{
    constructor(Radius_1267, Angle_1274)
    {
        // field initialization 
        this.Radius_1267 = Radius_1267;
        this.Angle_1274 = Angle_1274;
        this.Default_3170 = PolarCoordinate_69_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = PolarCoordinate_69_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = PolarCoordinate_69_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = PolarCoordinate_69_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = PolarCoordinate_69_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Radius_1267 = function(self) { return self.Radius_1267; }
    static Angle_1274 = function(self) { return self.Angle_1274; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(PolarCoordinate_69_Type);
    static Any_8_Concept = new Any_8_Concept(PolarCoordinate_69_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class LogPolarCoordinate_70_Type
{
    constructor(Rho_1281, Azimuth_1288)
    {
        // field initialization 
        this.Rho_1281 = Rho_1281;
        this.Azimuth_1288 = Azimuth_1288;
        this.Default_3170 = LogPolarCoordinate_70_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = LogPolarCoordinate_70_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = LogPolarCoordinate_70_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = LogPolarCoordinate_70_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = LogPolarCoordinate_70_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Rho_1281 = function(self) { return self.Rho_1281; }
    static Azimuth_1288 = function(self) { return self.Azimuth_1288; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(LogPolarCoordinate_70_Type);
    static Any_8_Concept = new Any_8_Concept(LogPolarCoordinate_70_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class CylindricalCoordinate_71_Type
{
    constructor(RadialDistance_1295, Azimuth_1302, Height_1309)
    {
        // field initialization 
        this.RadialDistance_1295 = RadialDistance_1295;
        this.Azimuth_1302 = Azimuth_1302;
        this.Height_1309 = Height_1309;
        this.Default_3170 = CylindricalCoordinate_71_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = CylindricalCoordinate_71_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = CylindricalCoordinate_71_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = CylindricalCoordinate_71_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = CylindricalCoordinate_71_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static RadialDistance_1295 = function(self) { return self.RadialDistance_1295; }
    static Azimuth_1302 = function(self) { return self.Azimuth_1302; }
    static Height_1309 = function(self) { return self.Height_1309; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(CylindricalCoordinate_71_Type);
    static Any_8_Concept = new Any_8_Concept(CylindricalCoordinate_71_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class HorizontalCoordinate_72_Type
{
    constructor(Radius_1316, Azimuth_1323, Height_1330)
    {
        // field initialization 
        this.Radius_1316 = Radius_1316;
        this.Azimuth_1323 = Azimuth_1323;
        this.Height_1330 = Height_1330;
        this.Default_3170 = HorizontalCoordinate_72_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = HorizontalCoordinate_72_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = HorizontalCoordinate_72_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = HorizontalCoordinate_72_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = HorizontalCoordinate_72_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Radius_1316 = function(self) { return self.Radius_1316; }
    static Azimuth_1323 = function(self) { return self.Azimuth_1323; }
    static Height_1330 = function(self) { return self.Height_1330; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(HorizontalCoordinate_72_Type);
    static Any_8_Concept = new Any_8_Concept(HorizontalCoordinate_72_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class GeoCoordinate_73_Type
{
    constructor(Latitude_1337, Longitude_1344)
    {
        // field initialization 
        this.Latitude_1337 = Latitude_1337;
        this.Longitude_1344 = Longitude_1344;
        this.Default_3170 = GeoCoordinate_73_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = GeoCoordinate_73_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = GeoCoordinate_73_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = GeoCoordinate_73_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = GeoCoordinate_73_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Latitude_1337 = function(self) { return self.Latitude_1337; }
    static Longitude_1344 = function(self) { return self.Longitude_1344; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(GeoCoordinate_73_Type);
    static Any_8_Concept = new Any_8_Concept(GeoCoordinate_73_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class GeoCoordinateWithAltitude_74_Type
{
    constructor(Coordinate_1351, Altitude_1358)
    {
        // field initialization 
        this.Coordinate_1351 = Coordinate_1351;
        this.Altitude_1358 = Altitude_1358;
        this.Default_3170 = GeoCoordinateWithAltitude_74_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = GeoCoordinateWithAltitude_74_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = GeoCoordinateWithAltitude_74_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = GeoCoordinateWithAltitude_74_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = GeoCoordinateWithAltitude_74_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Coordinate_1351 = function(self) { return self.Coordinate_1351; }
    static Altitude_1358 = function(self) { return self.Altitude_1358; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(GeoCoordinateWithAltitude_74_Type);
    static Any_8_Concept = new Any_8_Concept(GeoCoordinateWithAltitude_74_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Circle_75_Type
{
    constructor(Center_1365, Radius_1372)
    {
        // field initialization 
        this.Center_1365 = Center_1365;
        this.Radius_1372 = Radius_1372;
        this.Default_3170 = Circle_75_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Circle_75_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Circle_75_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Circle_75_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Circle_75_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Center_1365 = function(self) { return self.Center_1365; }
    static Radius_1372 = function(self) { return self.Radius_1372; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Circle_75_Type);
    static Any_8_Concept = new Any_8_Concept(Circle_75_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Chord_76_Type
{
    constructor(Circle_1379, Arc_1386)
    {
        // field initialization 
        this.Circle_1379 = Circle_1379;
        this.Arc_1386 = Arc_1386;
        this.Default_3170 = Chord_76_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Chord_76_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Chord_76_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Chord_76_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Chord_76_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Circle_1379 = function(self) { return self.Circle_1379; }
    static Arc_1386 = function(self) { return self.Arc_1386; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Chord_76_Type);
    static Any_8_Concept = new Any_8_Concept(Chord_76_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Size2D_77_Type
{
    constructor(Width_1393, Height_1400)
    {
        // field initialization 
        this.Width_1393 = Width_1393;
        this.Height_1400 = Height_1400;
        this.Default_3170 = Size2D_77_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Size2D_77_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Size2D_77_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Size2D_77_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Size2D_77_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Width_1393 = function(self) { return self.Width_1393; }
    static Height_1400 = function(self) { return self.Height_1400; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Size2D_77_Type);
    static Any_8_Concept = new Any_8_Concept(Size2D_77_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Size3D_78_Type
{
    constructor(Width_1407, Height_1414, Depth_1421)
    {
        // field initialization 
        this.Width_1407 = Width_1407;
        this.Height_1414 = Height_1414;
        this.Depth_1421 = Depth_1421;
        this.Default_3170 = Size3D_78_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Size3D_78_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Size3D_78_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Size3D_78_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Size3D_78_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Width_1407 = function(self) { return self.Width_1407; }
    static Height_1414 = function(self) { return self.Height_1414; }
    static Depth_1421 = function(self) { return self.Depth_1421; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Size3D_78_Type);
    static Any_8_Concept = new Any_8_Concept(Size3D_78_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Rectangle2D_79_Type
{
    constructor(Center_1428, Size_1435)
    {
        // field initialization 
        this.Center_1428 = Center_1428;
        this.Size_1435 = Size_1435;
        this.Default_3170 = Rectangle2D_79_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Rectangle2D_79_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Rectangle2D_79_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Rectangle2D_79_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Rectangle2D_79_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Center_1428 = function(self) { return self.Center_1428; }
    static Size_1435 = function(self) { return self.Size_1435; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Rectangle2D_79_Type);
    static Any_8_Concept = new Any_8_Concept(Rectangle2D_79_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Proportion_80_Type
{
    constructor(Value_1442)
    {
        // field initialization 
        this.Value_1442 = Value_1442;
        this.Zero_3240 = Proportion_80_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = Proportion_80_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = Proportion_80_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = Proportion_80_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = Proportion_80_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Proportion_80_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Proportion_80_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Proportion_80_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Proportion_80_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = Proportion_80_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = Proportion_80_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = Proportion_80_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = Proportion_80_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = Proportion_80_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = Proportion_80_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = Proportion_80_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Proportion_80_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Proportion_80_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Proportion_80_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Proportion_80_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Proportion_80_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Proportion_80_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Proportion_80_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Proportion_80_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Proportion_80_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Proportion_80_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Proportion_80_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Proportion_80_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Proportion_80_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Proportion_80_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Proportion_80_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Proportion_80_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Proportion_80_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Proportion_80_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Proportion_80_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Proportion_80_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Proportion_80_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Proportion_80_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Value_1442 = function(self) { return self.Value_1442; }
    // implemented concepts 
    static Numerical_13_Concept = new Numerical_13_Concept(Proportion_80_Type);
    static Value_9_Concept = new Value_9_Concept(Proportion_80_Type);
    static Any_8_Concept = new Any_8_Concept(Proportion_80_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(Proportion_80_Type);
    static Value_9_Concept = new Value_9_Concept(Proportion_80_Type);
    static Any_8_Concept = new Any_8_Concept(Proportion_80_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Proportion_80_Type);
    static Value_9_Concept = new Value_9_Concept(Proportion_80_Type);
    static Any_8_Concept = new Any_8_Concept(Proportion_80_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Proportion_80_Type);
    static Value_9_Concept = new Value_9_Concept(Proportion_80_Type);
    static Any_8_Concept = new Any_8_Concept(Proportion_80_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Proportion_80_Type);
    static Value_9_Concept = new Value_9_Concept(Proportion_80_Type);
    static Any_8_Concept = new Any_8_Concept(Proportion_80_Type);
    static Implements = [Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Fraction_81_Type
{
    constructor(Numerator_1449, Denominator_1456)
    {
        // field initialization 
        this.Numerator_1449 = Numerator_1449;
        this.Denominator_1456 = Denominator_1456;
        this.Default_3170 = Fraction_81_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Fraction_81_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Fraction_81_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Fraction_81_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Fraction_81_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Numerator_1449 = function(self) { return self.Numerator_1449; }
    static Denominator_1456 = function(self) { return self.Denominator_1456; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Fraction_81_Type);
    static Any_8_Concept = new Any_8_Concept(Fraction_81_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Angle_82_Type
{
    constructor(Radians_1463)
    {
        // field initialization 
        this.Radians_1463 = Radians_1463;
        this.Value_3230 = Angle_82_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = Angle_82_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Angle_82_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Angle_82_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Angle_82_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Angle_82_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Angle_82_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Angle_82_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Angle_82_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Angle_82_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Angle_82_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Angle_82_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Angle_82_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Angle_82_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Angle_82_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Angle_82_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Angle_82_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Angle_82_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Angle_82_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Angle_82_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Angle_82_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Angle_82_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Angle_82_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Angle_82_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Angle_82_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Angle_82_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Angle_82_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Angle_82_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Angle_82_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Angle_82_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Angle_82_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Angle_82_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Angle_82_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Angle_82_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Radians_1463 = function(self) { return self.Radians_1463; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(Angle_82_Type);
    static Value_9_Concept = new Value_9_Concept(Angle_82_Type);
    static Any_8_Concept = new Any_8_Concept(Angle_82_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Angle_82_Type);
    static Value_9_Concept = new Value_9_Concept(Angle_82_Type);
    static Any_8_Concept = new Any_8_Concept(Angle_82_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Angle_82_Type);
    static Value_9_Concept = new Value_9_Concept(Angle_82_Type);
    static Any_8_Concept = new Any_8_Concept(Angle_82_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Angle_82_Type);
    static Value_9_Concept = new Value_9_Concept(Angle_82_Type);
    static Any_8_Concept = new Any_8_Concept(Angle_82_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Angle_82_Type);
    static Value_9_Concept = new Value_9_Concept(Angle_82_Type);
    static Any_8_Concept = new Any_8_Concept(Angle_82_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Length_83_Type
{
    constructor(Meters_1470)
    {
        // field initialization 
        this.Meters_1470 = Meters_1470;
        this.Value_3230 = Length_83_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = Length_83_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Length_83_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Length_83_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Length_83_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Length_83_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Length_83_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Length_83_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Length_83_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Length_83_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Length_83_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Length_83_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Length_83_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Length_83_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Length_83_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Length_83_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Length_83_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Length_83_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Length_83_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Length_83_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Length_83_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Length_83_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Length_83_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Length_83_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Length_83_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Length_83_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Length_83_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Length_83_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Length_83_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Length_83_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Length_83_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Length_83_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Length_83_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Length_83_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Meters_1470 = function(self) { return self.Meters_1470; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(Length_83_Type);
    static Value_9_Concept = new Value_9_Concept(Length_83_Type);
    static Any_8_Concept = new Any_8_Concept(Length_83_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Length_83_Type);
    static Value_9_Concept = new Value_9_Concept(Length_83_Type);
    static Any_8_Concept = new Any_8_Concept(Length_83_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Length_83_Type);
    static Value_9_Concept = new Value_9_Concept(Length_83_Type);
    static Any_8_Concept = new Any_8_Concept(Length_83_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Length_83_Type);
    static Value_9_Concept = new Value_9_Concept(Length_83_Type);
    static Any_8_Concept = new Any_8_Concept(Length_83_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Length_83_Type);
    static Value_9_Concept = new Value_9_Concept(Length_83_Type);
    static Any_8_Concept = new Any_8_Concept(Length_83_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Mass_84_Type
{
    constructor(Kilograms_1477)
    {
        // field initialization 
        this.Kilograms_1477 = Kilograms_1477;
        this.Value_3230 = Mass_84_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = Mass_84_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Mass_84_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Mass_84_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Mass_84_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Mass_84_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Mass_84_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Mass_84_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Mass_84_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Mass_84_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Mass_84_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Mass_84_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Mass_84_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Mass_84_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Mass_84_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Mass_84_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Mass_84_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Mass_84_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Mass_84_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Mass_84_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Mass_84_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Mass_84_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Mass_84_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Mass_84_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Mass_84_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Mass_84_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Mass_84_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Mass_84_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Mass_84_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Mass_84_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Mass_84_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Mass_84_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Mass_84_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Mass_84_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Kilograms_1477 = function(self) { return self.Kilograms_1477; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(Mass_84_Type);
    static Value_9_Concept = new Value_9_Concept(Mass_84_Type);
    static Any_8_Concept = new Any_8_Concept(Mass_84_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Mass_84_Type);
    static Value_9_Concept = new Value_9_Concept(Mass_84_Type);
    static Any_8_Concept = new Any_8_Concept(Mass_84_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Mass_84_Type);
    static Value_9_Concept = new Value_9_Concept(Mass_84_Type);
    static Any_8_Concept = new Any_8_Concept(Mass_84_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Mass_84_Type);
    static Value_9_Concept = new Value_9_Concept(Mass_84_Type);
    static Any_8_Concept = new Any_8_Concept(Mass_84_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Mass_84_Type);
    static Value_9_Concept = new Value_9_Concept(Mass_84_Type);
    static Any_8_Concept = new Any_8_Concept(Mass_84_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Temperature_85_Type
{
    constructor(Celsius_1484)
    {
        // field initialization 
        this.Celsius_1484 = Celsius_1484;
        this.Value_3230 = Temperature_85_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = Temperature_85_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Temperature_85_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Temperature_85_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Temperature_85_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Temperature_85_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Temperature_85_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Temperature_85_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Temperature_85_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Temperature_85_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Temperature_85_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Temperature_85_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Temperature_85_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Temperature_85_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Temperature_85_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Temperature_85_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Temperature_85_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Temperature_85_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Temperature_85_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Temperature_85_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Temperature_85_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Temperature_85_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Temperature_85_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Temperature_85_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Temperature_85_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Temperature_85_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Temperature_85_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Temperature_85_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Temperature_85_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Temperature_85_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Temperature_85_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Temperature_85_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Temperature_85_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Temperature_85_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Celsius_1484 = function(self) { return self.Celsius_1484; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(Temperature_85_Type);
    static Value_9_Concept = new Value_9_Concept(Temperature_85_Type);
    static Any_8_Concept = new Any_8_Concept(Temperature_85_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Temperature_85_Type);
    static Value_9_Concept = new Value_9_Concept(Temperature_85_Type);
    static Any_8_Concept = new Any_8_Concept(Temperature_85_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Temperature_85_Type);
    static Value_9_Concept = new Value_9_Concept(Temperature_85_Type);
    static Any_8_Concept = new Any_8_Concept(Temperature_85_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Temperature_85_Type);
    static Value_9_Concept = new Value_9_Concept(Temperature_85_Type);
    static Any_8_Concept = new Any_8_Concept(Temperature_85_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Temperature_85_Type);
    static Value_9_Concept = new Value_9_Concept(Temperature_85_Type);
    static Any_8_Concept = new Any_8_Concept(Temperature_85_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class TimeSpan_86_Type
{
    constructor(Seconds_1491)
    {
        // field initialization 
        this.Seconds_1491 = Seconds_1491;
        this.Value_3230 = TimeSpan_86_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = TimeSpan_86_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeSpan_86_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeSpan_86_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeSpan_86_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeSpan_86_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = TimeSpan_86_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = TimeSpan_86_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = TimeSpan_86_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = TimeSpan_86_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = TimeSpan_86_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = TimeSpan_86_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeSpan_86_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeSpan_86_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeSpan_86_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeSpan_86_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = TimeSpan_86_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = TimeSpan_86_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeSpan_86_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeSpan_86_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeSpan_86_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeSpan_86_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = TimeSpan_86_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = TimeSpan_86_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeSpan_86_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeSpan_86_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeSpan_86_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeSpan_86_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = TimeSpan_86_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = TimeSpan_86_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeSpan_86_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeSpan_86_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeSpan_86_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeSpan_86_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Seconds_1491 = function(self) { return self.Seconds_1491; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(TimeSpan_86_Type);
    static Value_9_Concept = new Value_9_Concept(TimeSpan_86_Type);
    static Any_8_Concept = new Any_8_Concept(TimeSpan_86_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(TimeSpan_86_Type);
    static Value_9_Concept = new Value_9_Concept(TimeSpan_86_Type);
    static Any_8_Concept = new Any_8_Concept(TimeSpan_86_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(TimeSpan_86_Type);
    static Value_9_Concept = new Value_9_Concept(TimeSpan_86_Type);
    static Any_8_Concept = new Any_8_Concept(TimeSpan_86_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(TimeSpan_86_Type);
    static Value_9_Concept = new Value_9_Concept(TimeSpan_86_Type);
    static Any_8_Concept = new Any_8_Concept(TimeSpan_86_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(TimeSpan_86_Type);
    static Value_9_Concept = new Value_9_Concept(TimeSpan_86_Type);
    static Any_8_Concept = new Any_8_Concept(TimeSpan_86_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class TimeRange_87_Type
{
    constructor(Min_1498, Max_1505)
    {
        // field initialization 
        this.Min_1498 = Min_1498;
        this.Max_1505 = Max_1505;
        this.Min_3514 = TimeRange_87_Type.Interval_20_Concept.Min_3514;
        this.Max_3517 = TimeRange_87_Type.Interval_20_Concept.Max_3517;
        this.Count_3197 = TimeRange_87_Type.Vector_11_Concept.Count_3197;
        this.At_3211 = TimeRange_87_Type.Vector_11_Concept.At_3211;
        this.Count_3176 = TimeRange_87_Type.Array_10_Concept.Count_3176;
        this.At_3181 = TimeRange_87_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = TimeRange_87_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeRange_87_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeRange_87_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeRange_87_Type.Any_8_Concept.TypeOf_3164;
        this.Zero_3240 = TimeRange_87_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = TimeRange_87_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = TimeRange_87_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = TimeRange_87_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = TimeRange_87_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeRange_87_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeRange_87_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeRange_87_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeRange_87_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = TimeRange_87_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = TimeRange_87_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = TimeRange_87_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = TimeRange_87_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = TimeRange_87_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = TimeRange_87_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = TimeRange_87_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeRange_87_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeRange_87_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeRange_87_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeRange_87_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = TimeRange_87_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = TimeRange_87_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeRange_87_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeRange_87_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeRange_87_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeRange_87_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = TimeRange_87_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = TimeRange_87_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeRange_87_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeRange_87_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeRange_87_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeRange_87_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = TimeRange_87_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = TimeRange_87_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeRange_87_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeRange_87_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeRange_87_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeRange_87_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = TimeRange_87_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = TimeRange_87_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = TimeRange_87_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = TimeRange_87_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = TimeRange_87_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = TimeRange_87_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeRange_87_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeRange_87_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeRange_87_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeRange_87_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Min_1498 = function(self) { return self.Min_1498; }
    static Max_1505 = function(self) { return self.Max_1505; }
    // implemented concepts 
    static Interval_20_Concept = new Interval_20_Concept(TimeRange_87_Type);
    static Vector_11_Concept = new Vector_11_Concept(TimeRange_87_Type);
    static Array_10_Concept = new Array_10_Concept(TimeRange_87_Type);
    static Any_8_Concept = new Any_8_Concept(TimeRange_87_Type);
    static Numerical_13_Concept = new Numerical_13_Concept(TimeRange_87_Type);
    static Value_9_Concept = new Value_9_Concept(TimeRange_87_Type);
    static Any_8_Concept = new Any_8_Concept(TimeRange_87_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(TimeRange_87_Type);
    static Value_9_Concept = new Value_9_Concept(TimeRange_87_Type);
    static Any_8_Concept = new Any_8_Concept(TimeRange_87_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(TimeRange_87_Type);
    static Value_9_Concept = new Value_9_Concept(TimeRange_87_Type);
    static Any_8_Concept = new Any_8_Concept(TimeRange_87_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(TimeRange_87_Type);
    static Value_9_Concept = new Value_9_Concept(TimeRange_87_Type);
    static Any_8_Concept = new Any_8_Concept(TimeRange_87_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(TimeRange_87_Type);
    static Value_9_Concept = new Value_9_Concept(TimeRange_87_Type);
    static Any_8_Concept = new Any_8_Concept(TimeRange_87_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(TimeRange_87_Type);
    static Value_9_Concept = new Value_9_Concept(TimeRange_87_Type);
    static Any_8_Concept = new Any_8_Concept(TimeRange_87_Type);
    static Implements = [Interval_20_Concept,Vector_11_Concept,Array_10_Concept,Any_8_Concept,Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept];
}
class DateTime_88_Type
{
    constructor()
    {
        // field initialization 
        this.Default_3170 = DateTime_88_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = DateTime_88_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = DateTime_88_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = DateTime_88_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = DateTime_88_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(DateTime_88_Type);
    static Any_8_Concept = new Any_8_Concept(DateTime_88_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class AnglePair_89_Type
{
    constructor(Start_1512, End_1519)
    {
        // field initialization 
        this.Start_1512 = Start_1512;
        this.End_1519 = End_1519;
        this.Min_3514 = AnglePair_89_Type.Interval_20_Concept.Min_3514;
        this.Max_3517 = AnglePair_89_Type.Interval_20_Concept.Max_3517;
        this.Count_3197 = AnglePair_89_Type.Vector_11_Concept.Count_3197;
        this.At_3211 = AnglePair_89_Type.Vector_11_Concept.At_3211;
        this.Count_3176 = AnglePair_89_Type.Array_10_Concept.Count_3176;
        this.At_3181 = AnglePair_89_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = AnglePair_89_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AnglePair_89_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AnglePair_89_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AnglePair_89_Type.Any_8_Concept.TypeOf_3164;
        this.Zero_3240 = AnglePair_89_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = AnglePair_89_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = AnglePair_89_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = AnglePair_89_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = AnglePair_89_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AnglePair_89_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AnglePair_89_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AnglePair_89_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AnglePair_89_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = AnglePair_89_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = AnglePair_89_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = AnglePair_89_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = AnglePair_89_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = AnglePair_89_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = AnglePair_89_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = AnglePair_89_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AnglePair_89_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AnglePair_89_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AnglePair_89_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AnglePair_89_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = AnglePair_89_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = AnglePair_89_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AnglePair_89_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AnglePair_89_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AnglePair_89_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AnglePair_89_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = AnglePair_89_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = AnglePair_89_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AnglePair_89_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AnglePair_89_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AnglePair_89_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AnglePair_89_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = AnglePair_89_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = AnglePair_89_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AnglePair_89_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AnglePair_89_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AnglePair_89_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AnglePair_89_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = AnglePair_89_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = AnglePair_89_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = AnglePair_89_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = AnglePair_89_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = AnglePair_89_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = AnglePair_89_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = AnglePair_89_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = AnglePair_89_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = AnglePair_89_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = AnglePair_89_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Start_1512 = function(self) { return self.Start_1512; }
    static End_1519 = function(self) { return self.End_1519; }
    // implemented concepts 
    static Interval_20_Concept = new Interval_20_Concept(AnglePair_89_Type);
    static Vector_11_Concept = new Vector_11_Concept(AnglePair_89_Type);
    static Array_10_Concept = new Array_10_Concept(AnglePair_89_Type);
    static Any_8_Concept = new Any_8_Concept(AnglePair_89_Type);
    static Numerical_13_Concept = new Numerical_13_Concept(AnglePair_89_Type);
    static Value_9_Concept = new Value_9_Concept(AnglePair_89_Type);
    static Any_8_Concept = new Any_8_Concept(AnglePair_89_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(AnglePair_89_Type);
    static Value_9_Concept = new Value_9_Concept(AnglePair_89_Type);
    static Any_8_Concept = new Any_8_Concept(AnglePair_89_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(AnglePair_89_Type);
    static Value_9_Concept = new Value_9_Concept(AnglePair_89_Type);
    static Any_8_Concept = new Any_8_Concept(AnglePair_89_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(AnglePair_89_Type);
    static Value_9_Concept = new Value_9_Concept(AnglePair_89_Type);
    static Any_8_Concept = new Any_8_Concept(AnglePair_89_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(AnglePair_89_Type);
    static Value_9_Concept = new Value_9_Concept(AnglePair_89_Type);
    static Any_8_Concept = new Any_8_Concept(AnglePair_89_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(AnglePair_89_Type);
    static Value_9_Concept = new Value_9_Concept(AnglePair_89_Type);
    static Any_8_Concept = new Any_8_Concept(AnglePair_89_Type);
    static Implements = [Interval_20_Concept,Vector_11_Concept,Array_10_Concept,Any_8_Concept,Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept];
}
class Ring_90_Type
{
    constructor(Circle_1526, InnerRadius_1533)
    {
        // field initialization 
        this.Circle_1526 = Circle_1526;
        this.InnerRadius_1533 = InnerRadius_1533;
        this.Zero_3240 = Ring_90_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = Ring_90_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = Ring_90_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = Ring_90_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = Ring_90_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Ring_90_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Ring_90_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Ring_90_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Ring_90_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = Ring_90_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = Ring_90_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = Ring_90_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = Ring_90_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = Ring_90_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = Ring_90_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = Ring_90_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Ring_90_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Ring_90_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Ring_90_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Ring_90_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Ring_90_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Ring_90_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Ring_90_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Ring_90_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Ring_90_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Ring_90_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Ring_90_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Ring_90_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Ring_90_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Ring_90_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Ring_90_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Ring_90_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Ring_90_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Ring_90_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Ring_90_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Ring_90_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Ring_90_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Ring_90_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Circle_1526 = function(self) { return self.Circle_1526; }
    static InnerRadius_1533 = function(self) { return self.InnerRadius_1533; }
    // implemented concepts 
    static Numerical_13_Concept = new Numerical_13_Concept(Ring_90_Type);
    static Value_9_Concept = new Value_9_Concept(Ring_90_Type);
    static Any_8_Concept = new Any_8_Concept(Ring_90_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(Ring_90_Type);
    static Value_9_Concept = new Value_9_Concept(Ring_90_Type);
    static Any_8_Concept = new Any_8_Concept(Ring_90_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Ring_90_Type);
    static Value_9_Concept = new Value_9_Concept(Ring_90_Type);
    static Any_8_Concept = new Any_8_Concept(Ring_90_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Ring_90_Type);
    static Value_9_Concept = new Value_9_Concept(Ring_90_Type);
    static Any_8_Concept = new Any_8_Concept(Ring_90_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Ring_90_Type);
    static Value_9_Concept = new Value_9_Concept(Ring_90_Type);
    static Any_8_Concept = new Any_8_Concept(Ring_90_Type);
    static Implements = [Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Arc_91_Type
{
    constructor(Angles_1540, Cirlce_1547)
    {
        // field initialization 
        this.Angles_1540 = Angles_1540;
        this.Cirlce_1547 = Cirlce_1547;
        this.Default_3170 = Arc_91_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Arc_91_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Arc_91_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Arc_91_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Arc_91_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Angles_1540 = function(self) { return self.Angles_1540; }
    static Cirlce_1547 = function(self) { return self.Cirlce_1547; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Arc_91_Type);
    static Any_8_Concept = new Any_8_Concept(Arc_91_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class TimeInterval_92_Type
{
    constructor(Start_1554, End_1561)
    {
        // field initialization 
        this.Start_1554 = Start_1554;
        this.End_1561 = End_1561;
        this.Min_3514 = TimeInterval_92_Type.Interval_20_Concept.Min_3514;
        this.Max_3517 = TimeInterval_92_Type.Interval_20_Concept.Max_3517;
        this.Count_3197 = TimeInterval_92_Type.Vector_11_Concept.Count_3197;
        this.At_3211 = TimeInterval_92_Type.Vector_11_Concept.At_3211;
        this.Count_3176 = TimeInterval_92_Type.Array_10_Concept.Count_3176;
        this.At_3181 = TimeInterval_92_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = TimeInterval_92_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeInterval_92_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeInterval_92_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeInterval_92_Type.Any_8_Concept.TypeOf_3164;
        this.Zero_3240 = TimeInterval_92_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = TimeInterval_92_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = TimeInterval_92_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = TimeInterval_92_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = TimeInterval_92_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeInterval_92_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeInterval_92_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeInterval_92_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeInterval_92_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = TimeInterval_92_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = TimeInterval_92_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = TimeInterval_92_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = TimeInterval_92_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = TimeInterval_92_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = TimeInterval_92_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = TimeInterval_92_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeInterval_92_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeInterval_92_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeInterval_92_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeInterval_92_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = TimeInterval_92_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = TimeInterval_92_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeInterval_92_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeInterval_92_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeInterval_92_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeInterval_92_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = TimeInterval_92_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = TimeInterval_92_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeInterval_92_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeInterval_92_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeInterval_92_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeInterval_92_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = TimeInterval_92_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = TimeInterval_92_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeInterval_92_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeInterval_92_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeInterval_92_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeInterval_92_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = TimeInterval_92_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = TimeInterval_92_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = TimeInterval_92_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = TimeInterval_92_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = TimeInterval_92_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = TimeInterval_92_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = TimeInterval_92_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = TimeInterval_92_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = TimeInterval_92_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = TimeInterval_92_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Start_1554 = function(self) { return self.Start_1554; }
    static End_1561 = function(self) { return self.End_1561; }
    // implemented concepts 
    static Interval_20_Concept = new Interval_20_Concept(TimeInterval_92_Type);
    static Vector_11_Concept = new Vector_11_Concept(TimeInterval_92_Type);
    static Array_10_Concept = new Array_10_Concept(TimeInterval_92_Type);
    static Any_8_Concept = new Any_8_Concept(TimeInterval_92_Type);
    static Numerical_13_Concept = new Numerical_13_Concept(TimeInterval_92_Type);
    static Value_9_Concept = new Value_9_Concept(TimeInterval_92_Type);
    static Any_8_Concept = new Any_8_Concept(TimeInterval_92_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(TimeInterval_92_Type);
    static Value_9_Concept = new Value_9_Concept(TimeInterval_92_Type);
    static Any_8_Concept = new Any_8_Concept(TimeInterval_92_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(TimeInterval_92_Type);
    static Value_9_Concept = new Value_9_Concept(TimeInterval_92_Type);
    static Any_8_Concept = new Any_8_Concept(TimeInterval_92_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(TimeInterval_92_Type);
    static Value_9_Concept = new Value_9_Concept(TimeInterval_92_Type);
    static Any_8_Concept = new Any_8_Concept(TimeInterval_92_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(TimeInterval_92_Type);
    static Value_9_Concept = new Value_9_Concept(TimeInterval_92_Type);
    static Any_8_Concept = new Any_8_Concept(TimeInterval_92_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(TimeInterval_92_Type);
    static Value_9_Concept = new Value_9_Concept(TimeInterval_92_Type);
    static Any_8_Concept = new Any_8_Concept(TimeInterval_92_Type);
    static Implements = [Interval_20_Concept,Vector_11_Concept,Array_10_Concept,Any_8_Concept,Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept];
}
class RealInterval_93_Type
{
    constructor(A_1568, B_1575)
    {
        // field initialization 
        this.A_1568 = A_1568;
        this.B_1575 = B_1575;
        this.Min_3514 = RealInterval_93_Type.Interval_20_Concept.Min_3514;
        this.Max_3517 = RealInterval_93_Type.Interval_20_Concept.Max_3517;
        this.Count_3197 = RealInterval_93_Type.Vector_11_Concept.Count_3197;
        this.At_3211 = RealInterval_93_Type.Vector_11_Concept.At_3211;
        this.Count_3176 = RealInterval_93_Type.Array_10_Concept.Count_3176;
        this.At_3181 = RealInterval_93_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = RealInterval_93_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = RealInterval_93_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = RealInterval_93_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = RealInterval_93_Type.Any_8_Concept.TypeOf_3164;
        this.Zero_3240 = RealInterval_93_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = RealInterval_93_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = RealInterval_93_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = RealInterval_93_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = RealInterval_93_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = RealInterval_93_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = RealInterval_93_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = RealInterval_93_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = RealInterval_93_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = RealInterval_93_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = RealInterval_93_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = RealInterval_93_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = RealInterval_93_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = RealInterval_93_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = RealInterval_93_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = RealInterval_93_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = RealInterval_93_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = RealInterval_93_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = RealInterval_93_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = RealInterval_93_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = RealInterval_93_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = RealInterval_93_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = RealInterval_93_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = RealInterval_93_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = RealInterval_93_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = RealInterval_93_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = RealInterval_93_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = RealInterval_93_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = RealInterval_93_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = RealInterval_93_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = RealInterval_93_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = RealInterval_93_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = RealInterval_93_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = RealInterval_93_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = RealInterval_93_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = RealInterval_93_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = RealInterval_93_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = RealInterval_93_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = RealInterval_93_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = RealInterval_93_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = RealInterval_93_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = RealInterval_93_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = RealInterval_93_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = RealInterval_93_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = RealInterval_93_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = RealInterval_93_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = RealInterval_93_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = RealInterval_93_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static A_1568 = function(self) { return self.A_1568; }
    static B_1575 = function(self) { return self.B_1575; }
    // implemented concepts 
    static Interval_20_Concept = new Interval_20_Concept(RealInterval_93_Type);
    static Vector_11_Concept = new Vector_11_Concept(RealInterval_93_Type);
    static Array_10_Concept = new Array_10_Concept(RealInterval_93_Type);
    static Any_8_Concept = new Any_8_Concept(RealInterval_93_Type);
    static Numerical_13_Concept = new Numerical_13_Concept(RealInterval_93_Type);
    static Value_9_Concept = new Value_9_Concept(RealInterval_93_Type);
    static Any_8_Concept = new Any_8_Concept(RealInterval_93_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(RealInterval_93_Type);
    static Value_9_Concept = new Value_9_Concept(RealInterval_93_Type);
    static Any_8_Concept = new Any_8_Concept(RealInterval_93_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(RealInterval_93_Type);
    static Value_9_Concept = new Value_9_Concept(RealInterval_93_Type);
    static Any_8_Concept = new Any_8_Concept(RealInterval_93_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(RealInterval_93_Type);
    static Value_9_Concept = new Value_9_Concept(RealInterval_93_Type);
    static Any_8_Concept = new Any_8_Concept(RealInterval_93_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(RealInterval_93_Type);
    static Value_9_Concept = new Value_9_Concept(RealInterval_93_Type);
    static Any_8_Concept = new Any_8_Concept(RealInterval_93_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(RealInterval_93_Type);
    static Value_9_Concept = new Value_9_Concept(RealInterval_93_Type);
    static Any_8_Concept = new Any_8_Concept(RealInterval_93_Type);
    static Implements = [Interval_20_Concept,Vector_11_Concept,Array_10_Concept,Any_8_Concept,Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept];
}
class Interval2D_94_Type
{
    constructor(A_1582, B_1589)
    {
        // field initialization 
        this.A_1582 = A_1582;
        this.B_1589 = B_1589;
        this.Min_3514 = Interval2D_94_Type.Interval_20_Concept.Min_3514;
        this.Max_3517 = Interval2D_94_Type.Interval_20_Concept.Max_3517;
        this.Count_3197 = Interval2D_94_Type.Vector_11_Concept.Count_3197;
        this.At_3211 = Interval2D_94_Type.Vector_11_Concept.At_3211;
        this.Count_3176 = Interval2D_94_Type.Array_10_Concept.Count_3176;
        this.At_3181 = Interval2D_94_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = Interval2D_94_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Interval2D_94_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Interval2D_94_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Interval2D_94_Type.Any_8_Concept.TypeOf_3164;
        this.Zero_3240 = Interval2D_94_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = Interval2D_94_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = Interval2D_94_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = Interval2D_94_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = Interval2D_94_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Interval2D_94_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Interval2D_94_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Interval2D_94_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Interval2D_94_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = Interval2D_94_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = Interval2D_94_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = Interval2D_94_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = Interval2D_94_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = Interval2D_94_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = Interval2D_94_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = Interval2D_94_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Interval2D_94_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Interval2D_94_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Interval2D_94_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Interval2D_94_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Interval2D_94_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Interval2D_94_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Interval2D_94_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Interval2D_94_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Interval2D_94_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Interval2D_94_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Interval2D_94_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Interval2D_94_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Interval2D_94_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Interval2D_94_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Interval2D_94_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Interval2D_94_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Interval2D_94_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Interval2D_94_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Interval2D_94_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Interval2D_94_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Interval2D_94_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Interval2D_94_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Interval2D_94_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Interval2D_94_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Interval2D_94_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Interval2D_94_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Interval2D_94_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Interval2D_94_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Interval2D_94_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Interval2D_94_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Interval2D_94_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Interval2D_94_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static A_1582 = function(self) { return self.A_1582; }
    static B_1589 = function(self) { return self.B_1589; }
    // implemented concepts 
    static Interval_20_Concept = new Interval_20_Concept(Interval2D_94_Type);
    static Vector_11_Concept = new Vector_11_Concept(Interval2D_94_Type);
    static Array_10_Concept = new Array_10_Concept(Interval2D_94_Type);
    static Any_8_Concept = new Any_8_Concept(Interval2D_94_Type);
    static Numerical_13_Concept = new Numerical_13_Concept(Interval2D_94_Type);
    static Value_9_Concept = new Value_9_Concept(Interval2D_94_Type);
    static Any_8_Concept = new Any_8_Concept(Interval2D_94_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(Interval2D_94_Type);
    static Value_9_Concept = new Value_9_Concept(Interval2D_94_Type);
    static Any_8_Concept = new Any_8_Concept(Interval2D_94_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Interval2D_94_Type);
    static Value_9_Concept = new Value_9_Concept(Interval2D_94_Type);
    static Any_8_Concept = new Any_8_Concept(Interval2D_94_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Interval2D_94_Type);
    static Value_9_Concept = new Value_9_Concept(Interval2D_94_Type);
    static Any_8_Concept = new Any_8_Concept(Interval2D_94_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Interval2D_94_Type);
    static Value_9_Concept = new Value_9_Concept(Interval2D_94_Type);
    static Any_8_Concept = new Any_8_Concept(Interval2D_94_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Interval2D_94_Type);
    static Value_9_Concept = new Value_9_Concept(Interval2D_94_Type);
    static Any_8_Concept = new Any_8_Concept(Interval2D_94_Type);
    static Implements = [Interval_20_Concept,Vector_11_Concept,Array_10_Concept,Any_8_Concept,Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept];
}
class Interval3D_95_Type
{
    constructor(A_1596, B_1603)
    {
        // field initialization 
        this.A_1596 = A_1596;
        this.B_1603 = B_1603;
        this.Min_3514 = Interval3D_95_Type.Interval_20_Concept.Min_3514;
        this.Max_3517 = Interval3D_95_Type.Interval_20_Concept.Max_3517;
        this.Count_3197 = Interval3D_95_Type.Vector_11_Concept.Count_3197;
        this.At_3211 = Interval3D_95_Type.Vector_11_Concept.At_3211;
        this.Count_3176 = Interval3D_95_Type.Array_10_Concept.Count_3176;
        this.At_3181 = Interval3D_95_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = Interval3D_95_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Interval3D_95_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Interval3D_95_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Interval3D_95_Type.Any_8_Concept.TypeOf_3164;
        this.Zero_3240 = Interval3D_95_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = Interval3D_95_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = Interval3D_95_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = Interval3D_95_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = Interval3D_95_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Interval3D_95_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Interval3D_95_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Interval3D_95_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Interval3D_95_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = Interval3D_95_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = Interval3D_95_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = Interval3D_95_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = Interval3D_95_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = Interval3D_95_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = Interval3D_95_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = Interval3D_95_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Interval3D_95_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Interval3D_95_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Interval3D_95_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Interval3D_95_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Interval3D_95_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Interval3D_95_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Interval3D_95_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Interval3D_95_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Interval3D_95_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Interval3D_95_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Interval3D_95_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Interval3D_95_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Interval3D_95_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Interval3D_95_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Interval3D_95_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Interval3D_95_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Interval3D_95_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Interval3D_95_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Interval3D_95_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Interval3D_95_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Interval3D_95_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Interval3D_95_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Interval3D_95_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Interval3D_95_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Interval3D_95_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Interval3D_95_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Interval3D_95_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Interval3D_95_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Interval3D_95_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Interval3D_95_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Interval3D_95_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Interval3D_95_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static A_1596 = function(self) { return self.A_1596; }
    static B_1603 = function(self) { return self.B_1603; }
    // implemented concepts 
    static Interval_20_Concept = new Interval_20_Concept(Interval3D_95_Type);
    static Vector_11_Concept = new Vector_11_Concept(Interval3D_95_Type);
    static Array_10_Concept = new Array_10_Concept(Interval3D_95_Type);
    static Any_8_Concept = new Any_8_Concept(Interval3D_95_Type);
    static Numerical_13_Concept = new Numerical_13_Concept(Interval3D_95_Type);
    static Value_9_Concept = new Value_9_Concept(Interval3D_95_Type);
    static Any_8_Concept = new Any_8_Concept(Interval3D_95_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(Interval3D_95_Type);
    static Value_9_Concept = new Value_9_Concept(Interval3D_95_Type);
    static Any_8_Concept = new Any_8_Concept(Interval3D_95_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Interval3D_95_Type);
    static Value_9_Concept = new Value_9_Concept(Interval3D_95_Type);
    static Any_8_Concept = new Any_8_Concept(Interval3D_95_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Interval3D_95_Type);
    static Value_9_Concept = new Value_9_Concept(Interval3D_95_Type);
    static Any_8_Concept = new Any_8_Concept(Interval3D_95_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Interval3D_95_Type);
    static Value_9_Concept = new Value_9_Concept(Interval3D_95_Type);
    static Any_8_Concept = new Any_8_Concept(Interval3D_95_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Interval3D_95_Type);
    static Value_9_Concept = new Value_9_Concept(Interval3D_95_Type);
    static Any_8_Concept = new Any_8_Concept(Interval3D_95_Type);
    static Implements = [Interval_20_Concept,Vector_11_Concept,Array_10_Concept,Any_8_Concept,Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept];
}
class Capsule_96_Type
{
    constructor(Line_1610, Radius_1617)
    {
        // field initialization 
        this.Line_1610 = Line_1610;
        this.Radius_1617 = Radius_1617;
        this.Default_3170 = Capsule_96_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Capsule_96_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Capsule_96_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Capsule_96_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Capsule_96_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Line_1610 = function(self) { return self.Line_1610; }
    static Radius_1617 = function(self) { return self.Radius_1617; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Capsule_96_Type);
    static Any_8_Concept = new Any_8_Concept(Capsule_96_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Matrix3D_97_Type
{
    constructor(Column1_1624, Column2_1631, Column3_1638, Column4_1645)
    {
        // field initialization 
        this.Column1_1624 = Column1_1624;
        this.Column2_1631 = Column2_1631;
        this.Column3_1638 = Column3_1638;
        this.Column4_1645 = Column4_1645;
        this.Default_3170 = Matrix3D_97_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Matrix3D_97_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Matrix3D_97_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Matrix3D_97_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Matrix3D_97_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Column1_1624 = function(self) { return self.Column1_1624; }
    static Column2_1631 = function(self) { return self.Column2_1631; }
    static Column3_1638 = function(self) { return self.Column3_1638; }
    static Column4_1645 = function(self) { return self.Column4_1645; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Matrix3D_97_Type);
    static Any_8_Concept = new Any_8_Concept(Matrix3D_97_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Cylinder_98_Type
{
    constructor(Line_1652, Radius_1659)
    {
        // field initialization 
        this.Line_1652 = Line_1652;
        this.Radius_1659 = Radius_1659;
        this.Default_3170 = Cylinder_98_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Cylinder_98_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Cylinder_98_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Cylinder_98_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Cylinder_98_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Line_1652 = function(self) { return self.Line_1652; }
    static Radius_1659 = function(self) { return self.Radius_1659; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Cylinder_98_Type);
    static Any_8_Concept = new Any_8_Concept(Cylinder_98_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Cone_99_Type
{
    constructor(Line_1666, Radius_1673)
    {
        // field initialization 
        this.Line_1666 = Line_1666;
        this.Radius_1673 = Radius_1673;
        this.Default_3170 = Cone_99_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Cone_99_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Cone_99_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Cone_99_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Cone_99_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Line_1666 = function(self) { return self.Line_1666; }
    static Radius_1673 = function(self) { return self.Radius_1673; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Cone_99_Type);
    static Any_8_Concept = new Any_8_Concept(Cone_99_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Tube_100_Type
{
    constructor(Line_1680, InnerRadius_1687, OuterRadius_1694)
    {
        // field initialization 
        this.Line_1680 = Line_1680;
        this.InnerRadius_1687 = InnerRadius_1687;
        this.OuterRadius_1694 = OuterRadius_1694;
        this.Default_3170 = Tube_100_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Tube_100_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Tube_100_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Tube_100_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Tube_100_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Line_1680 = function(self) { return self.Line_1680; }
    static InnerRadius_1687 = function(self) { return self.InnerRadius_1687; }
    static OuterRadius_1694 = function(self) { return self.OuterRadius_1694; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Tube_100_Type);
    static Any_8_Concept = new Any_8_Concept(Tube_100_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class ConeSegment_101_Type
{
    constructor(Line_1701, Radius1_1708, Radius2_1715)
    {
        // field initialization 
        this.Line_1701 = Line_1701;
        this.Radius1_1708 = Radius1_1708;
        this.Radius2_1715 = Radius2_1715;
        this.Default_3170 = ConeSegment_101_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ConeSegment_101_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ConeSegment_101_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ConeSegment_101_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ConeSegment_101_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Line_1701 = function(self) { return self.Line_1701; }
    static Radius1_1708 = function(self) { return self.Radius1_1708; }
    static Radius2_1715 = function(self) { return self.Radius2_1715; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(ConeSegment_101_Type);
    static Any_8_Concept = new Any_8_Concept(ConeSegment_101_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Box2D_102_Type
{
    constructor(Center_1722, Rotation_1729, Extent_1736)
    {
        // field initialization 
        this.Center_1722 = Center_1722;
        this.Rotation_1729 = Rotation_1729;
        this.Extent_1736 = Extent_1736;
        this.Default_3170 = Box2D_102_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Box2D_102_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Box2D_102_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Box2D_102_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Box2D_102_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Center_1722 = function(self) { return self.Center_1722; }
    static Rotation_1729 = function(self) { return self.Rotation_1729; }
    static Extent_1736 = function(self) { return self.Extent_1736; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Box2D_102_Type);
    static Any_8_Concept = new Any_8_Concept(Box2D_102_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Box3D_103_Type
{
    constructor(Center_1743, Rotation_1750, Extent_1757)
    {
        // field initialization 
        this.Center_1743 = Center_1743;
        this.Rotation_1750 = Rotation_1750;
        this.Extent_1757 = Extent_1757;
        this.Default_3170 = Box3D_103_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Box3D_103_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Box3D_103_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Box3D_103_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Box3D_103_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Center_1743 = function(self) { return self.Center_1743; }
    static Rotation_1750 = function(self) { return self.Rotation_1750; }
    static Extent_1757 = function(self) { return self.Extent_1757; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(Box3D_103_Type);
    static Any_8_Concept = new Any_8_Concept(Box3D_103_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class CubicBezierTriangle3D_104_Type
{
    constructor(A_1764, B_1771, C_1778, A2B_1785, AB2_1792, B2C_1799, BC2_1806, AC2_1813, A2C_1820, ABC_1827)
    {
        // field initialization 
        this.A_1764 = A_1764;
        this.B_1771 = B_1771;
        this.C_1778 = C_1778;
        this.A2B_1785 = A2B_1785;
        this.AB2_1792 = AB2_1792;
        this.B2C_1799 = B2C_1799;
        this.BC2_1806 = BC2_1806;
        this.AC2_1813 = AC2_1813;
        this.A2C_1820 = A2C_1820;
        this.ABC_1827 = ABC_1827;
        this.Default_3170 = CubicBezierTriangle3D_104_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = CubicBezierTriangle3D_104_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = CubicBezierTriangle3D_104_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = CubicBezierTriangle3D_104_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = CubicBezierTriangle3D_104_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static A_1764 = function(self) { return self.A_1764; }
    static B_1771 = function(self) { return self.B_1771; }
    static C_1778 = function(self) { return self.C_1778; }
    static A2B_1785 = function(self) { return self.A2B_1785; }
    static AB2_1792 = function(self) { return self.AB2_1792; }
    static B2C_1799 = function(self) { return self.B2C_1799; }
    static BC2_1806 = function(self) { return self.BC2_1806; }
    static AC2_1813 = function(self) { return self.AC2_1813; }
    static A2C_1820 = function(self) { return self.A2C_1820; }
    static ABC_1827 = function(self) { return self.ABC_1827; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(CubicBezierTriangle3D_104_Type);
    static Any_8_Concept = new Any_8_Concept(CubicBezierTriangle3D_104_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class CubicBezier2D_105_Type
{
    constructor(A_1834, B_1841, C_1848, D_1855)
    {
        // field initialization 
        this.A_1834 = A_1834;
        this.B_1841 = B_1841;
        this.C_1848 = C_1848;
        this.D_1855 = D_1855;
        this.Default_3170 = CubicBezier2D_105_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = CubicBezier2D_105_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = CubicBezier2D_105_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = CubicBezier2D_105_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = CubicBezier2D_105_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static A_1834 = function(self) { return self.A_1834; }
    static B_1841 = function(self) { return self.B_1841; }
    static C_1848 = function(self) { return self.C_1848; }
    static D_1855 = function(self) { return self.D_1855; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(CubicBezier2D_105_Type);
    static Any_8_Concept = new Any_8_Concept(CubicBezier2D_105_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class UV_106_Type
{
    constructor(U_1862, V_1869)
    {
        // field initialization 
        this.U_1862 = U_1862;
        this.V_1869 = V_1869;
        this.Count_3197 = UV_106_Type.Vector_11_Concept.Count_3197;
        this.At_3211 = UV_106_Type.Vector_11_Concept.At_3211;
        this.Count_3176 = UV_106_Type.Array_10_Concept.Count_3176;
        this.At_3181 = UV_106_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = UV_106_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = UV_106_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = UV_106_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = UV_106_Type.Any_8_Concept.TypeOf_3164;
        this.Zero_3240 = UV_106_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = UV_106_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = UV_106_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = UV_106_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = UV_106_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = UV_106_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = UV_106_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = UV_106_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = UV_106_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = UV_106_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = UV_106_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = UV_106_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = UV_106_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = UV_106_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = UV_106_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = UV_106_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = UV_106_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = UV_106_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = UV_106_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = UV_106_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = UV_106_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = UV_106_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = UV_106_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = UV_106_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = UV_106_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = UV_106_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = UV_106_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = UV_106_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = UV_106_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = UV_106_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = UV_106_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = UV_106_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = UV_106_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = UV_106_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = UV_106_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = UV_106_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = UV_106_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = UV_106_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = UV_106_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = UV_106_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = UV_106_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = UV_106_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = UV_106_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = UV_106_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = UV_106_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = UV_106_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = UV_106_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = UV_106_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static U_1862 = function(self) { return self.U_1862; }
    static V_1869 = function(self) { return self.V_1869; }
    // implemented concepts 
    static Vector_11_Concept = new Vector_11_Concept(UV_106_Type);
    static Array_10_Concept = new Array_10_Concept(UV_106_Type);
    static Any_8_Concept = new Any_8_Concept(UV_106_Type);
    static Numerical_13_Concept = new Numerical_13_Concept(UV_106_Type);
    static Value_9_Concept = new Value_9_Concept(UV_106_Type);
    static Any_8_Concept = new Any_8_Concept(UV_106_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(UV_106_Type);
    static Value_9_Concept = new Value_9_Concept(UV_106_Type);
    static Any_8_Concept = new Any_8_Concept(UV_106_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(UV_106_Type);
    static Value_9_Concept = new Value_9_Concept(UV_106_Type);
    static Any_8_Concept = new Any_8_Concept(UV_106_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(UV_106_Type);
    static Value_9_Concept = new Value_9_Concept(UV_106_Type);
    static Any_8_Concept = new Any_8_Concept(UV_106_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(UV_106_Type);
    static Value_9_Concept = new Value_9_Concept(UV_106_Type);
    static Any_8_Concept = new Any_8_Concept(UV_106_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(UV_106_Type);
    static Value_9_Concept = new Value_9_Concept(UV_106_Type);
    static Any_8_Concept = new Any_8_Concept(UV_106_Type);
    static Implements = [Vector_11_Concept,Array_10_Concept,Any_8_Concept,Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept];
}
class UVW_107_Type
{
    constructor(U_1876, V_1883, W_1890)
    {
        // field initialization 
        this.U_1876 = U_1876;
        this.V_1883 = V_1883;
        this.W_1890 = W_1890;
        this.Count_3197 = UVW_107_Type.Vector_11_Concept.Count_3197;
        this.At_3211 = UVW_107_Type.Vector_11_Concept.At_3211;
        this.Count_3176 = UVW_107_Type.Array_10_Concept.Count_3176;
        this.At_3181 = UVW_107_Type.Array_10_Concept.At_3181;
        this.FieldNames_3155 = UVW_107_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = UVW_107_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = UVW_107_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = UVW_107_Type.Any_8_Concept.TypeOf_3164;
        this.Zero_3240 = UVW_107_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = UVW_107_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = UVW_107_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = UVW_107_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = UVW_107_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = UVW_107_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = UVW_107_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = UVW_107_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = UVW_107_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = UVW_107_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = UVW_107_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = UVW_107_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = UVW_107_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = UVW_107_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = UVW_107_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = UVW_107_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = UVW_107_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = UVW_107_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = UVW_107_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = UVW_107_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = UVW_107_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = UVW_107_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = UVW_107_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = UVW_107_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = UVW_107_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = UVW_107_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = UVW_107_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = UVW_107_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = UVW_107_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = UVW_107_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = UVW_107_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = UVW_107_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = UVW_107_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = UVW_107_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = UVW_107_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = UVW_107_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = UVW_107_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = UVW_107_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = UVW_107_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = UVW_107_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = UVW_107_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = UVW_107_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = UVW_107_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = UVW_107_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = UVW_107_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = UVW_107_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = UVW_107_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = UVW_107_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static U_1876 = function(self) { return self.U_1876; }
    static V_1883 = function(self) { return self.V_1883; }
    static W_1890 = function(self) { return self.W_1890; }
    // implemented concepts 
    static Vector_11_Concept = new Vector_11_Concept(UVW_107_Type);
    static Array_10_Concept = new Array_10_Concept(UVW_107_Type);
    static Any_8_Concept = new Any_8_Concept(UVW_107_Type);
    static Numerical_13_Concept = new Numerical_13_Concept(UVW_107_Type);
    static Value_9_Concept = new Value_9_Concept(UVW_107_Type);
    static Any_8_Concept = new Any_8_Concept(UVW_107_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(UVW_107_Type);
    static Value_9_Concept = new Value_9_Concept(UVW_107_Type);
    static Any_8_Concept = new Any_8_Concept(UVW_107_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(UVW_107_Type);
    static Value_9_Concept = new Value_9_Concept(UVW_107_Type);
    static Any_8_Concept = new Any_8_Concept(UVW_107_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(UVW_107_Type);
    static Value_9_Concept = new Value_9_Concept(UVW_107_Type);
    static Any_8_Concept = new Any_8_Concept(UVW_107_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(UVW_107_Type);
    static Value_9_Concept = new Value_9_Concept(UVW_107_Type);
    static Any_8_Concept = new Any_8_Concept(UVW_107_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(UVW_107_Type);
    static Value_9_Concept = new Value_9_Concept(UVW_107_Type);
    static Any_8_Concept = new Any_8_Concept(UVW_107_Type);
    static Implements = [Vector_11_Concept,Array_10_Concept,Any_8_Concept,Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept];
}
class CubicBezier3D_108_Type
{
    constructor(A_1897, B_1904, C_1911, D_1918)
    {
        // field initialization 
        this.A_1897 = A_1897;
        this.B_1904 = B_1904;
        this.C_1911 = C_1911;
        this.D_1918 = D_1918;
        this.Default_3170 = CubicBezier3D_108_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = CubicBezier3D_108_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = CubicBezier3D_108_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = CubicBezier3D_108_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = CubicBezier3D_108_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static A_1897 = function(self) { return self.A_1897; }
    static B_1904 = function(self) { return self.B_1904; }
    static C_1911 = function(self) { return self.C_1911; }
    static D_1918 = function(self) { return self.D_1918; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(CubicBezier3D_108_Type);
    static Any_8_Concept = new Any_8_Concept(CubicBezier3D_108_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class QuadraticBezier2D_109_Type
{
    constructor(A_1925, B_1932, C_1939)
    {
        // field initialization 
        this.A_1925 = A_1925;
        this.B_1932 = B_1932;
        this.C_1939 = C_1939;
        this.Default_3170 = QuadraticBezier2D_109_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = QuadraticBezier2D_109_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = QuadraticBezier2D_109_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = QuadraticBezier2D_109_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = QuadraticBezier2D_109_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static A_1925 = function(self) { return self.A_1925; }
    static B_1932 = function(self) { return self.B_1932; }
    static C_1939 = function(self) { return self.C_1939; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(QuadraticBezier2D_109_Type);
    static Any_8_Concept = new Any_8_Concept(QuadraticBezier2D_109_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class QuadraticBezier3D_110_Type
{
    constructor(A_1946, B_1953, C_1960)
    {
        // field initialization 
        this.A_1946 = A_1946;
        this.B_1953 = B_1953;
        this.C_1960 = C_1960;
        this.Default_3170 = QuadraticBezier3D_110_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = QuadraticBezier3D_110_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = QuadraticBezier3D_110_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = QuadraticBezier3D_110_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = QuadraticBezier3D_110_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static A_1946 = function(self) { return self.A_1946; }
    static B_1953 = function(self) { return self.B_1953; }
    static C_1960 = function(self) { return self.C_1960; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(QuadraticBezier3D_110_Type);
    static Any_8_Concept = new Any_8_Concept(QuadraticBezier3D_110_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Area_111_Type
{
    constructor(MetersSquared_1967)
    {
        // field initialization 
        this.MetersSquared_1967 = MetersSquared_1967;
        this.Value_3230 = Area_111_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = Area_111_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Area_111_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Area_111_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Area_111_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Area_111_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Area_111_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Area_111_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Area_111_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Area_111_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Area_111_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Area_111_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Area_111_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Area_111_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Area_111_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Area_111_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Area_111_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Area_111_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Area_111_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Area_111_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Area_111_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Area_111_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Area_111_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Area_111_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Area_111_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Area_111_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Area_111_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Area_111_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Area_111_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Area_111_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Area_111_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Area_111_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Area_111_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Area_111_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static MetersSquared_1967 = function(self) { return self.MetersSquared_1967; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(Area_111_Type);
    static Value_9_Concept = new Value_9_Concept(Area_111_Type);
    static Any_8_Concept = new Any_8_Concept(Area_111_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Area_111_Type);
    static Value_9_Concept = new Value_9_Concept(Area_111_Type);
    static Any_8_Concept = new Any_8_Concept(Area_111_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Area_111_Type);
    static Value_9_Concept = new Value_9_Concept(Area_111_Type);
    static Any_8_Concept = new Any_8_Concept(Area_111_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Area_111_Type);
    static Value_9_Concept = new Value_9_Concept(Area_111_Type);
    static Any_8_Concept = new Any_8_Concept(Area_111_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Area_111_Type);
    static Value_9_Concept = new Value_9_Concept(Area_111_Type);
    static Any_8_Concept = new Any_8_Concept(Area_111_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Volume_112_Type
{
    constructor(MetersCubed_1974)
    {
        // field initialization 
        this.MetersCubed_1974 = MetersCubed_1974;
        this.Value_3230 = Volume_112_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = Volume_112_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Volume_112_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Volume_112_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Volume_112_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Volume_112_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Volume_112_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Volume_112_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Volume_112_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Volume_112_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Volume_112_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Volume_112_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Volume_112_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Volume_112_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Volume_112_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Volume_112_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Volume_112_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Volume_112_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Volume_112_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Volume_112_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Volume_112_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Volume_112_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Volume_112_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Volume_112_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Volume_112_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Volume_112_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Volume_112_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Volume_112_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Volume_112_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Volume_112_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Volume_112_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Volume_112_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Volume_112_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Volume_112_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static MetersCubed_1974 = function(self) { return self.MetersCubed_1974; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(Volume_112_Type);
    static Value_9_Concept = new Value_9_Concept(Volume_112_Type);
    static Any_8_Concept = new Any_8_Concept(Volume_112_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Volume_112_Type);
    static Value_9_Concept = new Value_9_Concept(Volume_112_Type);
    static Any_8_Concept = new Any_8_Concept(Volume_112_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Volume_112_Type);
    static Value_9_Concept = new Value_9_Concept(Volume_112_Type);
    static Any_8_Concept = new Any_8_Concept(Volume_112_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Volume_112_Type);
    static Value_9_Concept = new Value_9_Concept(Volume_112_Type);
    static Any_8_Concept = new Any_8_Concept(Volume_112_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Volume_112_Type);
    static Value_9_Concept = new Value_9_Concept(Volume_112_Type);
    static Any_8_Concept = new Any_8_Concept(Volume_112_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Velocity_113_Type
{
    constructor(MetersPerSecond_1981)
    {
        // field initialization 
        this.MetersPerSecond_1981 = MetersPerSecond_1981;
        this.Value_3230 = Velocity_113_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = Velocity_113_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Velocity_113_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Velocity_113_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Velocity_113_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Velocity_113_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Velocity_113_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Velocity_113_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Velocity_113_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Velocity_113_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Velocity_113_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Velocity_113_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Velocity_113_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Velocity_113_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Velocity_113_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Velocity_113_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Velocity_113_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Velocity_113_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Velocity_113_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Velocity_113_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Velocity_113_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Velocity_113_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Velocity_113_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Velocity_113_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Velocity_113_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Velocity_113_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Velocity_113_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Velocity_113_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Velocity_113_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Velocity_113_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Velocity_113_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Velocity_113_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Velocity_113_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Velocity_113_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static MetersPerSecond_1981 = function(self) { return self.MetersPerSecond_1981; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(Velocity_113_Type);
    static Value_9_Concept = new Value_9_Concept(Velocity_113_Type);
    static Any_8_Concept = new Any_8_Concept(Velocity_113_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Velocity_113_Type);
    static Value_9_Concept = new Value_9_Concept(Velocity_113_Type);
    static Any_8_Concept = new Any_8_Concept(Velocity_113_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Velocity_113_Type);
    static Value_9_Concept = new Value_9_Concept(Velocity_113_Type);
    static Any_8_Concept = new Any_8_Concept(Velocity_113_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Velocity_113_Type);
    static Value_9_Concept = new Value_9_Concept(Velocity_113_Type);
    static Any_8_Concept = new Any_8_Concept(Velocity_113_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Velocity_113_Type);
    static Value_9_Concept = new Value_9_Concept(Velocity_113_Type);
    static Any_8_Concept = new Any_8_Concept(Velocity_113_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Acceleration_114_Type
{
    constructor(MetersPerSecondSquared_1988)
    {
        // field initialization 
        this.MetersPerSecondSquared_1988 = MetersPerSecondSquared_1988;
        this.Value_3230 = Acceleration_114_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = Acceleration_114_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Acceleration_114_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Acceleration_114_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Acceleration_114_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Acceleration_114_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Acceleration_114_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Acceleration_114_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Acceleration_114_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Acceleration_114_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Acceleration_114_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Acceleration_114_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Acceleration_114_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Acceleration_114_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Acceleration_114_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Acceleration_114_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Acceleration_114_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Acceleration_114_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Acceleration_114_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Acceleration_114_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Acceleration_114_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Acceleration_114_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Acceleration_114_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Acceleration_114_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Acceleration_114_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Acceleration_114_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Acceleration_114_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Acceleration_114_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Acceleration_114_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Acceleration_114_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Acceleration_114_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Acceleration_114_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Acceleration_114_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Acceleration_114_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static MetersPerSecondSquared_1988 = function(self) { return self.MetersPerSecondSquared_1988; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(Acceleration_114_Type);
    static Value_9_Concept = new Value_9_Concept(Acceleration_114_Type);
    static Any_8_Concept = new Any_8_Concept(Acceleration_114_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Acceleration_114_Type);
    static Value_9_Concept = new Value_9_Concept(Acceleration_114_Type);
    static Any_8_Concept = new Any_8_Concept(Acceleration_114_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Acceleration_114_Type);
    static Value_9_Concept = new Value_9_Concept(Acceleration_114_Type);
    static Any_8_Concept = new Any_8_Concept(Acceleration_114_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Acceleration_114_Type);
    static Value_9_Concept = new Value_9_Concept(Acceleration_114_Type);
    static Any_8_Concept = new Any_8_Concept(Acceleration_114_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Acceleration_114_Type);
    static Value_9_Concept = new Value_9_Concept(Acceleration_114_Type);
    static Any_8_Concept = new Any_8_Concept(Acceleration_114_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Force_115_Type
{
    constructor(Newtons_1995)
    {
        // field initialization 
        this.Newtons_1995 = Newtons_1995;
        this.Value_3230 = Force_115_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = Force_115_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Force_115_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Force_115_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Force_115_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Force_115_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Force_115_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Force_115_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Force_115_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Force_115_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Force_115_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Force_115_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Force_115_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Force_115_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Force_115_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Force_115_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Force_115_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Force_115_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Force_115_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Force_115_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Force_115_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Force_115_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Force_115_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Force_115_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Force_115_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Force_115_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Force_115_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Force_115_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Force_115_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Force_115_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Force_115_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Force_115_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Force_115_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Force_115_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Newtons_1995 = function(self) { return self.Newtons_1995; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(Force_115_Type);
    static Value_9_Concept = new Value_9_Concept(Force_115_Type);
    static Any_8_Concept = new Any_8_Concept(Force_115_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Force_115_Type);
    static Value_9_Concept = new Value_9_Concept(Force_115_Type);
    static Any_8_Concept = new Any_8_Concept(Force_115_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Force_115_Type);
    static Value_9_Concept = new Value_9_Concept(Force_115_Type);
    static Any_8_Concept = new Any_8_Concept(Force_115_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Force_115_Type);
    static Value_9_Concept = new Value_9_Concept(Force_115_Type);
    static Any_8_Concept = new Any_8_Concept(Force_115_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Force_115_Type);
    static Value_9_Concept = new Value_9_Concept(Force_115_Type);
    static Any_8_Concept = new Any_8_Concept(Force_115_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Pressure_116_Type
{
    constructor(Pascals_2002)
    {
        // field initialization 
        this.Pascals_2002 = Pascals_2002;
        this.Value_3230 = Pressure_116_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = Pressure_116_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Pressure_116_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Pressure_116_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Pressure_116_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Pressure_116_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Pressure_116_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Pressure_116_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Pressure_116_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Pressure_116_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Pressure_116_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Pressure_116_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Pressure_116_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Pressure_116_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Pressure_116_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Pressure_116_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Pressure_116_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Pressure_116_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Pressure_116_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Pressure_116_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Pressure_116_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Pressure_116_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Pressure_116_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Pressure_116_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Pressure_116_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Pressure_116_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Pressure_116_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Pressure_116_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Pressure_116_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Pressure_116_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Pressure_116_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Pressure_116_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Pressure_116_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Pressure_116_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Pascals_2002 = function(self) { return self.Pascals_2002; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(Pressure_116_Type);
    static Value_9_Concept = new Value_9_Concept(Pressure_116_Type);
    static Any_8_Concept = new Any_8_Concept(Pressure_116_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Pressure_116_Type);
    static Value_9_Concept = new Value_9_Concept(Pressure_116_Type);
    static Any_8_Concept = new Any_8_Concept(Pressure_116_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Pressure_116_Type);
    static Value_9_Concept = new Value_9_Concept(Pressure_116_Type);
    static Any_8_Concept = new Any_8_Concept(Pressure_116_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Pressure_116_Type);
    static Value_9_Concept = new Value_9_Concept(Pressure_116_Type);
    static Any_8_Concept = new Any_8_Concept(Pressure_116_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Pressure_116_Type);
    static Value_9_Concept = new Value_9_Concept(Pressure_116_Type);
    static Any_8_Concept = new Any_8_Concept(Pressure_116_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Energy_117_Type
{
    constructor(Joules_2009)
    {
        // field initialization 
        this.Joules_2009 = Joules_2009;
        this.Value_3230 = Energy_117_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = Energy_117_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Energy_117_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Energy_117_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Energy_117_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Energy_117_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Energy_117_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Energy_117_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Energy_117_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Energy_117_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Energy_117_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Energy_117_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Energy_117_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Energy_117_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Energy_117_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Energy_117_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Energy_117_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Energy_117_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Energy_117_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Energy_117_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Energy_117_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Energy_117_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Energy_117_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Energy_117_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Energy_117_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Energy_117_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Energy_117_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Energy_117_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Energy_117_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Energy_117_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Energy_117_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Energy_117_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Energy_117_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Energy_117_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Joules_2009 = function(self) { return self.Joules_2009; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(Energy_117_Type);
    static Value_9_Concept = new Value_9_Concept(Energy_117_Type);
    static Any_8_Concept = new Any_8_Concept(Energy_117_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Energy_117_Type);
    static Value_9_Concept = new Value_9_Concept(Energy_117_Type);
    static Any_8_Concept = new Any_8_Concept(Energy_117_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Energy_117_Type);
    static Value_9_Concept = new Value_9_Concept(Energy_117_Type);
    static Any_8_Concept = new Any_8_Concept(Energy_117_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Energy_117_Type);
    static Value_9_Concept = new Value_9_Concept(Energy_117_Type);
    static Any_8_Concept = new Any_8_Concept(Energy_117_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Energy_117_Type);
    static Value_9_Concept = new Value_9_Concept(Energy_117_Type);
    static Any_8_Concept = new Any_8_Concept(Energy_117_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Memory_118_Type
{
    constructor(Bytes_2016)
    {
        // field initialization 
        this.Bytes_2016 = Bytes_2016;
        this.Value_3230 = Memory_118_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = Memory_118_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Memory_118_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Memory_118_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Memory_118_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Memory_118_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Memory_118_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Memory_118_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Memory_118_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Memory_118_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Memory_118_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Memory_118_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Memory_118_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Memory_118_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Memory_118_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Memory_118_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Memory_118_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Memory_118_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Memory_118_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Memory_118_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Memory_118_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Memory_118_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Memory_118_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Memory_118_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Memory_118_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Memory_118_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Memory_118_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Memory_118_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Memory_118_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Memory_118_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Memory_118_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Memory_118_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Memory_118_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Memory_118_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Bytes_2016 = function(self) { return self.Bytes_2016; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(Memory_118_Type);
    static Value_9_Concept = new Value_9_Concept(Memory_118_Type);
    static Any_8_Concept = new Any_8_Concept(Memory_118_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Memory_118_Type);
    static Value_9_Concept = new Value_9_Concept(Memory_118_Type);
    static Any_8_Concept = new Any_8_Concept(Memory_118_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Memory_118_Type);
    static Value_9_Concept = new Value_9_Concept(Memory_118_Type);
    static Any_8_Concept = new Any_8_Concept(Memory_118_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Memory_118_Type);
    static Value_9_Concept = new Value_9_Concept(Memory_118_Type);
    static Any_8_Concept = new Any_8_Concept(Memory_118_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Memory_118_Type);
    static Value_9_Concept = new Value_9_Concept(Memory_118_Type);
    static Any_8_Concept = new Any_8_Concept(Memory_118_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Frequency_119_Type
{
    constructor(Hertz_2023)
    {
        // field initialization 
        this.Hertz_2023 = Hertz_2023;
        this.Value_3230 = Frequency_119_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = Frequency_119_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Frequency_119_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Frequency_119_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Frequency_119_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Frequency_119_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Frequency_119_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Frequency_119_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Frequency_119_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Frequency_119_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Frequency_119_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Frequency_119_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Frequency_119_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Frequency_119_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Frequency_119_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Frequency_119_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Frequency_119_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Frequency_119_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Frequency_119_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Frequency_119_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Frequency_119_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Frequency_119_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Frequency_119_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Frequency_119_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Frequency_119_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Frequency_119_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Frequency_119_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Frequency_119_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Frequency_119_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Frequency_119_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Frequency_119_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Frequency_119_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Frequency_119_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Frequency_119_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Hertz_2023 = function(self) { return self.Hertz_2023; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(Frequency_119_Type);
    static Value_9_Concept = new Value_9_Concept(Frequency_119_Type);
    static Any_8_Concept = new Any_8_Concept(Frequency_119_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Frequency_119_Type);
    static Value_9_Concept = new Value_9_Concept(Frequency_119_Type);
    static Any_8_Concept = new Any_8_Concept(Frequency_119_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Frequency_119_Type);
    static Value_9_Concept = new Value_9_Concept(Frequency_119_Type);
    static Any_8_Concept = new Any_8_Concept(Frequency_119_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Frequency_119_Type);
    static Value_9_Concept = new Value_9_Concept(Frequency_119_Type);
    static Any_8_Concept = new Any_8_Concept(Frequency_119_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Frequency_119_Type);
    static Value_9_Concept = new Value_9_Concept(Frequency_119_Type);
    static Any_8_Concept = new Any_8_Concept(Frequency_119_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Loudness_120_Type
{
    constructor(Decibels_2030)
    {
        // field initialization 
        this.Decibels_2030 = Decibels_2030;
        this.Value_3230 = Loudness_120_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = Loudness_120_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Loudness_120_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Loudness_120_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Loudness_120_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Loudness_120_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Loudness_120_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Loudness_120_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Loudness_120_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Loudness_120_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Loudness_120_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Loudness_120_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Loudness_120_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Loudness_120_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Loudness_120_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Loudness_120_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Loudness_120_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Loudness_120_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Loudness_120_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Loudness_120_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Loudness_120_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Loudness_120_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Loudness_120_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Loudness_120_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Loudness_120_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Loudness_120_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Loudness_120_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Loudness_120_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Loudness_120_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Loudness_120_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Loudness_120_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Loudness_120_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Loudness_120_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Loudness_120_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Decibels_2030 = function(self) { return self.Decibels_2030; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(Loudness_120_Type);
    static Value_9_Concept = new Value_9_Concept(Loudness_120_Type);
    static Any_8_Concept = new Any_8_Concept(Loudness_120_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Loudness_120_Type);
    static Value_9_Concept = new Value_9_Concept(Loudness_120_Type);
    static Any_8_Concept = new Any_8_Concept(Loudness_120_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Loudness_120_Type);
    static Value_9_Concept = new Value_9_Concept(Loudness_120_Type);
    static Any_8_Concept = new Any_8_Concept(Loudness_120_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Loudness_120_Type);
    static Value_9_Concept = new Value_9_Concept(Loudness_120_Type);
    static Any_8_Concept = new Any_8_Concept(Loudness_120_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Loudness_120_Type);
    static Value_9_Concept = new Value_9_Concept(Loudness_120_Type);
    static Any_8_Concept = new Any_8_Concept(Loudness_120_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class LuminousIntensity_121_Type
{
    constructor(Candelas_2037)
    {
        // field initialization 
        this.Candelas_2037 = Candelas_2037;
        this.Value_3230 = LuminousIntensity_121_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = LuminousIntensity_121_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = LuminousIntensity_121_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = LuminousIntensity_121_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = LuminousIntensity_121_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = LuminousIntensity_121_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = LuminousIntensity_121_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = LuminousIntensity_121_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = LuminousIntensity_121_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = LuminousIntensity_121_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = LuminousIntensity_121_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = LuminousIntensity_121_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = LuminousIntensity_121_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = LuminousIntensity_121_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = LuminousIntensity_121_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = LuminousIntensity_121_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = LuminousIntensity_121_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = LuminousIntensity_121_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = LuminousIntensity_121_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = LuminousIntensity_121_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = LuminousIntensity_121_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = LuminousIntensity_121_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = LuminousIntensity_121_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = LuminousIntensity_121_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = LuminousIntensity_121_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = LuminousIntensity_121_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = LuminousIntensity_121_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = LuminousIntensity_121_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = LuminousIntensity_121_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = LuminousIntensity_121_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = LuminousIntensity_121_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = LuminousIntensity_121_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = LuminousIntensity_121_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = LuminousIntensity_121_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Candelas_2037 = function(self) { return self.Candelas_2037; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(LuminousIntensity_121_Type);
    static Value_9_Concept = new Value_9_Concept(LuminousIntensity_121_Type);
    static Any_8_Concept = new Any_8_Concept(LuminousIntensity_121_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(LuminousIntensity_121_Type);
    static Value_9_Concept = new Value_9_Concept(LuminousIntensity_121_Type);
    static Any_8_Concept = new Any_8_Concept(LuminousIntensity_121_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(LuminousIntensity_121_Type);
    static Value_9_Concept = new Value_9_Concept(LuminousIntensity_121_Type);
    static Any_8_Concept = new Any_8_Concept(LuminousIntensity_121_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(LuminousIntensity_121_Type);
    static Value_9_Concept = new Value_9_Concept(LuminousIntensity_121_Type);
    static Any_8_Concept = new Any_8_Concept(LuminousIntensity_121_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(LuminousIntensity_121_Type);
    static Value_9_Concept = new Value_9_Concept(LuminousIntensity_121_Type);
    static Any_8_Concept = new Any_8_Concept(LuminousIntensity_121_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class ElectricPotential_122_Type
{
    constructor(Volts_2044)
    {
        // field initialization 
        this.Volts_2044 = Volts_2044;
        this.Value_3230 = ElectricPotential_122_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = ElectricPotential_122_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricPotential_122_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricPotential_122_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricPotential_122_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricPotential_122_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = ElectricPotential_122_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = ElectricPotential_122_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = ElectricPotential_122_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = ElectricPotential_122_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = ElectricPotential_122_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = ElectricPotential_122_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricPotential_122_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricPotential_122_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricPotential_122_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricPotential_122_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = ElectricPotential_122_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = ElectricPotential_122_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricPotential_122_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricPotential_122_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricPotential_122_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricPotential_122_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = ElectricPotential_122_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = ElectricPotential_122_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricPotential_122_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricPotential_122_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricPotential_122_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricPotential_122_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = ElectricPotential_122_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = ElectricPotential_122_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricPotential_122_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricPotential_122_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricPotential_122_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricPotential_122_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Volts_2044 = function(self) { return self.Volts_2044; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(ElectricPotential_122_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricPotential_122_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricPotential_122_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(ElectricPotential_122_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricPotential_122_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricPotential_122_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(ElectricPotential_122_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricPotential_122_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricPotential_122_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(ElectricPotential_122_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricPotential_122_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricPotential_122_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(ElectricPotential_122_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricPotential_122_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricPotential_122_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class ElectricCharge_123_Type
{
    constructor(Columbs_2051)
    {
        // field initialization 
        this.Columbs_2051 = Columbs_2051;
        this.Value_3230 = ElectricCharge_123_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = ElectricCharge_123_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricCharge_123_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricCharge_123_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricCharge_123_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricCharge_123_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = ElectricCharge_123_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = ElectricCharge_123_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = ElectricCharge_123_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = ElectricCharge_123_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = ElectricCharge_123_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = ElectricCharge_123_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricCharge_123_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricCharge_123_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricCharge_123_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricCharge_123_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = ElectricCharge_123_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = ElectricCharge_123_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricCharge_123_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricCharge_123_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricCharge_123_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricCharge_123_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = ElectricCharge_123_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = ElectricCharge_123_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricCharge_123_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricCharge_123_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricCharge_123_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricCharge_123_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = ElectricCharge_123_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = ElectricCharge_123_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricCharge_123_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricCharge_123_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricCharge_123_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricCharge_123_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Columbs_2051 = function(self) { return self.Columbs_2051; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(ElectricCharge_123_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricCharge_123_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricCharge_123_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(ElectricCharge_123_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricCharge_123_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricCharge_123_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(ElectricCharge_123_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricCharge_123_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricCharge_123_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(ElectricCharge_123_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricCharge_123_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricCharge_123_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(ElectricCharge_123_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricCharge_123_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricCharge_123_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class ElectricCurrent_124_Type
{
    constructor(Amperes_2058)
    {
        // field initialization 
        this.Amperes_2058 = Amperes_2058;
        this.Value_3230 = ElectricCurrent_124_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = ElectricCurrent_124_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricCurrent_124_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricCurrent_124_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricCurrent_124_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricCurrent_124_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = ElectricCurrent_124_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = ElectricCurrent_124_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = ElectricCurrent_124_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = ElectricCurrent_124_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = ElectricCurrent_124_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = ElectricCurrent_124_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricCurrent_124_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricCurrent_124_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricCurrent_124_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricCurrent_124_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = ElectricCurrent_124_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = ElectricCurrent_124_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricCurrent_124_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricCurrent_124_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricCurrent_124_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricCurrent_124_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = ElectricCurrent_124_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = ElectricCurrent_124_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricCurrent_124_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricCurrent_124_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricCurrent_124_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricCurrent_124_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = ElectricCurrent_124_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = ElectricCurrent_124_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricCurrent_124_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricCurrent_124_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricCurrent_124_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricCurrent_124_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Amperes_2058 = function(self) { return self.Amperes_2058; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(ElectricCurrent_124_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricCurrent_124_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricCurrent_124_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(ElectricCurrent_124_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricCurrent_124_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricCurrent_124_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(ElectricCurrent_124_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricCurrent_124_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricCurrent_124_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(ElectricCurrent_124_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricCurrent_124_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricCurrent_124_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(ElectricCurrent_124_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricCurrent_124_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricCurrent_124_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class ElectricResistance_125_Type
{
    constructor(Ohms_2065)
    {
        // field initialization 
        this.Ohms_2065 = Ohms_2065;
        this.Value_3230 = ElectricResistance_125_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = ElectricResistance_125_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricResistance_125_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricResistance_125_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricResistance_125_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricResistance_125_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = ElectricResistance_125_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = ElectricResistance_125_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = ElectricResistance_125_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = ElectricResistance_125_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = ElectricResistance_125_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = ElectricResistance_125_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricResistance_125_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricResistance_125_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricResistance_125_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricResistance_125_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = ElectricResistance_125_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = ElectricResistance_125_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricResistance_125_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricResistance_125_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricResistance_125_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricResistance_125_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = ElectricResistance_125_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = ElectricResistance_125_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricResistance_125_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricResistance_125_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricResistance_125_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricResistance_125_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = ElectricResistance_125_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = ElectricResistance_125_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = ElectricResistance_125_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = ElectricResistance_125_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = ElectricResistance_125_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = ElectricResistance_125_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Ohms_2065 = function(self) { return self.Ohms_2065; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(ElectricResistance_125_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricResistance_125_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricResistance_125_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(ElectricResistance_125_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricResistance_125_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricResistance_125_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(ElectricResistance_125_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricResistance_125_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricResistance_125_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(ElectricResistance_125_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricResistance_125_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricResistance_125_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(ElectricResistance_125_Type);
    static Value_9_Concept = new Value_9_Concept(ElectricResistance_125_Type);
    static Any_8_Concept = new Any_8_Concept(ElectricResistance_125_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Power_126_Type
{
    constructor(Watts_2072)
    {
        // field initialization 
        this.Watts_2072 = Watts_2072;
        this.Value_3230 = Power_126_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = Power_126_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Power_126_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Power_126_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Power_126_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Power_126_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Power_126_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Power_126_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Power_126_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Power_126_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Power_126_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Power_126_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Power_126_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Power_126_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Power_126_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Power_126_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Power_126_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Power_126_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Power_126_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Power_126_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Power_126_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Power_126_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Power_126_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Power_126_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Power_126_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Power_126_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Power_126_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Power_126_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Power_126_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Power_126_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Power_126_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Power_126_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Power_126_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Power_126_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Watts_2072 = function(self) { return self.Watts_2072; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(Power_126_Type);
    static Value_9_Concept = new Value_9_Concept(Power_126_Type);
    static Any_8_Concept = new Any_8_Concept(Power_126_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Power_126_Type);
    static Value_9_Concept = new Value_9_Concept(Power_126_Type);
    static Any_8_Concept = new Any_8_Concept(Power_126_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Power_126_Type);
    static Value_9_Concept = new Value_9_Concept(Power_126_Type);
    static Any_8_Concept = new Any_8_Concept(Power_126_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Power_126_Type);
    static Value_9_Concept = new Value_9_Concept(Power_126_Type);
    static Any_8_Concept = new Any_8_Concept(Power_126_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Power_126_Type);
    static Value_9_Concept = new Value_9_Concept(Power_126_Type);
    static Any_8_Concept = new Any_8_Concept(Power_126_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class Density_127_Type
{
    constructor(KilogramsPerMeterCubed_2079)
    {
        // field initialization 
        this.KilogramsPerMeterCubed_2079 = KilogramsPerMeterCubed_2079;
        this.Value_3230 = Density_127_Type.Measure_12_Concept.Value_3230;
        this.Default_3170 = Density_127_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Density_127_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Density_127_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Density_127_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Density_127_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3405 = Density_127_Type.ScalarArithmetic_18_Concept.Add_3405;
        this.Subtract_3419 = Density_127_Type.ScalarArithmetic_18_Concept.Subtract_3419;
        this.Multiply_3433 = Density_127_Type.ScalarArithmetic_18_Concept.Multiply_3433;
        this.Divide_3447 = Density_127_Type.ScalarArithmetic_18_Concept.Divide_3447;
        this.Modulo_3461 = Density_127_Type.ScalarArithmetic_18_Concept.Modulo_3461;
        this.Default_3170 = Density_127_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Density_127_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Density_127_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Density_127_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Density_127_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Density_127_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Density_127_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Density_127_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Density_127_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Density_127_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Density_127_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Density_127_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Density_127_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Density_127_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Density_127_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Density_127_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Density_127_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Density_127_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Density_127_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Density_127_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Density_127_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Density_127_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Density_127_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static KilogramsPerMeterCubed_2079 = function(self) { return self.KilogramsPerMeterCubed_2079; }
    // implemented concepts 
    static Measure_12_Concept = new Measure_12_Concept(Density_127_Type);
    static Value_9_Concept = new Value_9_Concept(Density_127_Type);
    static Any_8_Concept = new Any_8_Concept(Density_127_Type);
    static ScalarArithmetic_18_Concept = new ScalarArithmetic_18_Concept(Density_127_Type);
    static Value_9_Concept = new Value_9_Concept(Density_127_Type);
    static Any_8_Concept = new Any_8_Concept(Density_127_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Density_127_Type);
    static Value_9_Concept = new Value_9_Concept(Density_127_Type);
    static Any_8_Concept = new Any_8_Concept(Density_127_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Density_127_Type);
    static Value_9_Concept = new Value_9_Concept(Density_127_Type);
    static Any_8_Concept = new Any_8_Concept(Density_127_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Density_127_Type);
    static Value_9_Concept = new Value_9_Concept(Density_127_Type);
    static Any_8_Concept = new Any_8_Concept(Density_127_Type);
    static Implements = [Measure_12_Concept,Value_9_Concept,Any_8_Concept,ScalarArithmetic_18_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class NormalDistribution_128_Type
{
    constructor(Mean_2086, StandardDeviation_2093)
    {
        // field initialization 
        this.Mean_2086 = Mean_2086;
        this.StandardDeviation_2093 = StandardDeviation_2093;
        this.Default_3170 = NormalDistribution_128_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = NormalDistribution_128_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = NormalDistribution_128_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = NormalDistribution_128_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = NormalDistribution_128_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Mean_2086 = function(self) { return self.Mean_2086; }
    static StandardDeviation_2093 = function(self) { return self.StandardDeviation_2093; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(NormalDistribution_128_Type);
    static Any_8_Concept = new Any_8_Concept(NormalDistribution_128_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class PoissonDistribution_129_Type
{
    constructor(Expected_2100, Occurrences_2107)
    {
        // field initialization 
        this.Expected_2100 = Expected_2100;
        this.Occurrences_2107 = Occurrences_2107;
        this.Default_3170 = PoissonDistribution_129_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = PoissonDistribution_129_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = PoissonDistribution_129_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = PoissonDistribution_129_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = PoissonDistribution_129_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Expected_2100 = function(self) { return self.Expected_2100; }
    static Occurrences_2107 = function(self) { return self.Occurrences_2107; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(PoissonDistribution_129_Type);
    static Any_8_Concept = new Any_8_Concept(PoissonDistribution_129_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class BernoulliDistribution_130_Type
{
    constructor(P_2114)
    {
        // field initialization 
        this.P_2114 = P_2114;
        this.Default_3170 = BernoulliDistribution_130_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = BernoulliDistribution_130_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = BernoulliDistribution_130_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = BernoulliDistribution_130_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = BernoulliDistribution_130_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static P_2114 = function(self) { return self.P_2114; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(BernoulliDistribution_130_Type);
    static Any_8_Concept = new Any_8_Concept(BernoulliDistribution_130_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}
class Probability_131_Type
{
    constructor(Value_2121)
    {
        // field initialization 
        this.Value_2121 = Value_2121;
        this.Zero_3240 = Probability_131_Type.Numerical_13_Concept.Zero_3240;
        this.One_3243 = Probability_131_Type.Numerical_13_Concept.One_3243;
        this.MinValue_3246 = Probability_131_Type.Numerical_13_Concept.MinValue_3246;
        this.MaxValue_3249 = Probability_131_Type.Numerical_13_Concept.MaxValue_3249;
        this.Default_3170 = Probability_131_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Probability_131_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Probability_131_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Probability_131_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Probability_131_Type.Any_8_Concept.TypeOf_3164;
        this.Add_3317 = Probability_131_Type.Arithmetic_17_Concept.Add_3317;
        this.Negative_3327 = Probability_131_Type.Arithmetic_17_Concept.Negative_3327;
        this.Reciprocal_3337 = Probability_131_Type.Arithmetic_17_Concept.Reciprocal_3337;
        this.Multiply_3354 = Probability_131_Type.Arithmetic_17_Concept.Multiply_3354;
        this.Divide_3371 = Probability_131_Type.Arithmetic_17_Concept.Divide_3371;
        this.Modulo_3388 = Probability_131_Type.Arithmetic_17_Concept.Modulo_3388;
        this.Default_3170 = Probability_131_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Probability_131_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Probability_131_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Probability_131_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Probability_131_Type.Any_8_Concept.TypeOf_3164;
        this.Equals_3297 = Probability_131_Type.Equatable_16_Concept.Equals_3297;
        this.Default_3170 = Probability_131_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Probability_131_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Probability_131_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Probability_131_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Probability_131_Type.Any_8_Concept.TypeOf_3164;
        this.Compare_3274 = Probability_131_Type.Comparable_15_Concept.Compare_3274;
        this.Default_3170 = Probability_131_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Probability_131_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Probability_131_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Probability_131_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Probability_131_Type.Any_8_Concept.TypeOf_3164;
        this.Magnitude_3268 = Probability_131_Type.Magnitudinal_14_Concept.Magnitude_3268;
        this.Default_3170 = Probability_131_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = Probability_131_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = Probability_131_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = Probability_131_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = Probability_131_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Value_2121 = function(self) { return self.Value_2121; }
    // implemented concepts 
    static Numerical_13_Concept = new Numerical_13_Concept(Probability_131_Type);
    static Value_9_Concept = new Value_9_Concept(Probability_131_Type);
    static Any_8_Concept = new Any_8_Concept(Probability_131_Type);
    static Arithmetic_17_Concept = new Arithmetic_17_Concept(Probability_131_Type);
    static Value_9_Concept = new Value_9_Concept(Probability_131_Type);
    static Any_8_Concept = new Any_8_Concept(Probability_131_Type);
    static Equatable_16_Concept = new Equatable_16_Concept(Probability_131_Type);
    static Value_9_Concept = new Value_9_Concept(Probability_131_Type);
    static Any_8_Concept = new Any_8_Concept(Probability_131_Type);
    static Comparable_15_Concept = new Comparable_15_Concept(Probability_131_Type);
    static Value_9_Concept = new Value_9_Concept(Probability_131_Type);
    static Any_8_Concept = new Any_8_Concept(Probability_131_Type);
    static Magnitudinal_14_Concept = new Magnitudinal_14_Concept(Probability_131_Type);
    static Value_9_Concept = new Value_9_Concept(Probability_131_Type);
    static Any_8_Concept = new Any_8_Concept(Probability_131_Type);
    static Implements = [Numerical_13_Concept,Value_9_Concept,Any_8_Concept,Arithmetic_17_Concept,Value_9_Concept,Any_8_Concept,Equatable_16_Concept,Value_9_Concept,Any_8_Concept,Comparable_15_Concept,Value_9_Concept,Any_8_Concept,Magnitudinal_14_Concept,Value_9_Concept,Any_8_Concept];
}
class BinomialDistribution_132_Type
{
    constructor(Trials_2128, P_2135)
    {
        // field initialization 
        this.Trials_2128 = Trials_2128;
        this.P_2135 = P_2135;
        this.Default_3170 = BinomialDistribution_132_Type.Value_9_Concept.Default_3170;
        this.FieldNames_3155 = BinomialDistribution_132_Type.Any_8_Concept.FieldNames_3155;
        this.FieldValues_3158 = BinomialDistribution_132_Type.Any_8_Concept.FieldValues_3158;
        this.FieldTypes_3161 = BinomialDistribution_132_Type.Any_8_Concept.FieldTypes_3161;
        this.TypeOf_3164 = BinomialDistribution_132_Type.Any_8_Concept.TypeOf_3164;
    }
    // field accessors
    static Trials_2128 = function(self) { return self.Trials_2128; }
    static P_2135 = function(self) { return self.P_2135; }
    // implemented concepts 
    static Value_9_Concept = new Value_9_Concept(BinomialDistribution_132_Type);
    static Any_8_Concept = new Any_8_Concept(BinomialDistribution_132_Type);
    static Implements = [Value_9_Concept,Any_8_Concept];
}

// This is appended to every JavaScript program generated from Plato