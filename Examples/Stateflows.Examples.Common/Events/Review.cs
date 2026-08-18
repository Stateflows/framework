using System.ComponentModel.DataAnnotations;
using Stateflows.Common;
using Stateflows.Extensions.MinimalAPIs.Attributes;

namespace Stateflows.Examples.Common.Events;

public class BaseReview : IRequest<ReviewResponse>
{
    [MinLength(8)]
    public string Content { get; set; }
    public int Rating { get; set; } = 42;
}

public class Review : BaseReview
{
    public bool Recommended { get; set; }
}

public class ReviewResponse
{
    public string Summary { get; set; } 
}