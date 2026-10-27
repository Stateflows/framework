using System.Collections.Generic;

namespace Stateflows.Activities.Context.Interfaces
{
    /// <summary>Defines how an activity action emits or forwards tokens.</summary>
    /// <remarks>Outputs are staged for the activity engine's flow processing; they are not delivered synchronously to another node.</remarks>
    public interface IOutput
    {
        /// <summary>Emits one token from the current action.</summary>
        /// <typeparam name="TToken">The token type.</typeparam>
        /// <param name="token">The token value to emit.</param>
        void Output<TToken>(TToken token);

        /// <summary>Emits each token in a sequence from the current action.</summary>
        /// <typeparam name="TToken">The token type.</typeparam>
        /// <param name="tokens">The token values to emit.</param>
        void OutputRange<TToken>(IEnumerable<TToken> tokens);
        
        /// <summary>Forwards all available input tokens of the specified type to outgoing flows.</summary>
        /// <typeparam name="TToken">The token type to forward.</typeparam>
        void PassTokensOfTypeOn<TToken>();

        /// <summary>Forwards all available input tokens to outgoing flows.</summary>
        void PassAllTokensOn();
    }

    // public interface IActionOutput : IOutput
    // {
    //     void PassTokensOfTypeOn<TToken>();
    //
    //     void PassAllTokensOn();
    // }
}
