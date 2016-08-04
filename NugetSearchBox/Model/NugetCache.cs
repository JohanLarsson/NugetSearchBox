namespace NugetSearchBox
{
    using System.Collections.Concurrent;

    public static class NugetCache
    {
        private static readonly ConcurrentDictionary<string, JsonAndPackageInfo> PackageCache = new ConcurrentDictionary<string, JsonAndPackageInfo>();

        internal static bool TryGet(string id, out JsonAndPackageInfo result)
        {
            return PackageCache.TryGetValue(id, out result);
        }

        internal static void UpdatePackageCache(string id, JsonAndPackageInfo jsonAndPackageInfo)
        {
            PackageCache.AddOrUpdate(id, jsonAndPackageInfo, (_, __) => jsonAndPackageInfo);
        }

        internal class JsonAndPackageInfo
        {
            internal readonly string Json;
            internal readonly PackageInfo Package;

            public JsonAndPackageInfo(string json, PackageInfo package)
            {
                this.Json = json;
                this.Package = package;
            }
        }
    }
}