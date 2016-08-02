namespace NugetSearchBox
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public static class Nuget
    {
        public static async Task<IEnumerable<string>> GetAutoCompletesAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Enumerable.Empty<string>();
            }

            using (var client = new WebClient())
            {
                var address = new Uri($@"https://api-v2v3search-0.nuget.org/autocomplete?q={text}");
                var result = await client.DownloadStringTaskAsync(address).ConfigureAwait(false);
                // quick & dirty here
                var start = result.IndexOf('[') + 1;
                var end = result.LastIndexOf(']');
                var slice = result.Slice(start, end);
                var matches = Regex.Matches(slice, "\"(?<word>[^\"]+)\"");
                return matches.OfType<Match>().Select(m => m.Groups["word"].Value);
            }
        }

        private static string Slice(this string text, int start, int end)
        {
            return text.Substring(start, end - start);
        }
    }
}
