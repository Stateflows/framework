using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Inspection.Classes
{
    internal class FlowInspection : IFlowInspection
    {
        private Executor Executor { get; }

        private Edge Edge { get; }

        public FlowInspection(Executor executor, Edge edge)
        {
            Executor = executor;
            Edge = edge;
            Executor.Observer.InspectionFlows.Add(Edge, this);
        }

        public bool Active { get; set; }

        private INodeInspection source;

        public INodeInspection Source
        {
            get
            {
                if (
                    source == null &&
                    Executor.Observer.InspectionNodes.TryGetValue(Edge.Source, out var s)
                )
                {
                    source = s;
                }

                return source;
            }
        }

        private INodeInspection target;

        public INodeInspection Target
        {
            get
            {
                if (
                    target == null &&
                    Edge.Target != null &&
                    Executor.Observer.InspectionNodes.TryGetValue(Edge.Target, out var t)
                )
                {
                    target = t;
                }

                return target;
            }
        }

        public string TokenName => Edge.TokenType.FullName;

        public FlowType Type => Edge.TokenType == typeof(ControlToken)
            ? FlowType.ControlFlow
            : FlowType.ObjectFlow;
    }
}
