using Godot;
using System;

public partial class UI : Node2D
{
    float height_statusbar = 40;
    [Export]
    PanelContainer bg;
    [Export]
    DebugPanel debug;
    [Export]
    ColorPicker colorPicker;
    [Export]
    Label layerIndicator;
    [Export]
    Label strokeIndicator;
    // Called when the node enters the scene tree for the first time.

    public event Godot.ColorPicker.ColorChangedEventHandler colorChange;
    public override void _Ready()
    {
        updateLayout(GetViewportRect());
        debug.Visible = false;
        debug.MouseEntered += debugPanelEntered;
        debug.MouseExited += debugPanelLeave;
        colorPicker.needRedraw += redrawViewport;
        colorPicker.colorChange += pickerColorChange;
    }
    void pickerColorChange(Color c)
    {
        colorChange.Invoke(c);
    }
    private void redrawViewport()
    {
        RenderingServer.ViewportSetUpdateMode(GetViewport().GetViewportRid(), RenderingServer.ViewportUpdateMode.Once);
        RenderingServer.ViewportSetClearMode(GetViewport().GetViewportRid(), RenderingServer.ViewportClearMode.OnlyNextFrame);
    }

    public void updateLayout(Rect2 vp_rect)
    {
        bg.Position = new Vector2(0, vp_rect.Size.Y - height_statusbar);
        bg.Size = new Vector2(vp_rect.Size.X, height_statusbar);
    }
    public void updateStatus(SketchPad pad)
    {
        layerIndicator.Text = $"{pad.currentLayerID + 1}/{pad.layers.Count}";
        strokeIndicator.Text = $"{pad.currentLayer.store.elemCount}";
    }
    public void toggleDebugPanel()
    {
        debug.Visible = !debug.Visible;
    }
    void debugPanelEntered()
    {
        RenderingServer.ViewportSetUpdateMode(GetViewport().GetViewportRid(), RenderingServer.ViewportUpdateMode.WhenVisible);
        RenderingServer.ViewportSetClearMode(GetViewport().GetViewportRid(), RenderingServer.ViewportClearMode.Always);
    }
    void debugPanelLeave()
    {
        RenderingServer.ViewportSetUpdateMode(GetViewport().GetViewportRid(), RenderingServer.ViewportUpdateMode.Once);
        RenderingServer.ViewportSetClearMode(GetViewport().GetViewportRid(), RenderingServer.ViewportClearMode.OnlyNextFrame);
        GD.Print("leave");
    }
}
