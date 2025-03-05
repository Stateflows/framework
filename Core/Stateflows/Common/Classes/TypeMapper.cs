using System;
using System.Collections.Generic;
using System.Reflection;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    public class TypeMapper : ITypeMapper
    {
        private readonly List<IStateflowsTypeMapper> TypeMappers;

        public TypeMapper(List<IStateflowsTypeMapper> typeMappers)
        {
            TypeMappers = typeMappers;
        }
        
        public IEnumerable<Type> GetMappedTypes(Type type)
        {
            foreach (var typeMapper in TypeMappers)
            {
                if (typeMapper.TryMapType(type, out var triggerTypes))
                {
                    return triggerTypes;
                }
            }

            return new Type[] { type };
        }
        
        private MethodInfo VisitMethod = typeof(ITypeVisitor).GetMethod(nameof(ITypeVisitor.Visit));

        public void VisitMappedTypes<T>(ITypeVisitor typeVisitor)
        {
            var types = GetMappedTypes(typeof(T));
            foreach (var type in types)
            {
                VisitMethod.MakeGenericMethod(type).Invoke(typeVisitor, Array.Empty<object>());
            }
        }
    }
}