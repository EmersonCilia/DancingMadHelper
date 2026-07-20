using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace DancingMadHelper.Windows;

public class SettingsWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private readonly MainWindow mainWindow;

    public SettingsWindow(DancingMadHelper plugin, MainWindow mainWindow)
    : base("Dancing Mad Helper Settings")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        this.mainWindow = mainWindow;
        configuration = plugin.Configuration;

        Size = new Vector2(260, 130);
        SizeCondition = ImGuiCond.Always;

    }

    public void Dispose() { }

    public override void PreDraw()
    {
        if (configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        bool pinWindow = mainWindow.Flags.HasFlag(ImGuiWindowFlags.NoMove);

        if (ImGui.Checkbox("Pin window", ref pinWindow))
        {
            if (pinWindow)
                mainWindow.Flags |= ImGuiWindowFlags.NoMove;
            else
                mainWindow.Flags &= ~ImGuiWindowFlags.NoMove;
        }

        bool autoResize = mainWindow.Flags.HasFlag(ImGuiWindowFlags.AlwaysAutoResize);

        if (ImGui.Checkbox("Auto resize", ref autoResize))
        {
            if (autoResize)
                mainWindow.Flags |= ImGuiWindowFlags.AlwaysAutoResize;
            else
                mainWindow.Flags &= ~ImGuiWindowFlags.AlwaysAutoResize;
        }

        ImGui.ColorEdit4(
            "Background",
            ref mainWindow.BackgroundColor,
            ImGuiColorEditFlags.AlphaBar
        );

    }
}
