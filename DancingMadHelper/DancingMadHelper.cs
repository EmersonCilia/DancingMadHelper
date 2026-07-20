using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DancingMadHelper.Windows;
namespace DancingMadHelper;

public sealed class DancingMadHelper : IDalamudPlugin
{

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    private const string CommandName = "/dmhelper";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("DancingMadHelper");
    private SettingsWindow SettingsWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public DancingMadHelper()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        MainWindow = new MainWindow(this);
        SettingsWindow = new SettingsWindow(this, MainWindow);

        WindowSystem.AddWindow(SettingsWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "to open dmhelper config"
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        
        WindowSystem.RemoveAllWindows();

        SettingsWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        MainWindow.Toggle();
    }
    
    public void ToggleConfigUi() => SettingsWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}
