using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiSeStringRenderer;
using Dalamud.Interface.Utility;
using Lumina.Text.ReadOnly;

namespace KillFeed;

internal record ActiveNotification(DateTime CreatedAt, DateTime Expiry, TimeSpan Duration, ReadOnlySeString Content, string Title)
{
    private static long _idCounter;
    private long id = Interlocked.Increment(ref _idCounter);

    /// <summary>
    /// Draws the notification window with the given pivot point placed at <paramref name="position"/> (in absolute screen coordinates).
    /// </summary>
    /// <returns>The height of the drawn window.</returns>
    /// <summary>Pushes the shared window style used by notifications and the position handle. Pair with <see cref="PopWindowStyle"/>.</summary>
    public static unsafe void PushWindowStyle()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.8f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(NotificationConstants.ScaledWindowPadding));
        ImGui.PushStyleColor(ImGuiCol.WindowBg,
            *ImGui.GetStyleColorVec4(ImGuiCol.WindowBg) *
            new Vector4(1, 1, 1, 0.82f));
    }

    public static void PopWindowStyle()
    {
        ImGui.PopStyleColor();
        ImGui.PopStyleVar(3);
    }

    public float Draw(Vector2 position, Vector2 pivot, float width)
    {
        var actionWindowHeight = ImGui.GetTextLineHeight() + NotificationConstants.ScaledWindowPadding * 2;

        PushWindowStyle();

        ImGuiHelpers.ForceNextWindowMainViewport();
        ImGui.SetNextWindowPos(position, ImGuiCond.Always, pivot);
        var size = new Vector2(width, actionWindowHeight);
        ImGui.SetNextWindowSizeConstraints(size, size);
        ImGui.Begin($"##KillFeedNotification{id}",
            ImGuiWindowFlags.AlwaysAutoResize |
            ImGuiWindowFlags.NoDecoration |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoFocusOnAppearing |
            ImGuiWindowFlags.NoDocking |
            ImGuiWindowFlags.NoInputs |
            ImGuiWindowFlags.NoSavedSettings);
        ImGui.PushID(id.GetHashCode());
        DrawWindowBackgroundProgressBar();
        var textColumnWidth = width + NotificationConstants.ScaledWindowPadding;
        var textColumnOffset = new Vector2(NotificationConstants.ScaledWindowPadding, NotificationConstants.ScaledCoponentGap);
        textColumnOffset.Y += DrawTitle(textColumnOffset, textColumnWidth);
        textColumnOffset.Y += NotificationConstants.ScaledCoponentGap;
        DrawContentBody(textColumnOffset, textColumnWidth);
        DrawExpiryBar();
        var windowSize = ImGui.GetWindowSize();
        ImGui.PopID();
        ImGui.End();

        PopWindowStyle();
        return windowSize.Y;
    }

    private void DrawExpiryBar()
    {
        var barL = 1 - (float)((Expiry - DateTime.Now).TotalMilliseconds / Duration.TotalMilliseconds);
        var barR = 1;
        var windowPos = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();
        ImGui.PushClipRect(windowPos, windowPos + windowSize, false);
        ImGui.GetWindowDrawList().AddRectFilled(
            windowPos + new Vector2(
                windowSize.X * barL,
                windowSize.Y - NotificationConstants.ScaledExpiryProgressBarHeight),
            windowPos + windowSize with { X = windowSize.X * barR },
            ImGui.GetColorU32(ImGuiColors.DalamudWhite));
        ImGui.PopClipRect();
    }

    private void DrawContentBody(Vector2 minCoord, float width)
    {
        ImGui.SetCursorPos(minCoord);
        ImGui.PushTextWrapPos(minCoord.X + width);
        ImGuiHelpers.SeStringWrapped(Content, new SeStringDrawParams
        {
            Color = NotificationConstants.BodyTextColor,
            Opacity = 1
        });
        ImGui.PopTextWrapPos();
    }

    private float DrawTitle(Vector2 minCoord, float width)
    {
        ImGui.PushTextWrapPos(minCoord.X + width);

        ImGui.SetCursorPos(minCoord);
        ImGui.PushStyleColor(ImGuiCol.Text, NotificationConstants.TitleTextColor);
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 1);
        ImGui.TextUnformatted(Title);
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();

        ImGui.PopTextWrapPos();
        return ImGui.GetCursorPosY() - minCoord.Y;
    }

    private void DrawWindowBackgroundProgressBar()
    {
        var progress = Math.Clamp((float)((DateTime.Now - CreatedAt).TotalMilliseconds / Duration.TotalMilliseconds), 0f, 1f);

        var elapsed = (float)((DateTime.Now - CreatedAt).TotalMilliseconds %
                              NotificationConstants.ProgressWaveLoopDuration /
                              NotificationConstants.ProgressWaveLoopDuration);
        elapsed /= NotificationConstants.ProgressWaveIdleTimeRatio;

        var colorElapsed = elapsed < NotificationConstants.ProgressWaveLoopMaxColorTimeRatio
            ? elapsed / NotificationConstants.ProgressWaveLoopMaxColorTimeRatio
            : (NotificationConstants.ProgressWaveLoopMaxColorTimeRatio * 2 - elapsed) /
              NotificationConstants.ProgressWaveLoopMaxColorTimeRatio;

        elapsed = Math.Clamp(elapsed, 0f, 1f);
        colorElapsed = Math.Clamp(colorElapsed, 0f, 1f);
        colorElapsed = MathF.Sin(colorElapsed * (MathF.PI / 2f));

        if (progress >= 1f)
            elapsed = colorElapsed = 0f;

        var windowPos = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();
        var rb = windowPos + windowSize;
        var midp = windowPos + windowSize with { X = windowSize.X * progress * elapsed };
        var rp = windowPos + windowSize with { X = windowSize.X * progress };

        ImGui.PushClipRect(windowPos, rb, false);
        ImGui.GetWindowDrawList().AddRectFilled(
            windowPos,
            midp,
            ImGui.GetColorU32(
                Vector4.Lerp(
                    NotificationConstants.BackgroundProgressColorMin,
                    NotificationConstants.BackgroundProgressColorMax,
                    colorElapsed)));
        ImGui.GetWindowDrawList().AddRectFilled(
            midp with { Y = 0 },
            rp,
            ImGui.GetColorU32(NotificationConstants.BackgroundProgressColorMin));
        ImGui.PopClipRect();
    }
}