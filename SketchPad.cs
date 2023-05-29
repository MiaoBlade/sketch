using Godot;
using System.Collections.Generic;
using Sketchpad;
public class SketchPad
{
    public List<SketchLayer> layers = new List<SketchLayer>();
    public int currentLayerID = 0;
    public SketchLayer currentLayer;
    public PadState state = PadState.Idle;
    public DrawMode drawMode = DrawMode.Pen;
    public event SketchPadUpdate update;
    float baseStrokeSize = 6;
    float defaultPressure = 0.5f;
    float eraseSizeMultiplier = 0.5f;
    public SketchPad()
    {
        layers.Add(new SketchLayer());
        currentLayerID = 0;
        currentLayer = layers[currentLayerID];
    }

    public void colorChange(Color color)
    {
        currentLayer.setting.color = color;
    }

    public void beginStroke(Vector2 vec)
    {
        if (state == PadState.Idle)
        {
            state = PadState.Draw;
            if (drawMode == DrawMode.Pen)
            {
                currentLayer.beginStroke(vec, mapPressureToSize(defaultPressure));
            }
            else
            {
                currentLayer.beginErase(vec, mapPressureToSize(defaultPressure * eraseSizeMultiplier));
            }
            update.Invoke(EventType.Stroke);
        }
        else
        {
            GD.Print("bad draw :", state);
        }
    }
    public void endStroke(Vector2 vec)
    {
        if (state == PadState.Draw)
        {
            state = PadState.Idle;
            if (drawMode == DrawMode.Pen)
            {
                currentLayer.endStroke(vec);
                update.Invoke(EventType.Stroke);
            }
            else
            {
                currentLayer.endErase();
            }
        }
        else
        {
            GD.Print("bad draw :", state);
        }
    }
    public void appendStroke(Vector2 vec, float pressure)
    {
        if (state == PadState.Draw)
        {
            if (drawMode == DrawMode.Pen)
            {
                currentLayer.appendStroke(vec, mapPressureToSize(pressure));
            }
            else
            {
                currentLayer.appendErase(vec, mapPressureToSize(eraseSizeMultiplier * pressure));
            }
            update.Invoke(EventType.Stroke);
        }
        else
        {
            GD.Print("bad draw :", state);
        }
    }
    public void beginDrag(Vector2 vec)
    {
        if (state == PadState.Idle)
        {
            currentLayer.beginDrag(vec);
            state = PadState.Drag;
        }
        else
        {
            GD.Print("bad drag :", state);
        }
    }
    public void endDrag(Vector2 vec)
    {
        if (state == PadState.Drag)
        {
            currentLayer.endDrag(vec);
            update.Invoke(EventType.Drag);
            state = PadState.Idle;
        }
        else
        {
            GD.Print("bad drag :", state);
        }
    }
    public void updateDrag(Vector2 vec)
    {
        if (state == PadState.Drag)
        {
            currentLayer.updateDrag(vec);
            update.Invoke(EventType.Drag);
        }
        else
        {
            GD.Print("bad drag :", state);
        }
    }
    public void setGrid(GridType t)
    {
        if (currentLayer.gtype != t)
        {
            if (t != GridType.Refresh)
            {
                currentLayer.gtype = t;
            }
        }
        else
        {
            currentLayer.gtype = GridType.None;
        }
        update.Invoke(EventType.Grid);
    }
    public GridType getGrid()
    {
        return currentLayer.gtype;
    }
    public void clear()
    {
        currentLayer.clear();
        update.Invoke(EventType.Stroke);
    }
    public void nextPage()
    {
        if (state != PadState.Idle)
        {
            return;
        }
        if (currentLayerID == layers.Count - 1)
        {
            layers.Add(new SketchLayer());
        }
        currentLayerID += 1;
        currentLayer = layers[currentLayerID];
        update.Invoke(EventType.Layer);
    }
    public void prevPage()
    {
        if (state != PadState.Idle)
        {
            return;
        }
        if (currentLayerID != 0)
        {
            currentLayerID -= 1;
            currentLayer = layers[currentLayerID];
            update.Invoke(EventType.Layer);
        }
    }
    public void zoomOut(Vector2 zoomCenter)
    {
        currentLayer.zoomOut(zoomCenter);
        update.Invoke(EventType.Zoom);
    }
    public void zoomIn(Vector2 zoomCenter)
    {
        currentLayer.zoomIn(zoomCenter);
        update.Invoke(EventType.Zoom);
    }
    public void toggleEraseMode()
    {
        if (drawMode == DrawMode.Pen)
        {
            drawMode = DrawMode.Erase;
        }
        else
        {
            drawMode = DrawMode.Pen;
        }
    }
    float mapPressureToSize(float p)
    {
        return Mathf.Sqrt(p) * baseStrokeSize;
    }
    public void setDebugDisplayEnabled(bool value)
    {
        currentLayer.setting.useDebugColor = value;
        update.Invoke(EventType.Debug);
    }
}
