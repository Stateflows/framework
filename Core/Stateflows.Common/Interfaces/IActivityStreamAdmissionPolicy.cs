using Stateflows.Common.Context;

namespace Stateflows.Common.Interfaces;

/// <summary>Exposes whether a runtime can admit a streamed Activity input for a behavior.</summary>
public interface IActivityStreamAdmissionPolicy
{
    /// <summary>Determines whether an Activity with the supplied committed state can start streamed execution.</summary>
    bool CanAdmitStream(BehaviorId id, StateflowsContext context);

    /// <summary>Checks the entire token batch against the Activity's declared input boundary.</summary>
    bool AcceptsInput(BehaviorId id, StateflowsContext context, TokensInputEvent input);
}
