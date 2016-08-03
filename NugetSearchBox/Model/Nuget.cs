namespace NugetSearchBox
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Newtonsoft.Json;

    public static class Nuget
    {
        private static readonly ThreadLocal<JsonSerializer> Serializer = new ThreadLocal<JsonSerializer>(() => JsonSerializer.Create(new JsonSerializerSettings { Converters = JsonConverters.Default }));
        private static readonly string[] EmptyStrings = new string[0];
        private static readonly ThreadLocal<StringBuilder> QueryBuilder = new ThreadLocal<StringBuilder>(() => new StringBuilder());
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

        public static Task<IReadOnlyList<PackageInfo>> GetResultsAsync(string text, int? skip = null, int? take = null)
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

            if (skip != null)
            {
                if (builder.Length != 0)
                {
                    builder.Append('&');
                }

                builder.Append("skip=");
                builder.Append(skip);
            }

            var query = builder.ToString();
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
                using (var result = await client.OpenReadTaskAsync(address).ConfigureAwait(false))
                {
                    using (var sr = new StreamReader(result))
                    {
                        using (var reader = new JsonTextReader(sr))
                        {
                            var response = Serializer.Value.Deserialize<AutoCompleteResponse>(reader);
                            return response.Data;
                        }
                    }
                }
            }
        }

        private static async Task<IReadOnlyList<PackageInfo>> DownloadQueryResultsAsync(string query)
        {
            using (var client = new WebClient())
            {
                var address = string.IsNullOrEmpty(query)
                    ? new Uri($@"https://api-v2v3search-0.nuget.org/query")
                    : new Uri($@"https://api-v2v3search-0.nuget.org/query?{query}");
                using (var result = await client.OpenReadTaskAsync(address).ConfigureAwait(false))
                {
                    using (var sr = new StreamReader(result))
                    {
                        using (var reader = new JsonTextReader(sr))
                        {
                            var response = Serializer.Value.Deserialize<QueryResponse>(reader);
                            return response.Data;
                        }
                    }
                }
            }
        }
    }
}
