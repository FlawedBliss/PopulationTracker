using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace PopulationTracker.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;


    public ConfigWindow(Plugin plugin) : base("Configuration###PTConfigWindow")
    {
        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        ImGui.SetNextWindowSize(new(300, 10), ImGuiCond.FirstUseEver);
    }

    public override void Draw()
    {
        var trackInterval = Configuration.TrackInterval;
        ImGui.SetNextItemWidth(60);
        if(ImGui.InputUInt("Tracking Interval (seconds)", ref trackInterval))
        {
            if (trackInterval < 1) trackInterval = 1;
            Configuration.TrackInterval = trackInterval;
            Configuration.Save();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("How often to snapshot the current player count (seconds)");
        }
        
        var recentPlayerTimeout = Configuration.RecentPlayerTimeout;
        ImGui.SetNextItemWidth(60);
        if(ImGui.InputUInt("Recent Player Timeout (seconds)", ref recentPlayerTimeout))
        {
            if (recentPlayerTimeout < 1) recentPlayerTimeout = 1;
            Configuration.RecentPlayerTimeout = recentPlayerTimeout;
            Configuration.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Time after a player has been last seen until they are no longer considered present");
        }
    }
}
