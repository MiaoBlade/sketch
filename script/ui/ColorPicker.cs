using Godot;
using System;

public partial class ColorPicker : Control
{
    [Export]
    Color color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
    [Export]
    Godot.ColorPicker picker;
    Color color_border = new Color(0.8f, 0.8f, 0.8f, 1.0f);
    Color color_rim = new Color(1.0f, 1.0f, 1.0f, 0.2f);
    public event Action needRedraw;
    public event Godot.ColorPicker.ColorChangedEventHandler colorChange;
    float iconRadius = 15;
    float margin = 10;
    bool isHover = false;

    public float IconRadius
    {
        get { return iconRadius; }
        set
        {
            iconRadius = value;
            calcMinSize();
        }
    }

    public Color Color
    {
        get => color;
        set
        {
            color = value;
            if (picker != null)
            {
                picker.Color = value;
            }
            QueueRedraw();
        }
    }

    void calcMinSize()
    {
        CustomMinimumSize = Vector2.One * iconRadius * 2.2f + Vector2.Right * margin * 2;
    }
    public override void _Ready()
    {
        calcMinSize();
        if (picker != null)
        {
            picker.ColorChanged += pickerColorChange;
        }
        else
        {
            GD.PushWarning("Color Button dont have picker assigned.");
        }
    }

    private void pickerColorChange(Color color)
    {
        this.Color = color;
        colorChange.Invoke(color);
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mbe && mbe.ButtonIndex == MouseButton.Left && mbe.Pressed)
        {
            if (picker == null)
            {
                return;
            }
            if (picker.Visible)
            {
                picker.Visible = false;
            }
            else
            {
                picker.Position = GetScreenPosition() + (new Vector2(0, -picker.Size.Y));
                picker.Visible = true;
                picker.Color = Color;
            }
            needRedraw.Invoke();
            AcceptEvent();
        }
    }
    public override void _Notification(int what)
    {
        switch ((long)what)
        {
            case NotificationMouseEnter:
                isHover = true;
                QueueRedraw();
                break;

            case NotificationMouseExit:
                isHover = false;
                QueueRedraw();
                break;

            case NotificationDraw:
                var r = isHover ? iconRadius * 1.1f : iconRadius;
                DrawCircle(Size / 2, r, Color);
                // if (isHover)
                // {
                //     DrawArc(Size / 2, r - 1, 0, MathF.Tau, 12, color_rim, 4, true);
                // }
                DrawArc(Size / 2, r, 0, MathF.Tau, 16, color_border, 1, true);
                needRedraw.Invoke();
                break;

            case NotificationResized:
                QueueRedraw();
                break;
        }
    }
}
