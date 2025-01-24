using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Initializer;
using Stateflows.Common.Registration.Builders;
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
        public static IStateflowsBuilder AddActivities(this IStateflowsBuilder stateflowsBuilder, ActivitiesBuildAction buildAction = null)
        {
            var register = stateflowsBuilder.EnsureActivitiesServices();
            buildAction?.Invoke(new ActivitiesBuilder(register));

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddDefaultInstance<TActivity>(this IStateflowsBuilder stateflowsBuilder, DefaultInstanceInitializationRequestFactoryAsync initializationRequestFactoryAsync = null)
            where TActivity : class, IActivity
            => stateflowsBuilder.AddDefaultInstance(new StateMachineClass(Activity<TActivity>.Name).BehaviorClass, initializationRequestFactoryAsync);

        private static ActivitiesRegister EnsureActivitiesServices(this IStateflowsBuilder stateflowsBuilder)
        {
            lock (Registers)
            {
                if (!Registers.TryGetValue(stateflowsBuilder, out var register))
                {
                    register = new ActivitiesRegister(stateflowsBuilder as StateflowsBuilder, stateflowsBuilder.ServiceCollection);
                    Registers.Add(stateflowsBuilder, register);

                    stateflowsBuilder
                        .EnsureStateflowServices()
                        .ServiceCollection
                        .AddScoped<AcceptEvents>()
                        .AddScoped<IActivityPlugin>(serviceProvider => serviceProvider.GetRequiredService<AcceptEvents>())
                        .AddSingleton(register)
                        .AddSingleton<IActivitiesRegister>(register)
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
                        .AddSingleton<IActivityEventHandler, TokensOutputHandler>()
                        .AddSingleton<IActivityEventHandler, TypedTokensOutputHandler>()
                        .AddTransient(provider =>
                            ActivitiesContextHolder.ActivityContext.Value ??
                            throw new InvalidOperationException($"No service for type '{typeof(IActivityContext).FullName}' is available in this context.")
                        )
                        .AddTransient(provider =>
                            ActivitiesContextHolder.NodeContext.Value ??
                            throw new InvalidOperationException($"No service for type '{typeof(INodeContext).FullName}' is available in this context.")
                        )
                        .AddTransient(provider =>
                            ActivitiesContextHolder.FlowContext.Value ??
                            throw new InvalidOperationException($"No service for type '{typeof(IFlowContext).FullName}' is available in this context.")
                        )
                        .AddTransient(provider =>
                            ActivitiesContextHolder.ExceptionContext.Value ??
                            throw new InvalidOperationException($"No service for type '{typeof(IExceptionContext).FullName}' is available in this context.")
                        )
                        .AddTransient(typeof(Input<>))
                        .AddTransient(typeof(SingleInput<>))
                        .AddTransient(typeof(OptionalInput<>))
                        .AddTransient(typeof(OptionalSingleInput<>))
                        .AddTransient(typeof(Output<>))
                        ;
                }

                return register;
            }
        }
    }
}
