using System;

namespace Mews.LocalizationBuilder.Storage
{
    public sealed class VersionData
    {
        public VersionData(Version version, string commit)
        {
            Version = version.ToString();
            Commit = commit;
        }

        public string Version { get; }

        public string Commit { get; }
    }
}