namespace Mews.LocalizationBuilder.Storage
{
    public sealed class VersionedLocalizationData
    {
        public VersionedLocalizationData(VersionData versionData, LocalizationData localization)
        {
            VersionData = versionData;
            Localization = localization;
        }

        public VersionData VersionData { get; }

        public LocalizationData Localization { get; }
    }
}