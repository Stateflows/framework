using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    public sealed class Deny : IGuardElement
    {
        public Task<bool> GuardAsync()
            => Task.FromResult(false);
    }
}