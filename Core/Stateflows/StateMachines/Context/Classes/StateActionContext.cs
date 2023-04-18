using System.Diagnostics;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateActionContext : BaseContext, IStateActionContext, IStateActionInspectionContext, IRootContext
    {
        public Vertex Vertex { get; set; }

        public string ActionName { get; }

        IStateMachineContext IStateMachineActionContext.StateMachine => StateMachine;
        IStateMachineInspectionContext IStateActionInspectionContext.StateMachine => StateMachine;

        public StateActionContext(RootContext context, Vertex vertex, string name)
            : base(context)
        {
            Vertex = vertex;
            ActionName = name;
        }

        private IStateContext currentState = null;
        public IStateContext CurrentState
        {
            get
            {
                if (!Context.Context.Values.TryGetValue(Constants.State, out var stateName))
                {
                    Debug.Assert(true, "State name string is not available. Is context set up properly?");
                }

                return currentState ?? (currentState = new StateContext(stateName as string, Context));
            }
        }
    }
}
