using System;
using System.Reflection;

namespace Stateflows.Common.Interfaces;

/// <summary>Resolves model-element constructor parameters that require framework-specific metadata.</summary>
public interface IModelElementParameterResolver
{
    /// <summary>Determines whether this resolver handles the specified constructor parameter.</summary>
    bool CanResolve(ParameterInfo parameter);

    /// <summary>Creates the value for a parameter handled by this resolver.</summary>
    object Resolve(IServiceProvider serviceProvider, ParameterInfo parameter);
}
