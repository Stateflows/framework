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

        public virtual Task OnInitializeAsync()
            => Task.CompletedTask;

        public virtual Task OnFinalizeAsync()
            => Task.CompletedTask;

        public abstract void Build(ITypedStateMachineBuilder builder);
    }

    public abstract class StateMachine<TInitializationRequest> : StateMachine
        where TInitializationRequest : InitializationRequest, new()
    {
        public override sealed Task OnInitializeAsync()
            => base.OnInitializeAsync();

        public abstract Task OnInitializeAsync(TInitializationRequest initializationEvent);
    }

    public static class StateMachineInfo<TStateMachine>
        where TStateMachine : StateMachine
    {
        public static string Name
        {
            get
            {
                var stateMachineType = typeof(TStateMachine);
                var attribute = stateMachineType.GetCustomAttribute<StateMachineAttribute>();
                return attribute != null
                    ? attribute.Name
                    : stateMachineType.FullName;
            }
        }
    }
}
