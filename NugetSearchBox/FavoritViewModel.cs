namespace NugetSearchBox
{
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using NugetSearchBox.Annotations;

    public class FavoritViewModel : INotifyPropertyChanged
    {
        private bool isFavorite;

        internal static DirectoryInfo FavoritesFolder
        {
            get
            {
                var path = System.IO.Path.Combine(Paket.Constants.NuGetCacheFolder, "Paket", "Favorites");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return new DirectoryInfo(path);
            }
        }

        public FavoritViewModel(PackageInfo package)
        {
            this.Package = package;
            this.FileName = System.IO.Path.Combine(FavoritesFolder.FullName, $"{this.Package.Id}.favorite");
            this.isFavorite = System.IO.File.Exists(this.FileName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PackageInfo Package { get; }

        public bool IsFavorite
        {
            get { return this.isFavorite; }
            set
            {
                if (value == this.isFavorite)
                {
                    return;
                }
                this.isFavorite = value;
                this.OnPropertyChanged();
                this.UpdateFile();
            }
        }

        private string FileName { get; }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void UpdateFile()
        {
            if (this.IsFavorite)
            {
                NugetCache.JsonAndPackageInfo result;
                NugetCache.TryGet(this.Package.Id, out result);
                await File.WriteAllTextAsync(this.FileName, result.Json).ConfigureAwait(false);
            }
            else
            {
                System.IO.File.Delete(this.FileName);
            }
        }
    }
}
