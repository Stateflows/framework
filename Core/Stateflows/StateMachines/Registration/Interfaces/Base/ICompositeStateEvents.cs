namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Combines initialization and finalization callbacks for a composite state's nested configuration.
    /// </summary>
    /// <typeparam name="TReturn">The fluent state builder returned after callback registration.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.2.2.-Composite-State" />
    public interface ICompositeStateEvents<out TReturn> :
        ICompositeStateInitialization<TReturn>,
        ICompositeStateFinalization<TReturn>;
}
