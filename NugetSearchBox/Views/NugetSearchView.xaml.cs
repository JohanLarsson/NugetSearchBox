using System.Windows.Controls;

namespace NugetSearchBox
{
    using System.Windows;

    public partial class NugetSearchView : UserControl
    {
        public NugetSearchView()
        {
            this.InitializeComponent();
        }

        public NugetSearchViewModel SearchViewModel => this.DataContext as NugetSearchViewModel;

        private async void OnCloseToBottom(object sender, RoutedEventArgs e)
        {
            this.SearchViewModel?.FetchMorePackagesAsync();
        }
    }
}
