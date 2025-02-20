using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Classes;
using Stateflows.Common.Registration.Builders;
using Stateflows.Activities.Models;
using Stateflows.Activities.Exceptions;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Registration
{
    internal class ActivitiesRegister : IActivitiesRegister
    {
        private IServiceCollection Services { get; }

        public List<ActivityExceptionHandlerFactoryAsync> GlobalExceptionHandlerFactories { get; set; } = new List<ActivityExceptionHandlerFactoryAsync>();

        public List<ActivityInterceptorFactoryAsync> GlobalInterceptorFactories { get; set; } = new List<ActivityInterceptorFactoryAsync>();

        public List<ActivityObserverFactoryAsync> GlobalObserverFactories { get; set; } = new List<ActivityObserverFactoryAsync>();

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

            var activity = StateflowsActivator.CreateUninitializedInstance(activityType) as IActivity;

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
            => GlobalInterceptorFactories.Add(serviceProvider => Task.FromResult(interceptorFactory(serviceProvider)));
        
        [DebuggerHidden]
        public void AddInterceptor(ActivityInterceptorFactoryAsync interceptorFactory)
            => GlobalInterceptorFactories.Add(interceptorFactory);

        [DebuggerHidden]
        public void AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor
            => AddInterceptor(async serviceProvider => await StateflowsActivator.CreateInstanceAsync<TInterceptor>(serviceProvider, "interceptor"));

        [DebuggerHidden]
        public void AddExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory)
            => GlobalExceptionHandlerFactories.Add(serviceProvider => Task.FromResult(exceptionHandlerFactory(serviceProvider)));
        
        [DebuggerHidden]
        public void AddExceptionHandler(ActivityExceptionHandlerFactoryAsync exceptionHandlerFactory)
            => GlobalExceptionHandlerFactories.Add(exceptionHandlerFactory);

        [DebuggerHidden]
        public void AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler
            => AddExceptionHandler(async serviceProvider => await StateflowsActivator.CreateInstanceAsync<TExceptionHandler>(serviceProvider, "exception handler"));

        [DebuggerHidden]
        public void AddObserver(ActivityObserverFactory observerFactory)
            => GlobalObserverFactories.Add(serviceProvider => Task.FromResult(observerFactory(serviceProvider)));
        
        [DebuggerHidden]
        public void AddObserver(ActivityObserverFactoryAsync observerFactory)
            => GlobalObserverFactories.Add(observerFactory);

        [DebuggerHidden]
        public void AddObserver<TObserver>()
            where TObserver : class, IActivityObserver
            => AddObserver(async serviceProvider => await StateflowsActivator.CreateInstanceAsync<TObserver>(serviceProvider, "observer"));
        #endregion
    }
}
