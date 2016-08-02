namespace NugetSearchBox
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using Newtonsoft.Json;

    public static class Nuget
    {
        private static readonly string[] EmptyStrings = new string[0];
        private static readonly ConcurrentDictionary<string, Task<IReadOnlyList<PackageInfo>>> QueryCache = new ConcurrentDictionary<string, Task<IReadOnlyList<PackageInfo>>>();
        private static readonly ConcurrentDictionary<string, Task<IReadOnlyList<string>>> AutoCompletesCache = new ConcurrentDictionary<string, Task<IReadOnlyList<string>>>();

        public static async Task<IReadOnlyList<string>> GetAutoCompletesAsync(string text, int? take = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                return EmptyStrings;
            }
            var takestring = take == null ? "" : $"&take = {take}";
            var query = $@"https://api-v2v3search-0.nuget.org/autocomplete?q={HttpUtility.UrlEncode(text, Encoding.UTF8)}{takestring}";
            var task = AutoCompletesCache.GetOrAdd(query, DownloadAutoCompletesAsync);
            return await task.ConfigureAwait(false);
        }

        public static Task<IReadOnlyList<PackageInfo>> GetResultsAsync(string text, int? take = null)
        {
            var takestring = take == null ? "" : $"&take = {take}";
            return GetQueryResultsAsync($"q={text}{takestring}");
        }

        public static async Task<IReadOnlyList<PackageInfo>> GetQueryResultsAsync(string query)
        {
            var task = QueryCache.GetOrAdd(query, DownloadQueryResultsAsync);
            return await task.ConfigureAwait(false);
        }

        private static async Task<IReadOnlyList<string>> DownloadAutoCompletesAsync(string query)
        {
            using (var client = new WebClient())
            {
                var address = new Uri(query);
                var result = await client.DownloadStringTaskAsync(address).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<AutoCompleteResponse>(result).Data;
            }
        }

        private static async Task<IReadOnlyList<PackageInfo>> DownloadQueryResultsAsync(string query)
        {
            using (var client = new WebClient())
            {
                var address = string.IsNullOrEmpty(query)
                    ? new Uri($@"https://api-v2v3search-0.nuget.org/query?")
                    : new Uri($@"https://api-v2v3search-0.nuget.org/query?{query}");
                var result = await client.DownloadStringTaskAsync(address).ConfigureAwait(false);
                var queryResponse = JsonConvert.DeserializeObject<QueryResponse>(result);
                return queryResponse.Data;
            }
        }
    }
}
