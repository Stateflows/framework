using Stateflows.Common;

namespace Stateflows.Extensions.PlantUml.Events
{
    [DoNotTrace]
    public sealed class PlantUmlRequest : Request<PlantUmlResponse>
    {
        public override string Name => nameof(PlantUmlRequest);
    }
}
