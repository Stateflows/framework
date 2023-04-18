using Stateflows.Common;

namespace Stateflows.StateMachines.Events
{
    public class StateDescriptor
    {
        public string Name { get; set; }

        public StateDescriptor InnerState { get; set; }
    }

    public sealed class CurrentStateResponse : Response
    {
        public StateDescriptor CurrentState { get; set; }
    }
}
