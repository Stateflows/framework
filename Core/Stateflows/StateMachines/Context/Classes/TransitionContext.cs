using System;
using System.Collections.Generic;
using System.Threading;
using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class TransitionContext<TEvent>(RootContext context, Edge edge, TransitionRoute? route = null, int routeIndex = 0) :
        EventContext<TEvent>(context),
        ITransitionContext<TEvent>,
        IEdgeContext
    {
        public Edge Edge { get; } = edge;
        public string Identifier => Edge.Identifier;
        public IReadOnlyList<string> RouteTransitionIds { get; } = route?.Identifiers ?? Array.AsReadOnly(new[] { edge.Identifier });
        public int RouteIndex { get; } = routeIndex;

        private IStateContext sourceState = null;
        public IStateContext Source => sourceState ??= new StateContext(Edge.Source, Context);

        private bool targetStateSet = false;
        private IStateContext targetState = null;

        public IStateContext Target
        {
            get
            {
                if (!targetStateSet)
                {
                    targetStateSet = true;

                    if (!(Edge.Target is null))
                    {
                        targetState = new StateContext(Edge.Target, Context);
                    }
                }

                return targetState;
            }
        }

        public Type TriggerType => Edge.TriggerType;

        object ITransitionContext.Trigger => Event;
    }
}
