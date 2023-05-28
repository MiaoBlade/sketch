using Godot;
using System;
using System.Collections.Generic;

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
public class StrokePoint
{
    public Vector2 pos;
    public float hsize;

    public StrokePoint(Vector2 p, float r)
    {
        pos = p;
        hsize = r;
    }

}
public struct StrokeState
{
    public int strokeCount = 0;
    public Color color = Color.Color8(100, 50, 50, 255);
    public StrokePoint p0 = null;
    public StrokePoint p1 = null;

    public float p0_dir = float.NaN;
    public int p0_id = 0;//smoothed dir
    public float p0_xdir = 0;//xform dir

    public StrokeState()
    {

    }
}

public class StrokeStore
{
    float gridDim = 100;
    int bufferStride = 16;//8 for xform, 4 for color, 4 for custom
    public int elemCount = 0;
    public int RowCount = 0;
    float distThreshold = 5.0f;
    float distIgnoreThreshold = 1f;
    public int capacity = 8 * 1024;
    public float[] buffer;

    public StrokeState strokeState;
    RowCollider entry = new RowCollider();//linklist root,next point to smallest id

    SketchLayer layer;
    public StrokeStore(SketchLayer l)
    {
        layer = l;
        buffer = new float[capacity * bufferStride];
        strokeState = new StrokeState();
    }

    public void addPureStroke(StrokePoint p)
    {
        var color = layer.setting.Debug_COLOR_KEYPOINT;
        color.A = 1.0f;//use alpha as elem type
        insertStroke(new StrokeElement(p.pos, p.hsize, 0), Transform2D.Identity, color);
    }
    public void addStroke(StrokePoint p)
    {
        StrokeElement elem = new StrokeElement(p.pos, p.hsize, 0);
        var p0 = strokeState.p0;
        var p1 = strokeState.p1;
        var p2 = p;
        if (p0 != null)
        {
            var dist_21 = p2.pos.DistanceTo(p1.pos);
            if (dist_21 < distIgnoreThreshold)
            {
                //too close,discard
                return;
            }
            // p1 dir
            var dir0 = strokeState.p0_dir;

            var (dir1, dir1_b, dir1_f) = interpolateDir(p1.pos - p0.pos, p2.pos - p1.pos);
            var breakCurve = MathF.PI - MathF.Abs(MathF.Abs(dir1_f - dir1_b) - MathF.PI) > MathF.PI * 0.5f;
            if (breakCurve)
            {
                GD.Print("breaking");
            }

            //try interpolate p0 p1
            var dist_10 = p0.pos.DistanceTo(p1.pos);
            int id_p1;
            if (dist_10 > p0.hsize + p1.hsize)
            {
                //interpolate
                float space = Math.Max(distThreshold, p0.hsize);
                int num_interp = Mathf.CeilToInt((dist_10 - p0.hsize - p1.hsize) / space);

                var c_extend = dist_10 / 4;
                var c0 = p0.pos + c_extend * Vector2.Right.Rotated(dir0);
                var c1 = p1.pos - c_extend * Vector2.Right.Rotated(breakCurve ? dir1_b : dir1);
                var curve = new CubicCurve(p0.pos, c0, c1, p1.pos);
                var curve_pts = new Vector2[num_interp];
                var curve_hsize = new float[num_interp];

                for (int i = 0; i < num_interp; i++)
                {
                    var t = (i + 1f) / (num_interp + 1);
                    curve_pts[i] = curve.getPoint(t);
                    curve_hsize[i] = p0.hsize + (p1.hsize - p0.hsize) * t;
                }
                //fix p0
                fix_p0(curve_pts[0], curve_hsize[0]);

                if (curve_pts.Length > 1)
                {
                    appendInterpElement(curve_pts[0], curve_hsize[0], p0.pos, p0.hsize, curve_pts[1], curve_hsize[1]);
                    for (int i = 1; i < num_interp - 1; i++)
                    {
                        appendInterpElement(curve_pts[i], curve_hsize[i], curve_pts[i - 1], curve_hsize[i - 1], curve_pts[i + 1], curve_hsize[i + 1]);
                    }
                    appendInterpElement(curve_pts[num_interp - 1], curve_hsize[num_interp - 1], curve_pts[num_interp - 2], curve_hsize[num_interp - 2], p1.pos, p1.hsize);
                }
                else
                {
                    appendInterpElement(curve_pts[0], curve_hsize[0], p0.pos, p0.hsize, p1.pos, p1.hsize);
                }
                id_p1 = append_p1(p1.pos, p1.hsize, curve_pts[num_interp - 1], curve_hsize[num_interp - 1], p2.pos);
            }
            else
            {
                //line
                fix_p0(p1.pos, p1.hsize);
                id_p1 = append_p1(p1.pos, p1.hsize, p0.pos, p0.hsize, p2.pos);
            }

            strokeState.p0_dir = breakCurve ? dir1_f : dir1;
            strokeState.p0_id = id_p1;
        }
        else
        {
            if (p1 != null)//this is the second point
            {
                //init for the first point
                strokeState.p0_dir = (p.pos - p1.pos).Angle();
                strokeState.p0_xdir = strokeState.p0_dir;

                var p0_elem = new StrokeElement(p1.pos, p1.hsize, strokeState.p0_xdir);
                var color = strokeState.color;
                color.A = 1.0f;//use alpha as elem type
                strokeState.p0_id = insertStroke(p0_elem, Transform2D.Identity.RotatedLocal(-strokeState.p0_xdir), color);

            }
        }
        strokeState.p0 = strokeState.p1;
        strokeState.p1 = p;
    }
    public void endStroke(StrokePoint p)
    {
        addStroke(p);
        fix_stroke_end();
        strokeState.p1 = null;
        strokeState.p0 = null;
        strokeState.strokeCount++;
    }
    float packPosition(float x, float y)
    {
        ushort x_short = BitConverter.HalfToUInt16Bits((Half)x);
        ushort y_short = BitConverter.HalfToUInt16Bits((Half)y);

        uint bits = ((uint)y_short << 16) + x_short;

        return BitConverter.UInt32BitsToSingle(bits);
    }
    int insertStroke(StrokeElement elem, Transform2D xform, Color c, float[] custom = null)
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
        writeColorToBuffer(elemCount, c);

        if (custom != null)
        {
            writeCustomDataToBuffer(elemCount, custom);
        }
        else
        {
            float size_half = elem.hsize;
            buffer[pos + 12] = packPosition(size_half, -size_half);
            buffer[pos + 13] = packPosition(-size_half, -size_half);
            buffer[pos + 14] = packPosition(size_half, size_half);
            buffer[pos + 15] = packPosition(-size_half, size_half);
        }


        elem.ID = elemCount;
        RowCollider rs = findOrInsertRowStore(elem.pos);
        rs.addStroke(elem);
        elemCount++;
        return elem.ID;
    }
    void overrideStroke(StrokeElement elem, Transform2D xform, float[] custom = null)
    {
        xform.Origin = elem.pos;
        writeXformToBuffer(elem.ID, xform);

        if (custom != null)
        {
            writeCustomDataToBuffer(elem.ID, custom);
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
    void writeColorToBuffer(int index, Color color)
    {
        int pos = index * bufferStride;
        buffer[pos + 8] = color.R;
        buffer[pos + 9] = color.G;
        buffer[pos + 10] = color.B;
        buffer[pos + 11] = color.A;
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
    void writeCustomDataToBuffer(int index, float[] custom)
    {
        int pos = index * bufferStride;
        buffer[pos + 12] = custom[0];
        buffer[pos + 13] = custom[1];
        buffer[pos + 14] = custom[2];
        buffer[pos + 15] = custom[3];
    }

    public void clear()
    {
        RowCollider start = entry.next;
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

        RowCollider rs = entry.next;
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
    (float, float, float) interpolateDir(Vector2 back, Vector2 front)
    {
        var a = back.Angle();
        var b = front.Angle();
        var m = (a + b) / 2;
        if (Math.Abs(b - a) > Mathf.Pi)
        {
            m += Mathf.Pi;
        }
        return (m, a, b);
    }
    void fix_p0(Vector2 next, float hsize)
    {
        var custom = getCustom(strokeState.p0_id);
        var vec = next - strokeState.p0.pos;
        var mag = vec.Length() / 2;
        var angle = vec.Angle() - strokeState.p0_xdir;
        var hsize_m = (hsize + strokeState.p0.hsize) / 2;
        var v0 = new Vector2(mag, -hsize_m).Rotated(angle);
        var v2 = new Vector2(mag, hsize_m).Rotated(angle);


        custom[0] = packPosition(v0.X, v0.Y);
        custom[2] = packPosition(v2.X, v2.Y);
    }
    void appendInterpElement(Vector2 p, float hsize, Vector2 prev, float hsize_p, Vector2 next, float hsize_n)
    {
        var vec_p = p - prev;
        var vec_n = next - p;
        var angle_p = vec_p.Angle();
        var angle_n = vec_n.Angle();

        var hsize_p_m = (hsize + hsize_p) / 2;
        var hsize_n_m = (hsize + hsize_n) / 2;

        var mag_p = vec_p.Length() / 2;
        var mag_n = vec_n.Length() / 2;

        var v0 = new Vector2(mag_n, -hsize_n_m).Rotated((angle_n - angle_p));
        var v2 = new Vector2(mag_n, hsize_n_m).Rotated((angle_n - angle_p));

        var custom = new float[4];
        custom[0] = packPosition(v0.X, v0.Y);
        custom[2] = packPosition(v2.X, v2.Y);
        custom[1] = packPosition(-mag_p, -hsize_p_m);
        custom[3] = packPosition(-mag_p, hsize_p_m);

        var p_elem = new StrokeElement(p, hsize, angle_p);

        var color = strokeState.color;
        color.A = 0.0f;//use alpha as elem type
        insertStroke(p_elem, Transform2D.Identity.RotatedLocal(-angle_p), color, custom);
    }

    int append_p1(Vector2 p, float hsize, Vector2 prev, float hsize_p, Vector2 next, bool shrink = false)
    {
        var vec_p = p - prev;
        var vec_n = next - p;
        var angle_p = vec_p.Angle();
        var angle_n = vec_n.Angle();

        var hsize_p_m = (hsize + hsize_p) / 2;

        var mag_p = vec_p.Length() / 2;

        var ext = shrink ? 1 : hsize;

        var v0 = new Vector2(ext, -ext).Rotated((angle_n - angle_p));
        var v2 = new Vector2(ext, ext).Rotated((angle_n - angle_p));

        var custom = new float[4];
        custom[0] = packPosition(v0.X, v0.Y);
        custom[2] = packPosition(v2.X, v2.Y);
        custom[1] = packPosition(-mag_p, -hsize_p_m);
        custom[3] = packPosition(-mag_p, hsize_p_m);

        var p_elem = new StrokeElement(p, hsize, angle_p);


        strokeState.p0_xdir = angle_p;

        var color = strokeState.color;
        color.A = 1.0f;//use alpha as elem type

        return insertStroke(p_elem, Transform2D.Identity.RotatedLocal(-angle_p), color, custom);
    }
    //make line end between p0 p1
    void fix_stroke_end()
    {
        var p0 = strokeState.p0;
        var p1 = strokeState.p1;
        // p1 dir
        var dir0 = strokeState.p0_dir;
        var dir1 = (p1.pos - p0.pos).Angle();
        var tailSize = p1.hsize / 2;
        //try interpolate p0 p1
        var dist_10 = p0.pos.DistanceTo(p1.pos);
        if (dist_10 > p0.hsize + p1.hsize)
        {
            //interpolate
            float space = Math.Max(distThreshold, p0.hsize);
            int num_interp = Mathf.CeilToInt((dist_10 - p0.hsize - p1.hsize) / space);

            var c_extend = dist_10 / 4;
            var c0 = p0.pos + c_extend * Vector2.Right.Rotated(dir0);
            var c1 = p1.pos - c_extend * Vector2.Right.Rotated(dir1);
            var curve = new CubicCurve(p0.pos, c0, c1, p1.pos);
            var curve_pts = new Vector2[num_interp];
            var curve_hsize = new float[num_interp];


            for (int i = 0; i < num_interp; i++)
            {
                var t = (i + 1f) / (num_interp + 1);
                curve_pts[i] = curve.getPoint(t);
                curve_hsize[i] = p0.hsize + (tailSize - p0.hsize) * t;
            }
            //fix p0
            fix_p0(curve_pts[0], curve_hsize[0]);

            if (curve_pts.Length > 1)
            {
                appendInterpElement(curve_pts[0], curve_hsize[0], p0.pos, p0.hsize, curve_pts[1], curve_hsize[1]);
                for (int i = 1; i < num_interp - 1; i++)
                {
                    appendInterpElement(curve_pts[i], curve_hsize[i], curve_pts[i - 1], curve_hsize[i - 1], curve_pts[i + 1], curve_hsize[i + 1]);
                }
                appendInterpElement(curve_pts[num_interp - 1], curve_hsize[num_interp - 1], curve_pts[num_interp - 2], curve_hsize[num_interp - 2], p1.pos, tailSize);
            }
            else
            {
                appendInterpElement(curve_pts[0], curve_hsize[0], p0.pos, p0.hsize, p1.pos, tailSize);
            }
            append_p1(p1.pos, tailSize, curve_pts[num_interp - 1], curve_hsize[num_interp - 1], p1.pos * 2 - p0.pos);
        }
        else
        {
            //line
            fix_p0(p1.pos, tailSize);
            append_p1(p1.pos, tailSize, p0.pos, p0.hsize, p1.pos * 2 - p0.pos);
        }
    }
    void hideElement(int id)
    {
        int pos = id * bufferStride;
        buffer[pos] = 0;
        buffer[pos + 1] = 0;
    }
    RowCollider findOrInsertRowStore(Vector2 pos)
    {
        int id = Mathf.FloorToInt(pos.Y / gridDim);
        if (entry.next == null)
        {
            //we dont have any rowstore,just add new one
            entry.next = new RowCollider();
            entry.next.prev = entry;
            entry.next.ID = id;
            RowCount++;
            return entry.next;
        }
        RowCollider rs = entry.next;
        RowCollider newRS;
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
                newRS = new RowCollider();
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
        newRS = new RowCollider();
        newRS.ID = id;
        newRS.prev = rs;
        rs.next = newRS;
        RowCount++;
        return newRS;
    }
    Span<float> getCustom(int index)
    {
        int pos = index * bufferStride;
        return buffer.AsSpan<float>(pos + 12, 4);
    }
}
