using Godot;
using System;

public partial class entry : Node2D
{
    public SketchPad pad;
    public override void _Ready()
    {
        Input.UseAccumulatedInput = false;

        pad = new SketchPad();
        Canvas canvas = GetNode<Canvas>("canvas");
        pad.canvas = canvas;
        pad.grid = GetNode<Grid>("grid");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
    public override void _Draw()
    {
        DrawRect(GetViewportRect(), Color.Color8(250, 250, 255));
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton)
        {
            if (eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                if (eventMouseButton.Pressed)
                {
                    GD.Print("draw started");
                    pad.beginStroke(eventMouseButton.Position);
                }
                else
                {
                    GD.Print("draw stoped");
                    pad.endStroke();
                }
                GetViewport().SetInputAsHandled();
            }
            else if (eventMouseButton.ButtonIndex == MouseButton.Middle)
            {
                if (eventMouseButton.Pressed)
                {
                    GD.Print("drag started");
                    pad.beginDrag(eventMouseButton.Position);
                }
                else
                {
                    GD.Print("drag stoped");
                    pad.endDrag(eventMouseButton.Position);
                }
                GetViewport().SetInputAsHandled();
            }
        }
        else if (@event is InputEventMouseMotion eventMouseMotion)
        {
            if (eventMouseMotion.ButtonMask == MouseButtonMask.Left)
            {
                // GD.Print("drawing ", eventMouseMotion.Pressure);
                pad.appendStroke(eventMouseMotion.Position, eventMouseMotion.Pressure);

                GetWindow().Title = pad.currentLayer.elems.Count.ToString();
                GetViewport().SetInputAsHandled();
            }
            else if (eventMouseMotion.ButtonMask == MouseButtonMask.Middle)
            {
                pad.updateDrag(eventMouseMotion.Position);
                pad.setGrid(GridType.Refresh);
                GetViewport().SetInputAsHandled();
            }

        }
        else if (@event is InputEventKey eventKey)
        {
            if (eventKey.Pressed)
            {
                switch (eventKey.Keycode)
                {
                    case Key.Kp4:
                        pad.setGrid(GridType.Square);
                        break;

                    default:
                        return;
                }
            }
        }
    }
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("sketchpad_clear"))
        {
            GD.Print("sketchpad_clear occurred!");
            pad.clear();
        }
    }
}
