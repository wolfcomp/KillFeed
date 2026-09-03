using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

namespace KillFeed;

public class ConfigWindow : Window
{
    private readonly Configuration config;

    public ConfigWindow(Configuration config) : base("KillFeed Settings###KillFeedConfig")
    {
        this.config = config;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(360, 200),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
        Size = new Vector2(400, 240);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void OnClose() => Lock();

    public override void Draw()
    {
        ImGui.TextUnformatted("Feed position");
        ImGui.Separator();

        var unlocked = NotificationPreview.Enabled;
        if (ImGui.Checkbox("Unlock position", ref unlocked))
        {
            if (unlocked) NotificationPreview.Enabled = true;
            else Lock();
        }
        ImGui.TextColoredWrapped(ImGuiColors.DalamudGrey,
            unlocked
                ? "Drag the highlighted notification on screen to move the feed. Lock again (or close this window) when you're done."
                : "Unlock to show preview notifications and drag the feed where you want it.");

        ImGuiHelpers.ScaledDummy(6f);

        ImGui.TextUnformatted("Stack direction:");
        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.DalamudGrey, NotificationDrawer.GrowsUpwards ? "upwards" : "downwards");
        Tooltip("Chosen automatically: a feed in the lower half of the screen stacks upwards, in the upper half it stacks downwards.");

        ImGuiHelpers.ScaledDummy(6f);

        if (ImGui.Button("Reset position"))
            config.ResetPosition();
        Tooltip("Moves the feed back to the bottom-right corner.");

        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.DalamudGrey,
            config.Position is { } pos ? $"({pos.X:0}, {pos.Y:0})" : "Default (bottom right)");
    }

    private void Lock()
    {
        if (!NotificationPreview.Enabled) return;
        NotificationPreview.Enabled = false;
        config.Save();
    }

    private static void Tooltip(string text)
    {
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(text);
    }
}
