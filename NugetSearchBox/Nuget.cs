﻿namespace NugetSearchBox
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;

    public static class Nuget
    {
        private static readonly string[] EmptyStrings = new string[0];
        private static readonly ConcurrentDictionary<string, Task<IReadOnlyList<string>>> ResultsCache = new ConcurrentDictionary<string, Task<IReadOnlyList<string>>>();
        private static readonly ConcurrentDictionary<string, Task<IReadOnlyList<string>>> AutoCompletesCache = new ConcurrentDictionary<string, Task<IReadOnlyList<string>>>();

        public static async Task<IReadOnlyList<string>> GetAutoCompletesAsync(string text, int? take = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                return EmptyStrings;
            }
            var takestring = take == null ? "" : $"&take = {take}";
            var query = $@"https://api-v2v3search-0.nuget.org/autocomplete?q={HttpUtility.UrlEncode(text, Encoding.UTF8)}{takestring}";
            var task = AutoCompletesCache.GetOrAdd(query, q => DownloadAutoCompletesAsync(q));
            return await task.ConfigureAwait(false);

        }

        public static Task<IReadOnlyList<string>> GetResultsAsync(string text, int? take = null)
        {
            var takestring = take == null ? "" : $"&take = {take}";
            return GetQueryResultsAsync($"q={text}{takestring}");
        }

        public static async Task<IReadOnlyList<string>> GetQueryResultsAsync(string query)
        {
            var task = ResultsCache.GetOrAdd(query, q => DownloadQueryResultsAsync(q));
            return await task.ConfigureAwait(false);
        }

        private static async Task<IReadOnlyList<string>> DownloadAutoCompletesAsync(string query)
        {
            using (var client = new WebClient())
            {
                var address = new Uri(query);
                var result = await client.DownloadStringTaskAsync(address).ConfigureAwait(false);
                // quick & dirty here
                var start = result.IndexOf('[') + 1;
                var end = result.LastIndexOf(']');
                var slice = result.Slice(start, end);
                var matches = Regex.Matches(slice, "\"(?<word>[^\"]+)\"");
                return matches.OfType<Match>().Select(m => m.Groups["word"].Value).ToArray();
            }
        }

        private static async Task<IReadOnlyList<string>> DownloadQueryResultsAsync(string query)
        {
            using (var client = new WebClient())
            {
                var address = string.IsNullOrEmpty(query)
                    ? new Uri($@"https://api-v2v3search-0.nuget.org/query?")
                    : new Uri($@"https://api-v2v3search-0.nuget.org/query?{query}");
                var result = await client.DownloadStringTaskAsync(address).ConfigureAwait(false);
                // quick & dirty here
                var start = result.IndexOf('[') + 1;
                var end = result.LastIndexOf(']');
                var slice = result.Slice(start, end);
                var matches = Regex.Matches(slice, "\"title\":\"(?<word>[^\"]+)\"");
                return matches.OfType<Match>().Select(m => m.Groups["word"].Value).ToArray();
            }
        }
        private static string Slice(this string text, int start, int end)
        {
            return text.Substring(start, end - start);
        }
    }
}
