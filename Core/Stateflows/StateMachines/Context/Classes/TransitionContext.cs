using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class TransitionContext<TEvent> : EventContext<TEvent>, ITransitionContext<TEvent>, ITransitionInspectionContext<TEvent>, IEdgeContext, IRootContext
        where TEvent : Event
    {
        public Edge Edge { get; }

        public TransitionContext(RootContext context, Edge edge) : base(context)
        {
            Edge = edge;
        }

        IStateMachineInspectionContext ITransitionInspectionContext<TEvent>.StateMachine => StateMachine;

        private IStateContext sourceState = null;
        public IStateContext SourceState
        {
            get
            {
                if (!Context.Context.Values.TryGetValue(Constants.SourceState, out var stateName))
                {
                    Debug.Assert(true, "Source state name string is not available. Is context set up properly?");
                }

                return sourceState ?? (sourceState = new StateContext(stateName as string, Context));
            }
        }

        private bool targetStateSet = false;
        private IStateContext targetState = null;
        public IStateContext TargetState
        {
            get
            {
                if (!targetStateSet)
                {
                    if (!Context.Context.Values.TryGetValue(Constants.TargetState, out var stateName))
                    {
                        Debug.Assert(true, "Target state name string is not available. Is context set up properly?");
                    }

                    targetStateSet = true;

                    if ((string)stateName != Constants.DefaultTransitionTarget)
                    {
                        targetState = new StateContext(stateName as string, Context);
                    }
                }

                return targetState;
            }
        }
    }
}
