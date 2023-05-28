using Godot;
using System;

public partial class ColorPicker : Control
{
    [Export]
    public Color color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
    Color color_border = new Color(0.8f, 0.8f, 0.8f, 1.0f);
    Color color_rim = new Color(1.0f, 1.0f, 1.0f, 0.2f);
    public event Action needRedraw;
    public event Godot.ColorPicker.ColorChangedEventHandler colorChange;
    float iconRadius = 15;
    float margin = 10;
    bool isHover = false;
    [Export]
    public Godot.ColorPicker picker;
    public float IconRadius
    {
        get { return iconRadius; }
        set
        {
            iconRadius = value;
            calcMinSize();
        }
    }
    void calcMinSize()
    {
        CustomMinimumSize = Vector2.One * iconRadius * 2.2f + Vector2.Right * margin * 2;
    }
    public override void _Ready()
    {
        calcMinSize();
        picker.ColorChanged += pickerColorChange;
    }

    private void pickerColorChange(Color color)
    {
        this.color = color;
        colorChange.Invoke(color);
        QueueRedraw();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mbe && mbe.ButtonIndex == MouseButton.Left && mbe.Pressed)
        {
            if (picker.Visible)
            {
                picker.Visible = false;
            }
            else
            {
                picker.Position = GetScreenPosition() + (new Vector2(0, -picker.Size.Y));
                picker.Visible = true;
                picker.Color = color;
            }
            needRedraw.Invoke();
        }
    }
    public override void _Notification(int what)
    {
        switch ((long)what)
        {
            case NotificationMouseEnter:
                isHover = true;
                QueueRedraw();
                needRedraw.Invoke();
                break;

            case NotificationMouseExit:
                isHover = false;
                QueueRedraw();
                needRedraw.Invoke();
                break;

            case NotificationDraw:
                var r = isHover ? iconRadius * 1.1f : iconRadius;
                DrawCircle(Size / 2, r, color);
                // if (isHover)
                // {
                //     DrawArc(Size / 2, r - 1, 0, MathF.Tau, 12, color_rim, 4, true);
                // }
                DrawArc(Size / 2, r, 0, MathF.Tau, 16, color_border, 1, true);
                break;

            case NotificationResized:
                calcMinSize();
                if (needRedraw != null)
                {
                    needRedraw.Invoke();
                }
                break;
        }
    }
}
