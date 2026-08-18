namespace Stateflows.Entities;

public interface IProjection<in TEntityTemplate, out TProjectionTemplate>
{
    static abstract TProjectionTemplate Project(TEntityTemplate template);
}