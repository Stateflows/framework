﻿using System;
using Stateflows.Common.Extensions;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public static class TokenInfoExtensions
    {
        public static string GetTokenName(this Type @type)
        {
            if (!@type.IsSubclassOf(typeof(Token)))
            {
                throw new ArgumentException("Given type is not subclass of Token class");
            }

            var token = @type.GetUninitializedInstance() as Token;
            return token.Name;
        }
    }
}
