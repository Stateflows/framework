using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateContext : IStateContext
    {
        public string Name { get; }

        private RootContext Context { get; }

        private StateValues StateValues { get; }

        public StateContext(string stateName, RootContext context)
        {
            Name = stateName;
            Context = context;
            StateValues = Context.GetStateValues(Name);
            Values = new ContextValues(StateValues.Values);
        }

        public IContextValues Values { get; }

        public bool TryGetParentState(out IStateContext parentStateContext)
        {
            var parent = Context.Executor.Graph.AllVertices[Name].Parent;

            parentStateContext = parent != null
                ? new StateContext(parent.Name, Context)
                : null;

            return parentStateContext != null;
        }
    }
}
