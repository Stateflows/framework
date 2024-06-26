﻿using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Initializer;
using Stateflows.Common.Registration.Interfaces;
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
            where TStateMachine : StateMachine
            => stateflowsBuilder.AddDefaultInstance(new StateMachineClass(StateMachineInfo<TStateMachine>.Name).BehaviorClass, initializationRequestFactoryAsync);

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
                    .AddSingleton<IStateMachineEventHandler, InitializationHandler>()
                    .AddSingleton<IStateMachineEventHandler, BehaviorStatusRequestHandler>()
                    .AddSingleton<IStateMachineEventHandler, CurrentStateRequestHandler>()
                    .AddSingleton<IStateMachineEventHandler, FinalizationHandler>()
                    .AddSingleton<IStateMachineEventHandler, ResetHandler>()
                    .AddSingleton<IStateMachineEventHandler, SubscriptionHandler>()
                    .AddSingleton<IStateMachineEventHandler, UnsubscriptionHandler>()
                    .AddSingleton<IStateMachineEventHandler, NotificationsHandler>()
                    ;
            }

            return register;
        }
    }
}
