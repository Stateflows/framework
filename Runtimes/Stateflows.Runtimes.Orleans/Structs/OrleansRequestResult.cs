using Stateflows.Common;

namespace Stateflows;

[GenerateSerializer]
[Alias("Stateflows.OrleansRequestResult")]
public struct OrleansRequestResult
{
    [Id(0)]
    public OrleansEventHolder? Response { get; set; }

    [Id(1)]
    public EventStatus Status { get; set; }
        
    [Id(2)]
    public OrleansEventValidation Validation { get; set; }
    
    public static implicit operator RequestResult(OrleansRequestResult orleansRequestResult)
        => new RequestResult(
            orleansRequestResult.Response,
            orleansRequestResult.Status,
            orleansRequestResult.Validation
        );

    public static implicit operator OrleansRequestResult(RequestResult requestResult)
        => new OrleansRequestResult()
        {
            Response = requestResult.Response == null
                ? (OrleansEventHolder?)null
                : (OrleansEventHolder)requestResult.Response,
            Status = requestResult.Status,
            Validation = requestResult.Validation,
        };
}