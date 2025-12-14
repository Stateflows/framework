using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Stateflows.Activities.Attributes;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities.Registration.Builders
{
    internal class ActivitiesBuilder(ActivitiesRegister register, bool systemRegistrations) : IActivitiesBuilder
    {
        [DebuggerHidden]
        public IActivitiesBuilder AddFromAssembly(Assembly assembly)
        {
            assembly.GetAttributedTypes<ActivityBehaviorAttribute>().ToList().ForEach(@type =>
            {
                if (!typeof(IActivity).IsAssignableFrom(@type))
                {
                    return;
                }

                var attribute = @type.GetCustomAttributes(typeof(ActivityBehaviorAttribute)).FirstOrDefault() as ActivityBehaviorAttribute;

                if (register is IIsSystemRegistration registration)
                {
                    // Register is a singleton, modify IsSystemRegistration only for the duration of the AddAction call
                    var valueBefore = registration.IsSystemRegistration;
                    registration.IsSystemRegistration = systemRegistrations;

                    register.AddActivity(attribute?.Name ?? @type.FullName, attribute?.Version ?? 1, @type);
                    registration.IsSystemRegistration = valueBefore;

                    return;
                }

                register.AddActivity(attribute?.Name ?? @type.FullName, attribute?.Version ?? 1, @type);
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
            if (register is IIsSystemRegistration registration)
            {
                // Register is a singleton, modify IsSystemRegistration only for the duration of the AddAction call
                var valueBefore = registration.IsSystemRegistration;
                registration.IsSystemRegistration = systemRegistrations;

                register.AddActivity(activityName, version, buildAction);
                registration.IsSystemRegistration = valueBefore;

                return this;
            }

            register.AddActivity(activityName, version, buildAction);

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddActivity<TActivity>(string activityName = null, int version = 1)
            where TActivity : class, IActivity
        {
            if (register is IIsSystemRegistration registration)
            {
                // Register is a singleton, modify IsSystemRegistration only for the duration of the AddAction call
                var valueBefore = registration.IsSystemRegistration;
                registration.IsSystemRegistration = systemRegistrations;

                register.AddActivity<TActivity>(activityName ?? Activity<TActivity>.Name, version);
                registration.IsSystemRegistration = valueBefore;

                return this;
            }

            register.AddActivity<TActivity>(activityName ?? Activity<TActivity>.Name, version);

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
