using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class StateMachineBuilderEventsPayloadExtensions
    {
        public static IStateMachineBuilder AddInitializer<TInitializationPayload>(this IStateMachineBuilder builder, Func<IStateMachineInitializationContext<Initialize<TInitializationPayload>>, Task<bool>> actionAsync)
            => builder.AddInitializer(actionAsync);
    }
}
