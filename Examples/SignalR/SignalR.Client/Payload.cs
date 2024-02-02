using System.ComponentModel.DataAnnotations;

namespace SignalR.Client
{
    public class Payload
    {
        [Required]
        public string lorem { get; set; }
    }
}