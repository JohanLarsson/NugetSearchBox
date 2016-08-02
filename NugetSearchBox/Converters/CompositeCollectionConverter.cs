namespace NugetSearchBox
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    public class CompositeCollectionConverter : IMultiValueConverter
    {
        public static readonly CompositeCollectionConverter Default = new CompositeCollectionConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var compositeCollection = new CompositeCollection();
            foreach (var value in values.OfType<IEnumerable>())
            {
                compositeCollection.Add(new CollectionContainer {Collection = value});
            }

            return compositeCollection;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
