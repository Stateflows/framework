using System.Threading.Tasks;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    internal class TransitionActivityBuilder<TEvent> :
        BaseEmbeddedBehaviorBuilder,
        ITransitionActivityBuilder<TEvent>,
        IInstantiatedTransitionActivityBuilder<TEvent>
    {
        private TransitionBehaviorInstanceBuilderAsync<TEvent> InstanceBuilder { get; set; } = null;
        
        public async Task<string> GetInstanceAsync(ITransitionContext<TEvent> context, string defaultInstance)
            => InstanceBuilder != null
                ? await InstanceBuilder(context)
                : defaultInstance;
        
        public TransitionBehaviorInitializationBuilderAsync<TEvent, object> InitializationBuilder { get; private set; } = null;

        public TransitionActivityBuilder(TransitionActivityBuildAction<TEvent> buildAction)
        {
            buildAction?.Invoke(this);
        }

        public IInstantiatedTransitionActivityBuilder<TEvent> InstantiateAs(TransitionBehaviorInstanceBuilderAsync<TEvent> builderAsync)
        {
            if (builderAsync != null)
            {
                InstanceBuilder = builderAsync;
            }

            return this;
        }

        public void InitializeWith<TInitializationEvent>(TransitionBehaviorInitializationBuilderAsync<TEvent, TInitializationEvent> builderAsync)
        {
            if (builderAsync != null)
            {
                InitializationBuilder = async c => await builderAsync(c);
            }
        }
    }
}
