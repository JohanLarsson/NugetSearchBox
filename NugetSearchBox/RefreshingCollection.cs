namespace NugetSearchBox
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
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
            this.UnionWith(newItems);
            this.ExceptWith(newItems);
        }

        internal void Clear()
        {
            BeginInvoke(this.inner.Clear);
        }

        internal void UnionWith(IEnumerable<T> newItems)
        {
            BeginInvoke(()=> this.UnionWithCore(newItems));
        }

        internal void ExceptWith(IEnumerable<T> newItems)
        {
            BeginInvoke(() => this.ExceptWithCore(newItems));
        }

        private static void BeginInvoke(Action action)
        {
            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher == null)
            {
                action();
            }
            else
            {
                dispatcher.BeginInvoke(DispatcherPriority.Background, action);
            }
        }

        private void UnionWithCore(IEnumerable<T> newItems)
        {
            this.set.Clear();
            this.set.UnionWith(newItems);
            this.set.ExceptWith(this.inner);
            foreach (var item in this.set)
            {
                this.inner.Add(item);
            }
        }

        private void ExceptWithCore(IEnumerable<T> newItems)
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