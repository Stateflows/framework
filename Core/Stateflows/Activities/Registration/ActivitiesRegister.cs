using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Extensions;
using Stateflows.Common.Registration.Builders;
using Stateflows.Activities.Models;
using Stateflows.Activities.Exceptions;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Classes;

namespace Stateflows.Activities.Registration
{
    internal class ActivitiesRegister : IActivitiesRegister
    {
        private IServiceCollection Services { get; }

        public List<ActivityExceptionHandlerFactory> GlobalExceptionHandlerFactories { get; set; } = new List<ActivityExceptionHandlerFactory>();

        public List<ActivityInterceptorFactory> GlobalInterceptorFactories { get; set; } = new List<ActivityInterceptorFactory>();

        public List<ActivityObserverFactory> GlobalObserverFactories { get; set; } = new List<ActivityObserverFactory>();

        private readonly StateflowsBuilder stateflowsBuilder = null;

        public ActivitiesRegister(StateflowsBuilder stateflowsBuilder, IServiceCollection services)
        {
            this.stateflowsBuilder = stateflowsBuilder;
            Services = services;
        }

        public readonly Dictionary<string, Graph> Activities = new Dictionary<string, Graph>();

        public readonly Dictionary<string, int> CurrentVersions = new Dictionary<string, int>();

        private bool IsNewestVersion(string activityName, int version)
        {
            var result = false;

            if (CurrentVersions.TryGetValue(activityName, out var currentVersion))
            {
                if (currentVersion < version)
                {
                    result = true;
                    CurrentVersions[activityName] = version;
                }
            }
            else
            {
                result = true;
                CurrentVersions[activityName] = version;
            }

            return result;
        }

        [DebuggerHidden]
        public void AddActivity(string activityName, int version, ReactiveActivityBuildAction buildAction)
        {
            var key = $"{activityName}.{version}";
            var currentKey = $"{activityName}.current";

            if (Activities.ContainsKey(key))
            {
                throw new ActivityDefinitionException($"Activity '{activityName}' with version '{version}' is already registered", new ActivityClass(activityName));
            }

            var builder = new ActivityBuilder(activityName, version, null, stateflowsBuilder, Services);
            buildAction(builder);
            builder.Result.Build();

            Activities.Add(key, builder.Result);

            if (IsNewestVersion(activityName, version))
            {
                Activities[currentKey] = builder.Result;
            }
        }

        [DebuggerHidden]
        public void AddActivity(string activityName, int version, Type activityType)
        {
            var key = $"{activityName}.{version}";
            var currentKey = $"{activityName}.current";

            if (Activities.ContainsKey(key))
            {
                throw new ActivityDefinitionException($"Activity '{activityName}' with version '{version}' is already registered", new ActivityClass(activityName));
            }

            Services.AddServiceType(activityType);

            var activity = FormatterServices.GetUninitializedObject(activityType) as IActivity;

            var builder = new ActivityBuilder(activityName, version, null, stateflowsBuilder, Services);
            builder.Result.ActivityType = activityType;
            activity.Build(builder);
            builder.Result.Build();

            Activities.Add(key, builder.Result);

            if (IsNewestVersion(activityName, version))
            {
                Activities[currentKey] = builder.Result;
            }
        }

        [DebuggerHidden]
        public void AddActivity<TActivity>(string activityName = null, int version = 1)
            where TActivity : class, IActivity
            => AddActivity(activityName ?? Activity<TActivity>.Name, version, typeof(TActivity));

        #region Observability
        [DebuggerHidden]
        public void AddInterceptor(ActivityInterceptorFactory interceptorFactory)
            => GlobalInterceptorFactories.Add(interceptorFactory);

        [DebuggerHidden]
        public void AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor
            => AddInterceptor(serviceProvider => StateflowsActivator.CreateInstance<TInterceptor>(serviceProvider));

        [DebuggerHidden]
        public void AddExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory)
            => GlobalExceptionHandlerFactories.Add(exceptionHandlerFactory);

        [DebuggerHidden]
        public void AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler
            => AddExceptionHandler(serviceProvider => StateflowsActivator.CreateInstance<TExceptionHandler>(serviceProvider));

        [DebuggerHidden]
        public void AddObserver(ActivityObserverFactory observerFactory)
            => GlobalObserverFactories.Add(observerFactory);

        [DebuggerHidden]
        public void AddObserver<TObserver>()
            where TObserver : class, IActivityObserver
            => AddObserver(serviceProvider => StateflowsActivator.CreateInstance<TObserver>(serviceProvider));
        #endregion
    }
}
