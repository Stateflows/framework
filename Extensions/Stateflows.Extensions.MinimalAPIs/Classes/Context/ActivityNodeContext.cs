using Stateflows.Activities;
using IActivityNodeContext = Stateflows.Extensions.MinimalAPIs.Interfaces.IActivityNodeContext;

namespace Stateflows.Extensions.MinimalAPIs;

public class ActivityNodeContext : IActivityNodeContext
{
    public string Name { get; internal set; }
    public NodeType NodeType { get; internal set; }
}