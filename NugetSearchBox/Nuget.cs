namespace NugetSearchBox
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;

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
                var address = new Uri($@"https://api-v2v3search-0.nuget.org/autocomplete?q={HttpUtility.UrlEncode(text, Encoding.UTF8)}&take=5");
                var result = await client.DownloadStringTaskAsync(address).ConfigureAwait(false);
                // quick & dirty here
                var start = result.IndexOf('[') + 1;
                var end = result.LastIndexOf(']');
                var slice = result.Slice(start, end);
                var matches = Regex.Matches(slice, "\"(?<word>[^\"]+)\"");
                return matches.OfType<Match>().Select(m => m.Groups["word"].Value);
            }
        }

        public static async Task<IEnumerable<string>> GetResultsAsync(string text)
        {
            using (var client = new WebClient())
            {
                var address = string.IsNullOrEmpty(text) 
                    ? new Uri($@"https://api-v2v3search-0.nuget.org/query?take=20")
                    : new Uri($@"https://api-v2v3search-0.nuget.org/query?q={HttpUtility.UrlEncode(text, Encoding.UTF8)}&take=20");
                var result = await client.DownloadStringTaskAsync(address).ConfigureAwait(false);
                // quick & dirty here
                var start = result.IndexOf('[') + 1;
                var end = result.LastIndexOf(']');
                var slice = result.Slice(start, end);
                var matches = Regex.Matches(slice, "\"title\":\"(?<word>[^\"]+)\"");
                return matches.OfType<Match>().Select(m => m.Groups["word"].Value);
            }
        }

        private static string Slice(this string text, int start, int end)
        {
            return text.Substring(start, end - start);
        }
    }
}
