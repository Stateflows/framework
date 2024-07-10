using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public abstract class DefaultInitializer : BaseInitializer, IDefaultInitializer
    {
        public abstract Task<bool> OnInitialize();
    }
}
