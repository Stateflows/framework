using System;
using System.Collections.Generic;
using System.Linq;
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
        var type = typeof(T);
        var description = $"{name} v{version}: {usage}";
        types.Add(type, description);
        var request = type.GetInterfaces().Append(type)
            .FirstOrDefault(contract => contract.IsGenericType &&
                contract.GetGenericTypeDefinition() == typeof(IRequest<>));
        if (request != null)
            types.Add(request.GetGenericArguments()[0], $"{description} response");
        return Task.CompletedTask;
    }

    private sealed class Machines(StartupSerializationValidation types) : StateMachineVisitor
    {
        public override Task InitializerAddedAsync<TEvent>(string initializerId, string name, int version, IReadOnlyList<string> eventNames)
            => Add<TEvent>(types, name, version, "initializer");
        public override Task TransitionAddedAsync<TEvent>(string transitionId, string name, int version,
            string sourceVertexId, string? targetVertexId, bool isElse, bool isLocal, bool polymorphicTriggers,
            IReadOnlyList<string> triggerNames)
            => Add<TEvent>(types, name, version, "transition");
        public override Task DeferralAddedAsync<TEvent>(string deferralId, string name, int version,
            string vertexId, IReadOnlyList<string> eventNames)
            => Add<TEvent>(types, name, version, "deferred event");
        public override Task HostedBehaviorEventAddedAsync<TEvent>(string hostedBehaviorEventId, string name, int version,
            string hostedBehaviorId, IReadOnlyList<string> eventNames)
            => Add<TEvent>(types, name, version, "hosted behavior event");
    }

    private sealed class Actions(StartupSerializationValidation types) : ActionVisitor
    {
        public override Task CustomEventAddedAsync<TEvent>(string bindingId, string name, int version,
            IReadOnlyList<string> eventNames, BehaviorStatus[] supportedStatuses)
            => Add<TEvent>(types, name, version, "action consumed event/token");
        public override Task ProducedEventAddedAsync<TEvent>(string bindingId, string name, int version,
            IReadOnlyList<string> eventNames)
            => Add<TEvent>(types, name, version, "action produced event");
        public override Task InputTokenAddedAsync<TToken>(string pinId, string name, int version)
            => Add<TToken>(types, name, version, "action input token");
        public override Task OutputTokenAddedAsync<TToken>(string pinId, string name, int version)
            => Add<TToken>(types, name, version, "action output token");
    }

    private sealed class Activities(StartupSerializationValidation types) : ActivityVisitor
    {
        public override Task InitializerAddedAsync<TEvent>(string initializerId, string name, int version,
            IReadOnlyList<string> eventNames, int order)
            => Add<TEvent>(types, name, version, "initializer");
        public override Task InputPinAddedAsync<TToken>(string pinId, string name, int version, string? parentNodeId)
            => Add<TToken>(types, name, version, "input token");
        public override Task OutputPinAddedAsync<TToken>(string pinId, string name, int version, string? parentNodeId)
            => Add<TToken>(types, name, version, "output token");
        public override Task AcceptEventNodeAddedAsync<TEvent>(string nodeId, string name, int version,
            IReadOnlyList<string> eventNames)
            => Add<TEvent>(types, name, version, "accepted event");
        public override Task SendEventNodeAddedAsync<TEvent>(string nodeId, string name, int version,
            IReadOnlyList<string> eventNames)
            => Add<TEvent>(types, name, version, "sent event");
        public override Task PublishEventNodeAddedAsync<TEvent>(string nodeId, string name, int version,
            IReadOnlyList<string> eventNames)
            => Add<TEvent>(types, name, version, "published event");
        public override Task FlowAddedAsync<TToken>(string flowId, string name, int version,
            string sourceNodeId, string targetNodeId, bool isElse)
            => Add<TToken>(types, name, version, "flow token");
        public override Task TransformationFlowAddedAsync<TToken, TOut>(string transformationId, string name,
            int version, string flowId, int order)
        {
            Add<TToken>(types, name, version, "transformation input token");
            return Add<TOut>(types, name, version, "transformation output token");
        }
    }

    private sealed class Entities(StartupSerializationValidation types) : EntityVisitor
    {
        public override Task InitializerAddedAsync<TTemplate, TEvent>(string initializerId, string name, int version,
            IReadOnlyList<string> eventNames)
            => Add<TEvent>(types, name, version, "entity initializer");
        public override Task MutationAddedAsync<TTemplate, TEvent>(string mutationId, string name, int version,
            IReadOnlyList<string> eventNames)
            => Add<TEvent>(types, name, version, "entity mutation");
        public override Task FieldAddedAsync<TTemplate, TField>(string fieldId, string name, int version,
            string fieldName, bool hasDefaultValue, TField defaultValue, bool isComputed, FieldAccess fieldAccess)
            => Add<TField>(types, name, version, $"field {fieldName}");
        public override Task ProjectionAddedAsync<TTemplate, TProjection>(string projectionId, string name,
            int version, PublishScope publishScope)
            => Add<TProjection>(types, name, version, "entity projection");
    }
}
