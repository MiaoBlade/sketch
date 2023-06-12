using Godot;
using System;
using System.Collections.Generic;

public struct StrokeElement
{
    public int ID = 0;
    public StrokeElement(Vector2 p, float hs, float d)
    {
        pos = p;
        hsize = hs;
        dir = d;

    }
    public Vector2 pos = Vector2.Zero;
    public float dir = 0;
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

    public bool hasMotion = false;//mouse anf pen behave diffrently
    public bool strokeStarted = false;
    public bool strokeStoped = false;

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

    StrokeState strokeState;
    RowCollider entry = new RowCollider();//linklist root,next point to smallest id

    SketchLayer layer;

    float latestSize = 0;
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
        insertStroke(new StrokeElement(p.pos, p.hsize, 0), Transform2D.Identity, color, makeCustom_default(5));
    }
    public void beginStroke(StrokePoint p)
    {
        strokeState.p1 = p;
        strokeState.strokeStarted = true;
        strokeState.strokeStoped = false;
    }
    public void addStroke(StrokePoint p)
    {
        if (!strokeState.strokeStarted) return;
        if (strokeState.strokeStoped) return;
        if (p.pos == strokeState.p1.pos) return;
        var needHandleTail = false;
        if (p.hsize == 0)//consider 0 size as stroke end 
        {
            strokeState.strokeStoped = true;
            needHandleTail = true;
        }
        var p0 = strokeState.p0;
        var p1 = strokeState.p1;
        var p2 = p;
        latestSize = MathF.Max(p.hsize, latestSize);
        strokeState.hasMotion = true;
        var dist_21 = p2.pos.DistanceTo(p1.pos);
        if (p0 != null)
        {
            if (dist_21 < distIgnoreThreshold)
            {
                //too close,discard
                if (needHandleTail)//finish this stroke here
                {
                    var (id, angle) = connectStrokePoint(p0, strokeState.p0_dir, strokeState.p0_id, p1, (p1.pos - p0.pos).Angle());
                    drawStrokeTailPatch(p1.pos, angle, p1.hsize, id);
                }
                return;
            }
            var dist_10 = p1.pos.DistanceTo(p0.pos);
            if (dist_21 < 3 && dist_10 < 3)
            {
                //pa,pb,p2 is very close,we do some stablization here
                p1.pos = p0.pos * 0.25f + p2.pos * 0.25f + p1.pos * 0.5f;
                strokeState.p1 = p1;
            }
            var dir0 = strokeState.p0_dir;
            var (dir1, dir1_b, dir1_f) = interpolateDir(p1.pos - p0.pos, p2.pos - p1.pos);
            var breakCurve = MathF.PI - MathF.Abs(MathF.Abs(dir1_f - dir1_b) - MathF.PI) > MathF.PI * 0.5f;
            if (breakCurve)
            {
                GD.Print("breaking");
            }
            var (pb_id, pb_xangle) = connectStrokePoint(p0, strokeState.p0_dir, strokeState.p0_id, p1, breakCurve ? dir1_b : dir1);

            strokeState.p0_dir = breakCurve ? dir1_f : dir1;
            strokeState.p0_id = pb_id;
            strokeState.p0_xdir = pb_xangle;
        }
        else
        {//this is the second point
            //init for the first point
            strokeState.p0_dir = (p2.pos - p1.pos).Angle();
            strokeState.p0_xdir = strokeState.p0_dir;
            p1.hsize = p2.hsize;//use p2 size
            strokeState.p1 = p1;
            var elem = new StrokeElement(p1.pos, p1.hsize, strokeState.p0_xdir);
            var color = layer.setting.color;
            color.A = 1.0f;//use alpha as elem type
            insertStroke(elem, Transform2D.Identity.RotatedLocal(-strokeState.p0_xdir), color, makeCustom_start_0(p1.hsize));
            color.A = 0.0f;
            strokeState.p0_id = insertStroke(elem, Transform2D.Identity.RotatedLocal(-strokeState.p0_xdir), color, makeCustom_start_1(p1.hsize, p1.hsize));
        }
        strokeState.p0 = strokeState.p1;
        strokeState.p1 = p;
        if (needHandleTail)
        {
            //finish this stroke here
            connectStrokePoint(p0, strokeState.p0_dir, strokeState.p0_id, p1, (p1.pos - p0.pos).Angle());
        }
    }
    public void endStroke(StrokePoint p)
    {
        if (!strokeState.strokeStarted) return;

        if (!strokeState.strokeStoped)
        {
            if (strokeState.p0 == null)
            {
                drawPointStroke(strokeState.p1.pos, strokeState.hasMotion ? latestSize : strokeState.p1.hsize, p.pos);
            }
            else
            {
                var p0 = strokeState.p0;
                var p1 = strokeState.p1;
                var p2 = p;
                var dist_21 = p2.pos.DistanceTo(p1.pos);
                if (dist_21 < distIgnoreThreshold)
                {
                    var (pb_id, pb_xangle) = connectStrokePoint(p0, strokeState.p0_dir, strokeState.p0_id, p1, (p1.pos - p0.pos).Angle());
                    drawStrokeTailPatch(p1.pos, pb_xangle, p1.hsize, pb_id);
                }
                else
                {
                    var dir0 = strokeState.p0_dir;
                    var (dir1, dir1_b, dir1_f) = interpolateDir(p1.pos - p0.pos, p2.pos - p1.pos);
                    var breakCurve = MathF.PI - MathF.Abs(MathF.Abs(dir1_f - dir1_b) - MathF.PI) > MathF.PI * 0.5f;
                    if (breakCurve)
                    {
                        GD.Print("breaking");
                    }
                    var (pb_id, pb_xangle) = connectStrokePoint(p0, strokeState.p0_dir, strokeState.p0_id, p1, breakCurve ? dir1_b : dir1);
                    p2.hsize = Mathf.Max(0.5f, 0.5f + (p1.hsize - 0.5f) * (1 - MathF.Min(1, dist_21 / p1.hsize / 10)));
                    var (p2_id, p2_xangle) = connectStrokePoint(p1, breakCurve ? dir1_f : dir1, pb_id, p2, (p2.pos - p1.pos).Angle());

                    drawStrokeTailPatch(p2.pos, p2_xangle, p2.hsize, p2_id);
                }
            }
        }
        latestSize = 0;
        strokeState.hasMotion = false;
        strokeState.p1 = null;
        strokeState.p0 = null;
        strokeState.strokeCount++;
        strokeState.strokeStarted = false;
        strokeState.strokeStoped = true;
    }
    float packPosition(float x, float y)
    {
        ushort x_short = BitConverter.HalfToUInt16Bits((Half)x);
        ushort y_short = BitConverter.HalfToUInt16Bits((Half)y);

        uint bits = ((uint)y_short << 16) + x_short;

        return BitConverter.UInt32BitsToSingle(bits);
    }
    int insertStroke(StrokeElement elem, Transform2D xform, Color c, float[] custom)
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
        writeCustomDataToBuffer(elemCount, custom);
        elem.ID = elemCount;
        RowCollider rs = findOrInsertRowStore(elem.pos);
        rs.addStroke(elem);
        elemCount++;
        return elem.ID;
    }
    (int pb_id, float pb_xangle) connectStrokePoint(StrokePoint pa, float pa_angle, int pa_id, StrokePoint pb, float pb_angle)
    {
        var dist_10 = pa.pos.DistanceTo(pb.pos);
        int id_pb;
        float xangle_pb = 0;
        if (dist_10 > pa.hsize + pb.hsize)
        {
            //interpolate
            float space = Math.Max(distThreshold, pa.hsize);
            int num_interp = Mathf.CeilToInt((dist_10 - pa.hsize - pb.hsize) / space);

            var c_extend = dist_10 / 4;
            var c0 = pa.pos + c_extend * Vector2.Right.Rotated(pa_angle);
            var c1 = pb.pos - c_extend * Vector2.Right.Rotated(pb_angle);
            var curve = new CubicCurve(pa.pos, c0, c1, pb.pos);
            var curve_pts = new Vector2[num_interp];
            var curve_hsize = new float[num_interp];

            for (int i = 0; i < num_interp; i++)
            {
                var t = (i + 1f) / (num_interp + 1);
                curve_pts[i] = curve.getPoint(t);
                curve_hsize[i] = pa.hsize + (pb.hsize - pa.hsize) * t;
            }
            //fix pa
            fix_p0(curve_pts[0], curve_hsize[0]);

            if (curve_pts.Length > 1)
            {
                appendInterpElement(curve_pts[0], curve_hsize[0], pa.pos, pa.hsize, curve_pts[1], curve_hsize[1]);
                for (int i = 1; i < num_interp - 1; i++)
                {
                    appendInterpElement(curve_pts[i], curve_hsize[i], curve_pts[i - 1], curve_hsize[i - 1], curve_pts[i + 1], curve_hsize[i + 1]);
                }
                appendInterpElement(curve_pts[num_interp - 1], curve_hsize[num_interp - 1], curve_pts[num_interp - 2], curve_hsize[num_interp - 2], pb.pos, pb.hsize);
            }
            else
            {
                appendInterpElement(curve_pts[0], curve_hsize[0], pa.pos, pa.hsize, pb.pos, pb.hsize);
            }
            (id_pb, xangle_pb) = append_p1(pb.pos, pb.hsize, curve_pts[num_interp - 1], curve_hsize[num_interp - 1]);
        }
        else
        {
            fix_p0(pb.pos, pb.hsize);
            (id_pb, xangle_pb) = append_p1(pb.pos, pb.hsize, pa.pos, pa.hsize);
        }
        return (id_pb, xangle_pb);
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
    void applyRightPatch(int id, float hs, float ext)
    {
        var custom = getCustom(id);
        custom[0] = packPosition(ext, -hs);
        custom[2] = packPosition(ext, hs);
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

        var color = layer.setting.color;
        color.A = 0.0f;//use alpha as elem type
        insertStroke(p_elem, Transform2D.Identity.RotatedLocal(-angle_p), color, custom);
    }
    void drawPointStroke(Vector2 p, float hsize, Vector2 next)
    {
        var vec = next - p;
        var mag = vec.Length();
        if (mag < distIgnoreThreshold)
        {
            var elem = new StrokeElement(p, hsize, 0);
            var color = layer.setting.color;
            color.A = 1.0f;//use alpha as elem type
            insertStroke(elem, Transform2D.Identity, color, makeCustom_start_0(hsize));
            insertStroke(elem, Transform2D.Identity, color, makeCustom_end(hsize));
        }
        else
        {
            var angle = vec.Angle();
            var elem = new StrokeElement(p, hsize, angle);
            var color = layer.setting.color;
            color.A = 1.0f;//use alpha as elem type
            insertStroke(elem, Transform2D.Identity.RotatedLocal(-angle), color, makeCustom_start_0(hsize));
            elem.pos = next;
            insertStroke(elem, Transform2D.Identity.RotatedLocal(-angle), color, makeCustom_end(hsize));
            color.A = 0.0f;
            insertStroke(elem, Transform2D.Identity.RotatedLocal(-angle), color, makeCustom_interp(hsize, mag / 2));
        }
    }

    private float[] makeCustom_interp(float hs, float ext)
    {
        var custom = new float[4];
        custom[0] = packPosition(ext, -hs);
        custom[2] = packPosition(ext, hs);
        custom[1] = packPosition(-ext, -hs);
        custom[3] = packPosition(-ext, hs);
        return custom;
    }

    private float[] makeCustom_end(float hs)
    {
        var custom = new float[4];
        custom[0] = packPosition(hs * 0.866f, -hs * 0.5f);
        custom[2] = packPosition(hs * 0.866f, hs * 0.5f);
        custom[1] = packPosition(0, -hs);
        custom[3] = packPosition(0, hs);
        return custom;
    }

    (int, float) append_p1(Vector2 p, float hsize, Vector2 prev, float hsize_p)
    {
        var vec_p = p - prev;
        var angle_p = vec_p.Angle();
        var hsize_p_m = (hsize + hsize_p) / 2;
        var mag_p = vec_p.Length() / 2;

        var custom = new float[4];
        custom[0] = packPosition(0, -hsize);
        custom[2] = packPosition(0, hsize);
        custom[1] = packPosition(-mag_p, -hsize_p_m);
        custom[3] = packPosition(-mag_p, hsize_p_m);

        var elem = new StrokeElement(p, hsize, angle_p);
        var color = layer.setting.color;
        color.A = 1.0f;//use alpha as elem type
        return (insertStroke(elem, Transform2D.Identity.RotatedLocal(-angle_p), color, custom), angle_p);
    }
    void drawStrokeTailPatch(Vector2 p, float r, float s, int id)
    {
        if (s < 0.5) return;
        applyRightPatch(id, s, 0);
        var elem = new StrokeElement(p, s, r);
        var color = layer.setting.color;
        color.A = 1.0f;
        insertStroke(elem, Transform2D.Identity.RotatedLocal(-r), color, makeCustom_end(s));
    }
    float[] makeCustom_default(float hs)
    {
        var custom = new float[4];
        custom[0] = packPosition(hs, -hs);
        custom[2] = packPosition(hs, hs);
        custom[1] = packPosition(-hs, -hs);
        custom[3] = packPosition(-hs, hs);
        return custom;
    }
    float[] makeCustom_start_0(float hs)
    {
        var custom = new float[4];
        custom[0] = packPosition(0, -hs);
        custom[2] = packPosition(0, hs);
        custom[1] = packPosition(-hs * 0.866f, -hs * 0.5f);
        custom[3] = packPosition(-hs * 0.866f, hs * 0.5f);
        return custom;
    }
    float[] makeCustom_start_1(float hs, float ext)
    {
        var custom = new float[4];
        custom[0] = packPosition(ext, -hs);
        custom[2] = packPosition(ext, hs);
        custom[1] = packPosition(0, -hs);
        custom[3] = packPosition(0, hs);
        return custom;
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
