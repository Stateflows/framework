using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Context;

namespace Stateflows.Common.Interfaces;

/// <summary>Provides activation-owned Activity execution sessions for stream-capable runtimes.</summary>
public interface IActivityStreamSessionProcessor
{
    Task<EventStatus> ExecuteInActivitySessionAsync(
        BehaviorId id,
        EventHolder eventHolder,
        bool streamed,
        List<Exception> exceptions);

    Task<TimeEvent[]> GetActivitySessionTimeEventsAsync(BehaviorId id);

    /// <summary>
    /// Returns the session-owned mutable context, or null when no session exists. The runtime must not
    /// serialize it while firings are active or retain it as committed state without detaching it.
    /// </summary>
    StateflowsContext? GetActivitySessionContext(BehaviorId id);

    Task<bool?> WaitForActivitySessionInitializationAsync(BehaviorId id);

    bool IsCurrentActivityInvocationStreamed(BehaviorId id);

    Task RequestActivitySessionCheckpointPauseAsync(BehaviorId id);

    Task<bool> ResumeCheckpointedActivityAsync(BehaviorId id);

    Task EndActivitySessionAsync(BehaviorId id, bool persist = true);

    /// <summary>Registers (or removes, when null) the runtime host that runs streamed work handed off by a regular transaction; the host returns false when it cannot accept work.</summary>
    void SetActivitySessionWorkHost(BehaviorId id, Func<Func<Task>, bool> host);
}
