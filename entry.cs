using Godot;
using System;
using Sketchpad;
public partial class entry : Node2D
{
    [Export] UI ui;
    [Export] Canvas canvas;
    [Export] Grid grid;
    [Export] AudioStreamPlayer uiSound;
    SketchPad pad;
    AudioStream snd_layerChange;
    bool needRefresh = false;
    bool needSetCursor = false;
    Input.CursorShape cursor = Input.CursorShape.Arrow;
    public override void _Ready()
    {
        Input.UseAccumulatedInput = false;
        Engine.MaxFps = 30;
        RenderingServer.SetDefaultClearColor(Color.Color8(240, 240, 245));
        //Stop rendering for manually drawing
        RenderingServer.RenderLoopEnabled = false;

        pad = new SketchPad();
        pad.update += padUpdate;
        GetViewport().SizeChanged += viewportChange;
        GetWindow().MinSize = new Vector2I(640, 480);
        GetWindow().FocusEntered += winFocusEnter;
        GetWindow().FocusExited += winFocusLost;
        GetWindow().CloseRequested += beforeClose;
        GetWindow().Title = "Sketch pad";

        Session.loadSession(pad);

        setCursor(pad.drawMode == DrawMode.Pen ? Input.CursorShape.Cross : Input.CursorShape.Arrow);

        ui.colorChange += pad.colorChange;
        ui.needRedraw += viewportRedraw;
        ui.updateStatus(pad);

        snd_layerChange = ResourceLoader.Load<AudioStream>("res://audio/p.mp3");

        viewportChange();
    }

    private void beforeClose()
    {
        Session.saveSession(pad);
    }

    private void winFocusLost()
    {
        GetTree().Paused = true;
    }

    private void winFocusEnter()
    {
        GetTree().Paused = false;
        viewportRedraw();
    }

    public override void _Process(double delta)
    {
        if (needRefresh)
        {
            RenderingServer.ForceDraw();
            needRefresh = false;
        }
        if (needSetCursor)
        {
            Input.SetDefaultCursorShape(cursor);
            needSetCursor = false;
        }
    }

    private void padUpdate(EventType t)
    {
        switch (t)
        {
            case EventType.Color:
                ui.updateStatus(pad);
                break;
            case EventType.Layer:
                grid.drawGrid(pad.currentLayer);
                canvas.drawStroke(pad.currentLayer);
                ui.updateStatus(pad);
                canvas.setDebugDisplayEnabled(pad.currentLayer.setting.useDebugColor);
                uiSound.Stream = snd_layerChange;
                uiSound.Play();
                break;
            case EventType.Stats:
                ui.updateStatus(pad);
                break;
            case EventType.Zoom:
                grid.drawGrid(pad.currentLayer);
                canvas.drawStroke(pad.currentLayer);
                break;
            case EventType.Grid:
                grid.drawGrid(pad.currentLayer);
                break;
            case EventType.Debug:
                canvas.setDebugDisplayEnabled(pad.currentLayer.setting.useDebugColor);
                break;
            case EventType.Stroke:
                canvas.drawStroke(pad.currentLayer);
                ui.updateStatus(pad);
                break;
            case EventType.Drag:
                canvas.Position = pad.currentLayer.pos;
                grid.drawGrid(pad.currentLayer);
                break;
            default:
                break;
        }
        viewportRedraw();
        GD.Print($"Pad Event: {t}");
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
                }
                GetViewport().SetInputAsHandled();
            }
            else if (eventMouseButton.ButtonIndex == MouseButton.Middle)
            {
                if (eventMouseButton.Pressed)
                {
                    GD.Print("drag started");
                    pad.beginDrag(eventMouseButton.Position);
                    setCursor(Input.CursorShape.PointingHand);

                }
                else
                {
                    GD.Print("drag stoped");
                    pad.endDrag(eventMouseButton.Position);
                    pad.setGrid(GridType.Refresh);
                    setCursor(pad.drawMode == DrawMode.Pen ? Input.CursorShape.Cross : Input.CursorShape.Arrow);
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
        }
        else if (@event.IsActionPressed("sketchpad_grid_hexgon"))
        {
            pad.setGrid(GridType.Hexgon);
        }
        else if (@event.IsActionPressed("sketchpad_grid_squre"))
        {
            pad.setGrid(GridType.Square);
        }
        else if (@event.IsActionPressed("sketchpad_next"))
        {
            pad.nextPage();
        }
        else if (@event.IsActionPressed("sketchpad_prev"))
        {
            pad.prevPage();
        }
        else if (@event.IsActionPressed("sketchpad_erase"))
        {
            pad.toggleEraseMode();
            viewportRedraw();
            setCursor(pad.drawMode == DrawMode.Pen ? Input.CursorShape.Cross : Input.CursorShape.Arrow);
        }
        else if (@event.IsActionPressed("sketchpad_debug"))
        {
            ui.toggleDebugPanel();
        }
        else if (@event.IsActionPressed("sketchpad_zoomout"))
        {
            pad.zoomOut(GetLocalMousePosition());
        }
        else if (@event.IsActionPressed("sketchpad_zoomin"))
        {
            pad.zoomIn(GetLocalMousePosition());
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
        var rect = GetViewportRect();
        grid.drawGrid(pad.currentLayer);
        canvas.drawStroke(pad.currentLayer);
        ui.updateLayout(rect);
        viewportRedraw();
    }
    void viewportRedraw()
    {
        needRefresh = true;
    }
    public void setDebugEnabled(bool value)
    {
        pad.setDebugDisplayEnabled(value);
    }
    void setCursor(Input.CursorShape c)
    {
        //seems setting cursor  using "Input.SetDefaultCursorShape" while:
        //1. Input.UseAccumulatedInput == false;
        //2. Called in _Input function
        //will not change immediately(simulated mm not working)
        //So i delay the function to _Process,and it works.
        needSetCursor = true;
        cursor = c;
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

        canvas.drawStroke(pad.currentLayer);

        viewportRedraw();
    }
}
