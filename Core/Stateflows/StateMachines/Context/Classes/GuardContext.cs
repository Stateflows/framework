using Stateflows.StateMachines.Models;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class GuardContext<TEvent> : TransitionContext<TEvent>
    {
        public GuardContext(RootContext context, Edge edge, TransitionRoute? route = null, int routeIndex = 0) : base(context, edge, route, routeIndex)
        { }
    }
}
