using Godot;
using System;
using System.Collections.Generic;

public partial class Canvas : Node2D
{
    Color drawColor = Color.Color8(255, 50, 0);
    Color backColor = Color.Color8(255, 255, 255);
    // Called when the node enters the scene tree for the first time.

    QuadMesh qMesh;
    Texture2D tex;
    SketchLayer layer;
    MultiMesh mmesh;
    public override void _Ready()
    {
        qMesh = new QuadMesh();
        tex = GD.Load<Texture2D>("res://brush_round.png");
        mmesh = new MultiMesh();
        mmesh.Mesh = qMesh;
        mmesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform2D;
    }
    public override void _Draw()
    {
        if (layer != null)
        {
            if (mmesh.InstanceCount < layer.elems.Count)
            {
                mmesh.InstanceCount = layer.elems.Count;
            }
            mmesh.VisibleInstanceCount = layer.elems.Count;
            var i = 0;
            var position = Transform2D.Identity.ScaledLocal(new Vector2(10, 10));
            foreach (var elem in layer.elems)
            {
                mmesh.SetInstanceTransform2D(i, position.Translated(elem.pos).RotatedLocal(elem.dir).ScaledLocal(elem.size));
                i++;
            }
            DrawMultimesh(mmesh, tex);
        }
    }
    public void drawStroke(SketchLayer sl)
    {
        layer = sl;
        QueueRedraw();
    }
    public override void _Process(double delta)
    {
    }
}
