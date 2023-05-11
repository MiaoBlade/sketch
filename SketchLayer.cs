using Godot;
using System;
using System.Collections.Generic;

public enum GridType
{
    Square, None, Refresh, Hexgon
}
public struct StrokeElement
{
    public int ID = 0;
    public StrokeElement(Vector2 p, float s, float d)
    {
        pos = p;
        size = s;
        dir = d;
    }
    public Vector2 pos = Vector2.Zero;
    public float dir = 0;
    public float size = 1;
}
public class SketchLayer
{
    static float tau_f = (float)Math.Tau;
    float distThreshold = 5.0f;
    float distIgnoreThreshold = 1f;
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
        StrokeElement se = new StrokeElement(vec - pos, size, rnd.Randf() * tau_f);
        store.addStroke(se);
    }
    public void endStroke()
    {
    }
    public void appendStroke(Vector2 vec, float size)
    {
        StrokeElement se;
        var layerCoord = vec - pos;
        if (store.elemCount != 0)
        {
            var lastElem = store.lastStrokeElement;
            var dist = layerCoord.DistanceTo(lastElem.pos);
            if (dist < distIgnoreThreshold)
            {
                return;
            }
            if (dist < distThreshold)
            {
                //too close,just add
                se = new StrokeElement(layerCoord, size, rnd.Randf() * tau_f);
                store.addStroke(se);
            }
            else
            {
                //interpoation
                var lerpStep = distThreshold / dist;
                var lerpAccumulate = lerpStep;
                var lastPressure = lastElem.size;
                while (lerpAccumulate < 1)
                {
                    var i_pressure = lastPressure + (size - lastPressure) * lerpAccumulate;
                    var newElem = new StrokeElement(lastElem.pos.Lerp(layerCoord, lerpAccumulate), i_pressure, rnd.Randf() * tau_f);
                    store.addStroke(newElem);
                    lerpAccumulate += lerpStep;
                }

            }

        }

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