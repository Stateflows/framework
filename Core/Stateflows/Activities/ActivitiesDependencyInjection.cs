using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Initializer;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Activities.Engine;
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
        public static IStateflowsBuilder AddActivities(this IStateflowsBuilder stateflowsBuilder, ActivitiesBuildAction buildAction)
        {
            buildAction(new ActivitiesBuilder(stateflowsBuilder.EnsureActivitiesServices()));

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddDefaultInstance<TActivity>(this IStateflowsBuilder stateflowsBuilder, DefaultInstanceInitializationRequestFactoryAsync initializationRequestFactoryAsync = null)
            where TActivity: Activity
            => stateflowsBuilder.AddDefaultInstance(new ActivityClass(ActivityInfo<TActivity>.Name).BehaviorClass, initializationRequestFactoryAsync);


        private static ActivitiesRegister EnsureActivitiesServices(this IStateflowsBuilder stateflowsBuilder)
        {
            if (!Registers.TryGetValue(stateflowsBuilder, out var register))
            {
                register = new ActivitiesRegister(stateflowsBuilder.ServiceCollection);
                Registers.Add(stateflowsBuilder, register);

                stateflowsBuilder
                    .EnsureStateflowServices()
                    .ServiceCollection
                    .AddScoped<AcceptEvents>()
                    .AddScoped<IActivityPlugin>(serviceProvider => serviceProvider.GetRequiredService<AcceptEvents>())
                    .AddSingleton(register)
                    .AddScoped<IEventProcessor, Processor>()
                    .AddTransient<IBehaviorProvider, Provider>()
                    .AddSingleton<IActivityEventHandler, BehaviorStatusHandler>()
                    .AddSingleton<IActivityEventHandler, InitializationHandler>()
                    .AddSingleton<IActivityEventHandler, ExecutionHandler>()
                    .AddSingleton<IActivityEventHandler, FinalizationHandler>()
                    .AddSingleton<IActivityEventHandler, ResetHandler>()
                    .AddSingleton<IActivityEventHandler, SubscriptionHandler>()
                    .AddSingleton<IActivityEventHandler, UnsubscriptionHandler>()
                    .AddSingleton<IActivityEventHandler, NotificationsHandler>()
                    ;
            }

            return register;
        }
    }
}
