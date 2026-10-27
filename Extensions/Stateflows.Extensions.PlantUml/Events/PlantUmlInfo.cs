using Stateflows.Common;
using Stateflows.Extensions.PlantUml.Classes;

namespace Stateflows.Extensions.PlantUml.Events
{
    /// <summary>Contains PlantUML source and image-server URLs for a registered behavior's diagram.</summary>
    [Event(nameof(PlantUmlInfo)), Retain, StrictOwnership]
    public sealed class PlantUmlInfo
    {
        private string plantUml;

        /// <summary>Gets or sets the PlantUML source; assigning it refreshes the generated image URLs.</summary>
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
        /// <summary>Gets or sets the URL of the PlantUML server's PNG rendering.</summary>
        public string PNGUrl { get; set; }
        /// <summary>Gets or sets the URL of the PlantUML server's SVG rendering.</summary>
        public string SVGUrl { get; set; }

        private string GetUrl(FileType fileType = FileType.PNG)
            => $"http://www.plantuml.com/plantuml/{fileType.ToString().ToLower()}/{PlantUmlTextEncoder.Encode(PlantUml)}";
    }
}
