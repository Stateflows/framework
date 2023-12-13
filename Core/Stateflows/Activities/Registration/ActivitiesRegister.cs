using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Models;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Exceptions;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Registration
{
    internal class ActivitiesRegister
    {
        private IServiceCollection Services { get; }

        //public List<ExceptionHandlerFactory> GlobalExceptionHandlerFactories { get; set; } = new List<ExceptionHandlerFactory>();

        //public List<InterceptorFactory> GlobalInterceptorFactories { get; set; } = new List<InterceptorFactory>();

        //public List<ObserverFactory> GlobalObserverFactories { get; set; } = new List<ObserverFactory>();

        public ActivitiesRegister(IServiceCollection services)
        {
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
        public void AddActivity(string activityName, int version, ActivityBuilderAction buildAction)
        {
            var key = $"{activityName}.{version}";
            var currentKey = $"{activityName}.current";

            if (Activities.ContainsKey(key))
            {
                throw new ActivityDefinitionException($"Activity '{activityName}' with version '{version}' is already registered");
            }

            var builder = new ActivityBuilder(activityName, version, null, Services);
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

            Services.RegisterActivity(activityType);

            var activity = FormatterServices.GetUninitializedObject(activityType) as Activity;

            var builder = new ActivityBuilder(activityName, version, null, Services);
            builder.AddActivityEvents(activityType);
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
            where TActivity : Activity
            => AddActivity(activityName, version, typeof(TActivity));

        //public void AddGlobalInterceptor(InterceptorFactory interceptorFactory)
        //    => GlobalInterceptorFactories.Add(interceptorFactory);

        //public void AddGlobalInterceptor<TInterceptor>()
        //    where TInterceptor : class, IActivityInterceptor
        //{
        //    Services.RegisterInterceptor<TInterceptor>();
        //    AddGlobalInterceptor(serviceProvider => serviceProvider.GetRequiredService<TInterceptor>());
        //}

        //public void AddGlobalExceptionHandler(ExceptionHandlerFactory exceptionHandlerFactory)
        //    => GlobalExceptionHandlerFactories.Add(exceptionHandlerFactory);

        //public void AddGlobalExceptionHandler<TExceptionHandler>()
        //    where TExceptionHandler : class, IActivityExceptionHandler
        //{
        //    Services.RegisterExceptionHandler<TExceptionHandler>();
        //    AddGlobalExceptionHandler(serviceProvider => serviceProvider.GetRequiredService<TExceptionHandler>());
        //}

        //public void AddGlobalObserver(ObserverFactory observerFactory)
        //    => GlobalObserverFactories.Add(observerFactory);

        //public void AddGlobalObserver<TObserver>()
        //    where TObserver : class, IActivityObserver
        //{
        //    Services.RegisterObserver<TObserver>();
        //    AddGlobalObserver(serviceProvider => serviceProvider.GetRequiredService<TObserver>());
        //}
    }
}
