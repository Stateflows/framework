using System;
using System.Collections.Generic;
using System.Linq;
using Stateflows.Entities.Engine;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities.Models
{
    internal abstract class EntityModel
    {
        public abstract Type TemplateType { get; }
        
        public Dictionary<string, FieldModel> Fields { get; } = [];

        public Dictionary<Type, MutationModel> Mutations { get; } = [];

        public Delegate DefaultInitializer { get; set; }

        public Action<Dictionary<string, object>> DefaultInitializerInvoke { get; set; }

        public Dictionary<Type, InitializerModel> Initializers { get; } = [];

        public Delegate DefaultProjection { get; set; }

        public Action<Dictionary<string, object>> DefaultProjectionInvoke { get; set; }

        public Dictionary<Type, ProjectionModel> Projections { get; } = [];
    }

    internal class EntityModel<TTemplate> : EntityModel
    {
        public override Type TemplateType => typeof(TTemplate);
    }

    internal abstract class FieldModel(EntityModel entityModel, string name)
    {
        protected readonly EntityModel EntityModel = entityModel;

        public string Name => name;
        
        public abstract bool IsComputed { get; }

        public abstract void Compute(Dictionary<string, object> values);

        public abstract Type ValueType { get; }

        public List<Delegate> Observations { get; } = [];

        /// <summary>
        /// Internal triggers added automatically when another field's computation depends on this field.
        /// Each trigger recomputes the dependent computed field by writing its result back into context.Values.
        /// </summary>
        public Dictionary<string, Action<Dictionary<string, object>>> ComputationTriggers { get; } = [];
    }

    internal class FieldModel<TTemplate, TField>(EntityModel entityModel, string name) : FieldModel(entityModel, name)
        where TTemplate : class
    {
        public FieldComputation<TTemplate, TField>? Computation { get; set; }

        public override Type ValueType => typeof(TField);

        public override bool IsComputed => Computation != null;

        public override void Compute(Dictionary<string, object> values)
        {
            if (Computation == null)
            {
                return;
            }
            
            foreach (var entityModelField in EntityModel.Fields.Values.Where(entityModelField => entityModelField != this))
            {
                entityModelField.ComputationTriggers.Remove(Name);
            }
            
            var (proxy, entity) = EntityProxy<TTemplate>.Create(values, EntityModel);
            var result = Computation(entity);
            values[Name.GetFieldKey()] = result;
            
            foreach (var entityModelField in EntityModel.Fields.Values.Where(entityModelField =>
                         proxy.ReadFields.Contains(entityModelField.Name) &&
                         entityModelField != this
                ))
            {
                entityModelField.ComputationTriggers[Name] = Compute;
            }
        }
    }

    internal class MutationModel
    {
        public Type MutationType { get; set; }

        public Delegate MutationAction { get; set; }
        
        public Action<Dictionary<string, object>, object> Invoke { get; set; }
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

        public Func<Dictionary<string, object>, object> Invoke { get; set; }
    }
}
