using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Stateflows.Tools.Tracer.Utils
{
    internal static class StringExtensions
    {
        static char[]? _invalids = null;

        /// <summary>Replaces characters in <c>text</c> that are not allowed in 
        /// file names with the specified replacement character.</summary>
        /// <param name="text">Text to make into a valid filename. The same string is returned if it is valid already.</param>
        /// <param name="replacement">Replacement character, or null to simply remove bad characters.</param>
        /// <returns>A string that can be used as a filename. If the output string would otherwise be empty, returns "_".</returns>
        public static string ToValidFileName(this string text, char? replacement = '_')
        {
            StringBuilder sb = new StringBuilder(text.Length);
            var invalids = _invalids ??= Path.GetInvalidFileNameChars();
            bool changed = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (invalids.Contains(c))
                {
                    changed = true;
                    var repl = replacement ?? '\0';
                    if (repl != '\0')
                    {
                        sb.Append(repl);
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (sb.Length == 0)
            {
                return "_";
            }

            return changed ? sb.ToString() : text;
        }
    }
}
