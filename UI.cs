using Godot;
using System;

public partial class UI : Node2D
{
    float height_statusbar = 40;
    [Export]
    PanelContainer bg;
    [Export]
    Label layerIndicator;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // bg = GetNode<PanelContainer>("statusbar");
        // layerIndicator = GetNode<Label>("layerIndicator");
        updateLayout(GetViewportRect());
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
    public void updateLayout(Rect2 vp_rect)
    {
        bg.Position = new Vector2(0, vp_rect.Size.Y - height_statusbar);
        bg.Size = new Vector2(vp_rect.Size.X, height_statusbar);
    }
    public void updateStatus(SketchPad pad)
    {
        layerIndicator.Text = $"{pad.currentLayerID + 1}/{pad.layers.Count}";

    }
}
