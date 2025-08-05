using Stateflows.Common;

namespace WarszawskieDniInformatyki.StateMachines.Document.Events;

public class AcceptResponse
{ }

public class Accept : IRequest<AcceptResponse>
{
    public bool Truly { get; set; }
}