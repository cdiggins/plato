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
    static Cos_3044 = function (x_3043/* : Angle_84 */) /* : Number_30 */{ return null; };
    static Sin_3047 = function (x_3046/* : Angle_84 */) /* : Number_30 */{ return null; };
    static Tan_3050 = function (x_3049/* : Angle_84 */) /* : Number_30 */{ return null; };
    static Acos_3053 = function (x_3052/* : Number_30 */) /* : Angle_84 */{ return null; };
    static Asin_3056 = function (x_3055/* : Number_30 */) /* : Angle_84 */{ return null; };
    static Atan_3059 = function (x_3058/* : Number_30 */) /* : Angle_84 */{ return null; };
    static Pow_3064 = function (x_3061/* : Number_30 */, y_3063/* : Number_30 */) /* : Number_30 */{ return null; };
    static Log_3069 = function (x_3066/* : Number_30 */, y_3068/* : Number_30 */) /* : Number_30 */{ return null; };
    static NaturalLog_3072 = function (x_3071/* : Number_30 */) /* : Number_30 */{ return null; };
    static NaturalPower_3075 = function (x_3074/* : Number_30 */) /* : Number_30 */{ return null; };
    static Interpolate_3078 = function (xs_3077/* : Array_15 */) /* : String_8 */{ return null; };
    static Throw_3081 = function (x_3080/* : Any_14 */) /* : Any_14 */{ return null; };
    static TypeOf_3084 = function (x_3083/* : Any_14 */) /* : Type_12 */{ return null; };
    static Add_3089 = function (x_3086/* : Number_30 */, y_3088/* : Number_30 */) /* : Number_30 */{ return null; };
    static Subtract_3094 = function (x_3091/* : Number_30 */, y_3093/* : Number_30 */) /* : Number_30 */{ return null; };
    static Divide_3099 = function (x_3096/* : Number_30 */, y_3098/* : Number_30 */) /* : Number_30 */{ return null; };
    static Multiply_3104 = function (x_3101/* : Number_30 */, y_3103/* : Number_30 */) /* : Number_30 */{ return null; };
    static Modulo_3109 = function (x_3106/* : Number_30 */, y_3108/* : Number_30 */) /* : Number_30 */{ return null; };
    static Negative_3112 = function (x_3111/* : Number_30 */) /* : Number_30 */{ return null; };
    static Add_3117 = function (x_3114/* : Integer_27 */, y_3116/* : Integer_27 */) /* : Integer_27 */{ return null; };
    static Subtract_3122 = function (x_3119/* : Integer_27 */, y_3121/* : Integer_27 */) /* : Integer_27 */{ return null; };
    static Divide_3127 = function (x_3124/* : Integer_27 */, y_3126/* : Integer_27 */) /* : Integer_27 */{ return null; };
    static Multiply_3132 = function (x_3129/* : Integer_27 */, y_3131/* : Integer_27 */) /* : Integer_27 */{ return null; };
    static Modulo_3137 = function (x_3134/* : Integer_27 */, y_3136/* : Integer_27 */) /* : Integer_27 */{ return null; };
    static Negative_3140 = function (x_3139/* : Integer_27 */) /* : Integer_27 */{ return null; };
    static And_3145 = function (x_3142/* : Boolean_25 */, y_3144/* : Boolean_25 */) /* : Boolean_25 */{ return null; };
    static Or_3150 = function (x_3147/* : Boolean_25 */, y_3149/* : Boolean_25 */) /* : Boolean_25 */{ return null; };
    static Not_3153 = function (x_3152/* : Boolean_25 */) /* : Boolean_25 */{ return null; };
}
class Array_135_Library
{
    static Map_3773 = function (xs_3749/* : Array_15 */, f_3751/* : Function_4 */) /* : Array_15 */{ return Tuple(function (xs_3168/* : Array_15 */) /* : Count_28 */{ return null; }(xs_3749/* : UnknownType */)/* : UnknownType */, function (i_3758/* : UnknownType */) /* : Lambda_3 */{ return f_3751/* : Function_4 */(function (xs_3171/* : Array_15 */, n_3173/* : Index_29 */) /* : Any_14 */{ return null; }(xs_3749/* : UnknownType */, i_3758/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; })/* : UnknownType */; };
    static Reverse_3806 = function (xs_3775/* : Array_15 */) /* : Array_15 */{ return Tuple(function (xs_3168/* : Array_15 */) /* : Count_28 */{ return null; }(xs_3775/* : UnknownType */)/* : UnknownType */, function (i_3782/* : UnknownType */) /* : Lambda_3 */{ return null; })/* : UnknownType */; };
    static Zip_3837 = function (xs_3808/* : Array_15 */, ys_3810/* : Array_15 */, f_3812/* : Function_4 */) /* : Array_15 */{ return Tuple(function (xs_3168/* : Array_15 */) /* : Count_28 */{ return null; }(xs_3808/* : UnknownType */)/* : UnknownType */, function (i_3819/* : UnknownType */) /* : Lambda_3 */{ return null; })/* : UnknownType */; };
    static Zip_3877 = function (xs_3839/* : Array_15 */, ys_3841/* : Array_15 */, zs_3843/* : Array_15 */, f_3845/* : Function_4 */) /* : Array_15 */{ return Tuple(function (xs_3168/* : Array_15 */) /* : Count_28 */{ return null; }(xs_3839/* : UnknownType */)/* : UnknownType */, function (i_3852/* : UnknownType */) /* : Lambda_3 */{ return null; })/* : UnknownType */; };
    static Skip_3899 = function (xs_3879/* : Array_15 */, n_3881/* : Count_28 */) /* : Array_15 */{ return null; };
    static Take_3917 = function (xs_3901/* : Array_15 */, n_3903/* : Count_28 */) /* : Array_15 */{ return Tuple(n_3903/* : UnknownType */, function (i_3907/* : UnknownType */) /* : Lambda_3 */{ return null; })/* : UnknownType */; };
    static Aggregate_3942 = function (xs_3919/* : Array_15 */, init_3921/* : Any_14 */, f_3923/* : Function_4 */) /* : Any_14 */{ return function (xs_2221/* : UnknownType */) /* : Boolean_25 */{ return null; }(xs_3919/* : UnknownType */)/* : UnknownType */
        ? init_3921/* : Any_14 */
        : f_3923/* : Function_4 */(init_3921/* : UnknownType */, f_3923/* : Function_4 */(function (xs_2215/* : UnknownType */) /* : Array_15 */{ return null; }(xs_3919/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Function_4 */
    ; };
    static Rest_3952 = function (xs_3944/* : Array_15 */) /* : Array_15 */{ return function (xs_3879/* : Array_15 */, n_3881/* : Count_28 */) /* : Array_15 */{ return null; }(xs_3944/* : UnknownType */, 1/* : Integer_10 */)/* : Array_15 */; };
    static IsEmpty_3965 = function (xs_3954/* : Array_15 */) /* : Boolean_25 */{ return null; };
    static First_3975 = function (xs_3967/* : Array_15 */) /* : Any_14 */{ return function (xs_3171/* : Array_15 */, n_3173/* : Index_29 */) /* : Any_14 */{ return null; }(xs_3967/* : UnknownType */, 0/* : Integer_10 */)/* : Any_14 */; };
    static Last_3991 = function (xs_3977/* : Array_15 */) /* : Any_14 */{ return null; };
    static Slice_4009 = function (xs_3993/* : Array_15 */, from_3995/* : Index_29 */, count_3997/* : Count_28 */) /* : Array_15 */{ return function (xs_3901/* : Array_15 */, n_3903/* : Count_28 */) /* : Array_15 */{ return Tuple(n_3903/* : UnknownType */, function (i_3907/* : UnknownType */) /* : Lambda_3 */{ return null; })/* : UnknownType */; }(function (xs_3879/* : Array_15 */, n_3881/* : Count_28 */) /* : Array_15 */{ return null; }(xs_3993/* : UnknownType */, from_3995/* : UnknownType */)/* : UnknownType */, count_3997/* : UnknownType */)/* : Array_15 */; };
    static Join_4057 = function (xs_4011/* : Array_15 */, sep_4013/* : String_8 */) /* : String_8 */{ return function (xs_3954/* : Array_15 */) /* : Boolean_25 */{ return null; }(xs_4011/* : UnknownType */)/* : UnknownType */
        ? ""/* : String_8 */
        : null
    ; };
    static All_4087 = function (xs_4059/* : Array_15 */, f_4061/* : Function_4 */) /* : Boolean_25 */{ return function (xs_3954/* : Array_15 */) /* : Boolean_25 */{ return null; }(xs_4059/* : UnknownType */)/* : UnknownType */
        ? True/* : Boolean_9 */
        : null
    ; };
}
class Interval_136_Library
{
    static Size_4102 = function (x_4089/* : Interval_26 */) /* : Numerical_19 */{ return null; };
    static IsEmpty_4117 = function (x_4104/* : Interval_26 */) /* : Boolean_25 */{ return function (a_2791/* : UnknownType */, b_2793/* : UnknownType */) /* : Boolean_25 */{ return null; }(function (x_3527/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4104/* : UnknownType */)/* : UnknownType */, function (x_3530/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4104/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Lerp_4147 = function (x_4119/* : Interval_26 */, amount_4121/* : Unit_31 */) /* : Numerical_19 */{ return null; };
    static InverseLerp_4167 = function (x_4149/* : Interval_26 */, value_4151/* : Numerical_19 */) /* : Unit_31 */{ return null; };
    static Negate_4186 = function (x_4169/* : Interval_26 */) /* : Interval_26 */{ return null; };
    static Reverse_4201 = function (x_4188/* : Interval_26 */) /* : Interval_26 */{ return Tuple(function (x_3530/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4188/* : UnknownType */)/* : UnknownType */, function (x_3527/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4188/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Center_4211 = function (x_4203/* : Interval_26 */) /* : Numerical_19 */{ return function (x_4119/* : Interval_26 */, amount_4121/* : Unit_31 */) /* : Numerical_19 */{ return null; }(x_4203/* : UnknownType */, 0.5/* : Float_11 */)/* : Numerical_19 */; };
    static Contains_4236 = function (x_4213/* : Interval_26 */, value_4215/* : Numerical_19 */) /* : Boolean_25 */{ return null; };
    static Contains_4260 = function (x_4238/* : Interval_26 */, other_4240/* : Interval_26 */) /* : Boolean_25 */{ return null; };
    static Overlaps_4277 = function (x_4262/* : Interval_26 */, y_4264/* : Interval_26 */) /* : Boolean_25 */{ return null; };
    static Split_4298 = function (x_4279/* : Interval_26 */, t_4281/* : Unit_31 */) /* : Tuple_7 */{ return Tuple(function (x_2349/* : UnknownType */, t_2351/* : UnknownType */) /* : Interval_26 */{ return null; }(x_4279/* : UnknownType */, t_4281/* : UnknownType */)/* : UnknownType */, function (x_2357/* : UnknownType */, t_2359/* : UnknownType */) /* : Interval_26 */{ return null; }(x_4279/* : UnknownType */, t_4281/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Split_4308 = function (x_4300/* : Interval_26 */) /* : Tuple_7 */{ return function (x_4279/* : Interval_26 */, t_4281/* : Unit_31 */) /* : Tuple_7 */{ return Tuple(function (x_2349/* : UnknownType */, t_2351/* : UnknownType */) /* : Interval_26 */{ return null; }(x_4279/* : UnknownType */, t_4281/* : UnknownType */)/* : UnknownType */, function (x_2357/* : UnknownType */, t_2359/* : UnknownType */) /* : Interval_26 */{ return null; }(x_4279/* : UnknownType */, t_4281/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; }(x_4300/* : UnknownType */, 0.5/* : Float_11 */)/* : Tuple_7 */; };
    static Left_4327 = function (x_4310/* : Interval_26 */, t_4312/* : Unit_31 */) /* : Interval_26 */{ return Tuple(function (x_3527/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4310/* : UnknownType */)/* : UnknownType */, function (x_4119/* : Interval_26 */, amount_4121/* : Unit_31 */) /* : Numerical_19 */{ return null; }(x_4310/* : UnknownType */, t_4312/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Right_4346 = function (x_4329/* : Interval_26 */, t_4331/* : Unit_31 */) /* : Interval_26 */{ return Tuple(function (x_4119/* : Interval_26 */, amount_4121/* : Unit_31 */) /* : Numerical_19 */{ return null; }(x_4329/* : UnknownType */, t_4331/* : UnknownType */)/* : UnknownType */, function (x_3530/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4329/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static MoveTo_4363 = function (x_4348/* : Interval_26 */, v_4350/* : Numerical_19 */) /* : Interval_26 */{ return null; };
    static LeftHalf_4373 = function (x_4365/* : Interval_26 */) /* : Interval_26 */{ return function (x_4310/* : Interval_26 */, t_4312/* : Unit_31 */) /* : Interval_26 */{ return Tuple(function (x_3527/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4310/* : UnknownType */)/* : UnknownType */, function (x_4119/* : Interval_26 */, amount_4121/* : Unit_31 */) /* : Numerical_19 */{ return null; }(x_4310/* : UnknownType */, t_4312/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; }(x_4365/* : UnknownType */, 0.5/* : Float_11 */)/* : Interval_26 */; };
    static RightHalf_4383 = function (x_4375/* : Interval_26 */) /* : Interval_26 */{ return function (x_4329/* : Interval_26 */, t_4331/* : Unit_31 */) /* : Interval_26 */{ return Tuple(function (x_4119/* : Interval_26 */, amount_4121/* : Unit_31 */) /* : Numerical_19 */{ return null; }(x_4329/* : UnknownType */, t_4331/* : UnknownType */)/* : UnknownType */, function (x_3530/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4329/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; }(x_4375/* : UnknownType */, 0.5/* : Float_11 */)/* : Interval_26 */; };
    static HalfSize_4393 = function (x_4385/* : Interval_26 */) /* : Numerical_19 */{ return function (x_2559/* : UnknownType */) /* : Numerical_19 */{ return null; }(function (x_4089/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4385/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Recenter_4418 = function (x_4395/* : Interval_26 */, c_4397/* : Numerical_19 */) /* : Interval_26 */{ return null; };
    static Clamp_4445 = function (x_4420/* : Interval_26 */, y_4422/* : Interval_26 */) /* : Interval_26 */{ return Tuple(function (x_2407/* : UnknownType */, value_2409/* : UnknownType */) /* : Numerical_19 */{ return null; }(x_4420/* : UnknownType */, function (x_3527/* : Interval_26 */) /* : Numerical_19 */{ return null; }(y_4422/* : UnknownType */)/* : UnknownType */)/* : UnknownType */, function (x_2407/* : UnknownType */, value_2409/* : UnknownType */) /* : Numerical_19 */{ return null; }(x_4420/* : UnknownType */, function (x_3530/* : Interval_26 */) /* : Numerical_19 */{ return null; }(y_4422/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Clamp_4479 = function (x_4447/* : Interval_26 */, value_4449/* : Numerical_19 */) /* : Numerical_19 */{ return function (a_2767/* : UnknownType */, b_2769/* : UnknownType */) /* : Boolean_25 */{ return null; }(value_4449/* : UnknownType */, function (x_3527/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4447/* : UnknownType */)/* : UnknownType */
        ? function (x_3527/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4447/* : UnknownType */)/* : UnknownType */
        : function (a_2783/* : UnknownType */, b_2785/* : UnknownType */) /* : Boolean_25 */{ return null; }(value_4449/* : UnknownType */, function (x_3530/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4447/* : UnknownType */)/* : UnknownType */
            ? function (x_3530/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4447/* : UnknownType */)/* : UnknownType */
            : value_4449/* : UnknownType */
        )/* : UnknownType */
    )/* : UnknownType */; };
    static Within_4504 = function (x_4481/* : Interval_26 */, value_4483/* : Numerical_19 */) /* : Boolean_25 */{ return null; };
}
class Value_137_Library
{
    static ToString_4517 = function (x_4506/* : Value_16 */) /* : String_8 */{ return function (xs_4011/* : Array_15 */, sep_4013/* : String_8 */) /* : String_8 */{ return function (xs_3954/* : Array_15 */) /* : Boolean_25 */{ return null; }(xs_4011/* : UnknownType */)/* : UnknownType */
        ? ""/* : String_8 */
        : null
    ; }(function (x_3160/* : Any_14 */) /* : Array_15 */{ return null; }(x_4506/* : UnknownType */)/* : UnknownType */, ", "/* : String_8 */)/* : String_8 */; };
}
class Vector_138_Library
{
    static Sum_4527 = function (v_4519/* : Vector_17 */) /* : Number_30 */{ return null; };
    static SumSquares_4540 = function (v_4529/* : Vector_17 */) /* : Number_30 */{ return null; };
    static LengthSquared_4547 = function (v_4542/* : Vector_17 */) /* : Number_30 */{ return function (v_4529/* : Vector_17 */) /* : Number_30 */{ return null; }(v_4542/* : UnknownType */)/* : Number_30 */; };
    static Length_4557 = function (v_4549/* : Vector_17 */) /* : Number_30 */{ return function (x_2467/* : UnknownType */) /* : Numerical_19 */{ return null; }(function (v_4542/* : Vector_17 */) /* : Number_30 */{ return function (v_4529/* : Vector_17 */) /* : Number_30 */{ return null; }(v_4542/* : UnknownType */)/* : Number_30 */; }(v_4549/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Dot_4569 = function (v1_4559/* : Vector_17 */, v2_4561/* : Vector_17 */) /* : Number_30 */{ return null; };
    static Normal_4581 = function (v_4571/* : Vector_17 */) /* : Vector_17 */{ return null; };
}
class Numerical_139_Library
{
    static SquareRoot_4591 = function (x_4583/* : Numerical_19 */) /* : Numerical_19 */{ return function (x_3061/* : Number_30 */, y_3063/* : Number_30 */) /* : Number_30 */{ return null; }(x_4583/* : UnknownType */, 0.5/* : Float_11 */)/* : Number_30 */; };
    static Square_4600 = function (x_4593/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Clamp_4611 = function (x_4602/* : Numerical_19 */, i_4604/* : Interval_26 */) /* : Numerical_19 */{ return function (x_4447/* : Interval_26 */, value_4449/* : Numerical_19 */) /* : Numerical_19 */{ return function (a_2767/* : UnknownType */, b_2769/* : UnknownType */) /* : Boolean_25 */{ return null; }(value_4449/* : UnknownType */, function (x_3527/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4447/* : UnknownType */)/* : UnknownType */
        ? function (x_3527/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4447/* : UnknownType */)/* : UnknownType */
        : function (a_2783/* : UnknownType */, b_2785/* : UnknownType */) /* : Boolean_25 */{ return null; }(value_4449/* : UnknownType */, function (x_3530/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4447/* : UnknownType */)/* : UnknownType */
            ? function (x_3530/* : Interval_26 */) /* : Numerical_19 */{ return null; }(x_4447/* : UnknownType */)/* : UnknownType */
            : value_4449/* : UnknownType */
        )/* : UnknownType */
    )/* : UnknownType */; }(i_4604/* : UnknownType */, x_4602/* : UnknownType */)/* : Numerical_19 */; };
    static Clamp_4625 = function (x_4613/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static PlusOne_4637 = function (x_4627/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static MinusOne_4649 = function (x_4639/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static FromOne_4661 = function (x_4651/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static IsPositive_4671 = function (x_4663/* : Numerical_19 */) /* : Boolean_25 */{ return function (a_2791/* : UnknownType */, b_2793/* : UnknownType */) /* : Boolean_25 */{ return null; }(x_4663/* : UnknownType */, 0/* : Integer_10 */)/* : UnknownType */; };
    static GtZ_4681 = function (x_4673/* : Numerical_19 */) /* : Boolean_25 */{ return function (a_2783/* : UnknownType */, b_2785/* : UnknownType */) /* : Boolean_25 */{ return null; }(x_4673/* : UnknownType */, 0/* : Integer_10 */)/* : UnknownType */; };
    static LtZ_4691 = function (x_4683/* : Numerical_19 */) /* : Boolean_25 */{ return function (a_2767/* : UnknownType */, b_2769/* : UnknownType */) /* : Boolean_25 */{ return null; }(x_4683/* : UnknownType */, 0/* : Integer_10 */)/* : UnknownType */; };
    static GtEqZ_4701 = function (x_4693/* : Numerical_19 */) /* : Boolean_25 */{ return function (a_2791/* : UnknownType */, b_2793/* : UnknownType */) /* : Boolean_25 */{ return null; }(x_4693/* : UnknownType */, 0/* : Integer_10 */)/* : UnknownType */; };
    static LtEqZ_4711 = function (x_4703/* : Numerical_19 */) /* : Boolean_25 */{ return function (a_2775/* : UnknownType */, b_2777/* : UnknownType */) /* : Boolean_25 */{ return null; }(x_4703/* : UnknownType */, 0/* : Integer_10 */)/* : UnknownType */; };
    static IsNegative_4721 = function (x_4713/* : Numerical_19 */) /* : Boolean_25 */{ return function (a_2767/* : UnknownType */, b_2769/* : UnknownType */) /* : Boolean_25 */{ return null; }(x_4713/* : UnknownType */, 0/* : Integer_10 */)/* : UnknownType */; };
    static Sign_4749 = function (x_4723/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Abs_4762 = function (x_4751/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Half_4772 = function (x_4764/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Third_4782 = function (x_4774/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Quarter_4792 = function (x_4784/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Fifth_4802 = function (x_4794/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Sixth_4812 = function (x_4804/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Seventh_4822 = function (x_4814/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Eighth_4832 = function (x_4824/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Ninth_4842 = function (x_4834/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Tenth_4852 = function (x_4844/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Sixteenth_4862 = function (x_4854/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Hundredth_4872 = function (x_4864/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Thousandth_4882 = function (x_4874/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Millionth_4897 = function (x_4884/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Billionth_4916 = function (x_4899/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Hundred_4926 = function (x_4918/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Thousand_4936 = function (x_4928/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Million_4951 = function (x_4938/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Billion_4970 = function (x_4953/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Twice_4980 = function (x_4972/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Thrice_4990 = function (x_4982/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static SmoothStep_5010 = function (x_4992/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Pow2_5019 = function (x_5012/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Pow3_5031 = function (x_5021/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Pow4_5043 = function (x_5033/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Pow5_5055 = function (x_5045/* : Numerical_19 */) /* : Numerical_19 */{ return null; };
    static Pi_5059 = function (self_5057/* : Numerical_139 */) /* : Number_30 */{ return 3.1415926535897/* : Float_11 */; };
    static AlmostZero_5072 = function (x_5061/* : Numerical_19 */) /* : Boolean_25 */{ return function (a_2767/* : UnknownType */, b_2769/* : UnknownType */) /* : Boolean_25 */{ return null; }(function (x_4751/* : Numerical_19 */) /* : Numerical_19 */{ return null; }(x_5061/* : UnknownType */)/* : UnknownType */, 1E-08/* : Float_11 */)/* : UnknownType */; };
    static Lerp_5098 = function (a_5074/* : Numerical_19 */, b_5076/* : Numerical_19 */, t_5078/* : Unit_31 */) /* : Numerical_19 */{ return null; };
    static Between_5139 = function (self_5100/* : Numerical_19 */, min_5102/* : Numerical_19 */, max_5104/* : Numerical_19 */) /* : Boolean_25 */{ return function (xs_3839/* : Array_15 */, ys_3841/* : Array_15 */, zs_3843/* : Array_15 */, f_3845/* : Function_4 */) /* : Array_15 */{ return Tuple(function (xs_3168/* : Array_15 */) /* : Count_28 */{ return null; }(xs_3839/* : UnknownType */)/* : UnknownType */, function (i_3852/* : UnknownType */) /* : Lambda_3 */{ return null; })/* : UnknownType */; }(function (x_3160/* : Any_14 */) /* : Array_15 */{ return null; }(self_5100/* : UnknownType */)/* : UnknownType */, function (x_3160/* : Any_14 */) /* : Array_15 */{ return null; }(min_5102/* : UnknownType */)/* : UnknownType */, function (x_3160/* : Any_14 */) /* : Array_15 */{ return null; }(max_5104/* : UnknownType */)/* : UnknownType */, function (x_5121/* : UnknownType */, y_5123/* : UnknownType */, z_5125/* : UnknownType */) /* : Lambda_3 */{ return function (self_2731/* : UnknownType */, min_2733/* : UnknownType */, max_2735/* : UnknownType */) /* : Boolean_25 */{ return null; }(x_5121/* : UnknownType */, y_5123/* : UnknownType */, z_5125/* : UnknownType */)/* : UnknownType */; })/* : Array_15 */; };
}
class Angles_140_Library
{
    static Radians_5143 = function (x_5141/* : Number_30 */) /* : Angle_84 */{ return x_5141/* : Number_30 */; };
    static Degrees_5153 = function (x_5145/* : Number_30 */) /* : Angle_84 */{ return null; };
    static Turns_5164 = function (x_5155/* : Number_30 */) /* : Angle_84 */{ return null; };
}
class Comparable_141_Library
{
    static Equals_5178 = function (a_5166/* : Comparable_21 */, b_5168/* : Comparable_21 */) /* : Boolean_25 */{ return null; };
    static LessThan_5192 = function (a_5180/* : Comparable_21 */, b_5182/* : Comparable_21 */) /* : Boolean_25 */{ return null; };
    static LessThanOrEquals_5206 = function (a_5194/* : Comparable_21 */, b_5196/* : Comparable_21 */) /* : Boolean_25 */{ return null; };
    static GreaterThan_5220 = function (a_5208/* : Comparable_21 */, b_5210/* : Comparable_21 */) /* : Boolean_25 */{ return null; };
    static GreaterThanOrEquals_5234 = function (a_5222/* : Comparable_21 */, b_5224/* : Comparable_21 */) /* : Boolean_25 */{ return null; };
    static Between_5255 = function (v_5236/* : Comparable_21 */, a_5238/* : Comparable_21 */, b_5240/* : Comparable_21 */) /* : Value_16 */{ return null; };
    static Between_5266 = function (v_5257/* : Value_16 */, i_5259/* : Interval_26 */) /* : Interval_26 */{ return null; };
    static Min_5280 = function (a_5268/* : Comparable_21 */, b_5270/* : Comparable_21 */) /* : Comparable_21 */{ return function (a_5194/* : Comparable_21 */, b_5196/* : Comparable_21 */) /* : Boolean_25 */{ return null; }(a_5268/* : UnknownType */, b_5270/* : UnknownType */
        ? a_5268/* : UnknownType */
        : b_5270/* : UnknownType */
    )/* : Boolean_25 */; };
    static Max_5294 = function (a_5282/* : Comparable_21 */, b_5284/* : Comparable_21 */) /* : Comparable_21 */{ return function (a_5222/* : Comparable_21 */, b_5224/* : Comparable_21 */) /* : Boolean_25 */{ return null; }(a_5282/* : UnknownType */, b_5284/* : UnknownType */
        ? a_5282/* : UnknownType */
        : b_5284/* : UnknownType */
    )/* : Boolean_25 */; };
}
class Equatable_142_Library
{
    static NotEquals_5308 = function (x_5296/* : Equatable_22 */, y_5298/* : Equatable_22 */) /* : Boolean_25 */{ return null; };
}
class Easings_143_Library
{
    static BlendEaseFunc_5357 = function (p_5310/* : Number_30 */, easeIn_5312/* : Function_4 */, easeOut_5314/* : Function_4 */) /* : Number_30 */{ return null; };
    static InvertEaseFunc_5375 = function (p_5359/* : Number_30 */, easeIn_5361/* : Function_4 */) /* : Number_30 */{ return null; };
    static Linear_5379 = function (p_5377/* : Number_30 */) /* : Number_30 */{ return p_5377/* : Number_30 */; };
    static QuadraticEaseIn_5386 = function (p_5381/* : Number_30 */) /* : Number_30 */{ return function (x_5012/* : Numerical_19 */) /* : Numerical_19 */{ return null; }(p_5381/* : UnknownType */)/* : Numerical_19 */; };
    static QuadraticEaseOut_5393 = function (p_5388/* : Number_30 */) /* : Number_30 */{ return null; };
    static QuadraticEaseInOut_5402 = function (p_5395/* : Number_30 */) /* : Number_30 */{ return null; };
    static CubicEaseIn_5409 = function (p_5404/* : Number_30 */) /* : Number_30 */{ return function (x_5021/* : Numerical_19 */) /* : Numerical_19 */{ return null; }(p_5404/* : UnknownType */)/* : Numerical_19 */; };
    static CubicEaseOut_5416 = function (p_5411/* : Number_30 */) /* : Number_30 */{ return null; };
    static CubicEaseInOut_5425 = function (p_5418/* : Number_30 */) /* : Number_30 */{ return null; };
    static QuarticEaseIn_5432 = function (p_5427/* : Number_30 */) /* : Number_30 */{ return function (x_5033/* : Numerical_19 */) /* : Numerical_19 */{ return null; }(p_5427/* : UnknownType */)/* : Numerical_19 */; };
    static QuarticEaseOut_5439 = function (p_5434/* : Number_30 */) /* : Number_30 */{ return null; };
    static QuarticEaseInOut_5448 = function (p_5441/* : Number_30 */) /* : Number_30 */{ return null; };
    static QuinticEaseIn_5455 = function (p_5450/* : Number_30 */) /* : Number_30 */{ return function (x_5045/* : Numerical_19 */) /* : Numerical_19 */{ return null; }(p_5450/* : UnknownType */)/* : Numerical_19 */; };
    static QuinticEaseOut_5462 = function (p_5457/* : Number_30 */) /* : Number_30 */{ return null; };
    static QuinticEaseInOut_5471 = function (p_5464/* : Number_30 */) /* : Number_30 */{ return null; };
    static SineEaseIn_5478 = function (p_5473/* : Number_30 */) /* : Number_30 */{ return null; };
    static SineEaseOut_5491 = function (p_5480/* : Number_30 */) /* : Number_30 */{ return function (x_3046/* : Angle_84 */) /* : Number_30 */{ return null; }(function (x_5155/* : Number_30 */) /* : Angle_84 */{ return null; }(function (x_4784/* : Numerical_19 */) /* : Numerical_19 */{ return null; }(p_5480/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Number_30 */; };
    static SineEaseInOut_5500 = function (p_5493/* : Number_30 */) /* : Number_30 */{ return null; };
    static CircularEaseIn_5516 = function (p_5502/* : Number_30 */) /* : Number_30 */{ return function (x_4651/* : Numerical_19 */) /* : Numerical_19 */{ return null; }(function (x_4583/* : Numerical_19 */) /* : Numerical_19 */{ return function (x_3061/* : Number_30 */, y_3063/* : Number_30 */) /* : Number_30 */{ return null; }(x_4583/* : UnknownType */, 0.5/* : Float_11 */)/* : Number_30 */; }(function (x_4651/* : Numerical_19 */) /* : Numerical_19 */{ return null; }(function (x_5012/* : Numerical_19 */) /* : Numerical_19 */{ return null; }(p_5502/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : Numerical_19 */; };
    static CircularEaseOut_5523 = function (p_5518/* : Number_30 */) /* : Number_30 */{ return null; };
    static CircularEaseInOut_5532 = function (p_5525/* : Number_30 */) /* : Number_30 */{ return null; };
    static ExponentialEaseIn_5556 = function (p_5534/* : Number_30 */) /* : Number_30 */{ return function (x_5061/* : Numerical_19 */) /* : Boolean_25 */{ return function (a_2767/* : UnknownType */, b_2769/* : UnknownType */) /* : Boolean_25 */{ return null; }(function (x_4751/* : Numerical_19 */) /* : Numerical_19 */{ return null; }(x_5061/* : UnknownType */)/* : UnknownType */, 1E-08/* : Float_11 */)/* : UnknownType */; }(p_5534/* : UnknownType */)/* : UnknownType */
        ? p_5534/* : Number_30 */
        : null
    ; };
    static ExponentialEaseOut_5563 = function (p_5558/* : Number_30 */) /* : Number_30 */{ return null; };
    static ExponentialEaseInOut_5572 = function (p_5565/* : Number_30 */) /* : Number_30 */{ return null; };
    static ElasticEaseIn_5605 = function (p_5574/* : Number_30 */) /* : Number_30 */{ return null; };
    static ElasticEaseOut_5612 = function (p_5607/* : Number_30 */) /* : Number_30 */{ return null; };
    static ElasticEaseInOut_5621 = function (p_5614/* : Number_30 */) /* : Number_30 */{ return null; };
    static BackEaseIn_5647 = function (p_5623/* : Number_30 */) /* : Number_30 */{ return null; };
    static BackEaseOut_5654 = function (p_5649/* : Number_30 */) /* : Number_30 */{ return null; };
    static BackEaseInOut_5663 = function (p_5656/* : Number_30 */) /* : Number_30 */{ return null; };
    static BounceEaseIn_5670 = function (p_5665/* : Number_30 */) /* : Number_30 */{ return null; };
    static BounceEaseOut_5828 = function (p_5672/* : Number_30 */) /* : Number_30 */{ return null; };
    static BounceEaseInOut_5837 = function (p_5830/* : Number_30 */) /* : Number_30 */{ return null; };
}
class Any_14_Concept
{
    constructor(self) { this.Self = self; };
    static FieldNames_3158 = function (self_3157/* : Any_14 */) /* : Array_15 */{ return null; };
    static FieldValues_3161 = function (x_3160/* : Any_14 */) /* : Array_15 */{ return null; };
    static TypeOf_3164 = function (self_3163/* : Any_14 */) /* : Type_12 */{ return null; };
}
class Array_15_Concept
{
    constructor(self) { this.Self = self; };
    static Count_3169 = function (xs_3168/* : Array_15 */) /* : Count_28 */{ return null; };
    static At_3174 = function (xs_3171/* : Array_15 */, n_3173/* : Index_29 */) /* : Any_14 */{ return null; };
}
class Value_16_Concept
{
    constructor(self) { this.Self = self; };
    static Default_3186 = function (self_3178/* : Value_16 */) /* : Value_16 */{ return function (self_379/* : UnknownType */) /* : Value_16 */{ return null; }(function (x_3160/* : Any_14 */) /* : Array_15 */{ return null; }(Self_3176/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Vector_17_Concept
{
    constructor(self) { this.Self = self; };
    static Count_3198 = function (v_3190/* : Vector_17 */) /* : Count_28 */{ return function (xs_3168/* : Array_15 */) /* : Count_28 */{ return null; }(function (self_408/* : UnknownType */) /* : Array_15 */{ return null; }(Self_3188/* : UnknownType */)/* : UnknownType */)/* : Count_28 */; };
    static At_3212 = function (v_3200/* : Vector_17 */, n_3202/* : Index_29 */) /* : Numerical_19 */{ return function (xs_3171/* : Array_15 */, n_3173/* : Index_29 */) /* : Any_14 */{ return null; }(function (x_3160/* : Any_14 */) /* : Array_15 */{ return null; }(v_3200/* : UnknownType */)/* : UnknownType */, n_3202/* : UnknownType */)/* : Any_14 */; };
}
class Measure_18_Concept
{
    constructor(self) { this.Self = self; };
    static Value_3227 = function (x_3216/* : Measure_18 */) /* : Number_30 */{ return function (xs_3171/* : Array_15 */, n_3173/* : Index_29 */) /* : Any_14 */{ return null; }(function (x_3160/* : Any_14 */) /* : Array_15 */{ return null; }(x_3216/* : UnknownType */)/* : UnknownType */, 0/* : Integer_10 */)/* : Any_14 */; };
}
class Numerical_19_Concept
{
    constructor(self) { this.Self = self; };
    static FieldTypes_3232 = function (self_3231/* : Numerical_19 */) /* : Array_15 */{ return null; };
    static Zero_3242 = function (self_3234/* : Numerical_19 */) /* : Numerical_19 */{ return function (self_414/* : UnknownType */) /* : Numerical_19 */{ return null; }(function (self_3231/* : Numerical_19 */) /* : Array_15 */{ return null; }(Self_3229/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static One_3252 = function (self_3244/* : Numerical_19 */) /* : Numerical_19 */{ return function (self_420/* : UnknownType */) /* : Numerical_19 */{ return null; }(function (self_3231/* : Numerical_19 */) /* : Array_15 */{ return null; }(Self_3229/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static MinValue_3262 = function (self_3254/* : Numerical_19 */) /* : Numerical_19 */{ return function (self_426/* : UnknownType */) /* : Numerical_19 */{ return null; }(function (self_3231/* : Numerical_19 */) /* : Array_15 */{ return null; }(Self_3229/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static MaxValue_3272 = function (self_3264/* : Numerical_19 */) /* : Numerical_19 */{ return function (self_432/* : UnknownType */) /* : Numerical_19 */{ return null; }(function (self_3231/* : Numerical_19 */) /* : Array_15 */{ return null; }(Self_3229/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Magnitudinal_20_Concept
{
    constructor(self) { this.Self = self; };
    static Magnitude_3290 = function (x_3276/* : Magnitudinal_20 */) /* : Number_30 */{ return function (x_2467/* : UnknownType */) /* : Numerical_19 */{ return null; }(function (v_2429/* : UnknownType */) /* : Number_30 */{ return null; }(function (x_2473/* : UnknownType */) /* : Numerical_19 */{ return null; }(function (x_3160/* : Any_14 */) /* : Array_15 */{ return null; }(x_3276/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
}
class Comparable_21_Concept
{
    constructor(self) { this.Self = self; };
    static Compare_3295 = function (x_3294/* : Comparable_21 */) /* : Integer_27 */{ return null; };
}
class Equatable_22_Concept
{
    constructor(self) { this.Self = self; };
    static Equals_3315 = function (a_3299/* : Equatable_22 */, b_3301/* : Equatable_22 */) /* : Boolean_25 */{ return null; };
}
class Arithmetic_23_Concept
{
    constructor(self) { this.Self = self; };
    static Add_3334 = function (self_3319/* : Arithmetic_23 */, other_3321/* : Arithmetic_23 */) /* : Arithmetic_23 */{ return null; };
    static Negative_3344 = function (self_3336/* : Arithmetic_23 */) /* : Arithmetic_23 */{ return null; };
    static Reciprocal_3354 = function (self_3346/* : Arithmetic_23 */) /* : Arithmetic_23 */{ return function (self_472/* : UnknownType */) /* : Arithmetic_23 */{ return null; }(function (x_3160/* : Any_14 */) /* : Array_15 */{ return null; }(self_3346/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; };
    static Multiply_3371 = function (self_3356/* : Arithmetic_23 */, other_3358/* : Arithmetic_23 */) /* : Arithmetic_23 */{ return null; };
    static Divide_3388 = function (self_3373/* : Arithmetic_23 */, other_3375/* : Arithmetic_23 */) /* : Arithmetic_23 */{ return null; };
    static Modulo_3405 = function (self_3390/* : Arithmetic_23 */, other_3392/* : Arithmetic_23 */) /* : Arithmetic_23 */{ return null; };
}
class ScalarArithmetic_24_Concept
{
    constructor(self) { this.Self = self; };
    static Add_3421 = function (self_3409/* : ScalarArithmetic_24 */, scalar_3411/* : Number_30 */) /* : ScalarArithmetic_24 */{ return null; };
    static Subtract_3435 = function (self_3423/* : ScalarArithmetic_24 */, scalar_3425/* : Number_30 */) /* : ScalarArithmetic_24 */{ return function (self_3409/* : ScalarArithmetic_24 */, scalar_3411/* : Number_30 */) /* : ScalarArithmetic_24 */{ return null; }(self_3423/* : UnknownType */, function (x_3111/* : Number_30 */) /* : Number_30 */{ return null; }(scalar_3425/* : UnknownType */)/* : UnknownType */)/* : ScalarArithmetic_24 */; };
    static Multiply_3449 = function (self_3437/* : ScalarArithmetic_24 */, scalar_3439/* : Number_30 */) /* : ScalarArithmetic_24 */{ return null; };
    static Divide_3463 = function (self_3451/* : ScalarArithmetic_24 */, scalar_3453/* : Number_30 */) /* : ScalarArithmetic_24 */{ return function (self_3437/* : ScalarArithmetic_24 */, scalar_3439/* : Number_30 */) /* : ScalarArithmetic_24 */{ return null; }(self_3451/* : UnknownType */, function (self_3346/* : Arithmetic_23 */) /* : Arithmetic_23 */{ return function (self_472/* : UnknownType */) /* : Arithmetic_23 */{ return null; }(function (x_3160/* : Any_14 */) /* : Array_15 */{ return null; }(self_3346/* : UnknownType */)/* : UnknownType */)/* : UnknownType */; }(scalar_3453/* : UnknownType */)/* : UnknownType */)/* : ScalarArithmetic_24 */; };
    static Modulo_3477 = function (self_3465/* : ScalarArithmetic_24 */, scalar_3467/* : Number_30 */) /* : ScalarArithmetic_24 */{ return null; };
}
class Boolean_25_Concept
{
    constructor(self) { this.Self = self; };
    static And_3496 = function (a_3481/* : Boolean_25 */, b_3483/* : Boolean_25 */) /* : Boolean_25 */{ return null; };
    static Or_3513 = function (a_3498/* : Boolean_25 */, b_3500/* : Boolean_25 */) /* : Boolean_25 */{ return null; };
    static Not_3523 = function (a_3515/* : Boolean_25 */) /* : Boolean_25 */{ return null; };
}
class Interval_26_Concept
{
    constructor(self) { this.Self = self; };
    static Min_3528 = function (x_3527/* : Interval_26 */) /* : Numerical_19 */{ return null; };
    static Max_3531 = function (x_3530/* : Interval_26 */) /* : Numerical_19 */{ return null; };
}
class Integer_27_Type
{
    constructor(Value_576)
    {
        // field initialization 
        this.Value_576 = Value_576;
        this.Default_3186 = Integer_27_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = Integer_27_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = Integer_27_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = Integer_27_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = Integer_27_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = Integer_27_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = Integer_27_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = Integer_27_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Integer_27_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Integer_27_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = Integer_27_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Integer_27_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Integer_27_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Integer_27_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Integer_27_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = Integer_27_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = Integer_27_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = Integer_27_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = Integer_27_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = Integer_27_Type.Numerical_19_Concept.MaxValue_3272;
    }
    // field accessors
    static Value_576 = function(self) { return self.Value_576; }
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
    constructor(Value_583)
    {
        // field initialization 
        this.Value_583 = Value_583;
        this.Default_3186 = Count_28_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = Count_28_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = Count_28_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = Count_28_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = Count_28_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = Count_28_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = Count_28_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = Count_28_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Count_28_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Count_28_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = Count_28_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Count_28_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Count_28_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Count_28_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Count_28_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = Count_28_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = Count_28_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = Count_28_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = Count_28_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = Count_28_Type.Numerical_19_Concept.MaxValue_3272;
    }
    // field accessors
    static Value_583 = function(self) { return self.Value_583; }
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
    constructor(Value_590)
    {
        // field initialization 
        this.Value_590 = Value_590;
        this.Default_3186 = Index_29_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Value_590 = function(self) { return self.Value_590; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Index_29_Type);
    static Implements = [Value_16_Concept];
}
class Number_30_Type
{
    constructor(Value_597)
    {
        // field initialization 
        this.Value_597 = Value_597;
        this.Default_3186 = Number_30_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = Number_30_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = Number_30_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = Number_30_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = Number_30_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = Number_30_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = Number_30_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = Number_30_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Number_30_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Number_30_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = Number_30_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Number_30_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Number_30_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Number_30_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Number_30_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = Number_30_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = Number_30_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = Number_30_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = Number_30_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = Number_30_Type.Numerical_19_Concept.MaxValue_3272;
    }
    // field accessors
    static Value_597 = function(self) { return self.Value_597; }
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
    constructor(Value_604)
    {
        // field initialization 
        this.Value_604 = Value_604;
        this.Default_3186 = Unit_31_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = Unit_31_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = Unit_31_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = Unit_31_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = Unit_31_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = Unit_31_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = Unit_31_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = Unit_31_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Unit_31_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Unit_31_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = Unit_31_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Unit_31_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Unit_31_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Unit_31_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Unit_31_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = Unit_31_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = Unit_31_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = Unit_31_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = Unit_31_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = Unit_31_Type.Numerical_19_Concept.MaxValue_3272;
    }
    // field accessors
    static Value_604 = function(self) { return self.Value_604; }
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
    constructor(Value_611)
    {
        // field initialization 
        this.Value_611 = Value_611;
        this.Default_3186 = Percent_32_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = Percent_32_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = Percent_32_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = Percent_32_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = Percent_32_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = Percent_32_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = Percent_32_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = Percent_32_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Percent_32_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Percent_32_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = Percent_32_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Percent_32_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Percent_32_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Percent_32_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Percent_32_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = Percent_32_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = Percent_32_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = Percent_32_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = Percent_32_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = Percent_32_Type.Numerical_19_Concept.MaxValue_3272;
    }
    // field accessors
    static Value_611 = function(self) { return self.Value_611; }
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
    constructor(X_618, Y_625, Z_632, W_639)
    {
        // field initialization 
        this.X_618 = X_618;
        this.Y_625 = Y_625;
        this.Z_632 = Z_632;
        this.W_639 = W_639;
        this.Default_3186 = Quaternion_33_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static X_618 = function(self) { return self.X_618; }
    static Y_625 = function(self) { return self.Y_625; }
    static Z_632 = function(self) { return self.Z_632; }
    static W_639 = function(self) { return self.W_639; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Quaternion_33_Type);
    static Implements = [Value_16_Concept];
}
class Unit2D_34_Type
{
    constructor(X_646, Y_653)
    {
        // field initialization 
        this.X_646 = X_646;
        this.Y_653 = Y_653;
        this.Default_3186 = Unit2D_34_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static X_646 = function(self) { return self.X_646; }
    static Y_653 = function(self) { return self.Y_653; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Unit2D_34_Type);
    static Implements = [Value_16_Concept];
}
class Unit3D_35_Type
{
    constructor(X_660, Y_667, Z_674)
    {
        // field initialization 
        this.X_660 = X_660;
        this.Y_667 = Y_667;
        this.Z_674 = Z_674;
        this.Default_3186 = Unit3D_35_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static X_660 = function(self) { return self.X_660; }
    static Y_667 = function(self) { return self.Y_667; }
    static Z_674 = function(self) { return self.Z_674; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Unit3D_35_Type);
    static Implements = [Value_16_Concept];
}
class Direction3D_36_Type
{
    constructor(Value_681)
    {
        // field initialization 
        this.Value_681 = Value_681;
        this.Default_3186 = Direction3D_36_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Value_681 = function(self) { return self.Value_681; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Direction3D_36_Type);
    static Implements = [Value_16_Concept];
}
class AxisAngle_37_Type
{
    constructor(Axis_688, Angle_695)
    {
        // field initialization 
        this.Axis_688 = Axis_688;
        this.Angle_695 = Angle_695;
        this.Default_3186 = AxisAngle_37_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Axis_688 = function(self) { return self.Axis_688; }
    static Angle_695 = function(self) { return self.Angle_695; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(AxisAngle_37_Type);
    static Implements = [Value_16_Concept];
}
class EulerAngles_38_Type
{
    constructor(Yaw_702, Pitch_709, Roll_716)
    {
        // field initialization 
        this.Yaw_702 = Yaw_702;
        this.Pitch_709 = Pitch_709;
        this.Roll_716 = Roll_716;
        this.Default_3186 = EulerAngles_38_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Yaw_702 = function(self) { return self.Yaw_702; }
    static Pitch_709 = function(self) { return self.Pitch_709; }
    static Roll_716 = function(self) { return self.Roll_716; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(EulerAngles_38_Type);
    static Implements = [Value_16_Concept];
}
class Rotation3D_39_Type
{
    constructor(Quaternion_723)
    {
        // field initialization 
        this.Quaternion_723 = Quaternion_723;
        this.Default_3186 = Rotation3D_39_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Quaternion_723 = function(self) { return self.Quaternion_723; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Rotation3D_39_Type);
    static Implements = [Value_16_Concept];
}
class Vector2D_40_Type
{
    constructor(X_730, Y_737)
    {
        // field initialization 
        this.X_730 = X_730;
        this.Y_737 = Y_737;
        this.Count_3169 = Vector2D_40_Type.Array_15_Concept.Count_3169;
        this.At_3174 = Vector2D_40_Type.Array_15_Concept.At_3174;
        this.Default_3186 = Vector2D_40_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = Vector2D_40_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = Vector2D_40_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = Vector2D_40_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = Vector2D_40_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = Vector2D_40_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = Vector2D_40_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = Vector2D_40_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Vector2D_40_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Vector2D_40_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = Vector2D_40_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Vector2D_40_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Vector2D_40_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Vector2D_40_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Vector2D_40_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = Vector2D_40_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = Vector2D_40_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = Vector2D_40_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = Vector2D_40_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = Vector2D_40_Type.Numerical_19_Concept.MaxValue_3272;
        this.Count_3198 = Vector2D_40_Type.Vector_17_Concept.Count_3198;
        this.At_3212 = Vector2D_40_Type.Vector_17_Concept.At_3212;
    }
    // field accessors
    static X_730 = function(self) { return self.X_730; }
    static Y_737 = function(self) { return self.Y_737; }
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
    constructor(X_744, Y_751, Z_758)
    {
        // field initialization 
        this.X_744 = X_744;
        this.Y_751 = Y_751;
        this.Z_758 = Z_758;
        this.Count_3169 = Vector3D_41_Type.Array_15_Concept.Count_3169;
        this.At_3174 = Vector3D_41_Type.Array_15_Concept.At_3174;
        this.Default_3186 = Vector3D_41_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = Vector3D_41_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = Vector3D_41_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = Vector3D_41_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = Vector3D_41_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = Vector3D_41_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = Vector3D_41_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = Vector3D_41_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Vector3D_41_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Vector3D_41_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = Vector3D_41_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Vector3D_41_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Vector3D_41_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Vector3D_41_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Vector3D_41_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = Vector3D_41_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = Vector3D_41_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = Vector3D_41_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = Vector3D_41_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = Vector3D_41_Type.Numerical_19_Concept.MaxValue_3272;
        this.Count_3198 = Vector3D_41_Type.Vector_17_Concept.Count_3198;
        this.At_3212 = Vector3D_41_Type.Vector_17_Concept.At_3212;
    }
    // field accessors
    static X_744 = function(self) { return self.X_744; }
    static Y_751 = function(self) { return self.Y_751; }
    static Z_758 = function(self) { return self.Z_758; }
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
    constructor(X_765, Y_772, Z_779, W_786)
    {
        // field initialization 
        this.X_765 = X_765;
        this.Y_772 = Y_772;
        this.Z_779 = Z_779;
        this.W_786 = W_786;
        this.Count_3169 = Vector4D_42_Type.Array_15_Concept.Count_3169;
        this.At_3174 = Vector4D_42_Type.Array_15_Concept.At_3174;
        this.Default_3186 = Vector4D_42_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = Vector4D_42_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = Vector4D_42_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = Vector4D_42_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = Vector4D_42_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = Vector4D_42_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = Vector4D_42_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = Vector4D_42_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Vector4D_42_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Vector4D_42_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = Vector4D_42_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Vector4D_42_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Vector4D_42_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Vector4D_42_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Vector4D_42_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = Vector4D_42_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = Vector4D_42_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = Vector4D_42_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = Vector4D_42_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = Vector4D_42_Type.Numerical_19_Concept.MaxValue_3272;
        this.Count_3198 = Vector4D_42_Type.Vector_17_Concept.Count_3198;
        this.At_3212 = Vector4D_42_Type.Vector_17_Concept.At_3212;
    }
    // field accessors
    static X_765 = function(self) { return self.X_765; }
    static Y_772 = function(self) { return self.Y_772; }
    static Z_779 = function(self) { return self.Z_779; }
    static W_786 = function(self) { return self.W_786; }
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
    constructor(Value_793)
    {
        // field initialization 
        this.Value_793 = Value_793;
        this.Default_3186 = Orientation3D_43_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Value_793 = function(self) { return self.Value_793; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Orientation3D_43_Type);
    static Implements = [Value_16_Concept];
}
class Pose2D_44_Type
{
    constructor(Position_800, Orientation_807)
    {
        // field initialization 
        this.Position_800 = Position_800;
        this.Orientation_807 = Orientation_807;
        this.Default_3186 = Pose2D_44_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Position_800 = function(self) { return self.Position_800; }
    static Orientation_807 = function(self) { return self.Orientation_807; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Pose2D_44_Type);
    static Implements = [Value_16_Concept];
}
class Pose3D_45_Type
{
    constructor(Position_814, Orientation_821)
    {
        // field initialization 
        this.Position_814 = Position_814;
        this.Orientation_821 = Orientation_821;
        this.Default_3186 = Pose3D_45_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Position_814 = function(self) { return self.Position_814; }
    static Orientation_821 = function(self) { return self.Orientation_821; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Pose3D_45_Type);
    static Implements = [Value_16_Concept];
}
class Transform3D_46_Type
{
    constructor(Translation_828, Rotation_835, Scale_842)
    {
        // field initialization 
        this.Translation_828 = Translation_828;
        this.Rotation_835 = Rotation_835;
        this.Scale_842 = Scale_842;
        this.Default_3186 = Transform3D_46_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Translation_828 = function(self) { return self.Translation_828; }
    static Rotation_835 = function(self) { return self.Rotation_835; }
    static Scale_842 = function(self) { return self.Scale_842; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Transform3D_46_Type);
    static Implements = [Value_16_Concept];
}
class Transform2D_47_Type
{
    constructor(Translation_849, Rotation_856, Scale_863)
    {
        // field initialization 
        this.Translation_849 = Translation_849;
        this.Rotation_856 = Rotation_856;
        this.Scale_863 = Scale_863;
        this.Default_3186 = Transform2D_47_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Translation_849 = function(self) { return self.Translation_849; }
    static Rotation_856 = function(self) { return self.Rotation_856; }
    static Scale_863 = function(self) { return self.Scale_863; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Transform2D_47_Type);
    static Implements = [Value_16_Concept];
}
class AlignedBox2D_48_Type
{
    constructor(A_870, B_877)
    {
        // field initialization 
        this.A_870 = A_870;
        this.B_877 = B_877;
        this.Count_3169 = AlignedBox2D_48_Type.Array_15_Concept.Count_3169;
        this.At_3174 = AlignedBox2D_48_Type.Array_15_Concept.At_3174;
        this.Default_3186 = AlignedBox2D_48_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = AlignedBox2D_48_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = AlignedBox2D_48_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = AlignedBox2D_48_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = AlignedBox2D_48_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = AlignedBox2D_48_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = AlignedBox2D_48_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = AlignedBox2D_48_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = AlignedBox2D_48_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = AlignedBox2D_48_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = AlignedBox2D_48_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = AlignedBox2D_48_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = AlignedBox2D_48_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = AlignedBox2D_48_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = AlignedBox2D_48_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = AlignedBox2D_48_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = AlignedBox2D_48_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = AlignedBox2D_48_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = AlignedBox2D_48_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = AlignedBox2D_48_Type.Numerical_19_Concept.MaxValue_3272;
        this.Count_3198 = AlignedBox2D_48_Type.Vector_17_Concept.Count_3198;
        this.At_3212 = AlignedBox2D_48_Type.Vector_17_Concept.At_3212;
        this.Min_3528 = AlignedBox2D_48_Type.Interval_26_Concept.Min_3528;
        this.Max_3531 = AlignedBox2D_48_Type.Interval_26_Concept.Max_3531;
    }
    // field accessors
    static A_870 = function(self) { return self.A_870; }
    static B_877 = function(self) { return self.B_877; }
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
    constructor(A_884, B_891)
    {
        // field initialization 
        this.A_884 = A_884;
        this.B_891 = B_891;
        this.Count_3169 = AlignedBox3D_49_Type.Array_15_Concept.Count_3169;
        this.At_3174 = AlignedBox3D_49_Type.Array_15_Concept.At_3174;
        this.Default_3186 = AlignedBox3D_49_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = AlignedBox3D_49_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = AlignedBox3D_49_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = AlignedBox3D_49_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = AlignedBox3D_49_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = AlignedBox3D_49_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = AlignedBox3D_49_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = AlignedBox3D_49_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = AlignedBox3D_49_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = AlignedBox3D_49_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = AlignedBox3D_49_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = AlignedBox3D_49_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = AlignedBox3D_49_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = AlignedBox3D_49_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = AlignedBox3D_49_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = AlignedBox3D_49_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = AlignedBox3D_49_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = AlignedBox3D_49_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = AlignedBox3D_49_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = AlignedBox3D_49_Type.Numerical_19_Concept.MaxValue_3272;
        this.Count_3198 = AlignedBox3D_49_Type.Vector_17_Concept.Count_3198;
        this.At_3212 = AlignedBox3D_49_Type.Vector_17_Concept.At_3212;
        this.Min_3528 = AlignedBox3D_49_Type.Interval_26_Concept.Min_3528;
        this.Max_3531 = AlignedBox3D_49_Type.Interval_26_Concept.Max_3531;
    }
    // field accessors
    static A_884 = function(self) { return self.A_884; }
    static B_891 = function(self) { return self.B_891; }
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
    constructor(Real_898, Imaginary_905)
    {
        // field initialization 
        this.Real_898 = Real_898;
        this.Imaginary_905 = Imaginary_905;
        this.Count_3169 = Complex_50_Type.Array_15_Concept.Count_3169;
        this.At_3174 = Complex_50_Type.Array_15_Concept.At_3174;
        this.Default_3186 = Complex_50_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = Complex_50_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = Complex_50_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = Complex_50_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = Complex_50_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = Complex_50_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = Complex_50_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = Complex_50_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Complex_50_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Complex_50_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = Complex_50_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Complex_50_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Complex_50_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Complex_50_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Complex_50_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = Complex_50_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = Complex_50_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = Complex_50_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = Complex_50_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = Complex_50_Type.Numerical_19_Concept.MaxValue_3272;
        this.Count_3198 = Complex_50_Type.Vector_17_Concept.Count_3198;
        this.At_3212 = Complex_50_Type.Vector_17_Concept.At_3212;
    }
    // field accessors
    static Real_898 = function(self) { return self.Real_898; }
    static Imaginary_905 = function(self) { return self.Imaginary_905; }
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
    constructor(Direction_912, Position_919)
    {
        // field initialization 
        this.Direction_912 = Direction_912;
        this.Position_919 = Position_919;
        this.Default_3186 = Ray3D_51_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Direction_912 = function(self) { return self.Direction_912; }
    static Position_919 = function(self) { return self.Position_919; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Ray3D_51_Type);
    static Implements = [Value_16_Concept];
}
class Ray2D_52_Type
{
    constructor(Direction_926, Position_933)
    {
        // field initialization 
        this.Direction_926 = Direction_926;
        this.Position_933 = Position_933;
        this.Default_3186 = Ray2D_52_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Direction_926 = function(self) { return self.Direction_926; }
    static Position_933 = function(self) { return self.Position_933; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Ray2D_52_Type);
    static Implements = [Value_16_Concept];
}
class Sphere_53_Type
{
    constructor(Center_940, Radius_947)
    {
        // field initialization 
        this.Center_940 = Center_940;
        this.Radius_947 = Radius_947;
        this.Default_3186 = Sphere_53_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Center_940 = function(self) { return self.Center_940; }
    static Radius_947 = function(self) { return self.Radius_947; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Sphere_53_Type);
    static Implements = [Value_16_Concept];
}
class Plane_54_Type
{
    constructor(Normal_954, D_961)
    {
        // field initialization 
        this.Normal_954 = Normal_954;
        this.D_961 = D_961;
        this.Default_3186 = Plane_54_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Normal_954 = function(self) { return self.Normal_954; }
    static D_961 = function(self) { return self.D_961; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Plane_54_Type);
    static Implements = [Value_16_Concept];
}
class Triangle3D_55_Type
{
    constructor(A_968, B_975, C_982)
    {
        // field initialization 
        this.A_968 = A_968;
        this.B_975 = B_975;
        this.C_982 = C_982;
        this.Default_3186 = Triangle3D_55_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static A_968 = function(self) { return self.A_968; }
    static B_975 = function(self) { return self.B_975; }
    static C_982 = function(self) { return self.C_982; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Triangle3D_55_Type);
    static Implements = [Value_16_Concept];
}
class Triangle2D_56_Type
{
    constructor(A_989, B_996, C_1003)
    {
        // field initialization 
        this.A_989 = A_989;
        this.B_996 = B_996;
        this.C_1003 = C_1003;
        this.Default_3186 = Triangle2D_56_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static A_989 = function(self) { return self.A_989; }
    static B_996 = function(self) { return self.B_996; }
    static C_1003 = function(self) { return self.C_1003; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Triangle2D_56_Type);
    static Implements = [Value_16_Concept];
}
class Quad3D_57_Type
{
    constructor(A_1010, B_1017, C_1024, D_1031)
    {
        // field initialization 
        this.A_1010 = A_1010;
        this.B_1017 = B_1017;
        this.C_1024 = C_1024;
        this.D_1031 = D_1031;
        this.Default_3186 = Quad3D_57_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static A_1010 = function(self) { return self.A_1010; }
    static B_1017 = function(self) { return self.B_1017; }
    static C_1024 = function(self) { return self.C_1024; }
    static D_1031 = function(self) { return self.D_1031; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Quad3D_57_Type);
    static Implements = [Value_16_Concept];
}
class Quad2D_58_Type
{
    constructor(A_1038, B_1045, C_1052, D_1059)
    {
        // field initialization 
        this.A_1038 = A_1038;
        this.B_1045 = B_1045;
        this.C_1052 = C_1052;
        this.D_1059 = D_1059;
        this.Default_3186 = Quad2D_58_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static A_1038 = function(self) { return self.A_1038; }
    static B_1045 = function(self) { return self.B_1045; }
    static C_1052 = function(self) { return self.C_1052; }
    static D_1059 = function(self) { return self.D_1059; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Quad2D_58_Type);
    static Implements = [Value_16_Concept];
}
class Point3D_59_Type
{
    constructor(Value_1066)
    {
        // field initialization 
        this.Value_1066 = Value_1066;
        this.Default_3186 = Point3D_59_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Value_1066 = function(self) { return self.Value_1066; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Point3D_59_Type);
    static Implements = [Value_16_Concept];
}
class Point2D_60_Type
{
    constructor(Value_1073)
    {
        // field initialization 
        this.Value_1073 = Value_1073;
        this.Default_3186 = Point2D_60_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Value_1073 = function(self) { return self.Value_1073; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Point2D_60_Type);
    static Implements = [Value_16_Concept];
}
class Line3D_61_Type
{
    constructor(A_1080, B_1087)
    {
        // field initialization 
        this.A_1080 = A_1080;
        this.B_1087 = B_1087;
        this.Count_3169 = Line3D_61_Type.Array_15_Concept.Count_3169;
        this.At_3174 = Line3D_61_Type.Array_15_Concept.At_3174;
        this.Default_3186 = Line3D_61_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = Line3D_61_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = Line3D_61_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = Line3D_61_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = Line3D_61_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = Line3D_61_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = Line3D_61_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = Line3D_61_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Line3D_61_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Line3D_61_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = Line3D_61_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Line3D_61_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Line3D_61_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Line3D_61_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Line3D_61_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = Line3D_61_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = Line3D_61_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = Line3D_61_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = Line3D_61_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = Line3D_61_Type.Numerical_19_Concept.MaxValue_3272;
        this.Count_3198 = Line3D_61_Type.Vector_17_Concept.Count_3198;
        this.At_3212 = Line3D_61_Type.Vector_17_Concept.At_3212;
        this.Min_3528 = Line3D_61_Type.Interval_26_Concept.Min_3528;
        this.Max_3531 = Line3D_61_Type.Interval_26_Concept.Max_3531;
    }
    // field accessors
    static A_1080 = function(self) { return self.A_1080; }
    static B_1087 = function(self) { return self.B_1087; }
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
    constructor(A_1094, B_1101)
    {
        // field initialization 
        this.A_1094 = A_1094;
        this.B_1101 = B_1101;
        this.Count_3169 = Line2D_62_Type.Array_15_Concept.Count_3169;
        this.At_3174 = Line2D_62_Type.Array_15_Concept.At_3174;
        this.Default_3186 = Line2D_62_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = Line2D_62_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = Line2D_62_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = Line2D_62_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = Line2D_62_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = Line2D_62_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = Line2D_62_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = Line2D_62_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Line2D_62_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Line2D_62_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = Line2D_62_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Line2D_62_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Line2D_62_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Line2D_62_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Line2D_62_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = Line2D_62_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = Line2D_62_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = Line2D_62_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = Line2D_62_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = Line2D_62_Type.Numerical_19_Concept.MaxValue_3272;
        this.Count_3198 = Line2D_62_Type.Vector_17_Concept.Count_3198;
        this.At_3212 = Line2D_62_Type.Vector_17_Concept.At_3212;
        this.Min_3528 = Line2D_62_Type.Interval_26_Concept.Min_3528;
        this.Max_3531 = Line2D_62_Type.Interval_26_Concept.Max_3531;
    }
    // field accessors
    static A_1094 = function(self) { return self.A_1094; }
    static B_1101 = function(self) { return self.B_1101; }
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
    constructor(R_1108, G_1115, B_1122, A_1129)
    {
        // field initialization 
        this.R_1108 = R_1108;
        this.G_1115 = G_1115;
        this.B_1122 = B_1122;
        this.A_1129 = A_1129;
        this.Default_3186 = Color_63_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static R_1108 = function(self) { return self.R_1108; }
    static G_1115 = function(self) { return self.G_1115; }
    static B_1122 = function(self) { return self.B_1122; }
    static A_1129 = function(self) { return self.A_1129; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Color_63_Type);
    static Implements = [Value_16_Concept];
}
class ColorLUV_64_Type
{
    constructor(Lightness_1136, U_1143, V_1150)
    {
        // field initialization 
        this.Lightness_1136 = Lightness_1136;
        this.U_1143 = U_1143;
        this.V_1150 = V_1150;
        this.Default_3186 = ColorLUV_64_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Lightness_1136 = function(self) { return self.Lightness_1136; }
    static U_1143 = function(self) { return self.U_1143; }
    static V_1150 = function(self) { return self.V_1150; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ColorLUV_64_Type);
    static Implements = [Value_16_Concept];
}
class ColorLAB_65_Type
{
    constructor(Lightness_1157, A_1164, B_1171)
    {
        // field initialization 
        this.Lightness_1157 = Lightness_1157;
        this.A_1164 = A_1164;
        this.B_1171 = B_1171;
        this.Default_3186 = ColorLAB_65_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Lightness_1157 = function(self) { return self.Lightness_1157; }
    static A_1164 = function(self) { return self.A_1164; }
    static B_1171 = function(self) { return self.B_1171; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ColorLAB_65_Type);
    static Implements = [Value_16_Concept];
}
class ColorLCh_66_Type
{
    constructor(Lightness_1178, ChromaHue_1185)
    {
        // field initialization 
        this.Lightness_1178 = Lightness_1178;
        this.ChromaHue_1185 = ChromaHue_1185;
        this.Default_3186 = ColorLCh_66_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Lightness_1178 = function(self) { return self.Lightness_1178; }
    static ChromaHue_1185 = function(self) { return self.ChromaHue_1185; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ColorLCh_66_Type);
    static Implements = [Value_16_Concept];
}
class ColorHSV_67_Type
{
    constructor(Hue_1192, S_1199, V_1206)
    {
        // field initialization 
        this.Hue_1192 = Hue_1192;
        this.S_1199 = S_1199;
        this.V_1206 = V_1206;
        this.Default_3186 = ColorHSV_67_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Hue_1192 = function(self) { return self.Hue_1192; }
    static S_1199 = function(self) { return self.S_1199; }
    static V_1206 = function(self) { return self.V_1206; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ColorHSV_67_Type);
    static Implements = [Value_16_Concept];
}
class ColorHSL_68_Type
{
    constructor(Hue_1213, Saturation_1220, Luminance_1227)
    {
        // field initialization 
        this.Hue_1213 = Hue_1213;
        this.Saturation_1220 = Saturation_1220;
        this.Luminance_1227 = Luminance_1227;
        this.Default_3186 = ColorHSL_68_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Hue_1213 = function(self) { return self.Hue_1213; }
    static Saturation_1220 = function(self) { return self.Saturation_1220; }
    static Luminance_1227 = function(self) { return self.Luminance_1227; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ColorHSL_68_Type);
    static Implements = [Value_16_Concept];
}
class ColorYCbCr_69_Type
{
    constructor(Y_1234, Cb_1241, Cr_1248)
    {
        // field initialization 
        this.Y_1234 = Y_1234;
        this.Cb_1241 = Cb_1241;
        this.Cr_1248 = Cr_1248;
        this.Default_3186 = ColorYCbCr_69_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Y_1234 = function(self) { return self.Y_1234; }
    static Cb_1241 = function(self) { return self.Cb_1241; }
    static Cr_1248 = function(self) { return self.Cr_1248; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ColorYCbCr_69_Type);
    static Implements = [Value_16_Concept];
}
class SphericalCoordinate_70_Type
{
    constructor(Radius_1255, Azimuth_1262, Polar_1269)
    {
        // field initialization 
        this.Radius_1255 = Radius_1255;
        this.Azimuth_1262 = Azimuth_1262;
        this.Polar_1269 = Polar_1269;
        this.Default_3186 = SphericalCoordinate_70_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Radius_1255 = function(self) { return self.Radius_1255; }
    static Azimuth_1262 = function(self) { return self.Azimuth_1262; }
    static Polar_1269 = function(self) { return self.Polar_1269; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(SphericalCoordinate_70_Type);
    static Implements = [Value_16_Concept];
}
class PolarCoordinate_71_Type
{
    constructor(Radius_1276, Angle_1283)
    {
        // field initialization 
        this.Radius_1276 = Radius_1276;
        this.Angle_1283 = Angle_1283;
        this.Default_3186 = PolarCoordinate_71_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Radius_1276 = function(self) { return self.Radius_1276; }
    static Angle_1283 = function(self) { return self.Angle_1283; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(PolarCoordinate_71_Type);
    static Implements = [Value_16_Concept];
}
class LogPolarCoordinate_72_Type
{
    constructor(Rho_1290, Azimuth_1297)
    {
        // field initialization 
        this.Rho_1290 = Rho_1290;
        this.Azimuth_1297 = Azimuth_1297;
        this.Default_3186 = LogPolarCoordinate_72_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Rho_1290 = function(self) { return self.Rho_1290; }
    static Azimuth_1297 = function(self) { return self.Azimuth_1297; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(LogPolarCoordinate_72_Type);
    static Implements = [Value_16_Concept];
}
class CylindricalCoordinate_73_Type
{
    constructor(RadialDistance_1304, Azimuth_1311, Height_1318)
    {
        // field initialization 
        this.RadialDistance_1304 = RadialDistance_1304;
        this.Azimuth_1311 = Azimuth_1311;
        this.Height_1318 = Height_1318;
        this.Default_3186 = CylindricalCoordinate_73_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static RadialDistance_1304 = function(self) { return self.RadialDistance_1304; }
    static Azimuth_1311 = function(self) { return self.Azimuth_1311; }
    static Height_1318 = function(self) { return self.Height_1318; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(CylindricalCoordinate_73_Type);
    static Implements = [Value_16_Concept];
}
class HorizontalCoordinate_74_Type
{
    constructor(Radius_1325, Azimuth_1332, Height_1339)
    {
        // field initialization 
        this.Radius_1325 = Radius_1325;
        this.Azimuth_1332 = Azimuth_1332;
        this.Height_1339 = Height_1339;
        this.Default_3186 = HorizontalCoordinate_74_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Radius_1325 = function(self) { return self.Radius_1325; }
    static Azimuth_1332 = function(self) { return self.Azimuth_1332; }
    static Height_1339 = function(self) { return self.Height_1339; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(HorizontalCoordinate_74_Type);
    static Implements = [Value_16_Concept];
}
class GeoCoordinate_75_Type
{
    constructor(Latitude_1346, Longitude_1353)
    {
        // field initialization 
        this.Latitude_1346 = Latitude_1346;
        this.Longitude_1353 = Longitude_1353;
        this.Default_3186 = GeoCoordinate_75_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Latitude_1346 = function(self) { return self.Latitude_1346; }
    static Longitude_1353 = function(self) { return self.Longitude_1353; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(GeoCoordinate_75_Type);
    static Implements = [Value_16_Concept];
}
class GeoCoordinateWithAltitude_76_Type
{
    constructor(Coordinate_1360, Altitude_1367)
    {
        // field initialization 
        this.Coordinate_1360 = Coordinate_1360;
        this.Altitude_1367 = Altitude_1367;
        this.Default_3186 = GeoCoordinateWithAltitude_76_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Coordinate_1360 = function(self) { return self.Coordinate_1360; }
    static Altitude_1367 = function(self) { return self.Altitude_1367; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(GeoCoordinateWithAltitude_76_Type);
    static Implements = [Value_16_Concept];
}
class Circle_77_Type
{
    constructor(Center_1374, Radius_1381)
    {
        // field initialization 
        this.Center_1374 = Center_1374;
        this.Radius_1381 = Radius_1381;
        this.Default_3186 = Circle_77_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Center_1374 = function(self) { return self.Center_1374; }
    static Radius_1381 = function(self) { return self.Radius_1381; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Circle_77_Type);
    static Implements = [Value_16_Concept];
}
class Chord_78_Type
{
    constructor(Circle_1388, Arc_1395)
    {
        // field initialization 
        this.Circle_1388 = Circle_1388;
        this.Arc_1395 = Arc_1395;
        this.Default_3186 = Chord_78_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Circle_1388 = function(self) { return self.Circle_1388; }
    static Arc_1395 = function(self) { return self.Arc_1395; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Chord_78_Type);
    static Implements = [Value_16_Concept];
}
class Size2D_79_Type
{
    constructor(Width_1402, Height_1409)
    {
        // field initialization 
        this.Width_1402 = Width_1402;
        this.Height_1409 = Height_1409;
        this.Default_3186 = Size2D_79_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Width_1402 = function(self) { return self.Width_1402; }
    static Height_1409 = function(self) { return self.Height_1409; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Size2D_79_Type);
    static Implements = [Value_16_Concept];
}
class Size3D_80_Type
{
    constructor(Width_1416, Height_1423, Depth_1430)
    {
        // field initialization 
        this.Width_1416 = Width_1416;
        this.Height_1423 = Height_1423;
        this.Depth_1430 = Depth_1430;
        this.Default_3186 = Size3D_80_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Width_1416 = function(self) { return self.Width_1416; }
    static Height_1423 = function(self) { return self.Height_1423; }
    static Depth_1430 = function(self) { return self.Depth_1430; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Size3D_80_Type);
    static Implements = [Value_16_Concept];
}
class Rectangle2D_81_Type
{
    constructor(Center_1437, Size_1444)
    {
        // field initialization 
        this.Center_1437 = Center_1437;
        this.Size_1444 = Size_1444;
        this.Default_3186 = Rectangle2D_81_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Center_1437 = function(self) { return self.Center_1437; }
    static Size_1444 = function(self) { return self.Size_1444; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Rectangle2D_81_Type);
    static Implements = [Value_16_Concept];
}
class Proportion_82_Type
{
    constructor(Value_1451)
    {
        // field initialization 
        this.Value_1451 = Value_1451;
        this.Default_3186 = Proportion_82_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = Proportion_82_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = Proportion_82_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = Proportion_82_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = Proportion_82_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = Proportion_82_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = Proportion_82_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = Proportion_82_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Proportion_82_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Proportion_82_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = Proportion_82_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Proportion_82_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Proportion_82_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Proportion_82_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Proportion_82_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = Proportion_82_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = Proportion_82_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = Proportion_82_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = Proportion_82_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = Proportion_82_Type.Numerical_19_Concept.MaxValue_3272;
    }
    // field accessors
    static Value_1451 = function(self) { return self.Value_1451; }
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
    constructor(Numerator_1458, Denominator_1465)
    {
        // field initialization 
        this.Numerator_1458 = Numerator_1458;
        this.Denominator_1465 = Denominator_1465;
        this.Default_3186 = Fraction_83_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Numerator_1458 = function(self) { return self.Numerator_1458; }
    static Denominator_1465 = function(self) { return self.Denominator_1465; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Fraction_83_Type);
    static Implements = [Value_16_Concept];
}
class Angle_84_Type
{
    constructor(Radians_1472)
    {
        // field initialization 
        this.Radians_1472 = Radians_1472;
        this.Default_3186 = Angle_84_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = Angle_84_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Angle_84_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Angle_84_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Angle_84_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Angle_84_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = Angle_84_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Angle_84_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Angle_84_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = Angle_84_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Radians_1472 = function(self) { return self.Radians_1472; }
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
    constructor(Meters_1479)
    {
        // field initialization 
        this.Meters_1479 = Meters_1479;
        this.Default_3186 = Length_85_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = Length_85_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Length_85_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Length_85_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Length_85_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Length_85_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = Length_85_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Length_85_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Length_85_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = Length_85_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Meters_1479 = function(self) { return self.Meters_1479; }
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
    constructor(Kilograms_1486)
    {
        // field initialization 
        this.Kilograms_1486 = Kilograms_1486;
        this.Default_3186 = Mass_86_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = Mass_86_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Mass_86_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Mass_86_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Mass_86_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Mass_86_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = Mass_86_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Mass_86_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Mass_86_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = Mass_86_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Kilograms_1486 = function(self) { return self.Kilograms_1486; }
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
    constructor(Celsius_1493)
    {
        // field initialization 
        this.Celsius_1493 = Celsius_1493;
        this.Default_3186 = Temperature_87_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = Temperature_87_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Temperature_87_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Temperature_87_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Temperature_87_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Temperature_87_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = Temperature_87_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Temperature_87_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Temperature_87_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = Temperature_87_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Celsius_1493 = function(self) { return self.Celsius_1493; }
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
    constructor(Seconds_1500)
    {
        // field initialization 
        this.Seconds_1500 = Seconds_1500;
        this.Default_3186 = TimeSpan_88_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = TimeSpan_88_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = TimeSpan_88_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = TimeSpan_88_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = TimeSpan_88_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = TimeSpan_88_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = TimeSpan_88_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = TimeSpan_88_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = TimeSpan_88_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = TimeSpan_88_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Seconds_1500 = function(self) { return self.Seconds_1500; }
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
    constructor(Min_1507, Max_1514)
    {
        // field initialization 
        this.Min_1507 = Min_1507;
        this.Max_1514 = Max_1514;
        this.Count_3169 = TimeRange_89_Type.Array_15_Concept.Count_3169;
        this.At_3174 = TimeRange_89_Type.Array_15_Concept.At_3174;
        this.Default_3186 = TimeRange_89_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = TimeRange_89_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = TimeRange_89_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = TimeRange_89_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = TimeRange_89_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = TimeRange_89_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = TimeRange_89_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = TimeRange_89_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = TimeRange_89_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = TimeRange_89_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = TimeRange_89_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = TimeRange_89_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = TimeRange_89_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = TimeRange_89_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = TimeRange_89_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = TimeRange_89_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = TimeRange_89_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = TimeRange_89_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = TimeRange_89_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = TimeRange_89_Type.Numerical_19_Concept.MaxValue_3272;
        this.Count_3198 = TimeRange_89_Type.Vector_17_Concept.Count_3198;
        this.At_3212 = TimeRange_89_Type.Vector_17_Concept.At_3212;
        this.Min_3528 = TimeRange_89_Type.Interval_26_Concept.Min_3528;
        this.Max_3531 = TimeRange_89_Type.Interval_26_Concept.Max_3531;
    }
    // field accessors
    static Min_1507 = function(self) { return self.Min_1507; }
    static Max_1514 = function(self) { return self.Max_1514; }
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
        this.Default_3186 = DateTime_90_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(DateTime_90_Type);
    static Implements = [Value_16_Concept];
}
class AnglePair_91_Type
{
    constructor(Start_1521, End_1528)
    {
        // field initialization 
        this.Start_1521 = Start_1521;
        this.End_1528 = End_1528;
        this.Count_3169 = AnglePair_91_Type.Array_15_Concept.Count_3169;
        this.At_3174 = AnglePair_91_Type.Array_15_Concept.At_3174;
        this.Default_3186 = AnglePair_91_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = AnglePair_91_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = AnglePair_91_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = AnglePair_91_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = AnglePair_91_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = AnglePair_91_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = AnglePair_91_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = AnglePair_91_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = AnglePair_91_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = AnglePair_91_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = AnglePair_91_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = AnglePair_91_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = AnglePair_91_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = AnglePair_91_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = AnglePair_91_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = AnglePair_91_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = AnglePair_91_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = AnglePair_91_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = AnglePair_91_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = AnglePair_91_Type.Numerical_19_Concept.MaxValue_3272;
        this.Count_3198 = AnglePair_91_Type.Vector_17_Concept.Count_3198;
        this.At_3212 = AnglePair_91_Type.Vector_17_Concept.At_3212;
        this.Min_3528 = AnglePair_91_Type.Interval_26_Concept.Min_3528;
        this.Max_3531 = AnglePair_91_Type.Interval_26_Concept.Max_3531;
    }
    // field accessors
    static Start_1521 = function(self) { return self.Start_1521; }
    static End_1528 = function(self) { return self.End_1528; }
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
    constructor(Circle_1535, InnerRadius_1542)
    {
        // field initialization 
        this.Circle_1535 = Circle_1535;
        this.InnerRadius_1542 = InnerRadius_1542;
        this.Default_3186 = Ring_92_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = Ring_92_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = Ring_92_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = Ring_92_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = Ring_92_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = Ring_92_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = Ring_92_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = Ring_92_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Ring_92_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Ring_92_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = Ring_92_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Ring_92_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Ring_92_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Ring_92_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Ring_92_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = Ring_92_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = Ring_92_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = Ring_92_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = Ring_92_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = Ring_92_Type.Numerical_19_Concept.MaxValue_3272;
    }
    // field accessors
    static Circle_1535 = function(self) { return self.Circle_1535; }
    static InnerRadius_1542 = function(self) { return self.InnerRadius_1542; }
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
    constructor(Angles_1549, Cirlce_1556)
    {
        // field initialization 
        this.Angles_1549 = Angles_1549;
        this.Cirlce_1556 = Cirlce_1556;
        this.Default_3186 = Arc_93_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Angles_1549 = function(self) { return self.Angles_1549; }
    static Cirlce_1556 = function(self) { return self.Cirlce_1556; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Arc_93_Type);
    static Implements = [Value_16_Concept];
}
class TimeInterval_94_Type
{
    constructor(Start_1563, End_1570)
    {
        // field initialization 
        this.Start_1563 = Start_1563;
        this.End_1570 = End_1570;
        this.Count_3169 = TimeInterval_94_Type.Array_15_Concept.Count_3169;
        this.At_3174 = TimeInterval_94_Type.Array_15_Concept.At_3174;
        this.Default_3186 = TimeInterval_94_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = TimeInterval_94_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = TimeInterval_94_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = TimeInterval_94_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = TimeInterval_94_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = TimeInterval_94_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = TimeInterval_94_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = TimeInterval_94_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = TimeInterval_94_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = TimeInterval_94_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = TimeInterval_94_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = TimeInterval_94_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = TimeInterval_94_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = TimeInterval_94_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = TimeInterval_94_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = TimeInterval_94_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = TimeInterval_94_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = TimeInterval_94_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = TimeInterval_94_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = TimeInterval_94_Type.Numerical_19_Concept.MaxValue_3272;
        this.Count_3198 = TimeInterval_94_Type.Vector_17_Concept.Count_3198;
        this.At_3212 = TimeInterval_94_Type.Vector_17_Concept.At_3212;
        this.Min_3528 = TimeInterval_94_Type.Interval_26_Concept.Min_3528;
        this.Max_3531 = TimeInterval_94_Type.Interval_26_Concept.Max_3531;
    }
    // field accessors
    static Start_1563 = function(self) { return self.Start_1563; }
    static End_1570 = function(self) { return self.End_1570; }
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
    constructor(A_1577, B_1584)
    {
        // field initialization 
        this.A_1577 = A_1577;
        this.B_1584 = B_1584;
        this.Count_3169 = RealInterval_95_Type.Array_15_Concept.Count_3169;
        this.At_3174 = RealInterval_95_Type.Array_15_Concept.At_3174;
        this.Default_3186 = RealInterval_95_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = RealInterval_95_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = RealInterval_95_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = RealInterval_95_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = RealInterval_95_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = RealInterval_95_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = RealInterval_95_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = RealInterval_95_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = RealInterval_95_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = RealInterval_95_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = RealInterval_95_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = RealInterval_95_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = RealInterval_95_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = RealInterval_95_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = RealInterval_95_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = RealInterval_95_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = RealInterval_95_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = RealInterval_95_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = RealInterval_95_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = RealInterval_95_Type.Numerical_19_Concept.MaxValue_3272;
        this.Count_3198 = RealInterval_95_Type.Vector_17_Concept.Count_3198;
        this.At_3212 = RealInterval_95_Type.Vector_17_Concept.At_3212;
        this.Min_3528 = RealInterval_95_Type.Interval_26_Concept.Min_3528;
        this.Max_3531 = RealInterval_95_Type.Interval_26_Concept.Max_3531;
    }
    // field accessors
    static A_1577 = function(self) { return self.A_1577; }
    static B_1584 = function(self) { return self.B_1584; }
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
    constructor(A_1591, B_1598)
    {
        // field initialization 
        this.A_1591 = A_1591;
        this.B_1598 = B_1598;
        this.Count_3169 = Interval2D_96_Type.Array_15_Concept.Count_3169;
        this.At_3174 = Interval2D_96_Type.Array_15_Concept.At_3174;
        this.Default_3186 = Interval2D_96_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = Interval2D_96_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = Interval2D_96_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = Interval2D_96_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = Interval2D_96_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = Interval2D_96_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = Interval2D_96_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = Interval2D_96_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Interval2D_96_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Interval2D_96_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = Interval2D_96_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Interval2D_96_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Interval2D_96_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Interval2D_96_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Interval2D_96_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = Interval2D_96_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = Interval2D_96_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = Interval2D_96_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = Interval2D_96_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = Interval2D_96_Type.Numerical_19_Concept.MaxValue_3272;
        this.Count_3198 = Interval2D_96_Type.Vector_17_Concept.Count_3198;
        this.At_3212 = Interval2D_96_Type.Vector_17_Concept.At_3212;
        this.Min_3528 = Interval2D_96_Type.Interval_26_Concept.Min_3528;
        this.Max_3531 = Interval2D_96_Type.Interval_26_Concept.Max_3531;
    }
    // field accessors
    static A_1591 = function(self) { return self.A_1591; }
    static B_1598 = function(self) { return self.B_1598; }
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
    constructor(A_1605, B_1612)
    {
        // field initialization 
        this.A_1605 = A_1605;
        this.B_1612 = B_1612;
        this.Count_3169 = Interval3D_97_Type.Array_15_Concept.Count_3169;
        this.At_3174 = Interval3D_97_Type.Array_15_Concept.At_3174;
        this.Default_3186 = Interval3D_97_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = Interval3D_97_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = Interval3D_97_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = Interval3D_97_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = Interval3D_97_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = Interval3D_97_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = Interval3D_97_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = Interval3D_97_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Interval3D_97_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Interval3D_97_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = Interval3D_97_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Interval3D_97_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Interval3D_97_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Interval3D_97_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Interval3D_97_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = Interval3D_97_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = Interval3D_97_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = Interval3D_97_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = Interval3D_97_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = Interval3D_97_Type.Numerical_19_Concept.MaxValue_3272;
        this.Count_3198 = Interval3D_97_Type.Vector_17_Concept.Count_3198;
        this.At_3212 = Interval3D_97_Type.Vector_17_Concept.At_3212;
        this.Min_3528 = Interval3D_97_Type.Interval_26_Concept.Min_3528;
        this.Max_3531 = Interval3D_97_Type.Interval_26_Concept.Max_3531;
    }
    // field accessors
    static A_1605 = function(self) { return self.A_1605; }
    static B_1612 = function(self) { return self.B_1612; }
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
    constructor(Line_1619, Radius_1626)
    {
        // field initialization 
        this.Line_1619 = Line_1619;
        this.Radius_1626 = Radius_1626;
        this.Default_3186 = Capsule_98_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Line_1619 = function(self) { return self.Line_1619; }
    static Radius_1626 = function(self) { return self.Radius_1626; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Capsule_98_Type);
    static Implements = [Value_16_Concept];
}
class Matrix3D_99_Type
{
    constructor(Column1_1633, Column2_1640, Column3_1647, Column4_1654)
    {
        // field initialization 
        this.Column1_1633 = Column1_1633;
        this.Column2_1640 = Column2_1640;
        this.Column3_1647 = Column3_1647;
        this.Column4_1654 = Column4_1654;
        this.Default_3186 = Matrix3D_99_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Column1_1633 = function(self) { return self.Column1_1633; }
    static Column2_1640 = function(self) { return self.Column2_1640; }
    static Column3_1647 = function(self) { return self.Column3_1647; }
    static Column4_1654 = function(self) { return self.Column4_1654; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Matrix3D_99_Type);
    static Implements = [Value_16_Concept];
}
class Cylinder_100_Type
{
    constructor(Line_1661, Radius_1668)
    {
        // field initialization 
        this.Line_1661 = Line_1661;
        this.Radius_1668 = Radius_1668;
        this.Default_3186 = Cylinder_100_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Line_1661 = function(self) { return self.Line_1661; }
    static Radius_1668 = function(self) { return self.Radius_1668; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Cylinder_100_Type);
    static Implements = [Value_16_Concept];
}
class Cone_101_Type
{
    constructor(Line_1675, Radius_1682)
    {
        // field initialization 
        this.Line_1675 = Line_1675;
        this.Radius_1682 = Radius_1682;
        this.Default_3186 = Cone_101_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Line_1675 = function(self) { return self.Line_1675; }
    static Radius_1682 = function(self) { return self.Radius_1682; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Cone_101_Type);
    static Implements = [Value_16_Concept];
}
class Tube_102_Type
{
    constructor(Line_1689, InnerRadius_1696, OuterRadius_1703)
    {
        // field initialization 
        this.Line_1689 = Line_1689;
        this.InnerRadius_1696 = InnerRadius_1696;
        this.OuterRadius_1703 = OuterRadius_1703;
        this.Default_3186 = Tube_102_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Line_1689 = function(self) { return self.Line_1689; }
    static InnerRadius_1696 = function(self) { return self.InnerRadius_1696; }
    static OuterRadius_1703 = function(self) { return self.OuterRadius_1703; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Tube_102_Type);
    static Implements = [Value_16_Concept];
}
class ConeSegment_103_Type
{
    constructor(Line_1710, Radius1_1717, Radius2_1724)
    {
        // field initialization 
        this.Line_1710 = Line_1710;
        this.Radius1_1717 = Radius1_1717;
        this.Radius2_1724 = Radius2_1724;
        this.Default_3186 = ConeSegment_103_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Line_1710 = function(self) { return self.Line_1710; }
    static Radius1_1717 = function(self) { return self.Radius1_1717; }
    static Radius2_1724 = function(self) { return self.Radius2_1724; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(ConeSegment_103_Type);
    static Implements = [Value_16_Concept];
}
class Box2D_104_Type
{
    constructor(Center_1731, Rotation_1738, Extent_1745)
    {
        // field initialization 
        this.Center_1731 = Center_1731;
        this.Rotation_1738 = Rotation_1738;
        this.Extent_1745 = Extent_1745;
        this.Default_3186 = Box2D_104_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Center_1731 = function(self) { return self.Center_1731; }
    static Rotation_1738 = function(self) { return self.Rotation_1738; }
    static Extent_1745 = function(self) { return self.Extent_1745; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Box2D_104_Type);
    static Implements = [Value_16_Concept];
}
class Box3D_105_Type
{
    constructor(Center_1752, Rotation_1759, Extent_1766)
    {
        // field initialization 
        this.Center_1752 = Center_1752;
        this.Rotation_1759 = Rotation_1759;
        this.Extent_1766 = Extent_1766;
        this.Default_3186 = Box3D_105_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Center_1752 = function(self) { return self.Center_1752; }
    static Rotation_1759 = function(self) { return self.Rotation_1759; }
    static Extent_1766 = function(self) { return self.Extent_1766; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(Box3D_105_Type);
    static Implements = [Value_16_Concept];
}
class CubicBezierTriangle3D_106_Type
{
    constructor(A_1773, B_1780, C_1787, A2B_1794, AB2_1801, B2C_1808, BC2_1815, AC2_1822, A2C_1829, ABC_1836)
    {
        // field initialization 
        this.A_1773 = A_1773;
        this.B_1780 = B_1780;
        this.C_1787 = C_1787;
        this.A2B_1794 = A2B_1794;
        this.AB2_1801 = AB2_1801;
        this.B2C_1808 = B2C_1808;
        this.BC2_1815 = BC2_1815;
        this.AC2_1822 = AC2_1822;
        this.A2C_1829 = A2C_1829;
        this.ABC_1836 = ABC_1836;
        this.Default_3186 = CubicBezierTriangle3D_106_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static A_1773 = function(self) { return self.A_1773; }
    static B_1780 = function(self) { return self.B_1780; }
    static C_1787 = function(self) { return self.C_1787; }
    static A2B_1794 = function(self) { return self.A2B_1794; }
    static AB2_1801 = function(self) { return self.AB2_1801; }
    static B2C_1808 = function(self) { return self.B2C_1808; }
    static BC2_1815 = function(self) { return self.BC2_1815; }
    static AC2_1822 = function(self) { return self.AC2_1822; }
    static A2C_1829 = function(self) { return self.A2C_1829; }
    static ABC_1836 = function(self) { return self.ABC_1836; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(CubicBezierTriangle3D_106_Type);
    static Implements = [Value_16_Concept];
}
class CubicBezier2D_107_Type
{
    constructor(A_1843, B_1850, C_1857, D_1864)
    {
        // field initialization 
        this.A_1843 = A_1843;
        this.B_1850 = B_1850;
        this.C_1857 = C_1857;
        this.D_1864 = D_1864;
        this.Default_3186 = CubicBezier2D_107_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static A_1843 = function(self) { return self.A_1843; }
    static B_1850 = function(self) { return self.B_1850; }
    static C_1857 = function(self) { return self.C_1857; }
    static D_1864 = function(self) { return self.D_1864; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(CubicBezier2D_107_Type);
    static Implements = [Value_16_Concept];
}
class UV_108_Type
{
    constructor(U_1871, V_1878)
    {
        // field initialization 
        this.U_1871 = U_1871;
        this.V_1878 = V_1878;
        this.Count_3169 = UV_108_Type.Array_15_Concept.Count_3169;
        this.At_3174 = UV_108_Type.Array_15_Concept.At_3174;
        this.Default_3186 = UV_108_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = UV_108_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = UV_108_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = UV_108_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = UV_108_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = UV_108_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = UV_108_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = UV_108_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = UV_108_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = UV_108_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = UV_108_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = UV_108_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = UV_108_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = UV_108_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = UV_108_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = UV_108_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = UV_108_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = UV_108_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = UV_108_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = UV_108_Type.Numerical_19_Concept.MaxValue_3272;
        this.Count_3198 = UV_108_Type.Vector_17_Concept.Count_3198;
        this.At_3212 = UV_108_Type.Vector_17_Concept.At_3212;
    }
    // field accessors
    static U_1871 = function(self) { return self.U_1871; }
    static V_1878 = function(self) { return self.V_1878; }
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
    constructor(U_1885, V_1892, W_1899)
    {
        // field initialization 
        this.U_1885 = U_1885;
        this.V_1892 = V_1892;
        this.W_1899 = W_1899;
        this.Count_3169 = UVW_109_Type.Array_15_Concept.Count_3169;
        this.At_3174 = UVW_109_Type.Array_15_Concept.At_3174;
        this.Default_3186 = UVW_109_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = UVW_109_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = UVW_109_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = UVW_109_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = UVW_109_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = UVW_109_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = UVW_109_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = UVW_109_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = UVW_109_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = UVW_109_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = UVW_109_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = UVW_109_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = UVW_109_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = UVW_109_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = UVW_109_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = UVW_109_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = UVW_109_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = UVW_109_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = UVW_109_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = UVW_109_Type.Numerical_19_Concept.MaxValue_3272;
        this.Count_3198 = UVW_109_Type.Vector_17_Concept.Count_3198;
        this.At_3212 = UVW_109_Type.Vector_17_Concept.At_3212;
    }
    // field accessors
    static U_1885 = function(self) { return self.U_1885; }
    static V_1892 = function(self) { return self.V_1892; }
    static W_1899 = function(self) { return self.W_1899; }
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
    constructor(A_1906, B_1913, C_1920, D_1927)
    {
        // field initialization 
        this.A_1906 = A_1906;
        this.B_1913 = B_1913;
        this.C_1920 = C_1920;
        this.D_1927 = D_1927;
        this.Default_3186 = CubicBezier3D_110_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static A_1906 = function(self) { return self.A_1906; }
    static B_1913 = function(self) { return self.B_1913; }
    static C_1920 = function(self) { return self.C_1920; }
    static D_1927 = function(self) { return self.D_1927; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(CubicBezier3D_110_Type);
    static Implements = [Value_16_Concept];
}
class QuadraticBezier2D_111_Type
{
    constructor(A_1934, B_1941, C_1948)
    {
        // field initialization 
        this.A_1934 = A_1934;
        this.B_1941 = B_1941;
        this.C_1948 = C_1948;
        this.Default_3186 = QuadraticBezier2D_111_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static A_1934 = function(self) { return self.A_1934; }
    static B_1941 = function(self) { return self.B_1941; }
    static C_1948 = function(self) { return self.C_1948; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(QuadraticBezier2D_111_Type);
    static Implements = [Value_16_Concept];
}
class QuadraticBezier3D_112_Type
{
    constructor(A_1955, B_1962, C_1969)
    {
        // field initialization 
        this.A_1955 = A_1955;
        this.B_1962 = B_1962;
        this.C_1969 = C_1969;
        this.Default_3186 = QuadraticBezier3D_112_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static A_1955 = function(self) { return self.A_1955; }
    static B_1962 = function(self) { return self.B_1962; }
    static C_1969 = function(self) { return self.C_1969; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(QuadraticBezier3D_112_Type);
    static Implements = [Value_16_Concept];
}
class Area_113_Type
{
    constructor(MetersSquared_1976)
    {
        // field initialization 
        this.MetersSquared_1976 = MetersSquared_1976;
        this.Default_3186 = Area_113_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = Area_113_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Area_113_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Area_113_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Area_113_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Area_113_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = Area_113_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Area_113_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Area_113_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = Area_113_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static MetersSquared_1976 = function(self) { return self.MetersSquared_1976; }
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
    constructor(MetersCubed_1983)
    {
        // field initialization 
        this.MetersCubed_1983 = MetersCubed_1983;
        this.Default_3186 = Volume_114_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = Volume_114_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Volume_114_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Volume_114_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Volume_114_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Volume_114_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = Volume_114_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Volume_114_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Volume_114_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = Volume_114_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static MetersCubed_1983 = function(self) { return self.MetersCubed_1983; }
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
    constructor(MetersPerSecond_1990)
    {
        // field initialization 
        this.MetersPerSecond_1990 = MetersPerSecond_1990;
        this.Default_3186 = Velocity_115_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = Velocity_115_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Velocity_115_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Velocity_115_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Velocity_115_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Velocity_115_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = Velocity_115_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Velocity_115_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Velocity_115_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = Velocity_115_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static MetersPerSecond_1990 = function(self) { return self.MetersPerSecond_1990; }
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
    constructor(MetersPerSecondSquared_1997)
    {
        // field initialization 
        this.MetersPerSecondSquared_1997 = MetersPerSecondSquared_1997;
        this.Default_3186 = Acceleration_116_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = Acceleration_116_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Acceleration_116_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Acceleration_116_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Acceleration_116_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Acceleration_116_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = Acceleration_116_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Acceleration_116_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Acceleration_116_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = Acceleration_116_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static MetersPerSecondSquared_1997 = function(self) { return self.MetersPerSecondSquared_1997; }
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
    constructor(Newtons_2004)
    {
        // field initialization 
        this.Newtons_2004 = Newtons_2004;
        this.Default_3186 = Force_117_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = Force_117_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Force_117_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Force_117_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Force_117_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Force_117_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = Force_117_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Force_117_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Force_117_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = Force_117_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Newtons_2004 = function(self) { return self.Newtons_2004; }
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
    constructor(Pascals_2011)
    {
        // field initialization 
        this.Pascals_2011 = Pascals_2011;
        this.Default_3186 = Pressure_118_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = Pressure_118_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Pressure_118_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Pressure_118_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Pressure_118_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Pressure_118_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = Pressure_118_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Pressure_118_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Pressure_118_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = Pressure_118_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Pascals_2011 = function(self) { return self.Pascals_2011; }
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
    constructor(Joules_2018)
    {
        // field initialization 
        this.Joules_2018 = Joules_2018;
        this.Default_3186 = Energy_119_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = Energy_119_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Energy_119_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Energy_119_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Energy_119_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Energy_119_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = Energy_119_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Energy_119_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Energy_119_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = Energy_119_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Joules_2018 = function(self) { return self.Joules_2018; }
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
    constructor(Bytes_2025)
    {
        // field initialization 
        this.Bytes_2025 = Bytes_2025;
        this.Default_3186 = Memory_120_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = Memory_120_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Memory_120_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Memory_120_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Memory_120_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Memory_120_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = Memory_120_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Memory_120_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Memory_120_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = Memory_120_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Bytes_2025 = function(self) { return self.Bytes_2025; }
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
    constructor(Hertz_2032)
    {
        // field initialization 
        this.Hertz_2032 = Hertz_2032;
        this.Default_3186 = Frequency_121_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = Frequency_121_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Frequency_121_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Frequency_121_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Frequency_121_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Frequency_121_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = Frequency_121_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Frequency_121_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Frequency_121_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = Frequency_121_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Hertz_2032 = function(self) { return self.Hertz_2032; }
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
    constructor(Decibels_2039)
    {
        // field initialization 
        this.Decibels_2039 = Decibels_2039;
        this.Default_3186 = Loudness_122_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = Loudness_122_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Loudness_122_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Loudness_122_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Loudness_122_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Loudness_122_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = Loudness_122_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Loudness_122_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Loudness_122_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = Loudness_122_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Decibels_2039 = function(self) { return self.Decibels_2039; }
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
    constructor(Candelas_2046)
    {
        // field initialization 
        this.Candelas_2046 = Candelas_2046;
        this.Default_3186 = LuminousIntensity_123_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = LuminousIntensity_123_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = LuminousIntensity_123_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = LuminousIntensity_123_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = LuminousIntensity_123_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = LuminousIntensity_123_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = LuminousIntensity_123_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = LuminousIntensity_123_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = LuminousIntensity_123_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = LuminousIntensity_123_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Candelas_2046 = function(self) { return self.Candelas_2046; }
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
    constructor(Volts_2053)
    {
        // field initialization 
        this.Volts_2053 = Volts_2053;
        this.Default_3186 = ElectricPotential_124_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = ElectricPotential_124_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = ElectricPotential_124_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = ElectricPotential_124_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = ElectricPotential_124_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = ElectricPotential_124_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = ElectricPotential_124_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = ElectricPotential_124_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = ElectricPotential_124_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = ElectricPotential_124_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Volts_2053 = function(self) { return self.Volts_2053; }
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
    constructor(Columbs_2060)
    {
        // field initialization 
        this.Columbs_2060 = Columbs_2060;
        this.Default_3186 = ElectricCharge_125_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = ElectricCharge_125_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = ElectricCharge_125_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = ElectricCharge_125_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = ElectricCharge_125_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = ElectricCharge_125_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = ElectricCharge_125_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = ElectricCharge_125_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = ElectricCharge_125_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = ElectricCharge_125_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Columbs_2060 = function(self) { return self.Columbs_2060; }
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
    constructor(Amperes_2067)
    {
        // field initialization 
        this.Amperes_2067 = Amperes_2067;
        this.Default_3186 = ElectricCurrent_126_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = ElectricCurrent_126_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = ElectricCurrent_126_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = ElectricCurrent_126_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = ElectricCurrent_126_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = ElectricCurrent_126_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = ElectricCurrent_126_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = ElectricCurrent_126_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = ElectricCurrent_126_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = ElectricCurrent_126_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Amperes_2067 = function(self) { return self.Amperes_2067; }
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
    constructor(Ohms_2074)
    {
        // field initialization 
        this.Ohms_2074 = Ohms_2074;
        this.Default_3186 = ElectricResistance_127_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = ElectricResistance_127_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = ElectricResistance_127_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = ElectricResistance_127_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = ElectricResistance_127_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = ElectricResistance_127_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = ElectricResistance_127_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = ElectricResistance_127_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = ElectricResistance_127_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = ElectricResistance_127_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Ohms_2074 = function(self) { return self.Ohms_2074; }
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
    constructor(Watts_2081)
    {
        // field initialization 
        this.Watts_2081 = Watts_2081;
        this.Default_3186 = Power_128_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = Power_128_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Power_128_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Power_128_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Power_128_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Power_128_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = Power_128_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Power_128_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Power_128_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = Power_128_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static Watts_2081 = function(self) { return self.Watts_2081; }
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
    constructor(KilogramsPerMeterCubed_2088)
    {
        // field initialization 
        this.KilogramsPerMeterCubed_2088 = KilogramsPerMeterCubed_2088;
        this.Default_3186 = Density_129_Type.Value_16_Concept.Default_3186;
        this.Add_3421 = Density_129_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Density_129_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Density_129_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Density_129_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Density_129_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.Equals_3315 = Density_129_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Density_129_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Density_129_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Value_3227 = Density_129_Type.Measure_18_Concept.Value_3227;
    }
    // field accessors
    static KilogramsPerMeterCubed_2088 = function(self) { return self.KilogramsPerMeterCubed_2088; }
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
    constructor(Mean_2095, StandardDeviation_2102)
    {
        // field initialization 
        this.Mean_2095 = Mean_2095;
        this.StandardDeviation_2102 = StandardDeviation_2102;
        this.Default_3186 = NormalDistribution_130_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Mean_2095 = function(self) { return self.Mean_2095; }
    static StandardDeviation_2102 = function(self) { return self.StandardDeviation_2102; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(NormalDistribution_130_Type);
    static Implements = [Value_16_Concept];
}
class PoissonDistribution_131_Type
{
    constructor(Expected_2109, Occurrences_2116)
    {
        // field initialization 
        this.Expected_2109 = Expected_2109;
        this.Occurrences_2116 = Occurrences_2116;
        this.Default_3186 = PoissonDistribution_131_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Expected_2109 = function(self) { return self.Expected_2109; }
    static Occurrences_2116 = function(self) { return self.Occurrences_2116; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(PoissonDistribution_131_Type);
    static Implements = [Value_16_Concept];
}
class BernoulliDistribution_132_Type
{
    constructor(P_2123)
    {
        // field initialization 
        this.P_2123 = P_2123;
        this.Default_3186 = BernoulliDistribution_132_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static P_2123 = function(self) { return self.P_2123; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(BernoulliDistribution_132_Type);
    static Implements = [Value_16_Concept];
}
class Probability_133_Type
{
    constructor(Value_2130)
    {
        // field initialization 
        this.Value_2130 = Value_2130;
        this.Default_3186 = Probability_133_Type.Value_16_Concept.Default_3186;
        this.Add_3334 = Probability_133_Type.Arithmetic_23_Concept.Add_3334;
        this.Negative_3344 = Probability_133_Type.Arithmetic_23_Concept.Negative_3344;
        this.Reciprocal_3354 = Probability_133_Type.Arithmetic_23_Concept.Reciprocal_3354;
        this.Multiply_3371 = Probability_133_Type.Arithmetic_23_Concept.Multiply_3371;
        this.Divide_3388 = Probability_133_Type.Arithmetic_23_Concept.Divide_3388;
        this.Modulo_3405 = Probability_133_Type.Arithmetic_23_Concept.Modulo_3405;
        this.Equals_3315 = Probability_133_Type.Equatable_22_Concept.Equals_3315;
        this.Compare_3295 = Probability_133_Type.Comparable_21_Concept.Compare_3295;
        this.Magnitude_3290 = Probability_133_Type.Magnitudinal_20_Concept.Magnitude_3290;
        this.Add_3421 = Probability_133_Type.ScalarArithmetic_24_Concept.Add_3421;
        this.Subtract_3435 = Probability_133_Type.ScalarArithmetic_24_Concept.Subtract_3435;
        this.Multiply_3449 = Probability_133_Type.ScalarArithmetic_24_Concept.Multiply_3449;
        this.Divide_3463 = Probability_133_Type.ScalarArithmetic_24_Concept.Divide_3463;
        this.Modulo_3477 = Probability_133_Type.ScalarArithmetic_24_Concept.Modulo_3477;
        this.FieldTypes_3232 = Probability_133_Type.Numerical_19_Concept.FieldTypes_3232;
        this.Zero_3242 = Probability_133_Type.Numerical_19_Concept.Zero_3242;
        this.One_3252 = Probability_133_Type.Numerical_19_Concept.One_3252;
        this.MinValue_3262 = Probability_133_Type.Numerical_19_Concept.MinValue_3262;
        this.MaxValue_3272 = Probability_133_Type.Numerical_19_Concept.MaxValue_3272;
    }
    // field accessors
    static Value_2130 = function(self) { return self.Value_2130; }
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
    constructor(Trials_2137, P_2144)
    {
        // field initialization 
        this.Trials_2137 = Trials_2137;
        this.P_2144 = P_2144;
        this.Default_3186 = BinomialDistribution_134_Type.Value_16_Concept.Default_3186;
    }
    // field accessors
    static Trials_2137 = function(self) { return self.Trials_2137; }
    static P_2144 = function(self) { return self.P_2144; }
    // implemented concepts 
    static Value_16_Concept = new Value_16_Concept(BinomialDistribution_134_Type);
    static Implements = [Value_16_Concept];
}

// This is appended to every JavaScript program generated from Plato