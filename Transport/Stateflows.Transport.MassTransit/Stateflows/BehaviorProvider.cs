using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Transport.Common.Classes;
using Stateflows.Transport.MassTransit.MassTransit;

namespace Stateflows.Transport.MassTransit.Stateflows
{
    internal class BehaviorProvider : BehaviorClassesRepository, IBehaviorProvider
    {
        private IBus Bus { get; }

        public bool IsLocal => false;

        public event ActionAsync<IBehaviorProvider> BehaviorClassesChanged;

        public BehaviorProvider(IServiceProvider serviceProvider)
            : base(serviceProvider, serviceProvider.GetService<Discoverer>())
        {
            Bus = serviceProvider.GetService<IBus>();
        }

        public bool TryProvideBehavior(BehaviorId id, out IBehavior behavior)
        {
            behavior = BehaviorClasses.Any(c => id.BehaviorClass == c)
                ? new Behavior(Bus, id)
                : null;

            return behavior != null;
        }

        protected override Task OnBehaviorClassesChanged()
        {
            return BehaviorClassesChanged?.Invoke(this) ?? Task.CompletedTask;
        }
    }
}
