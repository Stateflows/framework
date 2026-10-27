using System.Reflection;
using Newtonsoft.Json.Linq;
using Stateflows.Common;

namespace Stateflows.StateMachines.Json;

internal sealed record ClrBinding(Type Type, string Path, TypedJson Serializer,
    PropertyInfo? ConfigurationProperty, Type? ConfigurationType, JToken? Configuration)
{
    public void Configure(object instance)
    {
        if (ConfigurationProperty != null)
            ConfigurationProperty.SetValue(instance, Serializer.Read(Configuration!, ConfigurationType!));
    }

    public object CreateDefinition()
    {
        // Transition definitions follow the existing constructor-free definition convention.
        var instance = System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(Type);
        Configure(instance);
        return instance;
    }

    public async Task<T> ConfigureAsync<T>(Task<T> instanceTask)
    {
        var instance = await instanceTask;
        if (instance == null)
            throw new InvalidOperationException($"Could not instantiate '{Type}' selected at '{Path}'.");
        Configure(instance);
        return instance;
    }
}
