using System.Reflection;
using Microsoft.Extensions.Options;
using Orleans.Providers;

namespace Stateflows.Storage;

internal sealed class StateflowsPersistentStateMapper(
    IPersistentStateFactory factory,
    IOptions<StateflowsRuntimeOptions> options
) : IAttributeToFactoryMapper<StateflowsPersistentStateAttribute>
{
    public Factory<IGrainContext, object> GetFactory(ParameterInfo parameter, StateflowsPersistentStateAttribute metadata)
    {
        var stateType = parameter.ParameterType.GetGenericArguments().Single();
        return (Factory<IGrainContext, object>)typeof(StateflowsPersistentStateMapper)
            .GetMethod(nameof(CreateFactory), BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(stateType)
            .Invoke(this, [metadata.StateName])!;
    }

    private Factory<IGrainContext, object> CreateFactory<TState>(string stateName)
        => context => factory.Create<TState>(context, new PersistentStateAttribute(
            stateName, options.Value.StorageProviderName ?? ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
}
