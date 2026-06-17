using System.Linq;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Entities.Attributes;
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

        private static Type GetEntityTemplateType(Type type)
            => type
                .GetInterfaces()
                .FirstOrDefault(@interface =>
                    @interface.IsGenericType &&
                    @interface.GetGenericTypeDefinition() == typeof(IEntity<>)
                )
                ?.GetGenericArguments()
                .SingleOrDefault();

        private void RegisterTypedEntity(Type templateType, string entityName, int version, Type entityType)
        {
            var addEntityMethod = AddTypedEntityRegistrationMethod.MakeGenericMethod(templateType);

            if (register is IOwnedRegistration registration)
            {
                var originalOwnerClass = registration.OwnerClass;
                var originalParentClass = registration.ParentClass;
                registration.OwnerClass = ownerClass;
                registration.ParentClass = parentClass;

                addEntityMethod.Invoke(register, [entityName, version, entityType, null]);

                registration.OwnerClass = originalOwnerClass;
                registration.ParentClass = originalParentClass;
            }
            else
            {
                addEntityMethod.Invoke(register, [entityName, version, entityType, null]);
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

            if (register is IOwnedRegistration registration)
            {
                var originalOwnerClass = registration.OwnerClass;
                var originalParentClass = registration.ParentClass;
                registration.OwnerClass = ownerClass;
                registration.ParentClass = parentClass;

                register.AddEntity(entityName, version, buildAction);

                registration.OwnerClass = originalOwnerClass;
                registration.ParentClass = originalParentClass;

                return this;
            }

            register.AddEntity(entityName, version, buildAction);

            return this;
        }

        [DebuggerHidden]
        public IEntitiesBuilder AddEntity<TTemplate, TEntity>(string entityName = null, int version = 1, EntityBuildAction<TTemplate> buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>
        {
            if (register is IOwnedRegistration registration)
            {
                var originalOwnerClass = registration.OwnerClass;
                var originalParentClass = registration.ParentClass;
                registration.OwnerClass = ownerClass;
                registration.ParentClass = parentClass;

                register.AddEntity<TTemplate, TEntity>(entityName ?? Entity<TTemplate, TEntity>.Name, version, buildAction);

                registration.OwnerClass = originalOwnerClass;
                registration.ParentClass = originalParentClass;

                return this;
            }

            register.AddEntity<TTemplate, TEntity>(entityName ?? Entity<TTemplate, TEntity>.Name, version, buildAction);

            return this;
        }

        [DebuggerHidden]
        public IEntitiesBuilder AddEntity<TTemplate, TEntity>(int version, EntityBuildAction<TTemplate> buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>
            => AddEntity<TTemplate, TEntity>(null, version, buildAction);
    }
}


