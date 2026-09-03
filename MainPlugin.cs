using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

#pragma warning disable SeStringEvaluator

namespace KillFeed;

public class MainPlugin : IDalamudPlugin
{
    private const string CommandName = "/killfeed";

    private static DalamudServiceIntermediate<IFramework> framework = null!;
    private static DalamudServiceIntermediate<IPluginLog> logger = null!;
    private static DalamudServiceIntermediate<ICommandManager> commandManager = null!;
    internal static DalamudServiceIntermediate<IDataManager> DataManager = null!;
    private readonly IDalamudPluginInterface pluginInterface;
    internal static DalamudServiceIntermediate<ISeStringEvaluator> SeStringEvaluator = null!;
    internal static DalamudServiceIntermediate<IGameInteropProvider> GameInteropProvider = null!;
    private readonly PacketCapture packetCapture;

    private readonly Configuration config;
    private readonly WindowSystem windowSystem = new("KillFeed");
    private readonly ConfigWindow configWindow;

    public MainPlugin(IDalamudPluginInterface pluginInterface)
    {
        framework = new DalamudServiceIntermediate<IFramework>(pluginInterface);
        // framework.Service.Update += Service_Update;
        this.pluginInterface = pluginInterface;
        logger = new DalamudServiceIntermediate<IPluginLog>(pluginInterface);
        commandManager = new DalamudServiceIntermediate<ICommandManager>(pluginInterface);
        DataManager = new DalamudServiceIntermediate<IDataManager>(pluginInterface);
        SeStringEvaluator = new DalamudServiceIntermediate<ISeStringEvaluator>(pluginInterface);
        GameInteropProvider = new DalamudServiceIntermediate<IGameInteropProvider>(pluginInterface);

        config = Configuration.Load(pluginInterface);
        configWindow = new ConfigWindow(config);
        windowSystem.AddWindow(configWindow);

        pluginInterface.UiBuilder.Draw += Draw;
        pluginInterface.UiBuilder.OpenConfigUi += ToggleConfigWindow;
        pluginInterface.UiBuilder.OpenMainUi += ToggleConfigWindow;

        commandManager.Service.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the KillFeed settings window.",
        });

        packetCapture = new PacketCapture();
    }

    private void OnCommand(string command, string args) => ToggleConfigWindow();

    private void ToggleConfigWindow() => configWindow.Toggle();

    private void Draw()
    {
        NotificationDrawer.Draw(config);
        windowSystem.Draw();
    }

    public void Dispose()
    {
        // framework.Service.Update -= Service_Update;
        pluginInterface.UiBuilder.Draw -= Draw;
        pluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigWindow;
        pluginInterface.UiBuilder.OpenMainUi -= ToggleConfigWindow;
        commandManager.Service.RemoveHandler(CommandName);
        windowSystem.RemoveAllWindows();
        packetCapture.Dispose();
        framework.Dispose();
        logger.Dispose();
        commandManager.Dispose();
        DataManager.Dispose();
        SeStringEvaluator.Dispose();
        GameInteropProvider.Dispose();
    }
}
