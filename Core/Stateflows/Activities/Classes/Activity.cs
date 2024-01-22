using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Attributes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public abstract class Activity
    {
        public IActivityActionContext Context { get; internal set; }

        public virtual Task<bool> OnInitializeAsync()
            => Task.FromResult(true);

        public virtual Task OnFinalizeAsync()
            => Task.CompletedTask;

        public abstract void Build(ITypedActivityBuilder builder);
    }

    public abstract class Activity<TInitializationRequest> : Activity, IInitializedBy<TInitializationRequest>
        where TInitializationRequest : InitializationRequest, new()
    {
        public abstract Task<bool> OnInitializeAsync(TInitializationRequest initializationRequest);
    }

    public abstract class Activity<TInitializationRequest1, TInitializationRequest2> : Activity<TInitializationRequest1>, IInitializedBy<TInitializationRequest2>
        where TInitializationRequest1 : InitializationRequest, new()
        where TInitializationRequest2 : InitializationRequest, new()
    {
        public abstract Task<bool> OnInitializeAsync(TInitializationRequest2 initializationRequest);
    }

    public abstract class Activity<TInitializationRequest1, TInitializationRequest2, TInitializationRequest3> : Activity<TInitializationRequest1, TInitializationRequest2>, IInitializedBy<TInitializationRequest3>
        where TInitializationRequest1 : InitializationRequest, new()
        where TInitializationRequest2 : InitializationRequest, new()
        where TInitializationRequest3 : InitializationRequest, new()
    {
        public abstract Task<bool> OnInitializeAsync(TInitializationRequest3 initializationRequest);
    }

    public static class ActivityInfo<TActivity>
        where TActivity : Activity
    {
        public static string Name
        {
            get
            {
                var stateMachineType = typeof(TActivity);
                var attribute = stateMachineType.GetCustomAttribute<ActivityBehaviorAttribute>();
                return attribute != null && attribute.Name != null
                    ? attribute.Name
                    : stateMachineType.FullName;
            }
        }
    }
}
