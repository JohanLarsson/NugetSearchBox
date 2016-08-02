namespace NugetSearchBox
{
    public class VersionInfo
    {
        public VersionInfo(string version, int downloads, string id)
        {
            this.Version = version;
            this.Downloads = downloads;
            this.Id = id;
        }

        public string Version { get;  }

        public int Downloads { get;  }

        public string Id { get;  }
    }
}