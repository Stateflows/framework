using System.Threading.Tasks;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    internal class TransitionActionBuilder<TEvent> :
        BaseEmbeddedBehaviorBuilder,
        ITransitionActionBuilder<TEvent>
    {
        private TransitionBehaviorInstanceBuilderAsync<TEvent> InstanceBuilder { get; set; } = null;
        
        public async Task<string> GetInstanceAsync(ITransitionContext<TEvent> context, string defaultInstance)
            => InstanceBuilder != null
                ? await InstanceBuilder(context)
                : defaultInstance;

        public TransitionActionBuilder(TransitionActionBuildAction<TEvent> buildAction)
        {
            buildAction?.Invoke(this);
        }

        public void InstantiateAs(TransitionBehaviorInstanceBuilderAsync<TEvent> builderAsync)
        {
            InstanceBuilder = builderAsync;
        }
    }
}
