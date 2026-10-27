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
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Context;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.EventHandlers;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Registers state-machine definitions and their runtime services with Stateflows.</summary>
    /// <remarks>See <see href="https://github.com/Stateflows/framework/wiki/behaviors">behavior registration</see>.</remarks>
    public static class StateMachinesDependencyInjection
    {
        private static readonly Dictionary<IStateflowsBuilder, StateMachinesRegister> Registers = new();

        internal static void Cleanup(IStateflowsBuilder builder)
        {
            lock (Registers)
            {
                if (Registers.TryGetValue(builder, out var register) && !register.StateMachines.Any())
                {
                    var serviceDescriptor = builder.ServiceCollection.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IStateMachinesRegister));
                    builder.ServiceCollection.Remove(serviceDescriptor);
                }
            }
        }

        internal static void Build(IStateflowsBuilder builder)
        {   
            lock (Registers)
            {
                if (
                    builder.ServiceCollection.IsServiceRegistered<IStateMachinesRegister>() &&
                    Registers.TryGetValue(builder, out var register)
                )
                {
                    foreach (var graph in register.StateMachines.Values)
                    {
                        graph.Build();
                    }
                }
            }
        }

        /// <summary>Enables the state-machine runtime and optionally registers its definitions and global middleware.</summary>
        /// <param name="stateflowsBuilder">The Stateflows registration builder.</param>
        /// <param name="buildAction">Optional registration callback; omitting it still enables state-machine services.</param>
        /// <returns>The original Stateflows builder for registering other behavior types.</returns>
        [DebuggerHidden]
        public static IStateflowsBuilder AddStateMachines(this IStateflowsBuilder stateflowsBuilder,
            StateMachinesBuildAction buildAction = null)
            => AddStateMachines(stateflowsBuilder, buildAction, null, null);

        [DebuggerHidden]
        internal static IStateflowsBuilder AddStateMachines(this IStateflowsBuilder stateflowsBuilder,
            StateMachinesBuildAction buildAction, BehaviorClass? ownerClass, BehaviorClass? parentClass)
        {
            var register = stateflowsBuilder.EnsureStateMachinesServices();
            buildAction?.Invoke(new StateMachinesBuilder(register, ownerClass, parentClass));

            return stateflowsBuilder;
        }

        /// <summary>Registers a default instance for a typed state machine using its resolved behavior name.</summary>
        /// <typeparam name="TStateMachine">The registered state-machine definition type.</typeparam>
        /// <param name="stateflowsBuilder">The Stateflows registration builder.</param>
        /// <param name="initializationRequestFactoryAsync">Optional factory for the default instance's initialization request.</param>
        /// <returns>The original Stateflows builder for continued registration.</returns>
        [DebuggerHidden]
        public static IStateflowsBuilder AddDefaultInstance<TStateMachine>(this IStateflowsBuilder stateflowsBuilder, DefaultInstanceInitializationRequestFactoryAsync initializationRequestFactoryAsync = null)
            where TStateMachine : class, IStateMachine
            => stateflowsBuilder.AddDefaultInstance(new StateMachineClass(StateMachine<TStateMachine>.Name).BehaviorClass, initializationRequestFactoryAsync);

        [DebuggerHidden]
        internal static IStateMachinesRegister EnsureStateMachinesServices(this IStateflowsBuilder stateflowsBuilder)
        {
            lock (Registers)
            {
                if (!Registers.TryGetValue(stateflowsBuilder, out var register))
                {
                    register = new StateMachinesRegister(stateflowsBuilder as StateflowsBuilder);
                    Registers.Add(stateflowsBuilder, register);

                    stateflowsBuilder
                        .EnsureStateflowServices()
                        .ServiceCollection
                        .AddScoped<IStateMachinePlugin, Behaviors>()
                        .AddScoped<IStateMachinePlugin, TimeEvents>()
                        .AddScoped<IStateMachinePlugin, ContextCleanup>()
                        .AddScoped<IStateMachinePlugin, Notifications>()
                        .AddScoped<IStateMachinePlugin, Engine.Exceptions>()
                        .AddSingleton(register)
                        .AddSingleton<IStateMachinesRegister>(register)
                        .AddScoped<IEventProcessor, Processor>()
                        .AddTransient<IBehaviorProvider, Provider>()
                        .AddSingleton<IStateMachineEventHandler, BehaviorStatusRequestHandler>()
                        .AddSingleton<IStateMachineEventHandler, StateMachineInfoRequestHandler>()
                        .AddSingleton<IStateMachineEventHandler, InitializeHandler>()
                        .AddSingleton<IStateMachineEventHandler, FinalizationHandler>()
                        .AddSingleton<IStateMachineEventHandler, ResetHandler>()
                        .AddSingleton<IStateMachineEventHandler, SubscriptionHandler>()
                        .AddSingleton<IStateMachineEventHandler, UnsubscriptionHandler>()
                        .AddTransient(_ =>
                            StateMachinesContextHolder.StateMachineContext.Value ??
                            throw new InvalidOperationException(
                                $"No service for type '{typeof(IStateMachineContext).FullName}' is available in this context.")
                        )
                        .AddTransient(_ =>
                            StateMachinesContextHolder.StateContext.Value ??
                            throw new InvalidOperationException(
                                $"No service for type '{typeof(IStateContext).FullName}' is available in this context.")
                        )
                        .AddTransient(_ =>
                            StateMachinesContextHolder.TransitionContext.Value ??
                            throw new InvalidOperationException(
                                $"No service for type '{typeof(ITransitionContext).FullName}' is available in this context.")
                        )
                        .AddTransient(_ =>
                            StateMachinesContextHolder.ExecutionContext.Value ??
                            throw new InvalidOperationException(
                                $"No service for type '{typeof(IExecutionContext).FullName}' is available in this context.")
                        )
                        .AddTransient(_ =>
                            StateMachinesContextHolder.Inspection.Value ??
                            throw new InvalidOperationException(
                                $"No service for type '{typeof(IStateMachineInspection).FullName}' is available in this context.")
                        )
                        ;
                }

                return register;
            }
        }
    }
}
