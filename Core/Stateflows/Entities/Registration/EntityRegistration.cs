using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Registration.Builders;
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
        
        public StateflowsBuilder StateflowsBuilder { get; init; }

        public List<Func<IEntityVisitor, Task>> VisitingTasks { get; set; } = [];
    }
}
