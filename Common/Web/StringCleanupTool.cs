// Tool to remove specific unwanted substrings from a string, e.g., trailing HTML/Markdown artifacts
using System;

namespace Argus.Common.Web
{
    public static class StringCleanupTool
    {
        // List of unwanted substrings to remove from LLM output
        private static readonly string[] UnwantedSubstrings = new[]
        {
            "```</body>",
            // Add more patterns here as needed
        };

        /// <summary>
        /// Removes all occurrences of the specified unwanted substring from the input string.
        /// </summary>
        /// <param name="input">The input string to clean.</param>
        /// <param name="unwantedSubstring">The substring to remove (e.g., "```</body>").</param>
        /// <returns>The cleaned string.</returns>
        public static string RemoveUnwantedSubstring(string input, string unwantedSubstring)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(unwantedSubstring))
                return input;
            return input.Replace(unwantedSubstring, string.Empty);
        }

        /// <summary>
        /// Removes all known unwanted substrings from the input string.
        /// </summary>
        /// <param name="input">The input string to clean.</param>
        /// <returns>The cleaned string.</returns>
        public static string CleanAllUnwantedSubstrings(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            foreach (var pattern in UnwantedSubstrings)
            {
                input = input.Replace(pattern, string.Empty);
            }
            return input;
        }
    }
}
