using System.Diagnostics;
using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Assigns an embedded activity whose lifetime is bound to the current state's activation.</summary>
    /// <typeparam name="TReturn">The state builder stage after assigning its do behavior.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
    public interface IStateDoActivity<out TReturn>
    {
        /// <summary>Embeds an Activity whose lifetime is bound to the current state's activation.</summary>
        /// <typeparam name="TActivity">Embedded behavior implementation of <see cref="IActivity" />. Its definition is
        /// registered for this embedding site.</typeparam>
        /// <param name="buildAction">Optional callback configuring the typed embedded Activity's utilities and middleware
        /// without redefining its graph.</param>
        /// <returns>The state builder stage with its do behavior assigned.</returns>
        /// <remarks>The embedded behavior is initialized on state entry and finalized on state exit. Events accepted by it
        /// are offered to it first; rejected events are retransmitted to the embedding State Machine.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        public TReturn AddDoActivity<TActivity>(ActivityUtilsBuildAction buildAction = null)
            where TActivity : class, IActivity;

        /// <summary>Embeds an Activity whose lifetime is bound to the current state's activation.</summary>
        /// <param name="activityBuildAction">Callback configuring the embedded Activity's nodes, flows, and lifecycle.
        /// </param>
        /// <returns>The state builder stage with its do behavior assigned.</returns>
        /// <remarks>The embedded behavior is initialized on state entry and finalized on state exit. Events accepted by it
        /// are offered to it first; rejected events are retransmitted to the embedding State Machine.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        public TReturn AddDoActivity(ReactiveActivityBuildAction activityBuildAction);
    }
}
