namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IRegionsOverrides<out TReturn>
    {
        TReturn UseRegion(int index, OverridenRegionBuildAction buildAction);
    }
}
