using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Context.Classes;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Engine;
using Stateflows.Entities.Enums;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities.Models
{
    internal abstract class EntityModel
    {
        public string? ResourceName = null;
        
        public abstract Type TemplateType { get; }
        
        public Dictionary<string, FieldModel> Fields { get; } = [];

        public Dictionary<Type, MutationModel> Mutations { get; } = [];

        public List<Action<Dictionary<string, object>>> DefaultInitializerInvoke { get; set; } = [];

        public Dictionary<Type, InitializerModel> Initializers { get; } = [];

        public Dictionary<Type, ProjectionModel> Projections { get; } = [];
    }

    internal class EntityModel<TTemplate> : EntityModel
    {
        public override Type TemplateType => typeof(TTemplate);
    }

    internal abstract class FieldModel(EntityModel entityModel, string name, FieldAccess access)
    {
        protected readonly EntityModel EntityModel = entityModel;

        public string Name => name;

        public bool HasDefaultValue { get; set; }

        public object? DefaultValue { get; set; }
        
        public abstract bool IsComputed { get; }

        public abstract Task ComputeAsync(Dictionary<string, object> values, EntityInspection inspection);

        public abstract Type ValueType { get; }
        
        public FieldAccess Access { get; set; } = access;
    }

    internal class FieldModel<TTemplate, TField>(EntityModel entityModel, string name, FieldAccess access) : FieldModel(entityModel, name, access)
        where TTemplate : class
    {
        public FieldComputation<TTemplate, TField>? Computation { get; set; }

        public override Type ValueType => typeof(TField);

        public override bool IsComputed => Computation != null;

        public override async Task ComputeAsync(Dictionary<string, object> values, EntityInspection inspection)
        {
            if (Computation == null)
            {
                return;
            }

            var (proxy, entity) = EntityProxy<TTemplate>.Create(values, EntityModel);
            var observedEntity = inspection.HasOperationContexts
                ? EntityProxy<TTemplate>.Create(values, EntityModel).Item2
                : entity;
            var result = await inspection.ComputedFieldAsync<TTemplate, TField>(
                observedEntity, Name, () => Task.FromResult(Computation(entity)));
            values[Name.GetFieldKey()] = result;
            EntityContextValues.SetFieldDependencies(values, Name, proxy.ReadFields.Where(fieldName => fieldName != Name));
        }
    }

    internal class MutationModel
    {
        public Type MutationType { get; set; }

        public Delegate MutationAction { get; set; }
        
        public Func<Dictionary<string, object>, BehaviorContext, EntityInspection, object, Task> InvokeAsync { get; set; }
    }

    internal class InitializerModel
    {
        public Type InitializationEventType { get; set; }

        public Delegate InitializerAction { get; set; }

        public Action<Dictionary<string, object>, object> Invoke { get; set; }
    }

    internal class ProjectionModel
    {
        public Type ProjectionType { get; set; }

        public Delegate ProjectionAction { get; set; }

        public Func<Dictionary<string, object>, EntityInspection, Task<object>> InvokeAsync { get; set; }

        public PublishScope PublishScope { get; set; } = PublishScope.None;
    }
}
