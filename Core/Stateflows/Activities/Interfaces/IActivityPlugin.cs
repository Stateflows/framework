using Stateflows.Activities.Context.Interfaces;
using System.Threading.Tasks;

namespace Stateflows.Activities
{
    internal interface IActivityPlugin : IActivityInterceptor, IActivityObserver
    { }
}
