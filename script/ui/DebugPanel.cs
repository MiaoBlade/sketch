using Godot;
using System;

public partial class DebugPanel : Container
{
    float margin = 10f;
    float spacing = 5f;
    float childHeight = 50f;

    Color bg = Color.Color8(100, 100, 100, 255);
    public event Action needRedraw;
    public override void _Ready()
    {
        foreach (Control c in GetChildren())
        {
            c.Draw += emitRedraw;
        }
    }
    void emitRedraw()
    {
        needRedraw?.Invoke();
    }
    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), bg);
        emitRedraw();
    }
    public override void _Notification(int what)
    {
        if (what == NotificationSortChildren)
        {
            var rect = new Rect2(new Vector2(margin, margin), new Vector2(Size.X - margin * 2, childHeight));
            var dy = new Vector2(0, spacing + childHeight);
            // Must re-sort the children
            foreach (Control c in GetChildren())
            {
                Vector2 cize = c.Size;
                cize.X = Size.X - margin * 2;
                rect.Size = cize;
                dy.Y = spacing + cize.Y;

                FitChildInRect(c, rect);
                rect.Position = rect.Position + dy;
            }
            emitRedraw();
        }
        else if (what == NotificationVisibilityChanged)
        {
            emitRedraw();
        }
    }
}
