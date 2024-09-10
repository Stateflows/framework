using Stateflows.Common;

namespace Stateflows.Extensions.PlantUml.Events
{
    [NoTracing]
    [NoImplicitInitialization]
    [Event(nameof(PlantUmlRequest))]
    public sealed class PlantUmlRequest : IRequest<PlantUmlInfo>
    { }
}
