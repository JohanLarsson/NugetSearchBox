namespace NugetSearchBox
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using NugetSearchBox.Annotations;

    public class ViewModel : INotifyPropertyChanged
    {
        private IEnumerable<string> nugetResults;
        private string query;
        private IEnumerable<string> nugetAutoComplete;
        private string text;
        private const string Lorem = "Lorem ipsum dolor sit amet, falli solet invenire in eum, alia prima possit cum an. Ex nec numquam perpetua, nusquam constituto eos ut. In offendit percipitur his, soleat nostrud nec an. Et mei debitis legendos.\r\n\r\nNam ea dicit cetero, dolore volutpat mei eu. Ferri fuisset ei eos, sale democritum nam et, qui et assum audire. Amet stet mel et, ne nam esse fugit fuisset. Nostro fabulas urbanitas qui id, et sed commodo indoctum, vero nemore persius cu per.\r\n\r\nAd has amet copiosae dignissim. Te quidam essent nam, alterum voluptaria philosophia nec et. An eligendi consetetur his. Nisl purto oblique no pro. Id omittam lucilius qui.\r\n\r\nUt nec quot admodum inciderint. Ex apeirian comprehensam est, qui accumsan accusata contentiones cu, vel eu laudem minimum. Vix ocurreret maiestatis elaboraret no, at eam audire labitur saperet, vocent delenit eu his. Ad labores indoctum qui, no sit facer nominati inimicus, ea vide scribentur sea. Ipsum expetenda contentiones vel in, has ad tation imperdiet, mei in porro eripuit instructior. Mel ea nonumy mandamus.\r\n\r\nDelenit dolorum delicata cu sit, liber meliore maluisset per an. Ignota nusquam qualisque has ne, ad audire gloriatur duo, no mea fastidii adipisci. At has splendide vituperatoribus, nec aeterno epicuri ei. Animal phaedrum efficiendi in sed. Et mel duis convenire inciderint, appareat delicata expetenda eos ut.";

        public ViewModel()
        {
            var matches = Regex.Matches(Lorem, "(?<word>)\\w+");
            foreach (var match in matches.OfType<Match>())
            {
                this.Values.Add(match.Value);
            }

            this.UpdateResults();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Values { get; } = new ObservableCollection<string>();

        public IEnumerable<string> NugetAutoComplete
        {
            get { return this.nugetAutoComplete; }
            private set
            {
                if (Equals(value, this.nugetAutoComplete)) return;
                this.nugetAutoComplete = value;
                this.OnPropertyChanged();
            }
        }

        public IEnumerable<string> NugetResults
        {
            get { return this.nugetResults; }
            private set
            {
                if (Equals(value, this.nugetResults)) return;
                this.nugetResults = value;
                this.OnPropertyChanged();
            }
        }

        public string Query
        {
            get { return this.query; }
            set
            {
                if (value == this.query) return;
                this.query = value;
                this.OnPropertyChanged();
            }
        }

        public string Text
        {
            get { return this.text; }
            set
            {
                if (value == this.text) return;
                this.text = value;
                this.OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == nameof(this.Query))
            {
                if (string.IsNullOrEmpty(this.text))
                {
                    this.UpdateAutoComplete();
                }

                this.UpdateResults();
            }
        }

        private async void UpdateAutoComplete()
        {
            try
            {
                this.NugetAutoComplete = await Nuget.GetAutoCompletesAsync(this.Query).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.NugetAutoComplete = new[] { e.Message };
            }
        }

        private async void UpdateResults()
        {
            try
            {
                this.NugetResults = await Nuget.GetResultsAsync(this.Query).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.NugetResults = new[] { e.Message };
            }
        }

    }
}
