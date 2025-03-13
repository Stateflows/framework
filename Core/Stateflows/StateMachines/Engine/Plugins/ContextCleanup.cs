using System.Linq;
using System.Collections.Generic;
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
            var vertex = (context as StateActionContext).Vertex;

            ExitedStates.Add(vertex);
        }

        public override void AfterTransitionEffect<TEvent>(ITransitionContext<TEvent> context)
        {
            if (context.Target != null)
            {
                var ctx = (context as IRootContext).Context;
                foreach (var vertexName in ExitedStates.Select(v => v.Name))
                {
                    if (vertexName != context.Target.Name)
                    {
                        ctx.ClearStateValues(vertexName);
                    }
                }
                ExitedStates.Clear();
            }
        }
    }
}
