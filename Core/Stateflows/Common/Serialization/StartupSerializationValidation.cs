using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Common;

/// <summary>Configures additional event/token contracts and representative startup samples.</summary>
public sealed class StartupSerializationValidation
{
    internal readonly Dictionary<Type, HashSet<string>> Types = new();
    internal readonly List<System.Action> Samples = new();

    /// <summary>Adds an event type for validation only; does not register routing or event names.</summary>
    public StartupSerializationValidation AddEvent<T>() => AddEvent(typeof(T));
    /// <summary>Adds an event type for validation only.</summary>
    public StartupSerializationValidation AddEvent(Type type) => Add(type, "manual event");
    /// <summary>Adds an activity token payload for validation only.</summary>
    public StartupSerializationValidation AddToken<T>() => AddToken(typeof(T));
    /// <summary>Adds an activity token payload for validation only.</summary>
    public StartupSerializationValidation AddToken(Type type) => Add(type, "manual token");

    /// <summary>Adds a sample factory and optional semantic check; invoked only during enabled startup verification.</summary>
    public StartupSerializationValidation AddSample<T>(Func<T> factory, Action<T, T>? assertRoundTrip = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        Add(typeof(T), "sample");
        Samples.Add(() =>
        {
            var source = factory();
            var json = StateflowsJsonConverter.SerializePolymorphicObject(source);
            var restored = (T)StateflowsJsonConverter.DeserializeObject(json, typeof(T));
            if (!JToken.DeepEquals(JToken.Parse(json),
                    JToken.Parse(StateflowsJsonConverter.SerializePolymorphicObject(restored))))
                throw new InvalidOperationException($"SER004: Round trip changed serialized state of '{typeof(T)}'.");
            assertRoundTrip?.Invoke(source, restored);
        });
        return this;
    }

    internal StartupSerializationValidation Add(Type type, string usage)
    {
        ArgumentNullException.ThrowIfNull(type);
        if (!Types.TryGetValue(type, out var usages)) Types.Add(type, usages = new());
        usages.Add(usage);
        return this;
    }
}

/// <summary>Enables opt-in startup serialization verification using existing definition visitors.</summary>
public static class StartupSerializationValidationExtensions
{
    /// <summary>Enables verification; omit this call to disable it, for example in production.</summary>
    public static IStateflowsBuilder AddStartupSerializationValidation(this IStateflowsBuilder builder,
        Action<StartupSerializationValidation>? configure = null)
    {
        var descriptor = builder.ServiceCollection.LastOrDefault(service =>
            service.ServiceType == typeof(StartupSerializationValidation));
        var options = descriptor?.ImplementationInstance as StartupSerializationValidation;
        if (options == null)
        {
            options = new();
            builder.ServiceCollection.AddSingleton(options);
            builder.ServiceCollection.AddSingleton<StartupSerializationValidator>();
            builder.ServiceCollection.TryAddEnumerable(
                ServiceDescriptor.Singleton<IHostedService, SerializationValidationHostedService>());
        }
        configure?.Invoke(options);
        return builder;
    }
}

/// <summary>Runs configured startup contract verification. Also used by runtime startup hooks.</summary>
public sealed class StartupSerializationValidator(
    IServiceProvider services, StartupSerializationValidation configuration,
    ILogger<StartupSerializationValidator> logger)
{
    private Task? validation;
    private readonly object gate = new();

    /// <summary>Verifies once per host, preserving failure for all callers.</summary>
    public Task ValidateAsync()
    {
        lock (gate) return validation ??= ValidateCoreAsync();
    }

    private async Task ValidateCoreAsync()
    {
        var types = new StartupSerializationValidation();
        foreach (var (type, usages) in configuration.Types)
            foreach (var usage in usages) types.Add(type, usage);
        await SerializationValidationVisitors.CollectAsync(services, types);
        var errors = new List<string>();
        foreach (var (type, usages) in types.Types)
        {
            foreach (var diagnostic in SerializationContractValidator.Validate(type))
            {
                var message = $"{diagnostic.Rule}: {diagnostic.Path}: {diagnostic.Message} Usage: {string.Join("; ", usages)}";
                if (diagnostic.IsError) errors.Add(message);
                else logger.LogWarning("{SerializationDiagnostic}", message);
            }
        }
        foreach (var sample in configuration.Samples)
        {
            try { sample(); }
            catch (Exception exception)
            {
                errors.Add($"SER004: Sample verification failed: {exception}");
            }
        }
        if (errors.Count != 0)
            throw new InvalidOperationException("Stateflows startup serialization validation failed:" +
                Environment.NewLine + string.Join(Environment.NewLine, errors));
    }
}

internal sealed class SerializationValidationHostedService(StartupSerializationValidator validator) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => validator.ValidateAsync();
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
