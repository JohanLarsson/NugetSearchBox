namespace NugetSearchBox
{
    using System.Collections.Generic;

    public class QueryResponse
    {
        public QueryResponse(IReadOnlyList<PackageInfo> data)
        {
            this.Data = data;
        }

        public IReadOnlyList<PackageInfo> Data { get; set; }
    }
}