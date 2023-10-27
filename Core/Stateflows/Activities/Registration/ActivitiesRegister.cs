using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Models;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Exceptions;

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

        public Dictionary<string, Graph> Activities { get; set; } = new Dictionary<string, Graph>();

        [DebuggerHidden]
        public void AddActivity(string activityName, ActivityBuilderAction buildAction)
        {
            if (Activities.ContainsKey(activityName))
            {
                throw new ActivityDefinitionException($"Activity '{activityName}' is already registered");
            }

            var builder = new ActivityBuilder(activityName, null, Services);
            buildAction(builder);
            builder.Result.Build();

            Activities.Add(builder.Result.Name, builder.Result);
        }

        [DebuggerHidden]
        public void AddActivity(string activityName, Type activityType)
        {
            if (Activities.ContainsKey(activityName))
            {
                throw new ActivityDefinitionException($"Activity '{activityName}' is already registered");
            }

            Services.RegisterActivity(activityType);

            var activity = FormatterServices.GetUninitializedObject(activityType) as Activity;

            var builder = new ActivityBuilder(activityName, null, Services);
            builder.AddActivityEvents(activityType);
            builder.Result.ActivityType = activityType;
            activity.Build(builder);
            builder.Result.Build();

            Activities.Add(builder.Result.Name, builder.Result);
        }

        [DebuggerHidden]
        public void AddActivity<TActivity>(string stateMachineName)
            where TActivity : Activity
            => AddActivity(stateMachineName, typeof(TActivity));

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
