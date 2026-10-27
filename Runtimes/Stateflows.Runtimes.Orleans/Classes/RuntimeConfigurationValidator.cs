using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Providers;
using Orleans.Storage;

namespace Stateflows;

internal sealed class RuntimeConfigurationValidator(
    IServiceProviderIsService services,
    IServiceProviderIsKeyedService keyedServices,
    IOptions<StateflowsRuntimeOptions> options
) : IConfigurationValidator
{
    public void ValidateConfiguration()
    {
        var missing = new List<string>();
        var configuredName = options.Value.StorageProviderName;
        if (configuredName != null && string.IsNullOrWhiteSpace(configuredName))
            throw new OrleansConfigurationException(
                "StateflowsRuntimeOptions.StorageProviderName must be null for default storage or a nonempty provider name.");
        var storageName = configuredName ?? ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME;
        if (!keyedServices.IsKeyedService(typeof(IGrainStorage), storageName))
            missing.Add(configuredName == null
                ? "the Orleans default grain storage provider"
                : $"a grain storage provider registered with the exact key '{storageName}'");
        if (!services.IsService(typeof(IReminderService)) || !services.IsService(typeof(IReminderTable)))
            missing.Add("Orleans reminders (both IReminderService and IReminderTable)");

        if (missing.Count > 0)
            throw new OrleansConfigurationException(
                $"Stateflows Orleans runtime requires {string.Join(" and ", missing)}. " +
                "Configure storage and reminders on the silo builder before starting the host. " +
                "For development, use AddMemoryGrainStorageAsDefault() and UseInMemoryReminderService(), " +
                "or select a named provider with StateflowsRuntimeOptions.StorageProviderName; " +
                "for production, configure durable storage and reminder providers.");
    }
}
