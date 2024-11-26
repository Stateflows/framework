using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class TransitionContext<TEvent> : EventContext<TEvent>,
        ITransitionInspectionContext<TEvent>,
        IEdgeContext,
        IRootContext

    {
        public Edge Edge { get; }

        public TransitionContext(RootContext context, Edge edge) : base(context)
        {
            Edge = edge;
        }

        IStateMachineInspectionContext ITransitionInspectionContext<TEvent>.StateMachine => StateMachine;

        private IStateContext sourceState = null;
        public IStateContext SourceState => sourceState ??= new StateContext(Edge.Source, Context);

        private bool targetStateSet = false;
        private IStateContext targetState = null;
        public IStateContext TargetState
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
    }
}
