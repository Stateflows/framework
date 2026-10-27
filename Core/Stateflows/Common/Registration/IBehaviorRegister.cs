using System;

namespace Stateflows.Common
{
    /// <summary>Registers behavior definitions with the Stateflows host.</summary>
    public interface IBehaviorRegister
    {
        /// <summary>Builds and registers behaviors using the host service provider.</summary>
        /// <param name="serviceProvider">The service provider used while building registrations.</param>
        void Build(IServiceProvider serviceProvider);
    }
}