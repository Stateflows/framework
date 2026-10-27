using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Context;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Defines persistence for behavior execution contexts.</summary>
    public interface IStateflowsStorage : IDisposable
    {
        /// <summary>Loads the persisted context for a behavior.</summary>
        /// <param name="behaviorId">The behavior whose context is loaded.</param>
        /// <returns>The behavior context associated with the requested identifier.</returns>
        Task<StateflowsContext> HydrateAsync(BehaviorId behaviorId);
        /// <summary>Persists the current state of a behavior context.</summary>
        /// <param name="context">The context to persist.</param>
        Task DehydrateAsync(StateflowsContext context);
        /// <summary>Retrieves contexts for the specified behavior classes.</summary>
        /// <param name="behaviorClasses">The behavior classes to include.</param>
        /// <returns>The matching persisted contexts.</returns>
        Task<IEnumerable<StateflowsContext>> GetAllContextsAsync(IEnumerable<BehaviorClass> behaviorClasses);
        /// <summary>Retrieves identifiers for contexts belonging to the specified behavior classes.</summary>
        /// <param name="behaviorClasses">The behavior classes to include.</param>
        /// <returns>The matching behavior identifiers.</returns>
        Task<IEnumerable<BehaviorId>> GetAllContextIdsAsync(IEnumerable<BehaviorClass> behaviorClasses);
        /// <summary>Retrieves contexts that are due for time-triggered execution.</summary>
        /// <param name="behaviorClasses">The behavior classes to include.</param>
        /// <returns>The contexts scheduled for time-triggered execution.</returns>
        Task<IEnumerable<StateflowsContext>> GetTimeTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses);
        /// <summary>Retrieves contexts that are waiting for startup-triggered execution.</summary>
        /// <param name="behaviorClasses">The behavior classes to include.</param>
        /// <returns>The contexts scheduled for startup-triggered execution.</returns>
        Task<IEnumerable<StateflowsContext>> GetStartupTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses);
    }
}
