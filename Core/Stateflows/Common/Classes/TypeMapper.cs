using System;
using System.Collections.Generic;
using System.Reflection;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    /// <summary>Resolves mapped types using registered type mappers and visits the resolved types.</summary>
    public class TypeMapper : ITypeMapper
    {
        private readonly List<IStateflowsTypeMapper> TypeMappers;
        private readonly Action<Type> registerType;

        /// <summary>Creates a type mapper from the configured mapping providers.</summary>
        /// <param name="typeMappers">The providers consulted in order for a type mapping.</param>
        /// <param name="registerType">An optional callback invoked for each resolved type.</param>
        public TypeMapper(List<IStateflowsTypeMapper> typeMappers, Action<Type> registerType = null)
        {
            TypeMappers = typeMappers;
            this.registerType = registerType;
        }
        
        /// <inheritdoc />
        public IEnumerable<Type> GetMappedTypes(Type type)
        {
            foreach (var typeMapper in TypeMappers)
            {
                if (typeMapper.TryMapType(type, out var triggerTypes))
                {
                    return RegisterTypes(triggerTypes);
                }
            }

            return RegisterTypes(new Type[] { type });
        }

        private IEnumerable<Type> RegisterTypes(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                registerType?.Invoke(type);
                yield return type;
            }
        }
        
        private MethodInfo VisitMethod = typeof(ITypeVisitor).GetMethod(nameof(ITypeVisitor.Visit));

        /// <inheritdoc />
        public void VisitMappedTypes<T>(ITypeVisitor typeVisitor)
            => VisitMappedTypes(typeof(T), typeVisitor);

        /// <inheritdoc />
        public void VisitMappedTypes(Type type, ITypeVisitor typeVisitor)
        {
            var mappedTypes = GetMappedTypes(type);
            foreach (var mappedType in mappedTypes)
            {
                VisitMethod.MakeGenericMethod(mappedType).Invoke(typeVisitor, Array.Empty<object>());
            }
        }
    }
}