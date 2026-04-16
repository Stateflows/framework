using System.ComponentModel.DataAnnotations;
using Stateflows.Common;

namespace Stateflows.Examples.Common.Events;

public class Review : IRequest<ReviewResponse>
{
    [MinLength(8)]
    public string Content { get; set; }
    public int Rating { get; set; } = 42;
}

public class ReviewResponse
{
    public string Summary { get; set; } 
}