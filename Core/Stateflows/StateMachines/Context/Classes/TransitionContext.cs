using System;
using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class TransitionContext<TEvent> : EventContext<TEvent>, ITransitionContext<TEvent>, IEdgeContext
    {
        public Edge Edge { get; }

        IStateMachineContext IStateMachineActionContext.StateMachine => StateMachine;

        public TransitionContext(RootContext context, Edge edge) : base(context)
        {
            Edge = edge;
        }

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
    }
}
