namespace NugetSearchBox
{
    using System.Collections.Generic;
    using System.Diagnostics;

    [DebuggerDisplay("{Id}")]
    public class PackageInfo
    {
        public PackageInfo(
            string type,
            string registration, 
            string id, 
            string version,
            string description, 
            string summary,
            string title, 
            string iconUrl,
            string licenseUrl,
            string projectUrl,
            string[] tags, 
            string[] authors, 
            int totalDownloads,
            VersionInfo[] versions)
        {
            this.Type = type;
            this.Registration = registration;
            this.Id = id;
            this.Version = version;
            this.Description = description;
            this.Summary = summary;
            this.Title = title;
            this.IconUrl = iconUrl;
            this.LicenseUrl = licenseUrl;
            this.ProjectUrl = projectUrl;
            this.Tags = tags;
            this.Authors = authors;
            this.TotalDownloads = totalDownloads;
            this.Versions = versions;
        }

        public string Type { get; }

        public string Registration { get; }

        public string Id { get; }

        public string Version { get; }

        public string Description { get; }

        public string Summary { get; }

        public string Title { get; }

        public string IconUrl { get; }

        public string LicenseUrl { get; }

        public string ProjectUrl { get; }

        public IReadOnlyList<string> Tags { get; }

        public IReadOnlyList<string> Authors { get; }

        public int TotalDownloads { get; }

        public IReadOnlyList<VersionInfo> Versions { get; }

        public static bool operator ==(PackageInfo left, PackageInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PackageInfo left, PackageInfo right)
        {
            return !Equals(left, right);
        }

        protected bool Equals(PackageInfo other)
        {
            return string.Equals(this.Id, other.Id) && string.Equals(this.Version, other.Version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((PackageInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Id.GetHashCode()*397) ^ this.Version.GetHashCode();
            }
        }
    }
}