using System.Diagnostics;
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
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Context;
using System;
using Stateflows.Common;

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
                    //.AddSingleton<IStateMachineEventHandler, InitializationHandler>()
                    .AddSingleton<IStateMachineEventHandler, BehaviorStatusRequestHandler>()
                    .AddSingleton<IStateMachineEventHandler, CurrentStateRequestHandler>()
                    .AddSingleton<IStateMachineEventHandler, InitializeHandler>()
                    .AddSingleton<IStateMachineEventHandler, FinalizationHandler>()
                    .AddSingleton<IStateMachineEventHandler, ResetHandler>()
                    .AddSingleton<IStateMachineEventHandler, SubscriptionHandler>()
                    .AddSingleton<IStateMachineEventHandler, UnsubscriptionHandler>()
                    .AddSingleton<IStateMachineEventHandler, NotificationsHandler>()
                    .AddTransient<IStateMachineContext>(provider =>
                    {
                        if (ContextHolder.StateMachineContext.Value == null)
                        {
                            throw new InvalidOperationException($"No service for type '{typeof(IStateMachineContext).FullName}' is available in this context.");
                        }

                        return ContextHolder.StateMachineContext.Value;
                    })
                    .AddTransient<IStateContext>(provider =>
                    {
                        if (ContextHolder.StateContext.Value == null)
                        {
                            throw new InvalidOperationException($"No service for type '{typeof(IStateContext).FullName}' is available in this context.");
                        }

                        return ContextHolder.StateContext.Value;
                    })
                    .AddTransient<IExecutionContext>(provider =>
                    {
                        if (ContextHolder.ExecutionContext.Value == null)
                        {
                            throw new InvalidOperationException($"No service for type '{typeof(IExecutionContext).FullName}' is available in this context.");
                        }

                        return ContextHolder.ExecutionContext.Value;
                    })
                ;
            }

            return register;
        }
    }
}
