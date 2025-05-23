using Stateflows.Extensions.MinimalAPIs.Interfaces;

namespace Stateflows.Extensions.MinimalAPIs;

public class ActivityNodeContext : IActivityNodeContext
{
    public string Name { get; internal set; }
}