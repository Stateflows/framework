using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public abstract class Activity
    {
        public IActivityActionContext Context { get; internal set; }

        public virtual Task OnInitializeAsync()
            => Task.CompletedTask;

        public virtual Task OnFinalizeAsync()
            => Task.CompletedTask;

        public abstract void Build(IActivityBuilder builder);
    }

    public abstract class Activity<TInitializationRequest> : Activity
    {
        public override sealed Task OnInitializeAsync()
            => base.OnInitializeAsync();

        public abstract Task OnInitializeAsync(TInitializationRequest initializationRequest);
    }

    public static class ActivityInfo<TActivity>
        where TActivity : Activity
    {
        public static string Name => typeof(TActivity).FullName;
    }
}
