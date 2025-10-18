namespace Stateflows.Common;

[NoForwarding]
public class Finalized : SystemEvent
{
    public bool ForcefulFinalization { get; set; } = false;
}