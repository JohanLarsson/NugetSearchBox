namespace NugetSearchBox
{
    using System.Collections.Generic;

    public class AutoCompleteResponse
    {
        public AutoCompleteResponse(IReadOnlyList<string> data)
        {
            this.Data = data;
        }

        public IReadOnlyList<string> Data { get; }
    }
}
