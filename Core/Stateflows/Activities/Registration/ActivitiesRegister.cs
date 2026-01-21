using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        public List<ActivityExceptionHandlerFactoryAsync> GlobalExceptionHandlerFactories = [];

        public List<ActivityInterceptorFactoryAsync> GlobalInterceptorFactories = [];

        public List<ActivityObserverFactoryAsync> GlobalObserverFactories = [];

        private readonly StateflowsBuilder stateflowsBuilder = null;

        public ActivitiesRegister(StateflowsBuilder stateflowsBuilder)
        {
            this.stateflowsBuilder = stateflowsBuilder;
        }

        public readonly Dictionary<string, Graph> Activities = [];

        public readonly Dictionary<string, int> CurrentVersions = [];

        private readonly MethodInfo ActivityTypeAddedAsyncMethod =
            typeof(IActivityVisitor).GetMethod(nameof(IActivityVisitor.ActivityTypeAddedAsync));

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

        private static void RegisterActivity(Type activityType, ActivityBuilder activityBuilder)
        {
            var staticRegister = activityType.GetMethod(
                nameof(IActivity.Build),
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: [ typeof(ActivityBuilder) ],
                modifiers: null
            );

            staticRegister.Invoke(null, [ activityBuilder ]);
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

            var activityBuilder = new ActivityBuilder(activityName, version, null, stateflowsBuilder);
            buildAction(activityBuilder);
            activityBuilder.Graph.Build();

            activityBuilder.Graph.VisitingTasks.Add(v => v.ActivityAddedAsync(activityName, version));

            Activities.Add(key, activityBuilder.Graph);

            if (IsNewestVersion(activityName, version))
            {
                Activities[currentKey] = activityBuilder.Graph;
            }
        }

        [DebuggerHidden]
        public void AddActivity(string activityName, int version, Type activityType, ActivityUtilsBuildAction buildAction = null)
        {
            var key = $"{activityName}.{version}";
            var currentKey = $"{activityName}.current";

            if (Activities.ContainsKey(key))
            {
                throw new ActivityDefinitionException($"Activity '{activityName}' with version '{version}' is already registered", new ActivityClass(activityName));
            }

            var activityBuilder = new ActivityBuilder(activityName, version, null, stateflowsBuilder)
            {
                Graph =
                {
                    ActivityType = activityType
                }
            };
            RegisterActivity(activityType, activityBuilder);
            activityBuilder.Graph.Build();
            buildAction?.Invoke(new ActivityUtilsBuilder(activityBuilder.Graph));

            var method = ActivityTypeAddedAsyncMethod.MakeGenericMethod(activityType);
            
            activityBuilder.Graph.VisitingTasks.AddRange(
            [
                v => v.ActivityAddedAsync(activityName, version),
                v => (Task)method.Invoke(v, [ activityName, version ])
            ]);

            Activities.Add(key, activityBuilder.Graph);

            if (IsNewestVersion(activityName, version))
            {
                Activities[currentKey] = activityBuilder.Graph;
            }
        }

        [DebuggerHidden]
        public void AddActivity<TActivity>(string activityName = null, int version = 1, ActivityUtilsBuildAction buildAction = null)
            where TActivity : class, IActivity
            => AddActivity(activityName ?? Activity<TActivity>.Name, version, typeof(TActivity), buildAction);

        public async Task VisitActivitiesAsync(IActivityVisitor visitor)
        {
            var tasks = Activities
                .Where((item, index) => !item.Key.EndsWith(".current"))
                .Select(item => item.Value)
                .SelectMany(graph => graph.VisitingTasks);
            
            foreach (var task in tasks)
            {
                await task(visitor);
            }
        }

        public async Task VisitActivitiesAsync(string activityName, int version, IActivityVisitor visitor)
        {
            var tasks = Activities
                .Where(item => item.Key == $"{activityName}.{version}")
                .Select(item => item.Value)
                .SelectMany(graph => graph.VisitingTasks);
            
            foreach (var task in tasks)
            {
                await task(visitor);
            }
        }

        #region Observability
        [DebuggerHidden]
        public void AddInterceptor(ActivityInterceptorFactoryAsync interceptorFactory)
            => GlobalInterceptorFactories.Add(interceptorFactory);

        [DebuggerHidden]
        public void AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor
            => AddInterceptor(async serviceProvider => await StateflowsActivator.CreateModelElementInstanceAsync<TInterceptor>(serviceProvider, "interceptor"));
        
        [DebuggerHidden]
        public void AddExceptionHandler(ActivityExceptionHandlerFactoryAsync exceptionHandlerFactory)
            => GlobalExceptionHandlerFactories.Add(exceptionHandlerFactory);

        [DebuggerHidden]
        public void AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler
            => AddExceptionHandler(async serviceProvider => await StateflowsActivator.CreateModelElementInstanceAsync<TExceptionHandler>(serviceProvider, "exception handler"));
        
        [DebuggerHidden]
        public void AddObserver(ActivityObserverFactoryAsync observerFactory)
            => GlobalObserverFactories.Add(observerFactory);

        [DebuggerHidden]
        public void AddObserver<TObserver>()
            where TObserver : class, IActivityObserver
            => AddObserver(async serviceProvider => await StateflowsActivator.CreateModelElementInstanceAsync<TObserver>(serviceProvider, "observer"));
        #endregion
    }
}
