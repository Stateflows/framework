using System;

namespace Stateflows.Common
{
    public interface IBehaviorRegister
    {
        void Build(IServiceProvider serviceProvider);
    }
}