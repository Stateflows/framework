﻿using System.Collections.Generic;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IOutput
    {
        void Output<TToken>(TToken token);

        void OutputRange<TToken>(IEnumerable<TToken> tokens);
    }

    public interface IActionOutput : IOutput
    {
        void PassTokensOfTypeOn<TToken>();

        void PassAllTokensOn();
    }
}
