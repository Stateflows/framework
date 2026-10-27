namespace Stateflows.StateMachines.Registration.Interfaces.Base;

/// <summary>Assigns an embedded machine whose lifetime is bound to the current state's activation.</summary>
/// <typeparam name="TReturn">The state builder returned after embedding the submachine.</typeparam>
/// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
public interface IStateSubmachine<out TReturn>
{
    /// <summary>Embeds a State Machine whose lifetime is bound to the current state's activation.</summary>
    /// <typeparam name="TStateMachine">Embedded State Machine implementation whose definition supplies the submachine's
    /// graph.</typeparam>
    /// <param name="buildAction">Optional callback configuring the typed submachine's utilities and middleware without
    /// redefining its graph.</param>
    /// <returns>The state builder with the submachine configured.</returns>
    /// <remarks>The embedded behavior is initialized on state entry and finalized on state exit. Events accepted by it
    /// are offered to it first; rejected events are retransmitted to the embedding State Machine.</remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
    public TReturn AddSubmachine<TStateMachine>(StateMachineUtilsBuildAction buildAction = null)
        where TStateMachine : class, IStateMachine;

    /// <summary>Embeds a State Machine whose lifetime is bound to the current state's activation.</summary>
    /// <param name="stateMachineBuildAction">Callback configuring the embedded State Machine's states, transitions, and
    /// lifecycle.</param>
    /// <returns>The state builder with the submachine configured.</returns>
    /// <remarks>The embedded behavior is initialized on state entry and finalized on state exit. Events accepted by it
    /// are offered to it first; rejected events are retransmitted to the embedding State Machine.</remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
    public TReturn AddSubmachine(StateMachineBuildAction stateMachineBuildAction);
}
