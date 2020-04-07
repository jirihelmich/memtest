namespace Mews.LocalizationBuilder.Dto
{
    public sealed class VersionedLocalizationData
    {
        public VersionData VersionData { get; set; }

        public LocalizationData Localization { get; set; }
    }
}