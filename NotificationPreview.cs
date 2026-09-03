namespace KillFeed;

/// <summary>
/// Keeps a few sample notifications alive while the feed position is unlocked,
/// so it can be placed without waiting for a real kill.
/// </summary>
internal static class NotificationPreview
{
    private const int PreviewCount = 2;
    private static readonly TimeSpan PreviewDuration = TimeSpan.FromSeconds(6);

    private static readonly List<ActiveNotification> Items = [];

    /// <summary>While true the feed position is unlocked: previews are shown and the drag handle is drawn.</summary>
    public static bool Enabled { get; set; }

    public static IReadOnlyList<ActiveNotification> GetNotifications()
    {
        if (!Enabled)
        {
            Items.Clear();
            return Items;
        }

        var now = DateTime.Now;
        Items.RemoveAll(n => n.Expiry < now);
        while (Items.Count < PreviewCount)
            Items.Add(new Notification(PreviewDuration, "Preview: Warrior of Light WAR was killed by Striking Dummy!"u8, "Kill feed (preview)").ToActiveNotification);

        return Items;
    }
}
