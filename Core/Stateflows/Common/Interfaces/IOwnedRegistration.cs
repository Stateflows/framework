namespace Stateflows.Common.Interfaces;

internal interface IOwnedRegistration
{
    BehaviorClass? OwnerClass { get; set; }
    BehaviorClass? ParentClass { get; set; }
}
