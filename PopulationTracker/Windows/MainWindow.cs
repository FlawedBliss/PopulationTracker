using System;
using System.IO;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImPlot;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Windowing;

namespace PopulationTracker.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin _plugin;
    private readonly PopulationTrackerService _populationTrackerService;
    private readonly FileDialogManager _fileDialogManager = new();

    public MainWindow(Plugin plugin, PopulationTrackerService populationTrackerService)
        : base("PopulationTracker##PTMain")
    {
        this._plugin = plugin;
        this._populationTrackerService = populationTrackerService;
    }

    public void Dispose() { }
    
    public override void PreDraw()
    {
        ImGui.SetNextWindowSize(new(800, 300), ImGuiCond.FirstUseEver);
    }

    public override void Draw()
    {
        ImGui.BeginTable("PopulationTracker##Container", 2);
        ImGui.TableSetupColumn("##Graph", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("##Info", ImGuiTableColumnFlags.WidthFixed, 200.0f);
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        if (ImPlot.BeginPlot("Nearby Players", new Vector2(-1, -1)))
        {
            var a = _populationTrackerService.PopulationHistory.ConvertAll(p => p.Key.ToOADate()).ToArray();
            var b = _populationTrackerService.PopulationHistory.ConvertAll(p => (double)p.Value).ToArray();
            ImPlot.SetupAxes("Time", "Players", ImPlotAxisFlags.NoTickLabels, 0);
            ImPlot.SetupAxisLimits(ImAxis.Y1, 0, 110, ImPlotCond.Always);
            ImPlot.SetupAxisLimits(ImAxis.X1, a[0], a[^1], ImPlotCond.Always);
            ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.2f);
            ImPlot.PlotShaded(
                "Players",
                ref a[0],
                ref b[0],
                _populationTrackerService.PopulationHistory.Count,
                ImPlotShadedFlags.None);
            ImPlot.PlotLine(
                "Players",
                ref a[0],
                ref b[0],
                _populationTrackerService.PopulationHistory.Count,
                ImPlotLineFlags.None);
            ImPlot.EndPlot();
        }

        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"Total Unique Players: {_populationTrackerService.UniquePlayers.Count}");
        ImGui.TextUnformatted(
            $"Total Time Tracked: {(DateTime.Now - _populationTrackerService.TrackingStart).ToString(@"hh\:mm\:ss")}");
        if (_populationTrackerService.Enabled)
        {
            ImGui.TextColored(ImGuiColors.HealerGreen, "Running");
        }
        else
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, "Paused");
        }

        if (ImGui.Button(_populationTrackerService.Enabled ? "Pause" : "Start"))
        {
            _populationTrackerService.Enabled = !_populationTrackerService.Enabled;
        }

        ImGui.SameLine();
        if (ImGui.Button("Config"))
        {
            _plugin.ToggleConfigWindow();
        }

        ImGui.SameLine();
        if (ImGui.Button("Export"))
        {
            _fileDialogManager.SaveFileDialog("Export Population Data",
                                              "",
                                              $"population_{DateTime.Now:MM_dd-HH_mm_ss}.csv",
                                              ".csv",
                                              (success, path) =>
                                              {
                                                  if (!success || string.IsNullOrEmpty(path))
                                                      return;
                                                  using var writer = new StreamWriter(path);
                                                  writer.WriteLine("Timestamp,Population");
                                                  foreach (var entry in _populationTrackerService.PopulationHistory)
                                                  {
                                                      writer.WriteLine($"{entry.Key:HH:mm:ss},{entry.Value}");
                                                  }
                                              });
        }

        _fileDialogManager.Draw();
        ImGui.EndTable();
    }
}
