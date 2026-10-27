using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Extensions;
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
    /// <summary>Registers activity services, activity behaviors, and default activity instances with dependency injection.</summary>
    public static class ActivitiesDependencyInjection
    {
        private static readonly Dictionary<IStateflowsBuilder, ActivitiesRegister> Registers = new();

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

        /// <summary>Adds activity services and registers activities in a Stateflows builder.</summary>
        /// <param name="stateflowsBuilder">The Stateflows service and behavior builder.</param>
        /// <param name="buildAction">Callback that declares activity registrations.</param>
        /// <returns>The Stateflows builder for further configuration.</returns>
        /// <remarks>Call during application configuration before the Stateflows builder is built.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activities">Activities documentation</seealso>
        [DebuggerHidden]
        public static IStateflowsBuilder AddActivities(this IStateflowsBuilder stateflowsBuilder,
            ActivitiesBuildAction buildAction = null)
            => AddActivities(stateflowsBuilder, buildAction, null, null);
        
        [DebuggerHidden]
        internal static IStateflowsBuilder AddActivities(this IStateflowsBuilder stateflowsBuilder, ActivitiesBuildAction buildAction, BehaviorClass? ownerClass, BehaviorClass? parentClass)
        {
            var register = stateflowsBuilder.EnsureActivitiesServices();
            buildAction?.Invoke(new ActivitiesBuilder(register, ownerClass, parentClass));

            return stateflowsBuilder;
        }

        /// <summary>Registers a default instance of a typed activity behavior.</summary>
        /// <typeparam name="TActivity">The activity implementation type.</typeparam>
        /// <param name="stateflowsBuilder">The Stateflows builder.</param>
        /// <param name="initializationRequestFactoryAsync">Optional factory for the default instance's initialization request.</param>
        /// <returns>The Stateflows builder for further configuration.</returns>
        [DebuggerHidden]
        public static IStateflowsBuilder AddDefaultInstance<TActivity>(this IStateflowsBuilder stateflowsBuilder, DefaultInstanceInitializationRequestFactoryAsync initializationRequestFactoryAsync = null)
            where TActivity : class, IActivity
            => stateflowsBuilder.AddDefaultInstance(new StateMachineClass(Activity<TActivity>.Name).BehaviorClass, initializationRequestFactoryAsync);

        internal static ActivitiesRegister EnsureActivitiesServices(this IStateflowsBuilder stateflowsBuilder)
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
                        .AddScoped<IActivityPlugin, Behaviors>()
                        .AddSingleton(register)
                        .AddSingleton<IActivitiesRegister>(register)
                        .AddScoped<IEventProcessor, Processor>()
                        .AddTransient<IBehaviorProvider, Provider>()
                        .AddSingleton<IActivityEventHandler, BehaviorStatusHandler>()
                        .AddSingleton<IActivityEventHandler, ActivityInfoRequestHandler>()
                        .AddSingleton<IActivityEventHandler, InitializeHandler>()
                        .AddSingleton<IActivityEventHandler, FinalizationHandler>()
                        .AddSingleton<IActivityEventHandler, ResetHandler>()
                        .AddSingleton<IActivityEventHandler, SubscriptionHandler>()
                        .AddSingleton<IActivityEventHandler, UnsubscriptionHandler>()
                        .AddSingleton<IActivityEventHandler, TokensOutputHandler>()
                        .AddSingleton<IActivityEventHandler, TypedTokensOutputHandler>()
                        .AddTransient(_ =>
                            ActivitiesContextHolder.ActivityContext.Value ??
                            throw new InvalidOperationException($"No service for type '{typeof(IActivityContext).FullName}' is available in this context.")
                        )
                        .AddTransient(_ =>
                            ActivitiesContextHolder.NodeContext.Value ??
                            throw new InvalidOperationException($"No service for type '{typeof(INodeContext).FullName}' is available in this context.")
                        )
                        .AddTransient(_ =>
                            ActivitiesContextHolder.FlowContext.Value ??
                            throw new InvalidOperationException($"No service for type '{typeof(IFlowContext).FullName}' is available in this context.")
                        )
                        .AddTransient(_ =>
                            ActivitiesContextHolder.ExceptionContext.Value ??
                            throw new InvalidOperationException($"No service for type '{typeof(IExceptionContext).FullName}' is available in this context.")
                        )
                        .AddTransient(_ =>
                            ActivitiesContextHolder.Inspection.Value ??
                            throw new InvalidOperationException(
                                $"No service for type '{typeof(IActivityInspection).FullName}' is available in this context.")
                        )
                        .AddTransient(typeof(IInputTokens<>), typeof(InputTokens<>))
                        .AddTransient(typeof(IInputToken<>), typeof(InputToken<>))
                        .AddTransient(typeof(IOptionalInputTokens<>), typeof(OptionalInputTokens<>))
                        .AddTransient(typeof(IOptionalInputToken<>), typeof(OptionalInputToken<>))
                        
                        .AddTransient(typeof(IInputOutputTokens<>), typeof(InputOutputTokens<>))
                        .AddTransient(typeof(IInputOutputToken<>), typeof(InputOutputToken<>))
                        .AddTransient(typeof(IOptionalInputOutputTokens<>), typeof(OptionalInputOutputTokens<>))
                        .AddTransient(typeof(IOptionalInputOutputToken<>), typeof(OptionalInputOutputToken<>))
                        
                        .AddTransient(typeof(IOutputTokens<>), typeof(OutputTokens<>))
                        ;
                }

                return register;
            }
        }

        internal static void Build(IStateflowsBuilder builder)
        {   
            lock (Registers)
            {
                if (
                    builder.ServiceCollection.IsServiceRegistered<IActivitiesRegister>() &&
                    Registers.TryGetValue(builder, out var register)
                )
                {
                    foreach (var graph in register.Activities.Values)
                    {
                        graph.Build();
                    }
                }
            }
        }
    }
}
