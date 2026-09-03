using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace KillFeed;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    /// <summary>
    /// Top-left corner of the first notification slot, in main-viewport-relative pixels.
    /// <c>null</c> means "not set yet" and falls back to the bottom-right corner of the screen.
    /// The stack grows away from whichever half of the screen the slot sits in.
    /// </summary>
    public Vector2? Position { get; set; }

    [NonSerialized] private IDalamudPluginInterface? pluginInterface;

    public static Configuration Load(IDalamudPluginInterface pluginInterface)
    {
        var config = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        config.pluginInterface = pluginInterface;
        return config;
    }

    public void Save() => pluginInterface?.SavePluginConfig(this);

    public void ResetPosition()
    {
        Position = null;
        Save();
    }
}
