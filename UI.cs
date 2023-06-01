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
    CheckButton debugToggle;
    [Export]
    ColorPicker colorPicker;
    [Export]
    Label layerIndicator;
    [Export]
    Label strokeIndicator;
    public event Godot.ColorPicker.ColorChangedEventHandler colorChange;
    public event Action needRedraw;
    public override void _Ready()
    {
        updateLayout(GetViewportRect());
        debug.needRedraw += redrawViewport;
        colorPicker.needRedraw += redrawViewport;
        colorPicker.colorChange += pickerColorChange;
        debug.Visible = false;
    }
    void pickerColorChange(Color c)
    {
        colorChange.Invoke(c);
    }
    private void redrawViewport()
    {
        needRedraw?.Invoke();
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
        colorPicker.Color = pad.currentLayer.setting.color;
        debugToggle.SetPressedNoSignal(pad.currentLayer.setting.useDebugColor);
        redrawViewport();
    }
    public void toggleDebugPanel()
    {
        debug.Visible = !debug.Visible;
    }
}
