namespace NugetSearchBox
{
    using Newtonsoft.Json;

    internal static class JsonConverters
    {
        public static readonly JsonConverter[] Default =
        {
            PackageInfoConverter.Default,
        };
    }
}
