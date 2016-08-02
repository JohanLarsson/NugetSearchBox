namespace NugetSearchBox
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using NugetSearchBox.Annotations;

    public class RefreshingCollection<T> : ReadOnlyObservableCollection<T>
    {
        private readonly ObservableCollection<T> inner;
        private readonly HashSet<T> set = new HashSet<T>();
        public RefreshingCollection()
            : this( new ObservableCollection<T>())
        {
        }

        private RefreshingCollection([NotNull] ObservableCollection<T> inner)
            : base(inner)
        {
            this.inner = inner;
        }

        internal void RefreshWith(IEnumerable<T> newItems)
        {
            this.AddNewItems(newItems);
            this.RemoveOldItems(newItems);
        }

        internal void Clear()
        {
           this.inner.Clear();
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