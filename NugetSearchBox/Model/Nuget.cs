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
        private static readonly ConcurrentDictionary<Uri, Task<IReadOnlyList<PackageInfo>>> QueryCache = new ConcurrentDictionary<Uri, Task<IReadOnlyList<PackageInfo>>>();
        private static readonly ConcurrentDictionary<Uri, Task<IReadOnlyList<string>>> AutoCompletesCache = new ConcurrentDictionary<Uri, Task<IReadOnlyList<string>>>();

        public static async Task<IReadOnlyList<string>> GetAutoCompletesAsync(string text, int? take = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return EmptyStrings;
            }

            var query = CreateQuery(text, @"https://api-v2v3search-0.nuget.org/autocomplete", null, take);
            var task = AutoCompletesCache.GetOrAdd(query, DownloadAutoCompletesAsync);
            return await task.ConfigureAwait(false);
        }

        public static Task<IReadOnlyList<PackageInfo>> GetResultsAsync(string text, int? skip = null, int? take = null)
        {
            var query = CreateQuery(text, @"https://api-v2v3search-0.nuget.org/query", skip, take);
            return GetQueryResultsAsync(query);
        }

        internal static async Task<IReadOnlyList<PackageInfo>> GetQueryResultsAsync(Uri query)
        {
            var task = QueryCache.GetOrAdd(query, DownloadQueryResultsAsync);
            return await task.ConfigureAwait(false);
        }

        internal static Uri CreateQuery(string text, string baseUrl, int? skip = null, int? take = null)
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
            var address = string.IsNullOrEmpty(query)
                ? new Uri(baseUrl)
                : new Uri($@"{baseUrl}?{query}");
            return address;
        }

        private static async Task<IReadOnlyList<string>> DownloadAutoCompletesAsync(Uri query)
        {
            using (var client = new WebClient())
            {
                using (var result = await client.OpenReadTaskAsync(query).ConfigureAwait(false))
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

        private static async Task<IReadOnlyList<PackageInfo>> DownloadQueryResultsAsync(Uri query)
        {
            using (var client = new WebClient())
            {
                using (var result = await client.OpenReadTaskAsync(query).ConfigureAwait(false))
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
