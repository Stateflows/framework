using System;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Models;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities.Registration.Builders
{
    internal class FieldBuilder<TTemplate, TField>(FieldModel<TTemplate, TField> fieldModel) :
        IFieldBuilder<TTemplate, TField>
        where TTemplate : class
    {
        public IFieldBuilder<TTemplate, TField> SetAccess(FieldAccess fieldAccess)
        {
            fieldModel.Access = fieldAccess;
            
            return this;
        }

        public void AddComputation(FieldComputation<TTemplate, TField> computation)
        {
            if (fieldModel.Computation != null)
            {
                throw new InvalidOperationException($"Field '{fieldModel.Name}' already has a computation registered");
            }

            if (fieldModel.HasDefaultValue)
            {
                throw new InvalidOperationException($"Computed field '{fieldModel.Name}' cannot declare a default value.");
            }

            fieldModel.Computation = computation;
            // var model = entityBuilder.Registration.Model;

            // Compile the computation once to avoid repeated compilation at runtime
            // var compiledComputation = computation.Compile();
            // var computedFieldName = fieldModel.Name;
            // var model = entityBuilder.Registration.Model;

            // Walk the expression tree to find all property accesses on the entity parameter
            // var visitor = new MemberAccessVisitor(computation.Parameters[0]);
            // visitor.Visit(computation.Body);

            // For each dependency property, add a computation trigger to that field via entityBuilder
            // foreach (var propertyName in visitor.AccessedProperties)
            // {
                // entityBuilder.AddComputationTriggerByPropertyName(propertyName, values =>
                // {
                //     var entity = EntityProxy<TTemplate>.Create(values, model);
                //     var result = computation(entity);
                //     values[computedFieldName.GetFieldKey()] = result;
                // });
            // }
        }

        public void AddDefaultValue(TField defaultValue)
        {
            if (fieldModel.IsComputed)
            {
                throw new InvalidOperationException($"Computed field '{fieldModel.Name}' cannot declare a default value.");
            }

            fieldModel.DefaultValue = defaultValue;
            fieldModel.HasDefaultValue = true;
        }

        // /// <summary>
        // /// Visits an expression tree and collects the names of all interface properties
        // /// accessed directly on the specified lambda parameter.
        // /// </summary>
        // private sealed class MemberAccessVisitor(ParameterExpression entityParam) : ExpressionVisitor
        // {
        //     public List<string> AccessedProperties { get; } = [];
        //
        //     protected override Expression VisitMember(MemberExpression node)
        //     {
        //         if (node.Expression == entityParam && node.Member is PropertyInfo)
        //             AccessedProperties.Add(node.Member.Name);
        //
        //         return base.VisitMember(node);
        //     }
        // }
    }
}
