using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Engine;
using Stateflows.Entities.Enums;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities.Registration.Builders
{
    internal class EntitiesBuilder(EntitiesRegister register, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null) : IEntitiesBuilder
    {
        private static readonly MethodInfo AddTypedEntityRegistrationMethod = typeof(IEntitiesRegister)
            .GetMethods()
            .Single(method =>
                method.Name == nameof(IEntitiesRegister.AddEntity) &&
                method.IsGenericMethodDefinition &&
                method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 4 &&
                method.GetParameters()[2].ParameterType == typeof(Type)
            );

        private static readonly MethodInfo RegisterStoredFieldMethod = typeof(EntitiesBuilder)
            .GetMethod(nameof(RegisterStoredField), BindingFlags.Static | BindingFlags.NonPublic)!;

        private static readonly MethodInfo RegisterComputedFieldMethod = typeof(EntitiesBuilder)
            .GetMethod(nameof(RegisterComputedField), BindingFlags.Static | BindingFlags.NonPublic)!;

        private static readonly MethodInfo RegisterProjectionMethod = typeof(EntitiesBuilder)
            .GetMethod(nameof(RegisterProjection), BindingFlags.Static | BindingFlags.NonPublic)!;

        private static readonly MethodInfo RegisterMutationMethod = typeof(EntitiesBuilder)
            .GetMethod(nameof(RegisterMutation), BindingFlags.Static | BindingFlags.NonPublic)!;

        private static readonly MethodInfo AutoAnalyzeTemplateMethod = typeof(EntitiesBuilder)
            .GetMethod(nameof(AutoAnalyzeTemplate), BindingFlags.Static | BindingFlags.NonPublic)!;

        private static Type GetEntityTemplateType(Type type)
            => type
                .GetInterfaces()
                .FirstOrDefault(@interface =>
                    @interface.IsGenericType &&
                    @interface.GetGenericTypeDefinition() == typeof(IEntity<>)
                )
                ?.GetGenericArguments()
                .SingleOrDefault();

        private sealed class TemplateAnalysis
        {
            public List<PropertyInfo> StoredFields { get; } = [];

            public List<PropertyInfo> ComputedFields { get; } = [];

            public List<PropertyInfo> Projections { get; } = [];

            public List<MethodInfo> Mutations { get; } = [];
        }

        private static EntityBuildAction<TTemplate> AutoAnalyzeTemplate<TTemplate>(EntityBuildAction<TTemplate> buildAction)
            where TTemplate : class
            => builder =>
            {
                RegisterAnnotatedMembers(builder);
                buildAction?.Invoke(builder);
            };

        private static object BuildAutoAnalyzedBuildAction(Type templateType, object buildAction = null)
            => AutoAnalyzeTemplateMethod
                .MakeGenericMethod(templateType)
                .Invoke(null, [buildAction]);

        private static void RegisterAnnotatedMembers<TTemplate>(IEntityBuilder<TTemplate> builder)
            where TTemplate : class
        {
            var analysis = AnalyzeTemplate(typeof(TTemplate));

            foreach (var property in analysis.StoredFields.OrderBy(property => property.Name))
            {
                RegisterStoredFieldMethod
                    .MakeGenericMethod(typeof(TTemplate), property.PropertyType)
                    .Invoke(null, [builder, property]);
            }

            foreach (var property in analysis.ComputedFields.OrderBy(property => property.Name))
            {
                RegisterComputedFieldMethod
                    .MakeGenericMethod(typeof(TTemplate), property.PropertyType)
                    .Invoke(null, [builder, property]);
            }

            foreach (var property in analysis.Projections.OrderBy(property => property.Name))
            {
                RegisterProjectionMethod
                    .MakeGenericMethod(typeof(TTemplate), property.PropertyType)
                    .Invoke(null, [builder, property]);
            }

            foreach (var method in analysis.Mutations.OrderBy(method => method.Name))
            {
                RegisterMutationMethod
                    .MakeGenericMethod(typeof(TTemplate), method.GetParameters()[0].ParameterType)
                    .Invoke(null, [builder, method]);
            }
        }

        private static TemplateAnalysis AnalyzeTemplate(Type templateType)
        {
            if (!templateType.IsInterface)
            {
                throw new InvalidOperationException(
                    $"Entity template type '{templateType.FullName}' must be an interface."
                );
            }

            var analysis = new TemplateAnalysis();

            var allInterfaceTypes = new[] { templateType }.Concat(templateType.GetInterfaces()).ToArray();

            foreach (var property in allInterfaceTypes.SelectMany(t => t.GetProperties()).Distinct())
            {
                var fieldAttribute = property.GetCustomAttribute<FieldAttribute>();
                var projectionAttribute = property.GetCustomAttribute<ProjectionAttribute>();
                var hasDefaultValue = property.GetCustomAttribute<System.ComponentModel.DefaultValueAttribute>() != null;

                if (fieldAttribute == null && projectionAttribute == null)
                {
                    continue;
                }

                if (fieldAttribute != null && projectionAttribute != null)
                {
                    throw new InvalidOperationException(
                        $"Property '{templateType.FullName}.{property.Name}' cannot be marked with both [Field] and [Projection]."
                    );
                }

                if (property.GetIndexParameters().Length != 0)
                {
                    throw new InvalidOperationException(
                        $"Indexed property '{templateType.FullName}.{property.Name}' cannot be used as an entity member."
                    );
                }

                if (fieldAttribute != null)
                {
                    if (property.GetMethod == null)
                    {
                        throw new InvalidOperationException(
                            $"Field property '{templateType.FullName}.{property.Name}' must declare a getter."
                        );
                    }

                    if (property.SetMethod != null)
                    {
                        if (hasDefaultValue && !property.TryGetValidatedDefaultValueForProperty(out _, out var validationError))
                        {
                            throw new InvalidOperationException(validationError);
                        }

                        analysis.StoredFields.Add(property);
                    }
                    else
                    {
                        if (hasDefaultValue)
                        {
                            throw new InvalidOperationException(
                                $"Computed field property '{templateType.FullName}.{property.Name}' cannot declare [DefaultValue]. Only non-computed [Field] properties support default values."
                            );
                        }

                        if (!DefaultInterfaceImplementationInvoker.HasDefaultImplementation(property.GetMethod))
                        {
                            throw new InvalidOperationException(
                                $"Computed field property '{templateType.FullName}.{property.Name}' must provide a default getter implementation."
                            );
                        }

                        analysis.ComputedFields.Add(property);
                    }

                    continue;
                }

                if (property.GetMethod == null)
                {
                    throw new InvalidOperationException(
                        $"Projection property '{templateType.FullName}.{property.Name}' must declare a getter."
                    );
                }

                if (property.SetMethod != null)
                {
                    throw new InvalidOperationException(
                        $"Projection property '{templateType.FullName}.{property.Name}' must be read-only."
                    );
                }

                if (hasDefaultValue)
                {
                    throw new InvalidOperationException(
                        $"Projection property '{templateType.FullName}.{property.Name}' cannot declare [DefaultValue]. Only non-computed [Field] properties support default values."
                    );
                }

                if (!DefaultInterfaceImplementationInvoker.HasDefaultImplementation(property.GetMethod))
                {
                    throw new InvalidOperationException(
                        $"Projection property '{templateType.FullName}.{property.Name}' must provide a default getter implementation."
                    );
                }

                analysis.Projections.Add(property);
            }

            foreach (var method in allInterfaceTypes.SelectMany(t => t.GetMethods()).Where(method => !method.IsSpecialName).Distinct())
            {
                if (method.GetCustomAttribute<MutationAttribute>() == null)
                {
                    continue;
                }

                if (method.ReturnType != typeof(void))
                {
                    throw new InvalidOperationException(
                        $"Mutation method '{templateType.FullName}.{method.Name}' must return void."
                    );
                }

                var parameters = method.GetParameters();
                if (parameters.Length != 1 || parameters[0].ParameterType.IsByRef)
                {
                    throw new InvalidOperationException(
                        $"Mutation method '{templateType.FullName}.{method.Name}' must declare exactly one non-byref parameter."
                    );
                }

                if (!DefaultInterfaceImplementationInvoker.HasDefaultImplementation(method))
                {
                    throw new InvalidOperationException(
                        $"Mutation method '{templateType.FullName}.{method.Name}' must provide a default implementation."
                    );
                }

                analysis.Mutations.Add(method);
            }

            var duplicateProjectionType = analysis.Projections
                .GroupBy(property => property.PropertyType)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicateProjectionType != null)
            {
                throw new InvalidOperationException(
                    $"Projection type '{duplicateProjectionType.Key.FullName}' is declared multiple times on template '{templateType.FullName}'."
                );
            }

            var duplicateMutationType = analysis.Mutations
                .GroupBy(method => method.GetParameters()[0].ParameterType)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicateMutationType != null)
            {
                throw new InvalidOperationException(
                    $"Mutation event type '{duplicateMutationType.Key.FullName}' is declared multiple times on template '{templateType.FullName}'."
                );
            }

            return analysis;
        }

        private static void RegisterStoredField<TTemplate, TField>(IEntityBuilder<TTemplate> builder, PropertyInfo property)
            where TTemplate : class
        {
            builder.AddField(CreateFieldSelector<TTemplate, TField>(property));

            if (builder is EntityBuilder<TTemplate> entityBuilder)
            {
                var fieldModel = entityBuilder.Registration.Model.Fields[property.Name];
                var fieldAttribute = property.GetCustomAttribute<FieldAttribute>();
                fieldModel.Access = fieldAttribute?.Access ?? FieldAccess.None;

                if (property.TryGetValidatedDefaultValueForProperty(out var defaultValue, out _))
                {
                    fieldModel.HasDefaultValue = true;
                    fieldModel.DefaultValue = defaultValue;
                }
            }
        }

        private static void RegisterComputedField<TTemplate, TField>(IEntityBuilder<TTemplate> builder, PropertyInfo property)
            where TTemplate : class
        {
            builder.AddField(
                CreateFieldSelector<TTemplate, TField>(property),
                field => field.AddComputation(entity => (TField)DefaultInterfaceImplementationInvoker.Invoke(entity, property.GetMethod!, [])!)
            );

            if (builder is EntityBuilder<TTemplate> entityBuilder)
            {
                var fieldModel = entityBuilder.Registration.Model.Fields[property.Name];
                var fieldAttribute = property.GetCustomAttribute<FieldAttribute>();
                if (fieldAttribute != null)
                    fieldModel.Access = fieldAttribute.Access;
            }
        }

        private static void RegisterProjection<TTemplate, TProjection>(IEntityBuilder<TTemplate> builder, PropertyInfo property)
            where TTemplate : class
            => builder.AddProjection<TProjection>(
                entity => (TProjection)DefaultInterfaceImplementationInvoker.Invoke(entity, property.GetMethod!, [])!,
                property.GetCustomAttribute<ProjectionAttribute>()?.PublishScope ?? PublishScope.None
            );

        private static void RegisterMutation<TTemplate, TMutation>(IEntityBuilder<TTemplate> builder, MethodInfo method)
            where TTemplate : class
            => builder.AddMutation<TMutation>(
                context => DefaultInterfaceImplementationInvoker.Invoke(context.Entity, method, [context.MutationEvent])
            );

        private static Expression<Func<TTemplate, TField>> CreateFieldSelector<TTemplate, TField>(PropertyInfo property)
            where TTemplate : class
        {
            var entityParameter = Expression.Parameter(typeof(TTemplate), "entity");
            var propertyAccess = Expression.Property(entityParameter, property);
            return Expression.Lambda<Func<TTemplate, TField>>(propertyAccess, entityParameter);
        }

        private void RegisterTypedEntity(Type templateType, string entityName, int version, Type entityType)
        {
            var addEntityMethod = AddTypedEntityRegistrationMethod.MakeGenericMethod(templateType);
            var effectiveBuildAction = BuildAutoAnalyzedBuildAction(templateType);

            if (register is IOwnedRegistration registration)
            {
                var originalOwnerClass = registration.OwnerClass;
                var originalParentClass = registration.ParentClass;
                registration.OwnerClass = ownerClass;
                registration.ParentClass = parentClass;

                addEntityMethod.Invoke(register, [entityName, version, entityType, effectiveBuildAction]);

                registration.OwnerClass = originalOwnerClass;
                registration.ParentClass = originalParentClass;
            }
            else
            {
                addEntityMethod.Invoke(register, [entityName, version, entityType, effectiveBuildAction]);
            }
        }

        [DebuggerHidden]
        public IEntitiesBuilder AddFromAssembly(Assembly assembly)
        {
            assembly.GetAttributedTypes<EntityBehaviorAttribute>().ToList().ForEach(type =>
            {
                var templateType = GetEntityTemplateType(type);

                if (
                    templateType is not null &&
                    !type.IsAbstract &&
                    !type.ContainsGenericParameters &&
                    type.GetCustomAttributes(typeof(EntityBehaviorAttribute)).FirstOrDefault() is EntityBehaviorAttribute attribute
                )
                {
                    var entityName = attribute.Name ?? type.FullName ?? type.Name;
                    RegisterTypedEntity(templateType, entityName, attribute.Version, type);
                }
            });

            return this;
        }

        [DebuggerHidden]
        public IEntitiesBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                AddFromAssembly(assembly);
            }

            return this;
        }

        [DebuggerHidden]
        public IEntitiesBuilder AddEntity<TTemplate>(string entityName, EntityBuildAction<TTemplate> buildAction = null)
            where TTemplate : class
            => AddEntity(entityName, 1, buildAction);

        [DebuggerHidden]
        public IEntitiesBuilder AddEntity<TTemplate>(string entityName, int version, EntityBuildAction<TTemplate> buildAction = null)
            where TTemplate : class
        {
            var templateType = GetEntityTemplateType(typeof(TTemplate));

            if (templateType != null)
            {
                if (buildAction != null)
                {
                    throw new InvalidOperationException($"Build action cannot be provided when '{typeof(TTemplate).FullName}' is registered as an entity behavior type.");
                }

                RegisterTypedEntity(templateType, entityName, version, typeof(TTemplate));

                return this;
            }

            if (!typeof(TTemplate).IsInterface)
            {
                throw new InvalidOperationException(
                    $"Entity template type '{typeof(TTemplate).FullName}' must be an interface."
                );
            }

            var effectiveBuildAction = AutoAnalyzeTemplate(buildAction);

            if (register is IOwnedRegistration registration)
            {
                var originalOwnerClass = registration.OwnerClass;
                var originalParentClass = registration.ParentClass;
                registration.OwnerClass = ownerClass;
                registration.ParentClass = parentClass;

                register.AddEntity(entityName, version, effectiveBuildAction);

                registration.OwnerClass = originalOwnerClass;
                registration.ParentClass = originalParentClass;

                return this;
            }

            register.AddEntity(entityName, version, effectiveBuildAction);

            return this;
        }

        [DebuggerHidden]
        public IEntitiesBuilder AddEntity<TTemplate, TEntity>(string entityName = null, int version = 1, EntityBuildAction<TTemplate> buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>
        {
            if (!typeof(TTemplate).IsInterface)
            {
                throw new InvalidOperationException(
                    $"Entity template type '{typeof(TTemplate).FullName}' must be an interface."
                );
            }

            var effectiveBuildAction = AutoAnalyzeTemplate(buildAction);

            if (register is IOwnedRegistration registration)
            {
                var originalOwnerClass = registration.OwnerClass;
                var originalParentClass = registration.ParentClass;
                registration.OwnerClass = ownerClass;
                registration.ParentClass = parentClass;

                register.AddEntity<TTemplate, TEntity>(entityName ?? Entity<TTemplate, TEntity>.Name, version, effectiveBuildAction);

                registration.OwnerClass = originalOwnerClass;
                registration.ParentClass = originalParentClass;

                return this;
            }

            register.AddEntity<TTemplate, TEntity>(entityName ?? Entity<TTemplate, TEntity>.Name, version, effectiveBuildAction);

            return this;
        }

        [DebuggerHidden]
        public IEntitiesBuilder AddEntity<TTemplate, TEntity>(int version, EntityBuildAction<TTemplate> buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>
            => AddEntity<TTemplate, TEntity>(null, version, buildAction);
    }
}


