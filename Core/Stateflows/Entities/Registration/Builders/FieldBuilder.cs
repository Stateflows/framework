using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;
using Stateflows.Entities.Models;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities.Registration.Builders
{
    internal class FieldBuilder<TTemplate, TField>(FieldModel<TTemplate, TField> fieldModel, EntityBuilder<TTemplate> entityBuilder) :
        IFieldBuilder<TTemplate, TField>,
        IComputedFieldBuilder<TTemplate, TField>
        where TTemplate : class
    {
        public IComputedFieldBuilder<TTemplate, TField> AddComputation(FieldComputation<TTemplate, TField> computation)
        {
            if (fieldModel.Computation != null)
            {
                throw new InvalidOperationException($"Field '{fieldModel.Name}' already has a computation registered");
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

            return this;
        }

        public IFieldBuilder<TTemplate, TField> AddObservation(FieldObservation<TField> observation)
        {
            fieldModel.Observations.Add(observation);

            return this;
        }

        /// <summary>
        /// Visits an expression tree and collects the names of all interface properties
        /// accessed directly on the specified lambda parameter.
        /// </summary>
        private sealed class MemberAccessVisitor(ParameterExpression entityParam) : ExpressionVisitor
        {
            public List<string> AccessedProperties { get; } = [];

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression == entityParam && node.Member is PropertyInfo)
                    AccessedProperties.Add(node.Member.Name);

                return base.VisitMember(node);
            }
        }

        IComputedFieldBuilder<TTemplate, TField> IComputedFieldBuilder<TTemplate, TField>.AddObservation(FieldObservation<TField> observation)
            => AddObservation(observation) as IComputedFieldBuilder<TTemplate, TField>;
    }
}
