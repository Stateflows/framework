namespace Stateflows.StateMachines.Registration.Interfaces.Base;

public interface IStateSubmachine<out TReturn>
{
    /// <summary>
    /// Embeds State Machine in current state.<br/><br/>
    /// Embedded State Machine will be initialized on state entry and finalized on state exit. Every event accepted by embedded State Machine will be sent to it first and retransmitted to embedding State Machine in case of rejection by embedded one.
    /// </summary>
    /// <typeparam name="TStateMachine">State Machine class; must implement <see cref="IStateMachine"/> interface</typeparam>
    /// <param name="buildAction">Build action</param>
    public TReturn AddSubmachine<TStateMachine>(StateMachineUtilsBuildAction buildAction = null)
        where TStateMachine : class, IStateMachine;

    /// <summary>
    /// Registers State Machine to be embedded in current state.<br/>
    /// Embedded State Machine will be initialized on state entry and finalized on state exit. Every event accepted by embedded State Machine will be sent to it first and retransmitted to embedding State Machine in case of rejection by embedded one.
    /// </summary>
    /// <param name="stateMachineBuildAction">State Machine build action</param>
    public TReturn AddSubmachine(StateMachineBuildAction stateMachineBuildAction);
}
