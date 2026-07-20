using Dalamud.Configuration;
using System;

namespace DancingMadHelper;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;

    public void Save()
    {
        DancingMadHelper.PluginInterface.SavePluginConfig(this);
    }
}
