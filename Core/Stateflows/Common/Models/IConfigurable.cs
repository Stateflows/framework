namespace Stateflows.Common;

public interface IConfigurable<in TConfiguration> : IAbstractElement
{
    public TConfiguration Configuration { set; }
}