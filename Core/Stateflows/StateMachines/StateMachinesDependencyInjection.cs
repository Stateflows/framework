﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Initializer;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.StateMachines.Context;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.EventHandlers;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class SystemDependencyInjection
    {
        private readonly static Dictionary<IStateflowsBuilder, StateMachinesRegister> Registers = new Dictionary<IStateflowsBuilder, StateMachinesRegister>();

        [DebuggerHidden]
        public static IStateflowsBuilder AddStateMachines(this IStateflowsBuilder stateflowsBuilder, StateMachinesBuildAction buildAction)
        {
            buildAction(new StateMachinesBuilder(stateflowsBuilder.EnsureStateMachinesServices()));

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddDefaultInstance<TStateMachine>(this IStateflowsBuilder stateflowsBuilder, DefaultInstanceInitializationRequestFactoryAsync initializationRequestFactoryAsync = null)
            where TStateMachine : class, IStateMachine
            => stateflowsBuilder.AddDefaultInstance(new StateMachineClass(StateMachine<TStateMachine>.Name).BehaviorClass, initializationRequestFactoryAsync);

        [DebuggerHidden]
        private static StateMachinesRegister EnsureStateMachinesServices(this IStateflowsBuilder stateflowsBuilder)
        {
            if (!Registers.TryGetValue(stateflowsBuilder, out var register))
            {
                register = new StateMachinesRegister(stateflowsBuilder.ServiceCollection);
                Registers.Add(stateflowsBuilder, register);

                stateflowsBuilder
                    .EnsureStateflowServices()
                    .ServiceCollection
                    .AddScoped<IStateMachinePlugin, TimeEvents>()
                    .AddScoped<IStateMachinePlugin, Behaviors>()
                    .AddScoped<IStateMachinePlugin, ContextCleanup>()
                    .AddScoped<IStateMachinePlugin, Notifications>()
                    .AddSingleton(register)
                    .AddSingleton<IEventProcessor, Processor>()
                    .AddTransient<IBehaviorProvider, Provider>()
                    .AddSingleton<IStateMachineEventHandler, BehaviorStatusRequestHandler>()
                    .AddSingleton<IStateMachineEventHandler, CurrentStateRequestHandler>()
                    .AddSingleton<IStateMachineEventHandler, InitializeHandler>()
                    .AddSingleton<IStateMachineEventHandler, FinalizationHandler>()
                    .AddSingleton<IStateMachineEventHandler, ResetHandler>()
                    .AddSingleton<IStateMachineEventHandler, SubscriptionHandler>()
                    .AddSingleton<IStateMachineEventHandler, UnsubscriptionHandler>()
                    .AddSingleton<IStateMachineEventHandler, NotificationsHandler>()
                    .AddTransient(provider =>
                        ContextHolder.StateMachineContext.Value ??
                        throw new InvalidOperationException($"No service for type '{typeof(IStateMachineContext).FullName}' is available in this context.")
                    )
                    .AddTransient(provider =>
                        ContextHolder.StateContext.Value ?? 
                        throw new InvalidOperationException($"No service for type '{typeof(IStateContext).FullName}' is available in this context.")
                    )
                    .AddTransient(provider =>
                        ContextHolder.TransitionContext.Value ??
                        throw new InvalidOperationException($"No service for type '{typeof(ITransitionContext).FullName}' is available in this context.")
                    )
                    .AddTransient(provider =>
                        ContextHolder.ExecutionContext.Value ??
                        throw new InvalidOperationException($"No service for type '{typeof(IExecutionContext).FullName}' is available in this context.")
                    )
                ;
            }

            return register;
        }
    }
}
