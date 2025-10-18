namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface ICompositeStateEvents<out TReturn> :
        ICompositeStateInitialization<TReturn>,
        ICompositeStateFinalization<TReturn>
    { }
}
