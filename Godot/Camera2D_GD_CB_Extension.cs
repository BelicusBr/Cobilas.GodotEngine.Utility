using Cobilas.GodotEngine.Utility;
using Cobilas.GodotEngine.Utility.Numerics;

namespace Godot;
/// <summary>Extension for <see cref="Godot.Camera2D"/> class.</summary>
public static class Camera2D_GD_CB_Extension {
    /// <summary>Converts a position on the screen to a position in the 2D world.</summary>
    /// <param name="C">The 2D camera that will be used for conversion.</param>
    /// <param name="mousePosition">The position on the screen that will be converted.</param>
    /// <returns>Return a two-dimensional vector with the result of converting the screen position to a position in the 2D world.</returns>
    public static Vector2D ScreenToWorldPoint(this Camera2D C, Vector2D mousePosition) {
        Rect2 rect;
        Vector2D size = (Vector2D)Screen.CurrentResolution * C.Zoom;
        mousePosition *= C.Zoom;
        if (C.AnchorMode == Camera2D.AnchorModeEnum.DragCenter)
            rect = new Rect2(C.Position - size * .5f, size);
        else rect = new Rect2(C.Position, size);
        return mousePosition + Vector2D.Right * rect.Left() + Vector2D.Down * rect.Top();
    }
    /// <summary>Converts a position in the 2D world to a position on the screen.</summary>
    /// <param name="C">The 2D camera that will be used for conversion.</param>
    /// <param name="position">The position of the 2D world that will be converted.</param>
    /// <returns>Returns a two-dimensional vector with the result of converting the 2D world position to a screen position.</returns>
    public static Vector2D WorldToScreenPoint(this Camera2D C, Vector2D position) {
        Vector2D rect = C.Position;
        if (C.AnchorMode == Camera2D.AnchorModeEnum.DragCenter)
            rect = C.Position - (Vector2D)Screen.CurrentResolution * C.Zoom * .5f;
        return position - rect;
    }
}