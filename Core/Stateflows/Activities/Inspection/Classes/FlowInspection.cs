﻿using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities.Inspection.Classes
{
    internal class FlowInspection : IFlowInspection
    {
        private Executor Executor { get; }
        
        private Inspector Inspector { get; }

        private Edge Edge { get; }

        public FlowInspection(Executor executor, Inspector inspector, Edge edge)
        {
            Executor = executor;
            Inspector = inspector;
            Edge = edge;
            Inspector.InspectionFlows.Add(Edge, this);
        }

        public bool Active { get; set; }

        private INodeInspection source;

        public INodeInspection Source
        {
            get
            {
                if (
                    source == null &&
                    Inspector.InspectionNodes.TryGetValue(Edge.Source, out var s)
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
                    Inspector.InspectionNodes.TryGetValue(Edge.Target, out var t)
                )
                {
                    target = t;
                }

                return target;
            }
        }

        public string TokenName => Edge.TokenType.GetTokenName();

        public FlowType Type => Edge.TokenType == typeof(ControlToken)
            ? FlowType.ControlFlow
            : FlowType.ObjectFlow;
    }
}
