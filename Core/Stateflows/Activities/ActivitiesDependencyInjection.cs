using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context;
using Stateflows.Activities.Registration;
using Stateflows.Activities.EventHandlers;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ActivitiesDependencyInjection
    {
        private readonly static Dictionary<IStateflowsBuilder, ActivitiesRegister> Registers = new Dictionary<IStateflowsBuilder, ActivitiesRegister>();

        [DebuggerHidden]
        public static IStateflowsBuilder AddActivities(this IStateflowsBuilder stateflowsBuilder, ActivitiesBuildAction buildAction)
        {
            buildAction(new ActivitiesBuilder(stateflowsBuilder.EnsureActivitiesServices()));

            return stateflowsBuilder;
        }

        private static ActivitiesRegister EnsureActivitiesServices(this IStateflowsBuilder stateflowsBuilder)
        {
            if (!Registers.TryGetValue(stateflowsBuilder, out var register))
            {
                register = new ActivitiesRegister(stateflowsBuilder.ServiceCollection);
                Registers.Add(stateflowsBuilder, register);

                stateflowsBuilder
                    .EnsureStateflowServices()
                    .ServiceCollection
                    .AddScoped<AcceptEvents>()
                    .AddScoped<IActivityPlugin>(serviceProvider => serviceProvider.GetRequiredService<AcceptEvents>())
                    .AddSingleton(register)
                    .AddScoped<IEventProcessor, Processor>()
                    .AddTransient<IBehaviorProvider, Provider>()
                    .AddSingleton<IActivityEventHandler, BehaviorStatusHandler>()
                    .AddSingleton<IActivityEventHandler, InitializeHandler>()
                    .AddSingleton<IActivityEventHandler, FinalizationHandler>()
                    .AddSingleton<IActivityEventHandler, ResetHandler>()
                    .AddSingleton<IActivityEventHandler, SubscriptionHandler>()
                    .AddSingleton<IActivityEventHandler, UnsubscriptionHandler>()
                    .AddSingleton<IActivityEventHandler, NotificationsHandler>()
                    .AddSingleton<IActivityEventHandler, SetGlobalValuesHandler>()
                    .AddTransient(provider =>
                        ContextHolder.ActivityContext.Value ??
                        throw new InvalidOperationException($"No service for type '{typeof(IActivityContext).FullName}' is available in this context.")
                    )
                    .AddTransient(provider =>
                        ContextHolder.NodeContext.Value ??
                        throw new InvalidOperationException($"No service for type '{typeof(INodeContext).FullName}' is available in this context.")
                    )
                    .AddTransient(provider =>
                        ContextHolder.FlowContext.Value ??
                        throw new InvalidOperationException($"No service for type '{typeof(IFlowContext).FullName}' is available in this context.")
                    )
                    .AddTransient(provider =>
                        ContextHolder.ExecutionContext.Value ??
                        throw new InvalidOperationException($"No service for type '{typeof(IExecutionContext).FullName}' is available in this context.")
                    )
                    .AddTransient(provider =>
                        ContextHolder.ExceptionContext.Value ??
                        throw new InvalidOperationException($"No service for type '{typeof(IExceptionContext).FullName}' is available in this context.")
                    )
                    ;
            }

            return register;
        }
    }
}
