using System;

namespace Stateflows.Common
{
    /// <summary>Initializes a Stateflows service using the host service provider.</summary>
    public interface IStateflowsInitializer
    {
        /// <summary>Performs initialization after host services are available.</summary>
        /// <param name="serviceProvider">The host service provider.</param>
        void Initialize(IServiceProvider serviceProvider);
    }
}