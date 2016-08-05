namespace NugetSearchBox
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class PackageToFavoriteViewModelConverter : IValueConverter
    {
        public static  readonly PackageToFavoriteViewModelConverter Default = new PackageToFavoriteViewModelConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new FavoritViewModel((PackageInfo) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
