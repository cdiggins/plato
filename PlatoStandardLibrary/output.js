class P_Vector
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Count_1412 = function (P_v) 
    // ParameterSymbol=v$1411:Vector Declared:Vector
    // Candidates = Vector
    // Called function: Ref=>TypeDefSymbol=Count$26: candidates = Vector_13.Count_143, Array_24.Count_210
    // Called function: Ref=>FunctionGroupSymbol=FieldTypes$187: candidates = Value_22.FieldTypes_186
    { return P_Count(P_FieldTypes(P_Self)); };
    static P_At_1415 = function (P_v, P_n) 
    // ParameterSymbol=v$1413:Vector Declared:Vector, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Vector
    // ParameterSymbol=n$1414:Index Declared:Index, Argument:Ref=>FunctionGroupSymbol=At$213:(1/2)
    // Candidates = Index
    // Called function: Ref=>FunctionGroupSymbol=At$213: candidates = Vector_13.At_145, Array_24.At_212
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    { return P_At(P_FieldValues(P_v), P_n); };
}
class P_Measure
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Value_1417 = function (P_x) 
    // ParameterSymbol=x$1416:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // Called function: Ref=>FunctionGroupSymbol=At$213: candidates = Vector_13.At_145, Array_24.At_212
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    { return P_At(P_FieldValues(P_x), 0); };
}
class P_Numerical
{
    constructor()
    {
    }
    // field accessors
    // functions 
}
class P_Magnitude
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Magnitude_1419 = function (P_x) 
    // ParameterSymbol=x$1418:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // Called function: Ref=>FunctionGroupSymbol=SquareRoot$1205: candidates = Numerical_135.SquareRoot_1204
    // Called function: Ref=>FunctionGroupSymbol=Sum$1163: candidates = Vector_134.Sum_1162
    // Called function: Ref=>FunctionGroupSymbol=Square$1209: candidates = Numerical_135.Square_1208
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    { return P_SquareRoot(P_Sum(P_Square(P_FieldValues(P_x)))); };
}
class P_Comparable
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Compare_1422 = function (P_a, P_b) 
    // ParameterSymbol=a$1420:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=Magnitude$150:(0/1), Argument:Ref=>FunctionGroupSymbol=Magnitude$150:(0/1)
    // Candidates = Self
    // ParameterSymbol=b$1421:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=Magnitude$150:(0/1), Argument:Ref=>FunctionGroupSymbol=Magnitude$150:(0/1)
    // Candidates = Self
    // Called function: Ref=>FunctionGroupSymbol=LessThan$1283: candidates = Comparable_136.LessThan_1282
    // Called function: Ref=>FunctionGroupSymbol=Magnitude$150: candidates = Magnitude_16.Magnitude_149
    // Called function: Ref=>FunctionGroupSymbol=Magnitude$150: candidates = Magnitude_16.Magnitude_149
    // Called function: Ref=>FunctionGroupSymbol=Negative$158: candidates = Arithmetic_19.Negative_157
    // Called function: Ref=>FunctionGroupSymbol=GreaterThan$1291: candidates = Comparable_136.GreaterThan_1290
    // Called function: Ref=>FunctionGroupSymbol=Magnitude$150: candidates = Magnitude_16.Magnitude_149
    // Called function: Ref=>FunctionGroupSymbol=Magnitude$150: candidates = Magnitude_16.Magnitude_149
    { return P_LessThan(P_Magnitude(P_a), P_Magnitude(P_b)
        ? P_Negative(1)
        : P_GreaterThan(P_Magnitude(P_a), P_Magnitude(P_b)
            ? 1
            : 0
        )
    ); };
}
class P_Equatable
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Equals_1425 = function (P_a, P_b) 
    // ParameterSymbol=a$1423:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=b$1424:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // Called function: Ref=>FunctionGroupSymbol=All$1333: candidates = Array_139.All_1332
    // Called function: Ref=>FunctionGroupSymbol=Equals$1281: candidates = Equatable_18.Equals_153, Comparable_136.Equals_1280
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    { return P_All(P_Equals(P_FieldValues(P_a), P_FieldValues(P_b))); };
}
class P_Arithmetic
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Add_1428 = function (P_self, P_other) 
    // ParameterSymbol=self$1426:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=other$1427:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // Called function: Ref=>FunctionGroupSymbol=Add$169: candidates = Arithmetic_19.Add_155, ScalarArithmetic_20.Add_168
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    { return P_Add(P_FieldValues(P_self), P_FieldValues(P_other)); };
    static P_Negative_1430 = function (P_self) 
    // ParameterSymbol=self$1429:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // Called function: Ref=>FunctionGroupSymbol=Negative$158: candidates = Arithmetic_19.Negative_157
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    { return P_Negative(P_FieldValues(P_self)); };
    static P_Reciprocal_1432 = function (P_self) 
    // ParameterSymbol=self$1431:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // Called function: Ref=>FunctionGroupSymbol=Reciprocal$160: candidates = Arithmetic_19.Reciprocal_159
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    { return P_Reciprocal(P_FieldValues(P_self)); };
    static P_Multiply_1435 = function (P_self, P_other) 
    // ParameterSymbol=self$1433:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=other$1434:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // Called function: Ref=>FunctionGroupSymbol=Add$169: candidates = Arithmetic_19.Add_155, ScalarArithmetic_20.Add_168
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    { return P_Add(P_FieldValues(P_self), P_FieldValues(P_other)); };
    static P_Divide_1438 = function (P_self, P_other) 
    // ParameterSymbol=self$1436:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=other$1437:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    { return P_Divide(P_FieldValues(P_self), P_FieldValues(P_other)); };
    static P_Modulo_1441 = function (P_self, P_other) 
    // ParameterSymbol=self$1439:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=other$1440:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // Called function: Ref=>FunctionGroupSymbol=Modulo$177: candidates = Arithmetic_19.Modulo_165, ScalarArithmetic_20.Modulo_176
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    { return P_Modulo(P_FieldValues(P_self), P_FieldValues(P_other)); };
}
class P_ScalarArithmetic
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Add_1445 = function (P_self, P_scalar) 
    // ParameterSymbol=self$1443:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=scalar$1444:T Declared:T, Argument:Ref=>FunctionGroupSymbol=Add$169:(1/2)
    // Candidates = T
    // Called function: Ref=>FunctionGroupSymbol=Add$169: candidates = Arithmetic_19.Add_155, ScalarArithmetic_20.Add_168
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    { return P_Add(P_FieldValues(P_self), P_scalar); };
    static P_Subtract_1448 = function (P_self, P_scalar) 
    // ParameterSymbol=self$1446:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=Add$169:(0/2)
    // Candidates = Self
    // ParameterSymbol=scalar$1447:T Declared:T, Argument:Ref=>FunctionGroupSymbol=Negative$158:(0/1)
    // Candidates = T
    // Called function: Ref=>FunctionGroupSymbol=Add$169: candidates = Arithmetic_19.Add_155, ScalarArithmetic_20.Add_168
    // Called function: Ref=>FunctionGroupSymbol=Negative$158: candidates = Arithmetic_19.Negative_157
    { return P_Add(P_self, P_Negative(P_scalar)); };
    static P_Multiply_1451 = function (P_self, P_scalar) 
    // ParameterSymbol=self$1449:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=scalar$1450:T Declared:T, Argument:Ref=>FunctionGroupSymbol=Multiply$173:(1/2)
    // Candidates = T
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    { return P_Multiply(P_FieldValues(P_self), P_scalar); };
    static P_Divide_1454 = function (P_self, P_scalar) 
    // ParameterSymbol=self$1452:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Self
    // ParameterSymbol=scalar$1453:T Declared:T, Argument:Ref=>FunctionGroupSymbol=Reciprocal$160:(0/1)
    // Candidates = T
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Reciprocal$160: candidates = Arithmetic_19.Reciprocal_159
    { return P_Multiply(P_self, P_Reciprocal(P_scalar)); };
    static P_Modulo_1457 = function (P_self, P_scalar) 
    // ParameterSymbol=self$1455:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=scalar$1456:T Declared:T, Argument:Ref=>FunctionGroupSymbol=Modulo$177:(1/2)
    // Candidates = T
    // Called function: Ref=>FunctionGroupSymbol=Modulo$177: candidates = Arithmetic_19.Modulo_165, ScalarArithmetic_20.Modulo_176
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    { return P_Modulo(P_FieldValues(P_self), P_scalar); };
}
class P_Boolean
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_And_1460 = function (P_a, P_b) 
    // ParameterSymbol=a$1458:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=b$1459:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // Called function: Ref=>FunctionGroupSymbol=And$179: candidates = Boolean_21.And_178
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    { return P_And(P_FieldValues(P_a), P_FieldValues(P_b)); };
    static P_Or_1463 = function (P_a, P_b) 
    // ParameterSymbol=a$1461:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=b$1462:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // Called function: Ref=>FunctionGroupSymbol=Or$181: candidates = Boolean_21.Or_180
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    { return P_Or(P_FieldValues(P_a), P_FieldValues(P_b)); };
    static P_Not_1465 = function (P_a) 
    // ParameterSymbol=a$1464:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // Called function: Ref=>FunctionGroupSymbol=Not$183: candidates = Boolean_21.Not_182
    // Called function: Ref=>FunctionGroupSymbol=FieldValues$191: candidates = Value_22.FieldValues_190
    { return P_Not(P_FieldValues(P_a)); };
}
class P_Value
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Type_1466 = function () 
    { return P_intrinsic; };
    static P_FieldTypes_1467 = function () 
    { return P_intrinsic; };
    static P_FieldNames_1468 = function () 
    { return P_intrinsic; };
    static P_FieldValues_1470 = function (P_self) 
    // ParameterSymbol=self$1469:Self Declared:Self
    // Candidates = Self
    { return P_intrinsic; };
    static P_Zero_1471 = function () 
    // Called function: Ref=>FunctionGroupSymbol=Zero$193: candidates = Value_22.Zero_192
    { return P_Zero(P_FieldTypes); };
    static P_One_1472 = function () 
    // Called function: Ref=>FunctionGroupSymbol=One$195: candidates = Value_22.One_194
    { return P_One(P_FieldTypes); };
    static P_Default_1473 = function () 
    // Called function: Ref=>FunctionGroupSymbol=Default$197: candidates = Value_22.Default_196
    { return P_Default(P_FieldTypes); };
    static P_MinValue_1474 = function () 
    // Called function: Ref=>FunctionGroupSymbol=MinValue$199: candidates = Value_22.MinValue_198
    { return P_MinValue(P_FieldTypes); };
    static P_MaxValue_1475 = function () 
    // Called function: Ref=>FunctionGroupSymbol=MaxValue$201: candidates = Value_22.MaxValue_200
    { return P_MaxValue(P_FieldTypes); };
    static P_ToString_1477 = function (P_x) 
    // ParameterSymbol=x$1476:Self Declared:Self
    // Candidates = Self
    // Called function: Ref=>FunctionGroupSymbol=JoinStrings$1335: candidates = Array_139.JoinStrings_1334
    { return P_JoinStrings(P_FieldValues, ","); };
}
class P_Interval
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Min_1480 = function (P_x) 
    // ParameterSymbol=x$1479:Self Declared:Self
    // Candidates = Self
    { return null; };
    static P_Max_1482 = function (P_x) 
    // ParameterSymbol=x$1481:Self Declared:Self
    // Candidates = Self
    { return null; };
}
class P_Array
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Count_1485 = function (P_xs) 
    // ParameterSymbol=xs$1484:Self Declared:Self
    // Candidates = Self
    { return null; };
    static P_At_1488 = function (P_xs, P_n) 
    // ParameterSymbol=xs$1486:Self Declared:Self
    // Candidates = Self
    // ParameterSymbol=n$1487:Index Declared:Index
    // Candidates = Index
    { return null; };
}
class P_Integer
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value_214 = function(self) { return self._field_Value; }
    // functions 
}
class P_Count
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value_218 = function(self) { return self._field_Value; }
    // functions 
}
class P_Index
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value_222 = function(self) { return self._field_Value; }
    // functions 
}
class P_Number
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value_226 = function(self) { return self._field_Value; }
    // functions 
}
class P_Unit
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value_230 = function(self) { return self._field_Value; }
    // functions 
}
class P_Percent
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value_234 = function(self) { return self._field_Value; }
    // functions 
}
class P_Quaternion
{
    constructor(P_X, P_Y, P_Z, P_W)
    {
        this._field_X = P_X;
        this._field_Y = P_Y;
        this._field_Z = P_Z;
        this._field_W = P_W;
    }
    // field accessors
    static P_X_238 = function(self) { return self._field_X; }
    static P_Y_242 = function(self) { return self._field_Y; }
    static P_Z_246 = function(self) { return self._field_Z; }
    static P_W_250 = function(self) { return self._field_W; }
    // functions 
}
class P_Unit2D
{
    constructor(P_X, P_Y)
    {
        this._field_X = P_X;
        this._field_Y = P_Y;
    }
    // field accessors
    static P_X_254 = function(self) { return self._field_X; }
    static P_Y_258 = function(self) { return self._field_Y; }
    // functions 
}
class P_Unit3D
{
    constructor(P_X, P_Y, P_Z)
    {
        this._field_X = P_X;
        this._field_Y = P_Y;
        this._field_Z = P_Z;
    }
    // field accessors
    static P_X_262 = function(self) { return self._field_X; }
    static P_Y_266 = function(self) { return self._field_Y; }
    static P_Z_270 = function(self) { return self._field_Z; }
    // functions 
}
class P_Direction3D
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value_274 = function(self) { return self._field_Value; }
    // functions 
}
class P_AxisAngle
{
    constructor(P_Axis, P_Angle)
    {
        this._field_Axis = P_Axis;
        this._field_Angle = P_Angle;
    }
    // field accessors
    static P_Axis_278 = function(self) { return self._field_Axis; }
    static P_Angle_282 = function(self) { return self._field_Angle; }
    // functions 
}
class P_EulerAngles
{
    constructor(P_Yaw, P_Pitch, P_Roll)
    {
        this._field_Yaw = P_Yaw;
        this._field_Pitch = P_Pitch;
        this._field_Roll = P_Roll;
    }
    // field accessors
    static P_Yaw_286 = function(self) { return self._field_Yaw; }
    static P_Pitch_290 = function(self) { return self._field_Pitch; }
    static P_Roll_294 = function(self) { return self._field_Roll; }
    // functions 
}
class P_Rotation3D
{
    constructor(P_Quaternion)
    {
        this._field_Quaternion = P_Quaternion;
    }
    // field accessors
    static P_Quaternion_298 = function(self) { return self._field_Quaternion; }
    // functions 
}
class P_Vector2D
{
    constructor(P_X, P_Y)
    {
        this._field_X = P_X;
        this._field_Y = P_Y;
    }
    // field accessors
    static P_X_302 = function(self) { return self._field_X; }
    static P_Y_306 = function(self) { return self._field_Y; }
    // functions 
}
class P_Vector3D
{
    constructor(P_X, P_Y, P_Z)
    {
        this._field_X = P_X;
        this._field_Y = P_Y;
        this._field_Z = P_Z;
    }
    // field accessors
    static P_X_310 = function(self) { return self._field_X; }
    static P_Y_314 = function(self) { return self._field_Y; }
    static P_Z_318 = function(self) { return self._field_Z; }
    // functions 
}
class P_Vector4D
{
    constructor(P_X, P_Y, P_Z, P_W)
    {
        this._field_X = P_X;
        this._field_Y = P_Y;
        this._field_Z = P_Z;
        this._field_W = P_W;
    }
    // field accessors
    static P_X_322 = function(self) { return self._field_X; }
    static P_Y_326 = function(self) { return self._field_Y; }
    static P_Z_330 = function(self) { return self._field_Z; }
    static P_W_334 = function(self) { return self._field_W; }
    // functions 
}
class P_Orientation3D
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value_338 = function(self) { return self._field_Value; }
    // functions 
}
class P_Pose2D
{
    constructor(P_Position, P_Orientation)
    {
        this._field_Position = P_Position;
        this._field_Orientation = P_Orientation;
    }
    // field accessors
    static P_Position_342 = function(self) { return self._field_Position; }
    static P_Orientation_346 = function(self) { return self._field_Orientation; }
    // functions 
}
class P_Pose3D
{
    constructor(P_Position, P_Orientation)
    {
        this._field_Position = P_Position;
        this._field_Orientation = P_Orientation;
    }
    // field accessors
    static P_Position_350 = function(self) { return self._field_Position; }
    static P_Orientation_354 = function(self) { return self._field_Orientation; }
    // functions 
}
class P_Transform3D
{
    constructor(P_Translation, P_Rotation, P_Scale)
    {
        this._field_Translation = P_Translation;
        this._field_Rotation = P_Rotation;
        this._field_Scale = P_Scale;
    }
    // field accessors
    static P_Translation_358 = function(self) { return self._field_Translation; }
    static P_Rotation_362 = function(self) { return self._field_Rotation; }
    static P_Scale_366 = function(self) { return self._field_Scale; }
    // functions 
}
class P_Transform2D
{
    constructor(P_Translation, P_Rotation, P_Scale)
    {
        this._field_Translation = P_Translation;
        this._field_Rotation = P_Rotation;
        this._field_Scale = P_Scale;
    }
    // field accessors
    static P_Translation_370 = function(self) { return self._field_Translation; }
    static P_Rotation_374 = function(self) { return self._field_Rotation; }
    static P_Scale_378 = function(self) { return self._field_Scale; }
    // functions 
}
class P_AlignedBox2D
{
    constructor(P_A, P_B)
    {
        this._field_A = P_A;
        this._field_B = P_B;
    }
    // field accessors
    static P_A_382 = function(self) { return self._field_A; }
    static P_B_386 = function(self) { return self._field_B; }
    // functions 
}
class P_AlignedBox3D
{
    constructor(P_A, P_B)
    {
        this._field_A = P_A;
        this._field_B = P_B;
    }
    // field accessors
    static P_A_390 = function(self) { return self._field_A; }
    static P_B_394 = function(self) { return self._field_B; }
    // functions 
}
class P_Complex
{
    constructor(P_Real, P_Imaginary)
    {
        this._field_Real = P_Real;
        this._field_Imaginary = P_Imaginary;
    }
    // field accessors
    static P_Real_398 = function(self) { return self._field_Real; }
    static P_Imaginary_402 = function(self) { return self._field_Imaginary; }
    // functions 
}
class P_Ray3D
{
    constructor(P_Direction, P_Position)
    {
        this._field_Direction = P_Direction;
        this._field_Position = P_Position;
    }
    // field accessors
    static P_Direction_406 = function(self) { return self._field_Direction; }
    static P_Position_410 = function(self) { return self._field_Position; }
    // functions 
}
class P_Ray2D
{
    constructor(P_Direction, P_Position)
    {
        this._field_Direction = P_Direction;
        this._field_Position = P_Position;
    }
    // field accessors
    static P_Direction_414 = function(self) { return self._field_Direction; }
    static P_Position_418 = function(self) { return self._field_Position; }
    // functions 
}
class P_Sphere
{
    constructor(P_Center, P_Radius)
    {
        this._field_Center = P_Center;
        this._field_Radius = P_Radius;
    }
    // field accessors
    static P_Center_422 = function(self) { return self._field_Center; }
    static P_Radius_426 = function(self) { return self._field_Radius; }
    // functions 
}
class P_Plane
{
    constructor(P_Normal, P_D)
    {
        this._field_Normal = P_Normal;
        this._field_D = P_D;
    }
    // field accessors
    static P_Normal_430 = function(self) { return self._field_Normal; }
    static P_D_434 = function(self) { return self._field_D; }
    // functions 
}
class P_Triangle3D
{
    constructor(P_A, P_B, P_C)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
    }
    // field accessors
    static P_A_438 = function(self) { return self._field_A; }
    static P_B_442 = function(self) { return self._field_B; }
    static P_C_446 = function(self) { return self._field_C; }
    // functions 
}
class P_Triangle2D
{
    constructor(P_A, P_B, P_C)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
    }
    // field accessors
    static P_A_450 = function(self) { return self._field_A; }
    static P_B_454 = function(self) { return self._field_B; }
    static P_C_458 = function(self) { return self._field_C; }
    // functions 
}
class P_Quad3D
{
    constructor(P_A, P_B, P_C, P_D)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
        this._field_D = P_D;
    }
    // field accessors
    static P_A_462 = function(self) { return self._field_A; }
    static P_B_466 = function(self) { return self._field_B; }
    static P_C_470 = function(self) { return self._field_C; }
    static P_D_474 = function(self) { return self._field_D; }
    // functions 
}
class P_Quad2D
{
    constructor(P_A, P_B, P_C, P_D)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
        this._field_D = P_D;
    }
    // field accessors
    static P_A_478 = function(self) { return self._field_A; }
    static P_B_482 = function(self) { return self._field_B; }
    static P_C_486 = function(self) { return self._field_C; }
    static P_D_490 = function(self) { return self._field_D; }
    // functions 
}
class P_Point3D
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value_494 = function(self) { return self._field_Value; }
    // functions 
}
class P_Point2D
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value_498 = function(self) { return self._field_Value; }
    // functions 
}
class P_Line3D
{
    constructor(P_A, P_B)
    {
        this._field_A = P_A;
        this._field_B = P_B;
    }
    // field accessors
    static P_A_502 = function(self) { return self._field_A; }
    static P_B_506 = function(self) { return self._field_B; }
    // functions 
}
class P_Line2D
{
    constructor(P_A, P_B)
    {
        this._field_A = P_A;
        this._field_B = P_B;
    }
    // field accessors
    static P_A_510 = function(self) { return self._field_A; }
    static P_B_514 = function(self) { return self._field_B; }
    // functions 
}
class P_Color
{
    constructor(P_R, P_G, P_B, P_A)
    {
        this._field_R = P_R;
        this._field_G = P_G;
        this._field_B = P_B;
        this._field_A = P_A;
    }
    // field accessors
    static P_R_518 = function(self) { return self._field_R; }
    static P_G_522 = function(self) { return self._field_G; }
    static P_B_526 = function(self) { return self._field_B; }
    static P_A_530 = function(self) { return self._field_A; }
    // functions 
}
class P_ColorLUV
{
    constructor(P_Lightness, P_U, P_V)
    {
        this._field_Lightness = P_Lightness;
        this._field_U = P_U;
        this._field_V = P_V;
    }
    // field accessors
    static P_Lightness_534 = function(self) { return self._field_Lightness; }
    static P_U_538 = function(self) { return self._field_U; }
    static P_V_542 = function(self) { return self._field_V; }
    // functions 
}
class P_ColorLAB
{
    constructor(P_Lightness, P_A, P_B)
    {
        this._field_Lightness = P_Lightness;
        this._field_A = P_A;
        this._field_B = P_B;
    }
    // field accessors
    static P_Lightness_546 = function(self) { return self._field_Lightness; }
    static P_A_550 = function(self) { return self._field_A; }
    static P_B_554 = function(self) { return self._field_B; }
    // functions 
}
class P_ColorLCh
{
    constructor(P_Lightness, P_ChromaHue)
    {
        this._field_Lightness = P_Lightness;
        this._field_ChromaHue = P_ChromaHue;
    }
    // field accessors
    static P_Lightness_558 = function(self) { return self._field_Lightness; }
    static P_ChromaHue_562 = function(self) { return self._field_ChromaHue; }
    // functions 
}
class P_ColorHSV
{
    constructor(P_Hue, P_S, P_V)
    {
        this._field_Hue = P_Hue;
        this._field_S = P_S;
        this._field_V = P_V;
    }
    // field accessors
    static P_Hue_566 = function(self) { return self._field_Hue; }
    static P_S_570 = function(self) { return self._field_S; }
    static P_V_574 = function(self) { return self._field_V; }
    // functions 
}
class P_ColorHSL
{
    constructor(P_Hue, P_Saturation, P_Luminance)
    {
        this._field_Hue = P_Hue;
        this._field_Saturation = P_Saturation;
        this._field_Luminance = P_Luminance;
    }
    // field accessors
    static P_Hue_578 = function(self) { return self._field_Hue; }
    static P_Saturation_582 = function(self) { return self._field_Saturation; }
    static P_Luminance_586 = function(self) { return self._field_Luminance; }
    // functions 
}
class P_ColorYCbCr
{
    constructor(P_Y, P_Cb, P_Cr)
    {
        this._field_Y = P_Y;
        this._field_Cb = P_Cb;
        this._field_Cr = P_Cr;
    }
    // field accessors
    static P_Y_590 = function(self) { return self._field_Y; }
    static P_Cb_594 = function(self) { return self._field_Cb; }
    static P_Cr_598 = function(self) { return self._field_Cr; }
    // functions 
}
class P_SphericalCoordinate
{
    constructor(P_Radius, P_Azimuth, P_Polar)
    {
        this._field_Radius = P_Radius;
        this._field_Azimuth = P_Azimuth;
        this._field_Polar = P_Polar;
    }
    // field accessors
    static P_Radius_602 = function(self) { return self._field_Radius; }
    static P_Azimuth_606 = function(self) { return self._field_Azimuth; }
    static P_Polar_610 = function(self) { return self._field_Polar; }
    // functions 
}
class P_PolarCoordinate
{
    constructor(P_Radius, P_Angle)
    {
        this._field_Radius = P_Radius;
        this._field_Angle = P_Angle;
    }
    // field accessors
    static P_Radius_614 = function(self) { return self._field_Radius; }
    static P_Angle_618 = function(self) { return self._field_Angle; }
    // functions 
}
class P_LogPolarCoordinate
{
    constructor(P_Rho, P_Azimuth)
    {
        this._field_Rho = P_Rho;
        this._field_Azimuth = P_Azimuth;
    }
    // field accessors
    static P_Rho_622 = function(self) { return self._field_Rho; }
    static P_Azimuth_626 = function(self) { return self._field_Azimuth; }
    // functions 
}
class P_CylindricalCoordinate
{
    constructor(P_RadialDistance, P_Azimuth, P_Height)
    {
        this._field_RadialDistance = P_RadialDistance;
        this._field_Azimuth = P_Azimuth;
        this._field_Height = P_Height;
    }
    // field accessors
    static P_RadialDistance_630 = function(self) { return self._field_RadialDistance; }
    static P_Azimuth_634 = function(self) { return self._field_Azimuth; }
    static P_Height_638 = function(self) { return self._field_Height; }
    // functions 
}
class P_HorizontalCoordinate
{
    constructor(P_Radius, P_Azimuth, P_Height)
    {
        this._field_Radius = P_Radius;
        this._field_Azimuth = P_Azimuth;
        this._field_Height = P_Height;
    }
    // field accessors
    static P_Radius_642 = function(self) { return self._field_Radius; }
    static P_Azimuth_646 = function(self) { return self._field_Azimuth; }
    static P_Height_650 = function(self) { return self._field_Height; }
    // functions 
}
class P_GeoCoordinate
{
    constructor(P_Latitude, P_Longitude)
    {
        this._field_Latitude = P_Latitude;
        this._field_Longitude = P_Longitude;
    }
    // field accessors
    static P_Latitude_654 = function(self) { return self._field_Latitude; }
    static P_Longitude_658 = function(self) { return self._field_Longitude; }
    // functions 
}
class P_GeoCoordinateWithAltitude
{
    constructor(P_Coordinate, P_Altitude)
    {
        this._field_Coordinate = P_Coordinate;
        this._field_Altitude = P_Altitude;
    }
    // field accessors
    static P_Coordinate_662 = function(self) { return self._field_Coordinate; }
    static P_Altitude_666 = function(self) { return self._field_Altitude; }
    // functions 
}
class P_Circle
{
    constructor(P_Center, P_Radius)
    {
        this._field_Center = P_Center;
        this._field_Radius = P_Radius;
    }
    // field accessors
    static P_Center_670 = function(self) { return self._field_Center; }
    static P_Radius_674 = function(self) { return self._field_Radius; }
    // functions 
}
class P_Chord
{
    constructor(P_Circle, P_Arc)
    {
        this._field_Circle = P_Circle;
        this._field_Arc = P_Arc;
    }
    // field accessors
    static P_Circle_678 = function(self) { return self._field_Circle; }
    static P_Arc_682 = function(self) { return self._field_Arc; }
    // functions 
}
class P_Size2D
{
    constructor(P_Width, P_Height)
    {
        this._field_Width = P_Width;
        this._field_Height = P_Height;
    }
    // field accessors
    static P_Width_686 = function(self) { return self._field_Width; }
    static P_Height_690 = function(self) { return self._field_Height; }
    // functions 
}
class P_Size3D
{
    constructor(P_Width, P_Height, P_Depth)
    {
        this._field_Width = P_Width;
        this._field_Height = P_Height;
        this._field_Depth = P_Depth;
    }
    // field accessors
    static P_Width_694 = function(self) { return self._field_Width; }
    static P_Height_698 = function(self) { return self._field_Height; }
    static P_Depth_702 = function(self) { return self._field_Depth; }
    // functions 
}
class P_Rectangle2D
{
    constructor(P_Center, P_Size)
    {
        this._field_Center = P_Center;
        this._field_Size = P_Size;
    }
    // field accessors
    static P_Center_706 = function(self) { return self._field_Center; }
    static P_Size_710 = function(self) { return self._field_Size; }
    // functions 
}
class P_Proportion
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value_714 = function(self) { return self._field_Value; }
    // functions 
}
class P_Fraction
{
    constructor(P_Numerator, P_Denominator)
    {
        this._field_Numerator = P_Numerator;
        this._field_Denominator = P_Denominator;
    }
    // field accessors
    static P_Numerator_718 = function(self) { return self._field_Numerator; }
    static P_Denominator_722 = function(self) { return self._field_Denominator; }
    // functions 
}
class P_Angle
{
    constructor(P_Radians)
    {
        this._field_Radians = P_Radians;
    }
    // field accessors
    static P_Radians_726 = function(self) { return self._field_Radians; }
    // functions 
}
class P_Length
{
    constructor(P_Meters)
    {
        this._field_Meters = P_Meters;
    }
    // field accessors
    static P_Meters_730 = function(self) { return self._field_Meters; }
    // functions 
}
class P_Mass
{
    constructor(P_Kilograms)
    {
        this._field_Kilograms = P_Kilograms;
    }
    // field accessors
    static P_Kilograms_734 = function(self) { return self._field_Kilograms; }
    // functions 
}
class P_Temperature
{
    constructor(P_Celsius)
    {
        this._field_Celsius = P_Celsius;
    }
    // field accessors
    static P_Celsius_738 = function(self) { return self._field_Celsius; }
    // functions 
}
class P_TimeSpan
{
    constructor(P_Seconds)
    {
        this._field_Seconds = P_Seconds;
    }
    // field accessors
    static P_Seconds_742 = function(self) { return self._field_Seconds; }
    // functions 
}
class P_TimeRange
{
    constructor(P_Min, P_Max)
    {
        this._field_Min = P_Min;
        this._field_Max = P_Max;
    }
    // field accessors
    static P_Min_746 = function(self) { return self._field_Min; }
    static P_Max_750 = function(self) { return self._field_Max; }
    // functions 
}
class P_DateTime
{
    constructor()
    {
    }
    // field accessors
    // functions 
}
class P_AnglePair
{
    constructor(P_Start, P_End)
    {
        this._field_Start = P_Start;
        this._field_End = P_End;
    }
    // field accessors
    static P_Start_754 = function(self) { return self._field_Start; }
    static P_End_758 = function(self) { return self._field_End; }
    // functions 
}
class P_Ring
{
    constructor(P_Circle, P_InnerRadius)
    {
        this._field_Circle = P_Circle;
        this._field_InnerRadius = P_InnerRadius;
    }
    // field accessors
    static P_Circle_762 = function(self) { return self._field_Circle; }
    static P_InnerRadius_766 = function(self) { return self._field_InnerRadius; }
    // functions 
}
class P_Arc
{
    constructor(P_Angles, P_Cirlce)
    {
        this._field_Angles = P_Angles;
        this._field_Cirlce = P_Cirlce;
    }
    // field accessors
    static P_Angles_770 = function(self) { return self._field_Angles; }
    static P_Cirlce_774 = function(self) { return self._field_Cirlce; }
    // functions 
}
class P_TimeInterval
{
    constructor(P_Start, P_End)
    {
        this._field_Start = P_Start;
        this._field_End = P_End;
    }
    // field accessors
    static P_Start_778 = function(self) { return self._field_Start; }
    static P_End_782 = function(self) { return self._field_End; }
    // functions 
}
class P_RealInterval
{
    constructor(P_A, P_B)
    {
        this._field_A = P_A;
        this._field_B = P_B;
    }
    // field accessors
    static P_A_786 = function(self) { return self._field_A; }
    static P_B_790 = function(self) { return self._field_B; }
    // functions 
}
class P_Interval2D
{
    constructor(P_A, P_B)
    {
        this._field_A = P_A;
        this._field_B = P_B;
    }
    // field accessors
    static P_A_794 = function(self) { return self._field_A; }
    static P_B_798 = function(self) { return self._field_B; }
    // functions 
}
class P_Interval3D
{
    constructor(P_A, P_B)
    {
        this._field_A = P_A;
        this._field_B = P_B;
    }
    // field accessors
    static P_A_802 = function(self) { return self._field_A; }
    static P_B_806 = function(self) { return self._field_B; }
    // functions 
}
class P_Capsule
{
    constructor(P_Line, P_Radius)
    {
        this._field_Line = P_Line;
        this._field_Radius = P_Radius;
    }
    // field accessors
    static P_Line_810 = function(self) { return self._field_Line; }
    static P_Radius_814 = function(self) { return self._field_Radius; }
    // functions 
}
class P_Matrix3D
{
    constructor(P_Column1, P_Column2, P_Column3, P_Column4)
    {
        this._field_Column1 = P_Column1;
        this._field_Column2 = P_Column2;
        this._field_Column3 = P_Column3;
        this._field_Column4 = P_Column4;
    }
    // field accessors
    static P_Column1_818 = function(self) { return self._field_Column1; }
    static P_Column2_822 = function(self) { return self._field_Column2; }
    static P_Column3_826 = function(self) { return self._field_Column3; }
    static P_Column4_830 = function(self) { return self._field_Column4; }
    // functions 
}
class P_Cylinder
{
    constructor(P_Line, P_Radius)
    {
        this._field_Line = P_Line;
        this._field_Radius = P_Radius;
    }
    // field accessors
    static P_Line_834 = function(self) { return self._field_Line; }
    static P_Radius_838 = function(self) { return self._field_Radius; }
    // functions 
}
class P_Cone
{
    constructor(P_Line, P_Radius)
    {
        this._field_Line = P_Line;
        this._field_Radius = P_Radius;
    }
    // field accessors
    static P_Line_842 = function(self) { return self._field_Line; }
    static P_Radius_846 = function(self) { return self._field_Radius; }
    // functions 
}
class P_Tube
{
    constructor(P_Line, P_InnerRadius, P_OuterRadius)
    {
        this._field_Line = P_Line;
        this._field_InnerRadius = P_InnerRadius;
        this._field_OuterRadius = P_OuterRadius;
    }
    // field accessors
    static P_Line_850 = function(self) { return self._field_Line; }
    static P_InnerRadius_854 = function(self) { return self._field_InnerRadius; }
    static P_OuterRadius_858 = function(self) { return self._field_OuterRadius; }
    // functions 
}
class P_ConeSegment
{
    constructor(P_Line, P_Radius1, P_Radius2)
    {
        this._field_Line = P_Line;
        this._field_Radius1 = P_Radius1;
        this._field_Radius2 = P_Radius2;
    }
    // field accessors
    static P_Line_862 = function(self) { return self._field_Line; }
    static P_Radius1_866 = function(self) { return self._field_Radius1; }
    static P_Radius2_870 = function(self) { return self._field_Radius2; }
    // functions 
}
class P_Box2D
{
    constructor(P_Center, P_Rotation, P_Extent)
    {
        this._field_Center = P_Center;
        this._field_Rotation = P_Rotation;
        this._field_Extent = P_Extent;
    }
    // field accessors
    static P_Center_874 = function(self) { return self._field_Center; }
    static P_Rotation_878 = function(self) { return self._field_Rotation; }
    static P_Extent_882 = function(self) { return self._field_Extent; }
    // functions 
}
class P_Box3D
{
    constructor(P_Center, P_Rotation, P_Extent)
    {
        this._field_Center = P_Center;
        this._field_Rotation = P_Rotation;
        this._field_Extent = P_Extent;
    }
    // field accessors
    static P_Center_886 = function(self) { return self._field_Center; }
    static P_Rotation_890 = function(self) { return self._field_Rotation; }
    static P_Extent_894 = function(self) { return self._field_Extent; }
    // functions 
}
class P_CubicBezierTriangle3D
{
    constructor(P_A, P_B, P_C, P_A2B, P_AB2, P_B2C, P_BC2, P_AC2, P_A2C, P_ABC)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
        this._field_A2B = P_A2B;
        this._field_AB2 = P_AB2;
        this._field_B2C = P_B2C;
        this._field_BC2 = P_BC2;
        this._field_AC2 = P_AC2;
        this._field_A2C = P_A2C;
        this._field_ABC = P_ABC;
    }
    // field accessors
    static P_A_898 = function(self) { return self._field_A; }
    static P_B_902 = function(self) { return self._field_B; }
    static P_C_906 = function(self) { return self._field_C; }
    static P_A2B_910 = function(self) { return self._field_A2B; }
    static P_AB2_914 = function(self) { return self._field_AB2; }
    static P_B2C_918 = function(self) { return self._field_B2C; }
    static P_BC2_922 = function(self) { return self._field_BC2; }
    static P_AC2_926 = function(self) { return self._field_AC2; }
    static P_A2C_930 = function(self) { return self._field_A2C; }
    static P_ABC_934 = function(self) { return self._field_ABC; }
    // functions 
}
class P_CubicBezier2D
{
    constructor(P_A, P_B, P_C, P_D)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
        this._field_D = P_D;
    }
    // field accessors
    static P_A_938 = function(self) { return self._field_A; }
    static P_B_942 = function(self) { return self._field_B; }
    static P_C_946 = function(self) { return self._field_C; }
    static P_D_950 = function(self) { return self._field_D; }
    // functions 
}
class P_UV
{
    constructor(P_U, P_V)
    {
        this._field_U = P_U;
        this._field_V = P_V;
    }
    // field accessors
    static P_U_954 = function(self) { return self._field_U; }
    static P_V_958 = function(self) { return self._field_V; }
    // functions 
}
class P_UVW
{
    constructor(P_U, P_V, P_W)
    {
        this._field_U = P_U;
        this._field_V = P_V;
        this._field_W = P_W;
    }
    // field accessors
    static P_U_962 = function(self) { return self._field_U; }
    static P_V_966 = function(self) { return self._field_V; }
    static P_W_970 = function(self) { return self._field_W; }
    // functions 
}
class P_CubicBezier3D
{
    constructor(P_A, P_B, P_C, P_D)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
        this._field_D = P_D;
    }
    // field accessors
    static P_A_974 = function(self) { return self._field_A; }
    static P_B_978 = function(self) { return self._field_B; }
    static P_C_982 = function(self) { return self._field_C; }
    static P_D_986 = function(self) { return self._field_D; }
    // functions 
}
class P_QuadraticBezier2D
{
    constructor(P_A, P_B, P_C)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
    }
    // field accessors
    static P_A_990 = function(self) { return self._field_A; }
    static P_B_994 = function(self) { return self._field_B; }
    static P_C_998 = function(self) { return self._field_C; }
    // functions 
}
class P_QuadraticBezier3D
{
    constructor(P_A, P_B, P_C)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
    }
    // field accessors
    static P_A_1002 = function(self) { return self._field_A; }
    static P_B_1006 = function(self) { return self._field_B; }
    static P_C_1010 = function(self) { return self._field_C; }
    // functions 
}
class P_Area
{
    constructor(P_MetersSquared)
    {
        this._field_MetersSquared = P_MetersSquared;
    }
    // field accessors
    static P_MetersSquared_1014 = function(self) { return self._field_MetersSquared; }
    // functions 
}
class P_Volume
{
    constructor(P_MetersCubed)
    {
        this._field_MetersCubed = P_MetersCubed;
    }
    // field accessors
    static P_MetersCubed_1018 = function(self) { return self._field_MetersCubed; }
    // functions 
}
class P_Velocity
{
    constructor(P_MetersPerSecond)
    {
        this._field_MetersPerSecond = P_MetersPerSecond;
    }
    // field accessors
    static P_MetersPerSecond_1022 = function(self) { return self._field_MetersPerSecond; }
    // functions 
}
class P_Acceleration
{
    constructor(P_MetersPerSecondSquared)
    {
        this._field_MetersPerSecondSquared = P_MetersPerSecondSquared;
    }
    // field accessors
    static P_MetersPerSecondSquared_1026 = function(self) { return self._field_MetersPerSecondSquared; }
    // functions 
}
class P_Force
{
    constructor(P_Newtons)
    {
        this._field_Newtons = P_Newtons;
    }
    // field accessors
    static P_Newtons_1030 = function(self) { return self._field_Newtons; }
    // functions 
}
class P_Pressure
{
    constructor(P_Pascals)
    {
        this._field_Pascals = P_Pascals;
    }
    // field accessors
    static P_Pascals_1034 = function(self) { return self._field_Pascals; }
    // functions 
}
class P_Energy
{
    constructor(P_Joules)
    {
        this._field_Joules = P_Joules;
    }
    // field accessors
    static P_Joules_1038 = function(self) { return self._field_Joules; }
    // functions 
}
class P_Memory
{
    constructor(P_Bytes)
    {
        this._field_Bytes = P_Bytes;
    }
    // field accessors
    static P_Bytes_1042 = function(self) { return self._field_Bytes; }
    // functions 
}
class P_Frequency
{
    constructor(P_Hertz)
    {
        this._field_Hertz = P_Hertz;
    }
    // field accessors
    static P_Hertz_1046 = function(self) { return self._field_Hertz; }
    // functions 
}
class P_Loudness
{
    constructor(P_Decibels)
    {
        this._field_Decibels = P_Decibels;
    }
    // field accessors
    static P_Decibels_1050 = function(self) { return self._field_Decibels; }
    // functions 
}
class P_LuminousIntensity
{
    constructor(P_Candelas)
    {
        this._field_Candelas = P_Candelas;
    }
    // field accessors
    static P_Candelas_1054 = function(self) { return self._field_Candelas; }
    // functions 
}
class P_ElectricPotential
{
    constructor(P_Volts)
    {
        this._field_Volts = P_Volts;
    }
    // field accessors
    static P_Volts_1058 = function(self) { return self._field_Volts; }
    // functions 
}
class P_ElectricCharge
{
    constructor(P_Columbs)
    {
        this._field_Columbs = P_Columbs;
    }
    // field accessors
    static P_Columbs_1062 = function(self) { return self._field_Columbs; }
    // functions 
}
class P_ElectricCurrent
{
    constructor(P_Amperes)
    {
        this._field_Amperes = P_Amperes;
    }
    // field accessors
    static P_Amperes_1066 = function(self) { return self._field_Amperes; }
    // functions 
}
class P_ElectricResistance
{
    constructor(P_Ohms)
    {
        this._field_Ohms = P_Ohms;
    }
    // field accessors
    static P_Ohms_1070 = function(self) { return self._field_Ohms; }
    // functions 
}
class P_Power
{
    constructor(P_Watts)
    {
        this._field_Watts = P_Watts;
    }
    // field accessors
    static P_Watts_1074 = function(self) { return self._field_Watts; }
    // functions 
}
class P_Density
{
    constructor(P_KilogramsPerMeterCubed)
    {
        this._field_KilogramsPerMeterCubed = P_KilogramsPerMeterCubed;
    }
    // field accessors
    static P_KilogramsPerMeterCubed_1078 = function(self) { return self._field_KilogramsPerMeterCubed; }
    // functions 
}
class P_NormalDistribution
{
    constructor(P_Mean, P_StandardDeviation)
    {
        this._field_Mean = P_Mean;
        this._field_StandardDeviation = P_StandardDeviation;
    }
    // field accessors
    static P_Mean_1082 = function(self) { return self._field_Mean; }
    static P_StandardDeviation_1086 = function(self) { return self._field_StandardDeviation; }
    // functions 
}
class P_PoissonDistribution
{
    constructor(P_Expected, P_Occurrences)
    {
        this._field_Expected = P_Expected;
        this._field_Occurrences = P_Occurrences;
    }
    // field accessors
    static P_Expected_1090 = function(self) { return self._field_Expected; }
    static P_Occurrences_1094 = function(self) { return self._field_Occurrences; }
    // functions 
}
class P_BernoulliDistribution
{
    constructor(P_P)
    {
        this._field_P = P_P;
    }
    // field accessors
    static P_P_1098 = function(self) { return self._field_P; }
    // functions 
}
class P_Probability
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value_1102 = function(self) { return self._field_Value; }
    // functions 
}
class P_BinomialDistribution
{
    constructor(P_Trials, P_P)
    {
        this._field_Trials = P_Trials;
        this._field_P = P_P;
    }
    // field accessors
    static P_Trials_1106 = function(self) { return self._field_Trials; }
    static P_P_1110 = function(self) { return self._field_P; }
    // functions 
}
class P_Interval
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Size_1490 = function (P_x) 
    // ParameterSymbol=x$1489: Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1), Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1)
    // Candidates = Interval,Comparable
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    // Called function: Ref=>FunctionGroupSymbol=Max$1297: candidates = Interval_23.Max_207, TimeRange_87.Max_750, Comparable_136.Max_1296
    // Called function: Ref=>FunctionGroupSymbol=Min$1295: candidates = Interval_23.Min_205, TimeRange_87.Min_746, Comparable_136.Min_1294
    { return P_Subtract(P_Max(P_x), P_Min(P_x)); };
    static P_IsEmpty_1492 = function (P_x) 
    // ParameterSymbol=x$1491: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1)
    // Candidates = Interval,Comparable
    // Called function: Ref=>FunctionGroupSymbol=GreaterThanOrEquals$1293: candidates = Comparable_136.GreaterThanOrEquals_1292
    // Called function: Ref=>FunctionGroupSymbol=Min$1295: candidates = Interval_23.Min_205, TimeRange_87.Min_746, Comparable_136.Min_1294
    // Called function: Ref=>FunctionGroupSymbol=Max$1297: candidates = Interval_23.Max_207, TimeRange_87.Max_750, Comparable_136.Max_1296
    { return P_GreaterThanOrEquals(P_Min(P_x), P_Max(P_x)); };
    static P_Lerp_1495 = function (P_x, P_amount) 
    // ParameterSymbol=x$1493: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=amount$1494: Argument:Ref=>FunctionGroupSymbol=Subtract$171:(1/2), Argument:Ref=>FunctionGroupSymbol=Multiply$173:(1/2)
    // Candidates = ScalarArithmetic,Arithmetic
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Min$1295: candidates = Interval_23.Min_205, TimeRange_87.Min_746, Comparable_136.Min_1294
    // Called function: Ref=>FunctionGroupSymbol=Add$169: candidates = Arithmetic_19.Add_155, ScalarArithmetic_20.Add_168
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Max$1297: candidates = Interval_23.Max_207, TimeRange_87.Max_750, Comparable_136.Max_1296
    { return P_Multiply(P_Min(P_x), P_Add(P_Subtract(1, P_amount), P_Multiply(P_Max(P_x), P_amount))); };
    static P_InverseLerp_1498 = function (P_x, P_value) 
    // ParameterSymbol=x$1496: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Size$1115:(0/1)
    // Candidates = Interval,Comparable,Interval
    // ParameterSymbol=value$1497: Argument:Ref=>FunctionGroupSymbol=Subtract$171:(0/2)
    // Candidates = ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    // Called function: Ref=>FunctionGroupSymbol=Min$1295: candidates = Interval_23.Min_205, TimeRange_87.Min_746, Comparable_136.Min_1294
    // Called function: Ref=>FunctionGroupSymbol=Size$1115: candidates = Rectangle2D_79.Size_710, Interval_133.Size_1114
    { return P_Divide(P_Subtract(P_value, P_Min(P_x)), P_Size(P_x)); };
    static P_Negate_1500 = function (P_x) 
    // ParameterSymbol=x$1499: Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1), Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1)
    // Candidates = Interval,Comparable
    // Called function: Ref=>PredefinedSymbol=Tuple$1: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=Negative$158: candidates = Arithmetic_19.Negative_157
    // Called function: Ref=>FunctionGroupSymbol=Max$1297: candidates = Interval_23.Max_207, TimeRange_87.Max_750, Comparable_136.Max_1296
    // Called function: Ref=>FunctionGroupSymbol=Negative$158: candidates = Arithmetic_19.Negative_157
    // Called function: Ref=>FunctionGroupSymbol=Min$1295: candidates = Interval_23.Min_205, TimeRange_87.Min_746, Comparable_136.Min_1294
    { return P_Tuple(P_Negative(P_Max(P_x)), P_Negative(P_Min(P_x))); };
    static P_Reverse_1502 = function (P_x) 
    // ParameterSymbol=x$1501: Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1), Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1)
    // Candidates = Interval,Comparable
    // Called function: Ref=>PredefinedSymbol=Tuple$1: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=Max$1297: candidates = Interval_23.Max_207, TimeRange_87.Max_750, Comparable_136.Max_1296
    // Called function: Ref=>FunctionGroupSymbol=Min$1295: candidates = Interval_23.Min_205, TimeRange_87.Min_746, Comparable_136.Min_1294
    { return P_Tuple(P_Max(P_x), P_Min(P_x)); };
    static P_Resize_1505 = function (P_x, P_size) 
    // ParameterSymbol=x$1503: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=size$1504: Argument:Ref=>FunctionGroupSymbol=Add$169:(1/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>PredefinedSymbol=Tuple$1: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=Min$1295: candidates = Interval_23.Min_205, TimeRange_87.Min_746, Comparable_136.Min_1294
    // Called function: Ref=>FunctionGroupSymbol=Add$169: candidates = Arithmetic_19.Add_155, ScalarArithmetic_20.Add_168
    // Called function: Ref=>FunctionGroupSymbol=Min$1295: candidates = Interval_23.Min_205, TimeRange_87.Min_746, Comparable_136.Min_1294
    { return P_Tuple(P_Min(P_x), P_Add(P_Min(P_x), P_size)); };
    static P_Center_1507 = function (P_x) 
    // ParameterSymbol=x$1506: Argument:Ref=>FunctionGroupSymbol=Lerp$1119:(0/2)
    // Candidates = Interval
    // Called function: Ref=>FunctionGroupSymbol=Lerp$1119: candidates = Interval_133.Lerp_1118
    { return P_Lerp(P_x, 0.5); };
    static P_Contains_1510 = function (P_x, P_value) 
    // ParameterSymbol=x$1508: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=value$1509: Argument:Ref=>FunctionGroupSymbol=And$179:(0/2), Argument:Ref=>FunctionGroupSymbol=LessThanOrEquals$1289:(0/2)
    // Candidates = Boolean,Comparable
    // Called function: Ref=>FunctionGroupSymbol=LessThanOrEquals$1289: candidates = Comparable_136.LessThanOrEquals_1288
    // Called function: Ref=>FunctionGroupSymbol=Min$1295: candidates = Interval_23.Min_205, TimeRange_87.Min_746, Comparable_136.Min_1294
    // Called function: Ref=>FunctionGroupSymbol=And$179: candidates = Boolean_21.And_178
    // Called function: Ref=>FunctionGroupSymbol=LessThanOrEquals$1289: candidates = Comparable_136.LessThanOrEquals_1288
    // Called function: Ref=>FunctionGroupSymbol=Max$1297: candidates = Interval_23.Max_207, TimeRange_87.Max_750, Comparable_136.Max_1296
    { return P_LessThanOrEquals(P_Min(P_x), P_And(P_value, P_LessThanOrEquals(P_value, P_Max(P_x)))); };
    static P_Contains_1513 = function (P_x, P_other) 
    // ParameterSymbol=x$1511: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=other$1512: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1)
    // Candidates = Interval,Comparable
    // Called function: Ref=>FunctionGroupSymbol=LessThanOrEquals$1289: candidates = Comparable_136.LessThanOrEquals_1288
    // Called function: Ref=>FunctionGroupSymbol=Min$1295: candidates = Interval_23.Min_205, TimeRange_87.Min_746, Comparable_136.Min_1294
    // Called function: Ref=>FunctionGroupSymbol=And$179: candidates = Boolean_21.And_178
    // Called function: Ref=>FunctionGroupSymbol=Min$1295: candidates = Interval_23.Min_205, TimeRange_87.Min_746, Comparable_136.Min_1294
    // Called function: Ref=>FunctionGroupSymbol=GreaterThanOrEquals$1293: candidates = Comparable_136.GreaterThanOrEquals_1292
    // Called function: Ref=>FunctionGroupSymbol=Max$1297: candidates = Interval_23.Max_207, TimeRange_87.Max_750, Comparable_136.Max_1296
    { return P_LessThanOrEquals(P_Min(P_x), P_And(P_Min(P_other), P_GreaterThanOrEquals(P_Max, P_Max(P_other)))); };
    static P_Overlaps_1516 = function (P_x, P_y) 
    // ParameterSymbol=x$1514: Argument:Ref=>FunctionGroupSymbol=Clamp$1215:(0/2)
    // Candidates = Interval,Numerical
    // ParameterSymbol=y$1515: Argument:Ref=>FunctionGroupSymbol=Clamp$1215:(1/2)
    // Candidates = Interval,Numerical
    // Called function: Ref=>FunctionGroupSymbol=Not$183: candidates = Boolean_21.Not_182
    // Called function: Ref=>FunctionGroupSymbol=IsEmpty$1323: candidates = Interval_133.IsEmpty_1116, Array_139.IsEmpty_1322
    // Called function: Ref=>FunctionGroupSymbol=Clamp$1215: candidates = Interval_133.Clamp_1154, Interval_133.Clamp_1156, Numerical_135.Clamp_1210, Numerical_135.Clamp_1212, Numerical_135.Clamp_1214
    { return P_Not(P_IsEmpty(P_Clamp(P_x, P_y))); };
    static P_Split_1519 = function (P_x, P_t) 
    // ParameterSymbol=x$1517: Argument:Ref=>FunctionGroupSymbol=Left$1141:(0/2), Argument:Ref=>FunctionGroupSymbol=Right$1143:(0/2)
    // Candidates = Interval
    // ParameterSymbol=t$1518: Argument:Ref=>FunctionGroupSymbol=Left$1141:(1/2), Argument:Ref=>FunctionGroupSymbol=Right$1143:(1/2)
    // Candidates = Interval
    // Called function: Ref=>PredefinedSymbol=Tuple$1: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=Left$1141: candidates = Interval_133.Left_1140
    // Called function: Ref=>FunctionGroupSymbol=Right$1143: candidates = Interval_133.Right_1142
    { return P_Tuple(P_Left(P_x, P_t), P_Right(P_x, P_t)); };
    static P_Split_1521 = function (P_x) 
    // ParameterSymbol=x$1520: Argument:Ref=>FunctionGroupSymbol=Split$1139:(0/2)
    // Candidates = Interval
    // Called function: Ref=>FunctionGroupSymbol=Split$1139: candidates = Interval_133.Split_1136, Interval_133.Split_1138
    { return P_Split(P_x, 0.5); };
    static P_Left_1524 = function (P_x, P_t) 
    // ParameterSymbol=x$1522: Argument:Ref=>FunctionGroupSymbol=Lerp$1119:(0/2)
    // Candidates = Interval
    // ParameterSymbol=t$1523: Argument:Ref=>FunctionGroupSymbol=Lerp$1119:(1/2)
    // Candidates = Interval
    // Called function: Ref=>PredefinedSymbol=Tuple$1: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=Lerp$1119: candidates = Interval_133.Lerp_1118
    { return P_Tuple(P_Min, P_Lerp(P_x, P_t)); };
    static P_Right_1527 = function (P_x, P_t) 
    // ParameterSymbol=x$1525: Argument:Ref=>FunctionGroupSymbol=Lerp$1119:(0/2), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1)
    // Candidates = Interval,Interval,Comparable
    // ParameterSymbol=t$1526: Argument:Ref=>FunctionGroupSymbol=Lerp$1119:(1/2)
    // Candidates = Interval
    // Called function: Ref=>PredefinedSymbol=Tuple$1: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=Lerp$1119: candidates = Interval_133.Lerp_1118
    // Called function: Ref=>FunctionGroupSymbol=Max$1297: candidates = Interval_23.Max_207, TimeRange_87.Max_750, Comparable_136.Max_1296
    { return P_Tuple(P_Lerp(P_x, P_t), P_Max(P_x)); };
    static P_MoveTo_1530 = function (P_x, P_t) 
    // ParameterSymbol=x$1528: Argument:Ref=>FunctionGroupSymbol=Size$1115:(0/1)
    // Candidates = Interval
    // ParameterSymbol=t$1529: Argument:Ref=>PredefinedSymbol=Tuple$1:(0/2), Argument:Ref=>FunctionGroupSymbol=Add$169:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>PredefinedSymbol=Tuple$1: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=Add$169: candidates = Arithmetic_19.Add_155, ScalarArithmetic_20.Add_168
    // Called function: Ref=>FunctionGroupSymbol=Size$1115: candidates = Rectangle2D_79.Size_710, Interval_133.Size_1114
    { return P_Tuple(P_t, P_Add(P_t, P_Size(P_x))); };
    static P_LeftHalf_1532 = function (P_x) 
    // ParameterSymbol=x$1531: Argument:Ref=>FunctionGroupSymbol=Left$1141:(0/2)
    // Candidates = Interval
    // Called function: Ref=>FunctionGroupSymbol=Left$1141: candidates = Interval_133.Left_1140
    { return P_Left(P_x, 0.5); };
    static P_RightHalf_1534 = function (P_x) 
    // ParameterSymbol=x$1533: Argument:Ref=>FunctionGroupSymbol=Right$1143:(0/2)
    // Candidates = Interval
    // Called function: Ref=>FunctionGroupSymbol=Right$1143: candidates = Interval_133.Right_1142
    { return P_Right(P_x, 0.5); };
    static P_HalfSize_1536 = function (P_x) 
    // ParameterSymbol=x$1535: Argument:Ref=>FunctionGroupSymbol=Size$1115:(0/1)
    // Candidates = Interval
    // Called function: Ref=>FunctionGroupSymbol=Half$1227: candidates = Numerical_135.Half_1226
    // Called function: Ref=>FunctionGroupSymbol=Size$1115: candidates = Rectangle2D_79.Size_710, Interval_133.Size_1114
    { return P_Half(P_Size(P_x)); };
    static P_Recenter_1539 = function (P_x, P_c) 
    // ParameterSymbol=x$1537: Argument:Ref=>FunctionGroupSymbol=HalfSize$1151:(0/1), Argument:Ref=>FunctionGroupSymbol=HalfSize$1151:(0/1)
    // Candidates = Interval
    // ParameterSymbol=c$1538: Argument:Ref=>FunctionGroupSymbol=Subtract$171:(0/2), Argument:Ref=>FunctionGroupSymbol=Add$169:(0/2)
    // Candidates = ScalarArithmetic,Arithmetic
    // Called function: Ref=>PredefinedSymbol=Tuple$1: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    // Called function: Ref=>FunctionGroupSymbol=HalfSize$1151: candidates = Interval_133.HalfSize_1150
    // Called function: Ref=>FunctionGroupSymbol=Add$169: candidates = Arithmetic_19.Add_155, ScalarArithmetic_20.Add_168
    // Called function: Ref=>FunctionGroupSymbol=HalfSize$1151: candidates = Interval_133.HalfSize_1150
    { return P_Tuple(P_Subtract(P_c, P_HalfSize(P_x)), P_Add(P_c, P_HalfSize(P_x))); };
    static P_Clamp_1542 = function (P_x, P_y) 
    // ParameterSymbol=x$1540: Argument:Ref=>FunctionGroupSymbol=Clamp$1215:(0/2), Argument:Ref=>FunctionGroupSymbol=Clamp$1215:(0/2)
    // Candidates = Interval,Numerical
    // ParameterSymbol=y$1541: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1)
    // Candidates = Interval,Comparable
    // Called function: Ref=>PredefinedSymbol=Tuple$1: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=Clamp$1215: candidates = Interval_133.Clamp_1154, Interval_133.Clamp_1156, Numerical_135.Clamp_1210, Numerical_135.Clamp_1212, Numerical_135.Clamp_1214
    // Called function: Ref=>FunctionGroupSymbol=Min$1295: candidates = Interval_23.Min_205, TimeRange_87.Min_746, Comparable_136.Min_1294
    // Called function: Ref=>FunctionGroupSymbol=Clamp$1215: candidates = Interval_133.Clamp_1154, Interval_133.Clamp_1156, Numerical_135.Clamp_1210, Numerical_135.Clamp_1212, Numerical_135.Clamp_1214
    // Called function: Ref=>FunctionGroupSymbol=Max$1297: candidates = Interval_23.Max_207, TimeRange_87.Max_750, Comparable_136.Max_1296
    { return P_Tuple(P_Clamp(P_x, P_Min(P_y)), P_Clamp(P_x, P_Max(P_y))); };
    static P_Clamp_1545 = function (P_x, P_value) 
    // ParameterSymbol=x$1543: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=value$1544: Argument:Ref=>FunctionGroupSymbol=LessThan$1283:(0/2), Argument:Ref=>FunctionGroupSymbol=GreaterThan$1291:(0/2)
    // Candidates = Comparable
    // Called function: Ref=>FunctionGroupSymbol=LessThan$1283: candidates = Comparable_136.LessThan_1282
    // Called function: Ref=>FunctionGroupSymbol=Min$1295: candidates = Interval_23.Min_205, TimeRange_87.Min_746, Comparable_136.Min_1294
    // Called function: Ref=>FunctionGroupSymbol=Min$1295: candidates = Interval_23.Min_205, TimeRange_87.Min_746, Comparable_136.Min_1294
    // Called function: Ref=>FunctionGroupSymbol=GreaterThan$1291: candidates = Comparable_136.GreaterThan_1290
    // Called function: Ref=>FunctionGroupSymbol=Max$1297: candidates = Interval_23.Max_207, TimeRange_87.Max_750, Comparable_136.Max_1296
    // Called function: Ref=>FunctionGroupSymbol=Max$1297: candidates = Interval_23.Max_207, TimeRange_87.Max_750, Comparable_136.Max_1296
    { return P_LessThan(P_value, P_Min(P_x)
        ? P_Min(P_x)
        : P_GreaterThan(P_value, P_Max(P_x)
            ? P_Max(P_x)
            : P_value
        )
    ); };
    static P_Between_1548 = function (P_x, P_value) 
    // ParameterSymbol=x$1546: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=value$1547: Argument:Ref=>FunctionGroupSymbol=GreaterThanOrEquals$1293:(0/2), Argument:Ref=>FunctionGroupSymbol=LessThanOrEquals$1289:(0/2)
    // Candidates = Comparable
    // Called function: Ref=>FunctionGroupSymbol=GreaterThanOrEquals$1293: candidates = Comparable_136.GreaterThanOrEquals_1292
    // Called function: Ref=>FunctionGroupSymbol=And$179: candidates = Boolean_21.And_178
    // Called function: Ref=>FunctionGroupSymbol=Min$1295: candidates = Interval_23.Min_205, TimeRange_87.Min_746, Comparable_136.Min_1294
    // Called function: Ref=>FunctionGroupSymbol=LessThanOrEquals$1289: candidates = Comparable_136.LessThanOrEquals_1288
    // Called function: Ref=>FunctionGroupSymbol=Max$1297: candidates = Interval_23.Max_207, TimeRange_87.Max_750, Comparable_136.Max_1296
    { return P_GreaterThanOrEquals(P_value, P_And(P_Min(P_x), P_LessThanOrEquals(P_value, P_Max(P_x)))); };
    static P_Unit_1549 = function () 
    // Called function: Ref=>PredefinedSymbol=Tuple$1: candidates = 
    { return P_Tuple(0, 1); };
}
class P_Vector
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Sum_1551 = function (P_v) 
    // ParameterSymbol=v$1550: Argument:Ref=>FunctionGroupSymbol=Aggregate$1319:(0/3)
    // Candidates = Array
    // Called function: Ref=>FunctionGroupSymbol=Aggregate$1319: candidates = Array_139.Aggregate_1318
    { return P_Aggregate(P_v, 0, P_Add); };
    static P_SumSquares_1553 = function (P_v) 
    // ParameterSymbol=v$1552: Argument:Ref=>FunctionGroupSymbol=Square$1209:(0/1)
    // Candidates = Numerical
    // Called function: Ref=>FunctionGroupSymbol=Aggregate$1319: candidates = Array_139.Aggregate_1318
    // Called function: Ref=>FunctionGroupSymbol=Square$1209: candidates = Numerical_135.Square_1208
    { return P_Aggregate(P_Square(P_v), 0, P_Add); };
    static P_LengthSquared_1555 = function (P_v) 
    // ParameterSymbol=v$1554: Argument:Ref=>FunctionGroupSymbol=SumSquares$1165:(0/1)
    // Candidates = Vector
    // Called function: Ref=>FunctionGroupSymbol=SumSquares$1165: candidates = Vector_134.SumSquares_1164
    { return P_SumSquares(P_v); };
    static P_Length_1557 = function (P_v) 
    // ParameterSymbol=v$1556: Argument:Ref=>FunctionGroupSymbol=LengthSquared$1167:(0/1)
    // Candidates = Vector
    // Called function: Ref=>FunctionGroupSymbol=SquareRoot$1205: candidates = Numerical_135.SquareRoot_1204
    // Called function: Ref=>FunctionGroupSymbol=LengthSquared$1167: candidates = Vector_134.LengthSquared_1166
    { return P_SquareRoot(P_LengthSquared(P_v)); };
    static P_Dot_1560 = function (P_v1, P_v2) 
    // ParameterSymbol=v1$1558: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // ParameterSymbol=v2$1559: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(1/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Sum$1163: candidates = Vector_134.Sum_1162
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    { return P_Sum(P_Multiply(P_v1, P_v2)); };
}
class P_Numerical
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Cos_1562 = function (P_x) 
    // ParameterSymbol=x$1561: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_Sin_1564 = function (P_x) 
    // ParameterSymbol=x$1563: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_Tan_1566 = function (P_x) 
    // ParameterSymbol=x$1565: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_Acos_1568 = function (P_x) 
    // ParameterSymbol=x$1567: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_Asin_1570 = function (P_x) 
    // ParameterSymbol=x$1569: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_Atan_1572 = function (P_x) 
    // ParameterSymbol=x$1571: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_Cosh_1574 = function (P_x) 
    // ParameterSymbol=x$1573: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_Sinh_1576 = function (P_x) 
    // ParameterSymbol=x$1575: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_Tanh_1578 = function (P_x) 
    // ParameterSymbol=x$1577: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_Acosh_1580 = function (P_x) 
    // ParameterSymbol=x$1579: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_Asinh_1582 = function (P_x) 
    // ParameterSymbol=x$1581: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_Atanh_1584 = function (P_x) 
    // ParameterSymbol=x$1583: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_Pow_1587 = function (P_x, P_y) 
    // ParameterSymbol=x$1585: 
    // Candidates = Any
    // ParameterSymbol=y$1586: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_Log_1590 = function (P_x, P_y) 
    // ParameterSymbol=x$1588: 
    // Candidates = Any
    // ParameterSymbol=y$1589: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_NaturalLog_1592 = function (P_x) 
    // ParameterSymbol=x$1591: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_NaturalPower_1594 = function (P_x) 
    // ParameterSymbol=x$1593: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_SquareRoot_1596 = function (P_x) 
    // ParameterSymbol=x$1595: Argument:Ref=>FunctionGroupSymbol=Pow$1197:(0/2)
    // Candidates = Numerical
    // Called function: Ref=>FunctionGroupSymbol=Pow$1197: candidates = Numerical_135.Pow_1196
    { return P_Pow(P_x, 0.5); };
    static P_CubeRoot_1598 = function (P_x) 
    // ParameterSymbol=x$1597: Argument:Ref=>FunctionGroupSymbol=Pow$1197:(0/2)
    // Candidates = Numerical
    // Called function: Ref=>FunctionGroupSymbol=Pow$1197: candidates = Numerical_135.Pow_1196
    { return P_Pow(P_x, 0.5); };
    static P_Square_1600 = function (P_x) 
    // ParameterSymbol=x$1599: 
    // Candidates = Any
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    { return P_Multiply(P_Value, P_Value); };
    static P_Clamp_1604 = function (P_x, P_min, P_max) 
    // ParameterSymbol=x$1601: Argument:Ref=>FunctionGroupSymbol=Clamp$1215:(0/2)
    // Candidates = Interval,Numerical
    // ParameterSymbol=min$1602: Argument:Ref=>TypeDefSymbol=Interval$133:(0/2)
    // Candidates = Any
    // ParameterSymbol=max$1603: Argument:Ref=>TypeDefSymbol=Interval$133:(1/2)
    // Candidates = Any
    // Called function: Ref=>FunctionGroupSymbol=Clamp$1215: candidates = Interval_133.Clamp_1154, Interval_133.Clamp_1156, Numerical_135.Clamp_1210, Numerical_135.Clamp_1212, Numerical_135.Clamp_1214
    // Called function: Ref=>TypeDefSymbol=Interval$133: candidates = 
    { return P_Clamp(P_x, P_Interval(P_min, P_max)); };
    static P_Clamp_1607 = function (P_x, P_i) 
    // ParameterSymbol=x$1605: Argument:Ref=>FunctionGroupSymbol=Clamp$1215:(1/2)
    // Candidates = Interval,Numerical
    // ParameterSymbol=i$1606: Argument:Ref=>FunctionGroupSymbol=Clamp$1215:(0/2)
    // Candidates = Interval,Numerical
    // Called function: Ref=>FunctionGroupSymbol=Clamp$1215: candidates = Interval_133.Clamp_1154, Interval_133.Clamp_1156, Numerical_135.Clamp_1210, Numerical_135.Clamp_1212, Numerical_135.Clamp_1214
    { return P_Clamp(P_i, P_x); };
    static P_Clamp_1609 = function (P_x) 
    // ParameterSymbol=x$1608: Argument:Ref=>FunctionGroupSymbol=Clamp$1215:(0/3)
    // Candidates = Interval,Numerical
    // Called function: Ref=>FunctionGroupSymbol=Clamp$1215: candidates = Interval_133.Clamp_1154, Interval_133.Clamp_1156, Numerical_135.Clamp_1210, Numerical_135.Clamp_1212, Numerical_135.Clamp_1214
    { return P_Clamp(P_x, 0, 1); };
    static P_PlusOne_1611 = function (P_x) 
    // ParameterSymbol=x$1610: Argument:Ref=>FunctionGroupSymbol=Add$169:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Add$169: candidates = Arithmetic_19.Add_155, ScalarArithmetic_20.Add_168
    { return P_Add(P_x, 1); };
    static P_MinusOne_1613 = function (P_x) 
    // ParameterSymbol=x$1612: Argument:Ref=>FunctionGroupSymbol=Subtract$171:(0/2)
    // Candidates = ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    { return P_Subtract(P_x, 1); };
    static P_FromOne_1615 = function (P_x) 
    // ParameterSymbol=x$1614: Argument:Ref=>FunctionGroupSymbol=Subtract$171:(1/2)
    // Candidates = ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    { return P_Subtract(1, P_x); };
    static P_Sign_1617 = function (P_x) 
    // ParameterSymbol=x$1616: Argument:Ref=>FunctionGroupSymbol=LessThan$1283:(0/2), Argument:Ref=>FunctionGroupSymbol=GreaterThan$1291:(0/2)
    // Candidates = Comparable
    // Called function: Ref=>FunctionGroupSymbol=LessThan$1283: candidates = Comparable_136.LessThan_1282
    // Called function: Ref=>FunctionGroupSymbol=Negative$158: candidates = Arithmetic_19.Negative_157
    // Called function: Ref=>FunctionGroupSymbol=GreaterThan$1291: candidates = Comparable_136.GreaterThan_1290
    { return P_LessThan(P_x, 0
        ? P_Negative(1)
        : P_GreaterThan(P_x, 0
            ? 1
            : 0
        )
    ); };
    static P_Abs_1619 = function (P_x) 
    // ParameterSymbol=x$1618: 
    // Candidates = Any
    // Called function: Ref=>FunctionGroupSymbol=LessThan$1283: candidates = Comparable_136.LessThan_1282
    // Called function: Ref=>FunctionGroupSymbol=Negative$158: candidates = Arithmetic_19.Negative_157
    { return P_LessThan(P_Value, 0
        ? P_Negative(P_Value)
        : P_Value
    ); };
    static P_Half_1621 = function (P_x) 
    // ParameterSymbol=x$1620: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    { return P_Divide(P_x, 2); };
    static P_Third_1623 = function (P_x) 
    // ParameterSymbol=x$1622: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    { return P_Divide(P_x, 3); };
    static P_Quarter_1625 = function (P_x) 
    // ParameterSymbol=x$1624: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    { return P_Divide(P_x, 4); };
    static P_Fifth_1627 = function (P_x) 
    // ParameterSymbol=x$1626: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    { return P_Divide(P_x, 5); };
    static P_Sixth_1629 = function (P_x) 
    // ParameterSymbol=x$1628: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    { return P_Divide(P_x, 6); };
    static P_Seventh_1631 = function (P_x) 
    // ParameterSymbol=x$1630: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    { return P_Divide(P_x, 7); };
    static P_Eighth_1633 = function (P_x) 
    // ParameterSymbol=x$1632: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    { return P_Divide(P_x, 8); };
    static P_Ninth_1635 = function (P_x) 
    // ParameterSymbol=x$1634: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    { return P_Divide(P_x, 9); };
    static P_Tenth_1637 = function (P_x) 
    // ParameterSymbol=x$1636: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    { return P_Divide(P_x, 10); };
    static P_Sixteenth_1639 = function (P_x) 
    // ParameterSymbol=x$1638: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    { return P_Divide(P_x, 16); };
    static P_Hundredth_1641 = function (P_x) 
    // ParameterSymbol=x$1640: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    { return P_Divide(P_x, 100); };
    static P_Thousandth_1643 = function (P_x) 
    // ParameterSymbol=x$1642: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    { return P_Divide(P_x, 1000); };
    static P_Millionth_1645 = function (P_x) 
    // ParameterSymbol=x$1644: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    { return P_Divide(P_x, P_Divide(1000, 1000)); };
    static P_Billionth_1647 = function (P_x) 
    // ParameterSymbol=x$1646: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    { return P_Divide(P_x, P_Divide(1000, P_Divide(1000, 1000))); };
    static P_Hundred_1649 = function (P_x) 
    // ParameterSymbol=x$1648: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    { return P_Multiply(P_x, 100); };
    static P_Thousand_1651 = function (P_x) 
    // ParameterSymbol=x$1650: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    { return P_Multiply(P_x, 1000); };
    static P_Million_1653 = function (P_x) 
    // ParameterSymbol=x$1652: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    { return P_Multiply(P_x, P_Multiply(1000, 1000)); };
    static P_Billion_1655 = function (P_x) 
    // ParameterSymbol=x$1654: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    { return P_Multiply(P_x, P_Multiply(1000, P_Multiply(1000, 1000))); };
    static P_Twice_1657 = function (P_x) 
    // ParameterSymbol=x$1656: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    { return P_Multiply(P_x, 2); };
    static P_Thrice_1659 = function (P_x) 
    // ParameterSymbol=x$1658: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    { return P_Multiply(P_x, 3); };
    static P_SmoothStep_1661 = function (P_x) 
    // ParameterSymbol=x$1660: Argument:Ref=>FunctionGroupSymbol=Square$1209:(0/1), Argument:Ref=>FunctionGroupSymbol=Twice$1263:(0/1)
    // Candidates = Numerical
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Square$1209: candidates = Numerical_135.Square_1208
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    // Called function: Ref=>FunctionGroupSymbol=Twice$1263: candidates = Numerical_135.Twice_1262
    { return P_Multiply(P_Square(P_x), P_Subtract(3, P_Twice(P_x))); };
    static P_Pow2_1663 = function (P_x) 
    // ParameterSymbol=x$1662: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2), Argument:Ref=>FunctionGroupSymbol=Multiply$173:(1/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    { return P_Multiply(P_x, P_x); };
    static P_Pow3_1665 = function (P_x) 
    // ParameterSymbol=x$1664: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(1/2), Argument:Ref=>FunctionGroupSymbol=Pow2$1269:(0/1)
    // Candidates = Arithmetic,ScalarArithmetic,Numerical
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Pow2$1269: candidates = Numerical_135.Pow2_1268
    { return P_Multiply(P_Pow2(P_x), P_x); };
    static P_Pow4_1667 = function (P_x) 
    // ParameterSymbol=x$1666: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(1/2), Argument:Ref=>FunctionGroupSymbol=Pow3$1271:(0/1)
    // Candidates = Arithmetic,ScalarArithmetic,Numerical
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Pow3$1271: candidates = Numerical_135.Pow3_1270
    { return P_Multiply(P_Pow3(P_x), P_x); };
    static P_Pow5_1669 = function (P_x) 
    // ParameterSymbol=x$1668: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(1/2), Argument:Ref=>FunctionGroupSymbol=Pow4$1273:(0/1)
    // Candidates = Arithmetic,ScalarArithmetic,Numerical
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Pow4$1273: candidates = Numerical_135.Pow4_1272
    { return P_Multiply(P_Pow4(P_x), P_x); };
    static P_Turns_1671 = function (P_x) 
    // ParameterSymbol=x$1670: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    { return P_Multiply(P_x, P_Multiply(3.1415926535897, 2)); };
    static P_AlmostZero_1673 = function (P_x) 
    // ParameterSymbol=x$1672: Argument:Ref=>FunctionGroupSymbol=Abs$1225:(0/1)
    // Candidates = Numerical
    // Called function: Ref=>FunctionGroupSymbol=LessThan$1283: candidates = Comparable_136.LessThan_1282
    // Called function: Ref=>FunctionGroupSymbol=Abs$1225: candidates = Numerical_135.Abs_1224
    { return P_LessThan(P_Abs(P_x), 1E-08); };
}
class P_Comparable
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Equals_1676 = function (P_a, P_b) 
    // ParameterSymbol=a$1674: Argument:Ref=>FunctionGroupSymbol=Compare$152:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1675: Argument:Ref=>FunctionGroupSymbol=Compare$152:(1/2)
    // Candidates = Comparable
    // Called function: Ref=>FunctionGroupSymbol=Compare$152: candidates = Comparable_17.Compare_151
    { return P_Compare(P_a, P_b); };
    static P_LessThan_1679 = function (P_a, P_b) 
    // ParameterSymbol=a$1677: Argument:Ref=>FunctionGroupSymbol=Compare$152:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1678: Argument:Ref=>FunctionGroupSymbol=Compare$152:(1/2)
    // Candidates = Comparable
    // Called function: Ref=>FunctionGroupSymbol=LessThan$1283: candidates = Comparable_136.LessThan_1282
    // Called function: Ref=>FunctionGroupSymbol=Compare$152: candidates = Comparable_17.Compare_151
    { return P_LessThan(P_Compare(P_a, P_b), 0); };
    static P_Lesser_1682 = function (P_a, P_b) 
    // ParameterSymbol=a$1680: Argument:Ref=>FunctionGroupSymbol=LessThanOrEquals$1289:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1681: Argument:Ref=>FunctionGroupSymbol=LessThanOrEquals$1289:(1/2)
    // Candidates = Comparable
    // Called function: Ref=>FunctionGroupSymbol=LessThanOrEquals$1289: candidates = Comparable_136.LessThanOrEquals_1288
    { return P_LessThanOrEquals(P_a, P_b)
        ? P_a
        : P_b
    ; };
    static P_Greater_1685 = function (P_a, P_b) 
    // ParameterSymbol=a$1683: Argument:Ref=>FunctionGroupSymbol=GreaterThanOrEquals$1293:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1684: Argument:Ref=>FunctionGroupSymbol=GreaterThanOrEquals$1293:(1/2)
    // Candidates = Comparable
    // Called function: Ref=>FunctionGroupSymbol=GreaterThanOrEquals$1293: candidates = Comparable_136.GreaterThanOrEquals_1292
    { return P_GreaterThanOrEquals(P_a, P_b)
        ? P_a
        : P_b
    ; };
    static P_LessThanOrEquals_1688 = function (P_a, P_b) 
    // ParameterSymbol=a$1686: Argument:Ref=>FunctionGroupSymbol=Compare$152:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1687: Argument:Ref=>FunctionGroupSymbol=Compare$152:(1/2)
    // Candidates = Comparable
    // Called function: Ref=>FunctionGroupSymbol=LessThanOrEquals$1289: candidates = Comparable_136.LessThanOrEquals_1288
    // Called function: Ref=>FunctionGroupSymbol=Compare$152: candidates = Comparable_17.Compare_151
    { return P_LessThanOrEquals(P_Compare(P_a, P_b), 0); };
    static P_GreaterThan_1691 = function (P_a, P_b) 
    // ParameterSymbol=a$1689: Argument:Ref=>FunctionGroupSymbol=Compare$152:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1690: Argument:Ref=>FunctionGroupSymbol=Compare$152:(1/2)
    // Candidates = Comparable
    // Called function: Ref=>FunctionGroupSymbol=GreaterThan$1291: candidates = Comparable_136.GreaterThan_1290
    // Called function: Ref=>FunctionGroupSymbol=Compare$152: candidates = Comparable_17.Compare_151
    { return P_GreaterThan(P_Compare(P_a, P_b), 0); };
    static P_GreaterThanOrEquals_1694 = function (P_a, P_b) 
    // ParameterSymbol=a$1692: Argument:Ref=>FunctionGroupSymbol=Compare$152:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1693: Argument:Ref=>FunctionGroupSymbol=Compare$152:(1/2)
    // Candidates = Comparable
    // Called function: Ref=>FunctionGroupSymbol=GreaterThanOrEquals$1293: candidates = Comparable_136.GreaterThanOrEquals_1292
    // Called function: Ref=>FunctionGroupSymbol=Compare$152: candidates = Comparable_17.Compare_151
    { return P_GreaterThanOrEquals(P_Compare(P_a, P_b), 0); };
    static P_Min_1697 = function (P_a, P_b) 
    // ParameterSymbol=a$1695: Argument:Ref=>FunctionGroupSymbol=LessThan$1283:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1696: Argument:Ref=>FunctionGroupSymbol=LessThan$1283:(1/2)
    // Candidates = Comparable
    // Called function: Ref=>FunctionGroupSymbol=LessThan$1283: candidates = Comparable_136.LessThan_1282
    { return P_LessThan(P_a, P_b)
        ? P_a
        : P_b
    ; };
    static P_Max_1700 = function (P_a, P_b) 
    // ParameterSymbol=a$1698: Argument:Ref=>FunctionGroupSymbol=GreaterThan$1291:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1699: Argument:Ref=>FunctionGroupSymbol=GreaterThan$1291:(1/2)
    // Candidates = Comparable
    // Called function: Ref=>FunctionGroupSymbol=GreaterThan$1291: candidates = Comparable_136.GreaterThan_1290
    { return P_GreaterThan(P_a, P_b)
        ? P_a
        : P_b
    ; };
    static P_Between_1704 = function (P_v, P_a, P_b) 
    // ParameterSymbol=v$1701: Argument:Ref=>FunctionGroupSymbol=Between$1301:(0/2)
    // Candidates = Interval,Comparable
    // ParameterSymbol=a$1702: Argument:Ref=>TypeDefSymbol=Interval$133:(0/2)
    // Candidates = Any
    // ParameterSymbol=b$1703: Argument:Ref=>TypeDefSymbol=Interval$133:(1/2)
    // Candidates = Any
    // Called function: Ref=>FunctionGroupSymbol=Between$1301: candidates = Interval_133.Between_1158, Comparable_136.Between_1298, Comparable_136.Between_1300
    // Called function: Ref=>TypeDefSymbol=Interval$133: candidates = 
    { return P_Between(P_v, P_Interval(P_a, P_b)); };
    static P_Between_1707 = function (P_v, P_i) 
    // ParameterSymbol=v$1705: Argument:Ref=>FunctionGroupSymbol=Contains$1133:(1/2)
    // Candidates = Interval
    // ParameterSymbol=i$1706: Argument:Ref=>FunctionGroupSymbol=Contains$1133:(0/2)
    // Candidates = Interval
    // Called function: Ref=>FunctionGroupSymbol=Contains$1133: candidates = Interval_133.Contains_1130, Interval_133.Contains_1132
    { return P_Contains(P_i, P_v); };
}
class P_Boolean
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_XOr_1710 = function (P_a, P_b) 
    // ParameterSymbol=a$1708: 
    // Candidates = Any
    // ParameterSymbol=b$1709: Argument:Ref=>FunctionGroupSymbol=Not$183:(0/1)
    // Candidates = Boolean
    // Called function: Ref=>FunctionGroupSymbol=Not$183: candidates = Boolean_21.Not_182
    { return P_a
        ? P_Not(P_b)
        : P_b
    ; };
    static P_NAnd_1713 = function (P_a, P_b) 
    // ParameterSymbol=a$1711: Argument:Ref=>FunctionGroupSymbol=And$179:(0/2)
    // Candidates = Boolean
    // ParameterSymbol=b$1712: Argument:Ref=>FunctionGroupSymbol=And$179:(1/2)
    // Candidates = Boolean
    // Called function: Ref=>FunctionGroupSymbol=Not$183: candidates = Boolean_21.Not_182
    // Called function: Ref=>FunctionGroupSymbol=And$179: candidates = Boolean_21.And_178
    { return P_Not(P_And(P_a, P_b)); };
    static P_NOr_1716 = function (P_a, P_b) 
    // ParameterSymbol=a$1714: Argument:Ref=>FunctionGroupSymbol=Or$181:(0/2)
    // Candidates = Boolean
    // ParameterSymbol=b$1715: Argument:Ref=>FunctionGroupSymbol=Or$181:(1/2)
    // Candidates = Boolean
    // Called function: Ref=>FunctionGroupSymbol=Not$183: candidates = Boolean_21.Not_182
    // Called function: Ref=>FunctionGroupSymbol=Or$181: candidates = Boolean_21.Or_180
    { return P_Not(P_Or(P_a, P_b)); };
}
class P_Equatable
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_NotEquals_1718 = function (P_x) 
    // ParameterSymbol=x$1717: Argument:Ref=>FunctionGroupSymbol=Equals$1281:(0/1)
    // Candidates = Equatable,Comparable
    // Called function: Ref=>FunctionGroupSymbol=Not$183: candidates = Boolean_21.Not_182
    // Called function: Ref=>FunctionGroupSymbol=Equals$1281: candidates = Equatable_18.Equals_153, Comparable_136.Equals_1280
    { return P_Not(P_Equals(P_x)); };
}
class P_Array
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Map_1723 = function (P_xs, P_f) 
    // ParameterSymbol=xs$1719: Argument:Ref=>TypeDefSymbol=Count$26:(0/1), Argument:Ref=>FunctionGroupSymbol=At$213:(0/2)
    // Candidates = Vector,Array
    // ParameterSymbol=f$1720: Invoked:(ArgumentSymbol)
    // Candidates = Function
    // Called function: Ref=>FunctionGroupSymbol=Map$1311: candidates = Array_139.Map_1310
    // Called function: Ref=>TypeDefSymbol=Count$26: candidates = Vector_13.Count_143, Array_24.Count_210
    // Called function: Ref=>ParameterSymbol=f$1720: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=At$213: candidates = Vector_13.At_145, Array_24.At_212
    { return P_Map(P_Count(P_xs), function (P_i) 
    // ParameterSymbol=i$1721: Argument:Ref=>FunctionGroupSymbol=At$213:(1/2)
    // Candidates = Vector,Array
    // Called function: Ref=>ParameterSymbol=f$1720: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=At$213: candidates = Vector_13.At_145, Array_24.At_212
    { return P_f(P_At(P_xs, P_i)); }); };
    static P_Zip_1729 = function (P_xs, P_ys, P_f) 
    // ParameterSymbol=xs$1724: Argument:Ref=>TypeDefSymbol=Count$26:(0/1)
    // Candidates = Vector,Array
    // ParameterSymbol=ys$1725: Argument:Ref=>FunctionGroupSymbol=At$213:(0/2)
    // Candidates = Vector,Array
    // ParameterSymbol=f$1726: Invoked:(ArgumentSymbol,ArgumentSymbol)
    // Candidates = Function
    // Called function: Ref=>TypeDefSymbol=Array$139: candidates = 
    // Called function: Ref=>TypeDefSymbol=Count$26: candidates = Vector_13.Count_143, Array_24.Count_210
    // Called function: Ref=>ParameterSymbol=f$1726: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=At$213: candidates = Vector_13.At_145, Array_24.At_212
    // Called function: Ref=>FunctionGroupSymbol=At$213: candidates = Vector_13.At_145, Array_24.At_212
    { return P_Array(P_Count(P_xs), function (P_i) 
    // ParameterSymbol=i$1727: Argument:Ref=>FunctionGroupSymbol=At$213:(0/1), Argument:Ref=>FunctionGroupSymbol=At$213:(1/2)
    // Candidates = Vector,Array
    // Called function: Ref=>ParameterSymbol=f$1726: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=At$213: candidates = Vector_13.At_145, Array_24.At_212
    // Called function: Ref=>FunctionGroupSymbol=At$213: candidates = Vector_13.At_145, Array_24.At_212
    { return P_f(P_At(P_i), P_At(P_ys, P_i)); }); };
    static P_Skip_1734 = function (P_xs, P_n) 
    // ParameterSymbol=xs$1730: 
    // Candidates = Any
    // ParameterSymbol=n$1731: Argument:Ref=>FunctionGroupSymbol=Subtract$171:(1/2), Argument:Ref=>FunctionGroupSymbol=Subtract$171:(1/2)
    // Candidates = ScalarArithmetic
    // Called function: Ref=>TypeDefSymbol=Array$139: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    // Called function: Ref=>FunctionGroupSymbol=At$213: candidates = Vector_13.At_145, Array_24.At_212
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    { return P_Array(P_Subtract(P_Count, P_n), function (P_i) 
    // ParameterSymbol=i$1732: Argument:Ref=>FunctionGroupSymbol=Subtract$171:(0/2)
    // Candidates = ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=At$213: candidates = Vector_13.At_145, Array_24.At_212
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    { return P_At(P_Subtract(P_i, P_n)); }); };
    static P_Take_1739 = function (P_xs, P_n) 
    // ParameterSymbol=xs$1735: 
    // Candidates = Any
    // ParameterSymbol=n$1736: Argument:Ref=>TypeDefSymbol=Array$139:(0/2)
    // Candidates = Any
    // Called function: Ref=>TypeDefSymbol=Array$139: candidates = 
    { return P_Array(P_n, function (P_i) 
    // ParameterSymbol=i$1737: 
    // Candidates = Any
    { return P_At; }); };
    static P_Aggregate_1743 = function (P_xs, P_init, P_f) 
    // ParameterSymbol=xs$1740: Argument:Ref=>FunctionGroupSymbol=IsEmpty$1323:(0/1), Argument:Ref=>FunctionGroupSymbol=Rest$1321:(0/1)
    // Candidates = Interval,Array
    // ParameterSymbol=init$1741: Argument:Ref=>ParameterSymbol=f$1742:(0/2)
    // Candidates = Any
    // ParameterSymbol=f$1742: Invoked:(ArgumentSymbol,ArgumentSymbol), Invoked:(ArgumentSymbol)
    // Candidates = Function
    // Called function: Ref=>FunctionGroupSymbol=IsEmpty$1323: candidates = Interval_133.IsEmpty_1116, Array_139.IsEmpty_1322
    // Called function: Ref=>ParameterSymbol=f$1742: candidates = 
    // Called function: Ref=>ParameterSymbol=f$1742: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=Rest$1321: candidates = Array_139.Rest_1320
    { return P_IsEmpty(P_xs)
        ? P_init
        : P_f(P_init, P_f(P_Rest(P_xs)))
    ; };
    static P_Rest_1745 = function (P_xs) 
    // ParameterSymbol=xs$1744: 
    // Candidates = Any
    // Called function: Ref=>FunctionGroupSymbol=Skip$1315: candidates = Array_139.Skip_1314
    { return P_Skip(1); };
    static P_IsEmpty_1747 = function (P_xs) 
    // ParameterSymbol=xs$1746: Argument:Ref=>TypeDefSymbol=Count$26:(0/1)
    // Candidates = Vector,Array
    // Called function: Ref=>FunctionGroupSymbol=Equals$1281: candidates = Equatable_18.Equals_153, Comparable_136.Equals_1280
    // Called function: Ref=>TypeDefSymbol=Count$26: candidates = Vector_13.Count_143, Array_24.Count_210
    { return P_Equals(P_Count(P_xs), 0); };
    static P_First_1749 = function (P_xs) 
    // ParameterSymbol=xs$1748: Argument:Ref=>FunctionGroupSymbol=At$213:(0/2)
    // Candidates = Vector,Array
    // Called function: Ref=>FunctionGroupSymbol=At$213: candidates = Vector_13.At_145, Array_24.At_212
    { return P_At(P_xs, 0); };
    static P_Last_1751 = function (P_xs) 
    // ParameterSymbol=xs$1750: Argument:Ref=>FunctionGroupSymbol=At$213:(0/2), Argument:Ref=>TypeDefSymbol=Count$26:(0/1)
    // Candidates = Vector,Array
    // Called function: Ref=>FunctionGroupSymbol=At$213: candidates = Vector_13.At_145, Array_24.At_212
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    // Called function: Ref=>TypeDefSymbol=Count$26: candidates = Vector_13.Count_143, Array_24.Count_210
    { return P_At(P_xs, P_Subtract(P_Count(P_xs), 1)); };
    static P_Slice_1755 = function (P_xs, P_from, P_count) 
    // ParameterSymbol=xs$1752: Argument:Ref=>FunctionGroupSymbol=Skip$1315:(0/2)
    // Candidates = Array
    // ParameterSymbol=from$1753: Argument:Ref=>FunctionGroupSymbol=Skip$1315:(1/2)
    // Candidates = Array
    // ParameterSymbol=count$1754: Argument:Ref=>FunctionGroupSymbol=Take$1317:(1/2)
    // Candidates = Array
    // Called function: Ref=>FunctionGroupSymbol=Take$1317: candidates = Array_139.Take_1316
    // Called function: Ref=>FunctionGroupSymbol=Skip$1315: candidates = Array_139.Skip_1314
    { return P_Take(P_Skip(P_xs, P_from), P_count); };
    static P_Join_1758 = function (P_xs, P_sep) 
    // ParameterSymbol=xs$1756: Argument:Ref=>FunctionGroupSymbol=IsEmpty$1323:(0/1), Argument:Ref=>FunctionGroupSymbol=First$1325:(0/1), Argument:Ref=>FunctionGroupSymbol=Skip$1315:(0/2)
    // Candidates = Interval,Array
    // ParameterSymbol=sep$1757: 
    // Candidates = Any
    // Called function: Ref=>FunctionGroupSymbol=IsEmpty$1323: candidates = Interval_133.IsEmpty_1116, Array_139.IsEmpty_1322
    // Called function: Ref=>FunctionGroupSymbol=Add$169: candidates = Arithmetic_19.Add_155, ScalarArithmetic_20.Add_168
    // Called function: Ref=>FunctionGroupSymbol=ToString$203: candidates = Value_22.ToString_202
    // Called function: Ref=>FunctionGroupSymbol=First$1325: candidates = Array_139.First_1324
    // Called function: Ref=>FunctionGroupSymbol=Aggregate$1319: candidates = Array_139.Aggregate_1318
    // Called function: Ref=>FunctionGroupSymbol=Skip$1315: candidates = Array_139.Skip_1314
    { return P_IsEmpty(P_xs)
        ? ""
        : P_Add(P_ToString(P_First(P_xs)), P_Aggregate(P_Skip(P_xs, 1)))
    ; };
    static P_All_1761 = function (P_xs, P_f) 
    // ParameterSymbol=xs$1759: Argument:Ref=>FunctionGroupSymbol=IsEmpty$1323:(0/1), Argument:Ref=>FunctionGroupSymbol=First$1325:(0/1), Argument:Ref=>FunctionGroupSymbol=Rest$1321:(0/1)
    // Candidates = Interval,Array
    // ParameterSymbol=f$1760: Invoked:(ArgumentSymbol), Invoked:(ArgumentSymbol)
    // Candidates = Function
    // Called function: Ref=>FunctionGroupSymbol=IsEmpty$1323: candidates = Interval_133.IsEmpty_1116, Array_139.IsEmpty_1322
    // Called function: Ref=>FunctionGroupSymbol=And$179: candidates = Boolean_21.And_178
    // Called function: Ref=>ParameterSymbol=f$1760: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=First$1325: candidates = Array_139.First_1324
    // Called function: Ref=>ParameterSymbol=f$1760: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=Rest$1321: candidates = Array_139.Rest_1320
    { return P_IsEmpty(P_xs)
        ? True
        : P_And(P_f(P_First(P_xs)), P_f(P_Rest(P_xs)))
    ; };
    static P_JoinStrings_1764 = function (P_xs, P_sep) 
    // ParameterSymbol=xs$1762: Argument:Ref=>FunctionGroupSymbol=IsEmpty$1323:(0/1), Argument:Ref=>FunctionGroupSymbol=First$1325:(0/1), Argument:Ref=>FunctionGroupSymbol=Rest$1321:(0/1)
    // Candidates = Interval,Array
    // ParameterSymbol=sep$1763: 
    // Candidates = Any
    // Called function: Ref=>FunctionGroupSymbol=IsEmpty$1323: candidates = Interval_133.IsEmpty_1116, Array_139.IsEmpty_1322
    // Called function: Ref=>FunctionGroupSymbol=Add$169: candidates = Arithmetic_19.Add_155, ScalarArithmetic_20.Add_168
    // Called function: Ref=>FunctionGroupSymbol=First$1325: candidates = Array_139.First_1324
    // Called function: Ref=>FunctionGroupSymbol=Aggregate$1319: candidates = Array_139.Aggregate_1318
    // Called function: Ref=>FunctionGroupSymbol=Rest$1321: candidates = Array_139.Rest_1320
    { return P_IsEmpty(P_xs)
        ? ""
        : P_Add(P_First(P_xs), P_Aggregate(P_Rest(P_xs)))
    ; };
}
class P_Easings
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_BlendEaseFunc_1768 = function (P_p, P_easeIn, P_easeOut) 
    // ParameterSymbol=p$1765: Argument:Ref=>FunctionGroupSymbol=LessThan$1283:(0/2), Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2), Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Comparable,Arithmetic,ScalarArithmetic
    // ParameterSymbol=easeIn$1766: Invoked:(ArgumentSymbol)
    // Candidates = Function
    // ParameterSymbol=easeOut$1767: Invoked:(ArgumentSymbol)
    // Candidates = Function
    // Called function: Ref=>FunctionGroupSymbol=LessThan$1283: candidates = Comparable_136.LessThan_1282
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>ParameterSymbol=easeIn$1766: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Add$169: candidates = Arithmetic_19.Add_155, ScalarArithmetic_20.Add_168
    // Called function: Ref=>ParameterSymbol=easeOut$1767: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    { return P_LessThan(P_p, 0.5
        ? P_Multiply(0.5, P_easeIn(P_Multiply(P_p, 2)))
        : P_Multiply(0.5, P_Add(P_easeOut(P_Multiply(P_p, P_Subtract(2, 1))), 0.5))
    ); };
    static P_InvertEaseFunc_1771 = function (P_p, P_easeIn) 
    // ParameterSymbol=p$1769: Argument:Ref=>FunctionGroupSymbol=Subtract$171:(1/2)
    // Candidates = ScalarArithmetic
    // ParameterSymbol=easeIn$1770: Invoked:(ArgumentSymbol)
    // Candidates = Function
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    // Called function: Ref=>ParameterSymbol=easeIn$1770: candidates = 
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    { return P_Subtract(1, P_easeIn(P_Subtract(1, P_p))); };
    static P_Linear_1773 = function (P_p) 
    // ParameterSymbol=p$1772: 
    // Candidates = Any
    { return P_p; };
    static P_QuadraticEaseIn_1775 = function (P_p) 
    // ParameterSymbol=p$1774: Argument:Ref=>FunctionGroupSymbol=Pow2$1269:(0/1)
    // Candidates = Numerical
    // Called function: Ref=>FunctionGroupSymbol=Pow2$1269: candidates = Numerical_135.Pow2_1268
    { return P_Pow2(P_p); };
    static P_QuadraticEaseOut_1777 = function (P_p) 
    // ParameterSymbol=p$1776: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=InvertEaseFunc$1339: candidates = Easings_140.InvertEaseFunc_1338
    { return P_InvertEaseFunc(P_p, P_QuadraticEaseIn); };
    static P_QuadraticEaseInOut_1779 = function (P_p) 
    // ParameterSymbol=p$1778: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=BlendEaseFunc$1337: candidates = Easings_140.BlendEaseFunc_1336
    { return P_BlendEaseFunc(P_p, P_QuadraticEaseIn, P_QuadraticEaseOut); };
    static P_CubicEaseIn_1781 = function (P_p) 
    // ParameterSymbol=p$1780: Argument:Ref=>FunctionGroupSymbol=Pow3$1271:(0/1)
    // Candidates = Numerical
    // Called function: Ref=>FunctionGroupSymbol=Pow3$1271: candidates = Numerical_135.Pow3_1270
    { return P_Pow3(P_p); };
    static P_CubicEaseOut_1783 = function (P_p) 
    // ParameterSymbol=p$1782: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=InvertEaseFunc$1339: candidates = Easings_140.InvertEaseFunc_1338
    { return P_InvertEaseFunc(P_p, P_CubicEaseIn); };
    static P_CubicEaseInOut_1785 = function (P_p) 
    // ParameterSymbol=p$1784: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=BlendEaseFunc$1337: candidates = Easings_140.BlendEaseFunc_1336
    { return P_BlendEaseFunc(P_p, P_CubicEaseIn, P_CubicEaseOut); };
    static P_QuarticEaseIn_1787 = function (P_p) 
    // ParameterSymbol=p$1786: Argument:Ref=>FunctionGroupSymbol=Pow4$1273:(0/1)
    // Candidates = Numerical
    // Called function: Ref=>FunctionGroupSymbol=Pow4$1273: candidates = Numerical_135.Pow4_1272
    { return P_Pow4(P_p); };
    static P_QuarticEaseOut_1789 = function (P_p) 
    // ParameterSymbol=p$1788: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=InvertEaseFunc$1339: candidates = Easings_140.InvertEaseFunc_1338
    { return P_InvertEaseFunc(P_p, P_QuarticEaseIn); };
    static P_QuarticEaseInOut_1791 = function (P_p) 
    // ParameterSymbol=p$1790: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=BlendEaseFunc$1337: candidates = Easings_140.BlendEaseFunc_1336
    { return P_BlendEaseFunc(P_p, P_QuarticEaseIn, P_QuarticEaseOut); };
    static P_QuinticEaseIn_1793 = function (P_p) 
    // ParameterSymbol=p$1792: Argument:Ref=>FunctionGroupSymbol=Pow5$1275:(0/1)
    // Candidates = Numerical
    // Called function: Ref=>FunctionGroupSymbol=Pow5$1275: candidates = Numerical_135.Pow5_1274
    { return P_Pow5(P_p); };
    static P_QuinticEaseOut_1795 = function (P_p) 
    // ParameterSymbol=p$1794: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=InvertEaseFunc$1339: candidates = Easings_140.InvertEaseFunc_1338
    { return P_InvertEaseFunc(P_p, P_QuinticEaseIn); };
    static P_QuinticEaseInOut_1797 = function (P_p) 
    // ParameterSymbol=p$1796: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=BlendEaseFunc$1337: candidates = Easings_140.BlendEaseFunc_1336
    { return P_BlendEaseFunc(P_p, P_QuinticEaseIn, P_QuinticEaseOut); };
    static P_SineEaseIn_1799 = function (P_p) 
    // ParameterSymbol=p$1798: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=InvertEaseFunc$1339: candidates = Easings_140.InvertEaseFunc_1338
    { return P_InvertEaseFunc(P_p, P_SineEaseOut); };
    static P_SineEaseOut_1801 = function (P_p) 
    // ParameterSymbol=p$1800: Argument:Ref=>FunctionGroupSymbol=Quarter$1231:(0/1)
    // Candidates = Numerical
    // Called function: Ref=>FunctionGroupSymbol=Sin$1175: candidates = Numerical_135.Sin_1174
    // Called function: Ref=>FunctionGroupSymbol=Turns$1277: candidates = Numerical_135.Turns_1276
    // Called function: Ref=>FunctionGroupSymbol=Quarter$1231: candidates = Numerical_135.Quarter_1230
    { return P_Sin(P_Turns(P_Quarter(P_p))); };
    static P_SineEaseInOut_1803 = function (P_p) 
    // ParameterSymbol=p$1802: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=BlendEaseFunc$1337: candidates = Easings_140.BlendEaseFunc_1336
    { return P_BlendEaseFunc(P_p, P_SineEaseIn, P_SineEaseOut); };
    static P_CircularEaseIn_1805 = function (P_p) 
    // ParameterSymbol=p$1804: Argument:Ref=>FunctionGroupSymbol=Pow2$1269:(0/1)
    // Candidates = Numerical
    // Called function: Ref=>FunctionGroupSymbol=FromOne$1221: candidates = Numerical_135.FromOne_1220
    // Called function: Ref=>FunctionGroupSymbol=SquareRoot$1205: candidates = Numerical_135.SquareRoot_1204
    // Called function: Ref=>FunctionGroupSymbol=FromOne$1221: candidates = Numerical_135.FromOne_1220
    // Called function: Ref=>FunctionGroupSymbol=Pow2$1269: candidates = Numerical_135.Pow2_1268
    { return P_FromOne(P_SquareRoot(P_FromOne(P_Pow2(P_p)))); };
    static P_CircularEaseOut_1807 = function (P_p) 
    // ParameterSymbol=p$1806: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=InvertEaseFunc$1339: candidates = Easings_140.InvertEaseFunc_1338
    { return P_InvertEaseFunc(P_p, P_CircularEaseIn); };
    static P_CircularEaseInOut_1809 = function (P_p) 
    // ParameterSymbol=p$1808: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=BlendEaseFunc$1337: candidates = Easings_140.BlendEaseFunc_1336
    { return P_BlendEaseFunc(P_p, P_CircularEaseIn, P_CircularEaseOut); };
    static P_ExponentialEaseIn_1811 = function (P_p) 
    // ParameterSymbol=p$1810: Argument:Ref=>FunctionGroupSymbol=AlmostZero$1279:(0/1), Argument:Ref=>FunctionGroupSymbol=MinusOne$1219:(0/1)
    // Candidates = Numerical
    // Called function: Ref=>FunctionGroupSymbol=AlmostZero$1279: candidates = Numerical_135.AlmostZero_1278
    // Called function: Ref=>FunctionGroupSymbol=Pow$1197: candidates = Numerical_135.Pow_1196
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=MinusOne$1219: candidates = Numerical_135.MinusOne_1218
    { return P_AlmostZero(P_p)
        ? P_p
        : P_Pow(2, P_Multiply(10, P_MinusOne(P_p)))
    ; };
    static P_ExponentialEaseOut_1813 = function (P_p) 
    // ParameterSymbol=p$1812: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=InvertEaseFunc$1339: candidates = Easings_140.InvertEaseFunc_1338
    { return P_InvertEaseFunc(P_p, P_ExponentialEaseIn); };
    static P_ExponentialEaseInOut_1815 = function (P_p) 
    // ParameterSymbol=p$1814: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=BlendEaseFunc$1337: candidates = Easings_140.BlendEaseFunc_1336
    { return P_BlendEaseFunc(P_p, P_ExponentialEaseIn, P_ExponentialEaseOut); };
    static P_ElasticEaseIn_1817 = function (P_p) 
    // ParameterSymbol=p$1816: Argument:Ref=>FunctionGroupSymbol=Quarter$1231:(0/1), Argument:Ref=>FunctionGroupSymbol=MinusOne$1219:(0/1)
    // Candidates = Numerical
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Turns$1277: candidates = Numerical_135.Turns_1276
    // Called function: Ref=>FunctionGroupSymbol=Quarter$1231: candidates = Numerical_135.Quarter_1230
    // Called function: Ref=>FunctionGroupSymbol=Sin$1175: candidates = Numerical_135.Sin_1174
    // Called function: Ref=>FunctionGroupSymbol=Radians$729: candidates = Angle_82.Radians_726
    // Called function: Ref=>FunctionGroupSymbol=Pow$1197: candidates = Numerical_135.Pow_1196
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=MinusOne$1219: candidates = Numerical_135.MinusOne_1218
    { return P_Multiply(13, P_Multiply(P_Turns(P_Quarter(P_p)), P_Sin(P_Radians(P_Pow(2, P_Multiply(10, P_MinusOne(P_p))))))); };
    static P_ElasticEaseOut_1819 = function (P_p) 
    // ParameterSymbol=p$1818: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=InvertEaseFunc$1339: candidates = Easings_140.InvertEaseFunc_1338
    { return P_InvertEaseFunc(P_p, P_ElasticEaseIn); };
    static P_ElasticEaseInOut_1821 = function (P_p) 
    // ParameterSymbol=p$1820: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=BlendEaseFunc$1337: candidates = Easings_140.BlendEaseFunc_1336
    { return P_BlendEaseFunc(P_p, P_ElasticEaseIn, P_ElasticEaseOut); };
    static P_BackEaseIn_1823 = function (P_p) 
    // ParameterSymbol=p$1822: Argument:Ref=>FunctionGroupSymbol=Pow3$1271:(0/1), Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2), Argument:Ref=>FunctionGroupSymbol=Half$1227:(0/1)
    // Candidates = Numerical,Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    // Called function: Ref=>FunctionGroupSymbol=Pow3$1271: candidates = Numerical_135.Pow3_1270
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Sin$1175: candidates = Numerical_135.Sin_1174
    // Called function: Ref=>FunctionGroupSymbol=Turns$1277: candidates = Numerical_135.Turns_1276
    // Called function: Ref=>FunctionGroupSymbol=Half$1227: candidates = Numerical_135.Half_1226
    { return P_Subtract(P_Pow3(P_p), P_Multiply(P_p, P_Sin(P_Turns(P_Half(P_p))))); };
    static P_BackEaseOut_1825 = function (P_p) 
    // ParameterSymbol=p$1824: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=InvertEaseFunc$1339: candidates = Easings_140.InvertEaseFunc_1338
    { return P_InvertEaseFunc(P_p, P_BackEaseIn); };
    static P_BackEaseInOut_1827 = function (P_p) 
    // ParameterSymbol=p$1826: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=BlendEaseFunc$1337: candidates = Easings_140.BlendEaseFunc_1336
    { return P_BlendEaseFunc(P_p, P_BackEaseIn, P_BackEaseOut); };
    static P_BounceEaseIn_1829 = function (P_p) 
    // ParameterSymbol=p$1828: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=InvertEaseFunc$1339: candidates = Easings_140.InvertEaseFunc_1338
    { return P_InvertEaseFunc(P_p, P_BounceEaseOut); };
    static P_BounceEaseOut_1831 = function (P_p) 
    // ParameterSymbol=p$1830: Argument:Ref=>FunctionGroupSymbol=LessThan$1283:(0/2), Argument:Ref=>FunctionGroupSymbol=Pow2$1269:(0/1), Argument:Ref=>FunctionGroupSymbol=LessThan$1283:(0/2), Argument:Ref=>FunctionGroupSymbol=Pow2$1269:(0/1), Argument:Ref=>FunctionGroupSymbol=Add$169:(0/2), Argument:Ref=>FunctionGroupSymbol=LessThan$1283:(0/2), Argument:Ref=>FunctionGroupSymbol=Pow2$1269:(0/1), Argument:Ref=>FunctionGroupSymbol=Add$169:(0/2), Argument:Ref=>FunctionGroupSymbol=Pow2$1269:(0/1), Argument:Ref=>FunctionGroupSymbol=Add$169:(0/2)
    // Candidates = Comparable,Numerical,Arithmetic,ScalarArithmetic
    // Called function: Ref=>FunctionGroupSymbol=LessThan$1283: candidates = Comparable_136.LessThan_1282
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=Pow2$1269: candidates = Numerical_135.Pow2_1268
    // Called function: Ref=>FunctionGroupSymbol=LessThan$1283: candidates = Comparable_136.LessThan_1282
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    // Called function: Ref=>FunctionGroupSymbol=Pow2$1269: candidates = Numerical_135.Pow2_1268
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Add$169: candidates = Arithmetic_19.Add_155, ScalarArithmetic_20.Add_168
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=LessThan$1283: candidates = Comparable_136.LessThan_1282
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    // Called function: Ref=>FunctionGroupSymbol=Pow2$1269: candidates = Numerical_135.Pow2_1268
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Add$169: candidates = Arithmetic_19.Add_155, ScalarArithmetic_20.Add_168
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Subtract$171: candidates = ScalarArithmetic_20.Subtract_170
    // Called function: Ref=>FunctionGroupSymbol=Pow2$1269: candidates = Numerical_135.Pow2_1268
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    // Called function: Ref=>FunctionGroupSymbol=Multiply$173: candidates = Arithmetic_19.Multiply_161, ScalarArithmetic_20.Multiply_172
    // Called function: Ref=>FunctionGroupSymbol=Add$169: candidates = Arithmetic_19.Add_155, ScalarArithmetic_20.Add_168
    // Called function: Ref=>FunctionGroupSymbol=Divide$175: candidates = Arithmetic_19.Divide_163, ScalarArithmetic_20.Divide_174
    { return P_LessThan(P_p, P_Divide(4, 11))
        ? P_Multiply(121, P_Divide(P_Pow2(P_p), 16))
        : P_LessThan(P_p, P_Divide(8, 11))
            ? P_Divide(363, P_Multiply(40, P_Subtract(P_Pow2(P_p), P_Divide(99, P_Multiply(10, P_Add(P_p, P_Divide(17, 5)))))))
            : P_LessThan(P_p, P_Divide(9, 10))
                ? P_Divide(4356, P_Multiply(361, P_Subtract(P_Pow2(P_p), P_Divide(35442, P_Multiply(1805, P_Add(P_p, P_Divide(16061, 1805)))))))
                : P_Divide(54, P_Multiply(5, P_Subtract(P_Pow2(P_p), P_Divide(513, P_Multiply(25, P_Add(P_p, P_Divide(268, 25)))))))


    ; };
    static P_BounceEaseInOut_1833 = function (P_p) 
    // ParameterSymbol=p$1832: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    // Called function: Ref=>FunctionGroupSymbol=BlendEaseFunc$1337: candidates = Easings_140.BlendEaseFunc_1336
    { return P_BlendEaseFunc(P_p, P_BounceEaseIn, P_BounceEaseOut); };
}
class P_Intrinsics
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Interpolate_1835 = function (P_xs) 
    // ParameterSymbol=xs$1834: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_Throw_1837 = function (P_x) 
    // ParameterSymbol=x$1836: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_TypeOf_1839 = function (P_x) 
    // ParameterSymbol=x$1838: 
    // Candidates = Any
    { return P_intrinsic; };
    static P_New_1841 = function (P_x) 
    // ParameterSymbol=x$1840: 
    // Candidates = Any
    { return P_intrinsic; };
}
