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
        private IReadOnlyList<PackageInfo> queryResults;
        private string query;
        private TimeSpan time;
        private Stopwatch stopwatch = Stopwatch.StartNew();
        private Exception exception;

        public QueryViewModel()
        {
            this.UpdateResults();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IReadOnlyList<PackageInfo> QueryResults
        {
            get { return this.queryResults; }
            private set
            {
                if (Equals(value, this.queryResults)) return;
                this.queryResults = value;
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
                var uri = Nuget.CreateQuery(this.query, @"https://api-v2v3search-0.nuget.org/query");
                this.QueryResults = await Nuget.GetQueryResultsAsync(uri).ConfigureAwait(false);
                this.Time = this.stopwatch.Elapsed;
            }
            catch (Exception e)
            {
                this.Exception = e;
            }
        }
    }
}