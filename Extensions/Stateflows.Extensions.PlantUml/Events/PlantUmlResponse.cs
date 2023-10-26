using Stateflows.Common;

namespace Stateflows.Extensions.PlantUml.Events
{
    public sealed class PlantUmlResponse : Response
    {
        public override string EventName => nameof(PlantUmlResponse);

        public string PlantUml { get; set; }

        public string PlantUmlUrl { get; set; }
    }
}
