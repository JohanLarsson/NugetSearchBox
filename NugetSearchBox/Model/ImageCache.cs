namespace NugetSearchBox
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Security.Policy;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;

    public static class ImageCache
    {
        private static readonly ConcurrentDictionary<Uri, Task<BitmapImage>> Cache = new ConcurrentDictionary<Uri, Task<BitmapImage>>();

        public static readonly DependencyProperty ForUrlProperty = DependencyProperty.RegisterAttached(
            "ForUrl",
            typeof(Uri),
            typeof(ImageCache),
            new PropertyMetadata(default(Uri), OnForUrlChanged));

        public static readonly DependencyProperty DefaultImageProperty = DependencyProperty.RegisterAttached(
            "DefaultImage", 
            typeof(Uri), 
            typeof(ImageCache),
            new PropertyMetadata(default(Uri)));

        public static void SetForUrl(Image element, Uri value)
        {
            element.SetValue(ForUrlProperty, value);
        }

        public static Uri GetForUrl(Image element)
        {
            return (Uri)element.GetValue(ForUrlProperty);
        }

        public static void SetDefaultImage(DependencyObject element, Uri value)
        {
            element.SetValue(DefaultImageProperty, value);
        }

        public static Uri GetDefaultImage(DependencyObject element)
        {
            return (Uri)element.GetValue(DefaultImageProperty);
        }

        private static async void OnForUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var image = (Image)d;
            var url = (Uri)e.NewValue ?? GetDefaultImage(image);
            if (url == null)
            {
                image.Source = null;
                return;
            }

            var bitmap = await Cache.GetOrAdd(url, DownloadImage)
                                    .ConfigureAwait(true);
            image.Source = bitmap;
        }

        private static async Task<BitmapImage> DownloadImage(Uri url)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var stream = await client.OpenReadTaskAsync(url).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                return Cache.GetOrAdd()
                throw;
            }
        }
    }
}
