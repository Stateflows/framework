using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IHistoryBuilder :
        IPseudostateTransitions<IHistoryBuilder>
    { }
}
