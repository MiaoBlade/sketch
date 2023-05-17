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
        mmesh.UseCustomData=true;
    }
    public override void _Draw()
    {
        if (layer != null)
        {
            Position=layer.pos;
            if (mmesh.InstanceCount < layer.store.capacity)
            {
                mmesh.InstanceCount = layer.store.capacity;
            }
            mmesh.VisibleInstanceCount = layer.store.elemCount;

            mmesh.Buffer = layer.store.buffer;
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
