﻿namespace NugetSearchBox
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Security.Policy;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Newtonsoft.Json;
    
    public static class Nuget
    {
        private static readonly string[] EmptyStrings = new string[0];
        private static readonly ThreadLocal<StringBuilder> QueryBuilder = new ThreadLocal<StringBuilder>(()=> new StringBuilder());
        private static readonly ConcurrentDictionary<string, PackageInfo> PackageCache = new ConcurrentDictionary<string, PackageInfo>();
        private static readonly ConcurrentDictionary<string, Task<IReadOnlyList<PackageInfo>>> QueryCache = new ConcurrentDictionary<string, Task<IReadOnlyList<PackageInfo>>>();
        private static readonly ConcurrentDictionary<string, Task<IReadOnlyList<string>>> AutoCompletesCache = new ConcurrentDictionary<string, Task<IReadOnlyList<string>>>();

        public static async Task<IReadOnlyList<string>> GetAutoCompletesAsync(string text, int? take = null)
        {
            if (string.IsNullOrWhiteSpace(text))
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
            var builder = QueryBuilder.Value;
            builder.Clear();
            if (!string.IsNullOrWhiteSpace(text))
            {
                builder.Append("q=");
                builder.Append(Uri.EscapeDataString(text));
            }
            if (take != null)
            {
                if (builder.Length != 0)
                {
                    builder.Append('&');
                }
                builder.Append("take=");
                builder.Append(take);
            }

            var query =  builder.ToString();
            return GetQueryResultsAsync(query);
        }

        internal static async Task<IReadOnlyList<PackageInfo>> GetQueryResultsAsync(string query)
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
                //var jsonReader = new JsonTextReader(new StringReader(result));

                var queryResponse = JsonConvert.DeserializeObject<QueryResponse>(result);
                return queryResponse.Data;
            }
        }
    }
}
