

namespace Sketchpad
{
    public enum GridType
    {
        Square, None, Refresh, Hexgon
    }
    public enum PadState
    {
        Drag,
        Draw,
        Idle
    }
    public enum DrawMode
    {
        Pen,
        Erase
    }
    public enum EventType
    {
        Color,
        Layer,
        Stats,
        Zoom,
        Grid,
        Debug,
        Stroke,
        Drag
    }
    public delegate void SketchPadUpdate(EventType t);
}