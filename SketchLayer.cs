using Godot;
using System;
using System.Collections.Generic;
using Sketchpad;
public class SketchLayer
{
    Vector2 dragStart;
    Vector2 dragBase;//layer pos when drag start
    public SketchSetting setting = new SketchSetting();

    public Vector2 pos = Vector2.Zero;//layer origin in screen coord
    public int scaleLevel = 0;//log2(scale)

    public GridType gtype = GridType.None;
    public float gdim = 50f;

    public StrokeStore store;

    StrokePatchCollider lastErase;
    public List<StrokePoint> points = new List<StrokePoint>();
    public SketchLayer()
    {
        store = new StrokeStore(this);
    }
    public void beginErase(Vector2 vec, float size)
    {
        lastErase = new StrokePatchCollider(toLocal(vec), size / 2, 0);
    }
    public void endErase()
    {
    }
    public void appendErase(Vector2 vec, float size)
    {
        StrokePatchCollider newErase = new StrokePatchCollider(toLocal(vec), size / 2, 0);
        store.eraseCollide(lastErase, newErase);
        lastErase = newErase;
    }
    public void beginStroke(Vector2 vec, float size)
    {
        StrokePoint se = new StrokePoint(toLocal(vec), size / 2);
        store.beginStroke(se);
        points.Add(new StrokePoint(vec, size / 2));
    }
    public void endStroke(Vector2 vec)
    {
        StrokePoint se = new StrokePoint(toLocal(vec), 1);
        store.endStroke(se);
        points.Add(new StrokePoint(vec, 0));

        GD.Print("Points in latest stroke.(screen)");
        points.ForEach(p => { GD.Print($"{p.pos} @ {p.hsize}"); });
        points.Clear();
    }
    public void appendStroke(Vector2 vec, float size)
    {
        StrokePoint se = new StrokePoint(toLocal(vec), size / 2);
        store.addStroke(se);
        points.Add(new StrokePoint(vec, size / 2));
    }
    public void beginDrag(Vector2 vec)
    {
        dragBase = pos;
        dragStart = vec;
    }
    public void endDrag(Vector2 vec)
    {
        updateDrag(vec);
    }
    public void updateDrag(Vector2 vec)
    {
        pos = dragBase + vec - dragStart;
    }
    //zoomCenter in global coord
    public void zoomOut(Vector2 zoomCenter)
    {
        var scaleLevel_new = Math.Max(scaleLevel - 1, -10);
        pos = zoomCenter - (zoomCenter - pos) * MathF.Pow(2, scaleLevel_new - scaleLevel);
        scaleLevel = scaleLevel_new;
    }
    public void zoomIn(Vector2 zoomCenter)
    {
        var scaleLevel_new = Math.Min(scaleLevel + 1, 10);
        pos = zoomCenter - (zoomCenter - pos) * MathF.Pow(2, scaleLevel_new - scaleLevel);
        scaleLevel = scaleLevel_new;
    }

    public void clear()
    {
        store.clear();
    }
    public Vector2 toLocal(Vector2 gpos)
    {
        var realScale = Mathf.Pow(2, scaleLevel);
        return (gpos - pos) / realScale;
    }
}