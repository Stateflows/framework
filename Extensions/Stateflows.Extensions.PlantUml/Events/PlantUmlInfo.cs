using Stateflows.Common;
using Stateflows.Extensions.PlantUml.Classes;

namespace Stateflows.Extensions.PlantUml.Events
{
    [Event(nameof(PlantUmlInfo))]
    public sealed class PlantUmlInfo
    {
        public string PlantUml { get; set; }

        public string GetUrl(FileType fileType = FileType.PNG)
            => $"http://www.plantuml.com/plantuml/{fileType.ToString().ToLower()}/{PlantUmlTextEncoder.Encode(PlantUml)}";
    }
}
