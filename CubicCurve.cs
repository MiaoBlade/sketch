using Godot;
using System;

public class CubicCurve
{
    Vector2 a, b, c, d;
    public CubicCurve(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        a = p1;
        b = p2;
        c = p3;
        d = p4;
    }
    public Vector2 getPoint(float t)
    {
        float tc = 1 - t;
        float f1, f2, f3, f4;
        f1 = tc * tc * tc;
        f2 = 3 * tc * tc * t;
        f3 = 3 * tc * t * t;
        f4 = t * t * t;
        return f1 * a + f2 * b + f3 * c + f4 * d;
    }
}
