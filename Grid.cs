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
                case GridType.Hexgon:
                    drawHexgon();
                    break;
                default:
                    return;
            }
        }
    }
    void drawSquare()
    {
        Vector2 vp_size = GetViewportRect().Size;
        var realScale = Mathf.Pow(2, layer.scaleLevel);
        var d = layer.gdim * realScale;
        var pos = layer.pos;

        var gid_f = -pos / d;
        var x_g = Mathf.FloorToInt(gid_f.X) * d + pos.X;
        var y_g = Mathf.FloorToInt(gid_f.Y) * d + pos.Y;

        Vector2 from, to;

        //draw row
        from.X = 0;
        from.Y = y_g;
        to.X = vp_size.X;
        to.Y = y_g;
        while (from.Y < vp_size.Y)
        {
            DrawLine(from, to, drawColor);
            from.Y += d;
            to.Y += d;
        }
        //draw colomn
        from.X = x_g;
        from.Y = 0;
        to.X = x_g;
        to.Y = vp_size.Y;
        while (from.X < vp_size.X)
        {
            DrawLine(from, to, drawColor);
            from.X += d;
            to.X += d;
        }
    }
    void drawHexgon()
    {
        Rect2 vp_rect = GetViewportRect();
        var realScale = Mathf.Pow(2, layer.scaleLevel);
        var d = layer.gdim * realScale;
        var dx = layer.gdim * 3 * realScale;
        var dy = layer.gdim * Mathf.Sqrt(3) * realScale;
        var pos = layer.pos;

        var gid_f = -pos;
        var x_g = Mathf.FloorToInt(gid_f.X / dx) * dx + pos.X;
        var y_g = Mathf.FloorToInt(gid_f.Y / dy) * dy + pos.Y;

        Vector2[] pts = new Vector2[14];

        pts[0] = new Vector2(0, dy / 2);
        pts[1] = new Vector2(d / 2, dy / 2);

        pts[2] = new Vector2(d / 2, dy / 2);
        pts[3] = new Vector2(d, 0);

        pts[4] = new Vector2(d, 0);
        pts[5] = new Vector2(d * 2, 0);

        pts[6] = new Vector2(d * 2, 0);
        pts[7] = new Vector2(d * 2.5f, dy / 2);

        pts[8] = new Vector2(d * 2.5f, dy / 2);
        pts[9] = new Vector2(d * 3, dy / 2);

        pts[10] = new Vector2(d / 2, dy / 2);
        pts[11] = new Vector2(d, dy);

        pts[12] = new Vector2(d * 2, dy);
        pts[13] = new Vector2(d * 2.5f, dy / 2);


        var y_s = y_g;
        while (x_g < vp_rect.Size.X)
        {
            while (y_g < vp_rect.Size.Y)
            {
                DrawSetTransform(new Vector2(x_g, y_g));

                DrawMultiline(pts, drawColor);
                y_g += dy;
            }
            y_g = y_s;
            x_g += dx;
        }
        DrawSetTransform(Vector2.Zero);
    }
    public void drawGrid(SketchLayer sl)
    {
        layer = sl;
        QueueRedraw();
    }
}
