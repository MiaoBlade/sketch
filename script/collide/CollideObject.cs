using Godot;
using System;
using System.Collections.Generic;


public class CollideObject
{
    public Vector2 start;
    public Vector2 end;

    public float radius;
    public float cx;
    public float cy;
    public float rs;
    public float re;
    public float rmax;

    public Vector2 dir;
    public float mag;


    public List<int> results = new List<int>();

    public CollideObject(StrokePoint s, StrokePoint e)
    {
        start = s.pos;
        end = e.pos;
        cx = (s.pos.X + e.pos.X) / 2;
        cy = (s.pos.Y + e.pos.Y) / 2;
        rs = s.hsize;
        re = e.hsize;

        radius = (s.pos - e.pos).Length() / 2 + s.hsize + e.hsize;

        dir = e.pos - s.pos;
        mag = dir.Length();
        dir = dir / mag;

        rmax = Math.Max(rs, re);
    }
}
