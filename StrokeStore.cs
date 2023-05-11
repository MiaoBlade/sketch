using Godot;
using System;
using System.Collections.Generic;

class CollideObject
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

    public CollideObject(StrokeElement s, StrokeElement e)
    {
        start = s.pos;
        end = e.pos;
        cx = (s.pos.X + e.pos.X) / 2;
        cy = (s.pos.Y + e.pos.Y) / 2;
        rs = s.size;
        re = e.size;

        radius = ((s.pos - e.pos).Length() + s.size + e.size) / 2;

        dir = e.pos - s.pos;
        mag = dir.Length();
        dir = dir / mag;

        rmax = Math.Max(rs, re);
    }
}

class CellStore
{
    float gridDim = 100;
    public int elemCount = 0;
    public int ID = 0;
    public CellStore next = null;
    public CellStore prev = null;
    public List<StrokeElement> elems = new List<StrokeElement>();
    public void addStroke(StrokeElement elem)
    {
        elems.Add(elem);
        elemCount++;
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
            StrokeElement se = elems[i];
            if (se.ID == -1)
            {
                continue;
            }

            if (co.cx + co.radius < se.pos.X - se.size || co.cx - co.radius > se.pos.X + se.size)
            {
                continue;
            }
            if (co.cy + co.radius < se.pos.Y - se.size || co.cy - co.radius > se.pos.Y + se.size)
            {
                continue;
            }

            Vector2 t = se.pos - co.start;

            float prj = t.Dot(co.dir);

            if (prj < -co.rs - se.size || prj > co.mag + co.re + se.size)
            {
                continue;
            }
            if (prj < -se.size)
            {
                if (t.Length() < se.size + co.rs)
                {
                    co.results.Add(se.ID);

                }
                continue;
            }
            if (prj > co.mag + se.size)
            {
                if (t.Length() < se.size + co.re)
                {
                    co.results.Add(se.ID);

                }
                continue;
            }
            float d = MathF.Sqrt(t.LengthSquared() - prj * prj);

            if (d < se.size + co.rs + (co.re - co.rs) * prj / co.mag)
            {
                co.results.Add(se.ID);
                se.ID = -1;
            }

        }

    }
}
class RowStore
{
    public int elemCount = 0;
    public int CellCount = 0;
    float gridDim = 100;
    public int ID = 0;
    public RowStore next = null;
    public RowStore prev = null;
    public CellStore entry = new CellStore();
    public void clear()
    {

    }
    public void eraseCollide(CollideObject co)
    {
        float lbound = ID * gridDim;
        float hbound = lbound + gridDim;

        if (co.cy + co.radius < lbound)
        {
            return;
        }
        if (co.cy - co.radius > hbound)
        {
            return;
        }
        if (co.start.Y + co.rs < lbound && co.end.Y + co.re < lbound)
        {
            return;
        }
        if (co.start.Y - co.rs > hbound && co.end.Y - co.re > hbound)
        {
            return;
        }
        CellStore cs = entry.next;
        while (cs != null)
        {
            cs.eraseCollide(co);
            cs = cs.next;

        }
    }
    public void addStroke(StrokeElement elem)
    {
        elemCount++;
        CellStore cs = findOrInsertCellStore(elem.pos);
        cs.addStroke(elem);
    }
    CellStore findOrInsertCellStore(Vector2 pos)
    {
        int id = Mathf.FloorToInt(pos.X / gridDim);
        if (entry.next == null)
        {
            //we dont have any rowstore,just add new one
            entry.next = new CellStore();
            entry.next.prev = entry;
            entry.next.ID = id;
            CellCount++;
            return entry.next;
        }
        CellStore cs = entry.next;
        CellStore newCS;
        while (cs != null)
        {
            if (cs.ID == id)
            {
                // found it
                return cs;
            }
            else if (cs.ID > id)
            {
                //insert here
                newCS = new CellStore();
                newCS.ID = id;
                newCS.prev = cs.prev;
                newCS.next = cs;

                cs.prev.next = newCS;
                cs.prev = newCS;
                CellCount++;
                return newCS;
            }
            else
            {
                //jump next
                if (cs.next == null)
                {
                    break;
                }
                else
                {
                    cs = cs.next;
                }
            }
        }
        //we are the biggest,add
        newCS = new CellStore();
        newCS.ID = id;
        newCS.prev = cs;
        cs.next = newCS;
        CellCount++;
        return newCS;
    }
}
public class StrokeStore
{
    float gridDim = 100;
    int bufferStride = 8;
    public int elemCount = 0;
    public int RowCount = 0;

    public int capacity = 8 * 1024;
    public float[] buffer;
    public StrokeElement lastStrokeElement;

    RowStore entry = new RowStore();//linklist root,next point to smallest id
    public StrokeStore()
    {
        buffer = new float[capacity * bufferStride];
    }

    public void addStroke(StrokeElement elem)
    {
        if ((elemCount + 1) * bufferStride >= buffer.Length)
        {
            //resize
            capacity *= 2;
            Array.Resize<float>(ref buffer, capacity * bufferStride);
        }
        lastStrokeElement = elem;

        float cos_s = MathF.Cos(elem.dir) * elem.size;
        float sin_s = MathF.Sin(elem.dir) * elem.size;

        int pos = elemCount * bufferStride;
        buffer[pos] = cos_s;
        buffer[pos + 1] = -sin_s;
        buffer[pos + 2] = 0;
        buffer[pos + 3] = elem.pos.X;
        buffer[pos + 4] = sin_s;
        buffer[pos + 5] = cos_s;
        buffer[pos + 6] = 0;
        buffer[pos + 7] = elem.pos.Y;

        elem.ID = elemCount;

        RowStore rs = findOrInsertRowStore(elem.pos);
        rs.addStroke(elem);
        elemCount++;
    }

    public void clear()
    {
        RowStore start = entry.next;
        while (start != null)
        {
            start.clear();
            start.prev.next = null;
            start.prev = null;
            start = start.next;

        }
        RowCount = 0;
        elemCount = 0;
    }

    public void eraseCollide(StrokeElement start, StrokeElement end)
    {
        CollideObject co = new CollideObject(start, end);

        RowStore rs = entry.next;
        while (rs != null)
        {
            rs.eraseCollide(co);
            rs = rs.next;

        }

        foreach (var cid in co.results)
        {
            hideElement(cid);

        }
    }
    void hideElement(int id)
    {
        int pos = id * bufferStride;
        buffer[pos] = 0;
        buffer[pos + 1] = 0;
        // buffer[pos + 2] = 0;
        // buffer[pos + 3] = elem.pos.X;
        // buffer[pos + 4] = sin_s;
        // buffer[pos + 5] = cos_s;
        // buffer[pos + 6] = 0;
        // buffer[pos + 7] = elem.pos.Y;
    }
    RowStore findOrInsertRowStore(Vector2 pos)
    {
        int id = Mathf.FloorToInt(pos.Y / gridDim);
        if (entry.next == null)
        {
            //we dont have any rowstore,just add new one
            entry.next = new RowStore();
            entry.next.prev = entry;
            entry.next.ID = id;
            RowCount++;
            return entry.next;
        }
        RowStore rs = entry.next;
        RowStore newRS;
        while (rs != null)
        {
            if (rs.ID == id)
            {
                // found it
                return rs;
            }
            else if (rs.ID > id)
            {
                //insert here
                newRS = new RowStore();
                newRS.ID = id;
                newRS.prev = rs.prev;
                newRS.next = rs;

                rs.prev.next = newRS;
                rs.prev = newRS;

                RowCount++;

                return newRS;
            }
            else
            {
                //jump next
                if (rs.next == null)
                {
                    break;
                }
                else
                {
                    rs = rs.next;
                }
            }
        }
        //we are the biggest,add
        newRS = new RowStore();
        newRS.ID = id;
        newRS.prev = rs;
        rs.next = newRS;
        RowCount++;
        return newRS;
    }
}
