using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Registration.Builders;
using Stateflows.Entities.Models;
using Stateflows.Entities.Registration.Interfaces;
using Stateflows.Common.Initializer;
using Stateflows.Common;
using System.Linq;
using System.Reflection;

namespace Stateflows.Entities.Registration
{
    internal class EntityRegistration
    {
        public string Name { get; set; }

        public int Version { get; set; }

        public Type EntityType { get; set; }

        public BehaviorClass? OwnerClass { get; set; }

        public BehaviorClass? ParentClass { get; set; }

        public EntityModel Model { get; set; }
        
        public StateflowsBuilder StateflowsBuilder { get; init; }

        public string Identifier => $"entity:{Name}";

        public async Task VisitAsync(IEntityVisitor visitor)
        {
            Task Invoke(string method, Type[] types, params object?[] arguments)
                => (Task)typeof(IEntityVisitor).GetMethod(method)!.MakeGenericMethod(types)
                    .Invoke(visitor, BindingFlags.DoNotWrapExceptions, null, arguments, null)!;
            var template = Model.TemplateType;
            await Invoke(nameof(visitor.EntityAddingAsync), [template], Identifier, Name, Version, OwnerClass, ParentClass,
                BehaviorClassesInitializations.Instance.DefaultInstanceInitializationTokens.Any(token => token.BehaviorClass == new EntityClass(Name)));
            if (EntityType != null)
                await Invoke(nameof(visitor.EntityTypeAddedAsync), [template, EntityType], Identifier, Name, Version);
            foreach (var field in Model.Fields.Values)
                await Invoke(nameof(visitor.FieldAddedAsync), [template, field.ValueType], $"{Identifier}/field:{field.Name}",
                    Name, Version, field.Name, field.HasDefaultValue, field.DefaultValue ?? (field.ValueType.IsValueType ? Activator.CreateInstance(field.ValueType) : null), field.IsComputed, field.Access);
            for (var index = 0; index < Model.DefaultInitializerInvoke.Count; index++)
                await Invoke(nameof(visitor.DefaultInitializerAddedAsync), [template], $"{Identifier}/default-initializer:{index}", Name, Version, index);
            foreach (var type in Model.Initializers.Keys)
                await Invoke(nameof(visitor.InitializerAddedAsync), [template, type], $"{Identifier}/initializer:{Event.GetName(type)}", Name, Version, new[] { Event.GetName(type) });
            foreach (var type in Model.Mutations.Keys)
                await Invoke(nameof(visitor.MutationAddedAsync), [template, type], $"{Identifier}/mutation:{Event.GetName(type)}", Name, Version, new[] { Event.GetName(type) });
            foreach (var projection in Model.Projections.Values)
                await Invoke(nameof(visitor.ProjectionAddedAsync), [template, projection.ProjectionType],
                    $"{Identifier}/projection:{Event.GetName(projection.ProjectionType)}", Name, Version, projection.PublishScope);
            await Invoke(nameof(visitor.EntityAddedAsync), [template], Identifier, Name, Version);
        }

        public List<EntityObserverFactoryAsync> ObserverFactories { get; } = [];

        public List<EntityInterceptorFactoryAsync> InterceptorFactories { get; } = [];

        public List<EntityExceptionHandlerFactoryAsync> ExceptionHandlerFactories { get; } = [];
    }
}
