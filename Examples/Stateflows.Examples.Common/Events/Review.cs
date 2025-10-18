using System.ComponentModel.DataAnnotations;

namespace Stateflows.Examples.Common.Events;

public class Review
{
    [MinLength(8)]
    public string Content { get; set; }
    public int Rating { get; set; } = 42;
}