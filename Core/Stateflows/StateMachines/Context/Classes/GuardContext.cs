using Stateflows.StateMachines.Models;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class GuardContext<TEvent> : TransitionContext<TEvent>
    {
        public GuardContext(RootContext context, Edge edge) : base(context, edge)
        { }
    }
}
