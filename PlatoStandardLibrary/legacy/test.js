class Library {
    static Vector2D_At(xs, n) { return n == 0 ? xs.X(xs) : xs.Y(xs); }
    static Vector2D_Count(xs) { return 2; }
    static Vector3D_At(xs, n) { return n == 0 ? xs.X(xs) : n == 1 ? xs.Y(xs) : xs.Z(xs) }
    static Vector3D_Count(xs) { return 3; }
}


class Numerical_Concept {
    constructor(self) {
        this.Self = self;
    }

    Add(a, b) {
        var fieldsA = a.FieldValues(a);
        var fieldsB = b.FieldValues(b);
        var r = [];
        for (var i = 0; i < fieldsA.length; ++i) {
            r.push(fieldsA[i] + fieldsB[i]);
        }
        return new this.Self(...r);
    }
}

class Array_Concept {
    constructor(self, count_func, at_func) {
        this.Self = self;
        this.Count = count_func;
        this.At = at_func;
    }
}

class Vector2D
{
    constructor(X, Y) {
        this._field_X = X;
        this._field_Y = Y;

        console.log(this.constructor);

        Vector2D.FieldValues = function (self) { return [self._field_X, self._field_Y]; }
        Vector2D.X = function (self) { return self._field_X; }
        Vector2D.Y = function (self) { return self._field_Y; }
        Vector2D.Add = function (self, other) { return Vector2D.Numerical.Add(self, other); }

        this.FieldValues = Vector2D.FieldValues;
        this.At = Vector2D.Array.At;
        this.Count = Vector2D.Array.Count;
        this.Add = Vector2D.Numerical.Add;
        this.X = Vector2D.X;
        this.Y = Vector2D.Y;
    }

    /*
    static FieldValues(self) { return [self._field_X, self._field_Y]; }
    static X(self) { return self._field_X; }
    static Y(self) { return self._field_Y; }
    static Add(self, other) { return Vector2D.Numerical.Add(self, other); }
    */

    // concepts
    static Numerical = new Numerical_Concept(Vector2D);
    static Array = new Array_Concept(Vector2D, Library.Vector2D_Count, Library.Vector2D_At)
    static Concepts = [Vector2D.Numerical, Vector2D.Array];
}

class Vector3D {
    constructor(X, Y, Z) {
        this._field_X = X;
        this._field_Y = Y;
        this._field_Z = Z;

        this.FieldValues = Vector3D.FieldValues;
        this.At = Vector3D.Array.At;
        this.Count = Vector3D.Array.Count;
        this.Add = Vector3D.Numerical.Add;
        this.X = Vector3D.X;
        this.Y = Vector3D.Y;
        this.Z = Vector3D.Z;
    }

    static FieldValues(self) { return [self._field_X, self._field_Y, self._field_Z]; }
    static X(self) { return self._field_X; }
    static Y(self) { return self._field_Y; }
    static Z(self) { return self._field_Z; }

    // concepts
    static Numerical = new Numerical_Concept(Vector3D);
    static Array = new Array_Concept(Vector3D, Library.Vector3D_Count, Library.Vector3D_At)
    static Concepts = [Vector3D.Numerical, Vector3D.Array];
}

// Concepts.

// An instance of a concept refers to a specific type.
let v1 = new Vector2D(3, 4);
let v2 = new Vector2D(5, 6);
let v3 = Vector2D.Add(v1, v2)
console.log('%o', Vector2D.X(v3));
console.log('%o', Vector2D.Y(v3));