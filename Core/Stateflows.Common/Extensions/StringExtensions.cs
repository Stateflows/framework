using System.Linq;

namespace Stateflows.Common
{
    public static class StringExtensions
    {
        public static string GetShortName(this string name)
            => name.Contains('<')
                ? $"{name.Split('<').First().Split('.').Last()}<{name.Split('<').Last().Split('.').Last()}"
                : name.Split('.').Last();
    }
}