namespace Stateflows.Tools.Visualizer.DevHost;

public sealed class ActionDefinition(string name, int version)
{
    public string Name { get; } = name;
    public int Version { get; } = version;
    public string? Implementation { get; set; }
}
