using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Attributes;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Registration.Builders
{
    internal class ActivitiesBuilder(ActivitiesRegister register, bool systemRegistrations) : IActivitiesBuilder
    {
        private readonly bool SystemRegistrations = systemRegistrations;

        [DebuggerHidden]
        public IActivitiesBuilder AddFromAssembly(Assembly assembly)
        {
            assembly.GetAttributedTypes<ActivityBehaviorAttribute>().ToList().ForEach(@type =>
            {
                if (typeof(IActivity).IsAssignableFrom(@type))
                {
                    var attribute = @type.GetCustomAttributes(typeof(ActivityBehaviorAttribute)).FirstOrDefault() as ActivityBehaviorAttribute;
                    register.AddActivity(attribute?.Name ?? @type.FullName, attribute?.Version ?? 1, @type);
                }
            });

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                AddFromAssembly(assembly);
            }

            return this;
        }


        [DebuggerHidden]
        public IActivitiesBuilder AddFromLoadedAssemblies()
            => AddFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        [DebuggerHidden]
        public IActivitiesBuilder AddActivity(string activityName, ReactiveActivityBuildAction buildAction)
            => AddActivity(activityName, 1, buildAction);

        [DebuggerHidden]
        public IActivitiesBuilder AddActivity(string activityName, int version, ReactiveActivityBuildAction buildAction)
        {
            register.AddActivity(activityName, version, buildAction);

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddActivity<TActivity>(string activityName = null, int version = 1, ActivityUtilsBuildAction buildAction = null)
            where TActivity : class, IActivity
        {
            register.AddActivity<TActivity>(activityName ?? Activity<TActivity>.Name, version, buildAction);

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddActivity<TActivity>(int version, ActivityUtilsBuildAction buildAction = null)
            where TActivity : class, IActivity
            => AddActivity<TActivity>(null, version, buildAction);

        #region Observability
        [DebuggerHidden]
        public IActivitiesBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor
        {
            register.AddInterceptor<TInterceptor>();

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddInterceptor(ActivityInterceptorFactoryAsync interceptorFactoryAsync)
        {
            register.AddInterceptor(interceptorFactoryAsync);

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler
        {
            register.AddExceptionHandler<TExceptionHandler>();

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddExceptionHandler(ActivityExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
        {
            register.AddExceptionHandler(exceptionHandlerFactoryAsync);

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddObserver<TObserver>()
            where TObserver : class, IActivityObserver
        {
            register.AddObserver<TObserver>();

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddObserver(ActivityObserverFactoryAsync observerFactoryAsync)
        {
            register.AddObserver(observerFactoryAsync);

            return this;
        }
        #endregion
    }
}
