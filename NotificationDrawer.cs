using Dalamud.Interface.Utility;
using System.Numerics;

namespace KillFeed;

internal static class NotificationDrawer
{
    private static float DefaultEdgeMargin => MathF.Round(20 * ImGuiHelpers.GlobalScale);

    /// <summary>Direction resolved on the last frame: true when the feed stacks upwards from its first slot.</summary>
    public static bool GrowsUpwards { get; private set; } = true;

    private static unsafe float CalculateNotificationWidth()
    {
        var notificationWidthMeasurementString = "The width of this text will decide the width\\nof the notification window."u8;
        var viewportSize = ImGuiHelpers.MainViewport.WorkSize;
        Vector2 notificationSize;
        fixed (byte* ptr = notificationWidthMeasurementString)
            ImGuiNative.CalcTextSize(&notificationSize, ptr, ptr + notificationWidthMeasurementString.Length, 0, -1);
        var width = notificationSize.X;
        width += NotificationConstants.ScaledWindowPadding * 3;
        return Math.Min(width, viewportSize.X * NotificationConstants.MaxNotificationWindowWidthWrtMainViewportWidth);
    }

    /// <summary>
    /// Resolves the configured top-left of the first slot (falling back to the bottom-right corner)
    /// and clamps it so the slot stays fully on screen.
    /// </summary>
    private static Vector2 ResolveSlotTopLeft(Configuration config, Vector2 viewportSize, float width, float slotHeight)
    {
        var topLeft = config.Position ?? new Vector2(
            viewportSize.X - width - DefaultEdgeMargin,
            viewportSize.Y - slotHeight - DefaultEdgeMargin);

        var clamped = new Vector2(
            Math.Clamp(topLeft.X, 0f, Math.Max(0f, viewportSize.X - width)),
            Math.Clamp(topLeft.Y, 0f, Math.Max(0f, viewportSize.Y - slotHeight)));

        // Persist the clamp only while the user is actively placing the feed, so a temporary
        // resolution change (e.g. windowed mode) doesn't silently rewrite a saved position.
        if (NotificationPreview.Enabled && config.Position is { } stored && stored != clamped)
            config.Position = clamped;

        return clamped;
    }

    public static void Draw(Configuration config)
    {
        var width = CalculateNotificationWidth();
        var slotHeight = PositionHandle.EstimatedHeight;

        while (NotificationManager.PendingNotifications.TryTake(out var notification))
            NotificationManager.ActiveNotifications.Add(notification);

        var now = DateTime.Now;
        NotificationManager.ActiveNotifications.RemoveAll(n => n.Expiry < now);

        var viewport = ImGuiHelpers.MainViewport;
        var viewportSize = viewport.WorkSize;
        var viewportPos = viewport.Pos;

        var unlocked = NotificationPreview.Enabled;
        // Dragging needs a concrete stored position to add deltas to.
        if (unlocked && config.Position is null)
            config.Position = ResolveSlotTopLeft(config, viewportSize, width, slotHeight);

        var slotTopLeft = ResolveSlotTopLeft(config, viewportSize, width, slotHeight);

        // 50/50 split: a slot whose centre is in the lower half of the screen stacks upwards, otherwise downwards.
        GrowsUpwards = slotTopLeft.Y + slotHeight / 2 >= viewportSize.Y / 2;

        // Pin the first slot on the edge nearest the screen edge, so taller notifications grow away from it too.
        var origin = (GrowsUpwards ? slotTopLeft with { Y = slotTopLeft.Y + slotHeight } : slotTopLeft) + viewportPos;
        var pivot = new Vector2(0, GrowsUpwards ? 1 : 0);
        var direction = GrowsUpwards ? -1f : 1f;

        var stackHeight = 0f;
        if (unlocked)
        {
            stackHeight += PositionHandle.Draw(config, slotTopLeft + viewportPos, width);
            stackHeight += NotificationConstants.ScaledWindowGap;
        }

        foreach (var notification in NotificationManager.ActiveNotifications.Concat(NotificationPreview.GetNotifications()))
        {
            var position = origin with { Y = origin.Y + direction * stackHeight };
            stackHeight += notification.Draw(position, pivot, width);
            stackHeight += NotificationConstants.ScaledWindowGap;
        }
    }
}
