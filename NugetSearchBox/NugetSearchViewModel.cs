namespace NugetSearchBox
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using NugetSearchBox.Annotations;

    public class NugetSearchViewModel : INotifyPropertyChanged
    {
        private readonly Stopwatch stopwatch = new Stopwatch();
        private string searchText;
        private IEnumerable<string> nugetAutoComplete;
        private string text;
        private int? autoCompleteCount;
        private int? resultCount;
        private TimeSpan autoCompleteTime;
        private TimeSpan resultsTime;
        private TimeSpan autoCompleteResultsTime;

        public NugetSearchViewModel()
        {
            this.stopwatch.Restart();
            this.UpdateResults();
            this.resultsTime = this.stopwatch.Elapsed;
            this.stopwatch.Stop();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int? AutoCompleteCount
        {
            get { return this.autoCompleteCount; }
            set
            {
                if (value == this.autoCompleteCount) return;
                this.autoCompleteCount = value;
                this.OnPropertyChanged();
            }
        }

        public IEnumerable<string> NugetAutoComplete
        {
            get { return this.nugetAutoComplete; }
            private set
            {
                if (Equals(value, this.nugetAutoComplete)) return;
                this.nugetAutoComplete = value;
                this.OnPropertyChanged();
            }
        }

        public int? ResultCount
        {
            get { return this.resultCount; }
            set
            {
                if (value == this.resultCount) return;
                this.resultCount = value;
                this.OnPropertyChanged();
            }
        }

        public RefreshingCollection<PackageInfo> QueryResults { get; } = new RefreshingCollection<PackageInfo>();

        public RefreshingCollection<PackageInfo> AutoCompleteResults { get; } = new RefreshingCollection<PackageInfo>();

        public RefreshingCollection<PackageInfo> AllResults { get; } = new RefreshingCollection<PackageInfo>();

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

        public string Text
        {
            get { return this.text; }
            set
            {
                if (value == this.text) return;
                this.text = value;
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

        [NotifyPropertyChangedInvocator]
        protected virtual async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == nameof(this.SearchText))
            {
                this.stopwatch.Restart();
                this.UpdateAutoComplete();
                this.UpdateResults();
                this.stopwatch.Stop();
            }
        }

        private async void UpdateAutoComplete()
        {
            try
            {
                var query = this.searchText;
                this.NugetAutoComplete = await Nuget.GetAutoCompletesAsync(query, this.AutoCompleteCount)
                    .ConfigureAwait(false);
                this.AutoCompleteTime = this.stopwatch.Elapsed;
                if (this.nugetAutoComplete.Any())
                {
                    var results = await Task.WhenAll(this.nugetAutoComplete.Select(a => Nuget.GetResultsAsync(a)))
                                            .ConfigureAwait(false);
                    this.AutoCompleteResultsTime = this.stopwatch.Elapsed - this.autoCompleteTime;
                    if (this.searchText == query)
                    {
                        var newResults = results.SelectMany(r => r).Distinct().ToArray();
                        this.AutoCompleteResults.RefreshWith(newResults);
                        this.AllResults.RefreshWith(this.QueryResults.Concat(this.AutoCompleteResults));
                    }
                }
                else
                {
                    this.AutoCompleteResults.Clear();
                    this.AllResults.RefreshWith(this.QueryResults);
                }
            }
            catch (Exception e)
            {
                this.NugetAutoComplete = new[] { e.Message };
            }
        }

        private async void UpdateResults()
        {
            try
            {
                var query = this.searchText;
                var results = await Nuget.GetResultsAsync(this.SearchText, this.ResultCount)
                                         .ConfigureAwait(false);
                this.ResultsTime = this.stopwatch.Elapsed;

                if (query == this.searchText)
                {
                    this.QueryResults.RefreshWith(results);
                    this.AllResults.RefreshWith(this.QueryResults.Concat(this.AutoCompleteResults));
                }
            }
            catch (Exception)
            {
                //this.QueryResults.Clear();
                //this.QueryResults.Add(new PackageInfo());
            }
        }
    }
}
