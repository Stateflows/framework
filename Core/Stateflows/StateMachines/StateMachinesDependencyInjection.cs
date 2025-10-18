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
using Stateflows.StateMachines.Classes;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Context;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.EventHandlers;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateMachinesDependencyInjection
    {
        internal static readonly Dictionary<IStateflowsBuilder, StateMachinesRegister> Registers = new Dictionary<IStateflowsBuilder, StateMachinesRegister>();

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
                if (builder.ServiceCollection.IsServiceRegistered<IStateMachinesRegister>() &&
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

        [DebuggerHidden]
        public static IStateflowsBuilder AddStateMachines(this IStateflowsBuilder stateflowsBuilder,
            StateMachinesBuildAction buildAction = null)
            => AddStateMachines(stateflowsBuilder, buildAction, false);

        [DebuggerHidden]
        internal static IStateflowsBuilder AddStateMachines(this IStateflowsBuilder stateflowsBuilder,
            StateMachinesBuildAction buildAction, bool systemRegistrations)
        {
            var register = stateflowsBuilder.EnsureStateMachinesServices();
            buildAction?.Invoke(new StateMachinesBuilder(register, systemRegistrations));

            return stateflowsBuilder;
        }

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
                        .AddScoped<IStateMachineContextProvider, StateMachineContextProvider>()
                        .AddSingleton<IEventProcessor, Processor>()
                        .AddTransient<IBehaviorProvider, Provider>()
                        .AddSingleton<IStateMachineEventHandler, BehaviorStatusRequestHandler>()
                        .AddSingleton<IStateMachineEventHandler, StateMachineInfoRequestHandler>()
                        .AddSingleton<IStateMachineEventHandler, InitializeHandler>()
                        .AddSingleton<IStateMachineEventHandler, FinalizationHandler>()
                        .AddSingleton<IStateMachineEventHandler, ResetHandler>()
                        .AddSingleton<IStateMachineEventHandler, SubscriptionHandler>()
                        .AddSingleton<IStateMachineEventHandler, UnsubscriptionHandler>()
                        .AddSingleton<IStateMachineEventHandler, StartRelayHandler>()
                        .AddSingleton<IStateMachineEventHandler, StopRelayHandler>()
                        .AddSingleton<IStateMachineEventHandler, SetGlobalValuesHandler>()
                        .AddSingleton<IStateMachineEventHandler, SetContextOwnerHandler>()
                        .AddSingleton<IStateMachineEventHandler, ContextValuesRequestHandler>()
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
