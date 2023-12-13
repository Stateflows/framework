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
                if (typeof(Activity).IsAssignableFrom(@type))
                {
                    var attribute = @type.GetCustomAttributes(typeof(ActivityBehaviorAttribute)).FirstOrDefault() as ActivityBehaviorAttribute;
                    Register.AddActivity(attribute?.Name ?? @type.FullName, attribute.Version, @type);
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
        public IActivitiesBuilder AddActivity(string activityName, ActivityBuilderAction buildAction)
            => AddActivity(activityName, 1, buildAction);

        [DebuggerHidden]
        public IActivitiesBuilder AddActivity(string activityName, int version, ActivityBuilderAction buildAction)
        {
            Register.AddActivity(activityName, version, buildAction);

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddActivity<TActivity>(string activityName = null, int version = 1)
            where TActivity : Activity
        {
            Register.AddActivity<TActivity>(activityName ?? ActivityInfo<TActivity>.Name, version);

            return this;
        }

        [DebuggerHidden]
        public IActivitiesBuilder AddActivity<TActivity>(int version)
            where TActivity : Activity
            => AddActivity<TActivity>(null, version);
    }
}
