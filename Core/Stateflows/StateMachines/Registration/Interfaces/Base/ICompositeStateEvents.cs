namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Combines initialization and finalization callbacks for a composite state's nested configuration.
    /// </summary>
    /// <typeparam name="TReturn">The fluent state builder returned after callback registration.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    public interface ICompositeStateEvents<out TReturn> :
        ICompositeStateInitialization<TReturn>,
        ICompositeStateFinalization<TReturn>;
}
