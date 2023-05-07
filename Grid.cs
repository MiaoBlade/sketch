using Godot;
using System;
public partial class Grid : Node2D
{
    Color drawColor = Color.Color8(200, 200, 200);
    SketchLayer layer;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public override void _Draw()
    {
        if (layer != null)
        {
            switch (layer.gtype)
            {
                case GridType.None:
                    return;
                case GridType.Square:
                    drawSquare();
                    break;
                default:
                    return;
            }
        }
    }
    void drawSquare()
    {
        Rect2 vp_rect = GetViewportRect();

        //map layer  coords to viewport coords
        var d = layer.gdim;
        var pos = layer.pos;

        var gid_f = (Vector2.Zero - pos) / d;
        var x_g = Mathf.CeilToInt(gid_f.X) * d + pos.X;
        var y_g = Mathf.CeilToInt(gid_f.Y) * d + pos.Y;

        Vector2 from, to;

        //draw row
        from.X = 0;
        from.Y = y_g;
        to.X = vp_rect.Size.X;
        to.Y = y_g;
        while (from.Y < vp_rect.Size.Y)
        {
            DrawLine(from, to, drawColor);
            from.Y += d;
            to.Y += d;
        }
        //draw colomn
        from.X = x_g;
        from.Y = 0;
        to.X = x_g;
        to.Y = vp_rect.Size.Y;
        while (from.X < vp_rect.Size.X)
        {
            DrawLine(from, to, drawColor);
            from.X += d;
            to.X += d;
        }


    }
    public void drawGrid(SketchLayer sl)
    {
        layer = sl;
        QueueRedraw();
    }
}
