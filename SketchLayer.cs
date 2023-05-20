using Godot;
using System;
using System.Collections.Generic;

public class SketchLayer
{
    static float tau_f = (float)Math.Tau;
    RandomNumberGenerator rnd = new RandomNumberGenerator();

    bool isDraging = false;
    Vector2 dragStart;
    Vector2 dragBase;//layer pos when drag start

    public Vector2 pos = Vector2.Zero;//layer origin in screen coord

    public GridType gtype = GridType.None;
    public float gdim = 50f;

    public StrokeStore store = new StrokeStore();

    StrokeElement lastErase;
    public SketchLayer()
    {
    }
    public void beginErase(Vector2 vec, float size)
    {
        lastErase = new StrokeElement(vec - pos, size, 0);
    }
    public void endErase()
    {
    }
    public void appendErase(Vector2 vec, float size)
    {
        StrokeElement newErase = new StrokeElement(vec - pos, size, 0);
        store.eraseCollide(lastErase, newErase);
        lastErase = newErase;
    }
    public void beginStroke(Vector2 vec, float size)
    {
        StrokePoint se = new StrokePoint(vec - pos, size / 2);
        store.addStroke(se, true);
    }
    public void endStroke()
    {
        store.endStroke();
    }
    public void appendStroke(Vector2 vec, float size)
    {
        var layerCoord = vec - pos;
        StrokePoint se = new StrokePoint(layerCoord, size / 2);
        store.addStroke(se);
    }
    public void beginDrag(Vector2 vec)
    {
        isDraging = true;
        dragBase = pos;
        dragStart = vec;
    }
    public void endDrag(Vector2 vec)
    {
        updateDrag(vec);
        isDraging = false;
    }
    public void updateDrag(Vector2 vec)
    {
        pos = dragBase + vec - dragStart;
    }

    public void clear()
    {
        store.clear();
    }
}