using Stateflows.Common;

namespace Stateflows.Extensions.PlantUml.Events
{
    [NoTracing]
    [NoImplicitInitialization]
    [Event(nameof(PlantUmlInfoRequest))]
    public sealed class PlantUmlInfoRequest : IRequest<PlantUmlInfo>
    { }
}
