namespace NugetSearchBox
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;
    using NugetSearchBox.Annotations;

    public class RefreshingCollection<T> : ReadOnlyObservableCollection<T>
    {
        private readonly ObservableCollection<T> inner;
        private readonly HashSet<T> set = new HashSet<T>();
        public RefreshingCollection()
            : this(new ObservableCollection<T>())
        {
        }

        private RefreshingCollection([NotNull] ObservableCollection<T> inner)
            : base(inner)
        {
            this.inner = inner;
        }

        internal void RefreshWith(IEnumerable<T> newItems)
        {
            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher == null)
            {
                this.RefreshWithCore(newItems);
            }
            else
            {
                dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => this.RefreshWithCore(newItems)));
            }
        }

        internal void Clear()
        {
            this.inner.Clear();
        }

        private async void RefreshWithCore(IEnumerable<T> newItems)
        {
            this.AddNewItems(newItems);
            await Dispatcher.Yield();
            this.RemoveOldItems(newItems);
        }

        private void AddNewItems(IEnumerable<T> newItems)
        {
            this.set.Clear();
            this.set.UnionWith(newItems);
            this.set.ExceptWith(this.inner);
            foreach (var item in this.set)
            {
                this.inner.Add(item);
            }
        }

        private void RemoveOldItems(IEnumerable<T> newItems)
        {
            this.set.Clear();
            this.set.UnionWith(this.inner);
            this.set.ExceptWith(newItems);
            foreach (var item in this.set)
            {
                this.inner.Remove(item);
            }
        }
    }
}