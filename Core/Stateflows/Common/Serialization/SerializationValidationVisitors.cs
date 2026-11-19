using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Entities;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;
using Stateflows.StateMachines;

namespace Stateflows.Common;

internal static class SerializationValidationVisitors
{
    internal static async Task CollectAsync(IServiceProvider services, StartupSerializationValidation types)
    {
        if (services.GetService<IStateMachinesRegister>() is { } machines)
            await machines.VisitStateMachinesAsync(new Machines(types));
        if (services.GetService<IActivitiesRegister>() is { } activities)
            await activities.VisitActivitiesAsync(new Activities(types));
        if (services.GetService<IActionsRegister>() is { } actions)
            await actions.VisitActionsAsync(new Actions(types));
        if (services.GetService<IEntitiesRegister>() is { } entities)
            await entities.VisitEntitiesAsync(new Entities(types));
    }

    private static Task Add<T>(StartupSerializationValidation types, string name, int version, string usage)
    {
        types.Add(typeof(T), $"{name} v{version}: {usage}");
        return Task.CompletedTask;
    }

    private static Task Structural(StartupSerializationValidation types, EventDefinition definition, string name, int version)
    {
        if (definition.ResponseType != null)
            types.Add(definition.ResponseType, $"{name} v{version}: {definition.Name} response");
        return Task.CompletedTask;
    }

    private sealed class Machines(StartupSerializationValidation types) : StateMachineVisitor
    {
        public override Task InitializerAddedAsync<T>(string name, int version) => Add<T>(types, name, version, "initializer");
        public override Task TransitionAddedAsync<T>(string name, int version, string source, string target = null, bool isElse = false)
            => Add<T>(types, name, version, $"transition {source} -> {target}");
        public override Task CustomEventAddedAsync<T>(string name, int version, BehaviorStatus[] statuses)
            => Add<T>(types, name, version, "custom event");
        public override Task DoBehaviorAddedAsync(string name, int version, string state, string behavior, string kind, IEnumerable<Type> events)
        {
            foreach (var type in events) types.Add(type, $"{name} v{version}: {state} do {behavior}");
            return Task.CompletedTask;
        }
        public override Task TransitionAddedAsync(EventDefinition definition, string name, int version, string source, string target = null, bool isElse = false)
            => Structural(types, definition, name, version);
        public override Task DoBehaviorAddedAsync(EventDefinition definition, string name, int version, string state, string behavior, string kind)
            => Structural(types, definition, name, version);
    }

    private sealed class Actions(StartupSerializationValidation types) : ActionVisitor
    {
        public override Task CustomEventAddedAsync<T>(string name, int version, BehaviorStatus[] statuses)
            => Add<T>(types, name, version, "action consumed event/token");
        public override Task CustomEventAddedAsync(EventDefinition definition, string name, int version, BehaviorStatus[] statuses)
            => Structural(types, definition, name, version);
    }

    private sealed class Activities(StartupSerializationValidation types) : ActivityVisitor
    {
        public override Task InitializerAddedAsync<T>(string name, int version) => Add<T>(types, name, version, "initializer");
        public override Task CustomEventAddedAsync<T>(string name, int version, BehaviorStatus[] statuses) => Add<T>(types, name, version, "custom event");
        public override Task InputPinAddedAsync<T>(string name, int version, string node, string parent = null) => Add<T>(types, name, version, $"{node} input token");
        public override Task OutputPinAddedAsync<T>(string name, int version, string node, string parent = null) => Add<T>(types, name, version, $"{node} output token");
        public override Task AcceptEventNodeAddedAsync<T>(string name, int version, string node, string parent = null) => Add<T>(types, name, version, $"{node} accepted event");
        public override Task SendEventNodeAddedAsync<T>(string name, int version, string node, string parent = null) => Add<T>(types, name, version, $"{node} sent event");
        public override Task PublishEventNodeAddedAsync<T>(string name, int version, string node, string parent = null) => Add<T>(types, name, version, $"{node} published event");
        public override Task AcceptEventNodeAddedAsync(EventDefinition definition, string name, int version, string node, string parent = null) => Structural(types, definition, name, version);
        public override Task FlowAddedAsync<T>(string name, int version, string source, string target, bool isElse = false) => Add<T>(types, name, version, $"{source} -> {target} token");
        public override Task TransformationFlowAddedAsync<T, TOut>(string name, int version, string source, string target, bool isElse = false)
        {
            Add<T>(types, name, version, $"{source} -> {target} input token");
            return Add<TOut>(types, name, version, $"{source} -> {target} transformed token");
        }
    }

    private sealed class Entities(StartupSerializationValidation types) : EntityVisitor
    {
        public override Task InitializerAddedAsync<TTemplate, T>(string name, int version) => Add<T>(types, name, version, "initializer");
        public override Task MutationAddedAsync<TTemplate, T>(string name, int version) => Add<T>(types, name, version, "mutation");
        public override Task FieldAddedAsync<TTemplate, T>(string name, int version, string field, T value, bool computed, FieldAccess access) => Add<T>(types, name, version, $"field {field}");
        public override Task ProjectionAddedAsync<TTemplate, T>(string name, int version, PublishScope scope) => Add<T>(types, name, version, "projection");
    }
}
