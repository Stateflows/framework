using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Models;
using Stateflows.Activities.Exceptions;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Registration.Builders;

namespace Stateflows.Activities.Registration
{
    internal class ActivitiesRegister
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
                throw new ActivityDefinitionException($"Activity '{activityName}' with version '{version}' is already registered");
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
                throw new ActivityDefinitionException($"Activity '{activityName}' with version '{version}' is already registered");
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
        public void AddActivity<TActivity>(string activityName, int version)
            where TActivity : class, IActivity
            => AddActivity(activityName, version, typeof(TActivity));

        #region Observability
        [DebuggerHidden]
        public void AddGlobalInterceptor(ActivityInterceptorFactory interceptorFactory)
            => GlobalInterceptorFactories.Add(interceptorFactory);

        [DebuggerHidden]
        public void AddGlobalInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor
        {
            Services.AddServiceType<TInterceptor>();
            AddGlobalInterceptor(serviceProvider => serviceProvider.GetRequiredService<TInterceptor>());
        }

        [DebuggerHidden]
        public void AddGlobalExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory)
            => GlobalExceptionHandlerFactories.Add(exceptionHandlerFactory);

        [DebuggerHidden]
        public void AddGlobalExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler
        {
            Services.AddServiceType<TExceptionHandler>();
            AddGlobalExceptionHandler(serviceProvider => serviceProvider.GetRequiredService<TExceptionHandler>());
        }

        [DebuggerHidden]
        public void AddGlobalObserver(ActivityObserverFactory observerFactory)
            => GlobalObserverFactories.Add(observerFactory);

        [DebuggerHidden]
        public void AddGlobalObserver<TObserver>()
            where TObserver : class, IActivityObserver
        {
            Services.AddServiceType<TObserver>();
            AddGlobalObserver(serviceProvider => serviceProvider.GetRequiredService<TObserver>());
        }
        #endregion
    }
}
