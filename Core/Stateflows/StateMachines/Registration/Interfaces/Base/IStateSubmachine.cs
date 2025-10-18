using System.Diagnostics;
using Stateflows.StateMachines.Registration.Builders;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateSubmachine<out TReturn>
    {
        // /// <summary>
        // /// Embeds State Machine in current state.<br/><br/>
        // /// Embedded State Machine will be initialized on state entry and finalized on state exit. Every event accepted by embedded State Machine will be sent to it first and retransmitted to embedding State Machine in case of rejection by embedded one. 
        // /// </summary>
        // /// <param name="submachineName">State Machine name</param>
        // /// <param name="initializationBuilder">Initialization builder; generates initialization event for embedded State Machine</param>
        // /// <returns></returns>
        // TReturn AddSubmachine(string submachineName, StateActionInitializationBuilder initializationBuilder = null);

        /// <summary>
        /// Embeds State Machine in current state.<br/><br/>
        /// Embedded State Machine will be initialized on state entry and finalized on state exit. Every event accepted by embedded State Machine will be sent to it first and retransmitted to embedding State Machine in case of rejection by embedded one.
        /// </summary>
        /// <typeparam name="TStateMachine">State Machine class; must implement <see cref="IStateMachine"/> interface</typeparam>
        /// <param name="initializationBuilder">Initialization builder; generates initialization event for embedded State Machine</param>
        // [DebuggerHidden]
        public TReturn AddSubmachine<TStateMachine>(StateActionInitializationBuilder initializationBuilder = null)
            where TStateMachine : class, IStateMachine;
            // => AddSubmachine(StateMachine<TStateMachine>.Name, initializationBuilder);

        /// <summary>
        /// Registers State Machine to be embedded in current state.<br/>
        /// Embedded State Machine will be initialized on state entry and finalized on state exit. Every event accepted by embedded State Machine will be sent to it first and retransmitted to embedding State Machine in case of rejection by embedded one.
        /// </summary>
        /// <param name="stateMachineBuildAction">State Machine build action</param>
        /// <param name="initializationBuilder">Initialization builder; generates initialization event for embedded State Machine</param>
        public TReturn AddSubmachine(StateMachineBuildAction stateMachineBuildAction, StateActionInitializationBuilder initializationBuilder = null);
        // {
        //     var vertex = ((StateBuilder)this).Vertex;
        //     var stateMachineName = $"{vertex.Graph.Name}.{vertex.Name}.submachine";
        //     vertex.Graph.StateflowsBuilder.AddStateMachines(b =>
        //         b.AddStateMachine(stateMachineName, stateMachineBuildAction));
        //     return AddSubmachine(stateMachineName, initializationBuilder);
        // }
    }
}
