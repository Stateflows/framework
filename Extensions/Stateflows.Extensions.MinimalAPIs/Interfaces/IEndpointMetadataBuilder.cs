namespace Stateflows.Common;

public interface IEndpointMetadataBuilder
{
    Task BuildMetadataAsync(IDictionary<string, object> metadata);
}