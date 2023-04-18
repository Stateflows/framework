using Microsoft.Extensions.DependencyInjection;
using System;

namespace Stateflows.Common.Registration.Interfaces
{
    public interface IStateflowsClientBuilder
    {
        IServiceCollection Services { get; }
    }
}