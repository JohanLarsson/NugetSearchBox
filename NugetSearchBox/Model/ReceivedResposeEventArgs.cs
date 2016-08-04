namespace NugetSearchBox
{
    using System;

    internal class ReceivedResposeEventArgs : EventArgs
    {
        internal readonly string Searchtext;
        internal readonly string Json;

        public ReceivedResposeEventArgs(string searchtext, string json)
        {
            this.Searchtext = searchtext;
            this.Json = json;
        }
    }
}