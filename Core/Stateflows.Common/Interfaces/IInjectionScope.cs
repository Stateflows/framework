using System;

namespace Stateflows.Common.Interfaces
{
    public interface IInjectionScope
    {
        /// <summary>
        /// Service provider of current dependency injection scope
        /// </summary>
        IServiceProvider ServiceProvider { get; }
    }
}