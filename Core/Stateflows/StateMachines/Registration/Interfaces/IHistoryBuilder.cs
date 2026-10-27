using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Defines effect-only fallback paths for history when its containing state has no remembered
    /// configuration.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-history" />
    public interface IHistoryBuilder :
        IPseudostateElseTransitions<IHistoryBuilder>
    { }
}
