using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface ITransition<in TEvent>
    { }

    public interface ITransitionEffect<in TEvent> : ITransition<TEvent>
    {
        Task EffectAsync(TEvent @event);
    }

    public interface ITransitionGuard<in TEvent> : ITransition<TEvent>
    {
        Task<bool> GuardAsync(TEvent @event);
    }
}
