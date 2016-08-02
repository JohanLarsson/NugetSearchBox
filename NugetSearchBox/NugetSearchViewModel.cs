namespace NugetSearchBox
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
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
            this.UpdateResults();
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
                if (string.IsNullOrEmpty(this.text))
                {
                    this.UpdateAutoComplete();
                }

                this.UpdateResults();
            }
        }

        private async void UpdateAutoComplete()
        {
            try
            {
                var query = this.searchText;
                this.NugetAutoComplete = await Nuget.GetAutoCompletesAsync(query, this.AutoCompleteCount)
                    .ConfigureAwait(true);
                this.AutoCompleteTime = this.stopwatch.Elapsed;
                if (this.nugetAutoComplete.Any())
                {
                    var results = await Task.WhenAll(this.nugetAutoComplete.Select(a=> Nuget.GetResultsAsync(a)))
                                            .ConfigureAwait(true);
                    if (this.searchText == query)
                    {
                        var flattened = results.SelectMany(r => r).Distinct();
                        this.AutoCompleteResults.RefreshWith(flattened);
                    }
                }

                this.AutoCompleteResultsTime = this.stopwatch.Elapsed - this.autoCompleteTime;
            }
            catch (Exception e)
            {
                this.NugetAutoComplete = new[] {e.Message};
            }
        }

        private async void UpdateResults()
        {
            try
            {
                var query = this.searchText;
                var results = await Nuget.GetResultsAsync(this.SearchText, this.ResultCount)
                    .ConfigureAwait(true);
                this.ResultsTime = this.stopwatch.Elapsed;

                if (query == this.searchText)
                {
                    this.QueryResults.RefreshWith(results);
                }
            }
            catch (Exception)
            {
                this.QueryResults.Clear();
                //this.QueryResults.Add(new PackageInfo());
            }
        }
    }
}
