using System.Diagnostics;
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
        private static ActivitiesRegister Register;

        [DebuggerHidden]
        public static IStateflowsBuilder AddActivities(this IStateflowsBuilder stateflowsBuilder, ActivitiesBuilderAction buildAction)
        {
            buildAction(new ActivitiesBuilder(stateflowsBuilder.EnsureActivitiesServices()));

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddDefaultInstance<TActivity>(this IStateflowsBuilder stateflowsBuilder, InitializationRequestFactoryAsync initializationRequestFactoryAsync = null)
            where TActivity: Activity
            => stateflowsBuilder.AddDefaultInstance(new ActivityClass(ActivityInfo<TActivity>.Name).BehaviorClass, initializationRequestFactoryAsync);


        private static ActivitiesRegister EnsureActivitiesServices(this IStateflowsBuilder stateflowsBuilder)
        {
            if (Register == null)
            {
                Register = new ActivitiesRegister(stateflowsBuilder.ServiceCollection);

                stateflowsBuilder
                    .EnsureStateflowServices()
                    .ServiceCollection
                    .AddScoped<AcceptEvents>()
                    .AddScoped<IActivityPlugin>(serviceProvider => serviceProvider.GetRequiredService<AcceptEvents>())
                    .AddSingleton(Register)
                    .AddScoped<IEventProcessor, Processor>()
                    .AddTransient<IBehaviorProvider, Provider>()
                    .AddSingleton<IActivityEventHandler, InitializationHandler>()
                    .AddSingleton<IActivityEventHandler, ExecutionHandler>()
                    ;
            }

            return Register;
        }
    }
}
