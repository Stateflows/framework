using System.Diagnostics;
using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Builders;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateDoActivity<out TReturn>
    {
        /// <summary>
        /// Embeds Activity in current state.<br/>
        /// Embedded Activity will be initialized on state entry and finalized on state exit. Every event accepted by embedded Activity will be sent to it first and retransmitted to embedding State Machine in case of rejection by embedded one.
        /// </summary>
        /// <param name="doActivityName">Activity name</param>
        /// <param name="initializationBuilder">Initialization builder; generates initialization event for embedded Activity</param>
        TReturn AddDoActivity(string doActivityName, StateActionInitializationBuilder initializationBuilder = null);
        
        /// <summary>
        /// Embeds Activity in current state.<br/>
        /// Embedded Activity will be initialized on state entry and finalized on state exit. Every event accepted by embedded Activity will be sent to it first and retransmitted to embedding State Machine in case of rejection by embedded one.
        /// </summary>
        /// <typeparam name="TActivity">Activity class; must implement <see cref="IActivity"/> interface</typeparam>
        /// <param name="initializationBuilder">Initialization builder; generates initialization event for embedded Activity</param>
        [DebuggerHidden]
        public TReturn AddDoActivity<TActivity>(StateActionInitializationBuilder initializationBuilder = null)
            where TActivity : class, IActivity
            => AddDoActivity(Activity<TActivity>.Name, initializationBuilder);

        /// <summary>
        /// Registers Activity to be embedded in current state.<br/>
        /// Embedded Activity will be initialized on state entry and finalized on state exit. Every event accepted by embedded Activity will be sent to it first and retransmitted to embedding State Machine in case of rejection by embedded one.
        /// </summary>
        /// <param name="activityBuildAction">Activity build action</param>
        /// <param name="initializationBuilder">Initialization builder; generates initialization event for embedded Activity</param>
        public TReturn AddDoActivity(ReactiveActivityBuildAction activityBuildAction, StateActionInitializationBuilder initializationBuilder = null)
        {
            var vertex = ((StateBuilder)this).Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.doActivity";
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity(activityName, activityBuildAction));
            return AddDoActivity(activityName, initializationBuilder);
        }
    }
}
