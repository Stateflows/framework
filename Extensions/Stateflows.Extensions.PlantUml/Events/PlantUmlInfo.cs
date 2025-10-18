using Stateflows.Common;
using Stateflows.Extensions.PlantUml.Classes;

namespace Stateflows.Extensions.PlantUml.Events
{
    [Event(nameof(PlantUmlInfo)), Retain, StrictOwnership]
    public sealed class PlantUmlInfo
    {
        private string plantUml;

        public string PlantUml
        {
            get
            {
                return plantUml;
            }

            set
            {
                plantUml = value;
                
                PNGUrl = GetUrl(FileType.PNG);
                SVGUrl = GetUrl(FileType.SVG);
            }
        }
        public string PNGUrl { get; set; }
        public string SVGUrl { get; set; }

        private string GetUrl(FileType fileType = FileType.PNG)
            => $"http://www.plantuml.com/plantuml/{fileType.ToString().ToLower()}/{PlantUmlTextEncoder.Encode(PlantUml)}";
    }
}
