using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.Engine
{
    internal class ContextCleanup : StateMachinePlugin
    {
        private readonly List<Vertex> ExitedStates = [];

        public override async Task OnStateExitAsync(IStateActionContext context, Func<Task> next)
        {
            await next();
            var vertex = ((StateActionContext)context).Vertex;

            if (!(vertex.ParentRegion?.IsHistoryEnabled ?? false))
            {
                ExitedStates.Add(vertex);
            }
        }

        public override async Task OnTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task> next)
        {
            await next();
            if (context.Target != null)
            {
                foreach (var vertexName in ExitedStates.Select(v => v.Name))
                {
                    if (vertexName != context.Target.Name)
                    {
                        if (context.TryGetStateContext(vertexName, out var stateContext))
                        {
                            context.Behavior.GetExecutor().AddPendingTask(stateContext.Values.ClearAsync());
                        }
                    }
                }
                ExitedStates.Clear();
            }
        }
    }
}
