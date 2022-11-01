﻿// MIT License
// Copyright (C) 2019 VIMaec LLC.
// Copyright (C) 2019 Ara 3D. Inc
// https://ara3d.com

using System;

namespace Plato.Math
{
    public partial struct Quad : ITransformable<Quad>, IPoints, IMappable<Quad, Vector3>
    {
        public Quad Transform(Matrix4x4 mat) => Map(x => x.Transform(mat));
        public int NumPoints => 4;
        public Vector3 GetPoint(int n) => n == 0 ? A : n == 1 ? B : n == 2 ? C : D;
        public Quad Map(Func<Vector3, Vector3> f) => new Quad(f(A), f(B), f(C), f(D));
    }
}