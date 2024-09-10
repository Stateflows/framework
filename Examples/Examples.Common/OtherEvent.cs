using System.ComponentModel.DataAnnotations;

namespace Examples.Common
{
    public class OtherEvent
    {
        public int AnswerToLifeUniverseAndEverything { get; set; } = 42;

        [Required]
        public string RequiredParameter { get; set; } = "Take a look, I'm value";
    }
}