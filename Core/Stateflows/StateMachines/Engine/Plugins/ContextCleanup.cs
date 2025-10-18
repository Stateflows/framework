using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class ContextCleanup : StateMachinePlugin
    {
        private readonly List<Vertex> ExitedStates = new List<Vertex>();

        public override void AfterStateExit(IStateActionContext context)
        {
            var vertex = ((StateActionContext)context).Vertex;

            if (!(vertex.ParentRegion?.IsHistoryEnabled ?? false))
            {
                ExitedStates.Add(vertex);
            }
        }

        public override void AfterTransitionEffect<TEvent>(ITransitionContext<TEvent> context)
        {
            if (context.Target != null)
            {
                // var ctx = ((IRootContext)context).Context;
                foreach (var vertexName in ExitedStates.Select(v => v.Name))
                {
                    if (vertexName != context.Target.Name)
                    {
                        if (context.TryGetStateContext(vertexName, out var stateContext))
                        {
                            stateContext.Values.ClearAsync().GetAwaiter().GetResult();
                        }
                        
                        // ctx.ClearStateValues(vertexName);
                    }
                }
                ExitedStates.Clear();
            }
        }
    }
}
