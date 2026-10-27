using System;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Exposes the dependency-injection scope associated with a behavior operation.</summary>
    public interface IInjectionScope
    {
        /// <summary>
        /// Service provider of current dependency injection scope
        /// </summary>
        IServiceProvider ServiceProvider { get; }
    }
}