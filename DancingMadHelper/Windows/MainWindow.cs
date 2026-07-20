using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Windowing;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DancingMadHelper.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly DancingMadHelper plugin;

    public class DebuffInfo
    {
        public bool? IsFake { get; set; }
        public bool IsLong { get; set; }
    }
    private readonly Dictionary<uint, DebuffInfo> debuffStates = new();
    private readonly Dictionary<uint, uint> relatedDebuffs = new()
{
    { 5544, 5543 },
    { 5545, 5543 },
    { 5546, 5543 }

};
    public Vector4 BackgroundColor = new(0f, 0f, 0f, 0.5f);

    public void ResetDebuffs()
    {
        debuffStates.Clear();
    }
    public MainWindow(DancingMadHelper plugin)
     : base("Dancing Mad Helper",
       ImGuiWindowFlags.NoScrollbar
     | ImGuiWindowFlags.NoScrollWithMouse
     | ImGuiWindowFlags.NoMove
         | ImGuiWindowFlags.AlwaysAutoResize)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 50),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }
    private void DrawDebuffIcon(uint statusId, ExcelSheet<Status> statusSheet)
    {
        if (!statusSheet.TryGetRow(statusId, out var statusRow))
            return;

        var texture = DancingMadHelper.TextureProvider
            .GetFromGameIcon(new GameIconLookup(statusRow.Icon))
            .GetWrapOrEmpty();

        ImGui.Image(texture.Handle, new Vector2(30, 40));
    }
    private void DrawDebuff(uint statusId, ExcelSheet<Status> statusSheet)
    {
        if (!statusSheet.TryGetRow(statusId, out var statusRow))
            return;

        var texture = DancingMadHelper.TextureProvider
            .GetFromGameIcon(new GameIconLookup(statusRow.Icon))
            .GetWrapOrEmpty();


        ImGui.BeginGroup();
        ImGui.Text(debuffStates[statusId].IsLong ? "LONG  " : "SHORT ");
        ImGui.SameLine();

        ImGui.Image(texture.Handle, new Vector2(30, 40));

        ImGui.SameLine();


        if (debuffStates[statusId].IsFake == null)
        {
            if (ImGui.Button($"FAKE##{statusId}"))
                debuffStates[statusId].IsFake = true;

            ImGui.SameLine();

            if (ImGui.Button($"TRUE##{statusId}"))
                debuffStates[statusId].IsFake = false;
        }
        else
        {

            ImGui.SameLine();
            ImGui.Text(debuffStates[statusId].IsFake == true ? " FAKE  " : " TRUE  ");

            
            if (relatedDebuffs.TryGetValue(statusId, out var related))
            {
                ImGui.SameLine();
                DrawDebuffIcon(related, statusSheet);
                ImGui.SameLine();
                ImGui.Text(debuffStates[statusId].IsFake == true ? " FAKE  " : " TRUE  ");

            }
        }


        ImGui.EndGroup();
    }

    public void Dispose() { }

    private readonly uint[] trackedDebuffs =
     [
        5544,
        5545,
        5546,
        5547,
        5548
     ];

    public override void PreDraw()
    {
        ImGui.PushStyleColor(ImGuiCol.WindowBg, BackgroundColor);
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor();
    }
    public override void Draw()
    {

        if (ImGui.Button("Clear"))
        {
            ResetDebuffs();
        }

        var player = DancingMadHelper.ObjectTable
            .FirstOrDefault(x => x.EntityId == DancingMadHelper.PlayerState.EntityId);

        if (player is not IBattleChara battle)
            return;

        var statusSheet = DancingMadHelper.DataManager.GetExcelSheet<Status>();

        ImGui.Separator();


        foreach (var status in battle.StatusList)
        {
            if (!trackedDebuffs.Contains(status.StatusId))
                continue;

            if (!debuffStates.ContainsKey(status.StatusId))
            {
                debuffStates[status.StatusId] = new DebuffInfo
                {
                    IsFake = null,
                    IsLong = status.RemainingTime >= 60f
                };
            }
        }


        foreach (var debuffId in debuffStates.Keys.ToList())
        {
            DrawDebuff(debuffId, statusSheet);
        }
    }
}
