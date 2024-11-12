using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class ContextCleanup : StateMachinePlugin
    {
        private readonly List<Vertex> ExitedStates = new List<Vertex>();

        public override Task AfterStateEntryAsync(IStateActionContext context)
            => Task.CompletedTask;

        public override Task AfterStateExitAsync(IStateActionContext context)
        {
            var vertex = (context as StateActionContext).Vertex;

            ExitedStates.Add(vertex);

            return Task.CompletedTask;
        }

        public override Task AfterTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context)
        {
            if (context.TargetState != null)
            {
                var ctx = (context as IRootContext).Context;
                foreach (var vertexName in ExitedStates.Select(v => v.Name))
                {
                    if (vertexName != context.TargetState.Name)
                    {
                        ctx.ClearStateValues(vertexName);
                    }
                }
                ExitedStates.Clear();
            }

            return Task.CompletedTask;
        }
    }
}
