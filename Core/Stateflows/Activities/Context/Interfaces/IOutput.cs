using System.Collections.Generic;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IOutput
    {
        void Output<TToken>(TToken token);

        void OutputRange<TToken>(IEnumerable<TToken> tokens);
        
        void PassTokensOfTypeOn<TToken>();

        void PassAllTokensOn();
    }

    // public interface IActionOutput : IOutput
    // {
    //     void PassTokensOfTypeOn<TToken>();
    //
    //     void PassAllTokensOn();
    // }
}
