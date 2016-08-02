namespace NugetSearchBox
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using NugetSearchBox.Annotations;

    public class QueryViewModel : INotifyPropertyChanged
    {
        private IReadOnlyList<string> nugetResults;
        private string query;
        private TimeSpan time;
        private Stopwatch stopwatch = Stopwatch.StartNew();

        public QueryViewModel()
        {
            this.UpdateResults();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IReadOnlyList<string> NugetResults
        {
            get { return this.nugetResults; }
            private set
            {
                if (Equals(value, this.nugetResults)) return;
                this.nugetResults = value;
                this.OnPropertyChanged();
            }
        }

        public string Query
        {
            get { return this.query; }
            set
            {
                if (value == this.query) return;
                this.query = value;
                this.OnPropertyChanged();
            }
        }

        public TimeSpan Time
        {
            get { return this.time; }
            private set
            {
                if (value.Equals(this.time)) return;
                this.time = value;
                this.OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == nameof(this.Query))
            {
                this.UpdateResults();
            }
        }

        private async void UpdateResults()
        {
            try
            {
                this.stopwatch.Restart();
                this.NugetResults = await Nuget.GetQueryResultsAsync(this.Query).ConfigureAwait(false);
                this.Time = this.stopwatch.Elapsed;
            }
            catch (Exception e)
            {
                this.NugetResults = new[] { e.Message };
            }
        }

    }
}