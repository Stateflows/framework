using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedStateBuilderSubmachineTypedExtensions
    {
        /// <summary>
        /// Embeds State Machine in current state.<br/><br/>
        /// <b>Embedded Activity will be initialized on state entry and finalized on state exit. Events can be forwarded from State Machine to embedded Activity using <see cref="buildAction"/></b>
        /// </summary>
        /// <typeparam name="TStateMachine">State Machine class; must implement <see cref="IStateMachine"/> interface</typeparam>
        /// <param name="buildAction">Build action</param>
        /// <param name="initializationBuilder">Initialization builder; generates initialization event for embedded State Machine</param>
        [DebuggerHidden]
        public static IBehaviorTypedStateBuilder AddSubmachine<TStateMachine>(this ITypedStateBuilder builder, EmbeddedBehaviorBuildAction buildAction = null, StateActionInitializationBuilderAsync initializationBuilder = null)
            where TStateMachine : class, IStateMachine
            => builder.AddSubmachine(StateMachine<TStateMachine>.Name, buildAction, initializationBuilder);
    }
}
