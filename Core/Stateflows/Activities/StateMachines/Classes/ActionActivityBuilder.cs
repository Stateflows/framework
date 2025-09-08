using System.Threading.Tasks;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    internal class ActionActivityBuilder :
        BaseEmbeddedBehaviorBuilder,
        IActionActivityBuilder,
        IInstantiatedActionActivityBuilder
    {
        private StateActionBehaviorInstanceBuilderAsync InstanceBuilder { get; set; } = null;

        public async Task<string> GetInstanceAsync(IStateActionContext context, string defaultInstance)
            => InstanceBuilder != null
                ? await InstanceBuilder(context)
                : defaultInstance;
        
        public StateActionBehaviorInitializationBuilderAsync<object> InitializationBuilder { get; private set; } = null;

        public ActionActivityBuilder(StateActionActivityBuildAction buildAction)
        {
            buildAction?.Invoke(this);
        }

        public IInstantiatedActionActivityBuilder InstantiateAs(StateActionBehaviorInstanceBuilderAsync builderAsync)
        {
            if (builderAsync != null)
            {
                InstanceBuilder = builderAsync;
            }

            return this;
        }

        void IStateActionInitialization.InitializeWith<TInitializationEvent>(StateActionBehaviorInitializationBuilderAsync<TInitializationEvent> builderAsync)
        {
            if (builderAsync != null)
            {
                InitializationBuilder = async c => await builderAsync(c);
            }
        }
    }
}
