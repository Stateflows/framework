using System;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Receives a mapped CLR type during a type-mapping traversal.</summary>
    public interface ITypeVisitor
    {
        /// <summary>Handles a type reached during traversal.</summary>
        /// <typeparam name="T">The mapped type being visited.</typeparam>
        void Visit<T>();
    }
    
    /// <summary>Resolves and traverses type mappings registered by the host.</summary>
    public interface ITypeMapper
    {
        /// <summary>Gets the types mapped from a source type.</summary>
        /// <param name="type">The source type whose mappings are queried.</param>
        /// <returns>The mapped types, if any.</returns>
        IEnumerable<Type> GetMappedTypes(Type type);

        /// <summary>Visits all types mapped from a compile-time source type.</summary>
        /// <typeparam name="T">The source type whose mappings are traversed.</typeparam>
        /// <param name="typeVisitor">The visitor invoked for each mapped type.</param>
        void VisitMappedTypes<T>(ITypeVisitor typeVisitor);
        
        /// <summary>Visits all types mapped from a source type.</summary>
        /// <param name="type">The source type whose mappings are traversed.</param>
        /// <param name="typeVisitor">The visitor invoked for each mapped type.</param>
        void VisitMappedTypes(Type type, ITypeVisitor typeVisitor);
    }
}
