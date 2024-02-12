using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Attributes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class StateMachine
    {
        public IStateMachineActionContext Context { get; internal set; }

        public virtual Task<bool> OnInitializeAsync()
            => Task.FromResult(true);

        public virtual Task OnFinalizeAsync()
            => Task.CompletedTask;

        public abstract void Build(ITypedStateMachineBuilder builder);
    }

    public abstract class StateMachine<TInitializationRequest> : StateMachine, IInitializedBy<TInitializationRequest>
        where TInitializationRequest : InitializationRequest, new()
    {
        public abstract Task<bool> OnInitializeAsync(TInitializationRequest initializationRequest);
    }

    public abstract class StateMachine<TInitializationRequest1, TInitializationRequest2> : StateMachine<TInitializationRequest1>, IInitializedBy<TInitializationRequest2>
        where TInitializationRequest1 : InitializationRequest, new()
        where TInitializationRequest2 : InitializationRequest, new()
    {
        public abstract Task<bool> OnInitializeAsync(TInitializationRequest2 initializationRequest);
    }

    public abstract class StateMachine<TInitializationRequest1, TInitializationRequest2, TInitializationRequest3> : StateMachine<TInitializationRequest1, TInitializationRequest2>, IInitializedBy<TInitializationRequest3>
        where TInitializationRequest1 : InitializationRequest, new()
        where TInitializationRequest2 : InitializationRequest, new()
        where TInitializationRequest3 : InitializationRequest, new()
    {
        public abstract Task<bool> OnInitializeAsync(TInitializationRequest3 initializationRequest);
    }

    public static class StateMachineInfo<TStateMachine>
        where TStateMachine : StateMachine
    {
        public static string Name
        {
            get
            {
                var stateMachineType = typeof(TStateMachine);
                var attribute = stateMachineType.GetCustomAttribute<StateMachineBehaviorAttribute>();
                return attribute != null && attribute.Name != null
                    ? attribute.Name
                    : stateMachineType.FullName;
            }
        }

        public static BehaviorClass ToClass()
            => new BehaviorClass(BehaviorType.StateMachine, Name);

        public static BehaviorId ToId(string instance)
            => new BehaviorId(ToClass(), instance);
    }
}
