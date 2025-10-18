using Stateflows.Common;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines
{
    [Event(Constants.Completion), NoForwarding]
    public sealed class Completion : SystemEvent
    { }
}
