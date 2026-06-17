using System;
using System.Threading.Tasks;
using Stateflows.Entities.Models;

namespace Stateflows.Entities.Registration
{
    internal class EntityRegistration
    {
        public string Name { get; set; }

        public int Version { get; set; }

        public Type EntityType { get; set; }

        public BehaviorClass? OwnerClass { get; set; }

        public BehaviorClass? ParentClass { get; set; }

        public EntityModel Model { get; set; }

        public Func<IEntityVisitor, Task> VisitingAction { get; set; }
    }
}
