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
    public StrokeElement(Vector2 p, float pressure, float d)
    {
        pos = p;
        size = new Vector2(pressure, pressure);
        dir = d;
    }
    public Vector2 pos = Vector2.Zero;
    public float dir = 0;
    public Vector2 size = Vector2.One;
}
public class SketchLayer
{
    static float tau_f = (float)Math.Tau;
    float defaultPressure = 0.1f;
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
    public SketchLayer()
    {
    }
    public void beginStroke(Vector2 vec)
    {
        StrokeElement se = new StrokeElement(vec - pos, defaultPressure, rnd.Randf() * tau_f);
        store.addStroke(se);
    }
    public void endStroke()
    {
    }
    public void appendStroke(Vector2 vec, float pressure)
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
                se = new StrokeElement(layerCoord, pressure, rnd.Randf() * tau_f);
                store.addStroke(se);
            }
            else
            {
                //interpoation
                var lerpStep = distThreshold / dist;
                var lerpAccumulate = lerpStep;
                var lastPressure = lastElem.size.X;
                while (lerpAccumulate < 1)
                {
                    var i_pressure = lastPressure + (pressure - lastPressure) * lerpAccumulate;
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