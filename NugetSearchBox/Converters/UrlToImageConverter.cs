namespace NugetSearchBox
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media.Imaging;

    [MarkupExtensionReturnType(typeof(IValueConverter))]
    public class UrlToImageConverter : MarkupExtension, IValueConverter
    {
        private static readonly Dictionary<string, BitmapImage> Cache = new Dictionary<string, BitmapImage>();
        private static BitmapImage defaultImage;

        public Uri Default
        {
            get { return defaultImage.UriSource; }
            set
            {
                if (defaultImage != null)
                {
                    return;
                }

                defaultImage = new BitmapImage(value);
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var url = value as string;
            if (string.IsNullOrEmpty(url))
            {
                return defaultImage;
            }

            BitmapImage image;
            if (!Cache.TryGetValue(url, out image))
            {
                image = new BitmapImage(new Uri(url));
                Cache[url] = image;
            }

            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
