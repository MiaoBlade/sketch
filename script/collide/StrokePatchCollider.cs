using Godot;
using System;

public struct StrokePatchCollider
{
    public int ID = 0;
    public StrokePatchCollider(Vector2 p, float hs, float d)
    {
        pos = p;
        hsize = hs;
        dir = d;

    }
    public Vector2 pos = Vector2.Zero;
    public float dir = 0;
    public float hsize = 0.5f;
}
