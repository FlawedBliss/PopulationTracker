using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using PopulationTracker.Windows;

namespace PopulationTracker;

public class PopulationTrackerService : IDisposable
{
    private readonly uint MAX_HISTORY_LEN = 10000;
    private readonly IFramework _framework;
    private readonly IObjectTable _objectTable;
    private readonly Plugin _plugin;
    private Stopwatch _stopwatch = new();
    private Dictionary<string, DateTime> _recentPlayers = new();
    private bool _enabled = false;
    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            if (_enabled)
            {
                _stopwatch.Restart();
                TrackingStart = DateTime.Now;
            }
        }
    }

    public List<Pair<DateTime, int>> PopulationHistory = new();
    public HashSet<string> UniquePlayers = new();
    public DateTime TrackingStart
    {
        get;
        private set;
    }

    public PopulationTrackerService(
        Plugin plugin, IFramework framework, IObjectTable objectTable)
    {
        _framework = framework;
        _objectTable = objectTable;
        _plugin = plugin;
        
        PopulationHistory.Add(new Pair<DateTime, int> { Key = DateTime.Now, Value = 0 });
        
        framework.Update += OnFrameworkUpdate;
    }

    public void Dispose()
    {
        _framework.Update -= OnFrameworkUpdate;
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (!Enabled) return;
        foreach (var obj in _objectTable.PlayerObjects)
        {
            if (obj is not IPlayerCharacter pc)
                continue;
            _recentPlayers[$"{pc.Name.TextValue}@{pc.HomeWorld.Value.Name.ExtractText()}"] = DateTime.Now;
            UniquePlayers.Add($"{pc.Name.TextValue}@{pc.HomeWorld.Value.Name.ExtractText()}");
        }

        if (_stopwatch.Elapsed > TimeSpan.FromSeconds(_plugin.Configuration.TrackInterval))
        {
            RemoveStalePlayers();
            PopulationHistory.Add(new Pair<DateTime, int>()
            {
                Key = DateTime.Now,
                Value = _recentPlayers.Count
            });
            Plugin.Log.Information("Added entry: {0} players", _recentPlayers.Count);
            if (PopulationHistory.Count > MAX_HISTORY_LEN)
                PopulationHistory.RemoveAt(0);
            _stopwatch.Restart();
        }
    }

    private void RemoveStalePlayers()
    {
        var cutoff = DateTime.Now - TimeSpan.FromSeconds(_plugin.Configuration.RecentPlayerTimeout);
        var old = _recentPlayers.Count;
        _recentPlayers = _recentPlayers.Where(pair => pair.Value >= cutoff)
                                       .ToDictionary(pair => pair.Key, pair => pair.Value);
        Plugin.Log.Debug("Removed {0} stale players", old - _recentPlayers.Count);
    }
}
