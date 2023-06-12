using Godot;
using System;
using System.Collections.Generic;

class CellCollider
{
    float gridDim = 100;
    public int elemCount = 0;
    public int ID = 0;
    public CellCollider next = null;
    public CellCollider prev = null;
    public List<StrokePatchCollider> elems = new List<StrokePatchCollider>();
    public void addStroke(StrokePatchCollider elem)
    {
        elems.Add(elem);
        elemCount++;
    }
    public void clear()
    {
        elems.Clear();
        elemCount = 0;
    }
    public void eraseCollide(CollideObject co)
    {
        float lbound = ID * gridDim;
        float hbound = lbound + gridDim;

        if (co.cx + co.radius < lbound)
        {
            return;
        }
        if (co.cx - co.radius > hbound)
        {
            return;
        }
        if (co.start.X + co.rs < lbound && co.end.X + co.re < lbound)
        {
            return;
        }
        if (co.start.X - co.rs > hbound && co.end.X - co.re > hbound)
        {
            return;
        }
        for (int i = 0; i < elems.Count; i++)
        {
            StrokePatchCollider se = elems[i];
            if (se.ID == -1)
            {
                continue;
            }

            if (co.cx + co.radius < se.pos.X - se.hsize || co.cx - co.radius > se.pos.X + se.hsize)
            {
                continue;
            }
            if (co.cy + co.radius < se.pos.Y - se.hsize || co.cy - co.radius > se.pos.Y + se.hsize)
            {
                continue;
            }

            Vector2 t = se.pos - co.start;

            float prj = t.Dot(co.dir);

            if (prj < -co.rs - se.hsize || prj > co.mag + co.re + se.hsize)
            {
                continue;
            }
            if (prj < 0)
            {
                if (t.Length() < se.hsize + co.rs)
                {
                    co.results.Add(se.ID);

                }
                continue;
            }
            if (prj > co.mag)
            {
                if (co.end.DistanceTo(se.pos) < se.hsize + co.re)
                {
                    co.results.Add(se.ID);

                }
                continue;
            }
            float d = MathF.Sqrt(t.LengthSquared() - prj * prj);

            if (d < se.hsize + co.rs + (co.re - co.rs) * prj / co.mag)
            {
                co.results.Add(se.ID);
                se.ID = -1;
            }

        }

    }
}
