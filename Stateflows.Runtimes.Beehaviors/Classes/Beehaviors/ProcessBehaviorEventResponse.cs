using Stateflows.Common;

namespace Stateflows;

internal sealed class ProcessBehaviorEventResponse(
    EventStatus status,
    EventValidation validation,
    EventHolder? response
)
{
    public EventStatus Status { get; } = status;

    public EventValidation Validation { get; } = validation;

    public EventHolder? Response { get; } = response;
}
