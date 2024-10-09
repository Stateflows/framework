using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    //public interface IDefaultTransition
    //{ }

    public interface IDefaultTransitionEffect// : IDefaultTransition
    {
        Task EffectAsync();
    }

    public interface IDefaultTransitionGuard// : IDefaultTransition
    {
        Task<bool> GuardAsync();
    }
}
