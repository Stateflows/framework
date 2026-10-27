using Stateflows.Common;

namespace Stateflows.Extensions.PlantUml.Events
{
    /// <summary>Requests the current PlantUML source and rendering links for a behavior.</summary>
    [NoTracing]
    [NoImplicitInitialization]
    [Event(nameof(PlantUmlInfoRequest))]
    public sealed class PlantUmlInfoRequest : IRequest<PlantUmlInfo>
    { }
}
