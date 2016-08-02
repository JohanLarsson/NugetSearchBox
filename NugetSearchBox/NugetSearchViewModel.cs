namespace NugetSearchBox
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using NugetSearchBox.Annotations;

    public class NugetSearchViewModel : INotifyPropertyChanged
    {
        private IEnumerable<string> nugetResults;
        private string searchText;
        private IEnumerable<string> nugetAutoComplete;
        private string text;

        public NugetSearchViewModel()
        {
            this.UpdateResults();
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        public IEnumerable<string> NugetResults
        {
            get { return this.nugetResults; }
            private set
            {
                if (Equals(value, this.nugetResults)) return;
                this.nugetResults = value;
                this.OnPropertyChanged();
            }
        }

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
                this.NugetAutoComplete = await Nuget.GetAutoCompletesAsync(this.SearchText).ConfigureAwait(false);
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
                this.NugetResults = await Nuget.GetResultsAsync(this.SearchText).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.NugetResults = new[] { e.Message };
            }
        }
    }
}
