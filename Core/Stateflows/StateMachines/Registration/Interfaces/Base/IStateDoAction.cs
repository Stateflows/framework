using System.Diagnostics;
using Stateflows.Actions;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateDoAction<out TReturn>
    {
        /// <summary>
        /// Embeds Action in current state.<br/>
        /// Embedded Action will be initialized on state entry and finalized on state exit. Events can be forwarded from State Machine to embedded Action using <see cref="buildAction"/>
        /// </summary>
        /// <param name="doActionName">Action name</param>
        /// <param name="buildAction">Build action</param>
        /// <param name="initializationBuilder">Initialization builder; generates initialization event for embedded State Machine</param>
        TReturn AddDoAction(string doActionName, EmbeddedBehaviorBuildAction buildAction = null, StateActionInitializationBuilder initializationBuilder = null);
        
        /// <summary>
        /// Embeds Action in current state.<br/>
        /// Embedded Action will be initialized on state entry and finalized on state exit. Events can be forwarded from State Machine to embedded Action using <see cref="buildAction"/>
        /// </summary>
        /// <typeparam name="TAction">Action class; must implement <see cref="IAction"/> interface</typeparam>
        /// <param name="buildAction">Build action</param>
        /// <param name="initializationBuilder">Initialization builder; generates initialization event for embedded State Machine</param>
        [DebuggerHidden]
        public TReturn AddDoAction<TAction>(EmbeddedBehaviorBuildAction buildAction = null, StateActionInitializationBuilder initializationBuilder = null)
            where TAction : class, IAction
            => AddDoAction(Action<TAction>.Name, buildAction, initializationBuilder);
    }
}
