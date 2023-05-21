using Godot;
using System;

public partial class entry : SubViewportContainer
{
    [Export]
    UI ui;
    [Export]
    Canvas canvas;
    public SketchPad pad;
    public Rid vp_id;
    public override void _Ready()
    {
        Input.UseAccumulatedInput = false;
        Engine.MaxFps = 30;
        vp_id = GetViewport().GetViewportRid();
        RenderingServer.ViewportSetClearMode(vp_id, RenderingServer.ViewportClearMode.OnlyNextFrame);
        RenderingServer.SetDefaultClearColor(Color.Color8(240, 240, 245));

        pad = new SketchPad();
        canvas.ProcessMode = ProcessModeEnum.Pausable;
        pad.canvas = canvas;
        pad.grid = GetNode<Grid>("%grid");
        pad.ui = ui;
        GetViewport().SizeChanged += viewportChange;
        GetWindow().MinSize = new Vector2I(640, 480);
        GetTree().Paused = true;

        viewportChange();

        var vp = GetNode<SubViewport>("%SubViewport");
        RenderingServer.ViewportSetRenderDirectToScreen(vp.GetViewportRid(), true);

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        GetWindow().Title = pad.currentLayer.store.elemCount.ToString() + " ";
        pad.Process(delta);
    }
    // public override void _Draw()
    // {
    //     DrawRect(GetViewportRect(), Color.Color8(240, 240, 245));
    // }
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
                    pad.setGrid(GridType.Refresh);
                }
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
                        RenderingServer.ViewportSetClearMode(vp_id, RenderingServer.ViewportClearMode.OnlyNextFrame);
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
            pad.clear();
            RenderingServer.ViewportSetClearMode(vp_id, RenderingServer.ViewportClearMode.OnlyNextFrame);
        }
        else if (@event.IsActionPressed("sketchpad_grid_hexgon"))
        {
            pad.setGrid(GridType.Hexgon);
            RenderingServer.ViewportSetClearMode(vp_id, RenderingServer.ViewportClearMode.OnlyNextFrame);
        }
        else if (@event.IsActionPressed("sketchpad_next"))
        {
            pad.nextPage();
            RenderingServer.ViewportSetClearMode(vp_id, RenderingServer.ViewportClearMode.OnlyNextFrame);
        }
        else if (@event.IsActionPressed("sketchpad_prev"))
        {
            pad.prevPage();
            RenderingServer.ViewportSetClearMode(vp_id, RenderingServer.ViewportClearMode.OnlyNextFrame);
        }
        else if (@event.IsActionPressed("sketchpad_erase"))
        {
            pad.toggleEraseMode();
            RenderingServer.ViewportSetClearMode(vp_id, RenderingServer.ViewportClearMode.OnlyNextFrame);
        }
        else if (@event.IsActionPressed("sketchpad_debug"))
        {
            pad.toggleDebugPanel();
            RenderingServer.ViewportSetClearMode(vp_id, RenderingServer.ViewportClearMode.OnlyNextFrame);
        }
        else if (@event is InputEventMouseMotion eventMouseMotion)
        {
            if (eventMouseMotion.ButtonMask == MouseButtonMask.Left)
            {
                pad.appendStroke(eventMouseMotion.Position, eventMouseMotion.Pressure);
            }
            else if (eventMouseMotion.ButtonMask == MouseButtonMask.Middle)
            {
                pad.updateDrag(eventMouseMotion.Position);
                pad.setGrid(GridType.Refresh);
            }
        }
    }
    void viewportChange()
    {
        pad.viewportChange(GetViewportRect());
        Size = GetViewportRect().Size;
    }

    void debug_generate_stroke()
    {

        pad.beginStroke(Vector2.Zero);
        RandomNumberGenerator rd = new RandomNumberGenerator();
        Vector2 ssize = GetViewportRect().Size;

        for (int i = 0; i < 10000; i++)
        {
            pad.appendStroke(new Vector2(ssize.X * rd.Randf(), ssize.Y * rd.Randf()), rd.Randf());
            if (pad.currentLayer.store.elemCount > 10000)
            {
                break;
            }
        }
        pad.endStroke();
    }
    void debug_stroke_interp()
    {
        //interp test
        // pad.currentLayer.store.addStroke(new StrokePoint(new Vector2(200, 200), 5));
        // pad.currentLayer.store.addStroke(new StrokePoint(new Vector2(400, 400), 5));
        // pad.currentLayer.store.addStroke(new StrokePoint(new Vector2(600, 400), 5));
        pad.currentLayer.store.addStroke(new StrokePoint(new Vector2(600, 200), 5));
        pad.currentLayer.store.addStroke(new StrokePoint(new Vector2(600, 212), 5));
        pad.currentLayer.store.addStroke(new StrokePoint(new Vector2(600, 250), 5));

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
        pad.currentLayer.store.endStroke();
        pad.canvas.drawStroke(pad.currentLayer);
    }
}
