using Dalamud.Configuration;
using System;

namespace PopulationTracker;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public uint TrackInterval { get; set; } = 60;
    public uint RecentPlayerTimeout { get; set; } = 600;
    
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
