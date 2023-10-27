using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityObserver
    {
        Task BeforeActivityInitializeAsync(IActivityInitializationContext context);
        Task AfterActivityInitializeAsync(IActivityInitializationContext context);

        //Task BeforeStateInitializeAsync(IStateActionContext context);
        //Task AfterStateInitializeAsync(IStateActionContext context);
        //Task BeforeStateEntryAsync(IStateActionContext context);
        //Task AfterStateEntryAsync(IStateActionContext context);
        //Task BeforeStateExitAsync(IStateActionContext context);
        //Task AfterStateExitAsync(IStateActionContext context);
        //Task BeforeTransitionGuardAsync(IGuardContext<Event> context);
        //Task AfterTransitionGuardAsync(IGuardContext<Event> context, bool guardResult);
        //Task BeforeTransitionEffectAsync(ITransitionContext<Event> context);
        //Task AfterTransitionEffectAsync(ITransitionContext<Event> context);
    }
}
