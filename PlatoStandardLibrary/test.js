class P_Vector2D {
    constructor(P_X, P_Y) {
        this._field_X = P_X;
        this._field_Y = P_Y;
    }
    // field accessors
    static P_X = function (self) { return self._field_X; }
    static P_Y = function (self) { return self._field_Y; }
    // functions 
}

let v = new P_Vector2D(3, 4);
console.log('%o', P_Vector2D.P_X(v));