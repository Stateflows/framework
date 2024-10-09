using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    //public interface ITransition<in TEvent>
    //{ }

    public interface ITransitionEffect<in TEvent>// : ITransition<TEvent>
    {
        Task EffectAsync(TEvent @event);
    }

    public interface ITransitionGuard<in TEvent>// : ITransition<TEvent>
    {
        Task<bool> GuardAsync(TEvent @event);
    }

    public interface ITransitionGuard : IDefaultTransitionGuard, ITransitionGuard<object>
    {
        Task<bool> ITransitionGuard<object>.GuardAsync(object @event)
            => GuardAsync();
    }

    public interface ITransitionEffect : IDefaultTransitionEffect, ITransitionEffect<object>
    {
        Task ITransitionEffect<object>.EffectAsync(object @event)
            => EffectAsync();
    }

    //public interface IStateMachineAction : ITransitionEffect, IStateEntry, IStateExit, ICompositeStateInitialization, ICompositeStateFinalization
    //{
    //    Task IDefaultTransitionEffect.EffectAsync()
    //        => ExecuteAsync();

    //    Task IStateEntry.OnEntryAsync()
    //        => ExecuteAsync();

    //    Task IStateExit.OnExitAsync()
    //        => ExecuteAsync();

    //    Task ICompositeStateInitialization.OnInitializeAsync()
    //        => ExecuteAsync();

    //    Task ICompositeStateFinalization.OnFinalizeAsync()
    //        => ExecuteAsync();

    //    Task ExecuteAsync();
    //}

    //public interface  IStateMachineCondition : ITransitionGuard
    //{
    //    Task<bool> IDefaultTransitionGuard.GuardAsync()
    //        => ExecuteAsync();

    //    Task<bool> ExecuteAsync();
    //}
}
