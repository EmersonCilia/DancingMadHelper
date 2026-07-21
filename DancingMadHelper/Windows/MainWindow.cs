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
    private readonly Dictionary<uint, uint[]> relatedDebuffs = new()
{
    { 5544, [] },
    { 5545, [] },
    { 5546, [] },
};
    private readonly List<uint> castOrder = new();
    public Vector4 BackgroundColor = new(0f, 0f, 0f, 0.5f);



    //function to reset debuff states on button click
    //--------------------------------------------------------------------------------
    public void ResetDebuffs()
    {
        debuffStates.Clear();
        castOrder.Clear();
    }

    // Window constructor
    //--------------------------------------------------------------------------------

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
    //--------------------------------------------------------------------------------


    private void DrawDebuffIcon(uint statusId, ExcelSheet<Status> statusSheet, bool isFake = false)
    {
        if (isFake)
        {
            statusId = statusId switch
            {
                5543 => 4010,
                _ => statusId
            };
        }

        if (!statusSheet.TryGetRow(statusId, out var statusRow))
            return;

        var texture = DancingMadHelper.TextureProvider
            .GetFromGameIcon(new GameIconLookup(statusRow.Icon))
            .GetWrapOrEmpty();

        ImGui.Image(texture.Handle, new Vector2(30, 40));
    }


    private void DrawDebuff(uint statusId, ExcelSheet<Status> statusSheet)
    {
        uint displayStatusId = statusId;

        if (debuffStates[statusId].IsFake == true)
        {
            displayStatusId = statusId switch
            {
                5544 => 5545,
                5545 => 5544,
                5547 => 5548,
                5548 => 5547,
                5546 => 4189,
                5543 => 4010,
                _ => statusId
            };
        }

        if (!statusSheet.TryGetRow(displayStatusId, out var statusRow))
            return;

        var texture = DancingMadHelper.TextureProvider
            .GetFromGameIcon(new GameIconLookup(statusRow.Icon))
            .GetWrapOrEmpty();


        ImGui.BeginGroup();

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

            if (relatedDebuffs.TryGetValue(statusId, out var related))
            {
                foreach (var relatedStatus in related)
                {
                    if (relatedStatus == 5543)
                    {
                        ImGui.NewLine();
                    }
                    else
                    {
                        ImGui.SameLine();
                    }

                    DrawDebuffIcon(relatedStatus, statusSheet);
                }
            }
        }


        ImGui.EndGroup();
    }

    public void Dispose() { }



    //Draw the window with background
    //--------------------------------------------------------------------------------
    public override void PreDraw()
    {
        ImGui.PushStyleColor(ImGuiCol.WindowBg, BackgroundColor);
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor();
    }
    //--------------------------------------------------------------------------------




    // Draw a category rows of debuffs in the window
    //--------------------------------------------------------------------------------
    private void DrawCategory(HashSet<uint> debuffs, ExcelSheet<Status> statusSheet, bool isLong)
    {
        bool hasSpread = debuffs.Contains(5544);
        bool hasRealStack = debuffs.Contains(5545);

        bool drewFirstRow = false;

        if (!hasSpread && !hasRealStack)
        {
            DrawDebuffIcon(5545, statusSheet);
            drewFirstRow = true;
        }

        foreach (var debuff in new uint[] { 5544, 5545, 5546 })
        {
            if (!debuffs.Contains(debuff))
                continue;

            if (drewFirstRow)
                ImGui.SameLine();

            DrawDebuff(debuff, statusSheet);
            drewFirstRow = true;
        }


        // row 2 (gaze)
        if (drewFirstRow)
        {
            bool gazeFake = false;

            int castIndex = isLong ? 1 : 0;

            if (castOrder.Count > castIndex)
            {
                var cast = castOrder[castIndex];

                gazeFake = debuffStates[cast].IsFake == true;
            }

            DrawDebuffIcon(5543, statusSheet, gazeFake);
        }


        // Row 3
        bool drewThirdRow = false;

        foreach (var debuff in new uint[] { 5547, 5548 })
        {
            if (!debuffs.Contains(debuff))
                continue;

            if (drewThirdRow)
                ImGui.SameLine();

            DrawDebuff(debuff, statusSheet);
            drewThirdRow = true;
        }
    }
    //--------------------------------------------------------------------------------




    // Draw the main window with debuffs and buttons
    //--------------------------------------------------------------------------------

    private readonly uint[] trackedDebuffs =
     [
        5544,
            5545,
            5546,
            5547,
            5548
     ];

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

                if (status.StatusId is 5544 or 5545 or 5546)
                {
                    castOrder.Add(status.StatusId);
                }

            }
        }


        if (debuffStates.Count < 4)
        {
            foreach (var debuffId in debuffStates.Keys)
            {
                DrawDebuff(debuffId, statusSheet);
            }

            return;
        }

        var shortDebuffs = debuffStates
            .Where(x => !x.Value.IsLong)
            .Select(x => x.Key)
            .ToHashSet();

        var longDebuffs = debuffStates
            .Where(x => x.Value.IsLong)
            .Select(x => x.Key)
            .ToHashSet();

        DrawCategory(shortDebuffs, statusSheet, false);

        if (shortDebuffs.Count > 0 && longDebuffs.Count > 0)
            ImGui.Separator();

        DrawCategory(longDebuffs, statusSheet, true);
    }
}
