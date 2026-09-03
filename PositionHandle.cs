using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;

namespace KillFeed;

/// <summary>
/// A notification-shaped window that occupies the first slot of the feed while the position is unlocked.
/// Dragging it moves the feed origin and persists it to the configuration.
/// </summary>
internal static class PositionHandle
{
    private static readonly Vector4 HighlightColor = new(0.3f, 0.6f, 1f, 1f);

    private static bool dragging;

    /// <summary>Approximate height of a single-line notification, used to keep the handle on screen.</summary>
    public static float EstimatedHeight =>
        ImGui.GetTextLineHeight() * 2 + NotificationConstants.ScaledCoponentGap * 2 + NotificationConstants.ScaledWindowPadding * 2;

    /// <summary>
    /// Draws the handle with its top-left corner at <paramref name="topLeft"/> (absolute screen coordinates).
    /// </summary>
    /// <returns>The height of the drawn window.</returns>
    public static float Draw(Configuration config, Vector2 topLeft, float width)
    {
        ActiveNotification.PushWindowStyle();
        ImGui.PushStyleColor(ImGuiCol.Border, HighlightColor);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, MathF.Round(2 * ImGuiHelpers.GlobalScale));

        ImGuiHelpers.ForceNextWindowMainViewport();
        ImGui.SetNextWindowPos(topLeft, ImGuiCond.Always);
        var size = new Vector2(width, EstimatedHeight);
        ImGui.SetNextWindowSizeConstraints(size, size);
        ImGui.Begin("##KillFeedPositionHandle",
            ImGuiWindowFlags.AlwaysAutoResize |
            ImGuiWindowFlags.NoDecoration |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoDocking |
            ImGuiWindowFlags.NoScrollWithMouse |
            ImGuiWindowFlags.NoSavedSettings);

        var textOffset = new Vector2(NotificationConstants.ScaledWindowPadding, NotificationConstants.ScaledCoponentGap);
        ImGui.SetCursorPos(textOffset);
        ImGui.TextColored(HighlightColor, "Kill feed position");
        ImGui.SetCursorPosX(textOffset.X);
        ImGui.TextColored(ImGuiColors.DalamudWhite, dragging
            ? (NotificationDrawer.GrowsUpwards ? "Release to place (stacks upwards)" : "Release to place (stacks downwards)")
            : "Drag me to move the feed");

        HandleDrag(config);

        var windowSize = ImGui.GetWindowSize();
        ImGui.End();

        ImGui.PopStyleVar();
        ImGui.PopStyleColor();
        ActiveNotification.PopWindowStyle();
        return windowSize.Y;
    }

    private static void HandleDrag(Configuration config)
    {
        var hovered = ImGui.IsWindowHovered();
        if (hovered || dragging)
            ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeAll);

        if (!dragging && hovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            dragging = true;

        if (!dragging) return;

        if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            var delta = ImGui.GetIO().MouseDelta;
            if (delta != Vector2.Zero && config.Position is { } current)
                config.Position = current + delta;
        }
        else
        {
            dragging = false;
            config.Save();
        }
    }
}
