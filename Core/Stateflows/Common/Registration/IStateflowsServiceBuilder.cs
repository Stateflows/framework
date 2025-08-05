using System;

namespace Stateflows.Common
{
    public interface IStateflowsInitializer
    {
        void Initialize(IServiceProvider serviceProvider);
    }
}