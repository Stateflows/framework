using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IExecutionInterceptor
    {
        bool BeforeExecute(Event @event);
        void AfterExecute(Event @event);
    }
}
