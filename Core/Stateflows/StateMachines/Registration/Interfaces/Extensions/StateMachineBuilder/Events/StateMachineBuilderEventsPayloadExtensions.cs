using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class StateMachineBuilderEventsPayloadExtensions
    {
        public static IStateMachineBuilder AddOnInitialize<TInitializationPayload>(this IStateMachineBuilder builder, Func<IStateMachineInitializationContext<InitializationRequest<TInitializationPayload>>, Task<bool>> actionAsync)
            => builder.AddOnInitialize(actionAsync);
    }
}
