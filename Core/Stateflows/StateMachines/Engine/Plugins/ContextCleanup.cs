using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class ContextCleanup : IStateMachinePlugin
    {
        private readonly List<Vertex> Context_ExitedStates = new List<Vertex>();

        public Task AfterStateEntryAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task AfterStateExitAsync(IStateActionContext context)
        {
            var vertex = (context as StateActionContext).Vertex;

            Context_ExitedStates.Add(vertex);

            return Task.CompletedTask;
        }

        public Task AfterStateInitializeAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task AfterStateFinalizeAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task AfterStateMachineInitializeAsync(IStateMachineInitializationContext context)
            => Task.CompletedTask;

        public Task AfterStateMachineFinalizeAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task AfterTransitionEffectAsync(ITransitionContext<Event> context)
        {
            if (context.TargetState != null)
            {
                var ctx = (context as IRootContext).Context;
                foreach (var vertexName in Context_ExitedStates.Select(v => v.Name))
                {
                    if (vertexName != context.TargetState.Name)
                    {
                        ctx.ClearStateValues(vertexName);
                    }
                }
                Context_ExitedStates.Clear();
            }

            return Task.CompletedTask;
        }

        public Task AfterTransitionGuardAsync(IGuardContext<Event> context, bool guardResult)
            => Task.CompletedTask;

        public Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
            => Task.FromResult(true);

        public Task AfterProcessEventAsync(IEventContext<Event> context)
            => Task.CompletedTask;

        public Task BeforeStateEntryAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task BeforeStateExitAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task BeforeStateInitializeAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task BeforeStateFinalizeAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task BeforeStateMachineInitializeAsync(IStateMachineInitializationContext context)
            => Task.CompletedTask;

        public Task BeforeStateMachineFinalizeAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task BeforeTransitionEffectAsync(ITransitionContext<Event> context)
            => Task.CompletedTask;

        public Task BeforeTransitionGuardAsync(IGuardContext<Event> context)
            => Task.CompletedTask;

        public Task AfterHydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task BeforeDehydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;
    }
}
