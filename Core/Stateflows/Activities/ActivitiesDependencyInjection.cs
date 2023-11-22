using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Attributes;
using Stateflows.Activities.Registration;
using Stateflows.Activities.EventHandlers;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ActivitiesDependencyInjection
    {
        [DebuggerHidden]
        public static IStateflowsBuilder AddActivities(this IStateflowsBuilder stateflowsBuilder, Assembly assembly)
        {
            assembly.GetAttributedTypes<ActivityAttribute>().ToList().ForEach(@type =>
            {
                if (typeof(Activity).IsAssignableFrom(@type))
                {
                    var attribute = @type.GetCustomAttributes(typeof(ActivityAttribute)).FirstOrDefault() as ActivityAttribute;
                    stateflowsBuilder.EnsureActivitiesServices().AddActivity(attribute?.Name ?? @type.FullName, @type);
                }
            });

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddActivities(this IStateflowsBuilder stateflowsBuilder, IEnumerable<Assembly> assemblies = null)
        {
            if (assemblies == null)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }

            foreach (var assembly in assemblies)
            {
                stateflowsBuilder.AddActivities(assembly);
            }

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddActivity(this IStateflowsBuilder stateflowsBuilder, string activityName, ActivityBuilderAction buildAction)
        {
            stateflowsBuilder.EnsureActivitiesServices().AddActivity(activityName, buildAction);

            return stateflowsBuilder;
        }

        [DebuggerHidden]
        public static IStateflowsBuilder AddActivity<TActivity>(this IStateflowsBuilder stateflowsBuilder, string activityName = null)
            where TActivity : Activity
        {
            stateflowsBuilder.EnsureActivitiesServices().AddActivity<TActivity>(activityName ?? ActivityInfo<TActivity>.Name);

            return stateflowsBuilder;
        }

        private readonly static Dictionary<IStateflowsBuilder, ActivitiesRegister> Registers = new Dictionary<IStateflowsBuilder, ActivitiesRegister>();

        private static ActivitiesRegister EnsureActivitiesServices(this IStateflowsBuilder stateflowsBuilder)
        {

            if (!Registers.TryGetValue(stateflowsBuilder, out var register))
            {
                register = new ActivitiesRegister(stateflowsBuilder.ServiceCollection);
                Registers.Add(stateflowsBuilder, register);

                stateflowsBuilder
                    .EnsureStateflowServices()
                    .ServiceCollection
                    //.AddGlobalObserver(p => p.GetRequiredService<Observer>())
                    //.AddGlobalInterceptor(p => p.GetRequiredService<Observer>())
                    //.AddScoped<Observer>()
                    .AddSingleton(register)
                    .AddScoped<IEventProcessor, Processor>()
                    .AddTransient<IBehaviorProvider, Provider>()
                    .AddSingleton<IActivityEventHandler, InitializationHandler>()
                    .AddSingleton<IActivityEventHandler, ExecutionHandler>()
                    .AddSingleton<IActivityEventHandler, ExitHandler>()
                    //.AddSingleton<IActivityEventHandler, InitializedHandler>()
                    //.AddSingleton<IActivityEventHandler, CurrentStateHandler>()
                    //.AddSingleton<IActivityEventHandler, ExpectedEventsHandler>()
                    //.AddSingleton<IActivityEventHandler, ExitHandler>()
                    ;
            }

            return register;
        }
    }
}
