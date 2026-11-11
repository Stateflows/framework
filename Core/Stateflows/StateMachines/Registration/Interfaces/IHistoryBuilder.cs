using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Defines effect-only fallback paths for history when its containing state has no remembered
    /// configuration.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.3.5.-History" />
    public interface IHistoryBuilder :
        IPseudostateElseTransitions<IHistoryBuilder>
    { }
}
