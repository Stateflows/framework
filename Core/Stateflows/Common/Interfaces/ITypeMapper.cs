using System;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    public interface ITypeVisitor
    {
        void Visit<T>();
    }
    
    public interface ITypeMapper
    {
        IEnumerable<Type> GetMappedTypes(Type type);

        void VisitMappedTypes<T>(ITypeVisitor typeVisitor);
    }
}
