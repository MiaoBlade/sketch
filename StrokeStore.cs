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
        rs = s.hsize;
        re = e.hsize;

        radius = (s.pos - e.pos).Length() / 2 + s.hsize + e.hsize;

        dir = e.pos - s.pos;
        mag = dir.Length();
        dir = dir / mag;

        rmax = Math.Max(rs, re);
    }
}
public struct StrokeElement
{
    public int ID = 0;
    public StrokeElement(Vector2 p, float s, float d)
    {
        pos = p;
        hsize = s / 2;
        dir = d;

    }
    public Vector2 pos = Vector2.Zero;
    public float dir = 0;
    public float size = 1;
    public float hsize = 0.5f;
}
public struct StrokeState
{
    public int strokeCount = 0;
    public bool lastIsFirst = false;
    public StrokeState()
    {

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
            StrokeElement se = elems[i];
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
        CellStore start = entry.next;
        while (start != null)
        {
            start.clear();
            start.prev.next = null;
            start.prev = null;
            start = start.next;

        }
        CellCount = 0;
        elemCount = 0;
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
    static float tau_f = (float)Math.Tau;
    RandomNumberGenerator rnd = new RandomNumberGenerator();
    float gridDim = 100;
    int bufferStride = 12;//8 for xform, 4 for custom
    public int elemCount = 0;
    public int RowCount = 0;
    float distThreshold = 5.0f;
    float distIgnoreThreshold = 5f;
    public int capacity = 8 * 1024;
    public float[] buffer;
    public StrokeElement lastStrokeElement;

    StrokeState strokeState;
    RowStore entry = new RowStore();//linklist root,next point to smallest id
    public StrokeStore()
    {
        buffer = new float[capacity * bufferStride];
    }

    public void addStroke(StrokeElement elem, bool isFirst = false)
    {
        if (isFirst)
        {
            insertStroke(elem, Transform2D.Identity.RotatedLocal(elem.dir));
            strokeState.lastIsFirst = true;
        }
        else
        {
            var dist = elem.pos.DistanceTo(lastStrokeElement.pos);
            if (dist < distIgnoreThreshold)
            {
                //too close,skip
                return;
            }
            Vector2 dir = (elem.pos - lastStrokeElement.pos) / dist;
            Transform2D xform;
            var radianDir = dir.Angle();
            elem.dir = radianDir;
            float[] customData = new float[4];
            float size_half = elem.hsize;
            //fix last stroke connection
            float deltaAngle = radianDir - lastStrokeElement.dir;
            if (strokeState.lastIsFirst)
            {
                xform = Transform2D.Identity.RotatedLocal(-deltaAngle);
                xform.Origin = lastStrokeElement.pos;
                writeXformToBuffer(lastStrokeElement.ID, xform);
                strokeState.lastIsFirst = false;
            }
            else
            {
                var custom = getCustom(lastStrokeElement.ID);
                var vert = new Vector2(lastStrokeElement.hsize, -lastStrokeElement.hsize);
                var vert_r = vert.Rotated(deltaAngle);
                custom[0] = packPosition(vert_r.X, vert_r.Y);
                vert.Y = lastStrokeElement.hsize;
                vert_r = vert.Rotated(deltaAngle);
                custom[2] = packPosition(vert_r.X, vert_r.Y);
            }

            if (dist < lastStrokeElement.hsize + size_half)
            {
                //too close,just add
                elem.dir = radianDir;

                customData[0] = packPosition(size_half, -size_half);
                customData[1] = packPosition(-(dist - lastStrokeElement.hsize), -size_half);
                customData[2] = packPosition(size_half, size_half);
                customData[3] = packPosition(-(dist - lastStrokeElement.hsize), size_half);
                insertStroke(elem, Transform2D.Identity.RotatedLocal(-radianDir), customData);
            }
            else
            {
                StrokeElement interp_base = lastStrokeElement;
                float l_real = dist - elem.hsize - lastStrokeElement.hsize;
                float num_interp = Math.Max(1, Mathf.Floor(l_real / distThreshold));
                float d_interp = l_real / num_interp;

                //interpoation
                var l_start = d_interp / 2 + interp_base.hsize;
                var s_start = interp_base.hsize + (elem.hsize - interp_base.hsize) / num_interp;
                var s_step = (elem.hsize - interp_base.hsize) / num_interp;

                float r_interp = d_interp / 2;

                for (int i = 0; i < num_interp; i++)
                {
                    size_half = s_start + s_step * i;

                    customData[0] = packPosition(r_interp, -size_half);
                    customData[1] = packPosition(-r_interp, -size_half);
                    customData[2] = packPosition(r_interp, size_half);
                    customData[3] = packPosition(-r_interp, size_half);
                    var newElem = new StrokeElement(interp_base.pos + dir * (l_start + d_interp * i), size_half * 2, 0);
                    insertStroke(newElem, Transform2D.Identity.RotatedLocal(-radianDir), customData);
                }

                insertStroke(elem, Transform2D.Identity.RotatedLocal(-radianDir));

            }
        }
    }
    float packPosition(float x, float y)
    {
        ushort x_short = BitConverter.HalfToUInt16Bits((Half)x);
        ushort y_short = BitConverter.HalfToUInt16Bits((Half)y);

        uint bits = ((uint)y_short << 16) + x_short;

        return BitConverter.UInt32BitsToSingle(bits);
    }
    void insertStroke(StrokeElement elem, Transform2D xform, float[] custom = null)
    {
        if ((elemCount + 1) * bufferStride >= buffer.Length)
        {
            //resize
            capacity *= 2;
            Array.Resize<float>(ref buffer, capacity * bufferStride);
        }
        xform.Origin = elem.pos;
        int pos = elemCount * bufferStride;
        writeXformToBuffer(elemCount, xform);

        if (custom != null)
        {
            writeCUstomDataToBuffer(elemCount, custom);
        }
        else
        {
            float size_half = elem.hsize;
            buffer[pos + 8] = packPosition(size_half, -size_half);
            buffer[pos + 9] = packPosition(-size_half, -size_half);
            buffer[pos + 10] = packPosition(size_half, size_half);
            buffer[pos + 11] = packPosition(-size_half, size_half);
        }


        elem.ID = elemCount;
        RowStore rs = findOrInsertRowStore(elem.pos);
        rs.addStroke(elem);
        elemCount++;
        lastStrokeElement = elem;
    }
    void overrideStroke(StrokeElement elem, Transform2D xform, float[] custom = null)
    {
        xform.Origin = elem.pos;
        writeXformToBuffer(elem.ID, xform);

        if (custom != null)
        {
            writeCUstomDataToBuffer(elem.ID, custom);
        }
        else
        {
            int pos = elem.ID * bufferStride;
            float size_half = elem.hsize;
            buffer[pos + 8] = packPosition(size_half, -size_half);
            buffer[pos + 9] = packPosition(-size_half, -size_half);
            buffer[pos + 10] = packPosition(size_half, size_half);
            buffer[pos + 11] = packPosition(-size_half, size_half);
        }
    }
    void writeXformToBuffer(int index, Transform2D xform)
    {
        int pos = index * bufferStride;
        buffer[pos] = xform.X[0];
        buffer[pos + 1] = xform.X[1];
        buffer[pos + 2] = 0;
        buffer[pos + 3] = xform.Origin.X;
        buffer[pos + 4] = xform.Y[0];
        buffer[pos + 5] = xform.Y[1];
        buffer[pos + 6] = 0;
        buffer[pos + 7] = xform.Origin.Y;
    }
    void writeCUstomDataToBuffer(int index, float[] custom)
    {
        int pos = index * bufferStride;
        buffer[pos + 8] = custom[0];
        buffer[pos + 9] = custom[1];
        buffer[pos + 10] = custom[2];
        buffer[pos + 11] = custom[3];
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
    Span<float> getCustom(int index)
    {
        int pos = index * bufferStride;
        return buffer.AsSpan<float>(pos + 8, 4);
    }
}
