using System;
using System.Diagnostics;
using Stateflows.Activities;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class OverridenStateBuilderDoActivityTypedExtensions
    {
        /// <summary>
        /// Embeds Activity in current state.<br/><br/>
        /// <b>Embedded Activity will be initialized on state entry and finalized on state exit. Events can be forwarded from State Machine to embedded Activity using <see cref="buildAction"/></b>
        /// </summary>
        /// <typeparam name="TActivity">Activity class; must implement <see cref="IActivity"/> interface</typeparam>
        /// <param name="buildAction">Build action</param>
        /// <param name="initializationBuilder">Initialization builder; generates initialization event for embedded State Machine</param>
        [DebuggerHidden]
        public static IBehaviorOverridenStateBuilder AddDoActivity<TActivity>(this IOverridenStateBuilder builder, EmbeddedBehaviorBuildAction buildAction = null, StateActionInitializationBuilderAsync initializationBuilder = null)
            where TActivity : class, IActivity
            => builder.AddDoActivity(Activity<TActivity>.Name, buildAction, initializationBuilder);
    }
}
