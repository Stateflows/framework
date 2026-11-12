using Microsoft.Extensions.Options;
using Orleans.Transactions;
using Orleans.Transactions.Abstractions;
using Orleans.Providers;

namespace Stateflows.Storage;

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class StateflowsTransactionalStateAttribute(string stateName) : Attribute, IFacetMetadata, ITransactionalStateConfiguration
{
    public string StateName { get; } = stateName;
    public string? StorageName => null;
}

internal sealed class StateflowsTransactionalStateMapper(IOptions<StateflowsRuntimeOptions> options)
    : TransactionalStateAttributeMapper<StateflowsTransactionalStateAttribute>
{
    protected override TransactionalStateConfiguration AttributeToConfig(StateflowsTransactionalStateAttribute attribute)
        => new(new TransactionalStateAttribute(attribute.StateName,
            options.Value.StorageProviderName ?? ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
}
