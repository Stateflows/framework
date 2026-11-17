using System;
using System.Threading.Tasks;

namespace Stateflows.Common;

/// <summary>Internal dispatch bridge used by framework-provided behavior handles.</summary>
public interface IBehaviorDispatchObserver
{
    /// <summary>Routes an awaited send through the active behavior observation scope, if any.</summary>
    /// <param name="targetBehaviorId">The intended recipient instance.</param>
    /// <param name="eventHolder">The event envelope being sent.</param>
    /// <param name="next">The actual send operation.</param>
    /// <returns>The unchanged send result after observation completes.</returns>
    Task<SendResult> SendAsync(BehaviorId targetBehaviorId, EventHolder eventHolder, Func<Task<SendResult>> next);

    /// <summary>Routes an awaited request through the active behavior observation scope, if any.</summary>
    /// <typeparam name="TResponse">The expected response payload type.</typeparam>
    /// <param name="targetBehaviorId">The intended recipient instance.</param>
    /// <param name="eventHolder">The request envelope being sent.</param>
    /// <param name="next">The actual request operation.</param>
    /// <returns>The unchanged response and status after observation completes.</returns>
    Task<RequestResult<TResponse>> RequestAsync<TResponse>(BehaviorId targetBehaviorId, EventHolder eventHolder, Func<Task<RequestResult<TResponse>>> next);

    /// <summary>Observes the synchronous dispatch of a previously committed staged send.</summary>
    /// <param name="sourceBehaviorId">The behavior that staged the send.</param>
    /// <param name="targetBehaviorId">The intended recipient instance.</param>
    /// <param name="eventHolder">The committed staged event envelope.</param>
    /// <param name="serviceProvider">The provider used to create a scope for observation services.</param>
    /// <param name="next">The synchronous dispatch operation, not eventual target processing.</param>
    void DispatchCommitted(BehaviorId sourceBehaviorId, BehaviorId targetBehaviorId, EventHolder eventHolder, IServiceProvider serviceProvider, Action next);
}
