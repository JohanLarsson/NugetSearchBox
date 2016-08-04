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
    using Newtonsoft.Json;

    public static class Nuget
    {
        private static readonly Task<IReadOnlyList<PackageInfo>> EmptyResultTask =
            Task.FromResult((IReadOnlyList<PackageInfo>)new PackageInfo[0]);

        private static readonly string[] EmptyStrings = new string[0];

        private static readonly ThreadLocal<JsonSerializer> Serializer =
            new ThreadLocal<JsonSerializer>(
                () => JsonSerializer.Create(new JsonSerializerSettings { Converters = JsonConverters.Default }));

        private static readonly ThreadLocal<StringBuilder> QueryBuilder =
            new ThreadLocal<StringBuilder>(() => new StringBuilder());

        private static readonly ConcurrentDictionary<Uri, Task<IReadOnlyList<PackageInfo>>> QueryCache =
            new ConcurrentDictionary<Uri, Task<IReadOnlyList<PackageInfo>>>();

        private static readonly ConcurrentDictionary<Uri, Task<IReadOnlyList<string>>> AutoCompletesCache =
            new ConcurrentDictionary<Uri, Task<IReadOnlyList<string>>>();

        private static readonly string QueryUrl = @"https://api-v2v3search-0.nuget.org/query";

        private static QueryInfo? LastQuery;

        internal static event EventHandler<string> ReceivedRespose;

        public static async Task<IReadOnlyList<string>> GetAutoCompletesAsync(string searchText, int? take = null)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return EmptyStrings;
            }

            var query = CreateQuery(searchText, @"https://api-v2v3search-0.nuget.org/autocomplete", null, take);
            var task = AutoCompletesCache.GetOrAdd(query, DownloadAutoCompletesAsync);
            return await task.ConfigureAwait(false);
        }

        public static Task<IReadOnlyList<PackageInfo>> GetPackageInfosAsync(string searchText, int? take = null)
        {
            take = take ?? 20;
            var query = CreateQuery(searchText, QueryUrl, null, take);
            LastQuery = new QueryInfo(searchText, query, 0, take.Value);
            return GetQueryResultsAsync(query);
        }

        public static Task<IReadOnlyList<PackageInfo>> GetMoreResultsAsync(string searchText)
        {
            var moreResultsQuery = LastQuery?.CreateMoreResultsQuery(searchText);
            if (moreResultsQuery == null)
            {
                return EmptyResultTask;
            }

            LastQuery = moreResultsQuery;
            return QueryCache.GetOrAdd(moreResultsQuery.Value.Query, DownloadQueryResultsAsync);
        }

        internal static Task<IReadOnlyList<PackageInfo>> GetQueryResultsAsync(Uri query)
        {
            return QueryCache.GetOrAdd(query, DownloadQueryResultsAsync);
        }

        internal static Uri CreateQuery(string searchText, string baseUrl, int? skip = null, int? take = null)
        {
            var builder = QueryBuilder.Value;
            builder.Clear();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                builder.Append("q=");
                builder.Append(Uri.EscapeDataString(searchText));
            }

            if (skip != null && skip > 0)
            {
                if (builder.Length != 0)
                {
                    builder.Append('&');
                }

                builder.Append("skip=");
                builder.Append(skip);
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
                var handler = ReceivedRespose;
                if (handler != null)
                {
                    var json = await client.DownloadStringTaskAsync(query).ConfigureAwait(false);
                    handler.Invoke(null, json);
                    using (var sr = new StringReader(json))
                    {
                        using (var reader = new JsonTextReader(sr))
                        {
                            var response = Serializer.Value.Deserialize<QueryResponse>(reader);
                            return response.Data;
                        }
                    }
                }
                else
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

        private struct QueryInfo
        {
            internal readonly Uri Query;
            private readonly string searchText;
            private readonly int skip;
            private readonly int take;

            public QueryInfo(string searchText, Uri query, int skip, int take)
            {
                this.searchText = searchText;
                this.Query = query;
                this.skip = skip;
                this.take = take;
            }

            internal QueryInfo? CreateMoreResultsQuery(string searchText)
            {
                if (this.searchText != searchText)
                {
                    return null;
                }

                Task<IReadOnlyList<PackageInfo>> results;
                if (!QueryCache.TryGetValue(this.Query, out results))
                {
                    return null;
                }

                if (!results.IsCompleted)
                {
                    return null;
                }

                if (results.Result.Count < this.take)
                {
                    return null;
                }

                var skip = this.skip + this.take;
                return new QueryInfo(searchText, CreateQuery(searchText, QueryUrl, skip, 20), skip, 20);
            }
        }
    }
}