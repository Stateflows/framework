using System.Diagnostics;
using Stateflows.Activities;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateDoActivity<out TReturn>
    {
        /// <summary>
        /// Embeds Activity in current state.<br/>
        /// Embedded Activity will be initialized on state entry and finalized on state exit. Events can be forwarded from State Machine to embedded Activity using <see cref="buildAction"/>
        /// </summary>
        /// <param name="doActivityName">Activity name</param>
        /// <param name="buildAction">Build action</param>
        /// <param name="initializationBuilder">Initialization builder; generates initialization event for embedded State Machine</param>
        TReturn AddDoActivity(string doActivityName, EmbeddedBehaviorBuildAction buildAction = null, StateActionInitializationBuilder initializationBuilder = null);
        
        /// <summary>
        /// Embeds Activity in current state.<br/>
        /// Embedded Activity will be initialized on state entry and finalized on state exit. Events can be forwarded from State Machine to embedded Activity using <see cref="buildAction"/>
        /// </summary>
        /// <typeparam name="TActivity">Activity class; must implement <see cref="IActivity"/> interface</typeparam>
        /// <param name="buildAction">Build action</param>
        /// <param name="initializationBuilder">Initialization builder; generates initialization event for embedded State Machine</param>
        [DebuggerHidden]
        public TReturn AddDoActivity<TActivity>(EmbeddedBehaviorBuildAction buildAction = null, StateActionInitializationBuilder initializationBuilder = null)
            where TActivity : class, IActivity
            => AddDoActivity(Activity<TActivity>.Name, buildAction, initializationBuilder);
    }
}
