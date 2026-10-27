using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions.Context;
using Stateflows.Actions.Engine;
using Stateflows.Actions.Service;
using Stateflows.Actions.Registration;
using Stateflows.Actions.Registration.Builders;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Initializer;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows.Actions
{
    /// <summary>Registers action services, action behaviors, and default action instances with dependency injection.</summary>
    public static class ActionsDependencyInjection
    {
        private static readonly Dictionary<IStateflowsBuilder, ActionsRegister> Registers = new();

        internal static void Cleanup(IStateflowsBuilder builder)
        {
            lock (Registers)
            {
                if (Registers.TryGetValue(builder, out var register) && !register.Actions.Any())
                {
                    var serviceDescriptor = builder.ServiceCollection.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IActionsRegister));
                    builder.ServiceCollection.Remove(serviceDescriptor);
                }
            }
        }

        /// <summary>Adds action services and registers actions in a Stateflows builder.</summary>
        /// <param name="stateflowsBuilder">The Stateflows service and behavior builder.</param>
        /// <param name="buildAction">Callback that declares action registrations.</param>
        /// <returns>The Stateflows builder for further configuration.</returns>
        /// <remarks>Call during application configuration before the Stateflows builder is built.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/actions">Actions documentation</seealso>
        [DebuggerHidden]
        public static IStateflowsBuilder AddActions(this IStateflowsBuilder stateflowsBuilder, ActionsBuildAction buildAction = null)
            => AddActions(stateflowsBuilder, buildAction, null, null).StateflowsBuilder;
        
        [DebuggerHidden]
        internal static (ActionsRegister ActionsRegister, IStateflowsBuilder StateflowsBuilder) AddActions(this IStateflowsBuilder stateflowsBuilder, ActionsBuildAction buildAction, BehaviorClass? ownerClass, BehaviorClass? parentClass)
        {
            var register = stateflowsBuilder.EnsureActionsServices();
            buildAction?.Invoke(new ActionsBuilder(register, ownerClass, parentClass));

            return (register, stateflowsBuilder);
        }

        /// <summary>Registers a default instance of a typed action behavior.</summary>
        /// <typeparam name="TAction">The action implementation type.</typeparam>
        /// <param name="stateflowsBuilder">The Stateflows builder.</param>
        /// <param name="initializationRequestFactoryAsync">Optional factory for the default instance's initialization request.</param>
        /// <returns>The Stateflows builder for further configuration.</returns>
        [DebuggerHidden]
        public static IStateflowsBuilder AddDefaultInstance<TAction>(this IStateflowsBuilder stateflowsBuilder, DefaultInstanceInitializationRequestFactoryAsync initializationRequestFactoryAsync = null)
            where TAction : class, IAction
            => stateflowsBuilder.AddDefaultInstance(new StateMachineClass(Action<TAction>.Name).BehaviorClass, initializationRequestFactoryAsync);

        internal static ActionsRegister EnsureActionsServices(this IStateflowsBuilder stateflowsBuilder)
        {
            lock (Registers)
            {
                if (!Registers.TryGetValue(stateflowsBuilder, out var register))
                {
                    register = new ActionsRegister()
                    {
                        EventsRegister = (stateflowsBuilder as StateflowsBuilder)?.Events
                    };
                    Registers.Add(stateflowsBuilder, register);

                    stateflowsBuilder
                        .EnsureStateflowServices()
                        .ServiceCollection
                        .AddSingleton(register)
                        .AddSingleton<IActionsRegister>(register)
                        .AddScoped<IEventProcessor, Processor>()
                        .AddTransient<IBehaviorProvider, Provider>()
                        
                        .AddTransient(typeof(IInputTokens<>), typeof(InputTokens<>))
                        .AddTransient(typeof(IInputToken<>), typeof(InputToken<>))
                        .AddTransient(typeof(IOptionalInputTokens<>), typeof(OptionalInputTokens<>))
                        .AddTransient(typeof(IOptionalInputToken<>), typeof(OptionalInputToken<>))

                        .AddTransient(typeof(IInputOutputTokens<>), typeof(InputOutputTokens<>))
                        .AddTransient(typeof(IInputOutputToken<>), typeof(InputOutputToken<>))
                        .AddTransient(typeof(IOptionalInputOutputTokens<>), typeof(OptionalInputOutputTokens<>))
                        .AddTransient(typeof(IOptionalInputOutputToken<>), typeof(OptionalInputOutputToken<>))

                        .AddTransient(typeof(IOutputTokens<>), typeof(OutputTokens<>))
                        .AddTransient(_ =>
                            ActionsContextHolder.ExecutionContext.Value ??
                            throw new InvalidOperationException($"No service for type '{typeof(IExecutionContext).FullName}' is available in this context.")
                        )
                        .AddTransient(_ =>
                            ActionsContextHolder.ActionContext.Value ??
                            throw new InvalidOperationException(
                                $"No service for type '{typeof(IActionContext).FullName}' is available in this context.")
                        )
                        ;
                }

                return register;
            }
        }

        internal static void Build(IStateflowsBuilder builder)
        {
            var stateflowsBuilder = (StateflowsBuilder)builder;
            lock (Registers)
            {
                if (
                    builder.ServiceCollection.IsServiceRegistered<IActionsRegister>() &&
                    Registers.TryGetValue(builder, out var register)
                )
                {
                    foreach (var model in register.Actions.Values)
                    {
                        model.Build(stateflowsBuilder);
                    }
                }
            }
        }
    }
}
