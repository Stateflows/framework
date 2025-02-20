using System.Diagnostics;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateSubmachine<out TReturn>
    {
        TReturn AddSubmachine(string submachineName, EmbeddedBehaviorBuildAction buildAction, StateActionInitializationBuilderAsync initializationBuilder = null);
        
        /// <summary>
        /// Embeds State Machine in current state.<br/><br/>
        /// Embedded State Machine will be initialized on state entry and finalized on state exit. Events can be forwarded from State Machine to embedded State Machine using <see cref="buildAction"/>
        /// </summary>
        /// <typeparam name="TStateMachine">State Machine class; must implement <see cref="IStateMachine"/> interface</typeparam>
        /// <param name="buildAction">Build action</param>
        /// <param name="initializationBuilder">Initialization builder; generates initialization event for embedded State Machine</param>
        [DebuggerHidden]
        public TReturn AddSubmachine<TStateMachine>(EmbeddedBehaviorBuildAction buildAction = null, StateActionInitializationBuilderAsync initializationBuilder = null)
            where TStateMachine : class, IStateMachine
            => AddSubmachine(StateMachine<TStateMachine>.Name, buildAction, initializationBuilder);
    }
}
