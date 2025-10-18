using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateContext : IStateContext
    {
        public Vertex Vertex { get; }

        public string Name => Vertex.Name;

        public string Identifier => Vertex.Identifier;

        private RootContext Context { get; }

        private StateValues StateValues { get; }
        
        public StateContext(Vertex vertex, RootContext context)
        {
            Vertex = vertex;
            Context = context;
            StateValues = Context.GetStateValues(Name);
            // Values = new ContextValuesCollection(StateValues.Values);
            Values = new ValuesStorage(
                $"{Constants.State}.{vertex.Identifier}",
                Context.Context.ContextOwnerId ?? Context.Id,
                Context.Executor.ServiceProvider.GetRequiredService<IStateflowsLock>(),
                Context.Executor.ServiceProvider.GetRequiredService<IStateflowsValueStorage>()
            );
        }

        public IContextValues Values { get; }

        public bool TryGetParent(out IStateContext parentStateContext)
        {
            var parent = Context.Executor.Graph.AllVertices[Identifier].ParentRegion?.ParentVertex;

            parentStateContext = parent != null
                ? new StateContext(parent, Context)
                : null;

            return parentStateContext != null;
        }
    }
}
