namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IRegions<out TReturn>
    {
        TReturn AddRegion(RegionBuildAction buildAction);
    }
}

