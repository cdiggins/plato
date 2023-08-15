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
    static Cos_3033 = function (x_3032/* : Angle_82 */) /* : Number_21 */{ return null; };
    static Sin_3036 = function (x_3035/* : Angle_82 */) /* : Number_21 */{ return null; };
    static Tan_3039 = function (x_3038/* : Angle_82 */) /* : Number_21 */{ return null; };
    static Acos_3042 = function (x_3041/* : Number_21 */) /* : Angle_82 */{ return null; };
    static Asin_3045 = function (x_3044/* : Number_21 */) /* : Angle_82 */{ return null; };
    static Atan_3048 = function (x_3047/* : Number_21 */) /* : Angle_82 */{ return null; };
    static Pow_3053 = function (x_3050/* : Number_21 */, y_3052/* : Number_21 */) /* : Number_21 */{ return null; };
    static Log_3058 = function (x_3055/* : Number_21 */, y_3057/* : Number_21 */) /* : Number_21 */{ return null; };
    static NaturalLog_3061 = function (x_3060/* : Number_21 */) /* : Number_21 */{ return null; };
    static NaturalPower_3064 = function (x_3063/* : Number_21 */) /* : Number_21 */{ return null; };
    static Interpolate_3067 = function (xs_3066/* : Array_10 */) /* : String_23 */{ return null; };
    static Throw_3070 = function (x_3069/* : Any_8 */) /* : Any_8 */{ return null; };
    static TypeOf_3073 = function (x_3072/* : Any_8 */) /* : Type_25 */{ return null; };
    static Add_3078 = function (x_3075/* : Number_21 */, y_3077/* : Number_21 */) /* : Number_21 */{ return null; };
    static Subtract_3083 = function (x_3080/* : Number_21 */, y_3082/* : Number_21 */) /* : Number_21 */{ return null; };
    static Divide_3088 = function (x_3085/* : Number_21 */, y_3087/* : Number_21 */) /* : Number_21 */{ return null; };
    static Multiply_3093 = function (x_3090/* : Number_21 */, y_3092/* : Number_21 */) /* : Number_21 */{ return null; };
    static Modulo_3098 = function (x_3095/* : Number_21 */, y_3097/* : Number_21 */) /* : Number_21 */{ return null; };
    static Negative_3101 = function (x_3100/* : Number_21 */) /* : Number_21 */{ return null; };
    static Add_3106 = function (x_3103/* : Integer_22 */, y_3105/* : Integer_22 */) /* : Integer_22 */{ return null; };
    static Subtract_3111 = function (x_3108/* : Integer_22 */, y_3110/* : Integer_22 */) /* : Integer_22 */{ return null; };
    static Divide_3116 = function (x_3113/* : Integer_22 */, y_3115/* : Integer_22 */) /* : Integer_22 */{ return null; };
    static Multiply_3121 = function (x_3118/* : Integer_22 */, y_3120/* : Integer_22 */) /* : Integer_22 */{ return null; };
    static Modulo_3126 = function (x_3123/* : Integer_22 */, y_3125/* : Integer_22 */) /* : Integer_22 */{ return null; };
    static Negative_3129 = function (x_3128/* : Integer_22 */) /* : Integer_22 */{ return null; };
    static And_3134 = function (x_3131/* : Boolean_24 */, y_3133/* : Boolean_24 */) /* : Boolean_24 */{ return null; };
    static Or_3139 = function (x_3136/* : Boolean_24 */, y_3138/* : Boolean_24 */) /* : Boolean_24 */{ return null; };
    static Not_3142 = function (x_3141/* : Boolean_24 */) /* : Boolean_24 */{ return null; };
}
class Array_133_Library
{
    static Map_3874 = function (xs_3850/* : Array_10 */, f_3852/* : Function_3 */) /* : Array_10 */{ return Tuple_1/* : Error_6 */(Count_374/* : UnknownType */(xs_3850/* : UnknownType */)/* : UnknownType */, function (i_3859/* : UnknownType */) /* : Lambda_2 */{ return f_3852/* : UnknownType */(At_380/* : UnknownType */(xs_3850/* : UnknownType */, i_3859/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Error_6 */; };
    static Reverse_3911 = function (xs_3876/* : Array_10 */) /* : Array_10 */{ return Tuple_1/* : Error_6 */(Count_374/* : UnknownType */(xs_3876/* : UnknownType */)/* : UnknownType */, function (i_3883/* : UnknownType */) /* : Lambda_2 */{ return f_3852/* : UnknownType */(At_380/* : UnknownType */(xs_3876/* : UnknownType */, Subtract_233/* : UnknownType */(Count_374/* : UnknownType */(xs_3876/* : UnknownType */)/* : UnknownType */, Subtract_233/* : UnknownType */(1/* : UnknownType */, i_3883/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Error_6 */; };
    static Zip_3944 = function (xs_3913/* : Array_10 */, ys_3915/* : Array_10 */, f_3917/* : Function_3 */) /* : Array_10 */{ return Tuple_1/* : Error_6 */(Count_374/* : UnknownType */(xs_3913/* : UnknownType */)/* : UnknownType */, function (i_3924/* : UnknownType */) /* : Lambda_2 */{ return f_3917/* : UnknownType */(At_380/* : UnknownType */(i_3924/* : UnknownType */)/* : UnknownType */, At_380/* : UnknownType */(ys_3915/* : UnknownType */, i_3924/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Error_6 */; };
    static Zip_3986 = function (xs_3946/* : Array_10 */, ys_3948/* : Array_10 */, zs_3950/* : Array_10 */, f_3952/* : Function_3 */) /* : Array_10 */{ return Tuple_1/* : Error_6 */(Count_374/* : UnknownType */(xs_3946/* : UnknownType */)/* : UnknownType */, function (i_3959/* : UnknownType */) /* : Lambda_2 */{ return f_3952/* : UnknownType */(At_380/* : UnknownType */(i_3959/* : UnknownType */)/* : UnknownType */, At_380/* : UnknownType */(ys_3948/* : UnknownType */, i_3959/* : UnknownType */)/* : UnknownType */, At_380/* : UnknownType */(zs_3950/* : UnknownType */, i_3959/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Error_6 */; };
    static Skip_4014 = function (xs_3988/* : Array_10 */, n_3990/* : Integer_22 */) /* : Array_10 */{ return Tuple_1/* : Error_6 */(Subtract_233/* : UnknownType */(Count_374/* : UnknownType */, n_3990/* : UnknownType */)/* : UnknownType */, function (i_3999/* : UnknownType */) /* : Lambda_2 */{ return At_380/* : UnknownType */(Subtract_233/* : UnknownType */(i_3999/* : UnknownType */, n_3990/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : Error_6 */; };
    static Take_4032 = function (xs_4016/* : Array_10 */, n_4018/* : Integer_22 */) /* : Array_10 */{ return Tuple_1/* : Error_6 */(n_4018/* : UnknownType */, function (i_4022/* : UnknownType */) /* : Lambda_2 */{ return At_380/* : UnknownType */(i_4022/* : UnknownType */)/* : UnknownType */; })/* : Error_6 */; };
    static Aggregate_4057 = function (xs_4034/* : Array_10 */, init_4036/* : Any_8 */, f_4038/* : Function_3 */) /* : Any_8 */{ return IsEmpty_2210/* : UnknownType */(xs_4034/* : UnknownType */)/* : UnknownType */
        ? init_4036/* : Any_8 */
        : f_4038/* : Function_3 */(init_4036/* : UnknownType */, f_4038/* : UnknownType */(Rest_2204/* : UnknownType */(xs_4034/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Function_3 */
    ; };
    static Rest_4066 = function (xs_4059/* : Array_10 */) /* : Array_10 */{ return Skip_2178/* : UnknownType */(xs_4059/* : Array_10 */, 1/* : Integer_22 */)/* : Array_10 */; };
    static IsEmpty_4078 = function (xs_4068/* : Array_10 */) /* : Boolean_24 */{ return Equals_446/* : UnknownType */(Count_374/* : UnknownType */(xs_4068/* : Array_10 */)/* : Integer_22 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static First_4087 = function (xs_4080/* : Array_10 */) /* : Any_8 */{ return At_380/* : UnknownType */(xs_4080/* : Array_10 */, 0/* : Integer_22 */)/* : Any_8 */; };
    static Last_4104 = function (xs_4089/* : Array_10 */) /* : Any_8 */{ return At_380/* : UnknownType */(xs_4089/* : Array_10 */, Subtract_233/* : UnknownType */(Count_374/* : UnknownType */(xs_4089/* : Array_10 */)/* : Integer_22 */, 1/* : Integer_22 */)/* : Integer_22 */)/* : Any_8 */; };
    static Slice_4122 = function (xs_4106/* : Array_10 */, from_4108/* : Integer_22 */, count_4110/* : Integer_22 */) /* : Array_10 */{ return Take_2186/* : UnknownType */(Skip_2178/* : UnknownType */(xs_4106/* : Array_10 */, from_4108/* : Integer_22 */)/* : Array_10 */, count_4110/* : Integer_22 */)/* : Array_10 */; };
    static Join_4168 = function (xs_4124/* : Array_10 */, sep_4126/* : String_23 */) /* : String_23 */{ return IsEmpty_2210/* : UnknownType */(xs_4124/* : UnknownType */)/* : UnknownType */
        ? ""/* : String_23 */
        : Add_225/* : UnknownType */(ToString_2418/* : UnknownType */(First_2216/* : UnknownType */(xs_4124/* : Array_10 */)/* : Any_8 */)/* : String_23 */, Aggregate_2194/* : UnknownType */(Rest_2204/* : UnknownType */(xs_4124/* : Array_10 */)/* : Array_10 */, ""/* : String_23 */, function (acc_4148/* : UnknownType */, cur_4150/* : UnknownType */) /* : Lambda_2 */{ return Interpolate_207/* : UnknownType */(acc_4148/* : UnknownType */, sep_4126/* : UnknownType */, cur_4150/* : UnknownType */)/* : UnknownType */; })/* : Any_8 */)/* : Number_21 */
    ; };
    static All_4197 = function (xs_4170/* : Array_10 */, f_4172/* : Function_3 */) /* : Boolean_24 */{ return IsEmpty_2210/* : UnknownType */(xs_4170/* : UnknownType */)/* : UnknownType */
        ? True/* : Boolean_24 */
        : And_317/* : UnknownType */(f_4172/* : Function_3 */(First_2216/* : UnknownType */(xs_4170/* : UnknownType */)/* : UnknownType */)/* : Function_3 */, f_4172/* : Function_3 */(Rest_2204/* : UnknownType */(xs_4170/* : UnknownType */)/* : UnknownType */)/* : Function_3 */)/* : Boolean_24 */
    ; };
    static All_4210 = function (xs_4199/* : Array_10 */) /* : Boolean_24 */{ return All_2246/* : UnknownType */(xs_4199/* : Array_10 */, function (b_4203/* : UnknownType */) /* : Lambda_2 */{ return b_4203/* : UnknownType */; })/* : Boolean_24 */; };
}
class Interval_134_Library
{
    static Size_4225 = function (x_4212/* : Interval_20 */) /* : Numerical_13 */{ return Subtract_233/* : UnknownType */(Max_568/* : UnknownType */(x_4212/* : Interval_20 */)/* : Numerical_13 */, Min_562/* : UnknownType */(x_4212/* : Interval_20 */)/* : Numerical_13 */)/* : Number_21 */; };
    static IsEmpty_4240 = function (x_4227/* : Interval_20 */) /* : Boolean_24 */{ return GreaterThanOrEquals_2778/* : UnknownType */(Min_562/* : UnknownType */(x_4227/* : Interval_20 */)/* : Numerical_13 */, Max_568/* : UnknownType */(x_4227/* : Interval_20 */)/* : Numerical_13 */)/* : Boolean_24 */; };
    static Lerp_4272 = function (x_4242/* : Interval_20 */, amount_4244/* : Unit_29 */) /* : Numerical_13 */{ return Multiply_249/* : UnknownType */(Min_562/* : UnknownType */(x_4242/* : Interval_20 */)/* : Numerical_13 */, Add_225/* : UnknownType */(Subtract_233/* : UnknownType */(1/* : Number_21 */, amount_4244/* : Unit_29 */)/* : Number_21 */, Multiply_249/* : UnknownType */(Max_568/* : UnknownType */(x_4242/* : Interval_20 */)/* : Numerical_13 */, amount_4244/* : Unit_29 */)/* : Arithmetic_17 */)/* : Arithmetic_17 */)/* : Arithmetic_17 */; };
    static InverseLerp_4294 = function (x_4274/* : Interval_20 */, value_4276/* : Numerical_13 */) /* : Unit_29 */{ return Divide_241/* : UnknownType */(Subtract_233/* : UnknownType */(value_4276/* : Numerical_13 */, Min_562/* : UnknownType */(x_4274/* : Interval_20 */)/* : Numerical_13 */)/* : Number_21 */, Size_1435/* : UnknownType */(x_4274/* : Interval_20 */)/* : Numerical_13 */)/* : Arithmetic_17 */; };
    static Negate_4315 = function (x_4296/* : Interval_20 */) /* : Interval_20 */{ return Tuple_1/* : Error_6 */(Negative_265/* : UnknownType */(Max_568/* : UnknownType */(x_4296/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Negative_265/* : UnknownType */(Min_562/* : UnknownType */(x_4296/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Error_6 */; };
    static Reverse_4330 = function (x_4317/* : Interval_20 */) /* : Interval_20 */{ return Tuple_1/* : Error_6 */(Max_568/* : UnknownType */(x_4317/* : UnknownType */)/* : UnknownType */, Min_562/* : UnknownType */(x_4317/* : UnknownType */)/* : UnknownType */)/* : Error_6 */; };
    static Center_4339 = function (x_4332/* : Interval_20 */) /* : Numerical_13 */{ return Lerp_2272/* : UnknownType */(x_4332/* : Interval_20 */, 0.5/* : Number_21 */)/* : Numerical_13 */; };
    static Contains_4366 = function (x_4341/* : Interval_20 */, value_4343/* : Numerical_13 */) /* : Boolean_24 */{ return LessThanOrEquals_2762/* : UnknownType */(Min_562/* : UnknownType */(x_4341/* : Interval_20 */)/* : Numerical_13 */, And_317/* : UnknownType */(value_4343/* : Numerical_13 */, LessThanOrEquals_2762/* : UnknownType */(value_4343/* : Numerical_13 */, Max_568/* : UnknownType */(x_4341/* : Interval_20 */)/* : Numerical_13 */)/* : Boolean_24 */)/* : Boolean_24 */)/* : Boolean_24 */; };
    static Contains_4396 = function (x_4368/* : Interval_20 */, other_4370/* : Interval_20 */) /* : Boolean_24 */{ return LessThanOrEquals_2762/* : UnknownType */(Min_562/* : UnknownType */(x_4368/* : Interval_20 */)/* : Numerical_13 */, And_317/* : UnknownType */(Min_562/* : UnknownType */(other_4370/* : Interval_20 */)/* : Numerical_13 */, GreaterThanOrEquals_2778/* : UnknownType */(Max_568/* : Function_3 */, Max_568/* : UnknownType */(other_4370/* : Interval_20 */)/* : Numerical_13 */)/* : Boolean_24 */)/* : Boolean_24 */)/* : Boolean_24 */; };
    static Overlaps_4413 = function (x_4398/* : Interval_20 */, y_4400/* : Interval_20 */) /* : Boolean_24 */{ return Not_333/* : UnknownType */(IsEmpty_2210/* : UnknownType */(Clamp_2394/* : UnknownType */(x_4398/* : Interval_20 */, y_4400/* : Interval_20 */)/* : Interval_20 */)/* : Boolean_24 */)/* : Boolean_24 */; };
    static Split_4434 = function (x_4415/* : Interval_20 */, t_4417/* : Unit_29 */) /* : Tuple_5 */{ return Tuple_1/* : Error_6 */(Left_2344/* : UnknownType */(x_4415/* : UnknownType */, t_4417/* : UnknownType */)/* : UnknownType */, Right_2352/* : UnknownType */(x_4415/* : UnknownType */, t_4417/* : UnknownType */)/* : UnknownType */)/* : Error_6 */; };
    static Split_4443 = function (x_4436/* : Interval_20 */) /* : Tuple_5 */{ return Split_2330/* : UnknownType */(x_4436/* : Interval_20 */, 0.5/* : Number_21 */)/* : Tuple_5 */; };
    static Left_4462 = function (x_4445/* : Interval_20 */, t_4447/* : Unit_29 */) /* : Interval_20 */{ return Tuple_1/* : Error_6 */(Min_562/* : UnknownType */(x_4445/* : UnknownType */)/* : UnknownType */, Lerp_2272/* : UnknownType */(x_4445/* : UnknownType */, t_4447/* : UnknownType */)/* : UnknownType */)/* : Error_6 */; };
    static Right_4481 = function (x_4464/* : Interval_20 */, t_4466/* : Unit_29 */) /* : Interval_20 */{ return Tuple_1/* : Error_6 */(Lerp_2272/* : UnknownType */(x_4464/* : UnknownType */, t_4466/* : UnknownType */)/* : UnknownType */, Max_568/* : UnknownType */(x_4464/* : UnknownType */)/* : UnknownType */)/* : Error_6 */; };
    static MoveTo_4500 = function (x_4483/* : Interval_20 */, v_4485/* : Numerical_13 */) /* : Interval_20 */{ return Tuple_1/* : Error_6 */(v_4485/* : UnknownType */, Add_225/* : UnknownType */(v_4485/* : UnknownType */, Size_1435/* : UnknownType */(x_4483/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Error_6 */; };
    static LeftHalf_4509 = function (x_4502/* : Interval_20 */) /* : Interval_20 */{ return Left_2344/* : UnknownType */(x_4502/* : Interval_20 */, 0.5/* : Number_21 */)/* : Interval_20 */; };
    static RightHalf_4518 = function (x_4511/* : Interval_20 */) /* : Interval_20 */{ return Right_2352/* : UnknownType */(x_4511/* : Interval_20 */, 0.5/* : Number_21 */)/* : Interval_20 */; };
    static HalfSize_4528 = function (x_4520/* : Interval_20 */) /* : Numerical_13 */{ return Half_2546/* : UnknownType */(Size_1435/* : UnknownType */(x_4520/* : Interval_20 */)/* : Numerical_13 */)/* : Number_21 */; };
    static Recenter_4555 = function (x_4530/* : Interval_20 */, c_4532/* : Numerical_13 */) /* : Interval_20 */{ return Tuple_1/* : Error_6 */(Subtract_233/* : UnknownType */(c_4532/* : UnknownType */, HalfSize_2380/* : UnknownType */(x_4530/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Add_225/* : UnknownType */(c_4532/* : UnknownType */, HalfSize_2380/* : UnknownType */(x_4530/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Error_6 */; };
    static Clamp_4582 = function (x_4557/* : Interval_20 */, y_4559/* : Interval_20 */) /* : Interval_20 */{ return Tuple_1/* : Error_6 */(Clamp_2394/* : UnknownType */(x_4557/* : UnknownType */, Min_562/* : UnknownType */(y_4559/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, Clamp_2394/* : UnknownType */(x_4557/* : UnknownType */, Max_568/* : UnknownType */(y_4559/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Error_6 */; };
    static Clamp_4616 = function (x_4584/* : Interval_20 */, value_4586/* : Numerical_13 */) /* : Numerical_13 */{ return LessThan_2754/* : UnknownType */(value_4586/* : Numerical_13 */, Min_562/* : UnknownType */(x_4584/* : UnknownType */)/* : UnknownType */
        ? Min_562/* : UnknownType */(x_4584/* : Interval_20 */)/* : Numerical_13 */
        : GreaterThan_2770/* : UnknownType */(value_4586/* : Numerical_13 */, Max_568/* : UnknownType */(x_4584/* : UnknownType */)/* : UnknownType */
            ? Max_568/* : UnknownType */(x_4584/* : Interval_20 */)/* : Numerical_13 */
            : value_4586/* : Numerical_13 */
        )/* : Boolean_24 */
    )/* : Boolean_24 */; };
    static Within_4643 = function (x_4618/* : Interval_20 */, value_4620/* : Numerical_13 */) /* : Boolean_24 */{ return GreaterThanOrEquals_2778/* : UnknownType */(value_4620/* : Numerical_13 */, And_317/* : UnknownType */(Min_562/* : UnknownType */(x_4618/* : Interval_20 */)/* : Numerical_13 */, LessThanOrEquals_2762/* : UnknownType */(value_4620/* : Numerical_13 */, Max_568/* : UnknownType */(x_4618/* : Interval_20 */)/* : Numerical_13 */)/* : Boolean_24 */)/* : Boolean_24 */)/* : Boolean_24 */; };
}
class Value_135_Library
{
    static ToString_4655 = function (x_4645/* : Value_9 */) /* : String_23 */{ return Join_2238/* : UnknownType */(FieldValues_347/* : UnknownType */(x_4645/* : Value_9 */)/* : Array_10 */, ", "/* : String_23 */)/* : String_23 */; };
}
class Vector_136_Library
{
    static Sum_4666 = function (v_4657/* : Array_10 */) /* : Number_21 */{ return Aggregate_2194/* : UnknownType */(v_4657/* : Array_10 */, 0/* : Integer_22 */, Add_225/* : Function_3 */)/* : Any_8 */; };
    static SumSquares_4680 = function (v_4668/* : Array_10 */) /* : Number_21 */{ return Aggregate_2194/* : UnknownType */(Square_2468/* : UnknownType */(v_4668/* : Array_10 */)/* : Number_21 */, 0/* : Integer_22 */, Add_225/* : Function_3 */)/* : Any_8 */; };
    static LengthSquared_4687 = function (v_4682/* : Array_10 */) /* : Number_21 */{ return SumSquares_2430/* : UnknownType */(v_4682/* : Array_10 */)/* : Number_21 */; };
    static Length_4697 = function (v_4689/* : Array_10 */) /* : Number_21 */{ return SquareRoot_2462/* : UnknownType */(LengthSquared_2436/* : UnknownType */(v_4689/* : Array_10 */)/* : Number_21 */)/* : Number_21 */; };
    static Dot_4711 = function (v1_4699/* : Vector_11 */, v2_4701/* : Vector_11 */) /* : Number_21 */{ return Sum_2424/* : UnknownType */(Multiply_249/* : UnknownType */(v1_4699/* : Vector_11 */, v2_4701/* : Vector_11 */)/* : Arithmetic_17 */)/* : Number_21 */; };
    static Normal_4723 = function (v_4713/* : Vector_11 */) /* : Vector_11 */{ return Divide_241/* : UnknownType */(v_4713/* : Vector_11 */, Length_2442/* : UnknownType */(v_4713/* : Vector_11 */)/* : Number_21 */)/* : ScalarArithmetic_18 */; };
}
class Numerical_137_Library
{
    static SquareRoot_4732 = function (x_4725/* : Number_21 */) /* : Number_21 */{ return Pow_179/* : UnknownType */(x_4725/* : Number_21 */, 0.5/* : Number_21 */)/* : Number_21 */; };
    static Square_4741 = function (x_4734/* : Number_21 */) /* : Number_21 */{ return Multiply_249/* : UnknownType */(x_4734/* : Number_21 */, x_4734/* : Number_21 */)/* : Number_21 */; };
    static Clamp_4755 = function (x_4743/* : Number_21 */) /* : Number_21 */{ return Clamp_2394/* : UnknownType */(x_4743/* : Number_21 */, Tuple_1/* : Error_6 */(0/* : UnknownType */, 1/* : UnknownType */)/* : Error_6 */)/* : Interval_20 */; };
    static PlusOne_4767 = function (x_4757/* : Number_21 */) /* : Number_21 */{ return Add_225/* : UnknownType */(x_4757/* : Number_21 */, One_416/* : UnknownType */(x_4757/* : Number_21 */)/* : Numerical_13 */)/* : Arithmetic_17 */; };
    static MinusOne_4779 = function (x_4769/* : Number_21 */) /* : Number_21 */{ return Subtract_233/* : UnknownType */(x_4769/* : Number_21 */, One_416/* : UnknownType */(x_4769/* : Number_21 */)/* : Numerical_13 */)/* : Number_21 */; };
    static FromOne_4791 = function (x_4781/* : Number_21 */) /* : Number_21 */{ return Subtract_233/* : UnknownType */(One_416/* : UnknownType */(x_4781/* : Number_21 */)/* : Numerical_13 */, x_4781/* : Number_21 */)/* : Number_21 */; };
    static IsPositive_4800 = function (x_4793/* : Number_21 */) /* : Boolean_24 */{ return GreaterThanOrEquals_2778/* : UnknownType */(x_4793/* : Number_21 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static GtZ_4809 = function (x_4802/* : Number_21 */) /* : Boolean_24 */{ return GreaterThan_2770/* : UnknownType */(x_4802/* : Number_21 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static LtZ_4818 = function (x_4811/* : Number_21 */) /* : Boolean_24 */{ return LessThan_2754/* : UnknownType */(x_4811/* : Number_21 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static GtEqZ_4827 = function (x_4820/* : Number_21 */) /* : Boolean_24 */{ return GreaterThanOrEquals_2778/* : UnknownType */(x_4820/* : Number_21 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static LtEqZ_4836 = function (x_4829/* : Number_21 */) /* : Boolean_24 */{ return LessThanOrEquals_2762/* : UnknownType */(x_4829/* : Number_21 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static IsNegative_4845 = function (x_4838/* : Number_21 */) /* : Boolean_24 */{ return LessThan_2754/* : UnknownType */(x_4838/* : Number_21 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static Sign_4873 = function (x_4847/* : Number_21 */) /* : Number_21 */{ return LtZ_2510/* : UnknownType */(x_4847/* : UnknownType */)/* : UnknownType */
        ? Negative_265/* : UnknownType */(One_416/* : UnknownType */(x_4847/* : Number_21 */)/* : Numerical_13 */)/* : Arithmetic_17 */
        : GtZ_2504/* : UnknownType */(x_4847/* : UnknownType */)/* : UnknownType */
            ? One_416/* : UnknownType */(x_4847/* : Number_21 */)/* : Numerical_13 */
            : Zero_410/* : UnknownType */(x_4847/* : Number_21 */)/* : Numerical_13 */

    ; };
    static Abs_4886 = function (x_4875/* : Number_21 */) /* : Number_21 */{ return LtZ_2510/* : UnknownType */(x_4875/* : UnknownType */)/* : UnknownType */
        ? Negative_265/* : UnknownType */(x_4875/* : Number_21 */)/* : Number_21 */
        : x_4875/* : Number_21 */
    ; };
    static Half_4895 = function (x_4888/* : Number_21 */) /* : Number_21 */{ return Divide_241/* : UnknownType */(x_4888/* : Number_21 */, 2/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Third_4904 = function (x_4897/* : Number_21 */) /* : Number_21 */{ return Divide_241/* : UnknownType */(x_4897/* : Number_21 */, 3/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Quarter_4913 = function (x_4906/* : Number_21 */) /* : Number_21 */{ return Divide_241/* : UnknownType */(x_4906/* : Number_21 */, 4/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Fifth_4922 = function (x_4915/* : Number_21 */) /* : Number_21 */{ return Divide_241/* : UnknownType */(x_4915/* : Number_21 */, 5/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Sixth_4931 = function (x_4924/* : Number_21 */) /* : Number_21 */{ return Divide_241/* : UnknownType */(x_4924/* : Number_21 */, 6/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Seventh_4940 = function (x_4933/* : Number_21 */) /* : Number_21 */{ return Divide_241/* : UnknownType */(x_4933/* : Number_21 */, 7/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Eighth_4949 = function (x_4942/* : Number_21 */) /* : Number_21 */{ return Divide_241/* : UnknownType */(x_4942/* : Number_21 */, 8/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Ninth_4958 = function (x_4951/* : Number_21 */) /* : Number_21 */{ return Divide_241/* : UnknownType */(x_4951/* : Number_21 */, 9/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Tenth_4967 = function (x_4960/* : Number_21 */) /* : Number_21 */{ return Divide_241/* : UnknownType */(x_4960/* : Number_21 */, 10/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Sixteenth_4976 = function (x_4969/* : Number_21 */) /* : Number_21 */{ return Divide_241/* : UnknownType */(x_4969/* : Number_21 */, 16/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Hundredth_4985 = function (x_4978/* : Number_21 */) /* : Number_21 */{ return Divide_241/* : UnknownType */(x_4978/* : Number_21 */, 100/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Thousandth_4994 = function (x_4987/* : Number_21 */) /* : Number_21 */{ return Divide_241/* : UnknownType */(x_4987/* : Number_21 */, 1000/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Millionth_5008 = function (x_4996/* : Number_21 */) /* : Number_21 */{ return Divide_241/* : UnknownType */(x_4996/* : Number_21 */, Divide_241/* : UnknownType */(1000/* : Integer_22 */, 1000/* : Integer_22 */)/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Billionth_5027 = function (x_5010/* : Number_21 */) /* : Number_21 */{ return Divide_241/* : UnknownType */(x_5010/* : Number_21 */, Divide_241/* : UnknownType */(1000/* : Integer_22 */, Divide_241/* : UnknownType */(1000/* : Integer_22 */, 1000/* : Integer_22 */)/* : Integer_22 */)/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Hundred_5036 = function (x_5029/* : Number_21 */) /* : Number_21 */{ return Multiply_249/* : UnknownType */(x_5029/* : Number_21 */, 100/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Thousand_5045 = function (x_5038/* : Number_21 */) /* : Number_21 */{ return Multiply_249/* : UnknownType */(x_5038/* : Number_21 */, 1000/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Million_5059 = function (x_5047/* : Number_21 */) /* : Number_21 */{ return Multiply_249/* : UnknownType */(x_5047/* : Number_21 */, Multiply_249/* : UnknownType */(1000/* : Integer_22 */, 1000/* : Integer_22 */)/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Billion_5078 = function (x_5061/* : Number_21 */) /* : Number_21 */{ return Multiply_249/* : UnknownType */(x_5061/* : Number_21 */, Multiply_249/* : UnknownType */(1000/* : Integer_22 */, Multiply_249/* : UnknownType */(1000/* : Integer_22 */, 1000/* : Integer_22 */)/* : Integer_22 */)/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Twice_5087 = function (x_5080/* : Number_21 */) /* : Number_21 */{ return Multiply_249/* : UnknownType */(x_5080/* : Number_21 */, 2/* : Integer_22 */)/* : Arithmetic_17 */; };
    static Thrice_5096 = function (x_5089/* : Number_21 */) /* : Number_21 */{ return Multiply_249/* : UnknownType */(x_5089/* : Number_21 */, 3/* : Integer_22 */)/* : Arithmetic_17 */; };
    static SmoothStep_5116 = function (x_5098/* : Number_21 */) /* : Number_21 */{ return Multiply_249/* : UnknownType */(Square_2468/* : UnknownType */(x_5098/* : Number_21 */)/* : Number_21 */, Subtract_233/* : UnknownType */(3/* : Integer_22 */, Twice_2654/* : UnknownType */(x_5098/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */; };
    static Pow2_5125 = function (x_5118/* : Number_21 */) /* : Number_21 */{ return Multiply_249/* : UnknownType */(x_5118/* : Number_21 */, x_5118/* : Number_21 */)/* : Number_21 */; };
    static Pow3_5137 = function (x_5127/* : Number_21 */) /* : Number_21 */{ return Multiply_249/* : UnknownType */(Pow2_2672/* : UnknownType */(x_5127/* : Number_21 */)/* : Number_21 */, x_5127/* : Number_21 */)/* : Number_21 */; };
    static Pow4_5149 = function (x_5139/* : Number_21 */) /* : Number_21 */{ return Multiply_249/* : UnknownType */(Pow3_2678/* : UnknownType */(x_5139/* : Number_21 */)/* : Number_21 */, x_5139/* : Number_21 */)/* : Number_21 */; };
    static Pow5_5161 = function (x_5151/* : Number_21 */) /* : Number_21 */{ return Multiply_249/* : UnknownType */(Pow4_2684/* : UnknownType */(x_5151/* : Number_21 */)/* : Number_21 */, x_5151/* : Number_21 */)/* : Number_21 */; };
    static Pi_5165 = function (self_5163/* : Numerical_137 */) /* : Number_21 */{ return 3.1415926535897/* : Number_21 */; };
    static AlmostZero_5177 = function (x_5167/* : Number_21 */) /* : Boolean_24 */{ return LessThan_2754/* : UnknownType */(Abs_2540/* : UnknownType */(x_5167/* : Number_21 */)/* : Number_21 */, 1E-08/* : Number_21 */)/* : Boolean_24 */; };
    static Lerp_5205 = function (a_5179/* : Number_21 */, b_5181/* : Number_21 */, t_5183/* : Unit_29 */) /* : Number_21 */{ return Multiply_249/* : UnknownType */(Subtract_233/* : UnknownType */(1/* : Integer_22 */, t_5183/* : Unit_29 */)/* : Number_21 */, Add_225/* : UnknownType */(a_5179/* : Number_21 */, Multiply_249/* : UnknownType */(t_5183/* : Unit_29 */, b_5181/* : Number_21 */)/* : Arithmetic_17 */)/* : Arithmetic_17 */)/* : Arithmetic_17 */; };
    static Between_5246 = function (self_5207/* : Number_21 */, min_5209/* : Number_21 */, max_5211/* : Number_21 */) /* : Boolean_24 */{ return Zip_2156/* : UnknownType */(FieldValues_347/* : UnknownType */(self_5207/* : Number_21 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(min_5209/* : Number_21 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(max_5211/* : Number_21 */)/* : Array_10 */, function (x_5228/* : UnknownType */, y_5230/* : UnknownType */, z_5232/* : UnknownType */) /* : Lambda_2 */{ return Between_2718/* : UnknownType */(x_5228/* : UnknownType */, y_5230/* : UnknownType */, z_5232/* : UnknownType */)/* : UnknownType */; })/* : Array_10 */; };
}
class Angles_138_Library
{
    static Radians_5250 = function (x_5248/* : Number_21 */) /* : Angle_82 */{ return x_5248/* : Number_21 */; };
    static Degrees_5264 = function (x_5252/* : Number_21 */) /* : Angle_82 */{ return Multiply_249/* : UnknownType */(x_5252/* : Number_21 */, Divide_241/* : UnknownType */(Pi_2696/* : Function_3 */, 180/* : Integer_22 */)/* : Number_21 */)/* : Number_21 */; };
    static Turns_5278 = function (x_5266/* : Number_21 */) /* : Angle_82 */{ return Multiply_249/* : UnknownType */(x_5266/* : Number_21 */, Multiply_249/* : UnknownType */(2/* : Integer_22 */, Pi_2696/* : Function_3 */)/* : Number_21 */)/* : Number_21 */; };
}
class Comparable_139_Library
{
    static Equals_5294 = function (a_5280/* : Comparable_15 */, b_5282/* : Comparable_15 */) /* : Boolean_24 */{ return Equals_446/* : UnknownType */(Compare_440/* : UnknownType */(a_5280/* : UnknownType */, b_5282/* : UnknownType */)/* : Error_6 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static LessThan_5310 = function (a_5296/* : Comparable_15 */, b_5298/* : Comparable_15 */) /* : Boolean_24 */{ return LessThan_2754/* : UnknownType */(Compare_440/* : UnknownType */(a_5296/* : UnknownType */, b_5298/* : UnknownType */)/* : Error_6 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static LessThanOrEquals_5326 = function (a_5312/* : Comparable_15 */, b_5314/* : Comparable_15 */) /* : Boolean_24 */{ return LessThanOrEquals_2762/* : UnknownType */(Compare_440/* : UnknownType */(a_5312/* : UnknownType */, b_5314/* : UnknownType */)/* : Error_6 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static GreaterThan_5342 = function (a_5328/* : Comparable_15 */, b_5330/* : Comparable_15 */) /* : Boolean_24 */{ return GreaterThan_2770/* : UnknownType */(Compare_440/* : UnknownType */(a_5328/* : UnknownType */, b_5330/* : UnknownType */)/* : Error_6 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static GreaterThanOrEquals_5358 = function (a_5344/* : Comparable_15 */, b_5346/* : Comparable_15 */) /* : Boolean_24 */{ return GreaterThanOrEquals_2778/* : UnknownType */(Compare_440/* : UnknownType */(a_5344/* : UnknownType */, b_5346/* : UnknownType */)/* : Error_6 */, 0/* : Integer_22 */)/* : Boolean_24 */; };
    static Between_5381 = function (v_5360/* : Comparable_15 */, a_5362/* : Comparable_15 */, b_5364/* : Comparable_15 */) /* : Value_9 */{ return GreaterThanOrEquals_2778/* : UnknownType */(v_5360/* : Comparable_15 */, And_317/* : UnknownType */(a_5362/* : Comparable_15 */, LessThanOrEquals_2762/* : UnknownType */(v_5360/* : Comparable_15 */, b_5364/* : Comparable_15 */)/* : Boolean_24 */)/* : Boolean_24 */)/* : Boolean_24 */; };
    static Between_5392 = function (v_5383/* : Value_9 */, i_5385/* : Interval_20 */) /* : Interval_20 */{ return Contains_2306/* : UnknownType */(i_5385/* : Interval_20 */, v_5383/* : Value_9 */)/* : Boolean_24 */; };
    static Min_5406 = function (a_5394/* : Comparable_15 */, b_5396/* : Comparable_15 */) /* : Comparable_15 */{ return LessThanOrEquals_2762/* : UnknownType */(a_5394/* : Comparable_15 */, b_5396/* : UnknownType */
        ? a_5394/* : Comparable_15 */
        : b_5396/* : Comparable_15 */
    )/* : Boolean_24 */; };
    static Max_5420 = function (a_5408/* : Comparable_15 */, b_5410/* : Comparable_15 */) /* : Comparable_15 */{ return GreaterThanOrEquals_2778/* : UnknownType */(a_5408/* : Comparable_15 */, b_5410/* : UnknownType */
        ? a_5408/* : Comparable_15 */
        : b_5410/* : Comparable_15 */
    )/* : Boolean_24 */; };
}
class Equatable_140_Library
{
    static NotEquals_5434 = function (x_5422/* : Equatable_16 */, y_5424/* : Equatable_16 */) /* : Boolean_24 */{ return Not_333/* : UnknownType */(Equals_446/* : UnknownType */(x_5422/* : Equatable_16 */, y_5424/* : Equatable_16 */)/* : Boolean_24 */)/* : Boolean_24 */; };
}
class Easings_141_Library
{
    static BlendEaseFunc_5486 = function (p_5436/* : Number_21 */, easeIn_5438/* : Function_3 */, easeOut_5440/* : Function_3 */) /* : Number_21 */{ return LessThan_2754/* : UnknownType */(p_5436/* : Number_21 */, 0.5/* : UnknownType */
        ? Multiply_249/* : UnknownType */(0.5/* : Number_21 */, easeIn_5438/* : Function_3 */(Multiply_249/* : UnknownType */(p_5436/* : UnknownType */, 2/* : UnknownType */)/* : UnknownType */)/* : Function_3 */)/* : Number_21 */
        : Multiply_249/* : UnknownType */(0.5/* : Number_21 */, Add_225/* : UnknownType */(easeOut_5440/* : Function_3 */(Multiply_249/* : UnknownType */(p_5436/* : UnknownType */, Subtract_233/* : UnknownType */(2/* : UnknownType */, 1/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Function_3 */, 0.5/* : Number_21 */)/* : Number_21 */)/* : Number_21 */
    )/* : Boolean_24 */; };
    static InvertEaseFunc_5505 = function (p_5488/* : Number_21 */, easeIn_5490/* : Function_3 */) /* : Number_21 */{ return Subtract_233/* : UnknownType */(1/* : Integer_22 */, easeIn_5490/* : Function_3 */(Subtract_233/* : UnknownType */(1/* : UnknownType */, p_5488/* : UnknownType */)/* : UnknownType */)/* : Function_3 */)/* : Number_21 */; };
    static Linear_5509 = function (p_5507/* : Number_21 */) /* : Number_21 */{ return p_5507/* : Number_21 */; };
    static QuadraticEaseIn_5516 = function (p_5511/* : Number_21 */) /* : Number_21 */{ return Pow2_2672/* : UnknownType */(p_5511/* : Number_21 */)/* : Number_21 */; };
    static QuadraticEaseOut_5525 = function (p_5518/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2838/* : UnknownType */(p_5518/* : Number_21 */, QuadraticEaseIn_2852/* : Function_3 */)/* : Number_21 */; };
    static QuadraticEaseInOut_5536 = function (p_5527/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2828/* : UnknownType */(p_5527/* : Number_21 */, QuadraticEaseIn_2852/* : Function_3 */, QuadraticEaseOut_2858/* : Function_3 */)/* : Number_21 */; };
    static CubicEaseIn_5543 = function (p_5538/* : Number_21 */) /* : Number_21 */{ return Pow3_2678/* : UnknownType */(p_5538/* : Number_21 */)/* : Number_21 */; };
    static CubicEaseOut_5552 = function (p_5545/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2838/* : UnknownType */(p_5545/* : Number_21 */, CubicEaseIn_2870/* : Function_3 */)/* : Number_21 */; };
    static CubicEaseInOut_5563 = function (p_5554/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2828/* : UnknownType */(p_5554/* : Number_21 */, CubicEaseIn_2870/* : Function_3 */, CubicEaseOut_2876/* : Function_3 */)/* : Number_21 */; };
    static QuarticEaseIn_5570 = function (p_5565/* : Number_21 */) /* : Number_21 */{ return Pow4_2684/* : UnknownType */(p_5565/* : Number_21 */)/* : Number_21 */; };
    static QuarticEaseOut_5579 = function (p_5572/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2838/* : UnknownType */(p_5572/* : Number_21 */, QuarticEaseIn_2888/* : Function_3 */)/* : Number_21 */; };
    static QuarticEaseInOut_5590 = function (p_5581/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2828/* : UnknownType */(p_5581/* : Number_21 */, QuarticEaseIn_2888/* : Function_3 */, QuarticEaseOut_2894/* : Function_3 */)/* : Number_21 */; };
    static QuinticEaseIn_5597 = function (p_5592/* : Number_21 */) /* : Number_21 */{ return Pow5_2690/* : UnknownType */(p_5592/* : Number_21 */)/* : Number_21 */; };
    static QuinticEaseOut_5606 = function (p_5599/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2838/* : UnknownType */(p_5599/* : Number_21 */, QuinticEaseIn_2906/* : Function_3 */)/* : Number_21 */; };
    static QuinticEaseInOut_5617 = function (p_5608/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2828/* : UnknownType */(p_5608/* : Number_21 */, QuinticEaseIn_2906/* : Function_3 */, QuinticEaseOut_2912/* : Function_3 */)/* : Number_21 */; };
    static SineEaseIn_5626 = function (p_5619/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2838/* : UnknownType */(p_5619/* : Number_21 */, SineEaseOut_2930/* : Function_3 */)/* : Number_21 */; };
    static SineEaseOut_5639 = function (p_5628/* : Number_21 */) /* : Number_21 */{ return Sin_149/* : UnknownType */(Turns_2740/* : UnknownType */(Quarter_2558/* : UnknownType */(p_5628/* : Number_21 */)/* : Number_21 */)/* : Angle_82 */)/* : Number_21 */; };
    static SineEaseInOut_5650 = function (p_5641/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2828/* : UnknownType */(p_5641/* : Number_21 */, SineEaseIn_2924/* : Function_3 */, SineEaseOut_2930/* : Function_3 */)/* : Number_21 */; };
    static CircularEaseIn_5666 = function (p_5652/* : Number_21 */) /* : Number_21 */{ return FromOne_2492/* : UnknownType */(SquareRoot_2462/* : UnknownType */(FromOne_2492/* : UnknownType */(Pow2_2672/* : UnknownType */(p_5652/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */; };
    static CircularEaseOut_5675 = function (p_5668/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2838/* : UnknownType */(p_5668/* : Number_21 */, CircularEaseIn_2942/* : Function_3 */)/* : Number_21 */; };
    static CircularEaseInOut_5686 = function (p_5677/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2828/* : UnknownType */(p_5677/* : Number_21 */, CircularEaseIn_2942/* : Function_3 */, CircularEaseOut_2948/* : Function_3 */)/* : Number_21 */; };
    static ExponentialEaseIn_5709 = function (p_5688/* : Number_21 */) /* : Number_21 */{ return AlmostZero_2702/* : UnknownType */(p_5688/* : UnknownType */)/* : UnknownType */
        ? p_5688/* : Number_21 */
        : Pow_179/* : UnknownType */(2/* : Integer_22 */, Multiply_249/* : UnknownType */(10/* : Integer_22 */, MinusOne_2486/* : UnknownType */(p_5688/* : Number_21 */)/* : Number_21 */)/* : Arithmetic_17 */)/* : Number_21 */
    ; };
    static ExponentialEaseOut_5718 = function (p_5711/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2838/* : UnknownType */(p_5711/* : Number_21 */, ExponentialEaseIn_2960/* : Function_3 */)/* : Number_21 */; };
    static ExponentialEaseInOut_5729 = function (p_5720/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2828/* : UnknownType */(p_5720/* : Number_21 */, ExponentialEaseIn_2960/* : Function_3 */, ExponentialEaseOut_2966/* : Function_3 */)/* : Number_21 */; };
    static ElasticEaseIn_5768 = function (p_5731/* : Number_21 */) /* : Number_21 */{ return Multiply_249/* : UnknownType */(13/* : Integer_22 */, Multiply_249/* : UnknownType */(Turns_2740/* : UnknownType */(Quarter_2558/* : UnknownType */(p_5731/* : Number_21 */)/* : Number_21 */)/* : Angle_82 */, Sin_149/* : UnknownType */(Radians_1463/* : UnknownType */(Pow_179/* : UnknownType */(2/* : Integer_22 */, Multiply_249/* : UnknownType */(10/* : Integer_22 */, MinusOne_2486/* : UnknownType */(p_5731/* : Number_21 */)/* : Number_21 */)/* : Arithmetic_17 */)/* : Number_21 */)/* : Angle_82 */)/* : Number_21 */)/* : ScalarArithmetic_18 */)/* : Number_21 */; };
    static ElasticEaseOut_5777 = function (p_5770/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2838/* : UnknownType */(p_5770/* : Number_21 */, ElasticEaseIn_2978/* : Function_3 */)/* : Number_21 */; };
    static ElasticEaseInOut_5788 = function (p_5779/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2828/* : UnknownType */(p_5779/* : Number_21 */, ElasticEaseIn_2978/* : Function_3 */, ElasticEaseOut_2984/* : Function_3 */)/* : Number_21 */; };
    static BackEaseIn_5814 = function (p_5790/* : Number_21 */) /* : Number_21 */{ return Subtract_233/* : UnknownType */(Pow3_2678/* : UnknownType */(p_5790/* : Number_21 */)/* : Number_21 */, Multiply_249/* : UnknownType */(p_5790/* : Number_21 */, Sin_149/* : UnknownType */(Turns_2740/* : UnknownType */(Half_2546/* : UnknownType */(p_5790/* : Number_21 */)/* : Number_21 */)/* : Angle_82 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */; };
    static BackEaseOut_5823 = function (p_5816/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2838/* : UnknownType */(p_5816/* : Number_21 */, BackEaseIn_2996/* : Function_3 */)/* : Number_21 */; };
    static BackEaseInOut_5834 = function (p_5825/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2828/* : UnknownType */(p_5825/* : Number_21 */, BackEaseIn_2996/* : Function_3 */, BackEaseOut_3002/* : Function_3 */)/* : Number_21 */; };
    static BounceEaseIn_5843 = function (p_5836/* : Number_21 */) /* : Number_21 */{ return InvertEaseFunc_2838/* : UnknownType */(p_5836/* : Number_21 */, BounceEaseOut_3020/* : Function_3 */)/* : Number_21 */; };
    static BounceEaseOut_6013 = function (p_5845/* : Number_21 */) /* : Number_21 */{ return LessThan_2754/* : UnknownType */(p_5845/* : UnknownType */, Divide_241/* : UnknownType */(4/* : UnknownType */, 11/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
        ? Multiply_249/* : UnknownType */(121/* : Number_21 */, Divide_241/* : UnknownType */(Pow2_2672/* : UnknownType */(p_5845/* : Number_21 */)/* : Number_21 */, 16/* : Number_21 */)/* : Number_21 */)/* : Number_21 */
        : LessThan_2754/* : UnknownType */(p_5845/* : UnknownType */, Divide_241/* : UnknownType */(8/* : UnknownType */, 11/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
            ? Divide_241/* : UnknownType */(363/* : Number_21 */, Multiply_249/* : UnknownType */(40/* : Number_21 */, Subtract_233/* : UnknownType */(Pow2_2672/* : UnknownType */(p_5845/* : Number_21 */)/* : Number_21 */, Divide_241/* : UnknownType */(99/* : Number_21 */, Multiply_249/* : UnknownType */(10/* : Number_21 */, Add_225/* : UnknownType */(p_5845/* : Number_21 */, Divide_241/* : UnknownType */(17/* : Number_21 */, 5/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */
            : LessThan_2754/* : UnknownType */(p_5845/* : UnknownType */, Divide_241/* : UnknownType */(9/* : UnknownType */, 10/* : UnknownType */)/* : UnknownType */)/* : UnknownType */
                ? Divide_241/* : UnknownType */(4356/* : Number_21 */, Multiply_249/* : UnknownType */(361/* : Number_21 */, Subtract_233/* : UnknownType */(Pow2_2672/* : UnknownType */(p_5845/* : Number_21 */)/* : Number_21 */, Divide_241/* : UnknownType */(35442/* : Number_21 */, Multiply_249/* : UnknownType */(1805/* : Number_21 */, Add_225/* : UnknownType */(p_5845/* : Number_21 */, Divide_241/* : UnknownType */(16061/* : Number_21 */, 1805/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */
                : Divide_241/* : UnknownType */(54/* : Number_21 */, Multiply_249/* : UnknownType */(5/* : Number_21 */, Subtract_233/* : UnknownType */(Pow2_2672/* : UnknownType */(p_5845/* : Number_21 */)/* : Number_21 */, Divide_241/* : UnknownType */(513/* : Number_21 */, Multiply_249/* : UnknownType */(25/* : Number_21 */, Add_225/* : UnknownType */(p_5845/* : Number_21 */, Divide_241/* : UnknownType */(268/* : Number_21 */, 25/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */


    ; };
    static BounceEaseInOut_6024 = function (p_6015/* : Number_21 */) /* : Number_21 */{ return BlendEaseFunc_2828/* : UnknownType */(p_6015/* : Number_21 */, BounceEaseIn_3014/* : Function_3 */, BounceEaseOut_3020/* : Function_3 */)/* : Number_21 */; };
}
class Any_8_Concept
{
    constructor(self) { this.Self = self; };
    static FieldNames_3147 = function (self_3146/* : Any_8 */) /* : Array_10 */{ return null; };
    static FieldValues_3150 = function (x_3149/* : Any_8 */) /* : Array_10 */{ return null; };
    static FieldTypes_3153 = function (x_3152/* : Any_8 */) /* : Array_10 */{ return null; };
    static TypeOf_3156 = function (self_3155/* : Any_8 */) /* : Type_25 */{ return null; };
}
class Value_9_Concept
{
    constructor(self) { this.Self = self; };
    static Default_3162 = function (self_3161/* : Value_9 */) /* : Value_9 */{ return null; };
}
class Array_10_Concept
{
    constructor(self) { this.Self = self; };
    static Count_3168 = function (xs_3167/* : Array_10 */) /* : Integer_22 */{ return null; };
    static At_3173 = function (xs_3170/* : Array_10 */, n_3172/* : Integer_22 */) /* : Any_8 */{ return null; };
}
class Vector_11_Concept
{
    constructor(self) { this.Self = self; };
    static Count_3189 = function (v_3181/* : Vector_11 */) /* : Integer_22 */{ return Count_374/* : UnknownType */(FieldTypes_354/* : UnknownType */(Self_3179/* : Vector_11 */)/* : Array_10 */)/* : Integer_22 */; };
    static At_3203 = function (v_3191/* : Vector_11 */, n_3193/* : Integer_22 */) /* : Numerical_13 */{ return At_380/* : UnknownType */(FieldValues_347/* : UnknownType */(v_3191/* : Vector_11 */)/* : Array_10 */, n_3193/* : Integer_22 */)/* : Any_8 */; };
}
class Measure_12_Concept
{
    constructor(self) { this.Self = self; };
    static Value_3222 = function (x_3212/* : Measure_12 */) /* : Number_21 */{ return At_380/* : UnknownType */(FieldValues_347/* : UnknownType */(x_3212/* : Measure_12 */)/* : Array_10 */, 0/* : Integer_22 */)/* : Any_8 */; };
}
class Numerical_13_Concept
{
    constructor(self) { this.Self = self; };
    static Zero_3232 = function (x_3231/* : Numerical_13 */) /* : Numerical_13 */{ return null; };
    static One_3235 = function (x_3234/* : Numerical_13 */) /* : Numerical_13 */{ return null; };
    static MinValue_3238 = function (x_3237/* : Numerical_13 */) /* : Numerical_13 */{ return null; };
    static MaxValue_3241 = function (x_3240/* : Numerical_13 */) /* : Numerical_13 */{ return null; };
}
class Magnitudinal_14_Concept
{
    constructor(self) { this.Self = self; };
    static Magnitude_3260 = function (x_3246/* : Magnitudinal_14 */) /* : Number_21 */{ return SquareRoot_2462/* : UnknownType */(Sum_2424/* : UnknownType */(Square_2468/* : UnknownType */(FieldValues_347/* : UnknownType */(x_3246/* : Magnitudinal_14 */)/* : Array_10 */)/* : Number_21 */)/* : Number_21 */)/* : Number_21 */; };
}
class Comparable_15_Concept
{
    constructor(self) { this.Self = self; };
    static Compare_3266 = function (x_3265/* : Comparable_15 */) /* : Integer_22 */{ return null; };
}
class Equatable_16_Concept
{
    constructor(self) { this.Self = self; };
    static Equals_3289 = function (a_3271/* : Equatable_16 */, b_3273/* : Equatable_16 */) /* : Boolean_24 */{ return All_2246/* : UnknownType */(Equals_446/* : UnknownType */(FieldValues_347/* : UnknownType */(a_3271/* : Equatable_16 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(b_3273/* : Equatable_16 */)/* : Array_10 */)/* : Boolean_24 */)/* : Boolean_24 */; };
}
class Arithmetic_17_Concept
{
    constructor(self) { this.Self = self; };
    static Add_3309 = function (self_3294/* : Arithmetic_17 */, other_3296/* : Arithmetic_17 */) /* : Arithmetic_17 */{ return Add_225/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3294/* : Arithmetic_17 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(other_3296/* : Arithmetic_17 */)/* : Array_10 */)/* : Number_21 */; };
    static Negative_3319 = function (self_3311/* : Arithmetic_17 */) /* : Arithmetic_17 */{ return Negative_265/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3311/* : Arithmetic_17 */)/* : Array_10 */)/* : Arithmetic_17 */; };
    static Reciprocal_3329 = function (self_3321/* : Arithmetic_17 */) /* : Arithmetic_17 */{ return Reciprocal_468/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3321/* : Arithmetic_17 */)/* : Array_10 */)/* : Arithmetic_17 */; };
    static Multiply_3346 = function (self_3331/* : Arithmetic_17 */, other_3333/* : Arithmetic_17 */) /* : Arithmetic_17 */{ return Add_225/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3331/* : Arithmetic_17 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(other_3333/* : Arithmetic_17 */)/* : Array_10 */)/* : Number_21 */; };
    static Divide_3363 = function (self_3348/* : Arithmetic_17 */, other_3350/* : Arithmetic_17 */) /* : Arithmetic_17 */{ return Divide_241/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3348/* : Arithmetic_17 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(other_3350/* : Arithmetic_17 */)/* : Array_10 */)/* : Number_21 */; };
    static Modulo_3380 = function (self_3365/* : Arithmetic_17 */, other_3367/* : Arithmetic_17 */) /* : Arithmetic_17 */{ return Modulo_257/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3365/* : Arithmetic_17 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(other_3367/* : Arithmetic_17 */)/* : Array_10 */)/* : Number_21 */; };
}
class ScalarArithmetic_18_Concept
{
    constructor(self) { this.Self = self; };
    static Add_3397 = function (self_3385/* : ScalarArithmetic_18 */, scalar_3387/* : Number_21 */) /* : ScalarArithmetic_18 */{ return Add_225/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3385/* : ScalarArithmetic_18 */)/* : Array_10 */, scalar_3387/* : Number_21 */)/* : ScalarArithmetic_18 */; };
    static Subtract_3411 = function (self_3399/* : ScalarArithmetic_18 */, scalar_3401/* : Number_21 */) /* : ScalarArithmetic_18 */{ return Add_225/* : UnknownType */(self_3399/* : ScalarArithmetic_18 */, Negative_265/* : UnknownType */(scalar_3401/* : Number_21 */)/* : Number_21 */)/* : ScalarArithmetic_18 */; };
    static Multiply_3425 = function (self_3413/* : ScalarArithmetic_18 */, scalar_3415/* : Number_21 */) /* : ScalarArithmetic_18 */{ return Multiply_249/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3413/* : ScalarArithmetic_18 */)/* : Array_10 */, scalar_3415/* : Number_21 */)/* : ScalarArithmetic_18 */; };
    static Divide_3439 = function (self_3427/* : ScalarArithmetic_18 */, scalar_3429/* : Number_21 */) /* : ScalarArithmetic_18 */{ return Multiply_249/* : UnknownType */(self_3427/* : ScalarArithmetic_18 */, Reciprocal_468/* : UnknownType */(scalar_3429/* : Number_21 */)/* : Arithmetic_17 */)/* : Number_21 */; };
    static Modulo_3453 = function (self_3441/* : ScalarArithmetic_18 */, scalar_3443/* : Number_21 */) /* : ScalarArithmetic_18 */{ return Modulo_257/* : UnknownType */(FieldValues_347/* : UnknownType */(self_3441/* : ScalarArithmetic_18 */)/* : Array_10 */, scalar_3443/* : Number_21 */)/* : ScalarArithmetic_18 */; };
}
class BooleanOperations_19_Concept
{
    constructor(self) { this.Self = self; };
    static And_3472 = function (a_3457/* : BooleanOperations_19 */, b_3459/* : BooleanOperations_19 */) /* : BooleanOperations_19 */{ return And_317/* : UnknownType */(FieldValues_347/* : UnknownType */(a_3457/* : BooleanOperations_19 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(b_3459/* : BooleanOperations_19 */)/* : Array_10 */)/* : BooleanOperations_19 */; };
    static Or_3489 = function (a_3474/* : BooleanOperations_19 */, b_3476/* : BooleanOperations_19 */) /* : BooleanOperations_19 */{ return Or_325/* : UnknownType */(FieldValues_347/* : UnknownType */(a_3474/* : BooleanOperations_19 */)/* : Array_10 */, FieldValues_347/* : UnknownType */(b_3476/* : BooleanOperations_19 */)/* : Array_10 */)/* : BooleanOperations_19 */; };
    static Not_3499 = function (a_3491/* : BooleanOperations_19 */) /* : BooleanOperations_19 */{ return Not_333/* : UnknownType */(FieldValues_347/* : UnknownType */(a_3491/* : BooleanOperations_19 */)/* : Array_10 */)/* : BooleanOperations_19 */; };
}
class Interval_20_Concept
{
    constructor(self) { this.Self = self; };
    static Min_3506 = function (x_3505/* : Interval_20 */) /* : Numerical_13 */{ return null; };
    static Max_3509 = function (x_3508/* : Interval_20 */) /* : Numerical_13 */{ return null; };
}
class Number_21_Type
{
    constructor()
    {
        // field initialization 
        this.Zero_3232 = Number_21_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = Number_21_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = Number_21_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = Number_21_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = Number_21_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Number_21_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Number_21_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Number_21_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Number_21_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = Number_21_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = Number_21_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = Number_21_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = Number_21_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = Number_21_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = Number_21_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = Number_21_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Number_21_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Number_21_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Number_21_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Number_21_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Number_21_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Number_21_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Number_21_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Number_21_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Number_21_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Number_21_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Number_21_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Number_21_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Number_21_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Number_21_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Number_21_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Number_21_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Number_21_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Number_21_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Number_21_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Number_21_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Number_21_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Number_21_Type.Any_8_Concept.TypeOf_3156;
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
        this.Zero_3232 = Integer_22_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = Integer_22_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = Integer_22_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = Integer_22_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = Integer_22_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Integer_22_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Integer_22_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Integer_22_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Integer_22_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = Integer_22_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = Integer_22_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = Integer_22_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = Integer_22_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = Integer_22_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = Integer_22_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = Integer_22_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Integer_22_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Integer_22_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Integer_22_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Integer_22_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Integer_22_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Integer_22_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Integer_22_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Integer_22_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Integer_22_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Integer_22_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Integer_22_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Integer_22_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Integer_22_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Integer_22_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Integer_22_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Integer_22_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Integer_22_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Integer_22_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Integer_22_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Integer_22_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Integer_22_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Integer_22_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = String_23_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = String_23_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = String_23_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = String_23_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = String_23_Type.Any_8_Concept.TypeOf_3156;
        this.Count_3168 = String_23_Type.Array_10_Concept.Count_3168;
        this.At_3173 = String_23_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = String_23_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = String_23_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = String_23_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = String_23_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Boolean_24_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Boolean_24_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Boolean_24_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Boolean_24_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Boolean_24_Type.Any_8_Concept.TypeOf_3156;
        this.And_3472 = Boolean_24_Type.BooleanOperations_19_Concept.And_3472;
        this.Or_3489 = Boolean_24_Type.BooleanOperations_19_Concept.Or_3489;
        this.Not_3499 = Boolean_24_Type.BooleanOperations_19_Concept.Not_3499;
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
        this.Default_3162 = Type_25_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Type_25_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Type_25_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Type_25_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Type_25_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Character_26_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Character_26_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Character_26_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Character_26_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Character_26_Type.Any_8_Concept.TypeOf_3156;
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
        this.Zero_3232 = Count_27_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = Count_27_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = Count_27_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = Count_27_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = Count_27_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Count_27_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Count_27_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Count_27_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Count_27_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = Count_27_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = Count_27_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = Count_27_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = Count_27_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = Count_27_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = Count_27_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = Count_27_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Count_27_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Count_27_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Count_27_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Count_27_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Count_27_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Count_27_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Count_27_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Count_27_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Count_27_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Count_27_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Count_27_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Count_27_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Count_27_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Count_27_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Count_27_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Count_27_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Count_27_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Count_27_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Count_27_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Count_27_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Count_27_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Count_27_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Index_28_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Index_28_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Index_28_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Index_28_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Index_28_Type.Any_8_Concept.TypeOf_3156;
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
        this.Zero_3232 = Unit_29_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = Unit_29_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = Unit_29_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = Unit_29_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = Unit_29_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Unit_29_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Unit_29_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Unit_29_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Unit_29_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = Unit_29_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = Unit_29_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = Unit_29_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = Unit_29_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = Unit_29_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = Unit_29_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = Unit_29_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Unit_29_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Unit_29_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Unit_29_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Unit_29_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Unit_29_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Unit_29_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Unit_29_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Unit_29_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Unit_29_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Unit_29_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Unit_29_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Unit_29_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Unit_29_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Unit_29_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Unit_29_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Unit_29_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Unit_29_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Unit_29_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Unit_29_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Unit_29_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Unit_29_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Unit_29_Type.Any_8_Concept.TypeOf_3156;
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
        this.Zero_3232 = Percent_30_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = Percent_30_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = Percent_30_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = Percent_30_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = Percent_30_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Percent_30_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Percent_30_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Percent_30_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Percent_30_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = Percent_30_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = Percent_30_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = Percent_30_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = Percent_30_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = Percent_30_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = Percent_30_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = Percent_30_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Percent_30_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Percent_30_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Percent_30_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Percent_30_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Percent_30_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Percent_30_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Percent_30_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Percent_30_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Percent_30_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Percent_30_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Percent_30_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Percent_30_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Percent_30_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Percent_30_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Percent_30_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Percent_30_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Percent_30_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Percent_30_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Percent_30_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Percent_30_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Percent_30_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Percent_30_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Quaternion_31_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Quaternion_31_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Quaternion_31_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Quaternion_31_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Quaternion_31_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Unit2D_32_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Unit2D_32_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Unit2D_32_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Unit2D_32_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Unit2D_32_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Unit3D_33_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Unit3D_33_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Unit3D_33_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Unit3D_33_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Unit3D_33_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Direction3D_34_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Direction3D_34_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Direction3D_34_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Direction3D_34_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Direction3D_34_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = AxisAngle_35_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AxisAngle_35_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AxisAngle_35_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AxisAngle_35_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AxisAngle_35_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = EulerAngles_36_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = EulerAngles_36_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = EulerAngles_36_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = EulerAngles_36_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = EulerAngles_36_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Rotation3D_37_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Rotation3D_37_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Rotation3D_37_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Rotation3D_37_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Rotation3D_37_Type.Any_8_Concept.TypeOf_3156;
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
        this.Count_3189 = Vector2D_38_Type.Vector_11_Concept.Count_3189;
        this.At_3203 = Vector2D_38_Type.Vector_11_Concept.At_3203;
        this.Count_3168 = Vector2D_38_Type.Array_10_Concept.Count_3168;
        this.At_3173 = Vector2D_38_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = Vector2D_38_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector2D_38_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector2D_38_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector2D_38_Type.Any_8_Concept.TypeOf_3156;
        this.Zero_3232 = Vector2D_38_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = Vector2D_38_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = Vector2D_38_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = Vector2D_38_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = Vector2D_38_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector2D_38_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector2D_38_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector2D_38_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector2D_38_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = Vector2D_38_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = Vector2D_38_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = Vector2D_38_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = Vector2D_38_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = Vector2D_38_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = Vector2D_38_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = Vector2D_38_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector2D_38_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector2D_38_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector2D_38_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector2D_38_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Vector2D_38_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Vector2D_38_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector2D_38_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector2D_38_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector2D_38_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector2D_38_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Vector2D_38_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Vector2D_38_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector2D_38_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector2D_38_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector2D_38_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector2D_38_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Vector2D_38_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Vector2D_38_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector2D_38_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector2D_38_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector2D_38_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector2D_38_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Vector2D_38_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Vector2D_38_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Vector2D_38_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Vector2D_38_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Vector2D_38_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Vector2D_38_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector2D_38_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector2D_38_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector2D_38_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector2D_38_Type.Any_8_Concept.TypeOf_3156;
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
        this.Count_3189 = Vector3D_39_Type.Vector_11_Concept.Count_3189;
        this.At_3203 = Vector3D_39_Type.Vector_11_Concept.At_3203;
        this.Count_3168 = Vector3D_39_Type.Array_10_Concept.Count_3168;
        this.At_3173 = Vector3D_39_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = Vector3D_39_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector3D_39_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector3D_39_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector3D_39_Type.Any_8_Concept.TypeOf_3156;
        this.Zero_3232 = Vector3D_39_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = Vector3D_39_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = Vector3D_39_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = Vector3D_39_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = Vector3D_39_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector3D_39_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector3D_39_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector3D_39_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector3D_39_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = Vector3D_39_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = Vector3D_39_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = Vector3D_39_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = Vector3D_39_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = Vector3D_39_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = Vector3D_39_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = Vector3D_39_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector3D_39_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector3D_39_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector3D_39_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector3D_39_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Vector3D_39_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Vector3D_39_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector3D_39_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector3D_39_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector3D_39_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector3D_39_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Vector3D_39_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Vector3D_39_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector3D_39_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector3D_39_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector3D_39_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector3D_39_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Vector3D_39_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Vector3D_39_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector3D_39_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector3D_39_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector3D_39_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector3D_39_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Vector3D_39_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Vector3D_39_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Vector3D_39_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Vector3D_39_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Vector3D_39_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Vector3D_39_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector3D_39_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector3D_39_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector3D_39_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector3D_39_Type.Any_8_Concept.TypeOf_3156;
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
        this.Count_3189 = Vector4D_40_Type.Vector_11_Concept.Count_3189;
        this.At_3203 = Vector4D_40_Type.Vector_11_Concept.At_3203;
        this.Count_3168 = Vector4D_40_Type.Array_10_Concept.Count_3168;
        this.At_3173 = Vector4D_40_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = Vector4D_40_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector4D_40_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector4D_40_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector4D_40_Type.Any_8_Concept.TypeOf_3156;
        this.Zero_3232 = Vector4D_40_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = Vector4D_40_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = Vector4D_40_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = Vector4D_40_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = Vector4D_40_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector4D_40_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector4D_40_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector4D_40_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector4D_40_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = Vector4D_40_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = Vector4D_40_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = Vector4D_40_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = Vector4D_40_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = Vector4D_40_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = Vector4D_40_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = Vector4D_40_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector4D_40_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector4D_40_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector4D_40_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector4D_40_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Vector4D_40_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Vector4D_40_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector4D_40_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector4D_40_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector4D_40_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector4D_40_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Vector4D_40_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Vector4D_40_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector4D_40_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector4D_40_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector4D_40_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector4D_40_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Vector4D_40_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Vector4D_40_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector4D_40_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector4D_40_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector4D_40_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector4D_40_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Vector4D_40_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Vector4D_40_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Vector4D_40_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Vector4D_40_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Vector4D_40_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Vector4D_40_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Vector4D_40_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Vector4D_40_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Vector4D_40_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Vector4D_40_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Orientation3D_41_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Orientation3D_41_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Orientation3D_41_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Orientation3D_41_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Orientation3D_41_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Pose2D_42_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Pose2D_42_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Pose2D_42_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Pose2D_42_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Pose2D_42_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Pose3D_43_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Pose3D_43_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Pose3D_43_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Pose3D_43_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Pose3D_43_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Transform3D_44_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Transform3D_44_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Transform3D_44_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Transform3D_44_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Transform3D_44_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Transform2D_45_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Transform2D_45_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Transform2D_45_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Transform2D_45_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Transform2D_45_Type.Any_8_Concept.TypeOf_3156;
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
        this.Min_3506 = AlignedBox2D_46_Type.Interval_20_Concept.Min_3506;
        this.Max_3509 = AlignedBox2D_46_Type.Interval_20_Concept.Max_3509;
        this.Count_3189 = AlignedBox2D_46_Type.Vector_11_Concept.Count_3189;
        this.At_3203 = AlignedBox2D_46_Type.Vector_11_Concept.At_3203;
        this.Count_3168 = AlignedBox2D_46_Type.Array_10_Concept.Count_3168;
        this.At_3173 = AlignedBox2D_46_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = AlignedBox2D_46_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AlignedBox2D_46_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AlignedBox2D_46_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AlignedBox2D_46_Type.Any_8_Concept.TypeOf_3156;
        this.Zero_3232 = AlignedBox2D_46_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = AlignedBox2D_46_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = AlignedBox2D_46_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = AlignedBox2D_46_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = AlignedBox2D_46_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AlignedBox2D_46_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AlignedBox2D_46_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AlignedBox2D_46_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AlignedBox2D_46_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = AlignedBox2D_46_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = AlignedBox2D_46_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = AlignedBox2D_46_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = AlignedBox2D_46_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = AlignedBox2D_46_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = AlignedBox2D_46_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = AlignedBox2D_46_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AlignedBox2D_46_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AlignedBox2D_46_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AlignedBox2D_46_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AlignedBox2D_46_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = AlignedBox2D_46_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = AlignedBox2D_46_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AlignedBox2D_46_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AlignedBox2D_46_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AlignedBox2D_46_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AlignedBox2D_46_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = AlignedBox2D_46_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = AlignedBox2D_46_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AlignedBox2D_46_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AlignedBox2D_46_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AlignedBox2D_46_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AlignedBox2D_46_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = AlignedBox2D_46_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = AlignedBox2D_46_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AlignedBox2D_46_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AlignedBox2D_46_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AlignedBox2D_46_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AlignedBox2D_46_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = AlignedBox2D_46_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = AlignedBox2D_46_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = AlignedBox2D_46_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = AlignedBox2D_46_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = AlignedBox2D_46_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = AlignedBox2D_46_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AlignedBox2D_46_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AlignedBox2D_46_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AlignedBox2D_46_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AlignedBox2D_46_Type.Any_8_Concept.TypeOf_3156;
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
        this.Min_3506 = AlignedBox3D_47_Type.Interval_20_Concept.Min_3506;
        this.Max_3509 = AlignedBox3D_47_Type.Interval_20_Concept.Max_3509;
        this.Count_3189 = AlignedBox3D_47_Type.Vector_11_Concept.Count_3189;
        this.At_3203 = AlignedBox3D_47_Type.Vector_11_Concept.At_3203;
        this.Count_3168 = AlignedBox3D_47_Type.Array_10_Concept.Count_3168;
        this.At_3173 = AlignedBox3D_47_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = AlignedBox3D_47_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AlignedBox3D_47_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AlignedBox3D_47_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AlignedBox3D_47_Type.Any_8_Concept.TypeOf_3156;
        this.Zero_3232 = AlignedBox3D_47_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = AlignedBox3D_47_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = AlignedBox3D_47_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = AlignedBox3D_47_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = AlignedBox3D_47_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AlignedBox3D_47_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AlignedBox3D_47_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AlignedBox3D_47_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AlignedBox3D_47_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = AlignedBox3D_47_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = AlignedBox3D_47_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = AlignedBox3D_47_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = AlignedBox3D_47_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = AlignedBox3D_47_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = AlignedBox3D_47_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = AlignedBox3D_47_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AlignedBox3D_47_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AlignedBox3D_47_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AlignedBox3D_47_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AlignedBox3D_47_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = AlignedBox3D_47_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = AlignedBox3D_47_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AlignedBox3D_47_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AlignedBox3D_47_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AlignedBox3D_47_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AlignedBox3D_47_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = AlignedBox3D_47_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = AlignedBox3D_47_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AlignedBox3D_47_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AlignedBox3D_47_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AlignedBox3D_47_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AlignedBox3D_47_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = AlignedBox3D_47_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = AlignedBox3D_47_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AlignedBox3D_47_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AlignedBox3D_47_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AlignedBox3D_47_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AlignedBox3D_47_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = AlignedBox3D_47_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = AlignedBox3D_47_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = AlignedBox3D_47_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = AlignedBox3D_47_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = AlignedBox3D_47_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = AlignedBox3D_47_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AlignedBox3D_47_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AlignedBox3D_47_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AlignedBox3D_47_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AlignedBox3D_47_Type.Any_8_Concept.TypeOf_3156;
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
        this.Count_3189 = Complex_48_Type.Vector_11_Concept.Count_3189;
        this.At_3203 = Complex_48_Type.Vector_11_Concept.At_3203;
        this.Count_3168 = Complex_48_Type.Array_10_Concept.Count_3168;
        this.At_3173 = Complex_48_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = Complex_48_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Complex_48_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Complex_48_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Complex_48_Type.Any_8_Concept.TypeOf_3156;
        this.Zero_3232 = Complex_48_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = Complex_48_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = Complex_48_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = Complex_48_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = Complex_48_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Complex_48_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Complex_48_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Complex_48_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Complex_48_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = Complex_48_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = Complex_48_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = Complex_48_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = Complex_48_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = Complex_48_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = Complex_48_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = Complex_48_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Complex_48_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Complex_48_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Complex_48_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Complex_48_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Complex_48_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Complex_48_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Complex_48_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Complex_48_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Complex_48_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Complex_48_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Complex_48_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Complex_48_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Complex_48_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Complex_48_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Complex_48_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Complex_48_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Complex_48_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Complex_48_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Complex_48_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Complex_48_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Complex_48_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Complex_48_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Complex_48_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Complex_48_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Complex_48_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Complex_48_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Complex_48_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Complex_48_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Complex_48_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Complex_48_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Complex_48_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Complex_48_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Ray3D_49_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Ray3D_49_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Ray3D_49_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Ray3D_49_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Ray3D_49_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Ray2D_50_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Ray2D_50_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Ray2D_50_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Ray2D_50_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Ray2D_50_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Sphere_51_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Sphere_51_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Sphere_51_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Sphere_51_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Sphere_51_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Plane_52_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Plane_52_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Plane_52_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Plane_52_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Plane_52_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Triangle3D_53_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Triangle3D_53_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Triangle3D_53_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Triangle3D_53_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Triangle3D_53_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Triangle2D_54_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Triangle2D_54_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Triangle2D_54_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Triangle2D_54_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Triangle2D_54_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Quad3D_55_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Quad3D_55_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Quad3D_55_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Quad3D_55_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Quad3D_55_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Quad2D_56_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Quad2D_56_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Quad2D_56_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Quad2D_56_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Quad2D_56_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Point3D_57_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Point3D_57_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Point3D_57_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Point3D_57_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Point3D_57_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Point2D_58_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Point2D_58_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Point2D_58_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Point2D_58_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Point2D_58_Type.Any_8_Concept.TypeOf_3156;
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
        this.Min_3506 = Line3D_59_Type.Interval_20_Concept.Min_3506;
        this.Max_3509 = Line3D_59_Type.Interval_20_Concept.Max_3509;
        this.Count_3189 = Line3D_59_Type.Vector_11_Concept.Count_3189;
        this.At_3203 = Line3D_59_Type.Vector_11_Concept.At_3203;
        this.Count_3168 = Line3D_59_Type.Array_10_Concept.Count_3168;
        this.At_3173 = Line3D_59_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = Line3D_59_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Line3D_59_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Line3D_59_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Line3D_59_Type.Any_8_Concept.TypeOf_3156;
        this.Zero_3232 = Line3D_59_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = Line3D_59_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = Line3D_59_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = Line3D_59_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = Line3D_59_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Line3D_59_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Line3D_59_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Line3D_59_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Line3D_59_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = Line3D_59_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = Line3D_59_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = Line3D_59_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = Line3D_59_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = Line3D_59_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = Line3D_59_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = Line3D_59_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Line3D_59_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Line3D_59_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Line3D_59_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Line3D_59_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Line3D_59_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Line3D_59_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Line3D_59_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Line3D_59_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Line3D_59_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Line3D_59_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Line3D_59_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Line3D_59_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Line3D_59_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Line3D_59_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Line3D_59_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Line3D_59_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Line3D_59_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Line3D_59_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Line3D_59_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Line3D_59_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Line3D_59_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Line3D_59_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Line3D_59_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Line3D_59_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Line3D_59_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Line3D_59_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Line3D_59_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Line3D_59_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Line3D_59_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Line3D_59_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Line3D_59_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Line3D_59_Type.Any_8_Concept.TypeOf_3156;
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
        this.Min_3506 = Line2D_60_Type.Interval_20_Concept.Min_3506;
        this.Max_3509 = Line2D_60_Type.Interval_20_Concept.Max_3509;
        this.Count_3189 = Line2D_60_Type.Vector_11_Concept.Count_3189;
        this.At_3203 = Line2D_60_Type.Vector_11_Concept.At_3203;
        this.Count_3168 = Line2D_60_Type.Array_10_Concept.Count_3168;
        this.At_3173 = Line2D_60_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = Line2D_60_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Line2D_60_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Line2D_60_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Line2D_60_Type.Any_8_Concept.TypeOf_3156;
        this.Zero_3232 = Line2D_60_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = Line2D_60_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = Line2D_60_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = Line2D_60_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = Line2D_60_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Line2D_60_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Line2D_60_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Line2D_60_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Line2D_60_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = Line2D_60_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = Line2D_60_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = Line2D_60_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = Line2D_60_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = Line2D_60_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = Line2D_60_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = Line2D_60_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Line2D_60_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Line2D_60_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Line2D_60_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Line2D_60_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Line2D_60_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Line2D_60_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Line2D_60_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Line2D_60_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Line2D_60_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Line2D_60_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Line2D_60_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Line2D_60_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Line2D_60_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Line2D_60_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Line2D_60_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Line2D_60_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Line2D_60_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Line2D_60_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Line2D_60_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Line2D_60_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Line2D_60_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Line2D_60_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Line2D_60_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Line2D_60_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Line2D_60_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Line2D_60_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Line2D_60_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Line2D_60_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Line2D_60_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Line2D_60_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Line2D_60_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Line2D_60_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Color_61_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Color_61_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Color_61_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Color_61_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Color_61_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = ColorLUV_62_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ColorLUV_62_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ColorLUV_62_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ColorLUV_62_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ColorLUV_62_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = ColorLAB_63_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ColorLAB_63_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ColorLAB_63_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ColorLAB_63_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ColorLAB_63_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = ColorLCh_64_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ColorLCh_64_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ColorLCh_64_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ColorLCh_64_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ColorLCh_64_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = ColorHSV_65_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ColorHSV_65_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ColorHSV_65_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ColorHSV_65_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ColorHSV_65_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = ColorHSL_66_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ColorHSL_66_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ColorHSL_66_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ColorHSL_66_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ColorHSL_66_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = ColorYCbCr_67_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ColorYCbCr_67_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ColorYCbCr_67_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ColorYCbCr_67_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ColorYCbCr_67_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = SphericalCoordinate_68_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = SphericalCoordinate_68_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = SphericalCoordinate_68_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = SphericalCoordinate_68_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = SphericalCoordinate_68_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = PolarCoordinate_69_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = PolarCoordinate_69_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = PolarCoordinate_69_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = PolarCoordinate_69_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = PolarCoordinate_69_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = LogPolarCoordinate_70_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = LogPolarCoordinate_70_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = LogPolarCoordinate_70_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = LogPolarCoordinate_70_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = LogPolarCoordinate_70_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = CylindricalCoordinate_71_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = CylindricalCoordinate_71_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = CylindricalCoordinate_71_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = CylindricalCoordinate_71_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = CylindricalCoordinate_71_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = HorizontalCoordinate_72_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = HorizontalCoordinate_72_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = HorizontalCoordinate_72_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = HorizontalCoordinate_72_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = HorizontalCoordinate_72_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = GeoCoordinate_73_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = GeoCoordinate_73_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = GeoCoordinate_73_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = GeoCoordinate_73_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = GeoCoordinate_73_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = GeoCoordinateWithAltitude_74_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = GeoCoordinateWithAltitude_74_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = GeoCoordinateWithAltitude_74_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = GeoCoordinateWithAltitude_74_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = GeoCoordinateWithAltitude_74_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Circle_75_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Circle_75_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Circle_75_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Circle_75_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Circle_75_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Chord_76_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Chord_76_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Chord_76_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Chord_76_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Chord_76_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Size2D_77_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Size2D_77_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Size2D_77_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Size2D_77_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Size2D_77_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Size3D_78_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Size3D_78_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Size3D_78_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Size3D_78_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Size3D_78_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Rectangle2D_79_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Rectangle2D_79_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Rectangle2D_79_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Rectangle2D_79_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Rectangle2D_79_Type.Any_8_Concept.TypeOf_3156;
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
        this.Zero_3232 = Proportion_80_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = Proportion_80_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = Proportion_80_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = Proportion_80_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = Proportion_80_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Proportion_80_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Proportion_80_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Proportion_80_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Proportion_80_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = Proportion_80_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = Proportion_80_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = Proportion_80_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = Proportion_80_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = Proportion_80_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = Proportion_80_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = Proportion_80_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Proportion_80_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Proportion_80_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Proportion_80_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Proportion_80_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Proportion_80_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Proportion_80_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Proportion_80_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Proportion_80_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Proportion_80_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Proportion_80_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Proportion_80_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Proportion_80_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Proportion_80_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Proportion_80_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Proportion_80_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Proportion_80_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Proportion_80_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Proportion_80_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Proportion_80_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Proportion_80_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Proportion_80_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Proportion_80_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Fraction_81_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Fraction_81_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Fraction_81_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Fraction_81_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Fraction_81_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = Angle_82_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = Angle_82_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Angle_82_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Angle_82_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Angle_82_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Angle_82_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Angle_82_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Angle_82_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Angle_82_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Angle_82_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Angle_82_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Angle_82_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Angle_82_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Angle_82_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Angle_82_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Angle_82_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Angle_82_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Angle_82_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Angle_82_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Angle_82_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Angle_82_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Angle_82_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Angle_82_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Angle_82_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Angle_82_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Angle_82_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Angle_82_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Angle_82_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Angle_82_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Angle_82_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Angle_82_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Angle_82_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Angle_82_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Angle_82_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = Length_83_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = Length_83_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Length_83_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Length_83_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Length_83_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Length_83_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Length_83_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Length_83_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Length_83_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Length_83_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Length_83_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Length_83_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Length_83_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Length_83_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Length_83_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Length_83_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Length_83_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Length_83_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Length_83_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Length_83_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Length_83_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Length_83_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Length_83_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Length_83_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Length_83_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Length_83_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Length_83_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Length_83_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Length_83_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Length_83_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Length_83_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Length_83_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Length_83_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Length_83_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = Mass_84_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = Mass_84_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Mass_84_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Mass_84_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Mass_84_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Mass_84_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Mass_84_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Mass_84_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Mass_84_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Mass_84_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Mass_84_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Mass_84_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Mass_84_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Mass_84_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Mass_84_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Mass_84_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Mass_84_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Mass_84_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Mass_84_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Mass_84_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Mass_84_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Mass_84_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Mass_84_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Mass_84_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Mass_84_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Mass_84_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Mass_84_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Mass_84_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Mass_84_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Mass_84_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Mass_84_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Mass_84_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Mass_84_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Mass_84_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = Temperature_85_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = Temperature_85_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Temperature_85_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Temperature_85_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Temperature_85_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Temperature_85_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Temperature_85_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Temperature_85_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Temperature_85_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Temperature_85_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Temperature_85_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Temperature_85_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Temperature_85_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Temperature_85_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Temperature_85_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Temperature_85_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Temperature_85_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Temperature_85_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Temperature_85_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Temperature_85_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Temperature_85_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Temperature_85_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Temperature_85_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Temperature_85_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Temperature_85_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Temperature_85_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Temperature_85_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Temperature_85_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Temperature_85_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Temperature_85_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Temperature_85_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Temperature_85_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Temperature_85_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Temperature_85_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = TimeSpan_86_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = TimeSpan_86_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeSpan_86_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeSpan_86_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeSpan_86_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeSpan_86_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = TimeSpan_86_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = TimeSpan_86_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = TimeSpan_86_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = TimeSpan_86_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = TimeSpan_86_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = TimeSpan_86_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeSpan_86_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeSpan_86_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeSpan_86_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeSpan_86_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = TimeSpan_86_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = TimeSpan_86_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeSpan_86_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeSpan_86_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeSpan_86_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeSpan_86_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = TimeSpan_86_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = TimeSpan_86_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeSpan_86_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeSpan_86_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeSpan_86_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeSpan_86_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = TimeSpan_86_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = TimeSpan_86_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeSpan_86_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeSpan_86_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeSpan_86_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeSpan_86_Type.Any_8_Concept.TypeOf_3156;
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
        this.Min_3506 = TimeRange_87_Type.Interval_20_Concept.Min_3506;
        this.Max_3509 = TimeRange_87_Type.Interval_20_Concept.Max_3509;
        this.Count_3189 = TimeRange_87_Type.Vector_11_Concept.Count_3189;
        this.At_3203 = TimeRange_87_Type.Vector_11_Concept.At_3203;
        this.Count_3168 = TimeRange_87_Type.Array_10_Concept.Count_3168;
        this.At_3173 = TimeRange_87_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = TimeRange_87_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeRange_87_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeRange_87_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeRange_87_Type.Any_8_Concept.TypeOf_3156;
        this.Zero_3232 = TimeRange_87_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = TimeRange_87_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = TimeRange_87_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = TimeRange_87_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = TimeRange_87_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeRange_87_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeRange_87_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeRange_87_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeRange_87_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = TimeRange_87_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = TimeRange_87_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = TimeRange_87_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = TimeRange_87_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = TimeRange_87_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = TimeRange_87_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = TimeRange_87_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeRange_87_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeRange_87_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeRange_87_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeRange_87_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = TimeRange_87_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = TimeRange_87_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeRange_87_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeRange_87_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeRange_87_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeRange_87_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = TimeRange_87_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = TimeRange_87_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeRange_87_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeRange_87_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeRange_87_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeRange_87_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = TimeRange_87_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = TimeRange_87_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeRange_87_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeRange_87_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeRange_87_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeRange_87_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = TimeRange_87_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = TimeRange_87_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = TimeRange_87_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = TimeRange_87_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = TimeRange_87_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = TimeRange_87_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeRange_87_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeRange_87_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeRange_87_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeRange_87_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = DateTime_88_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = DateTime_88_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = DateTime_88_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = DateTime_88_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = DateTime_88_Type.Any_8_Concept.TypeOf_3156;
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
        this.Min_3506 = AnglePair_89_Type.Interval_20_Concept.Min_3506;
        this.Max_3509 = AnglePair_89_Type.Interval_20_Concept.Max_3509;
        this.Count_3189 = AnglePair_89_Type.Vector_11_Concept.Count_3189;
        this.At_3203 = AnglePair_89_Type.Vector_11_Concept.At_3203;
        this.Count_3168 = AnglePair_89_Type.Array_10_Concept.Count_3168;
        this.At_3173 = AnglePair_89_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = AnglePair_89_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AnglePair_89_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AnglePair_89_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AnglePair_89_Type.Any_8_Concept.TypeOf_3156;
        this.Zero_3232 = AnglePair_89_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = AnglePair_89_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = AnglePair_89_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = AnglePair_89_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = AnglePair_89_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AnglePair_89_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AnglePair_89_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AnglePair_89_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AnglePair_89_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = AnglePair_89_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = AnglePair_89_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = AnglePair_89_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = AnglePair_89_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = AnglePair_89_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = AnglePair_89_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = AnglePair_89_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AnglePair_89_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AnglePair_89_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AnglePair_89_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AnglePair_89_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = AnglePair_89_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = AnglePair_89_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AnglePair_89_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AnglePair_89_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AnglePair_89_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AnglePair_89_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = AnglePair_89_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = AnglePair_89_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AnglePair_89_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AnglePair_89_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AnglePair_89_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AnglePair_89_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = AnglePair_89_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = AnglePair_89_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AnglePair_89_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AnglePair_89_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AnglePair_89_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AnglePair_89_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = AnglePair_89_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = AnglePair_89_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = AnglePair_89_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = AnglePair_89_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = AnglePair_89_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = AnglePair_89_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = AnglePair_89_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = AnglePair_89_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = AnglePair_89_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = AnglePair_89_Type.Any_8_Concept.TypeOf_3156;
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
        this.Zero_3232 = Ring_90_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = Ring_90_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = Ring_90_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = Ring_90_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = Ring_90_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Ring_90_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Ring_90_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Ring_90_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Ring_90_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = Ring_90_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = Ring_90_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = Ring_90_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = Ring_90_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = Ring_90_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = Ring_90_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = Ring_90_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Ring_90_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Ring_90_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Ring_90_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Ring_90_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Ring_90_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Ring_90_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Ring_90_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Ring_90_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Ring_90_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Ring_90_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Ring_90_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Ring_90_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Ring_90_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Ring_90_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Ring_90_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Ring_90_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Ring_90_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Ring_90_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Ring_90_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Ring_90_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Ring_90_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Ring_90_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Arc_91_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Arc_91_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Arc_91_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Arc_91_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Arc_91_Type.Any_8_Concept.TypeOf_3156;
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
        this.Min_3506 = TimeInterval_92_Type.Interval_20_Concept.Min_3506;
        this.Max_3509 = TimeInterval_92_Type.Interval_20_Concept.Max_3509;
        this.Count_3189 = TimeInterval_92_Type.Vector_11_Concept.Count_3189;
        this.At_3203 = TimeInterval_92_Type.Vector_11_Concept.At_3203;
        this.Count_3168 = TimeInterval_92_Type.Array_10_Concept.Count_3168;
        this.At_3173 = TimeInterval_92_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = TimeInterval_92_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeInterval_92_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeInterval_92_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeInterval_92_Type.Any_8_Concept.TypeOf_3156;
        this.Zero_3232 = TimeInterval_92_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = TimeInterval_92_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = TimeInterval_92_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = TimeInterval_92_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = TimeInterval_92_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeInterval_92_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeInterval_92_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeInterval_92_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeInterval_92_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = TimeInterval_92_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = TimeInterval_92_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = TimeInterval_92_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = TimeInterval_92_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = TimeInterval_92_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = TimeInterval_92_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = TimeInterval_92_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeInterval_92_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeInterval_92_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeInterval_92_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeInterval_92_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = TimeInterval_92_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = TimeInterval_92_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeInterval_92_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeInterval_92_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeInterval_92_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeInterval_92_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = TimeInterval_92_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = TimeInterval_92_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeInterval_92_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeInterval_92_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeInterval_92_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeInterval_92_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = TimeInterval_92_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = TimeInterval_92_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeInterval_92_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeInterval_92_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeInterval_92_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeInterval_92_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = TimeInterval_92_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = TimeInterval_92_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = TimeInterval_92_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = TimeInterval_92_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = TimeInterval_92_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = TimeInterval_92_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = TimeInterval_92_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = TimeInterval_92_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = TimeInterval_92_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = TimeInterval_92_Type.Any_8_Concept.TypeOf_3156;
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
        this.Min_3506 = RealInterval_93_Type.Interval_20_Concept.Min_3506;
        this.Max_3509 = RealInterval_93_Type.Interval_20_Concept.Max_3509;
        this.Count_3189 = RealInterval_93_Type.Vector_11_Concept.Count_3189;
        this.At_3203 = RealInterval_93_Type.Vector_11_Concept.At_3203;
        this.Count_3168 = RealInterval_93_Type.Array_10_Concept.Count_3168;
        this.At_3173 = RealInterval_93_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = RealInterval_93_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = RealInterval_93_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = RealInterval_93_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = RealInterval_93_Type.Any_8_Concept.TypeOf_3156;
        this.Zero_3232 = RealInterval_93_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = RealInterval_93_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = RealInterval_93_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = RealInterval_93_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = RealInterval_93_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = RealInterval_93_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = RealInterval_93_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = RealInterval_93_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = RealInterval_93_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = RealInterval_93_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = RealInterval_93_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = RealInterval_93_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = RealInterval_93_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = RealInterval_93_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = RealInterval_93_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = RealInterval_93_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = RealInterval_93_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = RealInterval_93_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = RealInterval_93_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = RealInterval_93_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = RealInterval_93_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = RealInterval_93_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = RealInterval_93_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = RealInterval_93_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = RealInterval_93_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = RealInterval_93_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = RealInterval_93_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = RealInterval_93_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = RealInterval_93_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = RealInterval_93_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = RealInterval_93_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = RealInterval_93_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = RealInterval_93_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = RealInterval_93_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = RealInterval_93_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = RealInterval_93_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = RealInterval_93_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = RealInterval_93_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = RealInterval_93_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = RealInterval_93_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = RealInterval_93_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = RealInterval_93_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = RealInterval_93_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = RealInterval_93_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = RealInterval_93_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = RealInterval_93_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = RealInterval_93_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = RealInterval_93_Type.Any_8_Concept.TypeOf_3156;
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
        this.Min_3506 = Interval2D_94_Type.Interval_20_Concept.Min_3506;
        this.Max_3509 = Interval2D_94_Type.Interval_20_Concept.Max_3509;
        this.Count_3189 = Interval2D_94_Type.Vector_11_Concept.Count_3189;
        this.At_3203 = Interval2D_94_Type.Vector_11_Concept.At_3203;
        this.Count_3168 = Interval2D_94_Type.Array_10_Concept.Count_3168;
        this.At_3173 = Interval2D_94_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = Interval2D_94_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Interval2D_94_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Interval2D_94_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Interval2D_94_Type.Any_8_Concept.TypeOf_3156;
        this.Zero_3232 = Interval2D_94_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = Interval2D_94_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = Interval2D_94_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = Interval2D_94_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = Interval2D_94_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Interval2D_94_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Interval2D_94_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Interval2D_94_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Interval2D_94_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = Interval2D_94_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = Interval2D_94_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = Interval2D_94_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = Interval2D_94_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = Interval2D_94_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = Interval2D_94_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = Interval2D_94_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Interval2D_94_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Interval2D_94_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Interval2D_94_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Interval2D_94_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Interval2D_94_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Interval2D_94_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Interval2D_94_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Interval2D_94_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Interval2D_94_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Interval2D_94_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Interval2D_94_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Interval2D_94_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Interval2D_94_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Interval2D_94_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Interval2D_94_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Interval2D_94_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Interval2D_94_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Interval2D_94_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Interval2D_94_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Interval2D_94_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Interval2D_94_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Interval2D_94_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Interval2D_94_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Interval2D_94_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Interval2D_94_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Interval2D_94_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Interval2D_94_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Interval2D_94_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Interval2D_94_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Interval2D_94_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Interval2D_94_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Interval2D_94_Type.Any_8_Concept.TypeOf_3156;
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
        this.Min_3506 = Interval3D_95_Type.Interval_20_Concept.Min_3506;
        this.Max_3509 = Interval3D_95_Type.Interval_20_Concept.Max_3509;
        this.Count_3189 = Interval3D_95_Type.Vector_11_Concept.Count_3189;
        this.At_3203 = Interval3D_95_Type.Vector_11_Concept.At_3203;
        this.Count_3168 = Interval3D_95_Type.Array_10_Concept.Count_3168;
        this.At_3173 = Interval3D_95_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = Interval3D_95_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Interval3D_95_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Interval3D_95_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Interval3D_95_Type.Any_8_Concept.TypeOf_3156;
        this.Zero_3232 = Interval3D_95_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = Interval3D_95_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = Interval3D_95_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = Interval3D_95_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = Interval3D_95_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Interval3D_95_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Interval3D_95_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Interval3D_95_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Interval3D_95_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = Interval3D_95_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = Interval3D_95_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = Interval3D_95_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = Interval3D_95_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = Interval3D_95_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = Interval3D_95_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = Interval3D_95_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Interval3D_95_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Interval3D_95_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Interval3D_95_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Interval3D_95_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Interval3D_95_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Interval3D_95_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Interval3D_95_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Interval3D_95_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Interval3D_95_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Interval3D_95_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Interval3D_95_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Interval3D_95_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Interval3D_95_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Interval3D_95_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Interval3D_95_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Interval3D_95_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Interval3D_95_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Interval3D_95_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Interval3D_95_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Interval3D_95_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Interval3D_95_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Interval3D_95_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Interval3D_95_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Interval3D_95_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Interval3D_95_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Interval3D_95_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Interval3D_95_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Interval3D_95_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Interval3D_95_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Interval3D_95_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Interval3D_95_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Interval3D_95_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Capsule_96_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Capsule_96_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Capsule_96_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Capsule_96_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Capsule_96_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Matrix3D_97_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Matrix3D_97_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Matrix3D_97_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Matrix3D_97_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Matrix3D_97_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Cylinder_98_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Cylinder_98_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Cylinder_98_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Cylinder_98_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Cylinder_98_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Cone_99_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Cone_99_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Cone_99_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Cone_99_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Cone_99_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Tube_100_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Tube_100_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Tube_100_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Tube_100_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Tube_100_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = ConeSegment_101_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ConeSegment_101_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ConeSegment_101_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ConeSegment_101_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ConeSegment_101_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Box2D_102_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Box2D_102_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Box2D_102_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Box2D_102_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Box2D_102_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = Box3D_103_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Box3D_103_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Box3D_103_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Box3D_103_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Box3D_103_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = CubicBezierTriangle3D_104_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = CubicBezierTriangle3D_104_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = CubicBezierTriangle3D_104_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = CubicBezierTriangle3D_104_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = CubicBezierTriangle3D_104_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = CubicBezier2D_105_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = CubicBezier2D_105_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = CubicBezier2D_105_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = CubicBezier2D_105_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = CubicBezier2D_105_Type.Any_8_Concept.TypeOf_3156;
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
        this.Count_3189 = UV_106_Type.Vector_11_Concept.Count_3189;
        this.At_3203 = UV_106_Type.Vector_11_Concept.At_3203;
        this.Count_3168 = UV_106_Type.Array_10_Concept.Count_3168;
        this.At_3173 = UV_106_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = UV_106_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = UV_106_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = UV_106_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = UV_106_Type.Any_8_Concept.TypeOf_3156;
        this.Zero_3232 = UV_106_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = UV_106_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = UV_106_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = UV_106_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = UV_106_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = UV_106_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = UV_106_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = UV_106_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = UV_106_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = UV_106_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = UV_106_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = UV_106_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = UV_106_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = UV_106_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = UV_106_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = UV_106_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = UV_106_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = UV_106_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = UV_106_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = UV_106_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = UV_106_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = UV_106_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = UV_106_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = UV_106_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = UV_106_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = UV_106_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = UV_106_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = UV_106_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = UV_106_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = UV_106_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = UV_106_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = UV_106_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = UV_106_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = UV_106_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = UV_106_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = UV_106_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = UV_106_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = UV_106_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = UV_106_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = UV_106_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = UV_106_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = UV_106_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = UV_106_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = UV_106_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = UV_106_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = UV_106_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = UV_106_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = UV_106_Type.Any_8_Concept.TypeOf_3156;
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
        this.Count_3189 = UVW_107_Type.Vector_11_Concept.Count_3189;
        this.At_3203 = UVW_107_Type.Vector_11_Concept.At_3203;
        this.Count_3168 = UVW_107_Type.Array_10_Concept.Count_3168;
        this.At_3173 = UVW_107_Type.Array_10_Concept.At_3173;
        this.FieldNames_3147 = UVW_107_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = UVW_107_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = UVW_107_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = UVW_107_Type.Any_8_Concept.TypeOf_3156;
        this.Zero_3232 = UVW_107_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = UVW_107_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = UVW_107_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = UVW_107_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = UVW_107_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = UVW_107_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = UVW_107_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = UVW_107_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = UVW_107_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = UVW_107_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = UVW_107_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = UVW_107_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = UVW_107_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = UVW_107_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = UVW_107_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = UVW_107_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = UVW_107_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = UVW_107_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = UVW_107_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = UVW_107_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = UVW_107_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = UVW_107_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = UVW_107_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = UVW_107_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = UVW_107_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = UVW_107_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = UVW_107_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = UVW_107_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = UVW_107_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = UVW_107_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = UVW_107_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = UVW_107_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = UVW_107_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = UVW_107_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = UVW_107_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = UVW_107_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = UVW_107_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = UVW_107_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = UVW_107_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = UVW_107_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = UVW_107_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = UVW_107_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = UVW_107_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = UVW_107_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = UVW_107_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = UVW_107_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = UVW_107_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = UVW_107_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = CubicBezier3D_108_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = CubicBezier3D_108_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = CubicBezier3D_108_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = CubicBezier3D_108_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = CubicBezier3D_108_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = QuadraticBezier2D_109_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = QuadraticBezier2D_109_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = QuadraticBezier2D_109_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = QuadraticBezier2D_109_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = QuadraticBezier2D_109_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = QuadraticBezier3D_110_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = QuadraticBezier3D_110_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = QuadraticBezier3D_110_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = QuadraticBezier3D_110_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = QuadraticBezier3D_110_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = Area_111_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = Area_111_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Area_111_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Area_111_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Area_111_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Area_111_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Area_111_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Area_111_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Area_111_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Area_111_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Area_111_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Area_111_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Area_111_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Area_111_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Area_111_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Area_111_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Area_111_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Area_111_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Area_111_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Area_111_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Area_111_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Area_111_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Area_111_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Area_111_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Area_111_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Area_111_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Area_111_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Area_111_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Area_111_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Area_111_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Area_111_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Area_111_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Area_111_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Area_111_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = Volume_112_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = Volume_112_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Volume_112_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Volume_112_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Volume_112_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Volume_112_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Volume_112_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Volume_112_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Volume_112_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Volume_112_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Volume_112_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Volume_112_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Volume_112_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Volume_112_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Volume_112_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Volume_112_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Volume_112_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Volume_112_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Volume_112_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Volume_112_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Volume_112_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Volume_112_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Volume_112_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Volume_112_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Volume_112_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Volume_112_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Volume_112_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Volume_112_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Volume_112_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Volume_112_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Volume_112_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Volume_112_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Volume_112_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Volume_112_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = Velocity_113_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = Velocity_113_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Velocity_113_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Velocity_113_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Velocity_113_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Velocity_113_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Velocity_113_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Velocity_113_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Velocity_113_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Velocity_113_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Velocity_113_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Velocity_113_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Velocity_113_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Velocity_113_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Velocity_113_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Velocity_113_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Velocity_113_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Velocity_113_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Velocity_113_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Velocity_113_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Velocity_113_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Velocity_113_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Velocity_113_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Velocity_113_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Velocity_113_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Velocity_113_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Velocity_113_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Velocity_113_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Velocity_113_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Velocity_113_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Velocity_113_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Velocity_113_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Velocity_113_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Velocity_113_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = Acceleration_114_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = Acceleration_114_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Acceleration_114_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Acceleration_114_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Acceleration_114_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Acceleration_114_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Acceleration_114_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Acceleration_114_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Acceleration_114_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Acceleration_114_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Acceleration_114_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Acceleration_114_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Acceleration_114_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Acceleration_114_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Acceleration_114_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Acceleration_114_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Acceleration_114_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Acceleration_114_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Acceleration_114_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Acceleration_114_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Acceleration_114_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Acceleration_114_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Acceleration_114_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Acceleration_114_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Acceleration_114_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Acceleration_114_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Acceleration_114_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Acceleration_114_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Acceleration_114_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Acceleration_114_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Acceleration_114_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Acceleration_114_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Acceleration_114_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Acceleration_114_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = Force_115_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = Force_115_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Force_115_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Force_115_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Force_115_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Force_115_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Force_115_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Force_115_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Force_115_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Force_115_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Force_115_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Force_115_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Force_115_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Force_115_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Force_115_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Force_115_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Force_115_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Force_115_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Force_115_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Force_115_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Force_115_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Force_115_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Force_115_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Force_115_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Force_115_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Force_115_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Force_115_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Force_115_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Force_115_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Force_115_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Force_115_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Force_115_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Force_115_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Force_115_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = Pressure_116_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = Pressure_116_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Pressure_116_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Pressure_116_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Pressure_116_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Pressure_116_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Pressure_116_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Pressure_116_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Pressure_116_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Pressure_116_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Pressure_116_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Pressure_116_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Pressure_116_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Pressure_116_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Pressure_116_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Pressure_116_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Pressure_116_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Pressure_116_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Pressure_116_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Pressure_116_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Pressure_116_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Pressure_116_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Pressure_116_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Pressure_116_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Pressure_116_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Pressure_116_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Pressure_116_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Pressure_116_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Pressure_116_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Pressure_116_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Pressure_116_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Pressure_116_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Pressure_116_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Pressure_116_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = Energy_117_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = Energy_117_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Energy_117_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Energy_117_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Energy_117_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Energy_117_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Energy_117_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Energy_117_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Energy_117_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Energy_117_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Energy_117_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Energy_117_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Energy_117_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Energy_117_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Energy_117_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Energy_117_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Energy_117_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Energy_117_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Energy_117_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Energy_117_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Energy_117_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Energy_117_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Energy_117_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Energy_117_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Energy_117_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Energy_117_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Energy_117_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Energy_117_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Energy_117_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Energy_117_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Energy_117_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Energy_117_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Energy_117_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Energy_117_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = Memory_118_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = Memory_118_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Memory_118_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Memory_118_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Memory_118_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Memory_118_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Memory_118_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Memory_118_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Memory_118_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Memory_118_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Memory_118_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Memory_118_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Memory_118_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Memory_118_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Memory_118_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Memory_118_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Memory_118_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Memory_118_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Memory_118_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Memory_118_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Memory_118_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Memory_118_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Memory_118_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Memory_118_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Memory_118_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Memory_118_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Memory_118_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Memory_118_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Memory_118_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Memory_118_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Memory_118_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Memory_118_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Memory_118_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Memory_118_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = Frequency_119_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = Frequency_119_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Frequency_119_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Frequency_119_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Frequency_119_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Frequency_119_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Frequency_119_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Frequency_119_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Frequency_119_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Frequency_119_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Frequency_119_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Frequency_119_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Frequency_119_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Frequency_119_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Frequency_119_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Frequency_119_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Frequency_119_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Frequency_119_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Frequency_119_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Frequency_119_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Frequency_119_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Frequency_119_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Frequency_119_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Frequency_119_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Frequency_119_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Frequency_119_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Frequency_119_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Frequency_119_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Frequency_119_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Frequency_119_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Frequency_119_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Frequency_119_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Frequency_119_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Frequency_119_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = Loudness_120_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = Loudness_120_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Loudness_120_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Loudness_120_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Loudness_120_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Loudness_120_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Loudness_120_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Loudness_120_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Loudness_120_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Loudness_120_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Loudness_120_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Loudness_120_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Loudness_120_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Loudness_120_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Loudness_120_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Loudness_120_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Loudness_120_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Loudness_120_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Loudness_120_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Loudness_120_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Loudness_120_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Loudness_120_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Loudness_120_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Loudness_120_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Loudness_120_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Loudness_120_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Loudness_120_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Loudness_120_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Loudness_120_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Loudness_120_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Loudness_120_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Loudness_120_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Loudness_120_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Loudness_120_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = LuminousIntensity_121_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = LuminousIntensity_121_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = LuminousIntensity_121_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = LuminousIntensity_121_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = LuminousIntensity_121_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = LuminousIntensity_121_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = LuminousIntensity_121_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = LuminousIntensity_121_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = LuminousIntensity_121_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = LuminousIntensity_121_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = LuminousIntensity_121_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = LuminousIntensity_121_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = LuminousIntensity_121_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = LuminousIntensity_121_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = LuminousIntensity_121_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = LuminousIntensity_121_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = LuminousIntensity_121_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = LuminousIntensity_121_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = LuminousIntensity_121_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = LuminousIntensity_121_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = LuminousIntensity_121_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = LuminousIntensity_121_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = LuminousIntensity_121_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = LuminousIntensity_121_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = LuminousIntensity_121_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = LuminousIntensity_121_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = LuminousIntensity_121_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = LuminousIntensity_121_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = LuminousIntensity_121_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = LuminousIntensity_121_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = LuminousIntensity_121_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = LuminousIntensity_121_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = LuminousIntensity_121_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = LuminousIntensity_121_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = ElectricPotential_122_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = ElectricPotential_122_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricPotential_122_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricPotential_122_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricPotential_122_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricPotential_122_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = ElectricPotential_122_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = ElectricPotential_122_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = ElectricPotential_122_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = ElectricPotential_122_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = ElectricPotential_122_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = ElectricPotential_122_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricPotential_122_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricPotential_122_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricPotential_122_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricPotential_122_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = ElectricPotential_122_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = ElectricPotential_122_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricPotential_122_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricPotential_122_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricPotential_122_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricPotential_122_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = ElectricPotential_122_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = ElectricPotential_122_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricPotential_122_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricPotential_122_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricPotential_122_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricPotential_122_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = ElectricPotential_122_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = ElectricPotential_122_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricPotential_122_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricPotential_122_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricPotential_122_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricPotential_122_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = ElectricCharge_123_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = ElectricCharge_123_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricCharge_123_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricCharge_123_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricCharge_123_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricCharge_123_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = ElectricCharge_123_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = ElectricCharge_123_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = ElectricCharge_123_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = ElectricCharge_123_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = ElectricCharge_123_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = ElectricCharge_123_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricCharge_123_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricCharge_123_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricCharge_123_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricCharge_123_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = ElectricCharge_123_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = ElectricCharge_123_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricCharge_123_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricCharge_123_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricCharge_123_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricCharge_123_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = ElectricCharge_123_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = ElectricCharge_123_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricCharge_123_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricCharge_123_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricCharge_123_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricCharge_123_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = ElectricCharge_123_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = ElectricCharge_123_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricCharge_123_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricCharge_123_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricCharge_123_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricCharge_123_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = ElectricCurrent_124_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = ElectricCurrent_124_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricCurrent_124_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricCurrent_124_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricCurrent_124_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricCurrent_124_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = ElectricCurrent_124_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = ElectricCurrent_124_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = ElectricCurrent_124_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = ElectricCurrent_124_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = ElectricCurrent_124_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = ElectricCurrent_124_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricCurrent_124_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricCurrent_124_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricCurrent_124_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricCurrent_124_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = ElectricCurrent_124_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = ElectricCurrent_124_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricCurrent_124_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricCurrent_124_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricCurrent_124_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricCurrent_124_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = ElectricCurrent_124_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = ElectricCurrent_124_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricCurrent_124_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricCurrent_124_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricCurrent_124_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricCurrent_124_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = ElectricCurrent_124_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = ElectricCurrent_124_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricCurrent_124_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricCurrent_124_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricCurrent_124_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricCurrent_124_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = ElectricResistance_125_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = ElectricResistance_125_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricResistance_125_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricResistance_125_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricResistance_125_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricResistance_125_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = ElectricResistance_125_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = ElectricResistance_125_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = ElectricResistance_125_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = ElectricResistance_125_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = ElectricResistance_125_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = ElectricResistance_125_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricResistance_125_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricResistance_125_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricResistance_125_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricResistance_125_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = ElectricResistance_125_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = ElectricResistance_125_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricResistance_125_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricResistance_125_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricResistance_125_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricResistance_125_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = ElectricResistance_125_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = ElectricResistance_125_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricResistance_125_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricResistance_125_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricResistance_125_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricResistance_125_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = ElectricResistance_125_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = ElectricResistance_125_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = ElectricResistance_125_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = ElectricResistance_125_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = ElectricResistance_125_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = ElectricResistance_125_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = Power_126_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = Power_126_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Power_126_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Power_126_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Power_126_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Power_126_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Power_126_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Power_126_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Power_126_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Power_126_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Power_126_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Power_126_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Power_126_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Power_126_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Power_126_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Power_126_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Power_126_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Power_126_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Power_126_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Power_126_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Power_126_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Power_126_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Power_126_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Power_126_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Power_126_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Power_126_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Power_126_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Power_126_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Power_126_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Power_126_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Power_126_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Power_126_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Power_126_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Power_126_Type.Any_8_Concept.TypeOf_3156;
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
        this.Value_3222 = Density_127_Type.Measure_12_Concept.Value_3222;
        this.Default_3162 = Density_127_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Density_127_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Density_127_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Density_127_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Density_127_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3397 = Density_127_Type.ScalarArithmetic_18_Concept.Add_3397;
        this.Subtract_3411 = Density_127_Type.ScalarArithmetic_18_Concept.Subtract_3411;
        this.Multiply_3425 = Density_127_Type.ScalarArithmetic_18_Concept.Multiply_3425;
        this.Divide_3439 = Density_127_Type.ScalarArithmetic_18_Concept.Divide_3439;
        this.Modulo_3453 = Density_127_Type.ScalarArithmetic_18_Concept.Modulo_3453;
        this.Default_3162 = Density_127_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Density_127_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Density_127_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Density_127_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Density_127_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Density_127_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Density_127_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Density_127_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Density_127_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Density_127_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Density_127_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Density_127_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Density_127_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Density_127_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Density_127_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Density_127_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Density_127_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Density_127_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Density_127_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Density_127_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Density_127_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Density_127_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Density_127_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = NormalDistribution_128_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = NormalDistribution_128_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = NormalDistribution_128_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = NormalDistribution_128_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = NormalDistribution_128_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = PoissonDistribution_129_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = PoissonDistribution_129_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = PoissonDistribution_129_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = PoissonDistribution_129_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = PoissonDistribution_129_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = BernoulliDistribution_130_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = BernoulliDistribution_130_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = BernoulliDistribution_130_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = BernoulliDistribution_130_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = BernoulliDistribution_130_Type.Any_8_Concept.TypeOf_3156;
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
        this.Zero_3232 = Probability_131_Type.Numerical_13_Concept.Zero_3232;
        this.One_3235 = Probability_131_Type.Numerical_13_Concept.One_3235;
        this.MinValue_3238 = Probability_131_Type.Numerical_13_Concept.MinValue_3238;
        this.MaxValue_3241 = Probability_131_Type.Numerical_13_Concept.MaxValue_3241;
        this.Default_3162 = Probability_131_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Probability_131_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Probability_131_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Probability_131_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Probability_131_Type.Any_8_Concept.TypeOf_3156;
        this.Add_3309 = Probability_131_Type.Arithmetic_17_Concept.Add_3309;
        this.Negative_3319 = Probability_131_Type.Arithmetic_17_Concept.Negative_3319;
        this.Reciprocal_3329 = Probability_131_Type.Arithmetic_17_Concept.Reciprocal_3329;
        this.Multiply_3346 = Probability_131_Type.Arithmetic_17_Concept.Multiply_3346;
        this.Divide_3363 = Probability_131_Type.Arithmetic_17_Concept.Divide_3363;
        this.Modulo_3380 = Probability_131_Type.Arithmetic_17_Concept.Modulo_3380;
        this.Default_3162 = Probability_131_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Probability_131_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Probability_131_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Probability_131_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Probability_131_Type.Any_8_Concept.TypeOf_3156;
        this.Equals_3289 = Probability_131_Type.Equatable_16_Concept.Equals_3289;
        this.Default_3162 = Probability_131_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Probability_131_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Probability_131_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Probability_131_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Probability_131_Type.Any_8_Concept.TypeOf_3156;
        this.Compare_3266 = Probability_131_Type.Comparable_15_Concept.Compare_3266;
        this.Default_3162 = Probability_131_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Probability_131_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Probability_131_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Probability_131_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Probability_131_Type.Any_8_Concept.TypeOf_3156;
        this.Magnitude_3260 = Probability_131_Type.Magnitudinal_14_Concept.Magnitude_3260;
        this.Default_3162 = Probability_131_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = Probability_131_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = Probability_131_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = Probability_131_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = Probability_131_Type.Any_8_Concept.TypeOf_3156;
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
        this.Default_3162 = BinomialDistribution_132_Type.Value_9_Concept.Default_3162;
        this.FieldNames_3147 = BinomialDistribution_132_Type.Any_8_Concept.FieldNames_3147;
        this.FieldValues_3150 = BinomialDistribution_132_Type.Any_8_Concept.FieldValues_3150;
        this.FieldTypes_3153 = BinomialDistribution_132_Type.Any_8_Concept.FieldTypes_3153;
        this.TypeOf_3156 = BinomialDistribution_132_Type.Any_8_Concept.TypeOf_3156;
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