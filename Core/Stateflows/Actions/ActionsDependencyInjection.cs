using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions.Engine;
using Stateflows.Actions.Registration;
using Stateflows.Actions.Service;
using Stateflows.Activities;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Initializer;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows.Actions
{
    public static class ActionsDependencyInjection
    {
        private static readonly Dictionary<IStateflowsBuilder, ActionsRegister> Registers = new Dictionary<IStateflowsBuilder, ActionsRegister>();

        [DebuggerHidden]
        public static IStateflowsBuilder AddActions(this IStateflowsBuilder stateflowsBuilder, ActionsBuildAction buildAction = null)
        {
            var register = stateflowsBuilder.EnsureActivitiesServices();
            buildAction?.Invoke(register);

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddDefaultInstance<TAction>(this IStateflowsBuilder stateflowsBuilder, DefaultInstanceInitializationRequestFactoryAsync initializationRequestFactoryAsync = null)
            where TAction : class, IAction
            => stateflowsBuilder.AddDefaultInstance(new StateMachineClass(Action<TAction>.Name).BehaviorClass, initializationRequestFactoryAsync);

        private static ActionsRegister EnsureActivitiesServices(this IStateflowsBuilder stateflowsBuilder)
        {
            lock (Registers)
            {
                if (!Registers.TryGetValue(stateflowsBuilder, out var register))
                {
                    register = new ActionsRegister(stateflowsBuilder as StateflowsBuilder, stateflowsBuilder.ServiceCollection);
                    Registers.Add(stateflowsBuilder, register);

                    stateflowsBuilder
                        .EnsureStateflowServices()
                        .ServiceCollection
                        .AddSingleton(register)
                        .AddSingleton<IActionsRegister>(register)
                        .AddScoped<IEventProcessor, Processor>()
                        .AddTransient<IBehaviorProvider, Provider>()
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
