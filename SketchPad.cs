using Godot;
using System.Collections.Generic;

public enum GridType
{
    Square, None, Refresh, Hexgon
}
enum PadState
{
    Drag,
    Draw,
    Idle
}
enum DrawMode
{
    Pen,
    Erase
}
public class SketchPad
{
    public List<SketchLayer> layers = new List<SketchLayer>();
    public int currentLayerID = 0;
    public SketchLayer currentLayer;

    public Canvas canvas;
    public Grid grid;
    public UI ui;
    PadState state = PadState.Idle;
    DrawMode drawMode = DrawMode.Pen;

    float baseStrokeSize = 6;
    float defaultPressure = 0.5f;
    float eraseSizeMultiplier = 0.5f;
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
            state = PadState.Draw;
            if (drawMode == DrawMode.Pen)
            {
                currentLayer.beginStroke(vec, mapPressureToSize(defaultPressure));
            }
            else
            {
                currentLayer.beginErase(vec, mapPressureToSize(defaultPressure * eraseSizeMultiplier));
            }
            canvas.drawStroke(currentLayer);
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
            state = PadState.Idle;
            if (drawMode == DrawMode.Pen)
            {
                currentLayer.endStroke();
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

        grid.drawGrid(currentLayer);
        canvas.drawStroke(currentLayer);
        ui.updateStatus(this);
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
            grid.drawGrid(currentLayer);
            canvas.drawStroke(currentLayer);
            ui.updateStatus(this);
        }

    }
    public void toggleDebugPanel()
    {
        ui.toggleDebugPanel();
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
    public void viewportChange(Rect2 vp_rect)
    {
        grid.drawGrid(currentLayer);
        canvas.drawStroke(currentLayer);
        ui.updateLayout(vp_rect);
    }
    float mapPressureToSize(float p)
    {
        return Mathf.Sqrt(p) * baseStrokeSize;
    }
    public void Process(double delta)
    {
        if (state == PadState.Drag)
        {
            Input.SetDefaultCursorShape(Input.CursorShape.PointingHand);
        }
        else if (drawMode == DrawMode.Pen)
        {
            Input.SetDefaultCursorShape(Input.CursorShape.Cross);
        }
        else
        {
            Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
        }

    }
}
