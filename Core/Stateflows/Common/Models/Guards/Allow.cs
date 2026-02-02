using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    public sealed class Allow : IGuardElement
    {
        public Task<bool> GuardAsync()
            => Task.FromResult(true);
    }
}