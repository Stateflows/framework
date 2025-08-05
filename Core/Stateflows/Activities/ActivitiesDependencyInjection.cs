using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Initializer;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context;
using Stateflows.Activities.Service;
using Stateflows.Activities.Registration;
using Stateflows.Activities.EventHandlers;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ActivitiesDependencyInjection
    {
        private static readonly Dictionary<IStateflowsBuilder, ActivitiesRegister> Registers = new Dictionary<IStateflowsBuilder, ActivitiesRegister>();

        internal static void Cleanup(IStateflowsBuilder builder)
        {
            lock (Registers)
            {
                if (Registers.TryGetValue(builder, out var register) && !register.Activities.Any())
                {
                    var serviceDescriptor = builder.ServiceCollection.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IActivitiesRegister));
                    builder.ServiceCollection.Remove(serviceDescriptor);
                }
            }
        }
        
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
                    register = new ActivitiesRegister(stateflowsBuilder as StateflowsBuilder);
                    Registers.Add(stateflowsBuilder, register);

                    stateflowsBuilder
                        .EnsureStateflowServices()
                        .ServiceCollection
                        .AddScoped<AcceptEvents>()
                        .AddScoped<Notifications>()
                        .AddScoped<IActivityPlugin>(serviceProvider => serviceProvider.GetRequiredService<AcceptEvents>())
                        .AddScoped<IActivityPlugin>(serviceProvider => serviceProvider.GetRequiredService<Notifications>())
                        .AddSingleton(register)
                        .AddSingleton<IActivitiesRegister>(register)
                        .AddScoped<IActivityContextProvider, ActivityContextProvider>()
                        .AddScoped<IEventProcessor, Processor>()
                        .AddTransient<IBehaviorProvider, Provider>()
                        .AddSingleton<IActivityEventHandler, BehaviorStatusHandler>()
                        .AddSingleton<IActivityEventHandler, ActivityInfoRequestHandler>()
                        .AddSingleton<IActivityEventHandler, InitializeHandler>()
                        .AddSingleton<IActivityEventHandler, FinalizationHandler>()
                        .AddSingleton<IActivityEventHandler, ResetHandler>()
                        .AddSingleton<IActivityEventHandler, SubscriptionHandler>()
                        .AddSingleton<IActivityEventHandler, UnsubscriptionHandler>()
                        .AddSingleton<IActivityEventHandler, StartRelayHandler>()
                        .AddSingleton<IActivityEventHandler, StopRelayHandler>()
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
                        .AddTransient(provider =>
                            ActivitiesContextHolder.Inspection.Value ??
                            throw new InvalidOperationException(
                                $"No service for type '{typeof(IActivityInspection).FullName}' is available in this context.")
                        )
                        .AddTransient(typeof(IInputTokens<>), typeof(InputTokens<>))
                        .AddTransient(typeof(IInputToken<>), typeof(InputToken<>))
                        .AddTransient(typeof(IOptionalInputTokens<>), typeof(OptionalInputTokens<>))
                        .AddTransient(typeof(IOptionalInputToken<>), typeof(OptionalInputToken<>))
                        .AddTransient(typeof(IOutputTokens<>), typeof(OutputTokens<>))
                        ;
                }

                return register;
            }
        }
    }
}
