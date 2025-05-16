using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using SamplePlugin.Data;
using SamplePlugin.Windows;
using System;
using System.IO;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static WeatherManager WeatherManager { get; private set; }


    private const string WeatherCommand = "/weather";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Weather Plugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
        
        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        // Loads the Regions and Zones from the game data
        try {
            RegionLoader regionLoader = new RegionLoader();
            var regions = regionLoader.BuildRegionToZoneMap();

            foreach (var region in regions)
            {
                Log.Information($"Region: {region.Key}");
                foreach (var zone in region.Value)
                {
                    Log.Information($"  Zone: {zone}");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to load regions: {ex.Message}");
        }

        // Setup the command manager to listen for our custom command
        CommandManager.AddHandler(WeatherCommand, new CommandInfo(OnCommand)
        {
            HelpMessage = "Pick the weather you want to be notified for"
        });


        // Setup the plugin interface to listen for events
        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [SamplePlugin] ===A cool log message from Sample Plugin===
        Log.Information("Plugin loaded successfully.");
        Log.Information($"{PluginInterface.Manifest.Name}");
        Log.Information($"Version: {PluginInterface.Manifest.Description}");
        
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(WeatherCommand);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
