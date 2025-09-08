using System.Threading.Tasks;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    internal class ActionActionBuilder :
        BaseEmbeddedBehaviorBuilder,
        IActionActionBuilder
    {
        private StateActionBehaviorInstanceBuilderAsync InstanceBuilder { get; set; } = null;
        
        public async Task<string> GetInstanceAsync(IStateActionContext context, string defaultInstance)
            => InstanceBuilder != null
                ? await InstanceBuilder(context)
                : defaultInstance;

        public ActionActionBuilder(StateActionActionBuildAction buildAction)
        {
            buildAction?.Invoke(this);
        }

        public void InstantiateAs(StateActionBehaviorInstanceBuilderAsync builderAsync)
        {
            if (builderAsync != null)
            {
                InstanceBuilder = builderAsync;
            }
        }
    }
}
