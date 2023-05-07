using Godot;
using System.Collections.Generic;

enum PadState
{
    Drag,
    Draw,
    Idle
}
public class SketchPad
{
    List<SketchLayer> layers = new List<SketchLayer>();
    int currentLayerID = 0;
    public SketchLayer currentLayer;

    public Canvas canvas;
    public Grid grid;

    PadState state = PadState.Idle;
    public SketchPad()
    {
        layers.Add(new SketchLayer());
        currentLayerID = 0;
        currentLayer = layers[currentLayerID];
    }
    public void beginStroke(Vector2 vec)
    {
        if (state == PadState.Idle)
        {
            currentLayer.beginStroke(vec);
            state = PadState.Draw;
            Input.SetDefaultCursorShape(Input.CursorShape.Cross);
        }
        else
        {
            GD.Print("bad draw :", state);
        }
    }
    public void endStroke()
    {
        if (state == PadState.Draw)
        {
            currentLayer.endStroke();
            state = PadState.Idle;
            Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
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
            currentLayer.appendStroke(vec, pressure);
            canvas.drawStroke(currentLayer);
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
            Input.SetDefaultCursorShape(Input.CursorShape.PointingHand);
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
            canvas.Position = currentLayer.pos;
            state = PadState.Idle;
            Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
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
            canvas.Position = currentLayer.pos;
        }
        else
        {
            GD.Print("bad drag :", state);
        }
    }
    public void setGrid(GridType t)
    {
        if (t == GridType.Refresh)
        {
            grid.drawGrid(currentLayer);
            return;
        }
        if (currentLayer.gtype != t)
        {
            currentLayer.gtype = t;
            grid.drawGrid(currentLayer);
        }
        else
        {
            currentLayer.gtype = GridType.None;
            grid.drawGrid(currentLayer);
        }
    }
    public GridType getGrid()
    {
        return currentLayer.gtype;
    }
    public void clear()
    {
        currentLayer.clear();
        canvas.drawStroke(currentLayer);
    }
}
