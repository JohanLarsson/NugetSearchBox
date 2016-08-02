namespace NugetSearchBox
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Net.Mime;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using NugetSearchBox.Annotations;

    public class NugetSearchViewModel : INotifyPropertyChanged
    {
        private string searchText;
        private IEnumerable<string> nugetAutoComplete;
        private string text;
        private int? autoCompleteCount;
        private int? resultCount;

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

        public ObservableCollection<string> NugetResults { get; } = new ObservableCollection<string>();

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

        [NotifyPropertyChangedInvocator]
        protected virtual async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == nameof(this.SearchText))
            {
                this.NugetResults.Clear();
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
                    .ConfigureAwait(false);
                if (this.nugetAutoComplete.Any())
                {
                    var results = await Nuget.GetResultsAsync(string.Join(" ", this.nugetAutoComplete))
                        .ConfigureAwait(false);
                    if (this.searchText == query)
                    {
                        this.UpdateResults(results);
                    }
                }
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
                    .ConfigureAwait(false);
                if (this.searchText == query)
                {
                    this.UpdateResults(results);
                }
            }
            catch (Exception e)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.NugetResults.Clear();
                    this.NugetResults.Add(e.Message);
                }));
            }
        }

        private void UpdateResults(IReadOnlyList<string> results)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var newresults = results.Except(this.NugetResults).ToArray();
                foreach (var result in newresults)
                {
                    this.NugetResults.Add(result);
                }
            }));
        }
    }
}
