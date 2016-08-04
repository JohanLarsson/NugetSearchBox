namespace NugetSearchBox
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    internal static class File
    {
        public static async Task<string> ReadAllTextAsync(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                using (var sr = new StreamReader(stream))
                {
                    return await sr.ReadToEndAsync().ConfigureAwait(false);
                }
            }
        }

        public static async Task WriteAllTextAsync(string path, string contents)
        {
            using (var sw = new StreamWriter(path, false, new UTF8Encoding(false, true), 1024))
            {
                await sw.WriteAsync(contents).ConfigureAwait(false);
            }
        }
    }
}
