using Stateflows.Common;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines
{
    /// <summary>
    /// Event that triggers default transitions. It is sent automatically after each successful event consumption, f.e.
    /// by triggering a transition.
    /// </summary>
    [Event(Constants.Completion), NoForwarding]
    public sealed class Completion : SystemEvent;
}
