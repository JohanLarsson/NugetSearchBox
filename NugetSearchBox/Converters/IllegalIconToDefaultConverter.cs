namespace NugetSearchBox
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class IllegalIconToDefaultConverter : IValueConverter
    {
        public Uri DefaultIcon { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var url = value as string;
            if (string.IsNullOrWhiteSpace(url))
            {
                return this.DefaultIcon;
            }

            var extension = System.IO.Path.GetExtension(url);
            if (extension == ".svg")
            {
                return this.DefaultIcon;
            }

            return url;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
