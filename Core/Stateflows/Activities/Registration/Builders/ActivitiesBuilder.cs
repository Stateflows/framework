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
    internal class ActivitiesBuilder : IActivitiesBuilder
    {
        private readonly ActivitiesRegister Register;

        public ActivitiesBuilder(ActivitiesRegister register)
        {
            Register = register;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddFromAssembly(Assembly assembly)
        {
            assembly.GetAttributedTypes<ActivityBehaviorAttribute>().ToList().ForEach(@type =>
            {
                if (typeof(IActivity).IsAssignableFrom(@type))
                {
                    var attribute = @type.GetCustomAttributes(typeof(ActivityBehaviorAttribute)).FirstOrDefault() as ActivityBehaviorAttribute;
                    Register.AddActivity(attribute?.Name ?? @type.FullName, attribute?.Version ?? 1, @type);
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
            Register.AddActivity(activityName, version, buildAction);

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddActivity<TActivity>(string activityName = null, int version = 1)
            where TActivity : class, IActivity
        {
            Register.AddActivity<TActivity>(activityName ?? Activity<TActivity>.Name, version);

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddActivity<TActivity>(int version)
            where TActivity : class, IActivity
            => AddActivity<TActivity>(null, version);

        #region Observability
        [DebuggerHidden]
        public IActivitiesBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor
        {
            Register.AddInterceptor<TInterceptor>();

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddInterceptor(ActivityInterceptorFactoryAsync interceptorFactoryAsync)
        {
            Register.AddInterceptor(interceptorFactoryAsync);

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler
        {
            Register.AddExceptionHandler<TExceptionHandler>();

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddExceptionHandler(ActivityExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
        {
            Register.AddExceptionHandler(exceptionHandlerFactoryAsync);

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddObserver<TObserver>()
            where TObserver : class, IActivityObserver
        {
            Register.AddObserver<TObserver>();

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddObserver(ActivityObserverFactoryAsync observerFactoryAsync)
        {
            Register.AddObserver(observerFactoryAsync);

            return this;
        }
        #endregion
    }
}
