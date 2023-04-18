using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class BaseContext
    {
        public BaseContext(RootContext context)
        {
            Context = context;
        }

        public RootContext Context { get; }

        public StateMachineContext stateMachine;
        public StateMachineContext StateMachine => stateMachine ?? (stateMachine = new StateMachineContext(Context));

        private IBehaviorLocator behaviorLocator;
        private IBehaviorLocator BehaviorLocator => behaviorLocator ?? (behaviorLocator = Context.Executor.ServiceProvider.GetService<IBehaviorLocator>());

        public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
            => BehaviorLocator.TryLocateBehavior(id, out behavior);
    }
}
