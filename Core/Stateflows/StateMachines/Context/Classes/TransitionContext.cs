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

        private IVertexContext sourceVertex = null;
        public IVertexContext Source => sourceVertex ??= new VertexContext(Edge.Source, Context);

        private bool targetStateSet = false;
        private IVertexContext targetVertex = null;
        public IVertexContext Target
        {
            get
            {
                if (!targetStateSet)
                {
                    targetStateSet = true;

                    if (!(Edge.Target is null))
                    {
                        targetVertex = new VertexContext(Edge.Target, Context);
                    }
                }

                return targetVertex;
            }
        }
    }
}
