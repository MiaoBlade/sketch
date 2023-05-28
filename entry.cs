using Godot;
using System;
using Sketchpad;
public partial class entry : SubViewportContainer
{
    [Export] UI ui;
    [Export] Canvas canvas;
    [Export] Grid grid;
    public SketchPad pad;
    Rid vp_canvas_id;
    SubViewport vp_canvas;

    public override void _Ready()
    {
        Input.UseAccumulatedInput = false;
        Engine.MaxFps = 30;
        RenderingServer.ViewportSetClearMode(GetViewport().GetViewportRid(), RenderingServer.ViewportClearMode.OnlyNextFrame);
        RenderingServer.SetDefaultClearColor(Color.Color8(240, 240, 245));

        pad = new SketchPad();
        canvas.ProcessMode = ProcessModeEnum.Pausable;
        pad.canvas = canvas;
        pad.grid = grid;
        pad.update += padUpdate;
        ui.colorChange += pad.colorChange;
        GetViewport().SizeChanged += viewportChange;
        GetWindow().MinSize = new Vector2I(640, 480);

        vp_canvas = GetNode<SubViewport>("%SubViewport");
        vp_canvas_id = vp_canvas.GetViewportRid();

        viewportChange();

        GetWindow().FocusEntered += windowActive;
        GetWindow().FocusExited += windowIdle;

        MouseDefaultCursorShape = pad.drawMode == DrawMode.Pen ? CursorShape.Cross : CursorShape.Arrow;

        GetWindow().Title = "Sketch pad";
        ui.updateStatus(pad);
    }

    private void padUpdate(EventType t)
    {
        switch (t)
        {
            case EventType.Color:
                ui.updateStatus(pad);
                break;
            case EventType.Layer:
                ui.updateStatus(pad);
                break;
            case EventType.Stats:
                ui.updateStatus(pad);
                break;
            default:
                break;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

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
                    pad.endStroke(eventMouseButton.Position);
                    viewportRedraw();
                }
                GetViewport().SetInputAsHandled();
            }
            else if (eventMouseButton.ButtonIndex == MouseButton.Middle)
            {
                if (eventMouseButton.Pressed)
                {
                    GD.Print("drag started");
                    pad.beginDrag(eventMouseButton.Position);
                    MouseDefaultCursorShape = CursorShape.PointingHand;
                }
                else
                {
                    GD.Print("drag stoped");
                    pad.endDrag(eventMouseButton.Position);
                    pad.setGrid(GridType.Refresh);
                    MouseDefaultCursorShape = pad.drawMode == DrawMode.Pen ? CursorShape.Cross : CursorShape.Arrow;
                }
                GetViewport().SetInputAsHandled();
            }
        }
    }
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("sketchpad_clear"))
        {
            pad.clear();
            viewportRedraw();
        }
        else if (@event.IsActionPressed("sketchpad_grid_hexgon"))
        {
            pad.setGrid(GridType.Hexgon);
            viewportRedraw();
        }
        else if (@event.IsActionPressed("sketchpad_grid_squre"))
        {
            pad.setGrid(GridType.Square);
            viewportRedraw();
        }
        else if (@event.IsActionPressed("sketchpad_next"))
        {
            pad.nextPage();
            viewportRedraw();
        }
        else if (@event.IsActionPressed("sketchpad_prev"))
        {
            pad.prevPage();
            viewportRedraw();
        }
        else if (@event.IsActionPressed("sketchpad_erase"))
        {
            pad.toggleEraseMode();
            viewportRedraw();
            MouseDefaultCursorShape = pad.drawMode == DrawMode.Pen ? CursorShape.Cross : CursorShape.Arrow;
        }
        else if (@event.IsActionPressed("sketchpad_debug"))
        {
            ui.toggleDebugPanel();
            viewportRedraw();
        }
        else if (@event.IsActionPressed("sketchpad_zoomout"))
        {
            pad.zoomOut();
            viewportRedraw();
        }
        else if (@event.IsActionPressed("sketchpad_zoomin"))
        {
            pad.zoomIn();
            viewportRedraw();
        }
        else if (@event is InputEventMouseMotion eventMouseMotion)
        {
            if (eventMouseMotion.ButtonMask == MouseButtonMask.Left)
            {
                pad.appendStroke(eventMouseMotion.Position, eventMouseMotion.Pressure);
                viewportRedraw();
            }
            else if (eventMouseMotion.ButtonMask == MouseButtonMask.Middle)
            {
                pad.updateDrag(eventMouseMotion.Position);
                pad.setGrid(GridType.Refresh);
                viewportRedraw();
            }
        }
    }
    void viewportChange()
    {
        var rect = GetViewportRect();
        grid.drawGrid(pad.currentLayer);
        canvas.drawStroke(pad.currentLayer);
        ui.updateLayout(rect);
        Size = rect.Size;
        viewportRedraw();
    }
    void viewportRedraw()
    {
        RenderingServer.ViewportSetUpdateMode(vp_canvas_id, RenderingServer.ViewportUpdateMode.Once);
    }
    void windowIdle()
    {
        RenderingServer.ViewportSetUpdateMode(GetViewport().GetViewportRid(), RenderingServer.ViewportUpdateMode.Disabled);
        GetTree().Paused = true;
    }
    void windowActive()
    {
        RenderingServer.ViewportSetUpdateMode(GetViewport().GetViewportRid(), RenderingServer.ViewportUpdateMode.WhenVisible);
        GetTree().Paused = false;
    }
    public void setDebugEnabled(bool value)
    {
        pad.setDebugDisplayEnabled(value);
        viewportRedraw();
    }
    void debug_generate_stroke()
    {
        pad.beginStroke(Vector2.Zero);
        Vector2 ssize = GetViewportRect().Size;
        int currentCount = pad.currentLayer.store.elemCount;

        for (int i = 0; i < 10000; i++)
        {
            pad.appendStroke(new Vector2(ssize.X * GD.Randf(), ssize.Y * GD.Randf()), GD.Randf());
            if (pad.currentLayer.store.elemCount - currentCount > 10000)
            {
                break;
            }
        }
        pad.endStroke(Vector2.Zero);
        viewportRedraw();
    }
    void debug_stroke_interp()
    {
        //interp test
        pad.currentLayer.store.addStroke(new StrokePoint(new Vector2(500, 300), 5));
        pad.currentLayer.store.addStroke(new StrokePoint(new Vector2(450, 300), 5));
        pad.currentLayer.store.addStroke(new StrokePoint(new Vector2(400, 250), 5));

        pad.currentLayer.store.endStroke(new StrokePoint(new Vector2(200, 200), 5));
        // float x = 500;
        // float y = 200;
        // float x_step = 5;
        // float y_step = 2;
        // pad.currentLayer.store.addStroke(new StrokePoint(new Vector2(x, y), 2), true);

        // for (int i = 0; i < 10; i++)
        // {
        //     x += x_step;
        //     y += y_step;
        //     pad.currentLayer.store.addStroke(new StrokePoint(new Vector2(x, y), 2));
        //     y_step *= -1;
        // }

        // var bcurve = new CubicCurve(new Vector2(300, 200), new Vector2(400, 300), new Vector2(500, 100), new Vector2(600, 200));

        // for (int i = 0; i < 21; i++)
        // {
        //     // var p=bcurve.getPoint(((float)Math.Sqrt(i/10.0)));
        //     var p = bcurve.getPoint((float)(i / 20.0));
        //     pad.currentLayer.store.addPureStroke(new StrokePoint(p, 5));
        // }

        pad.canvas.drawStroke(pad.currentLayer);

        viewportRedraw();
    }
}
