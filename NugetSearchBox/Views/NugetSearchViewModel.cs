namespace NugetSearchBox
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using NugetSearchBox.Annotations;

    public sealed class NugetSearchViewModel : INotifyPropertyChanged
    {
        private readonly Stopwatch stopwatch = new Stopwatch();
        private string searchText;
        private IEnumerable<string> autoCompletes;
        private TimeSpan autoCompleteTime;
        private TimeSpan resultsTime;
        private TimeSpan autoCompleteResultsTime;
        private Exception exception;
        private static readonly string CacheFile = System.IO.Path.Combine(Paket.Constants.NuGetCacheFolder, "defaultSearch.paket");

        public NugetSearchViewModel()
        {
            this.Initialize();
        }

        // initialize needed here due to async
        private async void Initialize()
        {
            try
            {
                if (System.IO.File.Exists(CacheFile))
                {
                    var json = await File.ReadAllTextAsync(CacheFile).ConfigureAwait(false);
                    var packageInfos = JsonConvert.DeserializeObject<QueryResponse>(json, JsonConverters.Default).Data;
                    this.Packages.RefreshWith(packageInfos);
                }
            }
            catch (Exception e)
            {
                this.Exception = e;
            }

            Nuget.ReceivedRespose += this.OnReceivedResponse;
            this.UpdateResults();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<string> AutoCompletes
        {
            get { return this.autoCompletes; }
            private set
            {
                if (Equals(value, this.autoCompletes)) return;
                this.autoCompletes = value;
                this.OnPropertyChanged();
            }
        }

        public RefreshingCollection<PackageInfo> Packages { get; } = new RefreshingCollection<PackageInfo>();

        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                if (value == this.searchText) return;
                this.searchText = value;
                this.OnPropertyChanged();
            }
        }

        public TimeSpan AutoCompleteTime
        {
            get { return this.autoCompleteTime; }
            private set
            {
                if (value.Equals(this.autoCompleteTime)) return;
                this.autoCompleteTime = value;
                this.OnPropertyChanged();
            }
        }

        public TimeSpan ResultsTime
        {
            get { return this.resultsTime; }
            private set
            {
                if (value.Equals(this.resultsTime)) return;
                this.resultsTime = value;
                this.OnPropertyChanged();
            }
        }

        public TimeSpan AutoCompleteResultsTime
        {
            get { return this.autoCompleteResultsTime; }
            private set
            {
                if (value.Equals(this.autoCompleteResultsTime)) return;
                this.autoCompleteResultsTime = value;
                this.OnPropertyChanged();
            }
        }

        public Exception Exception
        {
            get { return this.exception; }
            private set
            {
                if (Equals(value, this.exception)) return;
                this.exception = value;
                this.OnPropertyChanged();
            }
        }

        internal async Task FetchMoreResults()
        {
            try
            {
                if (!this.stopwatch.IsRunning)
                {
                    this.stopwatch.Restart();
                }

                var startTime = this.stopwatch.Elapsed;
                var query = this.searchText;
                var results = await Nuget.GetMoreResultsAsync(this.SearchText)
                                         .ConfigureAwait(false);
                this.ResultsTime = this.stopwatch.Elapsed - startTime;

                if (query == this.searchText)
                {
                    this.Packages.UnionWith(results);
                }

                this.Exception = null;
            }
            catch (Exception e)
            {
                this.Exception = e;
            }
        }

        [NotifyPropertyChangedInvocator]
        private async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == nameof(this.SearchText))
            {
                this.stopwatch.Restart();
                var query = this.searchText;
                var ints = await Task.WhenAll(
                    this.UpdateResults(),
                    this.UpdateAutoComplete())
                                     .ConfigureAwait(false);

                if (query == this.searchText && ints[0] < 20)
                {
                    await this.AppendAutoCompleteResults(query).ConfigureAwait(false);
                }

                this.stopwatch.Stop();
            }
        }

        private async void OnReceivedResponse(object sender, string json)
        {
            Nuget.ReceivedRespose -= this.OnReceivedResponse;
            try
            {
                await File.WriteAllTextAsync(CacheFile, json).ConfigureAwait(false);

            }
            catch (Exception e)
            {
                this.Exception = e;
            }
        }

        private async Task AppendAutoCompleteResults(string query)
        {
            var names = this.autoCompletes;
            if (names.Any())
            {
                try
                {
                    var tasks = names.Select(name => Nuget.GetResultsAsync(name));
                    foreach (var task in tasks)
                    {
                        var results = await task.ConfigureAwait(false);
                        if (this.searchText != query)
                        {
                            break;
                        }

                        this.Packages.UnionWith(results);
                    }

                    this.Exception = null;
                    this.AutoCompleteResultsTime = this.stopwatch.Elapsed - this.autoCompleteTime;
                }
                catch (Exception e)
                {
                    this.Exception = e;
                }
            }
        }

        private async Task<int> UpdateAutoComplete()
        {
            if (this.autoCompletes?.Contains(this.searchText) == true)
            {
                return 0;
            }

            this.autoCompletes = Enumerable.Empty<string>();
            if (string.IsNullOrWhiteSpace(this.searchText))
            {
                return 0;
            }

            try
            {
                var query = this.searchText;
                this.AutoCompletes = await Nuget.GetAutoCompletesAsync(query)
                                                .ConfigureAwait(false);
                this.AutoCompleteTime = this.stopwatch.Elapsed;
                this.Exception = null;
                return this.autoCompletes?.Count() ?? 0;
            }
            catch (Exception e)
            {
                this.Exception = e;
            }

            return 0;
        }

        private async Task<int> UpdateResults(int? take = null)
        {
            try
            {
                if (!this.stopwatch.IsRunning)
                {
                    this.stopwatch.Restart();
                }

                var startTime = this.stopwatch.Elapsed;
                var query = this.searchText;
                var results = await Nuget.GetResultsAsync(this.SearchText, take)
                                         .ConfigureAwait(false);
                this.ResultsTime = this.stopwatch.Elapsed - startTime;

                if (query == this.searchText)
                {
                    this.Packages.RefreshWith(results);
                    return results.Count;
                }

                this.Exception = null;
                return int.MaxValue;
            }
            catch (Exception e)
            {
                this.Exception = e;
                return 0;
            }
        }
    }
}
